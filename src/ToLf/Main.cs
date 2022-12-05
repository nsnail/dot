using System.Diagnostics.CodeAnalysis;

namespace Dot.ToLf;

public sealed class Main : ToolBase<Option>, IDisposable
{
    private                 int              _breakCnt;
    private                 bool             _disposed;
    private static readonly object           _lockObj = new();
    private                 int              _procedCnt;
    private                 int              _replaceCnt;
    private                 ChildProgressBar _step2Bar;
    private                 int              _totalCnt;
    public Main(Option opt) : base(opt) { }


    ~Main()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) _step2Bar?.Dispose();
        _disposed = true;
    }

    private async ValueTask FileHandle(string file, CancellationToken _)
    {
        _step2Bar.Tick();
        ShowMessage(1, 0, 0);

        var tmpFile    = $"{file}.tmp";
        var isReplaced = false;
        var isBin      = false;
        int data;

        await using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            if (Opt.ReadOnly) { //测试，只读模式
                ShowMessage(0, 1, 0);
                return;
            }

            if (fsr is null) {
                ShowMessage(0, 0, 1);
                return;
            }


            await using var fsw = OpenFileStream(tmpFile, FileMode.OpenOrCreate, FileAccess.Write);

            while ((data = fsr.ReadByte()) != -1) {
                switch (data) {
                    case 0x0d when fsr.ReadByte() == 0x0a: // crlf windows
                        fsw.WriteByte(0x0a);
                        isReplaced = true;
                        continue;
                    case 0x0d: //cr macos
                        fsw.WriteByte(0x0a);
                        fsr.Seek(-1, SeekOrigin.Current);
                        isReplaced = true;
                        continue;
                    case 0x00 or 0xff: //非文本文件
                        isBin = true;
                        break;
                    default:
                        fsw.WriteByte((byte)data);
                        continue;
                }

                break;
            }
        }


        if (isReplaced && !isBin) {
            MoveFile(tmpFile, file);

            ShowMessage(0, 1, 0);
        }
        else {
            File.Delete(tmpFile);
            ShowMessage(0, 0, 1);
        }
    }


    private void ShowMessage(int procedCnt, int replaceCnt, int breakCnt)
    {
        lock (_lockObj) {
            _procedCnt        += procedCnt;
            _replaceCnt       += replaceCnt;
            _breakCnt         += breakCnt;
            _step2Bar.Message =  string.Format(Str.ShowMessageTemp, _procedCnt, _totalCnt, _replaceCnt, _breakCnt);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override async Task Run()
    {
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path), string.Format(Str.PathNotFound, Opt.Path));


        using var step1Bar = new IndeterminateProgressBar(Str.SearchingFile, DefaultProgressBarOptions);


        var fileList = EnumerateFiles(Opt.Path, Opt.Filter);
        _totalCnt = fileList.Count();

        step1Bar.Message = string.Format(Str.SearchingFileOK, _totalCnt);
        step1Bar.Finished();
        if (_totalCnt == 0) return;


        _step2Bar = step1Bar.Spawn(_totalCnt, string.Empty, DefaultProgressBarOptions);

        await Parallel.ForEachAsync(fileList, FileHandle);
    }
}
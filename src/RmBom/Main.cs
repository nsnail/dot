using System.Diagnostics.CodeAnalysis;

namespace Dot.RmBom;

public sealed class Main : ToolBase<Option>, IDisposable
{
    private                 int              _breakCnt;
    private                 bool             _disposed;
    private static readonly object           _lockObj = new();
    private                 int              _procedCnt;
    private                 ChildProgressBar _step2Bar;
    private                 int              _totalCnt;
    private                 int              _trimCnt;
    private readonly        byte[]           _utf8Bom = { 0xef, 0xbb, 0xbf };
    public Main(Option opt) : base(opt) { }


    ~Main()
    {
        Dispose(false);
    }

    private bool CreateTempFile(Stream fsr, string tmpFile)
    {
        Span<byte> buffer  = stackalloc byte[_utf8Bom.Length];
        var        readLen = fsr.Read(buffer);
        if (readLen != _utf8Bom.Length || !buffer.SequenceEqual(_utf8Bom)) return false;
        using var fsw = OpenFileStream(tmpFile, FileMode.OpenOrCreate, FileAccess.Write);
        int       data;
        while ((data = fsr.ReadByte()) != -1) fsw.WriteByte((byte)data);
        return true;
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

        var  tmpFile = $"{file}.tmp";
        bool isReplaced;
        await using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            if (!Opt.WriteMode) { //测试，只读模式
                ShowMessage(0, 1, 0);
                return;
            }

            if (fsr is null) {
                ShowMessage(0, 0, 1);
                return;
            }


            isReplaced = CreateTempFile(fsr, tmpFile);
        }

        if (isReplaced) {
            MoveFile(tmpFile, file);
            ShowMessage(0, 1, 0);
        }
        else {
            ShowMessage(0, 0, 1);
        }
    }


    private void ShowMessage(int procedCnt, int replaceCnt, int breakCnt)
    {
        lock (_lockObj) {
            _procedCnt        += procedCnt;
            _trimCnt          += replaceCnt;
            _breakCnt         += breakCnt;
            _step2Bar.Message =  string.Format(Str.ShowMessageTemp, _procedCnt, _totalCnt, _trimCnt, _breakCnt);
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
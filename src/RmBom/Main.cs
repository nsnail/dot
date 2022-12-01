using System.Diagnostics.CodeAnalysis;

namespace Dot.RmBom;

public sealed class Main : Tool<Option>, IDisposable
{
    private                 int              _breakCnt;
    private                 bool             _disposed;
    private static readonly object           _lockObj = new();
    private                 int              _procedCnt;
    private                 ChildProgressBar _step2Bar;
    private                 int              _totalCnt;
    private                 int              _trimCnt;
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


    private void ShowMessage(int procedCnt, int replaceCnt, int breakCnt)
    {
        lock (_lockObj) {
            _procedCnt        += procedCnt;
            _trimCnt          += replaceCnt;
            _breakCnt         += breakCnt;
            _step2Bar.Message =  string.Format(Strings.ShowMessageTemp, _procedCnt, _totalCnt, _trimCnt, _breakCnt);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public override void Run()
    {
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path), string.Format(Strings.PathNotFound, Opt.Path));


        var       utf8Bom  = new byte[] { 0xef, 0xbb, 0xbf };
        using var step1Bar = new IndeterminateProgressBar(Strings.SearchingFile, DefaultProgressBarOptions);


        var fileList = EnumerateFiles(Opt.Path, Opt.Filter);
        _totalCnt = fileList.Count();

        step1Bar.Message = string.Format(Strings.SearchingFileOK, _totalCnt);
        step1Bar.Finished();
        if (_totalCnt == 0) return;

        _step2Bar = step1Bar.Spawn(_totalCnt, string.Empty, DefaultProgressBarOptions);

        Parallel.ForEach(fileList, file => {
            _step2Bar.Tick();
            ShowMessage(1, 0, 0);

            var tmpFile    = $"{file}.tmp";
            var isReplaced = false;
            using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                if (Opt.ReadOnly) { //测试，只读模式
                    ShowMessage(0, 1, 0);
                    return;
                }

                if (fsr is null) {
                    ShowMessage(0, 0, 1);
                    return;
                }


                Span<byte> buffer  = stackalloc byte[utf8Bom.Length];
                var        readLen = fsr.Read(buffer);
                if (readLen == utf8Bom.Length && buffer.SequenceEqual(utf8Bom)) {
                    using var fsw = OpenFileStream(tmpFile, FileMode.OpenOrCreate, FileAccess.Write);
                    int       data;
                    while ((data = fsr.ReadByte()) != -1) fsw.WriteByte((byte)data);
                    isReplaced = true;
                }
            }

            if (isReplaced) {
                MoveFile(tmpFile, file);
                ShowMessage(0, 1, 0);
            }
            else {
                ShowMessage(0, 0, 1);
            }
        });
    }
}
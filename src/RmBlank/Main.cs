using System.Diagnostics.CodeAnalysis;
using NSExt.Extensions;

namespace Dot.RmBlank;

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
        int spacesCnt;

        await using var fsrw = OpenFileStream(file, FileMode.Open, FileAccess.ReadWrite);

        if (!Opt.WriteMode) { //测试，只读模式
            ShowMessage(0, 1, 0);
            return;
        }


        if (fsrw is null || fsrw.Length == 0 || (spacesCnt = GetSpacesCnt(fsrw)) == 0) {
            ShowMessage(0, 0, 1);
            return;
        }

        fsrw.Seek(0, SeekOrigin.Begin);
        if (!fsrw.IsTextStream()) return;
        ShowMessage(0, 1, 0);
        fsrw.SetLength(fsrw.Length - spacesCnt);
    }

    private static int GetSpacesCnt(Stream fsr)
    {
        var trimLen = 0;
        fsr.Seek(-1, SeekOrigin.End);
        int data;
        while ((data = fsr.ReadByte()) != -1)
            if (new[] { 0x20, 0x0d, 0x0a }.Contains(data)) {
                ++trimLen;
                if (fsr.Position - 2 < 0) break;
                fsr.Seek(-2, SeekOrigin.Current);
            }
            else {
                break;
            }

        return trimLen;
    }


    private void ShowMessage(int procedCnt, int removeCnt, int breakCnt)
    {
        lock (_lockObj) {
            _procedCnt        += procedCnt;
            _replaceCnt       += removeCnt;
            _breakCnt         += breakCnt;
            _step2Bar.Message =  string.Format(Str.ShowMessageTemp, _procedCnt, _totalCnt, _replaceCnt, _breakCnt);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
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
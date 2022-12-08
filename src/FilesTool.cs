using System.Collections.Concurrent;
using Panel = Spectre.Console.Panel;

namespace Dot;

public abstract class FilesTool<TOption> : ToolBase<TOption> where TOption : DirOption
{
    private                 int                               _breakCnt;           //跳过文件数
    private                 ProgressTask                      _childTask;          //子任务进度
    private static readonly object                            _lock = new();       //线程锁
    private                 int                               _readCnt;            //读取文件数
    private                 int                               _totalCnt;           //总文件数
    private                 int                               _writeCnt;           //写入文件数
    private readonly        ConcurrentDictionary<string, int> _writeStats = new(); //写入统计：后缀，数量

    protected FilesTool(TOption opt) : base(opt)
    {
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path), string.Format(Str.PathNotFound, Opt.Path));
    }

    private static string[] EnumerateFiles(string path, string searchPattern)
    {
        var fileList = Directory
                       .EnumerateFiles(path, searchPattern
                               ,             new EnumerationOptions {
                                                                  RecurseSubdirectories = true
                                                                , AttributesToSkip      = FileAttributes.ReparsePoint
                                                              })
                       .Where(x => !new[] { ".git", "node_modules" }.Any(
                                  y => x.Contains(y, StringComparison.OrdinalIgnoreCase)))
                       .ToArray();
        return fileList;
    }


    protected static void CopyFile(string source, string dest)
    {
        try {
            File.Copy(source, dest, true);
        }
        catch (UnauthorizedAccessException) {
            File.SetAttributes(dest, new FileInfo(dest).Attributes & ~FileAttributes.ReadOnly);
            File.Copy(source, dest, true);
        }
    }


    protected FileStream CreateTempFile(out string file)
    {
        file = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.tmp");
        return OpenFileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
    }


    protected abstract ValueTask FileHandle(string file, CancellationToken cancelToken);


    protected static FileStream OpenFileStream(string    file, FileMode mode, FileAccess access
                                             , FileShare share = FileShare.Read)
    {
        FileStream fsr = null;
        try {
            fsr = new FileStream(file, mode, access, share);
        }
        catch (UnauthorizedAccessException) {
            try {
                File.SetAttributes(file, new FileInfo(file).Attributes & ~FileAttributes.ReadOnly);
                fsr = new FileStream(file, mode, access, share);
            }
            catch (Exception) {
                // ignored
            }
        }
        catch (IOException) { }

        return fsr;
    }

    protected void ShowMessage(int readCnt, int writeCnt, int breakCnt)
    {
        lock (_lock) {
            _readCnt  += readCnt;
            _writeCnt += writeCnt;
            _breakCnt += breakCnt;
            if (readCnt > 0) _childTask.Increment(1);
            _childTask.Description
                = $"{Str.Read}: [green]{_readCnt}[/]/{_totalCnt}, {Str.Write}: [red]{_writeCnt}[/], {Str.Break}: [gray]{_breakCnt}[/]";
        }
    }

    protected void UpdateStats(string key)
    {
        _writeStats.AddOrUpdate(key, 1, (_, oldValue) => oldValue + 1);
    }

    public override async Task Run()
    {
        if (!Opt.WriteMode) AnsiConsole.MarkupLine("[gray]{0}[/]", Str.ExerciseMode);
        IEnumerable<string> fileList;
        await AnsiConsole.Progress()
                         .Columns(new ProgressBarColumn()                                //
                                , new ElapsedTimeColumn()                                //
                                , new PercentageColumn()                                 //
                                , new SpinnerColumn()                                    //
                                , new TaskDescriptionColumn { Alignment = Justify.Left } //
                         )
                         .StartAsync(async ctx => {
                             var taskSearchfile = ctx.AddTask(Str.SearchingFile).IsIndeterminate();
                             _childTask = ctx.AddTask("-/-", false);
                             fileList   = EnumerateFiles(Opt.Path, Opt.Filter);
                             _totalCnt  = fileList.Count();
                             taskSearchfile.IsIndeterminate(false);
                             taskSearchfile.Increment(100);

                             _childTask.MaxValue = _totalCnt;
                             _childTask.StartTask();
                             await Parallel.ForEachAsync(fileList, FileHandle);
                         });

        var grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(16))
                             .AddColumn(new GridColumn().Alignment(Justify.Right));

        foreach (var kv in _writeStats) grid.AddRow(kv.Key, kv.Value.ToString());

        AnsiConsole.Write(new Panel(grid).Header(Str.WriteFileStats));
    }
}
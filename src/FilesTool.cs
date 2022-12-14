using System.Collections.Concurrent;
using System.Text.RegularExpressions;
// ReSharper disable once RedundantUsingDirective
using Panel = Spectre.Console.Panel;

namespace Dot;

internal abstract class FilesTool<TOption> : ToolBase<TOption> where TOption : DirOption
{
    private int          _breakCnt;   //跳过文件数
    private ProgressTask _childTask;  //子任务进度
    private int          _excludeCnt; //排除文件数

    // ReSharper disable once StaticMemberInGenericType
    #pragma warning disable S2743
    private static readonly object                            _lock = new();       //线程锁
    private                 int                               _readCnt;            //读取文件数
    private                 int                               _totalCnt;           //总文件数
    private                 int                               _writeCnt;           //写入文件数
    private readonly        ConcurrentDictionary<string, int> _writeStats = new(); //写入统计：后缀，数量

    private async Task CoreInternal()
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
                             fileList   = EnumerateFiles(Opt.Path, Opt.Filter, out _excludeCnt);
                             _totalCnt  = fileList.Count();
                             taskSearchfile.StopTask();

                             _childTask.MaxValue = _totalCnt;
                             _childTask.StartTask();
                             await Parallel.ForEachAsync(fileList, FileHandle);
                         });

        var grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(16))
                             .AddColumn(new GridColumn().Alignment(Justify.Right));

        foreach (var kv in _writeStats.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
            grid.AddRow(kv.Key, kv.Value.ToString());

        AnsiConsole.Write(new Panel(grid).Header(Str.WriteFileStats));
    }


    // ReSharper disable once ReturnTypeCanBeEnumerable.Local
    private string[] EnumerateFiles(string path, string searchPattern, out int excludeCnt)
    {
        var exCnt = 0;
        if (Opt.ExcludeRegexes?.FirstOrDefault() is null) //默认排除.git 、 node_modules 目录
            Opt.ExcludeRegexes = new[] { @"\.git", "node_modules" };
        var excludeRegexes = Opt.ExcludeRegexes.Select(x => new Regex(x));
        var fileList = Directory.EnumerateFiles(path, searchPattern
                                              , new EnumerationOptions {
                                                                           RecurseSubdirectories = true
                                                                         , AttributesToSkip
                                                                               = FileAttributes.ReparsePoint
                                                                         , IgnoreInaccessible = true
                                                                         , MaxRecursionDepth  = Opt.MaxRecursionDepth
                                                                       })
                                .Where(x => {
                                    if (!excludeRegexes.Any(y => y.IsMatch(x))) return true;
                                    ++exCnt;
                                    return false;
                                })
                                .ToArray();
        excludeCnt = exCnt;
        return fileList;
    }

    protected override Task Core()
    {
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path), string.Format(Str.PathNotFound, Opt.Path));
        return CoreInternal();
    }


    protected static FileStream CreateTempFile(out string file)
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
        catch (IOException) {
            // ignored
        }

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
                = $"{Str.Read}: [green]{_readCnt}[/]/{_totalCnt}, {Str.Write}: [red]{_writeCnt}[/], {Str.Break}: [gray]{_breakCnt}[/], {Str.Exclude}: [yellow]{_excludeCnt}[/]";
        }
    }


    protected void UpdateStats(string key)
    {
        _writeStats.AddOrUpdate(key, 1, (_, oldValue) => oldValue + 1);
    }
}
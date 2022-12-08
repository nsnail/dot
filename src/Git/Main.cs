using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using NSExt.Extensions;

namespace Dot.Git;

public class Main : ToolBase<Option>
{
    private readonly Encoding                                               _gitOutputEnc; //git command rsp 编码
    private          ConcurrentDictionary<string, StringBuilder>            _repoRsp;      //仓库信息容器
    private          ConcurrentDictionary<string, TaskStatusColumn.Statues> _repoStatus;

    public Main(Option opt) : base(opt)
    {
        _gitOutputEnc = Encoding.GetEncoding(Opt.GitOutputEncoding);
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path) //
                                      , string.Format(Str.PathNotFound, Opt.Path));
    }


    private async ValueTask DirHandle(KeyValuePair<string, ProgressTask> payload, CancellationToken _)
    {
        payload.Value.StartTask();
        payload.Value.State.Status(TaskStatusColumn.Statues.Executing);

        // 打印 git command rsp
        void ExecRspReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            var msg = Encoding.UTF8.GetString(_gitOutputEnc.GetBytes(e.Data));
            _repoRsp[payload.Key].Append(msg.EscapeMarkup());
        }

        // 启动git进程
        {
            var startInfo = new ProcessStartInfo {
                                                     CreateNoWindow         = true
                                                   , WorkingDirectory       = payload.Key
                                                   , FileName               = "git"
                                                   , Arguments              = Opt.Args
                                                   , UseShellExecute        = false
                                                   , RedirectStandardOutput = true
                                                   , RedirectStandardError  = true
                                                 };
            using var p = Process.Start(startInfo);
            p!.OutputDataReceived += ExecRspReceived;
            p.ErrorDataReceived   += ExecRspReceived;
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            await p.WaitForExitAsync();

            payload.Value.IsIndeterminate(false);
            if (p.ExitCode == 0) {
                payload.Value.State.Status(TaskStatusColumn.Statues.Succeed);
                _repoStatus.AddOrUpdate(payload.Key, _ => TaskStatusColumn.Statues.Succeed
                                      , (_, _) => TaskStatusColumn.Statues.Succeed);
                payload.Value.Increment(100);
            }
            else {
                payload.Value.State.Status(TaskStatusColumn.Statues.Failed);
                _repoStatus.AddOrUpdate(payload.Key, _ => TaskStatusColumn.Statues.Failed
                                      , (_, _) => TaskStatusColumn.Statues.Failed);
                payload.Value.Increment(0);
            }
        }
    }


    protected override async Task Core()
    {
        var progressBar = new ProgressBarColumn { Width = 10 };
        await AnsiConsole.Progress()
                         .Columns(progressBar                                            //
                                , new ElapsedTimeColumn()                                //
                                , new SpinnerColumn()                                    //
                                , new TaskStatusColumn()                                 //
                                , new TaskDescriptionColumn { Alignment = Justify.Left } //
                         )
                         .StartAsync(async ctx => {
                             var taskFinder = ctx.AddTask(string.Format(Str.FindGitReps, Opt.Path)).IsIndeterminate();
                             var paths = Directory.GetDirectories(Opt.Path, ".git"       //
                                                                , new EnumerationOptions //
                                                                  {
                                                                      MaxRecursionDepth     = Opt.MaxRecursionDepth
                                                                    , RecurseSubdirectories = true
                                                                    , IgnoreInaccessible    = true
                                                                    , AttributesToSkip
                                                                          = FileAttributes.ReparsePoint
                                                                  })
                                                  .Select(x => Directory.GetParent(x)!.FullName);

                             _repoRsp    = new ConcurrentDictionary<string, StringBuilder>();
                             _repoStatus = new ConcurrentDictionary<string, TaskStatusColumn.Statues>();
                             var tasks = new Dictionary<string, ProgressTask>();
                             foreach (var path in paths) {
                                 _repoRsp.TryAdd(path, new StringBuilder());
                                 _repoStatus.TryAdd(path, default);
                                 var task = ctx.AddTask(new DirectoryInfo(path).Name, false).IsIndeterminate();
                                 tasks.Add(path, task);
                             }

                             taskFinder.IsIndeterminate(false);
                             taskFinder.Increment(100);
                             taskFinder.State.Status(TaskStatusColumn.Statues.Succeed);

                             await Parallel.ForEachAsync(tasks, DirHandle);
                         });

        var table = new Table().AddColumn(new TableColumn(Str.Repository) { Width = 50 })
                               .AddColumn(new TableColumn(Str.Command))
                               .AddColumn(new TableColumn(Str.Response) { Width = 50 })
                               .Caption(
                                   $"{Str.ZeroCode}: [green]{_repoStatus.Count(x => x.Value == TaskStatusColumn.Statues
                                                                                   .Succeed)}[/]/{_repoStatus.Count}");


        foreach (var repo in _repoRsp) {
            var status = _repoStatus[repo.Key].Desc();
            table.AddRow(status.Replace(_repoStatus[repo.Key].ToString(), new DirectoryInfo(repo.Key).Name), Opt.Args
                       , status.Replace(_repoStatus[repo.Key].ToString(), repo.Value.ToString()));
        }

        AnsiConsole.Write(table);
    }
}
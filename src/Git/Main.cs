// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using NSExt.Extensions;

namespace Dot.Git;

[Description(nameof(Ln.GitTool))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    private Encoding                                               _gitOutputEnc; // git command rsp 编码
    private ConcurrentDictionary<string, StringBuilder>            _repoRsp;      // 仓库信息容器
    private ConcurrentDictionary<string, TaskStatusColumn.Statues> _repoStatus;

    protected override Task Core()
    {
        return !Directory.Exists(Opt.Path)
            ? throw new ArgumentException( //
                nameof(Opt.Path)           //
              , string.Format(CultureInfo.InvariantCulture, Ln.PathNotFound, Opt.Path))
            : CoreInternal();
    }

    private async Task CoreInternal()
    {
        _gitOutputEnc = Encoding.GetEncoding(Opt.GitOutputEncoding);
        var progressBar = new ProgressBarColumn { Width = 10 };
        await AnsiConsole.Progress()
                         .Columns(                                                   //
                             progressBar                                             //
                           , new ElapsedTimeColumn()                                 //
                           , new SpinnerColumn()                                     //
                           , new TaskStatusColumn()                                  //
                           , new TaskDescriptionColumn { Alignment = Justify.Left }) //
                         .StartAsync(async ctx => {
                             var taskFinder = ctx
                                              .AddTask(string.Format(CultureInfo.InvariantCulture, Ln.FindGitReps
                                                                   , Opt.Path))
                                              .IsIndeterminate();
                             var paths = Directory.GetDirectories(Opt.Path, ".git"       //
                                                                , new EnumerationOptions //
                                                                  {
                                                                      MaxRecursionDepth = Opt.MaxRecursionDepth
                                                                    , RecurseSubdirectories = true
                                                                    , IgnoreInaccessible = true
                                                                    , AttributesToSkip = FileAttributes.ReparsePoint
                                                                  })
                                                  .Select(x => Directory.GetParent(x)!.FullName);

                             _repoRsp    = new ConcurrentDictionary<string, StringBuilder>();
                             _repoStatus = new ConcurrentDictionary<string, TaskStatusColumn.Statues>();
                             var tasks = new Dictionary<string, ProgressTask>();
                             foreach (var path in paths) {
                                 _ = _repoRsp.TryAdd(path, new StringBuilder());
                                 _ = _repoStatus.TryAdd(path, default);
                                 var task = ctx.AddTask(new DirectoryInfo(path).Name, false).IsIndeterminate();
                                 tasks.Add(path, task);
                             }

                             taskFinder.StopTask();
                             taskFinder.State.Status(TaskStatusColumn.Statues.Succeed);

                             await Parallel.ForEachAsync(tasks, DirHandle);
                         });

        var table = new Table().AddColumn(new TableColumn(Ln.Repository) { Width = 50 })
                               .AddColumn(new TableColumn(Ln.Command))
                               .AddColumn(new TableColumn(Ln.Response) { Width = 50 })
                               .Caption(
                                   $"{Ln.ZeroCode}: [green]{_repoStatus.Count(x => x.Value == TaskStatusColumn.Statues
                                                                                  .Succeed)}[/]/{_repoStatus.Count}");

        foreach (var repo in _repoRsp) {
            var status = _repoStatus[repo.Key].Desc();
            table.AddRow(status.Replace(_repoStatus[repo.Key].ToString(), new DirectoryInfo(repo.Key).Name), Opt.Args
                       , status.Replace(_repoStatus[repo.Key].ToString(), repo.Value.ToString()));
        }

        AnsiConsole.Write(table);
    }

    #pragma warning disable SA1313
    private async ValueTask DirHandle(KeyValuePair<string, ProgressTask> payload, CancellationToken _)
        #pragma warning restore SA1313
    {
        payload.Value.StartTask();
        payload.Value.State.Status(TaskStatusColumn.Statues.Executing);

        // 打印 git command rsp
        void ExecRspReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) {
                return;
            }

            var msg = Encoding.UTF8.GetString(_gitOutputEnc.GetBytes(e.Data));
            _repoRsp[payload.Key].Append(msg.EscapeMarkup());
        }

        // 启动git进程
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
        await p.WaitForExitAsync(CancellationToken.None);

        if (p.ExitCode == 0) {
            payload.Value.State.Status(TaskStatusColumn.Statues.Succeed);
            _repoStatus.AddOrUpdate(payload.Key, _ => TaskStatusColumn.Statues.Succeed
                                  , (_, _) => TaskStatusColumn.Statues.Succeed);
            payload.Value.StopTask();
        }
        else {
            payload.Value.State.Status(TaskStatusColumn.Statues.Failed);
            _repoStatus.AddOrUpdate(payload.Key, _ => TaskStatusColumn.Statues.Failed
                                  , (_, _) => TaskStatusColumn.Statues.Failed);
        }

        payload.Value.StopTask();
    }
}
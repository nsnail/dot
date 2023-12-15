// ReSharper disable ClassNeverInstantiated.Global

using System.Collections.Concurrent;
using System.Diagnostics;
using NSExt.Extensions;
#if NET8_0_WINDOWS
using Microsoft.Win32;
#endif

namespace Dot.Git;

[Description(nameof(Ln.Git批量操作工具))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    #if NET8_0_WINDOWS
    private static readonly Regex _regex = new("url = (http.*)");
    #endif
    private Encoding                                               _gitOutputEnc; // git command rsp 编码
    private ConcurrentDictionary<string, StringBuilder>            _repoRsp;      // 仓库信息容器
    private ConcurrentDictionary<string, TaskStatusColumn.Statues> _repoStatus;

    protected override Task CoreAsync()
    {
        #if NET8_0_WINDOWS
        #pragma warning disable IDE0046
        if (Opt.InstallRightClickMenu) {
            #pragma warning restore IDE0046
            return InstallRightClickMenuAsync();
        }

        if (Opt.UninstallRightClickMenu) {
            return UninstallRightClickMenuAsync();
        }

        if (Opt.OpenGitUrl) {
            return OpenGitUrlAsync();
        }
        #endif

        return !Directory.Exists(Opt.Path)
            ? throw new ArgumentException($"{Ln.指定的路径不存在}: {Opt.Path}", "PATH")
            : CoreInternalAsync();
    }

    private async Task CoreInternalAsync()
    {
        _gitOutputEnc = Encoding.GetEncoding(Opt.Git输出编码);
        var progressBar = new ProgressBarColumn { Width = 10 };
        await AnsiConsole.Progress()
                         .Columns(                                                   //
                             progressBar                                             //
                           , new ElapsedTimeColumn()                                 //
                           , new SpinnerColumn()                                     //
                           , new TaskStatusColumn()                                  //
                           , new TaskDescriptionColumn { Alignment = Justify.Left }) //
                         .StartAsync(ctx => {
                             var taskFinder = ctx.AddTask($"{Ln.查找此目录下所有Git仓库目录}: {Opt.Path}").IsIndeterminate();
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

                             return Parallel.ForEachAsync(tasks, DirHandleAsync);
                         })
                         .ConfigureAwait(false);

        var table = new Table().AddColumn(new TableColumn(Ln.仓库) { Width = 50 })
                               .AddColumn(new TableColumn(Ln.命令))
                               .AddColumn(new TableColumn(Ln.响应) { Width = 50 })
                               .Caption(
                                   $"{Ln.Git退出码为零的}: [green]{_repoStatus.Count(x => x.Value == TaskStatusColumn.Statues
                                                                                   .Succeed)}[/]/{_repoStatus.Count}");

        foreach (var repo in _repoRsp) {
            var status = _repoStatus[repo.Key].ResDesc<Ln>();
            _ = table.AddRow( //
                status.Replace(_repoStatus[repo.Key].ToString(), new DirectoryInfo(repo.Key).Name), Opt.Args
              , status.Replace(_repoStatus[repo.Key].ToString(), repo.Value.ToString()));
        }

        AnsiConsole.Write(table);
    }

    private async ValueTask DirHandleAsync(KeyValuePair<string, ProgressTask> payload, CancellationToken ct)
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
            _ = _repoRsp[payload.Key].Append(msg.EscapeMarkup());
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
        await p.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);

        if (p.ExitCode == 0) {
            payload.Value.State.Status(TaskStatusColumn.Statues.Succeed);
            _ = _repoStatus.AddOrUpdate(payload.Key, _ => TaskStatusColumn.Statues.Succeed
                                      , (_, _) => TaskStatusColumn.Statues.Succeed);
            payload.Value.StopTask();
        }
        else {
            payload.Value.State.Status(TaskStatusColumn.Statues.Failed);
            _ = _repoStatus.AddOrUpdate(payload.Key, _ => TaskStatusColumn.Statues.Failed
                                      , (_, _) => TaskStatusColumn.Statues.Failed);
        }

        payload.Value.StopTask();
    }

    #if NET8_0_WINDOWS
    #pragma warning disable SA1204
    private static Task InstallRightClickMenuAsync()
        #pragma warning restore SA1204
    {
        Registry.SetValue(@"HKEY_CLASSES_ROOT\Directory\shell\OpenGitUrl\command", string.Empty
                        , @$"""{Environment.ProcessPath}"" git -o ""%1""");
        Console.WriteLine(Ln.Windows右键菜单已添加);
        return Task.CompletedTask;
    }

    private static Task UninstallRightClickMenuAsync()
    {
        Registry.ClassesRoot.OpenSubKey(@"Directory\shell", true)?.DeleteSubKeyTree("OpenGitUrl", false);
        Console.WriteLine(Ln.Windows右键菜单已卸载);
        return Task.CompletedTask;
    }

    private Task OpenGitUrlAsync()
    {
        var file = @$"{Opt.Path}\.git\config";
        if (File.Exists(file)) {
            var content = File.ReadAllText(file);
            Console.WriteLine(content);
            var url = _regex.Match(content).Groups[1].Value;
            _ = Process.Start("explorer.exe", url);
        }

        return Task.CompletedTask;
    }
    #endif
}
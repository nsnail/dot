using System.Diagnostics;
using System.Text;
using NSExt.Extensions;

namespace Dot.Git;

public class Main : ToolBase<Option>
{
    private const    int            _POS_Y_MSG             = 74; //git command rsp 显示的位置 y
    private const    int            _POST_Y_LOADING        = 70; //loading 动画显示的位置 y
    private const    int            _REP_PATH_LENGTH_LIMIT = 32; //仓库路径长度显示截断阈值
    private          (int x, int y) _cursorPosBackup;            //光标位置备份
    private readonly Encoding       _gitOutputEnc;               //git command rsp 编码
    private          List<string>   _repoPathList;               //仓库目录列表


    public Main(Option opt) : base(opt)
    {
        _gitOutputEnc = Encoding.GetEncoding(Opt.GitOutputEncoding);
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path) //
                                      , string.Format(Str.PathNotFound, Opt.Path));
    }


    private async ValueTask DirHandle(string dir, CancellationToken cancelToken)
    {
        var row      = _repoPathList.FindIndex(x => x == dir); // 行号
        var tAnimate = LoadingAnimate(_POST_Y_LOADING, _cursorPosBackup.y + row, out var cts);

        // 打印 git command rsp
        void ExecRspReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            var msg = Encoding.UTF8.GetString(_gitOutputEnc.GetBytes(e.Data));
            ConcurrentWrite(_POS_Y_MSG, _cursorPosBackup.y + row, new string(' ', Console.WindowWidth - _POS_Y_MSG));
            ConcurrentWrite(_POS_Y_MSG, _cursorPosBackup.y + row, msg);
        }

        // 启动git进程
        {
            var startInfo = new ProcessStartInfo {
                                                     CreateNoWindow         = true
                                                   , WorkingDirectory       = dir
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
        }

        cts.Cancel();
        await tAnimate;
        cts.Dispose();
    }

    private void StashCurorPos()
    {
        _cursorPosBackup = Console.GetCursorPosition();
    }


    public override async Task Run()
    {
        // 查找git仓库目录
        {
            Console.Write(Str.FindGitReps, Opt.Path);
            StashCurorPos();

            var tAnimate = LoadingAnimate(_cursorPosBackup.x, _cursorPosBackup.y, out var cts);
            _repoPathList = Directory.GetDirectories(Opt.Path, ".git", SearchOption.AllDirectories)
                                     .Select(x => Directory.GetParent(x)!.FullName)
                                     .ToList();
            cts.Cancel();
            await tAnimate;
            cts.Dispose();
        }

        // 打印git仓库目录
        {
            Console.WriteLine(Str.Ok);
            StashCurorPos();

            var i = 0;
            Console.WriteLine( //
                string.Join(Environment.NewLine
                          , _repoPathList.Select(
                                x => $"{++i}: {new DirectoryInfo(x).Name.Sub(0, _REP_PATH_LENGTH_LIMIT)}"))
                //
            );
        }

        // 并行执行git命令
        await Parallel.ForEachAsync(_repoPathList, DirHandle);
        Console.SetCursorPosition(_cursorPosBackup.x, _cursorPosBackup.y + _repoPathList.Count);
    }
}
using System.Diagnostics;
using System.Text;
using NSExt.Extensions;

namespace Dot.Git;

public class Main : ToolBase<Option>
{
    private const int            _POS_Y_MSG      = 74;
    private const int            _POST_Y_LOADING = 70;
    private const int            _REP_MAX_LENGTH = 32;
    private       (int x, int y) _cursorInitPos;
    private       List<string>   _dirList;
    private       Encoding       _encGbk;


    public Main(Option opt) : base(opt) { }


    private async ValueTask DirHandle(string dir, CancellationToken cancelToken)
    {
        var index    = _dirList.FindIndex(x => x == dir);
        var tAnimate = LoadingAnimate(_POST_Y_LOADING, _cursorInitPos.y + index, out var cts);

        void Write(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            var msg = Encoding.UTF8.GetString(_encGbk.GetBytes(e.Data));
            ConcurrentWrite(_POS_Y_MSG, _cursorInitPos.y + index, new string(' ', Console.WindowWidth - _POS_Y_MSG));
            ConcurrentWrite(_POS_Y_MSG, _cursorInitPos.y + index, msg);
        }


        var gitStartInfo = new ProcessStartInfo {
                                                    CreateNoWindow         = true
                                                  , WorkingDirectory       = dir
                                                  , FileName               = "git"
                                                  , Arguments              = Opt.Args
                                                  , UseShellExecute        = false
                                                  , RedirectStandardOutput = true
                                                  , RedirectStandardError  = true
                                                };
        using var p = Process.Start(gitStartInfo);
        p.OutputDataReceived += Write;
        p.ErrorDataReceived  += Write;
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        await p.WaitForExitAsync();


        cts.Cancel();
        await tAnimate;
        cts.Dispose();
    }


    public override async Task Run()
    {
        if (!Directory.Exists(Opt.Path))
            throw new ArgumentException(nameof(Opt.Path), string.Format(Str.PathNotFound, Opt.Path));

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encGbk = Encoding.GetEncoding("gbk");

        Console.Write(Str.FindGitReps, Opt.Path);
        _cursorInitPos = Console.GetCursorPosition();
        var tAnimate = LoadingAnimate(_cursorInitPos.x, _cursorInitPos.y, out var cts);

        _dirList = Directory.GetDirectories(Opt.Path, ".git", SearchOption.AllDirectories)
                            .Select(x => Directory.GetParent(x)!.FullName)
                            .ToList();
        cts.Cancel();
        await tAnimate;

        cts.Dispose();

        Console.WriteLine(Str.Ok);
        _cursorInitPos = Console.GetCursorPosition();
        var i = 0;
        Console.WriteLine(string.Join(Environment.NewLine
                                    , _dirList.Select(
                                          x => $"{++i}: {new DirectoryInfo(x).Name.Sub(0, _REP_MAX_LENGTH)}")));


        await Parallel.ForEachAsync(_dirList, DirHandle);

        Console.SetCursorPosition(_cursorInitPos.x, _cursorInitPos.y + _dirList.Count);
    }
}
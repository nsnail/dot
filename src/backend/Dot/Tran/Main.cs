#if NET8_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NSExt.Extensions;

namespace Dot.Tran;

[Description(nameof(Ln.翻译工具))]
[Localization(typeof(Ln))]

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Main : ToolBase<Option>
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    protected override Task CoreAsync()
    {
        AnsiConsole.MarkupLine(Ln.选中文本按下Capslock开始翻译);
        AnsiConsole.MarkupLine(Ln.按下Esc隐藏译文);
        var th = new Thread(() => {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.ThreadException                += UIThreadException;
            using var frm = new WinMain();
            try {
                Application.Run();
            }
            catch (Exception ex) {
                Log(ex.Json());
            }
        });
        th.SetApartmentState(ApartmentState.STA);
        th.Start();
        th.Join();
        return Task.CompletedTask;
    }

    private static void Log(string msg)
    {
        var file = Path.Combine(Path.GetTempPath(), $"{DateTime.Now.yyyyMMdd()}.dotlog");
        File.AppendAllText(file, Environment.NewLine + msg);
    }

    private static void UIThreadException(object sender, ThreadExceptionEventArgs e)
    {
        Log(e.Json());
    }

    private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log(e.Json());
    }
}

#endif
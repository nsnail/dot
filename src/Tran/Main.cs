#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NSExt.Extensions;

namespace Dot.Tran;

[Description(nameof(Str.TranslateTool))]
[Localization(typeof(Str))]
internal sealed class Main : ToolBase<Option>
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    protected override Task Core()
    {
        AnsiConsole.MarkupLine(Str.StartTranslate);
        AnsiConsole.MarkupLine(Str.HideTranslate);
        var th = new Thread(() => {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.ThreadException                += UIThreadException;
            using var frm = new WinMain();
            Application.Run();
        });
        th.SetApartmentState(ApartmentState.STA);
        th.Start();
        th.Join();
        return Task.CompletedTask;
    }

    private static void Log(string msg)
    {
        var file = Path.Combine(Path.GetTempPath(), $"{DateTime.Now.yyyyMMdd()}.dotlog");
        File.AppendAllText(file, $"{Environment.NewLine}{msg}");
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
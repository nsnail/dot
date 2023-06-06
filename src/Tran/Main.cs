#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NSExt.Extensions;

namespace Dot.Tran;

[Description(nameof(Ln.TranslateTool))]
[Localization(typeof(Ln))]

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Main : ToolBase<Option>
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    protected override Task CoreAsync()
    {
        AnsiConsole.MarkupLine(Ln.StartTranslate);
        AnsiConsole.MarkupLine(Ln.HideTranslate);
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
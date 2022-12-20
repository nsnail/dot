#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

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
            using var frm = new FrmMain();
            Application.Run();
        });
        th.SetApartmentState(ApartmentState.STA);
        th.Start();
        th.Join();
        return Task.CompletedTask;
    }
}

#endif
#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Dot.Tran;

internal sealed class Main : ToolBase<Option>
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    protected override Task Core()
    {
        var th = new Thread(() => { Application.Run(new FrmMain()); });
        th.SetApartmentState(ApartmentState.STA);
        th.Start();
        th.Join();
        return Task.CompletedTask;
    }
}

#endif
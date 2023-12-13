#if NET8_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Dot.Color;

[Description(nameof(Ln.屏幕坐标颜色选取工具))]
[Localization(typeof(Ln))]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Main : ToolBase<Option>
{
    protected override Task CoreAsync()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}

#endif
#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Dot.Color;

[Description(nameof(Str.ScreenPixelTool))]
[Localization(typeof(Str))]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class Main : ToolBase<Option>
{
    protected override Task Core()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}

#endif
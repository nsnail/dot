// ReSharper disable ClassNeverInstantiated.Global

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if NET7_0_WINDOWS
namespace Dot.Color;

[Description(nameof(Str.ScreenPixelTool))]
[Localization(typeof(Str))]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class Main : ToolBase<Option>

{
    protected override Task Core()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}

#endif
// ReSharper disable ClassNeverInstantiated.Global

#if NET7_0_WINDOWS
namespace Dot.Color;

[Description(nameof(Str.ScreenPixelTool))]
[Localization(typeof(Str))]
internal sealed class Main : ToolBase<Option>

{
    protected override Task Core()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}

#endif
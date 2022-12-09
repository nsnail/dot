#if NET7_0_WINDOWS
namespace Dot.Color;

[Description(nameof(Str.ScreenPixelTool))]
[Localization(typeof(Str))]
public sealed class Main : ToolBase<Option>

{
    protected override Task Core()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}

#endif
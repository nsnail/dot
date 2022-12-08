namespace Dot.Color;

public sealed class Main : ToolBase<Option>

{
    public Main(Option opt) : base(opt) { }

    protected override Task Core()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}
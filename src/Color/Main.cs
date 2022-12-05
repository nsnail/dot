namespace Dot.Color;

public sealed class Main : ToolBase<Option>

{
    public Main(Option opt) : base(opt) { }

    public override Task Run()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}
namespace Dot.Color;

public sealed class Main : Tool<Option>

{
    public Main(Option opt) : base(opt) { }

    public override Task Run()
    {
        Application.Run(new WinMain());
        return Task.CompletedTask;
    }
}
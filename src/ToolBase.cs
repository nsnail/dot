namespace Dot;

public abstract class ToolBase<TOption> : Command<TOption> where TOption : OptionBase
{
    protected          TOption Opt { get; set; }
    protected abstract Task    Core();

    public override int Execute(CommandContext context, TOption option)
    {
        Opt = option;
        Run().Wait();
        return 0;
    }

    public virtual async Task Run()
    {
        await Core();
        if (Opt.KeepSession) {
            AnsiConsole.MarkupLine(Str.PressAnyKey);
            AnsiConsole.Console.Input.ReadKey(true);
        }
    }
}
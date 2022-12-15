namespace Dot;

internal abstract class ToolBase<TOption> : Command<TOption>
    where TOption : OptionBase
{
    protected TOption Opt { get; private set; }

    public override int Execute(CommandContext context, TOption settings)
    {
        Opt = settings;
        Run().Wait();
        return 0;
    }

    protected abstract Task Core();

    protected virtual async Task Run()
    {
        await Core();
        if (Opt.KeepSession) {
            AnsiConsole.MarkupLine(Str.PressAnyKey);
            _ = AnsiConsole.Console.Input.ReadKey(true);
        }
    }
}
namespace Dot;

internal abstract class ToolBase<TOption> : Command<TOption>
    where TOption : OptionBase
{
    protected TOption Opt { get; private set; }

    public override int Execute(CommandContext context, TOption settings)
    {
        Opt = settings;
        RunAsync().Wait();
        return 0;
    }

    protected abstract Task CoreAsync();

    protected virtual async Task RunAsync()
    {
        await CoreAsync();
        if (Opt.KeepSession) {
            AnsiConsole.MarkupLine(Ln.PressAnyKey);
            _ = AnsiConsole.Console.Input.ReadKey(true);
        }
    }
}
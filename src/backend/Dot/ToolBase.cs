namespace Dot;

internal abstract class ToolBase<TOption> : Command<TOption>
    where TOption : OptionBase
{
    protected TOption Opt { get; private set; }

    public override int Execute(CommandContext context, TOption settings)
    {
        Opt = settings;
        RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        return 0;
    }

    protected abstract Task CoreAsync();

    protected virtual async Task RunAsync()
    {
        await CoreAsync().ConfigureAwait(false);
        if (Opt.执行命令后保留会话) {
            AnsiConsole.MarkupLine(Ln.按下任意键继续);
            _ = AnsiConsole.Console.Input.ReadKey(true);
        }
    }
}
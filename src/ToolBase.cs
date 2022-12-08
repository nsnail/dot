namespace Dot;

public abstract class ToolBase<TOption> : ITool where TOption : OptionBase
{
    // ReSharper disable once StaticMemberInGenericType
    private static SpinLock _spinlock;


    protected TOption Opt { get; }

    protected ToolBase(TOption opt)
    {
        Opt = opt;
    }


    protected static void ConcurrentWrite(int x, int y, string text)
    {
        var lockTaken = false;
        try {
            _spinlock.Enter(ref lockTaken);
            Console.SetCursorPosition(x, y);
            Console.Write(text);
        }
        finally {
            if (lockTaken) _spinlock.Exit(false);
        }
    }

    protected abstract Task Core();


    protected static Task LoadingAnimate(int x, int y, out CancellationTokenSource cts)
    {
        char[] animateChars = { '-', '\\', '|', '/' };
        long   counter      = 0;

        cts = new CancellationTokenSource();
        var cancelToken = cts.Token;

        return Task.Run(async () => {
            for (;;) {
                if (cancelToken.IsCancellationRequested) {
                    ConcurrentWrite(x, y, @" ");
                    return;
                }

                ConcurrentWrite(x, y, animateChars[counter++ % 4].ToString());

                await Task.Delay(100);
            }
        });
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
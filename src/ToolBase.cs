namespace Dot;

public abstract class ToolBase<TOption> : ITool where TOption : OptionBase
{
    // ReSharper disable once StaticMemberInGenericType
    private static SpinLock _spinlock;

    protected readonly ProgressBarOptions //
        DefaultProgressBarOptions = new() {
                                              MessageEncodingName = "utf-8"
                                            , ProgressBarOnBottom = true
                                            , ForegroundColor     = ConsoleColor.Yellow
                                            , ForegroundColorDone = ConsoleColor.DarkGreen
                                            , BackgroundColor     = ConsoleColor.DarkGray
                                            , BackgroundCharacter = '\u2500'
                                            , ProgressCharacter   = '\u2500'
                                          };

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

    protected static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
    {
        var fileList = Directory
                       .EnumerateFiles(path, searchPattern
                               ,             new EnumerationOptions {
                                                                  RecurseSubdirectories = true
                                                                , AttributesToSkip      = FileAttributes.ReparsePoint
                                                              })
                       .Where(x => !new[] { ".git", "node_modules" }.Any(
                                  y => x.Contains(y, StringComparison.OrdinalIgnoreCase)));
        return fileList;
    }

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

    protected static void MoveFile(string source, string dest)
    {
        try {
            File.Move(source, dest, true);
        }
        catch (UnauthorizedAccessException) {
            File.SetAttributes(dest, new FileInfo(dest).Attributes & ~FileAttributes.ReadOnly);
            File.Move(source, dest, true);
        }
    }

    protected static FileStream OpenFileStream(string    file, FileMode mode, FileAccess access
                                             , FileShare share = FileShare.Read)
    {
        FileStream fsr = null;
        try {
            fsr = new FileStream(file, mode, access, share);
        }
        catch (UnauthorizedAccessException) {
            try {
                File.SetAttributes(file, new FileInfo(file).Attributes & ~FileAttributes.ReadOnly);
                fsr = new FileStream(file, mode, access, share);
            }
            catch (Exception) {
                // ignored
            }
        }
        catch (IOException) { }

        return fsr;
    }

    public abstract Task Run();
}
namespace Dot;

public abstract class Tool<TOption> : ITool
{
    protected readonly ProgressBarOptions //
        DefaultProgressBarOptions = new() {
                                              MessageEncodingName = "utf-8"
                                            , ProgressBarOnBottom = true
                                            , ForegroundColor     = ConsoleColor.Yellow
                                            , ForegroundColorDone = ConsoleColor.DarkGreen
                                            , BackgroundColor     = ConsoleColor.DarkGray
                                            , BackgroundCharacter = '\u2593'
                                          };

    protected virtual TOption Opt { get; set; }

    protected Tool(TOption opt)
    {
        Opt = opt;
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

    protected static FileStream OpenFileToWrite(string file)
    {
        FileStream fsr;
        try {
            fsr = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }
        catch (UnauthorizedAccessException) {
            File.SetAttributes(file, new FileInfo(file).Attributes & ~FileAttributes.ReadOnly);
            fsr = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        }

        return fsr;
    }


    public abstract void Run();
}
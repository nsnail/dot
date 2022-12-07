namespace Dot.Git;

[Verb("git", HelpText = nameof(Str.GitTool), ResourceType = typeof(Str))]
public class Option : OptionBase
{
    [Option('a', "args", HelpText = nameof(Str.GitArgs), Default = "status", ResourceType = typeof(Str))]
    public string Args { get; set; }

    [Option('e', "git-output-encoding", HelpText = nameof(Str.GitOutputEncoding), Default = "utf-8"
          , ResourceType = typeof(Str))]
    public string GitOutputEncoding { get; set; }

    [Option('d', "max-recursion-depth", HelpText = nameof(Str.MaxRecursionDepth), Default = int.MaxValue
          , ResourceType = typeof(Str))]
    public int MaxRecursionDepth { get; set; }

    [Value(0, HelpText = nameof(Str.FolderPath), Default = ".", ResourceType = typeof(Str))]
    public string Path { get; set; }
}
namespace Dot.Git;

[Verb("git", HelpText = nameof(Str.GitTool), ResourceType = typeof(Str))]
public class Option : OptionBase
{
    [Option('a', "args", HelpText = nameof(Str.GitArgs), Default = "status", ResourceType = typeof(Str))]
    public string Args { get; set; }

    [Value(0, HelpText = nameof(Str.FolderPath), Default = ".", ResourceType = typeof(Str))]
    public string Path { get; set; }
}
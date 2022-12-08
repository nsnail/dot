namespace Dot;

public class DirOption : OptionBase
{
    [Option('e', "exclude", HelpText = nameof(Str.ExcludePathRegexes), ResourceType = typeof(Str))]
    public IEnumerable<string> ExcludeRegexes { get; set; }

    [Option('f', "filter", HelpText = nameof(Str.FileSearchPattern), Default = "*", ResourceType = typeof(Str))]
    public string Filter { get; set; }


    [Option('d', "max-depth", HelpText = nameof(Str.MaxRecursionDepth), Default = int.MaxValue
          , ResourceType = typeof(Str))]
    public int MaxRecursionDepth { get; set; }

    [Value(0, HelpText = nameof(Str.FolderPath), Default = ".", ResourceType = typeof(Str))]
    public string Path { get; set; }


    [Option('w', "write", HelpText = nameof(Str.WriteMode), Default = false, ResourceType = typeof(Str))]
    public bool WriteMode { get; set; }
}
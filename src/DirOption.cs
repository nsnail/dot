namespace Dot;

public class DirOption : IOption
{
    [Option('f', "filter", Required = false, HelpText = nameof(Strings.FileSearchPattern), Default = "*.*"
          , ResourceType = typeof(Strings))]
    public string Filter { get; set; }

    [Value(0, HelpText = nameof(Strings.FolderPath), Default = ".", ResourceType = typeof(Strings))]
    public string Path { get; set; }
}
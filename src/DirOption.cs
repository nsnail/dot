namespace Dot;

public class DirOption : IOption
{
    [Option('f', "filter", HelpText = nameof(Strings.FileSearchPattern), Default = "*.*"
          , ResourceType = typeof(Strings))]
    public string Filter { get; set; }

    [Value(0, HelpText = nameof(Strings.FolderPath), Default = ".", ResourceType = typeof(Strings))]
    public string Path { get; set; }


    [Option('r', "readonly", HelpText = nameof(Strings.ReadOnly), Default = false, ResourceType = typeof(Strings))]
    public bool ReadOnly { get; set; }
}
namespace Dot;

public class DirOption : IOption
{
    [Option('f', "filter", HelpText = nameof(Str.FileSearchPattern), Default = "*.*", ResourceType = typeof(Str))]
    public string Filter { get; set; }

    [Value(0, HelpText = nameof(Str.FolderPath), Default = ".", ResourceType = typeof(Str))]
    public string Path { get; set; }


    [Option('r', "readonly", HelpText = nameof(Str.ReadOnly), Default = false, ResourceType = typeof(Str))]
    public bool ReadOnly { get; set; }
}
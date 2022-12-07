namespace Dot;

public class DirOption : OptionBase
{
    [Option('f', "filter", HelpText = nameof(Str.FileSearchPattern), Default = "*.*", ResourceType = typeof(Str))]
    public string Filter { get; set; }

    [Value(0, HelpText = nameof(Str.FolderPath), Default = ".", ResourceType = typeof(Str))]
    public string Path { get; set; }


    [Option('w', "write", HelpText = nameof(Str.WriteMode), Default = false, ResourceType = typeof(Str))]
    public bool WriteMode { get; set; }
}
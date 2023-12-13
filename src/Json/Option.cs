namespace Dot.Json;

[Verb("json", HelpText = nameof(Str.Json), ResourceType = typeof(Str))]
public class Option : OptionBase
{
    [Option('c', "compress", HelpText = nameof(Str.CompressJson), Default = false, ResourceType = typeof(Str))]
    public bool Compress { get; set; }


    [Option('s', "convert-to-string", HelpText = nameof(Str.FormatJson), Default = false, ResourceType = typeof(Str))]
    public bool ConvertToString { get; set; }

    [Option('f', "format", HelpText = nameof(Str.FormatJson), Default = true, ResourceType = typeof(Str))]
    public bool Format { get; set; }
}
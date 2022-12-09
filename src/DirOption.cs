namespace Dot;

public class DirOption : OptionBase
{
    [CommandOption("-e|--exclude")]
    [Description(nameof(Str.ExcludePathRegexes))]
    [Localization(typeof(Str))]
    public IEnumerable<string> ExcludeRegexes { get; set; }


    [CommandOption("-f|--filter")]
    [Description(nameof(Str.FileSearchPattern))]
    [Localization(typeof(Str))]
    [DefaultValue("*")]
    public string Filter { get; set; }


    [CommandOption("-d|--max-depth")]
    [Description(nameof(Str.MaxRecursionDepth))]
    [Localization(typeof(Str))]
    [DefaultValue(int.MaxValue)]
    public int MaxRecursionDepth { get; set; }


    [CommandArgument(0, "[path]")]
    [Description(nameof(Str.FolderPath))]
    [Localization(typeof(Str))]
    [DefaultValue(".")]
    public string Path { get; set; }


    [CommandOption("-w|--write")]
    [Description(nameof(Str.WriteMode))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool WriteMode { get; set; }
}
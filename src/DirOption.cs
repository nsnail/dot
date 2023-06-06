// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot;

internal abstract class DirOption : OptionBase
{
    [CommandOption("-e|--exclude")]
    [Description(nameof(Ln.ExcludePathRegexes))]
    [Localization(typeof(Ln))]
    public IEnumerable<string> ExcludeRegexes { get; set; }

    [CommandOption("-f|--filter")]
    [Description(nameof(Ln.FileSearchPattern))]
    [Localization(typeof(Ln))]
    [DefaultValue("*")]
    public string Filter { get; set; }

    [CommandOption("-d|--max-depth")]
    [Description(nameof(Ln.MaxRecursionDepth))]
    [Localization(typeof(Ln))]
    [DefaultValue(int.MaxValue)]
    public int MaxRecursionDepth { get; set; }

    [CommandArgument(0, "[path]")]
    [Description(nameof(Ln.FolderPath))]
    [Localization(typeof(Ln))]
    [DefaultValue(".")]
    public string Path { get; set; }

    [CommandOption("-w|--write")]
    [Description(nameof(Ln.WriteMode))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool WriteMode { get; set; }
}
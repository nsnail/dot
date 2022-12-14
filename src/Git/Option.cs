// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.Git;

internal class Option : OptionBase
{
    [CommandOption("-a|--args")]
    [Description(nameof(Str.GitArgs))]
    [Localization(typeof(Str))]
    [DefaultValue("status")]
    public string Args { get; set; }

    [CommandOption("-e|--git-output-encoding")]
    [Description(nameof(Str.GitOutputEncoding))]
    [Localization(typeof(Str))]
    [DefaultValue("utf-8")]
    public string GitOutputEncoding { get; set; }

    [CommandOption("-d|--max-recursion-depth")]
    [Description(nameof(Str.MaxRecursionDepth))]
    [Localization(typeof(Str))]
    [DefaultValue(int.MaxValue)]
    public int MaxRecursionDepth { get; set; }

    [CommandArgument(0, "[path]")]
    [Description(nameof(Str.FolderPath))]
    [Localization(typeof(Str))]
    [DefaultValue(".")]
    public string Path { get; set; }
}
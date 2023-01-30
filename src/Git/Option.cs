// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.Git;

internal sealed class Option : OptionBase
{
    [CommandOption("-a|--args")]
    [Description(nameof(Ln.GitArgs))]
    [Localization(typeof(Ln))]
    [DefaultValue("status")]
    public string Args { get; set; }

    [CommandOption("-e|--git-output-encoding")]
    [Description(nameof(Ln.GitOutputEncoding))]
    [Localization(typeof(Ln))]
    [DefaultValue("utf-8")]
    public string GitOutputEncoding { get; set; }

    [CommandOption("-d|--max-recursion-depth")]
    [Description(nameof(Ln.MaxRecursionDepth))]
    [Localization(typeof(Ln))]
    [DefaultValue(int.MaxValue)]
    public int MaxRecursionDepth { get; set; }

    [CommandArgument(0, "[path]")]
    [Description(nameof(Ln.FolderPath))]
    [Localization(typeof(Ln))]
    [DefaultValue(".")]
    public string Path { get; set; }
}
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot;

internal abstract class DirOption : OptionBase
{
    [CommandOption("-e|--exclude")]
    [Description(nameof(Ln.排除路径的正则表达式))]
    [Localization(typeof(Ln))]
    public string[] ExcludeRegexes { get; set; }

    [CommandOption("-f|--filter")]
    [Description(nameof(Ln.文件通配符))]
    [Localization(typeof(Ln))]
    [DefaultValue("*")]
    public string Filter { get; set; }

    [CommandOption("-d|--max-depth")]
    [Description(nameof(Ln.目录检索深度))]
    [Localization(typeof(Ln))]
    [DefaultValue(int.MaxValue)]
    public int MaxRecursionDepth { get; set; }

    [CommandArgument(0, "[PATH]")]
    [Description(nameof(Ln.要处理的目录路径))]
    [Localization(typeof(Ln))]
    [DefaultValue(".")]
    public string Path { get; set; }

    [CommandOption("-w|--write")]
    [Description(nameof(Ln.启用写入模式))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool WriteMode { get; set; }
}
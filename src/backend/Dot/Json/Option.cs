// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Json;

internal sealed class Option : OptionBase
{
    [CommandOption("-c|--compress")]
    [Description(nameof(Ln.压缩Json文本))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool Compress { get; set; }

    [CommandOption("-s|--convert-to-string")]
    [Description(nameof(Ln.Json文本转义成字符串))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool ConvertToString { get; set; }

    [CommandOption("-f|--format")]
    [Description(nameof(Ln.格式化Json文本))]
    [Localization(typeof(Ln))]
    [DefaultValue(true)]
    public bool Format { get; set; }

    [CommandArgument(0, "[INPUT TEXT]")]
    [Description(nameof(Ln.要处理的文本_默认取取剪贴板值))]
    [Localization(typeof(Ln))]
    public string InputText { get; set; }
}
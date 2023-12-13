// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.Text;

internal sealed class Option : OptionBase
{
    [CommandArgument(0, "[INPUT TEXT]")]
    [Description(nameof(Ln.要处理的文本_默认取取剪贴板值))]
    [Localization(typeof(Ln))]
    public string Text { get; set; }
}
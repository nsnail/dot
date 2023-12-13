// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Guid;

internal sealed class Option : OptionBase
{
    [CommandOption("-u|--upper")]
    [Description(nameof(Ln.使用大写输出))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool Upper { get; set; }
}
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot;

internal abstract class OptionBase : CommandSettings, IOption
{
    [CommandOption("-k|--keep--session")]
    [Description(nameof(Str.KeepSession))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool KeepSession { get; set; }
}
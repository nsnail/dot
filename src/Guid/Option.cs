// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Guid;

internal class Option : OptionBase
{
    [CommandOption("-u|--upper")]
    [Description(nameof(Str.UseUppercase))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool Upper { get; set; }
}
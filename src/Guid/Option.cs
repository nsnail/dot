namespace Dot.Guid;

public class Option : OptionBase
{
    [CommandOption("-u|--upper")]
    [Description(nameof(Str.UseUppercase))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool Upper { get; set; } //normal options here
}
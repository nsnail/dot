namespace Dot;

public abstract class OptionBase : CommandSettings, IOption
{
    [CommandOption("-k|--keep--session")]
    [Description(nameof(Str.KeepSession))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public virtual bool KeepSession { get; set; }
}
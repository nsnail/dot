namespace Dot;

public abstract class OptionBase : IOption
{
    [Option('k', "keep-session", HelpText = nameof(Str.KeepSession), Default = false, ResourceType = typeof(Str))]
    public virtual bool KeepSession { get; set; }
}
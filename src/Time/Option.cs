namespace Dot.Time;

public class Option : OptionBase
{
    [CommandOption("-s|--sync")]
    [Description(nameof(Str.SyncToLocalTime))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool Sync { get; set; }


    [CommandOption("-t|--timeout")]
    [Description(nameof(Str.TimeoutMillSecs))]
    [Localization(typeof(Str))]
    [DefaultValue(2000)]
    public int Timeout { get; set; }
}
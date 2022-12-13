// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Time;

internal class Option : OptionBase
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
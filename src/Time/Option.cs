// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Time;

internal sealed class Option : OptionBase
{
    [CommandOption("-s|--sync")]
    [Description(nameof(Ln.SyncToLocalTime))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool Sync { get; set; }

    [CommandOption("-t|--timeout")]
    [Description(nameof(Ln.TimeoutMillSecs))]
    [Localization(typeof(Ln))]
    [DefaultValue(2000)]
    public int Timeout { get; set; }
}
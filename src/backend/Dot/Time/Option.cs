// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Time;

internal sealed class Option : OptionBase
{
    [CommandOption("-s|--sync")]
    [Description(nameof(Ln.同步本机时间))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool Sync { get; set; }

    [CommandOption("-t|--timeout")]
    [Description(nameof(Ln.连接NTP服务器超时时间_毫秒))]
    [Localization(typeof(Ln))]
    [DefaultValue(2000)]
    public int Timeout { get; set; }
}
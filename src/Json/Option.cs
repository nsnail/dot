// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Json;

internal sealed class Option : OptionBase
{
    [CommandOption("-c|--compress")]
    [Description(nameof(Ln.CompressJson))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool Compress { get; set; }

    [CommandOption("-s|--convert-to-string")]
    [Description(nameof(Ln.JsonToString))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool ConvertToString { get; set; }

    [CommandOption("-f|--format")]
    [Description(nameof(Ln.FormatJson))]
    [Localization(typeof(Ln))]
    [DefaultValue(true)]
    public bool Format { get; set; }

    [CommandArgument(0, "[input text]")]
    [Description(nameof(Ln.TextTobeProcessed))]
    [Localization(typeof(Ln))]
    public string InputText { get; set; }
}
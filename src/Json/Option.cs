// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Json;

internal sealed class Option : OptionBase
{
    [CommandOption("-c|--compress")]
    [Description(nameof(Str.CompressJson))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool Compress { get; set; }

    [CommandOption("-s|--convert-to-string")]
    [Description(nameof(Str.JsonToString))]
    [Localization(typeof(Str))]
    [DefaultValue(false)]
    public bool ConvertToString { get; set; }

    [CommandOption("-f|--format")]
    [Description(nameof(Str.FormatJson))]
    [Localization(typeof(Str))]
    [DefaultValue(true)]
    public bool Format { get; set; }

    [CommandArgument(0, "[input text]")]
    [Description(nameof(Str.TextTobeProcessed))]
    [Localization(typeof(Str))]
    public string InputText { get; set; }
}
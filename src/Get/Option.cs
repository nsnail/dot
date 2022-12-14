// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Get;

internal class Option : OptionBase
{
    [CommandOption("-b|--buffer-size")]
    [Description(nameof(Str.BufferSize))]
    [Localization(typeof(Str))]
    [DefaultValue(8096)]
    public int BufferSize { get; set; }

    [CommandOption("-c|--chunk-number")]
    [Description(nameof(Str.ChunkNumbers))]
    [Localization(typeof(Str))]
    [DefaultValue(5)]
    public int ChunkNumbers { get; set; }

    [CommandOption("-m|--max-parallel")]
    [Description(nameof(Str.MaxParallel))]
    [Localization(typeof(Str))]
    [DefaultValue(5)]
    public int MaxParallel { get; set; }

    [CommandOption("-o|--output")]
    [Description(nameof(Str.OutputPath))]
    [Localization(typeof(Str))]
    [DefaultValue(".")]
    public string Output { get; set; }

    [CommandArgument(0, "<url>")]
    [Description(nameof(Str.Url))]
    [Localization(typeof(Str))]
    public string Url { get; set; }
}
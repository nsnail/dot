// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Get;

internal sealed class Option : OptionBase
{
    [CommandOption("-b|--buffer-size")]
    [Description(nameof(Ln.BufferSize))]
    [Localization(typeof(Ln))]
    [DefaultValue(8096)]
    public int BufferSize { get; set; }

    [CommandOption("-c|--chunk-number")]
    [Description(nameof(Ln.ChunkNumbers))]
    [Localization(typeof(Ln))]
    [DefaultValue(5)]
    public int ChunkNumbers { get; set; }

    [CommandOption("-m|--max-parallel")]
    [Description(nameof(Ln.MaxParallel))]
    [Localization(typeof(Ln))]
    [DefaultValue(5)]
    public int MaxParallel { get; set; }

    [CommandOption("-o|--output")]
    [Description(nameof(Ln.OutputPath))]
    [Localization(typeof(Ln))]
    [DefaultValue(".")]
    public string Output { get; set; }

    [CommandArgument(0, "<url>")]
    [Description(nameof(Ln.Url))]
    [Localization(typeof(Ln))]
    public string Url { get; set; }
}
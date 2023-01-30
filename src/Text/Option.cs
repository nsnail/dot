// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.Text;

internal sealed class Option : OptionBase
{
    [CommandArgument(0, "[input text]")]
    [Description(nameof(Ln.TextTobeProcessed))]
    [Localization(typeof(Ln))]
    public string Text { get; set; }
}
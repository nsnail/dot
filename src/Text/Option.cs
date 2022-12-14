// ReSharper disable ClassNeverInstantiated.Global


namespace Dot.Text;

internal sealed class Option : OptionBase
{
    [CommandArgument(0, "[input text]")]
    [Description(nameof(Str.TextTobeProcessed))]
    [Localization(typeof(Str))]
    public string Text { get; set; }
}
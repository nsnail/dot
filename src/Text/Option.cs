// ReSharper disable ClassNeverInstantiated.Global


namespace Dot.Text;

internal class Option : OptionBase
{
    [CommandArgument(0, "[input text]")]
    [Description(nameof(Str.TextTobeProcessed))]
    [Localization(typeof(Str))]
    public string Text { get; set; }
}
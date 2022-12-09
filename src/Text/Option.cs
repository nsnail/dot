namespace Dot.Text;

public class Option : OptionBase
{
    [CommandArgument(0, "[input text]")]
    [Description(nameof(Str.TextTobeProcessed))]
    [Localization(typeof(Str))]
    public string Text { get; set; }
}
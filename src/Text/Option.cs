namespace Dot.Text;

[Verb("text", HelpText = nameof(Str.HelpForText), ResourceType = typeof(Str))]
public class Option : IOption
{
    [Value(0, HelpText = nameof(Str.TextTobeProcessed), ResourceType = typeof(Str))]
    public string Text { get; set; }
}
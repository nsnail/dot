namespace Dot.Time;

[Verb("time", HelpText = nameof(Strings.HelpForText), ResourceType = typeof(Strings))]
public class Option : IOption
{
    [Value(0, HelpText = nameof(Strings.TextTobeProcessed), ResourceType = typeof(Strings))]
    public string Text { get; set; }
}
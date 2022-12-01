namespace Dot.Text;

[Verb("text", HelpText = nameof(Strings.HelpForText), ResourceType = typeof(Strings))]
public class Option : IOption
{
    [Value(0, HelpText = nameof(Strings.HelpForTextArg0), ResourceType = typeof(Strings))]
    public string Text { get; set; }
}
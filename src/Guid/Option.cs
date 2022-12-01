namespace Dot.Guid;

[Verb("guid", HelpText = nameof(Strings.GuidTool), ResourceType = typeof(Strings))]
public class Option : IOption
{
    [Option('u', "upper", HelpText = nameof(Strings.UseUppercase), Default = false, ResourceType = typeof(Strings))]
    public bool Upper { get; set; } //normal options here
}
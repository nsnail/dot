namespace Dot.Guid;

[Verb("guid", HelpText = nameof(Str.GuidTool), ResourceType = typeof(Str))]
public class Option : OptionBase
{
    [Option('u', "upper", HelpText = nameof(Str.UseUppercase), Default = false, ResourceType = typeof(Str))]
    public bool Upper { get; set; } //normal options here
}
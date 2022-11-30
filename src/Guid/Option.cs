namespace Dot.Guid;

[Verb("guid", HelpText = "GUID工具")]
public class Option : IOption
{
    [Option('u', "upper", HelpText = "大写", Default = false)]
    public bool Upper { get; set; } //normal options here
}
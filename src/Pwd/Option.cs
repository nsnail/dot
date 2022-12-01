namespace Dot.Pwd;

[Verb("pwd", HelpText = nameof(Strings.RandomPasswordGenerator), ResourceType = typeof(Strings))]
public class Option : IOption
{
    [Flags]
    public enum GenerateTypes
    {
        Number           = 1
      , LowerCaseLetter  = 2
      , UpperCaseLetter  = 4
      , SpecialCharacter = 8
    }

    [Value(1, Required = true, HelpText = nameof(Strings.PwdLength), ResourceType = typeof(Strings))]
    public int Length { get; set; }


    [Value(0, Required = true, HelpText = nameof(Strings.PwdGenerateTypes), ResourceType = typeof(Strings))]
    public GenerateTypes Type { get; set; }
}
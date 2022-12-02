namespace Dot.Pwd;

[Verb("pwd", HelpText = nameof(Str.RandomPasswordGenerator), ResourceType = typeof(Str))]
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

    [Value(1, Required = true, HelpText = nameof(Str.PwdLength), ResourceType = typeof(Str))]
    public int Length { get; set; }


    [Value(0, Required = true, HelpText = nameof(Str.PwdGenerateTypes), ResourceType = typeof(Str))]
    public GenerateTypes Type { get; set; }
}
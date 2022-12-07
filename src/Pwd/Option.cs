namespace Dot.Pwd;

[Verb("pwd", HelpText = nameof(Str.RandomPasswordGenerator), ResourceType = typeof(Str))]
public class Option : OptionBase
{
    [Flags]
    public enum GenerateTypes
    {
        Number           = 0b0001
      , LowerCaseLetter  = 0b0010
      , UpperCaseLetter  = 0b0100
      , SpecialCharacter = 0b1000
    }

    [Value(1, Required = true, HelpText = nameof(Str.PwdLength), ResourceType = typeof(Str))]
    public int Length { get; set; }


    [Value(0, Required = true, HelpText = nameof(Str.PwdGenerateTypes), ResourceType = typeof(Str))]
    public GenerateTypes Type { get; set; }
}
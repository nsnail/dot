// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Pwd;

internal class Option : OptionBase
{
    [Flags]
    public enum GenerateTypes
    {
        Number           = 0b0001
      , LowerCaseLetter  = 0b0010
      , UpperCaseLetter  = 0b0100
      , SpecialCharacter = 0b1000
    }

    [CommandArgument(1, "<password length>")]
    [Description(nameof(Str.PwdLength))]
    [Localization(typeof(Str))]
    public int Length { get; set; }

    [CommandArgument(0, "<generate type>")]
    [Description(nameof(Str.PwdGenerateTypes))]
    [Localization(typeof(Str))]
    public GenerateTypes Type { get; set; }
}
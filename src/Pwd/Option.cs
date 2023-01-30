// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Pwd;

internal sealed class Option : OptionBase
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
    [Description(nameof(Ln.PwdLength))]
    [Localization(typeof(Ln))]
    public int Length { get; set; }

    [CommandArgument(0, "<generate type>")]
    [Description(nameof(Ln.PwdGenerateTypes))]
    [Localization(typeof(Ln))]
    public GenerateTypes Type { get; set; }
}
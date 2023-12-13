// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Pwd;

internal sealed class Option : OptionBase
{
    [Flags]
    public enum GenerateTypes
    {
        None             = 0
      , Number           = 1
      , LowerCaseLetter  = 1 << 1
      , UpperCaseLetter  = 1 << 2
      , SpecialCharacter = 1 << 3
    }

    [CommandArgument(1, "<PASSWORD LENGTH>")]
    [Description(nameof(Ln.密码长度))]
    [Localization(typeof(Ln))]
    public int Length { get; set; }

    [CommandArgument(0, "<GENERATE TYPE>")]
    [Description(nameof(Ln.密码创建类型))]
    [Localization(typeof(Ln))]
    public GenerateTypes Type { get; set; }
}
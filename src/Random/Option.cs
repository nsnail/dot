namespace Dot.Random;

[Verb("rand", HelpText = "随机数生成器")]
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

    [Value(1, MetaName = "长度", Required = true, HelpText = "随机数字长度")]
    public int Length { get; set; }


    [Value(0, MetaName = "生成类型", Required = true, HelpText = "BitSet 1：[0-9]，2：[a-z]，4：[A-Z]，8：[ascii.0x21-0x2F]")]
    public GenerateTypes Type { get; set; }
}
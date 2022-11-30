using System.Text.RegularExpressions;
using NSExt.Extensions;
using TextCopy;

namespace Dot.Random;

public sealed partial class Main : Tool<Option>
{
    private readonly char[][] _charTable = {
                                               "0123456789".ToCharArray() //
                                             , "abcdefghijklmnopqrstuvwxyz".ToCharArray()
                                             , "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
                                             , "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".ToCharArray()
                                           };


    public Main(Option opt) : base(opt) { }

    [GeneratedRegex("[a-z]")]
    private static partial Regex RegexLowerCaseLetter();

    [GeneratedRegex("\\d")]
    private static partial Regex RegexNumber();

    [GeneratedRegex("[^\\da-zA-Z]")]
    private static partial Regex RegexSpecialCharacter();

    [GeneratedRegex("[A-Z]")]
    private static partial Regex RegexUpperCaseLetter();

    public override void Run()
    {
        unsafe {
            var pSource   = stackalloc char[_charTable.Sum(x => x.Length)];
            var pDest     = stackalloc char[Opt.Length];
            var sourceLen = 0;

            if (Opt.Type.HasFlag(Option.GenerateTypes.Number))
                foreach (var c in _charTable[0])
                    *(pSource + sourceLen++) = c;


            if (Opt.Type.HasFlag(Option.GenerateTypes.LowerCaseLetter))
                foreach (var c in _charTable[1])
                    *(pSource + sourceLen++) = c;

            if (Opt.Type.HasFlag(Option.GenerateTypes.UpperCaseLetter))
                foreach (var c in _charTable[2])
                    *(pSource + sourceLen++) = c;


            if (Opt.Type.HasFlag(Option.GenerateTypes.SpecialCharacter))
                foreach (var c in _charTable[3])
                    *(pSource + sourceLen++) = c;


            var randScope = new[] { 0, sourceLen };
            for (var i = 0; i != Opt.Length; ++i) //
                *(pDest + i) = *(pSource + randScope.Rand());

            var result = new string(pDest);
            ClipboardService.SetText(result);
            Console.WriteLine($"已复制到剪贴板：{result}");
        }
    }
}
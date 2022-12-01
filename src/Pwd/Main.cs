using NSExt.Extensions;
using TextCopy;

namespace Dot.Pwd;

public sealed class Main : Tool<Option>
{
    private readonly char[][] _charTable = {
                                               "0123456789".ToCharArray() //
                                             , "abcdefghijklmnopqrstuvwxyz".ToCharArray()
                                             , "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
                                             , "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".ToCharArray()
                                           };


    public Main(Option opt) : base(opt) { }

    public override Task Run()
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

            var result = new string(pDest, 0, Opt.Length);
            ClipboardService.SetText(result);
            Console.WriteLine(Str.Copied, result);
        }

        return Task.CompletedTask;
    }
}
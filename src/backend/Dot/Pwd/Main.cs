// ReSharper disable ClassNeverInstantiated.Global

using NSExt.Extensions;
#if NET8_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Pwd;

[Description(nameof(Ln.随机密码生成器))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    private readonly char[][] _charTable = {
                                               "0123456789".ToCharArray() //
                                             , "abcdefghijklmnopqrstuvwxyz".ToCharArray()
                                             , "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()
                                             , "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".ToCharArray()
                                           };

    protected override Task CoreAsync()
    {
        unsafe {
            var pSource   = stackalloc char[_charTable.Sum(x => x.Length)];
            var pDest     = stackalloc char[Opt.Length];
            var sourceLen = 0;

            if (Opt.Type.HasFlag(Option.GenerateTypes.Number)) {
                foreach (var c in _charTable[0]) {
                    *(pSource + sourceLen++) = c;
                }
            }

            if (Opt.Type.HasFlag(Option.GenerateTypes.LowerCaseLetter)) {
                foreach (var c in _charTable[1]) {
                    *(pSource + sourceLen++) = c;
                }
            }

            if (Opt.Type.HasFlag(Option.GenerateTypes.UpperCaseLetter)) {
                foreach (var c in _charTable[2]) {
                    *(pSource + sourceLen++) = c;
                }
            }

            if (Opt.Type.HasFlag(Option.GenerateTypes.SpecialCharacter)) {
                foreach (var c in _charTable[3]) {
                    *(pSource + sourceLen++) = c;
                }
            }

            var randScope = new[] { 0, sourceLen };
            for (var i = 0; i != Opt.Length; ++i) {
                *(pDest + i) = *(pSource + randScope.Rand());
            }

            var result = new string(pDest, 0, Opt.Length);
            Console.WriteLine($"{Ln.已复制到剪贴板}: {result}");
            #if NET8_0_WINDOWS
            ClipboardService.SetText(result);
            #endif
        }

        return Task.CompletedTask;
    }
}
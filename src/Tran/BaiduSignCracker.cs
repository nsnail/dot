using NSExt.Extensions;

namespace Dot.Tran;

internal static class BaiduSignCracker
{
    private const int _MAGIC_NUMBER_1 = 320305;
    private const int _MAGIC_NUMBER_2 = 131321201;

    public static string Sign(string text)
    {
        var hash = text.Length > 30
            ? string.Concat(text.AsSpan()[..10], text.AsSpan(text.Length / 2 - 5, 10), text.AsSpan()[^10..])
            : text;

        var e = new List<int>(hash.Length);
        for (var i = 0; i < hash.Length; i++) {
            var k = (int)hash[i];
            switch (k) {
                case < 128:
                    e.Add(k);
                    break;
                case < 2048:
                    e.Add((k >> 6) | 192);
                    break;
                default: {
                    if ((k & 64512) == 55296 && i + 1 < hash.Length && (hash[i + 1] & 64512) == 56320) {
                        k = 65536 + ((k & 1023) << 10) + (hash[++i] & 1023);
                        e.Add((k >> 18)        | 240);
                        e.Add(((k >> 12) & 63) | 128);
                    }
                    else {
                        e.Add((k >> 12)       | 224);
                        e.Add(((k >> 6) & 63) | 128);
                        e.Add((k        & 63) | 128);
                    }

                    break;
                }
            }
        }

        var ret = e.Aggregate(_MAGIC_NUMBER_1, (accumulator, source) => Compute(accumulator + source, "+-a^+6"));
        ret =  Compute(ret, "+-3^+b+-f");
        ret ^= _MAGIC_NUMBER_2;
        var longRet = ret < 0 ? 1L + (ret & int.MaxValue) + int.MaxValue : ret;
        longRet %= 1_000_000;
        return $"{longRet}.{longRet ^ _MAGIC_NUMBER_1}";
    }

    private static int Compute(int number, string password)
    {
        unchecked {
            for (var i = 0; i < password.Length - 2; i += 3) {
                var c       = password[i + 2];
                var moveBit = c               >= 'a' ? c - 87 : c.ToString().Int32();
                var d       = password[i + 1] == '+' ? number >>> moveBit : number << moveBit;
                number = password[i] == '+' ? (number + d) & (int)uint.MaxValue : number ^ d;
            }
        }

        return number;
    }
}
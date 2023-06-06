// ReSharper disable ClassNeverInstantiated.Global

using System.Security.Cryptography;
using System.Text;
using NSExt.Extensions;
#if NET7_0_WINDOWS
using System.Diagnostics;
using TextCopy;
#endif

namespace Dot.Text;

[Description(nameof(Ln.TextTool))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    #if NET7_0_WINDOWS
    protected override async Task CoreAsync()
        #else
    protected override Task CoreAsync()
        #endif
    {
        #if NET7_0_WINDOWS
        if (Opt.Text.NullOrEmpty()) {
            Opt.Text = await ClipboardService.GetTextAsync();
        }
        #endif
        if (Opt.Text.NullOrEmpty()) {
            throw new ArgumentException(Ln.InputTextIsEmpty);
        }

        ParseAndShow(Opt.Text);
        #if !NET7_0_WINDOWS
        return Task.CompletedTask;
        #endif
    }

    private static Output BuildOutput(string text, Encoding enc)
    {
        Output ret;
        var    inputHex = text.Hex(enc);

        ret.Base64DeCodeHex = ReadOnlySpan<char>.Empty;
        ret.Base64DeCode    = ReadOnlySpan<char>.Empty;
        ret.EncodingName    = enc.EncodingName;
        ret.Hex             = inputHex.Str();
        ret.Base64          = text.Base64(enc);
        #pragma warning disable CA5351, CA5350
        ret.Md5  = MD5.HashData(inputHex).Str();
        ret.Sha1 = SHA1.HashData(inputHex).Str();
        #pragma warning restore CA5350, CA5351
        ret.Sha256           = SHA256.HashData(inputHex).Str();
        ret.Sha512           = SHA512.HashData(inputHex).Str();
        ret.UrlEncode        = text.Url();
        ret.UrlDecode        = text.UrlDe();
        ret.HtmlDecode       = text.HtmlDe();
        ret.HtmlEncode       = text.Html();
        ret.PercentUnicode   = default;
        ret.AndUnicode       = default;
        ret.BacksLantUnicode = default;
        ret.UnicodeDecode    = text.UnicodeDe();

        if (Equals(enc, Encoding.BigEndianUnicode)) {
            ret.PercentUnicode   = inputHex.Str(false, "%u", 2);
            ret.AndUnicode       = inputHex.Str(false, ";&#x", 2)[1..] + ";";
            ret.BacksLantUnicode = inputHex.Str(false, @"\u", 2);
        }

        if (!text.IsBase64String()) {
            return ret;
        }

        byte[] base64DeHex = null;
        try {
            base64DeHex = text.Base64De();
        }
        catch {
            // ignored
        }

        if (base64DeHex == null) {
            return ret;
        }

        ret.Base64DeCodeHex = base64DeHex.Str();
        ret.Base64DeCode    = enc.GetString(base64DeHex);

        return ret;
    }

    private static string BuildTemplate(Output o)
    {
        var sb = new StringBuilder();

        _ = sb.AppendLine( //
            CultureInfo.InvariantCulture
          , $"{new string('-', 20)} {o.EncodingName} {new string('-', 30 - o.EncodingName.Length)}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"hex:               {o.Hex}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"base64:            {o.Base64}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"base64-decode-hex: {o.Base64DeCodeHex}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"base64-decode:     {o.Base64DeCode}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"md5:               {o.Md5}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"sha1:              {o.Sha1}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"sha256:            {o.Sha256}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"sha512:            {o.Sha512}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"url-encode:        {o.UrlEncode}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"url-decode:        {o.UrlDecode}");

        if (o.EncodingName.Equals(Encoding.BigEndianUnicode.EncodingName, StringComparison.OrdinalIgnoreCase)) {
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-percent:   {o.PercentUnicode}");
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-and:       {o.AndUnicode}");
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-back-slant: {o.BacksLantUnicode}");
            _ = sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-decode:    {o.UnicodeDecode}");
        }

        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"html-encode:       {o.HtmlEncode}");
        _ = sb.AppendLine(CultureInfo.InvariantCulture, $"html-decode:       {o.HtmlDecode}");

        return sb.ToString();
    }

    private static void ParseAndShow(string text)
    {
        var ansi                = BuildOutput(text, Encoding.GetEncoding("gbk"));
        var utf8                = BuildOutput(text, Encoding.UTF8);
        var unicodeLittleEndian = BuildOutput(text, Encoding.Unicode);
        var unicodeBigEndian    = BuildOutput(text, Encoding.BigEndianUnicode);

        var sb = new StringBuilder();
        sb.AppendLine(BuildTemplate(ansi))
          .AppendLine(BuildTemplate(utf8))
          .AppendLine(BuildTemplate(unicodeLittleEndian))
          .AppendLine(BuildTemplate(unicodeBigEndian));
        var str = sb.ToString();

        Console.WriteLine(str);

        #if NET7_0_WINDOWS
        var file = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.txt");
        File.WriteAllText(file, str);
        Process.Start("explorer", file);
        #endif
    }

    private ref struct Output
    {
        public ReadOnlySpan<char> AndUnicode;
        public ReadOnlySpan<char> BacksLantUnicode;
        public ReadOnlySpan<char> Base64;
        public ReadOnlySpan<char> Base64DeCode;
        public ReadOnlySpan<char> Base64DeCodeHex;
        public ReadOnlySpan<char> EncodingName;
        public ReadOnlySpan<char> Hex;
        public ReadOnlySpan<char> HtmlDecode;
        public ReadOnlySpan<char> HtmlEncode;
        public ReadOnlySpan<char> Md5;
        public ReadOnlySpan<char> PercentUnicode;
        public ReadOnlySpan<char> Sha1;
        public ReadOnlySpan<char> Sha256;
        public ReadOnlySpan<char> Sha512;
        public ReadOnlySpan<char> UnicodeDecode;
        public ReadOnlySpan<char> UrlDecode;
        public ReadOnlySpan<char> UrlEncode;
    }
}
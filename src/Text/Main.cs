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
    protected override async Task Core()
    #else
    protected override Task Core()
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
        ret.Base64DeCodeHex  = ReadOnlySpan<char>.Empty;
        ret.Base64DeCode     = ReadOnlySpan<char>.Empty;
        ret.EncodingName     = enc.EncodingName;
        ret.Hex              = inputHex.String();
        ret.Base64           = text.Base64(enc);
        ret.Md5              = MD5.HashData(inputHex).String();
        ret.Sha1             = SHA1.HashData(inputHex).String();
        ret.Sha256           = SHA256.HashData(inputHex).String();
        ret.Sha512           = SHA512.HashData(inputHex).String();
        ret.UrlEncode        = text.Url();
        ret.UrlDecode        = text.UrlDe();
        ret.HtmlDecode       = text.HtmlDe();
        ret.HtmlEncode       = text.Html();
        ret.PercentUnicode   = default;
        ret.AndUnicode       = default;
        ret.BacksLantUnicode = default;
        ret.UnicodeDecode    = text.UnicodeDe();

        if (Equals(enc, Encoding.BigEndianUnicode)) {
            ret.PercentUnicode   = inputHex.String(false, "%u", 2);
            ret.AndUnicode       = inputHex.String(false, @";&#x", 2)[1..] + ";";
            ret.BacksLantUnicode = inputHex.String(false, @"\u", 2);
        }

        if (!text.IsBase64String()) {
            return ret;
        }

        byte[] base64DeHex = null;
        try {
            base64DeHex = text.Base64De();
        }
        catch (Exception) {
            // ignored
        }

        if (base64DeHex == null) {
            return ret;
        }

        ret.Base64DeCodeHex = base64DeHex.String();
        ret.Base64DeCode    = enc.GetString(base64DeHex);

        return ret;
    }

    private static string BuildTemplate(Output o)
    {
        var sb = new StringBuilder();

        sb.AppendLine( //
            CultureInfo.InvariantCulture
          , $"{new string('-', 20)} {o.EncodingName} {new string('-', 30 - o.EncodingName.Length)}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"hex:               {o.Hex}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"base64:            {o.Base64}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"base64-decode-hex: {o.Base64DeCodeHex}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"base64-decode:     {o.Base64DeCode}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"md5:               {o.Md5}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"sha1:              {o.Sha1}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"sha256:            {o.Sha256}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"sha512:            {o.Sha512}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"url-encode:        {o.UrlEncode}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"url-decode:        {o.UrlDecode}");

        if (o.EncodingName.Equals(Encoding.BigEndianUnicode.EncodingName, StringComparison.OrdinalIgnoreCase)) {
            sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-percent:   {o.PercentUnicode}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-and:       {o.AndUnicode}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-backslant: {o.BacksLantUnicode}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"unicode-decode:    {o.UnicodeDecode}");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"html-encode:       {o.HtmlEncode}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"html-decode:       {o.HtmlDecode}");

        return sb.ToString();
    }

    private static void ParseAndShow(string text)
    {
        var ansi                = BuildOutput(text, Encoding.GetEncoding("gbk"));
        var utf8                = BuildOutput(text, Encoding.UTF8);
        var unicodeLittleEndian = BuildOutput(text, Encoding.Unicode);
        var unicodeBigEndian    = BuildOutput(text, Encoding.BigEndianUnicode);

        var sb = new StringBuilder();
        sb.AppendLine(BuildTemplate(ansi));
        sb.AppendLine(BuildTemplate(utf8));
        sb.AppendLine(BuildTemplate(unicodeLittleEndian));
        sb.AppendLine(BuildTemplate(unicodeBigEndian));
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
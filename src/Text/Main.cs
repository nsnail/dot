// ReSharper disable ClassNeverInstantiated.Global

using System.Security.Cryptography;
using System.Text;
using NSExt.Extensions;
#if NET7_0_WINDOWS
using System.Diagnostics;
using TextCopy;
#endif

namespace Dot.Text;

[Description(nameof(Str.TextTool))]
[Localization(typeof(Str))]
internal sealed partial class Main : ToolBase<Option>
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
            throw new ArgumentException(Str.InputTextIsEmpty);
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
        ret.OriginText      = text;
        ret.Hex             = inputHex.String();
        ret.Base64          = text.Base64(enc);
        ret.Md5             = MD5.HashData(inputHex).String();
        ret.Sha1            = SHA1.HashData(inputHex).String();
        ret.Sha256          = SHA256.HashData(inputHex).String();
        ret.Sha512          = SHA512.HashData(inputHex).String();
        ret.UrlEncode       = text.Url();
        ret.UrlDecode       = text.UrlDe();
        ret.HtmlDecode      = text.HtmlDe();
        ret.HtmlEncode      = text.Html();

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

    private static void ParseAndShow(string text)
    {
        var ansi                = BuildOutput(text, Encoding.GetEncoding("gbk"));
        var utf8                = BuildOutput(text, Encoding.UTF8);
        var unicodeLittleEndian = BuildOutput(text, Encoding.Unicode);
        var unicodeBigEndian    = BuildOutput(text, Encoding.BigEndianUnicode);

        PrintOutput(ansi);
        PrintOutput(utf8);
        PrintOutput(unicodeLittleEndian);
        PrintOutput(unicodeBigEndian);
    }

    private static void PrintOutput(Output o)
    {
        var outputTemp = $"""
origin-text:       {o.OriginText}
{new string('-', 20)} {o.EncodingName} {new string('-', 30 - o.EncodingName.Length)}
hex:               {o.Hex}
base64:            {o.Base64}
base64-decode-hex: {o.Base64DeCodeHex}
base64-decode:     {o.Base64DeCode}
md5:               {o.Md5}
sha1:              {o.Sha1}
sha256:            {o.Sha256}
sha512:            {o.Sha512}
url-encode:        {o.UrlEncode}
url-decode:        {o.UrlDecode}
html-encode:       {o.HtmlEncode}
html-decode:       {o.HtmlDecode}
""";
        Console.WriteLine(outputTemp);
        #if NET7_0_WINDOWS
        var file = Path.Combine(Path.GetTempPath(), $"{System.Guid.NewGuid()}.html");
        File.WriteAllText(file, outputTemp.Text2Html());
        Process.Start("explorer", file);
        #endif
    }
}
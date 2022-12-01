using System.Security.Cryptography;
using System.Text;
using NSExt.Extensions;
using TextCopy;

namespace Dot.Text;

public sealed class Main : Tool<Option>
{
    private ref struct Output
    {
        public ReadOnlySpan<char> Base64;
        public ReadOnlySpan<char> Base64DeCode;
        public ReadOnlySpan<char> Base64DeCodeHex;
        public ReadOnlySpan<char> EncodingName;
        public ReadOnlySpan<char> Hex;
        public ReadOnlySpan<char> HtmlDecode;
        public ReadOnlySpan<char> HtmlEncode;
        public ReadOnlySpan<char> Md5;
        public ReadOnlySpan<char> OriginText;
        public ReadOnlySpan<char> Sha1;
        public ReadOnlySpan<char> Sha256;
        public ReadOnlySpan<char> Sha512;
        public ReadOnlySpan<char> UrlDecode;
        public ReadOnlySpan<char> UrlEncode;
    }

    public Main(Option opt) : base(opt)
    {
        if (Opt.Text.NullOrEmpty()) Opt.Text = ClipboardService.GetText();
        if (Opt.Text.NullOrEmpty()) throw new ArgumentException(Strings.InputTextIsEmpty);
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


        if (!text.IsBase64String()) return ret;
        byte[] base64DeHex = null;
        try {
            base64DeHex = text.Base64De();
        }
        catch (Exception) {
            // ignored
        }

        if (base64DeHex == null) return ret;
        ret.Base64DeCodeHex = base64DeHex.String();
        ret.Base64DeCode    = enc.GetString(base64DeHex);


        return ret;
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
    }

    public override void Run()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var ansi                = BuildOutput(Opt.Text, Encoding.GetEncoding("gbk"));
        var utf8                = BuildOutput(Opt.Text, Encoding.UTF8);
        var unicodeLittleEndian = BuildOutput(Opt.Text, Encoding.Unicode);
        var unicodeBigEndian    = BuildOutput(Opt.Text, Encoding.BigEndianUnicode);


        PrintOutput(ansi);
        PrintOutput(utf8);
        PrintOutput(unicodeLittleEndian);
        PrintOutput(unicodeBigEndian);
        Console.Write(Strings.PressAnyKey);
        Console.ReadKey();
    }
}
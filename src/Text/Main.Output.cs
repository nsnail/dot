namespace Dot.Text;

internal sealed partial class Main
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
}
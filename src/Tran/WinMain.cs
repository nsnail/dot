#if NET7_0_WINDOWS
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Dot.Native;
using Dot.Tran.Dto;
using NSExt.Extensions;
using TextCopy;
using Size = System.Drawing.Size;

namespace Dot.Tran;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed partial class WinMain : Form
{
    private const    int          _RETRY_WAIT_MIL_SEC = 1000;
    private const    string       _TRANSLATE_API_URL = $"{_TRANSLATE_HOME_URL}/v2transapi";
    private const    string       _TRANSLATE_HOME_URL = "https://fanyi.baidu.com";
    private readonly HttpClient   _httpClient = new(new HttpClientHandler { UseProxy = false });
    private readonly KeyboardHook _keyboardHook = new();
    private readonly Label        _labelDest = new();
    private readonly MouseHook    _mouseHook = new();
    private readonly Size         _mouseMargin = new(10, 10);
    private readonly string       _stateFile = Path.Combine(Path.GetTempPath(), "dot-tran-state.tmp");
    private          bool         _capsLockPressed;
    private volatile string       _cookie;
    private          bool         _disposed;
    private          nint         _nextClipMonitor;
    private volatile string       _token;

    public WinMain()
    {
        InitForm();
        InitHook();
        InitLabelDest();
        InitHttpClient();
        InitTokenCookie();
        InitClipMonitor();
    }

    ~WinMain()
    {
        Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_disposed) {
            return;
        }

        if (disposing) {
            _httpClient?.Dispose();
            _labelDest?.Dispose();
            _mouseHook?.Dispose();
            _keyboardHook?.Dispose();
        }

        Win32.ChangeClipboardChain(Handle, _nextClipMonitor); // 从剪贴板监视链移除本窗体
        _disposed = true;
    }

    protected override void WndProc(ref Message m)
    {
        void SendToNext(Message message)
        {
            _ = Win32.SendMessageA(_nextClipMonitor, (uint)message.Msg, message.WParam, message.LParam);
        }

        switch (m.Msg) {
            case Win32.WM_DRAWCLIPBOARD:
                if (_capsLockPressed) {
                    _capsLockPressed = false;
                    TranslateAndShow();
                }

                SendToNext(m);
                break;
            case Win32.WM_CHANGECBCHAIN:
                if (m.WParam == _nextClipMonitor) {
                    _nextClipMonitor = m.LParam;
                }
                else {
                    SendToNext(m);
                }

                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }

    [GeneratedRegex("token: '(\\w+)'")]
    private static partial Regex TokenRegex();

    private void InitClipMonitor()
    {
        _nextClipMonitor = Win32.SetClipboardViewer(Handle);
    }

    private void InitForm()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MaximumSize = Screen.FromHandle(Handle).Bounds.Size / 2;
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        Visible = false;
    }

    private unsafe void InitHook()
    {
        _mouseHook.MouseMoveEvent += (_, e) => {
            var point = new Point(e.X, e.Y);
            point.Offset(new Point(_mouseMargin));
            Location = point;
        };
        _keyboardHook.KeyUpEvent += (_, e) => {
            switch (e.vkCode) {
                case VkCode.VK_CAPITAL: {
                    var keyInputs = new Win32.InputStruct[4];

                    keyInputs[0].type = Win32.INPUT_KEYBOARD;
                    keyInputs[0].ki.wVk = VkCode.VK_CONTROL;

                    keyInputs[1].type = Win32.INPUT_KEYBOARD;
                    keyInputs[1].ki.wVk = VkCode.VK_C;

                    keyInputs[2].type = Win32.INPUT_KEYBOARD;
                    keyInputs[2].ki.wVk = VkCode.VK_C;
                    keyInputs[2].ki.dwFlags = Win32.KEYEVENTF_KEYUP;

                    keyInputs[3].type = Win32.INPUT_KEYBOARD;
                    keyInputs[3].ki.wVk = VkCode.VK_CONTROL;
                    keyInputs[3].ki.dwFlags = Win32.KEYEVENTF_KEYUP;

                    Win32.SendInput((uint)keyInputs.Length, keyInputs, sizeof(Win32.InputStruct));
                    _capsLockPressed = true;
                    return true;
                }

                case VkCode.VK_ESCAPE:
                    Hide();
                    break;
                default:
                    return false;
            }

            return false;
        };
    }

    private void InitHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Add( //
            "User-Agent"
  , "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36");
    }

    private void InitLabelDest()
    {
        _labelDest.Font = new Font(_labelDest.Font.FontFamily, 16);
        _labelDest.BorderStyle = BorderStyle.None;
        _labelDest.Dock = DockStyle.Fill;
        _labelDest.AutoSize = true;
        Controls.Add(_labelDest);
    }

    private void InitTokenCookie()
    {
        if (File.Exists(_stateFile)) {
            var lines = File.ReadLines(_stateFile).ToArray();
            _token = lines[0];
            _cookie = lines[1];
            _httpClient.DefaultRequestHeaders.Add(nameof(Cookie), _cookie);
        }
        else {
            Task.Run(UpdateStateFile);
        }
    }

    private void TranslateAndShow()
    {
        var clipText = ClipboardService.GetText();
        if (clipText.NullOrWhiteSpace()) {
            return;
        }

        _labelDest.Text = Ln.Translating;
        Task.Run(() => {
            var translateText = TranslateText(clipText);
            ClipboardService.SetText(translateText);
            Invoke(() => { _labelDest.Text = translateText; });
        });

        var point = Cursor.Position;
        point.Offset(new Point(_mouseMargin));
        Location = point;
        Show();
    }

    private string TranslateText(string sourceText)
    {
        while (true) {
            var sign = BaiduSignCracker.Sign(sourceText);
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
                                                        new("from", "auto")
                                              , new( //
                                                            "to"
                                                  , CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                                              , new("query", sourceText)
                                              , new("simple_means_flag", "3")
                                              , new("sign", sign)
                                              , new("token", _token)
                                              , new("domain", "common")
                                                    });

            var rsp = _httpClient.PostAsync(_TRANSLATE_API_URL, content).Result;
            var rspObj = rsp.Content.ReadAsStringAsync().Result.Object<BaiduTranslateResultDto.Root>();
            if (rspObj.error == 0) {
                return string.Join(Environment.NewLine, rspObj.trans_result.data.Select(x => x.dst));
            }

            Console.Error.WriteLine(rspObj.Json().UnicodeDe());
            Console.Error.WriteLine(rsp.Headers.Json());

            // cookie or token invalid
            Task.Delay(_RETRY_WAIT_MIL_SEC).Wait();
            UpdateStateFile();
        }
    }

    private void UpdateStateFile()
    {
        var rsp = _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _TRANSLATE_HOME_URL)).Result;
        _cookie = string.Join(
            ';', rsp.Headers.First(x => x.Key == "Set-Cookie").Value.Select(x => x.Split(';').First()));
        _httpClient.DefaultRequestHeaders.Remove(nameof(Cookie));
        _httpClient.DefaultRequestHeaders.Add(nameof(Cookie), _cookie);
        var html = _httpClient.GetStringAsync(_TRANSLATE_HOME_URL).Result;
        _token = TokenRegex().Match(html).Groups[1].Value;

        File.WriteAllLines(_stateFile, new[] { _token, _cookie });
    }
}
#endif
#if NET7_0_WINDOWS
using System.Net;
using System.Text.RegularExpressions;
using Dot.Native;
using Dot.Tran.Dto;
using NSExt.Extensions;
using TextCopy;
using Size = System.Drawing.Size;

namespace Dot.Tran;

internal sealed partial class FrmMain : Form
{
    private const    int              _HIDING_MIL_SECS    = 5000; //                             隐藏窗体时间（秒
    private const    int              _RETRY_WAIT_MIL_SEC = 1000; //                             重试等待时间（秒）
    private const    string           _TRANSLATE_API_URL  = "https://fanyi.baidu.com/v2transapi";
    private const    string           _TRANSLATE_HOME_URL = "https://fanyi.baidu.com";
    private const    double           _WINDOW_OPACITY     = .5;         //                       窗体透明度
    private static   ManualResetEvent _mre                = new(false); //           隐藏窗体侦测线程信号
    private readonly HttpClient       _httpClient         = new();
    private readonly Label            _labelDest          = new(); //                            显示翻译内容的文本框

    private readonly MouseHook _mouseHook   = new();
    private readonly Size      _mouseMargin = new(10, 10);
    private          bool      _disposed;
    private          DateTime? _latestActiveTime; //                                           窗体最后激活时间
    private          nint      _nextHwnd;         //                                           下一个剪贴板监视链对象句柄
    private          string    _token = "ae72ebad4113270fd26ada5125301268";

    public FrmMain()
    {
        InitForm();
        InitHook();
        InitLabelDest();
        InitHttpClient();

        _nextHwnd = Win32.SetClipboardViewer(Handle);

        Task.Run(HideWindow);
    }

    ~FrmMain()
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
            _mre = null; // 结束定时隐藏窗体线程
            _mre?.Dispose();
            _httpClient?.Dispose();
            _labelDest?.Dispose();
            _mouseHook?.Dispose();
        }

        Win32.ChangeClipboardChain(Handle, _nextHwnd); // 从剪贴板监视链移除本窗体
        _disposed = true;
    }

    protected override void WndProc(ref Message m)
    {
        void SendToNext(Message message)
        {
            _ = Win32.SendMessageA(_nextHwnd, (uint)message.Msg, message.WParam, message.LParam);
        }

        switch (m.Msg) {
            case Win32.WM_DRAWCLIPBOARD:
                DisplayClipboardData();
                SendToNext(m);
                break;
            case Win32.WM_CHANGECBCHAIN:
                Console.WriteLine(Win32.WM_CHANGECBCHAIN);
                if (m.WParam == _nextHwnd) {
                    _nextHwnd = m.LParam;
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
    private static partial Regex MyRegex();

    /// <summary>
    ///     显示剪贴板内容.
    /// </summary>
    private void DisplayClipboardData()
    {
        var clipText = ClipboardService.GetText();
        if (clipText.NullOrWhiteSpace()) {
            return;
        }

        _labelDest.Text = Str.Translating;
        Task.Run(() => {
            var translateText = _labelDest.Text = TranslateText(clipText);
            Invoke(() => { _labelDest.Text = translateText; });
        });

        var point = Cursor.Position;
        point.Offset(new Point(_mouseMargin));
        Location = point;
        Show();
        _latestActiveTime = DateTime.Now;
        _mre.Set();
    }

    private async Task HideWindow()
    {
        while (_mre is not null) {
            while (Visible) {
                await Task.Delay(100);
                if (_latestActiveTime is null ||
                    !((DateTime.Now - _latestActiveTime.Value).TotalMilliseconds > _HIDING_MIL_SECS)) {
                    continue;
                }

                Invoke(Hide);
            }

            _mre.WaitOne();
        }
    }

    private void InitForm()
    {
        AutoSize        = true;
        AutoSizeMode    = AutoSizeMode.GrowAndShrink;
        FormBorderStyle = FormBorderStyle.None;
        Opacity         = _WINDOW_OPACITY;
        TopMost         = true;
        Visible         = false;
    }

    private void InitHook()
    {
        _mouseHook.MouseMoveEvent += (_, e) => {
            var point = new Point(e.X, e.Y);
            point.Offset(new Point(_mouseMargin));
            Location = point;
        };
    }

    private void InitHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Add( //
            "User-Agent"
          , " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36");
    }

    private void InitLabelDest()
    {
        _labelDest.BorderStyle = BorderStyle.None;
        _labelDest.Dock        = DockStyle.Fill;
        _labelDest.AutoSize    = true;
        Controls.Add(_labelDest);
    }

    private string TranslateText(string sourceText)
    {
        while (true) {
            var hash = sourceText.Length > 30
                ? string.Concat(sourceText.AsSpan()[..10], sourceText.AsSpan(sourceText.Length / 2 - 5, 10)
                              , sourceText.AsSpan()[^10..])
                : sourceText;
            var sign = BaiduSignCracker.Sign(hash);
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
                                                        new("from", "auto")
                                                      , new("to", "zh")
                                                      , new("query", sourceText)
                                                      , new("simple_means_flag", "3")
                                                      , new("sign", sign)
                                                      , new("token", _token)
                                                      , new("domain", "common")
                                                    });

            var rsp    = _httpClient.PostAsync(_TRANSLATE_API_URL, content).Result;
            var rspObj = rsp.Content.ReadAsStringAsync().Result.Object<BaiduTranslateResultDto.Root>();
            if (rspObj.error == 0) {
                return string.Join(Environment.NewLine, rspObj.trans_result.data.Select(x => x.dst));
            }

            Console.Error.WriteLine(rspObj.Json().UnicodeDe());
            Console.Error.WriteLine(rsp.Headers.Json());

            //cookie or token invalid
            Task.Delay(_RETRY_WAIT_MIL_SEC).Wait();
            var cookie = string.Join(
                ';', rsp.Headers.First(x => x.Key == "Set-Cookie").Value.Select(x => x.Split(';').First()));
            _httpClient.DefaultRequestHeaders.Remove(nameof(Cookie));
            _httpClient.DefaultRequestHeaders.Add(nameof(Cookie), cookie);
            var html = _httpClient.GetStringAsync(_TRANSLATE_HOME_URL).Result;
            _token = MyRegex().Match(html).Groups[1].Value;
        }
    }
}
#endif
#if NET7_0_WINDOWS
using NSExt.Extensions;
using Colors = System.Drawing.Color;
using Padding = System.Windows.Forms.Padding;
using Size = System.Drawing.Size;

namespace Dot.Tran;

internal sealed class FrmMain : Form
{
    private const  int _HIDING_MIL_SECS = 1000; //                                                鼠标离开超过此时间后隐藏窗体
    private const  string _TOKEN_URL = "https://fanyi.qq.com/api/reauth12f";
    private const  string _URL = "https://fanyi.qq.com/api/translate";
    private static ManualResetEvent _mre = new(false); //                             隐藏窗体侦测线程信号

    private readonly Colors _bgColor //                                                           背景颜色
        = Colors.FromArgb(0xff, 0x1e, 0x1e, 0x1e);

    private readonly Colors _foreColor = Colors.White; //                             字体颜色

    private readonly HttpClient  _httpClient     = new();
    private readonly RichTextBox _richTextSource = new(); //                                      显示剪贴板内容的富文本框
    private readonly TextBox     _textDest       = new(); //                                      显示翻译内容的文本框
    private readonly Padding     _windowPadding  = new(10, 10, 10, 10); //  窗体边距
    private readonly Size        _windowSize     = new(640, 360); //                 窗体大小
    private          DateTime?   _mouseLeaveTime; //                                              鼠标离开窗体时间
    private          nint        _nextHwnd; //                                                    下一个剪贴板监视链对象句柄

    public FrmMain()
    {
        InitForm();

        InitTextSource();
        InitTextDest();

        _nextHwnd = Win32.SetClipboardViewer(Handle);

        Task.Run(HideWindow);
    }

    ~FrmMain()
    {
        _mre?.Dispose();
        _richTextSource?.Dispose();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // 结束定时隐藏窗体线程
        _mre = null;

        // 从剪贴板监视链移除本窗体
        Win32.ChangeClipboardChain(Handle, _nextHwnd);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _mouseLeaveTime = null;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (ClientRectangle.Contains(PointToClient(MousePosition))) {
            return;
        }

        _mouseLeaveTime = DateTime.Now;
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

    /// <summary>
    ///     显示剪贴板内容.
    /// </summary>
    private void DisplayClipboardData()
    {
        var clipData = Clipboard.GetDataObject();
        if (clipData is null) {
            return;
        }

        if (clipData.GetDataPresent(DataFormats.Rtf)) {
            _richTextSource.Rtf = clipData.GetData(DataFormats.Rtf) as string;
        }
        else if (clipData.GetDataPresent(DataFormats.Text)) {
            _richTextSource.Text = clipData.GetData(DataFormats.Text) as string;
        }
        else {
            return;
        }

        var mousePos = Cursor.Position;
        mousePos.Offset(-_windowPadding.Left, -_windowPadding.Top);
        Location        = mousePos;
        Visible         = true;
        _mouseLeaveTime = null;
        _mre.Set();
    }

    private TokenStruct GetTranslateToken()
    {
        using var rsp    = _httpClient.PostAsync(_TOKEN_URL, new StringContent(string.Empty)).Result;
        var       rspStr = rsp.Content.ReadAsStringAsync().Result;
        return rspStr.Object<TokenStruct>();
    }

    private int HalfHeight()
    {
        return (_windowSize.Height - _windowPadding.Top - _windowPadding.Bottom) / 2 - _windowPadding.Top / 2;
    }

    private async Task HideWindow()
    {
        while (_mre is not null) {
            while (Visible) {
                await Task.Delay(100);
                if (_mouseLeaveTime is null ||
                    !((DateTime.Now - _mouseLeaveTime.Value).TotalMilliseconds > _HIDING_MIL_SECS)) {
                    continue;
                }

                Invoke(() => {
                    Visible = false;
                    _richTextSource.Clear();
                    _textDest.Clear();
                });
            }

            _mre.WaitOne();
        }
    }

    private void InitForm()
    {
        BackColor       = _bgColor;
        FormBorderStyle = FormBorderStyle.None;
        Padding         = _windowPadding;
        Size            = _windowSize;
        TopMost         = true;
        Visible         = false;
    }

    private void InitTextDest()
    {
        _textDest.BackColor   =  _bgColor;
        _textDest.BorderStyle =  BorderStyle.None;
        _textDest.Dock        =  DockStyle.Bottom;
        _textDest.ForeColor   =  _foreColor;
        _textDest.Height      =  HalfHeight();
        _textDest.Margin      =  new Padding(0, _windowPadding.Top / 2, 0, 0);
        _textDest.MouseEnter  += (_, e) => OnMouseEnter(e);
        _textDest.Multiline   =  true;
        _textDest.ScrollBars  =  ScrollBars.None;
        Controls.Add(_textDest);
    }

    private void InitTextSource()
    {
        _richTextSource.TextChanged += (sender, e) => {
            if (_richTextSource.Text.NullOrWhiteSpace()) {
                return;
            }

            _textDest.Text = Str.Translating;
            _textDest.Text = TranslateText(_richTextSource.Text);
        };
        _richTextSource.BackColor   =  _bgColor;
        _richTextSource.BorderStyle =  BorderStyle.None;
        _richTextSource.Dock        =  DockStyle.Top;
        _richTextSource.ForeColor   =  _foreColor;
        _richTextSource.Height      =  HalfHeight();
        _richTextSource.Margin      =  new Padding(0, 0, 0, _windowPadding.Top / 2);
        _richTextSource.MouseEnter  += (_, e) => OnMouseEnter(e);
        _richTextSource.ScrollBars  =  RichTextBoxScrollBars.None;
        Controls.Add(_richTextSource);
    }

    private string TranslateText(string sourceText)
    {
        var token = GetTranslateToken();
        var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
                                                    new("source", "auto")
                                                  , new("target", "zh")
                                                  , new("qtv", token.Qtv.Url())
                                                  , new("qtk", token.Qtk.Url())
                                                  , new("sourceText", sourceText.Url())
                                                });
        var rsp = _httpClient.PostAsync(_URL, content).Result;
        var ret = rsp.Content.ReadAsStringAsync().Result;

        return ret;
    }

    private readonly struct TokenStruct
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public string Qtk { get; init; }

        public string Qtv { get; init; }

        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
#endif
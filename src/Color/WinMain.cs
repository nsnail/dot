#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Dot.Native;
using TextCopy;

namespace Dot.Color;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WinMain : Form
{
    private readonly Bitmap  _bmp;
    private readonly WinInfo _winInfo = new(); //小图窗口

    private bool _disposed;

    public WinMain()
    {
        // 隐藏控制台窗口，避免捕获到截屏
        Win32.ShowWindow(Win32.GetConsoleWindow(), Win32.SW_HIDE);

        FormBorderStyle = FormBorderStyle.None;
        Size            = Screen.PrimaryScreen!.Bounds.Size;
        StartPosition   = FormStartPosition.Manual;
        Location        = new Point(0, 0);
        Opacity         = 0.01d; //主窗体加载截图过程设置为透明避免闪烁
        _bmp            = new Bitmap(Size.Width, Size.Height);
        using var g = Graphics.FromImage(_bmp);
        g.CopyFromScreen(0, 0, 0, 0, Size);
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
            _bmp?.Dispose();
            _winInfo?.Dispose();
        }

        _disposed = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) {
            Application.Exit();
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        _winInfo.Show();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        var color = _bmp.GetPixel(e.X, e.Y);
        ClipboardService.SetText($"{e.X},{e.Y} #{color.R:X2}{color.G:X2}{color.B:X2}({color.R},{color.G},{color.B})");
        Application.Exit();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        // 移动鼠标时更新小图窗口
        _winInfo.UpdateImage(_bmp, e.X, e.Y);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.DrawImage(_bmp, 0, 0);
        Opacity = 1;
    }
}

#endif
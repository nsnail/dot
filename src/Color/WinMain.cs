using TextCopy;

namespace Dot.Color;

public class WinMain : Form
{
    private readonly Bitmap  _bmp;
    private          bool    _disposed;
    private readonly WinInfo _winInfo = new();

    public WinMain()
    {
        FormBorderStyle = FormBorderStyle.None;
        Size            = Screen.PrimaryScreen!.Bounds.Size;
        StartPosition   = FormStartPosition.Manual;
        Location        = new Point(0, 0);
        Opacity         = 0.01d;
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
        if (_disposed) return;
        if (disposing) {
            _bmp?.Dispose();
            _winInfo?.Dispose();
        }

        _disposed = true;
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Application.Exit();
    }

    protected override void OnLoad(EventArgs e)
    {
        _winInfo.Show();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        var color = _bmp.GetPixel(e.X, e.Y);
        ClipboardService.SetText(
            $"{e.X},{e.Y} #{color.R.ToString("X2")}{color.G.ToString("X2")}{color.B.ToString("X2")}({color.R},{color.G},{color.B})");
        Application.Exit();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _winInfo.UpdateImage(_bmp, e.X, e.Y);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.DrawImage(_bmp, 0, 0);
        Opacity = 1;
    }
}
using System.Drawing.Drawing2D;

namespace Dot.Color;

public class WinInfo : Form
{
    private const    int        _WINDOW_SIZE = 480;
    private const    int        _ZOOM_RATE   = 16;
    private          bool       _disposed;
    private readonly Graphics   _graphics;
    private readonly PictureBox _pbox;

    public WinInfo()
    {
        FormBorderStyle              = FormBorderStyle.None;
        TopMost                      = true;
        MinimizeBox                  = false;
        MaximizeBox                  = false;
        Size                         = new Size(_WINDOW_SIZE, _WINDOW_SIZE);
        StartPosition                = FormStartPosition.Manual;
        Location                     = new Point(0, 0);
        _pbox                        = new PictureBox();
        _pbox.Location               = new Point(0, 0);
        _pbox.Size                   = Size;
        _pbox.Image                  = new Bitmap(_WINDOW_SIZE, _WINDOW_SIZE);
        _graphics                    = Graphics.FromImage(_pbox.Image);
        _graphics.InterpolationMode  = InterpolationMode.NearestNeighbor;
        _graphics.CompositingQuality = CompositingQuality.HighQuality;
        _graphics.SmoothingMode      = SmoothingMode.None;
        Controls.Add(_pbox);
    }


    ~WinInfo()
    {
        Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_disposed) return;
        if (disposing) {
            _graphics?.Dispose();
            _pbox?.Dispose();
        }

        _disposed = true;
    }


    public void UpdateImage(Bitmap img, int x, int y)
    {
        var copySize = new Size(_WINDOW_SIZE / _ZOOM_RATE, _WINDOW_SIZE / _ZOOM_RATE);
        _graphics.DrawImage(img, new Rectangle(0, 0, _WINDOW_SIZE, _WINDOW_SIZE) //
                          , x - copySize.Width  / 2                              //
                          , y - copySize.Height / 2                              //
                          , copySize.Width, copySize.Height, GraphicsUnit.Pixel);
        using var pen = new Pen(System.Drawing.Color.Aqua);
        _graphics.DrawRectangle(pen, _WINDOW_SIZE / 2 - _ZOOM_RATE / 2 //
                              , _WINDOW_SIZE      / 2 - _ZOOM_RATE / 2 //
                              , _ZOOM_RATE, _ZOOM_RATE);
        var posColor = img.GetPixel(x, y);
        _graphics.FillRectangle(Brushes.Black, 0, _WINDOW_SIZE - 30, _WINDOW_SIZE, 30);
        _graphics.DrawString($"{Str.ClickCopyColor}  X: {x} Y: {y} RGB({posColor.R},{posColor.G},{posColor.B})"
                           , new Font(FontFamily.GenericSerif, 10), Brushes.White, 0, _WINDOW_SIZE - 20);
        _pbox.Refresh();
    }
}
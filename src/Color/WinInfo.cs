#if NET7_0_WINDOWS
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Size = System.Drawing.Size;

namespace Dot.Color;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
internal sealed class WinInfo : Form
{
    private const    int        _WINDOW_SIZE = 480; // 窗口大小
    private const    int        _ZOOM_RATE   = 16;  // 缩放倍率
    private readonly Graphics   _graphics;
    private readonly PictureBox _pbox;
    private          bool       _disposed;

    public WinInfo()
    {
        #pragma warning disable IDE0017
        FormBorderStyle              =  FormBorderStyle.None;
        TopMost                      =  true;
        MinimizeBox                  =  false;
        MaximizeBox                  =  false;
        Size                         =  new Size(_WINDOW_SIZE, _WINDOW_SIZE);
        StartPosition                =  FormStartPosition.Manual;
        Location                     =  new Point(0, 0);
        _pbox                        =  new PictureBox();
        _pbox.Location               =  new Point(0, 0);
        _pbox.Size                   =  Size;
        _pbox.Image                  =  new Bitmap(_WINDOW_SIZE, _WINDOW_SIZE);
        _graphics                    =  Graphics.FromImage(_pbox.Image);
        _graphics.InterpolationMode  =  InterpolationMode.NearestNeighbor; // 指定最临近插值法，禁止平滑缩放（模糊）
        _graphics.CompositingQuality =  CompositingQuality.HighQuality;
        _graphics.SmoothingMode      =  SmoothingMode.None;
        _pbox.MouseEnter             += PboxOnMouseEnter;
        Controls.Add(_pbox);
        #pragma warning restore IDE0017
    }

    ~WinInfo()
    {
        Dispose(false);
    }

    public void UpdateImage(Bitmap img, int x, int y)
    {
        // 计算复制小图的区域
        var copySize = new Size(_WINDOW_SIZE / _ZOOM_RATE, _WINDOW_SIZE / _ZOOM_RATE);
        _graphics.DrawImage(img, new Rectangle(0, 0, _WINDOW_SIZE, _WINDOW_SIZE) //
                          , x - copySize.Width  / 2                              // 左移x，使光标位置居中
                          , y - copySize.Height / 2                              // 上移y，使光标位置居中
                          , copySize.Width, copySize.Height, GraphicsUnit.Pixel);
        using var pen = new Pen(System.Drawing.Color.Aqua);            // 绘制准星
        _graphics.DrawRectangle(pen, _WINDOW_SIZE / 2 - _ZOOM_RATE / 2 //
                              , _WINDOW_SIZE      / 2 - _ZOOM_RATE / 2 //
                              , _ZOOM_RATE, _ZOOM_RATE);

        // 取鼠标位置颜色
        var posColor = img.GetPixel(x, y);

        // 绘制底部文字信息
        _graphics.FillRectangle(Brushes.Black, 0, _WINDOW_SIZE - 30, _WINDOW_SIZE, 30);
        _graphics.DrawString( //
            $"{Ln.ClickCopyColor}  X: {x} Y: {y} RGB({posColor.R},{posColor.G},{posColor.B})"
          , new Font(FontFamily.GenericSerif, 10) //
          , Brushes.White, 0, _WINDOW_SIZE - 20);

        // 触发重绘
        _pbox.Refresh();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_disposed) {
            return;
        }

        if (disposing) {
            _graphics?.Dispose();
            _pbox?.Dispose();
        }

        _disposed = true;
    }

    private void PboxOnMouseEnter(object sender, EventArgs e)
    {
        // 信息窗口避开鼠标指针指向区域
        Location = new Point(Location.X, Location.Y == 0 ? Screen.PrimaryScreen!.Bounds.Height - _WINDOW_SIZE : 0);
    }
}
#endif
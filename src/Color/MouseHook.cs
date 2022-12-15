// ReSharper disable ClassNeverInstantiated.Global

#if NET7_0_WINDOWS
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dot.Color;

internal sealed class MouseHook : IDisposable
{
    private const    int  _WH_MOUSE_LL    = 14;
    private const    int  _WM_LBUTTONDOWN = 0x0201;
    private const    int  _WM_MOUSEMOVE   = 0x0200;
    private readonly nint _hookId;
    private          bool _disposed;

    public MouseHook()
    {
        _hookId = SetHook(HookCallback);
    }

    ~MouseHook()
    {
        Dispose(false);
    }

    public event MouseEventHandler MouseEvent;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static nint SetHook(Func<int, nint, nint, nint> proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule  = curProcess.MainModule!;
        return Win32.SetWindowsHookEx(_WH_MOUSE_LL, proc, Win32.GetModuleHandle(curModule.ModuleName), 0);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) {
            return;
        }

        if (disposing) {
            //
        }

        if (_hookId != default) {
            Win32.UnhookWindowsHookEx(_hookId);
        }

        _disposed = true;
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0 || (wParam != _WM_MOUSEMOVE && wParam != _WM_LBUTTONDOWN)) {
            return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        var hookStruct = (Msllhookstruct)Marshal.PtrToStructure(lParam, typeof(Msllhookstruct))!;
        MouseEvent?.Invoke(null, new MouseEventArgs(                                           //
                               wParam == _WM_MOUSEMOVE ? MouseButtons.None : MouseButtons.Left //
                             , 0                                                               //
                             , hookStruct.X                                                    //
                             , hookStruct.Y                                                    //
                             , 0));
        return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct Msllhookstruct
    {
        [FieldOffset(0)] public readonly int X;
        [FieldOffset(4)] public readonly int Y;
    }
}
#endif
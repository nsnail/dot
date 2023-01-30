// ReSharper disable ClassNeverInstantiated.Global

#if NET7_0_WINDOWS
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dot.Native;

internal sealed class MouseHook : IDisposable
{
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

    public event MouseEventHandler MouseMoveEvent;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static nint SetHook(Win32.HookProc lpfn)
    {
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        return Win32.SetWindowsHookExA(Win32.WH_MOUSE_LL, lpfn, module!.BaseAddress, 0);
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
            Win32.UnhookWindowsHookExA(_hookId);
        }

        _disposed = true;
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        // ReSharper disable once InvertIf
        if (wParam == Win32.WM_MOUSEMOVE) {
            var hookStruct = (Msllhookstruct)Marshal.PtrToStructure(lParam, typeof(Msllhookstruct))!;
            MouseMoveEvent?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, hookStruct.X, hookStruct.Y, 0));
        }

        return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct Msllhookstruct
    {
        [FieldOffset(0)]
        public readonly int X;

        [FieldOffset(4)]
        public readonly int Y;
    }
}
#endif
// ReSharper disable ClassNeverInstantiated.Global

#if NET7_0_WINDOWS
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dot.Native;

internal sealed class KeyboardHook : IDisposable
{
    private readonly nint _hookId;
    private          bool _disposed;

    public KeyboardHook()
    {
        _hookId = SetHook(HookCallback);
    }

    ~KeyboardHook()
    {
        Dispose(false);
    }

    public delegate bool KeyUpEventHandler(object sender, Win32.KbdllhooksStruct e);

    public KeyUpEventHandler KeyUpEvent { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static nint SetHook(Win32.HookProc lpfn)
    {
        using var process = Process.GetCurrentProcess();
        using var module  = process.MainModule;
        return Win32.SetWindowsHookExA(Win32.WH_KEYBOARD_LL, lpfn, module!.BaseAddress, 0);
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
        if (nCode >= 0 && wParam == Win32.WM_KEYDOWN) {
            var hookStruct = (Win32.KbdllhooksStruct)Marshal.PtrToStructure(lParam, typeof(Win32.KbdllhooksStruct))!;
            if (KeyUpEvent?.Invoke(null, hookStruct) ?? false) {
                return -1;
            }
        }

        return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }
}

#endif
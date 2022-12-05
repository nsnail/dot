using System.Runtime.InteropServices;

namespace Dot.Color;

public static class Win32
{
    public delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

    private const string _GDI32_DLL    = "gdi32.dll";
    private const string _KERNEL32_DLL = "kernel32.dll";
    private const string _USER32_DLL   = "user32.dll";


    public const int SW_HIDE = 0;


    [DllImport(_USER32_DLL)]
    public static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport(_KERNEL32_DLL)]
    public static extern nint GetConsoleWindow();


    [DllImport(_USER32_DLL)]
    public static extern nint GetDesktopWindow();


    [DllImport(_KERNEL32_DLL, CharSet = CharSet.Unicode)]
    public static extern nint GetModuleHandle(string lpModuleName);

    [DllImport(_GDI32_DLL)]
    public static extern uint GetPixel(nint dc, int x, int y);

    [DllImport(_USER32_DLL)]
    public static extern nint GetWindowDC(nint hWnd);

    [DllImport(_USER32_DLL)]
    public static extern int ReleaseDC(nint hWnd, nint dc);

    [DllImport(_USER32_DLL)]
    public static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

    [DllImport(_USER32_DLL)]
    public static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport(_USER32_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(nint hhk);
}
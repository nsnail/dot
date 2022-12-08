using System.Runtime.InteropServices;

namespace Dot.Color;

public static partial class Win32
{
    [StructLayout(LayoutKind.Explicit)]
    internal ref struct Systemtime
    {
        [FieldOffset(6)]  public ushort wDay;
        [FieldOffset(4)]  public ushort wDayOfWeek;
        [FieldOffset(8)]  public ushort wHour;
        [FieldOffset(14)] public ushort wMilliseconds;
        [FieldOffset(10)] public ushort wMinute;
        [FieldOffset(2)]  public ushort wMonth;
        [FieldOffset(12)] public ushort wSecond;
        [FieldOffset(0)]  public ushort wYear;
    }

    public delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

    private const string _GDI32_DLL    = "gdi32.dll";
    private const string _KERNEL32_DLL = "kernel32.dll";
    private const string _USER32_DLL   = "user32.dll";
    public const  int    SW_HIDE       = 0;


    [LibraryImport(_USER32_DLL)]
    internal static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [LibraryImport(_KERNEL32_DLL)]
    internal static partial nint GetConsoleWindow();


    [LibraryImport(_USER32_DLL)]
    internal static partial nint GetDesktopWindow();


    [LibraryImport(_KERNEL32_DLL, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial nint GetModuleHandle(string lpModuleName);

    [LibraryImport(_GDI32_DLL)]
    internal static partial uint GetPixel(nint dc, int x, int y);

    [LibraryImport(_USER32_DLL)]
    internal static partial nint GetWindowDC(nint hWnd);

    [LibraryImport(_USER32_DLL)]
    internal static partial int ReleaseDC(nint hWnd, nint dc);


    [LibraryImport(_KERNEL32_DLL)]
    internal static partial void SetLocalTime(Systemtime st);

    [LibraryImport(_USER32_DLL)]
    internal static partial nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

    [LibraryImport(_USER32_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ShowWindow(nint hWnd, int nCmdShow);

    [LibraryImport(_USER32_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnhookWindowsHookEx(nint hhk);
}
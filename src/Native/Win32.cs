// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global

#pragma warning disable SA1307, SX1309

using System.Runtime.InteropServices;

namespace Dot.Native;

internal static partial class Win32
{
    public const  int    CS_DROP_SHADOW   = 0x20000;
    public const  int    GCL_STYLE        = -26;
    public const  int    HC_ACTION        = 0;
    public const  int    INPUT_KEYBOARD   = 1;
    public const  int    KEYEVENTF_KEYUP  = 0x0002;
    public const  int    SW_HIDE          = 0;
    public const  int    WH_KEYBOARD_LL   = 13;
    public const  int    WH_MOUSE_LL      = 14;
    public const  int    WM_CHANGECBCHAIN = 0x030D;
    public const  int    WM_DRAWCLIPBOARD = 0x308;
    public const  int    WM_KEYDOWN       = 0x0100;
    public const  int    WM_KEYUP         = 0x0101;
    public const  int    WM_LBUTTONDOWN   = 0x0201;
    public const  int    WM_MOUSEMOVE     = 0x0200;
    private const string _GDI32_DLL       = "gdi32.dll";
    private const string _KERNEL32_DLL    = "kernel32.dll";
    private const string _USER32_DLL      = "user32.dll";

    public delegate nint HookProc(int nCode, nint wParam, nint lParam);

    [LibraryImport(_USER32_DLL)]
    public static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [LibraryImport(_USER32_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ChangeClipboardChain(nint hWndRemove, nint hWndNewNext);

    [LibraryImport(_USER32_DLL)]
    public static partial int GetClassLongA(nint hWnd, int nIndex);

    [LibraryImport(_KERNEL32_DLL)]
    public static partial nint GetConsoleWindow();

    [LibraryImport(_USER32_DLL)]
    public static partial nint GetDesktopWindow();

    [LibraryImport(_KERNEL32_DLL, StringMarshalling = StringMarshalling.Utf16)]
    public static partial nint GetModuleHandleA(string lpModuleName);

    [LibraryImport(_GDI32_DLL)]
    public static partial uint GetPixel(nint dc, int x, int y);

    [LibraryImport(_USER32_DLL)]
    public static partial nint GetWindowDC(nint hWnd);

    [LibraryImport(_USER32_DLL)]
    public static partial int ReleaseDC(nint hWnd, nint dc);

    [LibraryImport(_USER32_DLL)]
    public static partial uint SendInput(uint cInputs, [MarshalAs(UnmanagedType.LPArray)] InputStruct[] inputs
                                       , int  cbSize);

    [LibraryImport(_USER32_DLL)]
    public static partial int SendMessageA(nint hwnd, uint wMsg, nint wParam, nint lParam);

    [LibraryImport(_USER32_DLL)]
    public static partial int SetClassLongA(nint hWnd, int nIndex, int dwNewLong);

    [LibraryImport(_USER32_DLL)]
    public static partial int SetClipboardViewer(nint hWnd);

    [LibraryImport(_KERNEL32_DLL)]
    public static partial void SetLocalTime(Systemtime st);

    [LibraryImport(_USER32_DLL)]
    public static partial nint SetWindowsHookExA(int idHook, HookProc lpfn, nint hMod, uint dwThreadId);

    [LibraryImport(_USER32_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(nint hWnd, int nCmdShow);

    [LibraryImport(_USER32_DLL)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnhookWindowsHookExA(nint hhk);

    [StructLayout(LayoutKind.Explicit)]
    public readonly struct KbdllhooksStruct
    {
        [FieldOffset(0)]
        public readonly uint vkCode;

        [FieldOffset(16)]
        private readonly nint dwExtraInfo;

        [FieldOffset(8)]
        private readonly uint flags;

        [FieldOffset(4)]
        private readonly uint scanCode;

        [FieldOffset(12)]
        private readonly uint time;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputStruct
    {
        [FieldOffset(8)]
        public KeybdInputStruct ki;

        [FieldOffset(0)]
        public uint type;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct KeybdInputStruct
    {
        [FieldOffset(4)]
        public uint dwFlags;

        [FieldOffset(0)]
        public ushort wVk;

        [FieldOffset(20)]
        private readonly long _; // 补位以匹配 UNION的MOUSEINPUT参数 （28bytes）

        [FieldOffset(12)]
        private readonly nint dwExtraInfo;

        [FieldOffset(8)]
        private readonly uint time;

        [FieldOffset(2)]
        private readonly ushort wScan;
    }

    [StructLayout(LayoutKind.Explicit)]
    public ref struct Systemtime
    {
        [FieldOffset(6)]
        public ushort wDay;

        [FieldOffset(4)]
        public ushort wDayOfWeek;

        [FieldOffset(8)]
        public ushort wHour;

        [FieldOffset(14)]
        public ushort wMilliseconds;

        [FieldOffset(10)]
        public ushort wMinute;

        [FieldOffset(2)]
        public ushort wMonth;

        [FieldOffset(12)]
        public ushort wSecond;

        [FieldOffset(0)]
        public ushort wYear;
    }
}
// Ported from glfw/src/win32_platform.h -- GLFW 3.6 Win32 platform types
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2019 Camilla Loewy <elmindreda@glfw.org>
//
// This software is provided 'as-is', without any express or implied
// warranty. In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would
//    be appreciated but is not required.
//
// 2. Altered source versions must be plainly marked as such, and must not
//    be misrepresented as being the original software.
//
// 3. This notice may not be removed or altered from any source
//    distribution.

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Glfw
{
    // =======================================================================
    //  Win32 interop structs (matching C ABI layouts for p/invoke)
    // =======================================================================

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct WNDCLASSEXW
    {
        public uint cbSize;
        public uint style;
        public delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint> lpfnWndProc; // WNDPROC
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;       // HINSTANCE
        public nint hIcon;           // HICON
        public nint hCursor;         // HCURSOR
        public nint hbrBackground;   // HBRUSH
        public char* lpszMenuName;
        public char* lpszClassName;
        public nint hIconSm;         // HICON
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public nint hwnd;            // HWND
        public uint message;
        public nuint wParam;
        public nint lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct MONITORINFOEXW
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
        public fixed char szDevice[32];    // CCHDEVICENAME = 32
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct DEVMODEW
    {
        public fixed char dmDeviceName[32]; // CCHDEVICENAME = 32
        public ushort dmSpecVersion;
        public ushort dmDriverVersion;
        public ushort dmSize;
        public ushort dmDriverExtra;
        public uint dmFields;
        // union: printer or display
        public int dmPositionX;             // POINTL.x
        public int dmPositionY;             // POINTL.y
        public uint dmDisplayOrientation;
        public uint dmDisplayFixedOutput;
        // end union
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        public fixed char dmFormName[32];   // CCHFORMNAME = 32
        public ushort dmLogPixels;
        public uint dmBitsPerPel;
        public uint dmPelsWidth;
        public uint dmPelsHeight;
        public uint dmDisplayFlags;         // union with dmNup
        public uint dmDisplayFrequency;
        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;
        public uint dmPanningWidth;
        public uint dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public byte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public uint length;
        public uint flags;
        public uint showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TRACKMOUSEEVENT
    {
        public uint cbSize;
        public uint dwFlags;
        public nint hwndTrack;       // HWND
        public uint dwHoverTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TITLEBARINFOEX
    {
        public uint cbSize;
        public RECT rcTitleBar;
        public fixed uint rgstate[6]; // CCHILDREN_TITLEBAR + 1 = 6
        public TitlebarRectangles rgrect;
    }

    [InlineArray(6)]
    public struct TitlebarRectangles { private RECT element; }

    [StructLayout(LayoutKind.Sequential)]
    public struct HIGHCONTRASTW
    {
        public uint cbSize;
        public uint dwFlags;
        public nint lpszDefaultScheme; // LPWSTR
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CHANGEFILTERSTRUCT
    {
        public uint cbSize;
        public uint ExtStatus;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public nint hwndTarget;      // HWND
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public nint hDevice;         // HANDLE
        public nuint wParam;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct RAWMOUSE
    {
        [FieldOffset(0)] public ushort usFlags;
        // union { ULONG ulButtons; struct { USHORT usButtonFlags; USHORT usButtonData; } }
        [FieldOffset(4)] public ushort usButtonFlags;
        [FieldOffset(6)] public ushort usButtonData;
        [FieldOffset(8)] public uint ulRawButtons;
        [FieldOffset(12)] public int lLastX;
        [FieldOffset(16)] public int lLastY;
        [FieldOffset(20)] public uint ulExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUT
    {
        public RAWINPUTHEADER header;
        // union: mouse, keyboard, hid -- we only need mouse
        public RAWMOUSE mouse;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct DISPLAY_DEVICEW
    {
        public uint cb;
        public fixed char DeviceName[32];
        public fixed char DeviceString[128];
        public uint StateFlags;
        public fixed char DeviceID[128];
        public fixed char DeviceKey[128];
    }

    // BITMAPV5HEADER for CreateDIBSection (used for cursor creation)
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPV5HEADER
    {
        public uint bV5Size;
        public int bV5Width;
        public int bV5Height;
        public ushort bV5Planes;
        public ushort bV5BitCount;
        public uint bV5Compression;
        public uint bV5SizeImage;
        public int bV5XPelsPerMeter;
        public int bV5YPelsPerMeter;
        public uint bV5ClrUsed;
        public uint bV5ClrImportant;
        public uint bV5RedMask;
        public uint bV5GreenMask;
        public uint bV5BlueMask;
        public uint bV5AlphaMask;
        public uint bV5CSType;
        // CIEXYZTRIPLE (3*3*4 = 36 bytes)
        public int ciexyzRed_X, ciexyzRed_Y, ciexyzRed_Z;
        public int ciexyzGreen_X, ciexyzGreen_Y, ciexyzGreen_Z;
        public int ciexyzBlue_X, ciexyzBlue_Y, ciexyzBlue_Z;
        public uint bV5GammaRed;
        public uint bV5GammaGreen;
        public uint bV5GammaBlue;
        public uint bV5Intent;
        public uint bV5ProfileData;
        public uint bV5ProfileSize;
        public uint bV5Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONINFO
    {
        public int fIcon;           // BOOL
        public uint xHotspot;
        public uint yHotspot;
        public nint hbmMask;        // HBITMAP
        public nint hbmColor;       // HBITMAP
    }

    // =======================================================================
    //  DPI_AWARENESS_CONTEXT (opaque handle, typed as nint)
    // =======================================================================

    // DPI_AWARENESS_CONTEXT is a HANDLE (pointer-sized) in the Windows SDK.
    // We keep it as nint to match the C convention.
    //   DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = ((HANDLE) -4)

    // =======================================================================
    //  Win32 enums
    // =======================================================================

    public enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }

    public enum MONITOR_DPI_TYPE
    {
        MDT_EFFECTIVE_DPI = 0,
        MDT_ANGULAR_DPI = 1,
        MDT_RAW_DPI = 2,
        MDT_DEFAULT = MDT_EFFECTIVE_DPI
    }

    // =======================================================================
    //  Win32 constants
    // =======================================================================

    public static class Win32
    {
        // DPI_AWARENESS_CONTEXT values
        public static readonly nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = (nint)(-4);

        // User default DPI
        public const int USER_DEFAULT_SCREEN_DPI = 96;

        // ---------- Window styles (WS_*) ----------
        public const uint WS_OVERLAPPED    = 0x00000000;
        public const uint WS_POPUP         = 0x80000000;
        public const uint WS_CHILD         = 0x40000000;
        public const uint WS_MINIMIZE      = 0x20000000;
        public const uint WS_VISIBLE       = 0x10000000;
        public const uint WS_DISABLED      = 0x08000000;
        public const uint WS_CLIPSIBLINGS  = 0x04000000;
        public const uint WS_CLIPCHILDREN  = 0x02000000;
        public const uint WS_MAXIMIZE      = 0x01000000;
        public const uint WS_CAPTION       = 0x00C00000;
        public const uint WS_BORDER        = 0x00800000;
        public const uint WS_DLGFRAME      = 0x00400000;
        public const uint WS_VSCROLL       = 0x00200000;
        public const uint WS_HSCROLL       = 0x00100000;
        public const uint WS_SYSMENU       = 0x00080000;
        public const uint WS_THICKFRAME    = 0x00040000;
        public const uint WS_GROUP         = 0x00020000;
        public const uint WS_TABSTOP       = 0x00010000;
        public const uint WS_MINIMIZEBOX   = 0x00020000;
        public const uint WS_MAXIMIZEBOX   = 0x00010000;
        public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU |
                                                 WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const uint WS_POPUPWINDOW   = WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const uint WS_SIZEBOX       = WS_THICKFRAME;

        // ---------- Extended window styles (WS_EX_*) ----------
        public const uint WS_EX_DLGMODALFRAME  = 0x00000001;
        public const uint WS_EX_TOPMOST        = 0x00000008;
        public const uint WS_EX_ACCEPTFILES    = 0x00000010;
        public const uint WS_EX_TRANSPARENT    = 0x00000020;
        public const uint WS_EX_APPWINDOW      = 0x00040000;
        public const uint WS_EX_LAYERED        = 0x00080000;
        public const uint WS_EX_WINDOWEDGE     = 0x00000100;
        public const uint WS_EX_CLIENTEDGE     = 0x00000200;
        public const uint WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;

        // ---------- Window messages (WM_*) ----------
        public const uint WM_NULL              = 0x0000;
        public const uint WM_CREATE            = 0x0001;
        public const uint WM_DESTROY           = 0x0002;
        public const uint WM_MOVE              = 0x0003;
        public const uint WM_SIZE              = 0x0005;
        public const uint WM_ACTIVATE          = 0x0006;
        public const uint WM_SETFOCUS          = 0x0007;
        public const uint WM_KILLFOCUS         = 0x0008;
        public const uint WM_ENABLE            = 0x000A;
        public const uint WM_SETTEXT           = 0x000C;
        public const uint WM_GETTEXT           = 0x000D;
        public const uint WM_PAINT             = 0x000F;
        public const uint WM_CLOSE             = 0x0010;
        public const uint WM_QUIT              = 0x0012;
        public const uint WM_ERASEBKGND        = 0x0014;
        public const uint WM_SHOWWINDOW        = 0x0018;
        public const uint WM_ACTIVATEAPP       = 0x001C;
        public const uint WM_CANCELMODE        = 0x001F;
        public const uint WM_SETCURSOR         = 0x0020;
        public const uint WM_MOUSEACTIVATE     = 0x0021;
        public const uint WM_GETMINMAXINFO     = 0x0024;
        public const uint WM_WINDOWPOSCHANGING = 0x0046;
        public const uint WM_WINDOWPOSCHANGED  = 0x0047;
        public const uint WM_COPYGLOBALDATA    = 0x0049;
        public const uint WM_COPYDATA          = 0x004A;
        public const uint WM_INPUT             = 0x00FF;
        public const uint WM_KEYDOWN           = 0x0100;
        public const uint WM_KEYUP             = 0x0101;
        public const uint WM_CHAR              = 0x0102;
        public const uint WM_DEADCHAR          = 0x0103;
        public const uint WM_SYSKEYDOWN        = 0x0104;
        public const uint WM_SYSKEYUP          = 0x0105;
        public const uint WM_SYSCHAR           = 0x0106;
        public const uint WM_UNICHAR           = 0x0109;
        public const uint WM_COMMAND           = 0x0111;
        public const uint WM_SYSCOMMAND        = 0x0112;
        public const uint WM_TIMER             = 0x0113;
        public const uint WM_MOUSEMOVE         = 0x0200;
        public const uint WM_LBUTTONDOWN       = 0x0201;
        public const uint WM_LBUTTONUP         = 0x0202;
        public const uint WM_LBUTTONDBLCLK     = 0x0203;
        public const uint WM_RBUTTONDOWN       = 0x0204;
        public const uint WM_RBUTTONUP         = 0x0205;
        public const uint WM_RBUTTONDBLCLK     = 0x0206;
        public const uint WM_MBUTTONDOWN       = 0x0207;
        public const uint WM_MBUTTONUP         = 0x0208;
        public const uint WM_MBUTTONDBLCLK     = 0x0209;
        public const uint WM_MOUSEWHEEL        = 0x020A;
        public const uint WM_MOUSEHWHEEL       = 0x020E;
        public const uint WM_XBUTTONDOWN       = 0x020B;
        public const uint WM_XBUTTONUP         = 0x020C;
        public const uint WM_MOUSELEAVE        = 0x02A3;
        public const uint WM_CAPTURECHANGED    = 0x0215;
        public const uint WM_ENTERSIZEMOVE     = 0x0231;
        public const uint WM_EXITSIZEMOVE      = 0x0232;
        public const uint WM_DROPFILES         = 0x0233;
        public const uint WM_DISPLAYCHANGE     = 0x007E;
        public const uint WM_DEVICECHANGE      = 0x0219;
        public const uint WM_NCCREATE          = 0x0081;
        public const uint WM_NCDESTROY         = 0x0082;
        public const uint WM_NCCALCSIZE        = 0x0083;
        public const uint WM_NCHITTEST         = 0x0084;
        public const uint WM_NCPAINT           = 0x0085;
        public const uint WM_NCACTIVATE        = 0x0086;
        public const uint WM_NCMOUSEMOVE       = 0x00A0;
        public const uint WM_NCLBUTTONDOWN     = 0x00A1;
        public const uint WM_NCLBUTTONUP       = 0x00A2;
        public const uint WM_DWMCOMPOSITIONCHANGED = 0x031E;
        public const uint WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
        public const uint WM_DPICHANGED        = 0x02E0;
        public const uint WM_GETDPISCALEDSIZE  = 0x02E4;
        public const uint WM_THEMECHANGED      = 0x031A;
        public const uint WM_SETTINGCHANGE     = 0x001A;

        // ---------- Show window commands (SW_*) ----------
        public const int SW_HIDE            = 0;
        public const int SW_SHOWNORMAL      = 1;
        public const int SW_NORMAL          = 1;
        public const int SW_SHOWMINIMIZED   = 2;
        public const int SW_SHOWMAXIMIZED   = 3;
        public const int SW_MAXIMIZE        = 3;
        public const int SW_SHOWNOACTIVATE  = 4;
        public const int SW_SHOW            = 5;
        public const int SW_MINIMIZE        = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA          = 8;
        public const int SW_RESTORE         = 9;
        public const int SW_SHOWDEFAULT     = 10;

        // ---------- SetWindowLong / GetWindowLong indices (GWL_*) ----------
        public const int GWL_EXSTYLE  = -20;
        public const int GWL_STYLE    = -16;
        public const int GWLP_WNDPROC = -4;
        public const int GWLP_USERDATA = -21;

        // ---------- Class styles (CS_*) ----------
        public const uint CS_VREDRAW   = 0x0001;
        public const uint CS_HREDRAW   = 0x0002;
        public const uint CS_OWNDC     = 0x0020;

        // ---------- PeekMessage flags (PM_*) ----------
        public const uint PM_REMOVE    = 0x0001;
        public const uint PM_NOREMOVE  = 0x0000;

        // ---------- System metrics (SM_*) ----------
        public const int SM_CXSCREEN     = 0;
        public const int SM_CYSCREEN     = 1;
        public const int SM_CXVSCROLL    = 2;
        public const int SM_CYHSCROLL    = 3;
        public const int SM_CYCAPTION    = 4;
        public const int SM_CXBORDER     = 5;
        public const int SM_CYBORDER     = 6;
        public const int SM_CXDLGFRAME   = 7;
        public const int SM_CYDLGFRAME   = 8;
        public const int SM_CXFRAME      = 32;
        public const int SM_CYFRAME      = 33;
        public const int SM_CXMINTRACK   = 34;
        public const int SM_CYMINTRACK   = 35;
        public const int SM_CXMAXTRACK   = 59;
        public const int SM_CYMAXTRACK   = 60;
        public const int SM_CXICON       = 11;
        public const int SM_CYICON       = 12;
        public const int SM_CXSMICON     = 49;
        public const int SM_CYSMICON     = 50;
        public const int SM_CXCURSOR     = 13;
        public const int SM_CYCURSOR     = 14;
        public const int SM_SWAPBUTTON   = 23;
        public const int SM_CMONITORS    = 80;
        public const int SM_MOUSEWHEELPRESENT = 75;

        // ---------- SystemParametersInfo (SPI_*) ----------
        public const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        public const uint SPI_GETHIGHCONTRAST          = 0x0042;
        public const uint SPI_GETMOUSETRAILS           = 0x005E;

        // ---------- Monitor flags (MONITOR_*) ----------
        public const uint MONITOR_DEFAULTTONULL    = 0x00000000;
        public const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        public const uint MONITORINFOF_PRIMARY     = 0x00000001;

        // ---------- Display settings (DISP_*, EDS_*, CDS_*) ----------
        public const uint DISP_CHANGE_SUCCESSFUL = 0;
        public const uint DISP_CHANGE_RESTART    = 1;
        public const uint DISP_CHANGE_FAILED     = unchecked((uint)-1);
        public const uint DISP_CHANGE_BADMODE    = unchecked((uint)-2);
        public const uint EDS_ROTATEDMODE        = 0x00000004;
        public const uint ENUM_CURRENT_SETTINGS  = unchecked((uint)-1);
        public const uint CDS_FULLSCREEN         = 0x00000004;
        public const uint CDS_TEST               = 0x00000002;

        // ---------- PixelFormat flags (PFD_*) ----------
        public const uint PFD_DRAW_TO_WINDOW  = 0x00000004;
        public const uint PFD_SUPPORT_OPENGL  = 0x00000020;
        public const uint PFD_DOUBLEBUFFER    = 0x00000001;
        public const uint PFD_STEREO          = 0x00000002;
        public const uint PFD_TYPE_RGBA       = 0;
        public const uint PFD_GENERIC_ACCELERATED = 0x00001000;
        public const uint PFD_GENERIC_FORMAT  = 0x00000040;

        // ---------- SetWindowPos flags (SWP_*) ----------
        public const uint SWP_NOSIZE        = 0x0001;
        public const uint SWP_NOMOVE        = 0x0002;
        public const uint SWP_NOZORDER      = 0x0004;
        public const uint SWP_NOREDRAW      = 0x0008;
        public const uint SWP_NOACTIVATE    = 0x0010;
        public const uint SWP_FRAMECHANGED  = 0x0020;
        public const uint SWP_SHOWWINDOW    = 0x0040;
        public const uint SWP_HIDEWINDOW    = 0x0080;
        public const uint SWP_NOCOPYBITS    = 0x0100;
        public const uint SWP_NOOWNERZORDER = 0x0200;
        public const uint SWP_NOSENDCHANGING = 0x0400;

        // ---------- HWND_* special window handles ----------
        public static readonly nint HWND_TOP       = (nint)0;
        public static readonly nint HWND_BOTTOM    = (nint)1;
        public static readonly nint HWND_TOPMOST   = (nint)(-1);
        public static readonly nint HWND_NOTOPMOST = (nint)(-2);

        // ---------- Window activation (WA_*) ----------
        public const int WA_INACTIVE    = 0;
        public const int WA_ACTIVE      = 1;
        public const int WA_CLICKACTIVE = 2;

        // ---------- MapVirtualKey types (MAPVK_*) ----------
        public const uint MAPVK_VK_TO_VSC    = 0;
        public const uint MAPVK_VSC_TO_VK    = 1;
        public const uint MAPVK_VK_TO_CHAR   = 2;
        public const uint MAPVK_VSC_TO_VK_EX = 3;
        public const uint MAPVK_VK_TO_VSC_EX = 4;

        // ---------- SIZE_* (wParam of WM_SIZE) ----------
        public const int SIZE_RESTORED  = 0;
        public const int SIZE_MINIMIZED = 1;
        public const int SIZE_MAXIMIZED = 2;

        // ---------- System commands (SC_*) ----------
        public const uint SC_KEYMENU = 0xF100;
        public const uint SC_SCREENSAVE = 0xF140;
        public const uint SC_MONITORPOWER = 0xF170;

        // ---------- Cursor / Image / Icon constants ----------
        public const uint IMAGE_BITMAP  = 0;
        public const uint IMAGE_ICON    = 1;
        public const uint IMAGE_CURSOR  = 2;
        public const uint LR_DEFAULTSIZE = 0x00000040;
        public const uint LR_SHARED      = 0x00008000;

        // Standard cursor IDs (IDC_*)
        public static readonly nint IDC_ARROW       = (nint)32512;
        public static readonly nint IDC_IBEAM       = (nint)32513;
        public static readonly nint IDC_WAIT        = (nint)32514;
        public static readonly nint IDC_CROSS       = (nint)32515;
        public static readonly nint IDC_UPARROW     = (nint)32516;
        public static readonly nint IDC_SIZE        = (nint)32640;
        public static readonly nint IDC_ICON        = (nint)32641;
        public static readonly nint IDC_SIZENWSE    = (nint)32642;
        public static readonly nint IDC_SIZENESW    = (nint)32643;
        public static readonly nint IDC_SIZEWE      = (nint)32644;
        public static readonly nint IDC_SIZENS      = (nint)32645;
        public static readonly nint IDC_SIZEALL     = (nint)32646;
        public static readonly nint IDC_NO          = (nint)32648;
        public static readonly nint IDC_HAND        = (nint)32649;
        public static readonly nint IDC_APPSTARTING = (nint)32650;

        // OEM cursor resources (require #define OEMRESOURCE)
        public static readonly nint OCR_NORMAL      = (nint)32512;
        public static readonly nint OCR_IBEAM       = (nint)32513;
        public static readonly nint OCR_WAIT        = (nint)32514;
        public static readonly nint OCR_CROSS       = (nint)32515;
        public static readonly nint OCR_UP          = (nint)32516;
        public static readonly nint OCR_SIZENWSE    = (nint)32642;
        public static readonly nint OCR_SIZENESW    = (nint)32643;
        public static readonly nint OCR_SIZEWE      = (nint)32644;
        public static readonly nint OCR_SIZENS      = (nint)32645;
        public static readonly nint OCR_SIZEALL     = (nint)32646;
        public static readonly nint OCR_NO          = (nint)32648;
        public static readonly nint OCR_HAND        = (nint)32649;

        // Icon sizes
        public const int ICON_SMALL  = 0;
        public const int ICON_BIG    = 1;
        public const int ICON_SMALL2 = 2;

        // ---------- Hit test results (HT*) ----------
        public const int HTCLIENT = 1;

        // ---------- TrackMouseEvent flags (TME_*) ----------
        public const uint TME_LEAVE = 0x00000002;

        // ---------- Clipboard formats (CF_*) ----------
        public const uint CF_TEXT          = 1;
        public const uint CF_BITMAP       = 2;
        public const uint CF_UNICODETEXT  = 13;

        // ---------- GlobalAlloc flags (GMEM_*) ----------
        public const uint GMEM_MOVEABLE = 0x0002;

        // ---------- Mouse key flags (MK_*) ----------
        public const int MK_LBUTTON  = 0x0001;
        public const int MK_RBUTTON  = 0x0002;
        public const int MK_SHIFT    = 0x0004;
        public const int MK_CONTROL  = 0x0008;
        public const int MK_MBUTTON  = 0x0010;
        public const int MK_XBUTTON1 = 0x0020;
        public const int MK_XBUTTON2 = 0x0040;

        // ---------- XBUTTON indices ----------
        public const int XBUTTON1 = 0x0001;
        public const int XBUTTON2 = 0x0002;

        // ---------- Virtual key codes (VK_*) ----------
        public const int VK_LBUTTON  = 0x01;
        public const int VK_RBUTTON  = 0x02;
        public const int VK_CANCEL   = 0x03;
        public const int VK_MBUTTON  = 0x04;
        public const int VK_XBUTTON1 = 0x05;
        public const int VK_XBUTTON2 = 0x06;
        public const int VK_BACK     = 0x08;
        public const int VK_TAB      = 0x09;
        public const int VK_CLEAR    = 0x0C;
        public const int VK_RETURN   = 0x0D;
        public const int VK_SHIFT    = 0x10;
        public const int VK_CONTROL  = 0x11;
        public const int VK_MENU     = 0x12;
        public const int VK_PAUSE    = 0x13;
        public const int VK_CAPITAL  = 0x14;
        public const int VK_ESCAPE   = 0x1B;
        public const int VK_SPACE    = 0x20;
        public const int VK_PRIOR    = 0x21;
        public const int VK_NEXT     = 0x22;
        public const int VK_END      = 0x23;
        public const int VK_HOME     = 0x24;
        public const int VK_LEFT     = 0x25;
        public const int VK_UP       = 0x26;
        public const int VK_RIGHT    = 0x27;
        public const int VK_DOWN     = 0x28;
        public const int VK_SELECT   = 0x29;
        public const int VK_PRINT    = 0x2A;
        public const int VK_EXECUTE  = 0x2B;
        public const int VK_SNAPSHOT = 0x2C;
        public const int VK_INSERT   = 0x2D;
        public const int VK_DELETE   = 0x2E;
        public const int VK_HELP     = 0x2F;
        // VK_0 through VK_9: 0x30..0x39
        // VK_A through VK_Z: 0x41..0x5A
        public const int VK_LWIN     = 0x5B;
        public const int VK_RWIN     = 0x5C;
        public const int VK_APPS     = 0x5D;
        public const int VK_SLEEP    = 0x5F;
        public const int VK_NUMPAD0  = 0x60;
        public const int VK_NUMPAD1  = 0x61;
        public const int VK_NUMPAD2  = 0x62;
        public const int VK_NUMPAD3  = 0x63;
        public const int VK_NUMPAD4  = 0x64;
        public const int VK_NUMPAD5  = 0x65;
        public const int VK_NUMPAD6  = 0x66;
        public const int VK_NUMPAD7  = 0x67;
        public const int VK_NUMPAD8  = 0x68;
        public const int VK_NUMPAD9  = 0x69;
        public const int VK_MULTIPLY = 0x6A;
        public const int VK_ADD      = 0x6B;
        public const int VK_SEPARATOR = 0x6C;
        public const int VK_SUBTRACT = 0x6D;
        public const int VK_DECIMAL  = 0x6E;
        public const int VK_DIVIDE   = 0x6F;
        public const int VK_F1       = 0x70;
        public const int VK_F2       = 0x71;
        public const int VK_F3       = 0x72;
        public const int VK_F4       = 0x73;
        public const int VK_F5       = 0x74;
        public const int VK_F6       = 0x75;
        public const int VK_F7       = 0x76;
        public const int VK_F8       = 0x77;
        public const int VK_F9       = 0x78;
        public const int VK_F10      = 0x79;
        public const int VK_F11      = 0x7A;
        public const int VK_F12      = 0x7B;
        public const int VK_F13      = 0x7C;
        public const int VK_F14      = 0x7D;
        public const int VK_F15      = 0x7E;
        public const int VK_F16      = 0x7F;
        public const int VK_F17      = 0x80;
        public const int VK_F18      = 0x81;
        public const int VK_F19      = 0x82;
        public const int VK_F20      = 0x83;
        public const int VK_F21      = 0x84;
        public const int VK_F22      = 0x85;
        public const int VK_F23      = 0x86;
        public const int VK_F24      = 0x87;
        public const int VK_NUMLOCK  = 0x90;
        public const int VK_SCROLL   = 0x91;
        public const int VK_LSHIFT   = 0xA0;
        public const int VK_RSHIFT   = 0xA1;
        public const int VK_LCONTROL = 0xA2;
        public const int VK_RCONTROL = 0xA3;
        public const int VK_LMENU    = 0xA4;
        public const int VK_RMENU    = 0xA5;
        public const int VK_PROCESSKEY = 0xE5;
        public const int VK_OEM_1    = 0xBA;
        public const int VK_OEM_PLUS = 0xBB;
        public const int VK_OEM_COMMA = 0xBC;
        public const int VK_OEM_MINUS = 0xBD;
        public const int VK_OEM_PERIOD = 0xBE;
        public const int VK_OEM_2    = 0xBF;
        public const int VK_OEM_3    = 0xC0;
        public const int VK_OEM_4    = 0xDB;
        public const int VK_OEM_5    = 0xDC;
        public const int VK_OEM_6    = 0xDD;
        public const int VK_OEM_7    = 0xDE;
        public const int VK_OEM_8    = 0xDF;
        public const int VK_OEM_102  = 0xE2;

        // ---------- GetAncestor flags (GA_*) ----------
        public const uint GA_PARENT    = 1;
        public const uint GA_ROOT      = 2;
        public const uint GA_ROOTOWNER = 3;

        // ---------- DEVMODE field flags (DM_*) ----------
        public const uint DM_BITSPERPEL  = 0x00040000;
        public const uint DM_PELSWIDTH   = 0x00080000;
        public const uint DM_PELSHEIGHT  = 0x00100000;
        public const uint DM_DISPLAYFREQUENCY = 0x00400000;
        public const uint DM_POSITION    = 0x00000020;

        // ---------- ChangeWindowMessageFilterEx action ----------
        public const uint MSGFLT_ALLOW = 1;
        public const uint MSGFLT_DISALLOW = 2;
        public const uint MSGFLT_RESET = 0;

        // ---------- Device notification ----------
        public const uint DBT_DEVICEARRIVAL       = 0x8000;
        public const uint DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const uint DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;

        // ---------- Raw input (RI_*) ----------
        public const uint RID_INPUT  = 0x10000003;
        public const uint RIM_TYPEMOUSE    = 0;
        public const uint RIM_TYPEKEYBOARD = 1;
        public const uint RIM_TYPEHID      = 2;
        public const uint MOUSE_MOVE_RELATIVE = 0x00;
        public const uint RIDEV_REMOVE    = 0x00000001;
        public const uint RIDEV_NOLEGACY  = 0x00000030;

        // ---------- HID usage page / usage ----------
        public const ushort HID_USAGE_PAGE_GENERIC   = 0x01;
        public const ushort HID_USAGE_GENERIC_MOUSE  = 0x02;

        // ---------- DWM constants ----------
        public const uint DWM_BB_ENABLE     = 0x00000001;
        public const uint DWM_BB_BLURREGION = 0x00000002;
        public const uint DWMWA_COLOR_DEFAULT = 0xFFFFFFFF;
        public const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const uint DWMWA_BORDER_COLOR = 34;
        public const uint DWMWA_CAPTION_COLOR = 35;

        // ---------- BI_BITFIELDS for BITMAPV5HEADER ----------
        public const uint BI_BITFIELDS = 3;

        // ---------- DIB section usage ----------
        public const uint DIB_RGB_COLORS = 0;

        // ---------- GDI object deletion ----------
        public const int OBJ_BITMAP = 7;

        // ---------- Version info typemask ----------
        public const uint VER_MINORVERSION     = 0x0000001;
        public const uint VER_MAJORVERSION     = 0x0000002;
        public const uint VER_BUILDNUMBER      = 0x0000004;
        public const uint VER_SERVICEPACKMAJOR = 0x0000020;
        public const uint VER_GREATER_EQUAL    = 3;

        // ---------- _WIN32_WINNT version constants ----------
        public const ushort _WIN32_WINNT_WIN8     = 0x0602;
        public const ushort _WIN32_WINNT_WINBLUE  = 0x0603;

        // ---------- WGL constants ----------
        public const int WGL_NUMBER_PIXEL_FORMATS_ARB  = 0x2000;
        public const int WGL_SUPPORT_OPENGL_ARB        = 0x2010;
        public const int WGL_DRAW_TO_WINDOW_ARB        = 0x2001;
        public const int WGL_PIXEL_TYPE_ARB            = 0x2013;
        public const int WGL_TYPE_RGBA_ARB             = 0x202b;
        public const int WGL_ACCELERATION_ARB          = 0x2003;
        public const int WGL_NO_ACCELERATION_ARB       = 0x2025;
        public const int WGL_RED_BITS_ARB              = 0x2015;
        public const int WGL_RED_SHIFT_ARB             = 0x2016;
        public const int WGL_GREEN_BITS_ARB            = 0x2017;
        public const int WGL_GREEN_SHIFT_ARB           = 0x2018;
        public const int WGL_BLUE_BITS_ARB             = 0x2019;
        public const int WGL_BLUE_SHIFT_ARB            = 0x201a;
        public const int WGL_ALPHA_BITS_ARB            = 0x201b;
        public const int WGL_ALPHA_SHIFT_ARB           = 0x201c;
        public const int WGL_ACCUM_BITS_ARB            = 0x201d;
        public const int WGL_ACCUM_RED_BITS_ARB        = 0x201e;
        public const int WGL_ACCUM_GREEN_BITS_ARB      = 0x201f;
        public const int WGL_ACCUM_BLUE_BITS_ARB       = 0x2020;
        public const int WGL_ACCUM_ALPHA_BITS_ARB      = 0x2021;
        public const int WGL_DEPTH_BITS_ARB            = 0x2022;
        public const int WGL_STENCIL_BITS_ARB          = 0x2023;
        public const int WGL_AUX_BUFFERS_ARB           = 0x2024;
        public const int WGL_STEREO_ARB                = 0x2012;
        public const int WGL_DOUBLE_BUFFER_ARB         = 0x2011;
        public const int WGL_SAMPLES_ARB               = 0x2042;
        public const int WGL_FRAMEBUFFER_SRGB_CAPABLE_ARB = 0x20a9;
        public const int WGL_CONTEXT_DEBUG_BIT_ARB     = 0x00000001;
        public const int WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB = 0x00000002;
        public const int WGL_CONTEXT_PROFILE_MASK_ARB  = 0x9126;
        public const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        public const int WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB = 0x00000002;
        public const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        public const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
        public const int WGL_CONTEXT_FLAGS_ARB         = 0x2094;
        public const int WGL_CONTEXT_ES2_PROFILE_BIT_EXT = 0x00000004;
        public const int WGL_CONTEXT_ROBUST_ACCESS_BIT_ARB = 0x00000004;
        public const int WGL_LOSE_CONTEXT_ON_RESET_ARB = 0x8252;
        public const int WGL_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB = 0x8256;
        public const int WGL_NO_RESET_NOTIFICATION_ARB = 0x8261;
        public const int WGL_CONTEXT_RELEASE_BEHAVIOR_ARB = 0x2097;
        public const int WGL_CONTEXT_RELEASE_BEHAVIOR_NONE_ARB = 0;
        public const int WGL_CONTEXT_RELEASE_BEHAVIOR_FLUSH_ARB = 0x2098;
        public const int WGL_CONTEXT_OPENGL_NO_ERROR_ARB = 0x31b3;
        public const int WGL_COLORSPACE_EXT            = 0x309d;
        public const int WGL_COLORSPACE_SRGB_EXT       = 0x3089;

        public const int ERROR_INVALID_VERSION_ARB     = 0x2095;
        public const int ERROR_INVALID_PROFILE_ARB     = 0x2096;
        public const int ERROR_INCOMPATIBLE_DEVICE_CONTEXTS_ARB = 0x2054;

        // ---------- XInput constants ----------
        public const uint XINPUT_CAPS_WIRELESS         = 0x0002;
        public const uint XINPUT_DEVSUBTYPE_WHEEL      = 0x02;
        public const uint XINPUT_DEVSUBTYPE_ARCADE_STICK = 0x03;
        public const uint XINPUT_DEVSUBTYPE_FLIGHT_STICK = 0x04;
        public const uint XINPUT_DEVSUBTYPE_DANCE_PAD  = 0x05;
        public const uint XINPUT_DEVSUBTYPE_GUITAR     = 0x06;
        public const uint XINPUT_DEVSUBTYPE_DRUM_KIT   = 0x08;
        public const uint XINPUT_DEVSUBTYPE_ARCADE_PAD = 0x13;
        public const int  XUSER_MAX_COUNT              = 4;

        // ---------- DirectInput constants ----------
        public const uint DIDFT_OPTIONAL               = 0x80000000;
        public const uint DIRECTINPUT_VERSION           = 0x0800;

        // ---------- UNICODE_NOCHAR for WM_UNICHAR ----------
        public const uint UNICODE_NOCHAR = 0xFFFF;

        // ---------- Key state mask ----------
        public const int KF_EXTENDED = 0x0100;

        // ---------- GET_X_LPARAM / GET_Y_LPARAM helpers ----------
        public static int GET_X_LPARAM(nint lp) => unchecked((short)(ushort)(lp & 0xFFFF));
        public static int GET_Y_LPARAM(nint lp) => unchecked((short)(ushort)((lp >> 16) & 0xFFFF));
        public static int LOWORD(nuint w) => unchecked((short)(ushort)((ulong)w & 0xFFFF));
        public static int HIWORD(nuint w) => unchecked((short)(ushort)(((ulong)w >> 16) & 0xFFFF));
        public static int LOWORD(nint l) => unchecked((short)(ushort)(l & 0xFFFF));
        public static int HIWORD(nint l) => unchecked((short)(ushort)((l >> 16) & 0xFFFF));
        public static byte LOBYTE(ushort w) => (byte)(w & 0xFF);
        public static byte HIBYTE(ushort w) => (byte)((w >> 8) & 0xFF);

        // ---------- MAKEINTRESOURCE ----------
        public static nint MAKEINTRESOURCE(int id) => (nint)(ushort)id;

        // ---------- GWLP helpers (32/64 bit) ----------
        // On 64-bit, SetWindowLongPtr is the real symbol; on 32-bit it maps to SetWindowLong.
        // We always use the *Ptr variants.

        // ---------- MultiByteToWideChar / WideCharToMultiByte code pages ----------
        public const uint CP_UTF8 = 65001;
    }

    // =======================================================================
    //  _GLFWwindowWin32 -> GlfwWindowWin32
    // =======================================================================

    public class GlfwWindowWin32
    {
        public nint handle;              // HWND
        public nint bigIcon;             // HICON
        public nint smallIcon;           // HICON

        public bool cursorTracked;
        public bool frameAction;
        public bool iconified;
        public bool maximized;
        // Whether to enable framebuffer transparency on DWM
        public bool transparent;
        public bool scaleToMonitor;
        public bool keymenu;
        public bool showDefault;

        // Cached size used to filter out duplicate events
        public int width, height;

        // The last received cursor position, regardless of source
        public int lastCursorPosX, lastCursorPosY;
        // The last received high surrogate when decoding pairs of UTF-16 messages
        public char highSurrogate;
    }

    // =======================================================================
    //  _GLFWmonitorWin32 -> GlfwMonitorWin32
    // =======================================================================

    public class GlfwMonitorWin32
    {
        public nint handle;              // HMONITOR
        // This size matches the static size of DISPLAY_DEVICE.DeviceName
        public string adapterName = string.Empty;  // WCHAR[32]
        public string displayName = string.Empty;   // WCHAR[32]
        public string publicAdapterName = string.Empty; // char[32]
        public string publicDisplayName = string.Empty; // char[32]
        public bool modesPruned;
        public bool modeChanged;
    }

    // =======================================================================
    //  _GLFWlibraryWin32 -> GlfwLibraryWin32
    // =======================================================================

    public unsafe class GlfwLibraryWin32
    {
        public nint instance;            // HINSTANCE
        public nint helperWindowHandle;  // HWND
        public ushort helperWindowClass; // ATOM
        public ushort mainWindowClass;   // ATOM
        public nint deviceNotificationHandle; // HDEVNOTIFY
        public int acquiredMonitorCount;
        public string? clipboardString;
        public short[] keycodes = new short[512];
        public short[] scancodes = new short[GLFW.GLFW_KEY_LAST + 1];
        // C++ uses char keynames[GLFW_KEY_LAST+1][5] -- in C# we use string?[]
        public string?[] keynames = new string?[GLFW.GLFW_KEY_LAST + 1];
        // Where to place the cursor when re-enabled
        public double restoreCursorPosX, restoreCursorPosY;
        // The window whose disabled cursor mode is active
        public GlfwWindow? disabledCursorWindow;
        // The window the cursor is captured in
        public GlfwWindow? capturedCursorWindow;
        public byte[]? rawInput;
        public int rawInputSize;
        public uint mouseTrailSize;
        // The cursor handle to use to hide the cursor (NULL or a transparent cursor)
        public nint blankCursor;         // HCURSOR

        // ------------------------------------------------------------------
        //  dinput8 -- DirectInput8 (optional)
        // ------------------------------------------------------------------
        public Dinput8Functions dinput8 = new();

        public class Dinput8Functions
        {
            public nint instance;        // HINSTANCE (dinput8.dll)
            public nint api;             // IDirectInput8W*
            // PFN_DirectInput8Create: HRESULT(HINSTANCE, DWORD, REFIID, LPVOID*, LPUNKNOWN)
            public nint Create;          // raw function pointer (varargs-like COM, called via managed wrapper)
        }

        // ------------------------------------------------------------------
        //  xinput -- XInput (optional)
        // ------------------------------------------------------------------
        public XinputFunctions xinput = new();

        public unsafe class XinputFunctions
        {
            public nint instance;        // HINSTANCE (xinput.dll)
            // PFN_XInputGetCapabilities: DWORD(DWORD, DWORD, XINPUT_CAPABILITIES*)
            public delegate* unmanaged[Stdcall]<uint, uint, nint, uint> GetCapabilities;
            // PFN_XInputGetState: DWORD(DWORD, XINPUT_STATE*)
            public delegate* unmanaged[Stdcall]<uint, nint, uint> GetState;
        }

        // ------------------------------------------------------------------
        //  user32 -- optional functions loaded at runtime
        // ------------------------------------------------------------------
        public User32Functions user32 = new();

        public unsafe class User32Functions
        {
            public nint instance;        // HINSTANCE (user32.dll -- always loaded)
            // PFN_EnableNonClientDpiScaling: BOOL(HWND)
            public delegate* unmanaged[Stdcall]<nint, int> EnableNonClientDpiScaling_;
            // PFN_SetProcessDpiAwarenessContext: BOOL(HANDLE)
            public delegate* unmanaged[Stdcall]<nint, int> SetProcessDpiAwarenessContext_;
            // PFN_GetDpiForWindow: UINT(HWND)
            public delegate* unmanaged[Stdcall]<nint, uint> GetDpiForWindow_;
            // PFN_AdjustWindowRectExForDpi: BOOL(LPRECT, DWORD, BOOL, DWORD, UINT)
            public delegate* unmanaged[Stdcall]<RECT*, uint, int, uint, uint, int> AdjustWindowRectExForDpi_;
            // PFN_GetSystemMetricsForDpi: int(int, UINT)
            public delegate* unmanaged[Stdcall]<int, uint, int> GetSystemMetricsForDpi_;
        }

        // ------------------------------------------------------------------
        //  dwmapi -- Desktop Window Manager (optional)
        // ------------------------------------------------------------------
        public DwmapiFunctions dwmapi = new();

        public unsafe class DwmapiFunctions
        {
            public nint instance;        // HINSTANCE (dwmapi.dll)
            // PFN_DwmIsCompositionEnabled: HRESULT(BOOL*)
            public delegate* unmanaged[Stdcall]<int*, int> IsCompositionEnabled;
            // PFN_DwmFlush: HRESULT(VOID)
            public delegate* unmanaged[Stdcall]<int> Flush;
            // PFN_DwmEnableBlurBehindWindow: HRESULT(HWND, const DWM_BLURBEHIND*)
            public delegate* unmanaged[Stdcall]<nint, nint, int> EnableBlurBehindWindow;
            // PFN_DwmGetColorizationColor: HRESULT(DWORD*, BOOL*)
            public delegate* unmanaged[Stdcall]<uint*, int*, int> GetColorizationColor;
            // DwmSetWindowAttribute: HRESULT(HWND, DWORD, LPCVOID, DWORD)
            public delegate* unmanaged[Stdcall]<nint, uint, void*, uint, int> SetWindowAttribute;
        }

        // ------------------------------------------------------------------
        //  shcore -- Shell scaling helpers (optional, Win8.1+)
        // ------------------------------------------------------------------
        public ShcoreFunctions shcore = new();

        public unsafe class ShcoreFunctions
        {
            public nint instance;        // HINSTANCE (shcore.dll)
            // PFN_SetProcessDpiAwareness: HRESULT(PROCESS_DPI_AWARENESS)
            public delegate* unmanaged[Stdcall]<int, int> SetProcessDpiAwareness_;
            // PFN_GetDpiForMonitor: HRESULT(HMONITOR, MONITOR_DPI_TYPE, UINT*, UINT*)
            public delegate* unmanaged[Stdcall]<nint, int, uint*, uint*, int> GetDpiForMonitor_;
        }

        // ------------------------------------------------------------------
        //  ntdll -- NT version info (optional)
        // ------------------------------------------------------------------
        public NtdllFunctions ntdll = new();

        public unsafe class NtdllFunctions
        {
            public nint instance;        // HINSTANCE (ntdll.dll)
            // PFN_RtlVerifyVersionInfo: LONG(OSVERSIONINFOEXW*, ULONG, ULONGLONG)
            public delegate* unmanaged[Stdcall]<nint, uint, ulong, int> RtlVerifyVersionInfo_;
        }
    }

    // =======================================================================
    //  _GLFWcursorWin32 -> GlfwCursorWin32
    // =======================================================================

    public class GlfwCursorWin32
    {
        public nint handle;              // HCURSOR
    }

    // GlfwContextWGL and GlfwLibraryWGL are defined in wgl/wgl_native.cs
}

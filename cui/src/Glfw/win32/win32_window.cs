// Ported from glfw/src/win32_window.c (GLFW 3.6)
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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Glfw.GLFW;

namespace Glfw;

public static unsafe partial class Glfw
{
    // Scancode recovery must retain KF_EXTENDED when Windows omits its low byte.
    internal static int MergeWin32Scancode(int scancode, uint mapped) => scancode | (int)mapped;
    // =======================================================================
    //  Win32 functions accessed via Win32Native function pointers
    //  (see win32_native.cs for loading code)
    // =======================================================================

    // =======================================================================
    //  Win32 interop structs (window-specific)
    // =======================================================================

    // MONITORINFOEXW, TRACKMOUSEEVENT, RAWINPUTDEVICE, RAWINPUTHEADER, RAWMOUSE,
    // RAWINPUT, WINDOWPLACEMENT, ICONINFO are defined in win32_platform.cs.
    // MONITORINFOEXW uses MONITORINFOEXW (the extended version).

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPV5HEADER
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
        public unsafe fixed int bV5Endpoints[9]; // CIEXYZTRIPLE (3 * CIEXYZ, each 3 ints)
        public uint bV5GammaRed;
        public uint bV5GammaGreen;
        public uint bV5GammaBlue;
        public uint bV5Intent;
        public uint bV5ProfileData;
        public uint bV5ProfileSize;
        public uint bV5Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CREATESTRUCTW
    {
        public nint lpCreateParams;
        public nint hInstance;
        public nint hMenu;
        public nint hwndParent;
        public int cy;
        public int cx;
        public int y;
        public int x;
        public int style;
        public nint lpszName;   // LPCWSTR
        public nint lpszClass;  // LPCWSTR
        public uint dwExStyle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWM_BLURBEHIND
    {
        public uint dwFlags;
        public int fEnable;
        public nint hRgnBlur;
        public int fTransitionOnMaximized;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFOW
    {
        public uint cb;
        public nint lpReserved;
        public nint lpDesktop;
        public nint lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public ushort wShowWindow;
        public ushort cbReserved2;
        public nint lpReserved2;
        public nint hStdInput;
        public nint hStdOutput;
        public nint hStdError;
    }

    // =======================================================================
    //  Win32 constants (window-specific)
    // =======================================================================

    private const int GWL_STYLE   = -16;
    private const int GWL_EXSTYLE = -20;
    private const int GCLP_HICON  = -14;
    private const int GCLP_HICONSM = -34;

    private const uint WS_OVERLAPPEDWINDOW_W = 0x00CF0000;
    private const uint WS_POPUP_W      = 0x80000000;
    private const uint WS_CLIPSIBLINGS_W = 0x04000000;
    private const uint WS_CLIPCHILDREN_W = 0x02000000;
    private const uint WS_SYSMENU      = 0x00080000;
    private const uint WS_MINIMIZEBOX  = 0x00020000;
    private const uint WS_MAXIMIZEBOX  = 0x00010000;
    private const uint WS_CAPTION      = 0x00C00000;
    private const uint WS_THICKFRAME   = 0x00040000;
    private const uint WS_MAXIMIZE     = 0x01000000;

    private const uint WS_EX_APPWINDOW   = 0x00040000;
    private const uint WS_EX_TOPMOST     = 0x00000008;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint WS_EX_LAYERED     = 0x00080000;

    private static readonly nint HWND_TOP       = 0;
    private static readonly nint HWND_TOPMOST   = -1;
    private static readonly nint HWND_NOTOPMOST = -2;

    private const uint SWP_NOACTIVATE   = 0x0010;
    private const uint SWP_NOZORDER     = 0x0004;
    private const uint SWP_NOSIZE       = 0x0001;
    private const uint SWP_NOMOVE       = 0x0002;
    private const uint SWP_NOCOPYBITS   = 0x0100;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_SHOWWINDOW   = 0x0040;
    private const uint SWP_NOOWNERZORDER = 0x0200;

    private const int SW_HIDE     = 0;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE  = 9;
    private const int SW_MAXIMIZE = 3;
    private const int SW_SHOWNA   = 8;
    // private const int SW_SHOWDEFAULT = 10; // in win32_init.cs

    private const uint WM_NULL           = 0x0000;
    private const uint WM_SETFOCUS       = 0x0007;
    private const uint WM_KILLFOCUS      = 0x0008;
    private const uint WM_CLOSE          = 0x0010;
    private const uint WM_MOVE           = 0x0003;
    private const uint WM_SIZE           = 0x0005;
    private const uint WM_PAINT          = 0x000F;
    private const uint WM_ERASEBKGND     = 0x0014;
    private const uint WM_NCCREATE       = 0x0081;
    private const uint WM_NCACTIVATE     = 0x0086;
    private const uint WM_NCPAINT        = 0x0085;
    private const uint WM_SYSCOMMAND     = 0x0112;
    private const uint WM_MOUSEACTIVATE  = 0x0021;
    private const uint WM_CAPTURECHANGED = 0x0215;
    private const uint WM_CHAR           = 0x0102;
    private const uint WM_SYSCHAR        = 0x0106;
    private const uint WM_UNICHAR        = 0x0109;
    private const uint WM_KEYDOWN        = 0x0100;
    private const uint WM_SYSKEYDOWN     = 0x0104;
    private const uint WM_KEYUP          = 0x0101;
    private const uint WM_SYSKEYUP       = 0x0105;
    private const uint WM_LBUTTONDOWN    = 0x0201;
    private const uint WM_RBUTTONDOWN    = 0x0204;
    private const uint WM_MBUTTONDOWN    = 0x0207;
    private const uint WM_XBUTTONDOWN    = 0x020B;
    private const uint WM_LBUTTONUP      = 0x0202;
    private const uint WM_RBUTTONUP      = 0x0205;
    private const uint WM_MBUTTONUP      = 0x0208;
    private const uint WM_XBUTTONUP      = 0x020C;
    private const uint WM_MOUSEMOVE      = 0x0200;
    private const uint WM_INPUT          = 0x00FF;
    private const uint WM_MOUSELEAVE     = 0x02A3;
    private const uint WM_MOUSEWHEEL     = 0x020A;
    private const uint WM_MOUSEHWHEEL    = 0x020E;
    private const uint WM_ENTERSIZEMOVE  = 0x0231;
    private const uint WM_EXITSIZEMOVE   = 0x0232;
    private const uint WM_ENTERMENULOOP  = 0x0211;
    private const uint WM_EXITMENULOOP   = 0x0212;
    private const uint WM_SIZING         = 0x0214;
    private const uint WM_GETMINMAXINFO  = 0x0024;
    private const uint WM_SETCURSOR      = 0x0020;
    private const uint WM_DROPFILES      = 0x0233;
    private const uint WM_INPUTLANGCHANGE = 0x0051;
    private const uint WM_SETICON        = 0x0080;
    private const uint WM_DWMCOMPOSITIONCHANGED  = 0x031E;
    private const uint WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
    private const uint WM_GETDPISCALEDSIZE_W = 0x02E4;
    private const uint WM_DPICHANGED_W   = 0x02E0;
    private const uint WM_COPYDATA       = 0x004A;
    private const uint WM_COPYGLOBALDATA_W = 0x0049;
    private const uint WM_QUIT           = 0x0012;

    private const nuint SC_SCREENSAVE  = 0xF140;
    private const nuint SC_MONITORPOWER = 0xF170;
    private const nuint SC_KEYMENU     = 0xF100;

    private const nuint SIZE_MINIMIZED = 1;
    private const nuint SIZE_MAXIMIZED = 2;
    private const nuint SIZE_RESTORED  = 0;

    private const int WMSZ_LEFT        = 1;
    private const int WMSZ_RIGHT       = 2;
    private const int WMSZ_TOP         = 3;
    private const int WMSZ_TOPLEFT     = 4;
    private const int WMSZ_TOPRIGHT    = 5;
    private const int WMSZ_BOTTOM      = 6;
    private const int WMSZ_BOTTOMLEFT  = 7;
    private const int WMSZ_BOTTOMRIGHT = 8;

    private const int HTCLIENT = 1;

    private const uint PM_REMOVE   = 0x0001;
    private const uint PM_NOREMOVE = 0x0000;

    private const uint TME_LEAVE = 0x00000002;

    private const uint RID_INPUT = 0x10000003;
    private const uint RIDEV_REMOVE = 0x00000001;
    private const ushort MOUSE_MOVE_ABSOLUTE   = 0x0001;
    private const ushort MOUSE_VIRTUAL_DESKTOP = 0x0002;

    private const int SM_CXICON    = 11;
    private const int SM_CYICON    = 12;
    private const int SM_CXSMICON  = 49;
    private const int SM_CYSMICON  = 50;
    private const int SM_CYCAPTION = 4;
    private const int SM_CXSCREEN  = 0;
    private const int SM_CYSCREEN  = 1;
    private const int SM_XVIRTUALSCREEN  = 76;
    private const int SM_YVIRTUALSCREEN  = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;
    private const int SM_REMOTESESSION   = 0x1000;
    private const int SM_CXCURSOR  = 13;
    private const int SM_CYCURSOR  = 14;

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    private const uint KF_EXTENDED = 0x0100;
    private const uint KF_UP       = 0x8000;
    private const uint MAPVK_VK_TO_VSC = 0;

    private const int VK_SHIFT     = 0x10;
    private const int VK_CONTROL   = 0x11;
    private const int VK_MENU      = 0x12;
    private const int VK_LSHIFT    = 0xA0;
    private const int VK_RSHIFT    = 0xA1;
    private const int VK_LWIN      = 0x5B;
    private const int VK_RWIN      = 0x5C;
    private const int VK_CAPITAL   = 0x14;
    private const int VK_NUMLOCK   = 0x90;
    private const int VK_PROCESSKEY = 0xE5;
    private const int VK_SNAPSHOT  = 0x2C;

    private const uint XBUTTON1 = 1;
    private const uint XBUTTON2 = 2;

    private const int WHEEL_DELTA = 120;

    private const nuint UNICODE_NOCHAR = 0xFFFF;

    private const nuint ICON_BIG   = 1;
    private const nuint ICON_SMALL = 0;

    private const uint LWA_ALPHA = 0x00000002;

    private const uint IMAGE_ICON   = 1;
    private const uint IMAGE_CURSOR = 2;
    private const uint LR_DEFAULTSIZE = 0x00000040;
    private const uint LR_SHARED     = 0x00008000;

    private const uint CF_UNICODETEXT = 13;
    private const uint GMEM_MOVEABLE  = 0x0002;

    private const int OCR_NORMAL  = 32512;
    private const int OCR_IBEAM   = 32513;
    private const int OCR_CROSS   = 32515;
    private const int OCR_HAND    = 32649;
    private const int OCR_SIZEWE  = 32644;
    private const int OCR_SIZENS  = 32645;
    private const int OCR_SIZENWSE = 32642;
    private const int OCR_SIZENESW = 32643;
    private const int OCR_SIZEALL = 32646;
    private const int OCR_NO      = 32648;

    private const uint DWM_BB_ENABLE      = 0x00000001;
    private const uint DWM_BB_BLURREGION  = 0x00000002;

    private const uint BI_BITFIELDS = 3;
    private const uint DIB_RGB_COLORS = 0;

    private const uint MSGFLT_ALLOW = 1;

    private const uint ES_CONTINUOUS        = 0x80000000;
    private const uint ES_DISPLAY_REQUIRED  = 0x00000002;

    private const uint SPI_GETMOUSETRAILS = 0x005E;
    private const uint SPI_SETMOUSETRAILS = 0x005D;

    private const uint STARTF_USESHOWWINDOW = 0x00000001;

    private const uint QS_ALLINPUT = 0x04FF;

    private const uint CS_HREDRAW = 0x0002;
    private const uint CS_VREDRAW = 0x0001;
    private const uint CS_OWNDC   = 0x0020;

    private const int CW_USEDEFAULT = unchecked((int)0x80000000);

    private const int USER_DEFAULT_SCREEN_DPI = 96;

    private static char* IDC_ARROW => (char*)32512;

    private static char* MAKEINTRESOURCEW(int id) => (char*)(nuint)(ushort)id;

    // Macros
    private static int GET_X_LPARAM(nint lp) => (short)(ushort)((uint)lp & 0xFFFF);
    private static int GET_Y_LPARAM(nint lp) => (short)(ushort)(((uint)lp >> 16) & 0xFFFF);
    private static ushort LOWORD(nuint w) => (ushort)((uint)w & 0xFFFF);
    private static ushort HIWORD(nuint w) => (ushort)(((uint)w >> 16) & 0xFFFF);
    private static ushort LOWORD_LP(nint lp) => (ushort)((uint)lp & 0xFFFF);
    private static ushort HIWORD_LP(nint lp) => (ushort)(((uint)lp >> 16) & 0xFFFF);
    private static uint GET_XBUTTON_WPARAM(nuint w) => HIWORD(w);

    private static int _glfw_min(int a, int b) => a < b ? a : b;

    // =======================================================================
    //  GCHandle-based window lookup from HWND via SetPropW/GetPropW
    // =======================================================================

    private static readonly string GlfwPropName = "GLFW";

    private static void SetWindowGlfwProp(nint hWnd, GlfwWindow window)
    {
        var gcHandle = GCHandle.Alloc(window);
        fixed (char* prop = GlfwPropName)
            Win32Native.user32!.SetPropW(hWnd, prop, GCHandle.ToIntPtr(gcHandle));
    }

    private static GlfwWindow? GetWindowGlfwProp(nint hWnd)
    {
        nint ptr;
        fixed (char* prop = GlfwPropName)
            ptr = Win32Native.user32!.GetPropW(hWnd, prop);
        if (ptr == 0) return null;
        var gcHandle = GCHandle.FromIntPtr(ptr);
        return gcHandle.Target as GlfwWindow;
    }

    private static void RemoveWindowGlfwProp(nint hWnd)
    {
        nint ptr;
        fixed (char* prop = GlfwPropName)
        {
            ptr = Win32Native.user32!.GetPropW(hWnd, prop);
            Win32Native.user32!.RemovePropW(hWnd, prop);
        }
        if (ptr != 0)
        {
            var gcHandle = GCHandle.FromIntPtr(ptr);
            gcHandle.Free();
        }
    }

    // =======================================================================
    //  Static helpers (from win32_window.c static functions)
    // =======================================================================

    // Returns the window style for the specified window
    private static uint getWindowStyle(GlfwWindow window)
    {
        uint style = WS_CLIPSIBLINGS_W | WS_CLIPCHILDREN_W;

        if (window.Monitor != null)
            style |= WS_POPUP_W;
        else
        {
            style |= WS_SYSMENU | WS_MINIMIZEBOX;

            if (window.Decorated)
            {
                style |= WS_CAPTION;

                if (window.Resizable)
                    style |= WS_MAXIMIZEBOX | WS_THICKFRAME;
            }
            else
                style |= WS_POPUP_W;
        }

        return style;
    }

    // Returns the extended window style for the specified window
    private static uint getWindowExStyle(GlfwWindow window)
    {
        uint style = WS_EX_APPWINDOW;

        if (window.Monitor != null || window.Floating)
            style |= WS_EX_TOPMOST;

        return style;
    }

    // Returns the image whose area most closely matches the desired one
    private static int chooseImage(int count, GlfwImage[] images, int width, int height)
    {
        int leastDiff = int.MaxValue;
        int closest = 0;

        for (int i = 0; i < count; i++)
        {
            int currDiff = Math.Abs(images[i].Width * images[i].Height - width * height);
            if (currDiff < leastDiff)
            {
                closest = i;
                leastDiff = currDiff;
            }
        }

        return closest;
    }

    // Creates an RGBA icon or cursor
    private static nint createIcon(in GlfwImage image, int xhot, int yhot, bool icon)
    {
        nint dc;
        nint handle;
        nint color, mask;
        byte* target = null;
        var source = image.Pixels!;

        BITMAPV5HEADER bi = default;
        bi.bV5Size        = (uint)sizeof(BITMAPV5HEADER);
        bi.bV5Width       = image.Width;
        bi.bV5Height      = -image.Height;
        bi.bV5Planes      = 1;
        bi.bV5BitCount    = 32;
        bi.bV5Compression = BI_BITFIELDS;
        bi.bV5RedMask     = 0x00ff0000;
        bi.bV5GreenMask   = 0x0000ff00;
        bi.bV5BlueMask    = 0x000000ff;
        bi.bV5AlphaMask   = 0xff000000;

        dc = Win32Native.gdi32!.GetDC(0);
        color = Win32Native.gdi32!.CreateDIBSection(dc, (nint)(&bi), DIB_RGB_COLORS, (void**)&target, 0, 0);
        Win32Native.gdi32!.ReleaseDC(0, dc);

        if (color == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to create RGBA bitmap");
            return 0;
        }

        mask = Win32Native.gdi32!.CreateBitmap(image.Width, image.Height, 1, 1, 0);
        if (mask == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to create mask bitmap");
            Win32Native.gdi32!.DeleteObject(color);
            return 0;
        }

        for (int i = 0; i < image.Width * image.Height; i++)
        {
            target[0] = source[i * 4 + 2];
            target[1] = source[i * 4 + 1];
            target[2] = source[i * 4 + 0];
            target[3] = source[i * 4 + 3];
            target += 4;
        }

        ICONINFO ii = default;
        ii.fIcon    = icon ? 1 : 0;
        ii.xHotspot = (uint)xhot;
        ii.yHotspot = (uint)yhot;
        ii.hbmMask  = mask;
        ii.hbmColor = color;

        handle = Win32Native.user32!.CreateIconIndirect(&ii);

        Win32Native.gdi32!.DeleteObject(color);
        Win32Native.gdi32!.DeleteObject(mask);

        if (handle == 0)
        {
            if (icon)
                _glfwInputErrorWin32(GLFW_PLATFORM_ERROR, "Win32: Failed to create icon");
            else
                _glfwInputErrorWin32(GLFW_PLATFORM_ERROR, "Win32: Failed to create cursor");
        }

        return handle;
    }

    // Enforce the content area aspect ratio based on which edge is being dragged
    private static void applyAspectRatio(GlfwWindow window, int edge, RECT* area)
    {
        RECT frame = default;
        float ratio = (float)window.Numer / (float)window.Denom;
        uint style = getWindowStyle(window);
        uint exStyle = getWindowExStyle(window);
        var win32 = _glfw.Win32!;

        if (_glfwIsWindows10Version1607OrGreaterWin32())
            win32.user32.AdjustWindowRectExForDpi_(&frame, style, 0, exStyle,
                win32.user32.GetDpiForWindow_(window.Win32!.handle));
        else
            Win32Native.user32!.AdjustWindowRectEx(&frame, style, 0, exStyle);

        if (edge == WMSZ_LEFT || edge == WMSZ_BOTTOMLEFT ||
            edge == WMSZ_RIGHT || edge == WMSZ_BOTTOMRIGHT)
        {
            area->bottom = area->top + (frame.bottom - frame.top) +
                (int)(((area->right - area->left) - (frame.right - frame.left)) / ratio);
        }
        else if (edge == WMSZ_TOPLEFT || edge == WMSZ_TOPRIGHT)
        {
            area->top = area->bottom - (frame.bottom - frame.top) -
                (int)(((area->right - area->left) - (frame.right - frame.left)) / ratio);
        }
        else if (edge == WMSZ_TOP || edge == WMSZ_BOTTOM)
        {
            area->right = area->left + (frame.right - frame.left) +
                (int)(((area->bottom - area->top) - (frame.bottom - frame.top)) * ratio);
        }
    }

    // Updates the cursor image according to its cursor mode
    private static void updateCursorImageWin32(GlfwWindow window)
    {
        if (window.CursorMode == GLFW_CURSOR_NORMAL ||
            window.CursorMode == GLFW_CURSOR_CAPTURED)
        {
            if (window.Cursor != null && window.Cursor.Win32 != null)
                Win32Native.user32!.SetCursor(window.Cursor.Win32.handle);
            else
                Win32Native.user32!.SetCursor(Win32Native.user32!.LoadCursorW(0, IDC_ARROW));
        }
        else
        {
            Win32Native.user32!.SetCursor(_glfw.Win32!.blankCursor);
        }
    }

    // Sets the cursor clip rect to the window content area
    private static void captureCursorWin32(GlfwWindow window)
    {
        RECT clipRect;
        Win32Native.user32!.GetClientRect(window.Win32!.handle, &clipRect);
        Win32Native.user32!.ClientToScreen(window.Win32.handle, (POINT*)&clipRect.left);
        Win32Native.user32!.ClientToScreen(window.Win32.handle, (POINT*)&clipRect.right);
        Win32Native.user32!.ClipCursor(&clipRect);
        _glfw.Win32!.capturedCursorWindow = window;
    }

    // Disable clip cursor
    private static void releaseCursorWin32()
    {
        Win32Native.user32!.ClipCursor(null);
        _glfw.Win32!.capturedCursorWindow = null;
    }

    // Enables WM_INPUT messages for the mouse for the specified window
    private static void enableRawMouseMotionWin32(GlfwWindow window)
    {
        RAWINPUTDEVICE rid;
        rid.usUsagePage = 0x01;
        rid.usUsage = 0x02;
        rid.dwFlags = 0;
        rid.hwndTarget = window.Win32!.handle;

        if (Win32Native.user32!.RegisterRawInputDevices(&rid, 1, (uint)sizeof(RAWINPUTDEVICE)) == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to register raw input device");
        }
    }

    // Disables WM_INPUT messages for the mouse
    private static void disableRawMouseMotionWin32(GlfwWindow window)
    {
        RAWINPUTDEVICE rid;
        rid.usUsagePage = 0x01;
        rid.usUsage = 0x02;
        rid.dwFlags = RIDEV_REMOVE;
        rid.hwndTarget = 0;

        if (Win32Native.user32!.RegisterRawInputDevices(&rid, 1, (uint)sizeof(RAWINPUTDEVICE)) == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to remove raw input device");
        }
    }

    // Apply disabled cursor mode to a focused window
    private static void disableCursorWin32(GlfwWindow window)
    {
        var win32 = _glfw.Win32!;
        win32.disabledCursorWindow = window;
        _glfwGetCursorPosWin32(window, out win32.restoreCursorPosX, out win32.restoreCursorPosY);
        updateCursorImageWin32(window);
        _glfwCenterCursorInContentArea(window);
        captureCursorWin32(window);

        if (window.RawMouseMotion)
            enableRawMouseMotionWin32(window);
    }

    // Exit disabled cursor mode for the specified window
    private static void enableCursorWin32(GlfwWindow window)
    {
        var win32 = _glfw.Win32!;

        if (window.RawMouseMotion)
            disableRawMouseMotionWin32(window);

        win32.disabledCursorWindow = null;
        releaseCursorWin32();
        _glfwSetCursorPosWin32(window, win32.restoreCursorPosX, win32.restoreCursorPosY);
        updateCursorImageWin32(window);
    }

    // Returns whether the cursor is in the content area of the specified window
    private static bool cursorInContentArea(GlfwWindow window)
    {
        RECT area;
        POINT pos;

        if (Win32Native.user32!.GetCursorPos(&pos) == 0)
            return false;

        if (Win32Native.user32!.WindowFromPoint(pos) != window.Win32!.handle)
            return false;

        Win32Native.user32!.GetClientRect(window.Win32.handle, &area);
        Win32Native.user32!.ClientToScreen(window.Win32.handle, (POINT*)&area.left);
        Win32Native.user32!.ClientToScreen(window.Win32.handle, (POINT*)&area.right);

        return Win32Native.user32!.PtInRect(&area, pos) != 0;
    }

    // Update native window styles to match attributes
    private static void updateWindowStyles(GlfwWindow window)
    {
        RECT rect;
        uint style = (uint)Win32Native.user32!.GetWindowLongW(window.Win32!.handle, GWL_STYLE);
        style &= ~(WS_OVERLAPPEDWINDOW_W | WS_POPUP_W);
        style |= getWindowStyle(window);

        Win32Native.user32!.GetClientRect(window.Win32.handle, &rect);

        var win32 = _glfw.Win32!;
        if (_glfwIsWindows10Version1607OrGreaterWin32())
        {
            win32.user32.AdjustWindowRectExForDpi_(&rect, style, 0,
                getWindowExStyle(window),
                win32.user32.GetDpiForWindow_(window.Win32.handle));
        }
        else
            Win32Native.user32!.AdjustWindowRectEx(&rect, style, 0, getWindowExStyle(window));

        Win32Native.user32!.ClientToScreen(window.Win32.handle, (POINT*)&rect.left);
        Win32Native.user32!.ClientToScreen(window.Win32.handle, (POINT*)&rect.right);
        Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_STYLE, (int)style);
        Win32Native.user32!.SetWindowPos(window.Win32.handle, HWND_TOP,
            rect.left, rect.top,
            rect.right - rect.left, rect.bottom - rect.top,
            SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOZORDER);
    }

    // Update window framebuffer transparency
    private static void updateFramebufferTransparency(GlfwWindow window)
    {
        var win32 = _glfw.Win32!;
        int composition = 0;
        uint color;
        int opaque;

        if (win32.dwmapi.IsCompositionEnabled == null)
            return;
        int hr = win32.dwmapi.IsCompositionEnabled(&composition);
        if (hr < 0 || composition == 0)
            return;

        if (_glfwIsWindows8OrGreaterWin32() ||
            (win32.dwmapi.GetColorizationColor != null &&
             win32.dwmapi.GetColorizationColor(&color, &opaque) >= 0 && opaque == 0))
        {
            nint region = Win32Native.gdi32!.CreateRectRgn(0, 0, -1, -1);
            DWM_BLURBEHIND bb = default;
            bb.dwFlags = DWM_BB_ENABLE | DWM_BB_BLURREGION;
            bb.hRgnBlur = region;
            bb.fEnable = 1;

            if (win32.dwmapi.EnableBlurBehindWindow != null)
                win32.dwmapi.EnableBlurBehindWindow(window.Win32!.handle, (nint)(&bb));
            Win32Native.gdi32!.DeleteObject(region);
        }
        else
        {
            DWM_BLURBEHIND bb = default;
            bb.dwFlags = DWM_BB_ENABLE;
            if (win32.dwmapi.EnableBlurBehindWindow != null)
                win32.dwmapi.EnableBlurBehindWindow(window.Win32!.handle, (nint)(&bb));
        }
    }

    // Retrieves and translates modifier keys
    private static int getKeyMods()
    {
        int mods = 0;

        if ((Win32Native.user32!.GetKeyState(VK_SHIFT) & 0x8000) != 0)
            mods |= GLFW_MOD_SHIFT;
        if ((Win32Native.user32!.GetKeyState(VK_CONTROL) & 0x8000) != 0)
            mods |= GLFW_MOD_CONTROL;
        if ((Win32Native.user32!.GetKeyState(VK_MENU) & 0x8000) != 0)
            mods |= GLFW_MOD_ALT;
        if (((Win32Native.user32!.GetKeyState(VK_LWIN) | Win32Native.user32!.GetKeyState(VK_RWIN)) & 0x8000) != 0)
            mods |= GLFW_MOD_SUPER;
        if ((Win32Native.user32!.GetKeyState(VK_CAPITAL) & 1) != 0)
            mods |= GLFW_MOD_CAPS_LOCK;
        if ((Win32Native.user32!.GetKeyState(VK_NUMLOCK) & 1) != 0)
            mods |= GLFW_MOD_NUM_LOCK;

        return mods;
    }

    private static void fitToMonitor(GlfwWindow window)
    {
        MONITORINFOEXW mi;
        mi.cbSize = (uint)sizeof(MONITORINFOEXW);
        Win32Native.user32!.GetMonitorInfoW(window.Monitor!.Win32!.handle, (MONITORINFOEXW*)&mi);
        Win32Native.user32!.SetWindowPos(window.Win32!.handle, HWND_TOPMOST,
            mi.rcMonitor.left,
            mi.rcMonitor.top,
            mi.rcMonitor.right - mi.rcMonitor.left,
            mi.rcMonitor.bottom - mi.rcMonitor.top,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOCOPYBITS);
    }

    // Make the specified window and its video mode active on its monitor
    private static void acquireMonitorWin32(GlfwWindow window)
    {
        var win32 = _glfw.Win32!;

        if (win32.acquiredMonitorCount == 0)
        {
            Win32Native.kernel32!.SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED);

            uint mouseTrailSize;
            Win32Native.user32!.SystemParametersInfoW(SPI_GETMOUSETRAILS, 0, (nint)(&mouseTrailSize), 0);
            win32.mouseTrailSize = mouseTrailSize;
            Win32Native.user32!.SystemParametersInfoW(SPI_SETMOUSETRAILS, 0, 0, 0);
        }

        if (window.Monitor!.Window == null)
            win32.acquiredMonitorCount++;

        _glfwSetVideoModeWin32(window.Monitor, in window.VideoMode);
        _glfwInputMonitorWindow(window.Monitor, window);
    }

    // Remove the window and restore the original video mode
    private static void releaseMonitorWin32(GlfwWindow window)
    {
        if (window.Monitor!.Window != window)
            return;

        var win32 = _glfw.Win32!;
        win32.acquiredMonitorCount--;
        if (win32.acquiredMonitorCount == 0)
        {
            Win32Native.kernel32!.SetThreadExecutionState(ES_CONTINUOUS);

            uint trails = win32.mouseTrailSize;
            Win32Native.user32!.SystemParametersInfoW(SPI_SETMOUSETRAILS, trails, 0, 0);
        }

        _glfwInputMonitorWindow(window.Monitor, null);
        _glfwRestoreVideoModeWin32(window.Monitor);
    }

    // Manually maximize the window, for when SW_MAXIMIZE cannot be used
    private static void maximizeWindowManually(GlfwWindow window)
    {
        RECT rect;
        MONITORINFOEXW mi;
        mi.cbSize = (uint)sizeof(MONITORINFOEXW);

        Win32Native.user32!.GetMonitorInfoW(Win32Native.user32!.MonitorFromWindow(window.Win32!.handle,
            MONITOR_DEFAULTTONEAREST), (MONITORINFOEXW*)&mi);

        rect = mi.rcWork;

        if (window.MaxWidth != GLFW_DONT_CARE && window.MaxHeight != GLFW_DONT_CARE)
        {
            rect.right = _glfw_min(rect.right, rect.left + window.MaxWidth);
            rect.bottom = _glfw_min(rect.bottom, rect.top + window.MaxHeight);
        }

        uint style = (uint)Win32Native.user32!.GetWindowLongW(window.Win32.handle, GWL_STYLE);
        style |= WS_MAXIMIZE;
        Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_STYLE, (int)style);

        if (window.Decorated)
        {
            uint exStyle = (uint)Win32Native.user32!.GetWindowLongW(window.Win32.handle, GWL_EXSTYLE);
            var win32 = _glfw.Win32!;

            if (_glfwIsWindows10Version1607OrGreaterWin32())
            {
                uint dpi = win32.user32.GetDpiForWindow_(window.Win32.handle);
                win32.user32.AdjustWindowRectExForDpi_(&rect, style, 0, exStyle, dpi);
                Win32Native.user32!.OffsetRect(&rect, 0, win32.user32.GetSystemMetricsForDpi_(SM_CYCAPTION, dpi));
            }
            else
            {
                Win32Native.user32!.AdjustWindowRectEx(&rect, style, 0, exStyle);
                Win32Native.user32!.OffsetRect(&rect, 0, Win32Native.user32!.GetSystemMetrics(SM_CYCAPTION));
            }

            rect.bottom = _glfw_min(rect.bottom, mi.rcWork.bottom);
        }

        Win32Native.user32!.SetWindowPos(window.Win32.handle, HWND_TOP,
            rect.left,
            rect.top,
            rect.right - rect.left,
            rect.bottom - rect.top,
            SWP_NOACTIVATE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    // _glfwIsWindows8OrGreaterWin32 and _glfwIsWindows10Version1703OrGreaterWin32
    // are defined in win32_init.cs (shared via partial class)

    // =======================================================================
    //  Window procedure for user-created windows
    // =======================================================================

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static nint windowProc(nint hWnd, uint uMsg, nuint wParam, nint lParam)
    {
        var window = GetWindowGlfwProp(hWnd);
        if (window == null)
        {
            if (uMsg == WM_NCCREATE)
            {
                if (_glfwIsWindows10Version1607OrGreaterWin32())
                {
                    CREATESTRUCTW* cs = (CREATESTRUCTW*)lParam;
                    if (cs->lpCreateParams != 0)
                    {
                        // scaleToMonitor flag passed via lpCreateParams
                        var wndconfig = GCHandle.FromIntPtr(cs->lpCreateParams).Target as GlfwWndConfig;
                        if (wndconfig != null && wndconfig.ScaleToMonitor
                            && (nint)_glfw.Win32!.user32.EnableNonClientDpiScaling_ != 0)
                        {
                            _glfw.Win32.user32.EnableNonClientDpiScaling_(hWnd);
                        }
                    }
                }
            }

            return Win32Native.user32!.DefWindowProcW(hWnd, uMsg, wParam, lParam);
        }

        switch (uMsg)
        {
            case WM_MOUSEACTIVATE:
            {
                // HACK: Postpone cursor disabling when the window was activated by
                //       clicking a caption button
                if (HIWORD_LP(lParam) == WM_LBUTTONDOWN)
                {
                    if (LOWORD_LP(lParam) != HTCLIENT)
                        window.Win32!.frameAction = true;
                }

                break;
            }

            case WM_CAPTURECHANGED:
            {
                // HACK: Disable the cursor once the caption button action has been
                //       completed or cancelled
                if (lParam == 0 && window.Win32!.frameAction)
                {
                    if (window.CursorMode == GLFW_CURSOR_DISABLED)
                        disableCursorWin32(window);
                    else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                        captureCursorWin32(window);

                    window.Win32.frameAction = false;
                }

                break;
            }

            case WM_SETFOCUS:
            {
                _glfwInputWindowFocus(window, GLFW_TRUE);

                // HACK: Do not disable cursor while the user is interacting with
                //       a caption button
                if (window.Win32!.frameAction)
                    break;

                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    disableCursorWin32(window);
                else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                    captureCursorWin32(window);

                return 0;
            }

            case WM_KILLFOCUS:
            {
                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    enableCursorWin32(window);
                else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                    releaseCursorWin32();

                if (window.Monitor != null && window.AutoIconify)
                    _glfwIconifyWindowWin32(window);

                _glfwInputWindowFocus(window, GLFW_FALSE);
                return 0;
            }

            case WM_SYSCOMMAND:
            {
                switch (wParam & 0xfff0)
                {
                    case (nuint)SC_SCREENSAVE:
                    case (nuint)SC_MONITORPOWER:
                    {
                        if (window.Monitor != null)
                            return 0;
                        else
                            break;
                    }

                    case (nuint)SC_KEYMENU:
                    {
                        if (!window.Win32!.keymenu)
                            return 0;

                        break;
                    }
                }
                break;
            }

            case WM_CLOSE:
            {
                _glfwInputWindowCloseRequest(window);
                return 0;
            }

            case WM_INPUTLANGCHANGE:
            {
                _glfwUpdateKeyNamesWin32();
                break;
            }

            case WM_CHAR:
            case WM_SYSCHAR:
            {
                if (wParam >= 0xd800 && wParam <= 0xdbff)
                    window.Win32!.highSurrogate = (char)(ushort)wParam;
                else
                {
                    uint codepoint = 0;

                    if (wParam >= 0xdc00 && wParam <= 0xdfff)
                    {
                        if (window.Win32!.highSurrogate != 0)
                        {
                            codepoint += (uint)(window.Win32.highSurrogate - 0xd800) << 10;
                            codepoint += (ushort)wParam - 0xdc00u;
                            codepoint += 0x10000;
                        }
                    }
                    else
                        codepoint = (uint)(ushort)wParam;

                    window.Win32!.highSurrogate = '\0';
                    _glfwInputChar(window, codepoint, getKeyMods(), uMsg != WM_SYSCHAR);
                }

                if (uMsg == WM_SYSCHAR && window.Win32!.keymenu)
                    break;

                return 0;
            }

            case WM_UNICHAR:
            {
                if (wParam == UNICODE_NOCHAR)
                    return 1; // TRUE

                _glfwInputChar(window, (uint)wParam, getKeyMods(), true);
                return 0;
            }

            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
            case WM_KEYUP:
            case WM_SYSKEYUP:
            {
                int key, scancode;
                int action = (HIWORD_LP(lParam) & KF_UP) != 0 ? GLFW_RELEASE : GLFW_PRESS;
                int mods = getKeyMods();

                scancode = (int)(HIWORD_LP(lParam) & (KF_EXTENDED | 0xff));
                if ((scancode & 0xff) == 0)
                {
                    // Media keys and Clipboard History may omit the low byte.
                    // Preserve KF_EXTENDED while recovering the scan code.
                    scancode = MergeWin32Scancode(scancode, Win32Native.user32!.MapVirtualKeyW((uint)wParam, MAPVK_VK_TO_VSC));
                }

                // HACK: Alt+PrtSc has a different scancode than just PrtSc
                if (scancode == 0x54)
                    scancode = 0x137;

                // HACK: Ctrl+Pause has a different scancode than just Pause
                if (scancode == 0x146)
                    scancode = 0x45;

                // HACK: CJK IME sets the extended bit for right Shift
                if (scancode == 0x136)
                    scancode = 0x36;

                key = _glfw.Win32!.keycodes[scancode];

                // The Ctrl keys require special handling
                if (wParam == (nuint)VK_CONTROL)
                {
                    if ((HIWORD_LP(lParam) & KF_EXTENDED) != 0)
                    {
                        key = GLFW_KEY_RIGHT_CONTROL;
                    }
                    else
                    {
                        MSG next;
                        uint time = Win32Native.user32!.GetMessageTime();

                        if (Win32Native.user32!.PeekMessageW(&next, 0, 0, 0, PM_NOREMOVE) != 0)
                        {
                            if (next.message == WM_KEYDOWN ||
                                next.message == WM_SYSKEYDOWN ||
                                next.message == WM_KEYUP ||
                                next.message == WM_SYSKEYUP)
                            {
                                if (next.wParam == (nuint)VK_MENU &&
                                    (HIWORD_LP(next.lParam) & KF_EXTENDED) != 0 &&
                                    next.time == time)
                                {
                                    break;
                                }
                            }
                        }

                        key = GLFW_KEY_LEFT_CONTROL;
                    }
                }
                else if (wParam == (nuint)VK_PROCESSKEY)
                {
                    break;
                }

                if (action == GLFW_RELEASE && wParam == (nuint)VK_SHIFT)
                {
                    _glfwInputKey(window, GLFW_KEY_LEFT_SHIFT, scancode, action, mods);
                    _glfwInputKey(window, GLFW_KEY_RIGHT_SHIFT, scancode, action, mods);
                }
                else if (wParam == (nuint)VK_SNAPSHOT)
                {
                    _glfwInputKey(window, key, scancode, GLFW_PRESS, mods);
                    _glfwInputKey(window, key, scancode, GLFW_RELEASE, mods);
                }
                else
                    _glfwInputKey(window, key, scancode, action, mods);

                break;
            }

            case WM_LBUTTONDOWN:
            case WM_RBUTTONDOWN:
            case WM_MBUTTONDOWN:
            case WM_XBUTTONDOWN:
            case WM_LBUTTONUP:
            case WM_RBUTTONUP:
            case WM_MBUTTONUP:
            case WM_XBUTTONUP:
            {
                int button, action;

                if (uMsg == WM_LBUTTONDOWN || uMsg == WM_LBUTTONUP)
                    button = GLFW_MOUSE_BUTTON_LEFT;
                else if (uMsg == WM_RBUTTONDOWN || uMsg == WM_RBUTTONUP)
                    button = GLFW_MOUSE_BUTTON_RIGHT;
                else if (uMsg == WM_MBUTTONDOWN || uMsg == WM_MBUTTONUP)
                    button = GLFW_MOUSE_BUTTON_MIDDLE;
                else if (GET_XBUTTON_WPARAM(wParam) == XBUTTON1)
                    button = GLFW_MOUSE_BUTTON_4;
                else
                    button = GLFW_MOUSE_BUTTON_5;

                if (uMsg == WM_LBUTTONDOWN || uMsg == WM_RBUTTONDOWN ||
                    uMsg == WM_MBUTTONDOWN || uMsg == WM_XBUTTONDOWN)
                {
                    action = GLFW_PRESS;
                }
                else
                    action = GLFW_RELEASE;

                int i;
                for (i = 0; i <= GLFW_MOUSE_BUTTON_LAST; i++)
                {
                    if (window.MouseButtons[i] == GLFW_PRESS)
                        break;
                }

                if (i > GLFW_MOUSE_BUTTON_LAST)
                    Win32Native.user32!.SetCapture(hWnd);

                _glfwInputMouseClick(window, button, action, getKeyMods());

                for (i = 0; i <= GLFW_MOUSE_BUTTON_LAST; i++)
                {
                    if (window.MouseButtons[i] == GLFW_PRESS)
                        break;
                }

                if (i > GLFW_MOUSE_BUTTON_LAST)
                    Win32Native.user32!.ReleaseCapture();

                if (uMsg == WM_XBUTTONDOWN || uMsg == WM_XBUTTONUP)
                    return 1; // TRUE

                return 0;
            }

            case WM_MOUSEMOVE:
            {
                int x = GET_X_LPARAM(lParam);
                int y = GET_Y_LPARAM(lParam);

                if (!window.Win32!.cursorTracked)
                {
                    TRACKMOUSEEVENT tme = default;
                    tme.cbSize = (uint)sizeof(TRACKMOUSEEVENT);
                    tme.dwFlags = TME_LEAVE;
                    tme.hwndTrack = window.Win32.handle;
                    Win32Native.user32!.TrackMouseEvent(&tme);

                    window.Win32.cursorTracked = true;
                    _glfwInputCursorEnter(window, true);
                }

                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                {
                    int dx = x - window.Win32.lastCursorPosX;
                    int dy = y - window.Win32.lastCursorPosY;

                    if (_glfw.Win32!.disabledCursorWindow != window)
                        break;
                    if (window.RawMouseMotion)
                        break;

                    _glfwInputCursorPos(window,
                        window.VirtualCursorPosX + dx,
                        window.VirtualCursorPosY + dy);
                }
                else
                    _glfwInputCursorPos(window, x, y);

                window.Win32.lastCursorPosX = x;
                window.Win32.lastCursorPosY = y;

                return 0;
            }

            case WM_INPUT:
            {
                uint size = 0;
                nint ri = lParam;

                if (_glfw.Win32!.disabledCursorWindow != window)
                    break;
                if (!window.RawMouseMotion)
                    break;

                Win32Native.user32!.GetRawInputData(ri, RID_INPUT, null, &size, (uint)sizeof(RAWINPUTHEADER));

                var win32 = _glfw.Win32;
                if (size > (uint)win32.rawInputSize)
                {
                    win32.rawInput = new byte[size];
                    win32.rawInputSize = (int)size;
                }

                size = (uint)win32.rawInputSize;
                fixed (byte* rawPtr = win32.rawInput)
                {
                    if (Win32Native.user32!.GetRawInputData(ri, RID_INPUT, rawPtr, &size,
                            (uint)sizeof(RAWINPUTHEADER)) == unchecked((uint)-1))
                    {
                        _glfwInputError(GLFW_PLATFORM_ERROR,
                            "Win32: Failed to retrieve raw input data");
                        break;
                    }

                    RAWINPUT* data = (RAWINPUT*)rawPtr;
                    int dx, dy;

                    if ((data->mouse.usFlags & MOUSE_MOVE_ABSOLUTE) != 0)
                    {
                        POINT pos = default;
                        int width, height;

                        if ((data->mouse.usFlags & MOUSE_VIRTUAL_DESKTOP) != 0)
                        {
                            pos.x += Win32Native.user32!.GetSystemMetrics(SM_XVIRTUALSCREEN);
                            pos.y += Win32Native.user32!.GetSystemMetrics(SM_YVIRTUALSCREEN);
                            width = Win32Native.user32!.GetSystemMetrics(SM_CXVIRTUALSCREEN);
                            height = Win32Native.user32!.GetSystemMetrics(SM_CYVIRTUALSCREEN);
                        }
                        else
                        {
                            width = Win32Native.user32!.GetSystemMetrics(SM_CXSCREEN);
                            height = Win32Native.user32!.GetSystemMetrics(SM_CYSCREEN);
                        }

                        pos.x += (int)((data->mouse.lLastX / 65535.0f) * width);
                        pos.y += (int)((data->mouse.lLastY / 65535.0f) * height);
                        Win32Native.user32!.ScreenToClient(window.Win32!.handle, &pos);

                        dx = pos.x - window.Win32.lastCursorPosX;
                        dy = pos.y - window.Win32.lastCursorPosY;
                    }
                    else
                    {
                        dx = data->mouse.lLastX;
                        dy = data->mouse.lLastY;
                    }

                    _glfwInputCursorPos(window,
                        window.VirtualCursorPosX + dx,
                        window.VirtualCursorPosY + dy);

                    window.Win32!.lastCursorPosX += dx;
                    window.Win32.lastCursorPosY += dy;
                }
                break;
            }

            case WM_MOUSELEAVE:
            {
                window.Win32!.cursorTracked = false;
                _glfwInputCursorEnter(window, false);
                return 0;
            }

            case WM_MOUSEWHEEL:
            {
                _glfwInputScroll(window, 0.0, (short)HIWORD(wParam) / (double)WHEEL_DELTA);
                return 0;
            }

            case WM_MOUSEHWHEEL:
            {
                // NOTE: The X-axis is inverted for consistency with macOS and X11
                _glfwInputScroll(window, -((short)HIWORD(wParam) / (double)WHEEL_DELTA), 0.0);
                return 0;
            }

            case WM_ENTERSIZEMOVE:
            case WM_ENTERMENULOOP:
            {
                if (window.Win32!.frameAction)
                    break;

                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    enableCursorWin32(window);
                else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                    releaseCursorWin32();

                break;
            }

            case WM_EXITSIZEMOVE:
            case WM_EXITMENULOOP:
            {
                if (window.Win32!.frameAction)
                    break;

                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    disableCursorWin32(window);
                else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                    captureCursorWin32(window);

                break;
            }

            case WM_SIZE:
            {
                int width = LOWORD((nuint)(uint)lParam);
                int height = HIWORD((nuint)(uint)lParam);
                bool iconified = wParam == SIZE_MINIMIZED;
                bool maximized = wParam == SIZE_MAXIMIZED ||
                                 (window.Win32!.maximized && wParam != SIZE_RESTORED);

                if (_glfw.Win32!.capturedCursorWindow == window)
                    captureCursorWin32(window);

                if (window.Win32!.iconified != iconified)
                    _glfwInputWindowIconify(window, iconified ? 1 : 0);

                if (window.Win32.maximized != maximized)
                    _glfwInputWindowMaximize(window, maximized ? 1 : 0);

                if (width != window.Win32.width || height != window.Win32.height)
                {
                    window.Win32.width = width;
                    window.Win32.height = height;

                    _glfwInputFramebufferSize(window, width, height);
                    _glfwInputWindowSize(window, width, height);
                }

                if (window.Monitor != null && window.Win32.iconified != iconified)
                {
                    if (iconified)
                        releaseMonitorWin32(window);
                    else
                    {
                        acquireMonitorWin32(window);
                        fitToMonitor(window);
                    }
                }

                window.Win32.iconified = iconified;
                window.Win32.maximized = maximized;
                return 0;
            }

            case WM_MOVE:
            {
                if (_glfw.Win32!.capturedCursorWindow == window)
                    captureCursorWin32(window);

                _glfwInputWindowPos(window,
                    GET_X_LPARAM(lParam),
                    GET_Y_LPARAM(lParam));
                return 0;
            }

            case WM_SIZING:
            {
                if (window.Numer == GLFW_DONT_CARE ||
                    window.Denom == GLFW_DONT_CARE)
                {
                    break;
                }

                applyAspectRatio(window, (int)wParam, (RECT*)lParam);
                return 1; // TRUE
            }

            case WM_GETMINMAXINFO:
            {
                RECT frame = default;
                MINMAXINFO* mmi = (MINMAXINFO*)lParam;
                uint style = getWindowStyle(window);
                uint exStyle = getWindowExStyle(window);

                if (window.Monitor != null)
                    break;

                if (_glfwIsWindows10Version1607OrGreaterWin32())
                {
                    _glfw.Win32!.user32.AdjustWindowRectExForDpi_(&frame, style, 0, exStyle,
                        _glfw.Win32.user32.GetDpiForWindow_(window.Win32!.handle));
                }
                else
                    Win32Native.user32!.AdjustWindowRectEx(&frame, style, 0, exStyle);

                if (window.MinWidth != GLFW_DONT_CARE &&
                    window.MinHeight != GLFW_DONT_CARE)
                {
                    mmi->ptMinTrackSize.x = window.MinWidth + frame.right - frame.left;
                    mmi->ptMinTrackSize.y = window.MinHeight + frame.bottom - frame.top;
                }

                if (window.MaxWidth != GLFW_DONT_CARE &&
                    window.MaxHeight != GLFW_DONT_CARE)
                {
                    mmi->ptMaxTrackSize.x = window.MaxWidth + frame.right - frame.left;
                    mmi->ptMaxTrackSize.y = window.MaxHeight + frame.bottom - frame.top;
                }

                if (!window.Decorated)
                {
                    MONITORINFOEXW mi;
                    nint mh = Win32Native.user32!.MonitorFromWindow(window.Win32!.handle,
                        MONITOR_DEFAULTTONEAREST);

                    mi = default;
                    mi.cbSize = (uint)sizeof(MONITORINFOEXW);
                    Win32Native.user32!.GetMonitorInfoW(mh, (MONITORINFOEXW*)&mi);

                    mmi->ptMaxPosition.x = mi.rcWork.left - mi.rcMonitor.left;
                    mmi->ptMaxPosition.y = mi.rcWork.top - mi.rcMonitor.top;
                    mmi->ptMaxSize.x = mi.rcWork.right - mi.rcWork.left;
                    mmi->ptMaxSize.y = mi.rcWork.bottom - mi.rcWork.top;
                }

                return 0;
            }

            case WM_PAINT:
            {
                _glfwInputWindowDamage(window);
                break;
            }

            case WM_ERASEBKGND:
            {
                return 1; // TRUE
            }

            case WM_NCACTIVATE:
            case WM_NCPAINT:
            {
                if (!window.Decorated)
                    return 1; // TRUE

                break;
            }

            case WM_DWMCOMPOSITIONCHANGED:
            case WM_DWMCOLORIZATIONCOLORCHANGED:
            {
                if (window.Win32!.transparent)
                    updateFramebufferTransparency(window);
                return 0;
            }

            case WM_GETDPISCALEDSIZE_W:
            {
                if (window.Win32!.scaleToMonitor)
                    break;

                if (_glfwIsWindows10Version1703OrGreaterWin32())
                {
                    RECT source = default, target = default;
                    SIZE* size = (SIZE*)lParam;

                    _glfw.Win32!.user32.AdjustWindowRectExForDpi_(&source, getWindowStyle(window),
                        0, getWindowExStyle(window),
                        _glfw.Win32.user32.GetDpiForWindow_(window.Win32.handle));
                    _glfw.Win32.user32.AdjustWindowRectExForDpi_(&target, getWindowStyle(window),
                        0, getWindowExStyle(window),
                        LOWORD(wParam));

                    size->cx += (target.right - target.left) -
                                (source.right - source.left);
                    size->cy += (target.bottom - target.top) -
                                (source.bottom - source.top);
                    return 1; // TRUE
                }

                break;
            }

            case WM_DPICHANGED_W:
            {
                float xscale = HIWORD(wParam) / (float)USER_DEFAULT_SCREEN_DPI;
                float yscale = LOWORD(wParam) / (float)USER_DEFAULT_SCREEN_DPI;

                if (window.Monitor == null &&
                    (window.Win32!.scaleToMonitor ||
                     _glfwIsWindows10Version1703OrGreaterWin32()))
                {
                    RECT* suggested = (RECT*)lParam;
                    Win32Native.user32!.SetWindowPos(window.Win32.handle, HWND_TOP,
                        suggested->left,
                        suggested->top,
                        suggested->right - suggested->left,
                        suggested->bottom - suggested->top,
                        SWP_NOACTIVATE | SWP_NOZORDER);
                }

                _glfwInputWindowContentScale(window, xscale, yscale);
                break;
            }

            case WM_SETCURSOR:
            {
                if (LOWORD_LP(lParam) == HTCLIENT)
                {
                    updateCursorImageWin32(window);
                    return 1; // TRUE
                }

                break;
            }

            case WM_DROPFILES:
            {
                nint drop = (nint)wParam;
                POINT pt;

                int count = (int)Win32Native.shell32!.DragQueryFileW(drop, 0xffffffff, null, 0);
                string[] paths = new string[count];

                // Move the mouse to the position of the drop
                Win32Native.shell32!.DragQueryPoint(drop, &pt);
                _glfwInputCursorPos(window, pt.x, pt.y);

#pragma warning disable CA2014 // stackalloc in loop -- porting C code pattern, bounded allocation
                for (int i = 0; i < count; i++)
                {
                    uint length = Win32Native.shell32!.DragQueryFileW(drop, (uint)i, null, 0);
                    char* buffer = stackalloc char[(int)length + 1];

                    Win32Native.shell32!.DragQueryFileW(drop, (uint)i, buffer, length + 1);
                    paths[i] = new string(buffer);
                }
#pragma warning restore CA2014

                _glfwInputDrop(window, count, paths);

                Win32Native.shell32!.DragFinish(drop);
                return 0;
            }
        }

        return Win32Native.user32!.DefWindowProcW(hWnd, uMsg, wParam, lParam);
    }

    // =======================================================================
    //  Creates the GLFW window
    // =======================================================================

    private static bool createNativeWindow(GlfwWindow window,
        GlfwWndConfig wndconfig, GlfwFbConfig fbconfig)
    {
        int frameX, frameY, frameWidth, frameHeight;
        uint style = getWindowStyle(window);
        uint exStyle = getWindowExStyle(window);
        var win32 = _glfw.Win32!;

        if (win32.mainWindowClass == 0)
        {
            WNDCLASSEXW wc = default;
            wc.cbSize = (uint)sizeof(WNDCLASSEXW);
            wc.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
            wc.lpfnWndProc = &windowProc;
            wc.hInstance = win32.instance;
            wc.hCursor = Win32Native.user32!.LoadCursorW(0, IDC_ARROW);

            fixed (char* className = "GLFW30")
                wc.lpszClassName = className;

            // Load user-provided icon if available
            fixed (char* iconName = "GLFW_ICON")
            {
                nint moduleHandle = Win32Native.kernel32!.GetModuleHandleW(null);
                wc.hIcon = Win32Native.user32!.LoadImageW(moduleHandle, iconName, IMAGE_ICON,
                    0, 0, LR_DEFAULTSIZE | LR_SHARED);
            }

            if (wc.hIcon == 0)
            {
                // No user-provided icon found, load default icon
                wc.hIcon = Win32Native.user32!.LoadImageW(0, MAKEINTRESOURCEW(32512), IMAGE_ICON,
                    0, 0, LR_DEFAULTSIZE | LR_SHARED);
            }

            fixed (char* className = "GLFW30")
            {
                wc.lpszClassName = className;
                win32.mainWindowClass = Win32Native.user32!.RegisterClassExW(&wc);
            }

            if (win32.mainWindowClass == 0)
            {
                _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                    "Win32: Failed to register window class");
                return false;
            }
        }

        if (Win32Native.user32!.GetSystemMetrics(SM_REMOTESESSION) != 0)
        {
            if (win32.blankCursor == 0)
            {
                int cursorWidth = Win32Native.user32!.GetSystemMetrics(SM_CXCURSOR);
                int cursorHeight = Win32Native.user32!.GetSystemMetrics(SM_CYCURSOR);

                byte[] cursorPixels = new byte[cursorWidth * cursorHeight * 4];
                // NOTE: Windows checks whether the image is fully transparent and if so
                //       just ignores the alpha channel and makes the whole cursor opaque
                // HACK: Make one pixel slightly less transparent
                cursorPixels[3] = 1;

                GlfwImage cursorImage = new GlfwImage
                {
                    Width = cursorWidth,
                    Height = cursorHeight,
                    Pixels = cursorPixels
                };
                win32.blankCursor = createIcon(in cursorImage, 0, 0, false);

                if (win32.blankCursor == 0)
                    return false;
            }
        }

        if (window.Monitor != null)
        {
            MONITORINFOEXW mi;
            mi.cbSize = (uint)sizeof(MONITORINFOEXW);
            Win32Native.user32!.GetMonitorInfoW(window.Monitor.Win32!.handle, (MONITORINFOEXW*)&mi);

            frameX = mi.rcMonitor.left;
            frameY = mi.rcMonitor.top;
            frameWidth  = mi.rcMonitor.right - mi.rcMonitor.left;
            frameHeight = mi.rcMonitor.bottom - mi.rcMonitor.top;
        }
        else
        {
            RECT rect = new RECT { left = 0, top = 0, right = wndconfig.Width, bottom = wndconfig.Height };

            window.Win32!.maximized = wndconfig.Maximized;
            if (wndconfig.Maximized)
                style |= WS_MAXIMIZE;

            Win32Native.user32!.AdjustWindowRectEx(&rect, style, 0, exStyle);

            if (wndconfig.Xpos == GLFW_ANY_POSITION && wndconfig.Ypos == GLFW_ANY_POSITION)
            {
                frameX = CW_USEDEFAULT;
                frameY = CW_USEDEFAULT;
            }
            else
            {
                frameX = wndconfig.Xpos + rect.left;
                frameY = wndconfig.Ypos + rect.top;
            }

            frameWidth  = rect.right - rect.left;
            frameHeight = rect.bottom - rect.top;
        }

        // Pass wndconfig via GCHandle for WM_NCCREATE
        var wndconfigHandle = GCHandle.Alloc(wndconfig);

        fixed (char* wideTitle = window.Title ?? "")
        {
            window.Win32!.handle = Win32Native.user32!.CreateWindowExW(exStyle,
                Win32InitPInvoke.MAKEINTATOM(win32.mainWindowClass),
                wideTitle,
                style,
                frameX, frameY,
                frameWidth, frameHeight,
                0, // No parent window
                0, // No window menu
                win32.instance,
                GCHandle.ToIntPtr(wndconfigHandle));
        }

        wndconfigHandle.Free();

        if (window.Win32.handle == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to create window");
            return false;
        }

        SetWindowGlfwProp(window.Win32.handle, window);

        fixed (char* wmDropFiles = (string?)null)
        {
            Win32Native.user32!.ChangeWindowMessageFilterEx(window.Win32.handle, WM_DROPFILES, MSGFLT_ALLOW, null);
            Win32Native.user32!.ChangeWindowMessageFilterEx(window.Win32.handle, WM_COPYDATA, MSGFLT_ALLOW, null);
            Win32Native.user32!.ChangeWindowMessageFilterEx(window.Win32.handle, WM_COPYGLOBALDATA_W, MSGFLT_ALLOW, null);
        }

        window.Win32.scaleToMonitor = wndconfig.ScaleToMonitor;
        window.Win32.keymenu = wndconfig.Win32.Keymenu;
        window.Win32.showDefault = wndconfig.Win32.ShowDefault;

        if (window.Monitor == null)
        {
            RECT rect = new RECT { left = 0, top = 0, right = wndconfig.Width, bottom = wndconfig.Height };
            WINDOWPLACEMENT wp = default;
            wp.length = (uint)sizeof(WINDOWPLACEMENT);
            nint mh = Win32Native.user32!.MonitorFromWindow(window.Win32.handle, MONITOR_DEFAULTTONEAREST);

            if (wndconfig.ScaleToMonitor)
            {
                _glfwGetHMONITORContentScaleWin32(mh, out float xscale, out float yscale);

                if (xscale > 0.0f && yscale > 0.0f)
                {
                    rect.right = (int)(rect.right * xscale);
                    rect.bottom = (int)(rect.bottom * yscale);
                }
            }

            if (_glfwIsWindows10Version1607OrGreaterWin32())
            {
                win32.user32.AdjustWindowRectExForDpi_(&rect, style, 0, exStyle,
                    win32.user32.GetDpiForWindow_(window.Win32.handle));
            }
            else
                Win32Native.user32!.AdjustWindowRectEx(&rect, style, 0, exStyle);

            Win32Native.user32!.GetWindowPlacement(window.Win32.handle, &wp);
            Win32Native.user32!.OffsetRect(&rect,
                wp.rcNormalPosition.left - rect.left,
                wp.rcNormalPosition.top - rect.top);

            wp.rcNormalPosition = rect;
            wp.showCmd = (uint)SW_HIDE;
            Win32Native.user32!.SetWindowPlacement(window.Win32.handle, &wp);

            if (wndconfig.Maximized && !wndconfig.Decorated)
            {
                MONITORINFOEXW mi;
                mi.cbSize = (uint)sizeof(MONITORINFOEXW);
                Win32Native.user32!.GetMonitorInfoW(mh, (MONITORINFOEXW*)&mi);

                Win32Native.user32!.SetWindowPos(window.Win32.handle, HWND_TOP,
                    mi.rcWork.left,
                    mi.rcWork.top,
                    mi.rcWork.right - mi.rcWork.left,
                    mi.rcWork.bottom - mi.rcWork.top,
                    SWP_NOACTIVATE | SWP_NOZORDER);
            }
        }

        Win32Native.shell32!.DragAcceptFiles(window.Win32.handle, 1);

        if (fbconfig.Transparent)
        {
            updateFramebufferTransparency(window);
            window.Win32.transparent = true;
        }

        _glfwGetWindowSizeWin32(window, out window.Win32.width, out window.Win32.height);

        return true;
    }

    // =======================================================================
    //  Platform API methods
    // =======================================================================

    internal static bool _glfwCreateWindowWin32(GlfwWindow window,
        GlfwWndConfig wndconfig, GlfwCtxConfig ctxconfig, GlfwFbConfig fbconfig)
    {
        window.Win32 = new GlfwWindowWin32();

        if (!createNativeWindow(window, wndconfig, fbconfig))
            return false;

        if (ctxconfig.Client != GLFW_NO_API)
        {
            if (ctxconfig.Source == GLFW_NATIVE_CONTEXT_API)
            {
                if (!_glfwInitWGL())
                    return false;
                if (!_glfwCreateContextWGL(window, ctxconfig, fbconfig))
                    return false;
            }
            else if (ctxconfig.Source == GLFW_EGL_CONTEXT_API)
            {
                if (!_glfwInitEGL() || !_glfwCreateContextEGL(window, ctxconfig, fbconfig)) return false;
            }
            else if (ctxconfig.Source == GLFW_OSMESA_CONTEXT_API)
            {
                if (!_glfwInitOSMesa() || !_glfwCreateContextOSMesa(window, ctxconfig, fbconfig)) return false;
            }

            if (!_glfwRefreshContextAttribs(window, ctxconfig))
                return false;
        }

        if (wndconfig.MousePassthrough)
            _glfwSetWindowMousePassthroughWin32(window, true);

        if (window.Monitor != null)
        {
            _glfwShowWindowWin32(window);
            _glfwFocusWindowWin32(window);
            acquireMonitorWin32(window);
            fitToMonitor(window);

            if (wndconfig.CenterCursor)
                _glfwCenterCursorInContentArea(window);
        }
        else
        {
            if (wndconfig.Visible)
            {
                _glfwShowWindowWin32(window);
                if (wndconfig.Focused)
                    _glfwFocusWindowWin32(window);
            }
        }

        return true;
    }

    internal static void _glfwDestroyWindowWin32(GlfwWindow window)
    {
        if (window.Monitor != null)
            releaseMonitorWin32(window);

        if (window.context.destroy != null)
            window.context.destroy(window);

        if (_glfw.Win32!.disabledCursorWindow == window)
            enableCursorWin32(window);

        if (_glfw.Win32.capturedCursorWindow == window)
            releaseCursorWin32();

        if (window.Win32!.handle != 0)
        {
            RemoveWindowGlfwProp(window.Win32.handle);
            Win32Native.user32!.DestroyWindow(window.Win32.handle);
            window.Win32.handle = 0;
        }

        if (window.Win32.bigIcon != 0)
            Win32Native.user32!.DestroyIcon(window.Win32.bigIcon);

        if (window.Win32.smallIcon != 0)
            Win32Native.user32!.DestroyIcon(window.Win32.smallIcon);
    }

    internal static void _glfwSetWindowTitleWin32(GlfwWindow window, string title)
    {
        fixed (char* wideTitle = title)
            Win32Native.user32!.SetWindowTextW(window.Win32!.handle, wideTitle);
    }

    internal static void _glfwSetWindowIconWin32(GlfwWindow window, int count, GlfwImage[]? images)
    {
        nint bigIcon = 0, smallIcon = 0;

        if (count > 0 && images != null)
        {
            int bigIdx = chooseImage(count, images,
                Win32Native.user32!.GetSystemMetrics(SM_CXICON), Win32Native.user32!.GetSystemMetrics(SM_CYICON));
            int smallIdx = chooseImage(count, images,
                Win32Native.user32!.GetSystemMetrics(SM_CXSMICON), Win32Native.user32!.GetSystemMetrics(SM_CYSMICON));

            bigIcon = createIcon(in images[bigIdx], 0, 0, true);
            smallIcon = createIcon(in images[smallIdx], 0, 0, true);
        }
        else
        {
            bigIcon = (nint)Win32Native.user32!.GetClassLongPtrW(window.Win32!.handle, GCLP_HICON);
            smallIcon = (nint)Win32Native.user32!.GetClassLongPtrW(window.Win32.handle, GCLP_HICONSM);
        }

        Win32Native.user32!.SendMessageW(window.Win32!.handle, WM_SETICON, ICON_BIG, bigIcon);
        Win32Native.user32!.SendMessageW(window.Win32.handle, WM_SETICON, ICON_SMALL, smallIcon);

        if (window.Win32.bigIcon != 0)
            Win32Native.user32!.DestroyIcon(window.Win32.bigIcon);

        if (window.Win32.smallIcon != 0)
            Win32Native.user32!.DestroyIcon(window.Win32.smallIcon);

        if (count > 0)
        {
            window.Win32.bigIcon = bigIcon;
            window.Win32.smallIcon = smallIcon;
        }
    }

    internal static void _glfwGetWindowPosWin32(GlfwWindow window, out int xpos, out int ypos)
    {
        POINT pos = default;
        Win32Native.user32!.ClientToScreen(window.Win32!.handle, &pos);
        xpos = pos.x;
        ypos = pos.y;
    }

    internal static void _glfwSetWindowPosWin32(GlfwWindow window, int xpos, int ypos)
    {
        RECT rect = new RECT { left = xpos, top = ypos, right = xpos, bottom = ypos };
        var win32 = _glfw.Win32!;

        if (_glfwIsWindows10Version1607OrGreaterWin32())
        {
            win32.user32.AdjustWindowRectExForDpi_(&rect, getWindowStyle(window),
                0, getWindowExStyle(window),
                win32.user32.GetDpiForWindow_(window.Win32!.handle));
        }
        else
            Win32Native.user32!.AdjustWindowRectEx(&rect, getWindowStyle(window), 0, getWindowExStyle(window));

        Win32Native.user32!.SetWindowPos(window.Win32!.handle, 0, rect.left, rect.top, 0, 0,
            SWP_NOACTIVATE | SWP_NOZORDER | SWP_NOSIZE);
    }

    internal static void _glfwGetWindowSizeWin32(GlfwWindow window, out int width, out int height)
    {
        RECT area;
        Win32Native.user32!.GetClientRect(window.Win32!.handle, &area);
        width = area.right;
        height = area.bottom;
    }

    internal static void _glfwSetWindowSizeWin32(GlfwWindow window, int width, int height)
    {
        if (window.Monitor != null)
        {
            if (window.Monitor.Window == window)
            {
                acquireMonitorWin32(window);
                fitToMonitor(window);
            }
        }
        else
        {
            RECT rect = new RECT { left = 0, top = 0, right = width, bottom = height };
            var win32 = _glfw.Win32!;

            if (_glfwIsWindows10Version1607OrGreaterWin32())
            {
                win32.user32.AdjustWindowRectExForDpi_(&rect, getWindowStyle(window),
                    0, getWindowExStyle(window),
                    win32.user32.GetDpiForWindow_(window.Win32!.handle));
            }
            else
                Win32Native.user32!.AdjustWindowRectEx(&rect, getWindowStyle(window), 0, getWindowExStyle(window));

            Win32Native.user32!.SetWindowPos(window.Win32!.handle, HWND_TOP,
                0, 0, rect.right - rect.left, rect.bottom - rect.top,
                SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOMOVE | SWP_NOZORDER);
        }
    }

    internal static void _glfwSetWindowSizeLimitsWin32(GlfwWindow window,
        int minwidth, int minheight, int maxwidth, int maxheight)
    {
        if ((minwidth == GLFW_DONT_CARE || minheight == GLFW_DONT_CARE) &&
            (maxwidth == GLFW_DONT_CARE || maxheight == GLFW_DONT_CARE))
        {
            return;
        }

        RECT area;
        Win32Native.user32!.GetWindowRect(window.Win32!.handle, &area);
        Win32Native.user32!.MoveWindow(window.Win32.handle,
            area.left, area.top,
            area.right - area.left,
            area.bottom - area.top, 1);
    }

    internal static void _glfwSetWindowAspectRatioWin32(GlfwWindow window, int numer, int denom)
    {
        if (numer == GLFW_DONT_CARE || denom == GLFW_DONT_CARE)
            return;

        RECT area;
        Win32Native.user32!.GetWindowRect(window.Win32!.handle, &area);
        applyAspectRatio(window, WMSZ_BOTTOMRIGHT, &area);
        Win32Native.user32!.MoveWindow(window.Win32.handle,
            area.left, area.top,
            area.right - area.left,
            area.bottom - area.top, 1);
    }

    internal static void _glfwGetFramebufferSizeWin32(GlfwWindow window, out int width, out int height)
    {
        _glfwGetWindowSizeWin32(window, out width, out height);
    }

    internal static void _glfwGetWindowFrameSizeWin32(GlfwWindow window,
        out int left, out int top, out int right, out int bottom)
    {
        _glfwGetWindowSizeWin32(window, out int width, out int height);
        RECT rect;
        Win32Native.user32!.SetRect(&rect, 0, 0, width, height);

        var win32 = _glfw.Win32!;
        if (_glfwIsWindows10Version1607OrGreaterWin32())
        {
            win32.user32.AdjustWindowRectExForDpi_(&rect, getWindowStyle(window),
                0, getWindowExStyle(window),
                win32.user32.GetDpiForWindow_(window.Win32!.handle));
        }
        else
            Win32Native.user32!.AdjustWindowRectEx(&rect, getWindowStyle(window), 0, getWindowExStyle(window));

        left = -rect.left;
        top = -rect.top;
        right = rect.right - width;
        bottom = rect.bottom - height;
    }

    internal static void _glfwGetWindowContentScaleWin32(GlfwWindow window,
        out float xscale, out float yscale)
    {
        nint handle = Win32Native.user32!.MonitorFromWindow(window.Win32!.handle, MONITOR_DEFAULTTONEAREST);
        _glfwGetHMONITORContentScaleWin32(handle, out xscale, out yscale);
    }

    internal static void _glfwIconifyWindowWin32(GlfwWindow window)
    {
        Win32Native.user32!.ShowWindow(window.Win32!.handle, SW_MINIMIZE);
    }

    internal static void _glfwRestoreWindowWin32(GlfwWindow window)
    {
        Win32Native.user32!.ShowWindow(window.Win32!.handle, SW_RESTORE);
    }

    internal static void _glfwMaximizeWindowWin32(GlfwWindow window)
    {
        if (Win32Native.user32!.IsWindowVisible(window.Win32!.handle) != 0)
            Win32Native.user32!.ShowWindow(window.Win32.handle, SW_MAXIMIZE);
        else
            maximizeWindowManually(window);
    }

    internal static void _glfwShowWindowWin32(GlfwWindow window)
    {
        int showCommand = SW_SHOWNA;

        if (window.Win32!.showDefault)
        {
            STARTUPINFOW si = default;
            si.cb = (uint)sizeof(STARTUPINFOW);
            Win32Native.kernel32!.GetStartupInfoW(&si);
            if ((si.dwFlags & STARTF_USESHOWWINDOW) != 0)
                showCommand = si.wShowWindow;

            window.Win32.showDefault = false;
        }

        Win32Native.user32!.ShowWindow(window.Win32.handle, showCommand);
    }

    internal static void _glfwHideWindowWin32(GlfwWindow window)
    {
        Win32Native.user32!.ShowWindow(window.Win32!.handle, SW_HIDE);
    }

    internal static void _glfwRequestWindowAttentionWin32(GlfwWindow window)
    {
        Win32Native.user32!.FlashWindow(window.Win32!.handle, 1);
    }

    internal static void _glfwFocusWindowWin32(GlfwWindow window)
    {
        Win32Native.user32!.BringWindowToTop(window.Win32!.handle);
        Win32Native.user32!.SetForegroundWindow(window.Win32.handle);
        Win32Native.user32!.SetFocus(window.Win32.handle);
    }

    internal static void _glfwSetWindowMonitorWin32(GlfwWindow window,
        GlfwMonitor? monitor, int xpos, int ypos, int width, int height, int refreshRate)
    {
        if (window.Monitor == monitor)
        {
            if (monitor != null)
            {
                if (monitor.Window == window)
                {
                    acquireMonitorWin32(window);
                    fitToMonitor(window);
                }
            }
            else
            {
                RECT rect = new RECT { left = xpos, top = ypos, right = xpos + width, bottom = ypos + height };
                var win32 = _glfw.Win32!;

                if (_glfwIsWindows10Version1607OrGreaterWin32())
                {
                    win32.user32.AdjustWindowRectExForDpi_(&rect, getWindowStyle(window),
                        0, getWindowExStyle(window),
                        win32.user32.GetDpiForWindow_(window.Win32!.handle));
                }
                else
                    Win32Native.user32!.AdjustWindowRectEx(&rect, getWindowStyle(window), 0, getWindowExStyle(window));

                Win32Native.user32!.SetWindowPos(window.Win32!.handle, HWND_TOP,
                    rect.left, rect.top,
                    rect.right - rect.left, rect.bottom - rect.top,
                    SWP_NOCOPYBITS | SWP_NOACTIVATE | SWP_NOZORDER);
            }

            return;
        }

        if (window.Monitor != null)
            releaseMonitorWin32(window);

        _glfwInputWindowMonitor(window, monitor);

        if (window.Monitor != null)
        {
            MONITORINFOEXW mi;
            mi.cbSize = (uint)sizeof(MONITORINFOEXW);
            uint flags = SWP_SHOWWINDOW | SWP_NOACTIVATE | SWP_NOCOPYBITS;

            if (window.Decorated)
            {
                uint style = (uint)Win32Native.user32!.GetWindowLongW(window.Win32!.handle, GWL_STYLE);
                style &= ~WS_OVERLAPPEDWINDOW_W;
                style |= getWindowStyle(window);
                Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_STYLE, (int)style);
                flags |= SWP_FRAMECHANGED;
            }

            acquireMonitorWin32(window);

            Win32Native.user32!.GetMonitorInfoW(window.Monitor.Win32!.handle, (MONITORINFOEXW*)&mi);
            Win32Native.user32!.SetWindowPos(window.Win32!.handle, HWND_TOPMOST,
                mi.rcMonitor.left,
                mi.rcMonitor.top,
                mi.rcMonitor.right - mi.rcMonitor.left,
                mi.rcMonitor.bottom - mi.rcMonitor.top,
                flags);
        }
        else
        {
            nint after;
            RECT rect = new RECT { left = xpos, top = ypos, right = xpos + width, bottom = ypos + height };
            uint style = (uint)Win32Native.user32!.GetWindowLongW(window.Win32!.handle, GWL_STYLE);
            uint flags = SWP_NOACTIVATE | SWP_NOCOPYBITS;

            if (window.Decorated)
            {
                style &= ~WS_POPUP_W;
                style |= getWindowStyle(window);
                Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_STYLE, (int)style);

                flags |= SWP_FRAMECHANGED;
            }

            if (window.Floating)
                after = HWND_TOPMOST;
            else
                after = HWND_NOTOPMOST;

            if (_glfwIsWindows10Version1607OrGreaterWin32())
            {
                _glfw.Win32!.user32.AdjustWindowRectExForDpi_(&rect, getWindowStyle(window),
                    0, getWindowExStyle(window),
                    _glfw.Win32.user32.GetDpiForWindow_(window.Win32.handle));
            }
            else
                Win32Native.user32!.AdjustWindowRectEx(&rect, getWindowStyle(window), 0, getWindowExStyle(window));

            Win32Native.user32!.SetWindowPos(window.Win32.handle, after,
                rect.left, rect.top,
                rect.right - rect.left, rect.bottom - rect.top,
                flags);
        }
    }

    internal static bool _glfwWindowFocusedWin32(GlfwWindow window)
    {
        return window.Win32!.handle == Win32Native.user32!.GetActiveWindow();
    }

    internal static bool _glfwWindowIconifiedWin32(GlfwWindow window)
    {
        return Win32Native.user32!.IsIconic(window.Win32!.handle) != 0;
    }

    internal static bool _glfwWindowVisibleWin32(GlfwWindow window)
    {
        return Win32Native.user32!.IsWindowVisible(window.Win32!.handle) != 0;
    }

    internal static bool _glfwWindowMaximizedWin32(GlfwWindow window)
    {
        return Win32Native.user32!.IsZoomed(window.Win32!.handle) != 0;
    }

    internal static bool _glfwWindowHoveredWin32(GlfwWindow window)
    {
        return cursorInContentArea(window);
    }

    internal static bool _glfwFramebufferTransparentWin32(GlfwWindow window)
    {
        int composition = 0;
        uint color;
        int opaque;
        var win32 = _glfw.Win32!;

        if (!window.Win32!.transparent)
            return false;

        if (win32.dwmapi.IsCompositionEnabled == null)
            return false;
        if (win32.dwmapi.IsCompositionEnabled(&composition) < 0 || composition == 0)
            return false;

        if (!_glfwIsWindows8OrGreaterWin32())
        {
            if (win32.dwmapi.GetColorizationColor == null)
                return false;
            if (win32.dwmapi.GetColorizationColor(&color, &opaque) < 0 || opaque != 0)
                return false;
        }

        return true;
    }

    internal static void _glfwSetWindowResizableWin32(GlfwWindow window, bool enabled)
    {
        updateWindowStyles(window);
    }

    internal static void _glfwSetWindowDecoratedWin32(GlfwWindow window, bool enabled)
    {
        updateWindowStyles(window);
    }

    internal static void _glfwSetWindowFloatingWin32(GlfwWindow window, bool enabled)
    {
        nint after = enabled ? HWND_TOPMOST : HWND_NOTOPMOST;
        Win32Native.user32!.SetWindowPos(window.Win32!.handle, after, 0, 0, 0, 0,
            SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE);
    }

    internal static void _glfwSetWindowMousePassthroughWin32(GlfwWindow window, bool enabled)
    {
        uint key = 0;
        byte alpha = 0;
        uint flags = 0;
        uint exStyle = (uint)Win32Native.user32!.GetWindowLongW(window.Win32!.handle, GWL_EXSTYLE);

        if ((exStyle & WS_EX_LAYERED) != 0)
            Win32Native.user32!.GetLayeredWindowAttributes(window.Win32.handle, &key, &alpha, &flags);

        if (enabled)
            exStyle |= (WS_EX_TRANSPARENT | WS_EX_LAYERED);
        else
        {
            exStyle &= ~WS_EX_TRANSPARENT;
            if ((exStyle & WS_EX_LAYERED) != 0)
            {
                if ((flags & LWA_ALPHA) == 0)
                    exStyle &= ~WS_EX_LAYERED;
            }
        }

        Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_EXSTYLE, (int)exStyle);

        if (enabled)
            Win32Native.user32!.SetLayeredWindowAttributes(window.Win32.handle, key, alpha, flags);
    }

    internal static float _glfwGetWindowOpacityWin32(GlfwWindow window)
    {
        byte alpha;
        uint flags;

        if (((uint)Win32Native.user32!.GetWindowLongW(window.Win32!.handle, GWL_EXSTYLE) & WS_EX_LAYERED) != 0 &&
            Win32Native.user32!.GetLayeredWindowAttributes(window.Win32.handle, null, &alpha, &flags) != 0)
        {
            if ((flags & LWA_ALPHA) != 0)
                return alpha / 255.0f;
        }

        return 1.0f;
    }

    internal static void _glfwSetWindowOpacityWin32(GlfwWindow window, float opacity)
    {
        int exStyle = Win32Native.user32!.GetWindowLongW(window.Win32!.handle, GWL_EXSTYLE);
        if (opacity < 1.0f || ((uint)exStyle & WS_EX_TRANSPARENT) != 0)
        {
            byte alpha = (byte)(255 * opacity);
            exStyle |= (int)WS_EX_LAYERED;
            Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_EXSTYLE, exStyle);
            Win32Native.user32!.SetLayeredWindowAttributes(window.Win32.handle, 0, alpha, LWA_ALPHA);
        }
        else if (((uint)exStyle & WS_EX_TRANSPARENT) != 0)
        {
            Win32Native.user32!.SetLayeredWindowAttributes(window.Win32.handle, 0, 0, 0);
        }
        else
        {
            exStyle &= ~(int)WS_EX_LAYERED;
            Win32Native.user32!.SetWindowLongW(window.Win32.handle, GWL_EXSTYLE, exStyle);
        }
    }

    internal static void _glfwSetRawMouseMotionWin32(GlfwWindow window, bool enabled)
    {
        if (_glfw.Win32!.disabledCursorWindow != window)
            return;

        if (enabled)
            enableRawMouseMotionWin32(window);
        else
            disableRawMouseMotionWin32(window);
    }

    internal static bool _glfwRawMouseMotionSupportedWin32()
    {
        return true;
    }

    internal static void _glfwPollEventsWin32()
    {
        MSG msg;

        while (Win32Native.user32!.PeekMessageW(&msg, 0, 0, 0, PM_REMOVE) != 0)
        {
            if (msg.message == WM_QUIT)
            {
                var w = _glfw.windowListHead;
                while (w != null)
                {
                    _glfwInputWindowCloseRequest(w);
                    w = w.Next;
                }
            }
            else
            {
                Win32Native.user32!.TranslateMessage(&msg);
                Win32Native.user32!.DispatchMessageW(&msg);
            }
        }

        // HACK: Release modifier keys that the system did not emit KEYUP for
        nint handle = Win32Native.user32!.GetActiveWindow();
        if (handle != 0)
        {
            var window = GetWindowGlfwProp(handle);
            if (window != null)
            {
                int[][] keys = new int[][]
                {
                    new[] { VK_LSHIFT, GLFW_KEY_LEFT_SHIFT },
                    new[] { VK_RSHIFT, GLFW_KEY_RIGHT_SHIFT },
                    new[] { VK_LWIN, GLFW_KEY_LEFT_SUPER },
                    new[] { VK_RWIN, GLFW_KEY_RIGHT_SUPER }
                };

                for (int i = 0; i < 4; i++)
                {
                    int vk = keys[i][0];
                    int key = keys[i][1];
                    int scancode = _glfw.Win32!.scancodes[key];

                    if ((Win32Native.user32!.GetKeyState(vk) & 0x8000) != 0)
                        continue;
                    if (window.Keys[key] != GLFW_PRESS)
                        continue;

                    _glfwInputKey(window, key, scancode, GLFW_RELEASE, getKeyMods());
                }
            }
        }

        var disabledWindow = _glfw.Win32!.disabledCursorWindow;
        if (disabledWindow != null)
        {
            _glfwGetWindowSizeWin32(disabledWindow, out int width, out int height);

            if (disabledWindow.Win32!.lastCursorPosX != width / 2 ||
                disabledWindow.Win32.lastCursorPosY != height / 2)
            {
                _glfwSetCursorPosWin32(disabledWindow, width / 2, height / 2);
            }
        }
    }

    internal static void _glfwWaitEventsWin32()
    {
        Win32Native.user32!.WaitMessage();
        _glfwPollEventsWin32();
    }

    internal static void _glfwWaitEventsTimeoutWin32(double timeout)
    {
        Win32Native.user32!.MsgWaitForMultipleObjects(0, null, 0, (uint)(timeout * 1e3), QS_ALLINPUT);
        _glfwPollEventsWin32();
    }

    internal static void _glfwPostEmptyEventWin32()
    {
        Win32Native.user32!.PostMessageW(_glfw.Win32!.helperWindowHandle, WM_NULL, 0, 0);
    }

    internal static void _glfwGetCursorPosWin32(GlfwWindow window, out double xpos, out double ypos)
    {
        POINT pos;
        xpos = 0;
        ypos = 0;

        if (Win32Native.user32!.GetCursorPos(&pos) != 0)
        {
            Win32Native.user32!.ScreenToClient(window.Win32!.handle, &pos);
            xpos = pos.x;
            ypos = pos.y;
        }
    }

    internal static void _glfwSetCursorPosWin32(GlfwWindow window, double xpos, double ypos)
    {
        POINT pos = new POINT { x = (int)xpos, y = (int)ypos };

        // Store the new position so it can be recognized later
        window.Win32!.lastCursorPosX = pos.x;
        window.Win32.lastCursorPosY = pos.y;

        Win32Native.user32!.ClientToScreen(window.Win32.handle, &pos);
        Win32Native.user32!.SetCursorPos(pos.x, pos.y);
    }

    internal static void _glfwSetCursorModeWin32(GlfwWindow window, int mode)
    {
        if (_glfwWindowFocusedWin32(window))
        {
            if (mode == GLFW_CURSOR_DISABLED)
            {
                _glfwGetCursorPosWin32(window,
                    out _glfw.Win32!.restoreCursorPosX,
                    out _glfw.Win32.restoreCursorPosY);
                _glfwCenterCursorInContentArea(window);
                if (window.RawMouseMotion)
                    enableRawMouseMotionWin32(window);
            }
            else if (_glfw.Win32!.disabledCursorWindow == window)
            {
                if (window.RawMouseMotion)
                    disableRawMouseMotionWin32(window);
            }

            if (mode == GLFW_CURSOR_DISABLED || mode == GLFW_CURSOR_CAPTURED)
                captureCursorWin32(window);
            else
                releaseCursorWin32();

            if (mode == GLFW_CURSOR_DISABLED)
                _glfw.Win32!.disabledCursorWindow = window;
            else if (_glfw.Win32!.disabledCursorWindow == window)
            {
                _glfw.Win32.disabledCursorWindow = null;
                _glfwSetCursorPosWin32(window,
                    _glfw.Win32.restoreCursorPosX,
                    _glfw.Win32.restoreCursorPosY);
            }
        }

        if (cursorInContentArea(window))
            updateCursorImageWin32(window);
    }

    internal static string? _glfwGetScancodeNameWin32(int scancode)
    {
        if (scancode < 0 || scancode > (int)(KF_EXTENDED | 0xff))
        {
            _glfwInputError(GLFW_INVALID_VALUE, "Invalid scancode {0}", scancode);
            return null;
        }

        int key = _glfw.Win32!.keycodes[scancode];
        if (key == GLFW_KEY_UNKNOWN)
            return null;

        return _glfw.Win32.keynames[key];
    }

    internal static int _glfwGetKeyScancodeWin32(int key)
    {
        return _glfw.Win32!.scancodes[key];
    }

    internal static bool _glfwCreateCursorWin32(GlfwCursor cursor,
        in GlfwImage image, int xhot, int yhot)
    {
        cursor.Win32 ??= new GlfwCursorWin32();
        cursor.Win32.handle = createIcon(in image, xhot, yhot, false);
        if (cursor.Win32.handle == 0)
            return false;

        return true;
    }

    internal static bool _glfwCreateStandardCursorWin32(GlfwCursor cursor, int shape)
    {
        int id;

        switch (shape)
        {
            case GLFW_ARROW_CURSOR:        id = OCR_NORMAL; break;
            case GLFW_IBEAM_CURSOR:        id = OCR_IBEAM; break;
            case GLFW_CROSSHAIR_CURSOR:    id = OCR_CROSS; break;
            case GLFW_POINTING_HAND_CURSOR: id = OCR_HAND; break;
            case GLFW_RESIZE_EW_CURSOR:    id = OCR_SIZEWE; break;
            case GLFW_RESIZE_NS_CURSOR:    id = OCR_SIZENS; break;
            case GLFW_RESIZE_NWSE_CURSOR:  id = OCR_SIZENWSE; break;
            case GLFW_RESIZE_NESW_CURSOR:  id = OCR_SIZENESW; break;
            case GLFW_RESIZE_ALL_CURSOR:   id = OCR_SIZEALL; break;
            case GLFW_NOT_ALLOWED_CURSOR:  id = OCR_NO; break;
            default:
                _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Unknown standard cursor");
                return false;
        }

        cursor.Win32 ??= new GlfwCursorWin32();
        cursor.Win32.handle = Win32Native.user32!.LoadImageW(0, MAKEINTRESOURCEW(id), IMAGE_CURSOR, 0, 0,
            LR_DEFAULTSIZE | LR_SHARED);
        if (cursor.Win32.handle == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to create standard cursor");
            return false;
        }

        return true;
    }

    internal static void _glfwDestroyCursorWin32(GlfwCursor cursor)
    {
        if (cursor.Win32 != null && cursor.Win32.handle != 0)
            Win32Native.user32!.DestroyIcon(cursor.Win32.handle);
    }

    internal static void _glfwSetCursorWin32(GlfwWindow window, GlfwCursor? cursor)
    {
        if (cursorInContentArea(window))
            updateCursorImageWin32(window);
    }

    internal static void _glfwSetClipboardStringWin32(string text)
    {
        var win32 = _glfw.Win32!;
        int tries = 0;

        int characterCount;
        fixed (char* src = text)
            characterCount = text.Length + 1; // include null terminator

        if (characterCount == 0)
            return;

        nint obj = Win32Native.kernel32!.GlobalAlloc(GMEM_MOVEABLE, (nuint)(characterCount * sizeof(char)));
        if (obj == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to allocate global handle for clipboard");
            return;
        }

        char* buffer = (char*)Win32Native.kernel32!.GlobalLock(obj);
        if (buffer == null)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to lock global handle");
            Win32Native.kernel32!.GlobalFree(obj);
            return;
        }

        fixed (char* src = text)
        {
            for (int i = 0; i < text.Length; i++)
                buffer[i] = src[i];
            buffer[text.Length] = '\0';
        }
        Win32Native.kernel32!.GlobalUnlock(obj);

        while (Win32Native.user32!.OpenClipboard(win32.helperWindowHandle) == 0)
        {
            Win32Native.kernel32!.Sleep(1);
            tries++;

            if (tries == 3)
            {
                _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                    "Win32: Failed to open clipboard");
                Win32Native.kernel32!.GlobalFree(obj);
                return;
            }
        }

        Win32Native.user32!.EmptyClipboard();
        Win32Native.user32!.SetClipboardData(CF_UNICODETEXT, obj);
        Win32Native.user32!.CloseClipboard();
    }

    internal static string? _glfwGetClipboardStringWin32()
    {
        var win32 = _glfw.Win32!;
        int tries = 0;

        while (Win32Native.user32!.OpenClipboard(win32.helperWindowHandle) == 0)
        {
            Win32Native.kernel32!.Sleep(1);
            tries++;

            if (tries == 3)
            {
                _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                    "Win32: Failed to open clipboard");
                return null;
            }
        }

        nint obj = Win32Native.user32!.GetClipboardData(CF_UNICODETEXT);
        if (obj == 0)
        {
            _glfwInputError(GLFW_FORMAT_UNAVAILABLE,
                "Win32: Failed to convert clipboard to string");
            Win32Native.user32!.CloseClipboard();
            return null;
        }

        char* buffer = (char*)Win32Native.kernel32!.GlobalLock(obj);
        if (buffer == null)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to lock global handle");
            Win32Native.user32!.CloseClipboard();
            return null;
        }

        win32.clipboardString = new string(buffer);

        Win32Native.kernel32!.GlobalUnlock(obj);
        Win32Native.user32!.CloseClipboard();

        return win32.clipboardString;
    }
}

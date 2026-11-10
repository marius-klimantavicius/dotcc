// Ported from glfw/src/win32_init.c (GLFW 3.6)
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
using static Glfw.Win32;

namespace Glfw;

// =====================================================================
//  Win32InitPInvoke -- P/Invoke declarations needed by win32_init.cs
//  that are NOT already provided by Win32Native (win32_native.cs).
// =====================================================================

internal static unsafe class Win32InitPInvoke
{
    // All P/Invoke functions have been moved to Win32Native (win32_native.cs).
    // Callers use Win32Native.user32!, Win32Native.kernel32!, etc.

    // --- Constants ---
    internal const uint GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS = 0x00000004;
    internal const uint GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT = 0x00000002;

    internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

    internal const uint FORMAT_MESSAGE_FROM_SYSTEM    = 0x00001000;
    internal const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
    internal const uint FORMAT_MESSAGE_MAX_WIDTH_MASK = 0x000000FF;

    internal const uint CP_UTF8 = 65001;

    internal const uint LANG_NEUTRAL    = 0x00;
    internal const uint SUBLANG_DEFAULT = 0x01;
    internal static uint MAKELANGID(uint p, uint s) => (s << 10) | p;

    internal const ushort _WIN32_WINNT_WIN8     = 0x0602;
    internal const ushort _WIN32_WINNT_WINBLUE  = 0x0603;

    internal static ushort HIBYTE(ushort w) => (ushort)(w >> 8);
    internal static ushort LOBYTE(ushort w) => (ushort)(w & 0xFF);

    // --- Structs ---

    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_HDR
    {
        internal uint dbch_size;
        internal uint dbch_devicetype;
        internal uint dbch_reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_DEVICEINTERFACE_W
    {
        internal uint dbcc_size;
        internal uint dbcc_devicetype;
        internal uint dbcc_reserved;
        internal Guid dbcc_classguid;
        internal char dbcc_name; // first char of name array
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct OSVERSIONINFOEXW
    {
        internal uint dwOSVersionInfoSize;
        internal uint dwMajorVersion;
        internal uint dwMinorVersion;
        internal uint dwBuildNumber;
        internal uint dwPlatformId;
        internal fixed char szCSDVersion[128];
        internal ushort wServicePackMajor;
        internal ushort wServicePackMinor;
        internal ushort wSuiteMask;
        internal byte wProductType;
        internal byte wReserved;
    }

    // Helper: MAKEINTATOM equivalent -- cast ATOM (ushort) to char*
    internal static char* MAKEINTATOM(ushort atom)
    {
        return (char*)(nuint)atom;
    }
}

// =====================================================================
//  Win32PlatformConnector -- matches X11PlatformConnector pattern
// =====================================================================

public static unsafe class Win32PlatformConnector
{
    public static IGlfwPlatform? Connect(int platformId)
    {
        var win32 = new GlfwLibraryWin32();
        _glfw.Win32 = win32;

        return new Win32Platform();
    }
}

// =====================================================================
//  Win32Platform : IGlfwPlatform -- delegates to static Glfw._glfwXxxWin32
// =====================================================================

public partial class Win32Platform : IGlfwPlatform
{
    public int PlatformID => GlfwPlatformId.Win32;

    public bool Init() => Glfw.initWin32();
    public void Terminate() => Glfw.terminateWin32();

    // Input
    public void GetCursorPos(GlfwWindow window, out double xpos, out double ypos)
        => Glfw._glfwGetCursorPosWin32(window, out xpos, out ypos);
    public void SetCursorPos(GlfwWindow window, double xpos, double ypos)
        => Glfw._glfwSetCursorPosWin32(window, xpos, ypos);
    public void SetCursorMode(GlfwWindow window, int mode)
        => Glfw._glfwSetCursorModeWin32(window, mode);
    public void SetRawMouseMotion(GlfwWindow window, bool enabled)
        => Glfw._glfwSetRawMouseMotionWin32(window, enabled);
    public bool RawMouseMotionSupported()
        => Glfw._glfwRawMouseMotionSupportedWin32();
    public bool CreateCursor(GlfwCursor cursor, in GlfwImage image, int xhot, int yhot)
        => Glfw._glfwCreateCursorWin32(cursor, in image, xhot, yhot);
    public bool CreateStandardCursor(GlfwCursor cursor, int shape)
        => Glfw._glfwCreateStandardCursorWin32(cursor, shape);
    public void DestroyCursor(GlfwCursor cursor)
        => Glfw._glfwDestroyCursorWin32(cursor);
    public void SetCursor(GlfwWindow window, GlfwCursor? cursor)
        => Glfw._glfwSetCursorWin32(window, cursor);
    public string? GetScancodeName(int scancode)
        => Glfw._glfwGetScancodeNameWin32(scancode);
    public int GetKeyScancode(int key)
        => Glfw._glfwGetKeyScancodeWin32(key);
    public void SetClipboardString(string value)
        => Glfw._glfwSetClipboardStringWin32(value);
    public string? GetClipboardString()
        => Glfw._glfwGetClipboardStringWin32();

    // Joysticks
    public bool InitJoysticks()
        => Glfw._glfwInitJoysticksWin32();
    public void TerminateJoysticks()
        => Glfw._glfwTerminateJoysticksWin32();
    public bool PollJoystick(GlfwJoystick joystick, int mode) => Glfw._glfwPollJoystickWin32(joystick, mode);
    public string GetMappingName() => "Windows";
    public string UpdateGamepadGUID(string guid) => Glfw._glfwUpdateGamepadGUIDWin32(guid);

    // Monitor -- delegated to real implementations in win32_monitor.cs
    public void FreeMonitor(GlfwMonitor monitor)
        => Glfw._glfwFreeMonitorWin32(monitor);
    public void GetMonitorPos(GlfwMonitor monitor, out int xpos, out int ypos)
        => Glfw._glfwGetMonitorPosWin32(monitor, out xpos, out ypos);
    public void GetMonitorContentScale(GlfwMonitor monitor, out float xscale, out float yscale)
        => Glfw._glfwGetMonitorContentScaleWin32(monitor, out xscale, out yscale);
    public void GetMonitorWorkarea(GlfwMonitor monitor, out int xpos, out int ypos, out int width, out int height)
        => Glfw._glfwGetMonitorWorkareaWin32(monitor, out xpos, out ypos, out width, out height);
    public GlfwVidMode[]? GetVideoModes(GlfwMonitor monitor, out int count)
        => Glfw._glfwGetVideoModesWin32(monitor, out count);
    public bool GetVideoMode(GlfwMonitor monitor, out GlfwVidMode mode)
        => Glfw._glfwGetVideoModeWin32(monitor, out mode);
    public bool GetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
        => Glfw._glfwGetGammaRampWin32(monitor, ramp);
    public void SetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
        => Glfw._glfwSetGammaRampWin32(monitor, ramp);

    // Window
    public bool CreateWindow(GlfwWindow window, GlfwWndConfig wndconfig, GlfwCtxConfig ctxconfig, GlfwFbConfig fbconfig)
        => Glfw._glfwCreateWindowWin32(window, wndconfig, ctxconfig, fbconfig);
    public void DestroyWindow(GlfwWindow window)
        => Glfw._glfwDestroyWindowWin32(window);
    public void SetWindowTitle(GlfwWindow window, string title)
        => Glfw._glfwSetWindowTitleWin32(window, title);
    public void SetWindowIcon(GlfwWindow window, int count, GlfwImage[]? images)
        => Glfw._glfwSetWindowIconWin32(window, count, images);
    public void GetWindowPos(GlfwWindow window, out int xpos, out int ypos)
        => Glfw._glfwGetWindowPosWin32(window, out xpos, out ypos);
    public void SetWindowPos(GlfwWindow window, int xpos, int ypos)
        => Glfw._glfwSetWindowPosWin32(window, xpos, ypos);
    public void GetWindowSize(GlfwWindow window, out int width, out int height)
        => Glfw._glfwGetWindowSizeWin32(window, out width, out height);
    public void SetWindowSize(GlfwWindow window, int width, int height)
        => Glfw._glfwSetWindowSizeWin32(window, width, height);
    public void SetWindowSizeLimits(GlfwWindow window, int minwidth, int minheight, int maxwidth, int maxheight)
        => Glfw._glfwSetWindowSizeLimitsWin32(window, minwidth, minheight, maxwidth, maxheight);
    public void SetWindowAspectRatio(GlfwWindow window, int numer, int denom)
        => Glfw._glfwSetWindowAspectRatioWin32(window, numer, denom);
    public void GetFramebufferSize(GlfwWindow window, out int width, out int height)
        => Glfw._glfwGetFramebufferSizeWin32(window, out width, out height);
    public void GetWindowFrameSize(GlfwWindow window, out int left, out int top, out int right, out int bottom)
        => Glfw._glfwGetWindowFrameSizeWin32(window, out left, out top, out right, out bottom);
    public void GetWindowContentScale(GlfwWindow window, out float xscale, out float yscale)
        => Glfw._glfwGetWindowContentScaleWin32(window, out xscale, out yscale);
    public void IconifyWindow(GlfwWindow window)
        => Glfw._glfwIconifyWindowWin32(window);
    public void RestoreWindow(GlfwWindow window)
        => Glfw._glfwRestoreWindowWin32(window);
    public void MaximizeWindow(GlfwWindow window)
        => Glfw._glfwMaximizeWindowWin32(window);
    public void ShowWindow(GlfwWindow window)
        => Glfw._glfwShowWindowWin32(window);
    public void HideWindow(GlfwWindow window)
        => Glfw._glfwHideWindowWin32(window);
    public void RequestWindowAttention(GlfwWindow window)
        => Glfw._glfwRequestWindowAttentionWin32(window);
    public void FocusWindow(GlfwWindow window)
        => Glfw._glfwFocusWindowWin32(window);
    public void SetWindowMonitor(GlfwWindow window, GlfwMonitor? monitor, int xpos, int ypos, int width, int height, int refreshRate)
        => Glfw._glfwSetWindowMonitorWin32(window, monitor, xpos, ypos, width, height, refreshRate);
    public bool WindowFocused(GlfwWindow window)
        => Glfw._glfwWindowFocusedWin32(window);
    public bool WindowIconified(GlfwWindow window)
        => Glfw._glfwWindowIconifiedWin32(window);
    public bool WindowVisible(GlfwWindow window)
        => Glfw._glfwWindowVisibleWin32(window);
    public bool WindowMaximized(GlfwWindow window)
        => Glfw._glfwWindowMaximizedWin32(window);
    public bool WindowHovered(GlfwWindow window)
        => Glfw._glfwWindowHoveredWin32(window);
    public bool FramebufferTransparent(GlfwWindow window)
        => Glfw._glfwFramebufferTransparentWin32(window);
    public float GetWindowOpacity(GlfwWindow window)
        => Glfw._glfwGetWindowOpacityWin32(window);
    public void SetWindowResizable(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowResizableWin32(window, enabled);
    public void SetWindowDecorated(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowDecoratedWin32(window, enabled);
    public void SetWindowFloating(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowFloatingWin32(window, enabled);
    public void SetWindowOpacity(GlfwWindow window, float opacity)
        => Glfw._glfwSetWindowOpacityWin32(window, opacity);
    public void SetWindowMousePassthrough(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowMousePassthroughWin32(window, enabled);
    public void PollEvents()
        => Glfw._glfwPollEventsWin32();
    public void WaitEvents()
        => Glfw._glfwWaitEventsWin32();
    public void WaitEventsTimeout(double timeout)
        => Glfw._glfwWaitEventsTimeoutWin32(timeout);
    public void PostEmptyEvent()
        => Glfw._glfwPostEmptyEventWin32();
}

// =====================================================================
//  Static initialization functions in Glfw partial class
//  Ported from win32_init.c
// =====================================================================

public static unsafe partial class Glfw
{
    // GUID_DEVINTERFACE_HID
    private static readonly Guid _glfw_GUID_DEVINTERFACE_HID =
        new Guid(0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30);

    // -----------------------------------------------------------------
    //  loadLibraries -- Load necessary libraries (DLLs)
    //  Ported from win32_init.c loadLibraries()
    //
    //  Core libraries (user32, kernel32, gdi32, shell32) are loaded by
    //  Win32Native.LoadCoreLibraries() in win32_native.cs.
    //  Optional DLL function pointers are loaded into the typed
    //  GlfwLibraryWin32 fields via Win32Native helper methods.
    // -----------------------------------------------------------------

    private static bool loadLibrariesWin32()
    {
        var win32 = _glfw.Win32!;

        // Load core DLLs (user32, kernel32, gdi32, shell32) into Win32Native statics
        if (!Win32Native.LoadCoreLibraries())
        {
            // The loader has already unwound core DLLs, so error formatting
            // must not call through a released kernel32 function table.
            _glfwInputError(GLFW_PLATFORM_ERROR,
                "Win32: Failed to load core libraries or required entry points");
            return false;
        }

        // Get own module handle
        if (Win32Native.kernel32 != null)
        {
            win32.instance = Win32Native.kernel32.GetModuleHandleW(null);
        }

        if (win32.instance == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to retrieve own module handle");
            return false;
        }

        // Load optional user32 DPI functions into win32.user32
        win32.user32.instance = Win32Native.user32!.handle;
        Win32Native.LoadOptionalUser32Functions(win32);

        // Load dinput8.dll (optional)
        win32.dinput8.instance = _glfwPlatformLoadModule("dinput8.dll");
        if (win32.dinput8.instance != 0)
        {
            nint p = _glfwPlatformGetModuleSymbol(win32.dinput8.instance, "DirectInput8Create");
            win32.dinput8.Create = p;
        }

        // Try loading xinput in order of preference
        {
            string[] names = { "xinput1_4.dll", "xinput1_3.dll", "xinput9_1_0.dll", "xinput1_2.dll", "xinput1_1.dll" };

            for (int i = 0; i < names.Length; i++)
            {
                win32.xinput.instance = _glfwPlatformLoadModule(names[i]);
                if (win32.xinput.instance != 0)
                {
                    nint pCaps = _glfwPlatformGetModuleSymbol(win32.xinput.instance, "XInputGetCapabilities");
                    win32.xinput.GetCapabilities = (delegate* unmanaged[Stdcall]<uint, uint, nint, uint>)pCaps;

                    nint pState = _glfwPlatformGetModuleSymbol(win32.xinput.instance, "XInputGetState");
                    win32.xinput.GetState = (delegate* unmanaged[Stdcall]<uint, nint, uint>)pState;

                    break;
                }
            }
        }

        // Load dwmapi.dll (optional, Vista+)
        Win32Native.LoadDwmapi(win32);

        // Load shcore.dll (optional, Win8.1+)
        Win32Native.LoadShcore(win32);

        // Load ntdll.dll (optional)
        Win32Native.LoadNtdll(win32);

        return true;
    }

    // -----------------------------------------------------------------
    //  createKeyTables -- Create key code translation tables
    //  Ported from win32_init.c createKeyTables()
    // -----------------------------------------------------------------

    private static void createKeyTablesWin32()
    {
        var win32 = _glfw.Win32!;

        Array.Fill(win32.keycodes, (short)-1);
        Array.Fill(win32.scancodes, (short)-1);

        win32.keycodes[0x00B] = GLFW_KEY_0;
        win32.keycodes[0x002] = GLFW_KEY_1;
        win32.keycodes[0x003] = GLFW_KEY_2;
        win32.keycodes[0x004] = GLFW_KEY_3;
        win32.keycodes[0x005] = GLFW_KEY_4;
        win32.keycodes[0x006] = GLFW_KEY_5;
        win32.keycodes[0x007] = GLFW_KEY_6;
        win32.keycodes[0x008] = GLFW_KEY_7;
        win32.keycodes[0x009] = GLFW_KEY_8;
        win32.keycodes[0x00A] = GLFW_KEY_9;
        win32.keycodes[0x01E] = GLFW_KEY_A;
        win32.keycodes[0x030] = GLFW_KEY_B;
        win32.keycodes[0x02E] = GLFW_KEY_C;
        win32.keycodes[0x020] = GLFW_KEY_D;
        win32.keycodes[0x012] = GLFW_KEY_E;
        win32.keycodes[0x021] = GLFW_KEY_F;
        win32.keycodes[0x022] = GLFW_KEY_G;
        win32.keycodes[0x023] = GLFW_KEY_H;
        win32.keycodes[0x017] = GLFW_KEY_I;
        win32.keycodes[0x024] = GLFW_KEY_J;
        win32.keycodes[0x025] = GLFW_KEY_K;
        win32.keycodes[0x026] = GLFW_KEY_L;
        win32.keycodes[0x032] = GLFW_KEY_M;
        win32.keycodes[0x031] = GLFW_KEY_N;
        win32.keycodes[0x018] = GLFW_KEY_O;
        win32.keycodes[0x019] = GLFW_KEY_P;
        win32.keycodes[0x010] = GLFW_KEY_Q;
        win32.keycodes[0x013] = GLFW_KEY_R;
        win32.keycodes[0x01F] = GLFW_KEY_S;
        win32.keycodes[0x014] = GLFW_KEY_T;
        win32.keycodes[0x016] = GLFW_KEY_U;
        win32.keycodes[0x02F] = GLFW_KEY_V;
        win32.keycodes[0x011] = GLFW_KEY_W;
        win32.keycodes[0x02D] = GLFW_KEY_X;
        win32.keycodes[0x015] = GLFW_KEY_Y;
        win32.keycodes[0x02C] = GLFW_KEY_Z;

        win32.keycodes[0x028] = GLFW_KEY_APOSTROPHE;
        win32.keycodes[0x02B] = GLFW_KEY_BACKSLASH;
        win32.keycodes[0x033] = GLFW_KEY_COMMA;
        win32.keycodes[0x00D] = GLFW_KEY_EQUAL;
        win32.keycodes[0x029] = GLFW_KEY_GRAVE_ACCENT;
        win32.keycodes[0x01A] = GLFW_KEY_LEFT_BRACKET;
        win32.keycodes[0x00C] = GLFW_KEY_MINUS;
        win32.keycodes[0x034] = GLFW_KEY_PERIOD;
        win32.keycodes[0x01B] = GLFW_KEY_RIGHT_BRACKET;
        win32.keycodes[0x027] = GLFW_KEY_SEMICOLON;
        win32.keycodes[0x035] = GLFW_KEY_SLASH;
        win32.keycodes[0x056] = GLFW_KEY_WORLD_2;

        win32.keycodes[0x00E] = GLFW_KEY_BACKSPACE;
        win32.keycodes[0x153] = GLFW_KEY_DELETE;
        win32.keycodes[0x14F] = GLFW_KEY_END;
        win32.keycodes[0x01C] = GLFW_KEY_ENTER;
        win32.keycodes[0x001] = GLFW_KEY_ESCAPE;
        win32.keycodes[0x147] = GLFW_KEY_HOME;
        win32.keycodes[0x152] = GLFW_KEY_INSERT;
        win32.keycodes[0x15D] = GLFW_KEY_MENU;
        win32.keycodes[0x151] = GLFW_KEY_PAGE_DOWN;
        win32.keycodes[0x149] = GLFW_KEY_PAGE_UP;
        win32.keycodes[0x045] = GLFW_KEY_PAUSE;
        win32.keycodes[0x039] = GLFW_KEY_SPACE;
        win32.keycodes[0x00F] = GLFW_KEY_TAB;
        win32.keycodes[0x03A] = GLFW_KEY_CAPS_LOCK;
        win32.keycodes[0x145] = GLFW_KEY_NUM_LOCK;
        win32.keycodes[0x046] = GLFW_KEY_SCROLL_LOCK;
        win32.keycodes[0x03B] = GLFW_KEY_F1;
        win32.keycodes[0x03C] = GLFW_KEY_F2;
        win32.keycodes[0x03D] = GLFW_KEY_F3;
        win32.keycodes[0x03E] = GLFW_KEY_F4;
        win32.keycodes[0x03F] = GLFW_KEY_F5;
        win32.keycodes[0x040] = GLFW_KEY_F6;
        win32.keycodes[0x041] = GLFW_KEY_F7;
        win32.keycodes[0x042] = GLFW_KEY_F8;
        win32.keycodes[0x043] = GLFW_KEY_F9;
        win32.keycodes[0x044] = GLFW_KEY_F10;
        win32.keycodes[0x057] = GLFW_KEY_F11;
        win32.keycodes[0x058] = GLFW_KEY_F12;
        win32.keycodes[0x064] = GLFW_KEY_F13;
        win32.keycodes[0x065] = GLFW_KEY_F14;
        win32.keycodes[0x066] = GLFW_KEY_F15;
        win32.keycodes[0x067] = GLFW_KEY_F16;
        win32.keycodes[0x068] = GLFW_KEY_F17;
        win32.keycodes[0x069] = GLFW_KEY_F18;
        win32.keycodes[0x06A] = GLFW_KEY_F19;
        win32.keycodes[0x06B] = GLFW_KEY_F20;
        win32.keycodes[0x06C] = GLFW_KEY_F21;
        win32.keycodes[0x06D] = GLFW_KEY_F22;
        win32.keycodes[0x06E] = GLFW_KEY_F23;
        win32.keycodes[0x076] = GLFW_KEY_F24;
        win32.keycodes[0x038] = GLFW_KEY_LEFT_ALT;
        win32.keycodes[0x01D] = GLFW_KEY_LEFT_CONTROL;
        win32.keycodes[0x02A] = GLFW_KEY_LEFT_SHIFT;
        win32.keycodes[0x15B] = GLFW_KEY_LEFT_SUPER;
        win32.keycodes[0x137] = GLFW_KEY_PRINT_SCREEN;
        win32.keycodes[0x138] = GLFW_KEY_RIGHT_ALT;
        win32.keycodes[0x11D] = GLFW_KEY_RIGHT_CONTROL;
        win32.keycodes[0x036] = GLFW_KEY_RIGHT_SHIFT;
        win32.keycodes[0x15C] = GLFW_KEY_RIGHT_SUPER;
        win32.keycodes[0x150] = GLFW_KEY_DOWN;
        win32.keycodes[0x14B] = GLFW_KEY_LEFT;
        win32.keycodes[0x14D] = GLFW_KEY_RIGHT;
        win32.keycodes[0x148] = GLFW_KEY_UP;

        win32.keycodes[0x052] = GLFW_KEY_KP_0;
        win32.keycodes[0x04F] = GLFW_KEY_KP_1;
        win32.keycodes[0x050] = GLFW_KEY_KP_2;
        win32.keycodes[0x051] = GLFW_KEY_KP_3;
        win32.keycodes[0x04B] = GLFW_KEY_KP_4;
        win32.keycodes[0x04C] = GLFW_KEY_KP_5;
        win32.keycodes[0x04D] = GLFW_KEY_KP_6;
        win32.keycodes[0x047] = GLFW_KEY_KP_7;
        win32.keycodes[0x048] = GLFW_KEY_KP_8;
        win32.keycodes[0x049] = GLFW_KEY_KP_9;
        win32.keycodes[0x04E] = GLFW_KEY_KP_ADD;
        win32.keycodes[0x053] = GLFW_KEY_KP_DECIMAL;
        win32.keycodes[0x135] = GLFW_KEY_KP_DIVIDE;
        win32.keycodes[0x11C] = GLFW_KEY_KP_ENTER;
        win32.keycodes[0x059] = GLFW_KEY_KP_EQUAL;
        win32.keycodes[0x037] = GLFW_KEY_KP_MULTIPLY;
        win32.keycodes[0x04A] = GLFW_KEY_KP_SUBTRACT;

        for (int scancode = 0; scancode < 512; scancode++)
        {
            if (win32.keycodes[scancode] > 0)
                win32.scancodes[win32.keycodes[scancode]] = (short)scancode;
        }
    }

    // -----------------------------------------------------------------
    //  helperWindowProc -- Window procedure for the hidden helper window
    //  Ported from win32_init.c helperWindowProc()
    // -----------------------------------------------------------------

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static nint helperWindowProcWin32(nint hWnd, uint uMsg, nuint wParam, nint lParam)
    {
        switch (uMsg)
        {
            case WM_DISPLAYCHANGE:
                _glfwPollMonitorsWin32();
                break;

            case WM_DEVICECHANGE:
            {
                if (!_glfw.joysticksInitialized)
                    break;

                if (wParam == (nuint)DBT_DEVICEARRIVAL)
                {
                    Win32InitPInvoke.DEV_BROADCAST_HDR* dbh = (Win32InitPInvoke.DEV_BROADCAST_HDR*)lParam;
                    if (dbh != null && dbh->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                        _glfwDetectJoystickConnectionWin32();
                }
                else if (wParam == (nuint)DBT_DEVICEREMOVECOMPLETE)
                {
                    Win32InitPInvoke.DEV_BROADCAST_HDR* dbh = (Win32InitPInvoke.DEV_BROADCAST_HDR*)lParam;
                    if (dbh != null && dbh->dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                        _glfwDetectJoystickDisconnectionWin32();
                }

                break;
            }
        }

        return Win32Native.user32!.DefWindowProcW(hWnd, uMsg, wParam, lParam);
    }

    // -----------------------------------------------------------------
    //  createHelperWindow -- Creates a dummy window for behind-the-scenes work
    //  Ported from win32_init.c createHelperWindow()
    // -----------------------------------------------------------------

    private static bool createHelperWindowWin32()
    {
        var win32 = _glfw.Win32!;

        WNDCLASSEXW wc = default;
        wc.cbSize = (uint)sizeof(WNDCLASSEXW);
        wc.style = CS_OWNDC;
        wc.lpfnWndProc = (delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint>)&helperWindowProcWin32;
        wc.hInstance = win32.instance;

        fixed (char* className = "GLFW3 Helper")
        {
            wc.lpszClassName = className;

            win32.helperWindowClass = Win32Native.user32!.RegisterClassExW(&wc);
            if (win32.helperWindowClass == 0)
            {
                _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                    "Win32: Failed to register helper window class");
                return false;
            }
        }

        fixed (char* windowName = "GLFW message window")
        {
            win32.helperWindowHandle =
                Win32Native.user32!.CreateWindowExW(
                    WS_EX_OVERLAPPEDWINDOW,
                    Win32InitPInvoke.MAKEINTATOM(win32.helperWindowClass),
                    windowName,
                    WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
                    0, 0, 1, 1,
                    0, 0,
                    win32.instance,
                    0);
        }

        if (win32.helperWindowHandle == 0)
        {
            _glfwInputErrorWin32(GLFW_PLATFORM_ERROR,
                "Win32: Failed to create helper window");
            return false;
        }

        // HACK: The command to the first ShowWindow call is ignored if the parent
        //       process passed along a STARTUPINFO, so clear that with a no-op call
        Win32Native.user32!.ShowWindow(win32.helperWindowHandle, SW_HIDE);

        // Register for HID device notifications
        {
            Win32InitPInvoke.DEV_BROADCAST_DEVICEINTERFACE_W dbi = default;
            dbi.dbcc_size = (uint)sizeof(Win32InitPInvoke.DEV_BROADCAST_DEVICEINTERFACE_W);
            dbi.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            dbi.dbcc_classguid = _glfw_GUID_DEVINTERFACE_HID;

            win32.deviceNotificationHandle =
                Win32Native.user32!.RegisterDeviceNotificationW(
                    win32.helperWindowHandle,
                    &dbi,
                    Win32InitPInvoke.DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        MSG msg;
        while (Win32Native.user32!.PeekMessageW(&msg, win32.helperWindowHandle, 0, 0, PM_REMOVE) != 0)
        {
            Win32Native.user32!.TranslateMessage(&msg);
            Win32Native.user32!.DispatchMessageW(&msg);
        }

        return true;
    }

    // -----------------------------------------------------------------
    //  GLFW internal API
    // -----------------------------------------------------------------

    /// <summary>
    /// Reports the specified error, appending information about the last Win32 error.
    /// Ported from win32_init.c _glfwInputErrorWin32().
    /// </summary>
    internal static void _glfwInputErrorWin32(int error, string description)
    {
        uint lastError = Win32Native.kernel32!.GetLastError() & 0xffff;

        char* buffer = stackalloc char[GlfwConstants._GLFW_MESSAGE_SIZE];
        Win32Native.kernel32!.FormatMessageW(
            Win32InitPInvoke.FORMAT_MESSAGE_FROM_SYSTEM |
            Win32InitPInvoke.FORMAT_MESSAGE_IGNORE_INSERTS |
            Win32InitPInvoke.FORMAT_MESSAGE_MAX_WIDTH_MASK,
            0,
            lastError,
            Win32InitPInvoke.MAKELANGID(Win32InitPInvoke.LANG_NEUTRAL, Win32InitPInvoke.SUBLANG_DEFAULT),
            buffer,
            (uint)GlfwConstants._GLFW_MESSAGE_SIZE,
            0);

        string win32Message = new string(buffer);
        _glfwInputError(error, "{0}: {1}", description, win32Message);
    }

    /// <summary>
    /// Updates key names according to the current keyboard layout.
    /// Ported from win32_init.c _glfwUpdateKeyNamesWin32().
    /// </summary>
#pragma warning disable CA2014 // stackalloc in loop -- porting C code pattern, bounded allocation
    internal static void _glfwUpdateKeyNamesWin32()
    {
        var win32 = _glfw.Win32!;
        byte* state = stackalloc byte[256];
        new Span<byte>(state, 256).Clear();

        for (int i = 0; i < win32.keynames.Length; i++)
            win32.keynames[i] = null;

        for (int key = GLFW_KEY_SPACE; key <= GLFW_KEY_LAST; key++)
        {
            uint vk;
            int scancode, length;
            char* chars = stackalloc char[16];

            scancode = win32.scancodes[key];
            if (scancode == -1)
                continue;

            if (key >= GLFW_KEY_KP_0 && key <= GLFW_KEY_KP_ADD)
            {
                ReadOnlySpan<uint> vks = stackalloc uint[]
                {
                    (uint)VK_NUMPAD0,  (uint)VK_NUMPAD1,  (uint)VK_NUMPAD2,  (uint)VK_NUMPAD3,
                    (uint)VK_NUMPAD4,  (uint)VK_NUMPAD5,  (uint)VK_NUMPAD6,  (uint)VK_NUMPAD7,
                    (uint)VK_NUMPAD8,  (uint)VK_NUMPAD9,  (uint)VK_DECIMAL,  (uint)VK_DIVIDE,
                    (uint)VK_MULTIPLY, (uint)VK_SUBTRACT, (uint)VK_ADD
                };

                vk = vks[key - GLFW_KEY_KP_0];
            }
            else
            {
                vk = Win32Native.user32!.MapVirtualKeyW((uint)scancode, MAPVK_VSC_TO_VK);
            }

            length = Win32Native.user32!.ToUnicode(vk, (uint)scancode, state, chars, 16, 0);

            if (length == -1)
            {
                // This is a dead key, so we need a second simulated key press
                // to make it output its own character (usually a diacritic)
                length = Win32Native.user32!.ToUnicode(vk, (uint)scancode, state, chars, 16, 0);
            }

            if (length < 1)
                continue;

            // Convert the first wide char to a managed string
            win32.keynames[key] = new string(chars, 0, 1);
        }
    }
#pragma warning restore CA2014

    /// <summary>
    /// Replacement for IsWindowsVersionOrGreater, as we cannot rely on the
    /// application having a correct embedded manifest.
    /// Ported from win32_init.c _glfwIsWindowsVersionOrGreaterWin32().
    /// </summary>
    internal static bool _glfwIsWindowsVersionOrGreaterWin32(ushort major, ushort minor, ushort sp)
    {
        var win32 = _glfw.Win32!;

        Win32InitPInvoke.OSVERSIONINFOEXW osvi = default;
        osvi.dwOSVersionInfoSize = (uint)sizeof(Win32InitPInvoke.OSVERSIONINFOEXW);
        osvi.dwMajorVersion = major;
        osvi.dwMinorVersion = minor;
        osvi.wServicePackMajor = sp;

        uint mask = VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR;
        ulong cond = Win32Native.kernel32!.VerSetConditionMask(0, VER_MAJORVERSION, (byte)VER_GREATER_EQUAL);
        cond = Win32Native.kernel32!.VerSetConditionMask(cond, VER_MINORVERSION, (byte)VER_GREATER_EQUAL);
        cond = Win32Native.kernel32!.VerSetConditionMask(cond, VER_SERVICEPACKMAJOR, (byte)VER_GREATER_EQUAL);

        // HACK: Use RtlVerifyVersionInfo instead of VerifyVersionInfoW as the
        //       latter lies unless the user knew to embed a non-default manifest
        //       announcing support for Windows 10 via supportedOS GUID
        if (win32.ntdll.RtlVerifyVersionInfo_ != null)
        {
            return win32.ntdll.RtlVerifyVersionInfo_((nint)(&osvi), mask, cond) == 0;
        }

        // Fallback: use VerifyVersionInfoW directly (may lie on Win10+)
        return false;
    }

    /// <summary>
    /// Checks whether we are on at least the specified build of Windows 10.
    /// Ported from win32_init.c _glfwIsWindows10BuildOrGreaterWin32().
    /// </summary>
    internal static bool _glfwIsWindows10BuildOrGreaterWin32(ushort build)
    {
        var win32 = _glfw.Win32!;

        Win32InitPInvoke.OSVERSIONINFOEXW osvi = default;
        osvi.dwOSVersionInfoSize = (uint)sizeof(Win32InitPInvoke.OSVERSIONINFOEXW);
        osvi.dwMajorVersion = 10;
        osvi.dwMinorVersion = 0;
        osvi.dwBuildNumber = build;

        uint mask = VER_MAJORVERSION | VER_MINORVERSION | VER_BUILDNUMBER;
        ulong cond = Win32Native.kernel32!.VerSetConditionMask(0, VER_MAJORVERSION, (byte)VER_GREATER_EQUAL);
        cond = Win32Native.kernel32!.VerSetConditionMask(cond, VER_MINORVERSION, (byte)VER_GREATER_EQUAL);
        cond = Win32Native.kernel32!.VerSetConditionMask(cond, VER_BUILDNUMBER, (byte)VER_GREATER_EQUAL);

        // HACK: Use RtlVerifyVersionInfo instead of VerifyVersionInfoW
        if (win32.ntdll.RtlVerifyVersionInfo_ != null)
        {
            return win32.ntdll.RtlVerifyVersionInfo_((nint)(&osvi), mask, cond) == 0;
        }

        return false;
    }

    /// <summary>
    /// Windows 10 Anniversary Update (build 14393) or greater.
    /// Ported from win32_platform.h macro.
    /// </summary>
    internal static bool _glfwIsWindows10Version1607OrGreaterWin32()
        => _glfwIsWindows10BuildOrGreaterWin32(14393)
           && _glfw.Win32 != null
           && (nint)_glfw.Win32.user32.AdjustWindowRectExForDpi_ != 0;

    /// <summary>
    /// Windows 10 Creators Update (build 15063) or greater.
    /// Ported from win32_platform.h macro.
    /// </summary>
    internal static bool _glfwIsWindows10Version1703OrGreaterWin32()
        => _glfwIsWindows10BuildOrGreaterWin32(15063)
           && _glfw.Win32 != null
           && (nint)_glfw.Win32.user32.AdjustWindowRectExForDpi_ != 0;

    /// <summary>
    /// Windows 8 or greater.
    /// Ported from win32_platform.h IsWindows8OrGreater() macro.
    /// </summary>
    internal static bool _glfwIsWindows8OrGreaterWin32()
        => _glfwIsWindowsVersionOrGreaterWin32(
            Win32InitPInvoke.HIBYTE(Win32InitPInvoke._WIN32_WINNT_WIN8),
            Win32InitPInvoke.LOBYTE(Win32InitPInvoke._WIN32_WINNT_WIN8), 0);

    /// <summary>
    /// Windows 8.1 or greater.
    /// Ported from win32_platform.h IsWindows8Point1OrGreater() macro.
    /// </summary>
    internal static bool _glfwIsWindows8Point1OrGreaterWin32()
        => _glfwIsWindowsVersionOrGreaterWin32(
            Win32InitPInvoke.HIBYTE(Win32InitPInvoke._WIN32_WINNT_WINBLUE),
            Win32InitPInvoke.LOBYTE(Win32InitPInvoke._WIN32_WINNT_WINBLUE), 0);

    /// <summary>
    /// Returns a managed string from a wide char pointer.
    /// Ported from win32_init.c _glfwCreateUTF8FromWideStringWin32().
    /// In C#, strings are already Unicode -- this is mostly for P/Invoke interop.
    /// </summary>
    internal static string? _glfwCreateUTF8FromWideStringWin32(char* source)
    {
        if (source == null)
            return null;
        return new string(source);
    }

    /// <summary>
    /// Returns an unmanaged wide string copy of the specified string.
    /// Ported from win32_init.c _glfwCreateWideStringFromUTF8Win32().
    /// </summary>
    internal static char* _glfwCreateWideStringFromUTF8Win32(string source)
    {
        nint ptr = Marshal.StringToHGlobalUni(source);
        return (char*)ptr;
    }

    // -----------------------------------------------------------------
    //  initWin32 -- Main initialization
    //  Ported from win32_init.c _glfwInitWin32()
    // -----------------------------------------------------------------

    internal static bool initWin32()
    {
        var win32 = _glfw.Win32!;

        if (!loadLibrariesWin32())
            return false;

        createKeyTablesWin32();
        _glfwUpdateKeyNamesWin32();

        if (_glfwIsWindows10Version1703OrGreaterWin32())
        {
            // SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2)
            if (win32.user32.SetProcessDpiAwarenessContext_ != null)
            {
                win32.user32.SetProcessDpiAwarenessContext_(
                    DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            }
        }
        else if (_glfwIsWindows8Point1OrGreaterWin32())
        {
            // SetProcessDpiAwareness(PROCESS_PER_MONITOR_DPI_AWARE)
            if (win32.shcore.SetProcessDpiAwareness_ != null)
            {
                win32.shcore.SetProcessDpiAwareness_(
                    (int)PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
        }
        else
        {
            // Fallback for older Windows
            if (Win32Native.user32 != null && Win32Native.user32.SetProcessDPIAware != null)
            {
                Win32Native.user32.SetProcessDPIAware();
            }
        }

        if (!createHelperWindowWin32())
            return false;

        _glfwPollMonitorsWin32();
        return true;
    }

    // -----------------------------------------------------------------
    //  terminateWin32 -- Cleanup
    //  Ported from win32_init.c _glfwTerminateWin32()
    // -----------------------------------------------------------------

    internal static void terminateWin32()
    {
        var win32 = _glfw.Win32;
        if (win32 == null)
            return;

        if (win32.blankCursor != 0)
        {
            Win32Native.user32!.DestroyIcon(win32.blankCursor);
            win32.blankCursor = 0;
        }

        if (win32.deviceNotificationHandle != 0)
        {
            Win32Native.user32!.UnregisterDeviceNotification(win32.deviceNotificationHandle);
            win32.deviceNotificationHandle = 0;
        }

        if (win32.helperWindowHandle != 0)
        {
            Win32Native.user32!.DestroyWindow(win32.helperWindowHandle);
            win32.helperWindowHandle = 0;
        }

        if (win32.helperWindowClass != 0)
        {
            Win32Native.user32!.UnregisterClassW(
                Win32InitPInvoke.MAKEINTATOM(win32.helperWindowClass), win32.instance);
            win32.helperWindowClass = 0;
        }

        if (win32.mainWindowClass != 0)
        {
            Win32Native.user32!.UnregisterClassW(
                Win32InitPInvoke.MAKEINTATOM(win32.mainWindowClass), win32.instance);
            win32.mainWindowClass = 0;
        }

        win32.clipboardString = null;
        win32.rawInput = null;

        _glfwTerminateWGL();
        _glfwTerminateEGL();
        _glfwTerminateOSMesa();

        // One owner per acquired module reference, including partial init.
        // win32.user32.instance is a borrowed alias of the core user32 load.
        // win32.instance (GetModuleHandleW) is borrowed from the process.
        // FreeLibraries owns both core loads and independently loaded optional DLLs.
        Win32Native.FreeLibraries(win32);

        _glfw.Win32 = null;
    }


}

// =====================================================================
//  Extension of _glfw to hold joystick initialization state
// =====================================================================

public static partial class _glfw
{
    public static bool joysticksInitialized;
}

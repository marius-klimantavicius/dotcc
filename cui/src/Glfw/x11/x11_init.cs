// Ported from glfw/src/x11_init.c (GLFW 3.6)
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
using static Glfw.X11Constants;

namespace Glfw;

// =====================================================================
//  X11PlatformConnector -- matches NullPlatformConnector pattern
// =====================================================================

public static unsafe class X11PlatformConnector
{
    // LC_CTYPE is zero on the Linux platforms registered by this port.
    [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
    private static extern nint setlocale(int category, byte* locale);

    public static IGlfwPlatform? Connect(int platformId)
    {
        // Match _glfwConnectX11 before any Xlib/XIM initialization.  The C
        // locale is process-global; retain an application's non-C selection.
        if (Marshal.PtrToStringUTF8(setlocale(0, null)) == "C")
        {
            byte empty = 0;
            setlocale(0, &empty);
        }

        var x11 = new GlfwLibraryX11();
        _glfw.X11 = x11;

        // Load libX11.so.6
        if (!X11Native.LoadXlib(x11))
        {
            if (platformId == GlfwPlatformId.X11)
                Glfw._glfwInputError(GLFW_PLATFORM_ERROR, "X11: Failed to load Xlib");

            X11Native.FreeLibraries(x11);
            _glfw.X11 = null;
            return null;
        }

        // Load libc
        X11Native.LoadLibc(x11);

        // XInitThreads + XrmInitialize (called from LoadXlib already populated function pointers)
        if (x11.xlib.InitThreads != null)
            x11.xlib.InitThreads();

        if (x11.xrm.Initialize != null)
            x11.xrm.Initialize();

        // Open the X display
        nint display = x11.xlib.OpenDisplay(null);
        if (display == 0)
        {
            if (platformId == GlfwPlatformId.X11)
            {
                string? name = Environment.GetEnvironmentVariable("DISPLAY");
                if (name != null)
                {
                    Glfw._glfwInputError(GLFW_PLATFORM_UNAVAILABLE,
                        "X11: Failed to open display " + name);
                }
                else
                {
                    Glfw._glfwInputError(GLFW_PLATFORM_UNAVAILABLE,
                        "X11: The DISPLAY environment variable is missing");
                }
            }

            X11Native.FreeLibraries(x11);
            _glfw.X11 = null;
            return null;
        }

        x11.display = display;

        return new X11Platform();
    }
}

// =====================================================================
//  X11Platform : IGlfwPlatform -- delegates to static Glfw._glfwXxxX11
// =====================================================================

public partial class X11Platform : IGlfwPlatform
{
    public int PlatformID => GlfwPlatformId.X11;

    public bool Init() => Glfw.initX11();
    public void Terminate() => Glfw.terminateX11();

    // Input
    public void GetCursorPos(GlfwWindow window, out double xpos, out double ypos)
        => Glfw._glfwGetCursorPosX11(window, out xpos, out ypos);
    public void SetCursorPos(GlfwWindow window, double xpos, double ypos)
        => Glfw._glfwSetCursorPosX11(window, xpos, ypos);
    public void SetCursorMode(GlfwWindow window, int mode)
        => Glfw._glfwSetCursorModeX11(window, mode);
    public void SetRawMouseMotion(GlfwWindow window, bool enabled)
        => Glfw._glfwSetRawMouseMotionX11(window, enabled);
    public bool RawMouseMotionSupported()
        => Glfw._glfwRawMouseMotionSupportedX11();
    public bool CreateCursor(GlfwCursor cursor, in GlfwImage image, int xhot, int yhot)
        => Glfw._glfwCreateCursorX11(cursor, in image, xhot, yhot);
    public bool CreateStandardCursor(GlfwCursor cursor, int shape)
        => Glfw._glfwCreateStandardCursorX11(cursor, shape);
    public void DestroyCursor(GlfwCursor cursor)
        => Glfw._glfwDestroyCursorX11(cursor);
    public void SetCursor(GlfwWindow window, GlfwCursor? cursor)
        => Glfw._glfwSetCursorX11(window, cursor);
    public string? GetScancodeName(int scancode)
        => Glfw._glfwGetScancodeNameX11(scancode);
    public int GetKeyScancode(int key)
        => Glfw._glfwGetKeyScancodeX11(key);
    public void SetClipboardString(string value)
        => Glfw._glfwSetClipboardStringX11(value);
    public string? GetClipboardString()
        => Glfw._glfwGetClipboardStringX11();

    public bool InitJoysticks() => Glfw._glfwInitJoysticksLinux();
    public void TerminateJoysticks() => Glfw._glfwTerminateJoysticksLinux();
    public bool PollJoystick(GlfwJoystick joystick, int mode) => Glfw._glfwPollJoystickLinux(joystick, mode);
    public string GetMappingName() => "Linux";
    public string UpdateGamepadGUID(string guid) => guid;

    // Monitor
    public void FreeMonitor(GlfwMonitor monitor)
        => Glfw._glfwFreeMonitorX11(monitor);
    public void GetMonitorPos(GlfwMonitor monitor, out int xpos, out int ypos)
        => Glfw._glfwGetMonitorPosX11(monitor, out xpos, out ypos);
    public void GetMonitorContentScale(GlfwMonitor monitor, out float xscale, out float yscale)
        => Glfw._glfwGetMonitorContentScaleX11(monitor, out xscale, out yscale);
    public void GetMonitorWorkarea(GlfwMonitor monitor, out int xpos, out int ypos, out int width, out int height)
        => Glfw._glfwGetMonitorWorkareaX11(monitor, out xpos, out ypos, out width, out height);
    public GlfwVidMode[]? GetVideoModes(GlfwMonitor monitor, out int count)
        => Glfw._glfwGetVideoModesX11(monitor, out count);
    public bool GetVideoMode(GlfwMonitor monitor, out GlfwVidMode mode)
        => Glfw._glfwGetVideoModeX11(monitor, out mode);
    public bool GetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
        => Glfw._glfwGetGammaRampX11(monitor, ramp);
    public void SetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
        => Glfw._glfwSetGammaRampX11(monitor, ramp);

    // Window
    public bool CreateWindow(GlfwWindow window, GlfwWndConfig wndconfig, GlfwCtxConfig ctxconfig, GlfwFbConfig fbconfig)
        => Glfw._glfwCreateWindowX11(window, wndconfig, ctxconfig, fbconfig);
    public void DestroyWindow(GlfwWindow window)
        => Glfw._glfwDestroyWindowX11(window);
    public void SetWindowTitle(GlfwWindow window, string title)
        => Glfw._glfwSetWindowTitleX11(window, title);
    public void SetWindowIcon(GlfwWindow window, int count, GlfwImage[]? images)
        => Glfw._glfwSetWindowIconX11(window, count, images);
    public void GetWindowPos(GlfwWindow window, out int xpos, out int ypos)
        => Glfw._glfwGetWindowPosX11(window, out xpos, out ypos);
    public void SetWindowPos(GlfwWindow window, int xpos, int ypos)
        => Glfw._glfwSetWindowPosX11(window, xpos, ypos);
    public void GetWindowSize(GlfwWindow window, out int width, out int height)
        => Glfw._glfwGetWindowSizeX11(window, out width, out height);
    public void SetWindowSize(GlfwWindow window, int width, int height)
        => Glfw._glfwSetWindowSizeX11(window, width, height);
    public void SetWindowSizeLimits(GlfwWindow window, int minwidth, int minheight, int maxwidth, int maxheight)
        => Glfw._glfwSetWindowSizeLimitsX11(window, minwidth, minheight, maxwidth, maxheight);
    public void SetWindowAspectRatio(GlfwWindow window, int numer, int denom)
        => Glfw._glfwSetWindowAspectRatioX11(window, numer, denom);
    public void GetFramebufferSize(GlfwWindow window, out int width, out int height)
        => Glfw._glfwGetFramebufferSizeX11(window, out width, out height);
    public void GetWindowFrameSize(GlfwWindow window, out int left, out int top, out int right, out int bottom)
        => Glfw._glfwGetWindowFrameSizeX11(window, out left, out top, out right, out bottom);
    public void GetWindowContentScale(GlfwWindow window, out float xscale, out float yscale)
        => Glfw._glfwGetWindowContentScaleX11(window, out xscale, out yscale);
    public void IconifyWindow(GlfwWindow window)
        => Glfw._glfwIconifyWindowX11(window);
    public void RestoreWindow(GlfwWindow window)
        => Glfw._glfwRestoreWindowX11(window);
    public void MaximizeWindow(GlfwWindow window)
        => Glfw._glfwMaximizeWindowX11(window);
    public void ShowWindow(GlfwWindow window)
        => Glfw._glfwShowWindowX11(window);
    public void HideWindow(GlfwWindow window)
        => Glfw._glfwHideWindowX11(window);
    public void RequestWindowAttention(GlfwWindow window)
        => Glfw._glfwRequestWindowAttentionX11(window);
    public void FocusWindow(GlfwWindow window)
        => Glfw._glfwFocusWindowX11(window);
    public void SetWindowMonitor(GlfwWindow window, GlfwMonitor? monitor, int xpos, int ypos, int width, int height, int refreshRate)
        => Glfw._glfwSetWindowMonitorX11(window, monitor, xpos, ypos, width, height, refreshRate);
    public bool WindowFocused(GlfwWindow window)
        => Glfw._glfwWindowFocusedX11(window);
    public bool WindowIconified(GlfwWindow window)
        => Glfw._glfwWindowIconifiedX11(window);
    public bool WindowVisible(GlfwWindow window)
        => Glfw._glfwWindowVisibleX11(window);
    public bool WindowMaximized(GlfwWindow window)
        => Glfw._glfwWindowMaximizedX11(window);
    public bool WindowHovered(GlfwWindow window)
        => Glfw._glfwWindowHoveredX11(window);
    public bool FramebufferTransparent(GlfwWindow window)
        => Glfw._glfwFramebufferTransparentX11(window);
    public float GetWindowOpacity(GlfwWindow window)
        => Glfw._glfwGetWindowOpacityX11(window);
    public void SetWindowResizable(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowResizableX11(window, enabled);
    public void SetWindowDecorated(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowDecoratedX11(window, enabled);
    public void SetWindowFloating(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowFloatingX11(window, enabled);
    public void SetWindowOpacity(GlfwWindow window, float opacity)
        => Glfw._glfwSetWindowOpacityX11(window, opacity);
    public void SetWindowMousePassthrough(GlfwWindow window, bool enabled)
        => Glfw._glfwSetWindowMousePassthroughX11(window, enabled);
    public void PollEvents()
        => Glfw._glfwPollEventsX11();
    public void WaitEvents()
        => Glfw._glfwWaitEventsX11();
    public void WaitEventsTimeout(double timeout)
        => Glfw._glfwWaitEventsTimeoutX11(timeout);
    public void PostEmptyEvent()
        => Glfw._glfwPostEmptyEventX11();
}

// =====================================================================
//  Static initialization functions in Glfw partial class
// =====================================================================

public static unsafe partial class Glfw
{
    // -----------------------------------------------------------------
    //  X11 KeySym constants needed by translateKeySyms / createKeyTables
    // -----------------------------------------------------------------

    // Miscellaneous
    private const nuint XK_Escape       = 0xff1b;
    private const nuint XK_Tab          = 0xff09;
    private const nuint XK_Shift_L      = 0xffe1;
    private const nuint XK_Shift_R      = 0xffe2;
    private const nuint XK_Control_L    = 0xffe3;
    private const nuint XK_Control_R    = 0xffe4;
    private const nuint XK_Meta_L       = 0xffe7;
    private const nuint XK_Meta_R       = 0xffe8;
    private const nuint XK_Alt_L        = 0xffe9;
    private const nuint XK_Alt_R        = 0xffea;
    private const nuint XK_Super_L      = 0xffeb;
    private const nuint XK_Super_R      = 0xffec;
    private const nuint XK_Menu         = 0xff67;
    private const nuint XK_Num_Lock     = 0xff7f;
    private const nuint XK_Caps_Lock    = 0xffe5;
    private const nuint XK_Print        = 0xff61;
    private const nuint XK_Scroll_Lock  = 0xff14;
    private const nuint XK_Pause        = 0xff13;
    private const nuint XK_Delete       = 0xffff;
    private const nuint XK_BackSpace    = 0xff08;
    private const nuint XK_Return       = 0xff0d;
    private const nuint XK_Home         = 0xff50;
    private const nuint XK_End          = 0xff57;
    private const nuint XK_Page_Up      = 0xff55;
    private const nuint XK_Page_Down    = 0xff56;
    private const nuint XK_Insert       = 0xff63;
    private const nuint XK_Left         = 0xff51;
    private const nuint XK_Right        = 0xff53;
    private const nuint XK_Down         = 0xff54;
    private const nuint XK_Up           = 0xff52;
    private const nuint XK_F1           = 0xffbe;
    private const nuint XK_F2           = 0xffbf;
    private const nuint XK_F3           = 0xffc0;
    private const nuint XK_F4           = 0xffc1;
    private const nuint XK_F5           = 0xffc2;
    private const nuint XK_F6           = 0xffc3;
    private const nuint XK_F7           = 0xffc4;
    private const nuint XK_F8           = 0xffc5;
    private const nuint XK_F9           = 0xffc6;
    private const nuint XK_F10          = 0xffc7;
    private const nuint XK_F11          = 0xffc8;
    private const nuint XK_F12          = 0xffc9;
    private const nuint XK_F13          = 0xffca;
    private const nuint XK_F14          = 0xffcb;
    private const nuint XK_F15          = 0xffcc;
    private const nuint XK_F16          = 0xffcd;
    private const nuint XK_F17          = 0xffce;
    private const nuint XK_F18          = 0xffcf;
    private const nuint XK_F19          = 0xffd0;
    private const nuint XK_F20          = 0xffd1;
    private const nuint XK_F21          = 0xffd2;
    private const nuint XK_F22          = 0xffd3;
    private const nuint XK_F23          = 0xffd4;
    private const nuint XK_F24          = 0xffd5;
    private const nuint XK_F25          = 0xffd6;

    // Numeric keypad
    private const nuint XK_KP_0        = 0xffb0;
    private const nuint XK_KP_1        = 0xffb1;
    private const nuint XK_KP_2        = 0xffb2;
    private const nuint XK_KP_3        = 0xffb3;
    private const nuint XK_KP_4        = 0xffb4;
    private const nuint XK_KP_5        = 0xffb5;
    private const nuint XK_KP_6        = 0xffb6;
    private const nuint XK_KP_7        = 0xffb7;
    private const nuint XK_KP_8        = 0xffb8;
    private const nuint XK_KP_9        = 0xffb9;
    private const nuint XK_KP_Separator = 0xffac;
    private const nuint XK_KP_Decimal  = 0xffae;
    private const nuint XK_KP_Equal    = 0xffbd;
    private const nuint XK_KP_Enter    = 0xff8d;
    private const nuint XK_KP_Divide   = 0xffaf;
    private const nuint XK_KP_Multiply = 0xffaa;
    private const nuint XK_KP_Subtract = 0xffad;
    private const nuint XK_KP_Add      = 0xffab;

    // These are the primary keysym fallbacks (with NumLock off)
    private const nuint XK_KP_Insert    = 0xff9e;
    private const nuint XK_KP_End       = 0xff9c;
    private const nuint XK_KP_Down      = 0xff99;
    private const nuint XK_KP_Page_Down = 0xff9b;
    private const nuint XK_KP_Left      = 0xff96;
    private const nuint XK_KP_Right     = 0xff98;
    private const nuint XK_KP_Home      = 0xff95;
    private const nuint XK_KP_Up        = 0xff97;
    private const nuint XK_KP_Page_Up   = 0xff9a;
    private const nuint XK_KP_Delete    = 0xff9f;

    // Printable keys
    private const nuint XK_a            = 0x0061;
    private const nuint XK_b            = 0x0062;
    private const nuint XK_c            = 0x0063;
    private const nuint XK_d            = 0x0064;
    private const nuint XK_e            = 0x0065;
    private const nuint XK_f            = 0x0066;
    private const nuint XK_g            = 0x0067;
    private const nuint XK_h            = 0x0068;
    private const nuint XK_i            = 0x0069;
    private const nuint XK_j            = 0x006a;
    private const nuint XK_k            = 0x006b;
    private const nuint XK_l            = 0x006c;
    private const nuint XK_m            = 0x006d;
    private const nuint XK_n            = 0x006e;
    private const nuint XK_o            = 0x006f;
    private const nuint XK_p            = 0x0070;
    private const nuint XK_q            = 0x0071;
    private const nuint XK_r            = 0x0072;
    private const nuint XK_s            = 0x0073;
    private const nuint XK_t            = 0x0074;
    private const nuint XK_u            = 0x0075;
    private const nuint XK_v            = 0x0076;
    private const nuint XK_w            = 0x0077;
    private const nuint XK_x            = 0x0078;
    private const nuint XK_y            = 0x0079;
    private const nuint XK_z            = 0x007a;
    private const nuint XK_1            = 0x0031;
    private const nuint XK_2            = 0x0032;
    private const nuint XK_3            = 0x0033;
    private const nuint XK_4            = 0x0034;
    private const nuint XK_5            = 0x0035;
    private const nuint XK_6            = 0x0036;
    private const nuint XK_7            = 0x0037;
    private const nuint XK_8            = 0x0038;
    private const nuint XK_9            = 0x0039;
    private const nuint XK_0            = 0x0030;
    private const nuint XK_space        = 0x0020;
    private const nuint XK_minus        = 0x002d;
    private const nuint XK_equal        = 0x003d;
    private const nuint XK_bracketleft  = 0x005b;
    private const nuint XK_bracketright = 0x005d;
    private const nuint XK_backslash    = 0x005c;
    private const nuint XK_semicolon    = 0x003b;
    private const nuint XK_apostrophe   = 0x0027;
    private const nuint XK_grave        = 0x0060;
    private const nuint XK_comma        = 0x002c;
    private const nuint XK_period       = 0x002e;
    private const nuint XK_slash        = 0x002f;
    private const nuint XK_less         = 0x003c;

    // Modifier mode keys
    private const nuint XK_Mode_switch      = 0xff7e;
    private const nuint XK_ISO_Level3_Shift = 0xfe03;

    // XKB constants
    private const uint XkbUseCoreKbd       = 0x0100;
    private const uint XkbKeyNamesMask     = 1 << 9;   // (1 << 9) = 512
    private const uint XkbKeyAliasesMask   = 1 << 10;  // (1 << 10) = 1024
    private const int  XkbKeyNameLength    = 4;
    private const uint XkbStateNotify_     = 2;
    private const uint XkbGroupStateMask_  = 1 << 4;

    // Xlib Success constant
    private const int Success = 0;

    private const int F_GETFL = 3;
    private const int F_SETFL = 4;
    private const int F_GETFD = 1;
    private const int F_SETFD = 2;
    private const int O_NONBLOCK = 0x800;
    private const int FD_CLOEXEC = 1;

    // -----------------------------------------------------------------
    //  translateKeySyms -- Translate X11 KeySyms to GLFW key code
    // -----------------------------------------------------------------

    private static int translateKeySyms(nuint* keysyms, int width)
    {
        if (width > 1)
        {
            switch (keysyms[1])
            {
                case XK_KP_0:           return GLFW_KEY_KP_0;
                case XK_KP_1:           return GLFW_KEY_KP_1;
                case XK_KP_2:           return GLFW_KEY_KP_2;
                case XK_KP_3:           return GLFW_KEY_KP_3;
                case XK_KP_4:           return GLFW_KEY_KP_4;
                case XK_KP_5:           return GLFW_KEY_KP_5;
                case XK_KP_6:           return GLFW_KEY_KP_6;
                case XK_KP_7:           return GLFW_KEY_KP_7;
                case XK_KP_8:           return GLFW_KEY_KP_8;
                case XK_KP_9:           return GLFW_KEY_KP_9;
                case XK_KP_Separator:
                case XK_KP_Decimal:     return GLFW_KEY_KP_DECIMAL;
                case XK_KP_Equal:       return GLFW_KEY_KP_EQUAL;
                case XK_KP_Enter:       return GLFW_KEY_KP_ENTER;
                default:                break;
            }
        }

        switch (keysyms[0])
        {
            case XK_Escape:         return GLFW_KEY_ESCAPE;
            case XK_Tab:            return GLFW_KEY_TAB;
            case XK_Shift_L:        return GLFW_KEY_LEFT_SHIFT;
            case XK_Shift_R:        return GLFW_KEY_RIGHT_SHIFT;
            case XK_Control_L:      return GLFW_KEY_LEFT_CONTROL;
            case XK_Control_R:      return GLFW_KEY_RIGHT_CONTROL;
            case XK_Meta_L:
            case XK_Alt_L:          return GLFW_KEY_LEFT_ALT;
            case XK_Mode_switch:
            case XK_ISO_Level3_Shift:
            case XK_Meta_R:
            case XK_Alt_R:          return GLFW_KEY_RIGHT_ALT;
            case XK_Super_L:        return GLFW_KEY_LEFT_SUPER;
            case XK_Super_R:        return GLFW_KEY_RIGHT_SUPER;
            case XK_Menu:           return GLFW_KEY_MENU;
            case XK_Num_Lock:       return GLFW_KEY_NUM_LOCK;
            case XK_Caps_Lock:      return GLFW_KEY_CAPS_LOCK;
            case XK_Print:          return GLFW_KEY_PRINT_SCREEN;
            case XK_Scroll_Lock:    return GLFW_KEY_SCROLL_LOCK;
            case XK_Pause:          return GLFW_KEY_PAUSE;
            case XK_Delete:         return GLFW_KEY_DELETE;
            case XK_BackSpace:      return GLFW_KEY_BACKSPACE;
            case XK_Return:         return GLFW_KEY_ENTER;
            case XK_Home:           return GLFW_KEY_HOME;
            case XK_End:            return GLFW_KEY_END;
            case XK_Page_Up:        return GLFW_KEY_PAGE_UP;
            case XK_Page_Down:      return GLFW_KEY_PAGE_DOWN;
            case XK_Insert:         return GLFW_KEY_INSERT;
            case XK_Left:           return GLFW_KEY_LEFT;
            case XK_Right:          return GLFW_KEY_RIGHT;
            case XK_Down:           return GLFW_KEY_DOWN;
            case XK_Up:             return GLFW_KEY_UP;
            case XK_F1:             return GLFW_KEY_F1;
            case XK_F2:             return GLFW_KEY_F2;
            case XK_F3:             return GLFW_KEY_F3;
            case XK_F4:             return GLFW_KEY_F4;
            case XK_F5:             return GLFW_KEY_F5;
            case XK_F6:             return GLFW_KEY_F6;
            case XK_F7:             return GLFW_KEY_F7;
            case XK_F8:             return GLFW_KEY_F8;
            case XK_F9:             return GLFW_KEY_F9;
            case XK_F10:            return GLFW_KEY_F10;
            case XK_F11:            return GLFW_KEY_F11;
            case XK_F12:            return GLFW_KEY_F12;
            case XK_F13:            return GLFW_KEY_F13;
            case XK_F14:            return GLFW_KEY_F14;
            case XK_F15:            return GLFW_KEY_F15;
            case XK_F16:            return GLFW_KEY_F16;
            case XK_F17:            return GLFW_KEY_F17;
            case XK_F18:            return GLFW_KEY_F18;
            case XK_F19:            return GLFW_KEY_F19;
            case XK_F20:            return GLFW_KEY_F20;
            case XK_F21:            return GLFW_KEY_F21;
            case XK_F22:            return GLFW_KEY_F22;
            case XK_F23:            return GLFW_KEY_F23;
            case XK_F24:            return GLFW_KEY_F24;
            case XK_F25:            return GLFW_KEY_F25;

            // Numeric keypad
            case XK_KP_Divide:      return GLFW_KEY_KP_DIVIDE;
            case XK_KP_Multiply:    return GLFW_KEY_KP_MULTIPLY;
            case XK_KP_Subtract:    return GLFW_KEY_KP_SUBTRACT;
            case XK_KP_Add:         return GLFW_KEY_KP_ADD;

            // These should have been detected in secondary keysym test above!
            case XK_KP_Insert:      return GLFW_KEY_KP_0;
            case XK_KP_End:         return GLFW_KEY_KP_1;
            case XK_KP_Down:        return GLFW_KEY_KP_2;
            case XK_KP_Page_Down:   return GLFW_KEY_KP_3;
            case XK_KP_Left:        return GLFW_KEY_KP_4;
            case XK_KP_Right:       return GLFW_KEY_KP_6;
            case XK_KP_Home:        return GLFW_KEY_KP_7;
            case XK_KP_Up:          return GLFW_KEY_KP_8;
            case XK_KP_Page_Up:     return GLFW_KEY_KP_9;
            case XK_KP_Delete:      return GLFW_KEY_KP_DECIMAL;
            case XK_KP_Equal:       return GLFW_KEY_KP_EQUAL;
            case XK_KP_Enter:       return GLFW_KEY_KP_ENTER;

            // Printable keys (layout dependent fallback)
            case XK_a:              return GLFW_KEY_A;
            case XK_b:              return GLFW_KEY_B;
            case XK_c:              return GLFW_KEY_C;
            case XK_d:              return GLFW_KEY_D;
            case XK_e:              return GLFW_KEY_E;
            case XK_f:              return GLFW_KEY_F;
            case XK_g:              return GLFW_KEY_G;
            case XK_h:              return GLFW_KEY_H;
            case XK_i:              return GLFW_KEY_I;
            case XK_j:              return GLFW_KEY_J;
            case XK_k:              return GLFW_KEY_K;
            case XK_l:              return GLFW_KEY_L;
            case XK_m:              return GLFW_KEY_M;
            case XK_n:              return GLFW_KEY_N;
            case XK_o:              return GLFW_KEY_O;
            case XK_p:              return GLFW_KEY_P;
            case XK_q:              return GLFW_KEY_Q;
            case XK_r:              return GLFW_KEY_R;
            case XK_s:              return GLFW_KEY_S;
            case XK_t:              return GLFW_KEY_T;
            case XK_u:              return GLFW_KEY_U;
            case XK_v:              return GLFW_KEY_V;
            case XK_w:              return GLFW_KEY_W;
            case XK_x:              return GLFW_KEY_X;
            case XK_y:              return GLFW_KEY_Y;
            case XK_z:              return GLFW_KEY_Z;
            case XK_1:              return GLFW_KEY_1;
            case XK_2:              return GLFW_KEY_2;
            case XK_3:              return GLFW_KEY_3;
            case XK_4:              return GLFW_KEY_4;
            case XK_5:              return GLFW_KEY_5;
            case XK_6:              return GLFW_KEY_6;
            case XK_7:              return GLFW_KEY_7;
            case XK_8:              return GLFW_KEY_8;
            case XK_9:              return GLFW_KEY_9;
            case XK_0:              return GLFW_KEY_0;
            case XK_space:          return GLFW_KEY_SPACE;
            case XK_minus:          return GLFW_KEY_MINUS;
            case XK_equal:          return GLFW_KEY_EQUAL;
            case XK_bracketleft:    return GLFW_KEY_LEFT_BRACKET;
            case XK_bracketright:   return GLFW_KEY_RIGHT_BRACKET;
            case XK_backslash:      return GLFW_KEY_BACKSLASH;
            case XK_semicolon:      return GLFW_KEY_SEMICOLON;
            case XK_apostrophe:     return GLFW_KEY_APOSTROPHE;
            case XK_grave:          return GLFW_KEY_GRAVE_ACCENT;
            case XK_comma:          return GLFW_KEY_COMMA;
            case XK_period:         return GLFW_KEY_PERIOD;
            case XK_slash:          return GLFW_KEY_SLASH;
            case XK_less:           return GLFW_KEY_WORLD_1;
            default:                break;
        }

        return GLFW_KEY_UNKNOWN;
    }

    // -----------------------------------------------------------------
    //  XKB key name struct -- inline for createKeyTables
    // -----------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct XkbKeyNameRec
    {
        public unsafe fixed byte name[4]; // XkbKeyNameLength
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XkbKeyAliasRec
    {
        public unsafe fixed byte real[4];
        public unsafe fixed byte alias[4];
    }

    // Atom is unsigned long in the client-side X11 ABI. C# fixed buffers
    // cannot hold nuint, so use inline arrays to retain native width/alignment.
    [InlineArray(16)]
    private struct XkbVirtualModAtoms { private nuint element; }

    [InlineArray(32)]
    private struct XkbIndicatorAtoms { private nuint element; }

    [InlineArray(4)]
    private struct XkbGroupAtoms { private nuint element; }

    // Complete client-side XkbNamesRec, including the trailing counts.
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct XkbNamesRec
    {
        public nuint keycodes;      // Atom
        public nuint geometry;      // Atom
        public nuint symbols;       // Atom
        public nuint types;         // Atom
        public nuint compat;        // Atom
        public XkbVirtualModAtoms vmods;   // Atom[XkbNumVirtualMods]
        public XkbIndicatorAtoms indicators; // Atom[XkbNumIndicators]
        public XkbGroupAtoms groups;       // Atom[XkbNumKbdGroups]
        public XkbKeyNameRec* keys; // XkbKeyNameRec*
        public XkbKeyAliasRec* key_aliases; // XkbKeyAliasRec*
        public nuint* radio_groups; // Atom*
        public nuint phys_symbols;  // Atom
        public byte num_keys;
        public byte num_key_aliases;
        public ushort num_rg;
    }

    // Complete client-side XkbDescRec (opaque pointees retain native width).
    [StructLayout(LayoutKind.Sequential)]
    private struct XkbDescRec
    {
        public nint dpy;            // Display*
        public ushort flags;
        public ushort device_spec;
        public byte min_key_code;   // KeyCode
        public byte max_key_code;   // KeyCode
        public nint ctrls;          // XkbControlsPtr
        public nint server;         // XkbServerMapPtr
        public nint map;            // XkbClientMapPtr
        public nint indicators;     // XkbIndicatorPtr
        public XkbNamesRec* names;  // XkbNamesPtr
        public nint compat;         // XkbCompatMapPtr
        public nint geom;           // XkbGeometryPtr
    }

    // -----------------------------------------------------------------
    //  createKeyTables
    // -----------------------------------------------------------------

    private static void createKeyTables()
    {
        var x11 = _glfw.X11!;
        int scancodeMin, scancodeMax;

        // Fill keycodes with -1 (UNKNOWN)
        for (int i = 0; i < x11.keycodes.Length; i++)
            x11.keycodes[i] = -1;
        // Fill scancodes with -1
        for (int i = 0; i < x11.scancodes.Length; i++)
            x11.scancodes[i] = -1;

        if (x11.xkb.available)
        {
            // Use XKB to determine physical key locations independently of the
            // current keyboard layout
            nint descPtr = x11.xkb.GetMap(x11.display, 0, XkbUseCoreKbd);
            x11.xkb.GetNames(x11.display, XkbKeyNamesMask | XkbKeyAliasesMask, descPtr);

            XkbDescRec* desc = (XkbDescRec*)descPtr;
            scancodeMin = desc->min_key_code;
            scancodeMax = desc->max_key_code;

            // Key name to GLFW key code mapping table
            var keymap = new (int key, string name)[]
            {
                (GLFW_KEY_GRAVE_ACCENT, "TLDE"),
                (GLFW_KEY_1, "AE01"),
                (GLFW_KEY_2, "AE02"),
                (GLFW_KEY_3, "AE03"),
                (GLFW_KEY_4, "AE04"),
                (GLFW_KEY_5, "AE05"),
                (GLFW_KEY_6, "AE06"),
                (GLFW_KEY_7, "AE07"),
                (GLFW_KEY_8, "AE08"),
                (GLFW_KEY_9, "AE09"),
                (GLFW_KEY_0, "AE10"),
                (GLFW_KEY_MINUS, "AE11"),
                (GLFW_KEY_EQUAL, "AE12"),
                (GLFW_KEY_Q, "AD01"),
                (GLFW_KEY_W, "AD02"),
                (GLFW_KEY_E, "AD03"),
                (GLFW_KEY_R, "AD04"),
                (GLFW_KEY_T, "AD05"),
                (GLFW_KEY_Y, "AD06"),
                (GLFW_KEY_U, "AD07"),
                (GLFW_KEY_I, "AD08"),
                (GLFW_KEY_O, "AD09"),
                (GLFW_KEY_P, "AD10"),
                (GLFW_KEY_LEFT_BRACKET, "AD11"),
                (GLFW_KEY_RIGHT_BRACKET, "AD12"),
                (GLFW_KEY_A, "AC01"),
                (GLFW_KEY_S, "AC02"),
                (GLFW_KEY_D, "AC03"),
                (GLFW_KEY_F, "AC04"),
                (GLFW_KEY_G, "AC05"),
                (GLFW_KEY_H, "AC06"),
                (GLFW_KEY_J, "AC07"),
                (GLFW_KEY_K, "AC08"),
                (GLFW_KEY_L, "AC09"),
                (GLFW_KEY_SEMICOLON, "AC10"),
                (GLFW_KEY_APOSTROPHE, "AC11"),
                (GLFW_KEY_Z, "AB01"),
                (GLFW_KEY_X, "AB02"),
                (GLFW_KEY_C, "AB03"),
                (GLFW_KEY_V, "AB04"),
                (GLFW_KEY_B, "AB05"),
                (GLFW_KEY_N, "AB06"),
                (GLFW_KEY_M, "AB07"),
                (GLFW_KEY_COMMA, "AB08"),
                (GLFW_KEY_PERIOD, "AB09"),
                (GLFW_KEY_SLASH, "AB10"),
                (GLFW_KEY_BACKSLASH, "BKSL"),
                (GLFW_KEY_WORLD_1, "LSGT"),
                (GLFW_KEY_SPACE, "SPCE"),
                (GLFW_KEY_ESCAPE, "ESC\0"),
                (GLFW_KEY_ENTER, "RTRN"),
                (GLFW_KEY_TAB, "TAB\0"),
                (GLFW_KEY_BACKSPACE, "BKSP"),
                (GLFW_KEY_INSERT, "INS\0"),
                (GLFW_KEY_DELETE, "DELE"),
                (GLFW_KEY_RIGHT, "RGHT"),
                (GLFW_KEY_LEFT, "LEFT"),
                (GLFW_KEY_DOWN, "DOWN"),
                (GLFW_KEY_UP, "UP\0\0"),
                (GLFW_KEY_PAGE_UP, "PGUP"),
                (GLFW_KEY_PAGE_DOWN, "PGDN"),
                (GLFW_KEY_HOME, "HOME"),
                (GLFW_KEY_END, "END\0"),
                (GLFW_KEY_CAPS_LOCK, "CAPS"),
                (GLFW_KEY_SCROLL_LOCK, "SCLK"),
                (GLFW_KEY_NUM_LOCK, "NMLK"),
                (GLFW_KEY_PRINT_SCREEN, "PRSC"),
                (GLFW_KEY_PAUSE, "PAUS"),
                (GLFW_KEY_F1, "FK01"),
                (GLFW_KEY_F2, "FK02"),
                (GLFW_KEY_F3, "FK03"),
                (GLFW_KEY_F4, "FK04"),
                (GLFW_KEY_F5, "FK05"),
                (GLFW_KEY_F6, "FK06"),
                (GLFW_KEY_F7, "FK07"),
                (GLFW_KEY_F8, "FK08"),
                (GLFW_KEY_F9, "FK09"),
                (GLFW_KEY_F10, "FK10"),
                (GLFW_KEY_F11, "FK11"),
                (GLFW_KEY_F12, "FK12"),
                (GLFW_KEY_F13, "FK13"),
                (GLFW_KEY_F14, "FK14"),
                (GLFW_KEY_F15, "FK15"),
                (GLFW_KEY_F16, "FK16"),
                (GLFW_KEY_F17, "FK17"),
                (GLFW_KEY_F18, "FK18"),
                (GLFW_KEY_F19, "FK19"),
                (GLFW_KEY_F20, "FK20"),
                (GLFW_KEY_F21, "FK21"),
                (GLFW_KEY_F22, "FK22"),
                (GLFW_KEY_F23, "FK23"),
                (GLFW_KEY_F24, "FK24"),
                (GLFW_KEY_F25, "FK25"),
                (GLFW_KEY_KP_0, "KP0\0"),
                (GLFW_KEY_KP_1, "KP1\0"),
                (GLFW_KEY_KP_2, "KP2\0"),
                (GLFW_KEY_KP_3, "KP3\0"),
                (GLFW_KEY_KP_4, "KP4\0"),
                (GLFW_KEY_KP_5, "KP5\0"),
                (GLFW_KEY_KP_6, "KP6\0"),
                (GLFW_KEY_KP_7, "KP7\0"),
                (GLFW_KEY_KP_8, "KP8\0"),
                (GLFW_KEY_KP_9, "KP9\0"),
                (GLFW_KEY_KP_DECIMAL, "KPDL"),
                (GLFW_KEY_KP_DIVIDE, "KPDV"),
                (GLFW_KEY_KP_MULTIPLY, "KPMU"),
                (GLFW_KEY_KP_SUBTRACT, "KPSU"),
                (GLFW_KEY_KP_ADD, "KPAD"),
                (GLFW_KEY_KP_ENTER, "KPEN"),
                (GLFW_KEY_KP_EQUAL, "KPEQ"),
                (GLFW_KEY_LEFT_SHIFT, "LFSH"),
                (GLFW_KEY_LEFT_CONTROL, "LCTL"),
                (GLFW_KEY_LEFT_ALT, "LALT"),
                (GLFW_KEY_LEFT_SUPER, "LWIN"),
                (GLFW_KEY_RIGHT_SHIFT, "RTSH"),
                (GLFW_KEY_RIGHT_CONTROL, "RCTL"),
                (GLFW_KEY_RIGHT_ALT, "RALT"),
                (GLFW_KEY_RIGHT_ALT, "LVL3"),
                (GLFW_KEY_RIGHT_ALT, "MDSW"),
                (GLFW_KEY_RIGHT_SUPER, "RWIN"),
                (GLFW_KEY_MENU, "MENU"),
            };

            // Find the X11 key code -> GLFW key code mapping
            for (int scancode = scancodeMin; scancode <= scancodeMax; scancode++)
            {
                int key = GLFW_KEY_UNKNOWN;

                // Map the key name to a GLFW key code
                for (int i = 0; i < keymap.Length; i++)
                {
                    if (StrncmpKeyName(desc->names->keys[scancode].name,
                                       keymap[i].name,
                                       XkbKeyNameLength))
                    {
                        key = keymap[i].key;
                        break;
                    }
                }

                // Fall back to key aliases in case the key name did not match
                for (int i = 0; i < desc->names->num_key_aliases; i++)
                {
                    if (key != GLFW_KEY_UNKNOWN)
                        break;

                    if (!StrncmpKeyNameRaw(desc->names->key_aliases[i].real,
                                           desc->names->keys[scancode].name,
                                           XkbKeyNameLength))
                    {
                        continue;
                    }

                    for (int j = 0; j < keymap.Length; j++)
                    {
                        if (StrncmpKeyName(desc->names->key_aliases[i].alias,
                                           keymap[j].name,
                                           XkbKeyNameLength))
                        {
                            key = keymap[j].key;
                            break;
                        }
                    }
                }

                x11.keycodes[scancode] = (short)key;
            }

            x11.xkb.FreeNames(descPtr, XkbKeyNamesMask, 1);
            x11.xkb.FreeKeyboard(descPtr, 0, 1);
        }
        else
        {
            int smin, smax;
            x11.xlib.DisplayKeycodes(x11.display, &smin, &smax);
            scancodeMin = smin;
            scancodeMax = smax;
        }

        int width;
        nint keysymsPtr = x11.xlib.GetKeyboardMapping(x11.display,
                                                        (byte)scancodeMin,
                                                        scancodeMax - scancodeMin + 1,
                                                        &width);
        nuint* keysyms = (nuint*)keysymsPtr;

        for (int scancode = scancodeMin; scancode <= scancodeMax; scancode++)
        {
            // Translate the un-translated key codes using traditional X11 KeySym lookups
            if (x11.keycodes[scancode] < 0)
            {
                int @base = (scancode - scancodeMin) * width;
                x11.keycodes[scancode] = (short)translateKeySyms(&keysyms[@base], width);
            }

            // Store the reverse translation for faster key name lookup
            if (x11.keycodes[scancode] > 0)
                x11.scancodes[x11.keycodes[scancode]] = (short)scancode;
        }

        x11.xlib.Free(keysymsPtr);
    }

    // Compare a fixed-length XKB key name buffer against a C# string
    private static bool StrncmpKeyName(byte* xkbName, string csName, int maxLen)
    {
        for (int i = 0; i < maxLen; i++)
        {
            byte a = xkbName[i];
            byte b = i < csName.Length ? (byte)csName[i] : (byte)0;
            if (a != b)
                return false;
            if (a == 0)
                return true;
        }
        return true;
    }

    // Compare two fixed-length XKB key name buffers
    private static bool StrncmpKeyNameRaw(byte* a, byte* b, int maxLen)
    {
        for (int i = 0; i < maxLen; i++)
        {
            if (a[i] != b[i])
                return false;
            if (a[i] == 0)
                return true;
        }
        return true;
    }

    // -----------------------------------------------------------------
    //  hasUsableInputMethodStyle
    // -----------------------------------------------------------------

    private static bool hasUsableInputMethodStyle()
    {
        var x11 = _glfw.X11!;
        XIMStyles* styles = null;
        fixed (byte* name = "queryInputStyle\0"u8)
            if (((delegate* unmanaged[Cdecl]<nint, byte*, XIMStyles**, nint, nint>)x11.xlib.GetIMValues_ptr)(x11.im, name, &styles, 0) != 0) return false;
        bool found = false;
        for (int i = 0; i < styles->Count; i++)
            if (styles->Styles[i] == 0x408) { found = true; break; }
        x11.xlib.Free((nint)styles);
        return found;
    }

    // -----------------------------------------------------------------
    //  getAtomIfSupported
    // -----------------------------------------------------------------

    private static nuint getAtomIfSupported(nuint* supportedAtoms,
                                             nuint atomCount,
                                             ReadOnlySpan<byte> atomName)
    {
        var x11 = _glfw.X11!;
        nuint atom;
        fixed (byte* p = atomName)
        {
            atom = x11.xlib.InternAtom(x11.display, p, 0);
        }

        for (nuint i = 0; i < atomCount; i++)
        {
            if (supportedAtoms[i] == atom)
                return atom;
        }

        return 0; // None
    }

    // -----------------------------------------------------------------
    //  detectEWMH
    // -----------------------------------------------------------------

    private static void detectEWMH()
    {
        var x11 = _glfw.X11!;

        // First we read the _NET_SUPPORTING_WM_CHECK property on the root window
        nuint* windowFromRoot = null;
        if (_glfwGetWindowPropertyX11(x11.root,
                                       x11.NET_SUPPORTING_WM_CHECK,
                                       XA_WINDOW,
                                       (byte**)&windowFromRoot) == 0)
        {
            return;
        }

        _glfwGrabErrorHandlerX11();

        // If it exists, it should be the XID of a top-level window
        nuint* windowFromChild = null;
        if (_glfwGetWindowPropertyX11(*windowFromRoot,
                                       x11.NET_SUPPORTING_WM_CHECK,
                                       XA_WINDOW,
                                       (byte**)&windowFromChild) == 0)
        {
            _glfwReleaseErrorHandlerX11();
            x11.xlib.Free((nint)windowFromRoot);
            return;
        }

        _glfwReleaseErrorHandlerX11();

        if (*windowFromRoot != *windowFromChild)
        {
            x11.xlib.Free((nint)windowFromRoot);
            x11.xlib.Free((nint)windowFromChild);
            return;
        }

        x11.xlib.Free((nint)windowFromRoot);
        x11.xlib.Free((nint)windowFromChild);

        // Read _NET_SUPPORTED property
        nuint* supportedAtoms = null;
        nuint atomCount = _glfwGetWindowPropertyX11(x11.root,
                                                     x11.NET_SUPPORTED,
                                                     XA_ATOM,
                                                     (byte**)&supportedAtoms);

        x11.NET_WM_STATE =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_STATE\0"u8);
        x11.NET_WM_STATE_ABOVE =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_STATE_ABOVE\0"u8);
        x11.NET_WM_STATE_FULLSCREEN =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_STATE_FULLSCREEN\0"u8);
        x11.NET_WM_STATE_MAXIMIZED_VERT =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_STATE_MAXIMIZED_VERT\0"u8);
        x11.NET_WM_STATE_MAXIMIZED_HORZ =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_STATE_MAXIMIZED_HORZ\0"u8);
        x11.NET_WM_STATE_DEMANDS_ATTENTION =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_STATE_DEMANDS_ATTENTION\0"u8);
        x11.NET_WM_FULLSCREEN_MONITORS =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_FULLSCREEN_MONITORS\0"u8);
        x11.NET_WM_WINDOW_TYPE =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_WINDOW_TYPE\0"u8);
        x11.NET_WM_WINDOW_TYPE_NORMAL =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WM_WINDOW_TYPE_NORMAL\0"u8);
        x11.NET_WORKAREA =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_WORKAREA\0"u8);
        x11.NET_CURRENT_DESKTOP =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_CURRENT_DESKTOP\0"u8);
        x11.NET_ACTIVE_WINDOW =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_ACTIVE_WINDOW\0"u8);
        x11.NET_FRAME_EXTENTS =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_FRAME_EXTENTS\0"u8);
        x11.NET_REQUEST_FRAME_EXTENTS =
            getAtomIfSupported(supportedAtoms, atomCount, "_NET_REQUEST_FRAME_EXTENTS\0"u8);

        if ((nint)supportedAtoms != 0)
            x11.xlib.Free((nint)supportedAtoms);
    }

    // -----------------------------------------------------------------
    //  initExtensions -- detect and load X11 extensions
    // -----------------------------------------------------------------

    private static bool initExtensions()
    {
        var x11 = _glfw.X11!;

        // Load optional extension libraries via X11Native helpers
        // (the loading functions already populate the function pointers)
        X11Native.LoadVidmode(x11);
        if (x11.vidmode.handle != 0)
        {
            int evBase, errBase;
            if (x11.vidmode.QueryExtension != null &&
                x11.vidmode.QueryExtension(x11.display, &evBase, &errBase) != 0)
            {
                x11.vidmode.available = true;
                x11.vidmode.eventBase = evBase;
                x11.vidmode.errorBase = errBase;
            }
        }

        X11Native.LoadXi(x11);
        if (x11.xi.handle != 0)
        {
            int majorOp, evBase, errBase;
            fixed (byte* extName = "XInputExtension\0"u8)
            {
                if (x11.xlib.QueryExtension(x11.display, extName, &majorOp, &evBase, &errBase) != 0)
                {
                    x11.xi.majorOpcode = majorOp;
                    x11.xi.eventBase = evBase;
                    x11.xi.errorBase = errBase;

                    x11.xi.major = 2;
                    x11.xi.minor = 0;

                    int maj = x11.xi.major, min = x11.xi.minor;
                    if (x11.xi.QueryVersion != null &&
                        x11.xi.QueryVersion(x11.display, &maj, &min) == Success)
                    {
                        x11.xi.major = maj;
                        x11.xi.minor = min;
                        x11.xi.available = true;
                    }
                }
            }
        }

        X11Native.LoadRandr(x11);
        if (x11.randr.handle != 0)
        {
            int evBase, errBase;
            if (x11.randr.QueryExtension != null &&
                x11.randr.QueryExtension(x11.display, &evBase, &errBase) != 0)
            {
                x11.randr.eventBase = evBase;
                x11.randr.errorBase = errBase;

                int maj, min;
                if (x11.randr.QueryVersion != null &&
                    x11.randr.QueryVersion(x11.display, &maj, &min) != 0)
                {
                    x11.randr.major = maj;
                    x11.randr.minor = min;
                    // The GLFW RandR path requires at least version 1.3
                    if (x11.randr.major > 1 || x11.randr.minor >= 3)
                        x11.randr.available = true;
                }
                else
                {
                    _glfwInputError(GLFW_PLATFORM_ERROR,
                                    "X11: Failed to query RandR version");
                }
            }
        }

        if (x11.randr.available)
        {
            nint srPtr = x11.randr.GetScreenResourcesCurrent(x11.display, x11.root);
            XRRScreenResources* sr = (XRRScreenResources*)srPtr;

            if (sr->ncrtc == 0 ||
                x11.randr.GetCrtcGammaSize(x11.display, sr->crtcs[0]) == 0)
            {
                // Likely older Nvidia driver with broken gamma support
                x11.randr.gammaBroken = true;
            }

            if (sr->ncrtc == 0)
            {
                // A system without CRTCs -- disable RandR monitor path
                x11.randr.monitorBroken = true;
            }

            x11.randr.FreeScreenResources(srPtr);
        }

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            x11.randr.SelectInput(x11.display, x11.root,
                                   RROutputChangeNotifyMask);
        }

        X11Native.LoadXcursor(x11);

        X11Native.LoadXinerama(x11);
        if (x11.xinerama.handle != 0)
        {
            int maj, min;
            if (x11.xinerama.QueryExtension != null &&
                x11.xinerama.QueryExtension(x11.display, &maj, &min) != 0)
            {
                x11.xinerama.major = maj;
                x11.xinerama.minor = min;

                if (x11.xinerama.IsActive != null &&
                    x11.xinerama.IsActive(x11.display) != 0)
                {
                    x11.xinerama.available = true;
                }
            }
        }

        // Xkb (loaded from libX11 already)
        x11.xkb.major = 1;
        x11.xkb.minor = 0;
        {
            int majorOp = 0, evBase = 0, errBase = 0, maj = x11.xkb.major, min = x11.xkb.minor;
            x11.xkb.available =
                x11.xkb.QueryExtension != null &&
                x11.xkb.QueryExtension(x11.display, &majorOp, &evBase, &errBase, &maj, &min) != 0;
            if (x11.xkb.available)
            {
                x11.xkb.majorOpcode = majorOp;
                x11.xkb.eventBase = evBase;
                x11.xkb.errorBase = errBase;
                x11.xkb.major = maj;
                x11.xkb.minor = min;
            }
        }

        if (x11.xkb.available)
        {
            int supported;
            if (x11.xkb.SetDetectableAutoRepeat != null &&
                x11.xkb.SetDetectableAutoRepeat(x11.display, 1, &supported) != 0)
            {
                if (supported != 0)
                    x11.xkb.detectable = true;
            }

            XkbStateRec state;
            if (x11.xkb.GetState != null &&
                x11.xkb.GetState(x11.display, XkbUseCoreKbd, &state) == Success)
            {
                x11.xkb.group = (uint)state.group;
            }

            if (x11.xkb.SelectEventDetails != null)
            {
                x11.xkb.SelectEventDetails(x11.display, XkbUseCoreKbd, XkbStateNotify_,
                                             XkbGroupStateMask_, XkbGroupStateMask_);
            }
        }

        if (_glfw.hints.Init.X11.XcbVulkanSurface)
        {
            X11Native.LoadX11Xcb(x11);
        }

        X11Native.LoadXrender(x11);
        if (x11.xrender.handle != 0)
        {
            int evBase, errBase;
            if (x11.xrender.QueryExtension != null &&
                x11.xrender.QueryExtension(x11.display, &evBase, &errBase) != 0)
            {
                x11.xrender.eventBase = evBase;
                x11.xrender.errorBase = errBase;

                int maj, min;
                if (x11.xrender.QueryVersion != null &&
                    x11.xrender.QueryVersion(x11.display, &maj, &min) != 0)
                {
                    x11.xrender.major = maj;
                    x11.xrender.minor = min;
                    x11.xrender.available = true;
                }
            }
        }

        X11Native.LoadXshape(x11);
        if (x11.xshape.handle != 0)
        {
            int evBase, errBase;
            if (x11.xshape.QueryExtension != null &&
                x11.xshape.QueryExtension(x11.display, &evBase, &errBase) != 0)
            {
                x11.xshape.eventBase = evBase;
                x11.xshape.errorBase = errBase;

                int maj, min;
                if (x11.xshape.QueryVersion != null &&
                    x11.xshape.QueryVersion(x11.display, &maj, &min) != 0)
                {
                    x11.xshape.major = maj;
                    x11.xshape.minor = min;
                    x11.xshape.available = true;
                }
            }
        }

        // Update the key code LUT
        createKeyTables();

        // String format atoms
        fixed (byte* p = "NULL\0"u8)
            x11.NULL_ = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "UTF8_STRING\0"u8)
            x11.UTF8_STRING = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "ATOM_PAIR\0"u8)
            x11.ATOM_PAIR = x11.xlib.InternAtom(x11.display, p, 0);

        // Custom selection property atom
        fixed (byte* p = "GLFW_SELECTION\0"u8)
            x11.GLFW_SELECTION = x11.xlib.InternAtom(x11.display, p, 0);

        // ICCCM standard clipboard atoms
        fixed (byte* p = "TARGETS\0"u8)
            x11.TARGETS = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "MULTIPLE\0"u8)
            x11.MULTIPLE = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "PRIMARY\0"u8)
            x11.PRIMARY = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "INCR\0"u8)
            x11.INCR = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "CLIPBOARD\0"u8)
            x11.CLIPBOARD = x11.xlib.InternAtom(x11.display, p, 0);

        // Clipboard manager atoms
        fixed (byte* p = "CLIPBOARD_MANAGER\0"u8)
            x11.CLIPBOARD_MANAGER = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "SAVE_TARGETS\0"u8)
            x11.SAVE_TARGETS = x11.xlib.InternAtom(x11.display, p, 0);

        // Xdnd (drag and drop) atoms
        fixed (byte* p = "XdndAware\0"u8)
            x11.XdndAware = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndEnter\0"u8)
            x11.XdndEnter = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndPosition\0"u8)
            x11.XdndPosition = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndStatus\0"u8)
            x11.XdndStatus = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndActionCopy\0"u8)
            x11.XdndActionCopy = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndDrop\0"u8)
            x11.XdndDrop = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndFinished\0"u8)
            x11.XdndFinished = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndSelection\0"u8)
            x11.XdndSelection = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "XdndTypeList\0"u8)
            x11.XdndTypeList = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "text/uri-list\0"u8)
            x11.text_uri_list = x11.xlib.InternAtom(x11.display, p, 0);

        // ICCCM, EWMH and Motif window property atoms
        fixed (byte* p = "WM_PROTOCOLS\0"u8)
            x11.WM_PROTOCOLS = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "WM_STATE\0"u8)
            x11.WM_STATE = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "WM_DELETE_WINDOW\0"u8)
            x11.WM_DELETE_WINDOW = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_SUPPORTED\0"u8)
            x11.NET_SUPPORTED = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_SUPPORTING_WM_CHECK\0"u8)
            x11.NET_SUPPORTING_WM_CHECK = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_ICON\0"u8)
            x11.NET_WM_ICON = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_PING\0"u8)
            x11.NET_WM_PING = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_PID\0"u8)
            x11.NET_WM_PID = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_NAME\0"u8)
            x11.NET_WM_NAME = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_ICON_NAME\0"u8)
            x11.NET_WM_ICON_NAME = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_BYPASS_COMPOSITOR\0"u8)
            x11.NET_WM_BYPASS_COMPOSITOR = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_NET_WM_WINDOW_OPACITY\0"u8)
            x11.NET_WM_WINDOW_OPACITY = x11.xlib.InternAtom(x11.display, p, 0);
        fixed (byte* p = "_MOTIF_WM_HINTS\0"u8)
            x11.MOTIF_WM_HINTS = x11.xlib.InternAtom(x11.display, p, 0);

        // The compositing manager selection name contains the screen number
        {
            string name = $"_NET_WM_CM_S{x11.screen}";
            byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(name + "\0");
            fixed (byte* p = nameBytes)
                x11.NET_WM_CM_Sx = x11.xlib.InternAtom(x11.display, p, 0);
        }

        // Detect whether an EWMH-conformant window manager is running
        detectEWMH();

        return true;
    }

    // -----------------------------------------------------------------
    //  getSystemContentScale
    // -----------------------------------------------------------------

    internal static void getSystemContentScale(out float xscale, out float yscale)
    {
        var x11 = _glfw.X11!;

        // Start by assuming the default X11 DPI
        float xdpi = 96.0f, ydpi = 96.0f;

        // NOTE: Basing the scale on Xft.dpi where available should provide the most
        //       consistent user experience (matches Qt, Gtk, etc)
        nint rms = x11.xlib.ResourceManagerString(x11.display);
        if (rms != 0)
        {
            nint db = x11.xrm.GetStringDatabase((byte*)rms);
            if (db != 0)
            {
                XrmValue value;
                byte* type = null;

                fixed (byte* nameStr = "Xft.dpi\0"u8)
                fixed (byte* classStr = "Xft.Dpi\0"u8)
                {
                    if (x11.xrm.GetResource(db, nameStr, classStr, &type, &value) != 0)
                    {
                        if (type != null)
                        {
                            string typeStr = Marshal.PtrToStringAnsi((nint)type) ?? "";
                            if (typeStr == "String" && value.addr != 0)
                            {
                                string dpiStr = Marshal.PtrToStringAnsi(value.addr) ?? "";
                                if (float.TryParse(dpiStr, System.Globalization.NumberStyles.Float,
                                                   System.Globalization.CultureInfo.InvariantCulture,
                                                   out float dpi))
                                {
                                    xdpi = ydpi = dpi;
                                }
                            }
                        }
                    }
                }

                x11.xrm.DestroyDatabase(db);
            }
        }

        xscale = xdpi / 96.0f;
        yscale = ydpi / 96.0f;
    }

    // -----------------------------------------------------------------
    //  createHiddenCursor
    // -----------------------------------------------------------------

    private static nuint createHiddenCursor()
    {
        byte[] pixels = new byte[16 * 16 * 4]; // all zeros = transparent
        GlfwImage image = new GlfwImage { Width = 16, Height = 16, Pixels = pixels };
        return _glfwCreateNativeCursorX11(in image, 0, 0);
    }

    // -----------------------------------------------------------------
    //  createHelperWindow
    // -----------------------------------------------------------------

    private static nuint createHelperWindow()
    {
        var x11 = _glfw.X11!;
        XSetWindowAttributes wa = default;
        wa.event_mask = PropertyChangeMask;

        return x11.xlib.CreateWindow(x11.display, x11.root,
                                      0, 0, 1, 1, 0, 0,
                                      InputOnly,
                                      x11.xlib.DefaultVisual(x11.display, x11.screen),
                                      CWEventMask, &wa);
    }

    // -----------------------------------------------------------------
    //  createEmptyEventPipe
    // -----------------------------------------------------------------

    private static bool createEmptyEventPipe()
    {
        var x11 = _glfw.X11!;
        int* fds = stackalloc int[2];
        if (x11.libc.pipe(fds) != 0)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "X11: Failed to create empty event pipe");
            return false;
        }

        x11.emptyEventPipeRead = fds[0];
        x11.emptyEventPipeWrite = fds[1];

        for (int i = 0; i < 2; i++)
        {
            int fd = i == 0 ? fds[0] : fds[1];
            int sf = x11.libc.fcntl(fd, F_GETFL, 0);
            int df = x11.libc.fcntl(fd, F_GETFD, 0);

            if (sf == -1 || df == -1 ||
                x11.libc.fcntl(fd, F_SETFL, sf | O_NONBLOCK) == -1 ||
                x11.libc.fcntl(fd, F_SETFD, df | FD_CLOEXEC) == -1)
            {
                _glfwInputError(GLFW_PLATFORM_ERROR,
                                "X11: Failed to set flags for empty event pipe");
                return false;
            }
        }

        return true;
    }

    // -----------------------------------------------------------------
    //  X error handler
    // -----------------------------------------------------------------

    // We use an unmanaged callback for XSetErrorHandler
    // Since XSetErrorHandler expects a function pointer matching
    //   int (*XErrorHandler)(Display*, XErrorEvent*)
    // We use a managed delegate and pin it.

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static int errorHandler(nint display, nint eventPtr)
    {
        var x11 = _glfw.X11;
        if (x11 == null || x11.display != display)
            return 0;

        x11.errorCode = ((XErrorEvent*)eventPtr)->error_code;

        return 0;
    }

    // -----------------------------------------------------------------
    //  _glfwGrabErrorHandlerX11 / _glfwReleaseErrorHandlerX11
    //  Shared X11 error-handler implementation used by GLX
    // -----------------------------------------------------------------

    internal static void _glfwGrabErrorHandlerX11_Real()
    {
        var x11 = _glfw.X11!;
        x11.errorCode = Success;
        nint handler = (nint)(delegate* unmanaged[Cdecl]<nint, nint, int>)&errorHandler;
        x11.errorHandler = x11.xlib.SetErrorHandler(handler);
    }

    internal static void _glfwReleaseErrorHandlerX11_Real()
    {
        var x11 = _glfw.X11!;
        // Synchronize to make sure all commands are processed
        x11.xlib.Sync(x11.display, 0);
        x11.xlib.SetErrorHandler(x11.errorHandler);
        x11.errorHandler = 0;
    }

    internal static void _glfwInputErrorX11_Real(int error, string message)
    {
        var x11 = _glfw.X11!;
        byte* buffer = stackalloc byte[1024];
        x11.xlib.GetErrorText(x11.display, x11.errorCode, buffer, 1024);
        string xError = Marshal.PtrToStringAnsi((nint)buffer) ?? "";
        _glfwInputError(error, message + ": " + xError);
    }

    // -----------------------------------------------------------------
    //  _glfwIsVisualTransparentX11 -- real implementation
    // -----------------------------------------------------------------

    internal static bool _glfwIsVisualTransparentX11_Real(nint visual)
    {
        var x11 = _glfw.X11!;

        if (!x11.xrender.available)
            return false;

        nint format = x11.xrender.FindVisualFormat(x11.display, visual);
        if (format == 0)
            return false;

        return ((XRenderPictFormat*)format)->direct.alphaMask != 0;
    }

    // -----------------------------------------------------------------
    //  initX11 -- main initialization
    // -----------------------------------------------------------------

    internal static bool initX11()
    {
        var x11 = _glfw.X11!;

        // The function pointers are already loaded by X11Native.LoadXlib in the connector.
        // The xlib.utf8 flag is already set by LoadXlib.

        x11.screen = x11.xlib.DefaultScreen(x11.display);
        x11.root = x11.xlib.RootWindow(x11.display, x11.screen);
        x11.context = x11.xrm.UniqueQuark();

        getSystemContentScale(out x11.contentScaleX, out x11.contentScaleY);

        if (!createEmptyEventPipe())
            return false;

        if (!initExtensions())
            return false;

        x11.helperWindowHandle = createHelperWindow();
        x11.hiddenCursorHandle = createHiddenCursor();

        if (x11.xlib.SupportsLocale() != 0 && x11.xlib.utf8)
        {
            fixed (byte* empty = "\0"u8)
            {
                x11.xlib.SetLocaleModifiers(empty);
            }

            RegisterX11InputMethod(true);
        }

        _glfwPollMonitorsX11();
        return true;
    }

    // -----------------------------------------------------------------
    //  terminateX11 -- cleanup
    // -----------------------------------------------------------------

    internal static void terminateX11()
    {
        var x11 = _glfw.X11;
        if (x11 == null)
            return;

        if (x11.helperWindowHandle != 0)
        {
            if (x11.xlib.GetSelectionOwner(x11.display, x11.CLIPBOARD) ==
                x11.helperWindowHandle)
            {
                _glfwPushSelectionToManagerX11();
            }

            x11.xlib.DestroyWindow(x11.display, x11.helperWindowHandle);
            x11.helperWindowHandle = 0;
        }

        if (x11.hiddenCursorHandle != 0)
        {
            x11.xlib.FreeCursor(x11.display, x11.hiddenCursorHandle);
            x11.hiddenCursorHandle = 0;
        }

        x11.primarySelectionString = null;
        x11.clipboardString = null;

        if (x11.display != 0)
            RegisterX11InputMethod(false);

        if (x11.im != 0)
        {
            x11.xlib.CloseIM(x11.im);
            x11.im = 0;
        }

        if (x11.display != 0)
        {
            x11.xlib.CloseDisplay(x11.display);
            x11.display = 0;
        }

        _glfwTerminateOSMesa();
        _glfwTerminateEGL();
        _glfwTerminateGLX();

        if (x11.emptyEventPipeRead != 0 || x11.emptyEventPipeWrite != 0)
        {
            x11.libc.close(x11.emptyEventPipeRead);
            x11.libc.close(x11.emptyEventPipeWrite);
        }

        X11Native.FreeLibraries(x11);

        _glfw.X11 = null;
    }

}

// Ported from glfw/src/x11_window.c (GLFW 3.6)
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
using System.Text;
using static Glfw.GLFW;
using static Glfw.X11Events;
using static Glfw.X11Constants;

namespace Glfw;

public static partial class Glfw
{
    // Action for EWMH client messages
    private const long _NET_WM_STATE_REMOVE = 0;
    private const long _NET_WM_STATE_ADD    = 1;
    private const long _NET_WM_STATE_TOGGLE = 2;

    // Additional mouse button names for XButtonEvent
    private const uint Button1 = 1;
    private const uint Button2 = 2;
    private const uint Button3 = 3;
    private const uint Button4 = 4;
    private const uint Button5 = 5;
    private const uint Button6 = 6;
    private const uint Button7 = 7;

    // Motif WM hints flags
    private const int MWM_HINTS_DECORATIONS = 2;
    private const int MWM_DECOR_ALL         = 1;

    private const int _GLFW_XDND_VERSION = 5;

    // WM_STATE values
    private const int WithdrawnState = 0;
    private const int NormalState    = 1;
    private const int IconicState    = 3;

    // XSizeHints flags
    private const long USPosition  = 1 << 0;
    private const long USSize      = 1 << 1;
    private const long PPosition   = 1 << 2;
    private const long PSize       = 1 << 3;
    private const long PMinSize    = 1 << 4;
    private const long PMaxSize    = 1 << 5;
    private const long PResizeInc  = 1 << 6;
    private const long PAspect     = 1 << 7;
    private const long PBaseSize   = 1 << 8;
    private const long PWinGravity = 1 << 9;

    // XWMHints flags
    private const long InputHint   = 1 << 0;
    private const long StateHint   = 1 << 1;
    private const long IconPixmapHint = 1 << 2;

    // StaticGravity for XSizeHints
    private const int StaticGravity = 10;

    // DontPreferBlanking / DefaultExposures for XSetScreenSaver
    private const int DontPreferBlanking = 0;
    private const int DefaultExposures   = 2;

    // Map state
    private const int IsUnmapped  = 0;
    private const int IsUnviewable = 1;
    private const int IsViewable  = 2;

    // PropMode
    private const int PropModeReplace = 0;
    private const int PropModeAppend  = 2;

    // NotifyMode
    private const int NotifyNormal  = 0;
    private const int NotifyGrab    = 1;
    private const int NotifyUngrab  = 2;

    // Xlib error codes
    private const int BadWindow = 3;

    // NoEventMask
    private const nint NoEventMask = 0;

    // X11 modifier masks
    private const uint ShiftMask   = 1 << 0;
    private const uint LockMask    = 1 << 1;
    private const uint ControlMask = 1 << 2;
    private const uint Mod1Mask    = 1 << 3;
    private const uint Mod2Mask    = 1 << 4;
    private const uint Mod4Mask    = 1 << 6;

    // Xkb constants
    private const int XkbEventCode      = 0;
    private const int XkbStateNotify    = 2;
    private const uint XkbGroupStateMask = 1 << 4;

    // XI2 XIMask helpers
    private static unsafe bool XIMaskIsSet(byte* mask, int evt)
    {
        return (mask[evt >> 3] & (1 << (evt & 7))) != 0;
    }

    // XLookupString status codes
    private const int XBufferOverflow = -1;
    private const int XLookupNone     = 1;
    private const int XLookupChars    = 2;
    private const int XLookupKeySym   = 3;
    private const int XLookupBoth     = 4;

    // XIM style
    private const long XIMPreeditNothing = 0x0008;
    private const long XIMStatusNothing  = 0x0400;

    // X cursor font shapes
    private const uint XC_left_ptr           = 68;
    private const uint XC_xterm              = 152;
    private const uint XC_crosshair          = 34;
    private const uint XC_hand2              = 60;
    private const uint XC_sb_h_double_arrow  = 108;
    private const uint XC_sb_v_double_arrow  = 116;
    private const uint XC_fleur              = 52;

    // NoSymbol
    private const nuint NoSymbol = 0;

    // Xlib macros and libc functions are now loaded as function pointers
    // through x11.xlib.* and x11.libc.* (see x11_native.cs / x11_platform.cs).

    //////////////////////////////////////////////////////////////////////////
    //////                       Static helpers                         //////
    //////////////////////////////////////////////////////////////////////////

    // Wait for event data to arrive on the X11 display socket
    private static unsafe bool waitForX11Event(double* timeout)
    {
        var x11 = _glfw.X11!;
        PollFd fd;
        fd.fd = x11.xlib.ConnectionNumber(x11.display);
        fd.events = Glfw.POLLIN;
        fd.revents = 0;

        while (x11.xlib.Pending(x11.display) == 0)
        {
            if (!_glfwPollPOSIX(&fd, 1, timeout))
                return false;
        }

        return true;
    }

    // Wait for event data to arrive on any event file descriptor
    private static unsafe bool waitForAnyEvent(double* timeout)
    {
        var x11 = _glfw.X11!;
        PollFd* fds = stackalloc PollFd[3];
        fds[0].fd = x11.xlib.ConnectionNumber(x11.display);
        fds[0].events = Glfw.POLLIN;
        fds[0].revents = 0;
        fds[1].fd = x11.emptyEventPipeRead;
        fds[1].events = Glfw.POLLIN;
        fds[1].revents = 0;
        fds[2].fd = _glfw.joysticksInitialized ? linuxInotify : -1;
        fds[2].events = Glfw.POLLIN;
        fds[2].revents = 0;

        while (x11.xlib.Pending(x11.display) == 0)
        {
            if (!_glfwPollPOSIX(fds, 3, timeout))
                return false;

            for (int i = 1; i < 3; i++)
            {
                if ((fds[i].revents & Glfw.POLLIN) != 0)
                    return true;
            }
        }

        return true;
    }

    // Writes a byte to the empty event pipe
    private static unsafe void writeEmptyEvent()
    {
        var x11 = _glfw.X11!;
        byte b = 0;
        for (;;)
        {
            nint result = x11.libc.write(x11.emptyEventPipeWrite, &b, 1);
            if (result == 1 || (result == -1 && x11.libc.errno != 4 /*EINTR*/))
                break;
        }
    }

    // Drains available data from the empty event pipe
    private static unsafe void drainEmptyEvents()
    {
        var x11 = _glfw.X11!;
        byte* dummy = stackalloc byte[64];
        for (;;)
        {
            nint result = x11.libc.read(x11.emptyEventPipeRead, dummy, 64);
            if (result == -1 && x11.libc.errno != 4 /*EINTR*/)
                break;
        }
    }

    // Waits until a VisibilityNotify event arrives for the specified window
    private static unsafe bool waitForVisibilityNotify(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        XEvent dummy;
        double timeout = 0.1;

        while (x11.xlib.CheckTypedWindowEvent(x11.display, window.X11!.handle,
                                                VisibilityNotify, &dummy) == 0)
        {
            if (!waitForX11Event(&timeout))
                return false;
        }

        return true;
    }

    // Returns whether the window is iconified
    private static unsafe int getWindowState(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        int result = WithdrawnState;

        byte* data = null;
        nuint actualType;
        int actualFormat;
        nuint itemCount, bytesAfter;

        x11.xlib.GetWindowProperty(x11.display, window.X11!.handle,
                                    x11.WM_STATE, 0, (nint)0x7FFFFFFF, 0,
                                    x11.WM_STATE,
                                    &actualType, &actualFormat,
                                    &itemCount, &bytesAfter, &data);

        if (itemCount >= 2 && data != null)
        {
            // First CARD32 is the state field
            result = *(int*)data;
        }

        if (data != null)
            x11.xlib.Free((nint)data);

        return result;
    }

    // Translates an X event modifier state mask
    private static int translateState(uint state)
    {
        int mods = 0;

        if ((state & ShiftMask) != 0)
            mods |= GLFW_MOD_SHIFT;
        if ((state & ControlMask) != 0)
            mods |= GLFW_MOD_CONTROL;
        if ((state & Mod1Mask) != 0)
            mods |= GLFW_MOD_ALT;
        if ((state & Mod4Mask) != 0)
            mods |= GLFW_MOD_SUPER;
        if ((state & LockMask) != 0)
            mods |= GLFW_MOD_CAPS_LOCK;
        if ((state & Mod2Mask) != 0)
            mods |= GLFW_MOD_NUM_LOCK;

        return mods;
    }

    // Translates an X11 key code to a GLFW key token
    private static int translateKey(int scancode)
    {
        if (scancode < 0 || scancode > 255)
            return GLFW_KEY_UNKNOWN;

        return _glfw.X11!.keycodes[scancode];
    }

    // Sends an EWMH or ICCCM event to the window manager
    private static unsafe void sendEventToWM(GlfwWindow window, nuint type,
                                              long a, long b, long c, long d, long e)
    {
        var x11 = _glfw.X11!;
        XEvent ev = default;
        ev.type = ClientMessage;
        ev.xclient.window = window.X11!.handle;
        ev.xclient.format = 32;
        ev.xclient.message_type = type;
        ev.xclient.l[0] = (nint)a;
        ev.xclient.l[1] = (nint)b;
        ev.xclient.l[2] = (nint)c;
        ev.xclient.l[3] = (nint)d;
        ev.xclient.l[4] = (nint)e;

        x11.xlib.SendEvent(x11.display, x11.root, 0,
                            SubstructureNotifyMask | SubstructureRedirectMask,
                            &ev);
    }

    // Updates the normal hints according to the window settings
    private static unsafe void updateNormalHints(GlfwWindow window, int width, int height)
    {
        var x11 = _glfw.X11!;
        nint hintsPtr = x11.xlib.AllocSizeHints();
        if (hintsPtr == 0) return;

        XSizeHints* hints = (XSizeHints*)hintsPtr;

        nint supplied;
        x11.xlib.GetWMNormalHints(x11.display, window.X11!.handle, hints, &supplied);

        hints->flags = (nint)((long)hints->flags & (long)~(PMinSize | PMaxSize | PAspect));

        if (window.Monitor == null)
        {
            if (window.Resizable)
            {
                if (window.MinWidth != GLFW_DONT_CARE &&
                    window.MinHeight != GLFW_DONT_CARE)
                {
                    hints->flags = (nint)((long)hints->flags | (long)PMinSize);
                    hints->min_width = window.MinWidth;
                    hints->min_height = window.MinHeight;
                }

                if (window.MaxWidth != GLFW_DONT_CARE &&
                    window.MaxHeight != GLFW_DONT_CARE)
                {
                    hints->flags = (nint)((long)hints->flags | (long)PMaxSize);
                    hints->max_width = window.MaxWidth;
                    hints->max_height = window.MaxHeight;
                }

                if (window.Numer != GLFW_DONT_CARE &&
                    window.Denom != GLFW_DONT_CARE)
                {
                    hints->flags = (nint)((long)hints->flags | (long)PAspect);
                    hints->min_aspect_x = hints->max_aspect_x = window.Numer;
                    hints->min_aspect_y = hints->max_aspect_y = window.Denom;
                }
            }
            else
            {
                hints->flags = (nint)((long)hints->flags | (long)(PMinSize | PMaxSize));
                hints->min_width  = hints->max_width  = width;
                hints->min_height = hints->max_height = height;
            }
        }

        x11.xlib.SetWMNormalHints(x11.display, window.X11!.handle, hints);
        x11.xlib.Free(hintsPtr);
    }

    // Updates the full screen status of the window
    private static unsafe void updateWindowMode(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.Monitor != null)
        {
            if (x11.xinerama.available && x11.NET_WM_FULLSCREEN_MONITORS != 0)
            {
                sendEventToWM(window, x11.NET_WM_FULLSCREEN_MONITORS,
                              window.Monitor.X11!.index,
                              window.Monitor.X11.index,
                              window.Monitor.X11.index,
                              window.Monitor.X11.index, 0);
            }

            if (x11.NET_WM_STATE != 0 && x11.NET_WM_STATE_FULLSCREEN != 0)
            {
                sendEventToWM(window, x11.NET_WM_STATE,
                              _NET_WM_STATE_ADD,
                              (long)x11.NET_WM_STATE_FULLSCREEN,
                              0, 1, 0);
            }
            else
            {
                XSetWindowAttributes wa = default;
                wa.override_redirect = 1; // True
                x11.xlib.ChangeWindowAttributes(x11.display, window.X11!.handle,
                                                 CWOverrideRedirect, &wa);
                window.X11.overrideRedirect = true;
            }

            // Enable compositor bypass
            if (!window.X11!.transparent)
            {
                uint value = 1;
                x11.xlib.ChangeProperty(x11.display, window.X11.handle,
                                         x11.NET_WM_BYPASS_COMPOSITOR, XA_CARDINAL, 32,
                                         PropModeReplace, (byte*)&value, 1);
            }
        }
        else
        {
            if (x11.xinerama.available && x11.NET_WM_FULLSCREEN_MONITORS != 0)
            {
                x11.xlib.DeleteProperty(x11.display, window.X11!.handle,
                                         x11.NET_WM_FULLSCREEN_MONITORS);
            }

            if (x11.NET_WM_STATE != 0 && x11.NET_WM_STATE_FULLSCREEN != 0)
            {
                sendEventToWM(window, x11.NET_WM_STATE,
                              _NET_WM_STATE_REMOVE,
                              (long)x11.NET_WM_STATE_FULLSCREEN,
                              0, 1, 0);
            }
            else
            {
                XSetWindowAttributes wa = default;
                wa.override_redirect = 0; // False
                x11.xlib.ChangeWindowAttributes(x11.display, window.X11!.handle,
                                                 CWOverrideRedirect, &wa);
                window.X11.overrideRedirect = false;
            }

            // Disable compositor bypass
            if (!window.X11!.transparent)
            {
                x11.xlib.DeleteProperty(x11.display, window.X11.handle,
                                         x11.NET_WM_BYPASS_COMPOSITOR);
            }
        }
    }

    // Decode a Unicode code point from a UTF-8 stream
    private static unsafe uint decodeUTF8(ref byte* s)
    {
        uint codepoint = 0;
        uint count = 0;
        ReadOnlySpan<uint> offsets = stackalloc uint[]
        {
            0x00000000u, 0x00003080u, 0x000e2080u,
            0x03c82080u, 0xfa082080u, 0x82082080u
        };

        do
        {
            codepoint = (codepoint << 6) + *s;
            s++;
            count++;
        } while ((*s & 0xc0) == 0x80);

        return codepoint - offsets[(int)(count - 1)];
    }

    // Updates the cursor image according to its cursor mode
    private static unsafe void updateCursorImageX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.CursorMode == GLFW_CURSOR_NORMAL ||
            window.CursorMode == GLFW_CURSOR_CAPTURED)
        {
            if (window.Cursor != null && window.Cursor.X11 != null && window.Cursor.X11.handle != 0)
            {
                x11.xlib.DefineCursor(x11.display, window.X11!.handle,
                                       window.Cursor.X11.handle);
            }
            else
                x11.xlib.UndefineCursor(x11.display, window.X11!.handle);
        }
        else
        {
            x11.xlib.DefineCursor(x11.display, window.X11!.handle,
                                   x11.hiddenCursorHandle);
        }
    }

    // Grabs the cursor and confines it to the window
    private static unsafe void captureCursorX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        x11.xlib.GrabPointer(x11.display, window.X11!.handle, 1,
                              (uint)(ButtonPressMask | ButtonReleaseMask | PointerMotionMask),
                              GrabModeAsync, GrabModeAsync,
                              window.X11.handle, None_, CurrentTime);
    }

    // Ungrabs the cursor
    private static unsafe void releaseCursorX11()
    {
        var x11 = _glfw.X11!;
        x11.xlib.UngrabPointer(x11.display, CurrentTime);
    }

    // Enable XI2 raw mouse motion events
    private static unsafe void enableRawMouseMotionX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        byte* mask = stackalloc byte[4];
        for (int i = 0; i < 4; i++) mask[i] = 0;

        XIEventMask em;
        em.deviceid = XIAllMasterDevices;
        em.mask_len = 4;
        em.mask = (nint)mask;
        X11Constants.XISetMask(new Span<byte>(mask, 4), XI_RawMotion);

        x11.xi.SelectEvents(x11.display, x11.root, &em, 1);
    }

    // Disable XI2 raw mouse motion events
    private static unsafe void disableRawMouseMotionX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        byte mask = 0;

        XIEventMask em;
        em.deviceid = XIAllMasterDevices;
        em.mask_len = 1;
        em.mask = (nint)(&mask);

        x11.xi.SelectEvents(x11.display, x11.root, &em, 1);
    }

    // Apply disabled cursor mode to a focused window
    private static void disableCursorX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.RawMouseMotion)
            enableRawMouseMotionX11(window);

        x11.disabledCursorWindow = window;
        _glfwGetCursorPosX11(window, out x11.restoreCursorPosX, out x11.restoreCursorPosY);
        updateCursorImageX11(window);
        _glfwCenterCursorInContentArea(window);
        captureCursorX11(window);
    }

    // Exit disabled cursor mode for the specified window
    private static void enableCursorX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.RawMouseMotion)
            disableRawMouseMotionX11(window);

        x11.disabledCursorWindow = null;
        releaseCursorX11();
        _glfwSetCursorPosX11(window, x11.restoreCursorPosX, x11.restoreCursorPosY);
        updateCursorImageX11(window);
    }

    // Create the X11 window (and its colormap)
    private static unsafe bool createNativeWindow(GlfwWindow window,
                                                   GlfwWndConfig wndconfig,
                                                   nint visual, int depth)
    {
        var x11 = _glfw.X11!;
        int width = wndconfig.Width;
        int height = wndconfig.Height;

        if (wndconfig.ScaleToMonitor)
        {
            width = (int)(width * x11.contentScaleX);
            height = (int)(height * x11.contentScaleY);
        }

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        int xpos = 0, ypos = 0;
        if (wndconfig.Xpos != GLFW_ANY_POSITION && wndconfig.Ypos != GLFW_ANY_POSITION)
        {
            xpos = wndconfig.Xpos;
            ypos = wndconfig.Ypos;
        }

        window.X11 = new GlfwWindowX11();

        // Create a colormap based on the visual
        window.X11.colormap = x11.xlib.CreateColormap(x11.display, x11.root,
                                                        visual, AllocNone);

        window.X11.transparent = _glfwIsVisualTransparentX11(visual);

        XSetWindowAttributes wa = default;
        wa.colormap = window.X11.colormap;
        wa.event_mask = StructureNotifyMask | KeyPressMask | KeyReleaseMask |
                        PointerMotionMask | ButtonPressMask | ButtonReleaseMask |
                        ExposureMask | FocusChangeMask | VisibilityChangeMask |
                        EnterWindowMask | LeaveWindowMask | PropertyChangeMask;

        _glfwGrabErrorHandlerX11();

        window.X11.parent = x11.root;
        window.X11.handle = x11.xlib.CreateWindow(x11.display, x11.root,
                                                     xpos, ypos,
                                                     (uint)width, (uint)height,
                                                     0, depth, InputOutput,
                                                     visual,
                                                     CWBorderPixel | CWColormap | CWEventMask,
                                                     &wa);

        _glfwReleaseErrorHandlerX11();

        if (window.X11.handle == 0)
        {
            _glfwInputErrorX11(GLFW_PLATFORM_ERROR, "X11: Failed to create window");
            return false;
        }

        x11.xlib.SaveContext(x11.display, window.X11.handle, x11.context, null);

        if (!wndconfig.Decorated)
            _glfwSetWindowDecoratedX11(window, false);

        if (x11.NET_WM_STATE != 0 && window.Monitor == null)
        {
            nuint* states = stackalloc nuint[3];
            int count = 0;

            if (wndconfig.Floating)
            {
                if (x11.NET_WM_STATE_ABOVE != 0)
                    states[count++] = x11.NET_WM_STATE_ABOVE;
            }

            if (wndconfig.Maximized)
            {
                if (x11.NET_WM_STATE_MAXIMIZED_VERT != 0 &&
                    x11.NET_WM_STATE_MAXIMIZED_HORZ != 0)
                {
                    states[count++] = x11.NET_WM_STATE_MAXIMIZED_VERT;
                    states[count++] = x11.NET_WM_STATE_MAXIMIZED_HORZ;
                    window.X11.maximized = true;
                }
            }

            if (count > 0)
            {
                x11.xlib.ChangeProperty(x11.display, window.X11.handle,
                                         x11.NET_WM_STATE, XA_ATOM, 32,
                                         PropModeReplace, (byte*)states, count);
            }
        }

        // Declare the WM protocols supported by GLFW
        {
            nuint* protocols = stackalloc nuint[2];
            protocols[0] = x11.WM_DELETE_WINDOW;
            protocols[1] = x11.NET_WM_PING;
            x11.xlib.SetWMProtocols(x11.display, window.X11.handle, protocols, 2);
        }

        // Declare our PID
        {
            int pid = x11.libc.getpid();
            x11.xlib.ChangeProperty(x11.display, window.X11.handle,
                                     x11.NET_WM_PID, XA_CARDINAL, 32,
                                     PropModeReplace, (byte*)&pid, 1);
        }

        if (x11.NET_WM_WINDOW_TYPE != 0 && x11.NET_WM_WINDOW_TYPE_NORMAL != 0)
        {
            nuint type = x11.NET_WM_WINDOW_TYPE_NORMAL;
            x11.xlib.ChangeProperty(x11.display, window.X11.handle,
                                     x11.NET_WM_WINDOW_TYPE, XA_ATOM, 32,
                                     PropModeReplace, (byte*)&type, 1);
        }

        // Set ICCCM WM_HINTS property
        {
            nint hintsPtr = x11.xlib.AllocWMHints();
            if (hintsPtr == 0)
            {
                _glfwInputError(GLFW_OUT_OF_MEMORY, "X11: Failed to allocate WM hints");
                return false;
            }

            XWMHints* hints = (XWMHints*)hintsPtr;
            hints->flags = (nint)StateHint;
            hints->initial_state = NormalState;
            x11.xlib.SetWMHints(x11.display, window.X11.handle, hints);
            x11.xlib.Free(hintsPtr);
        }

        // Set ICCCM WM_NORMAL_HINTS property
        {
            nint hintsPtr = x11.xlib.AllocSizeHints();
            if (hintsPtr == 0)
            {
                _glfwInputError(GLFW_OUT_OF_MEMORY, "X11: Failed to allocate size hints");
                return false;
            }

            XSizeHints* hints = (XSizeHints*)hintsPtr;

            if (!wndconfig.Resizable)
            {
                hints->flags = (nint)((long)hints->flags | (long)(PMinSize | PMaxSize));
                hints->min_width  = hints->max_width  = width;
                hints->min_height = hints->max_height = height;
            }

            if (wndconfig.Xpos != GLFW_ANY_POSITION && wndconfig.Ypos != GLFW_ANY_POSITION)
            {
                hints->flags = (nint)((long)hints->flags | (long)PPosition);
                hints->x = 0;
                hints->y = 0;
            }

            hints->flags = (nint)((long)hints->flags | (long)PWinGravity);
            hints->win_gravity = StaticGravity;

            x11.xlib.SetWMNormalHints(x11.display, window.X11.handle, hints);
            x11.xlib.Free(hintsPtr);
        }

        // Set ICCCM WM_CLASS property
        {
            nint hintPtr = x11.xlib.AllocClassHint();
            if (hintPtr != 0)
            {
                XClassHint* hint = (XClassHint*)hintPtr;

                if (!string.IsNullOrEmpty(wndconfig.X11.InstanceName) &&
                    !string.IsNullOrEmpty(wndconfig.X11.ClassName))
                {
                    fixed (byte* instPtr = Encoding.UTF8.GetBytes(wndconfig.X11.InstanceName + "\0"))
                    fixed (byte* classPtr = Encoding.UTF8.GetBytes(wndconfig.X11.ClassName + "\0"))
                    {
                        hint->res_name = (nint)instPtr;
                        hint->res_class = (nint)classPtr;
                        x11.xlib.SetClassHint(x11.display, window.X11.handle, hint);
                    }
                }
                else
                {
                    string resName = Environment.GetEnvironmentVariable("RESOURCE_NAME") ?? "";
                    if (string.IsNullOrEmpty(resName))
                        resName = !string.IsNullOrEmpty(window.Title) ? window.Title : "glfw-application";

                    string resClass = !string.IsNullOrEmpty(window.Title) ? window.Title : "GLFW-Application";

                    fixed (byte* namePtr = Encoding.UTF8.GetBytes(resName + "\0"))
                    fixed (byte* classPtr = Encoding.UTF8.GetBytes(resClass + "\0"))
                    {
                        hint->res_name = (nint)namePtr;
                        hint->res_class = (nint)classPtr;
                        x11.xlib.SetClassHint(x11.display, window.X11.handle, hint);
                    }
                }

                x11.xlib.Free(hintPtr);
            }
        }

        // Announce support for Xdnd (drag and drop)
        {
            nuint version = _GLFW_XDND_VERSION;
            x11.xlib.ChangeProperty(x11.display, window.X11.handle,
                                     x11.XdndAware, XA_ATOM, 32,
                                     PropModeReplace, (byte*)&version, 1);
        }

        if (x11.im != 0)
            _glfwCreateInputContextX11(window);

        _glfwSetWindowTitleX11(window, window.Title ?? "");
        _glfwGetWindowPosX11(window, out window.X11.xpos, out window.X11.ypos);
        _glfwGetWindowSizeX11(window, out window.X11.width, out window.X11.height);

        return true;
    }

    // Set the specified property to the selection converted to the requested target
    private static unsafe nuint writeTargetToProperty(XSelectionRequestEvent* request)
    {
        var x11 = _glfw.X11!;
        string? selectionString = null;
        nuint* formats = stackalloc nuint[2];
        formats[0] = x11.UTF8_STRING;
        formats[1] = XA_STRING;
        int formatCount = 2;

        if (request->selection == x11.PRIMARY)
            selectionString = x11.primarySelectionString;
        else
            selectionString = x11.clipboardString;

        if (request->property == None_)
            return None_;

        if (request->target == x11.TARGETS)
        {
            nuint* targets = stackalloc nuint[4];
            targets[0] = x11.TARGETS;
            targets[1] = x11.MULTIPLE;
            targets[2] = x11.UTF8_STRING;
            targets[3] = XA_STRING;

            x11.xlib.ChangeProperty(x11.display, request->requestor,
                                     request->property, XA_ATOM, 32,
                                     PropModeReplace, (byte*)targets, 4);
            return request->property;
        }

        if (request->target == x11.MULTIPLE)
        {
            byte* rawTargets = null;
            nuint actualType;
            int actualFormat;
            nuint itemCount, bytesAfter;

            x11.xlib.GetWindowProperty(x11.display, request->requestor,
                                        request->property, 0, (nint)0x7FFFFFFF, 0,
                                        x11.ATOM_PAIR,
                                        &actualType, &actualFormat,
                                        &itemCount, &bytesAfter, &rawTargets);

            nuint* targetAtoms = (nuint*)rawTargets;

            for (nuint i = 0; i < itemCount; i += 2)
            {
                int j;
                for (j = 0; j < formatCount; j++)
                {
                    if (targetAtoms[i] == formats[j])
                        break;
                }

                if (j < formatCount && selectionString != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(selectionString);
                    fixed (byte* bp = bytes)
                    {
                        x11.xlib.ChangeProperty(x11.display, request->requestor,
                                                 targetAtoms[i + 1], targetAtoms[i], 8,
                                                 PropModeReplace, bp, bytes.Length);
                    }
                }
                else
                    targetAtoms[i + 1] = None_;
            }

            x11.xlib.ChangeProperty(x11.display, request->requestor,
                                     request->property, x11.ATOM_PAIR, 32,
                                     PropModeReplace, (byte*)targetAtoms, (int)itemCount);

            if (rawTargets != null)
                x11.xlib.Free((nint)rawTargets);

            return request->property;
        }

        if (request->target == x11.SAVE_TARGETS)
        {
            x11.xlib.ChangeProperty(x11.display, request->requestor,
                                     request->property, x11.NULL_, 32,
                                     PropModeReplace, null, 0);
            return request->property;
        }

        // Conversion to a data target was requested
        for (int i = 0; i < formatCount; i++)
        {
            if (request->target == formats[i] && selectionString != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(selectionString);
                fixed (byte* bp = bytes)
                {
                    x11.xlib.ChangeProperty(x11.display, request->requestor,
                                             request->property, request->target, 8,
                                             PropModeReplace, bp, bytes.Length);
                }
                return request->property;
            }
        }

        return None_;
    }

    private static unsafe void handleSelectionRequest(XEvent* ev)
    {
        var x11 = _glfw.X11!;
        XSelectionRequestEvent* request = &ev->xselectionrequest;

        XEvent reply = default;
        reply.type = SelectionNotify;
        reply.xselection.property = writeTargetToProperty(request);
        reply.xselection.display = request->display;
        reply.xselection.requestor = request->requestor;
        reply.xselection.selection = request->selection;
        reply.xselection.target = request->target;
        reply.xselection.time = request->time;

        x11.xlib.SendEvent(x11.display, request->requestor, 0, 0, &reply);
    }

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe int isSelPropNewValueNotify(nint display, XEvent* ev, nint pointer)
    {
        var notification = (XEvent*)pointer;
        return ev->type == PropertyNotify && ev->xproperty.state == PropertyNewValue &&
            ev->xproperty.window == notification->xselection.requestor &&
            ev->xproperty.atom == notification->xselection.property ? 1 : 0;
    }

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe int isSelectionEvent(nint display, XEvent* ev, nint pointer) =>
        ev->xkey.window == _glfw.X11!.helperWindowHandle &&
        (ev->type == SelectionRequest || ev->type == SelectionNotify || ev->type == SelectionClear) ? 1 : 0;

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static unsafe int isFrameExtentsEvent(nint display, XEvent* ev, nint window) =>
        ev->type == PropertyNotify && ev->xproperty.state == PropertyNewValue &&
        ev->xproperty.window == (nuint)window && ev->xproperty.atom == _glfw.X11!.NET_FRAME_EXTENTS ? 1 : 0;

    private static string convertLatin1toUTF8(ReadOnlySpan<byte> source)
    {
        // Upstream passes plain char directly to the uint32_t UTF-8 encoder.
        // Preserve its sign extension, including dropped high-bit bytes on
        // signed-char targets. ARM Linux's default plain char is unsigned.
        bool signed = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture is not
            (System.Runtime.InteropServices.Architecture.Arm or System.Runtime.InteropServices.Architecture.Arm64);
        byte[] result = new byte[checked(source.Length * 2)];
        int length = 0;
        foreach (byte value in source)
        {
            if (value == 0) break;
            uint codepoint = signed ? unchecked((uint)(sbyte)value) : value;
            length += _glfwEncodeUTF8(result, length, codepoint);
        }
        return Encoding.UTF8.GetString(result, 0, length);
    }

    private static unsafe string? getSelectionString(nuint selection)
    {
        var x11 = _glfw.X11!;
        nuint* targets = stackalloc nuint[2] { x11.UTF8_STRING, XA_STRING };
        bool primary = selection == x11.PRIMARY;
        if (x11.xlib.GetSelectionOwner(x11.display, selection) == x11.helperWindowHandle)
            return primary ? x11.primarySelectionString : x11.clipboardString;
        if (primary) x11.primarySelectionString = null; else x11.clipboardString = null;
        string? result = null;
        nint predicate = (nint)(delegate* unmanaged[Cdecl]<nint, XEvent*, nint, int>)&isSelPropNewValueNotify;
        for (int i = 0; i < 2; i++)
        {
            x11.xlib.ConvertSelection(x11.display, selection, targets[i], x11.GLFW_SELECTION, x11.helperWindowHandle, CurrentTime);
            XEvent notification, dummy;
            while (x11.xlib.CheckTypedWindowEvent(x11.display, x11.helperWindowHandle, SelectionNotify, &notification) == 0)
                waitForX11Event(null);
            if (notification.xselection.property == None_) continue;
            x11.xlib.CheckIfEvent(x11.display, &dummy, predicate, (nint)(&notification));
            byte* data = null;
            nuint actualType, count, after;
            int format;
            x11.xlib.GetWindowProperty(x11.display, notification.xselection.requestor, notification.xselection.property,
                0, nint.MaxValue, 1, 0, &actualType, &format, &count, &after, &data);
            if (actualType == x11.INCR)
            {
                System.Collections.Generic.List<byte>? bytes = null;
                for (;;)
                {
                    while (x11.xlib.CheckIfEvent(x11.display, &dummy, predicate, (nint)(&notification)) == 0)
                        waitForX11Event(null);
                    x11.xlib.Free((nint)data);
                    x11.xlib.GetWindowProperty(x11.display, notification.xselection.requestor, notification.xselection.property,
                        0, nint.MaxValue, 1, 0, &actualType, &format, &count, &after, &data);
                    if (count > 0)
                    {
                        bytes ??= new();
                        for (nuint b = 0; b < count && data[b] != 0; b++) bytes.Add(data[b]);
                    }
                    else
                    {
                        if (bytes != null) result = targets[i] == XA_STRING ? convertLatin1toUTF8(bytes.ToArray()) : Encoding.UTF8.GetString(bytes.ToArray());
                        break;
                    }
                }
            }
            else if (actualType == targets[i] && data != null)
            {
                nuint length = 0; while (length < count && data[length] != 0) length++;
                result = targets[i] == XA_STRING ? convertLatin1toUTF8(new ReadOnlySpan<byte>(data, checked((int)length))) : Encoding.UTF8.GetString(data, checked((int)length));
            }
            x11.xlib.Free((nint)data);
            if (result != null) break;
        }
        if (primary) x11.primarySelectionString = result; else x11.clipboardString = result;
        if (result == null) _glfwInputError(GLFW_FORMAT_UNAVAILABLE, "X11: Failed to convert selection to string");
        return result;
    }

    // Make the specified window and its video mode active on its monitor
    private static unsafe void acquireMonitorX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (x11.saver.count == 0)
        {
            int timeout, interval, blanking, exposure;
            x11.xlib.GetScreenSaver(x11.display, &timeout, &interval,
                                     &blanking, &exposure);
            x11.saver.timeout = timeout;
            x11.saver.interval = interval;
            x11.saver.blanking = blanking;
            x11.saver.exposure = exposure;

            x11.xlib.SetScreenSaver(x11.display, 0, 0, DontPreferBlanking,
                                     DefaultExposures);
        }

        if (window.Monitor!.Window == null)
            x11.saver.count++;

        _glfwSetVideoModeX11(window.Monitor, in window.VideoMode);

        if (window.X11!.overrideRedirect)
        {
            _glfwGetMonitorPosX11(window.Monitor, out int mxpos, out int mypos);
            _glfwGetVideoModeX11(window.Monitor, out var mode);

            x11.xlib.MoveResizeWindow(x11.display, window.X11.handle,
                                       mxpos, mypos, (uint)mode.Width, (uint)mode.Height);
        }

        _glfwInputMonitorWindow(window.Monitor, window);
    }

    // Remove the window and restore the original video mode
    private static unsafe void releaseMonitorX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.Monitor!.Window != window)
            return;

        _glfwInputMonitorWindow(window.Monitor, null);
        _glfwRestoreVideoModeX11(window.Monitor);

        x11.saver.count--;

        if (x11.saver.count == 0)
        {
            x11.xlib.SetScreenSaver(x11.display,
                                     x11.saver.timeout,
                                     x11.saver.interval,
                                     x11.saver.blanking,
                                     x11.saver.exposure);
        }
    }

    // Process the specified X event
    private static unsafe void processEvent(XEvent* ev)
    {
        var x11 = _glfw.X11!;
        int keycode = 0;
        int filtered = 0; // Bool: False

        // HACK: Save scancode as some IMs clear the field in XFilterEvent
        if (ev->type == KeyPress || ev->type == KeyRelease)
            keycode = (int)ev->xkey.keycode;

        filtered = x11.xlib.FilterEvent(ev, None_);

        if (x11.randr.available)
        {
            if (ev->type == x11.randr.eventBase + RRNotify)
            {
                x11.randr.UpdateConfiguration(ev);
                _glfwPollMonitorsX11();
                return;
            }
        }

        if (x11.xkb.available)
        {
            if (ev->type == x11.xkb.eventBase + XkbEventCode)
            {
                var state = (XkbStateNotifyEvent*)ev;
                if (state->xkb_type == XkbStateNotify &&
                    (state->changed & XkbGroupStateMask) != 0)
                    x11.xkb.group = (uint)state->group;
                return;
            }
        }

        if (ev->type == GenericEvent)
        {
            if (x11.xi.available)
            {
                GlfwWindow? dcw = x11.disabledCursorWindow;

                if (dcw != null &&
                    dcw.RawMouseMotion &&
                    ev->xcookie.extension == x11.xi.majorOpcode &&
                    x11.xlib.GetEventData(x11.display, &ev->xcookie) != 0 &&
                    ev->xcookie.evtype == XI_RawMotion)
                {
                    XIRawEvent* re = (XIRawEvent*)ev->xcookie.data;
                    if (re->valuators_mask_len > 0)
                    {
                        double* values = re->raw_values;
                        double xpos2 = dcw.VirtualCursorPosX;
                        double ypos2 = dcw.VirtualCursorPosY;

                        if (XIMaskIsSet(re->valuators_mask, 0))
                        {
                            xpos2 += *values;
                            values++;
                        }

                        if (XIMaskIsSet(re->valuators_mask, 1))
                            ypos2 += *values;

                        _glfwInputCursorPos(dcw, xpos2, ypos2);
                    }
                }

                x11.xlib.FreeEventData(x11.display, &ev->xcookie);
            }

            return;
        }

        if (ev->type == SelectionRequest)
        {
            handleSelectionRequest(ev);
            return;
        }

        // Find the GLFW window for this X11 event
        nint windowPtr;
        if (x11.xlib.FindContext(x11.display, ev->xkey.window /*xany.window*/,
                                  x11.context, &windowPtr) != 0)
        {
            return; // Event for a window that has already been destroyed
        }

        // Retrieve the GlfwWindow by scanning the window list
        GlfwWindow? window = null;
        for (GlfwWindow? w = _glfw.windowListHead; w != null; w = w.Next)
        {
            if (w.X11 != null && w.X11.handle == ev->xkey.window)
            {
                window = w;
                break;
            }
        }
        if (window == null)
            return;

        switch (ev->type)
        {
            case ReparentNotify:
            {
                window.X11!.parent = ev->xreparent.parent;
                return;
            }

            case KeyPress:
            {
                int key = translateKey(keycode);
                int mods = translateState(ev->xkey.state);
                bool plain = (mods & (GLFW_MOD_CONTROL | GLFW_MOD_ALT)) == 0;

                if (window.X11!.ic != 0)
                {
                    // HACK: Do not report the key press events duplicated by XIM
                    nuint diff = ev->xkey.time - window.X11.keyPressTimes[keycode];
                    if (diff == ev->xkey.time || (diff > 0 && diff < ((nuint)1 << 31)))
                    {
                        if (keycode != 0)
                            _glfwInputKey(window, key, keycode, GLFW_PRESS, mods);

                        window.X11.keyPressTimes[keycode] = ev->xkey.time;
                    }

                    if (filtered == 0)
                    {
                        byte* buffer = stackalloc byte[100];
                        int status;

                        int count = x11.xlib.utf8LookupString(window.X11.ic,
                                                               &ev->xkey,
                                                               buffer, 99,
                                                               null, &status);
                        byte* chars = buffer;
                        if (status == XBufferOverflow)
                        {
                            chars = (byte*)System.Runtime.InteropServices.NativeMemory.AllocZeroed((nuint)count + 1);
                            count = x11.xlib.utf8LookupString(window.X11.ic, &ev->xkey, chars, count, null, &status);
                        }
                        try
                        {
                            if (status == XLookupChars || status == XLookupBoth)
                            {
                                byte* c = chars;
                                chars[count] = 0;
                                while (c - chars < count)
                                    _glfwInputChar(window, decodeUTF8(ref c), mods, plain);
                            }
                        }
                        finally
                        {
                            if (chars != buffer) System.Runtime.InteropServices.NativeMemory.Free(chars);
                        }
                    }
                }
                else
                {
                    nuint keysym;
                    x11.xlib.LookupString(&ev->xkey, null, 0, &keysym, 0);

                    _glfwInputKey(window, key, keycode, GLFW_PRESS, mods);

                    uint cp = XkbUnicode._glfwKeySym2UnicodeX11((uint)keysym);
                    if (cp != XkbUnicode.GLFW_INVALID_CODEPOINT)
                        _glfwInputChar(window, cp, mods, plain);
                }
                return;
            }

            case KeyRelease:
            {
                int key = translateKey(keycode);
                int mods = translateState(ev->xkey.state);

                if (!x11.xkb.detectable)
                {
                    if (x11.xlib.EventsQueued(x11.display, QueuedAfterReading) > 0)
                    {
                        XEvent next;
                        x11.xlib.PeekEvent(x11.display, &next);

                        if (next.type == KeyPress &&
                            next.xkey.window == ev->xkey.window &&
                            next.xkey.keycode == (uint)keycode)
                        {
                            if ((next.xkey.time - ev->xkey.time) < 20)
                                return;
                        }
                    }
                }

                _glfwInputKey(window, key, keycode, GLFW_RELEASE, mods);
                return;
            }

            case ButtonPress:
            {
                int mods = translateState(ev->xbutton.state);

                if (ev->xbutton.button == Button1)
                    _glfwInputMouseClick(window, GLFW_MOUSE_BUTTON_LEFT, GLFW_PRESS, mods);
                else if (ev->xbutton.button == Button2)
                    _glfwInputMouseClick(window, GLFW_MOUSE_BUTTON_MIDDLE, GLFW_PRESS, mods);
                else if (ev->xbutton.button == Button3)
                    _glfwInputMouseClick(window, GLFW_MOUSE_BUTTON_RIGHT, GLFW_PRESS, mods);
                else if (ev->xbutton.button == Button4)
                    _glfwInputScroll(window, 0.0, 1.0);
                else if (ev->xbutton.button == Button5)
                    _glfwInputScroll(window, 0.0, -1.0);
                else if (ev->xbutton.button == Button6)
                    _glfwInputScroll(window, 1.0, 0.0);
                else if (ev->xbutton.button == Button7)
                    _glfwInputScroll(window, -1.0, 0.0);
                else
                {
                    _glfwInputMouseClick(window,
                                          (int)(ev->xbutton.button - Button1 - 4),
                                          GLFW_PRESS, mods);
                }
                return;
            }

            case ButtonRelease:
            {
                int mods = translateState(ev->xbutton.state);

                if (ev->xbutton.button == Button1)
                    _glfwInputMouseClick(window, GLFW_MOUSE_BUTTON_LEFT, GLFW_RELEASE, mods);
                else if (ev->xbutton.button == Button2)
                    _glfwInputMouseClick(window, GLFW_MOUSE_BUTTON_MIDDLE, GLFW_RELEASE, mods);
                else if (ev->xbutton.button == Button3)
                    _glfwInputMouseClick(window, GLFW_MOUSE_BUTTON_RIGHT, GLFW_RELEASE, mods);
                else if (ev->xbutton.button > Button7)
                {
                    _glfwInputMouseClick(window,
                                          (int)(ev->xbutton.button - Button1 - 4),
                                          GLFW_RELEASE, mods);
                }
                return;
            }

            case EnterNotify:
            {
                int cx = ev->xcrossing.x;
                int cy = ev->xcrossing.y;

                if (window.CursorMode == GLFW_CURSOR_HIDDEN)
                    updateCursorImageX11(window);

                _glfwInputCursorEnter(window, true);
                _glfwInputCursorPos(window, cx, cy);

                window.X11!.lastCursorPosX = cx;
                window.X11.lastCursorPosY = cy;
                return;
            }

            case LeaveNotify:
            {
                _glfwInputCursorEnter(window, false);
                return;
            }

            case MotionNotify:
            {
                int mx = ev->xmotion.x;
                int my = ev->xmotion.y;

                if (mx != window.X11!.warpCursorPosX ||
                    my != window.X11.warpCursorPosY)
                {
                    if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    {
                        if (x11.disabledCursorWindow != window)
                            return;
                        if (window.RawMouseMotion)
                            return;

                        int dx = mx - window.X11.lastCursorPosX;
                        int dy = my - window.X11.lastCursorPosY;

                        _glfwInputCursorPos(window,
                                             window.VirtualCursorPosX + dx,
                                             window.VirtualCursorPosY + dy);
                    }
                    else
                        _glfwInputCursorPos(window, mx, my);
                }

                window.X11!.lastCursorPosX = mx;
                window.X11.lastCursorPosY = my;
                return;
            }

            case ConfigureNotify:
            {
                if (ev->xconfigure.width != window.X11!.width ||
                    ev->xconfigure.height != window.X11.height)
                {
                    window.X11.width = ev->xconfigure.width;
                    window.X11.height = ev->xconfigure.height;

                    _glfwInputFramebufferSize(window,
                                               ev->xconfigure.width,
                                               ev->xconfigure.height);

                    _glfwInputWindowSize(window,
                                          ev->xconfigure.width,
                                          ev->xconfigure.height);
                }

                int cxpos = ev->xconfigure.x;
                int cypos = ev->xconfigure.y;

                if (ev->xkey.send_event == 0 && window.X11.parent != x11.root)
                {
                    _glfwGrabErrorHandlerX11();

                    nuint dummy3;
                    x11.xlib.TranslateCoordinates(x11.display,
                                                    window.X11.parent,
                                                    x11.root,
                                                    cxpos, cypos,
                                                    &cxpos, &cypos,
                                                    &dummy3);

                    _glfwReleaseErrorHandlerX11();
                    if (x11.errorCode == BadWindow)
                        return;
                }

                if (cxpos != window.X11.xpos || cypos != window.X11.ypos)
                {
                    window.X11.xpos = cxpos;
                    window.X11.ypos = cypos;
                    _glfwInputWindowPos(window, cxpos, cypos);
                }

                return;
            }

            case ClientMessage:
            {
                if (filtered != 0)
                    return;

                if (ev->xclient.message_type == None_)
                    return;

                if (ev->xclient.message_type == x11.WM_PROTOCOLS)
                {
                    nuint protocol = (nuint)(ulong)ev->xclient.l[0];
                    if (protocol == None_)
                        return;

                    if (protocol == x11.WM_DELETE_WINDOW)
                    {
                        _glfwInputWindowCloseRequest(window);
                    }
                    else if (protocol == x11.NET_WM_PING)
                    {
                        XEvent reply = *ev;
                        reply.xclient.window = x11.root;
                        x11.xlib.SendEvent(x11.display, x11.root, 0,
                                            SubstructureNotifyMask | SubstructureRedirectMask,
                                            &reply);
                    }
                }
                else if (ev->xclient.message_type == x11.XdndEnter)
                {
                    bool list = (ev->xclient.l[1] & 1) != 0;
                    x11.xdnd.source = (nuint)(ulong)ev->xclient.l[0];
                    x11.xdnd.version = (int)((ulong)ev->xclient.l[1] >> 24);
                    x11.xdnd.format = None_;

                    if (x11.xdnd.version > _GLFW_XDND_VERSION)
                        return;

                    if (list)
                    {
                        byte* data = null;
                        nuint atype, bafter, ic;
                        int afmt;
                        x11.xlib.GetWindowProperty(x11.display, x11.xdnd.source,
                                                    x11.XdndTypeList, 0, (nint)0x7FFFFFFF,
                                                    0, XA_ATOM,
                                                    &atype, &afmt, &ic, &bafter, &data);
                        nuint* fmts = (nuint*)data;
                        for (nuint fi = 0; fi < ic; fi++)
                        {
                            if (fmts[fi] == x11.text_uri_list)
                            {
                                x11.xdnd.format = x11.text_uri_list;
                                break;
                            }
                        }
                        if (data != null)
                            x11.xlib.Free((nint)data);
                    }
                    else
                    {
                        // data.l[2..4] contain up to 3 formats
                        nuint f2 = (nuint)(ulong)ev->xclient.l[2];
                        nuint f3 = (nuint)(ulong)ev->xclient.l[3];
                        nuint f4 = (nuint)(ulong)ev->xclient.l[4];
                        if (f2 == x11.text_uri_list || f3 == x11.text_uri_list || f4 == x11.text_uri_list)
                            x11.xdnd.format = x11.text_uri_list;
                    }
                }
                else if (ev->xclient.message_type == x11.XdndDrop)
                {
                    nuint time = CurrentTime;

                    if (x11.xdnd.version > _GLFW_XDND_VERSION)
                        return;

                    if (x11.xdnd.format != 0)
                    {
                        if (x11.xdnd.version >= 1)
                            time = (nuint)(ulong)ev->xclient.l[2];

                        x11.xlib.ConvertSelection(x11.display,
                                                    x11.XdndSelection, x11.xdnd.format,
                                                    x11.XdndSelection,
                                                    window.X11!.handle, time);
                    }
                    else if (x11.xdnd.version >= 2)
                    {
                        XEvent reply = default;
                        reply.type = ClientMessage;
                        reply.xclient.window = x11.xdnd.source;
                        reply.xclient.message_type = x11.XdndFinished;
                        reply.xclient.format = 32;
                        reply.xclient.l[0] = (nint)window.X11!.handle;
                        reply.xclient.l[1] = 0;
                        reply.xclient.l[2] = (nint)None_;

                        x11.xlib.SendEvent(x11.display, x11.xdnd.source,
                                            0, NoEventMask, &reply);
                        x11.xlib.Flush(x11.display);
                    }
                }
                else if (ev->xclient.message_type == x11.XdndPosition)
                {
                    int xabs = (int)(((ulong)ev->xclient.l[2] >> 16) & 0xffff);
                    int yabs = (int)((ulong)ev->xclient.l[2] & 0xffff);

                    if (x11.xdnd.version > _GLFW_XDND_VERSION)
                        return;

                    nuint dummy4;
                    int dxpos, dypos;
                    x11.xlib.TranslateCoordinates(x11.display, x11.root,
                                                    window.X11!.handle,
                                                    xabs, yabs, &dxpos, &dypos, &dummy4);

                    _glfwInputCursorPos(window, dxpos, dypos);

                    XEvent reply = default;
                    reply.type = ClientMessage;
                    reply.xclient.window = x11.xdnd.source;
                    reply.xclient.message_type = x11.XdndStatus;
                    reply.xclient.format = 32;
                    reply.xclient.l[0] = (nint)window.X11.handle;
                    reply.xclient.l[2] = 0;
                    reply.xclient.l[3] = 0;

                    if (x11.xdnd.format != 0)
                    {
                        reply.xclient.l[1] = 1;
                        if (x11.xdnd.version >= 2)
                            reply.xclient.l[4] = (nint)x11.XdndActionCopy;
                    }

                    x11.xlib.SendEvent(x11.display, x11.xdnd.source,
                                        0, NoEventMask, &reply);
                    x11.xlib.Flush(x11.display);
                }
                return;
            }

            case SelectionNotify:
            {
                if (ev->xselection.property == x11.XdndSelection)
                {
                    byte* data = null;
                    nuint atype, ic, bafter;
                    int afmt;

                    x11.xlib.GetWindowProperty(x11.display,
                                                ev->xselection.requestor,
                                                ev->xselection.property,
                                                0, (nint)0x7FFFFFFF, 0,
                                                ev->xselection.target,
                                                &atype, &afmt, &ic, &bafter, &data);

                    if (ic > 0 && data != null)
                    {
                        string uriData = Encoding.UTF8.GetString(data, (int)ic);
                        string[] paths = _glfwParseUriList(uriData);
                        if (paths.Length > 0)
                            _glfwInputDrop(window, paths.Length, paths);
                    }

                    if (data != null)
                        x11.xlib.Free((nint)data);

                    if (x11.xdnd.version >= 2)
                    {
                        XEvent reply = default;
                        reply.type = ClientMessage;
                        reply.xclient.window = x11.xdnd.source;
                        reply.xclient.message_type = x11.XdndFinished;
                        reply.xclient.format = 32;
                        reply.xclient.l[0] = (nint)window.X11!.handle;
                        reply.xclient.l[1] = (ic > 0 ? 1 : 0);
                        reply.xclient.l[2] = (nint)x11.XdndActionCopy;

                        x11.xlib.SendEvent(x11.display, x11.xdnd.source,
                                            0, NoEventMask, &reply);
                        x11.xlib.Flush(x11.display);
                    }
                }
                return;
            }

            case FocusIn:
            {
                if (ev->xfocus.mode == NotifyGrab || ev->xfocus.mode == NotifyUngrab)
                    return;

                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    disableCursorX11(window);
                else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                    captureCursorX11(window);

                if (window.X11!.ic != 0)
                    x11.xlib.SetICFocus(window.X11.ic);

                _glfwInputWindowFocus(window, GLFW_TRUE);
                return;
            }

            case FocusOut:
            {
                if (ev->xfocus.mode == NotifyGrab || ev->xfocus.mode == NotifyUngrab)
                    return;

                if (window.CursorMode == GLFW_CURSOR_DISABLED)
                    enableCursorX11(window);
                else if (window.CursorMode == GLFW_CURSOR_CAPTURED)
                    releaseCursorX11();

                if (window.X11!.ic != 0)
                    x11.xlib.UnsetICFocus(window.X11.ic);

                if (window.Monitor != null && window.AutoIconify)
                    _glfwIconifyWindowX11(window);

                _glfwInputWindowFocus(window, GLFW_FALSE);
                return;
            }

            case Expose:
            {
                _glfwInputWindowDamage(window);
                return;
            }

            case PropertyNotify:
            {
                if (ev->xproperty.state != PropertyNewValue)
                    return;

                if (ev->xproperty.atom == x11.WM_STATE)
                {
                    int state = getWindowState(window);
                    if (state != IconicState && state != NormalState)
                        return;

                    bool isIconified = (state == IconicState);
                    if (window.X11!.iconified != isIconified)
                    {
                        if (window.Monitor != null)
                        {
                            if (isIconified)
                                releaseMonitorX11(window);
                            else
                                acquireMonitorX11(window);
                        }

                        window.X11.iconified = isIconified;
                        _glfwInputWindowIconify(window, isIconified ? GLFW_TRUE : GLFW_FALSE);
                    }
                }
                else if (ev->xproperty.atom == x11.NET_WM_STATE)
                {
                    bool isMaximized = _glfwWindowMaximizedX11(window);
                    if (window.X11!.maximized != isMaximized)
                    {
                        window.X11.maximized = isMaximized;
                        _glfwInputWindowMaximize(window, isMaximized ? GLFW_TRUE : GLFW_FALSE);
                    }
                }
                return;
            }

            case DestroyNotify:
                return;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW internal API                      //////
    //////////////////////////////////////////////////////////////////////////

    // Retrieve a single window property of the specified type
    internal static unsafe nuint _glfwGetWindowPropertyX11(nuint window, nuint property,
                                                            nuint type, byte** value)
    {
        var x11 = _glfw.X11!;
        nuint actualType;
        int actualFormat;
        nuint itemCount, bytesAfter;

        x11.xlib.GetWindowProperty(x11.display, window, property,
                                    0, (nint)0x7FFFFFFF, 0, type,
                                    &actualType, &actualFormat,
                                    &itemCount, &bytesAfter, value);

        return itemCount;
    }

    // Push contents of our selection to clipboard manager
    internal static unsafe void _glfwPushSelectionToManagerX11()
    {
        var x11 = _glfw.X11!;

        x11.xlib.ConvertSelection(x11.display,
                                   x11.CLIPBOARD_MANAGER, x11.SAVE_TARGETS,
                                   None_, x11.helperWindowHandle, CurrentTime);

        for (;;)
        {
            XEvent sev;
            nint predicate = (nint)(delegate* unmanaged[Cdecl]<nint, XEvent*, nint, int>)&isSelectionEvent;
            while (x11.xlib.CheckIfEvent(x11.display, &sev, predicate, 0) != 0)
            {
                if (sev.type == SelectionRequest) handleSelectionRequest(&sev);
                else if (sev.type == SelectionNotify && sev.xselection.target == x11.SAVE_TARGETS) return;
            }

            waitForX11Event(null);
        }
    }

    // Create input context for a window
    internal static unsafe void _glfwCreateInputContextX11(GlfwWindow window)
    {
        CreateX11InputContext(window);
    }

    // Parse a URI list (from drag and drop) into an array of file paths
    internal static string[] _glfwParseUriList(string text)
    {
        var paths = new System.Collections.Generic.List<string>();
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (line.StartsWith("#", StringComparison.Ordinal))
                continue;
            int start = 0;
            if (line.StartsWith("file://", StringComparison.Ordinal))
            {
                start = line.IndexOf('/', 7);
                if (start < 0) start = line.Length;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(line[start..]);
            int length = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte value = bytes[i];
                if (value == '%' && i + 2 < bytes.Length)
                {
                    // strtol permits partial hex values; decode UTF-8 bytes,
                    // not UTF-16 characters, just like the native parser.
                    int parsed = 0, sign = 1, p = i + 1;
                    while (p <= i + 2 && " \t\r\n\f\v".IndexOf((char)bytes[p]) >= 0) p++;
                    if (p <= i + 2 && (bytes[p] == '-' || bytes[p] == '+')) { if (bytes[p] == '-') sign = -1; p++; }
                    for (; p <= i + 2; p++)
                    {
                        int digit = bytes[p] >= '0' && bytes[p] <= '9' ? bytes[p] - '0' : bytes[p] >= 'a' && bytes[p] <= 'f' ? bytes[p] - 'a' + 10 : bytes[p] >= 'A' && bytes[p] <= 'F' ? bytes[p] - 'A' + 10 : -1;
                        if (digit < 0) break;
                        parsed = parsed * 16 + digit;
                    }
                    value = unchecked((byte)(parsed * sign)); i += 2;
                }
                bytes[length++] = value;
            }
            int nul = Array.IndexOf(bytes, (byte)0, 0, length);
            paths.Add(Encoding.UTF8.GetString(bytes, 0, nul < 0 ? length : nul));
        }

        return paths.ToArray();
    }

    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW platform API                      //////
    //////////////////////////////////////////////////////////////////////////

    internal static unsafe bool _glfwCreateWindowX11(GlfwWindow window,
                                                      GlfwWndConfig wndconfig,
                                                      GlfwCtxConfig ctxconfig,
                                                      GlfwFbConfig fbconfig)
    {
        var x11 = _glfw.X11!;
        nint visual = 0;
        int depth = 0;

        if (ctxconfig.Client != GLFW_NO_API)
        {
            if (ctxconfig.Source == GLFW_NATIVE_CONTEXT_API)
            {
                if (!_glfwInitGLX())
                    return false;
                if (!_glfwChooseVisualGLX(wndconfig, ctxconfig, fbconfig, out visual, out depth))
                    return false;
            }
            else if (ctxconfig.Source == GLFW_EGL_CONTEXT_API)
            {
                if (!_glfwInitEGL()) return false;
                if (!_glfwChooseVisualEGL(wndconfig, ctxconfig, fbconfig, out visual, out depth)) return false;
            }
        }

        if (visual == 0)
        {
            visual = x11.xlib.DefaultVisual(x11.display, x11.screen);
            depth = x11.xlib.DefaultDepth(x11.display, x11.screen);
        }

        if (!createNativeWindow(window, wndconfig, visual, depth))
            return false;

        if (ctxconfig.Client != GLFW_NO_API)
        {
            if (ctxconfig.Source == GLFW_NATIVE_CONTEXT_API)
            {
                if (!_glfwCreateContextGLX(window, ctxconfig, fbconfig))
                    return false;
            }
            else if (ctxconfig.Source == GLFW_EGL_CONTEXT_API)
            {
                if (!_glfwCreateContextEGL(window, ctxconfig, fbconfig)) return false;
            }
            else if (ctxconfig.Source == GLFW_OSMESA_CONTEXT_API)
            {
                if (!_glfwInitOSMesa() || !_glfwCreateContextOSMesa(window, ctxconfig, fbconfig)) return false;
            }

            if (!_glfwRefreshContextAttribs(window, ctxconfig))
                return false;
        }

        if (wndconfig.MousePassthrough)
            _glfwSetWindowMousePassthroughX11(window, true);

        if (window.Monitor != null)
        {
            _glfwShowWindowX11(window);
            updateWindowMode(window);
            acquireMonitorX11(window);

            if (wndconfig.CenterCursor)
                _glfwCenterCursorInContentArea(window);
        }
        else
        {
            if (wndconfig.Visible)
            {
                _glfwShowWindowX11(window);
                if (wndconfig.Focused)
                    _glfwFocusWindowX11(window);
            }
        }

        x11.xlib.Flush(x11.display);
        return true;
    }

    internal static unsafe void _glfwDestroyWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (x11.disabledCursorWindow == window)
            enableCursorX11(window);

        if (window.Monitor != null)
            releaseMonitorX11(window);

        if (window.X11 != null && window.X11.ic != 0)
        {
            x11.xlib.DestroyIC(window.X11.ic);
            window.X11.ic = 0;
        }

        if (window.X11 != null && window.X11.icHandle != 0)
        {
            System.Runtime.InteropServices.GCHandle.FromIntPtr(window.X11.icHandle).Free();
            window.X11.icHandle = 0;
        }

        if (window.context.destroy != null)
            window.context.destroy(window);

        if (window.X11 != null && window.X11.handle != 0)
        {
            x11.xlib.DeleteContext(x11.display, window.X11.handle, x11.context);
            x11.xlib.UnmapWindow(x11.display, window.X11.handle);
            x11.xlib.DestroyWindow(x11.display, window.X11.handle);
            window.X11.handle = 0;
        }

        if (window.X11 != null && window.X11.colormap != 0)
        {
            x11.xlib.FreeColormap(x11.display, window.X11.colormap);
            window.X11.colormap = 0;
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwSetWindowTitleX11(GlfwWindow window, string title)
    {
        var x11 = _glfw.X11!;
        byte[] titleBytes = Encoding.UTF8.GetBytes(title);

        if (x11.xlib.utf8)
        {
            fixed (byte* tp = titleBytes)
            {
                // null-terminated
                byte* tpz = stackalloc byte[titleBytes.Length + 1];
                new Span<byte>(tp, titleBytes.Length).CopyTo(new Span<byte>(tpz, titleBytes.Length));
                tpz[titleBytes.Length] = 0;
                x11.xlib.utf8SetWMProperties(x11.display, window.X11!.handle,
                                               tpz, tpz, null, 0, null, null, null);
            }
        }

        fixed (byte* tp = titleBytes)
        {
            x11.xlib.ChangeProperty(x11.display, window.X11!.handle,
                                     x11.NET_WM_NAME, x11.UTF8_STRING, 8,
                                     PropModeReplace, tp, titleBytes.Length);

            x11.xlib.ChangeProperty(x11.display, window.X11.handle,
                                     x11.NET_WM_ICON_NAME, x11.UTF8_STRING, 8,
                                     PropModeReplace, tp, titleBytes.Length);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwSetWindowIconX11(GlfwWindow window, int count, GlfwImage[]? images)
    {
        var x11 = _glfw.X11!;

        if (count > 0 && images != null)
        {
            int longCount = 0;
            for (int i = 0; i < count; i++)
                longCount += 2 + images[i].Width * images[i].Height;

            nuint[] icon = new nuint[longCount];
            int idx = 0;

            for (int i = 0; i < count; i++)
            {
                icon[idx++] = (nuint)images[i].Width;
                icon[idx++] = (nuint)images[i].Height;

                for (int j = 0; j < images[i].Width * images[i].Height; j++)
                {
                    var px = images[i].Pixels!;
                    icon[idx++] = ((nuint)px[j * 4 + 0] << 16) |
                                  ((nuint)px[j * 4 + 1] << 8) |
                                  ((nuint)px[j * 4 + 2] << 0) |
                                  ((nuint)px[j * 4 + 3] << 24);
                }
            }

            fixed (nuint* iconPtr = icon)
            {
                x11.xlib.ChangeProperty(x11.display, window.X11!.handle,
                                         x11.NET_WM_ICON, XA_CARDINAL, 32,
                                         PropModeReplace, (byte*)iconPtr, longCount);
            }
        }
        else
        {
            x11.xlib.DeleteProperty(x11.display, window.X11!.handle, x11.NET_WM_ICON);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwGetWindowPosX11(GlfwWindow window, out int xpos, out int ypos)
    {
        var x11 = _glfw.X11!;
        nuint dummy;
        int x, y;
        x11.xlib.TranslateCoordinates(x11.display, window.X11!.handle, x11.root,
                                        0, 0, &x, &y, &dummy);
        xpos = x;
        ypos = y;
    }

    internal static unsafe void _glfwSetWindowPosX11(GlfwWindow window, int xpos, int ypos)
    {
        var x11 = _glfw.X11!;

        if (!_glfwWindowVisibleX11(window))
        {
            nint hintsPtr = x11.xlib.AllocSizeHints();
            if (hintsPtr != 0)
            {
                XSizeHints* hints = (XSizeHints*)hintsPtr;
                nint supplied;
                if (x11.xlib.GetWMNormalHints(x11.display, window.X11!.handle, hints, &supplied) != 0)
                {
                    hints->flags = (nint)((long)hints->flags | (long)PPosition);
                    hints->x = hints->y = 0;
                    x11.xlib.SetWMNormalHints(x11.display, window.X11.handle, hints);
                }
                x11.xlib.Free(hintsPtr);
            }
        }

        x11.xlib.MoveWindow(x11.display, window.X11!.handle, xpos, ypos);
        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwGetWindowSizeX11(GlfwWindow window, out int width, out int height)
    {
        var x11 = _glfw.X11!;
        XWindowAttributes attribs;
        x11.xlib.GetWindowAttributes(x11.display, window.X11!.handle, &attribs);
        width = attribs.width;
        height = attribs.height;
    }

    internal static unsafe void _glfwSetWindowSizeX11(GlfwWindow window, int width, int height)
    {
        var x11 = _glfw.X11!;
        width = Math.Max(1, width);
        height = Math.Max(1, height);

        if (window.Monitor != null)
        {
            if (window.Monitor.Window == window)
                acquireMonitorX11(window);
        }
        else
        {
            if (!window.Resizable)
                updateNormalHints(window, width, height);

            x11.xlib.ResizeWindow(x11.display, window.X11!.handle, (uint)width, (uint)height);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwSetWindowSizeLimitsX11(GlfwWindow window,
                                                              int minwidth, int minheight,
                                                              int maxwidth, int maxheight)
    {
        _glfwGetWindowSizeX11(window, out int w, out int h);
        updateNormalHints(window, w, h);
        _glfw.X11!.xlib.Flush(_glfw.X11.display);
    }

    internal static unsafe void _glfwSetWindowAspectRatioX11(GlfwWindow window, int numer, int denom)
    {
        _glfwGetWindowSizeX11(window, out int w, out int h);
        updateNormalHints(window, w, h);
        _glfw.X11!.xlib.Flush(_glfw.X11.display);
    }

    internal static unsafe void _glfwGetFramebufferSizeX11(GlfwWindow window, out int width, out int height)
    {
        _glfwGetWindowSizeX11(window, out width, out height);
    }

    internal static unsafe void _glfwGetWindowFrameSizeX11(GlfwWindow window,
                                                            out int left, out int top,
                                                            out int right, out int bottom)
    {
        left = top = right = bottom = 0;
        var x11 = _glfw.X11!;

        if (window.Monitor != null || !window.Decorated)
            return;

        if (x11.NET_FRAME_EXTENTS == None_)
            return;

        if (!_glfwWindowVisibleX11(window) && x11.NET_REQUEST_FRAME_EXTENTS != 0)
        {
            XEvent ev;
            double timeout = 0.5;
            sendEventToWM(window, x11.NET_REQUEST_FRAME_EXTENTS, 0, 0, 0, 0, 0);
            nint predicate = (nint)(delegate* unmanaged[Cdecl]<nint, XEvent*, nint, int>)&isFrameExtentsEvent;
            while (x11.xlib.CheckIfEvent(x11.display, &ev, predicate, (nint)window.X11!.handle) == 0)
            {
                if (!waitForX11Event(&timeout))
                {
                    _glfwInputError(GLFW_PLATFORM_ERROR, "X11: The window manager has a broken _NET_REQUEST_FRAME_EXTENTS implementation; please report this issue");
                    return;
                }
            }
        }

        byte* extents = null;
        nuint count = _glfwGetWindowPropertyX11(window.X11!.handle,
                                                 x11.NET_FRAME_EXTENTS, XA_CARDINAL,
                                                 &extents);

        if (count == 4 && extents != null)
        {
            nint* vals = (nint*)extents;
            left   = (int)vals[0];
            right  = (int)vals[1];
            top    = (int)vals[2];
            bottom = (int)vals[3];
        }

        if (extents != null)
            x11.xlib.Free((nint)extents);
    }

    internal static void _glfwGetWindowContentScaleX11(GlfwWindow window, out float xscale, out float yscale)
    {
        var x11 = _glfw.X11!;
        xscale = x11.contentScaleX;
        yscale = x11.contentScaleY;
    }

    internal static unsafe void _glfwIconifyWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.X11!.overrideRedirect)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "X11: Iconification of full screen windows requires a WM that supports EWMH full screen");
            return;
        }

        x11.xlib.IconifyWindow(x11.display, window.X11.handle, x11.screen);
        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwRestoreWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (window.X11!.overrideRedirect)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "X11: Iconification of full screen windows requires a WM that supports EWMH full screen");
            return;
        }

        if (_glfwWindowIconifiedX11(window))
        {
            x11.xlib.MapWindow(x11.display, window.X11.handle);
            waitForVisibilityNotify(window);
        }
        else if (_glfwWindowVisibleX11(window))
        {
            if (x11.NET_WM_STATE != 0 &&
                x11.NET_WM_STATE_MAXIMIZED_VERT != 0 &&
                x11.NET_WM_STATE_MAXIMIZED_HORZ != 0)
            {
                sendEventToWM(window, x11.NET_WM_STATE,
                              _NET_WM_STATE_REMOVE,
                              (long)x11.NET_WM_STATE_MAXIMIZED_VERT,
                              (long)x11.NET_WM_STATE_MAXIMIZED_HORZ,
                              1, 0);
            }
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwMaximizeWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (x11.NET_WM_STATE == 0 ||
            x11.NET_WM_STATE_MAXIMIZED_VERT == 0 ||
            x11.NET_WM_STATE_MAXIMIZED_HORZ == 0)
            return;

        if (_glfwWindowVisibleX11(window))
        {
            sendEventToWM(window, x11.NET_WM_STATE,
                          _NET_WM_STATE_ADD,
                          (long)x11.NET_WM_STATE_MAXIMIZED_VERT,
                          (long)x11.NET_WM_STATE_MAXIMIZED_HORZ,
                          1, 0);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwShowWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (_glfwWindowVisibleX11(window))
            return;

        x11.xlib.MapWindow(x11.display, window.X11!.handle);
        waitForVisibilityNotify(window);
    }

    internal static unsafe void _glfwHideWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        x11.xlib.UnmapWindow(x11.display, window.X11!.handle);
        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwRequestWindowAttentionX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        if (x11.NET_WM_STATE == 0 || x11.NET_WM_STATE_DEMANDS_ATTENTION == 0)
            return;

        sendEventToWM(window, x11.NET_WM_STATE,
                      _NET_WM_STATE_ADD,
                      (long)x11.NET_WM_STATE_DEMANDS_ATTENTION,
                      0, 1, 0);
    }

    internal static unsafe void _glfwFocusWindowX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;

        if (x11.NET_ACTIVE_WINDOW != 0)
            sendEventToWM(window, x11.NET_ACTIVE_WINDOW, 1, 0, 0, 0, 0);
        else if (_glfwWindowVisibleX11(window))
        {
            x11.xlib.RaiseWindow(x11.display, window.X11!.handle);
            x11.xlib.SetInputFocus(x11.display, window.X11.handle,
                                    RevertToParent, CurrentTime);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwSetWindowMonitorX11(GlfwWindow window,
                                                          GlfwMonitor? monitor,
                                                          int xpos, int ypos,
                                                          int width, int height,
                                                          int refreshRate)
    {
        var x11 = _glfw.X11!;

        if (window.Monitor == monitor)
        {
            if (monitor != null)
            {
                if (monitor.Window == window)
                    acquireMonitorX11(window);
            }
            else
            {
                if (!window.Resizable)
                    updateNormalHints(window, width, height);

                x11.xlib.MoveResizeWindow(x11.display, window.X11!.handle,
                                           xpos, ypos, (uint)width, (uint)height);
            }

            x11.xlib.Flush(x11.display);
            return;
        }

        if (window.Monitor != null)
        {
            _glfwSetWindowDecoratedX11(window, window.Decorated);
            _glfwSetWindowFloatingX11(window, window.Floating);
            releaseMonitorX11(window);
        }

        _glfwInputWindowMonitor(window, monitor);
        updateNormalHints(window, width, height);

        if (window.Monitor != null)
        {
            if (!_glfwWindowVisibleX11(window))
            {
                x11.xlib.MapRaised(x11.display, window.X11!.handle);
                waitForVisibilityNotify(window);
            }

            updateWindowMode(window);
            acquireMonitorX11(window);
        }
        else
        {
            updateWindowMode(window);
            x11.xlib.MoveResizeWindow(x11.display, window.X11!.handle,
                                       xpos, ypos, (uint)width, (uint)height);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe bool _glfwWindowFocusedX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        nuint focused;
        int state;
        x11.xlib.GetInputFocus(x11.display, &focused, &state);
        return window.X11!.handle == focused;
    }

    internal static bool _glfwWindowIconifiedX11(GlfwWindow window)
    {
        return getWindowState(window) == IconicState;
    }

    internal static unsafe bool _glfwWindowVisibleX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        XWindowAttributes wa;
        x11.xlib.GetWindowAttributes(x11.display, window.X11!.handle, &wa);
        return wa.map_state == IsViewable;
    }

    internal static unsafe bool _glfwWindowMaximizedX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        bool maximized = false;

        if (x11.NET_WM_STATE == 0 ||
            x11.NET_WM_STATE_MAXIMIZED_VERT == 0 ||
            x11.NET_WM_STATE_MAXIMIZED_HORZ == 0)
            return false;

        byte* statesRaw = null;
        nuint count = _glfwGetWindowPropertyX11(window.X11!.handle,
                                                 x11.NET_WM_STATE, XA_ATOM, &statesRaw);

        nuint* states = (nuint*)statesRaw;
        for (nuint i = 0; i < count; i++)
        {
            if (states[i] == x11.NET_WM_STATE_MAXIMIZED_VERT ||
                states[i] == x11.NET_WM_STATE_MAXIMIZED_HORZ)
            {
                maximized = true;
                break;
            }
        }

        if (statesRaw != null)
            x11.xlib.Free((nint)statesRaw);

        return maximized;
    }

    internal static unsafe bool _glfwWindowHoveredX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        nuint w = x11.root;

        while (w != 0)
        {
            nuint rootw;
            int rootX, rootY, childX, childY;
            uint mask;

            _glfwGrabErrorHandlerX11();

            int result = x11.xlib.QueryPointer(x11.display, w,
                                                &rootw, &w, &rootX, &rootY,
                                                &childX, &childY, &mask);

            _glfwReleaseErrorHandlerX11();

            if (x11.errorCode == BadWindow)
                w = x11.root;
            else if (result == 0)
                return false;
            else if (w == window.X11!.handle)
                return true;
        }

        return false;
    }

    internal static unsafe bool _glfwFramebufferTransparentX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        if (!window.X11!.transparent)
            return false;

        return x11.xlib.GetSelectionOwner(x11.display, x11.NET_WM_CM_Sx) != None_;
    }

    internal static unsafe void _glfwSetWindowResizableX11(GlfwWindow window, bool enabled)
    {
        _glfwGetWindowSizeX11(window, out int w, out int h);
        updateNormalHints(window, w, h);
    }

    internal static unsafe void _glfwSetWindowDecoratedX11(GlfwWindow window, bool enabled)
    {
        var x11 = _glfw.X11!;

        // Motif WM hints structure (5 unsigned longs)
        nuint* mwmHints = stackalloc nuint[5];
        mwmHints[0] = (nuint)MWM_HINTS_DECORATIONS; // flags
        mwmHints[1] = 0; // functions
        mwmHints[2] = enabled ? (nuint)MWM_DECOR_ALL : 0; // decorations
        mwmHints[3] = 0; // input_mode
        mwmHints[4] = 0; // status

        x11.xlib.ChangeProperty(x11.display, window.X11!.handle,
                                 x11.MOTIF_WM_HINTS, x11.MOTIF_WM_HINTS, 32,
                                 PropModeReplace, (byte*)mwmHints, 5);
    }

    internal static unsafe void _glfwSetWindowFloatingX11(GlfwWindow window, bool enabled)
    {
        var x11 = _glfw.X11!;

        if (x11.NET_WM_STATE == 0 || x11.NET_WM_STATE_ABOVE == 0)
            return;

        if (_glfwWindowVisibleX11(window))
        {
            long action = enabled ? _NET_WM_STATE_ADD : _NET_WM_STATE_REMOVE;
            sendEventToWM(window, x11.NET_WM_STATE, action,
                          (long)x11.NET_WM_STATE_ABOVE, 0, 1, 0);
        }

        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwSetWindowMousePassthroughX11(GlfwWindow window, bool enabled)
    {
        var x11 = _glfw.X11!;

        if (!x11.xshape.available)
            return;

        if (enabled)
        {
            nint region = x11.xlib.CreateRegion();
            x11.xshape.ShapeCombineRegion(x11.display, window.X11!.handle,
                                            ShapeInput, 0, 0, region, ShapeSet);
            x11.xlib.DestroyRegion(region);
        }
        else
        {
            x11.xshape.ShapeCombineMask(x11.display, window.X11!.handle,
                                          ShapeInput, 0, 0, None_, ShapeSet);
        }
    }

    internal static unsafe float _glfwGetWindowOpacityX11(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        float opacity = 1.0f;

        if (x11.xlib.GetSelectionOwner(x11.display, x11.NET_WM_CM_Sx) != 0)
        {
            byte* value = null;
            nuint count = _glfwGetWindowPropertyX11(window.X11!.handle,
                                                     x11.NET_WM_WINDOW_OPACITY,
                                                     XA_CARDINAL, &value);
            if (count > 0 && value != null)
            {
                uint val = *(uint*)value;
                opacity = (float)(val / (double)0xffffffffu);
            }

            if (value != null)
                x11.xlib.Free((nint)value);
        }

        return opacity;
    }

    internal static unsafe void _glfwSetWindowOpacityX11(GlfwWindow window, float opacity)
    {
        var x11 = _glfw.X11!;
        uint value = (uint)(0xffffffffu * (double)opacity);
        x11.xlib.ChangeProperty(x11.display, window.X11!.handle,
                                 x11.NET_WM_WINDOW_OPACITY, XA_CARDINAL, 32,
                                 PropModeReplace, (byte*)&value, 1);
    }

    internal static unsafe void _glfwSetRawMouseMotionX11(GlfwWindow window, bool enabled)
    {
        var x11 = _glfw.X11!;

        if (!x11.xi.available)
            return;

        if (x11.disabledCursorWindow != window)
            return;

        if (enabled)
            enableRawMouseMotionX11(window);
        else
            disableRawMouseMotionX11(window);
    }

    internal static bool _glfwRawMouseMotionSupportedX11()
    {
        return _glfw.X11!.xi.available;
    }

    internal static unsafe void _glfwPollEventsX11()
    {
        var x11 = _glfw.X11!;

        drainEmptyEvents();

        if (_glfw.joysticksInitialized)
            _glfwDetectJoystickConnectionLinux();

        x11.xlib.Pending(x11.display);

        while (x11.xlib.QLength(x11.display) > 0)
        {
            XEvent ev;
            x11.xlib.NextEvent(x11.display, &ev);
            processEvent(&ev);
        }

        GlfwWindow? dcw = x11.disabledCursorWindow;
        if (dcw != null)
        {
            _glfwGetWindowSizeX11(dcw, out int w, out int h);

            if (dcw.X11!.lastCursorPosX != w / 2 ||
                dcw.X11.lastCursorPosY != h / 2)
            {
                _glfwSetCursorPosX11(dcw, w / 2, h / 2);
            }
        }

        x11.xlib.Flush(x11.display);
    }

    internal static void _glfwWaitEventsX11()
    {
        unsafe { waitForAnyEvent(null); }
        _glfwPollEventsX11();
    }

    internal static unsafe void _glfwWaitEventsTimeoutX11(double timeout)
    {
        waitForAnyEvent(&timeout);
        _glfwPollEventsX11();
    }

    internal static void _glfwPostEmptyEventX11()
    {
        writeEmptyEvent();
    }

    internal static unsafe void _glfwGetCursorPosX11(GlfwWindow window, out double xpos, out double ypos)
    {
        var x11 = _glfw.X11!;
        nuint rootw, child;
        int rootX, rootY, childX, childY;
        uint mask;

        x11.xlib.QueryPointer(x11.display, window.X11!.handle,
                               &rootw, &child, &rootX, &rootY,
                               &childX, &childY, &mask);
        xpos = childX;
        ypos = childY;
    }

    internal static unsafe void _glfwSetCursorPosX11(GlfwWindow window, double x, double y)
    {
        var x11 = _glfw.X11!;

        window.X11!.warpCursorPosX = (int)x;
        window.X11.warpCursorPosY = (int)y;

        x11.xlib.WarpPointer(x11.display, None_, window.X11.handle,
                              0, 0, 0, 0, (int)x, (int)y);
        x11.xlib.Flush(x11.display);
    }

    internal static unsafe void _glfwSetCursorModeX11(GlfwWindow window, int mode)
    {
        var x11 = _glfw.X11!;

        if (_glfwWindowFocusedX11(window))
        {
            if (mode == GLFW_CURSOR_DISABLED)
            {
                _glfwGetCursorPosX11(window, out x11.restoreCursorPosX, out x11.restoreCursorPosY);
                _glfwCenterCursorInContentArea(window);
                if (window.RawMouseMotion)
                    enableRawMouseMotionX11(window);
            }
            else if (x11.disabledCursorWindow == window)
            {
                if (window.RawMouseMotion)
                    disableRawMouseMotionX11(window);
            }

            if (mode == GLFW_CURSOR_DISABLED || mode == GLFW_CURSOR_CAPTURED)
                captureCursorX11(window);
            else
                releaseCursorX11();

            if (mode == GLFW_CURSOR_DISABLED)
                x11.disabledCursorWindow = window;
            else if (x11.disabledCursorWindow == window)
            {
                x11.disabledCursorWindow = null;
                _glfwSetCursorPosX11(window, x11.restoreCursorPosX, x11.restoreCursorPosY);
            }
        }

        updateCursorImageX11(window);
        x11.xlib.Flush(x11.display);
    }

    internal static unsafe string? _glfwGetScancodeNameX11(int scancode)
    {
        var x11 = _glfw.X11!;

        if (!x11.xkb.available)
            return null;

        if (scancode < 0 || scancode > 0xff)
        {
            _glfwInputError(GLFW_INVALID_VALUE, "Invalid scancode " + scancode);
            return null;
        }

        int key = x11.keycodes[scancode];
        if (key == GLFW_KEY_UNKNOWN)
            return null;

        nuint keysym = x11.xkb.KeycodeToKeysym(x11.display, (byte)scancode, (int)x11.xkb.group, 0);
        if (keysym == NoSymbol)
            return null;

        uint codepoint = XkbUnicode._glfwKeySym2UnicodeX11((uint)keysym);
        if (codepoint == XkbUnicode.GLFW_INVALID_CODEPOINT)
            return null;

        byte[] buf = new byte[5];
        int count = _glfwEncodeUTF8(buf, 0, codepoint);
        if (count == 0)
            return null;

        string name = Encoding.UTF8.GetString(buf, 0, count);
        x11.keynames[key] = name;
        return name;
    }

    internal static int _glfwGetKeyScancodeX11(int key)
    {
        return _glfw.X11!.scancodes[key];
    }

    internal static unsafe bool _glfwCreateCursorX11(GlfwCursor cursor,
                                                      in GlfwImage image,
                                                      int xhot, int yhot)
    {
        cursor.X11 = new GlfwCursorX11();
        cursor.X11.handle = _glfwCreateNativeCursorX11(in image, xhot, yhot);
        return cursor.X11.handle != 0;
    }

    // Creates a native cursor from image data
    internal static unsafe nuint _glfwCreateNativeCursorX11(in GlfwImage image, int xhot, int yhot)
    {
        var x11 = _glfw.X11!;

        if (x11.xcursor.handle != 0 && x11.xcursor.ImageCreate != null)
        {
            nint cursorImage = x11.xcursor.ImageCreate(image.Width, image.Height);
            if (cursorImage == 0)
                return 0;

            // XcursorImage layout: version(4), size(4), width(4), height(4),
            //   xhot(4), yhot(4), delay(4), pixels(ptr)
            int* ip = (int*)cursorImage;
            ip[4] = xhot; // xhot
            ip[5] = yhot; // yhot

            // pixels pointer is at offset 7*4 = 28 on 32-bit, but on 64-bit
            // the pixels pointer is at a different offset due to alignment
            // XcursorImage: uint version, uint size, uint width, uint height,
            //               uint xhot, uint yhot, uint delay, uint *pixels
            nint pixelsPtr = *(nint*)(((byte*)cursorImage) + 7 * sizeof(uint));
            // Actually the struct is: 7 uints then a pointer
            // On 64-bit: 7*4 = 28 bytes, then padding to 8-byte boundary = 32, then pointer
            nint offset = 7 * 4;
            if (nint.Size == 8)
                offset = 32; // aligned
            uint* pixels = *(uint**)(((byte*)cursorImage) + offset);

            if (pixels != null && image.Pixels != null)
            {
                for (int i = 0; i < image.Width * image.Height; i++)
                {
                    pixels[i] = ((uint)image.Pixels[i * 4 + 3] << 24) |
                                ((uint)image.Pixels[i * 4 + 0] << 16) |
                                ((uint)image.Pixels[i * 4 + 1] << 8) |
                                ((uint)image.Pixels[i * 4 + 2] << 0);
                }
            }

            nuint handle = x11.xcursor.ImageLoadCursor(x11.display, cursorImage);
            x11.xcursor.ImageDestroy(cursorImage);
            return handle;
        }

        return 0;
    }

    internal static unsafe bool _glfwCreateStandardCursorX11(GlfwCursor cursor, int shape)
    {
        var x11 = _glfw.X11!;
        cursor.X11 = new GlfwCursorX11();

        if (x11.xcursor.handle != 0)
        {
            nint themePtr = x11.xcursor.GetTheme(x11.display);
            if (themePtr != 0)
            {
                int size = x11.xcursor.GetDefaultSize(x11.display);
                string? name = shape switch
                {
                    GLFW_ARROW_CURSOR => "default",
                    GLFW_IBEAM_CURSOR => "text",
                    GLFW_CROSSHAIR_CURSOR => "crosshair",
                    GLFW_POINTING_HAND_CURSOR => "pointer",
                    GLFW_RESIZE_EW_CURSOR => "ew-resize",
                    GLFW_RESIZE_NS_CURSOR => "ns-resize",
                    GLFW_RESIZE_NWSE_CURSOR => "nwse-resize",
                    GLFW_RESIZE_NESW_CURSOR => "nesw-resize",
                    GLFW_RESIZE_ALL_CURSOR => "all-scroll",
                    GLFW_NOT_ALLOWED_CURSOR => "not-allowed",
                    _ => null
                };

                if (name != null)
                {
                    byte[] nameBytes = Encoding.UTF8.GetBytes(name + "\0");
                    nint themeStr = themePtr;

                    fixed (byte* nb = nameBytes)
                    {
                        nint image = x11.xcursor.LibraryLoadImage(nb, (byte*)themeStr, size);
                        if (image != 0)
                        {
                            cursor.X11.handle = x11.xcursor.ImageLoadCursor(x11.display, image);
                            x11.xcursor.ImageDestroy(image);
                        }
                    }
                }
            }
        }

        if (cursor.X11.handle == 0)
        {
            uint native = shape switch
            {
                GLFW_ARROW_CURSOR => XC_left_ptr,
                GLFW_IBEAM_CURSOR => XC_xterm,
                GLFW_CROSSHAIR_CURSOR => XC_crosshair,
                GLFW_POINTING_HAND_CURSOR => XC_hand2,
                GLFW_RESIZE_EW_CURSOR => XC_sb_h_double_arrow,
                GLFW_RESIZE_NS_CURSOR => XC_sb_v_double_arrow,
                GLFW_RESIZE_ALL_CURSOR => XC_fleur,
                _ => 0
            };

            if (native == 0)
            {
                _glfwInputError(GLFW_CURSOR_UNAVAILABLE,
                                "X11: Standard cursor shape unavailable");
                return false;
            }

            cursor.X11.handle = x11.xlib.CreateFontCursor(x11.display, native);
            if (cursor.X11.handle == 0)
            {
                _glfwInputError(GLFW_PLATFORM_ERROR,
                                "X11: Failed to create standard cursor");
                return false;
            }
        }

        return true;
    }

    internal static unsafe void _glfwDestroyCursorX11(GlfwCursor cursor)
    {
        var x11 = _glfw.X11!;
        if (cursor.X11 != null && cursor.X11.handle != 0)
            x11.xlib.FreeCursor(x11.display, cursor.X11.handle);
    }

    internal static unsafe void _glfwSetCursorX11(GlfwWindow window, GlfwCursor? cursor)
    {
        if (window.CursorMode == GLFW_CURSOR_NORMAL ||
            window.CursorMode == GLFW_CURSOR_CAPTURED)
        {
            updateCursorImageX11(window);
            _glfw.X11!.xlib.Flush(_glfw.X11.display);
        }
    }

    internal static unsafe void _glfwSetClipboardStringX11(string text)
    {
        var x11 = _glfw.X11!;
        x11.clipboardString = text;

        x11.xlib.SetSelectionOwner(x11.display, x11.CLIPBOARD,
                                    x11.helperWindowHandle, CurrentTime);

        if (x11.xlib.GetSelectionOwner(x11.display, x11.CLIPBOARD) != x11.helperWindowHandle)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "X11: Failed to become owner of clipboard selection");
        }
    }

    internal static string? _glfwGetClipboardStringX11()
    {
        return getSelectionString(_glfw.X11!.CLIPBOARD);
    }
} // end partial class Glfw

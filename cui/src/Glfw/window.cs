// GLFW 3.6 - www.glfw.org
// Ported from glfw/src/window.c
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2019 Camilla Loewy <elmindreda@glfw.org>
// Copyright (c) 2012 Torsten Walluhn <tw@mad-cad.net>
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
using System.Diagnostics;

namespace Glfw;

public static partial class Glfw
{
    //////////////////////////////////////////////////////////////////////////
    //////                         GLFW event API                       //////
    //////////////////////////////////////////////////////////////////////////

    // Notifies shared code that a window has lost or received input focus
    //
    internal static void _glfwInputWindowFocus(GlfwWindow window, int focused)
    {
        Debug.Assert(window != null);
        Debug.Assert(focused == GLFW.GLFW_TRUE || focused == GLFW.GLFW_FALSE);

        window.Callbacks.Focus?.Invoke(window, focused);

        if (focused == GLFW.GLFW_FALSE)
        {
            for (int key = 0; key <= GLFW.GLFW_KEY_LAST; key++)
            {
                if (window.Keys[key] == GLFW.GLFW_PRESS)
                {
                    int scancode = _glfw.platform!.GetKeyScancode(key);
                    _glfwInputKey(window, key, scancode, GLFW.GLFW_RELEASE, 0);
                }
            }

            for (int button = 0; button <= GLFW.GLFW_MOUSE_BUTTON_LAST; button++)
            {
                if (window.MouseButtons[button] == GLFW.GLFW_PRESS)
                    _glfwInputMouseClick(window, button, GLFW.GLFW_RELEASE, 0);
            }
        }
    }

    // Notifies shared code that a window has moved
    // The position is specified in content area relative screen coordinates
    //
    internal static void _glfwInputWindowPos(GlfwWindow window, int x, int y)
    {
        Debug.Assert(window != null);

        window.Callbacks.Pos?.Invoke(window, x, y);
    }

    // Notifies shared code that a window has been resized
    // The size is specified in screen coordinates
    //
    internal static void _glfwInputWindowSize(GlfwWindow window, int width, int height)
    {
        Debug.Assert(window != null);
        Debug.Assert(width >= 0);
        Debug.Assert(height >= 0);

        window.Callbacks.Size?.Invoke(window, width, height);
    }

    // Notifies shared code that a window has been iconified or restored
    //
    internal static void _glfwInputWindowIconify(GlfwWindow window, int iconified)
    {
        Debug.Assert(window != null);
        Debug.Assert(iconified == GLFW.GLFW_TRUE || iconified == GLFW.GLFW_FALSE);

        window.Callbacks.Iconify?.Invoke(window, iconified);
    }

    // Notifies shared code that a window has been maximized or restored
    //
    internal static void _glfwInputWindowMaximize(GlfwWindow window, int maximized)
    {
        Debug.Assert(window != null);
        Debug.Assert(maximized == GLFW.GLFW_TRUE || maximized == GLFW.GLFW_FALSE);

        window.Callbacks.Maximize?.Invoke(window, maximized);
    }

    // Notifies shared code that a window framebuffer has been resized
    // The size is specified in pixels
    //
    internal static void _glfwInputFramebufferSize(GlfwWindow window, int width, int height)
    {
        Debug.Assert(window != null);
        Debug.Assert(width >= 0);
        Debug.Assert(height >= 0);

        window.Callbacks.Fbsize?.Invoke(window, width, height);
    }

    // Notifies shared code that a window content scale has changed
    // The scale is specified as the ratio between the current and default DPI
    //
    internal static void _glfwInputWindowContentScale(GlfwWindow window, float xscale, float yscale)
    {
        Debug.Assert(window != null);
        Debug.Assert(xscale > 0f);
        Debug.Assert(float.IsFinite(xscale));
        Debug.Assert(yscale > 0f);
        Debug.Assert(float.IsFinite(yscale));

        window.Callbacks.Scale?.Invoke(window, xscale, yscale);
    }

    // Notifies shared code that the window contents needs updating
    //
    internal static void _glfwInputWindowDamage(GlfwWindow window)
    {
        Debug.Assert(window != null);

        window.Callbacks.Refresh?.Invoke(window);
    }

    // Notifies shared code that the user wishes to close a window
    //
    internal static void _glfwInputWindowCloseRequest(GlfwWindow window)
    {
        Debug.Assert(window != null);

        window.ShouldClose = true;

        window.Callbacks.Close?.Invoke(window);
    }

    // Notifies shared code that a window has changed its desired monitor
    //
    internal static void _glfwInputWindowMonitor(GlfwWindow window, GlfwMonitor? monitor)
    {
        Debug.Assert(window != null);
        window.Monitor = monitor;
    }

    // _glfwIsValidContextConfig is defined in context.cs

    //////////////////////////////////////////////////////////////////////////
    //////                        GLFW public API                       //////
    //////////////////////////////////////////////////////////////////////////

    public static GlfwWindow? glfwCreateWindow(int width, int height,
                                               string title,
                                               GlfwMonitor? monitor,
                                               GlfwWindow? share)
    {
        Debug.Assert(title != null);
        Debug.Assert(width >= 0);
        Debug.Assert(height >= 0);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (width <= 0 || height <= 0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                            "Invalid window size {0}x{1}",
                            width, height);
            return null;
        }

        var fbconfig  = _glfw.hints.Framebuffer.Clone();
        var ctxconfig = _glfw.hints.Context.Clone();
        var wndconfig = _glfw.hints.Window.Clone();

        wndconfig.Width  = width;
        wndconfig.Height = height;
        ctxconfig.Share  = share;

        if (!_glfwIsValidContextConfig(ctxconfig))
            return null;

        var window = new GlfwWindow();
        window.Next = _glfw.windowListHead;
        _glfw.windowListHead = window;

        window.VideoMode.Width       = width;
        window.VideoMode.Height      = height;
        window.VideoMode.RedBits     = fbconfig.RedBits;
        window.VideoMode.GreenBits   = fbconfig.GreenBits;
        window.VideoMode.BlueBits    = fbconfig.BlueBits;
        window.VideoMode.RefreshRate = _glfw.hints.RefreshRate;

        window.Monitor          = monitor;
        window.Resizable        = wndconfig.Resizable;
        window.Decorated        = wndconfig.Decorated;
        window.AutoIconify      = wndconfig.AutoIconify;
        window.Floating         = wndconfig.Floating;
        window.FocusOnShow      = wndconfig.FocusOnShow;
        window.MousePassthrough = wndconfig.MousePassthrough;
        window.CursorMode       = GLFW.GLFW_CURSOR_NORMAL;

        window.doublebuffer = fbconfig.Doublebuffer;

        window.MinWidth  = GLFW.GLFW_DONT_CARE;
        window.MinHeight = GLFW.GLFW_DONT_CARE;
        window.MaxWidth  = GLFW.GLFW_DONT_CARE;
        window.MaxHeight = GLFW.GLFW_DONT_CARE;
        window.Numer     = GLFW.GLFW_DONT_CARE;
        window.Denom     = GLFW.GLFW_DONT_CARE;
        window.Title     = title;

        if (!_glfw.platform!.CreateWindow(window, wndconfig, ctxconfig, fbconfig))
        {
            glfwDestroyWindow(window);
            return null;
        }

        return window;
    }

    public static void glfwDefaultWindowHints()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        // The default is OpenGL with minimum version 1.0
        _glfw.hints.Context = new GlfwCtxConfig
        {
            Client = GLFW.GLFW_OPENGL_API,
            Source = GLFW.GLFW_NATIVE_CONTEXT_API,
            Major  = 1,
            Minor  = 0,
        };

        // The default is a focused, visible, resizable window with decorations
        _glfw.hints.Window = new GlfwWndConfig
        {
            Resizable        = true,
            Visible          = true,
            Decorated        = true,
            Focused          = true,
            AutoIconify      = true,
            CenterCursor     = true,
            FocusOnShow      = true,
            Xpos             = GLFW.GLFW_ANY_POSITION,
            Ypos             = GLFW.GLFW_ANY_POSITION,
            ScaleFramebuffer = true,
        };

        // The default is 24 bits of color, 24 bits of depth and 8 bits of stencil,
        // double buffered
        _glfw.hints.Framebuffer = new GlfwFbConfig
        {
            RedBits      = 8,
            GreenBits    = 8,
            BlueBits     = 8,
            AlphaBits    = 8,
            DepthBits    = 24,
            StencilBits  = 8,
            Doublebuffer = true,
        };

        // The default is to select the highest available refresh rate
        _glfw.hints.RefreshRate = GLFW.GLFW_DONT_CARE;
    }

    public static void glfwWindowHint(int hint, int value)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        switch (hint)
        {
            case GLFW.GLFW_RED_BITS:
                _glfw.hints.Framebuffer.RedBits = value;
                return;
            case GLFW.GLFW_GREEN_BITS:
                _glfw.hints.Framebuffer.GreenBits = value;
                return;
            case GLFW.GLFW_BLUE_BITS:
                _glfw.hints.Framebuffer.BlueBits = value;
                return;
            case GLFW.GLFW_ALPHA_BITS:
                _glfw.hints.Framebuffer.AlphaBits = value;
                return;
            case GLFW.GLFW_DEPTH_BITS:
                _glfw.hints.Framebuffer.DepthBits = value;
                return;
            case GLFW.GLFW_STENCIL_BITS:
                _glfw.hints.Framebuffer.StencilBits = value;
                return;
            case GLFW.GLFW_ACCUM_RED_BITS:
                _glfw.hints.Framebuffer.AccumRedBits = value;
                return;
            case GLFW.GLFW_ACCUM_GREEN_BITS:
                _glfw.hints.Framebuffer.AccumGreenBits = value;
                return;
            case GLFW.GLFW_ACCUM_BLUE_BITS:
                _glfw.hints.Framebuffer.AccumBlueBits = value;
                return;
            case GLFW.GLFW_ACCUM_ALPHA_BITS:
                _glfw.hints.Framebuffer.AccumAlphaBits = value;
                return;
            case GLFW.GLFW_AUX_BUFFERS:
                _glfw.hints.Framebuffer.AuxBuffers = value;
                return;
            case GLFW.GLFW_STEREO:
                _glfw.hints.Framebuffer.Stereo = value != 0;
                return;
            case GLFW.GLFW_DOUBLEBUFFER:
                _glfw.hints.Framebuffer.Doublebuffer = value != 0;
                return;
            case GLFW.GLFW_TRANSPARENT_FRAMEBUFFER:
                _glfw.hints.Framebuffer.Transparent = value != 0;
                return;
            case GLFW.GLFW_SAMPLES:
                _glfw.hints.Framebuffer.Samples = value;
                return;
            case GLFW.GLFW_SRGB_CAPABLE:
                _glfw.hints.Framebuffer.SRGB = value != 0;
                return;
            case GLFW.GLFW_RESIZABLE:
                _glfw.hints.Window.Resizable = value != 0;
                return;
            case GLFW.GLFW_DECORATED:
                _glfw.hints.Window.Decorated = value != 0;
                return;
            case GLFW.GLFW_FOCUSED:
                _glfw.hints.Window.Focused = value != 0;
                return;
            case GLFW.GLFW_AUTO_ICONIFY:
                _glfw.hints.Window.AutoIconify = value != 0;
                return;
            case GLFW.GLFW_FLOATING:
                _glfw.hints.Window.Floating = value != 0;
                return;
            case GLFW.GLFW_MAXIMIZED:
                _glfw.hints.Window.Maximized = value != 0;
                return;
            case GLFW.GLFW_VISIBLE:
                _glfw.hints.Window.Visible = value != 0;
                return;
            case GLFW.GLFW_POSITION_X:
                _glfw.hints.Window.Xpos = value;
                return;
            case GLFW.GLFW_POSITION_Y:
                _glfw.hints.Window.Ypos = value;
                return;
            case GLFW.GLFW_WIN32_KEYBOARD_MENU:
                _glfw.hints.Window.Win32.Keymenu = value != 0;
                return;
            case GLFW.GLFW_WIN32_SHOWDEFAULT:
                _glfw.hints.Window.Win32.ShowDefault = value != 0;
                return;
            case GLFW.GLFW_COCOA_GRAPHICS_SWITCHING:
                _glfw.hints.Context.Nsgl.Offline = value != 0;
                return;
            case GLFW.GLFW_SCALE_TO_MONITOR:
                _glfw.hints.Window.ScaleToMonitor = value != 0;
                return;
            case GLFW.GLFW_SCALE_FRAMEBUFFER:
            case GLFW.GLFW_COCOA_RETINA_FRAMEBUFFER:
                _glfw.hints.Window.ScaleFramebuffer = value != 0;
                return;
            case GLFW.GLFW_CENTER_CURSOR:
                _glfw.hints.Window.CenterCursor = value != 0;
                return;
            case GLFW.GLFW_FOCUS_ON_SHOW:
                _glfw.hints.Window.FocusOnShow = value != 0;
                return;
            case GLFW.GLFW_MOUSE_PASSTHROUGH:
                _glfw.hints.Window.MousePassthrough = value != 0;
                return;
            case GLFW.GLFW_CLIENT_API:
                _glfw.hints.Context.Client = value;
                return;
            case GLFW.GLFW_CONTEXT_CREATION_API:
                _glfw.hints.Context.Source = value;
                return;
            case GLFW.GLFW_CONTEXT_VERSION_MAJOR:
                _glfw.hints.Context.Major = value;
                return;
            case GLFW.GLFW_CONTEXT_VERSION_MINOR:
                _glfw.hints.Context.Minor = value;
                return;
            case GLFW.GLFW_CONTEXT_ROBUSTNESS:
                _glfw.hints.Context.Robustness = value;
                return;
            case GLFW.GLFW_OPENGL_FORWARD_COMPAT:
                _glfw.hints.Context.Forward = value != 0;
                return;
            case GLFW.GLFW_CONTEXT_DEBUG:
                _glfw.hints.Context.Debug = value != 0;
                return;
            case GLFW.GLFW_CONTEXT_NO_ERROR:
                _glfw.hints.Context.Noerror = value != 0;
                return;
            case GLFW.GLFW_OPENGL_PROFILE:
                _glfw.hints.Context.Profile = value;
                return;
            case GLFW.GLFW_CONTEXT_RELEASE_BEHAVIOR:
                _glfw.hints.Context.Release = value;
                return;
            case GLFW.GLFW_REFRESH_RATE:
                _glfw.hints.RefreshRate = value;
                return;
        }

        _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid window hint 0x{0:X8}", hint);
    }

    public static void glfwWindowHintString(int hint, string value)
    {
        Debug.Assert(value != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        switch (hint)
        {
            case GLFW.GLFW_COCOA_FRAME_NAME:
                _glfw.hints.Window.Ns.FrameName = value.Length > 255
                    ? value.Substring(0, 255)
                    : value;
                return;
            case GLFW.GLFW_X11_CLASS_NAME:
                _glfw.hints.Window.X11.ClassName = value.Length > 255
                    ? value.Substring(0, 255)
                    : value;
                return;
            case GLFW.GLFW_X11_INSTANCE_NAME:
                _glfw.hints.Window.X11.InstanceName = value.Length > 255
                    ? value.Substring(0, 255)
                    : value;
                return;
            case GLFW.GLFW_WAYLAND_APP_ID:
                _glfw.hints.Window.Wl.AppId = value.Length > 255
                    ? value.Substring(0, 255)
                    : value;
                return;
        }

        _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid window hint string 0x{0:X8}", hint);
    }

    public static void glfwDestroyWindow(GlfwWindow? window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        // Allow closing of null (to match the behavior of free)
        if (window == null)
            return;

        // Clear all callbacks to avoid exposing a half torn-down window object
        window.Callbacks = new GlfwWindow.WindowCallbacks();

        // The window's context must not be current on another thread when the
        // window is destroyed
        if (window == _glfw.contextSlot)
            glfwMakeContextCurrent(null);

        _glfw.platform!.DestroyWindow(window);

        // Unlink window from global linked list
        if (_glfw.windowListHead == window)
        {
            _glfw.windowListHead = window.Next;
        }
        else
        {
            var prev = _glfw.windowListHead;
            while (prev != null && prev.Next != window)
                prev = prev.Next;
            if (prev != null)
                prev.Next = window.Next;
        }

        // In C#, GC handles deallocation; just clear the title reference
        window.Title = null;
    }

    public static int glfwWindowShouldClose(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        Debug.Assert(window != null);

        return window.ShouldClose ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
    }

    public static void glfwSetWindowShouldClose(GlfwWindow window, int value)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        window.ShouldClose = value != 0;
    }

    public static string? glfwGetWindowTitle(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        return window.Title;
    }

    public static void glfwSetWindowTitle(GlfwWindow window, string title)
    {
        Debug.Assert(title != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        window.Title = title;

        _glfw.platform!.SetWindowTitle(window, title);
    }

    public static void glfwSetWindowIcon(GlfwWindow window, int count, GlfwImage[]? images)
    {
        Debug.Assert(count >= 0);
        Debug.Assert(count == 0 || images != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (count < 0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE, "Invalid image count for window icon");
            return;
        }

        if (images != null)
        {
            for (int i = 0; i < count; i++)
            {
                Debug.Assert(images[i].Pixels != null);

                if (images[i].Width <= 0 || images[i].Height <= 0)
                {
                    _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                                    "Invalid image dimensions for window icon");
                    return;
                }
            }
        }

        _glfw.platform!.SetWindowIcon(window, count, images ?? Array.Empty<GlfwImage>());
    }

    public static void glfwGetWindowPos(GlfwWindow window, out int xpos, out int ypos)
    {
        xpos = 0;
        ypos = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.GetWindowPos(window, out xpos, out ypos);
    }

    public static void glfwSetWindowPos(GlfwWindow window, int xpos, int ypos)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (window.Monitor != null)
            return;

        _glfw.platform!.SetWindowPos(window, xpos, ypos);
    }

    public static void glfwGetWindowSize(GlfwWindow window, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.GetWindowSize(window, out width, out height);
    }

    public static void glfwSetWindowSize(GlfwWindow window, int width, int height)
    {
        Debug.Assert(width >= 0);
        Debug.Assert(height >= 0);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        window.VideoMode.Width  = width;
        window.VideoMode.Height = height;

        _glfw.platform!.SetWindowSize(window, width, height);
    }

    public static void glfwSetWindowSizeLimits(GlfwWindow window,
                                               int minwidth, int minheight,
                                               int maxwidth, int maxheight)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (minwidth != GLFW.GLFW_DONT_CARE && minheight != GLFW.GLFW_DONT_CARE)
        {
            if (minwidth < 0 || minheight < 0)
            {
                _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                                "Invalid window minimum size {0}x{1}",
                                minwidth, minheight);
                return;
            }
        }

        if (maxwidth != GLFW.GLFW_DONT_CARE && maxheight != GLFW.GLFW_DONT_CARE)
        {
            if (maxwidth < 0 || maxheight < 0 ||
                maxwidth < minwidth || maxheight < minheight)
            {
                _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                                "Invalid window maximum size {0}x{1}",
                                maxwidth, maxheight);
                return;
            }
        }

        window.MinWidth  = minwidth;
        window.MinHeight = minheight;
        window.MaxWidth  = maxwidth;
        window.MaxHeight = maxheight;

        if (window.Monitor != null || !window.Resizable)
            return;

        _glfw.platform!.SetWindowSizeLimits(window,
                                            minwidth, minheight,
                                            maxwidth, maxheight);
    }

    public static void glfwSetWindowAspectRatio(GlfwWindow window, int numer, int denom)
    {
        Debug.Assert(numer != 0);
        Debug.Assert(denom != 0);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (numer != GLFW.GLFW_DONT_CARE && denom != GLFW.GLFW_DONT_CARE)
        {
            if (numer <= 0 || denom <= 0)
            {
                _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                                "Invalid window aspect ratio {0}:{1}",
                                numer, denom);
                return;
            }
        }

        window.Numer = numer;
        window.Denom = denom;

        if (window.Monitor != null || !window.Resizable)
            return;

        _glfw.platform!.SetWindowAspectRatio(window, numer, denom);
    }

    public static void glfwGetFramebufferSize(GlfwWindow window, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.GetFramebufferSize(window, out width, out height);
    }

    public static void glfwGetWindowFrameSize(GlfwWindow window,
                                              out int left, out int top,
                                              out int right, out int bottom)
    {
        left = 0;
        top = 0;
        right = 0;
        bottom = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.GetWindowFrameSize(window, out left, out top, out right, out bottom);
    }

    public static void glfwGetWindowContentScale(GlfwWindow window,
                                                 out float xscale, out float yscale)
    {
        xscale = 0f;
        yscale = 0f;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.GetWindowContentScale(window, out xscale, out yscale);
    }

    public static float glfwGetWindowOpacity(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0f;
        }

        Debug.Assert(window != null);

        return _glfw.platform!.GetWindowOpacity(window);
    }

    public static void glfwSetWindowOpacity(GlfwWindow window, float opacity)
    {
        Debug.Assert(float.IsFinite(opacity));
        Debug.Assert(opacity >= 0f);
        Debug.Assert(opacity <= 1f);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (!float.IsFinite(opacity) || opacity < 0f || opacity > 1f)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE, "Invalid window opacity {0}", opacity);
            return;
        }

        _glfw.platform!.SetWindowOpacity(window, opacity);
    }

    public static void glfwIconifyWindow(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.IconifyWindow(window);
    }

    public static void glfwRestoreWindow(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.RestoreWindow(window);
    }

    public static void glfwMaximizeWindow(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (window.Monitor != null)
            return;

        _glfw.platform!.MaximizeWindow(window);
    }

    public static void glfwShowWindow(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (window.Monitor != null)
            return;

        _glfw.platform!.ShowWindow(window);

        if (window.FocusOnShow)
            _glfw.platform!.FocusWindow(window);
    }

    public static void glfwRequestWindowAttention(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.RequestWindowAttention(window);
    }

    public static void glfwHideWindow(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (window.Monitor != null)
            return;

        _glfw.platform!.HideWindow(window);
    }

    public static void glfwFocusWindow(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        _glfw.platform!.FocusWindow(window);
    }

    public static int glfwGetWindowAttrib(GlfwWindow window, int attrib)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        Debug.Assert(window != null);

        switch (attrib)
        {
            case GLFW.GLFW_FOCUSED:
                return _glfw.platform!.WindowFocused(window) ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_ICONIFIED:
                return _glfw.platform!.WindowIconified(window) ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_VISIBLE:
                return _glfw.platform!.WindowVisible(window) ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_MAXIMIZED:
                return _glfw.platform!.WindowMaximized(window) ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_HOVERED:
                return _glfw.platform!.WindowHovered(window) ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_FOCUS_ON_SHOW:
                return window.FocusOnShow ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_MOUSE_PASSTHROUGH:
                return window.MousePassthrough ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_TRANSPARENT_FRAMEBUFFER:
                return _glfw.platform!.FramebufferTransparent(window) ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_RESIZABLE:
                return window.Resizable ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_DECORATED:
                return window.Decorated ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_FLOATING:
                return window.Floating ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_AUTO_ICONIFY:
                return window.AutoIconify ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_DOUBLEBUFFER:
                return window.doublebuffer ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_CLIENT_API:
                return window.context.Client;
            case GLFW.GLFW_CONTEXT_CREATION_API:
                return window.context.Source;
            case GLFW.GLFW_CONTEXT_VERSION_MAJOR:
                return window.context.Major;
            case GLFW.GLFW_CONTEXT_VERSION_MINOR:
                return window.context.Minor;
            case GLFW.GLFW_CONTEXT_REVISION:
                return window.context.Revision;
            case GLFW.GLFW_CONTEXT_ROBUSTNESS:
                return window.context.Robustness;
            case GLFW.GLFW_OPENGL_FORWARD_COMPAT:
                return window.context.Forward ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_CONTEXT_DEBUG:
                return window.context.Debug ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
            case GLFW.GLFW_OPENGL_PROFILE:
                return window.context.Profile;
            case GLFW.GLFW_CONTEXT_RELEASE_BEHAVIOR:
                return window.context.Release;
            case GLFW.GLFW_CONTEXT_NO_ERROR:
                return window.context.Noerror ? GLFW.GLFW_TRUE : GLFW.GLFW_FALSE;
        }

        _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid window attribute 0x{0:X8}", attrib);
        return 0;
    }

    public static void glfwSetWindowAttrib(GlfwWindow window, int attrib, int value)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        bool boolValue = value != 0;

        switch (attrib)
        {
            case GLFW.GLFW_AUTO_ICONIFY:
                window.AutoIconify = boolValue;
                return;

            case GLFW.GLFW_RESIZABLE:
                window.Resizable = boolValue;
                if (window.Monitor == null)
                    _glfw.platform!.SetWindowResizable(window, boolValue);
                return;

            case GLFW.GLFW_DECORATED:
                window.Decorated = boolValue;
                if (window.Monitor == null)
                    _glfw.platform!.SetWindowDecorated(window, boolValue);
                return;

            case GLFW.GLFW_FLOATING:
                window.Floating = boolValue;
                if (window.Monitor == null)
                    _glfw.platform!.SetWindowFloating(window, boolValue);
                return;

            case GLFW.GLFW_FOCUS_ON_SHOW:
                window.FocusOnShow = boolValue;
                return;

            case GLFW.GLFW_MOUSE_PASSTHROUGH:
                window.MousePassthrough = boolValue;
                _glfw.platform!.SetWindowMousePassthrough(window, boolValue);
                return;
        }

        _glfwInputError(GLFW.GLFW_INVALID_ENUM, "Invalid window attribute 0x{0:X8}", attrib);
    }

    public static GlfwMonitor? glfwGetWindowMonitor(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        return window.Monitor;
    }

    public static void glfwSetWindowMonitor(GlfwWindow window,
                                            GlfwMonitor? monitor,
                                            int xpos, int ypos,
                                            int width, int height,
                                            int refreshRate)
    {
        Debug.Assert(width >= 0);
        Debug.Assert(height >= 0);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        if (width <= 0 || height <= 0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                            "Invalid window size {0}x{1}",
                            width, height);
            return;
        }

        if (refreshRate < 0 && refreshRate != GLFW.GLFW_DONT_CARE)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                            "Invalid refresh rate {0}",
                            refreshRate);
            return;
        }

        window.VideoMode.Width       = width;
        window.VideoMode.Height      = height;
        window.VideoMode.RefreshRate = refreshRate;

        _glfw.platform!.SetWindowMonitor(window, monitor,
                                         xpos, ypos, width, height,
                                         refreshRate);
    }

    public static void glfwSetWindowUserPointer(GlfwWindow window, object? pointer)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(window != null);

        window.UserPointer = pointer;
    }

    public static object? glfwGetWindowUserPointer(GlfwWindow window)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        return window.UserPointer;
    }

    public static GlfwWindowPosFun? glfwSetWindowPosCallback(GlfwWindow window,
                                                             GlfwWindowPosFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Pos;
        window.Callbacks.Pos = cbfun;
        return previous;
    }

    public static GlfwWindowSizeFun? glfwSetWindowSizeCallback(GlfwWindow window,
                                                               GlfwWindowSizeFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Size;
        window.Callbacks.Size = cbfun;
        return previous;
    }

    public static GlfwWindowCloseFun? glfwSetWindowCloseCallback(GlfwWindow window,
                                                                 GlfwWindowCloseFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Close;
        window.Callbacks.Close = cbfun;
        return previous;
    }

    public static GlfwWindowRefreshFun? glfwSetWindowRefreshCallback(GlfwWindow window,
                                                                     GlfwWindowRefreshFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Refresh;
        window.Callbacks.Refresh = cbfun;
        return previous;
    }

    public static GlfwWindowFocusFun? glfwSetWindowFocusCallback(GlfwWindow window,
                                                                 GlfwWindowFocusFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Focus;
        window.Callbacks.Focus = cbfun;
        return previous;
    }

    public static GlfwWindowIconifyFun? glfwSetWindowIconifyCallback(GlfwWindow window,
                                                                     GlfwWindowIconifyFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Iconify;
        window.Callbacks.Iconify = cbfun;
        return previous;
    }

    public static GlfwWindowMaximizeFun? glfwSetWindowMaximizeCallback(GlfwWindow window,
                                                                       GlfwWindowMaximizeFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Maximize;
        window.Callbacks.Maximize = cbfun;
        return previous;
    }

    public static GlfwFramebufferSizeFun? glfwSetFramebufferSizeCallback(GlfwWindow window,
                                                                         GlfwFramebufferSizeFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Fbsize;
        window.Callbacks.Fbsize = cbfun;
        return previous;
    }

    public static GlfwWindowContentScaleFun? glfwSetWindowContentScaleCallback(GlfwWindow window,
                                                                               GlfwWindowContentScaleFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(window != null);

        var previous = window.Callbacks.Scale;
        window.Callbacks.Scale = cbfun;
        return previous;
    }

    public static void glfwPollEvents()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        _glfw.platform!.PollEvents();
    }

    public static void glfwWaitEvents()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        _glfw.platform!.WaitEvents();
    }

    public static void glfwWaitEventsTimeout(double timeout)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(double.IsFinite(timeout));
        Debug.Assert(timeout >= 0.0);

        if (!double.IsFinite(timeout) || timeout < 0.0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE, "Invalid time {0}", timeout);
            return;
        }

        _glfw.platform!.WaitEventsTimeout(timeout);
    }

    public static void glfwPostEmptyEvent()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        _glfw.platform!.PostEmptyEvent();
    }
}

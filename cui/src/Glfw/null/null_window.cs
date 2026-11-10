// Ported from glfw/src/null_window.c (GLFW 3.6)
//
// Copyright (c) 2016 Google Inc.
// Copyright (c) 2016-2019 Camilla Loewy <elmindreda@glfw.org>
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
using static Glfw.GLFW;
using SC = Glfw.GlfwNullSC;

namespace Glfw
{
    // Partial class -- window, input, and clipboard operations for the null platform.
    public partial class NullPlatform
    {
        //----------------------------------------------------------------------
        // Private helpers (from null_window.c static functions)
        //----------------------------------------------------------------------

        /// <summary>
        /// Applies size limits and aspect ratio constraints to width/height.
        /// Corresponds to C++ <c>applySizeLimits</c>.
        /// </summary>
        private static void ApplySizeLimits(GlfwWindow window, ref int width, ref int height)
        {
            if (window.Numer != GLFW_DONT_CARE && window.Denom != GLFW_DONT_CARE)
            {
                float ratio = (float)window.Numer / (float)window.Denom;
                height = (int)(width / ratio);
            }

            if (window.MinWidth != GLFW_DONT_CARE)
                width = Math.Max(width, window.MinWidth);
            else if (window.MaxWidth != GLFW_DONT_CARE)
                width = Math.Min(width, window.MaxWidth);

            if (window.MinHeight != GLFW_DONT_CARE)
                height = Math.Min(height, window.MinHeight);
            else if (window.MaxHeight != GLFW_DONT_CARE)
                height = Math.Max(height, window.MaxHeight);
        }

        /// <summary>
        /// Fits window to its current monitor's video mode.
        /// Corresponds to C++ <c>fitToMonitor</c>.
        /// </summary>
        private void FitToMonitor(GlfwWindow window)
        {
            GetVideoMode(window.Monitor!, out var mode);
            GetMonitorPos(window.Monitor!, out int mx, out int my);
            window.Null!.Xpos = mx;
            window.Null.Ypos = my;
            window.Null.Width = mode.Width;
            window.Null.Height = mode.Height;
        }

        /// <summary>
        /// Acquires the monitor for the given window.
        /// Corresponds to C++ <c>acquireMonitor</c>.
        /// </summary>
        private static void AcquireMonitor(GlfwWindow window)
        {
            Glfw._glfwInputMonitorWindow(window.Monitor!, window);
        }

        /// <summary>
        /// Releases the monitor from the given window.
        /// Corresponds to C++ <c>releaseMonitor</c>.
        /// </summary>
        private static void ReleaseMonitor(GlfwWindow window)
        {
            if (window.Monitor!.Window != window)
                return;
            Glfw._glfwInputMonitorWindow(window.Monitor, null);
        }

        /// <summary>
        /// Creates the native (null) window state.
        /// Corresponds to C++ <c>createNativeWindow</c>.
        /// </summary>
        private bool CreateNativeWindow(GlfwWindow window, GlfwWndConfig wndconfig, GlfwFbConfig fbconfig)
        {
            window.Null = new GlfwWindowNull();

            if (window.Monitor != null)
            {
                FitToMonitor(window);
            }
            else
            {
                if (wndconfig.Xpos == GLFW_ANY_POSITION && wndconfig.Ypos == GLFW_ANY_POSITION)
                {
                    window.Null.Xpos = 17;
                    window.Null.Ypos = 17;
                }
                else
                {
                    window.Null.Xpos = wndconfig.Xpos;
                    window.Null.Ypos = wndconfig.Ypos;
                }

                window.Null.Width = wndconfig.Width;
                window.Null.Height = wndconfig.Height;
            }

            window.Null.Visible = wndconfig.Visible;
            window.Null.Decorated = wndconfig.Decorated;
            window.Null.Maximized = wndconfig.Maximized;
            window.Null.Floating = wndconfig.Floating;
            window.Null.Transparent = fbconfig.Transparent;
            window.Null.Opacity = 1.0f;

            return true;
        }

        //----------------------------------------------------------------------
        // IGlfwPlatform -- Window operations
        //----------------------------------------------------------------------

        /// <summary>
        /// Creates a null window. Corresponds to C++ <c>_glfwCreateWindowNull</c>.
        /// Creates the requested OSMesa or surfaceless EGL context as upstream does.
        /// </summary>
        public bool CreateWindow(GlfwWindow window, GlfwWndConfig wndconfig, GlfwCtxConfig ctxconfig, GlfwFbConfig fbconfig)
        {
            if (!CreateNativeWindow(window, wndconfig, fbconfig))
                return false;

            if (ctxconfig.Client != GLFW_NO_API)
            {
                if (ctxconfig.Source == GLFW_NATIVE_CONTEXT_API || ctxconfig.Source == GLFW_OSMESA_CONTEXT_API)
                {
                    if (!Glfw._glfwInitOSMesa() || !Glfw._glfwCreateContextOSMesa(window, ctxconfig, fbconfig)) return false;
                }
                else if (ctxconfig.Source == GLFW_EGL_CONTEXT_API)
                {
                    if (!Glfw._glfwInitEGL() || !Glfw._glfwCreateContextEGL(window, ctxconfig, fbconfig)) return false;
                }
                if (!Glfw._glfwRefreshContextAttribs(window, ctxconfig)) return false;
            }

            if (wndconfig.MousePassthrough)
                SetWindowMousePassthrough(window, true);

            if (window.Monitor != null)
            {
                ShowWindow(window);
                FocusWindow(window);
                AcquireMonitor(window);

                if (wndconfig.CenterCursor)
                    Glfw._glfwCenterCursorInContentArea(window);
            }
            else
            {
                if (wndconfig.Visible)
                {
                    ShowWindow(window);
                    if (wndconfig.Focused)
                        FocusWindow(window);
                }
            }

            return true;
        }

        /// <summary>
        /// Destroys a null window. Corresponds to C++ <c>_glfwDestroyWindowNull</c>.
        /// </summary>
        public void DestroyWindow(GlfwWindow window)
        {
            if (window.Monitor != null)
                ReleaseMonitor(window);

            if (_glfw.Null != null && _glfw.Null.FocusedWindow == window)
                _glfw.Null.FocusedWindow = null;

            window.context.destroy?.Invoke(window);
        }

        public void SetWindowTitle(GlfwWindow window, string title)
        {
            // No-op for the null platform
        }

        public void SetWindowIcon(GlfwWindow window, int count, GlfwImage[]? images)
        {
            // No-op for the null platform
        }

        public void SetWindowMonitor(GlfwWindow window, GlfwMonitor? monitor,
                                     int xpos, int ypos, int width, int height,
                                     int refreshRate)
        {
            if (window.Monitor == monitor)
            {
                if (monitor == null)
                {
                    SetWindowPos(window, xpos, ypos);
                    SetWindowSize(window, width, height);
                }
                return;
            }

            if (window.Monitor != null)
                ReleaseMonitor(window);

            Glfw._glfwInputWindowMonitor(window, monitor);

            if (window.Monitor != null)
            {
                window.Null!.Visible = true;
                AcquireMonitor(window);
                FitToMonitor(window);
            }
            else
            {
                SetWindowPos(window, xpos, ypos);
                SetWindowSize(window, width, height);
            }
        }

        public void GetWindowPos(GlfwWindow window, out int xpos, out int ypos)
        {
            xpos = window.Null!.Xpos;
            ypos = window.Null.Ypos;
        }

        public void SetWindowPos(GlfwWindow window, int xpos, int ypos)
        {
            if (window.Monitor != null)
                return;

            if (window.Null!.Xpos != xpos || window.Null.Ypos != ypos)
            {
                window.Null.Xpos = xpos;
                window.Null.Ypos = ypos;
                Glfw._glfwInputWindowPos(window, xpos, ypos);
            }
        }

        public void GetWindowSize(GlfwWindow window, out int width, out int height)
        {
            width = window.Null!.Width;
            height = window.Null.Height;
        }

        public void SetWindowSize(GlfwWindow window, int width, int height)
        {
            if (window.Monitor != null)
                return;

            if (window.Null!.Width != width || window.Null.Height != height)
            {
                window.Null.Width = width;
                window.Null.Height = height;
                Glfw._glfwInputFramebufferSize(window, width, height);
                Glfw._glfwInputWindowDamage(window);
                Glfw._glfwInputWindowSize(window, width, height);
            }
        }

        public void SetWindowSizeLimits(GlfwWindow window, int minwidth, int minheight,
                                        int maxwidth, int maxheight)
        {
            int width = window.Null!.Width;
            int height = window.Null.Height;
            ApplySizeLimits(window, ref width, ref height);
            SetWindowSize(window, width, height);
        }

        public void SetWindowAspectRatio(GlfwWindow window, int numer, int denom)
        {
            int width = window.Null!.Width;
            int height = window.Null.Height;
            ApplySizeLimits(window, ref width, ref height);
            SetWindowSize(window, width, height);
        }

        public void GetFramebufferSize(GlfwWindow window, out int width, out int height)
        {
            width = window.Null!.Width;
            height = window.Null.Height;
        }

        public void GetWindowFrameSize(GlfwWindow window, out int left, out int top,
                                       out int right, out int bottom)
        {
            if (window.Null!.Decorated && window.Monitor == null)
            {
                left = 1;
                top = 10;
                right = 1;
                bottom = 1;
            }
            else
            {
                left = 0;
                top = 0;
                right = 0;
                bottom = 0;
            }
        }

        public void GetWindowContentScale(GlfwWindow window, out float xscale, out float yscale)
        {
            xscale = 1.0f;
            yscale = 1.0f;
        }

        public void IconifyWindow(GlfwWindow window)
        {
            if (_glfw.Null != null && _glfw.Null.FocusedWindow == window)
            {
                _glfw.Null.FocusedWindow = null;
                Glfw._glfwInputWindowFocus(window, GLFW_FALSE);
            }

            if (!window.Null!.Iconified)
            {
                window.Null.Iconified = true;
                Glfw._glfwInputWindowIconify(window, GLFW_TRUE);

                if (window.Monitor != null)
                    ReleaseMonitor(window);
            }
        }

        public void RestoreWindow(GlfwWindow window)
        {
            if (window.Null!.Iconified)
            {
                window.Null.Iconified = false;
                Glfw._glfwInputWindowIconify(window, GLFW_FALSE);

                if (window.Monitor != null)
                    AcquireMonitor(window);
            }
            else if (window.Null.Maximized)
            {
                window.Null.Maximized = false;
                Glfw._glfwInputWindowMaximize(window, GLFW_FALSE);
            }
        }

        public void MaximizeWindow(GlfwWindow window)
        {
            if (!window.Null!.Maximized)
            {
                window.Null.Maximized = true;
                Glfw._glfwInputWindowMaximize(window, GLFW_TRUE);
            }
        }

        public bool WindowMaximized(GlfwWindow window)
        {
            return window.Null!.Maximized;
        }

        public bool WindowHovered(GlfwWindow window)
        {
            return _glfw.Null != null &&
                   _glfw.Null.Xcursor >= window.Null!.Xpos &&
                   _glfw.Null.Ycursor >= window.Null.Ypos &&
                   _glfw.Null.Xcursor <= window.Null.Xpos + window.Null.Width - 1 &&
                   _glfw.Null.Ycursor <= window.Null.Ypos + window.Null.Height - 1;
        }

        public bool FramebufferTransparent(GlfwWindow window)
        {
            return window.Null!.Transparent;
        }

        public void SetWindowResizable(GlfwWindow window, bool enabled)
        {
            window.Null!.Resizable = enabled;
        }

        public void SetWindowDecorated(GlfwWindow window, bool enabled)
        {
            window.Null!.Decorated = enabled;
        }

        public void SetWindowFloating(GlfwWindow window, bool enabled)
        {
            window.Null!.Floating = enabled;
        }

        public void SetWindowMousePassthrough(GlfwWindow window, bool enabled)
        {
            // No-op for the null platform
        }

        public float GetWindowOpacity(GlfwWindow window)
        {
            return window.Null!.Opacity;
        }

        public void SetWindowOpacity(GlfwWindow window, float opacity)
        {
            window.Null!.Opacity = opacity;
        }

        public void SetRawMouseMotion(GlfwWindow window, bool enabled)
        {
            // No-op for the null platform
        }

        public bool RawMouseMotionSupported()
        {
            return true;
        }

        public void ShowWindow(GlfwWindow window)
        {
            window.Null!.Visible = true;
        }

        public void RequestWindowAttention(GlfwWindow window)
        {
            // No-op for the null platform
        }

        public void HideWindow(GlfwWindow window)
        {
            if (_glfw.Null != null && _glfw.Null.FocusedWindow == window)
            {
                _glfw.Null.FocusedWindow = null;
                Glfw._glfwInputWindowFocus(window, GLFW_FALSE);
            }

            window.Null!.Visible = false;
        }

        public void FocusWindow(GlfwWindow window)
        {
            if (_glfw.Null == null)
                return;

            if (_glfw.Null.FocusedWindow == window)
                return;

            if (!window.Null!.Visible)
                return;

            GlfwWindow? previous = _glfw.Null.FocusedWindow;
            _glfw.Null.FocusedWindow = window;

            if (previous != null)
            {
                Glfw._glfwInputWindowFocus(previous, GLFW_FALSE);
                if (previous.Monitor != null && previous.AutoIconify)
                    IconifyWindow(previous);
            }

            Glfw._glfwInputWindowFocus(window, GLFW_TRUE);
        }

        public bool WindowFocused(GlfwWindow window)
        {
            return _glfw.Null != null && _glfw.Null.FocusedWindow == window;
        }

        public bool WindowIconified(GlfwWindow window)
        {
            return window.Null!.Iconified;
        }

        public bool WindowVisible(GlfwWindow window)
        {
            return window.Null!.Visible;
        }

        //----------------------------------------------------------------------
        // IGlfwPlatform -- Event polling (all no-ops)
        //----------------------------------------------------------------------

        public void PollEvents()
        {
            // No-op for the null platform
        }

        public void WaitEvents()
        {
            // No-op for the null platform
        }

        public void WaitEventsTimeout(double timeout)
        {
            // No-op for the null platform
        }

        public void PostEmptyEvent()
        {
            // No-op for the null platform
        }

        //----------------------------------------------------------------------
        // IGlfwPlatform -- Cursor operations
        //----------------------------------------------------------------------

        public void GetCursorPos(GlfwWindow window, out double xpos, out double ypos)
        {
            xpos = (_glfw.Null != null) ? _glfw.Null.Xcursor - window.Null!.Xpos : 0;
            ypos = (_glfw.Null != null) ? _glfw.Null.Ycursor - window.Null!.Ypos : 0;
        }

        public void SetCursorPos(GlfwWindow window, double x, double y)
        {
            if (_glfw.Null != null)
            {
                _glfw.Null.Xcursor = window.Null!.Xpos + (int)x;
                _glfw.Null.Ycursor = window.Null.Ypos + (int)y;
            }
        }

        public void SetCursorMode(GlfwWindow window, int mode)
        {
            // No-op for the null platform
        }

        public bool CreateCursor(GlfwCursor cursor, in GlfwImage image, int xhot, int yhot)
        {
            return true;
        }

        public bool CreateStandardCursor(GlfwCursor cursor, int shape)
        {
            return true;
        }

        public void DestroyCursor(GlfwCursor cursor)
        {
            // No-op for the null platform
        }

        public void SetCursor(GlfwWindow window, GlfwCursor? cursor)
        {
            // No-op for the null platform
        }

        //----------------------------------------------------------------------
        // IGlfwPlatform -- Clipboard
        //----------------------------------------------------------------------

        public void SetClipboardString(string text)
        {
            if (_glfw.Null != null)
                _glfw.Null.ClipboardString = text;
        }

        public string? GetClipboardString()
        {
            return _glfw.Null?.ClipboardString;
        }

        //----------------------------------------------------------------------
        // IGlfwPlatform -- Scancode / key mapping
        //----------------------------------------------------------------------

        public string? GetScancodeName(int scancode)
        {
            if (scancode < SC.FIRST || scancode > SC.LAST)
            {
                Glfw._glfwInputError(GLFW_INVALID_VALUE,
                    "Invalid scancode {0}", scancode);
                return null;
            }

            switch (scancode)
            {
                case SC.APOSTROPHE:
                    return "'";
                case SC.COMMA:
                    return ",";
                case SC.MINUS:
                case SC.KP_SUBTRACT:
                    return "-";
                case SC.PERIOD:
                case SC.KP_DECIMAL:
                    return ".";
                case SC.SLASH:
                case SC.KP_DIVIDE:
                    return "/";
                case SC.SEMICOLON:
                    return ";";
                case SC.EQUAL:
                case SC.KP_EQUAL:
                    return "=";
                case SC.LEFT_BRACKET:
                    return "[";
                case SC.RIGHT_BRACKET:
                    return "]";
                case SC.KP_MULTIPLY:
                    return "*";
                case SC.KP_ADD:
                    return "+";
                case SC.BACKSLASH:
                case SC.WORLD_1:
                case SC.WORLD_2:
                    return "\\";
                case SC._0:
                case SC.KP_0:
                    return "0";
                case SC._1:
                case SC.KP_1:
                    return "1";
                case SC._2:
                case SC.KP_2:
                    return "2";
                case SC._3:
                case SC.KP_3:
                    return "3";
                case SC._4:
                case SC.KP_4:
                    return "4";
                case SC._5:
                case SC.KP_5:
                    return "5";
                case SC._6:
                case SC.KP_6:
                    return "6";
                case SC._7:
                case SC.KP_7:
                    return "7";
                case SC._8:
                case SC.KP_8:
                    return "8";
                case SC._9:
                case SC.KP_9:
                    return "9";
                case SC.A:
                    return "a";
                case SC.B:
                    return "b";
                case SC.C:
                    return "c";
                case SC.D:
                    return "d";
                case SC.E:
                    return "e";
                case SC.F:
                    return "f";
                case SC.G:
                    return "g";
                case SC.H:
                    return "h";
                case SC.I:
                    return "i";
                case SC.J:
                    return "j";
                case SC.K:
                    return "k";
                case SC.L:
                    return "l";
                case SC.M:
                    return "m";
                case SC.N:
                    return "n";
                case SC.O:
                    return "o";
                case SC.P:
                    return "p";
                case SC.Q:
                    return "q";
                case SC.R:
                    return "r";
                case SC.S:
                    return "s";
                case SC.T:
                    return "t";
                case SC.U:
                    return "u";
                case SC.V:
                    return "v";
                case SC.W:
                    return "w";
                case SC.X:
                    return "x";
                case SC.Y:
                    return "y";
                case SC.Z:
                    return "z";
            }

            return null;
        }

        public int GetKeyScancode(int key)
        {
            if (_glfw.Null == null)
                return -1;
            return _glfw.Null.Scancodes[key];
        }
    }
}

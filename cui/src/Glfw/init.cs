// GLFW 3.6 - www.glfw.org
// Ported from glfw/src/init.c
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2018 Camilla Loewy <elmindreda@glfw.org>
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
using System.Text;

namespace Glfw
{
    public static partial class Glfw
    {
        //----------------------------------------------------------------------
        // Global state outside of _glfw so they can be used before
        // initialization and after termination
        //----------------------------------------------------------------------

        [ThreadStatic]
        private static GlfwError? _glfwThreadError;
        [ThreadStatic]
        private static int _glfwThreadErrorGeneration;

        private static GlfwError _glfwMainThreadError = new();
        private static GLFWerrorfun? _glfwErrorCallback;
        private static GlfwInitConfig _glfwInitHints = new()
        {
            HatButtons = true,
            AngleType = GLFW.GLFW_ANGLE_PLATFORM_TYPE_NONE,
            PlatformID = GLFW.GLFW_ANY_PLATFORM,
            Ns = new GlfwInitConfig.NsConfig { Chdir = true, Menubar = true },
            X11 = new GlfwInitConfig.X11Config { XcbVulkanSurface = true },
            Wl = new GlfwInitConfig.WlConfig { LibdecorMode = GLFW.GLFW_WAYLAND_PREFER_LIBDECOR },
        };

        //----------------------------------------------------------------------
        // Terminate the library (private helper)
        //----------------------------------------------------------------------

        private static void Terminate()
        {
            // Clear library-level callbacks
            _glfw.monitorCallback = null;
            _glfw.joystickCallback = null;

            while (_glfw.windowListHead != null)
                glfwDestroyWindow(_glfw.windowListHead);

            while (_glfw.cursorListHead != null)
                glfwDestroyCursor(_glfw.cursorListHead);

            if (_glfw.monitors != null)
            {
                for (int i = 0; i < _glfw.monitorCount; i++)
                {
                    var monitor = _glfw.monitors[i];
                    if (monitor.OriginalRamp.Size > 0)
                        _glfw.platform!.SetGammaRamp(monitor, monitor.OriginalRamp);
                    _glfwFreeMonitor(monitor);
                }

                _glfw.monitors = null;
                _glfw.monitorCount = 0;
            }

            if (_glfw.joysticksInitialized)
                _glfw.platform?.TerminateJoysticks();
            _glfw.joysticksInitialized = false;
            _glfw.mappings.Clear();
            _glfw.platform?.Terminate();

            _glfw.initialized = false;
            _glfw.tlsGeneration++;
            _glfwThreadError = null;

            // In C#, GC handles error list cleanup; just clear the reference
            _glfw.errorListHead = null;

            // Reset all static fields to defaults
            _glfw.platform = null;
            _glfw.windowListHead = null;
            _glfw.cursorListHead = null;
            _glfw.Null = null;
            _glfw.Win32 = null;
            _glfw.X11 = null;
        }

        //----------------------------------------------------------------------
        //                       GLFW internal API
        //----------------------------------------------------------------------

        /// <summary>
        /// Encode a Unicode code point to a UTF-8 stream.
        /// Based on cutef8 by Jeff Bezanson (Public Domain).
        /// Returns the number of bytes written to the buffer.
        /// </summary>
        internal static int _glfwEncodeUTF8(byte[] s, int offset, uint codepoint)
        {
            int count = 0;

            if (codepoint < 0x80)
            {
                s[offset + count++] = (byte)codepoint;
            }
            else if (codepoint < 0x800)
            {
                s[offset + count++] = (byte)((codepoint >> 6) | 0xc0);
                s[offset + count++] = (byte)((codepoint & 0x3f) | 0x80);
            }
            else if (codepoint < 0x10000)
            {
                s[offset + count++] = (byte)((codepoint >> 12) | 0xe0);
                s[offset + count++] = (byte)(((codepoint >> 6) & 0x3f) | 0x80);
                s[offset + count++] = (byte)((codepoint & 0x3f) | 0x80);
            }
            else if (codepoint < 0x110000)
            {
                s[offset + count++] = (byte)((codepoint >> 18) | 0xf0);
                s[offset + count++] = (byte)(((codepoint >> 12) & 0x3f) | 0x80);
                s[offset + count++] = (byte)(((codepoint >> 6) & 0x3f) | 0x80);
                s[offset + count++] = (byte)((codepoint & 0x3f) | 0x80);
            }

            return count;
        }

        /// <summary>
        /// Convenience overload: encode a Unicode code point and return the UTF-8 string.
        /// </summary>
        internal static string _glfwEncodeUTF8(uint codepoint)
        {
            byte[] buf = new byte[4];
            int len = _glfwEncodeUTF8(buf, 0, codepoint);
            return Encoding.UTF8.GetString(buf, 0, len);
        }

        // _glfwFreeMonitor is defined in monitor.cs

        //----------------------------------------------------------------------
        //                         GLFW event API
        //----------------------------------------------------------------------

        /// <summary>
        /// Notifies shared code of an error.
        /// </summary>
        internal static void _glfwInputError(int code, string? format, params object[] args)
        {
            string description;

            if (format != null)
            {
                try
                {
                    description = args.Length > 0
                        ? string.Format(format, args)
                        : format;
                }
                catch (FormatException)
                {
                    description = format;
                }
            }
            else
            {
                description = code switch
                {
                    GLFW.GLFW_NOT_INITIALIZED      => "The GLFW library is not initialized",
                    GLFW.GLFW_NO_CURRENT_CONTEXT    => "There is no current context",
                    GLFW.GLFW_INVALID_ENUM          => "Invalid argument for enum parameter",
                    GLFW.GLFW_INVALID_VALUE         => "Invalid value for parameter",
                    GLFW.GLFW_OUT_OF_MEMORY         => "Out of memory",
                    GLFW.GLFW_API_UNAVAILABLE       => "The requested API is unavailable",
                    GLFW.GLFW_VERSION_UNAVAILABLE   => "The requested API version is unavailable",
                    GLFW.GLFW_PLATFORM_ERROR        => "A platform-specific error occurred",
                    GLFW.GLFW_FORMAT_UNAVAILABLE    => "The requested format is unavailable",
                    GLFW.GLFW_NO_WINDOW_CONTEXT     => "The specified window has no context",
                    GLFW.GLFW_CURSOR_UNAVAILABLE    => "The specified cursor shape is unavailable",
                    GLFW.GLFW_FEATURE_UNAVAILABLE   => "The requested feature cannot be implemented for this platform",
                    GLFW.GLFW_FEATURE_UNIMPLEMENTED => "The requested feature has not yet been implemented for this platform",
                    GLFW.GLFW_PLATFORM_UNAVAILABLE  => "The requested platform is unavailable",
                    _ => "ERROR: UNKNOWN GLFW ERROR",
                };
            }

            GlfwError error;

            if (_glfw.initialized)
            {
                if (_glfwThreadErrorGeneration != _glfw.tlsGeneration)
                {
                    _glfwThreadError = null;
                    _glfwThreadErrorGeneration = _glfw.tlsGeneration;
                }
                // Use [ThreadStatic] for per-thread error storage
                error = _glfwThreadError ?? new GlfwError();
                if (_glfwThreadError == null)
                {
                    _glfwThreadError = error;
                    lock (_glfw.errorLock)
                    {
                        error.Next = _glfw.errorListHead;
                        _glfw.errorListHead = error;
                    }
                }
            }
            else
            {
                error = _glfwMainThreadError;
            }

            error.Code = code;
            error.Description = description;

            _glfwErrorCallback?.Invoke(code, description);
        }

        // Convenience overload without format args
        internal static void _glfwInputError(int code, string? format)
        {
            _glfwInputError(code, format, Array.Empty<object>());
        }

        //----------------------------------------------------------------------
        //                        GLFW public API
        //----------------------------------------------------------------------

        /// <summary>
        /// Initializes the GLFW library.
        /// </summary>
        public static int glfwInit()
        {
            if (_glfw.initialized)
                return GLFW.GLFW_TRUE;

            // Reset global state
            _glfw.initialized = false;
            _glfw.platform = null;
            _glfw.errorListHead = null;
            _glfw.cursorListHead = null;
            _glfw.windowListHead = null;
            _glfw.monitors = null;
            _glfw.monitorCount = 0;
            _glfw.monitorCallback = null;
            _glfw.joystickCallback = null;
            _glfw.Null = null;
            _glfw.Win32 = null;
            _glfw.X11 = null;

            _glfw.hints.Init = new GlfwInitConfig
            {
                HatButtons = _glfwInitHints.HatButtons,
                AngleType = _glfwInitHints.AngleType,
                PlatformID = _glfwInitHints.PlatformID,
                Ns = new GlfwInitConfig.NsConfig { Chdir = _glfwInitHints.Ns.Chdir, Menubar = _glfwInitHints.Ns.Menubar },
                X11 = new GlfwInitConfig.X11Config { XcbVulkanSurface = _glfwInitHints.X11.XcbVulkanSurface },
                Wl = new GlfwInitConfig.WlConfig { LibdecorMode = _glfwInitHints.Wl.LibdecorMode },
            };

            var platform = GlfwPlatformSelector.SelectPlatform(_glfw.hints.Init.PlatformID);
            if (platform == null)
            {
                return GLFW.GLFW_FALSE;
            }
            _glfw.platform = platform;

            if (!_glfw.platform.Init())
            {
                Terminate();
                return GLFW.GLFW_FALSE;
            }

            // TLS and mutex not needed in C# ([ThreadStatic] + lock handle these)

            // Set up the main thread error slot
            _glfw.tlsGeneration++;
            _glfwThreadErrorGeneration = _glfw.tlsGeneration;
            _glfwThreadError = _glfwMainThreadError;

            _glfwInitGamepadMappings();

            // Initialize timer using Stopwatch
            _glfw.timer.Offset = (ulong)Stopwatch.GetTimestamp();

            _glfw.initialized = true;

            glfwDefaultWindowHints();
            return GLFW.GLFW_TRUE;
        }

        /// <summary>
        /// Terminates the GLFW library.
        /// </summary>
        public static void glfwTerminate()
        {
            if (!_glfw.initialized)
                return;

            Terminate();
        }

        /// <summary>
        /// Sets the specified init hint to the desired value.
        /// </summary>
        public static void glfwInitHint(int hint, int value)
        {
            switch (hint)
            {
                case GLFW.GLFW_JOYSTICK_HAT_BUTTONS:
                    _glfwInitHints.HatButtons = value != 0;
                    return;
                case GLFW.GLFW_ANGLE_PLATFORM_TYPE:
                    _glfwInitHints.AngleType = value;
                    return;
                case GLFW.GLFW_PLATFORM:
                    _glfwInitHints.PlatformID = value;
                    return;
                case GLFW.GLFW_COCOA_CHDIR_RESOURCES:
                    _glfwInitHints.Ns.Chdir = value != 0;
                    return;
                case GLFW.GLFW_COCOA_MENUBAR:
                    _glfwInitHints.Ns.Menubar = value != 0;
                    return;
                case GLFW.GLFW_X11_XCB_VULKAN_SURFACE:
                    _glfwInitHints.X11.XcbVulkanSurface = value != 0;
                    return;
                case GLFW.GLFW_WAYLAND_LIBDECOR:
                    _glfwInitHints.Wl.LibdecorMode = value;
                    return;
            }

            _glfwInputError(GLFW.GLFW_INVALID_ENUM,
                             "Invalid init hint 0x{0:X8}", hint);
        }

        /// <summary>
        /// Returns the GLFW version.
        /// </summary>
        public static void glfwGetVersion(out int major, out int minor, out int rev)
        {
            major = GLFW.GLFW_VERSION_MAJOR;
            minor = GLFW.GLFW_VERSION_MINOR;
            rev = GLFW.GLFW_VERSION_REVISION;
        }

        /// <summary>
        /// Returns and clears the last error for the calling thread.
        /// </summary>
        public static int glfwGetError(out string? description)
        {
            GlfwError? error;
            int code = GLFW.GLFW_NO_ERROR;
            description = null;

            if (_glfw.initialized)
                error = _glfwThreadErrorGeneration == _glfw.tlsGeneration ? _glfwThreadError : null;
            else
                error = _glfwMainThreadError;

            if (error != null)
            {
                code = error.Code;
                error.Code = GLFW.GLFW_NO_ERROR;
                if (code != GLFW.GLFW_NO_ERROR)
                    description = error.Description;
            }

            return code;
        }

        /// <summary>
        /// Sets the error callback.
        /// Returns the previously set callback, or null if no callback was set.
        /// </summary>
        public static GLFWerrorfun? glfwSetErrorCallback(GLFWerrorfun? cbfun)
        {
            var previous = _glfwErrorCallback;
            _glfwErrorCallback = cbfun;
            return previous;
        }
    }
}

// Ported from glfw/src/null_init.c (GLFW 3.6)
//
// Copyright (c) 2016 Google Inc.
// Copyright (c) 2016-2017 Camilla Loewy <elmindreda@glfw.org>
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
    /// <summary>
    /// Creates and returns a <see cref="NullPlatform"/> instance.
    /// Corresponds to C++ <c>_glfwConnectNull</c>.
    /// </summary>
    public static class NullPlatformConnector
    {
        public static IGlfwPlatform? Connect(int platformId)
        {
            return new NullPlatform();
        }
    }

    /// <summary>
    /// The null (headless/no-op) platform backend.
    /// Implements <see cref="IGlfwPlatform"/>.
    /// Ported from glfw/src/null_init.c, null_window.c, null_monitor.c, null_joystick.c.
    /// </summary>
    public partial class NullPlatform : IGlfwPlatform
    {
        public int PlatformID => GLFW_PLATFORM_NULL;

        //----------------------------------------------------------------------
        // Init / Terminate  (from null_init.c)
        //----------------------------------------------------------------------

        /// <summary>
        /// Initializes the null platform. Sets up keycode/scancode tables and
        /// polls for monitors. Corresponds to C++ <c>_glfwInitNull</c>.
        /// </summary>
        public bool Init()
        {
            // Ensure the library-level null state exists
            _glfw.Null ??= new GlfwLibraryNull();

            var null_ = _glfw.Null;

            // Fill keycodes with -1 (UNKNOWN) cast to ushort
            for (int i = 0; i < null_.Keycodes.Length; i++)
                null_.Keycodes[i] = unchecked((ushort)-1);

            // Fill scancodes with 0 (treated as "no scancode")
            Array.Fill<byte>(null_.Scancodes, 0);

            // Map null scancodes -> GLFW key tokens
            null_.Keycodes[SC.SPACE]         = (ushort)GLFW_KEY_SPACE;
            null_.Keycodes[SC.APOSTROPHE]    = (ushort)GLFW_KEY_APOSTROPHE;
            null_.Keycodes[SC.COMMA]         = (ushort)GLFW_KEY_COMMA;
            null_.Keycodes[SC.MINUS]         = (ushort)GLFW_KEY_MINUS;
            null_.Keycodes[SC.PERIOD]        = (ushort)GLFW_KEY_PERIOD;
            null_.Keycodes[SC.SLASH]         = (ushort)GLFW_KEY_SLASH;
            null_.Keycodes[SC._0]            = (ushort)GLFW_KEY_0;
            null_.Keycodes[SC._1]            = (ushort)GLFW_KEY_1;
            null_.Keycodes[SC._2]            = (ushort)GLFW_KEY_2;
            null_.Keycodes[SC._3]            = (ushort)GLFW_KEY_3;
            null_.Keycodes[SC._4]            = (ushort)GLFW_KEY_4;
            null_.Keycodes[SC._5]            = (ushort)GLFW_KEY_5;
            null_.Keycodes[SC._6]            = (ushort)GLFW_KEY_6;
            null_.Keycodes[SC._7]            = (ushort)GLFW_KEY_7;
            null_.Keycodes[SC._8]            = (ushort)GLFW_KEY_8;
            null_.Keycodes[SC._9]            = (ushort)GLFW_KEY_9;
            null_.Keycodes[SC.SEMICOLON]     = (ushort)GLFW_KEY_SEMICOLON;
            null_.Keycodes[SC.EQUAL]         = (ushort)GLFW_KEY_EQUAL;
            null_.Keycodes[SC.A]             = (ushort)GLFW_KEY_A;
            null_.Keycodes[SC.B]             = (ushort)GLFW_KEY_B;
            null_.Keycodes[SC.C]             = (ushort)GLFW_KEY_C;
            null_.Keycodes[SC.D]             = (ushort)GLFW_KEY_D;
            null_.Keycodes[SC.E]             = (ushort)GLFW_KEY_E;
            null_.Keycodes[SC.F]             = (ushort)GLFW_KEY_F;
            null_.Keycodes[SC.G]             = (ushort)GLFW_KEY_G;
            null_.Keycodes[SC.H]             = (ushort)GLFW_KEY_H;
            null_.Keycodes[SC.I]             = (ushort)GLFW_KEY_I;
            null_.Keycodes[SC.J]             = (ushort)GLFW_KEY_J;
            null_.Keycodes[SC.K]             = (ushort)GLFW_KEY_K;
            null_.Keycodes[SC.L]             = (ushort)GLFW_KEY_L;
            null_.Keycodes[SC.M]             = (ushort)GLFW_KEY_M;
            null_.Keycodes[SC.N]             = (ushort)GLFW_KEY_N;
            null_.Keycodes[SC.O]             = (ushort)GLFW_KEY_O;
            null_.Keycodes[SC.P]             = (ushort)GLFW_KEY_P;
            null_.Keycodes[SC.Q]             = (ushort)GLFW_KEY_Q;
            null_.Keycodes[SC.R]             = (ushort)GLFW_KEY_R;
            null_.Keycodes[SC.S]             = (ushort)GLFW_KEY_S;
            null_.Keycodes[SC.T]             = (ushort)GLFW_KEY_T;
            null_.Keycodes[SC.U]             = (ushort)GLFW_KEY_U;
            null_.Keycodes[SC.V]             = (ushort)GLFW_KEY_V;
            null_.Keycodes[SC.W]             = (ushort)GLFW_KEY_W;
            null_.Keycodes[SC.X]             = (ushort)GLFW_KEY_X;
            null_.Keycodes[SC.Y]             = (ushort)GLFW_KEY_Y;
            null_.Keycodes[SC.Z]             = (ushort)GLFW_KEY_Z;
            null_.Keycodes[SC.LEFT_BRACKET]  = (ushort)GLFW_KEY_LEFT_BRACKET;
            null_.Keycodes[SC.BACKSLASH]     = (ushort)GLFW_KEY_BACKSLASH;
            null_.Keycodes[SC.RIGHT_BRACKET] = (ushort)GLFW_KEY_RIGHT_BRACKET;
            null_.Keycodes[SC.GRAVE_ACCENT]  = (ushort)GLFW_KEY_GRAVE_ACCENT;
            null_.Keycodes[SC.WORLD_1]       = (ushort)GLFW_KEY_WORLD_1;
            null_.Keycodes[SC.WORLD_2]       = (ushort)GLFW_KEY_WORLD_2;
            null_.Keycodes[SC.ESCAPE]        = (ushort)GLFW_KEY_ESCAPE;
            null_.Keycodes[SC.ENTER]         = (ushort)GLFW_KEY_ENTER;
            null_.Keycodes[SC.TAB]           = (ushort)GLFW_KEY_TAB;
            null_.Keycodes[SC.BACKSPACE]     = (ushort)GLFW_KEY_BACKSPACE;
            null_.Keycodes[SC.INSERT]        = (ushort)GLFW_KEY_INSERT;
            null_.Keycodes[SC.DELETE]        = (ushort)GLFW_KEY_DELETE;
            null_.Keycodes[SC.RIGHT]         = (ushort)GLFW_KEY_RIGHT;
            null_.Keycodes[SC.LEFT]          = (ushort)GLFW_KEY_LEFT;
            null_.Keycodes[SC.DOWN]          = (ushort)GLFW_KEY_DOWN;
            null_.Keycodes[SC.UP]            = (ushort)GLFW_KEY_UP;
            null_.Keycodes[SC.PAGE_UP]       = (ushort)GLFW_KEY_PAGE_UP;
            null_.Keycodes[SC.PAGE_DOWN]     = (ushort)GLFW_KEY_PAGE_DOWN;
            null_.Keycodes[SC.HOME]          = (ushort)GLFW_KEY_HOME;
            null_.Keycodes[SC.END]           = (ushort)GLFW_KEY_END;
            null_.Keycodes[SC.CAPS_LOCK]     = (ushort)GLFW_KEY_CAPS_LOCK;
            null_.Keycodes[SC.SCROLL_LOCK]   = (ushort)GLFW_KEY_SCROLL_LOCK;
            null_.Keycodes[SC.NUM_LOCK]      = (ushort)GLFW_KEY_NUM_LOCK;
            null_.Keycodes[SC.PRINT_SCREEN]  = (ushort)GLFW_KEY_PRINT_SCREEN;
            null_.Keycodes[SC.PAUSE]         = (ushort)GLFW_KEY_PAUSE;
            null_.Keycodes[SC.F1]            = (ushort)GLFW_KEY_F1;
            null_.Keycodes[SC.F2]            = (ushort)GLFW_KEY_F2;
            null_.Keycodes[SC.F3]            = (ushort)GLFW_KEY_F3;
            null_.Keycodes[SC.F4]            = (ushort)GLFW_KEY_F4;
            null_.Keycodes[SC.F5]            = (ushort)GLFW_KEY_F5;
            null_.Keycodes[SC.F6]            = (ushort)GLFW_KEY_F6;
            null_.Keycodes[SC.F7]            = (ushort)GLFW_KEY_F7;
            null_.Keycodes[SC.F8]            = (ushort)GLFW_KEY_F8;
            null_.Keycodes[SC.F9]            = (ushort)GLFW_KEY_F9;
            null_.Keycodes[SC.F10]           = (ushort)GLFW_KEY_F10;
            null_.Keycodes[SC.F11]           = (ushort)GLFW_KEY_F11;
            null_.Keycodes[SC.F12]           = (ushort)GLFW_KEY_F12;
            null_.Keycodes[SC.F13]           = (ushort)GLFW_KEY_F13;
            null_.Keycodes[SC.F14]           = (ushort)GLFW_KEY_F14;
            null_.Keycodes[SC.F15]           = (ushort)GLFW_KEY_F15;
            null_.Keycodes[SC.F16]           = (ushort)GLFW_KEY_F16;
            null_.Keycodes[SC.F17]           = (ushort)GLFW_KEY_F17;
            null_.Keycodes[SC.F18]           = (ushort)GLFW_KEY_F18;
            null_.Keycodes[SC.F19]           = (ushort)GLFW_KEY_F19;
            null_.Keycodes[SC.F20]           = (ushort)GLFW_KEY_F20;
            null_.Keycodes[SC.F21]           = (ushort)GLFW_KEY_F21;
            null_.Keycodes[SC.F22]           = (ushort)GLFW_KEY_F22;
            null_.Keycodes[SC.F23]           = (ushort)GLFW_KEY_F23;
            null_.Keycodes[SC.F24]           = (ushort)GLFW_KEY_F24;
            null_.Keycodes[SC.F25]           = (ushort)GLFW_KEY_F25;
            null_.Keycodes[SC.KP_0]          = (ushort)GLFW_KEY_KP_0;
            null_.Keycodes[SC.KP_1]          = (ushort)GLFW_KEY_KP_1;
            null_.Keycodes[SC.KP_2]          = (ushort)GLFW_KEY_KP_2;
            null_.Keycodes[SC.KP_3]          = (ushort)GLFW_KEY_KP_3;
            null_.Keycodes[SC.KP_4]          = (ushort)GLFW_KEY_KP_4;
            null_.Keycodes[SC.KP_5]          = (ushort)GLFW_KEY_KP_5;
            null_.Keycodes[SC.KP_6]          = (ushort)GLFW_KEY_KP_6;
            null_.Keycodes[SC.KP_7]          = (ushort)GLFW_KEY_KP_7;
            null_.Keycodes[SC.KP_8]          = (ushort)GLFW_KEY_KP_8;
            null_.Keycodes[SC.KP_9]          = (ushort)GLFW_KEY_KP_9;
            null_.Keycodes[SC.KP_DECIMAL]    = (ushort)GLFW_KEY_KP_DECIMAL;
            null_.Keycodes[SC.KP_DIVIDE]     = (ushort)GLFW_KEY_KP_DIVIDE;
            null_.Keycodes[SC.KP_MULTIPLY]   = (ushort)GLFW_KEY_KP_MULTIPLY;
            null_.Keycodes[SC.KP_SUBTRACT]   = (ushort)GLFW_KEY_KP_SUBTRACT;
            null_.Keycodes[SC.KP_ADD]        = (ushort)GLFW_KEY_KP_ADD;
            null_.Keycodes[SC.KP_ENTER]      = (ushort)GLFW_KEY_KP_ENTER;
            null_.Keycodes[SC.KP_EQUAL]      = (ushort)GLFW_KEY_KP_EQUAL;
            null_.Keycodes[SC.LEFT_SHIFT]    = (ushort)GLFW_KEY_LEFT_SHIFT;
            null_.Keycodes[SC.LEFT_CONTROL]  = (ushort)GLFW_KEY_LEFT_CONTROL;
            null_.Keycodes[SC.LEFT_ALT]      = (ushort)GLFW_KEY_LEFT_ALT;
            null_.Keycodes[SC.LEFT_SUPER]    = (ushort)GLFW_KEY_LEFT_SUPER;
            null_.Keycodes[SC.RIGHT_SHIFT]   = (ushort)GLFW_KEY_RIGHT_SHIFT;
            null_.Keycodes[SC.RIGHT_CONTROL] = (ushort)GLFW_KEY_RIGHT_CONTROL;
            null_.Keycodes[SC.RIGHT_ALT]     = (ushort)GLFW_KEY_RIGHT_ALT;
            null_.Keycodes[SC.RIGHT_SUPER]   = (ushort)GLFW_KEY_RIGHT_SUPER;
            null_.Keycodes[SC.MENU]          = (ushort)GLFW_KEY_MENU;

            // Build reverse scancode table
            for (int scancode = SC.FIRST; scancode < SC.LAST; scancode++)
            {
                int key = (short)null_.Keycodes[scancode]; // treat as signed
                if (key > 0)
                    null_.Scancodes[key] = (byte)scancode;
            }

            PollMonitorsNull();
            return true;
        }

        /// <summary>
        /// Terminates the null platform. Corresponds to C++ <c>_glfwTerminateNull</c>.
        /// </summary>
        public void Terminate()
        {
            Glfw._glfwTerminateOSMesa();
            Glfw._glfwTerminateEGL();
            if (_glfw.Null != null)
            {
                _glfw.Null.ClipboardString = null;
                _glfw.Null.FocusedWindow = null;
                _glfw.Null = null;
            }
        }
    }
}

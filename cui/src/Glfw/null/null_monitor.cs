// Ported from glfw/src/null_monitor.c (GLFW 3.6)
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

namespace Glfw
{
    // Partial class -- monitor operations for the null platform.
    public partial class NullPlatform
    {
        //----------------------------------------------------------------------
        // Private helpers
        //----------------------------------------------------------------------

        /// <summary>
        /// Returns the single fake video mode for the null monitor.
        /// Corresponds to C++ <c>getVideoMode</c> in null_monitor.c.
        /// </summary>
        private static GlfwVidMode GetNullVideoMode()
        {
            return new GlfwVidMode
            {
                Width = 1920,
                Height = 1080,
                RedBits = 8,
                GreenBits = 8,
                BlueBits = 8,
                RefreshRate = 60,
            };
        }

        //----------------------------------------------------------------------
        // GLFW internal API
        //----------------------------------------------------------------------

        /// <summary>
        /// Creates and registers the single null monitor.
        /// Corresponds to C++ <c>_glfwPollMonitorsNull</c>.
        /// </summary>
        public void PollMonitorsNull()
        {
            const float dpi = 141.0f;
            var mode = GetNullVideoMode();
            var monitor = Glfw._glfwAllocMonitor(
                "Null SuperNoop 0",
                (int)(mode.Width * 25.4f / dpi),
                (int)(mode.Height * 25.4f / dpi));

            monitor.Null = new GlfwMonitorNull();
            Glfw._glfwInputMonitor(monitor, GLFW_CONNECTED, GlfwConstants._GLFW_INSERT_FIRST);
        }

        //----------------------------------------------------------------------
        // IGlfwPlatform -- Monitor operations
        //----------------------------------------------------------------------

        public void FreeMonitor(GlfwMonitor monitor)
        {
            if (monitor.Null != null)
            {
                Glfw._glfwFreeGammaArrays(monitor.Null.Ramp);
            }
        }

        public void GetMonitorPos(GlfwMonitor monitor, out int xpos, out int ypos)
        {
            xpos = 0;
            ypos = 0;
        }

        public void GetMonitorContentScale(GlfwMonitor monitor, out float xscale, out float yscale)
        {
            xscale = 1.0f;
            yscale = 1.0f;
        }

        public void GetMonitorWorkarea(GlfwMonitor monitor, out int xpos, out int ypos,
                                       out int width, out int height)
        {
            var mode = GetNullVideoMode();
            xpos = 0;
            ypos = 10;
            width = mode.Width;
            height = mode.Height - 10;
        }

        public GlfwVidMode[]? GetVideoModes(GlfwMonitor monitor, out int count)
        {
            count = 1;
            return new[] { GetNullVideoMode() };
        }

        public bool GetVideoMode(GlfwMonitor monitor, out GlfwVidMode mode)
        {
            mode = GetNullVideoMode();
            return true;
        }

        public bool GetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
        {
            if (monitor.Null == null)
                monitor.Null = new GlfwMonitorNull();

            var monitorRamp = monitor.Null.Ramp;
            if (monitorRamp.Size == 0)
            {
                Glfw._glfwAllocGammaArrays(monitorRamp, 256);

                for (int i = 0; i < (int)monitorRamp.Size; i++)
                {
                    const float gamma = 2.2f;
                    float value;
                    value = i / (float)(monitorRamp.Size - 1);
                    value = MathF.Pow(value, 1.0f / gamma) * 65535.0f + 0.5f;
                    value = MathF.Min(value, 65535.0f);

                    monitorRamp.Red![i]   = (ushort)value;
                    monitorRamp.Green![i] = (ushort)value;
                    monitorRamp.Blue![i]  = (ushort)value;
                }
            }

            Glfw._glfwAllocGammaArrays(ramp, monitorRamp.Size);
            Array.Copy(monitorRamp.Red!,   ramp.Red!,   (int)ramp.Size);
            Array.Copy(monitorRamp.Green!, ramp.Green!, (int)ramp.Size);
            Array.Copy(monitorRamp.Blue!,  ramp.Blue!,  (int)ramp.Size);
            return true;
        }

        public void SetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
        {
            if (monitor.Null == null)
                monitor.Null = new GlfwMonitorNull();

            var monitorRamp = monitor.Null.Ramp;
            if (monitorRamp.Size != ramp.Size)
            {
                Glfw._glfwInputError(GLFW_PLATFORM_ERROR,
                    "Null: Gamma ramp size must match current ramp size");
                return;
            }

            Array.Copy(ramp.Red!,   monitorRamp.Red!,   (int)ramp.Size);
            Array.Copy(ramp.Green!, monitorRamp.Green!, (int)ramp.Size);
            Array.Copy(ramp.Blue!,  monitorRamp.Blue!,  (int)ramp.Size);
        }
    }
}

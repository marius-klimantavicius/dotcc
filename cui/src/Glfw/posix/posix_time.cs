// Ported from glfw/src/posix_time.c (GLFW 3.6)
//
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2017 Camilla Loewy <elmindreda@glfw.org>
//
// C uses clock_gettime(CLOCK_MONOTONIC). C# uses Stopwatch which wraps
// the same underlying monotonic clock on all platforms.
//
// The actual implementations of _glfwPlatformInitTimer,
// _glfwPlatformGetTimerValue, and _glfwPlatformGetTimerFrequency live
// in init.cs and input.cs respectively, using System.Diagnostics.Stopwatch.
// This file exists for structural parity with the C source tree.

using System.Diagnostics;

namespace Glfw
{
    public static partial class Glfw
    {
        //////////////////////////////////////////////////////////////////
        //////                  GLFW platform API                   //////
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the high-resolution timer.
        /// Ported from posix_time.c _glfwPlatformInitTimer.
        /// Called during glfwInit() -- sets timer.Offset.
        /// </summary>
        internal static void _glfwPlatformInitTimer()
        {
            // In C, this selects CLOCK_MONOTONIC and sets frequency to 1e9.
            // In C#, Stopwatch already uses the monotonic clock.
            _glfw.timer.Offset = (ulong)Stopwatch.GetTimestamp();
        }

        // _glfwPlatformGetTimerValue and _glfwPlatformGetTimerFrequency
        // are defined in input.cs using Stopwatch.GetTimestamp() and
        // Stopwatch.Frequency.
    }
}

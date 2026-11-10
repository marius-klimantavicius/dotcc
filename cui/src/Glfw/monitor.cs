// GLFW 3.6 - www.glfw.org
// Ported from glfw/src/monitor.c
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
using System.Collections.Generic;
using System.Diagnostics;

namespace Glfw;

public static partial class Glfw
{
    //----------------------------------------------------------------------
    // Lexically compare video modes, used by sort
    //----------------------------------------------------------------------

    private static int CompareVideoModes(GlfwVidMode fm, GlfwVidMode sm)
    {
        int fbpp = fm.RedBits + fm.GreenBits + fm.BlueBits;
        int sbpp = sm.RedBits + sm.GreenBits + sm.BlueBits;
        int farea = fm.Width * fm.Height;
        int sarea = sm.Width * sm.Height;

        // First sort on color bits per pixel
        if (fbpp != sbpp)
            return fbpp - sbpp;

        // Then sort on screen area
        if (farea != sarea)
            return farea - sarea;

        // Then sort on width
        if (fm.Width != sm.Width)
            return fm.Width - sm.Width;

        // Lastly sort on refresh rate
        return fm.RefreshRate - sm.RefreshRate;
    }

    //----------------------------------------------------------------------
    // Retrieves the available modes for the specified monitor
    //----------------------------------------------------------------------

    private static bool RefreshVideoModes(GlfwMonitor monitor)
    {
        if (monitor.Modes != null)
            return true;

        var modes = _glfw.platform!.GetVideoModes(monitor, out int modeCount);
        if (modes == null)
            return false;

        Array.Sort(modes, 0, modeCount, Comparer<GlfwVidMode>.Create(CompareVideoModes));

        monitor.Modes = modes;
        monitor.ModeCount = modeCount;

        return true;
    }

    //----------------------------------------------------------------------
    //                         GLFW event API
    //----------------------------------------------------------------------

    /// <summary>
    /// Notifies shared code of a monitor connection or disconnection.
    /// </summary>
    internal static void _glfwInputMonitor(GlfwMonitor monitor, int action, int placement)
    {
        Debug.Assert(monitor != null);
        Debug.Assert(action == GLFW.GLFW_CONNECTED || action == GLFW.GLFW_DISCONNECTED);
        Debug.Assert(placement == GlfwConstants._GLFW_INSERT_FIRST || placement == GlfwConstants._GLFW_INSERT_LAST);

        if (action == GLFW.GLFW_CONNECTED)
        {
            _glfw.monitorCount++;
            var newMonitors = new GlfwMonitor[_glfw.monitorCount];

            if (placement == GlfwConstants._GLFW_INSERT_FIRST)
            {
                newMonitors[0] = monitor;
                if (_glfw.monitors != null)
                    Array.Copy(_glfw.monitors, 0, newMonitors, 1, _glfw.monitorCount - 1);
            }
            else
            {
                if (_glfw.monitors != null)
                    Array.Copy(_glfw.monitors, 0, newMonitors, 0, _glfw.monitorCount - 1);
                newMonitors[_glfw.monitorCount - 1] = monitor;
            }

            _glfw.monitors = newMonitors;
        }
        else if (action == GLFW.GLFW_DISCONNECTED)
        {
            for (var window = _glfw.windowListHead; window != null; window = window.Next)
            {
                if (window.Monitor == monitor)
                {
                    _glfw.platform!.GetWindowSize(window, out int width, out int height);
                    _glfw.platform.SetWindowMonitor(window, null, 0, 0, width, height, 0);
                    _glfw.platform.GetWindowFrameSize(window, out int xoff, out int yoff, out _, out _);
                    _glfw.platform.SetWindowPos(window, xoff, yoff);
                }
            }

            if (_glfw.monitors != null)
            {
                for (int i = 0; i < _glfw.monitorCount; i++)
                {
                    if (_glfw.monitors[i] == monitor)
                    {
                        _glfw.monitorCount--;
                        var newMonitors = new GlfwMonitor[_glfw.monitorCount];
                        if (i > 0)
                            Array.Copy(_glfw.monitors, 0, newMonitors, 0, i);
                        if (i < _glfw.monitorCount)
                            Array.Copy(_glfw.monitors, i + 1, newMonitors, i, _glfw.monitorCount - i);
                        _glfw.monitors = _glfw.monitorCount > 0 ? newMonitors : null;
                        break;
                    }
                }
            }
        }

        _glfw.monitorCallback?.Invoke(monitor, action);

        if (action == GLFW.GLFW_DISCONNECTED)
            _glfwFreeMonitor(monitor);
    }

    /// <summary>
    /// Notifies shared code that a full screen window has acquired or released a monitor.
    /// </summary>
    internal static void _glfwInputMonitorWindow(GlfwMonitor monitor, GlfwWindow? window)
    {
        Debug.Assert(monitor != null);
        monitor.Window = window;
    }

    //----------------------------------------------------------------------
    //                       GLFW internal API
    //----------------------------------------------------------------------

    /// <summary>
    /// Allocates and returns a monitor object with the specified name and dimensions.
    /// </summary>
    internal static GlfwMonitor _glfwAllocMonitor(string name, int widthMM, int heightMM)
    {
        var monitor = new GlfwMonitor
        {
            // C++ strncpy limits to sizeof(monitor->name) - 1 = 127 chars
            Name = name.Length > 127 ? name.Substring(0, 127) : name,
            WidthMM = widthMM,
            HeightMM = heightMM,
        };

        return monitor;
    }

    /// <summary>
    /// Frees a monitor object and any data associated with it.
    /// </summary>
    internal static void _glfwFreeMonitor(GlfwMonitor? monitor)
    {
        if (monitor == null)
            return;

        _glfw.platform!.FreeMonitor(monitor);

        _glfwFreeGammaArrays(monitor.OriginalRamp);
        _glfwFreeGammaArrays(monitor.CurrentRamp);

        monitor.Modes = null;
        monitor.ModeCount = 0;
    }

    /// <summary>
    /// Allocates red, green and blue value arrays of the specified size.
    /// </summary>
    internal static void _glfwAllocGammaArrays(GlfwGammaRamp ramp, uint size)
    {
        ramp.Red = new ushort[size];
        ramp.Green = new ushort[size];
        ramp.Blue = new ushort[size];
        ramp.Size = size;
    }

    /// <summary>
    /// Frees the red, green and blue value arrays and clears the ramp.
    /// </summary>
    internal static void _glfwFreeGammaArrays(GlfwGammaRamp ramp)
    {
        ramp.Red = null;
        ramp.Green = null;
        ramp.Blue = null;
        ramp.Size = 0;
    }

    /// <summary>
    /// Chooses the video mode most closely matching the desired one.
    /// </summary>
    internal static GlfwVidMode? _glfwChooseVideoMode(GlfwMonitor monitor, in GlfwVidMode desired)
    {
        uint leastSizeDiff = uint.MaxValue;
        uint leastRateDiff = uint.MaxValue;
        uint leastColorDiff = uint.MaxValue;
        GlfwVidMode? closest = null;

        if (!RefreshVideoModes(monitor))
            return null;

        for (int i = 0; i < monitor.ModeCount; i++)
        {
            var current = monitor.Modes![i];

            uint colorDiff = 0;

            if (desired.RedBits != GLFW.GLFW_DONT_CARE)
                colorDiff += (uint)Math.Abs(current.RedBits - desired.RedBits);
            if (desired.GreenBits != GLFW.GLFW_DONT_CARE)
                colorDiff += (uint)Math.Abs(current.GreenBits - desired.GreenBits);
            if (desired.BlueBits != GLFW.GLFW_DONT_CARE)
                colorDiff += (uint)Math.Abs(current.BlueBits - desired.BlueBits);

            uint sizeDiff = (uint)Math.Abs(
                (current.Width - desired.Width) * (current.Width - desired.Width) +
                (current.Height - desired.Height) * (current.Height - desired.Height));

            uint rateDiff;
            if (desired.RefreshRate != GLFW.GLFW_DONT_CARE)
                rateDiff = (uint)Math.Abs(current.RefreshRate - desired.RefreshRate);
            else
                rateDiff = uint.MaxValue - (uint)current.RefreshRate;

            if ((colorDiff < leastColorDiff) ||
                (colorDiff == leastColorDiff && sizeDiff < leastSizeDiff) ||
                (colorDiff == leastColorDiff && sizeDiff == leastSizeDiff && rateDiff < leastRateDiff))
            {
                closest = current;
                leastSizeDiff = sizeDiff;
                leastRateDiff = rateDiff;
                leastColorDiff = colorDiff;
            }
        }

        return closest;
    }

    /// <summary>
    /// Performs lexical comparison between two GLFWvidmode structures.
    /// </summary>
    internal static int _glfwCompareVideoModes(in GlfwVidMode fm, in GlfwVidMode sm)
    {
        return CompareVideoModes(fm, sm);
    }

    /// <summary>
    /// Splits a color depth into red, green and blue bit depths.
    /// </summary>
    internal static void _glfwSplitBPP(int bpp, out int red, out int green, out int blue)
    {
        // We assume that by 32 the user really meant 24
        if (bpp == 32)
            bpp = 24;

        // Convert "bits per pixel" to red, green & blue sizes
        red = green = blue = bpp / 3;
        int delta = bpp - (red * 3);
        if (delta >= 1)
            green = green + 1;

        if (delta == 2)
            red = red + 1;
    }

    //----------------------------------------------------------------------
    //                        GLFW public API
    //----------------------------------------------------------------------

    /// <summary>
    /// Returns an array of handles for all currently connected monitors.
    /// </summary>
    public static GlfwMonitor[]? glfwGetMonitors(out int count)
    {
        count = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        count = _glfw.monitorCount;
        return _glfw.monitors;
    }

    /// <summary>
    /// Returns the primary monitor.
    /// </summary>
    public static GlfwMonitor? glfwGetPrimaryMonitor()
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (_glfw.monitorCount == 0)
            return null;

        return _glfw.monitors![0];
    }

    /// <summary>
    /// Returns the position of the monitor's viewport on the virtual screen.
    /// </summary>
    public static void glfwGetMonitorPos(GlfwMonitor monitor, out int xpos, out int ypos)
    {
        xpos = 0;
        ypos = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        _glfw.platform!.GetMonitorPos(monitor, out xpos, out ypos);
    }

    /// <summary>
    /// Returns the work area of the monitor.
    /// </summary>
    public static void glfwGetMonitorWorkarea(GlfwMonitor monitor,
                                              out int xpos, out int ypos,
                                              out int width, out int height)
    {
        xpos = 0;
        ypos = 0;
        width = 0;
        height = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        _glfw.platform!.GetMonitorWorkarea(monitor, out xpos, out ypos, out width, out height);
    }

    /// <summary>
    /// Returns the physical size of the monitor.
    /// </summary>
    public static void glfwGetMonitorPhysicalSize(GlfwMonitor monitor, out int widthMM, out int heightMM)
    {
        widthMM = 0;
        heightMM = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        widthMM = monitor.WidthMM;
        heightMM = monitor.HeightMM;
    }

    /// <summary>
    /// Retrieves the content scale for the specified monitor.
    /// </summary>
    public static void glfwGetMonitorContentScale(GlfwMonitor monitor,
                                                   out float xscale, out float yscale)
    {
        xscale = 0f;
        yscale = 0f;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        _glfw.platform!.GetMonitorContentScale(monitor, out xscale, out yscale);
    }

    /// <summary>
    /// Returns a human-readable name of the monitor.
    /// </summary>
    public static string? glfwGetMonitorName(GlfwMonitor monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(monitor != null);

        return monitor.Name;
    }

    /// <summary>
    /// Sets the user pointer of the specified monitor.
    /// </summary>
    public static void glfwSetMonitorUserPointer(GlfwMonitor monitor, object? pointer)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        monitor.UserPointer = pointer;
    }

    /// <summary>
    /// Returns the user pointer of the specified monitor.
    /// </summary>
    public static object? glfwGetMonitorUserPointer(GlfwMonitor monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(monitor != null);

        return monitor.UserPointer;
    }

    /// <summary>
    /// Sets the monitor configuration callback.
    /// Returns the previously set callback, or null.
    /// </summary>
    public static GlfwMonitorFun? glfwSetMonitorCallback(GlfwMonitorFun? cbfun)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        var previous = _glfw.monitorCallback;
        _glfw.monitorCallback = cbfun;
        return previous;
    }

    /// <summary>
    /// Returns the available video modes for the specified monitor.
    /// </summary>
    public static GlfwVidMode[]? glfwGetVideoModes(GlfwMonitor monitor, out int count)
    {
        count = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(monitor != null);

        if (!RefreshVideoModes(monitor))
            return null;

        count = monitor.ModeCount;
        return monitor.Modes;
    }

    /// <summary>
    /// Returns the current mode of the specified monitor.
    /// </summary>
    public static GlfwVidMode? glfwGetVideoMode(GlfwMonitor monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(monitor != null);

        if (!_glfw.platform!.GetVideoMode(monitor, out var mode))
            return null;

        monitor.CurrentMode = mode;
        return mode;
    }

    /// <summary>
    /// Generates a gamma ramp and sets it for the specified monitor.
    /// </summary>
    public static void glfwSetGamma(GlfwMonitor monitor, float gamma)
    {
        Debug.Assert(gamma > 0f);
        Debug.Assert(float.IsFinite(gamma));

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        if (!float.IsFinite(gamma) || gamma <= 0f)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE, "Invalid gamma value {0}", gamma);
            return;
        }

        var original = glfwGetGammaRamp(monitor);
        if (original == null)
            return;

        var values = new ushort[original.Size];

        for (uint i = 0; i < original.Size; i++)
        {
            // Calculate intensity
            float value = i / (float)(original.Size - 1);
            // Apply gamma curve
            value = MathF.Pow(value, 1f / gamma) * 65535f + 0.5f;
            // Clamp to value range
            value = MathF.Min(value, 65535f);

            values[i] = (ushort)value;
        }

        var ramp = new GlfwGammaRamp
        {
            Red = values,
            Green = values,
            Blue = values,
            Size = original.Size,
        };

        glfwSetGammaRamp(monitor, ramp);
    }

    /// <summary>
    /// Returns the current gamma ramp for the specified monitor.
    /// </summary>
    public static GlfwGammaRamp? glfwGetGammaRamp(GlfwMonitor monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return null;
        }

        Debug.Assert(monitor != null);

        _glfwFreeGammaArrays(monitor.CurrentRamp);
        if (!_glfw.platform!.GetGammaRamp(monitor, monitor.CurrentRamp))
            return null;

        return monitor.CurrentRamp;
    }

    /// <summary>
    /// Sets the current gamma ramp for the specified monitor.
    /// </summary>
    public static void glfwSetGammaRamp(GlfwMonitor monitor, GlfwGammaRamp ramp)
    {
        Debug.Assert(ramp != null);
        Debug.Assert(ramp.Size > 0);
        Debug.Assert(ramp.Red != null);
        Debug.Assert(ramp.Green != null);
        Debug.Assert(ramp.Blue != null);

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return;
        }

        Debug.Assert(monitor != null);

        if (ramp.Size <= 0)
        {
            _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                            "Invalid gamma ramp size {0}", ramp.Size);
            return;
        }

        if (monitor.OriginalRamp.Size == 0)
        {
            if (!_glfw.platform!.GetGammaRamp(monitor, monitor.OriginalRamp))
                return;
        }

        _glfw.platform!.SetGammaRamp(monitor, ramp);
    }
}

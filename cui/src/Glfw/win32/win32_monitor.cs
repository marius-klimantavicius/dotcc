// Ported from glfw/src/win32_monitor.c (GLFW 3.6)
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
using static Glfw.GlfwConstants;

namespace Glfw;

public static unsafe partial class Glfw
{
    // -----------------------------------------------------------------------
    // Win32 constants for monitor functions (not defined elsewhere)
    // -----------------------------------------------------------------------

    private const uint DISPLAY_DEVICE_ACTIVE = 0x00000001;
    private const uint DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;
    private const uint DISPLAY_DEVICE_MODESPRUNED = 0x08000000;
    private const int DISP_CHANGE_BADDUALVIEW = -6;
    private const int DISP_CHANGE_BADFLAGS = -4;
    private const int DISP_CHANGE_BADPARAM = -5;
    private const int DISP_CHANGE_NOTUPDATED = -3;
    private const int HORZSIZE = 4;
    private const int VERTSIZE = 6;
    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;
    private const int S_OK = 0;

    // -----------------------------------------------------------------------
    // Callback for EnumDisplayMonitors in createMonitor
    // -----------------------------------------------------------------------

    // We pass the GlfwMonitor via a thread-static field since the LPARAM
    // is more conveniently used for the raw nint callback pointer.
    [ThreadStatic]
    private static GlfwMonitor? _monitorCallbackTarget;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static int MonitorCallbackProc(nint handle, nint dc, RECT* rect, nint data)
    {
        var u32 = Win32Native.user32!;

        MONITORINFOEXW mi = default;
        mi.cbSize = (uint)sizeof(MONITORINFOEXW);

        if (u32.GetMonitorInfoW(handle, &mi) != 0)
        {
            var monitor = _monitorCallbackTarget;
            if (monitor?.Win32 != null)
            {
                // Compare szDevice with monitor's adapterName
                fixed (char* adapterName = monitor.Win32.adapterName)
                {
                    if (StringEquals(mi.szDevice, adapterName))
                        monitor.Win32.handle = handle;
                }
            }
        }

        return 1; // TRUE -- continue enumeration
    }

    // Helper: compare two null-terminated wide strings
    private static bool StringEquals(char* a, char* b)
    {
        while (*a != '\0' && *b != '\0')
        {
            if (*a != *b) return false;
            a++;
            b++;
        }
        return *a == *b;
    }

    // Helper: copy a fixed char buffer to a managed string
    private static string FixedCharToString(char* src, int maxLen)
    {
        int len = 0;
        while (len < maxLen && src[len] != '\0')
            len++;
        return new string(src, 0, len);
    }

    // -----------------------------------------------------------------------
    // Create monitor from an adapter and (optionally) a display
    // -----------------------------------------------------------------------

    private static GlfwMonitor? CreateMonitorWin32(DISPLAY_DEVICEW* adapter,
                                                    DISPLAY_DEVICEW* display)
    {
        var u32 = Win32Native.user32!;
        var g32 = Win32Native.gdi32!;
        int widthMM, heightMM;
        string name;

        if (display != null)
            name = FixedCharToString(display->DeviceString, 128);
        else
            name = FixedCharToString(adapter->DeviceString, 128);
        if (string.IsNullOrEmpty(name))
            return null;

        DEVMODEW dm = default;
        dm.dmSize = (ushort)sizeof(DEVMODEW);
        u32.EnumDisplaySettingsW(adapter->DeviceName, Win32.ENUM_CURRENT_SETTINGS, &dm);

        fixed (char* displayStr = "DISPLAY")
        {
            nint dc = Win32Native.gdi32!.CreateDCW(displayStr, adapter->DeviceName, 0, 0);

            if (_glfwIsWindows8Point1OrGreaterWin32())
            {
                widthMM  = g32.GetDeviceCaps(dc, HORZSIZE);
                heightMM = g32.GetDeviceCaps(dc, VERTSIZE);
            }
            else
            {
                widthMM  = (int)(dm.dmPelsWidth * 25.4f / g32.GetDeviceCaps(dc, LOGPIXELSX));
                heightMM = (int)(dm.dmPelsHeight * 25.4f / g32.GetDeviceCaps(dc, LOGPIXELSY));
            }

            g32.DeleteDC(dc);
        }

        var monitor = _glfwAllocMonitor(name, widthMM, heightMM);

        monitor.Win32 = new GlfwMonitorWin32();

        if ((adapter->StateFlags & DISPLAY_DEVICE_MODESPRUNED) != 0)
            monitor.Win32.modesPruned = true;

        monitor.Win32.adapterName = FixedCharToString(adapter->DeviceName, 32);
        monitor.Win32.publicAdapterName = monitor.Win32.adapterName;

        if (display != null)
        {
            monitor.Win32.displayName = FixedCharToString(display->DeviceName, 32);
            monitor.Win32.publicDisplayName = monitor.Win32.displayName;
        }

        RECT rect;
        rect.left   = dm.dmPositionX;
        rect.top    = dm.dmPositionY;
        rect.right  = dm.dmPositionX + (int)dm.dmPelsWidth;
        rect.bottom = dm.dmPositionY + (int)dm.dmPelsHeight;

        _monitorCallbackTarget = monitor;
        u32.EnumDisplayMonitors(0, &rect, (nint)(delegate* unmanaged[Stdcall]<nint, nint, RECT*, nint, int>)&MonitorCallbackProc, 0);
        _monitorCallbackTarget = null;

        return monitor;
    }


    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW internal API                      //////
    //////////////////////////////////////////////////////////////////////////

    // Poll for changes in the set of connected monitors
    //
    internal static void _glfwPollMonitorsWin32()
    {
        var u32 = Win32Native.user32!;
        int i, disconnectedCount;
        GlfwMonitor?[]? disconnected = null;

        disconnectedCount = _glfw.monitorCount;
        if (disconnectedCount != 0)
        {
            disconnected = new GlfwMonitor?[_glfw.monitorCount];
            Array.Copy(_glfw.monitors!, disconnected, _glfw.monitorCount);
        }

        for (uint adapterIndex = 0; ; adapterIndex++)
        {
            int type = _GLFW_INSERT_LAST;

            DISPLAY_DEVICEW adapter = default;
            adapter.cb = (uint)sizeof(DISPLAY_DEVICEW);

            if (u32.EnumDisplayDevicesW(null, adapterIndex, &adapter, 0) == 0)
                break;

            if ((adapter.StateFlags & DISPLAY_DEVICE_ACTIVE) == 0)
                continue;

            if ((adapter.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0)
                type = _GLFW_INSERT_FIRST;

            uint displayIndex;
            for (displayIndex = 0; ; displayIndex++)
            {
                DISPLAY_DEVICEW display = default;
                display.cb = (uint)sizeof(DISPLAY_DEVICEW);

                if (u32.EnumDisplayDevicesW(adapter.DeviceName, displayIndex, &display, 0) == 0)
                    break;

                if ((display.StateFlags & DISPLAY_DEVICE_ACTIVE) == 0)
                    continue;

                string displayName = FixedCharToString(display.DeviceName, 32);

                for (i = 0; i < disconnectedCount; i++)
                {
                    if (disconnected![i] != null &&
                        disconnected[i]!.Win32 != null &&
                        disconnected[i]!.Win32!.displayName == displayName)
                    {
                        disconnected[i] = null;
                        // handle may have changed, update
                        _monitorCallbackTarget = _glfw.monitors![i];
                        u32.EnumDisplayMonitors(0, null, (nint)(delegate* unmanaged[Stdcall]<nint, nint, RECT*, nint, int>)&MonitorCallbackProc, 0);
                        _monitorCallbackTarget = null;
                        break;
                    }
                }

                if (i < disconnectedCount)
                    continue;

                var monitor = CreateMonitorWin32(&adapter, &display);
                if (monitor == null)
                    return;

                _glfwInputMonitor(monitor, GLFW_CONNECTED, type);

                type = _GLFW_INSERT_LAST;
            }

            // HACK: If an active adapter does not have any display devices
            //       (as sometimes happens), add it directly as a monitor
            if (displayIndex == 0)
            {
                string adapterName = FixedCharToString(adapter.DeviceName, 32);

                for (i = 0; i < disconnectedCount; i++)
                {
                    if (disconnected![i] != null &&
                        disconnected[i]!.Win32 != null &&
                        disconnected[i]!.Win32!.adapterName == adapterName)
                    {
                        disconnected[i] = null;
                        break;
                    }
                }

                if (i < disconnectedCount)
                    continue;

                var monitor = CreateMonitorWin32(&adapter, null);
                if (monitor == null)
                    return;

                _glfwInputMonitor(monitor, GLFW_CONNECTED, type);
            }
        }

        for (i = 0; i < disconnectedCount; i++)
        {
            if (disconnected![i] != null)
                _glfwInputMonitor(disconnected[i]!, GLFW_DISCONNECTED, 0);
        }
    }

    // Change the current video mode
    //
    internal static void _glfwSetVideoModeWin32(GlfwMonitor monitor, in GlfwVidMode desired)
    {
        var u32 = Win32Native.user32!;

        GlfwVidMode? best = _glfwChooseVideoMode(monitor, desired);
        if (best == null)
            return;
        _glfwGetVideoModeWin32(monitor, out var current);
        if (_glfwCompareVideoModes(current, best.Value) == 0)
            return;

        DEVMODEW dm = default;
        dm.dmSize = (ushort)sizeof(DEVMODEW);
        dm.dmFields           = Win32.DM_PELSWIDTH | Win32.DM_PELSHEIGHT | Win32.DM_BITSPERPEL |
                                Win32.DM_DISPLAYFREQUENCY;
        dm.dmPelsWidth        = (uint)best.Value.Width;
        dm.dmPelsHeight       = (uint)best.Value.Height;
        dm.dmBitsPerPel       = (uint)(best.Value.RedBits + best.Value.GreenBits + best.Value.BlueBits);
        dm.dmDisplayFrequency = (uint)best.Value.RefreshRate;

        if (dm.dmBitsPerPel < 15 || dm.dmBitsPerPel >= 24)
            dm.dmBitsPerPel = 32;

        int result;
        fixed (char* adapterName = monitor.Win32!.adapterName)
        {
            result = u32.ChangeDisplaySettingsExW(adapterName, &dm, 0, Win32.CDS_FULLSCREEN, 0);
        }

        if (result == (int)Win32.DISP_CHANGE_SUCCESSFUL)
            monitor.Win32.modeChanged = true;
        else
        {
            string description = "Unknown error";

            if (result == DISP_CHANGE_BADDUALVIEW)
                description = "The system uses DualView";
            else if (result == DISP_CHANGE_BADFLAGS)
                description = "Invalid flags";
            else if (result == unchecked((int)Win32.DISP_CHANGE_BADMODE))
                description = "Graphics mode not supported";
            else if (result == DISP_CHANGE_BADPARAM)
                description = "Invalid parameter";
            else if (result == unchecked((int)Win32.DISP_CHANGE_FAILED))
                description = "Graphics mode failed";
            else if (result == DISP_CHANGE_NOTUPDATED)
                description = "Failed to write to registry";
            else if (result == (int)Win32.DISP_CHANGE_RESTART)
                description = "Computer restart required";

            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "Win32: Failed to set video mode: {0}",
                            description);
        }
    }

    // Restore the previously saved (original) video mode
    //
    internal static void _glfwRestoreVideoModeWin32(GlfwMonitor monitor)
    {
        var u32 = Win32Native.user32!;

        if (monitor.Win32!.modeChanged)
        {
            fixed (char* adapterName = monitor.Win32.adapterName)
            {
                u32.ChangeDisplaySettingsExW(adapterName, null, 0, Win32.CDS_FULLSCREEN, 0);
            }
            monitor.Win32.modeChanged = false;
        }
    }

    internal static void _glfwGetHMONITORContentScaleWin32(nint handle, out float xscale, out float yscale)
    {
        xscale = 0f;
        yscale = 0f;

        if (_glfwIsWindows8Point1OrGreaterWin32())
        {
            var win32 = _glfw.Win32!;
            if (win32.shcore.GetDpiForMonitor_ != null)
            {
                uint xdpi, ydpi;
                int hr = win32.shcore.GetDpiForMonitor_(handle, (int)MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, &xdpi, &ydpi);
                if (hr != S_OK)
                {
                    _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to query monitor DPI");
                    return;
                }

                xscale = xdpi / (float)Win32.USER_DEFAULT_SCREEN_DPI;
                yscale = ydpi / (float)Win32.USER_DEFAULT_SCREEN_DPI;
                return;
            }
        }

        // Fallback: system DPI
        var g32 = Win32Native.gdi32!;
        nint dc = g32.GetDC(0);
        uint sxdpi = (uint)g32.GetDeviceCaps(dc, LOGPIXELSX);
        uint sydpi = (uint)g32.GetDeviceCaps(dc, LOGPIXELSY);
        g32.ReleaseDC(0, dc);

        xscale = sxdpi / (float)Win32.USER_DEFAULT_SCREEN_DPI;
        yscale = sydpi / (float)Win32.USER_DEFAULT_SCREEN_DPI;
    }


    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW platform API                      //////
    //////////////////////////////////////////////////////////////////////////

    internal static void _glfwFreeMonitorWin32(GlfwMonitor monitor)
    {
    }

    internal static void _glfwGetMonitorPosWin32(GlfwMonitor monitor, out int xpos, out int ypos)
    {
        var u32 = Win32Native.user32!;
        xpos = 0;
        ypos = 0;

        DEVMODEW dm = default;
        dm.dmSize = (ushort)sizeof(DEVMODEW);

        fixed (char* adapterName = monitor.Win32!.adapterName)
        {
            u32.EnumDisplaySettingsExW(adapterName, Win32.ENUM_CURRENT_SETTINGS, &dm, Win32.EDS_ROTATEDMODE);
        }

        xpos = dm.dmPositionX;
        ypos = dm.dmPositionY;
    }

    internal static void _glfwGetMonitorContentScaleWin32(GlfwMonitor monitor,
                                                           out float xscale, out float yscale)
    {
        _glfwGetHMONITORContentScaleWin32(monitor.Win32!.handle, out xscale, out yscale);
    }

    internal static void _glfwGetMonitorWorkareaWin32(GlfwMonitor monitor,
                                                       out int xpos, out int ypos,
                                                       out int width, out int height)
    {
        var u32 = Win32Native.user32!;
        xpos = 0;
        ypos = 0;
        width = 0;
        height = 0;

        MONITORINFOEXW mi = default;
        mi.cbSize = (uint)sizeof(MONITORINFOEXW);
        u32.GetMonitorInfoW(monitor.Win32!.handle, &mi);

        xpos   = mi.rcWork.left;
        ypos   = mi.rcWork.top;
        width  = mi.rcWork.right - mi.rcWork.left;
        height = mi.rcWork.bottom - mi.rcWork.top;
    }

    internal static GlfwVidMode[]? _glfwGetVideoModesWin32(GlfwMonitor monitor, out int count)
    {
        var u32 = Win32Native.user32!;
        int modeIndex = 0, size = 0;
        GlfwVidMode[]? result = null;

        count = 0;

        for (;;)
        {
            int i;
            GlfwVidMode mode;

            DEVMODEW dm = default;
            dm.dmSize = (ushort)sizeof(DEVMODEW);

            int ok;
            fixed (char* adapterName = monitor.Win32!.adapterName)
            {
                ok = u32.EnumDisplaySettingsW(adapterName, (uint)modeIndex, &dm);
            }
            if (ok == 0)
                break;

            modeIndex++;

            // Skip modes with less than 15 BPP
            if (dm.dmBitsPerPel < 15)
                continue;

            mode.Width  = (int)dm.dmPelsWidth;
            mode.Height = (int)dm.dmPelsHeight;
            mode.RefreshRate = (int)dm.dmDisplayFrequency;
            _glfwSplitBPP((int)dm.dmBitsPerPel,
                          out mode.RedBits,
                          out mode.GreenBits,
                          out mode.BlueBits);

            for (i = 0; i < count; i++)
            {
                if (_glfwCompareVideoModes(result![i], mode) == 0)
                    break;
            }

            // Skip duplicate modes
            if (i < count)
                continue;

            if (monitor.Win32.modesPruned)
            {
                // Skip modes not supported by the connected displays
                int testResult;
                fixed (char* adapterName = monitor.Win32.adapterName)
                {
                    testResult = u32.ChangeDisplaySettingsExW(adapterName, &dm, 0, Win32.CDS_TEST, 0);
                }
                if (testResult != (int)Win32.DISP_CHANGE_SUCCESSFUL)
                    continue;
            }

            if (count == size)
            {
                size += 128;
                var newResult = new GlfwVidMode[size];
                if (result != null)
                    Array.Copy(result, newResult, count);
                result = newResult;
            }

            count++;
            result![count - 1] = mode;
        }

        if (count == 0)
        {
            // HACK: Report the current mode if no valid modes were found
            result = new GlfwVidMode[1];
            _glfwGetVideoModeWin32(monitor, out result[0]);
            count = 1;
        }

        return result;
    }

    internal static bool _glfwGetVideoModeWin32(GlfwMonitor monitor, out GlfwVidMode mode)
    {
        var u32 = Win32Native.user32!;
        mode = default;

        DEVMODEW dm = default;
        dm.dmSize = (ushort)sizeof(DEVMODEW);

        int ok;
        fixed (char* adapterName = monitor.Win32!.adapterName)
        {
            ok = u32.EnumDisplaySettingsW(adapterName, Win32.ENUM_CURRENT_SETTINGS, &dm);
        }
        if (ok == 0)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR, "Win32: Failed to query display settings");
            return false;
        }

        mode.Width  = (int)dm.dmPelsWidth;
        mode.Height = (int)dm.dmPelsHeight;
        mode.RefreshRate = (int)dm.dmDisplayFrequency;
        _glfwSplitBPP((int)dm.dmBitsPerPel,
                      out mode.RedBits,
                      out mode.GreenBits,
                      out mode.BlueBits);

        return true;
    }

    internal static bool _glfwGetGammaRampWin32(GlfwMonitor monitor, GlfwGammaRamp ramp)
    {
        var g32 = Win32Native.gdi32!;

        // WORD values[3][256] -- 3 channels, 256 entries each
        ushort* values = stackalloc ushort[3 * 256];

        nint dc;
        fixed (char* displayStr = "DISPLAY")
        fixed (char* adapterName = monitor.Win32!.adapterName)
        {
            dc = Win32Native.gdi32!.CreateDCW(displayStr, adapterName, 0, 0);
        }
        g32.GetDeviceGammaRamp(dc, values);
        g32.DeleteDC(dc);

        _glfwAllocGammaArrays(ramp, 256);

        for (int i = 0; i < 256; i++)
        {
            ramp.Red![i]   = values[0 * 256 + i];
            ramp.Green![i] = values[1 * 256 + i];
            ramp.Blue![i]  = values[2 * 256 + i];
        }

        return true;
    }

    internal static void _glfwSetGammaRampWin32(GlfwMonitor monitor, GlfwGammaRamp ramp)
    {
        var g32 = Win32Native.gdi32!;

        if (ramp.Size != 256)
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "Win32: Gamma ramp size must be 256");
            return;
        }

        // WORD values[3][256]
        ushort* values = stackalloc ushort[3 * 256];

        for (int i = 0; i < 256; i++)
        {
            values[0 * 256 + i] = ramp.Red![i];
            values[1 * 256 + i] = ramp.Green![i];
            values[2 * 256 + i] = ramp.Blue![i];
        }

        nint dc;
        fixed (char* displayStr = "DISPLAY")
        fixed (char* adapterName = monitor.Win32!.adapterName)
        {
            dc = Win32Native.gdi32!.CreateDCW(displayStr, adapterName, 0, 0);
        }
        g32.SetDeviceGammaRamp(dc, values);
        g32.DeleteDC(dc);
    }


    //////////////////////////////////////////////////////////////////////////
    //////                        GLFW native API                       //////
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns the adapter device name of the specified monitor.
    /// Corresponds to C <c>glfwGetWin32Adapter</c>.
    /// </summary>
    public static string? glfwGetWin32Adapter(GlfwMonitor? monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW_PLATFORM_WIN32)
        {
            _glfwInputError(GLFW_PLATFORM_UNAVAILABLE, "Win32: Platform not initialized");
            return null;
        }

        if (monitor?.Win32 == null)
            return null;

        return monitor.Win32.publicAdapterName;
    }

    /// <summary>
    /// Returns the display device name of the specified monitor.
    /// Corresponds to C <c>glfwGetWin32Monitor</c>.
    /// </summary>
    public static string? glfwGetWin32Monitor(GlfwMonitor? monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW_NOT_INITIALIZED, null);
            return null;
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW_PLATFORM_WIN32)
        {
            _glfwInputError(GLFW_PLATFORM_UNAVAILABLE, "Win32: Platform not initialized");
            return null;
        }

        if (monitor?.Win32 == null)
            return null;

        return monitor.Win32.publicDisplayName;
    }
}

// Ported from glfw/src/x11_monitor.c (GLFW 3.6)
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
using System.Runtime.InteropServices;
using static Glfw.GLFW;
using static Glfw.GlfwConstants;

namespace Glfw;

// Partial class -- monitor operations for the X11 platform.
// The X11Platform class declaration and remaining IGlfwPlatform methods
// are provided by other partial files (x11_init.cs, x11_window.cs, etc.).
public static partial class Glfw
{
    //----------------------------------------------------------------------
    // XRandR struct layouts for reading opaque nint pointers.
    // These match the C struct layouts from <X11/extensions/Xrandr.h>.
    //----------------------------------------------------------------------

    // XRRModeInfo
    [StructLayout(LayoutKind.Sequential)]
    internal struct XRRModeInfo
    {
        public nuint id;           // RRMode
        public uint width;
        public uint height;
        public nuint dotClock;     // unsigned long
        public uint hSyncStart;
        public uint hSyncEnd;
        public uint hTotal;
        public uint hSkew;
        public uint vSyncStart;
        public uint vSyncEnd;
        public uint vTotal;
        public nint name;          // char*
        public uint nameLength;
        public nuint modeFlags;    // XRRModeFlags (unsigned long)
    }

    // XRRScreenResources
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct XRRScreenResources
    {
        public nuint timestamp;       // Time
        public nuint configTimestamp;  // Time
        public int ncrtc;
        public nuint* crtcs;          // RRCrtc*
        public int noutput;
        public nuint* outputs;        // RROutput*
        public int nmode;
        public XRRModeInfo* modes;    // XRRModeInfo*
    }

    // XRROutputInfo
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct XRROutputInfo
    {
        public nuint timestamp;       // Time
        public nuint crtc;            // RRCrtc
        public nint name;             // char*
        public int nameLen;
        public nuint mm_width;        // unsigned long
        public nuint mm_height;       // unsigned long
        public ushort connection;     // Connection
        public ushort subpixel_order; // SubpixelOrder
        public int ncrtc;
        public nuint* crtcs;          // RRCrtc*
        public int nclone;
        public nuint* clones;         // RROutput*
        public int nmode;
        public int npreferred;
        public nuint* modes;          // RRMode*
    }

    // XRRCrtcInfo
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct XRRCrtcInfo
    {
        public nuint timestamp;       // Time
        public int x;
        public int y;
        public uint width;
        public uint height;
        public nuint mode;            // RRMode
        public ushort rotation;       // Rotation
        public int noutput;
        public nuint* outputs;        // RROutput*
        public ushort rotations;      // Rotation
        public int npossible;
        public nuint* possible;       // RROutput*
    }

    // XRRCrtcGamma
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct XRRCrtcGamma
    {
        public int size;
        public ushort* red;
        public ushort* green;
        public ushort* blue;
    }

    // XineramaScreenInfo
    [StructLayout(LayoutKind.Sequential)]
    internal struct XineramaScreenInfo
    {
        public int screen_number;
        public short x_org;
        public short y_org;
        public short width;
        public short height;
    }

    //----------------------------------------------------------------------
    // XRandR / X11 constants
    //----------------------------------------------------------------------

    private const ushort RR_Connected  = 0;
    private const ushort RR_Rotate_90  = 0x0002;
    private const ushort RR_Rotate_270 = 0x0008;
    private const uint   RR_Interlace  = 0x00000010;

    // XA_CARDINAL is a pre-defined X11 atom (value 6)
    private const nuint XA_CARDINAL = 6;

    //----------------------------------------------------------------------
    // Private helpers  (correspond to file-scope statics in x11_monitor.c)
    //----------------------------------------------------------------------

    // Check whether the display mode should be included in enumeration
    //
    private static bool ModeIsGood(in XRRModeInfo mi)
    {
        return (mi.modeFlags & RR_Interlace) == 0;
    }

    // Calculates the refresh rate, in Hz, from the specified RandR mode info
    //
    private static int CalculateRefreshRate(in XRRModeInfo mi)
    {
        if (mi.hTotal != 0 && mi.vTotal != 0)
            return (int)Math.Round((double)mi.dotClock / ((double)mi.hTotal * (double)mi.vTotal), MidpointRounding.AwayFromZero);
        else
            return 0;
    }

    // Returns the mode info for a RandR mode XID
    //
    private static unsafe XRRModeInfo* GetModeInfo(XRRScreenResources* sr, nuint id)
    {
        for (int i = 0; i < sr->nmode; i++)
        {
            if (sr->modes[i].id == id)
                return &sr->modes[i];
        }

        return null;
    }

    // Convert RandR mode info to GLFW video mode
    //
    private static unsafe GlfwVidMode VidmodeFromModeInfo(XRRModeInfo* mi, XRRCrtcInfo* ci)
    {
        var x11 = _glfw.X11!;
        GlfwVidMode mode;

        if (ci->rotation == RR_Rotate_90 || ci->rotation == RR_Rotate_270)
        {
            mode.Width  = (int)mi->height;
            mode.Height = (int)mi->width;
        }
        else
        {
            mode.Width  = (int)mi->width;
            mode.Height = (int)mi->height;
        }

        mode.RefreshRate = CalculateRefreshRate(*mi);

        _glfwSplitBPP(x11.xlib.DefaultDepth(x11.display, x11.screen),
                       out mode.RedBits, out mode.GreenBits, out mode.BlueBits);

        return mode;
    }

    // Xlib macro equivalents -- now loaded as function pointers through
    // x11.xlib.DefaultDepth, x11.xlib.DisplayWidth, etc. (see x11_native.cs).

    // Helper: read an X11 window property, returning number of items.
    // Caller must XFree the returned value pointer.
    //
    internal static unsafe nuint _glfwGetWindowPropertyX11(nuint window, nuint property, nuint type, out nint value)
    {
        var x11 = _glfw.X11!;
        value = 0;

        nuint actualType;
        int actualFormat;
        nuint nItems;
        nuint bytesAfter;
        byte* propReturn = null;

        x11.xlib.GetWindowProperty(x11.display, window, property,
                                   0, nint.MaxValue, 0, type,
                                   &actualType, &actualFormat,
                                   &nItems, &bytesAfter, &propReturn);

        if (actualType != type)
        {
            if (propReturn != null)
                x11.xlib.Free((nint)propReturn);
            return 0;
        }

        value = (nint)propReturn;
        return nItems;
    }


    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW internal API                      //////
    //////////////////////////////////////////////////////////////////////////

    // Poll for changes in the set of connected monitors
    //
    internal static unsafe void _glfwPollMonitorsX11()
    {
        var x11 = _glfw.X11!;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            int disconnectedCount, screenCount = 0;
            GlfwMonitor?[]? disconnected = null;
            XineramaScreenInfo* screens = null;
            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            nuint primary = x11.randr.GetOutputPrimary(x11.display, x11.root);

            if (x11.xinerama.available)
                screens = (XineramaScreenInfo*)x11.xinerama.QueryScreens(x11.display, &screenCount);

            disconnectedCount = _glfw.monitorCount;
            if (disconnectedCount != 0)
            {
                disconnected = new GlfwMonitor?[_glfw.monitorCount];
                Array.Copy(_glfw.monitors!, disconnected, _glfw.monitorCount);
            }

            for (int i = 0; i < sr->noutput; i++)
            {
                int j, widthMM, heightMM;

                XRROutputInfo* oi = (XRROutputInfo*)x11.randr.GetOutputInfo(
                                        x11.display, (nint)sr, sr->outputs[i]);
                if (oi->connection != RR_Connected || oi->crtc == 0)
                {
                    x11.randr.FreeOutputInfo((nint)oi);
                    continue;
                }

                for (j = 0; j < disconnectedCount; j++)
                {
                    if (disconnected![j] != null &&
                        disconnected[j]!.X11!.output == sr->outputs[i])
                    {
                        disconnected[j] = null;
                        break;
                    }
                }

                if (j < disconnectedCount)
                {
                    x11.randr.FreeOutputInfo((nint)oi);
                    continue;
                }

                XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                      x11.display, (nint)sr, oi->crtc);
                if (ci == null)
                {
                    x11.randr.FreeOutputInfo((nint)oi);
                    continue;
                }

                if (ci->rotation == RR_Rotate_90 || ci->rotation == RR_Rotate_270)
                {
                    widthMM  = (int)oi->mm_height;
                    heightMM = (int)oi->mm_width;
                }
                else
                {
                    widthMM  = (int)oi->mm_width;
                    heightMM = (int)oi->mm_height;
                }

                if (widthMM <= 0 || heightMM <= 0)
                {
                    // HACK: If RandR does not provide a physical size, assume the
                    //       X11 default 96 DPI and calculate from the CRTC viewport
                    // NOTE: These members are affected by rotation, unlike the mode
                    //       info and output info members
                    widthMM  = (int)(ci->width * 25.4f / 96.0f);
                    heightMM = (int)(ci->height * 25.4f / 96.0f);
                }

                string name = Marshal.PtrToStringAnsi(oi->name, oi->nameLen) ?? string.Empty;
                GlfwMonitor monitor = _glfwAllocMonitor(name, widthMM, heightMM);
                monitor.X11 = new GlfwMonitorX11
                {
                    output = sr->outputs[i],
                    crtc   = oi->crtc,
                };

                for (j = 0; j < screenCount; j++)
                {
                    if (screens[j].x_org == ci->x &&
                        screens[j].y_org == ci->y &&
                        screens[j].width == (short)ci->width &&
                        screens[j].height == (short)ci->height)
                    {
                        monitor.X11.index = j;
                        break;
                    }
                }

                int type;
                if (monitor.X11.output == primary)
                    type = _GLFW_INSERT_FIRST;
                else
                    type = _GLFW_INSERT_LAST;

                _glfwInputMonitor(monitor, GLFW_CONNECTED, type);

                x11.randr.FreeOutputInfo((nint)oi);
                x11.randr.FreeCrtcInfo((nint)ci);
            }

            x11.randr.FreeScreenResources((nint)sr);

            if (screens != null)
                x11.xlib.Free((nint)screens);

            for (int i = 0; i < disconnectedCount; i++)
            {
                if (disconnected![i] != null)
                    _glfwInputMonitor(disconnected[i]!, GLFW_DISCONNECTED, 0);
            }
        }
        else
        {
            int widthMM = x11.xlib.DisplayWidthMM(x11.display, x11.screen);
            int heightMM = x11.xlib.DisplayHeightMM(x11.display, x11.screen);

            _glfwInputMonitor(_glfwAllocMonitor("Display", widthMM, heightMM),
                              GLFW_CONNECTED,
                              _GLFW_INSERT_FIRST);
        }
    }

    // Set the current video mode for the specified monitor
    //
    internal static unsafe void _glfwSetVideoModeX11(GlfwMonitor monitor, in GlfwVidMode desired)
    {
        var x11 = _glfw.X11!;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            GlfwVidMode current;
            nuint native_ = 0;  // RRMode None

            GlfwVidMode? best = _glfwChooseVideoMode(monitor, desired);
            if (best == null)
                return;
            _glfwGetVideoModeX11(monitor, out current);
            if (_glfwCompareVideoModes(current, best.Value) == 0)
                return;

            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                  x11.display, (nint)sr, monitor.X11!.crtc);
            XRROutputInfo* oi = (XRROutputInfo*)x11.randr.GetOutputInfo(
                                    x11.display, (nint)sr, monitor.X11.output);

            for (int i = 0; i < oi->nmode; i++)
            {
                XRRModeInfo* mi = GetModeInfo(sr, oi->modes[i]);
                if (mi == null || !ModeIsGood(*mi))
                    continue;

                GlfwVidMode mode = VidmodeFromModeInfo(mi, ci);
                if (_glfwCompareVideoModes(best.Value, mode) == 0)
                {
                    native_ = mi->id;
                    break;
                }
            }

            if (native_ != 0)
            {
                if (monitor.X11.oldMode == 0)
                    monitor.X11.oldMode = ci->mode;

                x11.randr.SetCrtcConfig(x11.display,
                                        (nint)sr, monitor.X11.crtc,
                                        0, // CurrentTime
                                        ci->x, ci->y,
                                        native_,
                                        ci->rotation,
                                        ci->outputs,
                                        ci->noutput);
            }

            x11.randr.FreeOutputInfo((nint)oi);
            x11.randr.FreeCrtcInfo((nint)ci);
            x11.randr.FreeScreenResources((nint)sr);
        }
    }

    // Restore the saved (original) video mode for the specified monitor
    //
    internal static unsafe void _glfwRestoreVideoModeX11(GlfwMonitor monitor)
    {
        var x11 = _glfw.X11!;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            if (monitor.X11 == null || monitor.X11.oldMode == 0)
                return;

            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                  x11.display, (nint)sr, monitor.X11.crtc);

            x11.randr.SetCrtcConfig(x11.display,
                                    (nint)sr, monitor.X11.crtc,
                                    0, // CurrentTime
                                    ci->x, ci->y,
                                    monitor.X11.oldMode,
                                    ci->rotation,
                                    ci->outputs,
                                    ci->noutput);

            x11.randr.FreeCrtcInfo((nint)ci);
            x11.randr.FreeScreenResources((nint)sr);

            monitor.X11.oldMode = 0;
        }
    }

    // Shared current-mode implementation for platform dispatch and SetVideoMode
    //
    internal static unsafe bool _glfwGetVideoModeX11(GlfwMonitor monitor, out GlfwVidMode mode)
    {
        var x11 = _glfw.X11!;
        mode = default;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            XRRModeInfo* mi = null;

            XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                  x11.display, (nint)sr, monitor.X11!.crtc);
            if (ci != null)
            {
                mi = GetModeInfo(sr, ci->mode);
                if (mi != null)
                    mode = VidmodeFromModeInfo(mi, ci);

                x11.randr.FreeCrtcInfo((nint)ci);
            }

            x11.randr.FreeScreenResources((nint)sr);

            if (mi == null)
            {
                _glfwInputError(GLFW_PLATFORM_ERROR, "X11: Failed to query video mode");
                return false;
            }
        }
        else
        {
            mode.Width = x11.xlib.DisplayWidth(x11.display, x11.screen);
            mode.Height = x11.xlib.DisplayHeight(x11.display, x11.screen);
            mode.RefreshRate = 0;

            _glfwSplitBPP(x11.xlib.DefaultDepth(x11.display, x11.screen),
                           out mode.RedBits, out mode.GreenBits, out mode.BlueBits);
        }

        return true;
    }


    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW platform API                      //////
    //////////////////////////////////////////////////////////////////////////

    internal static void _glfwFreeMonitorX11(GlfwMonitor monitor)
    {
        // No X11-specific resources to free for monitors
    }

    internal static unsafe void _glfwGetMonitorPosX11(GlfwMonitor monitor, out int xpos, out int ypos)
    {
        var x11 = _glfw.X11!;
        xpos = 0;
        ypos = 0;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                  x11.display, (nint)sr, monitor.X11!.crtc);

            if (ci != null)
            {
                xpos = ci->x;
                ypos = ci->y;

                x11.randr.FreeCrtcInfo((nint)ci);
            }

            x11.randr.FreeScreenResources((nint)sr);
        }
    }

    internal static void _glfwGetMonitorContentScaleX11(GlfwMonitor monitor,
                                                        out float xscale, out float yscale)
    {
        var x11 = _glfw.X11!;
        xscale = x11.contentScaleX;
        yscale = x11.contentScaleY;
    }

    internal static unsafe void _glfwGetMonitorWorkareaX11(GlfwMonitor monitor,
                                                           out int xpos, out int ypos,
                                                           out int width, out int height)
    {
        var x11 = _glfw.X11!;
        int areaX = 0, areaY = 0, areaWidth = 0, areaHeight = 0;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                  x11.display, (nint)sr, monitor.X11!.crtc);

            areaX = ci->x;
            areaY = ci->y;

            XRRModeInfo* mi = GetModeInfo(sr, ci->mode);

            if (ci->rotation == RR_Rotate_90 || ci->rotation == RR_Rotate_270)
            {
                areaWidth  = (int)mi->height;
                areaHeight = (int)mi->width;
            }
            else
            {
                areaWidth  = (int)mi->width;
                areaHeight = (int)mi->height;
            }

            x11.randr.FreeCrtcInfo((nint)ci);
            x11.randr.FreeScreenResources((nint)sr);
        }
        else
        {
            areaWidth  = x11.xlib.DisplayWidth(x11.display, x11.screen);
            areaHeight = x11.xlib.DisplayHeight(x11.display, x11.screen);
        }

        if (x11.NET_WORKAREA != 0 && x11.NET_CURRENT_DESKTOP != 0)
        {
            nuint extentCount = _glfwGetWindowPropertyX11(x11.root,
                                                          x11.NET_WORKAREA,
                                                          XA_CARDINAL,
                                                          out nint extentsPtr);

            nuint desktopCount = _glfwGetWindowPropertyX11(x11.root,
                                                           x11.NET_CURRENT_DESKTOP,
                                                           XA_CARDINAL,
                                                           out nint desktopPtr);

            if (desktopCount > 0)
            {
                // CARDINAL properties are stored as arrays of C long (nint-sized values)
                nuint desktop = (nuint)Marshal.ReadIntPtr(desktopPtr);

                if (extentCount >= 4 && desktop < extentCount / 4)
                {
                    int stride = nint.Size; // sizeof(long) on the platform
                    nint baseOff = (nint)(desktop * 4);

                    int globalX      = (int)Marshal.ReadIntPtr(extentsPtr, (int)(baseOff + 0) * stride);
                    int globalY      = (int)Marshal.ReadIntPtr(extentsPtr, (int)(baseOff + 1) * stride);
                    int globalWidth  = (int)Marshal.ReadIntPtr(extentsPtr, (int)(baseOff + 2) * stride);
                    int globalHeight = (int)Marshal.ReadIntPtr(extentsPtr, (int)(baseOff + 3) * stride);

                    if (areaX < globalX)
                    {
                        areaWidth -= globalX - areaX;
                        areaX = globalX;
                    }

                    if (areaY < globalY)
                    {
                        areaHeight -= globalY - areaY;
                        areaY = globalY;
                    }

                    if (areaX + areaWidth > globalX + globalWidth)
                        areaWidth = globalX - areaX + globalWidth;
                    if (areaY + areaHeight > globalY + globalHeight)
                        areaHeight = globalY - areaY + globalHeight;
                }
            }

            if (extentsPtr != 0)
                x11.xlib.Free(extentsPtr);
            if (desktopPtr != 0)
                x11.xlib.Free(desktopPtr);
        }

        xpos   = areaX;
        ypos   = areaY;
        width  = areaWidth;
        height = areaHeight;
    }

    internal static unsafe GlfwVidMode[]? _glfwGetVideoModesX11(GlfwMonitor monitor, out int count)
    {
        var x11 = _glfw.X11!;
        GlfwVidMode[] result;

        count = 0;

        if (x11.randr.available && !x11.randr.monitorBroken)
        {
            XRRScreenResources* sr = (XRRScreenResources*)x11.randr.GetScreenResourcesCurrent(
                                         x11.display, x11.root);
            XRRCrtcInfo* ci = (XRRCrtcInfo*)x11.randr.GetCrtcInfo(
                                  x11.display, (nint)sr, monitor.X11!.crtc);
            XRROutputInfo* oi = (XRROutputInfo*)x11.randr.GetOutputInfo(
                                    x11.display, (nint)sr, monitor.X11.output);

            result = new GlfwVidMode[oi->nmode];

            for (int i = 0; i < oi->nmode; i++)
            {
                XRRModeInfo* mi = GetModeInfo(sr, oi->modes[i]);
                if (mi == null || !ModeIsGood(*mi))
                    continue;

                GlfwVidMode mode = VidmodeFromModeInfo(mi, ci);
                int j;

                for (j = 0; j < count; j++)
                {
                    if (_glfwCompareVideoModes(result[j], mode) == 0)
                        break;
                }

                // Skip duplicate modes
                if (j < count)
                    continue;

                count++;
                result[count - 1] = mode;
            }

            x11.randr.FreeOutputInfo((nint)oi);
            x11.randr.FreeCrtcInfo((nint)ci);
            x11.randr.FreeScreenResources((nint)sr);
        }
        else
        {
            count = 1;
            result = new GlfwVidMode[1];
            _glfwGetVideoModeX11(monitor, out result[0]);
        }

        return result;
    }

    internal static unsafe bool _glfwGetGammaRampX11(GlfwMonitor monitor, GlfwGammaRamp ramp)
    {
        var x11 = _glfw.X11!;

        if (x11.randr.available && !x11.randr.gammaBroken)
        {
            int size = x11.randr.GetCrtcGammaSize(x11.display, monitor.X11!.crtc);
            XRRCrtcGamma* gamma = (XRRCrtcGamma*)x11.randr.GetCrtcGamma(
                                      x11.display, monitor.X11.crtc);

            _glfwAllocGammaArrays(ramp, (uint)size);

            for (int i = 0; i < size; i++)
            {
                ramp.Red![i]   = gamma->red[i];
                ramp.Green![i] = gamma->green[i];
                ramp.Blue![i]  = gamma->blue[i];
            }

            x11.randr.FreeGamma((nint)gamma);
            return true;
        }
        else if (x11.vidmode.available)
        {
            int size;
            x11.vidmode.GetGammaRampSize(x11.display, x11.screen, &size);

            _glfwAllocGammaArrays(ramp, (uint)size);

            fixed (ushort* r = ramp.Red, g = ramp.Green, b = ramp.Blue)
            {
                x11.vidmode.GetGammaRamp(x11.display, x11.screen, size, r, g, b);
            }
            return true;
        }
        else
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "X11: Gamma ramp access not supported by server");
            return false;
        }
    }

    internal static unsafe void _glfwSetGammaRampX11(GlfwMonitor monitor, GlfwGammaRamp ramp)
    {
        var x11 = _glfw.X11!;

        if (x11.randr.available && !x11.randr.gammaBroken)
        {
            if (x11.randr.GetCrtcGammaSize(x11.display, monitor.X11!.crtc) != (int)ramp.Size)
            {
                _glfwInputError(GLFW_PLATFORM_ERROR,
                                "X11: Gamma ramp size must match current ramp size");
                return;
            }

            XRRCrtcGamma* gamma = (XRRCrtcGamma*)x11.randr.AllocGamma((int)ramp.Size);

            for (int i = 0; i < (int)ramp.Size; i++)
            {
                gamma->red[i]   = ramp.Red![i];
                gamma->green[i] = ramp.Green![i];
                gamma->blue[i]  = ramp.Blue![i];
            }

            x11.randr.SetCrtcGamma(x11.display, monitor.X11.crtc, (nint)gamma);
            x11.randr.FreeGamma((nint)gamma);
        }
        else if (x11.vidmode.available)
        {
            fixed (ushort* r = ramp.Red, g = ramp.Green, b = ramp.Blue)
            {
                x11.vidmode.SetGammaRamp(x11.display, x11.screen, (int)ramp.Size, r, g, b);
            }
        }
        else
        {
            _glfwInputError(GLFW_PLATFORM_ERROR,
                            "X11: Gamma ramp access not supported by server");
        }
    }


    //////////////////////////////////////////////////////////////////////////
    //////                        GLFW native API                       //////
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns the XRandR CRTC (adapter) for the specified monitor.
    /// Corresponds to C <c>glfwGetX11Adapter</c>.
    /// </summary>
    public static nuint glfwGetX11Adapter(GlfwMonitor? monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW_NOT_INITIALIZED, null);
            return 0; // None
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW_PLATFORM_X11)
        {
            _glfwInputError(GLFW_PLATFORM_UNAVAILABLE, "X11: Platform not initialized");
            return 0; // None
        }

        if (monitor?.X11 == null)
            return 0;

        return monitor.X11.crtc;
    }

    /// <summary>
    /// Returns the XRandR output for the specified monitor.
    /// Corresponds to C <c>glfwGetX11Monitor</c>.
    /// </summary>
    public static nuint glfwGetX11Monitor(GlfwMonitor? monitor)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW_NOT_INITIALIZED, null);
            return 0; // None
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW_PLATFORM_X11)
        {
            _glfwInputError(GLFW_PLATFORM_UNAVAILABLE, "X11: Platform not initialized");
            return 0; // None
        }

        if (monitor?.X11 == null)
            return 0;

        return monitor.X11.output;
    }
}

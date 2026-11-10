// GLFW 3.6 WGL - www.glfw.org
// Ported from glfw/src/wgl_context.c
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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Glfw;

public static unsafe partial class Glfw
{
    // PFD flags
    private const uint PFD_DRAW_TO_WINDOW  = 0x00000004;
    private const uint PFD_SUPPORT_OPENGL  = 0x00000020;
    private const uint PFD_DOUBLEBUFFER    = 0x00000001;
    private const uint PFD_STEREO          = 0x00000002;
    private const uint PFD_GENERIC_FORMAT  = 0x00000040;
    private const uint PFD_GENERIC_ACCELERATED = 0x00001000;
    private const byte PFD_TYPE_RGBA       = 0;

    // HRESULT success check
    private static bool SUCCEEDED(int hr) => hr >= 0;

    // -----------------------------------------------------------------------
    // Return the value corresponding to the specified attribute
    // -----------------------------------------------------------------------

    private static int findPixelFormatAttribValueWGL(int* attribs,
                                                      int attribCount,
                                                      int* values,
                                                      int attrib)
    {
        for (int i = 0; i < attribCount; i++)
        {
            if (attribs[i] == attrib)
                return values[i];
        }

        _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                             "WGL: Unknown pixel format attribute requested");
        return 0;
    }

    // -----------------------------------------------------------------------
    // Return a list of available and usable framebuffer configs
    // -----------------------------------------------------------------------

    private static int choosePixelFormatWGL(GlfwWindow window,
                                             GlfwCtxConfig ctxconfig,
                                             GlfwFbConfig fbconfig)
    {
        int i, pixelFormat, nativeCount, usableCount = 0, attribCount = 0;
        int* attribs = stackalloc int[40];
        int* values  = stackalloc int[40];

        nativeCount = Win32Native.gdi32!.DescribePixelFormat(window.context.wgl!.dc,
                                           1,
                                           (uint)sizeof(PIXELFORMATDESCRIPTOR),
                                           null);

        if (_glfw.wgl!.ARB_pixel_format)
        {
            // ADD_ATTRIB macro equivalent
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_SUPPORT_OPENGL_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_DRAW_TO_WINDOW_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_PIXEL_TYPE_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ACCELERATION_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_RED_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_RED_SHIFT_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_GREEN_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_GREEN_SHIFT_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_BLUE_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_BLUE_SHIFT_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ALPHA_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ALPHA_SHIFT_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_DEPTH_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_STENCIL_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ACCUM_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ACCUM_RED_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ACCUM_GREEN_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ACCUM_BLUE_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_ACCUM_ALPHA_BITS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_AUX_BUFFERS_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_STEREO_ARB;
            Debug.Assert(attribCount < 40);
            attribs[attribCount++] = WGL.WGL_DOUBLE_BUFFER_ARB;

            if (_glfw.wgl.ARB_multisample)
            {
                Debug.Assert(attribCount < 40);
                attribs[attribCount++] = WGL.WGL_SAMPLES_ARB;
            }

            if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
            {
                if (_glfw.wgl.ARB_framebuffer_sRGB || _glfw.wgl.EXT_framebuffer_sRGB)
                {
                    Debug.Assert(attribCount < 40);
                    attribs[attribCount++] = WGL.WGL_FRAMEBUFFER_SRGB_CAPABLE_ARB;
                }
            }
            else
            {
                if (_glfw.wgl.EXT_colorspace)
                {
                    Debug.Assert(attribCount < 40);
                    attribs[attribCount++] = WGL.WGL_COLORSPACE_EXT;
                }
            }

            // NOTE: In a Parallels VM WGL_ARB_pixel_format returns fewer pixel formats than
            //       DescribePixelFormat, violating the guarantees of the extension spec
            // HACK: Iterate through the minimum of both counts

            int attrib = WGL.WGL_NUMBER_PIXEL_FORMATS_ARB;
            int extensionCount;

            if (_glfw.wgl.GetPixelFormatAttribivARB(window.context.wgl.dc,
                                                      1, 0, 1, &attrib, &extensionCount) == 0)
            {
                _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                     "WGL: Failed to retrieve pixel format attribute");
                return 0;
            }

            nativeCount = Math.Min(nativeCount, extensionCount);
        }

        var usableConfigs = new GlfwFbConfig[nativeCount];
        for (int j = 0; j < nativeCount; j++)
            usableConfigs[j] = new GlfwFbConfig();

        for (i = 0; i < nativeCount; i++)
        {
            var u = usableConfigs[usableCount];
            pixelFormat = i + 1;

            if (_glfw.wgl.ARB_pixel_format)
            {
                // Get pixel format attributes through "modern" extension

                if (_glfw.wgl.GetPixelFormatAttribivARB(window.context.wgl.dc,
                                                          pixelFormat, 0,
                                                          (uint)attribCount,
                                                          attribs, values) == 0)
                {
                    _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                        "WGL: Failed to retrieve pixel format attributes");
                    return 0;
                }

                if (findPixelFormatAttribValueWGL(attribs, attribCount, values,
                        WGL.WGL_SUPPORT_OPENGL_ARB) == 0 ||
                    findPixelFormatAttribValueWGL(attribs, attribCount, values,
                        WGL.WGL_DRAW_TO_WINDOW_ARB) == 0)
                {
                    continue;
                }

                if (findPixelFormatAttribValueWGL(attribs, attribCount, values,
                        WGL.WGL_PIXEL_TYPE_ARB) != WGL.WGL_TYPE_RGBA_ARB)
                    continue;

                if (findPixelFormatAttribValueWGL(attribs, attribCount, values,
                        WGL.WGL_ACCELERATION_ARB) == WGL.WGL_NO_ACCELERATION_ARB)
                    continue;

                if (findPixelFormatAttribValueWGL(attribs, attribCount, values,
                        WGL.WGL_DOUBLE_BUFFER_ARB) != (fbconfig.Doublebuffer ? 1 : 0))
                    continue;

                u.RedBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_RED_BITS_ARB);
                u.GreenBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_GREEN_BITS_ARB);
                u.BlueBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_BLUE_BITS_ARB);
                u.AlphaBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_ALPHA_BITS_ARB);

                u.DepthBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_DEPTH_BITS_ARB);
                u.StencilBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_STENCIL_BITS_ARB);

                u.AccumRedBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_ACCUM_RED_BITS_ARB);
                u.AccumGreenBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_ACCUM_GREEN_BITS_ARB);
                u.AccumBlueBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_ACCUM_BLUE_BITS_ARB);
                u.AccumAlphaBits = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_ACCUM_ALPHA_BITS_ARB);

                u.AuxBuffers = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_AUX_BUFFERS_ARB);

                u.Stereo = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                    WGL.WGL_STEREO_ARB) != 0;

                if (_glfw.wgl.ARB_multisample)
                    u.Samples = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                        WGL.WGL_SAMPLES_ARB);

                if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
                {
                    if (_glfw.wgl.ARB_framebuffer_sRGB || _glfw.wgl.EXT_framebuffer_sRGB)
                        u.SRGB = findPixelFormatAttribValueWGL(attribs, attribCount, values,
                            WGL.WGL_FRAMEBUFFER_SRGB_CAPABLE_ARB) != 0;
                }
                else
                {
                    if (_glfw.wgl.EXT_colorspace)
                    {
                        if (findPixelFormatAttribValueWGL(attribs, attribCount, values,
                                WGL.WGL_COLORSPACE_EXT) == WGL.WGL_COLORSPACE_SRGB_EXT)
                            u.SRGB = true;
                    }
                }
            }
            else
            {
                // Get pixel format attributes through legacy PFDs

                PIXELFORMATDESCRIPTOR pfd;

                if (Win32Native.gdi32!.DescribePixelFormat(window.context.wgl.dc,
                                         pixelFormat,
                                         (uint)sizeof(PIXELFORMATDESCRIPTOR),
                                         &pfd) == 0)
                {
                    _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                        "WGL: Failed to describe pixel format");
                    return 0;
                }

                if ((pfd.dwFlags & PFD_DRAW_TO_WINDOW) == 0 ||
                    (pfd.dwFlags & PFD_SUPPORT_OPENGL) == 0)
                {
                    continue;
                }

                if ((pfd.dwFlags & PFD_GENERIC_ACCELERATED) == 0 &&
                    (pfd.dwFlags & PFD_GENERIC_FORMAT) != 0)
                {
                    continue;
                }

                if (pfd.iPixelType != PFD_TYPE_RGBA)
                    continue;

                if (((pfd.dwFlags & PFD_DOUBLEBUFFER) != 0) != fbconfig.Doublebuffer)
                    continue;

                u.RedBits = pfd.cRedBits;
                u.GreenBits = pfd.cGreenBits;
                u.BlueBits = pfd.cBlueBits;
                u.AlphaBits = pfd.cAlphaBits;

                u.DepthBits = pfd.cDepthBits;
                u.StencilBits = pfd.cStencilBits;

                u.AccumRedBits = pfd.cAccumRedBits;
                u.AccumGreenBits = pfd.cAccumGreenBits;
                u.AccumBlueBits = pfd.cAccumBlueBits;
                u.AccumAlphaBits = pfd.cAccumAlphaBits;

                u.AuxBuffers = pfd.cAuxBuffers;
                u.Stereo = (pfd.dwFlags & PFD_STEREO) != 0;
            }

            u.Handle = (nuint)pixelFormat;
            usableCount++;
        }

        if (usableCount == 0)
        {
            _glfwInputError(GLFW.GLFW_API_UNAVAILABLE,
                            "WGL: The driver does not appear to support OpenGL");
            return 0;
        }

        int closestIndex = _glfwChooseFBConfig(fbconfig, usableConfigs, usableCount);
        if (closestIndex < 0)
        {
            _glfwInputError(GLFW.GLFW_FORMAT_UNAVAILABLE,
                            "WGL: Failed to find a suitable pixel format");
            return 0;
        }

        pixelFormat = (int)usableConfigs[closestIndex].Handle;

        return pixelFormat;
    }

    private static void makeContextCurrentWGL(GlfwWindow? window)
    {
        if (window != null)
        {
            if (_glfw.wgl!.MakeCurrent(window.context.wgl!.dc,
                                         window.context.wgl.handle) != 0)
                _glfw.contextSlot = window;
            else
            {
                _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                     "WGL: Failed to make context current");
                _glfw.contextSlot = null;
            }
        }
        else
        {
            if (_glfw.wgl!.MakeCurrent(0, 0) == 0)
            {
                _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                     "WGL: Failed to clear current context");
            }

            _glfw.contextSlot = null;
        }
    }

    private static void swapBuffersWGL(GlfwWindow window)
    {
        if (window.Monitor == null)
        {
            // HACK: Use DwmFlush when desktop composition is enabled on Windows 7
            if (!_glfwIsWindows8OrGreaterWin32())
            {
                var dwm = _glfw.Win32!.dwmapi;
                int enabled = 0;

                if (dwm.IsCompositionEnabled != null &&
                    SUCCEEDED(dwm.IsCompositionEnabled(&enabled)) && enabled != 0)
                {
                    int count = Math.Abs(window.context.wgl!.interval);
                    while (count-- > 0)
                        dwm.Flush();
                }
            }
        }

        Win32Native.gdi32!.SwapBuffers(window.context.wgl!.dc);
    }

    private static void swapIntervalWGL(int interval)
    {
        GlfwWindow? window = _glfw.contextSlot;
        Debug.Assert(window != null);

        window!.context.wgl!.interval = interval;

        if (window.Monitor == null)
        {
            // HACK: Disable WGL swap interval when desktop composition is enabled on
            //       Windows 7 to avoid interfering with DWM vsync
            if (!_glfwIsWindows8OrGreaterWin32())
            {
                var dwm = _glfw.Win32!.dwmapi;
                int enabled = 0;

                if (dwm.IsCompositionEnabled != null &&
                    SUCCEEDED(dwm.IsCompositionEnabled(&enabled)) && enabled != 0)
                    interval = 0;
            }
        }

        if (_glfw.wgl!.EXT_swap_control)
            _glfw.wgl.SwapIntervalEXT(interval);
    }

    private static bool extensionSupportedWGL(string extension)
    {
        nint extensions = 0;

        if (_glfw.wgl!.GetExtensionsStringARB != null)
            extensions = _glfw.wgl.GetExtensionsStringARB(_glfw.wgl.GetCurrentDC());
        else if (_glfw.wgl.GetExtensionsStringEXT != null)
            extensions = _glfw.wgl.GetExtensionsStringEXT();

        if (extensions == 0)
            return false;

        string? extensionsStr = Marshal.PtrToStringAnsi(extensions);
        if (extensionsStr == null)
            return false;

        return _glfwStringInExtensionString(extension, extensionsStr);
    }

    private static nint getProcAddressWGL(string procname)
    {
        byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(procname + '\0');
        fixed (byte* namePtr = nameBytes)
        {
            nint proc = _glfw.wgl!.GetProcAddress((nint)namePtr);
            if (proc != 0)
                return proc;
        }

        return _glfwPlatformGetModuleSymbol(_glfw.wgl!.instance, procname);
    }

    private static void destroyContextWGL(GlfwWindow window)
    {
        if (window.context.wgl != null && window.context.wgl.handle != 0)
        {
            _glfw.wgl!.DeleteContext(window.context.wgl.handle);
            window.context.wgl.handle = 0;
        }
    }


    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW internal API                      //////
    //////////////////////////////////////////////////////////////////////////

    internal static bool _glfwInitWGL()
    {
        if (_glfw.wgl == null)
            _glfw.wgl = new GlfwLibraryWGL();

        if (_glfw.wgl.instance != 0)
            return true;

        _glfw.wgl.instance = _glfwPlatformLoadModule("opengl32.dll");
        if (_glfw.wgl.instance == 0)
        {
            _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                 "WGL: Failed to load opengl32.dll");
            return false;
        }

        nint p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglCreateContext");
        _glfw.wgl.CreateContext = (delegate* unmanaged[Stdcall]<nint, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglDeleteContext");
        _glfw.wgl.DeleteContext = (delegate* unmanaged[Stdcall]<nint, int>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglGetProcAddress");
        _glfw.wgl.GetProcAddress = (delegate* unmanaged[Stdcall]<nint, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglGetCurrentDC");
        _glfw.wgl.GetCurrentDC = (delegate* unmanaged[Stdcall]<nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglGetCurrentContext");
        _glfw.wgl.GetCurrentContext = (delegate* unmanaged[Stdcall]<nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglMakeCurrent");
        _glfw.wgl.MakeCurrent = (delegate* unmanaged[Stdcall]<nint, nint, int>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.wgl.instance, "wglShareLists");
        _glfw.wgl.ShareLists = (delegate* unmanaged[Stdcall]<nint, nint, int>)p;

        // NOTE: A dummy context has to be created for opengl32.dll to load the
        //       OpenGL ICD, from which we can then query WGL extensions
        // NOTE: This code will accept the Microsoft GDI ICD; accelerated context
        //       creation failure occurs during manual pixel format enumeration

        nint dc = Win32Native.gdi32!.GetDC(_glfw.Win32!.helperWindowHandle);

        PIXELFORMATDESCRIPTOR pfd = default;
        pfd.nSize = (ushort)sizeof(PIXELFORMATDESCRIPTOR);
        pfd.nVersion = 1;
        pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
        pfd.iPixelType = PFD_TYPE_RGBA;
        pfd.cColorBits = 24;

        if (Win32Native.gdi32!.SetPixelFormat(dc, Win32Native.gdi32!.ChoosePixelFormat(dc, &pfd), &pfd) == 0)
        {
            _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                 "WGL: Failed to set pixel format for dummy context");
            return false;
        }

        nint rc = _glfw.wgl.CreateContext(dc);
        if (rc == 0)
        {
            _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                 "WGL: Failed to create dummy context");
            return false;
        }

        nint pdc = _glfw.wgl.GetCurrentDC();
        nint prc = _glfw.wgl.GetCurrentContext();

        if (_glfw.wgl.MakeCurrent(dc, rc) == 0)
        {
            _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                 "WGL: Failed to make dummy context current");
            _glfw.wgl.MakeCurrent(pdc, prc);
            _glfw.wgl.DeleteContext(rc);
            return false;
        }

        // NOTE: Functions must be loaded first as they're needed to retrieve the
        //       extension string that tells us whether the functions are supported

        byte[] extNameBytes;
        nint extFuncPtr;

        extNameBytes = System.Text.Encoding.ASCII.GetBytes("wglGetExtensionsStringEXT\0");
        fixed (byte* namePtr = extNameBytes)
        {
            extFuncPtr = _glfw.wgl.GetProcAddress((nint)namePtr);
            _glfw.wgl.GetExtensionsStringEXT = (delegate* unmanaged[Stdcall]<nint>)extFuncPtr;
        }

        extNameBytes = System.Text.Encoding.ASCII.GetBytes("wglGetExtensionsStringARB\0");
        fixed (byte* namePtr = extNameBytes)
        {
            extFuncPtr = _glfw.wgl.GetProcAddress((nint)namePtr);
            _glfw.wgl.GetExtensionsStringARB = (delegate* unmanaged[Stdcall]<nint, nint>)extFuncPtr;
        }

        extNameBytes = System.Text.Encoding.ASCII.GetBytes("wglCreateContextAttribsARB\0");
        fixed (byte* namePtr = extNameBytes)
        {
            extFuncPtr = _glfw.wgl.GetProcAddress((nint)namePtr);
            _glfw.wgl.CreateContextAttribsARB = (delegate* unmanaged[Stdcall]<nint, nint, int*, nint>)extFuncPtr;
        }

        extNameBytes = System.Text.Encoding.ASCII.GetBytes("wglSwapIntervalEXT\0");
        fixed (byte* namePtr = extNameBytes)
        {
            extFuncPtr = _glfw.wgl.GetProcAddress((nint)namePtr);
            _glfw.wgl.SwapIntervalEXT = (delegate* unmanaged[Stdcall]<int, int>)extFuncPtr;
        }

        extNameBytes = System.Text.Encoding.ASCII.GetBytes("wglGetPixelFormatAttribivARB\0");
        fixed (byte* namePtr = extNameBytes)
        {
            extFuncPtr = _glfw.wgl.GetProcAddress((nint)namePtr);
            _glfw.wgl.GetPixelFormatAttribivARB = (delegate* unmanaged[Stdcall]<nint, int, int, uint, int*, int*, int>)extFuncPtr;
        }

        // NOTE: WGL_ARB_extensions_string and WGL_EXT_extensions_string are not
        //       checked below as we are already using them
        _glfw.wgl.ARB_multisample =
            extensionSupportedWGL("WGL_ARB_multisample");
        _glfw.wgl.ARB_framebuffer_sRGB =
            extensionSupportedWGL("WGL_ARB_framebuffer_sRGB");
        _glfw.wgl.EXT_framebuffer_sRGB =
            extensionSupportedWGL("WGL_EXT_framebuffer_sRGB");
        _glfw.wgl.ARB_create_context =
            extensionSupportedWGL("WGL_ARB_create_context");
        _glfw.wgl.ARB_create_context_profile =
            extensionSupportedWGL("WGL_ARB_create_context_profile");
        _glfw.wgl.EXT_create_context_es2_profile =
            extensionSupportedWGL("WGL_EXT_create_context_es2_profile");
        _glfw.wgl.ARB_create_context_robustness =
            extensionSupportedWGL("WGL_ARB_create_context_robustness");
        _glfw.wgl.ARB_create_context_no_error =
            extensionSupportedWGL("WGL_ARB_create_context_no_error");
        _glfw.wgl.EXT_swap_control =
            extensionSupportedWGL("WGL_EXT_swap_control");
        _glfw.wgl.EXT_colorspace =
            extensionSupportedWGL("WGL_EXT_colorspace");
        _glfw.wgl.ARB_pixel_format =
            extensionSupportedWGL("WGL_ARB_pixel_format");
        _glfw.wgl.ARB_context_flush_control =
            extensionSupportedWGL("WGL_ARB_context_flush_control");

        _glfw.wgl.MakeCurrent(pdc, prc);
        _glfw.wgl.DeleteContext(rc);
        return true;
    }

    internal static void _glfwTerminateWGL()
    {
        if (_glfw.wgl != null && _glfw.wgl.instance != 0)
        {
            _glfwPlatformFreeModule(_glfw.wgl.instance);
            _glfw.wgl.instance = 0;
        }
        _glfw.wgl = null;
    }

    internal static bool _glfwCreateContextWGL(GlfwWindow window,
                                                GlfwCtxConfig ctxconfig,
                                                GlfwFbConfig fbconfig)
    {
        int pixelFormat;
        PIXELFORMATDESCRIPTOR pfd;
        nint share = 0;

        if (ctxconfig.Share != null)
            share = ctxconfig.Share.context.wgl!.handle;

        window.context.wgl ??= new GlfwContextWGL();

        window.context.wgl.dc = Win32Native.gdi32!.GetDC(window.Win32!.handle);
        if (window.context.wgl.dc == 0)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                            "WGL: Failed to retrieve DC for window");
            return false;
        }

        pixelFormat = choosePixelFormatWGL(window, ctxconfig, fbconfig);
        if (pixelFormat == 0)
            return false;

        if (Win32Native.gdi32!.DescribePixelFormat(window.context.wgl.dc,
                                 pixelFormat, (uint)sizeof(PIXELFORMATDESCRIPTOR), &pfd) == 0)
        {
            _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                 "WGL: Failed to retrieve PFD for selected pixel format");
            return false;
        }

        if (Win32Native.gdi32!.SetPixelFormat(window.context.wgl.dc, pixelFormat, &pfd) == 0)
        {
            _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                 "WGL: Failed to set selected pixel format");
            return false;
        }

        if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
        {
            if (ctxconfig.Forward)
            {
                if (!_glfw.wgl!.ARB_create_context)
                {
                    _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                    "WGL: A forward compatible OpenGL context requested but WGL_ARB_create_context is unavailable");
                    return false;
                }
            }

            if (ctxconfig.Profile != 0)
            {
                if (!_glfw.wgl!.ARB_create_context_profile)
                {
                    _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                    "WGL: OpenGL profile requested but WGL_ARB_create_context_profile is unavailable");
                    return false;
                }
            }
        }
        else
        {
            if (!_glfw.wgl!.ARB_create_context ||
                !_glfw.wgl.ARB_create_context_profile ||
                !_glfw.wgl.EXT_create_context_es2_profile)
            {
                _glfwInputError(GLFW.GLFW_API_UNAVAILABLE,
                                "WGL: OpenGL ES requested but WGL_ARB_create_context_es2_profile is unavailable");
                return false;
            }
        }

        if (_glfw.wgl!.ARB_create_context)
        {
            int* attribs = stackalloc int[40];
            int index = 0, mask = 0, flags = 0;

            if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
            {
                if (ctxconfig.Forward)
                    flags |= WGL.WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB;

                if (ctxconfig.Profile == GLFW.GLFW_OPENGL_CORE_PROFILE)
                    mask |= WGL.WGL_CONTEXT_CORE_PROFILE_BIT_ARB;
                else if (ctxconfig.Profile == GLFW.GLFW_OPENGL_COMPAT_PROFILE)
                    mask |= WGL.WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB;
            }
            else
                mask |= WGL.WGL_CONTEXT_ES2_PROFILE_BIT_EXT;

            if (ctxconfig.Debug)
                flags |= WGL.WGL_CONTEXT_DEBUG_BIT_ARB;

            if (ctxconfig.Robustness != 0)
            {
                if (_glfw.wgl.ARB_create_context_robustness)
                {
                    if (ctxconfig.Robustness == GLFW.GLFW_NO_RESET_NOTIFICATION)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = WGL.WGL_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB;
                        attribs[index++] = WGL.WGL_NO_RESET_NOTIFICATION_ARB;
                    }
                    else if (ctxconfig.Robustness == GLFW.GLFW_LOSE_CONTEXT_ON_RESET)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = WGL.WGL_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB;
                        attribs[index++] = WGL.WGL_LOSE_CONTEXT_ON_RESET_ARB;
                    }

                    flags |= WGL.WGL_CONTEXT_ROBUST_ACCESS_BIT_ARB;
                }
            }

            if (ctxconfig.Release != 0)
            {
                if (_glfw.wgl.ARB_context_flush_control)
                {
                    if (ctxconfig.Release == GLFW.GLFW_RELEASE_BEHAVIOR_NONE)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = WGL.WGL_CONTEXT_RELEASE_BEHAVIOR_ARB;
                        attribs[index++] = WGL.WGL_CONTEXT_RELEASE_BEHAVIOR_NONE_ARB;
                    }
                    else if (ctxconfig.Release == GLFW.GLFW_RELEASE_BEHAVIOR_FLUSH)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = WGL.WGL_CONTEXT_RELEASE_BEHAVIOR_ARB;
                        attribs[index++] = WGL.WGL_CONTEXT_RELEASE_BEHAVIOR_FLUSH_ARB;
                    }
                }
            }

            if (ctxconfig.Noerror)
            {
                if (_glfw.wgl.ARB_create_context_no_error)
                {
                    Debug.Assert(index + 1 < 40);
                    attribs[index++] = WGL.WGL_CONTEXT_OPENGL_NO_ERROR_ARB;
                    attribs[index++] = 1; // GLFW_TRUE
                }
            }

            // NOTE: Only request an explicitly versioned context when necessary, as
            //       explicitly requesting version 1.0 does not always return the
            //       highest version supported by the driver
            if (ctxconfig.Major != 1 || ctxconfig.Minor != 0)
            {
                Debug.Assert(index + 1 < 40);
                attribs[index++] = WGL.WGL_CONTEXT_MAJOR_VERSION_ARB;
                attribs[index++] = ctxconfig.Major;

                Debug.Assert(index + 1 < 40);
                attribs[index++] = WGL.WGL_CONTEXT_MINOR_VERSION_ARB;
                attribs[index++] = ctxconfig.Minor;
            }

            if (flags != 0)
            {
                Debug.Assert(index + 1 < 40);
                attribs[index++] = WGL.WGL_CONTEXT_FLAGS_ARB;
                attribs[index++] = flags;
            }

            if (mask != 0)
            {
                Debug.Assert(index + 1 < 40);
                attribs[index++] = WGL.WGL_CONTEXT_PROFILE_MASK_ARB;
                attribs[index++] = mask;
            }

            Debug.Assert(index + 1 < 40);
            attribs[index++] = 0;
            attribs[index++] = 0;

            window.context.wgl.handle =
                _glfw.wgl.CreateContextAttribsARB(window.context.wgl.dc, share, attribs);
            if (window.context.wgl.handle == 0)
            {
                uint error = Win32Native.kernel32!.GetLastError();

                if (error == (0xc0070000 | WGL.ERROR_INVALID_VERSION_ARB))
                {
                    if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
                    {
                        _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                        "WGL: Driver does not support OpenGL version {0}.{1}",
                                        ctxconfig.Major,
                                        ctxconfig.Minor);
                    }
                    else
                    {
                        _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                        "WGL: Driver does not support OpenGL ES version {0}.{1}",
                                        ctxconfig.Major,
                                        ctxconfig.Minor);
                    }
                }
                else if (error == (0xc0070000 | WGL.ERROR_INVALID_PROFILE_ARB))
                {
                    _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                    "WGL: Driver does not support the requested OpenGL profile");
                }
                else if (error == (0xc0070000 | WGL.ERROR_INCOMPATIBLE_DEVICE_CONTEXTS_ARB))
                {
                    _glfwInputError(GLFW.GLFW_INVALID_VALUE,
                                    "WGL: The share context is not compatible with the requested context");
                }
                else
                {
                    if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
                    {
                        _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                        "WGL: Failed to create OpenGL context");
                    }
                    else
                    {
                        _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                        "WGL: Failed to create OpenGL ES context");
                    }
                }

                return false;
            }
        }
        else
        {
            window.context.wgl.handle = _glfw.wgl.CreateContext(window.context.wgl.dc);
            if (window.context.wgl.handle == 0)
            {
                _glfwInputErrorWin32(GLFW.GLFW_VERSION_UNAVAILABLE,
                                     "WGL: Failed to create OpenGL context");
                return false;
            }

            if (share != 0)
            {
                if (_glfw.wgl.ShareLists(share, window.context.wgl.handle) == 0)
                {
                    _glfwInputErrorWin32(GLFW.GLFW_PLATFORM_ERROR,
                                         "WGL: Failed to enable sharing with specified OpenGL context");
                    return false;
                }
            }
        }

        window.context.makeCurrent = w => makeContextCurrentWGL(w);
        window.context.swapBuffers = w => swapBuffersWGL(w);
        window.context.swapInterval = i => swapIntervalWGL(i);
        window.context.extensionSupported = ext => extensionSupportedWGL(ext);
        window.context.getProcAddress = name => getProcAddressWGL(name);
        window.context.destroy = w => destroyContextWGL(w);

        return true;
    }

    internal static void _glfwDestroyContextWGL(GlfwWindow window)
    {
        destroyContextWGL(window);
    }


    //////////////////////////////////////////////////////////////////////////
    //////                        GLFW native API                       //////
    //////////////////////////////////////////////////////////////////////////

    public static nint glfwGetWGLContext(GlfwWindow? handle)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW.GLFW_PLATFORM_WIN32)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE,
                            "WGL: Platform not initialized");
            return 0;
        }

        GlfwWindow? window = handle;
        Debug.Assert(window != null);

        if (window!.context.Source != GLFW.GLFW_NATIVE_CONTEXT_API)
        {
            _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT, null);
            return 0;
        }

        return window.context.wgl!.handle;
    }
}

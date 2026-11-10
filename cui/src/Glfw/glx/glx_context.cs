// GLFW 3.6 GLX - www.glfw.org
// Ported from glfw/src/glx_context.c
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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Glfw;

public static unsafe partial class Glfw
{
    // Returns the specified attribute of the specified GLXFBConfig
    //
    private static int getGLXFBConfigAttrib(nint fbconfig, int attrib)
    {
        int value;
        _glfw.glx!.GetFBConfigAttrib(_glfw.X11!.display, fbconfig, attrib, &value);
        return value;
    }

    // Return the GLXFBConfig most closely matching the specified hints
    //
    private static bool chooseGLXFBConfig(GlfwFbConfig desired, out nint result)
    {
        result = 0;
        int nativeCount;
        bool trustWindowBit = true;

        // HACK: This is a (hopefully temporary) workaround for Chromium
        //       (VirtualBox GL) not setting the window bit on any GLXFBConfigs
        nint vendorPtr = _glfw.glx!.GetClientString(_glfw.X11!.display, GLX.GLX_VENDOR);
        string? vendor = vendorPtr != 0 ? Marshal.PtrToStringAnsi(vendorPtr) : null;
        if (vendor != null && vendor == "Chromium")
            trustWindowBit = false;

        nint nativeConfigsPtr =
            _glfw.glx.GetFBConfigs(_glfw.X11.display, _glfw.X11.screen, &nativeCount);
        if (nativeConfigsPtr == 0 || nativeCount == 0)
        {
            _glfwInputError(GLFW.GLFW_API_UNAVAILABLE, "GLX: No GLXFBConfigs returned");
            return false;
        }

        // nativeConfigsPtr is a GLXFBConfig* (array of pointers)
        nint* nativeConfigs = (nint*)nativeConfigsPtr;

        var usableConfigs = new GlfwFbConfig[nativeCount];
        for (int i = 0; i < nativeCount; i++)
            usableConfigs[i] = new GlfwFbConfig();
        int usableCount = 0;

        for (int i = 0; i < nativeCount; i++)
        {
            nint n = nativeConfigs[i];
            var u = usableConfigs[usableCount];

            // Only consider RGBA GLXFBConfigs
            if ((getGLXFBConfigAttrib(n, GLX.GLX_RENDER_TYPE) & GLX.GLX_RGBA_BIT) == 0)
                continue;

            // Only consider window GLXFBConfigs
            if ((getGLXFBConfigAttrib(n, GLX.GLX_DRAWABLE_TYPE) & GLX.GLX_WINDOW_BIT) == 0)
            {
                if (trustWindowBit)
                    continue;
            }

            if (getGLXFBConfigAttrib(n, GLX.GLX_DOUBLEBUFFER) != (desired.Doublebuffer ? 1 : 0))
                continue;

            if (desired.Transparent)
            {
                nint vi = _glfw.glx.GetVisualFromFBConfig(_glfw.X11.display, n);
                if (vi != 0)
                {
                    u.Transparent = _glfwIsVisualTransparentX11(vi);
                    _glfw.X11.xlib.Free(vi);
                }
            }

            u.RedBits = getGLXFBConfigAttrib(n, GLX.GLX_RED_SIZE);
            u.GreenBits = getGLXFBConfigAttrib(n, GLX.GLX_GREEN_SIZE);
            u.BlueBits = getGLXFBConfigAttrib(n, GLX.GLX_BLUE_SIZE);

            u.AlphaBits = getGLXFBConfigAttrib(n, GLX.GLX_ALPHA_SIZE);
            u.DepthBits = getGLXFBConfigAttrib(n, GLX.GLX_DEPTH_SIZE);
            u.StencilBits = getGLXFBConfigAttrib(n, GLX.GLX_STENCIL_SIZE);

            u.AccumRedBits = getGLXFBConfigAttrib(n, GLX.GLX_ACCUM_RED_SIZE);
            u.AccumGreenBits = getGLXFBConfigAttrib(n, GLX.GLX_ACCUM_GREEN_SIZE);
            u.AccumBlueBits = getGLXFBConfigAttrib(n, GLX.GLX_ACCUM_BLUE_SIZE);
            u.AccumAlphaBits = getGLXFBConfigAttrib(n, GLX.GLX_ACCUM_ALPHA_SIZE);

            u.AuxBuffers = getGLXFBConfigAttrib(n, GLX.GLX_AUX_BUFFERS);
            u.Stereo = getGLXFBConfigAttrib(n, GLX.GLX_STEREO) != 0;

            if (_glfw.glx.ARB_multisample)
                u.Samples = getGLXFBConfigAttrib(n, GLX.GLX_SAMPLES);

            if (_glfw.glx.ARB_framebuffer_sRGB || _glfw.glx.EXT_framebuffer_sRGB)
                u.SRGB = getGLXFBConfigAttrib(n, GLX.GLX_FRAMEBUFFER_SRGB_CAPABLE_ARB) != 0;

            u.Handle = (nuint)n;
            usableCount++;
        }

        int closestIndex = _glfwChooseFBConfig(desired, usableConfigs, usableCount);
        if (closestIndex >= 0)
            result = (nint)usableConfigs[closestIndex].Handle;

        _glfw.X11.xlib.Free(nativeConfigsPtr);

        return closestIndex >= 0;
    }

    // Create the OpenGL context using legacy API
    //
    private static nint createLegacyContextGLX(GlfwWindow window,
                                                nint fbconfig,
                                                nint share)
    {
        return _glfw.glx!.CreateNewContext(_glfw.X11!.display,
                                           fbconfig,
                                           GLX.GLX_RGBA_TYPE,
                                           share,
                                           1); // True
    }

    private static void makeContextCurrentGLX(GlfwWindow? window)
    {
        if (window != null)
        {
            if (_glfw.glx!.MakeCurrent(_glfw.X11!.display,
                                        window.context.glx!.window,
                                        window.context.glx.handle) == 0)
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                                "GLX: Failed to make context current");
                return;
            }
        }
        else
        {
            if (_glfw.glx!.MakeCurrent(_glfw.X11!.display, 0, 0) == 0)
            {
                _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                                "GLX: Failed to clear current context");
                return;
            }
        }

        _glfw.contextSlot = window;
    }

    private static void swapBuffersGLX(GlfwWindow window)
    {
        _glfw.glx!.SwapBuffers(_glfw.X11!.display, window.context.glx!.window);
    }

    private static void swapIntervalGLX(int interval)
    {
        GlfwWindow? window = _glfw.contextSlot;
        Debug.Assert(window != null);

        if (_glfw.glx!.EXT_swap_control)
        {
            _glfw.glx.SwapIntervalEXT(_glfw.X11!.display,
                                      window!.context.glx!.window,
                                      interval);
        }
        else if (_glfw.glx.MESA_swap_control)
            _glfw.glx.SwapIntervalMESA(interval);
        else if (_glfw.glx.SGI_swap_control)
        {
            if (interval > 0)
                _glfw.glx.SwapIntervalSGI(interval);
        }
    }

    private static bool extensionSupportedGLX(string extension)
    {
        nint extensionsPtr =
            _glfw.glx!.QueryExtensionsString(_glfw.X11!.display, _glfw.X11.screen);
        if (extensionsPtr != 0)
        {
            string? extensions = Marshal.PtrToStringAnsi(extensionsPtr);
            if (extensions != null)
            {
                if (_glfwStringInExtensionString(extension, extensions))
                    return true;
            }
        }

        return false;
    }

    private static nint getProcAddressGLX(string procname)
    {
        nint result;
        byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(procname + '\0');
        fixed (byte* namePtr = nameBytes)
        {
            if (_glfw.glx!.GetProcAddress != null)
            {
                result = _glfw.glx.GetProcAddress(namePtr);
                if (result != 0)
                    return result;
            }

            if (_glfw.glx.GetProcAddressARB != null)
            {
                result = _glfw.glx.GetProcAddressARB(namePtr);
                if (result != 0)
                    return result;
            }
        }

        // NOTE: glvnd provides GLX 1.4, so this can only happen with libGL
        return _glfwPlatformGetModuleSymbol(_glfw.glx.handle, procname);
    }

    private static void destroyContextGLX(GlfwWindow window)
    {
        if (window.context.glx!.window != 0)
        {
            _glfw.glx!.DestroyWindow(_glfw.X11!.display, window.context.glx.window);
            window.context.glx.window = 0;
        }

        if (window.context.glx.handle != 0)
        {
            _glfw.glx!.DestroyContext(_glfw.X11!.display, window.context.glx.handle);
            window.context.glx.handle = 0;
        }
    }


    //////////////////////////////////////////////////////////////////////////
    //////                       GLFW internal API                      //////
    //////////////////////////////////////////////////////////////////////////

    internal static bool _glfwInitGLX()
    {
        string[] sonames =
        {
            "libGLX.so.0",
            "libGL.so.1",
            "libGL.so",
        };

        if (_glfw.glx == null)
            _glfw.glx = new GlfwLibraryGLX();

        if (_glfw.glx.handle != 0)
            return true;

        for (int i = 0; i < sonames.Length; i++)
        {
            _glfw.glx.handle = _glfwPlatformLoadModule(sonames[i]);
            if (_glfw.glx.handle != 0)
                break;
        }

        if (_glfw.glx.handle == 0)
        {
            _glfwInputError(GLFW.GLFW_API_UNAVAILABLE, "GLX: Failed to load GLX");
            return false;
        }

        nint p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXGetFBConfigs");
        _glfw.glx.GetFBConfigs = (delegate* unmanaged[Cdecl]<nint, int, int*, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXGetFBConfigAttrib");
        _glfw.glx.GetFBConfigAttrib = (delegate* unmanaged[Cdecl]<nint, nint, int, int*, int>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXGetClientString");
        _glfw.glx.GetClientString = (delegate* unmanaged[Cdecl]<nint, int, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXQueryExtension");
        _glfw.glx.QueryExtension = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXQueryVersion");
        _glfw.glx.QueryVersion = (delegate* unmanaged[Cdecl]<nint, int*, int*, int>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXDestroyContext");
        _glfw.glx.DestroyContext = (delegate* unmanaged[Cdecl]<nint, nint, void>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXMakeCurrent");
        _glfw.glx.MakeCurrent = (delegate* unmanaged[Cdecl]<nint, nuint, nint, int>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXSwapBuffers");
        _glfw.glx.SwapBuffers = (delegate* unmanaged[Cdecl]<nint, nuint, void>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXQueryExtensionsString");
        _glfw.glx.QueryExtensionsString = (delegate* unmanaged[Cdecl]<nint, int, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXCreateNewContext");
        _glfw.glx.CreateNewContext = (delegate* unmanaged[Cdecl]<nint, nint, int, nint, int, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXCreateWindow");
        _glfw.glx.CreateWindow = (delegate* unmanaged[Cdecl]<nint, nint, nuint, int*, nuint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXDestroyWindow");
        _glfw.glx.DestroyWindow = (delegate* unmanaged[Cdecl]<nint, nuint, void>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXGetVisualFromFBConfig");
        _glfw.glx.GetVisualFromFBConfig = (delegate* unmanaged[Cdecl]<nint, nint, nint>)p;

        if (_glfw.glx.GetFBConfigs == null ||
            _glfw.glx.GetFBConfigAttrib == null ||
            _glfw.glx.GetClientString == null ||
            _glfw.glx.QueryExtension == null ||
            _glfw.glx.QueryVersion == null ||
            _glfw.glx.DestroyContext == null ||
            _glfw.glx.MakeCurrent == null ||
            _glfw.glx.SwapBuffers == null ||
            _glfw.glx.QueryExtensionsString == null ||
            _glfw.glx.CreateNewContext == null ||
            _glfw.glx.CreateWindow == null ||
            _glfw.glx.DestroyWindow == null ||
            _glfw.glx.GetVisualFromFBConfig == null)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                            "GLX: Failed to load required entry points");
            return false;
        }

        // NOTE: Unlike GLX 1.3 entry points these are not required to be present
        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXGetProcAddress");
        _glfw.glx.GetProcAddress = (delegate* unmanaged[Cdecl]<byte*, nint>)p;

        p = _glfwPlatformGetModuleSymbol(_glfw.glx.handle, "glXGetProcAddressARB");
        _glfw.glx.GetProcAddressARB = (delegate* unmanaged[Cdecl]<byte*, nint>)p;

        int errorBase, eventBase;
        if (_glfw.glx.QueryExtension(_glfw.X11!.display,
                                     &errorBase,
                                     &eventBase) == 0)
        {
            _glfwInputError(GLFW.GLFW_API_UNAVAILABLE, "GLX: GLX extension not found");
            return false;
        }
        _glfw.glx.errorBase = errorBase;
        _glfw.glx.eventBase = eventBase;

        int major, minor;
        if (_glfw.glx.QueryVersion(_glfw.X11.display, &major, &minor) == 0)
        {
            _glfwInputError(GLFW.GLFW_API_UNAVAILABLE,
                            "GLX: Failed to query GLX version");
            return false;
        }
        _glfw.glx.major = major;
        _glfw.glx.minor = minor;

        if (_glfw.glx.major == 1 && _glfw.glx.minor < 3)
        {
            _glfwInputError(GLFW.GLFW_API_UNAVAILABLE,
                            "GLX: GLX version 1.3 is required");
            return false;
        }

        if (extensionSupportedGLX("GLX_EXT_swap_control"))
        {
            _glfw.glx.SwapIntervalEXT = (delegate* unmanaged[Cdecl]<nint, nuint, int, void>)
                getProcAddressGLX("glXSwapIntervalEXT");

            if (_glfw.glx.SwapIntervalEXT != null)
                _glfw.glx.EXT_swap_control = true;
        }

        if (extensionSupportedGLX("GLX_SGI_swap_control"))
        {
            _glfw.glx.SwapIntervalSGI = (delegate* unmanaged[Cdecl]<int, int>)
                getProcAddressGLX("glXSwapIntervalSGI");

            if (_glfw.glx.SwapIntervalSGI != null)
                _glfw.glx.SGI_swap_control = true;
        }

        if (extensionSupportedGLX("GLX_MESA_swap_control"))
        {
            _glfw.glx.SwapIntervalMESA = (delegate* unmanaged[Cdecl]<int, int>)
                getProcAddressGLX("glXSwapIntervalMESA");

            if (_glfw.glx.SwapIntervalMESA != null)
                _glfw.glx.MESA_swap_control = true;
        }

        if (extensionSupportedGLX("GLX_ARB_multisample"))
            _glfw.glx.ARB_multisample = true;

        if (extensionSupportedGLX("GLX_ARB_framebuffer_sRGB"))
            _glfw.glx.ARB_framebuffer_sRGB = true;

        if (extensionSupportedGLX("GLX_EXT_framebuffer_sRGB"))
            _glfw.glx.EXT_framebuffer_sRGB = true;

        if (extensionSupportedGLX("GLX_ARB_create_context"))
        {
            _glfw.glx.CreateContextAttribsARB = (delegate* unmanaged[Cdecl]<nint, nint, nint, int, int*, nint>)
                getProcAddressGLX("glXCreateContextAttribsARB");

            if (_glfw.glx.CreateContextAttribsARB != null)
                _glfw.glx.ARB_create_context = true;
        }

        if (extensionSupportedGLX("GLX_ARB_create_context_robustness"))
            _glfw.glx.ARB_create_context_robustness = true;

        if (extensionSupportedGLX("GLX_ARB_create_context_profile"))
            _glfw.glx.ARB_create_context_profile = true;

        if (extensionSupportedGLX("GLX_EXT_create_context_es2_profile"))
            _glfw.glx.EXT_create_context_es2_profile = true;

        if (extensionSupportedGLX("GLX_ARB_create_context_no_error"))
            _glfw.glx.ARB_create_context_no_error = true;

        if (extensionSupportedGLX("GLX_ARB_context_flush_control"))
            _glfw.glx.ARB_context_flush_control = true;

        return true;
    }

    internal static void _glfwTerminateGLX()
    {
        // NOTE: This function must not call any X11 functions, as it is called
        //       after XCloseDisplay (see _glfwTerminateX11 for details)

        if (_glfw.glx != null && _glfw.glx.handle != 0)
        {
            _glfwPlatformFreeModule(_glfw.glx.handle);
            _glfw.glx.handle = 0;
        }
        // Native termination clears the full library state. Extension flags and
        // procedure addresses belong to this load, not to a later init cycle.
        _glfw.glx = null;
    }

    internal static bool _glfwCreateContextGLX(GlfwWindow window,
                                                GlfwCtxConfig ctxconfig,
                                                GlfwFbConfig fbconfig)
    {
        nint native = 0;
        nint share = 0;

        if (ctxconfig.Share != null)
            share = ctxconfig.Share.context.glx!.handle;

        if (!chooseGLXFBConfig(fbconfig, out native))
        {
            _glfwInputError(GLFW.GLFW_FORMAT_UNAVAILABLE,
                            "GLX: Failed to find a suitable GLXFBConfig");
            return false;
        }

        if (ctxconfig.Client == GLFW.GLFW_OPENGL_ES_API)
        {
            if (!_glfw.glx!.ARB_create_context ||
                !_glfw.glx.ARB_create_context_profile ||
                !_glfw.glx.EXT_create_context_es2_profile)
            {
                _glfwInputError(GLFW.GLFW_API_UNAVAILABLE,
                                "GLX: OpenGL ES requested but GLX_EXT_create_context_es2_profile is unavailable");
                return false;
            }
        }

        if (ctxconfig.Forward)
        {
            if (!_glfw.glx!.ARB_create_context)
            {
                _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                "GLX: Forward compatibility requested but GLX_ARB_create_context_profile is unavailable");
                return false;
            }
        }

        if (ctxconfig.Profile != 0)
        {
            if (!_glfw.glx!.ARB_create_context ||
                !_glfw.glx.ARB_create_context_profile)
            {
                _glfwInputError(GLFW.GLFW_VERSION_UNAVAILABLE,
                                "GLX: An OpenGL profile requested but GLX_ARB_create_context_profile is unavailable");
                return false;
            }
        }

        _glfwGrabErrorHandlerX11();

        window.context.glx ??= new GlfwContextGLX();

        if (_glfw.glx!.ARB_create_context)
        {
            int* attribs = stackalloc int[40];
            int index = 0, mask = 0, flags = 0;

            if (ctxconfig.Client == GLFW.GLFW_OPENGL_API)
            {
                if (ctxconfig.Forward)
                    flags |= GLX.GLX_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB;

                if (ctxconfig.Profile == GLFW.GLFW_OPENGL_CORE_PROFILE)
                    mask |= GLX.GLX_CONTEXT_CORE_PROFILE_BIT_ARB;
                else if (ctxconfig.Profile == GLFW.GLFW_OPENGL_COMPAT_PROFILE)
                    mask |= GLX.GLX_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB;
            }
            else
                mask |= GLX.GLX_CONTEXT_ES2_PROFILE_BIT_EXT;

            if (ctxconfig.Debug)
                flags |= GLX.GLX_CONTEXT_DEBUG_BIT_ARB;

            if (ctxconfig.Robustness != 0)
            {
                if (_glfw.glx.ARB_create_context_robustness)
                {
                    if (ctxconfig.Robustness == GLFW.GLFW_NO_RESET_NOTIFICATION)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = GLX.GLX_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB;
                        attribs[index++] = GLX.GLX_NO_RESET_NOTIFICATION_ARB;
                    }
                    else if (ctxconfig.Robustness == GLFW.GLFW_LOSE_CONTEXT_ON_RESET)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = GLX.GLX_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB;
                        attribs[index++] = GLX.GLX_LOSE_CONTEXT_ON_RESET_ARB;
                    }

                    flags |= GLX.GLX_CONTEXT_ROBUST_ACCESS_BIT_ARB;
                }
            }

            if (ctxconfig.Release != 0)
            {
                if (_glfw.glx.ARB_context_flush_control)
                {
                    if (ctxconfig.Release == GLFW.GLFW_RELEASE_BEHAVIOR_NONE)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = GLX.GLX_CONTEXT_RELEASE_BEHAVIOR_ARB;
                        attribs[index++] = GLX.GLX_CONTEXT_RELEASE_BEHAVIOR_NONE_ARB;
                    }
                    else if (ctxconfig.Release == GLFW.GLFW_RELEASE_BEHAVIOR_FLUSH)
                    {
                        Debug.Assert(index + 1 < 40);
                        attribs[index++] = GLX.GLX_CONTEXT_RELEASE_BEHAVIOR_ARB;
                        attribs[index++] = GLX.GLX_CONTEXT_RELEASE_BEHAVIOR_FLUSH_ARB;
                    }
                }
            }

            if (ctxconfig.Noerror)
            {
                if (_glfw.glx.ARB_create_context_no_error)
                {
                    Debug.Assert(index + 1 < 40);
                    attribs[index++] = GLX.GLX_CONTEXT_OPENGL_NO_ERROR_ARB;
                    attribs[index++] = 1; // GLFW_TRUE
                }
            }

            // NOTE: Only request an explicitly versioned context when necessary, as
            //       explicitly requesting version 1.0 does not always return the
            //       highest version supported by the driver
            if (ctxconfig.Major != 1 || ctxconfig.Minor != 0)
            {
                Debug.Assert(index + 1 < 40);
                attribs[index++] = GLX.GLX_CONTEXT_MAJOR_VERSION_ARB;
                attribs[index++] = ctxconfig.Major;

                Debug.Assert(index + 1 < 40);
                attribs[index++] = GLX.GLX_CONTEXT_MINOR_VERSION_ARB;
                attribs[index++] = ctxconfig.Minor;
            }

            if (mask != 0)
            {
                Debug.Assert(index + 1 < 40);
                attribs[index++] = GLX.GLX_CONTEXT_PROFILE_MASK_ARB;
                attribs[index++] = mask;
            }

            if (flags != 0)
            {
                Debug.Assert(index + 1 < 40);
                attribs[index++] = GLX.GLX_CONTEXT_FLAGS_ARB;
                attribs[index++] = flags;
            }

            Debug.Assert(index + 1 < 40);
            attribs[index++] = 0; // None
            attribs[index++] = 0; // None

            window.context.glx.handle =
                _glfw.glx.CreateContextAttribsARB(_glfw.X11!.display,
                                                  native,
                                                  share,
                                                  1, // True
                                                  attribs);

            // HACK: This is a fallback for broken versions of the Mesa
            //       implementation of GLX_ARB_create_context_profile that fail
            //       default 1.0 context creation with a GLXBadProfileARB error in
            //       violation of the extension spec
            if (window.context.glx.handle == 0)
            {
                if (_glfw.X11.errorCode == _glfw.glx.errorBase + GLX.GLXBadProfileARB &&
                    ctxconfig.Client == GLFW.GLFW_OPENGL_API &&
                    ctxconfig.Profile == GLFW.GLFW_OPENGL_ANY_PROFILE &&
                    !ctxconfig.Forward)
                {
                    window.context.glx.handle =
                        createLegacyContextGLX(window, native, share);
                }
            }
        }
        else
        {
            window.context.glx.handle =
                createLegacyContextGLX(window, native, share);
        }

        _glfwReleaseErrorHandlerX11();

        if (window.context.glx.handle == 0)
        {
            _glfwInputErrorX11(GLFW.GLFW_VERSION_UNAVAILABLE, "GLX: Failed to create context");
            return false;
        }

        window.context.destroy = destroyContextGLX;
        window.context.glx.window =
            _glfw.glx.CreateWindow(_glfw.X11!.display, native, window.X11!.handle, null);
        if (window.context.glx.window == 0)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_ERROR, "GLX: Failed to create window");
            return false;
        }

        window.context.glx.fbconfig = native;

        window.context.makeCurrent = w => makeContextCurrentGLX(w);
        window.context.swapBuffers = w => swapBuffersGLX(w);
        window.context.swapInterval = i => swapIntervalGLX(i);
        window.context.extensionSupported = ext => extensionSupportedGLX(ext);
        window.context.getProcAddress = name => getProcAddressGLX(name);
        window.context.destroy = w => destroyContextGLX(w);

        return true;
    }

    internal static void _glfwDestroyContextGLX(GlfwWindow window)
    {
        destroyContextGLX(window);
    }

    // Returns the Visual and depth of the chosen GLXFBConfig
    //
    internal static bool _glfwChooseVisualGLX(GlfwWndConfig wndconfig,
                                               GlfwCtxConfig ctxconfig,
                                               GlfwFbConfig fbconfig,
                                               out nint visual, out int depth)
    {
        visual = 0;
        depth = 0;

        nint native;
        if (!chooseGLXFBConfig(fbconfig, out native))
        {
            _glfwInputError(GLFW.GLFW_FORMAT_UNAVAILABLE,
                            "GLX: Failed to find a suitable GLXFBConfig");
            return false;
        }

        nint result = _glfw.glx!.GetVisualFromFBConfig(_glfw.X11!.display, native);
        if (result == 0)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_ERROR,
                            "GLX: Failed to retrieve Visual for GLXFBConfig");
            return false;
        }

        // XVisualInfo struct layout (64-bit Linux):
        //   Visual*    visual;     offset 0, size 8
        //   VisualID   visualid;   offset 8, size 8
        //   int        screen;     offset 16, size 4
        //   int        depth;      offset 20, size 4
        visual = Marshal.ReadIntPtr(result, 0);       // visual field
        depth = Marshal.ReadInt32(result, nint.Size + nint.Size + sizeof(int)); // depth field

        _glfw.X11.xlib.Free(result);
        return true;
    }


    //////////////////////////////////////////////////////////////////////////
    //////                        GLFW native API                       //////
    //////////////////////////////////////////////////////////////////////////

    public static nint glfwGetGLXContext(GlfwWindow? handle)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW.GLFW_PLATFORM_X11)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE, "GLX: Platform not initialized");
            return 0;
        }

        GlfwWindow? window = handle;
        Debug.Assert(window != null);

        if (window!.context.Source != GLFW.GLFW_NATIVE_CONTEXT_API)
        {
            _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT, null);
            return 0;
        }

        return window.context.glx!.handle;
    }

    public static nuint glfwGetGLXWindow(GlfwWindow? handle)
    {
        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return 0;
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW.GLFW_PLATFORM_X11)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE, "GLX: Platform not initialized");
            return 0;
        }

        GlfwWindow? window = handle;
        Debug.Assert(window != null);

        if (window!.context.Source != GLFW.GLFW_NATIVE_CONTEXT_API)
        {
            _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT, null);
            return 0;
        }

        return window.context.glx!.window;
    }

    public static bool glfwGetGLXFBConfig(GlfwWindow? handle, out nint config)
    {
        config = 0;

        if (!_glfw.initialized)
        {
            _glfwInputError(GLFW.GLFW_NOT_INITIALIZED, null);
            return false;
        }

        if (_glfw.platform == null || _glfw.platform.PlatformID != GLFW.GLFW_PLATFORM_X11)
        {
            _glfwInputError(GLFW.GLFW_PLATFORM_UNAVAILABLE, "GLX: Platform not initialized");
            return false;
        }

        GlfwWindow? window = handle;
        Debug.Assert(window != null);

        if (window!.context.Source != GLFW.GLFW_NATIVE_CONTEXT_API)
        {
            _glfwInputError(GLFW.GLFW_NO_WINDOW_CONTEXT, null);
            return false;
        }

        config = window.context.glx!.fbconfig;
        return true;
    }


    //////////////////////////////////////////////////////////////////////////
    //////              X11 helpers for GLX                        //////
    //////////////////////////////////////////////////////////////////////////
    // Delegate to the shared X11 error and visual implementations.

    // Delegates to real implementation in x11_init.cs
    internal static void _glfwGrabErrorHandlerX11()
    {
        if (_glfw.X11 != null)
            _glfwGrabErrorHandlerX11_Real();
    }

    // Delegates to real implementation in x11_init.cs
    internal static void _glfwReleaseErrorHandlerX11()
    {
        if (_glfw.X11 != null)
            _glfwReleaseErrorHandlerX11_Real();
    }

    // Delegates to real implementation in x11_init.cs
    internal static void _glfwInputErrorX11(int code, string description)
    {
        if (_glfw.X11 != null)
            _glfwInputErrorX11_Real(code, description);
        else
            _glfwInputError(code, description);
    }

    // Delegates to real implementation in x11_init.cs
    internal static bool _glfwIsVisualTransparentX11(nint visual)
    {
        if (_glfw.X11 != null)
            return _glfwIsVisualTransparentX11_Real(visual);
        return false;
    }
}

// -----------------------------------------------------------------------
// Extension to GlfwContext for GLX sub-object
// -----------------------------------------------------------------------

public partial class GlfwContext
{
    /// <summary>
    /// GLX-specific context state. Null until a GLX context is created.
    /// </summary>
    public GlfwContextGLX? glx;
}

// -----------------------------------------------------------------------
// Extension to _glfw for glx global state
// -----------------------------------------------------------------------

public static partial class _glfw
{
    /// <summary>
    /// GLX-specific global data. Null until <see cref="Glfw._glfwInitGLX"/> is called.
    /// </summary>
    public static GlfwLibraryGLX? glx;
}

// GLFW 3.6 GLX - www.glfw.org
// Ported from glfw/src/x11_platform.h (GLX types, constants, native function pointers)
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

namespace Glfw;

// -----------------------------------------------------------------------
// GLX constants (from x11_platform.h)
// -----------------------------------------------------------------------

internal static class GLX
{
    public const int GLX_VENDOR                                  = 1;
    public const int GLX_RGBA_BIT                                = 0x00000001;
    public const int GLX_WINDOW_BIT                              = 0x00000001;
    public const int GLX_DRAWABLE_TYPE                           = 0x8010;
    public const int GLX_RENDER_TYPE                             = 0x8011;
    public const int GLX_RGBA_TYPE                               = 0x8014;
    public const int GLX_DOUBLEBUFFER                            = 5;
    public const int GLX_STEREO                                  = 6;
    public const int GLX_AUX_BUFFERS                             = 7;
    public const int GLX_RED_SIZE                                = 8;
    public const int GLX_GREEN_SIZE                              = 9;
    public const int GLX_BLUE_SIZE                               = 10;
    public const int GLX_ALPHA_SIZE                              = 11;
    public const int GLX_DEPTH_SIZE                              = 12;
    public const int GLX_STENCIL_SIZE                            = 13;
    public const int GLX_ACCUM_RED_SIZE                          = 14;
    public const int GLX_ACCUM_GREEN_SIZE                        = 15;
    public const int GLX_ACCUM_BLUE_SIZE                         = 16;
    public const int GLX_ACCUM_ALPHA_SIZE                        = 17;
    public const int GLX_SAMPLES                                 = 0x186a1;
    public const int GLX_VISUAL_ID                               = 0x800b;

    public const int GLX_FRAMEBUFFER_SRGB_CAPABLE_ARB            = 0x20b2;
    public const int GLX_CONTEXT_DEBUG_BIT_ARB                   = 0x00000001;
    public const int GLX_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB   = 0x00000002;
    public const int GLX_CONTEXT_CORE_PROFILE_BIT_ARB            = 0x00000001;
    public const int GLX_CONTEXT_PROFILE_MASK_ARB                = 0x9126;
    public const int GLX_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB      = 0x00000002;
    public const int GLX_CONTEXT_MAJOR_VERSION_ARB               = 0x2091;
    public const int GLX_CONTEXT_MINOR_VERSION_ARB               = 0x2092;
    public const int GLX_CONTEXT_FLAGS_ARB                       = 0x2094;
    public const int GLX_CONTEXT_ES2_PROFILE_BIT_EXT             = 0x00000004;
    public const int GLX_CONTEXT_ROBUST_ACCESS_BIT_ARB           = 0x00000004;
    public const int GLX_LOSE_CONTEXT_ON_RESET_ARB               = 0x8252;
    public const int GLX_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB = 0x8256;
    public const int GLX_NO_RESET_NOTIFICATION_ARB               = 0x8261;
    public const int GLX_CONTEXT_RELEASE_BEHAVIOR_ARB            = 0x2097;
    public const int GLX_CONTEXT_RELEASE_BEHAVIOR_NONE_ARB       = 0;
    public const int GLX_CONTEXT_RELEASE_BEHAVIOR_FLUSH_ARB      = 0x2098;
    public const int GLX_CONTEXT_OPENGL_NO_ERROR_ARB             = 0x31b3;

    public const int GLXBadProfileARB                            = 13;
}

// -----------------------------------------------------------------------
// GlfwContextGLX  (was _GLFWcontextGLX)
// -----------------------------------------------------------------------

/// <summary>
/// GLX-specific per-context data. Stored inside <see cref="GlfwContext"/>.
/// </summary>
public class GlfwContextGLX
{
    public nint handle;        // GLXContext (opaque pointer)
    public nuint window;       // GLXWindow  (XID)
    public nint fbconfig;      // GLXFBConfig (opaque pointer)
}

// -----------------------------------------------------------------------
// GlfwLibraryGLX  (was _GLFWlibraryGLX)
// -----------------------------------------------------------------------

/// <summary>
/// GLX-specific global data. Stored in <see cref="_glfw"/>.
/// </summary>
public unsafe class GlfwLibraryGLX
{
    public int major, minor;
    public int eventBase;
    public int errorBase;

    public nint handle;    // dlopen handle for libGL/libGLX

    // GLX 1.3 functions
    // GLXFBConfig* glXGetFBConfigs(Display*, int screen, int* nelements)
    public delegate* unmanaged[Cdecl]<nint, int, int*, nint> GetFBConfigs;
    // int glXGetFBConfigAttrib(Display*, GLXFBConfig, int attribute, int* value)
    public delegate* unmanaged[Cdecl]<nint, nint, int, int*, int> GetFBConfigAttrib;
    // const char* glXGetClientString(Display*, int name)
    public delegate* unmanaged[Cdecl]<nint, int, nint> GetClientString;
    // Bool glXQueryExtension(Display*, int* errorBase, int* eventBase)
    public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryExtension;
    // Bool glXQueryVersion(Display*, int* major, int* minor)
    public delegate* unmanaged[Cdecl]<nint, int*, int*, int> QueryVersion;
    // void glXDestroyContext(Display*, GLXContext)
    public delegate* unmanaged[Cdecl]<nint, nint, void> DestroyContext;
    // Bool glXMakeCurrent(Display*, GLXDrawable, GLXContext)
    public delegate* unmanaged[Cdecl]<nint, nuint, nint, int> MakeCurrent;
    // void glXSwapBuffers(Display*, GLXDrawable)
    public delegate* unmanaged[Cdecl]<nint, nuint, void> SwapBuffers;
    // const char* glXQueryExtensionsString(Display*, int screen)
    public delegate* unmanaged[Cdecl]<nint, int, nint> QueryExtensionsString;
    // GLXContext glXCreateNewContext(Display*, GLXFBConfig, int renderType, GLXContext shareList, Bool direct)
    public delegate* unmanaged[Cdecl]<nint, nint, int, nint, int, nint> CreateNewContext;
    // XVisualInfo* glXGetVisualFromFBConfig(Display*, GLXFBConfig)
    public delegate* unmanaged[Cdecl]<nint, nint, nint> GetVisualFromFBConfig;
    // GLXWindow glXCreateWindow(Display*, GLXFBConfig, Window, const int* attribList)
    public delegate* unmanaged[Cdecl]<nint, nint, nuint, int*, nuint> CreateWindow;
    // void glXDestroyWindow(Display*, GLXWindow)
    public delegate* unmanaged[Cdecl]<nint, nuint, void> DestroyWindow;

    // GLX 1.4 and extension functions
    // __GLXextproc glXGetProcAddress(const GLubyte* procName)
    public delegate* unmanaged[Cdecl]<byte*, nint> GetProcAddress;
    // __GLXextproc glXGetProcAddressARB(const GLubyte* procName)
    public delegate* unmanaged[Cdecl]<byte*, nint> GetProcAddressARB;
    // int glXSwapIntervalSGI(int interval)
    public delegate* unmanaged[Cdecl]<int, int> SwapIntervalSGI;
    // void glXSwapIntervalEXT(Display*, GLXDrawable, int interval)
    public delegate* unmanaged[Cdecl]<nint, nuint, int, void> SwapIntervalEXT;
    // int glXSwapIntervalMESA(int interval)
    public delegate* unmanaged[Cdecl]<int, int> SwapIntervalMESA;
    // GLXContext glXCreateContextAttribsARB(Display*, GLXFBConfig, GLXContext shareContext, Bool direct, const int* attribList)
    public delegate* unmanaged[Cdecl]<nint, nint, nint, int, int*, nint> CreateContextAttribsARB;

    // Extension support booleans
    public bool SGI_swap_control;
    public bool EXT_swap_control;
    public bool MESA_swap_control;
    public bool ARB_multisample;
    public bool ARB_framebuffer_sRGB;
    public bool EXT_framebuffer_sRGB;
    public bool ARB_create_context;
    public bool ARB_create_context_profile;
    public bool ARB_create_context_robustness;
    public bool EXT_create_context_es2_profile;
    public bool ARB_create_context_no_error;
    public bool ARB_context_flush_control;
}

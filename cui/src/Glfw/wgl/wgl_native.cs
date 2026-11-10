// GLFW 3.6 WGL - www.glfw.org
// Ported from glfw/src/win32_platform.h (WGL types, constants, native function pointers)
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
// WGL constants (from win32_platform.h)
// -----------------------------------------------------------------------

internal static class WGL
{
    public const int WGL_NUMBER_PIXEL_FORMATS_ARB                = 0x2000;
    public const int WGL_SUPPORT_OPENGL_ARB                      = 0x2010;
    public const int WGL_DRAW_TO_WINDOW_ARB                      = 0x2001;
    public const int WGL_PIXEL_TYPE_ARB                          = 0x2013;
    public const int WGL_TYPE_RGBA_ARB                           = 0x202b;
    public const int WGL_ACCELERATION_ARB                        = 0x2003;
    public const int WGL_NO_ACCELERATION_ARB                     = 0x2025;
    public const int WGL_FULL_ACCELERATION_ARB                   = 0x2027;
    public const int WGL_RED_BITS_ARB                            = 0x2015;
    public const int WGL_RED_SHIFT_ARB                           = 0x2016;
    public const int WGL_GREEN_BITS_ARB                          = 0x2017;
    public const int WGL_GREEN_SHIFT_ARB                         = 0x2018;
    public const int WGL_BLUE_BITS_ARB                           = 0x2019;
    public const int WGL_BLUE_SHIFT_ARB                          = 0x201a;
    public const int WGL_ALPHA_BITS_ARB                          = 0x201b;
    public const int WGL_ALPHA_SHIFT_ARB                         = 0x201c;
    public const int WGL_ACCUM_BITS_ARB                          = 0x201d;
    public const int WGL_ACCUM_RED_BITS_ARB                      = 0x201e;
    public const int WGL_ACCUM_GREEN_BITS_ARB                    = 0x201f;
    public const int WGL_ACCUM_BLUE_BITS_ARB                     = 0x2020;
    public const int WGL_ACCUM_ALPHA_BITS_ARB                    = 0x2021;
    public const int WGL_DEPTH_BITS_ARB                          = 0x2022;
    public const int WGL_STENCIL_BITS_ARB                        = 0x2023;
    public const int WGL_AUX_BUFFERS_ARB                         = 0x2024;
    public const int WGL_STEREO_ARB                              = 0x2012;
    public const int WGL_DOUBLE_BUFFER_ARB                       = 0x2011;
    public const int WGL_SAMPLES_ARB                             = 0x2042;
    public const int WGL_FRAMEBUFFER_SRGB_CAPABLE_ARB            = 0x20a9;
    public const int WGL_CONTEXT_DEBUG_BIT_ARB                   = 0x00000001;
    public const int WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB      = 0x00000002;
    public const int WGL_CONTEXT_PROFILE_MASK_ARB                = 0x9126;
    public const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB            = 0x00000001;
    public const int WGL_CONTEXT_COMPATIBILITY_PROFILE_BIT_ARB   = 0x00000002;
    public const int WGL_CONTEXT_MAJOR_VERSION_ARB               = 0x2091;
    public const int WGL_CONTEXT_MINOR_VERSION_ARB               = 0x2092;
    public const int WGL_CONTEXT_FLAGS_ARB                       = 0x2094;
    public const int WGL_CONTEXT_ES2_PROFILE_BIT_EXT             = 0x00000004;
    public const int WGL_CONTEXT_ROBUST_ACCESS_BIT_ARB           = 0x00000004;
    public const int WGL_LOSE_CONTEXT_ON_RESET_ARB               = 0x8252;
    public const int WGL_CONTEXT_RESET_NOTIFICATION_STRATEGY_ARB = 0x8256;
    public const int WGL_NO_RESET_NOTIFICATION_ARB               = 0x8261;
    public const int WGL_CONTEXT_RELEASE_BEHAVIOR_ARB            = 0x2097;
    public const int WGL_CONTEXT_RELEASE_BEHAVIOR_NONE_ARB       = 0;
    public const int WGL_CONTEXT_RELEASE_BEHAVIOR_FLUSH_ARB      = 0x2098;
    public const int WGL_CONTEXT_OPENGL_NO_ERROR_ARB             = 0x31b3;
    public const int WGL_COLORSPACE_EXT                          = 0x309d;
    public const int WGL_COLORSPACE_SRGB_EXT                     = 0x3089;

    public const int ERROR_INVALID_VERSION_ARB                   = 0x2095;
    public const int ERROR_INVALID_PROFILE_ARB                   = 0x2096;
    public const int ERROR_INCOMPATIBLE_DEVICE_CONTEXTS_ARB      = 0x2054;
}

// -----------------------------------------------------------------------
// GlfwContextWGL  (was _GLFWcontextWGL)
// -----------------------------------------------------------------------

/// <summary>
/// WGL-specific per-context data. Stored inside <see cref="GlfwContext"/>.
/// </summary>
public class GlfwContextWGL
{
    public nint dc;       // HDC
    public nint handle;   // HGLRC
    public int interval;
}

// -----------------------------------------------------------------------
// GlfwLibraryWGL  (was _GLFWlibraryWGL)
// -----------------------------------------------------------------------

/// <summary>
/// WGL-specific global data. Stored in <see cref="_glfw"/>.
/// </summary>
public unsafe class GlfwLibraryWGL
{
    public nint instance;  // HINSTANCE for opengl32.dll

    // opengl32.dll function pointers (Stdcall calling convention on Win32)
    // HGLRC wglCreateContext(HDC)
    public delegate* unmanaged[Stdcall]<nint, nint> CreateContext;
    // BOOL wglDeleteContext(HGLRC)
    public delegate* unmanaged[Stdcall]<nint, int> DeleteContext;
    // PROC wglGetProcAddress(LPCSTR)
    public delegate* unmanaged[Stdcall]<nint, nint> GetProcAddress;
    // HDC wglGetCurrentDC(void)
    public delegate* unmanaged[Stdcall]<nint> GetCurrentDC;
    // HGLRC wglGetCurrentContext(void)
    public delegate* unmanaged[Stdcall]<nint> GetCurrentContext;
    // BOOL wglMakeCurrent(HDC, HGLRC)
    public delegate* unmanaged[Stdcall]<nint, nint, int> MakeCurrent;
    // BOOL wglShareLists(HGLRC, HGLRC)
    public delegate* unmanaged[Stdcall]<nint, nint, int> ShareLists;

    // WGL extension function pointers (also Stdcall -- WINAPI convention)
    // BOOL wglSwapIntervalEXT(int)
    public delegate* unmanaged[Stdcall]<int, int> SwapIntervalEXT;
    // BOOL wglGetPixelFormatAttribivARB(HDC, int, int, UINT, const int*, int*)
    public delegate* unmanaged[Stdcall]<nint, int, int, uint, int*, int*, int> GetPixelFormatAttribivARB;
    // const char* wglGetExtensionsStringEXT(void)
    public delegate* unmanaged[Stdcall]<nint> GetExtensionsStringEXT;
    // const char* wglGetExtensionsStringARB(HDC)
    public delegate* unmanaged[Stdcall]<nint, nint> GetExtensionsStringARB;
    // HGLRC wglCreateContextAttribsARB(HDC, HGLRC, const int*)
    public delegate* unmanaged[Stdcall]<nint, nint, int*, nint> CreateContextAttribsARB;

    // Extension support booleans
    public bool EXT_swap_control;
    public bool EXT_colorspace;
    public bool ARB_multisample;
    public bool ARB_framebuffer_sRGB;
    public bool EXT_framebuffer_sRGB;
    public bool ARB_pixel_format;
    public bool ARB_create_context;
    public bool ARB_create_context_profile;
    public bool EXT_create_context_es2_profile;
    public bool ARB_create_context_robustness;
    public bool ARB_create_context_no_error;
    public bool ARB_context_flush_control;
}

// -----------------------------------------------------------------------
// Extension to GlfwContext for WGL sub-object
// -----------------------------------------------------------------------

public partial class GlfwContext
{
    /// <summary>
    /// WGL-specific context state. Null until a WGL context is created.
    /// </summary>
    public GlfwContextWGL? wgl;
}

// -----------------------------------------------------------------------
// Extension to _glfw for wgl global state
// -----------------------------------------------------------------------

public static partial class _glfw
{
    /// <summary>
    /// WGL-specific global data. Null until <see cref="Glfw._glfwInitWGL"/> is called.
    /// </summary>
    public static GlfwLibraryWGL? wgl;
}

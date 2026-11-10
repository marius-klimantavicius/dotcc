//========================================================================
// GLFW 3.6 - www.glfw.org
//------------------------------------------------------------------------
// Copyright (c) 2002-2006 Marcus Geelnard
// Copyright (c) 2006-2019 Camilla Löwy <elmindreda@glfw.org>
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
//
//========================================================================

// Generated EGL constants and ABI signatures from GLFW internal.h at
// 92dcf4ce74f2e2554a98fea09be7c705c17daa5a.
namespace Glfw;
internal static class EGL
{
    internal const int EGL_SUCCESS = 0x3000;
    internal const int EGL_NOT_INITIALIZED = 0x3001;
    internal const int EGL_BAD_ACCESS = 0x3002;
    internal const int EGL_BAD_ALLOC = 0x3003;
    internal const int EGL_BAD_ATTRIBUTE = 0x3004;
    internal const int EGL_BAD_CONFIG = 0x3005;
    internal const int EGL_BAD_CONTEXT = 0x3006;
    internal const int EGL_BAD_CURRENT_SURFACE = 0x3007;
    internal const int EGL_BAD_DISPLAY = 0x3008;
    internal const int EGL_BAD_MATCH = 0x3009;
    internal const int EGL_BAD_NATIVE_PIXMAP = 0x300a;
    internal const int EGL_BAD_NATIVE_WINDOW = 0x300b;
    internal const int EGL_BAD_PARAMETER = 0x300c;
    internal const int EGL_BAD_SURFACE = 0x300d;
    internal const int EGL_CONTEXT_LOST = 0x300e;
    internal const int EGL_COLOR_BUFFER_TYPE = 0x303f;
    internal const int EGL_RGB_BUFFER = 0x308e;
    internal const int EGL_SURFACE_TYPE = 0x3033;
    internal const int EGL_WINDOW_BIT = 0x0004;
    internal const int EGL_RENDERABLE_TYPE = 0x3040;
    internal const int EGL_OPENGL_ES_BIT = 0x0001;
    internal const int EGL_OPENGL_ES2_BIT = 0x0004;
    internal const int EGL_OPENGL_BIT = 0x0008;
    internal const int EGL_ALPHA_SIZE = 0x3021;
    internal const int EGL_BLUE_SIZE = 0x3022;
    internal const int EGL_GREEN_SIZE = 0x3023;
    internal const int EGL_RED_SIZE = 0x3024;
    internal const int EGL_DEPTH_SIZE = 0x3025;
    internal const int EGL_STENCIL_SIZE = 0x3026;
    internal const int EGL_SAMPLES = 0x3031;
    internal const int EGL_OPENGL_ES_API = 0x30a0;
    internal const int EGL_OPENGL_API = 0x30a2;
    internal const int EGL_NONE = 0x3038;
    internal const int EGL_RENDER_BUFFER = 0x3086;
    internal const int EGL_SINGLE_BUFFER = 0x3085;
    internal const int EGL_EXTENSIONS = 0x3055;
    internal const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
    internal const int EGL_NATIVE_VISUAL_ID = 0x302e;
    internal const int EGL_PBUFFER_BIT = 0x0001;
    internal const int EGL_WIDTH = 0x3057;
    internal const int EGL_HEIGHT = 0x3056;
    internal const int EGL_CONTEXT_OPENGL_FORWARD_COMPATIBLE_BIT_KHR = 0x00000002;
    internal const int EGL_CONTEXT_OPENGL_CORE_PROFILE_BIT_KHR = 0x00000001;
    internal const int EGL_CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR = 0x00000002;
    internal const int EGL_CONTEXT_OPENGL_DEBUG_BIT_KHR = 0x00000001;
    internal const int EGL_CONTEXT_OPENGL_RESET_NOTIFICATION_STRATEGY_KHR = 0x31bd;
    internal const int EGL_NO_RESET_NOTIFICATION_KHR = 0x31be;
    internal const int EGL_LOSE_CONTEXT_ON_RESET_KHR = 0x31bf;
    internal const int EGL_CONTEXT_OPENGL_ROBUST_ACCESS_BIT_KHR = 0x00000004;
    internal const int EGL_CONTEXT_MAJOR_VERSION_KHR = 0x3098;
    internal const int EGL_CONTEXT_MINOR_VERSION_KHR = 0x30fb;
    internal const int EGL_CONTEXT_OPENGL_PROFILE_MASK_KHR = 0x30fd;
    internal const int EGL_CONTEXT_FLAGS_KHR = 0x30fc;
    internal const int EGL_CONTEXT_OPENGL_NO_ERROR_KHR = 0x31b3;
    internal const int EGL_GL_COLORSPACE_KHR = 0x309d;
    internal const int EGL_GL_COLORSPACE_SRGB_KHR = 0x3089;
    internal const int EGL_CONTEXT_RELEASE_BEHAVIOR_KHR = 0x2097;
    internal const int EGL_CONTEXT_RELEASE_BEHAVIOR_NONE_KHR = 0;
    internal const int EGL_CONTEXT_RELEASE_BEHAVIOR_FLUSH_KHR = 0x2098;
    internal const int EGL_PLATFORM_X11_EXT = 0x31d5;
    internal const int EGL_PLATFORM_WAYLAND_EXT = 0x31d8;
    internal const int EGL_PRESENT_OPAQUE_EXT = 0x31df;
    internal const int EGL_PLATFORM_ANGLE_ANGLE = 0x3202;
    internal const int EGL_PLATFORM_ANGLE_TYPE_ANGLE = 0x3203;
    internal const int EGL_PLATFORM_ANGLE_TYPE_OPENGL_ANGLE = 0x320d;
    internal const int EGL_PLATFORM_ANGLE_TYPE_OPENGLES_ANGLE = 0x320e;
    internal const int EGL_PLATFORM_ANGLE_TYPE_D3D9_ANGLE = 0x3207;
    internal const int EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE = 0x3208;
    internal const int EGL_PLATFORM_ANGLE_TYPE_VULKAN_ANGLE = 0x3450;
    internal const int EGL_PLATFORM_ANGLE_TYPE_METAL_ANGLE = 0x3489;
    internal const int EGL_PLATFORM_ANGLE_NATIVE_PLATFORM_TYPE_ANGLE = 0x348f;
    internal const int EGL_PLATFORM_SURFACELESS_MESA = 0x31dd;
}
internal unsafe sealed class GlfwLibraryEGL
{
    internal nint Handle, Display;
    internal int Platform, Major, Minor;
    internal bool Prefix, EXT_client_extensions, EXT_platform_base, EXT_platform_x11, ANGLE_platform_angle, ANGLE_platform_angle_opengl, ANGLE_platform_angle_d3d, ANGLE_platform_angle_vulkan, MESA_platform_surfaceless;
    internal bool KHR_create_context, KHR_create_context_no_error, KHR_gl_colorspace, KHR_get_all_proc_addresses, KHR_context_flush_control;
    internal delegate* unmanaged<nint, nint, int, int*, uint> GetConfigAttrib;
    internal delegate* unmanaged<nint, nint*, int, int*, uint> GetConfigs;
    internal delegate* unmanaged<nint, nint> GetDisplay;
    internal delegate* unmanaged<int> GetError;
    internal delegate* unmanaged<nint, int*, int*, uint> Initialize;
    internal delegate* unmanaged<nint, uint> Terminate;
    internal delegate* unmanaged<uint, uint> BindAPI;
    internal delegate* unmanaged<nint, nint, nint, int*, nint> CreateContext;
    internal delegate* unmanaged<nint, nint, uint> DestroySurface;
    internal delegate* unmanaged<nint, nint, uint> DestroyContext;
    internal delegate* unmanaged<nint, nint, nint, int*, nint> CreateWindowSurface;
    internal delegate* unmanaged<nint, nint, int*, nint> CreatePbufferSurface;
    internal delegate* unmanaged<nint, nint, nint, nint, uint> MakeCurrent;
    internal delegate* unmanaged<nint, nint, uint> SwapBuffers;
    internal delegate* unmanaged<nint, int, uint> SwapInterval;
    internal delegate* unmanaged<nint, int, byte*> QueryString;
    internal delegate* unmanaged<byte*, nint> GetProcAddress;
    internal delegate* unmanaged<uint, nint, int*, nint> GetPlatformDisplayEXT;
    internal delegate* unmanaged<nint, nint, nint, int*, nint> CreatePlatformWindowSurfaceEXT;
    internal bool LoadEntryPoints()
    {
        GetConfigAttrib = (delegate* unmanaged<nint, nint, int, int*, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglGetConfigAttrib");
        GetConfigs = (delegate* unmanaged<nint, nint*, int, int*, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglGetConfigs");
        GetDisplay = (delegate* unmanaged<nint, nint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglGetDisplay");
        GetError = (delegate* unmanaged<int>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglGetError");
        Initialize = (delegate* unmanaged<nint, int*, int*, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglInitialize");
        Terminate = (delegate* unmanaged<nint, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglTerminate");
        BindAPI = (delegate* unmanaged<uint, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglBindAPI");
        CreateContext = (delegate* unmanaged<nint, nint, nint, int*, nint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglCreateContext");
        DestroySurface = (delegate* unmanaged<nint, nint, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglDestroySurface");
        DestroyContext = (delegate* unmanaged<nint, nint, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglDestroyContext");
        CreateWindowSurface = (delegate* unmanaged<nint, nint, nint, int*, nint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglCreateWindowSurface");
        CreatePbufferSurface = (delegate* unmanaged<nint, nint, int*, nint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglCreatePbufferSurface");
        MakeCurrent = (delegate* unmanaged<nint, nint, nint, nint, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglMakeCurrent");
        SwapBuffers = (delegate* unmanaged<nint, nint, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglSwapBuffers");
        SwapInterval = (delegate* unmanaged<nint, int, uint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglSwapInterval");
        QueryString = (delegate* unmanaged<nint, int, byte*>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglQueryString");
        GetProcAddress = (delegate* unmanaged<byte*, nint>)Glfw._glfwPlatformGetModuleSymbol(Handle, "eglGetProcAddress");
        return GetConfigAttrib != null && GetConfigs != null && GetDisplay != null && GetError != null && Initialize != null && Terminate != null && BindAPI != null && CreateContext != null && DestroySurface != null && DestroyContext != null && CreateWindowSurface != null && CreatePbufferSurface != null && MakeCurrent != null && SwapBuffers != null && SwapInterval != null && QueryString != null && GetProcAddress != null;
    }
}

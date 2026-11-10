//========================================================================
// GLFW 3.6 EGL - www.glfw.org
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

// Managed port; provenance is in docs/glfw-update.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static Glfw.GLFW;
using static Glfw.EGL;

namespace Glfw;
public unsafe partial class GlfwContext { internal GlfwContextEGL? egl; }
internal sealed class GlfwContextEGL { internal nint Handle, Surface, Config, Client; }
public static unsafe partial class Glfw
{
    private static GlfwLibraryEGL egl = new();
    private static readonly List<nint> eglDeferredClients = new();
    private static string getEGLErrorString(int error) => error switch
    {
        EGL_SUCCESS => "Success", EGL_NOT_INITIALIZED => "EGL is not or could not be initialized", EGL_BAD_ACCESS => "EGL cannot access a requested resource",
        EGL_BAD_ALLOC => "EGL failed to allocate resources for the requested operation", EGL_BAD_ATTRIBUTE => "An unrecognized attribute or attribute value was passed in the attribute list",
        EGL_BAD_CONTEXT => "An EGLContext argument does not name a valid EGL rendering context", EGL_BAD_CONFIG => "An EGLConfig argument does not name a valid EGL frame buffer configuration",
        EGL_BAD_CURRENT_SURFACE => "The current surface of the calling thread is a window, pixel buffer or pixmap that is no longer valid", EGL_BAD_DISPLAY => "An EGLDisplay argument does not name a valid EGL display connection",
        EGL_BAD_SURFACE => "An EGLSurface argument does not name a valid surface configured for GL rendering", EGL_BAD_MATCH => "Arguments are inconsistent", EGL_BAD_PARAMETER => "One or more argument values are invalid",
        EGL_BAD_NATIVE_PIXMAP => "A NativePixmapType argument does not refer to a valid native pixmap", EGL_BAD_NATIVE_WINDOW => "A NativeWindowType argument does not refer to a valid native window",
        EGL_CONTEXT_LOST => "The application must destroy all contexts and reinitialise", _ => "ERROR: UNKNOWN EGL ERROR"
    };
    private static void EGLError(int code, string message) => _glfwInputError(code, message + ": {0}", getEGLErrorString(egl.GetError()));
    private static int getEGLConfigAttrib(nint config, int attribute) { int value; egl.GetConfigAttrib(egl.Display, config, attribute, &value); return value; }
    private static bool chooseEGLConfig(GlfwCtxConfig config, GlfwFbConfig desired, out nint result)
    {
        result = 0;
        int apiBit = config.Client == GLFW_OPENGL_ES_API ? config.Major == 1 ? EGL_OPENGL_ES_BIT : EGL_OPENGL_ES2_BIT : EGL_OPENGL_BIT;
        int surfaceBit = egl.Platform == EGL_PLATFORM_SURFACELESS_MESA ? EGL_PBUFFER_BIT : EGL_WINDOW_BIT;
        if (desired.Stereo) { _glfwInputError(GLFW_FORMAT_UNAVAILABLE, "EGL: Stereo rendering not supported"); return false; }
        int count = 0; egl.GetConfigs(egl.Display, null, 0, &count);
        if (count == 0) { _glfwInputError(GLFW_API_UNAVAILABLE, "EGL: No EGLConfigs returned"); return false; }
        var native = new nint[count];
        fixed (nint* p = native) egl.GetConfigs(egl.Display, p, count, &count);
        var usable = new GlfwFbConfig[count]; int usableCount = 0; bool wrongApi = false;
        for (int i = 0; i < count; i++)
        {
            nint n = native[i]; var u = new GlfwFbConfig();
            if (getEGLConfigAttrib(n, EGL_COLOR_BUFFER_TYPE) != EGL_RGB_BUFFER || (getEGLConfigAttrib(n, EGL_SURFACE_TYPE) & surfaceBit) == 0) continue;
            if (_glfw.platform!.PlatformID == GLFW_PLATFORM_X11)
            {
                XVisualInfo vi = default; vi.visualid = (nuint)getEGLConfigAttrib(n, EGL_NATIVE_VISUAL_ID);
                if (vi.visualid == 0) continue;
                if (desired.Transparent)
                {
                    int visualCount; nint vis = _glfw.X11!.xlib.GetVisualInfo(_glfw.X11.display, X11Constants.VisualIDMask, &vi, &visualCount);
                    if (vis != 0) { u.Transparent = _glfwIsVisualTransparentX11(((XVisualInfo*)vis)->visual); _glfw.X11.xlib.Free(vis); }
                }
            }
            if ((getEGLConfigAttrib(n, EGL_RENDERABLE_TYPE) & apiBit) == 0) { wrongApi = true; continue; }
            u.RedBits = getEGLConfigAttrib(n, EGL_RED_SIZE); u.GreenBits = getEGLConfigAttrib(n, EGL_GREEN_SIZE); u.BlueBits = getEGLConfigAttrib(n, EGL_BLUE_SIZE);
            u.AlphaBits = getEGLConfigAttrib(n, EGL_ALPHA_SIZE); u.DepthBits = getEGLConfigAttrib(n, EGL_DEPTH_SIZE); u.StencilBits = getEGLConfigAttrib(n, EGL_STENCIL_SIZE);
            u.Samples = getEGLConfigAttrib(n, EGL_SAMPLES); u.Doublebuffer = desired.Doublebuffer; u.Handle = (nuint)n;
            usable[usableCount++] = u;
        }
        int closest = _glfwChooseFBConfig(desired, usable, usableCount);
        if (closest >= 0) { result = (nint)usable[closest].Handle; return true; }
        if (wrongApi) _glfwInputError(GLFW_API_UNAVAILABLE, config.Client == GLFW_OPENGL_ES_API ? config.Major == 1 ? "EGL: Failed to find support for OpenGL ES 1.x" : "EGL: Failed to find support for OpenGL ES 2 or later" : "EGL: Failed to find support for OpenGL");
        else _glfwInputError(GLFW_FORMAT_UNAVAILABLE, "EGL: Failed to find a suitable EGLConfig");
        return false;
    }
    private static void makeContextCurrentEGL(GlfwWindow? window)
    {
        nint surface = window?.context.egl?.Surface ?? 0, context = window?.context.egl?.Handle ?? 0;
        if (egl.MakeCurrent(egl.Display, surface, surface, context) == 0) { EGLError(GLFW_PLATFORM_ERROR, window == null ? "EGL: Failed to clear current context" : "EGL: Failed to make context current"); return; }
        _glfw.contextSlot = window;
    }
    private static void swapBuffersEGL(GlfwWindow window)
    {
        if (_glfw.contextSlot != window) { _glfwInputError(GLFW_PLATFORM_ERROR, "EGL: The context must be current on the calling thread when swapping buffers"); return; }
        egl.SwapBuffers(egl.Display, window.context.egl!.Surface);
    }
    private static bool extensionSupportedEGL(string extension)
    {
        byte* p = egl.QueryString(egl.Display, EGL_EXTENSIONS);
        return p != null && _glfwStringInExtensionString(extension, Marshal.PtrToStringAnsi((nint)p)!);
    }
    private static nint EGLProc(string name) { fixed (byte* p = Encoding.ASCII.GetBytes(name + '\0')) return egl.GetProcAddress(p); }
    private static nint getProcAddressEGL(string name)
    {
        nint result = EGLProc(name);
        if (result != 0) return result;
        return !egl.KHR_get_all_proc_addresses ? _glfwPlatformGetModuleSymbol(_glfw.contextSlot!.context.egl!.Client, name) : 0;
    }
    private static void destroyContextEGL(GlfwWindow window)
    {
        var ctx = window.context.egl;
        if (ctx == null) return;
        if (_glfw.platform!.PlatformID == GLFW_PLATFORM_X11 && window.context.Client == GLFW_OPENGL_API)
        { if (ctx.Client != 0) eglDeferredClients.Add(ctx.Client); }
        else _glfwPlatformFreeModule(ctx.Client);
        ctx.Client = 0;
        if (ctx.Surface != 0) { egl.DestroySurface(egl.Display, ctx.Surface); ctx.Surface = 0; }
        if (ctx.Handle != 0) { egl.DestroyContext(egl.Display, ctx.Handle); ctx.Handle = 0; }
    }
    private static int GetEGLPlatform(out int[]? attributes)
    {
        attributes = null; int platform = _glfw.platform!.PlatformID;
        if (platform == GLFW_PLATFORM_NULL) return egl.EXT_platform_base && egl.MESA_platform_surfaceless ? EGL_PLATFORM_SURFACELESS_MESA : 0;
        if (egl.ANGLE_platform_angle)
        {
            int type = 0, hint = _glfw.hints.Init.AngleType;
            if (egl.ANGLE_platform_angle_opengl)
            {
                if (hint == GLFW_ANGLE_PLATFORM_TYPE_OPENGL) type = EGL_PLATFORM_ANGLE_TYPE_OPENGL_ANGLE;
                else if (platform == GLFW_PLATFORM_WIN32 && hint == GLFW_ANGLE_PLATFORM_TYPE_OPENGLES) type = EGL_PLATFORM_ANGLE_TYPE_OPENGLES_ANGLE;
            }
            if (platform == GLFW_PLATFORM_WIN32 && egl.ANGLE_platform_angle_d3d)
            {
                if (hint == GLFW_ANGLE_PLATFORM_TYPE_D3D9) type = EGL_PLATFORM_ANGLE_TYPE_D3D9_ANGLE;
                else if (hint == GLFW_ANGLE_PLATFORM_TYPE_D3D11) type = EGL_PLATFORM_ANGLE_TYPE_D3D11_ANGLE;
            }
            if (egl.ANGLE_platform_angle_vulkan && hint == GLFW_ANGLE_PLATFORM_TYPE_VULKAN) type = EGL_PLATFORM_ANGLE_TYPE_VULKAN_ANGLE;
            if (type != 0)
            {
                attributes = platform == GLFW_PLATFORM_X11 ? new[] { EGL_PLATFORM_ANGLE_TYPE_ANGLE, type, EGL_PLATFORM_ANGLE_NATIVE_PLATFORM_TYPE_ANGLE, EGL_PLATFORM_X11_EXT, EGL_NONE } : new[] { EGL_PLATFORM_ANGLE_TYPE_ANGLE, type, EGL_NONE };
                return EGL_PLATFORM_ANGLE_ANGLE;
            }
        }
        return platform == GLFW_PLATFORM_X11 && egl.EXT_platform_base && egl.EXT_platform_x11 ? EGL_PLATFORM_X11_EXT : 0;
    }
    internal static bool _glfwInitEGL()
    {
        if (egl.Handle != 0) return true;
        string[] names = OperatingSystem.IsWindows() ? new[] { "libEGL.dll", "EGL.dll" } : OperatingSystem.IsMacOS() ? new[] { "libEGL.dylib" } : new[] { "libEGL.so.1" };
        foreach (var name in names) { egl.Handle = _glfwPlatformLoadModule(name); if (egl.Handle != 0) { egl.Prefix = name.StartsWith("lib", StringComparison.Ordinal); break; } }
        if (egl.Handle == 0) { _glfwInputError(GLFW_API_UNAVAILABLE, "EGL: Library not found"); return false; }
        if (!egl.LoadEntryPoints()) { _glfwInputError(GLFW_PLATFORM_ERROR, "EGL: Failed to load required entry points"); _glfwTerminateEGL(); return false; }
        byte* extensions = egl.QueryString(0, EGL_EXTENSIONS);
        if (extensions != null && egl.GetError() == EGL_SUCCESS) egl.EXT_client_extensions = true;
        if (egl.EXT_client_extensions)
        {
            string text = Marshal.PtrToStringAnsi((nint)extensions)!;
            bool Has(string name) => _glfwStringInExtensionString(name, text);
            egl.EXT_platform_base = Has("EGL_EXT_platform_base"); egl.EXT_platform_x11 = Has("EGL_EXT_platform_x11");
            egl.ANGLE_platform_angle = Has("EGL_ANGLE_platform_angle"); egl.ANGLE_platform_angle_opengl = Has("EGL_ANGLE_platform_angle_opengl"); egl.ANGLE_platform_angle_d3d = Has("EGL_ANGLE_platform_angle_d3d"); egl.ANGLE_platform_angle_vulkan = Has("EGL_ANGLE_platform_angle_vulkan"); egl.MESA_platform_surfaceless = Has("EGL_MESA_platform_surfaceless");
        }
        if (egl.EXT_platform_base)
        {
            egl.GetPlatformDisplayEXT = (delegate* unmanaged<uint, nint, int*, nint>)EGLProc("eglGetPlatformDisplayEXT");
            egl.CreatePlatformWindowSurfaceEXT = (delegate* unmanaged<nint, nint, nint, int*, nint>)EGLProc("eglCreatePlatformWindowSurfaceEXT");
        }
        egl.Platform = GetEGLPlatform(out int[]? attrs);
        nint nativeDisplay = _glfw.platform!.PlatformID == GLFW_PLATFORM_X11 ? _glfw.X11!.display : _glfw.platform.PlatformID == GLFW_PLATFORM_WIN32 ? Win32Native.gdi32!.GetDC(_glfw.Win32!.helperWindowHandle) : 0;
        fixed (int* p = attrs) egl.Display = egl.Platform != 0 ? egl.GetPlatformDisplayEXT((uint)egl.Platform, nativeDisplay, p) : egl.GetDisplay(nativeDisplay);
        if (egl.Display == 0) { EGLError(GLFW_API_UNAVAILABLE, "EGL: Failed to get EGL display"); _glfwTerminateEGL(); return false; }
        int major, minor;
        if (egl.Initialize(egl.Display, &major, &minor) == 0) { EGLError(GLFW_API_UNAVAILABLE, "EGL: Failed to initialize EGL"); _glfwTerminateEGL(); return false; }
        egl.Major = major; egl.Minor = minor;
        egl.KHR_create_context = extensionSupportedEGL("EGL_KHR_create_context"); egl.KHR_create_context_no_error = extensionSupportedEGL("EGL_KHR_create_context_no_error"); egl.KHR_gl_colorspace = extensionSupportedEGL("EGL_KHR_gl_colorspace"); egl.KHR_get_all_proc_addresses = extensionSupportedEGL("EGL_KHR_get_all_proc_addresses"); egl.KHR_context_flush_control = extensionSupportedEGL("EGL_KHR_context_flush_control");
        return true;
    }
    internal static void _glfwTerminateEGL()
    {
        if (egl.Display != 0 && egl.Terminate != null) egl.Terminate(egl.Display);
        foreach (nint client in eglDeferredClients) _glfwPlatformFreeModule(client);
        eglDeferredClients.Clear(); _glfwPlatformFreeModule(egl.Handle); egl = new GlfwLibraryEGL();
    }
    internal static bool _glfwCreateContextEGL(GlfwWindow window, GlfwCtxConfig config, GlfwFbConfig fb)
    {
        if (egl.Display == 0) { _glfwInputError(GLFW_API_UNAVAILABLE, "EGL: API not available"); return false; }
        if (!chooseEGLConfig(config, fb, out nint nativeConfig)) return false;
        if (egl.BindAPI((uint)(config.Client == GLFW_OPENGL_ES_API ? EGL_OPENGL_ES_API : EGL_OPENGL_API)) == 0) { EGLError(GLFW_API_UNAVAILABLE, config.Client == GLFW_OPENGL_ES_API ? "EGL: Failed to bind OpenGL ES" : "EGL: Failed to bind OpenGL"); return false; }
        int* attrs = stackalloc int[40]; int index = 0;
        void Add(int attribute, int value) { attrs[index++] = attribute; attrs[index++] = value; }
        if (egl.KHR_create_context)
        {
            int mask = 0, flags = 0;
            if (config.Client == GLFW_OPENGL_API)
            {
                if (config.Forward) flags |= EGL_CONTEXT_OPENGL_FORWARD_COMPATIBLE_BIT_KHR;
                if (config.Profile == GLFW_OPENGL_CORE_PROFILE) mask |= EGL_CONTEXT_OPENGL_CORE_PROFILE_BIT_KHR;
                else if (config.Profile == GLFW_OPENGL_COMPAT_PROFILE) mask |= EGL_CONTEXT_OPENGL_COMPATIBILITY_PROFILE_BIT_KHR;
            }
            if (config.Debug) flags |= EGL_CONTEXT_OPENGL_DEBUG_BIT_KHR;
            if (config.Robustness != 0)
            {
                if (config.Robustness == GLFW_NO_RESET_NOTIFICATION) Add(EGL_CONTEXT_OPENGL_RESET_NOTIFICATION_STRATEGY_KHR, EGL_NO_RESET_NOTIFICATION_KHR);
                else if (config.Robustness == GLFW_LOSE_CONTEXT_ON_RESET) Add(EGL_CONTEXT_OPENGL_RESET_NOTIFICATION_STRATEGY_KHR, EGL_LOSE_CONTEXT_ON_RESET_KHR);
                flags |= EGL_CONTEXT_OPENGL_ROBUST_ACCESS_BIT_KHR;
            }
            if (config.Major != 1 || config.Minor != 0) { Add(EGL_CONTEXT_MAJOR_VERSION_KHR, config.Major); Add(EGL_CONTEXT_MINOR_VERSION_KHR, config.Minor); }
            if (config.Noerror && egl.KHR_create_context_no_error) Add(EGL_CONTEXT_OPENGL_NO_ERROR_KHR, 1);
            if (mask != 0) Add(EGL_CONTEXT_OPENGL_PROFILE_MASK_KHR, mask);
            if (flags != 0) Add(EGL_CONTEXT_FLAGS_KHR, flags);
        }
        else if (config.Client == GLFW_OPENGL_ES_API) Add(EGL_CONTEXT_CLIENT_VERSION, config.Major);
        if (egl.KHR_context_flush_control)
        {
            if (config.Release == GLFW_RELEASE_BEHAVIOR_NONE) Add(EGL_CONTEXT_RELEASE_BEHAVIOR_KHR, EGL_CONTEXT_RELEASE_BEHAVIOR_NONE_KHR);
            else if (config.Release == GLFW_RELEASE_BEHAVIOR_FLUSH) Add(EGL_CONTEXT_RELEASE_BEHAVIOR_KHR, EGL_CONTEXT_RELEASE_BEHAVIOR_FLUSH_KHR);
        }
        Add(EGL_NONE, EGL_NONE);
        var ctx = window.context.egl = new GlfwContextEGL();
        ctx.Handle = egl.CreateContext(egl.Display, nativeConfig, config.Share?.context.egl?.Handle ?? 0, attrs);
        if (ctx.Handle == 0) { EGLError(GLFW_VERSION_UNAVAILABLE, "EGL: Failed to create context"); return false; }
        // Install destruction immediately so every subsequent failed creation frees native resources.
        window.context.destroy = destroyContextEGL;
        index = 0;
        if (fb.SRGB && egl.KHR_gl_colorspace) Add(EGL_GL_COLORSPACE_KHR, EGL_GL_COLORSPACE_SRGB_KHR);
        if (!fb.Doublebuffer) Add(EGL_RENDER_BUFFER, EGL_SINGLE_BUFFER);
        if (egl.Platform == EGL_PLATFORM_SURFACELESS_MESA) { _glfw.platform!.GetFramebufferSize(window, out int width, out int height); Add(EGL_WIDTH, width); Add(EGL_HEIGHT, height); }
        Add(EGL_NONE, EGL_NONE);
        nuint x11Handle = window.X11?.handle ?? 0;
        nint nativeWindow = _glfw.platform!.PlatformID == GLFW_PLATFORM_X11 ? egl.Platform != 0 ? (nint)(&x11Handle) : (nint)x11Handle : window.Win32?.handle ?? 0;
        if (egl.Platform == 0 || egl.Platform == EGL_PLATFORM_ANGLE_ANGLE) ctx.Surface = egl.CreateWindowSurface(egl.Display, nativeConfig, nativeWindow, attrs);
        else if (egl.Platform == EGL_PLATFORM_SURFACELESS_MESA) ctx.Surface = egl.CreatePbufferSurface(egl.Display, nativeConfig, attrs);
        else ctx.Surface = egl.CreatePlatformWindowSurfaceEXT(egl.Display, nativeConfig, nativeWindow, attrs);
        if (ctx.Surface == 0) { EGLError(GLFW_PLATFORM_ERROR, "EGL: Failed to create window surface"); return false; }
        ctx.Config = nativeConfig;
        if (!egl.KHR_get_all_proc_addresses)
        {
            string[] names;
            if (config.Client == GLFW_OPENGL_ES_API)
                names = config.Major == 1 ? OperatingSystem.IsWindows() ? new[] { "GLESv1_CM.dll", "libGLES_CM.dll" } : new[] { "libGLESv1_CM.so.1", "libGLES_CM.so.1" } : OperatingSystem.IsWindows() ? new[] { "GLESv2.dll", "libGLESv2.dll" } : new[] { "libGLESv2.so.2" };
            else names = OperatingSystem.IsWindows() ? Array.Empty<string>() : new[] { "libOpenGL.so.0", "libGL.so.1" };
            foreach (var name in names)
            {
                if (egl.Prefix != name.StartsWith("lib", StringComparison.Ordinal)) continue;
                ctx.Client = _glfwPlatformLoadModule(name); if (ctx.Client != 0) break;
            }
            if (ctx.Client == 0) { _glfwInputError(GLFW_API_UNAVAILABLE, "EGL: Failed to load client library"); return false; }
        }
        window.context.makeCurrent = makeContextCurrentEGL; window.context.swapBuffers = swapBuffersEGL;
        window.context.swapInterval = interval => { egl.SwapInterval(egl.Display, interval); };
        window.context.extensionSupported = extensionSupportedEGL; window.context.getProcAddress = getProcAddressEGL;
        return true;
    }
    internal static bool _glfwChooseVisualEGL(GlfwWndConfig wnd, GlfwCtxConfig config, GlfwFbConfig fb, out nint visual, out int depth)
    {
        visual = 0; depth = 0;
        if (!chooseEGLConfig(config, fb, out nint native)) return false;
        XVisualInfo desired = default; desired.screen = _glfw.X11!.screen; desired.visualid = (nuint)getEGLConfigAttrib(native, EGL_NATIVE_VISUAL_ID);
        int count; nint result = _glfw.X11.xlib.GetVisualInfo(_glfw.X11.display, X11Constants.VisualIDMask | X11Constants.VisualScreenMask, &desired, &count);
        if (result == 0) { _glfwInputError(GLFW_PLATFORM_ERROR, "EGL: Failed to retrieve Visual for EGLConfig"); return false; }
        visual = ((XVisualInfo*)result)->visual; depth = ((XVisualInfo*)result)->depth; _glfw.X11.xlib.Free(result); return true;
    }
    public static nint glfwGetEGLDisplay()
    { if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return 0; } return egl.Display; }
    private static bool ValidEGLWindow(GlfwWindow window)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return false; }
        if (window.context.Source != GLFW_EGL_CONTEXT_API) { _glfwInputError(GLFW_NO_WINDOW_CONTEXT, null); return false; }
        return true;
    }
    public static nint glfwGetEGLContext(GlfwWindow window) => ValidEGLWindow(window) ? window.context.egl!.Handle : 0;
    public static nint glfwGetEGLSurface(GlfwWindow window) => ValidEGLWindow(window) ? window.context.egl!.Surface : 0;
    public static bool glfwGetEGLConfig(GlfwWindow window, out nint config) { config = 0; if (!ValidEGLWindow(window)) return false; config = window.context.egl!.Config; return true; }
}

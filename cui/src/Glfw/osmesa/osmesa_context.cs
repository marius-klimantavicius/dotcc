//========================================================================
// GLFW 3.6 OSMesa - www.glfw.org
//------------------------------------------------------------------------
// Copyright (c) 2016 Google Inc.
// Copyright (c) 2016-2017 Camilla Löwy <elmindreda@glfw.org>
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
using System.Runtime.InteropServices;
using System.Text;
using static Glfw.GLFW;

namespace Glfw;
public unsafe partial class GlfwContext
{
    internal GlfwContextOSMesa? osmesa;
}
internal sealed class GlfwContextOSMesa { internal nint Handle, Buffer; internal int Width, Height; }
internal unsafe sealed class GlfwLibraryOSMesa
{
    internal nint Handle;
    internal delegate* unmanaged<uint, int, int, int, nint, nint> CreateContextExt;
    internal delegate* unmanaged<int*, nint, nint> CreateContextAttribs;
    internal delegate* unmanaged<nint, void> DestroyContext;
    internal delegate* unmanaged<nint, nint, int, int, int, int> MakeCurrent;
    internal delegate* unmanaged<nint, int*, int*, int*, nint*, int> GetColorBuffer, GetDepthBuffer;
    internal delegate* unmanaged<byte*, nint> GetProcAddress;
}
public static unsafe partial class Glfw
{
    private static GlfwLibraryOSMesa osmesa = new();
    private static void makeContextCurrentOSMesa(GlfwWindow? window)
    {
        if (window != null)
        {
            _glfw.platform!.GetFramebufferSize(window, out int width, out int height);
            var ctx = window.context.osmesa!;
            if (ctx.Buffer == 0 || ctx.Width != width || ctx.Height != height)
            {
                NativeMemory.Free((void*)ctx.Buffer);
                ctx.Buffer = (nint)NativeMemory.AllocZeroed(4, checked((nuint)width * (nuint)height));
                ctx.Width = width; ctx.Height = height;
            }
            if (osmesa.MakeCurrent(ctx.Handle, ctx.Buffer, 0x1401, width, height) == 0)
            { _glfwInputError(GLFW_PLATFORM_ERROR, "OSMesa: Failed to make context current"); return; }
        }
        _glfw.contextSlot = window;
    }
    private static nint getProcAddressOSMesa(string name)
    { fixed (byte* p = Encoding.ASCII.GetBytes(name + '\0')) return osmesa.GetProcAddress(p); }
    private static void destroyContextOSMesa(GlfwWindow window)
    {
        var ctx = window.context.osmesa;
        if (ctx == null) return;
        if (ctx.Handle != 0) osmesa.DestroyContext(ctx.Handle);
        NativeMemory.Free((void*)ctx.Buffer);
        ctx.Handle = ctx.Buffer = 0; ctx.Width = ctx.Height = 0;
    }
    internal static bool _glfwInitOSMesa()
    {
        if (osmesa.Handle != 0) return true;
        string[] names = OperatingSystem.IsWindows() ? new[] { "libOSMesa.dll", "OSMesa.dll" } : OperatingSystem.IsMacOS() ? new[] { "libOSMesa.8.dylib" } : new[] { "libOSMesa.so.8", "libOSMesa.so.6" };
        foreach (var name in names) { osmesa.Handle = _glfwPlatformLoadModule(name); if (osmesa.Handle != 0) break; }
        if (osmesa.Handle == 0) { _glfwInputError(GLFW_API_UNAVAILABLE, "OSMesa: Library not found"); return false; }
        osmesa.CreateContextExt = (delegate* unmanaged<uint, int, int, int, nint, nint>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaCreateContextExt");
        osmesa.CreateContextAttribs = (delegate* unmanaged<int*, nint, nint>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaCreateContextAttribs");
        osmesa.DestroyContext = (delegate* unmanaged<nint, void>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaDestroyContext");
        osmesa.MakeCurrent = (delegate* unmanaged<nint, nint, int, int, int, int>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaMakeCurrent");
        osmesa.GetColorBuffer = (delegate* unmanaged<nint, int*, int*, int*, nint*, int>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaGetColorBuffer");
        osmesa.GetDepthBuffer = (delegate* unmanaged<nint, int*, int*, int*, nint*, int>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaGetDepthBuffer");
        osmesa.GetProcAddress = (delegate* unmanaged<byte*, nint>)_glfwPlatformGetModuleSymbol(osmesa.Handle, "OSMesaGetProcAddress");
        if (osmesa.CreateContextExt == null || osmesa.DestroyContext == null || osmesa.MakeCurrent == null || osmesa.GetColorBuffer == null || osmesa.GetDepthBuffer == null || osmesa.GetProcAddress == null)
        { _glfwInputError(GLFW_PLATFORM_ERROR, "OSMesa: Failed to load required entry points"); _glfwTerminateOSMesa(); return false; }
        return true;
    }
    internal static void _glfwTerminateOSMesa() { _glfwPlatformFreeModule(osmesa.Handle); osmesa = new GlfwLibraryOSMesa(); }
    internal static bool _glfwCreateContextOSMesa(GlfwWindow window, GlfwCtxConfig config, GlfwFbConfig fb)
    {
        if (config.Client == GLFW_OPENGL_ES_API) { _glfwInputError(GLFW_API_UNAVAILABLE, "OSMesa: OpenGL ES is not available on OSMesa"); return false; }
        var ctx = window.context.osmesa = new GlfwContextOSMesa();
        nint share = config.Share?.context.osmesa?.Handle ?? 0;
        int accum = fb.AccumRedBits + fb.AccumGreenBits + fb.AccumBlueBits + fb.AccumAlphaBits;
        if (osmesa.CreateContextAttribs != null)
        {
            int* attrs = stackalloc int[40]; int index = 0;
            void Add(int a, int v) { attrs[index++] = a; attrs[index++] = v; }
            Add(0x22, 0x1908); Add(0x30, fb.DepthBits); Add(0x31, fb.StencilBits); Add(0x32, accum);
            if (config.Profile == GLFW_OPENGL_CORE_PROFILE) Add(0x33, 0x34);
            else if (config.Profile == GLFW_OPENGL_COMPAT_PROFILE) Add(0x33, 0x35);
            if (config.Major != 1 || config.Minor != 0) { Add(0x36, config.Major); Add(0x37, config.Minor); }
            if (config.Forward) { _glfwInputError(GLFW_VERSION_UNAVAILABLE, "OSMesa: Forward-compatible contexts not supported"); return false; }
            Add(0, 0); ctx.Handle = osmesa.CreateContextAttribs(attrs, share);
        }
        else
        {
            if (config.Profile != 0) { _glfwInputError(GLFW_VERSION_UNAVAILABLE, "OSMesa: OpenGL profiles unavailable"); return false; }
            ctx.Handle = osmesa.CreateContextExt(0x1908, fb.DepthBits, fb.StencilBits, accum, share);
        }
        if (ctx.Handle == 0) { _glfwInputError(GLFW_VERSION_UNAVAILABLE, "OSMesa: Failed to create context"); return false; }
        window.context.makeCurrent = makeContextCurrentOSMesa;
        window.context.swapBuffers = _ => { }; // OSMesa has no double buffering.
        window.context.swapInterval = _ => { }; // OSMesa has no swap interval.
        window.context.extensionSupported = _ => false; // OSMesa has no platform extensions.
        window.context.getProcAddress = getProcAddressOSMesa;
        window.context.destroy = destroyContextOSMesa;
        return true;
    }
    public static nint glfwGetOSMesaContext(GlfwWindow window)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return 0; }
        if (window.context.Source != GLFW_OSMESA_CONTEXT_API) { _glfwInputError(GLFW_NO_WINDOW_CONTEXT, null); return 0; }
        return window.context.osmesa!.Handle;
    }
    public static bool glfwGetOSMesaColorBuffer(GlfwWindow window, out int width, out int height, out int format, out nint buffer) => GetOSMesaBuffer(window, false, out width, out height, out format, out buffer);
    public static bool glfwGetOSMesaDepthBuffer(GlfwWindow window, out int width, out int height, out int bytesPerValue, out nint buffer) => GetOSMesaBuffer(window, true, out width, out height, out bytesPerValue, out buffer);
    private static bool GetOSMesaBuffer(GlfwWindow window, bool depth, out int width, out int height, out int format, out nint buffer)
    {
        width = height = format = 0; buffer = 0;
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return false; }
        if (window.context.Source != GLFW_OSMESA_CONTEXT_API) { _glfwInputError(GLFW_NO_WINDOW_CONTEXT, null); return false; }
        int w, h, f; nint b;
        int success = depth ? osmesa.GetDepthBuffer(window.context.osmesa!.Handle, &w, &h, &f, &b) : osmesa.GetColorBuffer(window.context.osmesa!.Handle, &w, &h, &f, &b);
        if (success == 0) { _glfwInputError(GLFW_PLATFORM_ERROR, depth ? "OSMesa: Failed to retrieve depth buffer" : "OSMesa: Failed to retrieve color buffer"); return false; }
        width = w; height = h; format = f; buffer = b; return true;
    }
}

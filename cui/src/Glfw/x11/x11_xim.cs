//========================================================================
// GLFW 3.6 X11 - www.glfw.org
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Glfw;
public static unsafe partial class Glfw
{
    [StructLayout(LayoutKind.Sequential)] private struct XIMStyles { internal ushort Count; internal nuint* Styles; }
    [StructLayout(LayoutKind.Sequential)] private struct XIMCallback { internal nint Data, Callback; }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void inputMethodDestroyCallback(nint im, nint data, nint callData)
    { if (_glfw.X11 != null) _glfw.X11.im = 0; }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void inputMethodInstantiateCallback(nint display, nint data, nint callData)
    {
        var x11 = _glfw.X11!;
        if (x11.im != 0) return;
        x11.im = x11.xlib.OpenIM(x11.display, 0, null, null);
        if (x11.im != 0 && !hasUsableInputMethodStyle()) { x11.xlib.CloseIM(x11.im); x11.im = 0; }
        if (x11.im == 0) return;
        XIMCallback callback = new() { Callback = (nint)(delegate* unmanaged[Cdecl]<nint, nint, nint, void>)&inputMethodDestroyCallback };
        fixed (byte* name = "destroyCallback\0"u8)
            ((delegate* unmanaged[Cdecl]<nint, byte*, XIMCallback*, nint, nint>)x11.xlib.SetIMValues_ptr)(x11.im, name, &callback, 0);
        for (var window = _glfw.windowListHead; window != null; window = window.Next) _glfwCreateInputContextX11(window);
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void inputContextDestroyCallback(nint ic, nint data, nint callData)
    {
        var handle = GCHandle.FromIntPtr(data);
        if (handle.Target is GlfwWindow window && window.X11 != null) { window.X11.ic = 0; window.X11.icHandle = 0; }
        handle.Free();
    }
    private static void CreateX11InputContext(GlfwWindow window)
    {
        var x11 = _glfw.X11!;
        if (window.X11 == null || x11.im == 0) return;
        var handle = GCHandle.Alloc(window);
        XIMCallback callback = new() { Data = GCHandle.ToIntPtr(handle), Callback = (nint)(delegate* unmanaged[Cdecl]<nint, nint, nint, void>)&inputContextDestroyCallback };
        fixed (byte* style = "inputStyle\0"u8)
        fixed (byte* client = "clientWindow\0"u8)
        fixed (byte* focus = "focusWindow\0"u8)
        fixed (byte* destroy = "destroyCallback\0"u8)
        {
            // All variadic values have pointer/integer ABI class. Xlib copies
            // the XIMCallback; its data GCHandle stays alive until IC destruction.
            var create = (delegate* unmanaged[Cdecl]<nint, byte*, nuint, byte*, nuint, byte*, nuint, byte*, XIMCallback*, nint, nint>)x11.xlib.CreateIC_ptr;
            window.X11.ic = create(x11.im, style, 0x408, client, window.X11.handle, focus, window.X11.handle, destroy, &callback, 0);
        }
        if (window.X11.ic == 0) { handle.Free(); return; }
        window.X11.icHandle = GCHandle.ToIntPtr(handle);
        XWindowAttributes attributes;
        x11.xlib.GetWindowAttributes(x11.display, window.X11.handle, &attributes);
        nuint filter = 0;
        fixed (byte* name = "filterEvents\0"u8)
        {
            if (((delegate* unmanaged[Cdecl]<nint, byte*, nuint*, nint, nint>)x11.xlib.GetICValues_ptr)(window.X11.ic, name, &filter, 0) == 0)
                x11.xlib.SelectInput(x11.display, window.X11.handle, attributes.your_event_mask | (nint)filter);
        }
    }
    private static void RegisterX11InputMethod(bool register)
    {
        var x11 = _glfw.X11!;
        nint callback = (nint)(delegate* unmanaged[Cdecl]<nint, nint, nint, void>)&inputMethodInstantiateCallback;
        if (register) x11.xlib.RegisterIMInstantiateCallback(x11.display, 0, null, null, callback, 0);
        else x11.xlib.UnregisterIMInstantiateCallback(x11.display, 0, null, null, callback, 0);
    }
}

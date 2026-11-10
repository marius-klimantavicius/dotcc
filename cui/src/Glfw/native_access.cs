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
using static Glfw.GLFW;

namespace Glfw;
public static unsafe partial class Glfw
{
    public static nint glfwGetX11Display() => RequireX11() ? _glfw.X11!.display : 0;
    public static nuint glfwGetX11Window(GlfwWindow window) => RequireX11() ? window.X11!.handle : 0;
    public static void glfwSetX11SelectionString(string text)
    {
        if (!RequireX11()) return;
        var x11 = _glfw.X11!;
        x11.primarySelectionString = text;
        x11.xlib.SetSelectionOwner(x11.display, X11Constants.XA_PRIMARY, x11.helperWindowHandle, X11Constants.CurrentTime);
        if (x11.xlib.GetSelectionOwner(x11.display, X11Constants.XA_PRIMARY) != x11.helperWindowHandle)
            _glfwInputError(GLFW_PLATFORM_ERROR, "X11: Failed to become owner of primary selection");
    }
    public static string? glfwGetX11SelectionString() => RequireX11() ? getSelectionString(X11Constants.XA_PRIMARY) : null;
    private static bool RequireX11()
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return false; }
        if (_glfw.platform!.PlatformID != GLFW_PLATFORM_X11) { _glfwInputError(GLFW_PLATFORM_UNAVAILABLE, "X11: Platform not initialized"); return false; }
        return true;
    }
    public static nint glfwGetWin32Window(GlfwWindow window)
    {
        if (!_glfw.initialized) { _glfwInputError(GLFW_NOT_INITIALIZED, null); return 0; }
        if (_glfw.platform!.PlatformID != GLFW_PLATFORM_WIN32) { _glfwInputError(GLFW_PLATFORM_UNAVAILABLE, "Win32: Platform not initialized"); return 0; }
        return window.Win32!.handle;
    }
}

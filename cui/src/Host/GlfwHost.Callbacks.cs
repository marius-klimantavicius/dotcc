#nullable enable
using System;
using System.Runtime.InteropServices;
using G = global::Glfw.Glfw;
using static Managed.Cui.Cui;
namespace Managed.Cui;
internal static unsafe partial class GlfwHost
{
    public static delegate*<GLFWwindow*, double, double, void> glfwSetCursorPosCallback(GLFWwindow* w, delegate*<GLFWwindow*, double, double, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "CursorPos");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetCursorPosCallback(window, address == 0 ? null : (ignored, x, y) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, double, double, void>)address)((GLFWwindow*)token, x, y)));
        return (delegate*<GLFWwindow*, double, double, void>)previous;
    }
    public static delegate*<GLFWwindow*, int, void> glfwSetCursorEnterCallback(GLFWwindow* w, delegate*<GLFWwindow*, int, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "CursorEnter");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetCursorEnterCallback(window, address == 0 ? null : (ignored, entered) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, int, void>)address)((GLFWwindow*)token, entered)));
        return (delegate*<GLFWwindow*, int, void>)previous;
    }
    public static delegate*<GLFWwindow*, int, int, int, void> glfwSetMouseButtonCallback(GLFWwindow* w, delegate*<GLFWwindow*, int, int, int, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "MouseButton");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetMouseButtonCallback(window, address == 0 ? null : (ignored, button, action, mods) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, int, int, int, void>)address)((GLFWwindow*)token, button, action, mods)));
        return (delegate*<GLFWwindow*, int, int, int, void>)previous;
    }
    public static delegate*<GLFWwindow*, double, double, void> glfwSetScrollCallback(GLFWwindow* w, delegate*<GLFWwindow*, double, double, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "Scroll");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetScrollCallback(window, address == 0 ? null : (ignored, x, y) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, double, double, void>)address)((GLFWwindow*)token, x, y)));
        return (delegate*<GLFWwindow*, double, double, void>)previous;
    }
    public static delegate*<GLFWwindow*, int, int, int, int, void> glfwSetKeyCallback(GLFWwindow* w, delegate*<GLFWwindow*, int, int, int, int, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "Key");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetKeyCallback(window, address == 0 ? null : (ignored, key, scancode, action, mods) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, int, int, int, int, void>)address)((GLFWwindow*)token, key, scancode, action, mods)));
        return (delegate*<GLFWwindow*, int, int, int, int, void>)previous;
    }
    public static delegate*<GLFWwindow*, uint, void> glfwSetCharCallback(GLFWwindow* w, delegate*<GLFWwindow*, uint, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "Char");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetCharCallback(window, address == 0 ? null : (ignored, codepoint) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, uint, void>)address)((GLFWwindow*)token, codepoint)));
        return (delegate*<GLFWwindow*, uint, void>)previous;
    }
    public static delegate*<GLFWwindow*, int, void> glfwSetWindowFocusCallback(GLFWwindow* w, delegate*<GLFWwindow*, int, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "WindowFocus");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetWindowFocusCallback(window, address == 0 ? null : (ignored, focused) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, int, void>)address)((GLFWwindow*)token, focused)));
        return (delegate*<GLFWwindow*, int, void>)previous;
    }
    public static delegate*<GLFWwindow*, void> glfwSetWindowCloseCallback(GLFWwindow* w, delegate*<GLFWwindow*, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "WindowClose");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetWindowCloseCallback(window, address == 0 ? null : (ignored) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, void>)address)((GLFWwindow*)token)));
        return (delegate*<GLFWwindow*, void>)previous;
    }
    public static delegate*<GLFWwindow*, void> glfwSetWindowRefreshCallback(GLFWwindow* w, delegate*<GLFWwindow*, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "WindowRefresh");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetWindowRefreshCallback(window, address == 0 ? null : (ignored) =>
            Capture(token, ignored, () => ((delegate*<GLFWwindow*, void>)address)((GLFWwindow*)token)));
        return (delegate*<GLFWwindow*, void>)previous;
    }
    public static delegate*<GLFWwindow*, int, byte**, void> glfwSetDropCallback(GLFWwindow* w, delegate*<GLFWwindow*, int, byte**, void> callback)
    {
        var window = Window(w);
        var key = ((nint)w, "Drop");
        callbacks.TryGetValue(key, out var previous);
        nint address = (nint)callback, token = (nint)w;
        callbacks[key] = address;
        G.glfwSetDropCallback(window, address == 0 ? null : (ignored, count, paths) => Capture(token, ignored, () =>
        {
            byte** native = (byte**)NativeMemory.AllocZeroed((nuint)count, (nuint)sizeof(nint));
            try
            {
                for (int i = 0; i < count; i++) native[i] = (byte*)Marshal.StringToCoTaskMemUTF8(paths[i]);
                ((delegate*<GLFWwindow*, int, byte**, void>)address)((GLFWwindow*)token, count, native);
            }
            finally
            {
                for (int i = 0; i < count; i++) Marshal.FreeCoTaskMem((nint)native[i]);
                NativeMemory.Free(native);
            }
        }));
        return (delegate*<GLFWwindow*, int, byte**, void>)previous;
    }
}

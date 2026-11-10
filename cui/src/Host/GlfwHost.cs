#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using G = global::Glfw.Glfw;
using static Managed.Cui.Cui;

namespace Managed.Cui;

// Only the owner thread enters GLFW. Opaque identifiers are never dereferenced
// and are never reused, including across initialization cycles. The dictionaries
// root windows and their managed callback delegates until destruction completes.
internal static unsafe partial class GlfwHost
{
    private static readonly Dictionary<nint, object> objects = new();
    private static readonly Dictionary<object, nint> tokens = new(ReferenceEqualityComparer.Instance);
    private static readonly Dictionary<(nint, string), nint> callbacks = new();
    private static readonly Dictionary<nint, nint> videoModes = new();
    private static nint clipboard;
    private static int ownerThread;
    private static nint nextToken;
    private static int callbackDepth;
    private static bool terminating;
    private static ExceptionDispatchInfo? callbackError;

    internal static void CheckThread()
    {
        if (Volatile.Read(ref ownerThread) != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("CUI desktop calls must use the thread that initialized GLFW.");
    }

    private static void CheckOutsideCallback()
    {
        CheckThread();
        if (callbackDepth != 0)
            throw new InvalidOperationException("CUI cannot pump events or destroy GLFW resources inside a window callback.");
    }

    internal static void CheckContext()
    {
        CheckThread();
        if (G.glfwGetCurrentContext() is null)
            throw new InvalidOperationException("CUI OpenGL calls require a current context on the owner thread.");
    }

    private static string Text(byte* value) => value == null ? "" : Marshal.PtrToStringUTF8((nint)value)!;
    private static nint Token(object? value)
    {
        if (value is null) return 0;
        if (tokens.TryGetValue(value, out var existing)) return existing;
        nint token = checked(++nextToken);
        objects.Add(token, value);
        tokens.Add(value, token);
        return token;
    }

    private static T Get<T>(void* token) where T : class
    {
        CheckThread();
        return objects.TryGetValue((nint)token, out var obj) && obj is T typed
            ? typed : throw new InvalidOperationException("Invalid or released CUI GLFW handle.");
    }

    private static global::Glfw.GlfwWindow Window(GLFWwindow* token) => Get<global::Glfw.GlfwWindow>(token);
    private static void Release(void* token)
    {
        if (objects.Remove((nint)token, out var obj))
        {
            tokens.Remove(obj);
        }
    }

    private static void Capture(nint token, global::Glfw.GlfwWindow window, Action callback)
    {
        // Managed GLFW callbacks may be invoked by an unmanaged window procedure.
        // Store the error and surface it after the event pump has returned.
        try
        {
            if (Volatile.Read(ref ownerThread) == 0 || terminating) return;
            CheckThread();
            // Ignore an OS event retaining a callback from a destroyed window.
            if (!objects.TryGetValue(token, out var live) || !ReferenceEquals(live, window)) return;
            callbackDepth++;
            try { callback(); }
            finally { callbackDepth--; }
        }
        catch (Exception e) { Interlocked.CompareExchange(ref callbackError, ExceptionDispatchInfo.Capture(e), null); }
    }

    internal static void ThrowCallbackError()
    {
        Interlocked.Exchange(ref callbackError, null)?.Throw();
    }

    public static int glfwInit()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64 ||
            !(OperatingSystem.IsWindows() || OperatingSystem.IsLinux()))
            throw new PlatformNotSupportedException("CUI desktop supports Windows x64 and Linux x64/X11.");
        // A second CUI app must fail without changing the first app's GLFW state.
        if (Interlocked.CompareExchange(ref ownerThread, Environment.CurrentManagedThreadId, 0) != 0) return 0;
        try
        {
            // A raw cui_app_destroy may have run before its caller drained a
            // deferred callback error. Keep it visible at the next event pump.
            G.glfwInitHint(global::Glfw.GLFW.GLFW_PLATFORM,
                OperatingSystem.IsWindows() ? global::Glfw.GLFW.GLFW_PLATFORM_WIN32 : global::Glfw.GLFW.GLFW_PLATFORM_X11);
            int result = G.glfwInit();
            if (result == 0) Volatile.Write(ref ownerThread, 0);
            return result;
        }
        catch
        {
            try { G.glfwTerminate(); }
            finally { Volatile.Write(ref ownerThread, 0); }
            throw;
        }
    }

    public static void glfwTerminate()
    {
        // Like GLFW, termination after a failed initialization is harmless.
        if (Volatile.Read(ref ownerThread) == 0) return;
        CheckOutsideCallback();
        terminating = true;
        try { G.glfwTerminate(); }
        finally
        {
            foreach (var pointer in videoModes.Values) NativeMemory.Free((void*)pointer);
            videoModes.Clear();
            objects.Clear(); tokens.Clear(); callbacks.Clear();
            NativeMemory.Free((void*)clipboard); clipboard = 0;
            OpenGlHost.Reset();
            terminating = false;
            Volatile.Write(ref ownerThread, 0);
        }
        // Never throw a deferred callback error here: translated
        // cui_app_destroy still has C allocations to free after termination.
        // Event pumps and the owning wrapper's explicit drain surface it.
    }

    public static void glfwWindowHint(int hint, int value) { CheckThread(); G.glfwWindowHint(hint, value); }
    public static GLFWwindow* glfwCreateWindow(int width, int height, byte* title, GLFWmonitor* monitor, GLFWwindow* share)
    {
        CheckThread();
        var window = G.glfwCreateWindow(width, height, Text(title),
            monitor == null ? null : Get<global::Glfw.GlfwMonitor>(monitor), share == null ? null : Window(share));
        try { return (GLFWwindow*)Token(window); }
        catch { G.glfwDestroyWindow(window); throw; }
    }
    public static void glfwDestroyWindow(GLFWwindow* window)
    {
        CheckOutsideCallback();
        if (window == null) return;
        var managed = Window(window);
        try { G.glfwDestroyWindow(managed); }
        finally
        {
            managed.UserPointer = null;
            foreach (var key in new List<(nint, string)>(callbacks.Keys))
                if (key.Item1 == (nint)window) callbacks.Remove(key);
            Release(window);
            OpenGlHost.Reset();
        }
    }
    public static void glfwSetWindowUserPointer(GLFWwindow* window, void* user) => G.glfwSetWindowUserPointer(Window(window), (nint)user);
    public static void* glfwGetWindowUserPointer(GLFWwindow* window) => (void*)(nint)(G.glfwGetWindowUserPointer(Window(window)) ?? (nint)0);
    public static void glfwSetWindowSizeLimits(GLFWwindow* w, int a, int b, int c, int d) => G.glfwSetWindowSizeLimits(Window(w), a, b, c, d);
    public static void glfwSetWindowTitle(GLFWwindow* w, byte* title) => G.glfwSetWindowTitle(Window(w), Text(title));
    public static void glfwSetWindowSize(GLFWwindow* w, int width, int height) => G.glfwSetWindowSize(Window(w), width, height);
    public static void glfwShowWindow(GLFWwindow* w) => G.glfwShowWindow(Window(w));
    public static void glfwGetFramebufferSize(GLFWwindow* w, int* width, int* height)
    {
        G.glfwGetFramebufferSize(Window(w), out var x, out var y);
        if (width != null) *width = x;
        if (height != null) *height = y;
    }
    public static void glfwGetWindowContentScale(GLFWwindow* w, float* x, float* y)
    {
        G.glfwGetWindowContentScale(Window(w), out var sx, out var sy);
        if (x != null) *x = sx;
        if (y != null) *y = sy;
    }
    public static void glfwGetCursorPos(GLFWwindow* w, double* x, double* y)
    {
        G.glfwGetCursorPos(Window(w), out var px, out var py);
        if (x != null) *x = px;
        if (y != null) *y = py;
    }
    public static GLFWcursor* glfwCreateStandardCursor(int shape)
    {
        CheckThread();
        var cursor = G.glfwCreateStandardCursor(shape);
        try { return (GLFWcursor*)Token(cursor); }
        catch { G.glfwDestroyCursor(cursor); throw; }
    }
    public static void glfwDestroyCursor(GLFWcursor* cursor)
    {
        CheckOutsideCallback();
        if (cursor == null) return;
        var managed = Get<global::Glfw.GlfwCursor>(cursor);
        try { G.glfwDestroyCursor(managed); }
        finally { Release(cursor); }
    }
    public static void glfwSetCursor(GLFWwindow* w, GLFWcursor* cursor) => G.glfwSetCursor(Window(w), cursor == null ? null : Get<global::Glfw.GlfwCursor>(cursor));
    public static void glfwSetClipboardString(GLFWwindow* w, byte* text) => G.glfwSetClipboardString(Window(w), Text(text));
    public static byte* glfwGetClipboardString(GLFWwindow* w)
    {
        var text = G.glfwGetClipboardString(Window(w));
        NativeMemory.Free((void*)clipboard); clipboard = 0;
        if (text is null) return null;
        byte[] bytes = Encoding.UTF8.GetBytes(text + '\0');
        clipboard = (nint)NativeMemory.Alloc((nuint)bytes.Length);
        bytes.CopyTo(new Span<byte>((void*)clipboard, bytes.Length));
        return (byte*)clipboard;
    }
    public static void glfwMakeContextCurrent(GLFWwindow* w) { CheckThread(); G.glfwMakeContextCurrent(w == null ? null : Window(w)); }
    public static void glfwSwapInterval(int interval) { CheckThread(); G.glfwSwapInterval(interval); }
    public static void glfwSwapBuffers(GLFWwindow* w) => G.glfwSwapBuffers(Window(w));
    public static delegate*<void> glfwGetProcAddress(byte* name) { CheckThread(); return (delegate*<void>)OpenGlHost.GetProc(name); }
    public static double glfwGetTime() { CheckThread(); return G.glfwGetTime(); }
    public static void glfwPollEvents() { CheckOutsideCallback(); G.glfwPollEvents(); ThrowCallbackError(); }
    public static void glfwWaitEvents() { CheckOutsideCallback(); G.glfwWaitEvents(); ThrowCallbackError(); }
    public static void glfwWaitEventsTimeout(double timeout) { CheckOutsideCallback(); G.glfwWaitEventsTimeout(timeout); ThrowCallbackError(); }
    public static GLFWmonitor* glfwGetWindowMonitor(GLFWwindow* w) => (GLFWmonitor*)Token(G.glfwGetWindowMonitor(Window(w)));
    public static GLFWmonitor* glfwGetPrimaryMonitor() { CheckThread(); return (GLFWmonitor*)Token(G.glfwGetPrimaryMonitor()); }
    public static GLFWvidmode* glfwGetVideoMode(GLFWmonitor* monitor)
    {
        var mode = G.glfwGetVideoMode(Get<global::Glfw.GlfwMonitor>(monitor));
        if (mode is null) return null;
        if (!videoModes.TryGetValue((nint)monitor, out var pointer))
            videoModes.Add((nint)monitor, pointer = (nint)NativeMemory.Alloc((nuint)sizeof(GLFWvidmode)));
        var result = (GLFWvidmode*)pointer;
        var m = mode.Value;
        *result = new GLFWvidmode { width = m.Width, height = m.Height, redBits = m.RedBits,
            greenBits = m.GreenBits, blueBits = m.BlueBits, refreshRate = m.RefreshRate };
        return result;
    }
}

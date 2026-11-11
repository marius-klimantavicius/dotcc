using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using C = Managed.Forms.Cui;
using G = Glfw.Glfw;
using K = Glfw.GLFW;
using static Managed.Forms.Cui;

// Exercises actual emitted overrides and the translated OpenGL renderer.
// Both the full library and the focused build-probe.py output are supported.
internal static unsafe class Program
{
    private static int assertions;
    private static int callbackCalls;
    private static nint expectedWindow;
    private static Glfw.GlfwWindow? callbackWindow;
    private static int exceptionMode;
    private const string AppId = "org.example.CuiHost.λ🙂";
    [DllImport("libc", EntryPoint = "setenv", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SetEnvironment([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string value, int overwrite);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
    private static extern int XSendEvent(nint display, nuint window, int propagate, nint mask, void* ev);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
    private static extern int XSync(nint display, int discard);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
    private static extern int XGetClassHint(nint display, nuint window, out XClassHint hints);
    [DllImport("libX11.so.6", CallingConvention = CallingConvention.Cdecl)]
    private static extern int XFree(nint data);

    [StructLayout(LayoutKind.Sequential)]
    private struct XClassHint
    {
        public nint res_name, res_class;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XExpose
    {
        public int type;
        public nuint serial;
        public int sendEvent;
        public nint display;
        public nuint window;
        public int x, y, width, height, count;
    }

    private static void Assert(bool condition, string description)
    {
        assertions++;
        if (!condition) throw new Exception(description);
    }

    private static void Reject(Action action, string description)
    {
        try { action(); }
        catch (InvalidOperationException) { assertions++; return; }
        throw new Exception("Expected InvalidOperationException: " + description);
    }

    private static nint Proc(string name)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(name + '\0');
        fixed (byte* p = bytes) return (nint)C.glfwGetProcAddress(p);
    }

    private static GLFWwindow* CreateWindow()
    {
        C.glfwWindowHint(K.GLFW_CONTEXT_VERSION_MAJOR, 3);
        C.glfwWindowHint(K.GLFW_CONTEXT_VERSION_MINOR, 3);
        C.glfwWindowHint(K.GLFW_OPENGL_PROFILE, K.GLFW_OPENGL_CORE_PROFILE);
        C.glfwWindowHint(K.GLFW_VISIBLE, K.GLFW_FALSE);
        fixed (byte* title = "CUI host ABI λ"u8) return C.glfwCreateWindow(32, 32, title, null, null);
    }

    private static void SetAppIdHints()
    {
        // Match cui_app_glfw.c, then release/overwrite the caller's UTF-8
        // storage before window creation: the managed hint must own its text.
        Span<byte> utf8 = stackalloc byte[Encoding.UTF8.GetByteCount(AppId) + 1];
        Encoding.UTF8.GetBytes(AppId, utf8);
        utf8[^1] = 0;
        fixed (byte* value = utf8)
        {
            C.glfwWindowHintString(C.GLFW_X11_CLASS_NAME, value);
            C.glfwWindowHintString(C.GLFW_X11_INSTANCE_NAME, value);
            C.glfwWindowHintString(C.GLFW_WAYLAND_APP_ID, value);
        }
        utf8.Clear();
    }

    private static void WindowClass(Glfw.GlfwWindow window, string? appId)
    {
        if (!OperatingSystem.IsLinux()) return;
        int result = XGetClassHint(G.glfwGetX11Display(), G.glfwGetX11Window(window), out var hints);
        try
        {
            Assert(result != 0, "read actual X11 WM_CLASS");
            string? resourceName = Environment.GetEnvironmentVariable("RESOURCE_NAME");
            string? expectedName = appId ?? (string.IsNullOrEmpty(resourceName) ? window.Title : resourceName);
            Assert(Marshal.PtrToStringUTF8(hints.res_name) == expectedName, "WM_CLASS instance hint and initialization reset");
            Assert(Marshal.PtrToStringUTF8(hints.res_class) == (appId ?? window.Title), "WM_CLASS class hint and initialization reset");
        }
        finally
        {
            if (hints.res_name != 0) XFree(hints.res_name);
            if (hints.res_class != 0) XFree(hints.res_class);
        }
    }

    private static void Cursor(GLFWwindow* window, double x, double y)
    {
        Assert((nint)window == expectedWindow && x == 2.25 && y == -3.5, "cursor callback ABI");
        callbackCalls++;
    }
    private static void Enter(GLFWwindow* w, int entered) { Assert((nint)w == expectedWindow && entered == 1, "enter ABI"); callbackCalls++; }
    private static void Button(GLFWwindow* w, int b, int a, int m) { Assert((nint)w == expectedWindow && b == 2 && a == 1 && m == 5, "button ABI"); callbackCalls++; }
    private static void Scroll(GLFWwindow* w, double x, double y) { Assert((nint)w == expectedWindow && x == -0.5 && y == 1.25, "scroll ABI"); callbackCalls++; }
    private static void Key(GLFWwindow* w, int k, int s, int a, int m) { Assert((nint)w == expectedWindow && k == 65 && s == 38 && a == 1 && m == 4, "key ABI"); callbackCalls++; }
    private static void Char(GLFWwindow* w, uint cp) { Assert((nint)w == expectedWindow && cp == 0x1f642, "Unicode callback ABI"); callbackCalls++; }
    private static void Focus(GLFWwindow* w, int f) { Assert((nint)w == expectedWindow && f == 1, "focus ABI"); callbackCalls++; }
    private static void Close(GLFWwindow* w) { Assert((nint)w == expectedWindow, "close ABI"); callbackCalls++; }
    private static void Refresh(GLFWwindow* w)
    {
        if (exceptionMode == 1) throw new InvalidOperationException("callback sentinel");
        if (exceptionMode == 2) { C.glfwTerminate(); return; }
        Assert((nint)w == expectedWindow, "refresh ABI"); callbackCalls++;
    }
    private static void Drop(GLFWwindow* w, int count, byte** paths)
    {
        Assert((nint)w == expectedWindow && count == 2, "drop vector ABI");
        Assert(Marshal.PtrToStringUTF8((nint)paths[0]) == "/tmp/λ.txt" && Marshal.PtrToStringUTF8((nint)paths[1]) == "C:\\smile🙂.png", "drop UTF-8 ABI");
        callbackCalls++;
    }

    // If a managed exception escapes the host callback, this reverse P/Invoke
    // boundary terminates the test process instead of allowing a false pass.
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void NativeRefresh() => callbackWindow!.Callbacks.Refresh!(callbackWindow);

    private static void Callbacks(GLFWwindow* window, Glfw.GlfwWindow managed)
    {
        expectedWindow = (nint)window;
        delegate*<GLFWwindow*, double, double, void> cursorCallback = &Cursor;
        Assert(C.glfwSetCursorPosCallback(window, cursorCallback) == null, "initial cursor callback");
        Assert((nint)C.glfwSetCursorPosCallback(window, null) == (nint)cursorCallback, "callback previous pointer");
        Assert(C.glfwSetCursorPosCallback(window, &Cursor) == null && managed.Callbacks.CursorPos != null, "callback re-registration");
        C.glfwSetCursorEnterCallback(window, &Enter);
        C.glfwSetMouseButtonCallback(window, &Button);
        C.glfwSetScrollCallback(window, &Scroll);
        C.glfwSetKeyCallback(window, &Key);
        C.glfwSetCharCallback(window, &Char);
        C.glfwSetWindowFocusCallback(window, &Focus);
        C.glfwSetWindowCloseCallback(window, &Close);
        C.glfwSetWindowRefreshCallback(window, &Refresh);
        C.glfwSetDropCallback(window, &Drop);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        int before = callbackCalls;
        managed.Callbacks.CursorPos!(managed, 2.25, -3.5);
        managed.Callbacks.CursorEnter!(managed, 1);
        managed.Callbacks.MouseButton!(managed, 2, 1, 5);
        managed.Callbacks.Scroll!(managed, -0.5, 1.25);
        managed.Callbacks.Key!(managed, 65, 38, 1, 4);
        managed.Callbacks.Character!(managed, 0x1f642);
        managed.Callbacks.Focus!(managed, 1);
        managed.Callbacks.Close!(managed);
        managed.Callbacks.Refresh!(managed);
        managed.Callbacks.Drop!(managed, 2, new[] { "/tmp/λ.txt", "C:\\smile🙂.png" });
        Assert(callbackCalls == before + 10, "all callback delegates rooted and dispatched");
        // Poll surfaces any callback assertion only after returning to managed code.
        C.glfwPollEvents();
        callbackWindow = managed;
        exceptionMode = 1;
        ((delegate* unmanaged[Cdecl]<void>)&NativeRefresh)();
        Reject(C.glfwPollEvents, "callback exception deferred through native boundary");
        if (OperatingSystem.IsLinux())
        {
            // XEvent is 24 native longs on this selected x64 target.
            byte* ev = stackalloc byte[24 * 8];
            new Span<byte>(ev, 24 * 8).Clear();
            var expose = (XExpose*)ev;
            expose->type = 12; // Expose, from X11/X.h
            expose->display = G.glfwGetX11Display();
            expose->window = G.glfwGetX11Window(managed);
            expose->width = expose->height = 32;
            Assert(XSendEvent(expose->display, expose->window, 0, 1 << 15, ev) != 0, "send real X11 expose event");
            XSync(expose->display, 0);
            Reject(C.glfwPollEvents, "real OS callback exception deferred until event pump returns");
        }
        exceptionMode = 2;
        ((delegate* unmanaged[Cdecl]<void>)&NativeRefresh)();
        Reject(C.glfwPollEvents, "destruction inside callback rejected and contained");
        exceptionMode = 0;
        C.glfwPollEvents();
        callbackWindow = null;
    }

    private static void GlCalls()
    {
        var getString = (delegate*<uint, byte*>)Proc("glGetString");
        var clearColor = (delegate*<float, float, float, float, void>)Proc("glClearColor");
        var clear = (delegate*<uint, void>)Proc("glClear");
        var read = (delegate*<int, int, int, int, uint, uint, void*, void>)Proc("glReadPixels");
        var finish = (delegate*<void>)Proc("glFinish");
        Assert(getString != null && clearColor != null && clear != null && read != null && finish != null, "GL procs loaded");
        Console.WriteLine("GL_VERSION=" + Marshal.PtrToStringUTF8((nint)getString(0x1f02)));
        clearColor(0.25f, 0.5f, 0.75f, 1f);
        clear(0x4000);
        finish();
        byte* pixel = stackalloc byte[4];
        read(0, 0, 1, 1, 0x1908, 0x1401, pixel);
        Assert(Math.Abs(pixel[0] - 64) <= 1 && Math.Abs(pixel[1] - 128) <= 1 && Math.Abs(pixel[2] - 191) <= 1 && pixel[3] == 255, "float GL args and pixel readback");
        var gen = (delegate*<int, uint*, void>)Proc("glGenBuffers");
        var bind = (delegate*<uint, uint, void>)Proc("glBindBuffer");
        var data = (delegate*<uint, long, void*, uint, void>)Proc("glBufferData");
        var delete = (delegate*<int, uint*, void>)Proc("glDeleteBuffers");
        uint buffer;
        gen(1, &buffer); bind(0x8892, buffer); data(0x8892, 4L, pixel, 0x88e0); delete(1, &buffer);
        Assert(buffer != 0, "native GL buffer and pointer-sized size argument");
        // Diagnostic native query; glGetError is deliberately not exposed by
        // the reviewed CUI loader manifest.
        var getError = (delegate* unmanaged<uint>)G.glfwGetProcAddress("glGetError");
        Assert(getError() == 0, "GL calls accepted by native driver");
        var vertex = (delegate*<uint, int, uint, byte, int, void*, void>)Proc("glVertexAttribPointer");
        Assert(vertex != null, "byte GLboolean signature available");
        Assert(Proc("glNotInReviewedManifest") == 0 && Proc("glGetError") == 0, "unsupported GL procs never escape as unmanaged addresses");
        C.glfwMakeContextCurrent(null);
        Reject(() => clear(0x4000), "GL call without current context");
        Assert(Proc("glClear") == 0 && C.glfwGetProcAddress(null) == null, "loader fails without current context");
    }

    private static void Renderer()
    {
        Assert(C.cui_gl_create(null) == null, "null GL loader rejected");
        Assert(C.cui_gl_create(&MissingLoader) == null, "missing GL proc rejected before renderer initialization");
        var renderer = C.cui_gl_create(&Loader);
        Assert(renderer != null, "translated renderer shader compilation and initialization");
        var iface = C.cui_gl_iface(renderer);
        byte* pixels = stackalloc byte[4] { 255, 128, 64, 255 };
        ulong texture = iface.texture_create(iface.user, 1, 1, cui_texture_format.CUI_TEXTURE_RGBA8, pixels);
        Assert(texture != 0, "translated texture upload");
        iface.texture_update(iface.user, texture, 0, 0, 1, 1, pixels, 4);
        iface.texture_destroy(iface.user, texture);
        var draw = new cui_draw_data { width_px = 32, height_px = 32, clear_color = new cui_color { r = 64, g = 128, b = 191, a = 255 } };
        C.cui_gl_render(renderer, &draw, true);
        byte* readback = stackalloc byte[32 * 32 * 4];
        C.cui_gl_read_pixels(renderer, 32, 32, readback);
        Assert(Math.Abs(readback[0] - 64) <= 1 && Math.Abs(readback[1] - 128) <= 1 && Math.Abs(readback[2] - 191) <= 1 && readback[3] == 255, "translated renderer readback");
        cui_vertex* vertices = stackalloc cui_vertex[3];
        for (int i = 0; i < 3; i++) vertices[i] = new cui_vertex { mode = 4, color = new cui_color { r = 255, a = 255 } };
        vertices[0].x = 0; vertices[0].y = 0;
        vertices[1].x = 32; vertices[1].y = 0;
        vertices[2].x = 0; vertices[2].y = 32;
        uint* indices = stackalloc uint[3] { 0, 1, 2 };
        var command = new cui_draw_cmd { clip = new cui_rect { w = 32, h = 32 }, index_count = 3 };
        draw.vertices = vertices; draw.vertex_count = 3;
        draw.indices = indices; draw.index_count = 3;
        draw.commands = &command; draw.command_count = 1;
        C.cui_gl_render(renderer, &draw, true);
        C.cui_gl_read_pixels(renderer, 32, 32, readback);
        int offset = (4 * 32 + 4) * 4;
        Assert(readback[offset] == 255 && readback[offset + 1] == 0 && readback[offset + 2] == 0 && readback[offset + 3] == 255,
            "translated triangle draw exercises vertex attributes, GLboolean, uniforms, buffers and indices");
        offset = (28 * 32 + 28) * 4;
        Assert(Math.Abs(readback[offset] - 64) <= 1 && Math.Abs(readback[offset + 1] - 128) <= 1 && Math.Abs(readback[offset + 2] - 191) <= 1,
            "triangle coverage leaves exterior clear color intact");
        C.cui_gl_destroy(renderer);
    }
    private static void* Loader(byte* name) => (void*)C.glfwGetProcAddress(name);
    private static void* MissingLoader(byte* name) =>
        MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name).SequenceEqual("glBufferData"u8) ? null : Loader(name);

    public static int Main()
    {
        try
        {
            C.glfwTerminate();
            Assert(C.GLFW_X11_CLASS_NAME == K.GLFW_X11_CLASS_NAME &&
                C.GLFW_X11_INSTANCE_NAME == K.GLFW_X11_INSTANCE_NAME &&
                C.GLFW_WAYLAND_APP_ID == K.GLFW_WAYLAND_APP_ID, "emitted header and managed string-hint constants agree");
            Reject(SetAppIdHints, "string hints before initialization");
            string? display = Environment.GetEnvironmentVariable("DISPLAY");
            if (OperatingSystem.IsLinux())
            {
                Assert(SetEnvironment("DISPLAY", ":65530", 1) == 0, "set native X display environment");
                Assert(C.glfwInit() == 0, "initialization without an X display fails");
                C.glfwTerminate();
                Assert(SetEnvironment("DISPLAY", display ?? "", 1) == 0, "restore native X display environment");
            }
            nint lastWindow = 0;
            for (int cycle = 0; cycle < 2; cycle++)
            {
                Assert(C.glfwInit() == 1, "initialize/reinitialize GLFW");
                Assert(G.glfwGetPlatform() == (OperatingSystem.IsWindows() ? K.GLFW_PLATFORM_WIN32 : K.GLFW_PLATFORM_X11),
                    "managed GLFW runtime platform dispatch");
                Assert(C.glfwInit() == 0, "second active app rejected on owner thread");
                int duplicate = Task.Run(C.glfwInit).GetAwaiter().GetResult();
                Assert(duplicate == 0, "second active app rejected on another thread");
                Task.Run(() => Reject(() => C.glfwGetTime(), "wrong-thread GLFW call")).GetAwaiter().GetResult();
                Task.Run(() => Reject(SetAppIdHints, "wrong-thread string hint")).GetAwaiter().GetResult();
                fixed (byte* title = "invalid"u8) Assert(C.glfwCreateWindow(0, 0, title, null, null) == null, "failed window creation");
                G.glfwGetError(out _); // Drain the intentional invalid-window error.
                if (cycle == 0) SetAppIdHints();
                Assert(G.glfwGetError(out _) == K.GLFW_NO_ERROR, "all three upstream string hints accepted on selected platform");
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                var window = CreateWindow();
                Assert(window != null && (nint)window != lastWindow, "new live opaque window identifier");
                if (lastWindow != 0) Reject(() => C.glfwGetWindowUserPointer((GLFWwindow*)lastWindow), "released handle cannot revive");
                C.glfwSetWindowUserPointer(window, (void*)0x12345678);
                Assert(C.glfwGetWindowUserPointer(window) == (void*)0x12345678, "opaque CUI user pointer");
                fixed (byte* text = "clipboard λ🙂"u8) C.glfwSetClipboardString(window, text);
                Assert(Marshal.PtrToStringUTF8((nint)C.glfwGetClipboardString(window)) == "clipboard λ🙂", "clipboard UTF-8 round trip");
                Assert(Marshal.PtrToStringUTF8((nint)C.glfwGetClipboardString(window)) == "clipboard λ🙂", "clipboard replacement buffer lifetime");
                int width, height;
                C.glfwGetFramebufferSize(window, &width, &height);
                Assert(width == 32 && height == 32, "framebuffer int output pointers");
                float sx, sy;
                C.glfwGetWindowContentScale(window, &sx, &sy);
                Assert(sx > 0 && sy > 0, "content-scale float output pointers");
                double x, y;
                C.glfwGetCursorPos(window, &x, &y);
                Assert(double.IsFinite(x) && double.IsFinite(y), "cursor double output pointers");
                var monitor = C.glfwGetPrimaryMonitor();
                var mode = C.glfwGetVideoMode(monitor);
                Assert(sizeof(GLFWvidmode) == 24 && Marshal.OffsetOf<GLFWvidmode>(nameof(GLFWvidmode.refreshRate)) == 20, "emitted video mode ABI");
                Assert(mode != null && mode->width > 0 && mode->height > 0 && mode == C.glfwGetVideoMode(monitor), "video mode pointer lifetime");
                var cursor = C.glfwCreateStandardCursor(K.GLFW_ARROW_CURSOR);
                Assert(cursor != null, "cursor creation");
                C.glfwSetCursor(window, cursor);
                C.glfwDestroyCursor(cursor);
                Reject(() => C.glfwSetCursor(window, cursor), "released cursor rejected");
                C.glfwMakeContextCurrent(window);
                var managed = G.glfwGetCurrentContext()!;
                Assert(managed != null, "copied managed GLFW context is current");
                WindowClass(managed!, cycle == 0 ? AppId : null);
                nint ownerClear = Proc("glClear");
                Task.Run(() => Reject(() => ((delegate*<uint, void>)ownerClear)(0x4000), "wrong-thread GL thunk call")).GetAwaiter().GetResult();
                Task.Run(() => Reject(C.glfwTerminate, "wrong-thread termination")).GetAwaiter().GetResult();
                Callbacks(window, managed!);
                var retained = managed!.Callbacks.Refresh!;
                GlCalls();
                C.glfwMakeContextCurrent(window);
                Renderer();
                var stale = (delegate*<uint, void>)Proc("glClear");
                C.glfwDestroyWindow(window);
                retained(managed);
                C.glfwPollEvents();
                Reject(() => C.glfwGetWindowUserPointer(window), "released window rejected");
                Reject(() => stale(0x4000), "cached thunk after window destruction");
                // A new context must reload every proc; stale native addresses
                // from the old context must not be reused implicitly.
                var replacement = CreateWindow();
                C.glfwMakeContextCurrent(replacement);
                WindowClass(G.glfwGetCurrentContext()!, cycle == 0 ? AppId : null);
                Reject(() => stale(0x4000), "cached thunk after replacement context");
                ((delegate*<uint, void>)Proc("glClear"))(0x4000);
                lastWindow = (nint)window;
                // Termination must release live windows, cursors and callbacks.
                C.glfwTerminate();
                Reject(SetAppIdHints, "string hints after termination");
                retained(managed);
                C.glfwTerminate();
                Reject(() => stale(0x4000), "cached thunk after termination");
                Assert(callbackCalls == (cycle + 1) * 10, "released callbacks ignored");
            }
            Console.WriteLine($"PASS {assertions} assertions; host and translated GL renderer");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Managed.Forms;
using G = Glfw.Glfw;
using static Managed.Forms.Cui;

// Full translated app-loop qualification. Only the OS driver injects input;
// no cui_input_* calls, synthetic widget state, or direct input callbacks.
internal static unsafe class Program
{
    private static int assertions;
    private const string Sentinel = "desktop callback sentinel";
    private static delegate*<void*, void*, void> originalFree;
    private static nint watchedUi;
    private static int uiFrees;

    private static void WatchUiRelease(cui_ctx* ui)
    {
        originalFree = ui->alloc.free;
        watchedUi = (nint)ui;
        uiFrees = 0;
        ui->alloc.free = &ObserveFree;
    }

    private static void ObserveFree(void* pointer, void* user)
    {
        if ((nint)pointer == watchedUi) uiFrees++;
        originalFree(pointer, user);
    }

    private static void Check(bool condition, string description)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(description);
    }

    private sealed class FrameState
    {
        internal readonly byte[] Text = new byte[128];
        internal int Frames, Clicks, Changed, Submitted;
        internal cui_rect Button, Input;
        internal string Value => Encoding.UTF8.GetString(Text.AsSpan(0, Array.IndexOf(Text, (byte)0)));

        internal void Build(DesktopApplication app)
        {
            var ui = app.Ui;
            fixed (byte* button = "OS click target"u8)
            fixed (byte* input = "os-input"u8)
            fixed (byte* text = Text)
            {
                cui_column_begin(ui, null);
                if (cui_button_ex(ui, button, null) != 0) Clicks++;
                Button = cui_last_rect(ui);
                int flags = cui_input_ex(ui, input, text, (ulong)Text.Length, null);
                Input = cui_last_rect(ui);
                if ((flags & CUI_CHANGED) != 0) Changed++;
                if ((flags & CUI_SUBMITTED) != 0) Submitted++;
                cui_box_end(ui);
            }
            Frames++;
        }

        internal bool Frame(DesktopApplication app)
        {
            if (!app.NextFrame()) return false;
            Build(app);
            return true;
        }

        internal void Pump(DesktopApplication app, Func<bool> ready, string description)
        {
            var deadline = Stopwatch.StartNew();
            do
            {
                Check(Frame(app), $"Window closed while waiting for {description}");
                if (ready()) return;
                if (deadline.Elapsed > TimeSpan.FromSeconds(8))
                    throw new TimeoutException($"{description}: text='{Value}', clicks={Clicks}, frames={Frames}, button={Button.x},{Button.y},{Button.w},{Button.h}, input={Input.x},{Input.y},{Input.w},{Input.h}");
                Thread.Sleep(5);
            } while (true);
        }

        internal void Settle(DesktopApplication app)
        {
            // Give press and release distinct widget frames. This also makes
            // duplicate activations observable before the next test step.
            for (int i = 0; i < 3; i++)
            {
                Check(Frame(app), "App remains open");
                Thread.Sleep(10);
            }
        }
    }

    private static void Click(DesktopApplication app, FrameState state, OsInput input, cui_rect rect)
    {
        input.Move(rect.x + rect.w / 2, rect.y + rect.h / 2);
        state.Settle(app);
        input.Button(true);
        state.Settle(app);
        input.Button(false);
        state.Settle(app);
    }

    private static void Key(DesktopApplication app, FrameState state, OsInput input, InputKey key)
    {
        input.Key(key, true);
        state.Settle(app);
        input.Key(key, false);
        state.Settle(app);
    }

    private static void RunCycle(int cycle)
    {
        using var app = new DesktopApplication($"CUI OS input {Environment.ProcessId} cycle {cycle}",
            640, 480, continuous: true);
        var state = new FrameState();
        using var input = OsInput.Create();
        input.Focus();
        state.Pump(app, () => state.Frames >= 3 && state.Button.w > 0 && state.Input.w > 0,
            "widget layout");
        Check(state.Clicks == 0 && state.Value == "", "Fresh widget state each lifecycle");

        Click(app, state, input, state.Button);
        state.Pump(app, () => state.Clicks == 1, "OS pointer activates button");
        Check(state.Clicks == 1, "Exactly one pointer activation");
        Click(app, state, input, state.Input);
        input.Text("hello");
        state.Pump(app, () => input.Idle && state.Value == "hello", "OS text reaches translated input");
        Key(app, state, input, InputKey.Left);
        Key(app, state, input, InputKey.Backspace);
        state.Pump(app, () => state.Value == "helo", "left/backspace editing");
        Key(app, state, input, InputKey.Home);
        Key(app, state, input, InputKey.Delete);
        state.Pump(app, () => state.Value == "elo", "home/delete editing");
        Key(app, state, input, InputKey.End);
        input.Text("!");
        state.Pump(app, () => input.Idle && state.Value == "elo!", "end/text editing");

        input.Key(InputKey.Control, true);
        Key(app, state, input, InputKey.A);
        input.Key(InputKey.Control, false);
        state.Settle(app);
        input.Text("CUI λ");
        state.Pump(app, () => input.Idle && state.Value == "CUI λ", "Ctrl+A and Unicode replacement");
        Check(state.Changed >= 5, "Input reports changes");
        Key(app, state, input, InputKey.Enter);
        state.Pump(app, () => state.Submitted == 1, "Enter submits input");

        // Resize through the OS, then test widgets again using their new layout.
        input.Resize(800, 560);
        state.Pump(app, () =>
        {
            app.CapturePixels(out int w, out int h);
            return w == 800 && h == 560;
        }, "OS resize reaches framebuffer");
        state.Settle(app);
        Click(app, state, input, state.Button);
        state.Pump(app, () => state.Clicks == 2, "button works after resize");
        Check(state.Clicks == 2 && state.Submitted == 1 && state.Value == "CUI λ",
            "Final widget state after resize");

        byte[] pixels = app.CapturePixels(out int width, out int height);
        int different = 0;
        for (int i = 4; i < pixels.Length; i += 4)
            if (!pixels.AsSpan(i, 3).SequenceEqual(pixels.AsSpan(0, 3))) different++;
        Check(width == 800 && height == 560 && different > 100,
            "Actual OpenGL framebuffer contains UI after input and resize");
        input.Close();
        var closeDeadline = Stopwatch.StartNew();
        while (state.Frame(app))
        {
            if (closeDeadline.Elapsed > TimeSpan.FromSeconds(8))
                throw new TimeoutException("OS window-close did not end translated app loop");
            Thread.Sleep(5);
        }
        Check(state.Clicks == 2, "No duplicate button activation");
        Console.WriteLine($"PASS OS-input cycle {cycle}: clicks={state.Clicks}, text='{state.Value}', changed={state.Changed}, submitted={state.Submitted}, framebuffer={width}x{height}, differing-pixels={different}, frames={state.Frames}, OS-close");
        app.Dispose();
        app.Dispose();
    }

    private static void ThrowRefresh(GLFWwindow* ignored) => throw new InvalidOperationException(Sentinel);

    private static void ExpectSentinel(Action action, string description)
    {
        try { action(); }
        catch (InvalidOperationException error) when (error.Message == Sentinel)
        {
            assertions++;
            return;
        }
        throw new Exception("Callback error was lost: " + description);
    }

    private static void CallbackCleanup()
    {
        // Actual OS refresh -> copied GLFW -> host Capture. The normal translated
        // pump must report the stored exception after the OS callback returns.
        using (var app = new DesktopApplication("CUI callback pump", continuous: true))
        using (var input = OsInput.Create())
        {
            var state = new FrameState();
            state.Settle(app);
            glfwSetWindowRefreshCallback((GLFWwindow*)cui_app_native_window(null), &ThrowRefresh);
            input.Refresh();
            ExpectSentinel(() => app.NextFrame(), "owning event pump");
            glfwSetWindowRefreshCallback((GLFWwindow*)cui_app_native_window(null), null);
            Check(state.Frame(app), "App survives handled callback error");
        }

        // Deliberately use the copied GLFW pump here to leave a deferred error
        // undrained, then call raw C destruction. This regression isolates the
        // exception that previously skipped cui_destroy(ui) and free(app).
        var description = new cui_app_desc
        {
            width = 640, height = 480, continuous = true,
            renderer = cui_renderer_kind.CUI_RENDERER_OPENGL
        };
        var raw = cui_app_create(&description);
        Check(raw != null, "Raw app created for teardown regression");
        WatchUiRelease(raw->ui);
        try
        {
            using var input = OsInput.Create();
            glfwSetWindowRefreshCallback((GLFWwindow*)cui_app_native_window(raw), &ThrowRefresh);
            input.Refresh();
            G.glfwPollEvents(); // capture without host error drain
            var released = raw;
            raw = null;
            cui_app_destroy(released); // must finish all C cleanup without throwing
            Check(uiFrees == 1, "Raw destruction reaches C UI allocation release after GLFW termination");
            Check(G.glfwGetCurrentContext() is null, "Raw destruction releases GLFW context");
        }
        finally { if (raw != null) cui_app_destroy(raw); }
        using (var retry = new DesktopApplication("CUI callback retained", continuous: true))
        {
            ExpectSentinel(() => retry.NextFrame(), "error retained across raw destruction/reinit");
            Check(retry.NextFrame(), "Reinitialized app continues after draining error");
        }

        using (var app = new DesktopApplication("CUI callback dispose", continuous: true))
        using (var input = OsInput.Create())
        {
            WatchUiRelease(app.Ui);
            glfwSetWindowRefreshCallback((GLFWwindow*)cui_app_native_window(null), &ThrowRefresh);
            input.Refresh();
            G.glfwPollEvents();
            ExpectSentinel(app.Dispose, "owning disposal reports error after releasing app");
            Check(uiFrees == 1, "Throwing owning disposal still releases C UI allocation");
            app.Dispose();
        }
        using var final = new DesktopApplication("CUI cleanup retry", continuous: true);
        Check(final.NextFrame(), "App creation after throwing owning disposal");
        Console.WriteLine("PASS callback cleanup: OS callback error at event pump, raw destruction completes with error retained, owning disposal throws and releases, reinitialization");
    }

    private static int Main()
    {
        try
        {
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("This runner requires X11 or a Windows desktop.");
            for (int cycle = 1; cycle <= 3; cycle++) RunCycle(cycle);
            CallbackCleanup();
            Console.WriteLine($"PASS desktop input: {assertions} assertions; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}

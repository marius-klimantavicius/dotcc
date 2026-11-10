using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Managed.Cui;
using static Managed.Cui.Cui;
using static Managed.Cui.Gallery.Gallery;
using G = Glfw.Glfw;

internal sealed record PageResult(int Page, string Name, int Nodes, int MainText, uint TextHash, int Different);
internal sealed record GalleryReport(string Status, int Assertions, System.Collections.Generic.List<PageResult> Pages,
    System.Collections.Generic.List<string> Actions, string Platform, string Architecture, string Windows, string Cleanup);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(GalleryReport))]
internal partial class GalleryJsonContext : JsonSerializerContext { }

// Calls the translated original gallery. Inputs travel through XTEST -> GLFW
// -> translated app/widgets. State is observed, never synthesized for actions.
internal static unsafe class Program
{
    private static int assertions;
    private static readonly List<PageResult> pages = [];
    private static readonly List<string> actions = [];
    private static delegate*<void*, void*, void> originalFree;
    private static nint watchedUi;
    private static int frees;

    private static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void ObserveFree(void* pointer, void* user)
    {
        if ((nint)pointer == watchedUi) frees++;
        originalFree(pointer, user);
    }

    private static void Watch(DesktopApplication app)
    {
        watchedUi = (nint)app.Ui;
        originalFree = app.Ui->alloc.free;
        frees = 0;
        app.Ui->alloc.free = &ObserveFree;
    }

    private static string Utf8(byte* value) => Marshal.PtrToStringUTF8((nint)value) ?? "";
    private static string Name(int page) => Utf8(gallery_page_name(page));

    private sealed class Session : IDisposable
    {
        internal readonly DesktopApplication App = new("CUI upstream gallery qualification", 1240, 820, continuous: true,
            theme: cui_theme_mode.CUI_THEME_LIGHT, accent: cui_accent.CUI_ACCENT_NEUTRAL);
        internal gallery State;
        internal int Frames;
        internal Session()
        {
            fixed (gallery* g = &State) gallery_init(g);
            cui_set_theme(App.Ui, cui_theme_mode.CUI_THEME_LIGHT);
            cui_set_accent(App.Ui, cui_accent.CUI_ACCENT_NEUTRAL);
            Check(cui_get_theme_mode(App.Ui) == cui_theme_mode.CUI_THEME_LIGHT &&
                cui_get_accent(App.Ui) == cui_accent.CUI_ACCENT_NEUTRAL && App.Ui->theme_t == 1,
                "Gallery starts with settled light/neutral palette");
            Watch(App);
        }

        internal bool Frame(bool deterministic = false)
        {
            if (!App.NextFrame()) return false;
            if (deterministic) { App.Ui->time = 10; App.Ui->dt = 1f / 60; }
            fixed (gallery* g = &State) gallery_frame(App.Ui, g);
            // Complete layout before querying node rectangles or injecting input.
            App.Render();
            Frames++;
            return true;
        }

        internal void Settle(int count = 4, bool deterministic = false)
        {
            for (int i = 0; i < count; i++)
            {
                Check(Frame(deterministic), "Gallery stays open");
                if (!deterministic) Thread.Sleep(5);
            }
        }

        internal void Until(Func<bool> ready, string message)
        {
            var timer = Stopwatch.StartNew();
            do
            {
                Settle(1);
                if (ready()) { actions.Add(message); Console.WriteLine("PASS " + message); return; }
            } while (timer.Elapsed < TimeSpan.FromSeconds(8));
            throw new TimeoutException($"{message}; page={State.page} ({Name(State.page)}), frames={Frames}, main-scroll={ScrollY("main")}");
        }

        internal uint Id(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value + '\0');
            fixed (byte* p = bytes) return cui_make_id(App.Ui, p);
        }

        internal cui_node* Node(uint id)
        {
            var ui = App.Ui;
            for (int i = 0; i < ui->node_count; i++) if (ui->nodes[i].id == id) return &ui->nodes[i];
            throw new InvalidOperationException($"No node for id {id}");
        }
        internal cui_rect Rect(string value) => Node(Id(value))->rect;
        internal float ScrollY(string value) => Node(Id(value))->scroll_y;
        internal bool HasBlocker()
        {
            for (int i = 0; i < App.Ui->node_count; i++)
                if ((App.Ui->nodes[i].flags & CUI_NF_BLOCKER) != 0) return true;
            return false;
        }

        internal cui_rect Text(string text, bool sidebar = false)
        {
            var ui = App.Ui;
            for (int i = 0; i < ui->node_count; i++)
            {
                var n = &ui->nodes[i];
                if (n->text == null || (n->rect.x < 240) != sidebar) continue;
                if (Encoding.UTF8.GetString(new ReadOnlySpan<byte>(n->text, n->text_len)) == text) return n->rect;
            }
            throw new InvalidOperationException($"No {(sidebar ? "sidebar" : "content")} text '{text}' on {Name(State.page)}");
        }

        public void Dispose()
        {
            App.Dispose();
            App.Dispose();
            Check(frees == 1, "Gallery UI allocation released exactly once");
            Check(G.glfwGetCurrentContext() is null, "Gallery GLFW context released");
            Check(G.glfwGetX11Display() == 0, "Gallery X11 display released");
        }
    }

    private static int Different(byte[] pixels)
    {
        int count = 0;
        for (int y = 0; y < 820; y++)
            for (int x = 261; x < 1240; x++)
                if (!pixels.AsSpan((y * 1240 + x) * 4, 3).SequenceEqual(pixels.AsSpan((y * 1240 + 260) * 4, 3))) count++;
        return count;
    }

    private static void CapturePages(string directory)
    {
        using var s = new Session();
        Check(gallery_page_total() == 27, "Original upstream gallery exposes all 27 pages");
        var contentHashes = new HashSet<string>();
        for (int page = 0; page < 27; page++)
        {
            s.State.page = page; // only deterministic captures select directly; OS selection is tested separately
            fixed (byte* main = "main"u8) cui_scroll_to(s.App.Ui, main, 0);
            s.Settle(deterministic: true);
            byte[] pixels = s.App.CapturePixels(out int width, out int height);
            Check(width == 1240 && height == 820, $"Page {page} framebuffer size");
            uint hash = 2166136261;
            int text = 0;
            var labels = new StringBuilder();
            var ui = s.App.Ui;
            for (int i = 0; i < ui->node_count; i++)
            {
                var node = &ui->nodes[i];
                if (node->text == null || node->rect.x < 260) continue;
                text++;
                labels.Append(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(node->text, node->text_len))).Append('\n');
                for (int j = 0; j < node->text_len; j++) hash = unchecked((hash ^ node->text[j]) * 16777619);
                hash = unchecked((hash ^ 0xff) * 16777619);
            }
            int different = Different(pixels);
            Check(text >= 3 && different >= 1000, $"Page {page} has rendered main content beyond sidebar");
            Check(contentHashes.Add(hash.ToString()), $"Page {page} has distinct main-content text");
            File.WriteAllBytes(Path.Combine(directory, $"page-{page:00}.rgba"), pixels);
            File.WriteAllText(Path.Combine(directory, $"page-{page:00}.text"), labels.ToString());
            pages.Add(new PageResult(page, Name(page), ui->node_count, text, hash, different));
            Console.WriteLine($"PAGE\t{page}\t{Name(page)}\t{ui->node_count}\t{text}\t{hash}\t{different}");
        }
    }

    private static void Xdotool(params string[] arguments)
    {
        var start = new ProcessStartInfo("xdotool") { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string arg in arguments) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("xdotool launch failed");
        if (!process.WaitForExit(10000)) { process.Kill(true); throw new TimeoutException("xdotool"); }
        if (process.ExitCode != 0) throw new InvalidOperationException(process.StandardError.ReadToEnd());
    }

    private static void Click(Session s, OsInput input, cui_rect r, float x = .5f, float y = .5f)
    {
        Check(r.w > 0 && r.h > 0 && r.y >= 0 && r.y + r.h <= s.App.Ui->vh,
            $"Clickable rectangle is visible: {r.x},{r.y},{r.w},{r.h}");
        Move(s, r.x + r.w * x, r.y + r.h * y);
        input.Button(true); s.Settle(2);
        input.Button(false); s.Settle(3);
    }

    private static void Move(Session s, float x, float y)
    {
        var window = G.glfwGetCurrentContext() ?? throw new InvalidOperationException("No GL window");
        G.glfwGetWindowContentScale(window, out float scale, out _);
        // xdotool --sync can wait forever when the pointer is already at the
        // requested position. Pumping distinct frames provides synchronization.
        Xdotool("mousemove", "--window", G.glfwGetX11Window(window).ToString(),
            ((int)MathF.Round(x * scale)).ToString(), ((int)MathF.Round(y * scale)).ToString());
        s.Settle(2);
    }

    private static void Key(Session s, string key)
    {
        Xdotool("keydown", key); s.Settle(2);
        Xdotool("keyup", key); s.Settle(3);
    }

    private static void Wheel(Session s, OsInput input, bool sidebar, bool down, int clicks = 3)
    {
        var r = s.Rect(sidebar ? "nav" : "main");
        Move(s, r.x + r.w * .7f, r.y + r.h * .5f);
        for (int i = 0; i < clicks; i++) { Xdotool("click", down ? "5" : "4"); s.Settle(2); }
        s.Settle(12);
    }

    private static void Select(Session s, OsInput input, int page)
    {
        for (int i = 0; i < 30; i++)
        {
            var item = s.Text(Name(page), sidebar: true);
            var nav = s.Rect("nav");
            if (item.y >= nav.y + 2 && item.y + item.h <= nav.y + nav.h - 2)
            {
                Click(s, input, item);
                s.Until(() => s.State.page == page && s.ScrollY("main") < 1,
                    $"OS sidebar selects page {page}: {Name(page)} and scrolls to top");
                return;
            }
            Wheel(s, input, true, item.y + item.h > nav.y + nav.h - 2, 1);
        }
        throw new TimeoutException("Sidebar cannot reveal " + Name(page));
    }

    private static void Reveal(Session s, OsInput input, string label)
    {
        for (int i = 0; i < 30; i++)
        {
            var r = s.Text(label);
            if (r.y >= 12 && r.y + r.h < s.App.Ui->vh - 20) return;
            Wheel(s, input, false, r.y + r.h >= s.App.Ui->vh - 20, 1);
        }
        throw new TimeoutException("Cannot reveal " + label);
    }

    private static void Interact()
    {
        using var s = new Session();
        s.Settle();
        using var input = OsInput.Create();
        input.Focus();
        Select(s, input, 1); // ensure the Overview click must actually change state
        for (int page = 0; page < 27; page++)
        {
            Select(s, input, page);
            byte[] pixels = s.App.CapturePixels(out _, out _);
            Check(Different(pixels) >= 1000, $"OS selected page {page} renders nonblank main content");
        }

        Select(s, input, 3);
        Check(s.ScrollY("main") < 1, "Sidebar selection resets main scroll to top");
        Check(s.State.newsletter != 0 && s.State.bluetooth == 0 && s.State.terms == 0, "Original selection defaults");
        Click(s, input, s.Text("Email notifications"));
        s.Until(() => s.State.newsletter == 0, "Checkbox changes upstream newsletter state");
        Click(s, input, s.State.bluetooth_rect);
        s.Until(() => s.State.bluetooth != 0, "Switch changes upstream Bluetooth state");
        Click(s, input, s.Text("Disabled option"));
        Check(s.State.terms == 0, "Disabled checkbox preserves upstream state");
        Click(s, input, s.Text("Only when I save"));
        s.Until(() => s.State.plan == 2, "Radio changes upstream plan value");
        Wheel(s, input, false, true, 5);
        Check(s.ScrollY("main") > 40, "OS wheel scrolls main gallery content");
        Reveal(s, input, "Volume");
        var slider = s.Rect("Volume");
        Click(s, input, slider, .85f, .8f);
        s.Until(() => s.State.volume > 75 && s.State.volume <= 100, "Slider pointer updates upstream volume");

        Select(s, input, 2);
        Check(s.ScrollY("main") < 1, "Navigation resets previously scrolled content");
        Click(s, input, s.Rect("name"));
        input.Key(InputKey.Control, true); Key(s, "a"); input.Key(InputKey.Control, false); s.Settle();
        input.Text("Gallery test");
        s.Until(() => input.Idle && StateName(s) == "Gallery test", "Text input replaces upstream name buffer");

        Select(s, input, 1);
        Reveal(s, input, "Save changes");
        Click(s, input, s.Text("Save changes"));
        s.Until(() => s.State.loading != 0, "Button sets upstream loading state");
        Click(s, input, s.Text("Reset"));
        s.Until(() => s.State.loading == 0, "Reset button clears upstream loading state");

        Select(s, input, 4);
        Click(s, input, s.Text("Analytics"));
        s.Until(() => s.State.tab == 1, "Tabs change upstream selected tab");
        Click(s, input, s.Text("Next"));
        s.Until(() => s.State.step == 2, "Stepper Next changes upstream step");
        Click(s, input, s.Text("Back"));
        s.Until(() => s.State.step == 1, "Stepper Back restores upstream step");

        Select(s, input, 5);
        Click(s, input, s.Text("Edit profile"));
        s.Until(() => s.State.dialog_open != 0, "Dialog button opens upstream modal");
        Key(s, "Escape");
        s.Until(() => s.State.dialog_open == 0 && s.App.Ui->modal_layer == 0 && s.App.Ui->prev_modal_layer == 0 && !s.HasBlocker(),
            "Escape closes upstream modal and releases input layer");

        Select(s, input, 9);
        Click(s, input, s.Text("Save"));
        s.Until(() => s.Text("Saved").w > 0, "Extended Actions split button renders saved result");

        Select(s, input, 1);
        byte[] light = s.App.CapturePixels(out _, out _);
        Click(s, input, s.Text("Dark", sidebar: true));
        s.Until(() => s.State.dark != 0 && cui_get_theme_mode(s.App.Ui) == cui_theme_mode.CUI_THEME_DARK && s.App.Ui->theme_t == 1,
            "Sidebar theme control updates gallery and core theme");
        byte[] dark = s.App.CapturePixels(out _, out _);
        Check(!light.AsSpan().SequenceEqual(dark), "Theme changes actual GL pixels");
        Click(s, input, s.Rect("accent-3"));
        s.Until(() => s.State.accent == 3 && cui_get_accent(s.App.Ui) == cui_accent.CUI_ACCENT_GREEN && s.App.Ui->theme_t == 1,
            "Accent swatch updates gallery and core accent");
        byte[] green = s.App.CapturePixels(out _, out _);
        Check(!dark.AsSpan().SequenceEqual(green), "Accent changes actual GL pixels");
        input.Key(InputKey.Control, true); Key(s, "j"); input.Key(InputKey.Control, false); s.Settle();
        s.Until(() => s.State.dark == 0 && cui_get_theme_mode(s.App.Ui) == cui_theme_mode.CUI_THEME_LIGHT && s.App.Ui->theme_t == 1,
            "Ctrl+J shortcut changes upstream theme");

        Select(s, input, 3);
        Check(s.State.newsletter == 0 && s.State.bluetooth != 0 && s.State.plan == 2 && s.State.volume > 75,
            "Selection state persists across pages");
        Check(StateName(s) == "Gallery test", "Edited text persists across pages");
        input.Close();
        var deadline = Stopwatch.StartNew();
        while (s.Frame())
            if (deadline.Elapsed > TimeSpan.FromSeconds(8)) throw new TimeoutException("OS close did not end gallery app loop");
        actions.Add("OS close ends upstream gallery loop");
    }

    private static string StateName(Session s) { fixed (byte* p = s.State.name) return Utf8(p); }

    private static int Main(string[] args)
    {
        try
        {
            if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Gallery OS qualification currently executes Linux/X11 only; Windows remains unrun.");
            if (args.Length != 1) throw new ArgumentException("Expected capture/report directory");
            Directory.CreateDirectory(args[0]);
            CapturePages(args[0]);
            Interact();
            // A fresh process-local lifecycle after every upstream page has run.
            using (var retry = new Session())
            {
                retry.Settle();
                Check(retry.State.page == 0 && retry.State.bluetooth == 0 && retry.State.newsletter != 0,
                    "Reinitialized base gallery has fresh state");
                Check(StateName(retry) == "Maya Okafor", "Reinitialized text has original upstream default");
            }
            File.WriteAllText(Path.Combine(args[0], "report.json"), JsonSerializer.Serialize(
                new GalleryReport("passed", assertions, pages, actions, RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture.ToString(),
                    "unrun", "UI allocation freed once per lifecycle, GLFW context/display released, reinitialized"),
                GalleryJsonContext.Default.GalleryReport));
            Console.WriteLine($"PASS translated upstream gallery: 27 captures, 27 OS selections, {actions.Count} state-asserted actions, cleanup; {assertions} assertions");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}

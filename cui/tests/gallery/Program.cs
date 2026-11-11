using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Managed.Forms;
using static Managed.Forms.Cui;
using static Managed.Forms.Gallery.Gallery;
using G = Glfw.Glfw;

internal sealed record PageResult(int Page, string Name, int Nodes, int MainText, uint TextHash, int Different);
internal sealed record GalleryReport(string Status, int Assertions, System.Collections.Generic.List<PageResult> Pages,
    System.Collections.Generic.List<string> Actions, string Platform, string Architecture, string Windows, string Cleanup);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(GalleryReport))]
internal partial class GalleryJsonContext : JsonSerializerContext { }

// Calls the translated original gallery. Inputs travel through XTEST -> GLFW
// -> translated app/widgets. State is observed, never synthesized for actions.
internal static unsafe partial class Program
{
    private static int assertions;
    private static readonly List<PageResult> pages = [];
    private static readonly List<string> actions = [];
    private static delegate*<void*, void*, void> originalFree;
    private static nint watchedUi;
    private static int frees;
    private static readonly bool diagnosticInvisible = Environment.GetEnvironmentVariable("CUI_GALLERY_DIAGNOSTIC_INVISIBLE") == "1";
    private static int visibilityFailures;
    private static readonly bool captureOnly = Environment.GetEnvironmentVariable("CUI_GALLERY_CAPTURE_ONLY") == "1";
    private static readonly bool inputOnly = Environment.GetEnvironmentVariable("CUI_GALLERY_INPUT_ONLY") == "1";

    private static void VisibleCheck(bool condition, string name)
    {
        Console.WriteLine($"EMOJI_VISIBILITY_ASSERT\t{name}\t{(condition ? 1 : 0)}");
        if (!condition) visibilityFailures++;
        // Explicit before-fix evidence mode only. The runner never accepts a
        // diagnostic run as qualification, even if every observation succeeds.
        if (!diagnosticInvisible) Check(condition, "Inline emoji visibility: " + name);
    }

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
            theme: (cui_theme_mode)GalleryClockFixture.ThemeIndex, accent: (cui_accent)GalleryClockFixture.AccentIndex);
        internal gallery State;
        internal int Frames;
        internal Session()
        {
            fixed (gallery* g = &State) gallery_init(g);
            State.dark = GalleryClockFixture.ThemeIndex == 1; State.accent = GalleryClockFixture.AccentIndex;
            cui_set_theme(App.Ui, (cui_theme_mode)GalleryClockFixture.ThemeIndex);
            cui_set_accent(App.Ui, (cui_accent)GalleryClockFixture.AccentIndex);
            Check(cui_get_theme_mode(App.Ui) == (cui_theme_mode)GalleryClockFixture.ThemeIndex &&
                cui_get_accent(App.Ui) == (cui_accent)GalleryClockFixture.AccentIndex && App.Ui->theme_t == 1,
                "Gallery starts with the selected settled palette");
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
            GalleryObservation.WriteLayout(Path.Combine(directory, $"page-{page:00}.layout"), ui);
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

    private static uint SmartId(Session s, string value)
    {
        fixed (byte* scope = "Smart inputs"u8) cui_push_id(s.App.Ui, scope);
        uint id = s.Id(value);
        cui_pop_id(s.App.Ui);
        return id;
    }

    private static void EmojiMessage(Session s, string expected)
    {
        // The original page keeps message[] static. Observe the actual edit
        // renderer's payload, using its emitted type; never write that storage.
        var node = s.Node(SmartId(s, "message") ^ 0xED17u);
        Check(node->paint != null && node->paint_data != null, "Message has an edit renderer");
        var paint = (cui__edit_paint*)node->paint_data;
        byte[] bytes = Encoding.UTF8.GetBytes(expected);
        Check(paint->len == bytes.Length && new ReadOnlySpan<byte>(paint->buf, paint->len).SequenceEqual(bytes),
            $"Emoji message contains exact UTF-8 bytes: {expected}");
        Console.WriteLine($"EMOJI_MESSAGE\t{paint->len}\t{Convert.ToHexString(bytes)}\t{expected}");
    }

    private static void EmojiEditState(Session s, string step, string expected, int cursor, int anchor = -1)
    {
        EmojiMessage(s, expected);
        var p = (cui__edit_paint*)s.Node(SmartId(s, "message") ^ 0xED17u)->paint_data;
        if (anchor < 0) anchor = cursor;
        Check(p->focused != 0 && s.App.Ui->focus == SmartId(s, "message"), "OS editing focuses the original message input");
        Check(p->cursor == cursor && p->anchor == anchor, $"{step}: exact UTF-8 byte cursor/anchor {cursor}/{anchor}, got {p->cursor}/{p->anchor}");
        Check(cursor >= 0 && cursor <= p->len && (cursor == p->len || (p->buf[cursor] & 0xc0) != 0x80) &&
            anchor >= 0 && anchor <= p->len && (anchor == p->len || (p->buf[anchor] & 0xc0) != 0x80), "Caret and selection stay on UTF-8 boundaries");
        Console.WriteLine($"EMOJI_EDIT\t{step}\t{p->len}\t{p->cursor}\t{p->anchor}\t{Convert.ToHexString(new ReadOnlySpan<byte>(p->buf, p->len))}");
    }

    private static cui_node* EmojiGrid(Session s)
    {
        cui_node* found = null;
        for (int i = 0; i < s.App.Ui->node_count; i++)
        {
            var n = &s.App.Ui->nodes[i];
            if (n->paint == null || n->paint_data == null || n->id == 0 ||
                (n->flags & CUI_NF_FOCUSABLE) == 0 || n->rect.h != 260) continue;
            Check(found == null, "Exactly one original emoji grid");
            found = n;
        }
        Check(found != null, "Original 260px emoji grid is rendered");
        return found;
    }

    private static void EmojiPopup(Session s, bool open)
    {
        uint id = SmartId(s, "emoji-pop");
        bool found = false;
        for (int i = 0; i < s.App.Ui->popup_count; i++)
        {
            var p = &s.App.Ui->popups[i];
            if (p->id == id)
            {
                found = true;
                Console.WriteLine($"EMOJI_POPUP\t{id}\t{p->open}\t{p->s.x}");
                Check((p->open != 0) == open, $"Original emoji popup {id} open={p->open}, expected={open}");
            }
            else Check(p->open == 0, "No other popup is open");
        }
        Check(found, "Original scoped emoji popup exists");
    }

    private static void OpenEmoji(Session s, OsInput input)
    {
        var message = s.Node(SmartId(s, "message"))->rect;
        cui_rect trigger = default;
        int count = 0;
        for (int i = 0; i < s.App.Ui->node_count; i++)
        {
            var n = &s.App.Ui->nodes[i];
            if (n->rect.x > message.x + message.w && MathF.Abs(n->rect.y - message.y) < 10 &&
                (n->flags & CUI_NF_INTERACTIVE) != 0) { trigger = n->rect; count++; }
        }
        Check(count == 1, "Original smile icon trigger is beside the message");
        Click(s, input, trigger);
        s.Settle(100); // settle popup placement and budgeted color-emoji atlas uploads
        EmojiPopup(s, true);
        EmojiGrid(s);
        actions.Add("OS smile icon opens original emoji popup");
    }

    private static void EmojiRecent(Session s, params string[] expected)
    {
        var p = (cui__em_paint*)EmojiGrid(s)->paint_data;
        Check(p->none == 0 && p->g.nsec > 0 && p->g.nsec <= 11 && p->items != null && p->recent != null,
            "Emoji font and original picker paint data are available");
        Check(p->g.sec[0].kind == (expected.Length == 0 ? 1 : 0), "Recent section precedes smileys when populated");
        if (expected.Length > 0)
        {
            Check(p->g.sec[0].count == expected.Length, "Recent list deduplicates selected emoji");
            for (int i = 0; i < expected.Length; i++)
                Check(p->items[i] == -(i + 1) && Utf8(p->recent + i * 32) == expected[i],
                    $"Recent item {i} preserves actual emoji {expected[i]}");
        }
        Console.WriteLine("EMOJI_RECENT\t" + string.Join(" ", expected));
        actions.Add("Original emoji recent order: " + string.Join(" ", expected));
    }

    private static byte[] EmojiPixels(Session s, string directory, string name, cui_rect r, bool color = false, int inlineCount = -1)
    {
        byte[] frame = s.App.CapturePixels(out int width, out int height);
        int x = (int)MathF.Ceiling(r.x), y = (int)MathF.Ceiling(r.y);
        int w = (int)MathF.Floor(r.w) - 1, h = (int)MathF.Floor(r.h) - 1;
        Check(x >= 0 && y >= 0 && w > 0 && h > 0 && x + w <= width && y + h <= height, "Emoji GL crop is visible");
        byte[] pixels = new byte[w * h * 4];
        int colored = 0, ink = 0, minX = w, minY = h, maxX = -1, maxY = -1;
        var shades = new HashSet<int>();
        for (int row = 0; row < h; row++)
        {
            frame.AsSpan(((y + row) * width + x) * 4, w * 4).CopyTo(pixels.AsSpan(row * w * 4));
            for (int col = 0; col < w; col++)
            {
                int at = (row * w + col) * 4;
                int high = Math.Max(pixels[at], Math.Max(pixels[at + 1], pixels[at + 2]));
                int low = Math.Min(pixels[at], Math.Min(pixels[at + 1], pixels[at + 2]));
                if (high - low > 32)
                {
                    colored++;
                    minX = Math.Min(minX, col); maxX = Math.Max(maxX, col);
                    minY = Math.Min(minY, row); maxY = Math.Max(maxY, row);
                    shades.Add((pixels[at] >> 4) << 8 | (pixels[at + 1] >> 4) << 4 | (pixels[at + 2] >> 4));
                }
                if (row > 3 && row < h - 3 && col > 8 && col < w - 8 && high < 128) ink++;
            }
        }
        File.WriteAllBytes(Path.Combine(directory, name + ".rgba"), pixels);
        Console.WriteLine($"EMOJI_PIXELS\t{name}\t{w}\t{h}\t{colored}");
        if (color) Check(colored >= 50, "Picked emoji renders real colored GL pixels");
        else Check(ink >= 50, "Original message GL crop contains rendered text");
        if (inlineCount >= 0)
        {
            Console.WriteLine($"EMOJI_INLINE\t{name}\t{inlineCount}\t{colored}\t{minX}\t{minY}\t{maxX}\t{maxY}\t{shades.Count}");
            // Crop is the original edit-paint leaf, excluding the smile button,
            // borders, picker and all other controls. Neutral unfocused plain
            // text supplies an independent zero-color negative control.
            VisibleCheck(inlineCount == 0 ? colored == 0 :
                colored >= 30 * inlineCount && maxX - minX + 1 >= 6 * inlineCount && maxY - minY + 1 >= 6 && shades.Count >= 3, name);
        }
        return pixels;
    }

    private static byte[] EmojiInline(Session s, string directory, string name, int count)
    {
        var edit = s.Node(SmartId(s, "message") ^ 0xED17u);
        var p = (cui__edit_paint*)edit->paint_data;
        var field = s.Node(SmartId(s, "message"))->rect;
        Check(p->focused == 0 && p->password == 0 && p->multiline == 0, "Stable inline capture uses the original unfocused editable input");
        Check(edit->rect.x >= field.x && edit->rect.y >= field.y && edit->rect.x + edit->rect.w <= field.x + field.w &&
            edit->rect.y + edit->rect.h <= field.y + field.h, "Inline pixels are inside the actual original input");
        return EmojiPixels(s, directory, name, edit->rect, inlineCount: count);
    }

    private static void PickEmoji(Session s, OsInput input, string directory, int selection, int section, int column, string expected)
    {
        var grid = EmojiGrid(s);
        var p = (cui__em_paint*)grid->paint_data;
        Check(p->g.cell > 0 && p->g.cols > column && section < p->g.nsec && p->g.sec[section].count > column,
            "Selected original emoji cell exists");
        float top = 0;
        for (int i = 0; i < section; i++)
            top += (p->g.sec[i].kind == 10 ? 0 : p->g.header) +
                ((p->g.sec[i].count + p->g.cols - 1) / p->g.cols) * p->g.cell;
        var cell = new cui_rect { x = grid->rect.x + p->g.pad + column * p->g.cell,
            y = grid->rect.y + top + p->g.header - p->top, w = p->g.cell, h = p->g.cell };
        EmojiPixels(s, directory, $"emoji-{selection}-cell", cell, color: true);
        int popupCount = s.App.Ui->popup_count;
        uint gridId = grid->id;
        Click(s, input, cell);
        s.Settle(10);
        EmojiMessage(s, expected); // check append independently before checking close
        EmojiPopup(s, false);
        Check(s.App.Ui->popup_count == popupCount, "Picking creates no phantom popup in a nested ID scope");
        s.Settle(100);
        EmojiPopup(s, false);
        for (int i = 0; i < s.App.Ui->popup_count; i++)
            if (s.App.Ui->popups[i].id == SmartId(s, "emoji-pop"))
                Check(s.App.Ui->popups[i].s.x == 0, "Original emoji popup exit spring finishes");
        for (int i = 0; i < s.App.Ui->node_count; i++)
            Check(s.App.Ui->nodes[i].id != gridId, "Closed original emoji grid disappears from rendered nodes");
        EmojiMessage(s, expected);
        EmojiPixels(s, directory, $"emoji-{selection}-message", s.Node(SmartId(s, "message"))->rect);
        EmojiInline(s, directory, $"emoji-{selection}-inline", selection);
        actions.Add($"OS emoji selection {selection} appends exact UTF-8 and closes original popup: {expected}");
    }

    private static void EmojiWorkflow(Session s, OsInput input, string directory)
    {
        Select(s, input, 10);
        Reveal(s, input, "Emoji picker");
        Wheel(s, input, false, true, 3);
        EmojiMessage(s, "Release notes are up ");
        var before = EmojiPixels(s, directory, "emoji-0-message", s.Node(SmartId(s, "message"))->rect);
        EmojiInline(s, directory, "emoji-0-inline", 0);
        OpenEmoji(s, input);
        EmojiRecent(s);
        PickEmoji(s, input, directory, 1, 0, 0, "Release notes are up 😀");
        var after = EmojiPixels(s, directory, "emoji-1-rendered-message", s.Node(SmartId(s, "message"))->rect);
        bool changed = !before.AsSpan().SequenceEqual(after);
        Console.WriteLine("EMOJI_RENDER_CHANGE\t" + (changed ? 1 : 0));
        VisibleCheck(changed, "picked-message-changes");
        OpenEmoji(s, input);
        EmojiRecent(s, "😀");
        PickEmoji(s, input, directory, 2, 1, 1, "Release notes are up 😀😃");
        Select(s, input, 1);
        Select(s, input, 10);
        Reveal(s, input, "Emoji picker");
        Wheel(s, input, false, true, 3);
        EmojiMessage(s, "Release notes are up 😀😃");
        OpenEmoji(s, input);
        EmojiRecent(s, "😃", "😀");
        PickEmoji(s, input, directory, 3, 0, 1, "Release notes are up 😀😃😀");
        OpenEmoji(s, input);
        EmojiRecent(s, "😀", "😃");
        Key(s, "Escape");
        s.Settle(100);
        EmojiPopup(s, false);
        EmojiMessage(s, "Release notes are up 😀😃😀");
        actions.Add("Emoji message and recent history persist across popup reopen and page navigation");
        EmojiEditing(s, input, directory);
    }

    private static void EmojiEditing(Session s, OsInput input, string directory)
    {
        const string prefix = "Release notes are up ", original = prefix + "😀😃😀";
        int start = Encoding.UTF8.GetByteCount(prefix);
        Click(s, input, s.Node(SmartId(s, "message"))->rect);
        Key(s, "End"); EmojiEditState(s, "end", original, start + 12);
        Key(s, "Left"); EmojiEditState(s, "left-over-emoji", original, start + 8);
        Key(s, "Right"); EmojiEditState(s, "right-over-emoji", original, start + 12);
        Key(s, "Left"); EmojiEditState(s, "insert-position", original, start + 8);
        input.Text("X"); s.Until(() => input.Idle, "OS inserts plain text between picked emoji");
        EmojiEditState(s, "insert-between", prefix + "😀😃X😀", start + 9);
        Key(s, "Home"); EmojiEditState(s, "home", prefix + "😀😃X😀", 0);
        Key(s, "Right"); EmojiEditState(s, "right-over-ascii", prefix + "😀😃X😀", 1);
        Key(s, "End"); EmojiEditState(s, "end-mixed", prefix + "😀😃X😀", start + 13);
        Click(s, input, s.Text("Emoji picker")); s.Settle(100);
        EmojiInline(s, directory, "emoji-edit-mixed-inline", 3);
        Click(s, input, s.Node(SmartId(s, "message"))->rect);
        Key(s, "End");
        Key(s, "BackSpace"); EmojiEditState(s, "backspace-emoji", prefix + "😀😃X", start + 9);
        Key(s, "Left"); EmojiEditState(s, "left-over-ascii", prefix + "😀😃X", start + 8);
        Key(s, "Left"); EmojiEditState(s, "left-over-second", prefix + "😀😃X", start + 4);
        Key(s, "Delete"); EmojiEditState(s, "delete-emoji", prefix + "😀X", start + 4);
        Key(s, "Right"); EmojiEditState(s, "right-after-delete", prefix + "😀X", start + 5);
        Key(s, "BackSpace"); EmojiEditState(s, "backspace-ascii", prefix + "😀", start + 4);
        Xdotool("keydown", "Shift_L"); s.Settle(2);
        Key(s, "Left");
        Xdotool("keyup", "Shift_L"); s.Settle(2);
        EmojiEditState(s, "select-whole-emoji", prefix + "😀", start, start + 4);
        input.Text("Z"); s.Until(() => input.Idle, "OS replaces selected four-byte emoji with plain text");
        EmojiEditState(s, "replace-selected-emoji", prefix + "Z", start + 1);
        Key(s, "BackSpace"); EmojiEditState(s, "restore-plain-prefix", prefix, start);
        Click(s, input, s.Text("Emoji picker")); s.Settle(100);
        EmojiInline(s, directory, "emoji-edit-plain-inline", 0);
        Select(s, input, 1); Select(s, input, 10);
        Reveal(s, input, "Emoji picker"); Wheel(s, input, false, true, 3);
        EmojiMessage(s, prefix);
        OpenEmoji(s, input); EmojiRecent(s, "😀", "😃");
        Key(s, "Escape"); s.Settle(100); EmojiPopup(s, false); EmojiMessage(s, prefix);
        actions.Add("Original emoji input supports OS byte-boundary navigation, insertion, selection, Backspace/Delete and edited persistence");
    }

    private static void Interact(string directory)
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

        EmojiWorkflow(s, input, directory);

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
        ExtendedInteractions(s, input);
        ResponsiveInteractions(s, input);
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
            GalleryClockFixture.Validate();
            if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Gallery OS qualification currently executes Linux/X11 only; Windows remains unrun.");
            if (args.Length != 1) throw new ArgumentException("Expected capture/report directory");
            Directory.CreateDirectory(args[0]);
            if (Environment.GetEnvironmentVariable("CUI_GALLERY_RESPONSIVE_ONLY") == "1") return ResponsiveOnly();
            if (!inputOnly) CapturePages(args[0]);
            if (!captureOnly) Interact(args[0]);
            Console.WriteLine($"EMOJI_VISIBILITY_SUMMARY\t{visibilityFailures}\t{(diagnosticInvisible ? "diagnostic" : "strict")}");
            // A fresh process-local lifecycle after every upstream page has run.
            using (var retry = new Session())
            {
                retry.Settle();
                Check(retry.State.page == 0 && retry.State.bluetooth == 0 && retry.State.newsletter != 0,
                    "Reinitialized base gallery has fresh state");
                Check(StateName(retry) == "Maya Okafor", "Reinitialized text has original upstream default");
            }
            File.WriteAllText(Path.Combine(args[0], "report.json"), JsonSerializer.Serialize(
                new GalleryReport(inputOnly ? "input-only" : captureOnly ? "captures-only" : diagnosticInvisible ? "diagnostic" : "passed", assertions, pages, actions, RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture.ToString(),
                    "unrun", "UI allocation freed once per lifecycle, GLFW context/display released, reinitialized"),
                GalleryJsonContext.Default.GalleryReport));
            Console.WriteLine($"{(inputOnly ? "INPUT-ONLY" : captureOnly ? "CAPTURES-ONLY" : diagnosticInvisible ? "DIAGNOSTIC" : "PASS")} translated upstream gallery: {(inputOnly ? 0 : 27)} captures, {(captureOnly ? 0 : 27)} OS selections, {actions.Count} state-asserted actions, cleanup; {assertions} assertions");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}

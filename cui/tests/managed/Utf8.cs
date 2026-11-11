using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Cui.Managed;

internal static unsafe partial class Program
{
    private static void Utf8Inputs()
    {
        Utf8Surface();
        byte[] reference = Utf8Picture(0);
        Check(reference.AsSpan().SequenceEqual(Utf8Picture(1)), "UTF-8 span rendering matches strings exactly");
        Check(reference.AsSpan().SequenceEqual(Utf8Picture(2)), "TextBuffer rendering matches strings exactly");
        using var surface = new OffscreenSurface(640, 480);
        Ui ui = surface.Ui;
        using var buffer = new TextBuffer(128, "original 😀"u8);
        Check(buffer.Text == "original 😀", "UTF-8 constructor");
        Throws<ArgumentException>(() => new TextBuffer(4, "four"u8), "constructor capacity includes terminator");
        Throws<ArgumentException>(() => new TextBuffer(32, new byte[] { 0xff }), "constructor validates UTF-8");
        Throws<InvalidOperationException>(() => ui.Text("outside"u8), "UTF-8 requires active frame");
        Throws<InvalidOperationException>(() => ui.Text(buffer), "buffer text requires active frame");
        using (surface.BeginFrame())
        {
            Span<byte> stack = stackalloc byte[64];
            int length = Encoding.UTF8.GetBytes("stack-backed 😀", stack);
            ui.Text(stack[..length]);
            stack.Fill(0xff);
            byte[] movable = "movable UTF-8"u8.ToArray();
            ui.Label(movable);
            movable.AsSpan().Clear();
            ui.Text(buffer);
            buffer.Set("changed later"u8);
            buffer.Dispose();
            TextBuffer[] items = [new(64, "first item"u8), new(64, "second item"u8)];
            ui.Breadcrumb(items);
            foreach (var item in items) item.Dispose();
            ui.Text(ReadOnlySpan<byte>.Empty);
            Throws<ArgumentException>(() => ui.Text(new byte[] { 0xc0, 0xaf }), "overlong UTF-8");
            Throws<ArgumentException>(() => ui.Button(new byte[] { 0xed, 0xa0, 0x80 }), "UTF-8 surrogate");
            Throws<ArgumentException>(() => ui.Text("a\0b"u8), "embedded NUL");
            Throws<ArgumentException>(() => ui.Breadcrumb(new ReadOnlyMemory<byte>[] { new byte[] { 0xff } }), "collection validates UTF-8");
            Throws<ArgumentNullException>(() => ui.Label((TextBuffer)null!), "null read-only buffer");
            Throws<ArgumentNullException>(() => ui.Breadcrumb(new TextBuffer[] { null! }), "null collection entry");
            Throws<ObjectDisposedException>(() => ui.Text(buffer), "disposed read-only buffer");
            Pressure();
        }
        surface.Render();
        string texts = Text(ui);
        Check(texts.Contains("stack-backed 😀") && texts.Contains("movable UTF-8"), "byte inputs copied before stack/array mutation and GC");
        Check(texts.Contains("original 😀") && !texts.Contains("changed later"), "read-only buffer input is a stable snapshot");
        Check(texts.Contains("first item") && texts.Contains("second item"), "buffer collections survive immediate disposal");
        Check(ui.GetDiagnostics().LeasedTextBuffers == 0, "read-only buffer sources do not acquire editable leases");

        using var owned = new TextBuffer(64, "owner"u8);
        Exception? wrongThread = null;
        var thread = new Thread(() => { try { owned.Set("wrong"u8); } catch (Exception error) { wrongThread = error; } });
        thread.Start(); thread.Join();
        Check(wrongThread is InvalidOperationException, "UTF-8 buffer setter enforces owner thread");
        wrongThread = null;
        thread = new Thread(() =>
        {
            try { using var other = new OffscreenSurface(100, 100); using var frame = other.BeginFrame(); other.Ui.Text(owned); }
            catch (Exception error) { wrongThread = error; }
        });
        thread.Start(); thread.Join();
        Check(wrongThread is InvalidOperationException, "read-only buffer sources enforce owner thread");
        using var terminal = new TerminalBuffer();
        terminal.Write(owned);

        int paintCalls = 0;
        using var painter = ui.CreateCanvasPainter(paint =>
        {
            paintCalls++;
            paint.DrawText(Font.Body, 14, new(0, 0), Color.Foreground, "paint 😀"u8);
            paint.DrawText(Font.Body, 14, new(0, 20), Color.Foreground, owned);
            Throws<ArgumentOutOfRangeException>(() => ui.DrawText(Font.Body, 14, default, Color.Foreground, "é"u8, 3), "draw length uses bytes");
        });
        using (surface.BeginFrame()) ui.Canvas("canvas"u8, Size.Px(200), Size.Px(60), painter);
        surface.Render();
        Check(paintCalls > 0, "UTF-8 paint callback executed");

        // Measure only warmed submission, not renderer or initial native caches.
        for (int frame = 0; frame < 3; frame++)
        {
            using var token = surface.BeginFrame();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++) { ui.Text("warm UTF-8"u8); ui.Text(owned); }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            if (frame == 2) Check(allocated == 0, $"UTF-8/buffer submissions allocate zero managed bytes: {allocated}");
        }
        Console.WriteLine("UTF8\trendering=exact\tstable-input-copies=passed\tmanaged-submission-allocations=0");
    }

    private static byte[] Utf8Picture(int kind)
    {
        using var surface = new OffscreenSurface(640, 480, theme: ThemeMode.Light, accent: Accent.Neutral);
        using var text = new TextBuffer(128, "Café 😀"u8);
        using var id = new TextBuffer(128, "code"u8);
        using var edit = new TextBuffer(128, "editable 😀"u8);
        using var item = new TextBuffer(64, "First"u8);
        Ui ui = surface.Ui;
        using (surface.BeginFrame())
        using (ui.Column(new() { Gap = 8 }))
        {
            int selected = 0;
            if (kind == 0)
            {
                ui.Text("Café 😀"); ui.Button("Café 😀"); ui.Badge("Café 😀");
                ui.CodeBlock("code", "Café 😀"); ui.Input("input", edit);
                ui.Tabs("tabs", new string[] { "First" }, ref selected);
            }
            else if (kind == 1)
            {
                ui.Text("Café 😀"u8); ui.Button("Café 😀"u8); ui.Badge("Café 😀"u8);
                ui.CodeBlock("code", "Café 😀"u8); ui.Input("input"u8, edit);
                ui.Tabs("tabs"u8, new ReadOnlyMemory<byte>[] { "First"u8.ToArray() }, ref selected);
            }
            else
            {
                ui.Text(text); ui.Button(text); ui.Badge(text);
                ui.CodeBlock(id, text); ui.Input("input"u8, edit);
                ui.Tabs("tabs", new TextBuffer[] { item }, ref selected);
            }
        }
        surface.Render();
        return surface.Pixels.ToArray();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Qualification roots the ManagedCui assembly.")]
    private static void Utf8Surface()
    {
        int covered = 0;
        MethodInfo[] methods = typeof(Ui).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var method in methods)
        {
            Type[] original = method.GetParameters().Select(p => p.ParameterType).ToArray();
            if (!original.Any(t => t == typeof(string) || t == typeof(ReadOnlySpan<string>))) continue;
            // Each original signature's full Cartesian product must exist,
            // including mixed UTF-16 IDs and UTF-8 content/collections.
            void Verify(int index, Type[] signature)
            {
                if (index == signature.Length)
                {
                    Check(typeof(Ui).GetMethod(method.Name, signature) is not null, "Missing text overload: " + method.Name);
                    covered++; return;
                }
                Type type = original[index];
                signature[index] = type; Verify(index + 1, signature);
                if (type == typeof(string))
                {
                    signature[index] = typeof(ReadOnlySpan<byte>); Verify(index + 1, signature);
                    signature[index] = typeof(TextBuffer); Verify(index + 1, signature);
                }
                else if (type == typeof(ReadOnlySpan<string>))
                {
                    signature[index] = typeof(ReadOnlySpan<ReadOnlyMemory<byte>>); Verify(index + 1, signature);
                    signature[index] = typeof(ReadOnlySpan<TextBuffer>); Verify(index + 1, signature);
                }
            }
            Verify(0, new Type[original.Length]);
        }
        Console.WriteLine($"UTF8_SURFACE\tsignatures={covered}");
    }
}

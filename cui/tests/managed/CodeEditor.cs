using Cui.Managed;

internal static unsafe partial class Program
{
    private static void CodeEditorInputs()
    {
        using var surface = new OffscreenSurface(640, 480);
        using var buffer = new TextBuffer(128, "int n = 1;\n"u8);
        Ui ui = surface.Ui;
        var options = new CodeEditorOpts { Lang = "c", Height = 300, Autofocus = true };
        CodeEditorResult Frame(CodeEditorOpts opts)
        {
            CodeEditorResult result;
            using (surface.BeginFrame()) result = ui.CodeEditor("editor"u8, buffer, opts);
            surface.Render();
            return result;
        }
        Throws<InvalidOperationException>(() => ui.CodeEditor("outside", buffer), "code editor requires a frame");
        var initial = Frame(options);
        Check(initial.Focused && initial.Length == buffer.Length && initial.LineCount == 2,
            "code editor reports focus, UTF-8 length and line count");
        ui.InputText("// café "u8);
        var edited = Frame(options);
        Check(edited.Changed && !edited.Refused && buffer.Text == "// café int n = 1;\n", "code editor edits UTF-8 in place");
        Check(edited.Line == 1 && edited.Column == 9 && edited.Cursor == "// café "u8.Length,
            "code editor reports byte offsets and one-based caret position");
        options.ReadOnly = true;
        ui.InputText("ignored"u8);
        var readOnly = Frame(options);
        Check(!readOnly.Changed && buffer.Text == "// café int n = 1;\n", "read-only editor rejects typing");
        options.ReadOnly = false;
        ui.InputText(new string('x', 200));
        var refused = Frame(options);
        Check(refused.Refused && !refused.Changed && buffer.Text == "// café int n = 1;\n", "oversized insertion is refused whole");
        using (surface.BeginFrame())
        {
            Throws<ArgumentNullException>(() => ui.CodeEditor("null", null!), "code editor rejects null buffers");
            Throws<ArgumentException>(() => ui.CodeEditor("bad-marks", buffer, new()
            {
                Marks = new TextMark[] { new() { Start = 5, End = 7 }, new() { Start = 1, End = 2 } }
            }), "code editor rejects unsorted marks");
            Throws<ArgumentException>(() => ui.CodeEditor("bad-color", buffer, new()
            {
                Marks = new TextMark[] { new() { Start = 0, End = 1, Color = 7 } }
            }), "code editor rejects invalid mark colors");
        }
        using var other = new OffscreenSurface(320, 240);
        using (other.BeginFrame())
            Throws<ArgumentException>(() => other.Ui.CodeEditor("foreign", buffer), "editable buffer remains context-bound");

        // Copies of visible code, placeholder and marks survive delayed painting.
        byte[] expected = CodeEditorPicture(false);
        Check(expected.AsSpan().SequenceEqual(CodeEditorPicture(true)), "code editor paint survives buffer disposal and mark mutation");
        buffer.Dispose();
        using (surface.BeginFrame())
            Throws<ObjectDisposedException>(() => ui.CodeEditor("disposed", buffer), "code editor rejects disposed buffers");
        Console.WriteLine("CODE_EDITOR\tutf8-editing=passed\tread-only=passed\tcapacity=passed\tlifetimes=passed");
    }

    private static byte[] CodeEditorPicture(bool invalidate)
    {
        using var surface = new OffscreenSurface(640, 480);
        using var buffer = new TextBuffer(128, "int number = 42;\n"u8);
        using var empty = new TextBuffer(32);
        using var id = new TextBuffer(32, "buffer-id"u8);
        TextMark[] marks = [new() { Start = 4, End = 10, Color = 2 }];
        using (surface.BeginFrame())
        {
            surface.Ui.CodeEditor(id, buffer, new() { Lang = "c", Height = 160, Marks = marks });
            surface.Ui.CodeEditor("placeholder", empty, new() { Placeholder = "Empty code", Height = 160 });
            if (invalidate)
            {
                buffer.Set("changed"); buffer.Dispose(); empty.Dispose(); id.Dispose();
                marks[0] = default;
                Pressure();
            }
        }
        surface.Render();
        return surface.Pixels.ToArray();
    }
}

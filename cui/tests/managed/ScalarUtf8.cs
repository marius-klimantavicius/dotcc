using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Cui.Managed;

internal static unsafe partial class Program
{
    private static void ScopedScalarInputs()
    {
        for (int example = 0; example < 8; example++)
        {
            byte[] expected = ScalarPicture(example, 0);
            Check(!expected.AsSpan().SequenceEqual(ScalarPicture(example, 0, alternate: true)),
                $"scalar lifetime example {example} visibly renders its input");
            for (int kind = 1; kind < 4; kind++)
                Check(expected.AsSpan().SequenceEqual(ScalarPicture(example, kind)),
                    $"scalar lifetime example {example}, input kind {kind}: exact pixels after mutation/disposal");
        }
        Console.WriteLine("SCOPED_SCALARS\tdeferred-paint=exact\tstack-and-pool-reuse=passed\tretained-exceptions=passed");
    }

    private static byte[] ScalarPicture(int example, int kind, bool alternate = false)
    {
        string text = example switch
        {
            2 => "const label = 'copied code';\n// " + new string('x', 8192),
            3 => "{\"label\":\"copied JSON\",\"count\":42}",
            5 => "# Retained heading\n\n**Bold** and ordinary text.",
            6 => Icons.Folder,
            _ => "Copied caption"
        };
        if (alternate) text = example switch
        {
            3 => "{\"different\":true}",
            5 => "# Different heading",
            6 => Icons.Trash2,
            _ => "Different caption"
        };
        byte[] bytes = Encoding.UTF8.GetBytes(text + (kind == 1 ? "\0" : ""));
        using var buffer = new TextBuffer(Encoding.UTF8.GetByteCount(text) + 1, text);
        using var surface = new OffscreenSurface(720, 900, theme: ThemeMode.Light, accent: Accent.Neutral);
        Ui ui = surface.Ui;
        using (surface.BeginFrame())
        using (ui.Column(new() { Gap = 8 }))
        {
            switch (example)
            {
                case 0:
                    if (kind == 0) ui.ProgressRing(text, .6f, 160);
                    else if (kind == 2) ui.ProgressRing(buffer, .6f, 160);
                    else ui.ProgressRing(bytes, .6f, 160);
                    break;
                case 1:
                    if (kind == 0) ui.Gauge(text, .6f, 160);
                    else if (kind == 2) ui.Gauge(buffer, .6f, 160);
                    else ui.Gauge(bytes, .6f, 160);
                    break;
                case 2:
                    if (kind == 0) ui.CodeBlock("code", text);
                    else if (kind == 2) ui.CodeBlock("code"u8, buffer);
                    else ui.CodeBlock("code"u8, bytes);
                    break;
                case 3:
                    if (kind == 0) ui.JsonView("json", text);
                    else if (kind == 2) ui.JsonView("json"u8, buffer);
                    else ui.JsonView("json"u8, bytes);
                    break;
                case 4:
                    bool selected = true;
                    if (kind == 0)
                    {
                        ui.ConfirmButton(text); ui.Checkbox(text, ref selected);
                        ui.GradientText(text); ui.DotDisplay(text); ui.ChatMessage(text);
                        ui.Readout(text, text);
                    }
                    else if (kind == 2)
                    {
                        ui.ConfirmButton(buffer); ui.Checkbox(buffer, ref selected);
                        ui.GradientText(buffer); ui.DotDisplay(buffer); ui.ChatMessage(buffer);
                        ui.Readout(buffer, buffer);
                    }
                    else
                    {
                        ui.ConfirmButton(bytes); ui.Checkbox(bytes, ref selected);
                        ui.GradientText(bytes); ui.DotDisplay(bytes); ui.ChatMessage(bytes);
                        ui.Readout(bytes, bytes);
                    }
                    break;
                case 5:
                    if (kind == 0) ui.ChatMessage(text, new() { Markdown = true });
                    else if (kind == 2) ui.ChatMessage(buffer, new() { Markdown = true });
                    else ui.ChatMessage(bytes, new() { Markdown = true });
                    break;
                case 6:
                    using (ui.DockBegin("dock"u8))
                    {
                        if (kind == 0) ui.DockItem(text, "tooltip");
                        else if (kind == 2) ui.DockItem(buffer, "tooltip"u8);
                        else ui.DockItem(bytes, "tooltip"u8);
                    }
                    break;
                case 7:
                    using (ui.NodeEditorBegin("nodes"u8, new State<NodeView>(new() { Zoom = 1 }),
                        new() { Width = Size.Px(600), Height = Size.Px(350) }))
                    {
                        float x = 30, y = 30;
                        using var node = kind == 0 ? ui.NodeBegin(1, ref x, ref y, text)
                            : kind == 2 ? ui.NodeBegin(1, ref x, ref y, buffer)
                            : ui.NodeBegin(1, ref x, ref y, bytes);
                        ui.Text("node body"u8);
                    }
                    break;
            }
            long used = ui.GetDiagnostics().FrameArenaUsedBytes;
            Check(example < 5 ? used == 0 : used > 0,
                $"scalar example {example}: {(example < 5 ? "no wrapper copy" : "retained wrapper copy")}, bytes={used}");
            bytes.AsSpan().Fill(0xff);
            buffer.Set("X");
            buffer.Dispose();
            ClobberUtf8Temporaries();
            Pressure();
        }
        surface.Render();
        return surface.Pixels.ToArray();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ClobberUtf8Temporaries()
    {
        Span<byte> scratch = stackalloc byte[16384];
        scratch.Fill(0xff);
        byte[] rented = ArrayPool<byte>.Shared.Rent(16384);
        rented.AsSpan().Fill(0xff);
        ArrayPool<byte>.Shared.Return(rented);
        // Force the writes to remain observable to the optimizer.
        Check(scratch[0] == 0xff && scratch[^1] == 0xff, "temporary stack storage overwritten");
    }
}

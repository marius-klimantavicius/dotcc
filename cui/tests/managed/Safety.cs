using System.Text;
using Cui.Managed;
using Raw = Managed.Cui.Cui;

internal static unsafe partial class Program
{
    private static void Pressure()
    {
        for (int i = 0; i < 24; i++) _ = new byte[8192 + i];
        GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }
    private static string Text(Ui ui)
    {
        var raw = Context(ui); var result = new StringBuilder();
        for (int i = 0; i < raw->node_count; i++)
            if (raw->nodes[i].text != null) result.Append(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(raw->nodes[i].text, raw->nodes[i].text_len))).Append('\n');
        return result.ToString();
    }
    private static void Safety()
    {
        using var surface = new OffscreenSurface(640, 480, theme: ThemeMode.Light, accent: Accent.Neutral);
        var ui = surface.Ui;
        using var edit = new TextBuffer(128, "Maya 😀");
        Throws<InvalidOperationException>(() => ui.Text("outside"), "widget outside frame");
        Throws<InvalidOperationException>(() => ui.Input("outside", edit), "editor outside frame");
        Throws<ArgumentException>(() => edit.Set("a\0b"), "embedded NUL");
        Throws<ArgumentException>(() => edit.Set(new string('x', 128)), "capacity includes NUL");
        Throws<ArgumentException>(() => edit.CopyUtf8To(new byte[1]), "short copy destination");
        Throws<ArgumentException>(() => edit.SetUtf8(new byte[] { 0xc0, 0xaf }), "overlong UTF8 rejected");
        edit.SetUtf8("Maya 😀"u8); Check(edit.Text == "Maya 😀", "UTF8 edit initialization");
        Throws<ArgumentOutOfRangeException>(() => new OffscreenSurface(0, 10), "invalid dimensions");
        Throws<ArgumentOutOfRangeException>(() => new OffscreenSurface(10, 10, float.NaN), "nonfinite scale");
        Throws<ArgumentOutOfRangeException>(() => surface.BeginFrame(float.NaN), "nonfinite dt");
        var frame = surface.BeginFrame(); var frameCopy = frame;
        Throws<InvalidOperationException>(() => surface.BeginFrame(), "nested frame");
        var outer = ui.Column(new Box { Gap = 8 }); var outerCopy = outer;
        var inner = ui.Row();
        Throws<InvalidOperationException>(outer.Dispose, "scope order");
        inner.Dispose(); inner.Dispose();
        ui.Text(new string("retained string after compaction".AsSpan()));
        ui.Text("stack span".AsSpan());
        ui.TextUtf8("UTF8 frame text 😀"u8); ui.ButtonUtf8("UTF8 button"u8);
        Throws<ArgumentException>(() => ui.TextUtf8(new byte[] { 0xed, 0xa0, 0x80 }), "UTF8 surrogate rejected");
        ui.Input("edit", edit, new InputOpts { Placeholder = new string('p', 20) });
        outer.Dispose(); outerCopy.Dispose();
        Pressure(); frame.Dispose(); frameCopy.Dispose(); surface.Render();
        Check(Text(ui).Contains("retained string after compaction"), "retained string survives compacting GC");
        Check(edit.Text == "Maya 😀", "owned UTF8 survives compacting GC");
        Check(ui.GetDiagnostics().ActiveScopes == 0, "balanced copied scopes");
        var next = surface.BeginFrame();
        Throws<InvalidOperationException>(outer.Dispose, "stale scope");
        Throws<InvalidOperationException>(frame.Dispose, "stale frame");
        // Frame cleanup must close abandoned nested scopes after an exception.
        try { using var column = ui.Column(); _ = ui.Row(); throw new ApplicationException("build"); }
        catch (Exception e) when (e is ApplicationException or InvalidOperationException) { }
        next.Dispose();
        Check(ui.GetDiagnostics().ActiveScopes == 0 && Context(ui)->parent_depth == 0, "exception frame unwinds scopes");

        using var other = new OffscreenSurface(640, 480);
        Id id;
        using (surface.BeginFrame()) id = ui.MakeId("owner");
        Throws<ArgumentException>(() => other.Ui.GetRect(id), "wrong context id");
        using var otherFrame = other.BeginFrame();
        Throws<ArgumentException>(() => other.Ui.Input("edit", edit), "wrong context buffer");
        var selected = new State<int>(0);
        // Retained mutable option slots are context-bound even after a frame.
        using (surface.BeginFrame())
        {
            using var carousel = ui.Carousel("carousel", 2, selected);
            using var slide = ui.CarouselSlideBegin(); ui.Text("slide");
        }
        Throws<ArgumentException>(() => other.Ui.Carousel("wrong", 2, selected), "wrong context state");
        otherFrame.Dispose();
        Exception? wrongThread = null;
        var thread = new Thread(() => { try { ui.Time(); } catch (Exception e) { wrongThread = e; } });
        thread.Start(); thread.Join();
        Check(wrongThread is InvalidOperationException, "owner thread enforced");

        PaintContext saved = default; int paints = 0;
        using var painter = ui.CreateCanvasPainter(p => { saved = p; paints++; p.DrawCircle(new Vec2(16, 16), 10, Color.Accent); });
        using (surface.BeginFrame()) { ui.Canvas("canvas", Size.Px(32), Size.Px(32), painter); Pressure(); }
        Check(paints == 1, "rooted deferred painter invoked once");
        Throws<InvalidOperationException>(() => saved.DrawCircle(default, 1, default), "expired paint capability");
        using var badPainter = ui.CreateCanvasPainter(_ => throw new ApplicationException("paint failure"));
        var badFrame = surface.BeginFrame(); ui.Canvas("bad", Size.Px(32), Size.Px(32), badPainter);
        Throws<ApplicationException>(badFrame.Dispose, "callback exception contained and surfaced");
        using (surface.BeginFrame()) ui.Text("recover after callback");
        using var reentrant = ui.CreateCanvasPainter(_ => ui.Text("illegal reentry"));
        var reentry = surface.BeginFrame(); ui.Canvas("reentry", Size.Px(32), Size.Px(32), reentrant);
        Throws<InvalidOperationException>(reentry.Dispose, "widget reentry from painter");

        using (surface.BeginFrame())
        {
            Throws<ArgumentOutOfRangeException>(() => ui.Breadcrumb(new[] { "a" }, 2), "count beyond span");
            Throws<ArgumentOutOfRangeException>(() => ui.Heatmap("bad", new float[2], 2, 2), "matrix product beyond span");
            int first = 0, last = 0;
            Throws<ArgumentOutOfRangeException>(() => ui.DataGridBegin("bad", new GridColumn[1], 1, 3, ref first, ref last, new GridOpts { Selected = new bool[1] }), "selection capacity");
            using (ui.DataGrid("bounds", new GridColumn[] { new() { Title = "Name" } }, 3, ref first, ref last))
                Throws<ArgumentOutOfRangeException>(() => ui.DataGridRow(3), "grid row beyond declared storage");
            Throws<ArgumentOutOfRangeException>(() => { float value = 1; ui.Slider("bad", ref value, 10, 0); }, "inverted range");
        }
        // Releasing edit/terminal/texture owners between construction and deferred paint must be safe.
        var retiring = new TextBuffer(64, "disposed during frame");
        var terminal = new TerminalBuffer(); terminal.Write("\u001b[31mretained ANSI\u001b[0m\n");
        var texture = ui.ImageCreate(1, 1, new byte[] { 255, 80, 10, 255 });
        using (surface.BeginFrame())
        {
            ui.Input("retiring", retiring); ui.Terminal("terminal", terminal); ui.Image(texture);
            retiring.Dispose(); terminal.Dispose(); texture.Dispose(); Pressure();
            Throws<ObjectDisposedException>(() => ui.Input("retiring-again", retiring), "disposed edit reuse");
            Throws<ObjectDisposedException>(() => ui.Image(texture), "disposed texture reuse");
        }
        surface.Render(); surface.Render();
        using (surface.BeginFrame()) ui.Text("drain retired resources");
        Check(ui.GetDiagnostics().TextureOwners == 0, "texture retirement drained");

        using (surface.BeginFrame())
        {
            using var path = new TextBuffer(256, "/");
            Throws<ApplicationException>(() => ui.FileBrowser("badfiles", path, new FileBrowserOpts { List = (_, _, _) => throw new ApplicationException("files") }), "file callback exception");
            Check(ui.FileBrowser("unavailable", path, new FileBrowserOpts { List = (_, _, _) => -1 }).Error,
                "ordinary file listing failure is a result, not an exception");
        }
        // A warmed steady workload must stop growing wrapper storage/registrations.
        UiDiagnostics high = default;
        for (int i = 0; i < 100; i++)
        {
            using (surface.BeginFrame()) { using var column = ui.Column(); ui.Text("steady"); ui.Input("stable", edit); }
            if (i == 20) high = ui.GetDiagnostics();
        }
        var final = ui.GetDiagnostics();
        Check(final.FrameArenaReservedBytes == high.FrameArenaReservedBytes && final.StateOwners == high.StateOwners && final.CallbackRegistrations == high.CallbackRegistrations, "bounded retained wrapper resources");
        RetainedGrid();
        ReviewedBounds();
        AllocatorCleanup();
        surface.Dispose(); surface.Dispose();
        painter.Dispose(); painter.Dispose(); texture.Dispose(); texture.Dispose();
        Throws<ObjectDisposedException>(() => ui.Time(), "disposed UI");
        Throws<ObjectDisposedException>(() => surface.BeginFrame(), "disposed surface");
        Console.WriteLine($"SAFETY\tassertions={assertions}\tarenaHighWater={final.FrameArenaReservedBytes}\tregistrations={final.CallbackRegistrations}");
    }
    private static void ReviewedBounds()
    {
        using var surface = new OffscreenSurface(640, 480);
        var ui = surface.Ui;
        using (surface.BeginFrame())
        {
            Candle[] candles = new Candle[32];
            var shortLabels = new CandlestickOpts { Labels = new[] { new string('A', 256) } };
            Throws<ArgumentOutOfRangeException>(() => ui.Candlestick("short-explicit", candles, 32, shortLabels), "short candle labels explicit count");
            Throws<ArgumentOutOfRangeException>(() => ui.Candlestick("short-inferred", candles, shortLabels), "short candle labels inferred count");
            ui.Candlestick("empty-labels", candles);
            ui.Candlestick("full-labels", candles, new CandlestickOpts { Labels = Enumerable.Repeat("label", 32).ToArray() });
            for (int columns = 0; columns < 2; columns++)
            {
                using (ui.Table("table" + columns, new TableColumn[columns]))
                {
                    Throws<InvalidOperationException>(() => ui.TableCellText("before-row"), "table ordering");
                    ui.TableRow(false); ui.TableCellText("valid cell");
                }
                int first = 0, last = 0;
                using (ui.DataGrid("grid" + columns, new GridColumn[columns], 1, ref first, ref last))
                {
                    Throws<InvalidOperationException>(() => ui.DataGridCellText("before-row"), "grid ordering");
                    ui.DataGridRow(0); ui.DataGridCellText("valid cell");
                }
            }
            QueryField[] fields = [new() { Name = "Status" }, new() { Name = "Owner" }];
            QueryNode[] nodes = [new() { Group = true }, new() { Depth = 1, Field = int.MinValue }];
            int count = 2;
            Throws<ArgumentOutOfRangeException>(() => ui.QueryBuilder("bad-query", fields, nodes, ref count), "negative query field");
            nodes[1] = new() { Depth = 1, Field = 1, Value = "Maya" };
            ui.QueryBuilder("valid-query", fields, nodes, ref count);
            int[] oversized = new int[257];
            Throws<ArgumentOutOfRangeException>(() => ui.Reorder("oversized", oversized), "reorder native capacity");
            int[] maximum = Enumerable.Range(0, ManagedCuiInfo.ReorderCapacity).ToArray();
            using (ui.Reorder("maximum", maximum)) { }
            ui.Gantt("long-days", new GanttTask[1], 1, new Date(2026, 11, 1), 30);
        }
        using (surface.BeginFrame()) ui.Text("valid frame after rejected inputs");
        Check(ui.GetDiagnostics().ActiveScopes == 0, "bounds failures preserve subsequent frame construction");
    }
    private static void RetainedGrid()
    {
        using var surface = new OffscreenSurface(640, 480);
        bool[] selected = new bool[100];
        var sort = new State<TableSort>();
        GridColumn[] columns = [new() { Title = "Name", Width = 300, Sortable = true }];
        Rect row = default;
        void Build()
        {
            using var frame = surface.BeginFrame();
            int first = 0, last = 0;
            using var grid = surface.Ui.DataGrid("retained", columns, 100, ref first, ref last,
                new GridOpts { Selected = selected, Sort = sort, Height = Size.Px(350) });
            for (int i = first; i < last; i++)
            {
                surface.Ui.DataGridRow(i);
                if (i == 1) row = surface.Ui.LastRect();
                surface.Ui.DataGridCellText("Maya");
            }
        }
        Build(); Build();
        // Find the row's actual rectangle after layout; LastRect is a borrowed
        // previous-layout copy and can refer to the checkbox rather than row.
        var raw = Context(surface.Ui);
        for (int i = 0; i < raw->node_count; i++)
            if (raw->nodes[i].text != null && Encoding.UTF8.GetString(new ReadOnlySpan<byte>(raw->nodes[i].text, raw->nodes[i].text_len)) == "Maya" && raw->nodes[i].rect.y > 70)
            { row = new(raw->nodes[i].rect.x, raw->nodes[i].rect.y, raw->nodes[i].rect.w, raw->nodes[i].rect.h); break; }
        Check(row.W > 0, "retained grid has visible row");
        surface.Ui.InputMouseMove(row.X + row.W / 2, row.Y + row.H / 2);
        surface.Ui.InputMouseButton(MouseButton.Left, true); Build();
        Pressure();
        surface.Ui.InputMouseButton(MouseButton.Left, false); Build();
        Check(selected.Count(x => x) == 1, "retained moved selection array copies native edits back");
        int chosen = Array.FindIndex(selected, x => x);
        Build(); Check(selected[chosen], "selection survives another frame");
        Console.WriteLine($"RETAINED_GRID\tselected={chosen}\trows={selected.Length}");
    }
}

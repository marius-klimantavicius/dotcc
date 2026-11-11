using System.Runtime.CompilerServices;
using Cui.Managed;
using Raw = Managed.Forms.Cui;

internal static unsafe partial class Program
{
    private static volatile int clobberChecksum;
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ClobberReturnedFrames(int depth = 3)
    {
        Span<byte> bytes = stackalloc byte[8192];
        bytes.Fill((byte)(0xa0 + depth));
        if (depth > 0) ClobberReturnedFrames(depth - 1);
        clobberChecksum ^= bytes[depth * 17] + bytes[^1];
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void BuildDelayedSafe(Ui ui)
    {
        using var panel = ui.Column(new Box { Pad = 12, Gap = 10, Width = Size.Fill });
        ui.TextUtf8("Delayed native temporaries"u8, new TextOpts { Size = 18, Font = Font.Medium });
        VizItem[] rings = [new() { Label = new string("Move".AsSpan()), Value = .72f }, new() { Label = "Stand", Value = .48f }];
        ui.ActivityRings("rings", rings, 2, new RingsOpts { Size = 100, Thickness = 8, Center = new string("72%".AsSpan()), Caption = "Daily activity", Legend = true });
        float[] values = [1, 4, 2, 6, 3, 7];
        string[] labels = ["Sample", "Sample", "Sample", "Sample", "Sample", "Sample"];
        ui.LineChart("chart", new Series[] { new() { Name = "Requests", Values = values } },
            new ChartOpts { Labels = labels, Height = 140, Area = true, NoLegend = true });
        values.AsSpan().Fill(-500); labels[0] = "caller storage changed";
        ui.Progress(.42f, new ProgressOpts { Label = "Retained label", ShowValue = true, Height = 6 });
        using var table = ui.Table("table", new TableColumn[] { new() { Title = "Person", Width = Size.Fill } });
        ui.TableRow(false); ui.TableCellText("Maya Okafor");
        // Scope ends before the wrapper method returns; all painting is later.
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void BuildDelayedDirectAndFinish(Raw.cui_ctx* ui)
    {
        Raw.cui_box panel = new() { pad = 12, gap = 10, width = new() { kind = Raw.cui_size_kind.CUI_SIZE_GROW, value = 1 } };
        Raw.cui_text_opts textOptions = new() { size = 18, font = 1 };
        Raw.cui_rings_opts ringOptions = new() { size = 100, thickness = 8, legend = true };
        Raw.cui_chart_opts chartOptions = new() { height = 140, area = true, no_legend = true };
        Raw.cui_progress_opts progressOptions = new() { show_value = true, height = 6 };
        Raw.cui_table_opts tableOptions = default;
        fixed (byte* title = "Delayed native temporaries"u8)
        fixed (byte* ringsId = "rings"u8)
        fixed (byte* move = "Move"u8)
        fixed (byte* stand = "Stand"u8)
        fixed (byte* center = "72%"u8)
        fixed (byte* caption = "Daily activity"u8)
        fixed (byte* chartId = "chart"u8)
        fixed (byte* sample = "Sample"u8)
        fixed (byte* seriesName = "Requests"u8)
        fixed (byte* progressLabel = "Retained label"u8)
        fixed (byte* tableId = "table"u8)
        fixed (byte* columnName = "Person"u8)
        fixed (byte* name = "Maya Okafor"u8)
        {
            Raw.cui_viz_item* rings = stackalloc Raw.cui_viz_item[2];
            rings[0] = new() { label = move, value = .72f }; rings[1] = new() { label = stand, value = .48f };
            ringOptions.center = center; ringOptions.caption = caption;
            float* values = stackalloc float[] { 1, 4, 2, 6, 3, 7 };
            byte** labels = stackalloc byte*[6]; for (int i = 0; i < 6; i++) labels[i] = sample;
            Raw.cui_series series = new() { name = seriesName, values = values, count = 6 };
            chartOptions.labels = labels; chartOptions.label_count = 6;
            progressOptions.label = progressLabel;
            Raw.cui_table_column column = new() { title = columnName, width = new() { kind = Raw.cui_size_kind.CUI_SIZE_GROW, value = 1 } };
            Raw.cui_column_begin(ui, &panel); Raw.cui_text_ex(ui, title, &textOptions);
            Raw.cui_activity_rings_ex(ui, ringsId, rings, 2, &ringOptions);
            Raw.cui_line_chart_ex(ui, chartId, &series, 1, &chartOptions);
            Raw.cui_progress_ex(ui, .42f, &progressOptions);
            Raw.cui_table_begin_ex(ui, tableId, &column, 1, &tableOptions);
            Raw.cui_table_row(ui, false); Raw.cui_table_cell_text(ui, name); Raw.cui_table_end(ui); Raw.cui_box_end(ui);
            ClobberReturnedFrames(); Pressure();
            // Direct local addresses remain alive through paint. The safe
            // wrapper's corresponding method frames have already returned.
            Raw.cui_end_frame(ui);
        }
    }
    private static void DelayedNativeTemporaries()
    {
        using var safe = new OffscreenSurface(640, 700, theme: ThemeMode.Light, accent: Accent.Neutral);
        using var direct = new OffscreenSurface(640, 700, theme: ThemeMode.Light, accent: Accent.Neutral);
        for (int iteration = 0; iteration < 4; iteration++)
        {
            var frame = safe.BeginFrame(); Context(safe.Ui)->time = 10;
            BuildDelayedSafe(safe.Ui);
            ClobberReturnedFrames(); Pressure(); frame.Dispose(); safe.Render();
            var raw = Context(direct.Ui); Raw.cui_begin_frame(raw, 1f / 60); raw->time = 10;
            BuildDelayedDirectAndFinish(raw); direct.Render();
            Check(safe.Pixels.SequenceEqual(direct.Pixels), "Returned option/scalar stack frames and moved/modified input arrays preserve deferred paint exactly");
        }
        Console.WriteLine($"DELAYED_TEMPORARIES\tframes=4\tstackChecksum={clobberChecksum}\texactRgba=true");
    }
    private static void DescriptorOnlyFrame()
    {
        using var surface = new OffscreenSurface(128, 600, frameArenaLimit: 4096);
        using (surface.BeginFrame())
        {
            using var column = surface.Ui.Column(new Box { Width = Size.Fill, Height = Size.Fill, Gap = 1 });
            for (int row = 0; row < 256; row++)
                using (surface.Ui.Box(new Box { Width = Size.Fill, Height = Size.Px(1), Bg = row % 2 == 0 ? Color.Accent : Color.SurfaceMuted })) { }
            ClobberReturnedFrames(); Pressure();
        }
        surface.Render();
        Check(surface.Ui.GetDiagnostics().FrameArenaUsedBytes == 0,
            "A descriptor-only 256-box UI fits a 4 KiB marshalling budget without native arena singleton storage");
        Check(surface.Pixels.Length == 128 * 600 * 4, "Descriptor-only frame renders after returned stack frames are overwritten");
        Console.WriteLine("DESCRIPTOR_ONLY\tboxes=256\tarenaBudget=4096\tarenaUsed=0");
    }
}

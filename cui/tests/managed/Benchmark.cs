using System.Diagnostics;
using System.Runtime.InteropServices;
using Cui.Managed;
using Raw = Managed.Cui.Cui;

internal static unsafe partial class Program
{
    private static readonly GridColumn[] gridColumns = [new() { Title = "Name", Width = 240 }, new() { Title = "Role", Width = 180 }, new() { Title = "Status", Width = 140 }];
    private static readonly bool[] gridSelection = new bool[20000];
    private static float sliderValue = 64;
    private static void WrapperBuild(Ui ui, int workload)
    {
        using var root = ui.Column(new Box { Width = Size.Fill, Height = Size.Fill, Gap = 8, Pad = 12 });
        if (workload < 2)
        {
            for (int i = 0; i < (workload == 0 ? 4 : 40); i++)
            {
                using var id = ui.PushIdInt(i);
                using var row = ui.Row(new Box { Gap = 8 });
                ui.Text("Representative control"); ui.Button("Continue"); ui.Slider("Volume", ref sliderValue, 0, 100);
            }
        }
        else
        {
            int first = 0, last = 0;
            using var grid = ui.DataGrid("grid", gridColumns, 20000, ref first, ref last,
                new GridOpts { Height = Size.Fill, Selected = gridSelection, Striped = true });
            for (int i = first; i < last; i++)
            {
                ui.DataGridRow(i); ui.DataGridCellText("Maya Okafor"); ui.DataGridCellText("Engineer"); ui.DataGridCellText("Active");
            }
        }
    }
    private static void DirectBuild(Raw.cui_ctx* ui, int workload)
    {
        var root = new Raw.cui_box { width = new() { kind = Raw.cui_size_kind.CUI_SIZE_GROW, value = 1 }, height = new() { kind = Raw.cui_size_kind.CUI_SIZE_GROW, value = 1 }, gap = 8, pad = 12 };
        Raw.cui_column_begin(ui, &root);
        if (workload < 2)
        {
            var row = new Raw.cui_box { gap = 8 }; Raw.cui_text_opts text = default;
            Raw.cui_button_opts button = default; Raw.cui_slider_opts slider = default;
            fixed (byte* label = "Representative control"u8)
            fixed (byte* action = "Continue"u8)
            fixed (byte* volume = "Volume"u8)
            fixed (float* value = &sliderValue)
            for (int i = 0; i < (workload == 0 ? 4 : 40); i++)
            {
                Raw.cui_push_id_int(ui, i); Raw.cui_row_begin(ui, &row);
                Raw.cui_text_ex(ui, label, &text); Raw.cui_button_ex(ui, action, &button); Raw.cui_slider_ex(ui, volume, value, 0, 100, &slider);
                Raw.cui_box_end(ui); Raw.cui_pop_id(ui);
            }
        }
        else
        {
            fixed (byte* grid = "grid"u8)
            fixed (byte* title1 = "Name"u8)
            fixed (byte* title2 = "Role"u8)
            fixed (byte* title3 = "Status"u8)
            fixed (byte* name = "Maya Okafor"u8)
            fixed (byte* role = "Engineer"u8)
            fixed (byte* status = "Active"u8)
            {
                Raw.cui_grid_column* columns = stackalloc Raw.cui_grid_column[3];
                columns[0] = new() { title = title1, width = 240 }; columns[1] = new() { title = title2, width = 180 }; columns[2] = new() { title = title3, width = 140 };
                // CBool has byte layout; selection is stable native storage outside timings.
                var options = new Raw.cui_grid_opts { height = new() { kind = Raw.cui_size_kind.CUI_SIZE_GROW, value = 1 }, selected = directSelection, striped = true };
                int first = 0, last = 0;
                Raw.cui_data_grid_begin_ex(ui, grid, columns, 3, 20000, &first, &last, &options);
                for (int i = first; i < last; i++)
                {
                    Raw.cui_data_grid_row(ui, i); Raw.cui_data_grid_cell_text(ui, name); Raw.cui_data_grid_cell_text(ui, role); Raw.cui_data_grid_cell_text(ui, status);
                }
                Raw.cui_data_grid_end(ui);
            }
        }
        Raw.cui_box_end(ui);
    }
    private static Raw.CBool* directSelection;
    private static (double p50, double p95, double p99) Quantiles(long[] values)
    {
        Array.Sort(values); double scale = 1e6 / Stopwatch.Frequency;
        return (values[values.Length / 2] * scale, values[(int)(values.Length * .95)] * scale, values[(int)(values.Length * .99)] * scale);
    }
    private static void Benchmark()
    {
        directSelection = (Raw.CBool*)NativeMemory.AllocZeroed(20000);
        try
        {
            for (int workload = 0; workload < 3; workload++)
            {
                using var wrapper = new OffscreenSurface(800, 600, theme: ThemeMode.Light, accent: Accent.Neutral);
                using var direct = new OffscreenSurface(800, 600, theme: ThemeMode.Light, accent: Accent.Neutral);
                var raw = Context(direct.Ui);
                // Equal clocks and UI allow exact layout/draw-buffer/pixel comparison.
                for (int warmup = 0; warmup < 200; warmup++)
                {
                    using (wrapper.BeginFrame()) WrapperBuild(wrapper.Ui, workload);
                    Raw.cui_begin_frame(raw, 1f / 60); DirectBuild(raw, workload); Raw.cui_end_frame(raw);
                }
                wrapper.Render(); direct.Render();
                Check(wrapper.Pixels.SequenceEqual(direct.Pixels), "benchmark workloads have identical rendered RGBA");
                Check(Context(wrapper.Ui)->node_count == raw->node_count, "benchmark workloads have identical node counts");
                var baseline = wrapper.Ui.GetDiagnostics();
                const int samples = 1200;
                var wrapperBuild = new long[samples]; var directBuild = new long[samples];
                var wrapperEnd = new long[samples]; var directEnd = new long[samples];
                long wrapperAllocation = 0, directAllocation = 0;
                // Alternate ordering to reduce thermal/JIT/order bias; all arrays preallocated.
                for (int sample = 0; sample < samples; sample++)
                {
                    for (int order = 0; order < 2; order++)
                    {
                        bool wrapped = ((sample + order) & 1) == 0;
                        long allocation = GC.GetAllocatedBytesForCurrentThread();
                        long start = Stopwatch.GetTimestamp();
                        if (wrapped)
                        {
                            var frame = wrapper.BeginFrame(); WrapperBuild(wrapper.Ui, workload);
                            long built = Stopwatch.GetTimestamp(); frame.Dispose();
                            wrapperEnd[sample] = Stopwatch.GetTimestamp() - built; wrapperBuild[sample] = built - start;
                            wrapperAllocation += GC.GetAllocatedBytesForCurrentThread() - allocation;
                        }
                        else
                        {
                            Raw.cui_begin_frame(raw, 1f / 60); DirectBuild(raw, workload);
                            long built = Stopwatch.GetTimestamp(); Raw.cui_end_frame(raw);
                            directEnd[sample] = Stopwatch.GetTimestamp() - built; directBuild[sample] = built - start;
                            directAllocation += GC.GetAllocatedBytesForCurrentThread() - allocation;
                        }
                    }
                }
                var wb = Quantiles(wrapperBuild); var db = Quantiles(directBuild); var we = Quantiles(wrapperEnd); var de = Quantiles(directEnd);
                var final = wrapper.Ui.GetDiagnostics();
                Check(final.FrameArenaReservedBytes == baseline.FrameArenaReservedBytes, "warmed wrapper arena is stable");
                Check(wrapperAllocation == directAllocation, "steady wrapper adds zero managed allocation over identical core");
                Check(wb.p95 - db.p95 <= raw->node_count * .5, "wrapper construction p95 adds at most 0.5 microsecond per node");
                Check(wb.p50 + we.p50 <= 2 * (db.p50 + de.p50), "wrapper complete UI median stays within twice direct CUI");
                Console.WriteLine(FormattableString.Invariant($"BENCH\tworkload={workload}\tsamples={samples}\tnodes={raw->node_count}\twrapperBytesPerFrame={wrapperAllocation / (double)samples:F3}\tdirectBytesPerFrame={directAllocation / (double)samples:F3}\tarena={final.FrameArenaReservedBytes}\tarenaUsed={final.FrameArenaUsedBytes}\tbuildWrapperUs={wb.p50:F3},{wb.p95:F3},{wb.p99:F3}\tbuildDirectUs={db.p50:F3},{db.p95:F3},{db.p99:F3}\tendWrapperUs={we.p50:F3},{we.p95:F3},{we.p99:F3}\tendDirectUs={de.p50:F3},{de.p95:F3},{de.p99:F3}"));
                // Separate actual core-allocator instrumentation from timings.
                var measured = new OffscreenSurface(800, 600, theme: ThemeMode.Light, accent: Accent.Neutral);
                TrackAllocations(measured.Ui);
                ulong retainedAtWarmup = 0;
                for (int frame = 0; frame < 700; frame++)
                {
                    using (measured.BeginFrame()) WrapperBuild(measured.Ui, workload);
                    if (frame == 200) retainedAtWarmup = outstanding;
                }
                ulong retainedFinal = outstanding, peak = highWater;
                measured.Dispose();
                Check(outstanding == 0 && tracked.Count == 0, "benchmark core allocations fully released");
                Check(retainedFinal <= retainedAtWarmup, "steady core native storage does not grow");
                Console.WriteLine($"BENCH_MEMORY\tworkload={workload}\tcorePostCreateHighWater={peak}\tcoreRetainedAt200={retainedAtWarmup}\tcoreRetainedAt700={retainedFinal}\tafterDispose={outstanding}\twrapperArena={final.FrameArenaReservedBytes}");
            }
        }
        finally { NativeMemory.Free(directSelection); directSelection = null; }
    }
}

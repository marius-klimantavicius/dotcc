using System.Diagnostics;
using Cui.Managed;
using Cui.ManagedGallery;
using OriginalGallery = Managed.Cui.Gallery.Gallery;
using static Managed.Cui.Cui;

internal static unsafe partial class Program
{
    private static void GalleryBenchmark()
    {
        foreach (int page in new[] { 1, 15, 24 })
        {
            using var safe = new OffscreenSurface(1240, 820, theme: ThemeMode.Light, accent: Accent.Neutral);
            using var direct = new OffscreenSurface(1240, 820, theme: ThemeMode.Light, accent: Accent.Neutral);
            using var demo = new GalleryDemo { Page = page };
            OriginalGallery.gallery state = default; OriginalGallery.gallery_init(&state); state.page = page;
            var raw = Context(direct.Ui);
            for (int frame = 0; frame < 200; frame++)
            {
                using (safe.BeginFrame()) { Context(safe.Ui)->time = 10; demo.Frame(safe.Ui); }
                cui_begin_frame(raw, 1f / 60); raw->time = 10; OriginalGallery.gallery_frame(raw, &state); cui_end_frame(raw);
            }
            safe.Render(); direct.Render();
            Check(safe.Pixels.SequenceEqual(direct.Pixels), "Actual benchmark gallery page has exact direct software RGBA");
            var memory = safe.Ui.GetDiagnostics();
            const int samples = 600;
            var safeBuild = new long[samples]; var rawBuild = new long[samples];
            var safeEnd = new long[samples]; var rawEnd = new long[samples];
            long safeAllocated = 0, rawAllocated = 0;
            for (int frame = 0; frame < samples; frame++)
            {
                for (int order = 0; order < 2; order++)
                {
                    long allocated = GC.GetAllocatedBytesForCurrentThread(), start = Stopwatch.GetTimestamp();
                    if (((frame + order) & 1) == 0)
                    {
                        var token = safe.BeginFrame(); Context(safe.Ui)->time = 10; demo.Frame(safe.Ui);
                        long built = Stopwatch.GetTimestamp(); token.Dispose();
                        safeEnd[frame] = Stopwatch.GetTimestamp() - built; safeBuild[frame] = built - start;
                        safeAllocated += GC.GetAllocatedBytesForCurrentThread() - allocated;
                    }
                    else
                    {
                        cui_begin_frame(raw, 1f / 60); raw->time = 10; OriginalGallery.gallery_frame(raw, &state);
                        long built = Stopwatch.GetTimestamp(); cui_end_frame(raw);
                        rawEnd[frame] = Stopwatch.GetTimestamp() - built; rawBuild[frame] = built - start;
                        rawAllocated += GC.GetAllocatedBytesForCurrentThread() - allocated;
                    }
                }
            }
            static string Times(long[] values)
            {
                Array.Sort(values); double scale = 1e6 / Stopwatch.Frequency;
                return FormattableString.Invariant($"{values[values.Length / 2] * scale:F3},{values[(int)(values.Length * .95)] * scale:F3},{values[(int)(values.Length * .99)] * scale:F3}");
            }
            var final = safe.Ui.GetDiagnostics();
            Check(final.FrameArenaReservedBytes == memory.FrameArenaReservedBytes && final.StateOwners == memory.StateOwners && final.CallbackRegistrations == memory.CallbackRegistrations,
                "Actual gallery warmed resources remain stable");
            Console.WriteLine(FormattableString.Invariant($"GALLERY_BENCH\tpage={page}\tname={GalleryDemo.PageName(page)}\tsamples={samples}\tnodes={raw->node_count}\twrapperBytesPerFrame={safeAllocated / (double)samples:F3}\tdirectBytesPerFrame={rawAllocated / (double)samples:F3}\tbuildWrapperUs={Times(safeBuild)}\tbuildDirectUs={Times(rawBuild)}\tendWrapperUs={Times(safeEnd)}\tendDirectUs={Times(rawEnd)}\twrapperArena={final.FrameArenaReservedBytes}\twrapperArenaUsed={final.FrameArenaUsedBytes}\tcoreArena={raw->arena_cap}"));
        }
    }
}

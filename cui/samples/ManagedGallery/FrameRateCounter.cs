using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace Cui.ManagedGallery;

/// <summary>Actual frame cadence, including presentation and event waits.</summary>
internal sealed class FrameRateCounter
{
    private readonly long[] intervals = new long[120];
    private readonly byte[] label = new byte[32];
    private int count, next, length = 6;
    private long previous, total, lastRefresh;

    public FrameRateCounter() => "-- FPS"u8.CopyTo(label);

    public ReadOnlySpan<byte> NextFrame()
    {
        var now = Stopwatch.GetTimestamp();
        if (previous != 0)
        {
            var elapsed = now - previous;
            total += elapsed - intervals[next];
            intervals[next] = elapsed;
            next = (next + 1) % intervals.Length;
            count = Math.Min(count + 1, intervals.Length);
            if (total > 0 && (count == 1 || now - lastRefresh >= Stopwatch.Frequency / 4))
            {
                var fps = (double)count * Stopwatch.Frequency / total;
                Utf8Formatter.TryFormat(fps, label, out var written, new StandardFormat('F', 1));
                " FPS"u8.CopyTo(label.AsSpan(written));
                length = written + 4;
                lastRefresh = now;
            }
        }
        else lastRefresh = now;

        previous = now;
        return label.AsSpan(0, length);
    }
}
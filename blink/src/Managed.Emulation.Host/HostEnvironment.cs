using System.Security.Cryptography;

namespace Managed.Emulation.Host;

public enum HostClock { Realtime = 0, Monotonic = 1 }
public readonly record struct HostTimestamp(long Seconds, long Nanoseconds);
public interface IHostEntropy { void Fill(Span<byte> destination); }
public sealed class BclHostEntropy : IHostEntropy
{
    public void Fill(Span<byte> destination) => RandomNumberGenerator.Fill(destination);
}

/// <summary>Instance-owned clocks and entropy policy, with injectable providers
/// for deterministic tests. Neither provider executes guest instructions.</summary>
public sealed class HostEnvironment(TimeProvider? time = null, IHostEntropy? entropy = null)
{
    private readonly TimeProvider time = time ?? TimeProvider.System;
    private readonly IHostEntropy entropy = entropy ?? new BclHostEntropy();
    private readonly long origin = (time ?? TimeProvider.System).GetTimestamp();
    private readonly object sync = new();
    public const int MaximumEntropyChunk = 256;

    public HostResult<HostTimestamp> GetTime(HostClock clock)
    {
        try
        {
            long ticks;
            lock (sync)
            {
                ticks = clock switch
                {
                    HostClock.Realtime => time.GetUtcNow().UtcDateTime.Ticks - DateTime.UnixEpoch.Ticks,
                    HostClock.Monotonic => time.GetElapsedTime(origin).Ticks,
                    _ => throw new ArgumentOutOfRangeException(nameof(clock))
                };
            }
            long seconds = Math.DivRem(ticks, TimeSpan.TicksPerSecond, out long remainder);
            if (remainder < 0) { --seconds; remainder += TimeSpan.TicksPerSecond; }
            return HostResult<HostTimestamp>.Success(new(seconds, remainder * 100));
        }
        catch (ArgumentOutOfRangeException) { return HostResult<HostTimestamp>.Failure(GuestError.Invalid); }
        catch (Exception) { return HostResult<HostTimestamp>.Failure(GuestError.Io); }
    }
    /// <summary>The software clock's output quantum, not clock accuracy. UTC
    /// uses DateTime ticks; elapsed time also respects the provider tick rate.</summary>
    public HostResult<HostTimestamp> GetResolution(HostClock clock)
    {
        try
        {
            long nanoseconds;
            lock (sync)
            {
                if (clock == HostClock.Realtime) nanoseconds = 100;
                else if (clock == HostClock.Monotonic)
                {
                    long frequency = time.TimestampFrequency;
                    if (frequency <= 0) return HostResult<HostTimestamp>.Failure(GuestError.Io);
                    nanoseconds = Math.Max(100, 1_000_000_000 / frequency + (1_000_000_000 % frequency == 0 ? 0 : 1));
                }
                else return HostResult<HostTimestamp>.Failure(GuestError.Invalid);
            }
            return HostResult<HostTimestamp>.Success(new(nanoseconds / 1_000_000_000, nanoseconds % 1_000_000_000));
        }
        catch (Exception) { return HostResult<HostTimestamp>.Failure(GuestError.Io); }
    }
    public HostResult<int> GetRandom(Span<byte> destination, uint flags = 0)
    {
        if ((flags & ~3u) != 0) return HostResult<int>.Failure(GuestError.Invalid);
        int count = Math.Min(destination.Length, MaximumEntropyChunk);
        if (count == 0) return HostResult<int>.Success(0);
        try
        {
            lock (sync) entropy.Fill(destination[..count]);
            return HostResult<int>.Success(count);
        }
        catch (OutOfMemoryException) { return HostResult<int>.Failure(GuestError.NoMemory); }
        catch (Exception) { return HostResult<int>.Failure(GuestError.Io); }
    }
}

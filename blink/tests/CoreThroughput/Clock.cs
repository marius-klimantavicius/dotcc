namespace Managed.Emulation;
public static unsafe partial class BlinkCore
{
    public static ulong ThroughputNow() => (ulong)global::System.Diagnostics.Stopwatch.GetTimestamp();
    public static ulong ThroughputFrequency() => (ulong)global::System.Diagnostics.Stopwatch.Frequency;
}

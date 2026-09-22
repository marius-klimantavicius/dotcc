using System;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool value) { if (!value) throw new Exception("coarse clock assertion failed"); }
Blink.BindHostEnvironment(new HostEnvironment());
try { Check(Blink.CoarseClockProbe() == 0); }
finally { Blink.UnbindHostEnvironment(); }

// Nonzero origin and normal nonnegative elapsed times around output boundaries.
var rows = new (long elapsed, long seconds, long nanos, long fine)[] {
    (0, 0, 0, 0), (1, 0, 0, 100), (9999, 0, 0, 999900),
    (10000, 0, 1000000, 1000000), (10001, 0, 1000000, 1000100),
    (9999999, 0, 999000000, 999999900), (10000000, 1, 0, 0),
    (10000001, 1, 0, 100), (123456789, 12, 345000000, 345678900)
};
var provider = new ControlledTime(10_000_000);
var owner = new HostEnvironment(provider);
Check(provider.TimestampCalls == 1);
Blink.BindHostEnvironment(owner);
try {
    foreach (var row in rows) {
        provider.Elapsed = row.elapsed;
        int before = provider.TimestampCalls;
        Check(Blink.CoarseClockInjected(row.seconds, row.nanos, row.fine, 0, 1000000) == 0);
        Check(provider.TimestampCalls == before + 2); // one coarse and one fine read
    }
} finally { Blink.UnbindHostEnvironment(); }

// Slow but valid providers constrain reported precision; output stays normalized.
foreach (var row in new (long frequency, long elapsed, long seconds, long nanos, long fine, long resSeconds, long resNanos)[] {
    (1, 2, 2, 0, 0, 1, 0), (2, 1, 0, 500000000, 500000000, 0, 500000000),
    (3, 1, 0, 333000000, 333333300, 0, 333333334),
    (1000, 1001, 1, 1000000, 1000000, 0, 1000000)
}) {
    var slow = new ControlledTime(row.frequency);
    var environment = new HostEnvironment(slow);
    slow.Elapsed = row.elapsed;
    Blink.BindHostEnvironment(environment);
    try { Check(Blink.CoarseClockInjected(row.seconds, row.nanos, row.fine, row.resSeconds, row.resNanos) == 0); }
    finally { Blink.UnbindHostEnvironment(); }
    Check(slow.TimestampCalls == 3);
}

sealed class ControlledTime(long frequency) : TimeProvider {
    public long Elapsed { get; set; }
    public int TimestampCalls { get; private set; }
    public override long TimestampFrequency => frequency;
    public override long GetTimestamp() { ++TimestampCalls; return 7_000_000 + Elapsed; }
    public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch.AddTicks(1_230_004_567);
}

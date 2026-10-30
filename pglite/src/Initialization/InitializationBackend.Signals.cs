using System;
using Runtime = Managed.PGlite.Translated.PGlite.Libc;

namespace Managed.PGlite.Initialization;

// Kept independently compilable for the existing boundary fixture, which has
// signal/lifetime exports but deliberately contains no PostgreSQL bootstrap.
public sealed unsafe partial class InitializationBackend
{
    private static readonly object gate = new();
    private static InitializationBackend? active;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private Runtime.RuntimeContext? runtime;

    /// <summary>Shared signal safe-point dispatcher calls this first. The
    /// original backend still owns ProcessInterrupts and all error recovery.</summary>
    public static bool TryPollSignals()
    {
        InitializationBackend? owner;
        lock (gate) { owner = active; }
        if (owner == null) return false;
        if (owner.thread != Environment.CurrentManagedThreadId || owner.runtime == null ||
            Runtime.RuntimeContext.Current != owner.runtime)
            throw new InvalidOperationException("A bootstrap safe point requires its owning backend context.");
        owner.runtime.PumpGuestSignals();
        return true;
    }
}

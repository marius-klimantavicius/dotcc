using GeneratedProgram = global::Managed.PGlite.Translated.PGlite;
using Runtime = Managed.PGlite.Translated.PGlite.Libc;

namespace Managed.PGlite.Hosting;

/// <summary>Execution transport for a --state-context library. PostgreSQL's
/// actual recovery and shutdown entrypoints are supplied by the product host.
/// This class does not initialize a database or implement SQL.</summary>
public sealed class PgliteOwner : IDisposable
{
    private static readonly object gate = new();
    private static PgliteOwner? active;
    private readonly GeneratedProgram.__DotCcContext context;
    private readonly Runtime.RuntimeContext runtime;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private readonly Action recover, shutdown, cleanup;
    private readonly List<object> roots = new();
    private bool entering, closed, quarantined;

    public PgliteOwner(Action recover, Action shutdown, Action cleanup)
    {
        ArgumentNullException.ThrowIfNull(recover);
        ArgumentNullException.ThrowIfNull(shutdown);
        ArgumentNullException.ThrowIfNull(cleanup);
        this.recover = recover; this.shutdown = shutdown; this.cleanup = cleanup;
        lock (gate)
        {
            if (active != null) throw new InvalidOperationException("The static PGlite program already has an owner.");
            active = this;
        }
        try
        {
            context = GeneratedProgram.__DotCcCreateContext();
            try
            {
                using var binding = context.Enter();
                runtime = Runtime.RuntimeContext.Current!;
                runtime.EnableGuestSignals();
            }
            catch { context.Dispose(); throw; }
        }
        catch { lock (gate) { active = null; } throw; }
    }

    /// <summary>Retain callback delegates and other host-owned roots until all
    /// upstream cleanup finishes. C callbacks must contain managed exceptions.</summary>
    public void Retain(object root)
    {
        RequireEntry(); ArgumentNullException.ThrowIfNull(root); roots.Add(root);
    }

    private void RequireEntry()
    {
        if (thread != Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("PGlite must enter on its owning thread.");
        ObjectDisposedException.ThrowIf(closed, this);
        if (entering || quarantined)
            throw new InvalidOperationException("PGlite cannot reenter or resume a quarantined lifetime.");
    }

    /// <summary>Typed target for dpg_host_poll_signals at translated
    /// CHECK_FOR_INTERRUPTS safe points. It never binds an ambient substitute.</summary>
    public static void PollSignals()
    {
        if (global::Managed.PGlite.Initialization.InitializationBackend.TryPollSignals()) return;
        PgliteOwner? owner;
        lock (gate) { owner = active; }
        if (owner == null || owner.thread != Environment.CurrentManagedThreadId ||
            !owner.entering || owner.closed || owner.quarantined || Runtime.RuntimeContext.Current != owner.runtime)
            throw new InvalidOperationException("A PGlite safe point requires the entering owning context.");
        owner.runtime.PumpGuestSignals();
    }

    /// <returns>0 for a normal return, 99 for an input/startup yield, or 100
    /// after real translated PostgreSQL error recovery has returned.</returns>
    public int Dispatch(Action entry)
    {
        RequireEntry(); ArgumentNullException.ThrowIfNull(entry); entering = true;
        try
        {
            using var binding = context.Enter();
            int status = 0;
            try
            {
                // The fork uses these exit statuses to yield to its embedding host.
                using var transport = runtime.InterceptExits(99, 100);
                runtime.PumpGuestSignals();
                entry();
            }
            catch (Runtime.RuntimeYieldException yield) when (ReferenceEquals(yield.Owner, runtime))
            { status = yield.Status; }
            // Do not emulate AbortCurrentTransaction/FlushErrorState here.
            // This delegate must call the actual dpg_recover_query_error export.
            if (status == 100) recover();
            return status;
        }
        catch { quarantined = true; throw; }
        finally { entering = false; }
    }

    public void Dispose()
    {
        if (closed) return;
        RequireEntry(); entering = true;
        try
        {
            using (context.Enter())
            {
                bool exited = false;
                try { shutdown(); }
                catch (Runtime.RuntimeTerminationException exit) when (ReferenceEquals(exit.Owner, runtime) &&
                    exit.Termination.Kind == Runtime.RuntimeTerminationKind.Exit && exit.Termination.Status == 0)
                { exited = true; }
                if (!exited) throw new InvalidOperationException("PostgreSQL shutdown did not complete with a real exit(0).");
                // Registered upstream atexit callbacks remain translated and run
                // with their original context and callback roots still alive.
                cleanup();
            }
            context.Dispose();
            roots.Clear(); closed = true;
            lock (gate) { active = null; }
        }
        catch
        {
            // Keep storage and roots alive when shutdown is incomplete. Opening
            // another owner would hide leaked pointers or unfinished core work.
            quarantined = true;
            throw;
        }
        finally { entering = false; }
    }
}

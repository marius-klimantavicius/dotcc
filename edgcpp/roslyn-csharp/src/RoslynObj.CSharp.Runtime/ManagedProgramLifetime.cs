namespace RoslynObj.CSharp.Runtime;

public enum ManagedSelectedProgramLifetimeState
{
    New,
    Initializing,
    Ready,
    ShuttingDown,
    Shutdown,
    InitializationFailed,
    ShutdownFailed
}

/// <summary>
/// Explicit startup and finalization for one admitted generated program.
/// Callers supply the captured startup order and exact managed destructor,
/// object address and DSO address. This class neither discovers a schedule nor
/// owns or frees the borrowed application storage.
/// </summary>
public sealed class ManagedSelectedProgramLifetime
{
    private readonly object gate = new();
    private readonly List<Registration> registrations = [];
    private ManagedSelectedProgramLifetimeState state;
    private int owningThread;
    private long initializeCalls, startupInvocations, startupCompletions, startupFailures;
    private long registerCalls, registered, registrationFailures;
    private long shutdownCalls, shutdownInvocations, destructorStarts, destructorCompletions;
    private long shutdownCompletions, shutdownFailures, protocolRejections;
    private long sequence, startupStartSequence, lastRegistrationSequence, startupCompleteSequence;
    private long shutdownStartSequence, lastDestructorStartSequence, lastDestructorCompleteSequence, shutdownCompleteSequence;
    private long lastRegisteredEntry, lastStartedEntry, lastCompletedEntry;
    private nint lastRegisteredObject, lastRegisteredDso, lastDestroyedObject, lastDestroyedDso;

    private readonly record struct Registration(long Entry, Action<nint> Destructor, nint ObjectAddress, nint DsoAddress);

    public ManagedSelectedProgramLifetimeState State
    {
        get { lock (gate) return state; }
    }

    /// <summary>
    /// Runs the explicitly supplied startup once. A repeat after success does
    /// nothing. An unexpected managed exception is propagated directly and
    /// latches failure, retaining successful registrations without running them.
    /// This is not a C++ failed-initialization recovery implementation.
    /// </summary>
    public void Initialize(Action runStartup)
    {
        ArgumentNullException.ThrowIfNull(runStartup);
        lock (gate)
        {
            ++initializeCalls;
            if (state == ManagedSelectedProgramLifetimeState.Ready)
            {
                RequireOwningThread();
                return;
            }
            RequireState(ManagedSelectedProgramLifetimeState.New, "initialize");
            state = ManagedSelectedProgramLifetimeState.Initializing;
            owningThread = Environment.CurrentManagedThreadId;
            ++startupInvocations;
            startupStartSequence = ++sequence;
        }
        try
        {
            runStartup();
        }
        catch
        {
            lock (gate)
            {
                state = ManagedSelectedProgramLifetimeState.InitializationFailed;
                ++startupFailures;
                ++sequence;
            }
            throw;
        }
        lock (gate)
        {
            // A failed registration is an actual nonzero source result. If the
            // source ignores it and returns, startup still completed normally;
            // the unregistered object has no synthesized destructor entry.
            state = ManagedSelectedProgramLifetimeState.Ready;
            ++startupCompletions;
            startupCompleteSequence = ++sequence;
        }
    }

    /// <summary>
    /// Projects an admitted __cxa_atexit call during this explicit startup.
    /// Success is returned only after the exact entry is appended. Allocation
    /// failure returns -1 without an entry; the source decides how to use that
    /// result. Object and module addresses remain opaque borrowed values.
    /// </summary>
    public int Register(Action<nint> destructor, nint objectAddress, nint dsoAddress)
    {
        ArgumentNullException.ThrowIfNull(destructor);
        lock (gate)
        {
            ++registerCalls;
            RequireState(ManagedSelectedProgramLifetimeState.Initializing, "register a destructor");
            RequireOwningThread();
            var entry = new Registration(registerCalls, destructor, objectAddress, dsoAddress);
            try
            {
                registrations.Add(entry);
            }
            catch (OutOfMemoryException)
            {
                ++registrationFailures;
                ++sequence;
                return -1;
            }
            ++registered;
            lastRegisteredEntry = entry.Entry;
            lastRegisteredObject = objectAddress;
            lastRegisteredDso = dsoAddress;
            lastRegistrationSequence = ++sequence;
            return 0;
        }
    }

    /// <summary>Requires successful explicit startup before an advertised root.</summary>
    public void RequireInitialized()
    {
        lock (gate)
        {
            RequireState(ManagedSelectedProgramLifetimeState.Ready, "enter a selected program root");
            RequireOwningThread();
        }
    }

    /// <summary>
    /// Invokes successful registrations in reverse order exactly once. A repeat
    /// after successful shutdown does nothing. An entry is removed before its
    /// destructor runs; an unexpected exception latches failure without retrying
    /// that destructor or speculatively draining the remaining entries.
    /// </summary>
    public void Shutdown()
    {
        lock (gate)
        {
            ++shutdownCalls;
            if (state == ManagedSelectedProgramLifetimeState.Shutdown)
            {
                RequireOwningThread();
                return;
            }
            RequireState(ManagedSelectedProgramLifetimeState.Ready, "shut down");
            RequireOwningThread();
            state = ManagedSelectedProgramLifetimeState.ShuttingDown;
            ++shutdownInvocations;
            shutdownStartSequence = ++sequence;
        }
        while (true)
        {
            Registration entry;
            lock (gate)
            {
                if (registrations.Count == 0)
                {
                    state = ManagedSelectedProgramLifetimeState.Shutdown;
                    ++shutdownCompletions;
                    shutdownCompleteSequence = ++sequence;
                    return;
                }
                var index = registrations.Count - 1;
                entry = registrations[index];
                registrations.RemoveAt(index);
                ++destructorStarts;
                lastStartedEntry = entry.Entry;
                lastDestructorStartSequence = ++sequence;
            }
            try
            {
                entry.Destructor(entry.ObjectAddress);
            }
            catch
            {
                lock (gate)
                {
                    state = ManagedSelectedProgramLifetimeState.ShutdownFailed;
                    ++shutdownFailures;
                    ++sequence;
                }
                throw;
            }
            lock (gate)
            {
                ++destructorCompletions;
                lastCompletedEntry = entry.Entry;
                lastDestroyedObject = entry.ObjectAddress;
                lastDestroyedDso = entry.DsoAddress;
                lastDestructorCompleteSequence = ++sequence;
            }
        }
    }

    /// <summary>
    /// Detached observations for finite checks. Entry numbers and addresses do
    /// not resolve ownership, linkage or a current program; no delegate is
    /// exposed or inspected. Last* fields describe only the latest event.
    /// </summary>
    public ManagedSelectedProgramLifetimeDiagnostics GetDiagnostics()
    {
        lock (gate)
            return new(state, owningThread, initializeCalls, startupInvocations, startupCompletions, startupFailures,
                registerCalls, registered, registrationFailures, shutdownCalls, shutdownInvocations,
                destructorStarts, destructorCompletions, shutdownCompletions, shutdownFailures,
                protocolRejections, registrations.Count, lastRegisteredEntry, lastStartedEntry, lastCompletedEntry,
                lastRegisteredObject, lastRegisteredDso, lastDestroyedObject, lastDestroyedDso,
                startupStartSequence, lastRegistrationSequence, startupCompleteSequence, shutdownStartSequence,
                lastDestructorStartSequence, lastDestructorCompleteSequence, shutdownCompleteSequence);
    }

    private void RequireState(ManagedSelectedProgramLifetimeState expected, string operation)
    {
        if (state != expected)
            Reject("Cannot " + operation + " while the selected program lifetime state is " + state + ".");
    }

    private void RequireOwningThread()
    {
        if (owningThread != Environment.CurrentManagedThreadId)
            Reject("Foreign-thread selected program execution is outside this single-thread lifetime profile.");
    }

    private void Reject(string message)
    {
        ++protocolRejections;
        throw new InvalidOperationException(message);
    }
}

public sealed record ManagedSelectedProgramLifetimeDiagnostics(
    ManagedSelectedProgramLifetimeState State,
    int OwningManagedThreadId,
    long InitializeCalls, long StartupInvocations, long StartupCompletions, long StartupFailures,
    long RegisterCalls, long Registered, long RegistrationFailures,
    long ShutdownCalls, long ShutdownInvocations, long DestructorStarts, long DestructorCompletions,
    long ShutdownCompletions, long ShutdownFailures, long ProtocolRejections, int PendingEntries,
    long LastRegisteredEntry, long LastStartedEntry, long LastCompletedEntry,
    nint LastRegisteredObjectAddress, nint LastRegisteredDsoAddress,
    nint LastDestroyedObjectAddress, nint LastDestroyedDsoAddress,
    long StartupStartSequence, long LastRegistrationSequence, long StartupCompleteSequence,
    long ShutdownStartSequence, long LastDestructorStartSequence,
    long LastDestructorCompleteSequence, long ShutdownCompleteSequence);

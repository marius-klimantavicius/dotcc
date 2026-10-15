using System.Runtime.InteropServices;
using System.Threading;

namespace RoslynObj.CSharp.Runtime;

/// <summary>
/// A generated-only exception carrier for the admitted exact signed-32-bit
/// payload. Generated tokens represent captured semantic identities. This is
/// not a native exception ABI, general C++ type matcher or exception stack.
/// </summary>
public sealed unsafe class ManagedCppException : Exception
{
    private enum EntryState { Pending, Committed, Selected, Caught, Released }

    private sealed class PrimaryOwner
    {
        internal readonly object ExactTypeToken;
        internal readonly int CreatorThread;
        internal readonly long DiagnosticIdentity;
        internal void* Payload;
        internal int References;

        internal PrimaryOwner(object typeToken)
        {
            ExactTypeToken = typeToken;
            CreatorThread = Environment.CurrentManagedThreadId;
            Payload = NativeMemory.Alloc((nuint)sizeof(int));
            if (Payload == null) throw new OutOfMemoryException();
            if (((nuint)Payload & 3) != 0)
            {
                var unaligned = Payload; Payload = null; NativeMemory.Free(unaligned);
                throw new PlatformNotSupportedException("The exception payload requires natural 4-byte alignment.");
            }
            References = 1;
            DiagnosticIdentity = Interlocked.Increment(ref created);
            Interlocked.Increment(ref liveOwners);
            Interlocked.Increment(ref liveReferences);
            Volatile.Write(ref lastCreatedPrimaryOwner, DiagnosticIdentity);
        }
    }

    private readonly PrimaryOwner owner;
    private readonly ManagedCppException? predecessor;
    private ManagedCppException? rethrowChild;
    private EntryState state;
    private bool addressRequested;
    private object? selectedHandler;
    private long committedSequence, selectedSequence, firstCleanupSequence;

    private static long created, committed, selected, caught, released, pendingReleased, liveOwners;
    private static long unwindObservations, cleanupAssertions, selectionBeforeCleanupAssertions, protocolRejections;
    private static long sequence, lastSelectedSequence, lastFirstCleanupSequence, lastReleasedSequence;
    private static long rethrown, handlingEntriesReleased, liveReferences;
    private static long lastCreatedPrimaryOwner, lastRethrownPrimaryOwner, lastReleasedPrimaryOwner;

    private ManagedCppException(object typeToken) : base("Generated C++ integer exception.")
    {
        owner = new(typeToken);
        state = EntryState.Pending;
    }

    private ManagedCppException(ManagedCppException activeEntry) : base("Generated C++ integer rethrow.")
    {
        owner = activeEntry.owner;
        predecessor = activeEntry;
        state = EntryState.Committed;
        committedSequence = Interlocked.Increment(ref sequence);
    }

    /// <summary>Allocates one pending owner. The emitter separately admits the exact captured int type.</summary>
    public static ManagedCppException CreateInt32(object exactTypeToken)
    {
        ArgumentNullException.ThrowIfNull(exactTypeToken);
        return new(exactTypeToken);
    }

    /// <summary>Destination of the retained throw-initialization expression; pending ownership only.</summary>
    public void* ObjectAddress
    {
        get
        {
            RequireThread(); RequireState(EntryState.Pending);
            addressRequested = true;
            return owner.Payload;
        }
    }

    /// <summary>Transfers the initialized pending owner to one throw; call before throwing this carrier.</summary>
    public void CommitThrow()
    {
        RequireThread(); RequireState(EntryState.Pending);
        if (!addressRequested) Reject("The retained payload initializer has not requested its destination.");
        committedSequence = Interlocked.Increment(ref sequence);
        state = EntryState.Committed;
        Interlocked.Increment(ref committed);
    }

    /// <summary>
    /// Starts the one admitted rethrow from the original caught entry. The new
    /// propagation entry shares the primary payload; the active handler keeps
    /// its own reference until its retained no-object cleanup retires it.
    /// </summary>
    public ManagedCppException CreateRethrow(object activeHandlerToken)
    {
        RequireThread(); RequireHandler(activeHandlerToken); RequireState(EntryState.Caught);
        if (predecessor is not null || rethrowChild is not null || owner.References != 1)
            Reject("The bounded integer exception supports one rethrow from its original caught entry.");
        // Allocate the managed wrapper before retaining a second reference, so
        // an allocation failure cannot leave an unreturned propagation entry.
        var child = new ManagedCppException(this);
        owner.References++;
        rethrowChild = child;
        Interlocked.Increment(ref liveReferences);
        Interlocked.Increment(ref rethrown);
        Volatile.Write(ref lastRethrownPrimaryOwner, owner.DiagnosticIdentity);
        return child;
    }

    /// <summary>
    /// Preparation-finally cleanup. A committed owner remains owned by its
    /// selected exception path; repeated pending-cleanup attempts do not free it.
    /// </summary>
    public void ReleaseUncommitted()
    {
        RequireThread();
        if (state != EntryState.Pending) return;
        RetireReference(false);
        Interlocked.Increment(ref pendingReleased);
    }

    /// <summary>
    /// First-pass CLR filter: identity match and destination selection only.
    /// It runs before any generated exceptional-cleanup action.
    /// </summary>
    public bool SelectHandler(object exactTypeToken, object handlerToken)
    {
        ArgumentNullException.ThrowIfNull(exactTypeToken);
        ArgumentNullException.ThrowIfNull(handlerToken);
        RequireThread();
        if (state != EntryState.Committed || owner.Payload == null)
        {
            Reject("Only an unselected committed throw can select its one admitted handler.");
        }
        if (!ReferenceEquals(owner.ExactTypeToken, exactTypeToken)) return false;
        if (predecessor is not null && ReferenceEquals(predecessor.selectedHandler, handlerToken))
            Reject("A rethrow cannot select its already active source handler.");
        selectedHandler = handlerToken;
        selectedSequence = Interlocked.Increment(ref sequence);
        state = EntryState.Selected;
        Interlocked.Increment(ref selected);
        return true;
    }

    /// <summary>
    /// A function's false filter records the actual carrier without catching,
    /// selecting, cleaning or releasing it. Its finally executes after first-pass selection.
    /// </summary>
    public static bool ObserveUnwind(ManagedCppException exception, ref ManagedCppException? observed)
    {
        ArgumentNullException.ThrowIfNull(exception);
        observed = exception;
        Interlocked.Increment(ref unwindObservations);
        return false;
    }

    /// <summary>Required immediately before an encoded exceptional-unwind site.</summary>
    public void RequireSelectedHandler()
    {
        RequireThread(); RequireState(EntryState.Selected);
        if (selectedHandler is null || selectedSequence <= committedSequence || selectedSequence == 0)
            Reject("Exceptional cleanup requires a handler selected during the first pass.");
        if (firstCleanupSequence == 0) firstCleanupSequence = Interlocked.Increment(ref sequence);
        if (firstCleanupSequence <= selectedSequence)
            Reject("Exceptional cleanup preceded handler selection.");
        Interlocked.Increment(ref cleanupAssertions);
        Interlocked.Increment(ref selectionBeforeCleanupAssertions);
    }

    /// <summary>Stable source of this handler's explicit retained parameter copy, before MarkCaught.</summary>
    public void* CaughtObjectAddress(object handlerToken)
    {
        RequireThread(); RequireHandler(handlerToken);
        if (state is not (EntryState.Selected or EntryState.Caught) || owner.Payload == null)
            Reject("Caught-object storage is available only to the selected active handler.");
        return owner.Payload;
    }

    /// <summary>Marks the actual lowered point after the handler parameter has been copied.</summary>
    public void MarkCaught(object handlerToken)
    {
        RequireThread(); RequireHandler(handlerToken); RequireState(EntryState.Selected);
        if (predecessor is not null && predecessor.state != EntryState.Released)
            Reject("The rethrow source handling entry must retire before the destination is marked caught.");
        state = EntryState.Caught;
        Interlocked.Increment(ref caught);
    }

    /// <summary>
    /// Retires this handler's eligible reference at catch_epilogue or its
    /// admitted no-object cleanup. Only the last reference frees the primary
    /// payload. The emitter supplies context, not a native pointer argument.
    /// </summary>
    public void ReleaseCaught(object handlerToken)
    {
        RequireThread(); RequireHandler(handlerToken); RequireState(EntryState.Caught);
        if (rethrowChild is not null)
        {
            if (rethrowChild.state != EntryState.Selected || owner.References != 2)
                Reject("Rethrow destination selection must precede retirement of the source handler.");
        }
        else if (owner.References != 1)
            Reject("The caught exception has an unsupported primary-reference count.");
        RetireReference(true);
    }

    private void RequireThread()
    {
        if (owner.CreatorThread != Environment.CurrentManagedThreadId)
            Reject("The bounded exception owner cannot transfer to another managed thread.");
    }

    private void RequireState(EntryState expected)
    {
        if (state != expected || owner.Payload == null) Reject("The exception entry has an unsupported lifecycle transition.");
    }

    private void RequireHandler(object handlerToken)
    {
        ArgumentNullException.ThrowIfNull(handlerToken);
        if (selectedHandler is null || !ReferenceEquals(selectedHandler, handlerToken))
            Reject("The exception owner does not belong to this handler.");
    }

    private static void Reject(string message)
    {
        Interlocked.Increment(ref protocolRejections);
        throw new InvalidOperationException(message);
    }

    private void RetireReference(bool handled)
    {
        if (state == EntryState.Released || owner.Payload == null || owner.References <= 0)
            Reject("The exception entry or primary payload has already been released.");
        state = EntryState.Released;
        owner.References--;
        Interlocked.Decrement(ref liveReferences);
        if (handled) Interlocked.Increment(ref handlingEntriesReleased);
        if (owner.References != 0) return;
        var owned = owner.Payload; owner.Payload = null;
        NativeMemory.Free(owned);
        Volatile.Write(ref lastSelectedSequence, selectedSequence);
        Volatile.Write(ref lastFirstCleanupSequence, firstCleanupSequence);
        Volatile.Write(ref lastReleasedSequence, Interlocked.Increment(ref sequence));
        Volatile.Write(ref lastReleasedPrimaryOwner, owner.DiagnosticIdentity);
        Interlocked.Increment(ref released);
        Interlocked.Decrement(ref liveOwners);
    }

    /// <summary>Process-wide diagnostic counters, never used to resolve or select ownership.</summary>
    public static ManagedCppExceptionDiagnostics GetDiagnostics() => new(
        Interlocked.Read(ref created), Interlocked.Read(ref committed), Interlocked.Read(ref selected),
        Interlocked.Read(ref caught), Interlocked.Read(ref released), Interlocked.Read(ref pendingReleased),
        Interlocked.Read(ref liveOwners), Interlocked.Read(ref unwindObservations), Interlocked.Read(ref cleanupAssertions),
        Interlocked.Read(ref selectionBeforeCleanupAssertions), Interlocked.Read(ref protocolRejections),
        Interlocked.Read(ref lastSelectedSequence), Interlocked.Read(ref lastFirstCleanupSequence), Interlocked.Read(ref lastReleasedSequence));

    /// <summary>
    /// Additive diagnostics for the bounded shared-owner rethrow. Identities are
    /// opaque observation labels, never keys for finding or selecting an owner.
    /// </summary>
    public static ManagedCppExceptionRethrowDiagnostics GetRethrowDiagnostics() => new(
        Interlocked.Read(ref rethrown), Interlocked.Read(ref handlingEntriesReleased), Interlocked.Read(ref liveReferences),
        Interlocked.Read(ref lastCreatedPrimaryOwner), Interlocked.Read(ref lastRethrownPrimaryOwner),
        Interlocked.Read(ref lastReleasedPrimaryOwner));
}

public readonly record struct ManagedCppExceptionDiagnostics(
    long Created, long Committed, long Selected, long Caught, long Released, long PendingReleased,
    long LiveOwners, long UnwindObservations, long CleanupAssertions, long SelectionBeforeCleanupAssertions,
    long ProtocolRejections, long LastSelectedSequence, long LastFirstCleanupSequence, long LastReleasedSequence);

public readonly record struct ManagedCppExceptionRethrowDiagnostics(
    long Rethrown, long HandlingEntriesReleased, long LiveReferences,
    long LastCreatedPrimaryOwner, long LastRethrownPrimaryOwner, long LastReleasedPrimaryOwner);

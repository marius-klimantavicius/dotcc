using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace RoslynObj.CSharp.Runtime;

public enum ManagedCppTerminationPolicy
{
    UnwindBeforeTerminateDefaultFailFastV1 = 1
}

/// <summary>
/// A generated-only exception carrier for the admitted signed-32-bit payload
/// and bounded record payload. Generated tokens represent captured semantic
/// identities. This is not a native exception ABI, general C++ type matcher
/// or exception stack.
/// </summary>
public sealed unsafe class ManagedCppException : Exception
{
    private enum EntryState { Pending, Committed, Selected, Caught, Released, NoexceptSelected }

    public const string DefaultNoexceptTerminationMarker =
        "RoslynObj.CSharp.ManagedCppException:default-noexcept-termination:unwind-true:failfast-v1";

    private sealed class PrimaryOwner
    {
        internal readonly object ExactTypeToken;
        internal readonly int CreatorThread;
        internal readonly long DiagnosticIdentity;
        internal readonly Action<nint>? DestroyRecord;
        internal void* Payload;
        internal int References;
        internal bool RecordEvaluationComplete;
        internal bool RecordCopyComplete;
        internal bool RecordDestructionStarted;

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

        internal PrimaryOwner(object typeToken, nuint size, nuint alignment, Action<nint> destroy)
        {
            ExactTypeToken = typeToken;
            CreatorThread = Environment.CurrentManagedThreadId;
            DestroyRecord = destroy;
            Payload = NativeMemory.Alloc(size);
            if (Payload == null) throw new OutOfMemoryException();
            var aligned = ((nuint)Payload & (alignment - 1)) == 0;
            if (!aligned)
            {
                var unaligned = Payload; Payload = null; NativeMemory.Free(unaligned);
                throw new PlatformNotSupportedException("The record exception payload requires natural 8-byte alignment.");
            }
            References = 1;
            DiagnosticIdentity = Interlocked.Increment(ref created);
            Interlocked.Increment(ref liveOwners);
            Interlocked.Increment(ref liveReferences);
            Volatile.Write(ref lastCreatedPrimaryOwner, DiagnosticIdentity);
            Interlocked.Increment(ref recordCreated);
            Interlocked.Increment(ref recordLiveOwners);
            Volatile.Write(ref lastRecordPrimaryOwner, DiagnosticIdentity);
            Volatile.Write(ref lastRecordPayloadSize, (long)size);
            Volatile.Write(ref lastRecordPayloadAlignment, (long)alignment);
            Volatile.Write(ref lastRecordPayloadWasAligned, aligned ? 1 : 0);
            Volatile.Write(ref lastRecordCreatedSequence, Interlocked.Increment(ref sequence));
        }
    }

    private readonly PrimaryOwner owner;
    private readonly ManagedCppException? predecessor;
    private ManagedCppException? rethrowChild;
    private EntryState state;
    private bool addressRequested;
    private object? selectedHandler;
    private object? selectedNoexceptBoundary;
    private ManagedCppTerminationPolicy selectedTerminationPolicy;
    private long committedSequence, selectedSequence, firstCleanupSequence;

    private static long created, committed, selected, caught, released, pendingReleased, liveOwners;
    private static long unwindObservations, cleanupAssertions, selectionBeforeCleanupAssertions, protocolRejections;
    private static long sequence, lastSelectedSequence, lastFirstCleanupSequence, lastReleasedSequence;
    private static long rethrown, handlingEntriesReleased, liveReferences;
    private static long lastCreatedPrimaryOwner, lastRethrownPrimaryOwner, lastReleasedPrimaryOwner;
    private static long recordCreated, recordEvaluationCompleted, recordCommitted, recordDestructorStarted, recordDestructorCompleted, recordReleased, recordLiveOwners;
    private static long lastRecordPrimaryOwner, lastRecordPayloadSize, lastRecordPayloadAlignment;
    private static int lastRecordPayloadWasAligned;
    private static long lastRecordCreatedSequence, lastRecordEvaluationCompletedSequence, lastRecordCommittedSequence, lastRecordSelectedSequence,
        lastRecordFirstCleanupSequence, lastRecordDestructorStartedSequence, lastRecordDestructorCompletedSequence, lastRecordReleasedSequence;

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

    private ManagedCppException(object typeToken, nuint size, nuint alignment, Action<nint> destroy) : base("Generated C++ record exception.")
    {
        owner = new(typeToken, size, alignment, destroy);
        state = EntryState.Pending;
    }

    /// <summary>Allocates one pending owner. The emitter separately admits the exact captured int type.</summary>
    public static ManagedCppException CreateInt32(object exactTypeToken)
    {
        ArgumentNullException.ThrowIfNull(exactTypeToken);
        return new(exactTypeToken);
    }

    /// <summary>
    /// Allocates the admitted exact complete 16/8 record. The emitter supplies
    /// an ordinary managed callback to its selected closed noexcept destructor.
    /// No native callback or source type lookup is involved.
    /// </summary>
    public static ManagedCppException CreateRecord(object exactTypeToken, nuint size, nuint alignment, Action<nint> destroy)
    {
        ArgumentNullException.ThrowIfNull(exactTypeToken);
        ArgumentNullException.ThrowIfNull(destroy);
        if (size != 16 || alignment != 8 || sizeof(nint) != 8 || !BitConverter.IsLittleEndian)
            Reject("The bounded record exception owner admits only its exact complete 16/8 storage on a little-endian 64-bit host.");
        return new(exactTypeToken, size, alignment, destroy);
    }

    /// <summary>
    /// Records the actual exception_started occurrence before the selected
    /// copy constructor. This is separate from successful copy/throw commit.
    /// </summary>
    public void MarkRecordEvaluationComplete()
    {
        RequireThread(); RequireState(EntryState.Pending);
        if (owner.DestroyRecord is null || owner.RecordEvaluationComplete)
            Reject("The record evaluation marker requires one pending record owner and cannot repeat.");
        owner.RecordEvaluationComplete = true;
        Interlocked.Increment(ref recordEvaluationCompleted);
        Volatile.Write(ref lastRecordEvaluationCompletedSequence, Interlocked.Increment(ref sequence));
    }

    /// <summary>Destination of the retained throw-initialization expression; pending ownership only.</summary>
    public void* ObjectAddress
    {
        get
        {
            RequireThread(); RequireState(EntryState.Pending);
            if (owner.DestroyRecord is not null && !owner.RecordEvaluationComplete)
                Reject("The record destination requires its retained evaluation-complete marker before copy initialization.");
            addressRequested = true;
            return owner.Payload;
        }
    }

    /// <summary>Transfers the initialized pending owner to one throw; call before throwing this carrier.</summary>
    public void CommitThrow()
    {
        RequireThread(); RequireState(EntryState.Pending);
        if (!addressRequested) Reject("The retained payload initializer has not requested its destination.");
        if (owner.DestroyRecord is not null && !owner.RecordEvaluationComplete)
            Reject("The record copy cannot commit without its retained evaluation marker.");
        committedSequence = Interlocked.Increment(ref sequence);
        state = EntryState.Committed;
        Interlocked.Increment(ref committed);
        if (owner.DestroyRecord is not null)
        {
            owner.RecordCopyComplete = true;
            Interlocked.Increment(ref recordCommitted);
            Volatile.Write(ref lastRecordCommittedSequence, committedSequence);
        }
    }

    /// <summary>
    /// Starts the one admitted rethrow from the original caught entry. The new
    /// propagation entry shares the primary payload; the active handler keeps
    /// its own reference until its retained no-object cleanup retires it.
    /// </summary>
    public ManagedCppException CreateRethrow(object activeHandlerToken)
    {
        RequireThread(); RequireHandler(activeHandlerToken); RequireState(EntryState.Caught);
        if (owner.DestroyRecord is not null) Reject("Record exception rethrow is outside the admitted record owner contract.");
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
        if (owner.DestroyRecord is not null) Volatile.Write(ref lastRecordSelectedSequence, selectedSequence);
        return true;
    }

    /// <summary>
    /// First-pass selection of the admitted source noexcept boundary. This is
    /// a termination destination, not a typed catch or payload-release action.
    /// </summary>
    public bool SelectNoexceptBoundary(object boundaryToken, ManagedCppTerminationPolicy policy)
    {
        RequireThread(); RequireState(EntryState.Committed);
        if (owner.DestroyRecord is not null) Reject("Record exception termination is outside the admitted record owner contract.");
        RequireTerminationPolicy(policy);
        if (boundaryToken is null || predecessor is not null || rethrowChild is not null || owner.References != 1)
            Reject("The bounded noexcept destination requires one original committed throw and an exact boundary token.");
        selectedNoexceptBoundary = boundaryToken;
        selectedTerminationPolicy = policy;
        selectedSequence = Interlocked.Increment(ref sequence);
        state = EntryState.NoexceptSelected;
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
        if (owner.DestroyRecord is not null) Volatile.Write(ref lastRecordFirstCleanupSequence, firstCleanupSequence);
        Interlocked.Increment(ref cleanupAssertions);
        Interlocked.Increment(ref selectionBeforeCleanupAssertions);
    }

    /// <summary>
    /// Cleanup assertion for the explicitly admitted termination profile.
    /// Existing typed-handler assertions keep their original behavior.
    /// </summary>
    public void RequireSelectedDestination()
    {
        if (state == EntryState.Selected)
        {
            RequireSelectedHandler();
            return;
        }
        RequireThread(); RequireState(EntryState.NoexceptSelected);
        RequireTerminationPolicy(selectedTerminationPolicy);
        if (selectedNoexceptBoundary is null || selectedSequence <= committedSequence || selectedSequence == 0)
            Reject("Exceptional cleanup requires a noexcept destination selected during the first pass.");
        if (firstCleanupSequence == 0) firstCleanupSequence = Interlocked.Increment(ref sequence);
        if (firstCleanupSequence <= selectedSequence)
            Reject("Exceptional cleanup preceded noexcept destination selection.");
        Interlocked.Increment(ref cleanupAssertions);
        Interlocked.Increment(ref selectionBeforeCleanupAssertions);
    }

    /// <summary>
    /// Performs the selected default-only host termination after encoded
    /// unwind cleanup. The process still owns the committed integer payload.
    /// </summary>
    [DoesNotReturn]
    public void TerminateNoexceptBoundary(object boundaryToken, ManagedCppTerminationPolicy policy)
    {
        RequireThread(); RequireState(EntryState.NoexceptSelected);
        RequireTerminationPolicy(policy);
        if (boundaryToken is null || !ReferenceEquals(selectedNoexceptBoundary, boundaryToken) ||
            selectedTerminationPolicy != policy || firstCleanupSequence <= selectedSequence || firstCleanupSequence == 0)
            Reject("Noexcept termination requires its exact selected boundary/policy after encoded cleanup.");
        Environment.FailFast(DefaultNoexceptTerminationMarker);
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

    private static void RequireTerminationPolicy(ManagedCppTerminationPolicy policy)
    {
        if (policy != ManagedCppTerminationPolicy.UnwindBeforeTerminateDefaultFailFastV1)
            Reject("The generated noexcept termination policy is unsupported.");
    }

    private void RetireReference(bool handled)
    {
        if (state == EntryState.Released || owner.Payload == null || owner.References <= 0)
            Reject("The exception entry or primary payload has already been released.");
        if (owner.DestroyRecord is not null && handled &&
            (!owner.RecordCopyComplete || !owner.RecordEvaluationComplete || owner.RecordDestructionStarted ||
             state != EntryState.Caught || owner.References != 1 || firstCleanupSequence <= selectedSequence || firstCleanupSequence == 0))
            Reject("Record destruction requires its completed copy, selected active handler and encoded cleanup before final retirement.");
        state = EntryState.Released;
        owner.References--;
        Interlocked.Decrement(ref liveReferences);
        if (handled) Interlocked.Increment(ref handlingEntriesReleased);
        if (owner.References != 0) return;
        try
        {
            if (owner.DestroyRecord is { } destroy && handled)
            {
                // The pending path deliberately skips this branch. Generated
                // admission proves that the completed destructor cannot throw.
                owner.RecordDestructionStarted = true;
                Interlocked.Increment(ref recordDestructorStarted);
                Volatile.Write(ref lastRecordDestructorStartedSequence, Interlocked.Increment(ref sequence));
                try { destroy((nint)owner.Payload); }
                catch (Exception error)
                {
                    Interlocked.Increment(ref protocolRejections);
                    throw new InvalidOperationException("A throwing record destructor is outside the admitted generated exception contract.", error);
                }
                Interlocked.Increment(ref recordDestructorCompleted);
                Volatile.Write(ref lastRecordDestructorCompletedSequence, Interlocked.Increment(ref sequence));
            }
        }
        finally
        {
            // Clear ownership before free; the selected destructor has already
            // run with stable valid bytes, or pending initialization never committed.
            var owned = owner.Payload; owner.Payload = null;
            NativeMemory.Free(owned);
            Volatile.Write(ref lastSelectedSequence, selectedSequence);
            Volatile.Write(ref lastFirstCleanupSequence, firstCleanupSequence);
            var releaseSequence = Interlocked.Increment(ref sequence);
            Volatile.Write(ref lastReleasedSequence, releaseSequence);
            Volatile.Write(ref lastReleasedPrimaryOwner, owner.DiagnosticIdentity);
            Interlocked.Increment(ref released);
            Interlocked.Decrement(ref liveOwners);
            if (owner.DestroyRecord is not null)
            {
                Interlocked.Increment(ref recordReleased);
                Interlocked.Decrement(ref recordLiveOwners);
                Volatile.Write(ref lastRecordReleasedSequence, releaseSequence);
            }
        }
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

    /// <summary>Record-only observations. Primary identities are diagnostic labels, not ownership lookup keys.</summary>
    public static ManagedCppExceptionRecordDiagnostics GetRecordDiagnostics() => new(
        Interlocked.Read(ref recordCreated), Interlocked.Read(ref recordEvaluationCompleted), Interlocked.Read(ref recordCommitted),
        Interlocked.Read(ref recordDestructorStarted), Interlocked.Read(ref recordDestructorCompleted), Interlocked.Read(ref recordReleased),
        Interlocked.Read(ref recordLiveOwners), Interlocked.Read(ref lastRecordPrimaryOwner),
        (ulong)Interlocked.Read(ref lastRecordPayloadSize), (ulong)Interlocked.Read(ref lastRecordPayloadAlignment),
        Volatile.Read(ref lastRecordPayloadWasAligned) != 0,
        Interlocked.Read(ref lastRecordCreatedSequence), Interlocked.Read(ref lastRecordEvaluationCompletedSequence),
        Interlocked.Read(ref lastRecordCommittedSequence), Interlocked.Read(ref lastRecordSelectedSequence),
        Interlocked.Read(ref lastRecordFirstCleanupSequence), Interlocked.Read(ref lastRecordDestructorStartedSequence),
        Interlocked.Read(ref lastRecordDestructorCompletedSequence), Interlocked.Read(ref lastRecordReleasedSequence));
}

public readonly record struct ManagedCppExceptionDiagnostics(
    long Created, long Committed, long Selected, long Caught, long Released, long PendingReleased,
    long LiveOwners, long UnwindObservations, long CleanupAssertions, long SelectionBeforeCleanupAssertions,
    long ProtocolRejections, long LastSelectedSequence, long LastFirstCleanupSequence, long LastReleasedSequence);

public readonly record struct ManagedCppExceptionRethrowDiagnostics(
    long Rethrown, long HandlingEntriesReleased, long LiveReferences,
    long LastCreatedPrimaryOwner, long LastRethrownPrimaryOwner, long LastReleasedPrimaryOwner);

public readonly record struct ManagedCppExceptionRecordDiagnostics(
    long Created, long EvaluationCompleted, long Committed, long DestructorStarted, long DestructorCompleted, long Released,
    long LiveOwners, long LastPrimaryOwner, ulong LastPayloadSize, ulong LastPayloadAlignment, bool LastPayloadWasAligned,
    long LastCreatedSequence, long LastEvaluationCompletedSequence, long LastCommittedSequence, long LastSelectedSequence,
    long LastFirstCleanupSequence, long LastDestructorStartedSequence, long LastDestructorCompletedSequence, long LastReleasedSequence);

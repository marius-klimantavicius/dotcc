using System.Runtime.InteropServices;
using System.Threading;

namespace RoslynObj.CSharp.Runtime;

/// <summary>
/// A generated-only exception carrier for the admitted exact signed-32-bit
/// payload. Generated tokens represent captured semantic identities. This is
/// not a native exception ABI, general C++ type matcher or rethrow stack.
/// </summary>
public sealed unsafe class ManagedCppException : Exception
{
    private enum OwnerState { Pending, Committed, Selected, Caught, Released }

    private readonly object exactTypeToken;
    private readonly int creatorThread;
    private void* payload;
    private OwnerState state;
    private bool addressRequested;
    private object? selectedHandler;
    private long committedSequence, selectedSequence, firstCleanupSequence;

    private static long created, committed, selected, caught, released, pendingReleased, liveOwners;
    private static long unwindObservations, cleanupAssertions, selectionBeforeCleanupAssertions, protocolRejections;
    private static long sequence, lastSelectedSequence, lastFirstCleanupSequence, lastReleasedSequence;

    private ManagedCppException(object typeToken) : base("Generated C++ integer exception.")
    {
        exactTypeToken = typeToken;
        creatorThread = Environment.CurrentManagedThreadId;
        payload = NativeMemory.Alloc((nuint)sizeof(int));
        if (payload == null) throw new OutOfMemoryException();
        if (((nuint)payload & 3) != 0)
        {
            var unaligned = payload; payload = null; NativeMemory.Free(unaligned);
            throw new PlatformNotSupportedException("The exception payload requires natural 4-byte alignment.");
        }
        state = OwnerState.Pending;
        Interlocked.Increment(ref created);
        Interlocked.Increment(ref liveOwners);
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
            RequireThread(); RequireState(OwnerState.Pending);
            addressRequested = true;
            return payload;
        }
    }

    /// <summary>Transfers the initialized pending owner to one throw; call before throwing this carrier.</summary>
    public void CommitThrow()
    {
        RequireThread(); RequireState(OwnerState.Pending);
        if (!addressRequested) Reject("The retained payload initializer has not requested its destination.");
        committedSequence = Interlocked.Increment(ref sequence);
        state = OwnerState.Committed;
        Interlocked.Increment(ref committed);
    }

    /// <summary>
    /// Preparation-finally cleanup. A committed owner remains owned by its
    /// selected exception path; repeated pending-cleanup attempts do not free it.
    /// </summary>
    public void ReleaseUncommitted()
    {
        RequireThread();
        if (state != OwnerState.Pending) return;
        ReleasePayload();
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
        if (state != OwnerState.Committed)
        {
            Reject("Only an unselected committed throw can select its one admitted handler.");
        }
        if (!ReferenceEquals(this.exactTypeToken, exactTypeToken)) return false;
        selectedHandler = handlerToken;
        selectedSequence = Interlocked.Increment(ref sequence);
        state = OwnerState.Selected;
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
        RequireThread(); RequireState(OwnerState.Selected);
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
        if (state is not (OwnerState.Selected or OwnerState.Caught))
            Reject("Caught-object storage is available only to the selected active handler.");
        return payload;
    }

    /// <summary>Marks the actual lowered point after the handler parameter has been copied.</summary>
    public void MarkCaught(object handlerToken)
    {
        RequireThread(); RequireHandler(handlerToken); RequireState(OwnerState.Selected);
        state = OwnerState.Caught;
        Interlocked.Increment(ref caught);
    }

    /// <summary>
    /// Releases the selected eligible owner at catch_epilogue or its admitted
    /// no-object cleanup action. The emitter supplies context, not a native pointer argument.
    /// </summary>
    public void ReleaseCaught(object handlerToken)
    {
        RequireThread(); RequireHandler(handlerToken); RequireState(OwnerState.Caught);
        ReleasePayload();
    }

    private void RequireThread()
    {
        if (creatorThread != Environment.CurrentManagedThreadId)
            Reject("The bounded exception owner cannot transfer to another managed thread.");
    }

    private void RequireState(OwnerState expected)
    {
        if (state != expected || payload == null) Reject("The exception owner has an unsupported lifecycle transition.");
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

    private void ReleasePayload()
    {
        if (state == OwnerState.Released || payload == null) Reject("The exception payload has already been released.");
        var owned = payload; payload = null; state = OwnerState.Released;
        NativeMemory.Free(owned);
        Volatile.Write(ref lastSelectedSequence, selectedSequence);
        Volatile.Write(ref lastFirstCleanupSequence, firstCleanupSequence);
        Volatile.Write(ref lastReleasedSequence, Interlocked.Increment(ref sequence));
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
}

public readonly record struct ManagedCppExceptionDiagnostics(
    long Created, long Committed, long Selected, long Caught, long Released, long PendingReleased,
    long LiveOwners, long UnwindObservations, long CleanupAssertions, long SelectionBeforeCleanupAssertions,
    long ProtocolRejections, long LastSelectedSequence, long LastFirstCleanupSequence, long LastReleasedSequence);

// Shared implementation for the owned dotobj exception contract.
// No reflection, dynamic code, native ABI exception machinery, or pinned heap payloads.
#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

public static unsafe partial class CppExceptions
{
    [ThreadStatic] private static Handler? currentHandler;
    [ThreadStatic] private static int uncaught;

    /// <summary>Number of started throws/rethrows not yet past catch parameter initialization on this thread.</summary>
    public static int UncaughtExceptions => uncaught;

    [DoesNotReturn]
    private static void Terminate(string message, Exception? error = null)
        => Environment.FailFast("C++ exception runtime: " + message, error);

    private sealed class Payload : CppException
    {
        internal readonly CppExceptionType Type;
        internal readonly int OwnerThread = Environment.CurrentManagedThreadId;
        internal readonly delegate* managed<void*, void> Destructor;
        internal void* Memory;
        internal bool Started, Complete, Released;
        // Several rethrows of the SAME object can be pending at once: a
        // destructor may rethrow and catch it while an outer rethrow unwinds.
        internal int Pending, Handlers;
        public override void* ObjectAddress => Memory;

        internal Payload(CppExceptionType type, delegate* managed<void*, void> destructor)
        {
            Type = type;
            Destructor = destructor;
            nuint alignment = type.PayloadAlignment;
            if (alignment == 0 || (alignment & (alignment - 1)) != 0 || type.PayloadSize == 0 ||
                type.RequiresDestruction != (destructor != null))
                Terminate("invalid payload layout or destructor contract");
            alignment = nuint.Max(alignment, (nuint)sizeof(void*));
            nuint size = checked((checked(type.PayloadSize + alignment - 1)) & ~(alignment - 1));
            Memory = NativeMemory.AlignedAlloc(size, alignment);
            if (Memory == null) Terminate("cannot allocate exception payload");
        }

        internal void Check()
        {
            if (Released || OwnerThread != Environment.CurrentManagedThreadId)
                Terminate("released payload or unsupported cross-thread transfer");
        }

        internal void ReleaseIfUnused()
        {
            if (Pending != 0 || Handlers != 0) return;
            Check();
            Released = true;
            // Retain the address until the callback completes. Destruction of
            // the exception object is not allowed to escape with an exception.
            try
            {
                if (Complete && Destructor != null) Destructor(Memory);
            }
            catch (Exception error) { Terminate("exception object destructor failed", error); }
            finally { NativeMemory.AlignedFree(Memory); Memory = null; }
        }
    }

    private sealed class Handler : CppTypedCatch
    {
        internal readonly Exception Error;
        internal readonly Handler? Previous;
        internal readonly int OwnerThread = Environment.CurrentManagedThreadId;
        internal readonly Payload? Payload;
        internal bool IsActive = true, Caught;
        internal void* Address;
        internal void* PointerCell;
        public override bool Active => IsActive;
        public override void* ObjectAddress => Address;
        internal Handler(Exception error)
        {
            Error = error;
            Previous = currentHandler;
            Payload = error as Payload;
        }
        internal void Check()
        {
            if (!IsActive || OwnerThread != Environment.CurrentManagedThreadId || !ReferenceEquals(currentHandler, this))
                Terminate("invalid active handler order");
        }
    }

    private static Payload Owned(CppException exception)
    {
        if (exception is not Payload payload) Terminate("foreign C++ exception provider");
        payload = (Payload)exception;
        payload.Check();
        return payload;
    }

    public static partial CppException Allocate(CppExceptionType type, delegate* managed<void*, void> completeDestructor)
    {
        try { return new Payload(type, completeDestructor); }
        catch (Exception error) { Terminate("cannot allocate exception payload", error); throw; }
    }

    public static partial void EnsureStarted(CppException payload)
    {
        Payload p = Owned(payload);
        if (p.Started) return;
        p.Started = true;
        if (uncaught == int.MaxValue) Terminate("uncaught counter overflow");
        ++uncaught;
        ++p.Pending;
    }

    public static partial void MarkComplete(CppException payload)
    {
        Payload p = Owned(payload);
        if (!p.Started || p.Complete) Terminate("invalid exception completion");
        p.Complete = true;
    }

    public static partial bool FilterInitializationFailure(CppException payload, Exception error)
    {
        if (ReferenceEquals(payload, error)) return false;
        Payload p = Owned(payload);
        // Forced platform/runtime transfers must still pass through finally
        // cleanup, but are not C++ throws and do not trigger terminate here.
        if (p.Started && !ObjectProgram.IsRuntimeTransfer(error))
            Terminate("throw operand initialization failed", error);
        return true;
    }

    public static partial void ReleaseIncomplete(CppException payload)
    {
        Payload p = Owned(payload);
        if (p.Complete || p.Handlers != 0) Terminate("completed payload on initialization failure path");
        if (p.Started) { --p.Pending; --uncaught; }
        p.ReleaseIfUnused();
    }

    public static partial Exception Rethrow()
    {
        Handler? frame = currentHandler;
        if (frame is null) Terminate("rethrow without an active handler");
        frame.Check();
        if (frame.Payload is Payload p)
        {
            p.Check();
            if (uncaught == int.MaxValue || p.Pending == int.MaxValue) Terminate("rethrow counter overflow");
            ++p.Pending;
            ++uncaught;
        }
        // C++ identity concerns the payload, not a managed stack trace. Avoid
        // allocating ExceptionDispatchInfo after committing propagation state.
        throw frame.Error;
    }

    private static bool Match(Payload payload, CppCatchType type, out nuint offset)
    {
        offset = 0;
        CppExceptionType from = payload.Type;
        if (StringComparer.Ordinal.Equals(from.IdentityKey, type.IdentityKey)) return true;
        ReadOnlySpan<int> fq = from.PointerQualifiers.Span, tq = type.PointerQualifiers.Span;
        if (fq.Length != tq.Length) return false;
        for (int i = 0; i < fq.Length; ++i)
        {
            if ((fq[i] & ~tq[i]) != 0) return false;
            // A qualification addition at an inner level requires const at
            // every intervening pointer level (T** cannot become const T**).
            if (fq[i] != tq[i]) for (int j = 0; j < i; ++j) if ((tq[j] & 1) == 0) return false;
        }
        if (StringComparer.Ordinal.Equals(from.LeafIdentityKey, type.LeafIdentityKey)) return true;
        if (fq.Length == 1 && type.LeafKind == "void") return true;
        if (fq.Length > 1 || from.LeafKind != "record" || type.LeafKind != "record") return false;
        foreach (CppExceptionBase candidate in from.Bases.Span)
            if (StringComparer.Ordinal.Equals(candidate.IdentityKey, type.LeafIdentityKey) && candidate.IsPublic && !candidate.IsAmbiguous)
            { offset = candidate.ByteOffset; return true; }
        return false;
    }

    public static partial bool FilterTypedCatch(Exception error, CppCatchType type)
        => error is Payload payload && !ObjectProgram.IsRuntimeTransfer(error) && Match(payload, type, out _);

    public static partial bool FilterCatchAll(Exception error) => !ObjectProgram.IsRuntimeTransfer(error);

    public static partial CppCatch EnterCatch(Exception error) => Enter(error, null);

    public static partial CppTypedCatch EnterTypedCatch(Exception error, CppCatchType type) => Enter(error, type);

    private static Handler Enter(Exception error, CppCatchType? type)
    {
        try
        {
            if (ObjectProgram.IsRuntimeTransfer(error)) Terminate("runtime transfer entered source handler", error);
            var frame = new Handler(error);
            if (frame.Payload is Payload p)
            {
                p.Check();
                if (!p.Complete || p.Pending == 0) Terminate("handler entered without a propagating completed payload");
                nuint offset = 0;
                if (type is not null && !Match(p, type, out offset)) Terminate("selected exception match changed");
                if (type is not null && type.PointerQualifiers.Length != 0)
                {
                    frame.PointerCell = NativeMemory.Alloc((nuint)sizeof(void*));
                    if (frame.PointerCell == null) Terminate("cannot allocate converted catch pointer");
                    void* pointer = *(void**)p.Memory;
                    *(void**)frame.PointerCell = pointer == null ? null : (byte*)pointer + offset;
                    frame.Address = frame.PointerCell;
                }
                else frame.Address = (byte*)p.Memory + offset;
                if (p.Handlers == int.MaxValue) Terminate("handler counter overflow");
                ++p.Handlers;
            }
            else if (type is not null || error is CppException) Terminate("foreign typed exception provider", error);
            currentHandler = frame;
            return frame;
        }
        catch (Exception failure) { Terminate("cannot enter exception handler", failure); throw; }
    }

    public static partial void MarkCaught(CppCatch frame)
    {
        var f = (Handler)frame;
        f.Check();
        if (f.Caught) Terminate("exception marked caught twice");
        f.Caught = true;
        if (f.Payload is Payload p)
        {
            if (p.Pending <= 0 || uncaught <= 0) Terminate("invalid uncaught counter");
            --p.Pending;
            --uncaught;
        }
    }

    public static partial bool FilterHandlerFailure(CppCatch frame, Exception error)
    {
        var f = (Handler)frame;
        f.Check();
        if (!f.Caught && !ObjectProgram.IsRuntimeTransfer(error))
            Terminate("catch parameter initialization failed", error);
        return !ObjectProgram.IsRuntimeTransfer(error);
    }

    public static partial void LeaveCatch(CppCatch frame)
    {
        var f = (Handler)frame;
        if (!f.IsActive) return;
        f.Check();
        currentHandler = f.Previous;
        f.IsActive = false;
        NativeMemory.Free(f.PointerCell);
        f.PointerCell = null;
        f.Address = null;
        if (f.Payload is Payload p)
        {
            // A forced runtime transfer can abandon catch initialization
            // before the source __exception_caught marker was reached.
            if (!f.Caught) { --p.Pending; --uncaught; }
            --p.Handlers;
            p.ReleaseIfUnused();
        }
    }
}

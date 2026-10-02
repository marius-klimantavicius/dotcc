#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edg.Runtime;

internal static class ThreadTokenSource
{
    private static long next;
    internal static ulong Next()
    {
        long value = Interlocked.Increment(ref next);
        if (value <= 0) Environment.FailFast("Translated thread token space exhausted");
        return (ulong)value;
    }
}

/// <summary>Owner-specific libc++ external thread services. All stored handles
/// are portable 64-bit tokens; synchronization and threads use BCL objects.
/// The generated owner supplies static adapters for retained lifetime and TLS.
/// No reflection, runtime code generation or native pthread ABI is involved.</summary>
public sealed unsafe class LibcxxThreads<TOwner> : IDisposable where TOwner : class
{
    public const int Eagain = 11, Enomem = 12, Ebusy = 16, Einval = 22, Edeadlk = 35, Etimedout = 110;
    [StructLayout(LayoutKind.Sequential)]
    public struct Timespec { public long Seconds, Nanoseconds; }
    private readonly TOwner owner;
    private readonly delegate* managed<TOwner, void> retainOwner, releaseOwner, enterThread, leaveThread, releaseThread;
    private readonly delegate* managed<TOwner, Exception, void> terminateUnhandled;
    private readonly object gate = new();
    private readonly Dictionary<ulong, Resource> resources = new();
    private readonly Dictionary<ulong, ThreadEntry> threads = new();
    private readonly Dictionary<ulong, Key> keys = new();
    private readonly ThreadLocal<Dictionary<ulong, nint>> local = new(() => new(), true);
    private bool disposed;
    private int running;

    private enum Kind { Mutex, RecursiveMutex, Condition, Once }
    private abstract class Resource
    {
        internal readonly Kind Kind;
        internal readonly object Sync = new();
        internal bool Destroyed;
        protected Resource(Kind kind) { Kind = kind; }
    }
    private sealed class MutexEntry : Resource
    {
        internal int Owner, Depth, ConditionWaiters;
        internal MutexEntry(bool recursive) : base(recursive ? Kind.RecursiveMutex : Kind.Mutex) { }
    }
    private sealed class ConditionEntry : Resource
    {
        internal readonly LinkedList<Waiter> Waiters = new();
        internal int ActiveWaiters;
        internal ConditionEntry() : base(Kind.Condition) { }
    }
    private sealed class Waiter : IDisposable
    {
        internal readonly ManualResetEventSlim Event = new(false);
        internal LinkedListNode<Waiter>? Node;
        internal bool Signaled;
        public void Dispose() => Event.Dispose();
    }
    private sealed class OnceEntry : Resource
    {
        internal int State, Owner;
        internal OnceEntry() : base(Kind.Once) { }
    }
    private readonly struct Key
    {
        internal readonly nint Destructor;
        internal readonly bool HasOwner;
        internal Key(nint destructor, bool hasOwner) { Destructor = destructor; HasOwner = hasOwner; }
    }
    private sealed class ThreadEntry
    {
        internal readonly LibcxxThreads<TOwner> Context;
        internal readonly ulong Token;
        internal readonly nint Function, Argument;
        internal readonly bool HasOwner;
        internal readonly Thread Thread;
        internal int JoinState; // 0=joinable, 1=joining, 2=detached
        internal bool Completed;
        internal ThreadEntry(LibcxxThreads<TOwner> context, nint function, nint argument, bool hasOwner)
        {
            Context = context; Function = function; Argument = argument; HasOwner = hasOwner;
            Token = ThreadTokenSource.Next();
            // Returning from the translated process entry terminates remaining
            // C++ threads. Managed foreground threads would keep it alive.
            // Explicit owner shutdown still waits on the retained owner lease.
            Thread = new Thread(Run) { IsBackground = true };
        }
        private void Run() => Context.Run(this);
    }

    public LibcxxThreads(TOwner owner, delegate* managed<TOwner, void> retainOwner,
        delegate* managed<TOwner, void> releaseOwner, delegate* managed<TOwner, void> enterRetainedThread,
        delegate* managed<TOwner, void> leaveThread, delegate* managed<TOwner, void> releaseThread,
        delegate* managed<TOwner, Exception, void> terminateUnhandled)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (retainOwner == null || releaseOwner == null || enterRetainedThread == null ||
            leaveThread == null || releaseThread == null || terminateUnhandled == null)
            throw new ArgumentException("Every translated thread lifecycle adapter is required");
        this.owner = owner; this.retainOwner = retainOwner; this.releaseOwner = releaseOwner;
        enterThread = enterRetainedThread; this.leaveThread = leaveThread;
        this.releaseThread = releaseThread; this.terminateUnhandled = terminateUnhandled;
    }

    private Resource? Get(ulong* slot, Kind kind, bool create)
    {
        if (slot == null || ((nuint)slot & 7) != 0) return null;
        lock (gate)
        {
            if (disposed) return null;
            ulong token = Volatile.Read(ref *slot);
            if (token == 0 && create)
            {
                Resource resource = kind switch {
                    Kind.Mutex => new MutexEntry(false), Kind.RecursiveMutex => new MutexEntry(true),
                    Kind.Condition => new ConditionEntry(), _ => new OnceEntry() };
                ulong fresh = ThreadTokenSource.Next();
                resources.Add(fresh, resource);
                token = (ulong)Interlocked.CompareExchange(ref *(long*)slot, (long)fresh, 0);
                if (token == 0) return resource;
                resources.Remove(fresh);
            }
            return resources.TryGetValue(token, out var found) && found.Kind == kind ? found : null;
        }
    }
    private MutexEntry? Mutex(ulong* slot, bool recursive, bool create = true) =>
        Get(slot, recursive ? Kind.RecursiveMutex : Kind.Mutex, create) as MutexEntry;

    private static int Lock(MutexEntry entry, bool wait)
    {
        if (wait) Monitor.Enter(entry.Sync);
        else if (!Monitor.TryEnter(entry.Sync)) return Ebusy;
        if (entry.Destroyed) { Monitor.Exit(entry.Sync); return Einval; }
        if (entry.Depth != 0 && entry.Kind == Kind.Mutex)
        { Monitor.Exit(entry.Sync); return Edeadlk; }
        if (entry.Depth == int.MaxValue) { Monitor.Exit(entry.Sync); return Eagain; }
        entry.Owner = Environment.CurrentManagedThreadId;
        ++entry.Depth;
        return 0;
    }
    private static int Unlock(MutexEntry entry)
    {
        if (!Monitor.IsEntered(entry.Sync) || entry.Owner != Environment.CurrentManagedThreadId || entry.Depth == 0)
            return Einval;
        if (--entry.Depth == 0) entry.Owner = 0;
        Monitor.Exit(entry.Sync);
        return 0;
    }
    public int MutexLock(ulong* slot, bool recursive = false)
    { try { var entry = Mutex(slot, recursive); return entry is null ? Einval : Lock(entry, true); } catch (OutOfMemoryException) { return Enomem; } }
    public bool MutexTryLock(ulong* slot, bool recursive = false)
    { try { var entry = Mutex(slot, recursive); return entry is not null && Lock(entry, false) == 0; } catch (OutOfMemoryException) { return false; } }
    public int MutexUnlock(ulong* slot, bool recursive = false)
    { var entry = Mutex(slot, recursive, false); return entry is null ? Einval : Unlock(entry); }
    public int RecursiveMutexInit(ulong* slot)
    { try { return Mutex(slot, true) is null ? Einval : 0; } catch (OutOfMemoryException) { return Enomem; } }
    public int MutexDestroy(ulong* slot, bool recursive = false)
    {
        if (slot != null && *slot == 0) return 0;
        var entry = Mutex(slot, recursive, false);
        if (entry is null) return Einval;
        if (!Monitor.TryEnter(entry.Sync)) return Ebusy;
        try
        {
            if (entry.Depth != 0 || entry.ConditionWaiters != 0) return Ebusy;
            entry.Destroyed = true;
            lock (gate) { resources.Remove(*slot); *slot = 0; }
            return 0;
        }
        finally { Monitor.Exit(entry.Sync); }
    }

    public int ConditionSignal(ulong* slot, bool broadcast = false)
    {
        if (slot != null && *slot == 0) return 0;
        var entry = Get(slot, Kind.Condition, false) as ConditionEntry;
        if (entry is null) return Einval;
        lock (entry.Sync)
        {
            if (entry.Destroyed) return Einval;
            do
            {
                var first = entry.Waiters.First;
                if (first is null) break;
                entry.Waiters.RemoveFirst();
                first.Value.Signaled = true;
                first.Value.Event.Set();
            } while (broadcast);
        }
        return 0;
    }
    public int ConditionWait(ulong* condition, ulong* mutex, Timespec* deadline = null)
    {
        if (deadline != null && (deadline->Nanoseconds < 0 || deadline->Nanoseconds >= 1_000_000_000)) return Einval;
        MutexEntry? locked = Mutex(mutex, false, false);
        if (locked is null || !Monitor.IsEntered(locked.Sync) || locked.Depth != 1) return Einval;
        ConditionEntry? entry;
        Waiter waiter;
        Waiter? allocated = null;
        try
        {
            entry = Get(condition, Kind.Condition, true) as ConditionEntry;
            if (entry is null) return Einval;
            waiter = allocated = new Waiter();
            waiter.Node = new LinkedListNode<Waiter>(waiter);
            lock (entry.Sync)
            {
                if (entry.Destroyed) { waiter.Dispose(); return Einval; }
                entry.Waiters.AddLast(waiter.Node);
                ++entry.ActiveWaiters; ++locked.ConditionWaiters;
                Unlock(locked);
            }
        }
        catch (OutOfMemoryException) { allocated?.Dispose(); return Enomem; }
        try
        {
            if (deadline == null) waiter.Event.Wait();
            else
            {
                // The libc++ contract is an absolute realtime deadline. Short
                // bounded BCL waits recheck wall-clock changes without overflow.
                while (!waiter.Event.IsSet)
                {
                    Int128 now = (Int128)(DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) * 100;
                    Int128 remaining = (Int128)deadline->Seconds * 1_000_000_000 + deadline->Nanoseconds - now;
                    if (remaining <= 0) break;
                    int milliseconds = (int)Int128.Min(100, (remaining + 999_999) / 1_000_000);
                    waiter.Event.Wait(milliseconds);
                }
            }
        }
        finally
        {
            lock (entry.Sync)
            {
                if (waiter.Node!.List is not null) entry.Waiters.Remove(waiter.Node);
                --entry.ActiveWaiters;
            }
            int error = Lock(locked, true);
            if (error != 0) Environment.FailFast("Condition wait could not reacquire its translated mutex");
            --locked.ConditionWaiters;
            waiter.Dispose();
        }
        return waiter.Signaled ? 0 : Etimedout;
    }
    public int ConditionDestroy(ulong* slot)
    {
        if (slot != null && *slot == 0) return 0;
        var entry = Get(slot, Kind.Condition, false) as ConditionEntry;
        if (entry is null) return Einval;
        lock (entry.Sync)
        {
            if (entry.ActiveWaiters != 0) return Ebusy;
            entry.Destroyed = true;
            lock (gate) { resources.Remove(*slot); *slot = 0; }
        }
        return 0;
    }

    private int Once(ulong* flag, nint function, bool hasOwner)
    {
        if (function == 0) return Einval;
        OnceEntry? entry;
        try { entry = Get(flag, Kind.Once, true) as OnceEntry; }
        catch (OutOfMemoryException) { return Enomem; }
        if (entry is null) return Einval;
        lock (entry.Sync)
        {
            while (entry.State == 1)
            {
                if (entry.Owner == Environment.CurrentManagedThreadId) return Edeadlk;
                Monitor.Wait(entry.Sync);
            }
            if (entry.State == 2) return 0;
            entry.State = 1; entry.Owner = Environment.CurrentManagedThreadId;
        }
        bool completed = false;
        try
        {
            if (hasOwner) ((delegate* managed<TOwner, void>)function)(owner);
            else ((delegate* managed<void>)function)();
            completed = true;
            return 0;
        }
        finally
        {
            lock (entry.Sync) { entry.State = completed ? 2 : 0; entry.Owner = 0; Monitor.PulseAll(entry.Sync); }
        }
    }
    public int ExecuteOnce(ulong* flag, delegate* managed<void> function) => Once(flag, (nint)function, false);
    public int ExecuteOnce(ulong* flag, delegate* managed<TOwner, void> function) => Once(flag, (nint)function, true);

    private int Create(ulong* handle, nint function, void* argument, bool hasOwner)
    {
        if (handle == null || function == 0) return Einval;
        ThreadEntry? entry = null;
        bool retained = false, registered = false;
        try
        {
            lock (gate) { if (disposed) return Einval; }
            retainOwner(owner); retained = true;
            entry = new ThreadEntry(this, function, (nint)argument, hasOwner);
            lock (gate)
            {
                if (disposed) return Einval;
                threads.Add(entry.Token, entry); ++running; registered = true;
            }
            entry.Thread.Start();
            *handle = entry.Token;
            retained = false; // The started thread now owns this lease.
            return 0;
        }
        catch (OutOfMemoryException) { return Eagain; }
        catch (ThreadStateException) { return Eagain; }
        finally
        {
            if (retained)
            {
                if (registered) lock (gate) { threads.Remove(entry!.Token); --running; }
                releaseOwner(owner);
            }
        }
    }
    public int ThreadCreate(ulong* handle, delegate* managed<void*, void*> function, void* argument) => Create(handle, (nint)function, argument, false);
    public int ThreadCreate(ulong* handle, delegate* managed<TOwner, void*, void*> function, void* argument) => Create(handle, (nint)function, argument, true);
    private bool Terminate(Exception error)
    {
        terminateUnhandled(owner, error);
        Environment.FailFast("Translated thread terminate adapter returned", error);
        return false;
    }
    private void Run(ThreadEntry entry)
    {
        bool entered = false;
        try
        {
            try
            {
                enterThread(owner); entered = true;
                if (entry.HasOwner) ((delegate* managed<TOwner, void*, void*>)entry.Function)(owner, (void*)entry.Argument);
                else ((delegate* managed<void*, void*>)entry.Function)((void*)entry.Argument);
            }
            catch (Exception error) when (Terminate(error)) { }
            finally
            {
                if (entered) leaveThread(owner);
                releaseThread(owner);
            }
        }
        catch (Exception error) when (Terminate(error)) { }
        finally
        {
            lock (gate)
            {
                entry.Completed = true; --running;
                if (entry.JoinState == 2) threads.Remove(entry.Token);
                Monitor.PulseAll(gate);
            }
            releaseOwner(owner);
            GC.KeepAlive(owner);
        }
    }
    public static ulong CurrentThreadId() => (ulong)Environment.CurrentManagedThreadId;
    public static bool ThreadIdEqual(ulong left, ulong right) => left == right;
    public static bool ThreadIdLess(ulong left, ulong right) => left < right;
    public static bool ThreadIsNull(ulong* handle) => *handle == 0;
    public ulong ThreadGetId(ulong* handle)
    { lock (gate) return !disposed && threads.TryGetValue(*handle, out var entry) ? (ulong)entry.Thread.ManagedThreadId : 0; }
    public int ThreadJoin(ulong* handle)
    {
        ThreadEntry entry;
        lock (gate)
        {
            if (disposed || !threads.TryGetValue(*handle, out entry!) || entry.JoinState != 0) return Einval;
            if (entry.Thread.ManagedThreadId == Environment.CurrentManagedThreadId) return Edeadlk;
            entry.JoinState = 1;
        }
        try { entry.Thread.Join(); }
        catch { lock (gate) entry.JoinState = 0; throw; }
        lock (gate) { threads.Remove(entry.Token); *handle = 0; }
        return 0;
    }
    public int ThreadDetach(ulong* handle)
    {
        lock (gate)
        {
            if (disposed || !threads.TryGetValue(*handle, out var entry) || entry.JoinState != 0) return Einval;
            entry.JoinState = 2;
            if (entry.Completed) threads.Remove(entry.Token);
            *handle = 0;
            return 0;
        }
    }
    public static void ThreadYield() => Thread.Yield();
    public static void ThreadSleepFor(long nanoseconds)
    {
        if (nanoseconds <= 0) return;
        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        for (;;)
        {
            Int128 elapsed = (Int128)(System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1_000_000_000 /
                             System.Diagnostics.Stopwatch.Frequency;
            Int128 remaining = (Int128)nanoseconds - elapsed;
            if (remaining <= 0) return;
            Thread.Sleep((int)Int128.Min(int.MaxValue, (remaining + 999_999) / 1_000_000));
        }
    }

    private int CreateKey(ulong* key, nint destructor, bool hasOwner)
    {
        if (key == null) return Einval;
        try
        {
            lock (gate)
            {
                if (disposed) return Einval;
                ulong token = ThreadTokenSource.Next();
                keys.Add(token, new Key(destructor, hasOwner)); *key = token;
            }
            return 0;
        }
        catch (OutOfMemoryException) { return Enomem; }
    }
    public int TlsCreate(ulong* key, delegate* managed<void*, void> destructor) => CreateKey(key, (nint)destructor, false);
    public int TlsCreate(ulong* key, delegate* managed<TOwner, void*, void> destructor) => CreateKey(key, (nint)destructor, true);
    public void* TlsGet(ulong key)
    { lock (gate) return !disposed && keys.ContainsKey(key) && local.Value!.TryGetValue(key, out nint value) ? (void*)value : null; }
    public int TlsSet(ulong key, void* value)
    {
        try
        {
            lock (gate)
            {
                if (disposed || !keys.ContainsKey(key)) return Einval;
                if (value == null) local.Value!.Remove(key); else local.Value![key] = (nint)value;
            }
            return 0;
        }
        catch (OutOfMemoryException) { return Enomem; }
    }
    public int TlsDelete(ulong key)
    { lock (gate) return !disposed && keys.Remove(key) ? 0 : Einval; }
    // Called by the owner's ReleaseThread after C++ thread_local finalizers and
    // before resetting libc TLS. Destructors may set keys again; use four passes,
    // matching the portable POSIX minimum. No context lock spans user callbacks.
    public void ReleaseThread()
    {
        if (!local.IsValueCreated) return;
        for (int pass = 0; pass != 4; ++pass)
        {
            List<ulong> pending;
            lock (gate) { if (disposed) return; pending = new(local.Value!.Keys); }
            bool invoked = false;
            foreach (ulong token in pending)
            {
                Key key;
                nint value;
                lock (gate)
                {
                    if (!local.Value!.Remove(token, out value) || value == 0 ||
                        !keys.TryGetValue(token, out key) || key.Destructor == 0) continue;
                }
                invoked = true;
                if (key.HasOwner) ((delegate* managed<TOwner, void*, void>)key.Destructor)(owner, (void*)value);
                else ((delegate* managed<void*, void>)key.Destructor)((void*)value);
            }
            if (!invoked) break;
        }
        lock (gate) local.Value!.Clear();
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            if (running != 0 || threads.Count != 0)
                throw new InvalidOperationException("Join or detach all translated threads before owner disposal");
            foreach (var state in local.Values)
                if (state.Count != 0) throw new InvalidOperationException("Release each translated thread's TLS before owner disposal");
            foreach (var resource in resources.Values)
            {
                if (resource is MutexEntry mutex && (Volatile.Read(ref mutex.Depth) != 0 || Volatile.Read(ref mutex.ConditionWaiters) != 0) ||
                    resource is ConditionEntry condition && Volatile.Read(ref condition.ActiveWaiters) != 0 ||
                    resource is OnceEntry once && Volatile.Read(ref once.State) == 1)
                    throw new InvalidOperationException("Translated synchronization object is still in use");
            }
            disposed = true; resources.Clear(); keys.Clear(); local.Dispose();
        }
    }
}

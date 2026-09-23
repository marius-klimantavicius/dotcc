#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DotCC.Libc;

/// <summary>Process-private POSIX thread primitives implemented solely with BCL
/// threads, monitors and thread-local storage. Handles are dotcc ABI scalar IDs,
/// not host pthread objects. Unsupported POSIX facilities are not advertised.</summary>
public static unsafe partial class Libc
{
    public const int PTHREAD_CREATE_JOINABLE = 0;
    public const int PTHREAD_CREATE_DETACHED = 1;
    public const int PTHREAD_MUTEX_NORMAL = 0;
    public const int PTHREAD_MUTEX_RECURSIVE = 1;
    public const int PTHREAD_MUTEX_ERRORCHECK = 2;
    public const int PTHREAD_MUTEX_DEFAULT = PTHREAD_MUTEX_NORMAL;
    public const int PTHREAD_PROCESS_PRIVATE = 0;
    public const int PTHREAD_PROCESS_SHARED = 1;
    public const int PTHREAD_DESTRUCTOR_ITERATIONS = 4;

    internal sealed class PthreadState
    {
        public long Id;
        public IntPtr Function, Argument, Result;
        public Func<IntPtr, IntPtr>? InstanceStart;
        public Thread Thread = null!;
        public RuntimeContext? Context;
        public bool Detached, Joining, Finished;
    }
    private sealed class PthreadExitException : Exception { public IntPtr Result; }
    private static ConcurrentDictionary<long, PthreadState> _pthreads => RuntimeState.Threads;
    private static ref long _nextPthread => ref RuntimeState.NextThread;
    private static ref long _pthreadSelf => ref RuntimeThread.Self;
    private static ref bool _pthreadCreated => ref RuntimeThread.Created;

    public static long pthread_self()
    {
        if (_pthreadSelf == 0) _pthreadSelf = Interlocked.Increment(ref _nextPthread);
        return _pthreadSelf;
    }
    public static int pthread_equal(long a, long b) => a == b ? 1 : 0;

    // Thread attributes encode initialization plus detach state. Stack and
    // scheduling controls are deliberately absent: the BCL cannot honor them.
    public static int pthread_attr_init(int* attr) { if (attr == null) return EINVAL; *attr = 2; return 0; }
    public static int pthread_attr_destroy(int* attr)
    {
        if (attr == null || (*attr != 2 && *attr != 3)) return EINVAL;
        *attr = -1; return 0;
    }
    public static int pthread_attr_setdetachstate(int* attr, int state)
    {
        if (attr == null || (*attr != 2 && *attr != 3) || (state != 0 && state != 1)) return EINVAL;
        *attr = 2 | state; return 0;
    }
    public static int pthread_attr_getdetachstate(int* attr, int* state)
    {
        if (attr == null || state == null || (*attr != 2 && *attr != 3)) return EINVAL;
        *state = *attr & 1; return 0;
    }
    public static int pthread_create(long* thread, int* attr, delegate*<void*, void*> start, void* arg)
    {
        if (thread == null || start == null || (attr != null && *attr != 2 && *attr != 3)) return EINVAL;
        PthreadState? state = null;
        RuntimeContext? context = RuntimeContext.Current;
        bool retained = false;
        try
        {
            if (context != null) { context.Retain(); retained = true; }
            state = new PthreadState { Context = context, Id = Interlocked.Increment(ref _nextPthread),
                Function = (IntPtr)start, Argument = (IntPtr)arg, Detached = attr != null && *attr == 3 };
            state.Thread = new Thread(PthreadEntry);
            _pthreads[state.Id] = state;
            state.Thread.Start(state);
            *thread = state.Id;
            return 0;
        }
        catch (Exception ex) when (ex is OutOfMemoryException or ThreadStateException)
        {
            if (state != null) _pthreads.TryRemove(state.Id, out _);
            if (retained) context!.Release();
            return EAGAIN;
        }
    }
    /// <summary>Creates a thread in the explicitly supplied program runtime,
    /// even when invoked while another program is bound on the caller thread.</summary>
    public static int pthread_create<T>(T instance, long* thread, int* attr,
        delegate*<T, void*, void*> start, void* arg) where T : class, IProgramInstance
    {
        if (instance == null || thread == null || start == null || (attr != null && *attr != 2 && *attr != 3)) return EINVAL;
        RuntimeContext context = instance.__DotCcRuntime;
        PthreadState? state = null;
        context.Retain();
        try
        {
            IntPtr address = (IntPtr)start;
            state = new PthreadState
            {
                Context = context, Id = Interlocked.Increment(ref context.NextThread),
                InstanceStart = argument => (IntPtr)((delegate*<T, void*, void*>)address)(instance, (void*)argument),
                Argument = (IntPtr)arg, Detached = attr != null && *attr == 3
            };
            state.Thread = new Thread(PthreadEntry);
            context.Threads[state.Id] = state;
            state.Thread.Start(state);
            *thread = state.Id;
            return 0;
        }
        catch (Exception error)
        {
            if (state != null) context.Threads.TryRemove(state.Id, out _);
            context.Release();
            if (error is OutOfMemoryException or ThreadStateException) return EAGAIN;
            throw;
        }
    }
    private static void PthreadEntry(object? argument)
    {
        var state = (PthreadState)argument!;
        using var contextBinding = state.Context?.Enter();
        _pthreadSelf = state.Id;
        _pthreadCreated = true;
        try { state.Result = state.InstanceStart != null ? state.InstanceStart(state.Argument)
            : (IntPtr)((delegate*<void*, void*>)state.Function)((void*)state.Argument); }
        catch (PthreadExitException ex) { state.Result = ex.Result; }
        finally
        {
            try { RunPthreadDtors(); RunTssDtors(); }
            finally
            {
                lock (state)
                {
                    state.Finished = true;
                    if (state.Detached) _pthreads.TryRemove(state.Id, out _);
                }
                _pthreadCreated = false;
                state.Context?.Release();
            }
        }
    }
    public static int pthread_join(long thread, void** result)
    {
        if (thread == pthread_self()) return EDEADLK;
        if (!_pthreads.TryGetValue(thread, out var state)) return ESRCH;
        lock (state)
        {
            if (state.Detached || state.Joining) return EINVAL;
            state.Joining = true;
        }
        try { state.Thread.Join(); }
        catch { lock (state) state.Joining = false; throw; }
        if (result != null) *result = (void*)state.Result;
        _pthreads.TryRemove(thread, out _);
        return 0;
    }
    public static int pthread_detach(long thread)
    {
        if (!_pthreads.TryGetValue(thread, out var state)) return ESRCH;
        lock (state)
        {
            if (state.Detached || state.Joining) return EINVAL;
            state.Detached = true;
            if (state.Finished) _pthreads.TryRemove(thread, out _);
        }
        return 0;
    }
    public static void pthread_exit(void* result)
    {
        if (!_pthreadCreated) throw new PlatformNotSupportedException("pthread_exit requires a pthread_create thread in dotcc.");
        throw new PthreadExitException { Result = (IntPtr)result };
    }

    internal sealed class PthreadMutex
    {
        public int Type, Owner, Depth, Waiters, ConditionWaiters;
        public bool Destroyed;
    }
    internal sealed class PthreadWaiter { public bool Signaled; }
    internal sealed class PthreadCondition
    {
        public readonly LinkedList<PthreadWaiter> Waiters = new();
        public bool Destroyed;
        public int MutexId;
    }
    private static object _pthreadObjects => RuntimeState.Objects;
    private static Dictionary<int, PthreadMutex> _pthreadMutexes => RuntimeState.Mutexes;
    private static Dictionary<int, PthreadCondition> _pthreadConditions => RuntimeState.Conditions;
    private static ref int _nextPthreadMutex => ref RuntimeState.NextMutex;
    private static ref int _nextPthreadCondition => ref RuntimeState.NextCondition;

    public static int pthread_mutexattr_init(int* attr) { if (attr == null) return EINVAL; *attr = 0; return 0; }
    public static int pthread_mutexattr_destroy(int* attr)
    { if (attr == null || *attr < 0 || *attr > 2) return EINVAL; *attr = -1; return 0; }
    public static int pthread_mutexattr_settype(int* attr, int type)
    { if (attr == null || *attr < 0 || *attr > 2 || type < 0 || type > 2) return EINVAL; *attr = type; return 0; }
    public static int pthread_mutexattr_gettype(int* attr, int* type)
    { if (attr == null || type == null || *attr < 0 || *attr > 2) return EINVAL; *type = *attr; return 0; }
    public static int pthread_mutexattr_setpshared(int* attr, int shared)
    { if (attr == null || *attr < 0 || *attr > 2 || shared < 0 || shared > 1) return EINVAL; return shared == 0 ? 0 : ENOTSUP; }
    public static int pthread_mutexattr_getpshared(int* attr, int* shared)
    { if (attr == null || shared == null || *attr < 0 || *attr > 2) return EINVAL; *shared = 0; return 0; }

    private static PthreadMutex? PthreadGetMutex(int* mutex, out int error)
    {
        error = EINVAL;
        if (mutex == null) return null;
        try
        {
            lock (_pthreadObjects)
            {
                if (*mutex == 0) // PTHREAD_MUTEX_INITIALIZER, initialized once under the registry gate
                {
                    var state = new PthreadMutex();
                    int id = checked(++_nextPthreadMutex);
                    _pthreadMutexes.Add(id, state); *mutex = id;
                    return state;
                }
                return _pthreadMutexes.GetValueOrDefault(*mutex);
            }
        }
        catch (OutOfMemoryException) { error = ENOMEM; return null; }
        catch (OverflowException) { error = EAGAIN; return null; }
    }
    public static int pthread_mutex_init(int* mutex, int* attr)
    {
        if (mutex == null || (attr != null && (*attr < 0 || *attr > 2))) return EINVAL;
        try
        {
            lock (_pthreadObjects)
            {
                int id = checked(++_nextPthreadMutex);
                _pthreadMutexes.Add(id, new PthreadMutex { Type = attr == null ? 0 : *attr });
                *mutex = id;
            }
            return 0;
        }
        catch (OutOfMemoryException) { return ENOMEM; }
        catch (OverflowException) { return EAGAIN; }
    }
    public static int pthread_mutex_destroy(int* mutex)
    {
        if (mutex == null) return EINVAL;
        lock (_pthreadObjects)
        {
            if (*mutex == 0) { *mutex = -1; return 0; }
            if (!_pthreadMutexes.TryGetValue(*mutex, out var state)) return EINVAL;
            lock (state)
            {
                if (state.Owner != 0 || state.Waiters != 0 || state.ConditionWaiters != 0) return EBUSY;
                state.Destroyed = true;
                _pthreadMutexes.Remove(*mutex); *mutex = -1;
            }
        }
        return 0;
    }
    // Recompute after every wake/chunk and round UP: absolute waits must neither
    // overflow for large tv_sec nor expire early through millisecond truncation.
    private static int PthreadTimeout(timespec* deadline)
    {
        Int128 ns = (Int128)deadline->tv_sec * 1_000_000_000 + deadline->tv_nsec;
        Int128 now = (Int128)(global::System.DateTime.UtcNow.Ticks - global::System.DateTime.UnixEpoch.Ticks) * 100;
        Int128 ms = (ns - now + 999_999) / 1_000_000;
        // BCL monitor waits have no wall-clock-change notification. Poll at
        // most once per second so a forward clock adjustment is observed.
        return ms <= 0 ? 0 : ms > 1000 ? 1000 : (int)ms;
    }
    private static bool PthreadValidDeadline(timespec* deadline) => deadline != null && deadline->tv_nsec >= 0 && deadline->tv_nsec < 1_000_000_000;
    private static int PthreadLock(PthreadMutex state, bool attempt, timespec* deadline, bool timed)
    {
        int me = Environment.CurrentManagedThreadId;
        lock (state)
        {
            if (state.Destroyed) return EINVAL;
            if (state.Owner == me && state.Type == PTHREAD_MUTEX_RECURSIVE)
            {
                if (state.Depth == int.MaxValue) return EAGAIN;
                state.Depth++; return 0;
            }
            if (state.Owner != 0 && attempt) return EBUSY;
            if (state.Owner == me && state.Type == PTHREAD_MUTEX_ERRORCHECK) return EDEADLK;
            if (state.Owner != 0 && timed && !PthreadValidDeadline(deadline)) return EINVAL;
            state.Waiters++;
            try
            {
                while (state.Owner != 0)
                {
                    int timeout = timed ? PthreadTimeout(deadline) : Timeout.Infinite;
                    if (timeout == 0) return ETIMEDOUT;
                    Monitor.Wait(state, timeout);
                }
                state.Owner = me; state.Depth = 1; return 0;
            }
            finally { state.Waiters--; }
        }
    }
    public static int pthread_mutex_lock(int* mutex)
    { var state = PthreadGetMutex(mutex, out int error); return state == null ? error : PthreadLock(state, false, null, false); }
    public static int pthread_mutex_trylock(int* mutex)
    { var state = PthreadGetMutex(mutex, out int error); return state == null ? error : PthreadLock(state, true, null, false); }
    public static int pthread_mutex_timedlock(int* mutex, timespec* deadline)
    { var state = PthreadGetMutex(mutex, out int error); return state == null ? error : PthreadLock(state, false, deadline, true); }
    private static int PthreadUnlock(PthreadMutex state)
    {
        lock (state)
        {
            if (state.Destroyed) return EINVAL;
            if (state.Owner != Environment.CurrentManagedThreadId) return EPERM;
            if (--state.Depth == 0) { state.Owner = 0; Monitor.PulseAll(state); }
            return 0;
        }
    }
    public static int pthread_mutex_unlock(int* mutex)
    { var state = PthreadGetMutex(mutex, out int error); return state == null ? error : PthreadUnlock(state); }

    public static int pthread_condattr_init(int* attr) { if (attr == null) return EINVAL; *attr = 0; return 0; }
    public static int pthread_condattr_destroy(int* attr)
    { if (attr == null || *attr != 0) return EINVAL; *attr = -1; return 0; }
    public static int pthread_condattr_setpshared(int* attr, int shared)
    { if (attr == null || *attr != 0 || shared < 0 || shared > 1) return EINVAL; return shared == 0 ? 0 : ENOTSUP; }
    public static int pthread_condattr_getpshared(int* attr, int* shared)
    { if (attr == null || *attr != 0 || shared == null) return EINVAL; *shared = 0; return 0; }
    private static PthreadCondition? PthreadGetCondition(int* condition, out int error)
    {
        error = EINVAL;
        if (condition == null) return null;
        try
        {
            lock (_pthreadObjects)
            {
                if (*condition == 0)
                {
                    var state = new PthreadCondition();
                    int id = checked(++_nextPthreadCondition);
                    _pthreadConditions.Add(id, state); *condition = id;
                    return state;
                }
                return _pthreadConditions.GetValueOrDefault(*condition);
            }
        }
        catch (OutOfMemoryException) { error = ENOMEM; return null; }
        catch (OverflowException) { error = EAGAIN; return null; }
    }
    public static int pthread_cond_init(int* condition, int* attr)
    {
        if (condition == null || (attr != null && *attr != 0)) return EINVAL;
        try
        {
            lock (_pthreadObjects)
            {
                int id = checked(++_nextPthreadCondition);
                _pthreadConditions.Add(id, new PthreadCondition()); *condition = id;
            }
            return 0;
        }
        catch (OutOfMemoryException) { return ENOMEM; }
        catch (OverflowException) { return EAGAIN; }
    }
    public static int pthread_cond_destroy(int* condition)
    {
        if (condition == null) return EINVAL;
        lock (_pthreadObjects)
        {
            if (*condition == 0) { *condition = -1; return 0; }
            if (!_pthreadConditions.TryGetValue(*condition, out var state)) return EINVAL;
            lock (state)
            {
                if (state.Waiters.Count != 0) return EBUSY;
                state.Destroyed = true;
                _pthreadConditions.Remove(*condition); *condition = -1;
            }
        }
        return 0;
    }
    private static int PthreadSignal(int* condition, bool all)
    {
        var state = PthreadGetCondition(condition, out int error);
        if (state == null) return error;
        lock (state)
        {
            if (state.Destroyed) return EINVAL;
            foreach (var waiter in state.Waiters)
            {
                if (waiter.Signaled) continue;
                waiter.Signaled = true;
                if (!all) break;
            }
            Monitor.PulseAll(state);
        }
        return 0;
    }
    public static int pthread_cond_signal(int* condition) => PthreadSignal(condition, false);
    public static int pthread_cond_broadcast(int* condition) => PthreadSignal(condition, true);
    private static int PthreadWait(int* condition, int* mutex, timespec* deadline, bool timed)
    {
        if (timed && !PthreadValidDeadline(deadline)) return EINVAL;
        var cv = PthreadGetCondition(condition, out int error);
        if (cv == null) return error;
        var m = PthreadGetMutex(mutex, out error);
        if (m == null) return error;
        int result = 0;
        bool released = false;
        try
        {
            lock (cv)
            {
                if (cv.Destroyed || (cv.Waiters.Count != 0 && cv.MutexId != *mutex)) return EINVAL;
                lock (m)
                {
                    if (m.Destroyed) return EINVAL;
                    if (m.Owner != Environment.CurrentManagedThreadId) return EPERM;
                    // A recursive mutex at depth > 1 retains ownership, as POSIX
                    // specifies; applications must avoid such condition waits.
                    m.ConditionWaiters++;
                }
                LinkedListNode<PthreadWaiter> node;
                try { node = cv.Waiters.AddLast(new PthreadWaiter()); }
                catch (OutOfMemoryException) { lock (m) m.ConditionWaiters--; return ENOMEM; }
                cv.MutexId = *mutex;
                PthreadUnlock(m);
                released = true;
                try
                {
                    while (!node.Value.Signaled)
                    {
                        int timeout = timed ? PthreadTimeout(deadline) : Timeout.Infinite;
                        if (timeout == 0) { result = ETIMEDOUT; break; }
                        Monitor.Wait(cv, timeout);
                    }
                }
                finally
                {
                    cv.Waiters.Remove(node);
                    if (cv.Waiters.Count == 0) cv.MutexId = 0;
                }
            }
        }
        finally
        {
            // Also restore ownership/accounting if a foreign managed host
            // interrupts the wait. Never acquire m while holding cv's gate.
            if (released)
            {
                try { PthreadLock(m, false, null, false); }
                finally { lock (m) m.ConditionWaiters--; }
            }
        }
        return result;
    }
    public static int pthread_cond_wait(int* condition, int* mutex) => PthreadWait(condition, mutex, null, false);
    public static int pthread_cond_timedwait(int* condition, int* mutex, timespec* deadline) => PthreadWait(condition, mutex, deadline, true);

    public static int pthread_once(int* once, delegate*<void> initialize)
    {
        if (once == null || initialize == null) return EINVAL;
        var spin = new SpinWait();
        while (true)
        {
            int state = Volatile.Read(ref *once);
            if (state == 2) return 0;
            if (state == 0 && Interlocked.CompareExchange(ref *once, 1, 0) == 0)
            {
                try { initialize(); Volatile.Write(ref *once, 2); return 0; }
                catch { Volatile.Write(ref *once, 0); throw; }
            }
            if (state != 0 && state != 1) return EINVAL;
            spin.SpinOnce();
        }
    }
    public static int pthread_once<T>(T instance, int* once, delegate*<T, void> initialize)
        where T : class, IProgramInstance
    {
        if (once == null || initialize == null) return EINVAL;
        var context = instance.__DotCcRuntime;
        using var reservation = context.RetainLease();
        using var binding = context.Enter();
        var spin = new SpinWait();
        while (true)
        {
            int state = Volatile.Read(ref *once);
            if (state == 2) return 0;
            if (state == 0 && Interlocked.CompareExchange(ref *once, 1, 0) == 0)
            {
                try { initialize(instance); Volatile.Write(ref *once, 2); return 0; }
                catch { Volatile.Write(ref *once, 0); throw; }
            }
            if (state != 0 && state != 1) return EINVAL;
            spin.SpinOnce();
        }
    }
    private static ConcurrentDictionary<int, IntPtr> _pthreadKeys => RuntimeState.Keys;
    private static ref int _nextPthreadKey => ref RuntimeState.NextKey;
    private static ref Dictionary<int, IntPtr>? _pthreadValues => ref RuntimeThread.Values;
    private static ref int _pthreadSets => ref RuntimeThread.PthreadSets;
    public static int pthread_key_create(int* key, delegate*<void*, void> destructor)
    {
        if (key == null) return EINVAL;
        try
        {
            int id = Interlocked.Increment(ref _nextPthreadKey);
            if (id <= 0) return EAGAIN;
            _pthreadKeys[id] = (IntPtr)destructor; *key = id; return 0;
        }
        catch (OutOfMemoryException) { return ENOMEM; }
    }
    public static int pthread_key_delete(int key)
    {
        if (!_pthreadKeys.TryRemove(key, out _)) return EINVAL;
        _pthreadValues?.Remove(key); return 0;
    }
    public static int pthread_setspecific(int key, void* value)
    {
        if (!_pthreadKeys.ContainsKey(key)) return EINVAL;
        try
        {
            var values = _pthreadValues ??= new Dictionary<int, IntPtr>();
            if ((++_pthreadSets & 63) == 0)
                foreach (int oldKey in new List<int>(values.Keys))
                    if (!_pthreadKeys.ContainsKey(oldKey)) values.Remove(oldKey);
            values[key] = (IntPtr)value; return 0;
        }
        catch (OutOfMemoryException) { return ENOMEM; }
    }
    public static void* pthread_getspecific(int key) => _pthreadKeys.ContainsKey(key) && _pthreadValues != null && _pthreadValues.TryGetValue(key, out var value) ? (void*)value : null;
    private static void RunPthreadDtors()
    {
        var values = _pthreadValues;
        if (values == null) return;
        for (int round = 0; round < PTHREAD_DESTRUCTOR_ITERATIONS; round++)
        {
            bool any = false;
            foreach (int key in new List<int>(values.Keys))
            {
                if (!values.TryGetValue(key, out var value) || value == IntPtr.Zero || !_pthreadKeys.TryGetValue(key, out var destructor) || destructor == IntPtr.Zero) continue;
                values[key] = IntPtr.Zero;
                ((delegate*<void*, void>)destructor)((void*)value);
                any = true;
            }
            if (!any) break;
        }
        values.Clear();
    }
}

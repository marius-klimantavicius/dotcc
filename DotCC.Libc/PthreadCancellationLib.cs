#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public const int PTHREAD_CANCEL_ENABLE = 0, PTHREAD_CANCEL_DISABLE = 1;
    public const int PTHREAD_CANCEL_DEFERRED = 0, PTHREAD_CANCEL_ASYNCHRONOUS = 1;
    public static void* PTHREAD_CANCELED => (void*)(nint)(-1);
    private sealed class PthreadCancellationException : Exception { }

    // Cooperative cancellation only: never interrupt arbitrary managed/native
    // code. Mutex acquisition is deliberately not a cancellation point.
    public static int pthread_cancel(long thread)
    {
        if (!_pthreads.TryGetValue(thread, out var state)) return ESRCH;
        lock (state)
        {
            if (state.Finished) return ESRCH;
            Volatile.Write(ref state.CancellationRequested, 1);
        }
        return 0;
    }

    public static void pthread_testcancel() => PthreadTestCancellation(true);

    private static void PthreadTestCancellation(bool cleanup)
    {
        if (_pthreadCreated && RuntimeThread.CancellationState == PTHREAD_CANCEL_ENABLE &&
            _pthreads.TryGetValue(_pthreadSelf, out var state) &&
            Volatile.Read(ref state.CancellationRequested) != 0)
        {
            // Run before unwinding translated frames: handler arguments may
            // point to C locals in those still-live frames.
            if (cleanup) RunPthreadCleanup();
            throw new PthreadCancellationException();
        }
    }

    public static int pthread_setcancelstate(int state, int* previous)
    {
        if (state is not (PTHREAD_CANCEL_ENABLE or PTHREAD_CANCEL_DISABLE)) return EINVAL;
        if (previous != null) *previous = RuntimeThread.CancellationState;
        RuntimeThread.CancellationState = state;
        return 0;
    }

    public static int pthread_setcanceltype(int type, int* previous)
    {
        if (type == PTHREAD_CANCEL_ASYNCHRONOUS) return ENOTSUP;
        if (type != PTHREAD_CANCEL_DEFERRED) return EINVAL;
        if (previous != null) *previous = PTHREAD_CANCEL_DEFERRED;
        return 0;
    }

    public static void pthread_cleanup_push(delegate*<void*, void> routine, void* argument)
    {
        nint callback = (nint)routine, value = (nint)argument;
        (RuntimeThread.PthreadCleanup ??= new Stack<Action>()).Push(
            () => ((delegate*<void*, void>)callback)((void*)value));
    }

    public static void pthread_cleanup_push<T>(T instance, delegate*<T, void*, void> routine, void* argument)
        where T : class, IProgramInstance
    {
        nint callback = (nint)routine, value = (nint)argument;
        (RuntimeThread.PthreadCleanup ??= new Stack<Action>()).Push(() =>
        {
            using var binding = instance.__DotCcRuntime.Enter();
            ((delegate*<T, void*, void>)callback)(instance, (void*)value);
        });
    }

    public static void pthread_cleanup_pop(int execute)
    {
        var cleanup = RuntimeThread.PthreadCleanup;
        if (cleanup is null || cleanup.Count == 0)
            throw new InvalidOperationException("pthread_cleanup_pop has no matching push.");
        var callback = cleanup.Pop();
        if (execute != 0) callback();
    }

    private static void RunPthreadCleanup()
    {
        RuntimeThread.CancellationState = PTHREAD_CANCEL_DISABLE;
        var cleanup = RuntimeThread.PthreadCleanup;
        while (cleanup is { Count: > 0 }) cleanup.Pop()();
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace Managed.Networking;

/// <summary>Per-owner implementation of the upstream sys_arch port contract.</summary>
public static unsafe partial class LwipSysArch
{
    public const uint Timeout = uint.MaxValue;
    private const sbyte ErrMem = -1;
    private sealed class Stopped : OperationCanceledException { }
    private sealed class ServiceStopped : OperationCanceledException { }
    internal sealed class Resource
    {
        internal readonly object Gate = new();
        internal readonly Queue<nint> Messages = new();
        internal int Capacity;
        internal int Signals;
        internal bool Freed;
    }
    internal sealed class State
    {
        internal readonly object Registry = new();
        internal readonly object Protection = new();
        internal readonly ThreadLocal<uint> ProtectionDepth = new();
        internal readonly object CoreLock = new();
        internal readonly Dictionary<long, Resource> Resources = new();
        internal readonly List<Thread> Threads = new();
        internal readonly Dictionary<long,Thread> WorkerHandles = new();
        internal readonly Dictionary<long,nint> WorkerArguments = new();
        internal readonly HashSet<long> ServiceStopRequests = new();
        internal readonly ThreadLocal<long> CurrentWorker = new();
        internal readonly Dictionary<int, nint> ThreadSemaphores = new();
        internal readonly List<Exception> Failures = new();
        internal readonly Dictionary<byte, SerialChannel> Serial = new();
        internal readonly Dictionary<byte,long> SerialHandles = new();
        internal readonly Dictionary<long,SerialChannel> SerialByHandle = new();

        internal volatile bool Stopping;
        internal volatile bool WorkerStopRequested;
        internal int CoreThread;
        internal Func<uint>? Clock;
    }
    private static readonly ConditionalWeakTable<Lwip, State> States = new();
    private static long nextHandle;
    private static State For(Lwip owner) => States.GetValue(owner, static _ => new State());
    private static long Register(State state, Resource resource)
    {
        lock (state.Registry)
        {
            if (state.Stopping) throw new Stopped();
            long handle = Interlocked.Increment(ref nextHandle);
            state.Resources.Add(handle, resource);
            return handle;
        }
    }
    private static Resource Resolve(State state, long handle)
    {
        lock (state.Registry)
            return state.Resources.TryGetValue(handle, out var value) ? value : throw new InvalidOperationException("Invalid or foreign sys_arch handle.");
    }
    private static void Check(State state, Resource resource)
    {
        if (state.Stopping) throw new Stopped();
        if (resource.Freed) throw new InvalidOperationException("A sys_arch resource was freed while in use.");
    }
    private static void Free(State state, long* handle)
    {
        if (*handle == 0) throw new InvalidOperationException("Double free of sys_arch resource.");
        var resource = Resolve(state, *handle);
        lock (resource.Gate)
        {
            resource.Freed = true;
            Monitor.PulseAll(resource.Gate);
        }
        lock (state.Registry) state.Resources.Remove(*handle);
        *handle = 0;
    }
    private static uint Elapsed(long start) => (uint)Math.Min(uint.MaxValue - 1L, (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds);
    private static bool Wait(State state, Resource resource, uint timeout, long start)
    {
        Check(state, resource);
        if (timeout == 0) { Monitor.Wait(resource.Gate); return true; }
        uint elapsed = Elapsed(start);
        if (elapsed >= timeout) return false;
        Monitor.Wait(resource.Gate, (int)Math.Min(int.MaxValue, timeout - elapsed));
        return true;
    }
    public static int WorkerStopping(Lwip owner) => For(owner).WorkerStopRequested ? 1 : 0;
    public static void RequestWorkerStop(Lwip owner) => For(owner).WorkerStopRequested = true;
    public static void JoinWorkers(Lwip owner)
    {
        var state = For(owner); Thread[] threads;
        lock (state.Registry) threads = state.Threads.ToArray();
        foreach (var thread in threads)
        {
            if (thread == Thread.CurrentThread) throw new InvalidOperationException("Cannot join current worker.");
            if (!thread.Join(TimeSpan.FromSeconds(10))) throw new TimeoutException("lwIP worker did not drain; owner backing retained.");
        }
    }
    public static long[] FindWorkers(Lwip owner,string name,nint argument)
    {
        var state=For(owner);var matches=new List<long>();lock(state.Registry)
            foreach(var item in state.WorkerHandles)
                if(item.Value.Name==name&&state.WorkerArguments[item.Key]==argument&&item.Value.IsAlive)matches.Add(item.Key);
        return matches.ToArray();
    }
    /// <summary>Mark one ancillary worker for exit at an explicitly safe service boundary; does not interrupt sys_arch waits.</summary>
    public static void RequestServiceStop(Lwip owner,long worker)
    {
        var state=For(owner);lock(state.Registry)
        {if(!state.WorkerHandles.ContainsKey(worker))throw new InvalidOperationException("Invalid or foreign worker handle.");state.ServiceStopRequests.Add(worker);}
    }
    /// <summary>Call only after the upstream operation has returned and released its waiter/accounting state.</summary>
    public static void ExitServiceIfRequested(Lwip owner)
    {
        var state=For(owner);lock(state.Registry)
            if(state.ServiceStopRequests.Contains(state.CurrentWorker.Value))throw new ServiceStopped();
    }
    public static void JoinWorker(Lwip owner,long worker)
    {
        var state=For(owner);Thread thread;lock(state.Registry)
            thread=state.WorkerHandles.TryGetValue(worker,out var value)?value:throw new InvalidOperationException("Invalid or foreign worker handle.");
        if(thread==Thread.CurrentThread)throw new InvalidOperationException("Cannot join current worker.");
        if(!thread.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("Ancillary worker did not quiesce; owner retained.");
    }
    public static void Init(Lwip owner) => _ = For(owner);
    public static uint Now(Lwip owner) => For(owner).Clock?.Invoke() ?? unchecked((uint)Environment.TickCount64);
    public static uint Jiffies(Lwip owner) => Now(owner);
    public static uint Random(Lwip owner)
    {
        uint result;
        RandomNumberGenerator.Fill(new Span<byte>(&result, sizeof(uint)));
        return result;
    }
    public static void SetClock(Lwip owner, Func<uint> clock) => For(owner).Clock = clock;
    public static void Assert(Lwip owner, byte* message, byte* file, int line) => throw new InvalidOperationException($"{Marshal.PtrToStringUTF8((nint)file)}:{line}: {Marshal.PtrToStringUTF8((nint)message)}");
    public static sbyte SemNew(Lwip owner, long* handle, byte count)
    {
        try { *handle = Register(For(owner), new Resource { Signals = count }); return 0; }
        catch (OutOfMemoryException) { *handle = 0; return ErrMem; }
    }
    public static void SemSignal(Lwip owner, long* handle)
    {
        var state = For(owner); var resource = Resolve(state, *handle);
        lock (resource.Gate) { Check(state, resource); resource.Signals = 1; Monitor.Pulse(resource.Gate); }
    }
    public static uint SemWait(Lwip owner, long* handle, uint timeout)
    {
        var state = For(owner); var resource = Resolve(state, *handle); long start = Stopwatch.GetTimestamp();
        lock (resource.Gate)
        {
            while (resource.Signals == 0) if (!Wait(state, resource, timeout, start)) return Timeout;
            Check(state, resource); resource.Signals--; return Elapsed(start);
        }
    }
    public static void SemFree(Lwip owner, long* handle) => Free(For(owner), handle);
    public static sbyte MboxNew(Lwip owner, long* handle, int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        try { *handle = Register(For(owner), new Resource { Capacity = size }); return 0; }
        catch (OutOfMemoryException) { *handle = 0; return ErrMem; }
    }
    public static void MboxPost(Lwip owner, long* handle, void* message)
    {
        var state = For(owner); var resource = Resolve(state, *handle);
        lock (resource.Gate)
        {
            while (resource.Messages.Count == resource.Capacity) Wait(state, resource, 0, 0);
            Check(state, resource); resource.Messages.Enqueue((nint)message); Monitor.PulseAll(resource.Gate);
        }
    }
    public static sbyte MboxTryPost(Lwip owner, long* handle, void* message)
    {
        var state = For(owner); var resource = Resolve(state, *handle);
        lock (resource.Gate)
        {
            Check(state, resource); if (resource.Messages.Count == resource.Capacity) return ErrMem;
            resource.Messages.Enqueue((nint)message); Monitor.PulseAll(resource.Gate); return 0;
        }
    }
    public static uint MboxFetch(Lwip owner, long* handle, void** message, uint timeout)
    {
        var state = For(owner); var resource = Resolve(state, *handle); long start = Stopwatch.GetTimestamp();
        lock (resource.Gate)
        {
            while (resource.Messages.Count == 0) if (!Wait(state, resource, timeout, start)) return Timeout;
            Check(state, resource); var value = resource.Messages.Dequeue(); if (message != null) *message = (void*)value;
            Monitor.PulseAll(resource.Gate); return Elapsed(start);
        }
    }
    public static uint MboxTryFetch(Lwip owner, long* handle, void** message)
    {
        var state = For(owner); var resource = Resolve(state, *handle);
        lock (resource.Gate)
        {
            Check(state, resource); if (resource.Messages.Count == 0) return Timeout;
            var value = resource.Messages.Dequeue(); if (message != null) *message = (void*)value;
            Monitor.PulseAll(resource.Gate); return 0;
        }
    }
    public static void MboxFree(Lwip owner, long* handle)
    {
        var state = For(owner); var resource = Resolve(state, *handle);
        lock (resource.Gate) if (resource.Messages.Count != 0) throw new InvalidOperationException("Freeing a nonempty mailbox.");
        Free(state, handle);
    }
    public static sbyte MutexNew(Lwip owner, long* handle) => SemNew(owner, handle, 0);
    public static void MutexLock(Lwip owner, long* handle)
    {
        var state = For(owner); var resource = Resolve(state, *handle);
        Monitor.Enter(resource.Gate);
        try { Check(state, resource); } catch { Monitor.Exit(resource.Gate); throw; }
    }
    public static void MutexUnlock(Lwip owner, long* handle) => Monitor.Exit(Resolve(For(owner), *handle).Gate);
    public static void MutexFree(Lwip owner, long* handle) => Free(For(owner), handle);
    public static ulong Protect(Lwip owner)
    {
        var state = For(owner); Monitor.Enter(state.Protection); uint previous=state.ProtectionDepth.Value;state.ProtectionDepth.Value=checked(previous+1);return previous;
    }
    public static void Unprotect(Lwip owner, ulong token)
    {
        var state=For(owner);
        if(!Monitor.IsEntered(state.Protection)||state.ProtectionDepth.Value==0||token!=state.ProtectionDepth.Value-1)throw new InvalidOperationException("sys_arch protection must unwind on its entering thread in nesting order.");
        state.ProtectionDepth.Value--;Monitor.Exit(state.Protection);
    }
    /// <summary>Whether this thread is executing on the owner TCP/IP worker or holds its core lock.</summary>
    public static bool IsCoreContext(Lwip owner)
    {var state=For(owner);return state.CoreThread==Environment.CurrentManagedThreadId||Monitor.IsEntered(state.CoreLock);}
    public static void MarkCoreThread(Lwip owner) => For(owner).CoreThread = Environment.CurrentManagedThreadId;
    public static void LockCore(Lwip owner) => Monitor.Enter(For(owner).CoreLock);
    public static void UnlockCore(Lwip owner) => Monitor.Exit(For(owner).CoreLock);
    public static void CheckCore(Lwip owner)
    {
        var state = For(owner);
        if (state.CoreThread != 0 && state.CoreThread != Environment.CurrentManagedThreadId && !Monitor.IsEntered(state.CoreLock))
            throw new InvalidOperationException("lwIP core accessed outside its TCP/IP thread or core lock.");
    }
    public static long ThreadNew(Lwip owner, byte* name, delegate*<Lwip, void*, void> entry, void* argument, int stackSize, int priority)
    {
        var state = For(owner); var lease = owner.__DotCcRetain();
        var callback = (nint)entry; var payload = (nint)argument;long worker=Interlocked.Increment(ref nextHandle);
        Thread thread;
        try { thread = new Thread(() =>
        {
            try { state.CurrentWorker.Value=worker;using var binding = owner.__DotCcEnter(); ((delegate*<Lwip, void*, void>)callback)(owner, (void*)payload); }
            catch (Stopped) when (state.Stopping) { }
            catch (ServiceStopped) when (ServiceExitExpected(state,worker)) { }
            catch (Exception error) { lock (state.Registry) state.Failures.Add(error); }
            finally {
                while (Monitor.IsEntered(state.CoreLock)) Monitor.Exit(state.CoreLock);
                ThreadSemFree(owner);state.CurrentWorker.Value=0; lease.Dispose();
            }
        }, Math.Max(0,stackSize)) { IsBackground = true, Name = Marshal.PtrToStringUTF8((nint)name) ?? "lwIP" }; }
        catch { lease.Dispose();throw; }
        lock (state.Registry)
        {
            if (state.Stopping) { lease.Dispose(); throw new Stopped(); }
            state.Threads.Add(thread);state.WorkerHandles.Add(worker,thread);state.WorkerArguments.Add(worker,payload);
            try { thread.Start(); } catch { state.Threads.Remove(thread);state.WorkerHandles.Remove(worker);state.WorkerArguments.Remove(worker); lease.Dispose(); throw; }
            return worker;
        }
    }
    private static bool ServiceExitExpected(State state,long worker){lock(state.Registry)return state.ServiceStopRequests.Contains(worker);}
    public static long* ThreadSemGet(Lwip owner)
    {
        var state = For(owner);
        lock (state.Registry)
        {
            return state.ThreadSemaphores.TryGetValue(Environment.CurrentManagedThreadId, out var address) ? (long*)address : null;
        }
    }
    public static void ThreadSemAlloc(Lwip owner)
    {
        var state = For(owner);
        lock (state.Registry)
        {
            int id = Environment.CurrentManagedThreadId;
            if (state.ThreadSemaphores.ContainsKey(id)) return;
            var address = (long*)NativeMemory.AllocZeroed((nuint)sizeof(long));
            try
            {
                if (SemNew(owner, address, 0) != 0) throw new OutOfMemoryException();
                state.ThreadSemaphores.Add(id, (nint)address);
            }
            catch { NativeMemory.Free(address); throw; }
        }
    }
    public static void ThreadSemFree(Lwip owner)
    {
        var state = For(owner);
        lock (state.Registry)
        {
            if (state.ThreadSemaphores.Remove(Environment.CurrentManagedThreadId, out var address))
            { SemFree(owner, (long*)address); NativeMemory.Free((void*)address); }
        }
    }
    public static (int Resources, int Workers, int ThreadSemaphores) Counters(Lwip owner)
    {
        var state = For(owner); lock (state.Registry)
        { int workers = 0; foreach (var thread in state.Threads) if (thread.IsAlive) workers++; return (state.Resources.Count, workers, state.ThreadSemaphores.Count); }
    }
    /// <summary>Called after protocol cleanup and public callers have quiesced. Wakes idle workers and joins every callback lease.</summary>
    public static void Stop(Lwip owner)
    {
        var state = For(owner); Resource[] resources; Thread[] threads;
        lock (state.Registry)
        {
            foreach (var thread in state.Threads) if (thread == Thread.CurrentThread) throw new InvalidOperationException("Cannot dispose lwIP on its worker.");
            state.Stopping = true; resources = new List<Resource>(state.Resources.Values).ToArray(); threads = state.Threads.ToArray();
        }
        foreach (var resource in resources) lock (resource.Gate) Monitor.PulseAll(resource.Gate);
        foreach (var channel in state.Serial.Values) channel.Abort();
        foreach (var thread in threads) if (!thread.Join(TimeSpan.FromSeconds(10))) throw new TimeoutException("lwIP worker failed to stop; owner backing retained.");
        lock (state.Registry)
        {
            foreach (var address in state.ThreadSemaphores.Values) NativeMemory.Free((void*)address);
            state.ThreadSemaphores.Clear(); state.Resources.Clear();state.Serial.Clear();state.SerialHandles.Clear();state.SerialByHandle.Clear();
            if (state.Failures.Count != 0)
            {
                var failures=state.Failures.ToArray();state.Failures.Clear();
                throw new AggregateException("lwIP worker failed; host resources reclaimed. Dispose may be retried to release the retained owner.",failures);
            }
        }
    }
}

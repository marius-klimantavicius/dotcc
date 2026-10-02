#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Edg.Runtime;

internal sealed unsafe class Owner : IDisposable
{
    internal readonly ulong[] Storage = GC.AllocateArray<ulong>(32, pinned: true);
    internal ulong* Slots { get { fixed (ulong* p = Storage) return p; } }
    internal readonly LibcxxThreads<Owner> Threads;
    internal readonly object Gate = new();
    internal readonly ThreadLocal<int> Depth = new(() => 0);
    internal readonly ManualResetEventSlim Barrier = new(false);
    internal int Leases, Entered, Left, Released, TlsCalls, Count, OnceCalls, Ready, Waiting;
    internal bool Closing, Closed;
    internal Owner() => Threads = new(this, &Retain, &Release, &Enter, &Leave, &ReleaseThread, &Terminate);
    private static void Retain(Owner self) { lock (self.Gate) { if (self.Closing) throw new Exception("new lease during shutdown"); ++self.Leases; } }
    private static void Release(Owner self) { lock (self.Gate) { --self.Leases; Monitor.PulseAll(self.Gate); } }
    private static void Enter(Owner self) { lock (self.Gate) { Require(!self.Closed, "closed retained entry"); ++self.Entered; ++self.Depth.Value; } }
    private static void Leave(Owner self) { lock (self.Gate) { Require(self.Depth.Value == 1, "thread depth"); --self.Depth.Value; ++self.Left; } }
    private static void ReleaseThread(Owner self)
    {
        Require(self.Depth.Value == 0, "release follows leave");
        ++self.Depth.Value;
        try { self.Threads.ReleaseThread(); }
        finally { --self.Depth.Value; Interlocked.Increment(ref self.Released); }
    }
    private static void Terminate(Owner self, Exception error)
    { Console.WriteLine("terminate"); Console.Out.Flush(); Environment.Exit(73); }
    internal static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    public void Dispose()
    {
        lock (Gate)
        {
            Closing = true;
            while (Leases != 0) Monitor.Wait(Gate);
            Require(Entered == Left && Entered == Released, "all worker lifetime hooks paired");
            Require(!Closed, "owner disposed once");
            Threads.ReleaseThread(); Threads.Dispose(); Closed = true;
        }
        GC.KeepAlive(Storage);
    }
}

internal static unsafe class ThreadRuntimeTest
{
    private static void Check(bool condition, string message) => Owner.Require(condition, message);
    private static void TlsDestructor(Owner owner, void* pointer)
    {
        Check(owner.Depth.Value == 1, "key destructor runs inside retained cleanup scope");
        Interlocked.Increment(ref owner.TlsCalls);
        int* remaining = (int*)pointer;
        if (--*remaining != 0) Check(owner.Threads.TlsSet(owner.Slots[1], pointer) == 0, "TLS destructor reinstalls value");
        else NativeMemory.Free(pointer);
    }
    private static void* Counter(Owner owner, void* argument)
    {
        int* rounds = (int*)NativeMemory.Alloc(4); *rounds = 2;
        Check(owner.Threads.TlsSet(owner.Slots[1], rounds) == 0, "worker TLS set");
        for (int i = 0; i != 1500; ++i)
        {
            Check(owner.Threads.MutexLock(owner.Slots) == 0, "counter lock");
            ++owner.Count;
            Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "counter unlock");
        }
        return null;
    }
    private static void* StaticCounter(void* argument)
    { Interlocked.Increment(ref *(int*)argument); return argument; }
    private static void OnceBody(Owner owner)
    { if (Interlocked.Increment(ref owner.OnceCalls) == 1) throw new ApplicationException("retry once"); }
    private static int staticOnce;
    private static void StaticOnce() { ++staticOnce; }
    private static void* Notify(Owner owner, void* ignored)
    {
        Check(owner.Threads.MutexLock(owner.Slots) == 0, "producer lock");
        owner.Ready = 1;
        Check(owner.Threads.ConditionSignal(owner.Slots + 2) == 0, "producer signal");
        Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "producer unlock");
        return null;
    }
    private static void* Wait(Owner owner, void* ignored)
    {
        Check(owner.Threads.MutexLock(owner.Slots) == 0, "waiter lock");
        ++owner.Waiting;
        while (owner.Ready == 0) Check(owner.Threads.ConditionWait(owner.Slots + 2, owner.Slots) == 0, "waiter broadcast wait");
        Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "waiter unlock");
        return null;
    }
    private static void* Detached(Owner owner, void* ignored)
    { owner.Barrier.Wait(); Interlocked.Increment(ref owner.Count); return null; }
    private static void* Fatal(Owner owner, void* ignored)
    {
        try { Console.WriteLine("body"); throw new ApplicationException("thread exception"); }
        finally { Console.WriteLine("unwind"); }
    }
    private static void Counters(Owner owner)
    {
        Check(owner.Threads.TlsCreate(owner.Slots + 1, &TlsDestructor) == 0, "create TLS key");
        for (int i = 0; i != 4; ++i) Check(owner.Threads.ThreadCreate(owner.Slots + 8 + i, &Counter, null) == 0, "create counter");
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        for (int i = 0; i != 4; ++i)
        {
            ulong id = owner.Threads.ThreadGetId(owner.Slots + 8 + i);
            Check(id != 0 && id != LibcxxThreads<Owner>.CurrentThreadId(), "thread id");
            Check(owner.Threads.ThreadJoin(owner.Slots + 8 + i) == 0, "join counter");
            Check(LibcxxThreads<Owner>.ThreadIsNull(owner.Slots + 8 + i), "joined handle null");
        }
        Check(owner.Count == 6000 && owner.TlsCalls == 8, "counter/TLS rounds");
        int* count = (int*)NativeMemory.Alloc(4); *count = 0;
        try
        {
            Check(owner.Threads.ThreadCreate(owner.Slots + 8, &StaticCounter, count) == 0, "static callback");
            Check(owner.Threads.ThreadJoin(owner.Slots + 8) == 0 && *count == 1, "static callback result");
        }
        finally { NativeMemory.Free(count); }
    }
    private static void OnceAndMutex(Owner owner)
    {
        bool caught = false;
        try { owner.Threads.ExecuteOnce(owner.Slots + 3, &OnceBody); } catch (ApplicationException) { caught = true; }
        Check(caught && owner.OnceCalls == 1, "throwing once propagates");
        Parallel.For(0, 8, _ => Check(owner.Threads.ExecuteOnce(owner.Slots + 3, &OnceBody) == 0, "once retry/concurrency"));
        Check(owner.OnceCalls == 2, "once exactly one successful execution");
        Check(owner.Threads.ExecuteOnce(owner.Slots + 4, &StaticOnce) == 0 &&
              owner.Threads.ExecuteOnce(owner.Slots + 4, &StaticOnce) == 0 && staticOnce == 1, "static once");
        Check(owner.Threads.RecursiveMutexInit(owner.Slots + 5) == 0, "recursive init");
        Check(owner.Threads.MutexLock(owner.Slots + 5, true) == 0 && owner.Threads.MutexTryLock(owner.Slots + 5, true), "recursive nesting");
        Check(owner.Threads.MutexDestroy(owner.Slots + 5, true) == LibcxxThreads<Owner>.Ebusy, "busy destroy");
        Check(owner.Threads.MutexUnlock(owner.Slots + 5, true) == 0 && owner.Threads.MutexUnlock(owner.Slots + 5, true) == 0, "recursive unlock");
        Check(owner.Threads.MutexDestroy(owner.Slots + 5, true) == 0, "recursive destroy");
        Check(owner.Threads.MutexTryLock(owner.Slots), "plain try lock");
        Check(!owner.Threads.MutexTryLock(owner.Slots), "plain recursive misuse does not acquire extra level");
        Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "plain unlock");
        using var other = new Owner();
        Check(other.Threads.MutexLock(owner.Slots) == LibcxxThreads<Owner>.Einval, "foreign-owner mutex rejected");
        Check(other.Threads.TlsGet(owner.Slots[1]) == null && other.Threads.TlsSet(owner.Slots[1], null) == LibcxxThreads<Owner>.Einval, "owner TLS keys isolated");
    }
    private static void Conditions(Owner owner)
    {
        for (int i = 0; i != 10; ++i)
        {
            owner.Ready = 0;
            Check(owner.Threads.MutexLock(owner.Slots) == 0, "consumer lock");
            Check(owner.Threads.ThreadCreate(owner.Slots + 8, &Notify, null) == 0, "producer create");
            while (owner.Ready == 0) Check(owner.Threads.ConditionWait(owner.Slots + 2, owner.Slots) == 0, "consumer wait");
            Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "consumer unlock");
            Check(owner.Threads.ThreadJoin(owner.Slots + 8) == 0, "producer join");
        }
        owner.Ready = 0;
        for (int i = 0; i != 3; ++i) Check(owner.Threads.ThreadCreate(owner.Slots + 8 + i, &Wait, null) == 0, "broadcast waiter create");
        for (;;)
        {
            Check(owner.Threads.MutexLock(owner.Slots) == 0, "broadcast main lock");
            if (owner.Waiting == 3) break;
            Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "broadcast main unlock");
            Thread.Yield();
        }
        Check(owner.Threads.ConditionDestroy(owner.Slots + 2) == LibcxxThreads<Owner>.Ebusy, "waiting condition destroy rejected");
        owner.Ready = 1;
        Check(owner.Threads.ConditionSignal(owner.Slots + 2, true) == 0, "broadcast");
        Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "broadcast release mutex");
        for (int i = 0; i != 3; ++i) Check(owner.Threads.ThreadJoin(owner.Slots + 8 + i) == 0, "broadcast join");
        var past = new LibcxxThreads<Owner>.Timespec { Seconds = 0, Nanoseconds = 0 };
        Check(owner.Threads.MutexLock(owner.Slots) == 0, "timeout lock");
        Check(owner.Threads.ConditionWait(owner.Slots + 2, owner.Slots, &past) == LibcxxThreads<Owner>.Etimedout, "past deadline");
        past.Nanoseconds = -1;
        Check(owner.Threads.ConditionWait(owner.Slots + 2, owner.Slots, &past) == LibcxxThreads<Owner>.Einval, "invalid deadline");
        Check(owner.Threads.MutexUnlock(owner.Slots) == 0, "timeout reacquires mutex");
        Check(owner.Threads.ConditionDestroy(owner.Slots + 2) == 0, "condition destroy");
    }
    private static void DetachedLifetime()
    {
        var owner = new Owner();
        Check(owner.Threads.ThreadCreate(owner.Slots + 8, &Detached, null) == 0, "detached create");
        Check(owner.Threads.ThreadDetach(owner.Slots + 8) == 0 && owner.Slots[8] == 0, "detach invalidates handle");
        bool rejected = false;
        try { owner.Threads.Dispose(); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "live detached worker retains runtime");
        Task shutdown = Task.Run(owner.Dispose);
        SpinWait.SpinUntil(() => { lock (owner.Gate) return owner.Closing; });
        Check(!shutdown.IsCompleted, "shutdown waits retained worker");
        owner.Barrier.Set(); shutdown.GetAwaiter().GetResult();
        Check(owner.Closed && owner.Count == 1 && owner.Leases == 0, "detached worker completes before disposal");
    }
    public static int Main(string[] args)
    {
        if (args.Length != 0)
        {
            var fatal = new Owner();
            Check(fatal.Threads.ThreadCreate(fatal.Slots + 8, &Fatal, null) == 0, "fatal create");
            fatal.Threads.ThreadJoin(fatal.Slots + 8);
            return 99;
        }
        using (var owner = new Owner()) { Counters(owner); OnceAndMutex(owner); Conditions(owner); }
        DetachedLifetime();
        LibcxxThreads<Owner>.ThreadSleepFor(1_000_000);
        Console.WriteLine("libc++ BCL thread runtime: all synthetic checks passed");
        return 0;
    }
}

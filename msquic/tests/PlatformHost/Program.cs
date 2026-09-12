using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Managed.Transport;
using Managed.Transport.Hosting;

internal static unsafe partial class Program
{
    private const uint Tag = 0x5433504c;
    private static MSQUIC_HOST_TABLE table;
    private static MSQUIC_HOST_TABLE services;
    private static int failureKind;
    private static int failureAt;
    private static int failureCalls;
    private static CXPLAT_EVENTQ* currentQueue;
    private static int completions;
    private static int reenqueue;
    private static MsQuicHost? activeHost;
    private static CXPLAT_SQE* deferredTarget;
    private static int deferredRequested;
    private static int deferredFinished;
    private static int closingNotifications;
    private static readonly SemaphoreSlim executionSignal = new(0);
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static T* Allocate<T>() where T : unmanaged
    {
        var pointer = (T*)table.CxPlatAlloc(table.Context, (ulong)sizeof(T), Tag);
        Check(pointer != null, "allocation");
        return pointer;
    }
    private static void Free(void* pointer) => table.CxPlatFree(table.Context, pointer, Tag);
    private static bool Inject(int kind) => failureKind==kind && Interlocked.Increment(ref failureCalls)==failureAt;
    private static void* AllocWithFailure(void* context,ulong size,uint tag) => Inject(1)?null:services.CxPlatAlloc(context,size,tag);
    private static byte QueueWithFailure(void* context,CXPLAT_EVENTQ* queue) => Inject(2)?(byte)0:services.CxPlatEventQInitialize(context,queue);
    private static byte SqeWithFailure(void* context,CXPLAT_EVENTQ* queue,delegate*<CXPLAT_CQE*,void> completion,CXPLAT_SQE* sqe) => Inject(3)?(byte)0:services.CxPlatSqeInitialize(context,queue,completion,sqe);
    private static uint ThreadWithFailure(void* context,CXPLAT_THREAD_CONFIG* config,ulong* output) => Inject(4)?Status.OutOfMemory:services.CxPlatThreadCreate(context,config,output);
    private static void Completion(CXPLAT_CQE* item)
    {
        Interlocked.Increment(ref completions);
        if (Interlocked.Exchange(ref reenqueue, 0) == 1)
            Check(table.CxPlatEventQEnqueue(table.Context, currentQueue, item->Sqe) == 1, "reentrant enqueue");
    }
    private static void DeferredCompletion(CXPLAT_CQE* item)
    {
        if(item->Sqe==deferredTarget)
        {
            Check(deferredRequested==1&&deferredFinished==0,"later notification retains closed owner until batch return");
            closingNotifications++;
            return;
        }
        deferredRequested=1;
        activeHost!.CleanupDeferred(currentQueue,deferredTarget,()=>Interlocked.Increment(ref deferredFinished));
        Check(deferredTarget->Handle!=0&&deferredFinished==0,"deferred SQE remains alive during callbacks");
        Check(table.CxPlatEventQEnqueue(table.Context,currentQueue,deferredTarget)==0,"closing SQE rejects enqueue");
    }
    private static void* ThreadCallback(void* value)
    {
        Interlocked.Increment(ref *(int*)value);
        return null;
    }
    private static void* SelfJoin(void* value)
    {
        table.CxPlatThreadWait(table.Context,(ulong*)value);
        return null;
    }
    private static byte Execution(void* value, CXPLAT_EXECUTION_STATE* state)
    {
        int* words = (int*)value;
        Check(state->ThreadID == table.CxPlatCurThreadID(table.Context), "worker thread identity");
        Interlocked.Increment(ref words[0]);
        bool remove = Volatile.Read(ref words[1]) != 0;
        executionSignal.Release();
        return remove ? (byte)0 : (byte)1;
    }
    private static void Memory()
    {
        foreach (ulong length in new ulong[] {1,16,17,65536})
        {
            byte* p = (byte*)table.CxPlatAlloc(table.Context, length, Tag);
            Check(p != null && (nuint)p % 16 == 0, "16-byte actual allocation alignment");
            Check(new ReadOnlySpan<byte>(p,(int)length).IndexOfAnyExcept((byte)0) < 0, "zeroed allocation");
            p[0] = 91;
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            Check(p[0] == 91, "pinned allocation root");
            Free(p);
        }
        Check(table.CxPlatAlloc(table.Context, ulong.MaxValue, Tag) == null, "oversized allocation rejection");
        var random = (byte*)table.CxPlatAllocUninitialized(table.Context, 64, Tag);
        Check(random != null && (nuint)random % 16 == 0, "uninitialized alignment");
        Check(table.CxPlatRandom(table.Context,64,random) == 0, "entropy");
        Check(new ReadOnlySpan<byte>(random,64).IndexOfAnyExcept((byte)0) >= 0, "entropy output");
        Check(table.CxPlatRandom(table.Context,1,null) != 0, "entropy invalid argument");
        Free(random);
        table.CxPlatFree(table.Context,null,Tag);
    }
    private static void LocksAndEvents()
    {
        var recursive = Allocate<CXPLAT_LOCK>();
        table.CxPlatLockInitialize(table.Context,recursive);
        table.CxPlatLockAcquire(table.Context,recursive);
        table.CxPlatLockAcquire(table.Context,recursive);
        using var started = new ManualResetEventSlim();
        using var entered = new ManualResetEventSlim();
        nint lockAddress = (nint)recursive;
        var contender = new Thread(() => { started.Set(); table.CxPlatLockAcquire(table.Context,(CXPLAT_LOCK*)lockAddress); entered.Set(); table.CxPlatLockRelease(table.Context,(CXPLAT_LOCK*)lockAddress); });
        contender.Start(); started.Wait();
        Check(!entered.Wait(25), "recursive lock excludes contender");
        table.CxPlatLockRelease(table.Context,recursive);
        Check(!entered.Wait(25), "first recursive release retains lock");
        table.CxPlatLockRelease(table.Context,recursive);
        Check(entered.Wait(5000), "contender released"); contender.Join();
        table.CxPlatLockUninitialize(table.Context,recursive); Free(recursive);

        var rw = Allocate<CXPLAT_RW_LOCK>();
        table.CxPlatRwLockInitialize(table.Context,rw);
        table.CxPlatRwLockAcquireShared(table.Context,rw);
        table.CxPlatRwLockAcquireShared(table.Context,rw);
        started.Reset(); entered.Reset(); nint rwAddress=(nint)rw;
        contender = new Thread(() => { started.Set(); table.CxPlatRwLockAcquireExclusive(table.Context,(CXPLAT_RW_LOCK*)rwAddress); entered.Set(); table.CxPlatRwLockReleaseExclusive(table.Context,(CXPLAT_RW_LOCK*)rwAddress); });
        contender.Start(); started.Wait(); Check(!entered.Wait(25),"reader excludes writer");
        table.CxPlatRwLockReleaseShared(table.Context,rw); table.CxPlatRwLockReleaseShared(table.Context,rw);
        Check(entered.Wait(5000),"writer released"); contender.Join();
        table.CxPlatRwLockUninitialize(table.Context,rw); Free(rw);

        var ev=Allocate<CXPLAT_EVENT>(); nint eventAddress=(nint)ev;
        table.CxPlatEventInitialize(table.Context,ev,0,1);
        Check(table.CxPlatInternalEventWaitWithTimeout(table.Context,ev,0)==1,"initial auto-reset signal");
        Check(table.CxPlatInternalEventWaitWithTimeout(table.Context,ev,0)==0,"auto-reset consumed");
        using var ready=new CountdownEvent(2); using var completed=new SemaphoreSlim(0);
        Thread[] waiters=Enumerable.Range(0,2).Select(_=>new Thread(()=>{ready.Signal(); table.CxPlatInternalEventWaitForever(table.Context,(CXPLAT_EVENT*)eventAddress); completed.Release();})).ToArray();
        foreach(var t in waiters)t.Start(); ready.Wait();
        table.CxPlatInternalEventSet(table.Context,ev);
        Check(completed.Wait(5000),"one auto-reset waiter"); Check(!completed.Wait(25),"only one auto-reset waiter");
        table.CxPlatInternalEventSet(table.Context,ev); Check(completed.Wait(5000),"second auto-reset waiter");
        foreach(var t in waiters)t.Join();
        table.CxPlatInternalEventUninitialize(table.Context,ev);
        table.CxPlatEventInitialize(table.Context,ev,1,0);
        ready.Reset();
        waiters=Enumerable.Range(0,2).Select(_=>new Thread(()=>{ready.Signal(); table.CxPlatInternalEventWaitForever(table.Context,(CXPLAT_EVENT*)eventAddress); completed.Release();})).ToArray();
        foreach(var t in waiters)t.Start(); ready.Wait(); table.CxPlatInternalEventSet(table.Context,ev);
        Check(completed.Wait(5000)&&completed.Wait(5000),"manual-reset broadcasts"); foreach(var t in waiters)t.Join();
        Check(table.CxPlatInternalEventWaitWithTimeout(table.Context,ev,0)==1,"manual-reset retains signal");
        table.CxPlatInternalEventReset(table.Context,ev);
        ulong before=table.CxPlatTimeUs64(table.Context);
        Check(table.CxPlatInternalEventWaitWithTimeout(table.Context,ev,15)==0,"finite event timeout");
        Check(table.CxPlatTimeUs64(table.Context)-before>=10_000,"timeout monotonic units");
        table.CxPlatInternalEventUninitialize(table.Context,ev); Free(ev);
    }
    private static void Queue()
    {
        var q=Allocate<CXPLAT_EVENTQ>(); currentQueue=q;
        Check(table.CxPlatEventQInitialize(table.Context,q)==1,"queue init");
        var sqes=(CXPLAT_SQE*)table.CxPlatAlloc(table.Context,(ulong)(sizeof(CXPLAT_SQE)*32),Tag);
        Check(sqes!=null,"SQE array");
        for(int i=0;i<32;i++)Check(table.CxPlatSqeInitialize(table.Context,q,&Completion,sqes+i)==1,"SQE init");
        nint qa=(nint)q, sa=(nint)sqes;
        Thread[] producers=Enumerable.Range(0,4).Select(_=>new Thread(()=>{for(int n=0;n<1000;n++)Check(table.CxPlatEventQEnqueue(table.Context,(CXPLAT_EVENTQ*)qa,(CXPLAT_SQE*)sa+n%32)==1,"producer enqueue");})).ToArray();
        foreach(var p in producers)p.Start(); foreach(var p in producers)p.Join();
        CXPLAT_CQE* batch=stackalloc CXPLAT_CQE[16];
        for(int part=0;part<2;part++)
        {
            uint count=table.CxPlatEventQDequeue(table.Context,q,batch,16,0); Check(count==16,"bounded coalesced batch");
            for(int i=0;i<count;i++)batch[i].Sqe->Completion(batch+i);
            table.CxPlatEventQReturn(table.Context,q,count);
        }
        Check(completions==32&&table.CxPlatEventQDequeue(table.Context,q,batch,16,0)==0,"coalesced notifications");
        reenqueue=1; table.CxPlatEventQEnqueue(table.Context,q,sqes);
        for(int round=0;round<2;round++)
        {
            uint count=table.CxPlatEventQDequeue(table.Context,q,batch,16,0); Check(count==1,"callback reenqueued next batch");
            batch->Sqe->Completion(batch); table.CxPlatEventQReturn(table.Context,q,count);
        }
        Check(completions==34,"reentrant completion count");
        // A foreign cleanup waits for the current batch before freeing its token.
        table.CxPlatEventQEnqueue(table.Context,q,sqes+31);
        Check(table.CxPlatEventQDequeue(table.Context,q,batch,16,0)==1,"cleanup batch");
        using var cleaning=new ManualResetEventSlim(); using var cleaned=new ManualResetEventSlim();
        var cleaner=new Thread(()=>{cleaning.Set();table.CxPlatSqeCleanup(table.Context,(CXPLAT_EVENTQ*)qa,(CXPLAT_SQE*)sa+31);cleaned.Set();});
        cleaner.Start();cleaning.Wait();Check(!cleaned.Wait(25),"cleanup retains in-flight C SQE");
        batch->Sqe->Completion(batch);table.CxPlatEventQReturn(table.Context,q,1);
        Check(cleaned.Wait(5000),"cleanup released after return");cleaner.Join();
        for(int i=0;i<31;i++){table.CxPlatEventQEnqueue(table.Context,q,sqes+i);table.CxPlatSqeCleanup(table.Context,q,sqes+i);}
        Check(table.CxPlatEventQDequeue(table.Context,q,batch,16,0)==0,"pending cleanup cancels control notification");
        // Deleting a later SQE from another callback in this same batch is legal:
        // its internal notification sees closing, and memory is released at Return.
        Check(table.CxPlatSqeInitialize(table.Context,q,&DeferredCompletion,sqes)==1,"deferred closer init");
        Check(table.CxPlatSqeInitialize(table.Context,q,&DeferredCompletion,sqes+1)==1,"deferred target init");
        deferredTarget=sqes+1;
        table.CxPlatEventQEnqueue(table.Context,q,sqes);table.CxPlatEventQEnqueue(table.Context,q,sqes+1);
        Check(table.CxPlatEventQDequeue(table.Context,q,batch,16,0)==2,"deferred batch");
        for(int i=0;i<2;i++)batch[i].Sqe->Completion(batch+i);
        table.CxPlatEventQReturn(table.Context,q,2);
        Check(deferredFinished==1&&closingNotifications==1&&sqes[1].Handle==0,"deferred cleanup after complete batch");
        table.CxPlatSqeCleanup(table.Context,q,sqes);
        table.CxPlatEventQCleanup(table.Context,q);Free(sqes);Free(q);currentQueue=null;
    }
    private static void ThreadsAndWorker()
    {
        var state=(int*)table.CxPlatAlloc(table.Context,8,Tag); Check(state!=null,"thread state");
        CXPLAT_THREAD_CONFIG config=default;config.Callback=&ThreadCallback;config.Context=state;
        ulong thread=0;
        Check(table.CxPlatThreadCreate(table.Context,&config,&thread)==0&&thread!=0,"thread creation");
        config.Context=null; config.Callback=null;
        table.CxPlatThreadWait(table.Context,&thread); table.CxPlatThreadDelete(table.Context,&thread);
        Check(state[0]==1&&thread==0,"copied callback context and joined lifetime");
        config.Callback=&ThreadCallback;config.Context=state;config.Flags=2;
        Check(table.CxPlatThreadCreate(table.Context,&config,&thread)==Status.NotSupported&&thread==0,"affinity rejects");
        state[0]=0;
        var pool=MsQuic.CxPlatWorkerPoolCreate(null,CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_LIBRARY);
        Check(pool!=null&&MsQuic.CxPlatWorkerPoolGetCount(pool)==2,"actual translated worker pool");
        var ec=Allocate<CXPLAT_EXECUTION_CONTEXT>();ec->Callback=&Execution;ec->Context=state;ec->NextTimeUs=ulong.MaxValue;ec->Ready=1;
        MsQuic.CxPlatWorkerPoolAddExecutionContext(pool,ec,0);
        Check(executionSignal.Wait(5000),"actual initial EC callback");
        for(int i=0;i<20;i++)
        {
            Volatile.Write(ref ec->Ready,(byte)1); MsQuic.CxPlatWakeExecutionContext(ec);
            Check(executionSignal.Wait(5000),"actual cross-thread wake callback");
        }
        Volatile.Write(ref state[1],1);Volatile.Write(ref ec->Ready,(byte)1);MsQuic.CxPlatWakeExecutionContext(ec);
        Check(executionSignal.Wait(5000),"EC removal");
        MsQuic.CxPlatWorkerPoolDelete(pool,CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_LIBRARY);
        Check(state[0]==22,"exact execution callbacks");
        Free(ec);Free(state);
    }
    private static void FailureRollback(MsQuicHost host)
    {
        foreach(var pair in new[]{(Kind:1,Count:1),(Kind:2,Count:2),(Kind:3,Count:6),(Kind:4,Count:2)})
        for(int position=1;position<=pair.Count;position++)
        {
            failureKind=pair.Kind;failureAt=position;failureCalls=0;
            var pool=MsQuic.CxPlatWorkerPoolCreate(null,CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_LIBRARY);
            failureKind=0;
            Check(pool==null,"injected worker initialization failure");
            Check(host.OutstandingResources==0&&host.OutstandingPlatformAllocations==0,"partial worker initialization rolled back all ownership");
        }
    }
    private static int Main(string[] args)
    {
        if(args.Length!=0&&args[0]=="status"){PrintStatus();return 0;}
        using var host=new MsQuicHost();activeHost=host;table=host.CreatePlatformTable();services=table;
        // Failures are injected by the test table, never by production success stubs.
        table.CxPlatAlloc=&AllocWithFailure;table.CxPlatEventQInitialize=&QueueWithFailure;
        table.CxPlatSqeInitialize=&SqeWithFailure;table.CxPlatThreadCreate=&ThreadWithFailure;
        if(args.Length!=0 && args[0]=="self-join")
        {
            var output=Allocate<ulong>();CXPLAT_THREAD_CONFIG config=default;config.Context=output;config.Callback=&SelfJoin;
            Check(table.CxPlatThreadCreate(table.Context,&config,output)==0,"negative thread creation");
            table.CxPlatThreadWait(table.Context,output);
            throw new InvalidOperationException("self-join was not rejected");
        }
        if(args.Length!=0 && args[0]=="bad-return")
        {
            var queue=Allocate<CXPLAT_EVENTQ>();var sqe=Allocate<CXPLAT_SQE>();CXPLAT_CQE item=default;
            Check(table.CxPlatEventQInitialize(table.Context,queue)==1,"negative queue init");
            Check(table.CxPlatSqeInitialize(table.Context,queue,&Completion,sqe)==1,"negative SQE init");
            table.CxPlatEventQEnqueue(table.Context,queue,sqe);table.CxPlatEventQDequeue(table.Context,queue,&item,1,0);
            table.CxPlatEventQReturn(table.Context,queue,2);
            throw new InvalidOperationException("wrong return was not rejected");
        }
        MSQUIC_HOST_TABLE installation=table;
        Check(MsQuic.MsQuicHostInstall(&installation)==0,"actual table install");
        MsQuic.CxPlatSystemLoad();Check(table.CxPlatInitialize(table.Context)==0,"platform initialize");
        Memory(); LocksAndEvents(); Queue(); FailureRollback(host); ThreadsAndWorker();
        CXPLAT_STORAGE* storage=(CXPLAT_STORAGE*)1;
        Check(table.CxPlatStorageOpen(table.Context,null,null,null,0,&storage)==Status.NotSupported&&storage==null,"persistent storage rejects");
        Check(host.OutstandingResources==0&&host.OutstandingPlatformAllocations==0,"all platform ownership drained");
        table.CxPlatUninitialize(table.Context);MsQuic.CxPlatSystemUnload();Check(MsQuic.MsQuicHostUninstall()==0,"table uninstall");
        Console.WriteLine("platform services and translated workers passed");return 0;
    }
}

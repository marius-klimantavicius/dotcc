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
    internal int Leases, Entered, Left, Released, TlsCalls, Count, OnceCalls, Ready;
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

internal static unsafe class Test {
static void Check(bool ok,string message){Owner.Require(ok,message);}
static int Worker(Owner o,void* p){ulong*data=(ulong*)p;data[0]=o.Threads.CThreadCurrent();Check(o.Threads.CTssSet(o.Slots[1],data+1)==0,"key set");for(int i=0;i<2000;i++){Check(o.Threads.CMutexLock(o.Slots)==0,"mutex lock");o.Count++;Check(o.Threads.CMutexUnlock(o.Slots)==0,"unlock");}o.Threads.CCallOnce(o.Slots+3,&Once);if(data[2]!=0)o.Threads.CThreadExit(-91);return 77;}
static void Once(Owner o){Interlocked.Increment(ref o.OnceCalls);}
static void Destructor(Owner o,void*p){int*remaining=(int*)p;Interlocked.Increment(ref o.TlsCalls);if(--*remaining>0)Check(o.Threads.CTssSet(o.Slots[1],p)==0,"destructor reset");}
static int Timed(Owner o,void*p){var deadline=Deadline(25);return o.Threads.CMutexTimedLock(o.Slots+4,&deadline);}
static LibcxxThreads<Owner>.Timespec Deadline(int milliseconds){long ticks=DateTime.UtcNow.AddMilliseconds(milliseconds).Ticks-DateTime.UnixEpoch.Ticks;return new(){Seconds=ticks/10000000,Nanoseconds=ticks%10000000*100};}
static int Static(void*p){return *(int*)p;}
static void ExitDestructor(Owner o,void*p){Interlocked.Increment(ref o.TlsCalls);o.Threads.CThreadExit(53);}
static int ExitDestructorWorker(Owner o,void*p){o.Threads.CTssSet(o.Slots[7],(void*)1);return 1;}
static int Published(Owner o,void*p){return *(ulong*)p==o.Threads.CThreadCurrent()?0:88;}
static int Detached(Owner o,void*p){o.Barrier.Wait();Interlocked.Increment(ref o.Ready);return 14;}
static void Main(){
using var o=new Owner();using var other=new Owner();Check(o.Threads.CThreadCurrent()!=other.Threads.CThreadCurrent(),"owner identities");Check(o.Threads.CMutexInit(o.Slots,0)==0,"init");Check(o.Threads.CTssCreate(o.Slots+1,&Destructor)==0,"key create");
ulong*data=(ulong*)NativeMemory.AllocZeroed(4*3*8);ulong*handles=stackalloc ulong[4];try{
for(int i=0;i<4;i++){data[i*3+1]=6;data[i*3+2]=(ulong)(i%2);Check(o.Threads.CThreadCreate(handles+i,&Worker,data+i*3)==0,"create");}
for(int i=0;i<4;i++){int result=0;Check(other.Threads.CThreadJoin(handles[i],&result)==2,"foreign join rejected");Check(o.Threads.CThreadJoin(handles[i],&result)==0,"join");Check(result==(i%2==0?77:-91),"int exit result");Check(data[i*3]==handles[i],"current identity");Check(data[i*3+1]==2,"four destructor passes");Check(o.Threads.CThreadJoin(handles[i],null)==2,"double join");}
Check(o.Count==8000&&o.TlsCalls==16&&o.OnceCalls==1,"contention/tls/once");
}finally{NativeMemory.Free(data);}
ulong publication=0;for(int i=0;i<100;i++){int result;Check(o.Threads.CThreadCreate(&publication,&Published,&publication)==0&&o.Threads.CThreadJoin(publication,&result)==0&&result==0,"published caller handle");}
ulong destructorThread;int destructorResult;int priorCalls=o.TlsCalls;Check(o.Threads.CTssCreate(o.Slots+7,&ExitDestructor)==0&&o.Threads.CThreadCreate(&destructorThread,&ExitDestructorWorker,null)==0&&o.Threads.CThreadJoin(destructorThread,&destructorResult)==0&&destructorResult==53&&o.TlsCalls==priorCalls+1,"exit from TSS destructor");o.Threads.CTssDelete(o.Slots[7]);
int negative=-123;ulong handle;Check(o.Threads.CThreadCreate(&handle,&Static,&negative)==0,"static callback");int got;Check(o.Threads.CThreadJoin(handle,&got)==0&&got==negative,"static result");
Check(o.Threads.CMutexInit(o.Slots+4,2)==0&&o.Threads.CMutexLock(o.Slots+4)==0,"timed init lock");Check(o.Threads.CThreadCreate(&handle,&Timed,null)==0,"timed worker");Check(o.Threads.CThreadJoin(handle,&got)==0&&got==4,"timed result");Check(o.Threads.CMutexUnlock(o.Slots+4)==0,"timed unlock");var past=Deadline(-1);Check(o.Threads.CMutexTimedLock(o.Slots+4,&past)==0,"available lock past deadline");o.Threads.CMutexUnlock(o.Slots+4);
Check(o.Threads.CMutexInit(o.Slots+5,3)==0&&o.Threads.CMutexLock(o.Slots+5)==0&&o.Threads.CMutexTryLock(o.Slots+5)==0,"recursive depth");o.Threads.CMutexUnlock(o.Slots+5);Check(o.Threads.CConditionInit(o.Slots+6)==0,"cond init");Check(o.Threads.CConditionWait(o.Slots+6,o.Slots+5,&past)==4,"recursive condition timeout/relock");Check(o.Threads.CMutexUnlock(o.Slots+5)==0,"recursive postwait unlock");
var delay=new LibcxxThreads<Owner>.Timespec{Seconds=0,Nanoseconds=1000000};var watch=System.Diagnostics.Stopwatch.StartNew();Check(LibcxxThreads<Owner>.CThreadSleep(&delay,&delay)==0&&watch.ElapsedTicks>=System.Diagnostics.Stopwatch.Frequency/1000,"sleep lower bound alias");
Check(o.Threads.CThreadCreate(&handle,&Detached,null)==0&&o.Threads.CThreadDetach(handle)==0,"detach");o.Barrier.Set();SpinWait.SpinUntil(()=>Volatile.Read(ref o.Ready)==1,5000);
for(int i=0;i<100;i++){o.Threads.CMutexLock(o.Slots);o.Threads.CMutexUnlock(o.Slots);}long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++){o.Threads.CMutexLock(o.Slots);o.Threads.CMutexUnlock(o.Slots);}Check(before==GC.GetAllocatedBytesForCurrentThread(),"steady mutex allocation");
o.Threads.CTssDelete(o.Slots[1]);o.Threads.CConditionDestroy(o.Slots+6);o.Threads.CMutexDestroy(o.Slots);o.Threads.CMutexDestroy(o.Slots+4);o.Threads.CMutexDestroy(o.Slots+5);Console.WriteLine("C thread result/exit, owner isolation, four TSS passes, once, contention, timed/recursive mutex, condition, sleep, detach passed");}}

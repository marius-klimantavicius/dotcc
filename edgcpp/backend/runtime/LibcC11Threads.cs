#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
namespace Edg.Runtime;

// A thread control transfer, never a C++ exception. Generated catch/noexcept
// filters must let it reach the matching retained C thread boundary.
public sealed class CThreadExitTransfer : Exception
{
    internal readonly object Context;
    internal readonly ulong Token;
    public int Result { get; }
    internal CThreadExitTransfer(object context, ulong token, int result)
        : base("Translated C thread exit") { Context=context;Token=token;Result=result; }
}

public sealed unsafe partial class LibcxxThreads<TOwner> where TOwner : class
{
    private readonly ThreadLocal<ThreadEntry?> cCurrent = new();
    private readonly ThreadLocal<ulong> cExternalIdentity = new(ThreadTokenSource.Next);
    private static int CStatus(int error) => error switch { 0=>0,Ebusy=>1,Enomem=>3,Etimedout=>4,_=>2 };
    public int CThreadCreate(ulong* result,delegate* managed<void*,int> entry,void* argument) => CStatus(Create(result,(nint)entry,argument,false,true));
    public int CThreadCreate(ulong* result,delegate* managed<TOwner,void*,int> entry,void* argument) => CStatus(Create(result,(nint)entry,argument,true,true));
    public ulong CThreadCurrent() => cCurrent.Value?.Token ?? cExternalIdentity.Value;
    public int CThreadJoin(ulong token,int* result)
    {
        ThreadEntry entry;
        lock(gate) {
            if(disposed||!threads.TryGetValue(token,out entry!)||!entry.CStart||entry.JoinState!=0)return 2;
            if(entry.Thread.ManagedThreadId==Environment.CurrentManagedThreadId)return 2;
            entry.JoinState=1;
        }
        try {
            if(entry.Initial) {lock(gate)while(!entry.Completed)Monitor.Wait(gate);}
            else entry.Thread.Join();
        }
        catch(ThreadInterruptedException) {lock(gate)entry.JoinState=0;return 2;}
        lock(gate) {threads.Remove(token);if(result!=null)*result=entry.Result;}
        return 0;
    }
    public int CThreadDetach(ulong token) => CStatus(ThreadDetach(&token));
    [DoesNotReturn]
    public void CThreadExit(int result)
    {
        var entry=cCurrent.Value;
        if(entry==null)throw new NotSupportedException("thrd_exit requires a translated C thread or translated process-entry boundary");
        throw new CThreadExitTransfer(this,entry.Token,result);
    }
    // Used only by retained cleanup boundaries, never ordinary C++ handlers.
    public bool AcceptExitResult(CThreadExitTransfer exit)
    {
        var entry=cCurrent.Value;
        if(entry==null||!ReferenceEquals(exit.Context,this)||exit.Token!=entry.Token)return false;
        entry.Result=exit.Result;return true;
    }
    public static int CThreadSleep(Timespec* duration,Timespec* remaining)
    {
        if(duration==null||duration->Seconds<0||duration->Nanoseconds<0||duration->Nanoseconds>=1_000_000_000)return -2;
        Int128 total=(Int128)duration->Seconds*1_000_000_000+duration->Nanoseconds;
        long start=Stopwatch.GetTimestamp();
        try {
            for(;;) {
                Int128 elapsed=(Int128)(Stopwatch.GetTimestamp()-start)*1_000_000_000/Stopwatch.Frequency;
                Int128 left=total-elapsed;if(left<=0)return 0;
                Thread.Sleep((int)Int128.Min(int.MaxValue,(left+999_999)/1_000_000));
            }
        } catch(ThreadInterruptedException) {
            if(remaining!=null) {
                Int128 elapsed=(Int128)(Stopwatch.GetTimestamp()-start)*1_000_000_000/Stopwatch.Frequency;
                Int128 left=Int128.Max(0,total-elapsed);
                remaining->Seconds=(long)(left/1_000_000_000);remaining->Nanoseconds=(long)(left%1_000_000_000);
            }
            // CLR interruption is not a C signal. The signal provider owns -1.
            return -2;
        }
    }
    private MutexEntry? CMutex(ulong* slot)
    {
        if(slot==null||((nuint)slot&7)!=0)return null;
        lock(gate)return !disposed&&resources.TryGetValue(*slot,out var found)?found as MutexEntry:null;
    }
    public int CMutexInit(ulong* slot,int type)
    {
        if(slot==null||((nuint)slot&7)!=0||type<0||type>3)return 2;
        *slot=0;
        try {
            var entry=Mutex(slot,(type&1)!=0);if(entry==null)return 2;
            entry.CTimed=(type&2)!=0;return 0;
        } catch(OutOfMemoryException){return 2;}
    }
    public int CMutexLock(ulong* slot) {var entry=CMutex(slot);return entry==null?2:CStatus(Lock(entry,true));}
    public int CMutexTryLock(ulong* slot) {var entry=CMutex(slot);if(entry==null)return 2;int result=Lock(entry,false);return result==Edeadlk?1:CStatus(result);}
    public int CMutexUnlock(ulong* slot) {var entry=CMutex(slot);return entry==null?2:CStatus(Unlock(entry));}
    public void CMutexDestroy(ulong* slot) {var entry=CMutex(slot);if(entry!=null)MutexDestroy(slot,entry.Kind==Kind.RecursiveMutex);}
    public int CMutexTimedLock(ulong* slot,Timespec* deadline)
    {
        var entry=CMutex(slot);
        if(entry==null||!entry.CTimed||deadline==null||deadline->Nanoseconds<0||deadline->Nanoseconds>=1_000_000_000)return 2;
        for(;;) {
            int result=Lock(entry,false);if(result!=Ebusy&&result!=Edeadlk)return CStatus(result);
            Int128 now=(Int128)(DateTime.UtcNow.Ticks-DateTime.UnixEpoch.Ticks)*100;
            Int128 left=(Int128)deadline->Seconds*1_000_000_000+deadline->Nanoseconds-now;
            if(left<=0)return 4;
            // Monitor.TryEnter reserves the mutex without transient allocation;
            // bounded waits recheck realtime clock adjustments.
            if(!Monitor.TryEnter(entry.Sync,(int)Int128.Min(100,(left+999_999)/1_000_000)))continue;
            try {result=Lock(entry,false);} finally {Monitor.Exit(entry.Sync);}
            if(result!=Ebusy&&result!=Edeadlk)return CStatus(result);
        }
    }
    public int CConditionInit(ulong* slot)
    {
        if(slot==null||((nuint)slot&7)!=0)return 2;
        *slot=0;try{return Get(slot,Kind.Condition,true)==null?2:0;}catch(OutOfMemoryException){return 3;}
    }
    public int CConditionSignal(ulong* slot,bool broadcast) => ConditionSignal(slot,broadcast)==0?0:2;
    public void CConditionDestroy(ulong* slot) => ConditionDestroy(slot);
    public int CConditionWait(ulong* condition,ulong* mutex,Timespec* deadline)
    {var entry=CMutex(mutex);if(entry==null)return 2;int result=ConditionWait(condition,mutex,deadline,entry.Kind==Kind.RecursiveMutex);return result==0?0:result==Etimedout?4:2;}
    public int CTssCreate(ulong* key,delegate* managed<void*,void> destructor) => TlsCreate(key,destructor)==0?0:2;
    public int CTssCreate(ulong* key,delegate* managed<TOwner,void*,void> destructor) => TlsCreate(key,destructor)==0?0:2;
    public int CTssSet(ulong key,void* value) => TlsSet(key,value)==0?0:2;
    public void CTssDelete(ulong key) => TlsDelete(key);
    public void CCallOnce(ulong* flag,delegate* managed<void> function)
    {if(ExecuteOnce(flag,function)!=0)Environment.FailFast("C call_once could not complete");}
    public void CCallOnce(ulong* flag,delegate* managed<TOwner,void> function)
    {if(ExecuteOnce(flag,function)!=0)Environment.FailFast("C call_once could not complete");}
}

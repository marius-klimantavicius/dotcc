using Managed.Networking;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
unsafe class Program
{
    static int checks;
    static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    static void Main()
    {
        var a = new Lwip(); var b = new Lwip();
        long mailbox = 0;
        Check(LwipSysArch.MboxNew(a, &mailbox, 2) == 0, "mailbox create");
        Check(LwipSysArch.MboxTryPost(a, &mailbox, null) == 0, "NULL message is data");
        Check(LwipSysArch.MboxTryPost(a, &mailbox, (void*)42) == 0, "post second");
        Check(LwipSysArch.MboxTryPost(a, &mailbox, (void*)43) == -1, "capacity enforced");
        void* message = (void*)99;
        Check(LwipSysArch.MboxTryFetch(a, &mailbox, &message) == 0 && message == null, "FIFO NULL");
        Check(LwipSysArch.MboxTryFetch(a, &mailbox, &message) == 0 && message == (void*)42, "FIFO second");
        Check(LwipSysArch.MboxTryFetch(a, &mailbox, &message) == uint.MaxValue, "empty status");
        var elapsed = Stopwatch.StartNew();
        Check(LwipSysArch.MboxFetch(a, &mailbox, &message, 25) == uint.MaxValue && elapsed.ElapsedMilliseconds >= 20, "real mailbox timeout");
        long sem = 0; Check(LwipSysArch.SemNew(a, &sem, 0) == 0, "semaphore create");
        long foreign=0;LwipSysArch.SemNew(b,&foreign,0);bool rejected=false;try{LwipSysArch.SemSignal(b,&sem);}catch(InvalidOperationException){rejected=true;}Check(rejected,"foreign handle rejected despite occupied B registry");LwipSysArch.SemFree(b,&foreign);
        var semAddress = (long*)NativeMemory.Alloc((nuint)8); *semAddress = sem;
        var signaler = new Thread(() => { Thread.Sleep(20); LwipSysArch.SemSignal(a, semAddress); }); signaler.Start();
        Check(LwipSysArch.SemWait(a, &sem, 500) != uint.MaxValue, "semaphore wake"); signaler.Join(); NativeMemory.Free(semAddress);
        LwipSysArch.SemSignal(a, &sem); LwipSysArch.SemSignal(a, &sem);
        Check(LwipSysArch.SemWait(a, &sem, 20) != uint.MaxValue && LwipSysArch.SemWait(a, &sem, 20) == uint.MaxValue, "binary semaphore");
        ulong first = LwipSysArch.Protect(a), second = LwipSysArch.Protect(a); LwipSysArch.Unprotect(a, second); LwipSysArch.Unprotect(a, first);
        LwipSysArch.ThreadSemAlloc(a); Check(LwipSysArch.ThreadSemGet(a) != null && LwipSysArch.ThreadSemGet(b) == null, "TLS owner isolation"); LwipSysArch.ThreadSemFree(a);
        byte[]? written = null; var serial = new LwipSysArch.SerialChannel(bytes => written = bytes); LwipSysArch.AttachSerial(a, 0, serial);
        var fd = LwipSysArch.SerialOpen(a, 0); byte payload = 7; LwipSysArch.SerialWrite(a, fd, &payload, 1); Check(written![0] == 7, "serial output copy");
        serial.Feed(new byte[]{1,2}); byte* buffer = stackalloc byte[2]; Check(LwipSysArch.SerialRead(a, fd, buffer, 2) == 2 && buffer[1] == 2, "serial input");
        LwipSysArch.SerialAbort(a, fd); Check(LwipSysArch.SerialRead(a, fd, buffer, 2) == 0, "serial abort");
        LwipApplications.AddContent(a,"/index.html",new byte[]{3,4,5});
        int contentLength=0;fixed(byte* path="/index.html\0"u8)
        {
            byte* content=LwipApplications.HttpOpen(a,path,&contentLength);Check(content!=null&&contentLength==3&&content[1]==4,"HTTP content storage");
            Check(LwipApplications.HttpOpen(b,path,&contentLength)==null,"HTTP owner isolation");LwipApplications.HttpClose(a,content);
        }
        LwipApplications.SetPostHandler(a,(_,_)=>throw new InvalidOperationException("user callback failure"));
        fixed(byte* path="/upload\0"u8)
        {
            Check(LwipApplications.PostBegin(a,(void*)1,path,1)==1,"POST begin");byte one=1;Check(LwipApplications.PostData(a,(void*)1,&one,1)==1,"POST data");
            byte* response=stackalloc byte[64];LwipApplications.PostFinish(a,(void*)1,response,64);Check(response[0]==0,"callback exception contained");
            bool deferred=false;try{LwipApplications.ThrowErrors(a);}catch(AggregateException){deferred=true;}Check(deferred,"callback error exposed outside core");
        }
        LwipApplications.Release(a);
        LwipSysArch.SemFree(a, &sem); LwipSysArch.MboxFree(a, &mailbox);
        LwipSysArch.ThreadNew(a, null, &Worker, null, 0, 0);
        SpinWait.SpinUntil(() => LwipSysArch.Counters(a).Resources == 1, 2000);
        LwipSysArch.Stop(a); Check(LwipSysArch.Counters(a) == (0,0,0), "worker cancel join release");
        Check(LwipSysArch.SemNew(b, &sem, 1) == 0 && LwipSysArch.SemWait(b, &sem, 1) == 0, "B works after A stop"); LwipSysArch.SemFree(b,&sem); LwipSysArch.Stop(b);
        a.Dispose();b.Dispose();
#if STANDALONE_OWNER
        Console.WriteLine($"sys_arch host contract: {checks} checks passed (standalone owner witness)");
#else
        Console.WriteLine($"PASS sysarch: {checks} host contract checks with actual translated owner retention/binding/disposal");
#endif
    }
    static void Worker(Lwip owner, void* argument) { long box; LwipSysArch.MboxNew(owner, &box, 1); LwipSysArch.MboxFetch(owner, &box, null, 0); }
}

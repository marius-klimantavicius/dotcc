using Managed.Networking;
using System.Runtime.InteropServices;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void R(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    sealed class UdpService : IDisposable
    {
        readonly LwipStack stack;readonly int id;readonly List<int> stopped;nint pcb;
        internal UdpService(LwipStack stack,int id,List<int> stopped)
        {this.stack=stack;this.id=id;this.stopped=stopped;stack.Invoke(owner=>{pcb=(nint)owner.udp_new();R(pcb!=0,"facade-owned UDP service allocation");});}
        public void Dispose()
        {if(pcb==0)return;stack.Invoke(owner=>{owner.udp_remove((udp_pcb*)pcb);pcb=0;stopped.Add(id);if(stopped.Count==2)R(owner.Globals.memp_UDP_PCB.stats->used==0,"owned services release protocol pools while core still alive");});}
    }
    sealed class RetryService(LwipStack stack) : IDisposable
    {
        internal int Attempts;
        public void Dispose()
        {
            Attempts++;
            if(Attempts==1)throw new InvalidOperationException("injected service stop failure");
            stack.Invoke(owner=>R(owner.Globals.memp_UDP_PCB.stats->used==0,"service retry core remains available"));
        }
    }
    static void FailedWorker(Lwip owner,void*argument)=>throw new InvalidOperationException("injected worker failure");
    static void DisposalRetry()
    {
        using(var stack=new LwipStack())
        {
            stack.Start();var service=stack.StartService(s=>new RetryService(s));bool failed=false;
            try{stack.Dispose();}catch(InvalidOperationException error)when(error.Message=="injected service stop failure"){failed=true;}
            R(failed&&service.Attempts==1,"failed service retained for retry");stack.Dispose();R(service.Attempts==2,"service cleanup retried before marker");
        }
#if !LWIP_NOSYS
        using(var stack=new LwipStack())
        {
            stack.Start();var owner=stack.Core;long worker;
            using(owner.__DotCcEnter())fixed(byte*name="failure-witness\0"u8)worker=LwipSysArch.ThreadNew(owner,name,&FailedWorker,null,0,0);
            LwipSysArch.JoinWorker(owner,worker);bool failed=false;
            try{stack.Dispose();}catch(AggregateException error){failed=error.InnerExceptions.Any(e=>e.Message=="injected worker failure");}
            R(failed,"post-marker worker error surfaced with owner retained");
            Reject(()=>stack.CleanupForDisposal(_=>{}),"cleanup unavailable after accepted core stop marker");
            R(LwipSysArch.Counters(owner)==(0,0,0),"failed stop completed host reclamation");
            stack.Dispose();R(LwipSysArch.Counters(owner)==(0,0,0),"retry releases owner without redispatch to stopped TCP/IP core");
        }
#endif
        Console.WriteLine("PASS owned service retry and durable post-marker disposal phases");
    }
    static void Reject(Action action,string message)
    {bool rejected=false;try{action();}catch(InvalidOperationException){rejected=true;}R(rejected,message);}
    static void ApplicationCleanupRetry()
    {
        using var stack=new LwipStack();stack.Start();var owner=stack.Core;var file=(fs_file*)NativeMemory.AllocZeroed((nuint)sizeof(fs_file));R(file!=null,"HTTP file state allocate");bool opened=false;
        try
        {
            Reject(()=>stack.CleanupForDisposal(_=>{}),"cleanup unavailable before failed Dispose");
            LwipApplications.AddContent(owner,"/retained.txt","retained"u8);
            stack.Invoke(core=>{fixed(byte*name="/retained.txt\0"u8)R(core.fs_open(file,name)==0,"translated fs_open creates retained host context");opened=true;});
            R(LwipApplications.Counters(owner)==(1,0),"HTTP context retained");
            Reject(stack.Dispose,"active HTTP refuses shutdown before marker");R(LwipSysArch.WorkerStopping(owner)==0,"quiescence refusal preserves running core");
            bool admission=false;try{stack.Invoke(_=>{});}catch(ObjectDisposedException){admission=true;}R(admission,"failed disposal keeps ordinary admission closed");
            Reject(()=>stack.CleanupForDisposal(_=>throw new InvalidOperationException("cleanup witness")),"cleanup failure surfaced");
            admission=false;try{stack.StartService(s=>new UdpService(s,3,new List<int>()));}catch(ObjectDisposedException){admission=true;}R(admission,"cleanup failure does not reopen service admission");
            stack.CleanupForDisposal(core=>
            {
                Reject(stack.Dispose,"recursive Dispose rejected at callback boundary");
                Reject(()=>stack.StartService(s=>new RetryService(s)),"recursive service creation rejected at callback boundary");
                core.fs_close(file);opened=false;R(LwipApplications.Counters(core)==(0,0),"translated fs_close naturally releases retained context");
            });
            stack.Dispose();bool disposed=false;try{stack.CleanupForDisposal(_=>{});}catch(ObjectDisposedException){disposed=true;}R(disposed,"cleanup unavailable after owner disposal");
        }
        finally{if(opened)stack.CleanupForDisposal(core=>{core.fs_close(file);opened=false;});NativeMemory.Free(file);}
        Console.WriteLine("PASS active HTTP context refuses stop, explicit cleanup scope closes translated file, retry releases owner");
    }
    static void Main()
    {
        TimerCallbackReentry();
        DisposalRetry();
        ApplicationCleanupRetry();
#if !LWIP_NOSYS
        PendingService();
#endif
        using var a=new LwipStack();using var b=new LwipStack();a.Start();b.Start();var endpoints=new List<nint>();var packets=new List<nint>();
        try
        {
            a.Invoke(owner=>{for(int i=0;i<=MEMP_NUM_UDP_PCB;i++){var pcb=owner.udp_new();if(pcb==null)break;endpoints.Add((nint)pcb);}R(endpoints.Count==MEMP_NUM_UDP_PCB,"actual A UDP PCB pool exhausted at configured bound");R(owner.udp_new()==null,"exhausted pool stays exhausted");R(owner.Globals.memp_UDP_PCB.stats->used==MEMP_NUM_UDP_PCB,"actual emitted UDP pool allocation counter");});
            b.Invoke(owner=>{var pcb=owner.udp_new();R(pcb!=null,"B UDP allocation succeeds while A exhausted");owner.udp_remove(pcb);});
            a.Invoke(owner=>{foreach(var pcb in endpoints)owner.udp_remove((udp_pcb*)pcb);endpoints.Clear();R(owner.Globals.memp_UDP_PCB.stats->used==0,"A UDP pool naturally recovered");var recovered=owner.udp_new();R(recovered!=null,"A UDP allocation works after recovery");owner.udp_remove(recovered);});
            a.Invoke(owner=>{for(int i=0;i<=PBUF_POOL_SIZE;i++){var packet=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,32,pbuf_type.PBUF_POOL);if(packet==null)break;packets.Add((nint)packet);}R(packets.Count==PBUF_POOL_SIZE,"actual A pbuf pool exhausted at configured bound");R(owner.pbuf_alloc(pbuf_layer.PBUF_RAW,32,pbuf_type.PBUF_POOL)==null,"exhausted pbuf pool stays exhausted");R(owner.Globals.memp_PBUF_POOL.stats->used==PBUF_POOL_SIZE,"actual emitted pbuf pool counter");});
            b.Invoke(owner=>{var packet=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,32,pbuf_type.PBUF_POOL);R(packet!=null,"B pbuf allocation while A exhausted");R(owner.pbuf_free(packet)==1,"B independent pbuf release");});
            a.Invoke(owner=>{foreach(var packet in packets)R(owner.pbuf_free((pbuf*)packet)==1,"exactly-once exhausted pbuf release");packets.Clear();R(owner.Globals.memp_PBUF_POOL.stats->used==0,"A pbuf pool naturally recovered");var recovered=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,32,pbuf_type.PBUF_POOL);R(recovered!=null,"A pbuf allocation after recovery");owner.pbuf_free(recovered);});
            bool contained=false;try{a.Invoke(_=>throw new InvalidOperationException("injected user callback"));}catch(InvalidOperationException error)when(error.Message=="injected user callback"){contained=true;}R(contained,"translated dispatch contains callback failure until caller boundary");a.Invoke(_=>{});b.Invoke(_=>{});
            var stopped=new List<int>();b.StartService(stack=>new UdpService(stack,1,stopped));b.StartService(stack=>new UdpService(stack,2,stopped));b.Dispose();R(stopped.Count==2&&stopped[0]==2&&stopped[1]==1,"facade services stop in reverse order before core release");
            Console.WriteLine($"PASS pool-recovery: actual UDP={MEMP_NUM_UDP_PCB}/pbuf={PBUF_POOL_SIZE} exhaustion, B independent allocation, natural zero-used recovery, contained core callback failure");
        }
        finally{a.Invoke(owner=>{foreach(var pcb in endpoints)owner.udp_remove((udp_pcb*)pcb);foreach(var packet in packets)owner.pbuf_free((pbuf*)packet);});}
    }
}

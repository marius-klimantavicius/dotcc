using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    private sealed class State(Lwip owner,byte value)
    {
        internal readonly Lwip Owner=owner;
        internal readonly byte Value=value;
        internal int Datagrams,Timers;
        internal Exception? Error;
    }
    static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    static State From(void* argument)=>(State)GCHandle.FromIntPtr((nint)argument).Target!;
    static void Receive(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var state=From(argument);
        try{Require(ReferenceEquals(owner,state.Owner),"explicit callback owner");Require(packet->tot_len==32,"datagram size");byte* bytes=stackalloc byte[32];Require(owner.pbuf_copy_partial(packet,bytes,32,0)==32,"datagram copy");for(int i=0;i<32;i++)Require(bytes[i]==state.Value,"per-owner payload");Interlocked.Increment(ref state.Datagrams);}
        catch(Exception error){state.Error=error;}
        finally{owner.pbuf_free(packet);}
    }
    static void Timer(Lwip owner,void* argument)
    {var state=From(argument);if(!ReferenceEquals(owner,state.Owner))state.Error=new InvalidOperationException("timer callback owner");Interlocked.Increment(ref state.Timers);}
    static nint Bind(LwipStack stack,GCHandle handle)
    {
        nint result=0;stack.Invoke(owner=>{ip_addr address;fixed(byte* text="127.0.0.1\0"u8)Require(owner.ipaddr_aton(text,&address)==1,"loopback address");var pcb=owner.udp_new_ip_type((byte)lwip_ip_addr_type.IPADDR_TYPE_V4);Require(pcb!=null,"UDP allocate");result=(nint)pcb;Require(owner.udp_bind(pcb,&address,27000)==0,"same UDP address/port in independent owners");owner.udp_recv(pcb,&Receive,(void*)GCHandle.ToIntPtr(handle));});return result;
    }
    static void Send(LwipStack stack,nint endpoint,byte value)
    {
        stack.Invoke(owner=>{ip_addr address;fixed(byte* text="127.0.0.1\0"u8)Require(owner.ipaddr_aton(text,&address)==1,"destination");var packet=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,32,pbuf_type.PBUF_RAM);Require(packet!=null,"pbuf allocate");byte* bytes=stackalloc byte[32];new Span<byte>(bytes,32).Fill(value);try{Require(owner.pbuf_take(packet,bytes,32)==0,"pbuf fill");Require(owner.udp_sendto((udp_pcb*)endpoint,packet,&address,27000)==0,"UDP send");}finally{owner.pbuf_free(packet);}});
    }
    static void Wait(LwipStack stack,State state,Func<bool> ready)
    {var clock=Stopwatch.StartNew();while(!ready()){if(state.Error!=null)throw state.Error;Require(clock.Elapsed<TimeSpan.FromSeconds(10),"bounded callback progress");stack.Poll();stack.Invoke(_=>{});Thread.Yield();}if(state.Error!=null)throw state.Error;}
#if !LWIP_NOSYS
    static int SocketBind(Lwip owner)
    {
        using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();int socket=owner.lwip_socket(AF_INET,SOCK_DGRAM,0);Require(socket>=0,"socket allocation");sockaddr_in address=new(){sin_len=(byte)sizeof(sockaddr_in),sin_family=AF_INET,sin_port=owner.lwip_htons(27001),sin_addr=new(){s_addr=owner.lwip_htonl(0x7f000001)}};Require(owner.lwip_bind(socket,(sockaddr*)&address,(uint)sizeof(sockaddr_in))==0,"same socket address/port per owner");return socket;
    }
    static void SocketTraffic(Lwip owner,int socket,byte value)
    {
        using var entry=owner.__DotCcEnter();sockaddr_in address=new(){sin_len=(byte)sizeof(sockaddr_in),sin_family=AF_INET,sin_port=owner.lwip_htons(27001),sin_addr=new(){s_addr=owner.lwip_htonl(0x7f000001)}};timeval timeout=new(){tv_sec=5};Require(owner.lwip_setsockopt(socket,SOL_SOCKET,SO_RCVTIMEO,&timeout,(uint)sizeof(timeval))==0,"socket timeout");Require(owner.lwip_sendto(socket,&value,1,0,(sockaddr*)&address,(uint)sizeof(sockaddr_in))==1,"socket self-send");byte actual=0;Require(owner.lwip_recv(socket,&actual,1,0)==1&&actual==value,"socket owner payload");
    }
    static void SocketClose(Lwip owner,int socket){using var entry=owner.__DotCcEnter();Require(owner.lwip_close(socket)==0,"socket close");owner.lwip_socket_thread_cleanup();}
#endif
    readonly record struct Usage(uint Udp,uint Pbuf,uint Timers,uint Heap,uint Netconn,uint Netbuf);
    static Usage Snapshot(LwipStack stack)
    {
        Usage usage=default;stack.Invoke(owner=>{uint netconn=0,netbuf=0;
#if !LWIP_NOSYS
            netconn=owner.Globals.memp_NETCONN.stats->used;netbuf=owner.Globals.memp_NETBUF.stats->used;
#endif
            usage=new Usage(owner.Globals.memp_UDP_PCB.stats->used,owner.Globals.memp_PBUF_POOL.stats->used,owner.Globals.memp_SYS_TIMEOUT.stats->used,owner.Globals.lwip_stats.mem.used,netconn,netbuf);});return usage;
    }
    static void Main()
    {
        using var a=new LwipStack();using var b=new LwipStack();uint clock=uint.MaxValue-5;LwipSysArch.SetClock(b.Core,()=>Volatile.Read(ref clock));a.Start();b.Start();
        var baselineA=Snapshot(a);var baselineB=Snapshot(b);var hostBaselineB=LwipSysArch.Counters(b.Core);
        var stateA=new State(a.Core,0xa1);var stateB=new State(b.Core,0xb2);var handleA=GCHandle.Alloc(stateA);var handleB=GCHandle.Alloc(stateB);nint endpointA=0,endpointB=0;
        IDisposable? timerLease=null;
#if !LWIP_NOSYS
        int socketA=-1,socketB=-1;
#endif
        try
        {
            endpointA=Bind(a,handleA);endpointB=Bind(b,handleB);Require(endpointA!=endpointB,"simultaneously live PCB backing distinct");
#if !LWIP_NOSYS
            socketA=SocketBind(a.Core);socketB=SocketBind(b.Core);Require(socketA==socketB,"descriptor spaces overlap independently");SocketTraffic(a.Core,socketA,0xa1);SocketTraffic(b.Core,socketB,0xb2);
#endif
            Send(a,endpointA,0xa1);Send(b,endpointB,0xb2);Wait(a,stateA,()=>Volatile.Read(ref stateA.Datagrams)==1);Wait(b,stateB,()=>Volatile.Read(ref stateB.Datagrams)==1);
            timerLease=b.Core.__DotCcRetain();b.Invoke(owner=>owner.sys_timeout(10,&Timer,(void*)GCHandle.ToIntPtr(handleB)));
            GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();Require(stateB.Timers==0,"future callback remains pending across GC");
#if !LWIP_NOSYS
            SocketClose(a.Core,socketA);socketA=-1;
#endif
            a.Invoke(owner=>owner.udp_remove((udp_pcb*)endpointA));endpointA=0;Require(Snapshot(a)==baselineA,"A per-operation protocol allocations returned to initialized baseline before dispose");a.Dispose();
            Send(b,endpointB,0xb2);Wait(b,stateB,()=>Volatile.Read(ref stateB.Datagrams)==2);
#if !LWIP_NOSYS
            SocketTraffic(b.Core,socketB,0xb3);
#endif
            Volatile.Write(ref clock,5);Wait(b,stateB,()=>Volatile.Read(ref stateB.Timers)==1);b.Invoke(_=>{});Require(stateB.Timers==1,"timer fires once across uint32 wrap");timerLease.Dispose();timerLease=null;
        }
        finally
        {
            if(endpointA!=0)a.Invoke(owner=>owner.udp_remove((udp_pcb*)endpointA));
            b.Invoke(owner=>{owner.sys_untimeout(&Timer,(void*)GCHandle.ToIntPtr(handleB));if(endpointB!=0)owner.udp_remove((udp_pcb*)endpointB);});
#if !LWIP_NOSYS
            if(socketA>=0)SocketClose(a.Core,socketA);if(socketB>=0)SocketClose(b.Core,socketB);
#endif
            timerLease?.Dispose();handleA.Free();handleB.Free();
        }
        Require(Snapshot(b)==baselineB,"B protocol pools/timers/heap returned to initialized baseline before dispose");Require(LwipSysArch.Counters(b.Core)==hostBaselineB,"B host handles/workers/TLS returned to initialized baseline");
        Console.WriteLine($"PASS owner-lifecycle: overlapping endpoints, explicit owner, GC, A disposed with B live, clock wrap; protocol before={baselineB} after={Snapshot(b)}; host before-clear={hostBaselineB}");
    }
}

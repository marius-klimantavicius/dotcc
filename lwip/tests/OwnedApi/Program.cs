using Managed.Networking;
using System.Net;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void R(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    static T Await<T>(Task<T>task)=>task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
    static void Await(Task task)=>task.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
#if LWIP_HAS_SOCKETS
    static void WaitBlocked(LwipStack stack,LwipSockets.LwipSocket socket)
    {
        using var entry=stack.Core.__DotCcEnter();var wait=System.Diagnostics.Stopwatch.StartNew();while(wait.Elapsed<TimeSpan.FromSeconds(5))
        {
            ulong token=LwipSysArch.Protect(stack.Core);bool blocked;
            try{var native=stack.Core.lwip_socket_dbg_get_socket(socket.Descriptor);blocked=native!=null&&native->conn!=null&&native->conn->mbox_threads_waiting>0;}finally{LwipSysArch.Unprotect(stack.Core,token);}
            if(blocked)return;Thread.Yield();
        }
        throw new TimeoutException("Owning socket did not reach original blocking receive.");
    }
    static void SocketCases()
    {
        using var pair=new VirtualPair();var socketsA=LwipSockets.Start(pair.A);var socketsB=LwipSockets.Start(pair.B);
        foreach(bool ipv6 in new[]{false,true})
        {
            IPAddress ipA=IPAddress.Parse(ipv6?"fd00::1":"10.0.0.1"),ipB=IPAddress.Parse(ipv6?"fd00::2":"10.0.0.2");ushort port=(ushort)(ipv6?29801:29800);
            using var udpA=socketsA.CreateUdp(ipv6);using var udpB=socketsB.CreateUdp(ipv6);udpA.Bind(new(ipA,port));udpB.Bind(new(ipB,port));R(udpA.LocalAddress.Port==udpB.LocalAddress.Port,"independent owners bind same UDP port");
            var read=udpB.ReceiveFromAsync();WaitBlocked(pair.B,udpB);bool secondRead=false;try{Await(udpB.ReceiveFromAsync());}catch(InvalidOperationException){secondRead=true;}R(secondRead,"same-direction socket concurrency rejected before original call");byte[]payload="copied datagram"u8.ToArray();var sent=udpA.SendToAsync(payload,new(ipB,port));Array.Fill(payload,(byte)0);R(Await(sent)==15,"copied datagram send length");var datagram=Await(read);R(datagram.Bytes.AsSpan().SequenceEqual("copied datagram"u8)&&datagram.Sender.Address.Equals(ipA)&&datagram.Sender.Port==port,"copied datagram contents and address DTO");
            using var listener=socketsB.CreateTcp(ipv6);listener.Bind(new(ipB,(ushort)(port+10)));listener.Listen();var accept=listener.AcceptAsync();using var client=socketsA.CreateTcp(ipv6);Await(client.ConnectAsync(new(ipB,(ushort)(port+10))));using var peer=Await(accept);R(Await(client.SendAsync("owned TCP"u8.ToArray()))==9,"TCP copied send");R(Await(peer.ReceiveAsync()).AsSpan().SequenceEqual("owned TCP"u8),"TCP copied receive");client.ShutdownSend();R(Await(peer.ReceiveAsync()).Length==0,"owning TCP half-close EOF");
        }
        // Deterministically pause an admitted operation before its original
        // syscall; close cannot permit the descriptor to identify another socket.
        using(var old=socketsA.CreateUdp())
        using(var admitted=new ManualResetEventSlim())
        using(var enterSystemCall=new ManualResetEventSlim())
        {
            int oldDescriptor=old.Descriptor;
            var paused=Task.Run(()=>socketsA.Call(old,(core,fd)=>{admitted.Set();R(enterSystemCall.Wait(TimeSpan.FromSeconds(10)),"release admitted syscall");byte value;long result=core.lwip_recv(fd,&value,1,MSG_DONTWAIT);return (result,Libc.errno);}));
            R(admitted.Wait(TimeSpan.FromSeconds(10)),"operation pinned before close");var close=Task.Run(old.Dispose);
            R(SpinWait.SpinUntil(()=>Volatile.Read(ref old.Closed),TimeSpan.FromSeconds(10)),"original close finished while admitted operation waits");
            using var other=socketsA.CreateUdp();R(other.Descriptor!=oldDescriptor,"admitted original pin prevents descriptor reuse");enterSystemCall.Set();var result=Await(paused);R(result.result==-1&&result.errno==9,"late old syscall rejects closed descriptor without touching new socket");Await(close);
        }
        NetconnCases(pair);
        ResolverCases(pair);
        using(var canceled=socketsA.CreateUdp())
        {
            canceled.Bind(new(IPAddress.Parse("10.0.0.1"),29840));using var cancel=new CancellationTokenSource();var pending=canceled.ReceiveAsync(cancellation:cancel.Token);WaitBlocked(pair.A,canceled);cancel.Cancel();bool observed=false;try{Await(pending);}catch(OperationCanceledException){observed=true;}R(observed,"cancellation closes socket and wakes original blocked receive");
        }
        var survivor=socketsB.CreateUdp();survivor.Bind(new(IPAddress.Parse("10.0.0.2"),29850));var blocked=socketsA.CreateUdp();blocked.Bind(new(IPAddress.Parse("10.0.0.1"),29851));var waiting=blocked.ReceiveAsync();WaitBlocked(pair.A,blocked);socketsA.Dispose();bool closed=false;try{Await(waiting);}catch(LwipSocketException error){closed=error.Error==107;}R(closed,"socket service closes pending caller before core lifetime ends");
        pair.A.Invoke(core=>R(core.Globals.memp_NETCONN.stats->used==0&&core.Globals.memp_UDP_PCB.stats->used==0,"owning socket natural pool cleanup before owner release"));
        var localRead=survivor.ReceiveFromAsync();R(Await(survivor.SendToAsync("B alive"u8.ToArray(),new(IPAddress.Parse("10.0.0.2"),29850)))==7,"B existing socket send after A service disposal");R(Await(localRead).Bytes.AsSpan().SequenceEqual("B alive"u8),"B existing socket survives A service disposal");survivor.Dispose();
        // Scope is copied and remains stable when the caller mutates its IPAddress object.
        var scoped=new IPAddress(IPAddress.Parse("fe80::1").GetAddressBytes(),3);var dto=new LwipEndpoint(scoped,42);scoped.ScopeId=9;R(dto.Zone==3&&dto.Address.ScopeId==3,"copied numeric IPv6 zone");
        Console.WriteLine("PASS owned-api: copied TCP/UDP owning handles, async blocking/core progress, endpoint/zone DTOs, cancellation/close waiter accounting, owner isolation");
    }
#endif
    static void Main()
    {
#if LWIP_HAS_SOCKETS
        R(NO_SYS==0,"selected product metadata exposes sockets only for threaded profiles");
        SocketCases();
        AdoptionCases();
        ScheduledAdmissionCases();
#else
        R(NO_SYS!=0,"selected actual product supplies NO_SYS capability without a profile hint");
        using(var pair=new VirtualPair())ResolverCases(pair);
#endif
        bool zoneRejected=false;
        try{new LwipEndpoint(new IPAddress(IPAddress.Parse("fe80::1").GetAddressBytes(),256),1);}catch(ArgumentOutOfRangeException){zoneRejected=true;}
        R(zoneRejected,"socket/netconn endpoint zone cannot truncate to an lwIP index");
        InterfaceAdmissionCases();
        InterfaceRollbackCases();
        ResolverShutdownCases();
        Console.WriteLine("PASS owned-api: actual product capability selection and post-core-join callback root lifetime");
    }
    static void InterfaceAdmissionCases()
    {
        using var stack=new LwipStack();stack.Start();nint before=0;uint used=0;
        stack.Invoke(core=>{before=(nint)core.Globals.netif_list;used=core.Globals.lwip_stats.mem.used;});
        foreach(var create in new Action[]{
            ()=>stack.AddInterface("10.0.0.1","",new byte[]{2,0,0,0,0,1},null!),
            ()=>stack.AddInterface("10.0.0.1\0ignored","",new byte[]{2,0,0,0,0,1},_=>{}),
            ()=>stack.AddInterface("10.0.0.1","fd00::1%0",new byte[]{2,0,0,0,0,1},_=>{}),
            ()=>stack.AddInterface("10.0.0.1","",new byte[]{2,0,0,0,0,1},_=>{},gateway:"::1")})
        {
            bool rejected=false;try{create();}catch(ArgumentException){rejected=true;}R(rejected,"invalid interface API argument rejected before allocation");
            stack.Invoke(core=>R((nint)core.Globals.netif_list==before&&core.Globals.lwip_stats.mem.used==used,"failed interface admission preserves netif list and heap baseline"));
        }
    }
}

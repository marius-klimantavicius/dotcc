#if LWIP_HAS_SOCKETS
using Managed.Networking;
using System.Net;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void WaitNetconn(LwipStack stack,LwipNetconns.Connection connection)
    {
        using var entry=stack.Core.__DotCcEnter();var watch=System.Diagnostics.Stopwatch.StartNew();
        while(watch.Elapsed<TimeSpan.FromSeconds(5))
        {
            ulong token=LwipSysArch.Protect(stack.Core);int waiting;try{waiting=((netconn*)connection.Pointer)->mbox_threads_waiting;}finally{LwipSysArch.Unprotect(stack.Core,token);}
            if(waiting>0)return;Thread.Yield();
        }
        throw new TimeoutException("Owning netconn did not reach original blocked receive.");
    }
    static void NetconnCases(VirtualPair pair)
    {
        using var a=LwipNetconns.Start(pair.A);using var b=LwipNetconns.Start(pair.B);
        foreach(bool ipv6 in new[]{false,true})
        {
            IPAddress addressA=IPAddress.Parse(ipv6?"fd00::1":"10.0.0.1"),addressB=IPAddress.Parse(ipv6?"fd00::2":"10.0.0.2");ushort port=(ushort)(ipv6?29911:29910);
            using var udpA=a.CreateUdp(ipv6);using var udpB=b.CreateUdp(ipv6);udpA.Bind(new(addressA,port));udpB.Bind(new(addressB,port));var pending=udpB.ReceiveAsync();WaitNetconn(pair.B,udpB);bool secondRead=false;try{Await(udpB.ReceiveAsync());}catch(InvalidOperationException){secondRead=true;}R(secondRead,"same-direction netconn concurrency rejected before original call");R(Await(udpA.SendToAsync("netconn copy"u8.ToArray(),new(addressB,port)))==12,"owning netconn copied UDP send");var data=Await(pending);R(data!=null&&data.Bytes.AsSpan().SequenceEqual("netconn copy"u8)&&data.Sender!.Address.Equals(addressA),"owning netconn copy before netbuf release");
            using var listener=b.CreateTcp(ipv6);listener.Bind(new(addressB,(ushort)(port+10)));listener.Listen();var accept=listener.AcceptAsync();using var client=a.CreateTcp(ipv6);Await(client.ConnectAsync(new(addressB,(ushort)(port+10))));using var peer=Await(accept);R(Await(client.SendAsync("netconn TCP"u8.ToArray()))==11,"owning netconn TCP send");data=Await(peer.ReceiveAsync());R(data!=null&&data.Sender==null&&data.Bytes.AsSpan().SequenceEqual("netconn TCP"u8),"owning TCP buffer copied without invented datagram sender");
        }
        var blocked=a.CreateUdp();blocked.Bind(new(IPAddress.Parse("10.0.0.1"),29950));var receive=blocked.ReceiveAsync();WaitNetconn(pair.A,blocked);a.Dispose();bool closed=false;try{Await(receive);}catch(LwipNetconnException error){closed=error.Error==-11;}R(closed,"owning netconn prepare wakes actual caller with native ERR_CONN before delete");
        using(var cancelTarget=b.CreateUdp())
        {
            cancelTarget.Bind(new(IPAddress.Parse("10.0.0.2"),29951));using var token=new CancellationTokenSource();var read=cancelTarget.ReceiveAsync(token.Token);WaitNetconn(pair.B,cancelTarget);token.Cancel();bool canceled=false;try{Await(read);}catch(OperationCanceledException){canceled=true;}R(canceled,"direct netconn cancellation prepares shutdown without freeing live caller memory");
        }
        Console.WriteLine("PASS owning-netconn TCP/UDP copied buffers and accepted children, prepare/wake/join/delete, native ERR_CONN and cancellation");
    }
}
#endif

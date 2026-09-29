#if LWIP_HAS_SOCKETS
using System.Net;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void AdoptionCases()
    {
        using var pair=new VirtualPair();
        var socketsA=LwipSockets.Start(pair.A);var socketsB=LwipSockets.Start(pair.B);
        using(var listener=socketsB.CreateTcp())
        using(var client=socketsA.CreateTcp())
        using(var accepted=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        {
            listener.Bind(new(IPAddress.Parse("10.0.0.2"),29970));listener.Listen();
            var pending=Task.Run(()=>socketsB.Call(listener,(core,descriptor)=>
            {
                int child=core.lwip_accept(descriptor,null,null);R(child>=0,"original socket accept completes");accepted.Set();
                R(release.Wait(TimeSpan.FromSeconds(10)),"release socket adoption");return socketsB.AdoptAccepted(listener,child);
            },direction:1));
            Await(client.ConnectAsync(new(IPAddress.Parse("10.0.0.2"),29970)));
            R(accepted.Wait(TimeSpan.FromSeconds(10)),"socket child exists before registry adoption");
            var stop=Task.Run(socketsB.Dispose);
            R(SpinWait.SpinUntil(()=>Volatile.Read(ref listener.Closing),TimeSpan.FromSeconds(10)),"socket service stopping before adoption");release.Set();
            bool rejected=false;try{Await(pending);}catch(ObjectDisposedException){rejected=true;}
            Await(stop);R(rejected,"late accepted socket is closed through registry during shutdown");
            pair.B.Invoke(core=>R(core.Globals.memp_NETCONN.stats->used==0,"late accepted socket natural pool cleanup"));
        }
        var netconnsA=LwipNetconns.Start(pair.A);var netconnsB=LwipNetconns.Start(pair.B);
        using(var listener=netconnsB.CreateTcp())
        using(var client=netconnsA.CreateTcp())
        using(var accepted=new ManualResetEventSlim())
        using(var release=new ManualResetEventSlim())
        {
            listener.Bind(new(IPAddress.Parse("10.0.0.2"),29971));listener.Listen();
            var pending=Task.Run(()=>netconnsB.Call(listener,(core,pointer)=>
            {
                netconn*child=null;R(core.netconn_accept((netconn*)pointer,&child)==0&&child!=null,"original netconn accept completes");accepted.Set();
                R(release.Wait(TimeSpan.FromSeconds(10)),"release netconn adoption");return netconnsB.AdoptAccepted(listener,child);
            },direction:1));
            Await(client.ConnectAsync(new(IPAddress.Parse("10.0.0.2"),29971)));
            R(accepted.Wait(TimeSpan.FromSeconds(10)),"netconn child exists before registry adoption");
            var stop=Task.Run(netconnsB.Dispose);
            R(SpinWait.SpinUntil(()=>Volatile.Read(ref listener.Closing),TimeSpan.FromSeconds(10)),"netconn service stopping before adoption");release.Set();
            bool rejected=false;try{Await(pending);}catch(ObjectDisposedException){rejected=true;}
            Await(stop);R(rejected,"late accepted netconn is retained then prepared/deleted during shutdown");
            pair.B.Invoke(core=>R(core.Globals.memp_NETCONN.stats->used==0,"late accepted netconn natural pool cleanup"));
        }
        Console.WriteLine("PASS owning-accept: real accepted socket/netconn children raced with service shutdown, rejected adoption and natural pool recovery");
    }
}
#endif

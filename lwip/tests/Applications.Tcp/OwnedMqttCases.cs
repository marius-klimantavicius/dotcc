using System.Net;
using System.Runtime.CompilerServices;
using Managed.Networking;
using static Fixture;
using static MqttCases;

internal static unsafe class OwnedMqttCases
{
    static void Complete(Fixture fixture,Task task){fixture.Wait(()=>task.IsCompleted);task.GetAwaiter().GetResult();}
    static void Connect(Fixture fixture,TcpPeer peer,LwipMqttClient client,ushort port,string address="10.0.0.2",byte status=0)
    {
        var task=client.ConnectAsync(new LwipEndpoint(IPAddress.Parse(address),port),new("owned-client"){KeepAliveSeconds=0});
        var request=Frame(fixture,peer);Require(request[0]==0x10&&Body(request).AsSpan().SequenceEqual(new byte[]{0,4,77,81,84,84,4,2,0,0,0,12}.Concat("owned-client"u8.ToArray()).ToArray()),"owning client submits original clean CONNECT");
        peer.Send(fixture,new byte[]{0x20,2,0,status});Complete(fixture,task);
        Require((int)task.Result==status&&client.IsConnected==(status==0),"owning ordinary CONNACK result");
    }
    static void Subscribe(Fixture fixture,TcpPeer peer,LwipMqttClient client)
    {
        var task=client.SubscribeAsync("owned/in",1);var frame=Frame(fixture,peer);var body=Body(frame);
        Require(frame[0]==0x82&&body.AsSpan(2).SequenceEqual(new byte[]{0,8}.Concat("owned/in"u8.ToArray()).Append((byte)1).ToArray()),"owning subscription original encoding");
        peer.Send(fixture,new byte[]{0x90,3,body[0],body[1],1});Complete(fixture,task);
        Require(client.PendingRequests==0,"owning request releases admission after original callback");
    }
    static void Incoming(Fixture fixture,TcpPeer peer,ushort id,ReadOnlySpan<byte> payload)
    {
        var body=new byte[]{0,8}.Concat("owned/in"u8.ToArray()).Concat(new byte[]{(byte)(id>>8),(byte)id}).Concat(payload.ToArray()).ToArray();
        peer.Send(fixture,Packet(0x32,body));Require(Frame(fixture,peer).AsSpan().SequenceEqual(new byte[]{0x40,2,(byte)(id>>8),(byte)id}),"original inbound acknowledgement even when notification queue is full");
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference DisposedQueuedClient(Fixture fixture)
    {
        using var peer=new TcpPeer(fixture.Pair.B,1895);
        var service=LwipMqttClient.Create(fixture.Pair.A);
        Connect(fixture,peer,service,1895);
        Incoming(fixture,peer,81,"queued copied payload"u8);
        Require(service.ReceivedPublications==1&&service.DroppedPublications==0,"unread copied publication remains queued before disposal");
        var weak=new WeakReference(service);service.Dispose();fixture.Wait(()=>peer.Finished,peer.Check);
        return weak;
    }
    internal static void Run(Fixture fixture)
    {
        var disposedQueued=DisposedQueuedClient(fixture);
        GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
        Require(!disposedQueued.IsAlive,"live stack no longer roots disposed MQTT wrapper/channel with queued copied payload");
        // No transport allocated: natural original heap baseline isolates MQTT
        // allocation/root construction-disposal from TCP TIME_WAIT retention.
        uint before=0,after=0;fixture.Pair.A.Invoke(owner=>before=owner.Globals.lwip_stats.mem.used);
        for(int i=0;i<8;i++){using var idle=LwipMqttClient.Create(fixture.Pair.A);}
        fixture.Pair.A.Invoke(owner=>after=owner.Globals.lwip_stats.mem.used);Require(after==before,"idle owning MQTT allocation returns original heap to baseline");
        using var client=LwipMqttClient.Create(fixture.Pair.A,new(){MaximumPendingRequests=1,PublicationQueueCapacity=1,MaximumPublicationBytes=32,MaximumOutgoingPayloadBytes=32});
        using(var peer=new TcpPeer(fixture.Pair.B,1890))
        {
            Connect(fixture,peer,client,1890);Subscribe(fixture,peer,client);
            Incoming(fixture,peer,71,"copied"u8);Incoming(fixture,peer,72,"discarded"u8);
            Require(client.ReceivedPublications==2&&client.DroppedPublications==1,"bounded queue records accepted and dropped ordinary publications");
            Require(client.TryReadPublication(out var publication)&&publication!.Topic=="owned/in"&&publication.Payload.Span.SequenceEqual("copied"u8),"owning notification is a copied topic and payload");
            Incoming(fixture,peer,73,ReadOnlySpan<byte>.Empty);Require(client.TryReadPublication(out publication)&&publication!.Payload.IsEmpty&&!client.TryReadPublication(out _),"empty original publication delivered exactly once");
            Incoming(fixture,peer,74,new byte[33]);Require(client.DroppedPublications==2,"coherent publication above consumer capacity is discarded and acknowledged");
            Incoming(fixture,peer,75,"recovered"u8);Require(client.TryReadPublication(out publication)&&publication!.Payload.Span.SequenceEqual("recovered"u8),"notification capacity recovers");
            var pending=client.SubscribeAsync("owned/in",1);Require(Frame(fixture,peer)[0]==0x82,"pending owned request reached broker");
            bool rejected=false;try{client.SubscribeAsync("another",1);}catch(InvalidOperationException){rejected=true;}
            Require(rejected&&client.PendingRequests==1&&!pending.IsCompleted,"ordinary pending admission bound preserves accepted request");
            client.CancelPending();Require(pending.IsCanceled&&client.PendingRequests==0&&!client.IsConnected,"explicit original disconnect settles suppressed pending task");
            fixture.Wait(()=>peer.Finished,peer.Check);
        }
        using(var peer=new TcpPeer(fixture.Pair.B,1891))
        {
            Connect(fixture,peer,client,1891);Subscribe(fixture,peer,client);
            byte[] payload="copy before return"u8.ToArray();var task=client.PublishAsync("owned/out",payload,1);payload.AsSpan().Fill(0);
            var frame=Frame(fixture,peer);var body=Body(frame);Require(frame[0]==0x32&&body.AsSpan(13).SequenceEqual("copy before return"u8),"published input copied before returning to caller");
            peer.Send(fixture,new byte[]{0x40,2,body[11],body[12]});Complete(fixture,task);
            bool rejected=false;try{client.PublishAsync("owned/out",new byte[33],1);}catch(ArgumentOutOfRangeException){rejected=true;}
            Require(rejected&&client.IsConnected&&client.PendingRequests==0,"ordinary payload admission failure leaves client usable");
            Subscribe(fixture,peer,client);
            // Another original client may share this owner; it has distinct roots.
            using(var second=LwipMqttClient.Create(fixture.Pair.A))
            using(var other=new TcpPeer(fixture.Pair.B,1892))
            {
                Connect(fixture,other,second,1892);var secondPending=second.SubscribeAsync("owned/in",1);Frame(fixture,other);
                second.Dispose();Require(secondPending.IsCanceled,"service disposal settles pending original request");fixture.Wait(()=>other.Finished,other.Check);
                Subscribe(fixture,peer,client);
            }
            var pendingRead=client.ReadPublicationAsync().AsTask();var pending=client.SubscribeAsync("owned/in",1);Frame(fixture,peer);
            peer.FinishSend();fixture.Wait(()=>pending.IsCompleted);Require(pending.IsFaulted&&pending.Exception!.InnerException is LwipMqttDisconnectedException,"ordinary broker FIN faults pending task after original cleanup");fixture.Wait(()=>peer.Finished,peer.Check);
            client.Dispose();fixture.Wait(()=>pendingRead.IsCompleted);Require(pendingRead.IsFaulted,"service disposal completes pending channel read");
        }
        using(var refused=LwipMqttClient.Create(fixture.Pair.A))
        using(var peer=new TcpPeer(fixture.Pair.B,1893))
        {Connect(fixture,peer,refused,1893,status:3);fixture.Wait(()=>peer.Finished,peer.Check);refused.ThrowCallbackErrors();}
        // A separate owner with a reachable interface but no peer remains in
        // original TCP connect; disposing it cancels its Task without callbacks.
        using(var victim=new LwipStack())
        {
            victim.Start();victim.AddInterface("10.9.0.1","",new byte[]{2,9,0,0,0,1},_=>{});
            var service=LwipMqttClient.Create(victim);var task=service.ConnectAsync(new(IPAddress.Parse("10.9.0.2"),1883),new("victim"));
            Require(!task.IsCompleted,"ordinary unresponded connect pending");victim.Dispose();Require(task.IsCanceled,"owner disposal cancels original connect and frees client");
        }
        GC.Collect();GC.WaitForPendingFinalizers();
        using(var survivor=LwipMqttClient.Create(fixture.Pair.B))
        using(var peer=new TcpPeer(fixture.Pair.A,1894))
        {Connect(fixture,peer,survivor,1894,"10.0.0.1");Subscribe(fixture,peer,survivor);survivor.ThrowCallbackErrors();survivor.Disconnect();fixture.Wait(()=>peer.Finished,peer.Check);}
    }
}

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Managed.Transport;
using Managed.Transport.Hosting;

internal static unsafe class Program
{
    private static MSQUIC_HOST_TABLE table;
    private static readonly ConcurrentQueue<nint> packets = new();
    private static readonly SemaphoreSlim ready = new(0);
    private static readonly SemaphoreSlim unreachable = new(0);
    private static int callbacks;
    private static bool deleteInsideReceive;
    private static void Check(bool condition, string message) { if (!condition) { Console.Error.WriteLine("Datapath assertion: " + message); throw new InvalidOperationException(message); } }
    private static void Receive(CXPLAT_SOCKET* socket, void* context, CXPLAT_RECV_DATA* packet)
    {
        if(deleteInsideReceive) table.CxPlatSocketDelete(table.Context,socket);
        Check(context == (void*)0x9876, "copied callback context");
        Check(packet->Next == null && packet->Allocated == 1 && packet->DatapathType == 1, "real receive descriptor");
        Check(packet->PartitionIndex == 0 && packet->TypeOfService == 0 && packet->HopLimitTTL == 0, "nonadvertised metadata absent");
        Check(packet->Route->LocalAddress.Ipv6.sin6_scope_id > 0, "real receive interface metadata");
        var client = new Span<byte>(packet + 1, 37);
        Check(client.IndexOfAnyExcept((byte)0) < 0, "zeroed client receive context on pool reuse");
        client[36] = 0xa5;
        Interlocked.Increment(ref callbacks);
        packets.Enqueue((nint)packet); ready.Release();
    }
    private static void Unreachable(CXPLAT_SOCKET* socket, void* context, QUIC_ADDR* remote)
    { Check(remote != null && MsQuicHost.DatagramEndpoint(remote).Port != 0, "unreachable peer endpoint"); unreachable.Release(); }
    private static CXPLAT_RECV_DATA* WaitPacket()
    {
        Check(ready.Wait(5000), "receive completion timeout");
        Check(packets.TryDequeue(out var pointer), "receive queue"); return (CXPLAT_RECV_DATA*)pointer;
    }
    private static CXPLAT_SOCKET* Create(CXPLAT_DATAPATH* datapath, IPAddress address, IPEndPoint? remote = null, CXPLAT_SOCKET_FLAGS flags = CXPLAT_SOCKET_FLAGS.CXPLAT_SOCKET_FLAG_NONE, uint interfaceIndex = 0)
    {
        QUIC_ADDR local, peer;
        MsQuicHost.DatagramAddress(new IPEndPoint(address,0), &local);
        CXPLAT_UDP_CONFIG config = default; config.LocalAddress=&local; config.CallbackContext=(void*)0x9876;config.Flags=flags;config.InterfaceIndex=interfaceIndex;
        if(remote!=null) { MsQuicHost.DatagramAddress(remote,&peer);config.RemoteAddress=&peer; }
        CXPLAT_SOCKET* socket=null;
        uint result=table.CxPlatSocketCreateUdp(table.Context,datapath,&config,&socket);
        Check(result==0&&socket!=null,"UDP create: "+result);return socket;
    }
    private static void Send(CXPLAT_SOCKET* socket,CXPLAT_ROUTE* route,ReadOnlySpan<byte> data)
    {
        CXPLAT_SEND_CONFIG config=default; config.Route=route;config.MaxPacketSize=1472;
        var send=table.CxPlatSendDataAlloc(table.Context,socket,&config);
        Check(send!=null,"send allocation");
        Check(table.CxPlatSendDataIsFull(table.Context,send)==0,"empty send context");
        var buffer=table.CxPlatSendDataAllocBuffer(table.Context,send,(ushort)Math.Max(1,data.Length));
        Check(buffer!=null,"send buffer");data.CopyTo(new Span<byte>(buffer->Buffer,data.Length));buffer->Length=(uint)data.Length;
        Check(table.CxPlatSendDataIsFull(table.Context,send)==1,"no unadvertised segmentation");
        Check(table.CxPlatSendDataAllocBuffer(table.Context,send,1)==null,"single-datagram capacity");
        GC.Collect(2,GCCollectionMode.Forced,true,true);
        Check(new ReadOnlySpan<byte>(buffer->Buffer,data.Length).SequenceEqual(data),"send pointer retained across GC");
        table.CxPlatSocketSend(table.Context,socket,route,send);
    }
    private static void Echo(MsQuicHost host,CXPLAT_SOCKET* socket,IPAddress destination,int length)
    {
        QUIC_ADDR local;table.CxPlatSocketGetLocalAddress(table.Context,socket,&local);
        int port=MsQuicHost.DatagramEndpoint(&local).Port;
        using var peer=new Socket(destination.AddressFamily,SocketType.Dgram,ProtocolType.Udp);
        peer.ReceiveTimeout=5000;
        var bind=destination.AddressFamily==AddressFamily.InterNetwork?IPAddress.Loopback:destination;
        peer.Bind(new IPEndPoint(bind,0));
        var payload=new byte[length];for(int i=0;i<length;i++)payload[i]=(byte)(i*37+11);
        peer.SendTo(payload,new IPEndPoint(destination,port));
        var packet=WaitPacket();
        Check(packet->BufferLength==length&&new ReadOnlySpan<byte>(packet->Buffer,length).SequenceEqual(payload),"datagram payload and exact boundary");
        var localRoute=MsQuicHost.DatagramEndpoint(&packet->Route->LocalAddress);
        var remoteRoute=MsQuicHost.DatagramEndpoint(&packet->Route->RemoteAddress);
        Check(localRoute.Address.Equals(destination)&&localRoute.Port==port,"wildcard exact destination");
        Check(remoteRoute.Port==((IPEndPoint)peer.LocalEndPoint!).Port,"source port/NAT rebinding");
        GC.Collect(2,GCCollectionMode.Forced,true,true);
        Check(((byte*)(packet+1))[36]==0xa5&&new ReadOnlySpan<byte>(packet->Buffer,length).SequenceEqual(payload),"receive lease survives forced GC");
        Send(socket,packet->Route,payload);
        byte[] response=new byte[Math.Max(1,length)];EndPoint from=new IPEndPoint(bind,0);
        int count=peer.ReceiveFrom(response,ref from);
        Check(count==length&&response.AsSpan(0,count).SequenceEqual(payload),"reply datagram");
        var sender=(IPEndPoint)from;
        Check(sender.Port==port&&sender.Address.Equals(destination),"reply exact source address and listener port");
        table.CxPlatRecvDataReturn(table.Context,packet);
    }
    private static void Truncation(MsQuicHost host,CXPLAT_SOCKET* socket)
    {
        QUIC_ADDR local;table.CxPlatSocketGetLocalAddress(table.Context,socket,&local);int port=MsQuicHost.DatagramEndpoint(&local).Port;
        using var peer=new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
        long before=host.DatagramTruncations;peer.SendTo(new byte[4000],new IPEndPoint(IPAddress.Loopback,port));
        Check(SpinWait.SpinUntil(()=>host.DatagramTruncations>before,5000),"truncated receive explicitly dropped");
        Check(!ready.Wait(50),"truncated payload never upcalled");Echo(host,socket,IPAddress.Loopback,1200);
    }
    private static void UnreachablePeer(CXPLAT_DATAPATH* path)
    {
        int port;using(var reserve=new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp)) {reserve.Bind(new IPEndPoint(IPAddress.Loopback,0));port=((IPEndPoint)reserve.LocalEndPoint!).Port;}
        var remote=new IPEndPoint(IPAddress.Loopback,port);var socket=Create(path,IPAddress.Loopback,remote);
        CXPLAT_ROUTE route=default;MsQuicHost.DatagramAddress(remote,&route.RemoteAddress);table.CxPlatSocketGetLocalAddress(table.Context,socket,&route.LocalAddress);
        Send(socket,&route,new byte[]{1});Check(unreachable.Wait(5000),"connected UDP unreachable callback");table.CxPlatSocketDelete(table.Context,socket);
    }
    private static void AddressQueries(CXPLAT_DATAPATH* path)
    {
        QUIC_ADDR address;var expected=new IPEndPoint(IPAddress.Parse("127.0.0.2"),43210);MsQuicHost.DatagramAddress(expected,&address);
        table.CxPlatConvertToMappedV6(table.Context,&address,&address);Check(address.Ip.sa_family==10,"mapped IPv6 native family");
        table.CxPlatConvertFromMappedV6(table.Context,&address,&address);Check(address.Ip.sa_family==2&&MsQuicHost.DatagramEndpoint(&address).Equals(expected),"in-place mapped conversion preserves address/port");
        QUIC_ADDR local;Check(table.CxPlatDataPathGetLocalAddressForRemote(table.Context,&address,&local)==0,"BCL local route selection");
        Check(MsQuicHost.DatagramEndpoint(&local).Address.Equals(IPAddress.Loopback)&&MsQuicHost.DatagramEndpoint(&local).Port==0,"route selection does not leak ephemeral lookup port");
        byte* name=stackalloc byte[]{(byte)'l',(byte)'o',(byte)'c',(byte)'a',(byte)'l',(byte)'h',(byte)'o',(byte)'s',(byte)'t',0};
        Check(table.CxPlatDataPathResolveAddress(table.Context,path,name,&address)==0,"DNS resolution");
        Check(address.Ip.sa_family==2&&MsQuicHost.DatagramEndpoint(&address).Port==43210,"DNS family and port constraint");
        CXPLAT_ADAPTER_ADDRESS* addresses=null;uint count=0;
        Check(table.CxPlatDataPathGetLocalAddresses(table.Context,path,&addresses,&count)==0&&count>0,"BCL interface address enumeration");
        bool found=false;for(uint i=0;i<count;i++)if(MsQuicHost.DatagramEndpoint(&addresses[i].Address).Address.Equals(IPAddress.Loopback)){found=true;Check(addresses[i].InterfaceIndex>0&&addresses[i].InterfaceType==24,"actual loopback interface metadata");}
        Check(found,"loopback address enumerated");table.CxPlatFree(table.Context,addresses,unchecked((uint)MsQuic.QUIC_POOL_DATAPATH_ADDRESSES));
    }
    private static void RejectCapabilities(CXPLAT_DATAPATH* path,CXPLAT_SOCKET* socket)
    {
        Check(table.CxPlatDataPathGetSupportedFeatures(table.Context,path,0)==0,"no unsupported features advertised");
        Check(table.CxPlatSocketGetQtipEnabled(table.Context,socket)==0,"QTIP disabled");
        Check(table.CxPlatSocketGetLocalMtu(table.Context,socket,null)==1500,"qualified maximum MTU");
        Check(table.CxPlatSocketUpdateQeo(table.Context,socket,null,0)==95,"QEO explicit rejection");
        CXPLAT_RSS_CONFIG* rss=(CXPLAT_RSS_CONFIG*)1;Check(table.CxPlatDataPathRssConfigGet(table.Context,0,&rss)==95&&rss==null,"RSS explicit rejection");
        CXPLAT_SEND_CONFIG send=default;send.MaxPacketSize=1200;send.ECN=1;Check(table.CxPlatSendDataAlloc(table.Context,socket,&send)==null,"unsupported ECN send rejected");
        CXPLAT_UDP_CONFIG config=default;config.Flags=CXPLAT_SOCKET_FLAGS.CXPLAT_SOCKET_FLAG_XDP;CXPLAT_SOCKET* bad=(CXPLAT_SOCKET*)1;
        Check(table.CxPlatSocketCreateUdp(table.Context,path,&config,&bad)==95&&bad==null,"raw socket request rejected");
    }
    private static void DrainCycles(MsQuicHost host,CXPLAT_DATAPATH* path)
    {
        for(int i=0;i<32;i++)
        {
            var socket=Create(path,IPAddress.Any);
            Echo(host,socket,IPAddress.Loopback,i%2==0?1:1200);
            table.CxPlatSocketDelete(table.Context,socket);
            int count=Volatile.Read(ref callbacks);GC.Collect(2,GCCollectionMode.Forced,true,true);Thread.Sleep(1);
            Check(count==Volatile.Read(ref callbacks),"no upcalls after socket delete");
        }
    }
    private static void ListenerSocketFlags(MsQuicHost host,CXPLAT_DATAPATH* path)
    {
        // Exact unchanged src/core/listener.c combination, including SHARE.
        const CXPLAT_SOCKET_FLAGS flags=CXPLAT_SOCKET_FLAGS.CXPLAT_SOCKET_FLAG_SHARE|CXPLAT_SOCKET_FLAGS.CXPLAT_SOCKET_SERVER_OWNED;
        foreach(var local in new[]{IPAddress.Any,IPAddress.IPv6Any})
        {
            var socket=Create(path,local,flags:flags);
            Echo(host,socket,IPAddress.Loopback,1200);
            if(local.AddressFamily==AddressFamily.InterNetworkV6)Echo(host,socket,IPAddress.IPv6Loopback,1200);
            table.CxPlatSocketDelete(table.Context,socket);
        }
    }
    private static void SelectedInterfaces(MsQuicHost host,CXPLAT_DATAPATH* path,IPAddress scoped)
    {
        uint loopback=0;
        foreach(var adapter in NetworkInterface.GetAllNetworkInterfaces())
            if(adapter.NetworkInterfaceType==NetworkInterfaceType.Loopback)loopback=checked((uint)adapter.GetIPProperties().GetIPv4Properties()!.Index);
        Check(loopback!=0,"real loopback interface index");
        foreach(var local in new[]{IPAddress.Any,IPAddress.IPv6Any})
        {
            var socket=Create(path,local,interfaceIndex:loopback);
            Check(host.DatagramInterfacesMatch((nint)socket,loopback),"requested interface applied to initial physical socket");
            Echo(host,socket,IPAddress.Loopback,1200);
            if(local.AddressFamily==AddressFamily.InterNetworkV6)Echo(host,socket,IPAddress.IPv6Loopback,1200);
            Check(host.DatagramInterfacesMatch((nint)socket,loopback),"source-specific sockets retain requested interface");
            table.CxPlatSocketDelete(table.Context,socket);
        }
        var scopedSocket=Create(path,IPAddress.IPv6Any,interfaceIndex:checked((uint)scoped.ScopeId));
        Echo(host,scopedSocket,scoped,1200);
        Check(host.DatagramInterfacesMatch((nint)scopedSocket,checked((uint)scoped.ScopeId)),"link-local source socket retains interface");
        table.CxPlatSocketDelete(table.Context,scopedSocket);
        foreach(var address in new[]{IPAddress.Any,IPAddress.IPv6Any})
        {
            QUIC_ADDR local;MsQuicHost.DatagramAddress(new IPEndPoint(address,0),&local);
            CXPLAT_UDP_CONFIG config=default;config.LocalAddress=&local;config.InterfaceIndex=uint.MaxValue;
            CXPLAT_SOCKET* absent=(CXPLAT_SOCKET*)1;int before=host.OutstandingResources;
            Check(table.CxPlatSocketCreateUdp(table.Context,path,&config,&absent)==99&&absent==null,"nonexistent interface returns address-not-available");
            Check(host.OutstandingResources==before,"failed interface selection retains no token");
        }
    }
    private static void DeferredSendDrain(MsQuicHost host,CXPLAT_DATAPATH* path)
    {
        using var peer=new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);peer.Bind(new IPEndPoint(IPAddress.Loopback,0));peer.ReceiveTimeout=5000;
        var socket=Create(path,IPAddress.Loopback);
        CXPLAT_ROUTE route=default;MsQuicHost.DatagramAddress((IPEndPoint)peer.LocalEndPoint!,&route.RemoteAddress);table.CxPlatSocketGetLocalAddress(table.Context,socket,&route.LocalAddress);
        int resources=host.OutstandingResources;
        DatagramAsync.HoldNext();Send(socket,&route,new byte[]{29,37,43});
        Check(DatagramAsync.Sent.Wait(5000),"real BCL send completed before deferred callback gate");
        Check(peer.Receive(new byte[16])==3,"real pending-path datagram delivered");
        GC.Collect(2,GCCollectionMode.Forced,true,true);
        Check(host.OutstandingResources==resources+1,"deferred send retains opaque owner and pinned buffer");
        using var closing=new ManualResetEventSlim();using var closed=new ManualResetEventSlim();nint address=(nint)socket;
        var thread=new Thread(()=>{closing.Set();table.CxPlatSocketDelete(table.Context,(CXPLAT_SOCKET*)address);closed.Set();});thread.Start();closing.Wait();
        Check(!closed.Wait(50),"socket close waits for pending send completion");
        DatagramAsync.Release.SetResult();
        Check(closed.Wait(5000),"send completion releases close drain");thread.Join();
    }
    private static readonly ManualResetEventSlim blockerEntered = new();
    private static readonly ManualResetEventSlim unblock = new();
    private static readonly ManualResetEventSlim socketClosed = new();
    private static nint closeTarget;
    private static MsQuicHost? closeHost;
    private static CXPLAT_EVENTQ* closeQueue;
    private static void BlockWorker(CXPLAT_CQE* completion) { blockerEntered.Set(); Check(unblock.Wait(5000), "release worker blocker"); }
    private static void CloseFromWorker(CXPLAT_CQE* completion)
    { Check(closeHost!.CurrentBatchHasDatagram(closeQueue,closeTarget),"socket notification is in this same worker batch"); table.CxPlatSocketDelete(table.Context, (CXPLAT_SOCKET*)closeTarget); socketClosed.Set(); }
    private static void CloseWithSameBatchNotification(MsQuicHost host,CXPLAT_DATAPATH* path,CXPLAT_WORKER_POOL* workers)
    {
        var socket=Create(path,IPAddress.Any);closeTarget=(nint)socket;
        var queue=MsQuic.CxPlatWorkerPoolGetEventQ(workers,0);closeHost=host;closeQueue=queue;
        var controls=(CXPLAT_SQE*)table.CxPlatAlloc(table.Context,(ulong)(sizeof(CXPLAT_SQE)*2),0x55647054);
        Check(controls!=null,"control SQEs");
        Check(table.CxPlatSqeInitialize(table.Context,queue,&BlockWorker,controls)==1,"blocker registration");
        Check(table.CxPlatSqeInitialize(table.Context,queue,&CloseFromWorker,controls+1)==1,"closer registration");
        Check(table.CxPlatEventQEnqueue(table.Context,queue,controls)==1,"blocker enqueue");
        Check(blockerEntered.Wait(5000),"worker blocked before next batch");
        Check(table.CxPlatEventQEnqueue(table.Context,queue,controls+1)==1,"closer first in next batch");
        QUIC_ADDR local;table.CxPlatSocketGetLocalAddress(table.Context,socket,&local);
        using var peer=new Socket(AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
        peer.SendTo(new byte[]{17},new IPEndPoint(IPAddress.Loopback,MsQuicHost.DatagramEndpoint(&local).Port));
        Check(SpinWait.SpinUntil(()=>host.HasQueuedDatagram(closeQueue,closeTarget),5000),"packet queued after close control");
        int before=Volatile.Read(ref callbacks);unblock.Set();
        Check(socketClosed.Wait(5000),"socket close returned on its worker");
        table.CxPlatSqeCleanup(table.Context,queue,controls);table.CxPlatSqeCleanup(table.Context,queue,controls+1);
        table.CxPlatFree(table.Context,controls,0x55647054);
        Check(host.OutstandingDatagramReceives==0&&Volatile.Read(ref callbacks)==before,"later batch packet notification suppressed without freeing its SQE early");
    }
    private static int Main(string[] args)
    {
        using var host=new MsQuicHost();table=host.CreateDatapathTable();
        var installation=table;Check(MsQuic.MsQuicHostInstall(&installation)==0,"host install");MsQuic.CxPlatSystemLoad();Check(table.CxPlatInitialize(table.Context)==0,"PAL init");
        var workers=MsQuic.CxPlatWorkerPoolCreate(null,CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_LIBRARY);Check(workers!=null,"translated worker pool");
        CXPLAT_UDP_DATAPATH_CALLBACKS callbacks=new(){Receive=&Receive,Unreachable=&Unreachable};CXPLAT_DATAPATH* path=null;
        Check(table.CxPlatDataPathInitialize(table.Context,37,&callbacks,null,workers,null,&path)==0,"typed datapath init");
        var ipv4=Create(path,IPAddress.Any);
        if(args.Length!=0&&args[0]=="callback-delete") { deleteInsideReceive=true;Echo(host,ipv4,IPAddress.Loopback,1);throw new InvalidOperationException("Callback-local deletion unexpectedly returned."); }
        AddressQueries(path);RejectCapabilities(path,ipv4);ListenerSocketFlags(host,path);
        foreach(int length in new[]{0,1,1200,1472})Echo(host,ipv4,IPAddress.Loopback,length);
        Echo(host,ipv4,IPAddress.Parse("127.0.0.2"),1200);Echo(host,ipv4,IPAddress.Loopback,1200);Truncation(host,ipv4);
        table.CxPlatSocketDelete(table.Context,ipv4);
        var dual=Create(path,IPAddress.IPv6Any);Echo(host,dual,IPAddress.IPv6Loopback,1200);Echo(host,dual,IPAddress.Loopback,1200);table.CxPlatSocketDelete(table.Context,dual);
        var scoped=FindScopedAddress();Check(scoped!=null,"configured IPv6 link-local address is required for scope qualification");
        var scopeSocket=Create(path,IPAddress.IPv6Any);Echo(host,scopeSocket,scoped!,1200);table.CxPlatSocketDelete(table.Context,scopeSocket);
        SelectedInterfaces(host,path,scoped!);
        UnreachablePeer(path);DrainCycles(host,path);CloseWithSameBatchNotification(host,path,workers);DeferredSendDrain(host,path);
        Check(host.OutstandingDatagramReceives==0,"all receive descriptors returned");
        table.CxPlatDataPathUninitialize(table.Context,path);
        MsQuic.CxPlatWorkerPoolDelete(workers,CXPLAT_WORKER_POOL_REF.CXPLAT_WORKER_POOL_REF_LIBRARY);
        Check(host.OutstandingResources==0&&host.OutstandingPlatformAllocations==0,"all resources/buffers/threads drained");
        table.CxPlatUninitialize(table.Context);MsQuic.CxPlatSystemUnload();Check(MsQuic.MsQuicHostUninstall()==0,"host uninstall");
        Console.WriteLine("datapath typed buffers, dual-stack source selection, scopes, truncation, unreachable and drain passed; AOT="+(!RuntimeFeature.IsDynamicCodeSupported));return 0;
    }
    private static IPAddress? FindScopedAddress()
    {
        foreach(var adapter in NetworkInterface.GetAllNetworkInterfaces())foreach(var item in adapter.GetIPProperties().UnicastAddresses)
            if(item.Address.AddressFamily==AddressFamily.InterNetworkV6&&item.Address.IsIPv6LinkLocal)return item.Address;
        return null;
    }
}

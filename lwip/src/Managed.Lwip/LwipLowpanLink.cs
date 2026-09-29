using System.Net;
using System.Net.Sockets;
using static Managed.Networking.Lwip;
namespace Managed.Networking;

/// <summary>Owns an original IEEE802.15.4, RFC7668 BLE or ZEP interface. Radio PAN/short address/contexts and BLE peer addresses/contexts are shared by all matching links in one stack, as in upstream lwIP.</summary>
public sealed unsafe class LwipLowpanLink:IDisposable
{
    enum Kind:uint {Radio,Zep,BorrowedRadioTimer,Ble}
    readonly LwipStack stack;readonly Kind kind;
    nint handle,interfaceAddress;bool closing,stopped;volatile bool disposed;
    LwipLowpanLink(LwipStack stack,Kind kind){this.stack=stack;this.kind=kind;}
    static void Check(int result,string operation){if(result!=0)throw new IOException($"lwIP LOWPAN {operation} failed ({result}).");}
    static byte[] Hardware(ReadOnlySpan<byte> address)
    {if(address.Length!=6)throw new ArgumentException("This product's link hardware address contains six bytes.",nameof(address));return address.ToArray();}
    static LwipEndpoint Address(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if(address.AddressFamily!=AddressFamily.InterNetworkV6||address.ScopeId!=0)throw new ArgumentException("A zone-free IPv6 interface address is required.",nameof(address));
        return new LwipEndpoint(address,0);
    }
    public static LwipLowpanLink StartRadio(LwipStack stack,IPAddress address,ReadOnlySpan<byte> hardware,Action<byte[]> output)
    {
        ArgumentNullException.ThrowIfNull(stack);ArgumentNullException.ThrowIfNull(output);var ip=Address(address);byte[]mac=Hardware(hardware);
        return stack.StartInitializedService(s=>new LwipLowpanLink(s,Kind.Radio),link=>link.Initialize(ip,mac,output,null,null,0));
    }
    public static LwipLowpanLink StartBle(LwipStack stack,IPAddress address,ReadOnlySpan<byte> hardware,Action<byte[]> output)
    {
        ArgumentNullException.ThrowIfNull(stack);ArgumentNullException.ThrowIfNull(output);var ip=Address(address);byte[]mac=Hardware(hardware);
        return stack.StartInitializedService(s=>new LwipLowpanLink(s,Kind.Ble),link=>link.Initialize(ip,mac,output,null,null,0));
    }
    /// <summary>Owns the ZEP UDP PCB and radio interface. Optional transportInterface is a borrowed Ethernet handle from this same stack; stack-owned Ethernet interfaces outlive registered services. Port zero selects upstream defaults.</summary>
    public static LwipLowpanLink StartZep(LwipStack stack,IPAddress address,ReadOnlySpan<byte> hardware,LwipEndpoint source,LwipEndpoint destination,nint transportInterface=0)
    {
        ArgumentNullException.ThrowIfNull(stack);ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(destination);
        if(source.Address.AddressFamily!=destination.Address.AddressFamily)throw new ArgumentException("ZEP source and destination address families must match.");
        var ip=Address(address);byte[]mac=Hardware(hardware);
        return stack.StartInitializedService(s=>new LwipLowpanLink(s,Kind.Zep),link=>link.Initialize(ip,mac,null,source,destination,transportInterface));
    }
    /// <summary>Keep the shared radio timer/reassembly lifetime while using borrowed original radio APIs. Raw users must close their interfaces and callbacks before disposing this lease.</summary>
    public static LwipLowpanLink RetainBorrowedRadioTimer(LwipStack stack)
    {
        ArgumentNullException.ThrowIfNull(stack);
        return stack.StartInitializedService(s=>new LwipLowpanLink(s,Kind.BorrowedRadioTimer),link=>link.Initialize(null,Array.Empty<byte>(),null,null,null,0));
    }
    void Initialize(LwipEndpoint? address,byte[]hardware,Action<byte[]>? output,LwipEndpoint? source,LwipEndpoint? destination,nint transport)
    {
        stack.RunServiceBoundary(()=>
        {
            stack.RequireOwnedInterface(transport);
            stack.Invoke(core=>
            {
                Lwip.__DotCcTags.zepif_init configuration=default;ip_addr sourceAddress=default,destinationAddress=default;
                if(kind==Kind.Zep)
                {
                    source!.WriteIp(&sourceAddress);destination!.WriteIp(&destinationAddress);
                    configuration.zep_src_udp_port=source.Port;configuration.zep_dst_udp_port=destination.Port;
                    configuration.zep_src_ip_addr=&sourceAddress;configuration.zep_dst_ip_addr=&destinationAddress;configuration.zep_netif=(netif*)transport;
                    for(int i=0;i<6;i++)configuration.addr[i]=hardware[i];
                }
                handle=(nint)core.lwip_owned_lowpan_new((uint)kind,kind==Kind.Zep?&configuration:null);
                if(handle==0)throw new InvalidOperationException("LOWPAN service allocation or original interface initialization failed.");
                var net=core.lwip_owned_lowpan_netif((void*)handle);interfaceAddress=(nint)net;
                if(kind==Kind.BorrowedRadioTimer)return;
                if(net==null)throw new InvalidOperationException("LOWPAN service has no interface.");
                if(kind!=Kind.Zep)
                {
                    net->hwaddr_len=6;for(int i=0;i<6;i++)net->hwaddr[i]=hardware[i];
                    LwipFrames.Attach(core,interfaceAddress,output!);net->linkoutput=&Output;
                }
                ip_addr ip;address!.WriteIp(&ip);core.netif_ip6_addr_set(net,0,&ip.u_addr.ip6);core.netif_ip6_addr_set_state(net,0,IP6_ADDR_PREFERRED);
                core.netif_set_up(net);core.netif_set_link_up(net);
            });
        });
    }
    static sbyte Output(Lwip owner,netif*net,pbuf*packet)
    {
        try
        {
            byte[]bytes=new byte[packet->tot_len];fixed(byte*p=bytes)
            {
                if(owner.pbuf_copy_partial(packet,p,packet->tot_len,0)!=packet->tot_len)return -12;
                return checked((sbyte)LwipFrames.Send(owner,net,p,(uint)bytes.Length));
            }
        }
        catch(OutOfMemoryException){return -1;}
    }
    void WithInterface(Action<Lwip,nint> action)=>stack.RunServiceBoundary(()=>
    {
        ObjectDisposedException.ThrowIf(closing||disposed,this);
        if(interfaceAddress==0)throw new InvalidOperationException("A borrowed timer lease has no interface.");
        stack.Invoke(core=>action(core,interfaceAddress));
    });
    /// <summary>Original netif_get_index value, relative to this stack. Use as an IPv6 zone only with endpoints owned by the same stack.</summary>
    public byte Index {get{byte value=0;WithInterface((_,address)=>value=checked((byte)(((netif*)address)->num+1)));return value;}}
    /// <summary>The original interface MTU, including IPv6 headers. Radio link frames may be smaller and fragmented by the original adaptation algorithm.</summary>
    public ushort Mtu {get{ushort value=0;WithInterface((_,address)=>value=((netif*)address)->mtu);return value;}}
    public bool IsLinkUp {get{bool value=false;WithInterface((_,address)=>value=(((netif*)address)->flags&NETIF_FLAG_LINK_UP)!=0);return value;}}
    public void SetLinkState(bool up)=>WithInterface((core,address)=>{if(up)core.netif_set_link_up((netif*)address);else core.netif_set_link_down((netif*)address);});
    public void SetDefaultRoute()=>WithInterface((core,address)=>core.netif_set_default((netif*)address));
    /// <summary>Copy a complete decoded radio frame (FCS already removed) or complete BLE L2CAP payload into the original input algorithm. ZEP receives through its original UDP PCB instead.</summary>
    public void Receive(ReadOnlyMemory<byte> frame)
    {
        if(kind!=Kind.Radio&&kind!=Kind.Ble)throw new InvalidOperationException("Direct input is available only on radio or BLE links.");
        if(frame.Length>ushort.MaxValue)throw new ArgumentOutOfRangeException(nameof(frame));byte[]copy=frame.ToArray();
        stack.RunServiceBoundary(()=>
        {
            ObjectDisposedException.ThrowIf(closing||disposed,this);
            stack.Invoke(core=>
            {
                pbuf*packet=core.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)copy.Length,pbuf_type.PBUF_RAM);if(packet==null)throw new IOException("LOWPAN input allocation failed.");
                fixed(byte*p=copy){int result=core.pbuf_take(packet,p,(ushort)copy.Length);if(result!=0){core.pbuf_free(packet);Check(result,"input copy");}}
                // Original direct input takes ownership, including discarded frames.
                Check(kind==Kind.Ble?core.rfc7668_input(packet,(netif*)interfaceAddress):core.lowpan6_input(packet,(netif*)interfaceAddress),"input");
            });
        });
    }
    /// <summary>Configure the owner-wide IEEE802.15.4 PAN and optional short address used by every radio/ZEP interface in this stack.</summary>
    public static void ConfigureRadio(LwipStack stack,ushort pan,ushort? shortAddress=null)=>stack.Invoke(core=>
    {Check(core.lowpan6_set_pan_id(pan),"PAN");if(shortAddress.HasValue)Check(core.lowpan6_set_short_addr((byte)(shortAddress.Value>>8),(byte)shortAddress.Value),"short address");});
    /// <summary>Configure the owner-wide RFC7668 local/peer EUI64 pair. Every BLE interface in this stack uses the same pair.</summary>
    public static void ConfigureBlePeer(LwipStack stack,ReadOnlySpan<byte> local,ReadOnlySpan<byte> peer)
    {
        if(local.Length!=8||peer.Length!=8)throw new ArgumentException("BLE EUI64 addresses contain eight bytes.");byte[]a=local.ToArray(),b=peer.ToArray();
        stack.Invoke(core=>{fixed(byte*x=a,y=b){Check(core.rfc7668_set_local_addr_eui64(null,x,8),"BLE local address");Check(core.rfc7668_set_peer_addr_eui64(null,y,8),"BLE peer address");}});
    }
    /// <summary>Set an owner-wide IPv6 compression context for either radio/ZEP or BLE algorithms.</summary>
    public static void SetContext(LwipStack stack,bool ble,byte index,IPAddress prefix)
    {
        var address=Address(prefix);stack.Invoke(core=>{ip_addr ip;address.WriteIp(&ip);Check(ble?core.rfc7668_set_context(index,&ip.u_addr.ip6):core.lowpan6_set_context(index,&ip.u_addr.ip6),"compression context");});
    }
    public void Dispose()
    {
        if(disposed)return;
        try{stack.RunServiceBoundary(()=>
        {
            if(disposed)return;closing=true;
            if(handle!=0)
            {
                if(!stopped)
                {
                    stack.ServiceBarrier();
                    stack.Invoke(core=>{if(interfaceAddress!=0&&kind!=Kind.Zep)LwipFrames.Detach(core,interfaceAddress);Check(core.lwip_owned_lowpan_stop((void*)handle),"stop");});
                    stopped=true;
                }
                stack.ServiceBarrier();
                stack.Invoke(core=>Check(core.lwip_owned_lowpan_free((void*)handle),"free after FIFO"));handle=0;interfaceAddress=0;
            }
            disposed=true;stack.CompleteServiceDisposal(this);
        });}catch(ObjectDisposedException) when(disposed){}
    }
}

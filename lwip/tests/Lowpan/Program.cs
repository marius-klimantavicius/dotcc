using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    sealed class State { public required Lwip Owner; public int Received; public Exception? Failure; }
    static void R(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    static void Put16(byte[] bytes,int at,int value){bytes[at]=(byte)(value>>8);bytes[at+1]=(byte)value;}
    static uint Sum(ReadOnlySpan<byte> bytes,uint sum=0){for(int i=0;i<bytes.Length;i+=2)sum+=(uint)((bytes[i]<<8)|(i+1<bytes.Length?bytes[i+1]:0));while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return sum;}
    static sbyte Input(Lwip owner,pbuf* packet,netif* iface)=>owner.ip_input(packet,iface);
    static sbyte Radio(Lwip owner,netif* iface)=>owner.lowpan6_if_init(iface);
    static sbyte Ble(Lwip owner,netif* iface)=>owner.rfc7668_if_init(iface);
    static void Receive(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var s=(State)GCHandle.FromIntPtr((nint)argument).Target!;
        try{R(ReferenceEquals(owner,s.Owner)&&address->type==6&&port==9001&&packet->tot_len==17,"lowpan callback owner/address");byte* bytes=stackalloc byte[17];R(owner.pbuf_copy_partial(packet,bytes,17,0)==17,"complete UDP payload");for(int i=0;i<17;i++)R(bytes[i]==(byte)(i*7),"independent UDP bytes");s.Received++;}
        catch(Exception error){s.Failure=error;}finally{owner.pbuf_free(packet);}
    }
    static byte[] Frame(bool ble,bool compressed,int mode)
    {
        byte[] ip=new byte[65];ip[0]=0x60;Put16(ip,4,25);ip[6]=17;ip[7]=64;ip[8]=ip[24]=0xfd;ip[23]=2;ip[39]=1;Put16(ip,40,9001);Put16(ip,42,9000);Put16(ip,44,25);for(int i=0;i<17;i++)ip[48+i]=(byte)(i*7);Put16(ip,46,(int)~Sum(ip.AsSpan(40),Sum(ip.AsSpan(8,32),42)));
        byte[] frame=new byte[128];int at=0;
        if(!ble){bool shortSource=(mode&1)!=0,extendedDestination=(mode&2)!=0;frame[0]=0x41;frame[1]=(byte)((shortSource?0x80:0xc0)|(extendedDestination?0x0c:0x08));frame[2]=7;frame[3]=0x34;frame[4]=0x12;at=5;frame[at]=1;at+=extendedDestination?8:2;frame[at]=2;if(!shortSource)frame[at+7]=2;at+=shortSource?2:8;}
        if(compressed){frame[at++]=0x7a;frame[at++]=0;frame[at++]=17;ip.AsSpan(8,32).CopyTo(frame.AsSpan(at));at+=32;ip.AsSpan(40).CopyTo(frame.AsSpan(at));at+=25;}
        else{frame[at++]=0x41;ip.CopyTo(frame,at);at+=ip.Length;}
        return frame[..at];
    }
    static void Inject(Lwip owner,netif* iface,bool ble,byte[] bytes)
    {var packet=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)bytes.Length,pbuf_type.PBUF_RAM);R(packet!=null,"lowpan input allocation");fixed(byte* data=bytes)R(owner.pbuf_take(packet,data,(ushort)bytes.Length)==0,"lowpan input copy");R((ble?owner.rfc7668_input(packet,iface):owner.lowpan6_input(packet,iface))==0,"lowpan consumes input");}
    static void Case(Lwip owner,bool ble)
    {
        uint memory=owner.Globals.lwip_stats.mem.used,pool=owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used;
        netif iface=default;ip4_addr zero=default;R(owner.netif_add(&iface,&zero,&zero,&zero,null,ble?&Ble:&Radio,&Input)==&iface,"lowpan netif");ip6_addr address;fixed(byte* text="fd00::1\0"u8)R(owner.ip6addr_aton(text,&address)==1,"lowpan IPv6 address");owner.netif_ip6_addr_set(&iface,0,&address);owner.netif_ip6_addr_set_state(&iface,0,0x30);owner.netif_set_up(&iface);owner.netif_set_link_up(&iface);
        var state=new State{Owner=owner};var root=GCHandle.Alloc(state);ip_addr any=default;any.type=6;var udp=owner.udp_new_ip_type(6);R(udp!=null&&owner.udp_bind(udp,&any,9000)==0,"lowpan UDP listener");owner.udp_recv(udp,&Receive,(void*)GCHandle.ToIntPtr(root));
        try
        {
            if(ble){byte* mac=stackalloc byte[]{2,0x11,0x22,0x33,0x44,0x55};byte* eui=stackalloc byte[8];byte* back=stackalloc byte[6];owner.ble_addr_to_eui64(eui,mac,1);R(new ReadOnlySpan<byte>(eui,8).SequenceEqual(new byte[]{0,0x11,0x22,0xff,0xfe,0x33,0x44,0x55}),"BLE public EUI64 vector");owner.eui64_to_ble_addr(back,eui);R(back[0]==0&&new ReadOnlySpan<byte>(back+1,5).SequenceEqual(new ReadOnlySpan<byte>(mac+1,5)),"BLE reverse vector");R(owner.rfc7668_set_local_addr_mac48(&iface,mac,6,1)==0&&owner.rfc7668_set_peer_addr_eui64(&iface,eui,8)==0&&owner.rfc7668_set_local_addr_mac48(&iface,mac,5,1)==-6,"BLE address validation");}
            else{R(owner.lowpan6_set_pan_id(0x1234)==0,"PAN ID");fixed(byte* check="123456789"u8)R(owner.lowpan6_calc_crc(check,9)==0x2189,"802.15.4 CRC vector");}
            int count=0;for(int mode=0;mode<(ble?1:4);mode++)for(int compressed=0;compressed<2;compressed++){Inject(owner,&iface,ble,Frame(ble,compressed!=0,mode));if(state.Failure!=null)throw state.Failure;R(state.Received==++count,"raw/IPHC address combination delivered");}
        }
        finally{owner.udp_remove(udp);owner.netif_set_down(&iface);owner.netif_remove(&iface);root.Free();}
        R(owner.Globals.lwip_stats.mem.used==memory&&owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used==pool,"lowpan resources naturally restored");
    }
    static void Run(LwipStack stack){stack.Invoke(owner=>{Case(owner,false);Case(owner,true);});}
    static void Main(){var a=new LwipStack();using var b=new LwipStack();try{a.Start();b.Start();Run(a);Run(b);}finally{a.Dispose();}Run(b);Console.WriteLine("PASS lowpan-vectors: 30 real UDP deliveries, raw/IPHC IEEE address combinations, BLE EUI64 and CRC vectors, two owners and natural baselines");}
}

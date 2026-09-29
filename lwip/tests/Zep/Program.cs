using System.Buffers.Binary;
using System.Net;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    const byte IpV6=6; // upstream lwip_ip_addr_type.IPADDR_TYPE_V6
    static void R(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    static void Put(Span<byte> bytes,int offset,int value)=>BinaryPrimitives.WriteUInt16BigEndian(bytes[offset..],(ushort)value);
    static uint Sum(ReadOnlySpan<byte>bytes,uint total=0){for(int i=0;i<bytes.Length;i+=2)total+=(uint)((bytes[i]<<8)|(i+1<bytes.Length?bytes[i+1]:0));while(total>65535)total=(total&65535)+(total>>16);return total;}
    static ushort Crc(ReadOnlySpan<byte>bytes){uint value=0;foreach(byte b in bytes){value^=b;for(int i=0;i<8;i++)value=(value>>1)^((value&1)!=0?0x8408u:0);}return (ushort)value;}
    static byte[] Ip()
    {
        byte[]ip=new byte[65];ip[0]=0x60;Put(ip,4,25);ip[6]=17;ip[7]=64;ip[8]=ip[24]=0xfd;ip[23]=2;ip[39]=1;Put(ip,40,9001);Put(ip,42,9000);Put(ip,44,25);for(int i=0;i<17;i++)ip[48+i]=(byte)(i*7);Put(ip,46,(int)~Sum(ip.AsSpan(40),Sum(ip.AsSpan(8,32),42)));return ip;
    }
    static byte[] Radio(bool compressed=false)
    {
        byte[]ip=Ip();byte[]frame=new byte[compressed?75:81];frame[0]=0x41;frame[1]=0xc8;frame[2]=7;frame[3]=0x34;frame[4]=0x12;frame[5]=1;frame[7]=2;frame[14]=2;
        if(compressed){frame[15]=0x7a;frame[16]=0;frame[17]=17;ip.AsSpan(8,32).CopyTo(frame.AsSpan(18));ip.AsSpan(40).CopyTo(frame.AsSpan(50));}
        else{frame[15]=0x41;ip.CopyTo(frame,16);}return frame;
    }
    static byte[] Fragment(bool second,ushort tag)
    {
        byte[]ip=Ip(),frame=new byte[second?37:68];Radio().AsSpan(0,15).CopyTo(frame);frame[15]=(byte)(second?0xe0:0xc0);frame[16]=65;Put(frame,17,tag);
        if(second){frame[19]=6;ip.AsSpan(48).CopyTo(frame.AsSpan(20));}else{frame[19]=0x41;ip.AsSpan(0,48).CopyTo(frame.AsSpan(20));}return frame;
    }
    static byte[] Outer(ushort port)
    {
        byte[]radio=Radio();byte[]frame=new byte[143];frame[0]=0x45;Put(frame,2,frame.Length);frame[8]=64;frame[9]=17;frame[12]=frame[16]=10;frame[13]=frame[17]=1;frame[15]=2;frame[19]=1;Put(frame,10,(int)~Sum(frame.AsSpan(0,20)));Put(frame,20,17754);Put(frame,22,port);Put(frame,24,123);
        frame[28]=(byte)'E';frame[29]=(byte)'X';frame[30]=2;frame[31]=1;frame[35]=1;frame[59]=83;radio.CopyTo(frame,60);BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(141),Crc(radio));return frame;
    }
    sealed class Receiver:IDisposable
    {
        readonly LwipStack stack;GCHandle root;nint pcb;internal int Count;internal Exception? Error;
        internal Receiver(LwipStack stack)
        {
            this.stack=stack;root=GCHandle.Alloc(this);
            stack.Invoke(core=>{var p=core.udp_new_ip_type(IpV6);R(p!=null,"UDP receiver allocation");pcb=(nint)p;ip_addr any=default;any.type=IpV6;R(core.udp_bind(p,&any,9000)==0,"UDP receiver bind");core.udp_recv(p,&Received,(void*)GCHandle.ToIntPtr(root));});
        }
        static void Received(Lwip core,void*argument,udp_pcb*pcb,pbuf*packet,ip_addr*address,ushort port)
        {
            var state=(Receiver)GCHandle.FromIntPtr((nint)argument).Target!;
            try{R(address->type==IpV6&&port==9001&&packet->tot_len==17,"decoded original IPv6/UDP metadata");byte[]bytes=new byte[17];fixed(byte*p=bytes)R(core.pbuf_copy_partial(packet,p,17,0)==17,"decoded payload copy");for(int i=0;i<17;i++)R(bytes[i]==(byte)(i*7),"decoded original payload");state.Count++;}catch(Exception error){state.Error=error;}finally{core.pbuf_free(packet);}
        }
        public void Dispose(){if(pcb==0)return;stack.Invoke(core=>{core.udp_recv((udp_pcb*)pcb,null,null);core.udp_remove((udp_pcb*)pcb);});pcb=0;root.Free();}
    }
    static void Inject(LwipStack stack,nint transport,ushort port)
    {
        byte[]bytes=Outer(port);stack.Invoke(core=>{var packet=core.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)bytes.Length,pbuf_type.PBUF_RAM);R(packet!=null,"outer packet allocate");fixed(byte*p=bytes)R(core.pbuf_take(packet,p,(ushort)bytes.Length)==0,"outer packet copy");R(core.ip_input(packet,(netif*)transport)==0,"valid UDP ZEP input");});
    }
    static (uint Memory,uint Pbuf,uint Timers) Baseline(LwipStack stack)
    {uint memory=0,pbufs=0,timers=0;stack.Invoke(core=>{memory=core.Globals.lwip_stats.mem.used;pbufs=core.Globals.memp_PBUF_POOL.stats->used;timers=core.Globals.memp_SYS_TIMEOUT.stats->used;});return(memory,pbufs,timers);}
    static LwipLowpanLink Zep(LwipStack stack,nint transport,ushort port)=>LwipLowpanLink.StartZep(stack,IPAddress.Parse("fd00::1"),new byte[]{2,0,0,0,0,1},new(IPAddress.Any,port),new(IPAddress.Parse("10.1.0.2"),17754),transport);
    static void Main()
    {
        OutputCases.Run(false);OutputCases.Run(true);
        using var a=new LwipStack();using var b=new LwipStack();LwipSysArch.SetClock(a.Core,()=>1000);LwipSysArch.SetClock(b.Core,()=>1000);a.Start();b.Start();
        nint na=a.AddInterface("10.1.0.1","",new byte[]{2,0,0,0,0,1},_=>{}),nb=b.AddInterface("10.1.0.1","",new byte[]{2,0,0,0,0,1},_=>{});
        using var ra=new Receiver(a);using var rb=new Receiver(b);var baseline=Baseline(a);
        LwipLowpanLink.ConfigureRadio(a,0x1234);LwipLowpanLink.ConfigureRadio(b,0x1234);
        var za=Zep(a,na,17754);bool duplicate=false;try{Zep(a,na,17754);}catch(InvalidOperationException){duplicate=true;}R(duplicate,"original duplicate UDP bind rolls back partial timer/interface initialization");
        bool foreign=false;try{Zep(a,nb,17755);}catch(ArgumentException){foreign=true;}R(foreign,"transport identity belongs to same stack");
        var za2=Zep(a,na,17755);var radio=LwipLowpanLink.StartRadio(a,IPAddress.Parse("fd00::1"),new byte[]{2,0,0,0,0,1},_=>{});var ble=LwipLowpanLink.StartBle(a,IPAddress.Parse("fd00::1"),new byte[]{2,0,0,0,0,1},_=>{});var zb=Zep(b,nb,17754);
        R(radio.Index!=ble.Index&&radio.Mtu==1280&&ble.Mtu==1280,"typed owner-relative index and original IPv6 MTU");radio.SetLinkState(false);R(!radio.IsLinkUp,"original link-down state");radio.SetLinkState(true);R(radio.IsLinkUp,"original link-up state");
        R(Baseline(a).Timers==baseline.Timers+1,"radio/ZEP share one original timer; BLE has none");Inject(a,na,17754);Inject(a,na,17755);R(ra.Error==null,ra.Error?.ToString()??"A callback errors");R(ra.Count==2,"real IPv4 UDP ZEP radio IPv6 UDP receive");
        za.Dispose();Inject(a,na,17755);R(ra.Count==3,"remaining ZEP still works");radio.Receive(Fragment(false,0x1234));za2.Dispose();R(Baseline(a).Timers==baseline.Timers+1,"last ZEP cannot cancel direct radio timer");a.Invoke(core=>core.lowpan6_tmr());radio.Receive(Fragment(true,0x1234));R(ra.Count==4,"valid reassembly survives other service close and timer tick");radio.Receive(Radio(true));R(ra.Count==5,"independent valid IPHC input");
        radio.Receive(Fragment(false,0x1235));radio.Dispose();R(Baseline(a).Timers==baseline.Timers,"last radio lease releases timer even while BLE survives");ble.Receive(Radio(true).AsMemory(15));R(ra.Count==6,"BLE independent original input survives radio shutdown");ble.Dispose();
        using(var borrowed=LwipLowpanLink.RetainBorrowedRadioTimer(a))R(Baseline(a).Timers==baseline.Timers+1,"borrowed raw radio lease owns timer");
        var restarted=Zep(a,na,17755);Inject(a,na,17755);R(ra.Count==7,"last-close timer restarts");restarted.Dispose();R(Baseline(a)==baseline,"natural heap/pbuf/timer baseline after explicit service cleanup");
        var pending=LwipLowpanLink.StartRadio(a,IPAddress.Parse("fd00::1"),new byte[]{2,0,0,0,0,1},_=>{});pending.Receive(Fragment(false,0x1236));ra.Dispose();a.Dispose();
        GC.Collect();GC.WaitForPendingFinalizers();Inject(b,nb,17754);R(rb.Error==null,rb.Error?.ToString()??"B callback errors");R(rb.Count==1,"independent owner B survives A pending-fragment service disposal");zb.Dispose();rb.Dispose();b.Dispose();
        Console.WriteLine("PASS lowpan-services: valid ZEP/FCS and BLE input, shared radio timer and reassembly leases, pending fragments, rollback/restart and owner isolation");
    }
}

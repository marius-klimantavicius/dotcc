using System.Net;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;

// Deliver complete, unchanged frames emitted by the original algorithms.
// Reordering, duplication and loss do not alter any encoded byte.
static unsafe class OutputCases
{
    static void R(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    static byte Pattern(int i)=>(byte)(i*13+(i>>8));
    static ushort Crc(ReadOnlySpan<byte> bytes){uint value=0;foreach(byte b in bytes){value^=b;for(int i=0;i<8;i++)value=(value>>1)^((value&1)!=0?0x8408u:0);}return (ushort)value;}
    static ip_addr Address(string text){ip_addr result=default;result.type=6;IPAddress.Parse(text).GetAddressBytes().CopyTo(new Span<byte>(&result.u_addr.ip6.addr[0],16));return result;}
    static (uint,uint,uint) Baseline(LwipStack stack){(uint,uint,uint) value=default;stack.Invoke(core=>value=(core.Globals.lwip_stats.mem.used,core.Globals.memp_PBUF_POOL.stats->used,core.Globals.memp_SYS_TIMEOUT.stats->used));return value;}
    sealed class Receiver:IDisposable
    {
        readonly LwipStack stack;GCHandle root;nint pcb;internal int Expected,Count;internal Exception? Error;
        internal Receiver(LwipStack stack){this.stack=stack;root=GCHandle.Alloc(this);stack.Invoke(core=>{var p=core.udp_new_ip_type(6);R(p!=null,"output receiver allocate");pcb=(nint)p;ip_addr any=default;any.type=6;R(core.udp_bind(p,&any,9100)==0,"output receiver bind");core.udp_recv(p,&Receive,(void*)GCHandle.ToIntPtr(root));});}
        static void Receive(Lwip core,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* source,ushort port)
        {
            var state=(Receiver)GCHandle.FromIntPtr((nint)argument).Target!;
            try{R(source->type==6&&port==9101&&packet->tot_len==state.Expected,"output datagram metadata");byte[] bytes=new byte[state.Expected];fixed(byte*p=bytes)R(core.pbuf_copy_partial(packet,p,(ushort)bytes.Length,0)==bytes.Length,"output datagram copy");for(int i=0;i<bytes.Length;i++)R(bytes[i]==Pattern(i),"output datagram contents");state.Count++;}
            catch(Exception error){state.Error=error;}finally{core.pbuf_free(packet);}
        }
        public void Dispose(){if(pcb==0)return;stack.Invoke(core=>core.udp_remove((udp_pcb*)pcb));pcb=0;root.Free();}
    }
    internal static void Run(bool ble)
    {
        using var a=new LwipStack();using var b=new LwipStack();LwipSysArch.SetClock(a.Core,()=>1000);LwipSysArch.SetClock(b.Core,()=>1000);a.Start();b.Start();var baselineA=Baseline(a);var baselineB=Baseline(b);
        List<byte[]> frames=new();byte[] macA={2,0,0,0,0,2},macB={2,0,0,0,0,1};
        using var la=ble?LwipLowpanLink.StartBle(a,IPAddress.Parse("fd00::2"),macA,frames.Add):LwipLowpanLink.StartRadio(a,IPAddress.Parse("fd00::2"),macA,frames.Add);
        using var lb=ble?LwipLowpanLink.StartBle(b,IPAddress.Parse("fd00::1"),macB,_=>{}):LwipLowpanLink.StartRadio(b,IPAddress.Parse("fd00::1"),macB,_=>{});
        LwipLowpanLink.ConfigureRadio(a,0x1234);LwipLowpanLink.ConfigureRadio(b,0x1234);
        LwipLowpanLink.SetContext(a,ble,0,IPAddress.Parse("fd00::"));LwipLowpanLink.SetContext(b,ble,0,IPAddress.Parse("fd00::"));
        if(ble){byte[] euiA={2,0,0,0,0,0,0,2},euiB={2,0,0,0,0,0,0,1};LwipLowpanLink.ConfigureBlePeer(a,euiA,euiB);LwipLowpanLink.ConfigureBlePeer(b,euiB,euiA);}
        la.SetDefaultRoute();byte index=la.Index;nint sender=0;using var receiver=new Receiver(b);
        a.Invoke(core=>{var p=core.udp_new_ip_type(6);R(p!=null,"output sender allocate");sender=(nint)p;var source=Address("fd00::2");R(core.udp_bind(p,&source,9101)==0,"output sender bind");});
        void Send(int size)
        {
            receiver.Expected=size;frames.Clear();byte[] bytes=Enumerable.Range(0,size).Select(Pattern).ToArray();
            a.Invoke(core=>{var source=Address("fd00::2");var destination=Address(ble?"fd00::1":"ff02::1");destination.u_addr.ip6.zone=index;var net=core.netif_get_by_index(index);R(net!=null,"output owned interface");var p=core.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,(ushort)size,pbuf_type.PBUF_RAM);R(p!=null,"output send allocation");try{fixed(byte*x=bytes)R(core.pbuf_take(p,x,(ushort)size)==0,"output send copy");R(core.udp_sendto_if_src((udp_pcb*)sender,p,&destination,9100,net,&source)==0,"original output send");}finally{core.pbuf_free(p);}});
            R(frames.Count>0,"output callback invoked");foreach(byte[] frame in frames){R(frame.Length>0,"output callback copied bytes");if(!ble){R(frame.Length<=127&&frame.Length>=2,"radio frame MTU");R(Crc(frame.AsSpan(0,frame.Length-2))==(ushort)(frame[^2]|frame[^1]<<8),"valid original output FCS");}}
        }
        void Deliver(int i){var frame=frames[i];lb.Receive(ble?frame:frame.AsMemory(0,frame.Length-2));}
        void Complete(int before){R(receiver.Error==null,receiver.Error?.ToString()??"output callback");R(receiver.Count==before+1,"output reassembled exactly once");}
        byte[][]? retained=null,copies=null;
        foreach(int size in new[]{17,900,1232,1233}){int before=receiver.Count;Send(size);for(int i=0;i<frames.Count;i++)Deliver(i);Complete(before);if(retained!=null)for(int i=0;i<retained.Length;i++)R(retained[i].AsSpan().SequenceEqual(copies![i]),"retained output survives later original pbuf reuse");retained=frames.ToArray();copies=retained.Select(x=>x.ToArray()).ToArray();GC.Collect();GC.WaitForPendingFinalizers();}
        if(!ble)
        {
            int before=receiver.Count;Send(900);R(frames.Count>3,"radio fragments emitted");Deliver(0);Deliver(1);Deliver(1);for(int i=2;i<frames.Count;i++)Deliver(i);Complete(before);
            before=receiver.Count;Send(900);Deliver(0);Deliver(frames.Count-1);for(int i=1;i<frames.Count-1;i++)Deliver(i);Complete(before);
            before=receiver.Count;Send(900);Deliver(0);b.Invoke(core=>{core.lowpan6_tmr();core.lowpan6_tmr();});for(int i=1;i<frames.Count;i++)Deliver(i);R(receiver.Count==before,"valid incomplete packet expires");
        }
        a.Invoke(core=>core.udp_remove((udp_pcb*)sender));receiver.Dispose();la.Dispose();lb.Dispose();R(Baseline(a)==baselineA&&Baseline(b)==baselineB,"output natural heap/pbuf/timer baseline");
        Console.WriteLine("PASS lowpan output: "+(ble?"BLE":"radio")+" copied frames, compression, MTU, original reassembly and natural cleanup");
    }
}

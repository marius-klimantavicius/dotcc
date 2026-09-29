using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    private sealed class Receiver
    {
        internal readonly List<byte[]> Datagrams=new();
        internal readonly AutoResetEvent Arrived=new(false);
        internal Exception? Error;
    }
    static void Require(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    static void Receive(Lwip core,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var receiver=(Receiver)GCHandle.FromIntPtr((nint)argument).Target!;
        try{byte[] bytes=new byte[packet->tot_len];fixed(byte* data=bytes)Require(core.pbuf_copy_partial(packet,data,packet->tot_len,0)==packet->tot_len,"datagram pbuf copy");lock(receiver.Datagrams)receiver.Datagrams.Add(bytes);receiver.Arrived.Set();}
        catch(Exception error){receiver.Error=error;receiver.Arrived.Set();}
        finally{core.pbuf_free(packet);}
    }
    static void Main(string[] args)
    {
        using var capture=args.Length==0?null:new Pcap(args[0]);
        using var pair=new VirtualPair(capture==null?null:capture.Write);DatagramCase(pair,false,false);DatagramCase(pair,true,false);DatagramCase(pair,false,true);DatagramCase(pair,true,true);pair.Check();
        Console.WriteLine("PASS datagrams: IPv4/IPv6 fragmentation/reassembly and UDP-Lite partial coverage positive/negative fixtures");
    }
    static int ProtocolOffset(byte[] frame,bool ipv6,int protocol)
    {
        if(ipv6)return frame.Length>=62&&frame[12]==0x86&&frame[13]==0xdd&&frame[20]==protocol?54:-1;
        return frame.Length>=42&&frame[12]==8&&frame[13]==0&&frame[23]==protocol?14+(frame[14]&15)*4:-1;
    }
    static void DatagramCase(VirtualPair pair,bool ipv6,bool lite)
    {
        var receiver=new Receiver();var handle=GCHandle.Alloc(receiver);nint sender=0,listener=0;byte type=(byte)(ipv6?lwip_ip_addr_type.IPADDR_TYPE_V6:lwip_ip_addr_type.IPADDR_TYPE_V4);
        byte[] text=Encoding.UTF8.GetBytes((ipv6?"fd00::2":"10.0.0.2")+"\0");ushort port=(ushort)(25000+(ipv6?10:0)+(lite?1:0));
        int fragments=0;
        try
        {
            pair.Observe=(toB,frame)=>{if(!toB||lite)return;if(ipv6){if(frame.Length>62&&frame[12]==0x86&&frame[13]==0xdd&&frame[20]==44)Interlocked.Increment(ref fragments);}else if(frame.Length>34&&frame[12]==8&&frame[13]==0&&frame[23]==17&&(BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(20,2))&0x3fff)!=0)Interlocked.Increment(ref fragments);};
            pair.B.Invoke(core=>{ip_addr address;fixed(byte* bytes=text)Require(core.ipaddr_aton(bytes,&address)==1,"UDP address");var pcb=core.udp_new_ip_type(type);Require(pcb!=null,"UDP listener");listener=(nint)pcb;if(lite){pcb->flags|=(byte)UDP_FLAGS_UDPLITE;pcb->chksum_len_rx=16;}Require(core.udp_bind(pcb,&address,port)==0,"UDP bind");core.udp_recv(pcb,&Receive,(void*)GCHandle.ToIntPtr(handle));});
            pair.A.Invoke(core=>{var pcb=core.udp_new_ip_type(type);Require(pcb!=null,"UDP sender");sender=(nint)pcb;if(lite){pcb->flags|=(byte)UDP_FLAGS_UDPLITE;pcb->chksum_len_tx=16;}});
            byte[] payload=new byte[lite?64:4500];for(int i=0;i<payload.Length;i++)payload[i]=(byte)(i*31+(i>>8));
            void Send()
            {
                pair.A.Invoke(core=>{ip_addr address;fixed(byte* bytes=text)Require(core.ipaddr_aton(bytes,&address)==1,"UDP destination");var packet=core.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,(ushort)payload.Length,pbuf_type.PBUF_RAM);Require(packet!=null,"UDP pbuf");try{fixed(byte* bytes=payload)Require(core.pbuf_take(packet,bytes,(ushort)payload.Length)==0,"UDP payload");Require(core.udp_sendto((udp_pcb*)sender,packet,&address,port)==0,"UDP output");}finally{core.pbuf_free(packet);}});
            }
            byte[] Await(int count)
            {var end=DateTime.UtcNow.AddSeconds(10);while(true){if(receiver.Error!=null)throw receiver.Error;lock(receiver.Datagrams)if(receiver.Datagrams.Count>=count)return receiver.Datagrams[count-1];if(DateTime.UtcNow>end)throw new TimeoutException("UDP bounded delivery");receiver.Arrived.WaitOne(50);}}
            // Upstream ARP_QUEUEING=0 retains only one pending packet. Resolve
            // the neighbor with a small datagram before testing IP fragments.
            byte[] actualPayload=payload;payload=new byte[]{0xa5};Send();Require(Await(1).AsSpan().SequenceEqual(payload),"neighbor-resolution datagram");
            lock(receiver.Datagrams)receiver.Datagrams.Clear();payload=actualPayload;Volatile.Write(ref fragments,0);
            Send();Require(Await(1).AsSpan().SequenceEqual(payload),"UDP complete payload");
            if(lite)
            {
                pair.Filter=(toB,frame)=>{int offset=ProtocolOffset(frame,ipv6,136);if(toB&&offset>=0){Require(BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(offset+4,2))==16,"UDP-Lite wire coverage16");frame[offset+8+32]^=0x80;}return frame;};
                Send();byte[] changed=(byte[])payload.Clone();changed[32]^=0x80;Require(Await(2).AsSpan().SequenceEqual(changed),"UDP-Lite uncovered corruption accepted");
                pair.Filter=(toB,frame)=>{int offset=ProtocolOffset(frame,ipv6,136);if(toB&&offset>=0)frame[offset+8+2]^=0x80;return frame;};
                Send();pair.Drain();lock(receiver.Datagrams)Require(receiver.Datagrams.Count==2,"UDP-Lite covered corruption rejected");
                pair.Filter=null;Send();Require(Await(3).AsSpan().SequenceEqual(payload),"UDP-Lite recovery after checksum failure");
            }
            else Require(Volatile.Read(ref fragments)>=3,"UDP MTU fragmentation observed");
            Console.WriteLine($"PASS {(lite?"UDP-Lite coverage16":"UDP fragments")} IPv{(ipv6?6:4)} payload={payload.Length}");
        }
        finally
        {
            pair.Filter=null;pair.Observe=null;
            pair.A.Invoke(core=>{if(sender!=0)core.udp_remove((udp_pcb*)sender);});pair.B.Invoke(core=>{if(listener!=0)core.udp_remove((udp_pcb*)listener);});handle.Free();receiver.Arrived.Dispose();
        }
    }
}

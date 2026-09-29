using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    static void R(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    sealed class Receiver(Lwip owner){internal readonly Lwip Owner=owner;internal int Count;internal Exception? Error;}
    static void Receive(Lwip owner,void*argument,udp_pcb*pcb,pbuf*packet,ip_addr*address,ushort port)
    {
        var state=(Receiver)GCHandle.FromIntPtr((nint)argument).Target!;
        try{R(ReferenceEquals(owner,state.Owner),"multicast callback owner");R(packet->tot_len==5,"multicast payload length");byte* bytes=stackalloc byte[5];R(owner.pbuf_copy_partial(packet,bytes,5,0)==5&&new ReadOnlySpan<byte>(bytes,5).SequenceEqual("group"u8),"multicast payload");state.Count++;}
        catch(Exception error){state.Error=error;}finally{owner.pbuf_free(packet);}
    }
    static ushort Sum(ReadOnlySpan<byte> bytes){uint sum=0;for(int i=0;i<bytes.Length;i+=2)sum+=(uint)(bytes[i]<<8)|(uint)(i+1<bytes.Length?bytes[i+1]:0);while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return (ushort)~sum;}
    static byte[] Query4()
    {
        var f=new byte[42];new byte[]{1,0,0x5e,0,0,1}.CopyTo(f,0);new byte[]{2,0,0,0,0,2}.CopyTo(f,6);f[12]=8;f[14]=0x45;f[17]=28;f[22]=1;f[23]=2;new byte[]{10,0,0,2}.CopyTo(f,26);new byte[]{224,0,0,1}.CopyTo(f,30);BinaryPrimitives.WriteUInt16BigEndian(f.AsSpan(24),Sum(f.AsSpan(14,20)));f[34]=0x11;f[35]=10;BinaryPrimitives.WriteUInt16BigEndian(f.AsSpan(36),Sum(f.AsSpan(34,8)));return f;
    }
    static byte[] Query6()
    {
        var f=new byte[86];new byte[]{0x33,0x33,0,0,0,1}.CopyTo(f,0);new byte[]{2,0,0,0,0,2}.CopyTo(f,6);f[12]=0x86;f[13]=0xdd;f[14]=0x60;f[19]=32;f[20]=0;f[21]=1;f[22]=0xfe;f[23]=0x80;f[37]=2;f[38]=0xff;f[39]=2;f[53]=1;f[54]=58;f[56]=5;f[57]=2;f[60]=1;f[62]=130;BinaryPrimitives.WriteUInt16BigEndian(f.AsSpan(66),1000);
        byte[] pseudo=new byte[64];f.AsSpan(22,32).CopyTo(pseudo);pseudo[35]=24;pseudo[39]=58;f.AsSpan(62,24).CopyTo(pseudo.AsSpan(40));BinaryPrimitives.WriteUInt16BigEndian(f.AsSpan(64),Sum(pseudo));return f;
    }
    static int Message(byte[]frame,bool ipv6)
    {
        if(!ipv6){if(frame.Length<42||frame[12]!=8||frame[13]!=0||frame[23]!=2)return -1;int ipv4Offset=14+(frame[14]&15)*4;if(frame.Length<ipv4Offset+8||!frame.AsSpan(ipv4Offset+4,4).SequenceEqual(new byte[]{239,1,2,3}))return -1;return frame[ipv4Offset];}
        if(frame.Length<78||frame[12]!=0x86||frame[13]!=0xdd)return -1;int offset=54;byte next=frame[20];if(next==0){next=frame[offset];offset+=(frame[offset+1]+1)*8;}return next==58&&offset+24<=frame.Length&&frame.AsSpan(offset+8,16).SequenceEqual(System.Net.IPAddress.Parse("ff02::1234").GetAddressBytes())?frame[offset]:-1;
    }
    static void Case(VirtualPair pair,bool ipv6)
    {
        var state=new Receiver(pair.A.Core);var root=GCHandle.Alloc(state);nint receiver=0,sender=0,iface=0;bool joined=false;int reports=0,leaves=0;byte[]text=System.Text.Encoding.UTF8.GetBytes((ipv6?"ff02::1234":"239.1.2.3")+"\0");
        pair.Observe=(toB,frame)=>{if(toB){int type=Message(frame,ipv6);if(type==(ipv6?131:0x16))Interlocked.Increment(ref reports);if(type==(ipv6?132:0x17))Interlocked.Increment(ref leaves);}};
        try
        {
            pair.A.Invoke(owner=>{iface=owner.Globals.netif_default;var pcb=owner.udp_new_ip_type((byte)(ipv6?6:0));R(pcb!=null,"multicast receiver allocate");receiver=(nint)pcb;ip_addr any=default;any.type=(byte)(ipv6?6:0);R(owner.udp_bind(pcb,&any,28600)==0,"multicast receiver bind");owner.udp_recv(pcb,&Receive,(void*)GCHandle.ToIntPtr(root));});pair.B.Invoke(owner=>{sender=(nint)owner.udp_new_ip_type((byte)(ipv6?6:0));R(sender!=0,"multicast sender allocate");});
            void Send(){pair.B.Invoke(owner=>{ip_addr group;fixed(byte*p=text)R(owner.ipaddr_aton(p,&group)==1,"multicast address parse");var packet=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,5,pbuf_type.PBUF_RAM);R(packet!=null,"multicast payload allocate");try{fixed(byte*p="group"u8)R(owner.pbuf_take(packet,p,5)==0,"multicast payload write");R(owner.udp_sendto_if((udp_pcb*)sender,packet,&group,28600,(netif*)owner.Globals.netif_default)==0,"multicast send");}finally{owner.pbuf_free(packet);}});pair.Drain();if(state.Error!=null)throw state.Error;}
            void Membership(bool join){pair.A.Invoke(owner=>{ip_addr group;fixed(byte*p=text)R(owner.ipaddr_aton(p,&group)==1,"multicast membership address");int result=ipv6?(join?owner.mld6_joingroup_netif((netif*)iface,&group.u_addr.ip6):owner.mld6_leavegroup_netif((netif*)iface,&group.u_addr.ip6)):(join?owner.igmp_joingroup_netif((netif*)iface,&group.u_addr.ip4):owner.igmp_leavegroup_netif((netif*)iface,&group.u_addr.ip4));R(result==0,"multicast join/leave");joined=join;});pair.Drain();}
            Send();R(state.Count==0,"unjoined group rejected");Membership(true);pair.A.Invoke(owner=>{for(int i=0;i<110;i++){if(ipv6)owner.mld6_tmr();else owner.igmp_tmr();}});pair.Drain();R(reports>0,"join emits membership report");Send();R(state.Count==1,"joined group receives datagram");int before=reports;pair.A.Receive(iface,ipv6?Query6():Query4());pair.Drain();pair.A.Invoke(owner=>{for(int i=0;i<15;i++){if(ipv6)owner.mld6_tmr();else owner.igmp_tmr();}});pair.Drain();R(reports>before,"query timer emits renewed report");Membership(false);R(leaves>0,"leave emits protocol message");Send();R(state.Count==1,"left group rejects datagram");Console.WriteLine($"PASS {(ipv6?"MLD6":"IGMP")} per-interface join/admission/query-timer-report/leave/drop");
        }
        finally{pair.A.Invoke(owner=>{if(joined){ip_addr group;fixed(byte*p=text)owner.ipaddr_aton(p,&group);if(ipv6)owner.mld6_leavegroup_netif((netif*)iface,&group.u_addr.ip6);else owner.igmp_leavegroup_netif((netif*)iface,&group.u_addr.ip4);}if(receiver!=0)owner.udp_remove((udp_pcb*)receiver);});pair.B.Invoke(owner=>{if(sender!=0)owner.udp_remove((udp_pcb*)sender);});pair.Drain();pair.Observe=null;root.Free();}
    }
    static void Main(string[]args){using var capture=new Pcap(args[0]);using var pair=new VirtualPair(capture.Write);Case(pair,false);Case(pair,true);Console.WriteLine("PASS multicast-fixtures: original membership state controls real multicast ingress and reports");}
}

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    sealed class DhcpPeer(Lwip owner)
    {
        internal readonly Lwip Owner=owner;
        internal int Discovers,Requests,Declines,Releases,Naks;
        internal volatile bool Silent,NakNext,HoldOffers;
        internal Exception? Error;
    }
    static int DhcpType(ReadOnlySpan<byte> packet)
    {
        R(packet.Length>=244&&packet[0]==1&&packet[1]==1&&packet[2]==6,"DHCP BOOTP header");R(BinaryPrimitives.ReadUInt32BigEndian(packet[236..])==0x63825363,"DHCP cookie");
        for(int i=240;i<packet.Length;){byte code=packet[i++];if(code==255)break;if(code==0)continue;R(i<packet.Length,"DHCP option length");int length=packet[i++];R(i+length<=packet.Length,"DHCP option bounds");if(code==53){R(length==1,"DHCP message type length");return packet[i];}i+=length;}throw new InvalidOperationException("Missing DHCP message type");
    }
    static byte[] DhcpResponse(byte[] request,byte type)
    {
        var result=new byte[304];result[0]=2;result[1]=1;result[2]=6;request.AsSpan(4,4).CopyTo(result.AsSpan(4));request.AsSpan(28,16).CopyTo(result.AsSpan(28));result[10]=0x80;
        if(type!=6)new byte[]{10,0,0,100}.CopyTo(result,16);new byte[]{10,0,0,2}.CopyTo(result,20);BinaryPrimitives.WriteUInt32BigEndian(result.AsSpan(236),0x63825363);int cursor=240;
        void Option(byte code,byte[] bytes){result[cursor++]=code;result[cursor++]=(byte)bytes.Length;bytes.CopyTo(result,cursor);cursor+=bytes.Length;}
        Option(53,new[]{type});Option(54,new byte[]{10,0,0,2});if(type!=6){Option(51,new byte[]{0,0,0,120});Option(1,new byte[]{255,255,255,0});Option(3,new byte[]{10,0,0,2});Option(6,new byte[]{10,0,0,2});Option(58,new byte[]{0,0,0,60});Option(59,new byte[]{0,0,0,105});}result[cursor]=255;return result;
    }
    static void DhcpQuery(Lwip owner,void* argument,udp_pcb*pcb,pbuf*packet,ip_addr*address,ushort port)
    {
        var state=(DhcpPeer)GCHandle.FromIntPtr((nint)argument).Target!;pbuf*output=null;
        try
        {
            R(ReferenceEquals(owner,state.Owner),"DHCP server callback owner");var request=new byte[packet->tot_len];fixed(byte*bytes=request)R(owner.pbuf_copy_partial(packet,bytes,packet->tot_len,0)==packet->tot_len,"DHCP query copy");int type=DhcpType(request);
            if(type==1)Interlocked.Increment(ref state.Discovers);else if(type==3)Interlocked.Increment(ref state.Requests);else if(type==4){Interlocked.Increment(ref state.Declines);return;}else if(type==7){Interlocked.Increment(ref state.Releases);return;}else throw new InvalidOperationException("Unexpected DHCP request type "+type);
            if(state.Silent||(type==1&&state.HoldOffers))return;byte responseType=(byte)(type==1?2:5);
            if(type==3&&state.NakNext){state.NakNext=false;state.HoldOffers=true;responseType=6;Interlocked.Increment(ref state.Naks);}
            var response=DhcpResponse(request,responseType);output=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,(ushort)response.Length,pbuf_type.PBUF_RAM);R(output!=null,"DHCP response allocate");fixed(byte*bytes=response)R(owner.pbuf_take(output,bytes,(ushort)response.Length)==0,"DHCP response fill");ip_addr broadcast;fixed(byte*text="255.255.255.255\0"u8)R(owner.ipaddr_aton(text,&broadcast)==1,"DHCP broadcast parse");R(owner.udp_sendto(pcb,output,&broadcast,68)==0,"DHCP response broadcast");
        }
        catch(Exception error){state.Error=error;}
        finally{if(output!=null)owner.pbuf_free(output);owner.pbuf_free(packet);}
    }
    static byte[] ConflictArp()
    {
        byte[] frame=new byte[42];new byte[]{2,0,0,0,0,1}.CopyTo(frame,0);new byte[]{2,0,0,0,0,99}.CopyTo(frame,6);frame[12]=8;frame[13]=6;frame[15]=1;frame[16]=8;frame[18]=6;frame[19]=4;frame[21]=2;new byte[]{2,0,0,0,0,99}.CopyTo(frame,22);new byte[]{10,0,0,100}.CopyTo(frame,28);new byte[]{2,0,0,0,0,1}.CopyTo(frame,32);new byte[]{10,0,0,100}.CopyTo(frame,38);return frame;
    }
    static void Dhcp(VirtualPair pair)
    {
        var state=new DhcpPeer(pair.B.Core);var handle=GCHandle.Alloc(state);nint server=0,client=0;bool started=false;
        try
        {
            pair.B.Invoke(owner=>{var pcb=owner.udp_new();R(pcb!=null,"DHCP server UDP allocate");server=(nint)pcb;pcb->so_options|=(byte)SOF_BROADCAST;R(owner.udp_bind(pcb,null,67)==0,"DHCP server bind");owner.udp_recv(pcb,&DhcpQuery,(void*)GCHandle.ToIntPtr(handle));});
            pair.A.Invoke(owner=>{client=owner.Globals.netif_default;var n=(netif*)client;owner.netif_set_addr(n,null,null,null);R(owner.dhcp_start(n)==0,"DHCP start");started=true;});
            bool Bound(){bool result=false;pair.A.Invoke(owner=>result=owner.dhcp_supplied_address((netif*)client)!=0);return result;}
            void CheckPeer(){pair.Check();if(state.Error!=null)throw state.Error;}
            void Tick(){pair.Drain();CheckPeer();pair.A.Invoke(owner=>{owner.dhcp_fine_tmr();for(int i=0;i<5;i++)owner.acd_tmr();});pair.Drain();CheckPeer();}
            void Until(Func<bool> predicate,string name){for(int i=0;i<100;i++){pair.Drain();CheckPeer();if(predicate())return;Tick();}throw new TimeoutException("DHCP bounded transition: "+name);}
            void Address(bool bound){pair.A.Invoke(owner=>{var n=(netif*)client;R(n->ip_addr.u_addr.ip4.addr==(bound?owner.lwip_htonl(0x0a000064):0),"DHCP address assigned/cleared");if(bound){R(n->netmask.u_addr.ip4.addr==owner.lwip_htonl(0xffffff00),"DHCP netmask option");R(n->gw.u_addr.ip4.addr==owner.lwip_htonl(0x0a000002),"DHCP gateway option");}});}
            Until(Bound,"DORA/acd");Address(true);R(state.Discovers>=1&&state.Requests>=1,"DHCP discover/request observed");Console.WriteLine("PASS DHCPv4 DORA ACK options address-conflict-detection probes");
            int requests=state.Requests;pair.A.Invoke(owner=>owner.dhcp_coarse_tmr());Until(()=>state.Requests>requests&&Bound(),"T1 renewal");Address(true);Console.WriteLine("PASS DHCPv4 coarse timer T1 renewal");
            state.NakNext=true;pair.A.Invoke(owner=>R(owner.dhcp_renew((netif*)client)==0,"DHCP renew for NAK"));Until(()=>state.Naks==1,"NAK received");pair.Drain();R(!Bound(),"NAK loses lease");Address(false);state.HoldOffers=false;Until(Bound,"NAK recovery");Address(true);Console.WriteLine("PASS DHCPv4 NAK clears address and restarts discovery");
            state.Silent=true;pair.A.Invoke(owner=>{owner.dhcp_coarse_tmr();owner.dhcp_coarse_tmr();});pair.Drain();R(!Bound(),"expired lease no longer supplied");Address(false);state.Silent=false;Until(Bound,"expiry recovery");Address(true);Console.WriteLine("PASS DHCPv4 lease expiry clears address and recovers");
            state.HoldOffers=true;pair.A.Receive(client,ConflictArp());pair.Drain();pair.A.Receive(client,ConflictArp());Until(()=>state.Declines>0,"ACD conflict decline");R(!Bound(),"conflicting lease withdrawn");Address(false);state.HoldOffers=false;Until(Bound,"conflict recovery");Address(true);Console.WriteLine("PASS DHCPv4 repeated address conflict DECLINE and recovery");
        }
        finally
        {
            if(started)pair.A.Invoke(owner=>{owner.dhcp_release_and_stop((netif*)client);owner.dhcp_cleanup((netif*)client);});pair.Drain();pair.B.Invoke(owner=>{if(server!=0)owner.udp_remove((udp_pcb*)server);});handle.Free();
        }
    }
}

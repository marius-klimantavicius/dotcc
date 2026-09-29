using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void R(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    sealed class DnsPeer
    {
        internal int Queries;
        internal Exception? Error;
    }
    sealed class Answer(Lwip owner)
    {
        internal readonly Lwip Owner=owner;
        internal bool Completed,Missing;
        internal ip_addr Value;
        internal Exception? Error;
    }
    static void Found(Lwip owner,byte* name,ip_addr* address,void* argument)
    {
        var answer=(Answer)GCHandle.FromIntPtr((nint)argument).Target!;
        try{R(ReferenceEquals(owner,answer.Owner),"DNS callback owner");answer.Missing=address==null;if(address!=null)answer.Value=*address;}
        catch(Exception error){answer.Error=error;}
        finally{Volatile.Write(ref answer.Completed,true);}
    }
    static byte[] Response(ReadOnlySpan<byte> query)
    {
        R(query.Length>=17&&BinaryPrimitives.ReadUInt16BigEndian(query[4..])==1,"one DNS question");int cursor=12;
        while(cursor<query.Length&&query[cursor]!=0){int length=query[cursor++];R(length<=63&&cursor+length<query.Length,"uncompressed query label");cursor+=length;}
        R(cursor+5==query.Length,"DNS query ends after type/class");cursor++;ushort type=BinaryPrimitives.ReadUInt16BigEndian(query[cursor..]);R(type==1||type==28,"A or AAAA query");R(BinaryPrimitives.ReadUInt16BigEndian(query[(cursor+2)..])==1,"IN question");
        bool missing=query[12]==7&&query.Slice(13,7).SequenceEqual("missing"u8);int size=type==1?4:16;
        byte[] response=new byte[query.Length+(missing?0:12+size)];query.CopyTo(response);response[2]=0x81;response[3]=(byte)(missing?0x83:0x80);response[6]=0;response[7]=(byte)(missing?0:1);
        if(!missing){int tail=query.Length;response[tail++]=0xc0;response[tail++]=0x0c;BinaryPrimitives.WriteUInt16BigEndian(response.AsSpan(tail),type);tail+=2;response[tail++]=0;response[tail++]=1;BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(tail),60);tail+=4;BinaryPrimitives.WriteUInt16BigEndian(response.AsSpan(tail),(ushort)size);tail+=2;if(type==1)new byte[]{192,0,2,123}.CopyTo(response,tail);else{response[tail]=0x20;response[tail+1]=1;response[tail+2]=0x0d;response[tail+3]=0xb8;response[tail+15]=0x7b;}}
        return response;
    }
    static void Query(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var state=(DnsPeer)GCHandle.FromIntPtr((nint)argument).Target!;pbuf* output=null;
        try
        {
            var bytes=new byte[packet->tot_len];fixed(byte* p=bytes)R(owner.pbuf_copy_partial(packet,p,packet->tot_len,0)==packet->tot_len,"DNS query pbuf copy");var response=Response(bytes);Interlocked.Increment(ref state.Queries);
            output=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,(ushort)response.Length,pbuf_type.PBUF_RAM);R(output!=null,"DNS response pbuf");fixed(byte* p=response)R(owner.pbuf_take(output,p,(ushort)response.Length)==0,"DNS response copy");R(owner.udp_sendto(pcb,output,address,port)==0,"DNS response send");
        }
        catch(Exception error){state.Error=error;}
        finally{if(output!=null)owner.pbuf_free(output);owner.pbuf_free(packet);}
    }
    static void Dns(VirtualPair pair,List<GCHandle> pending)
    {
        var peer=new DnsPeer();var peerHandle=GCHandle.Alloc(peer);nint listener=0;
        try
        {
            pair.B.Invoke(owner=>{ip_addr address;fixed(byte* text="10.0.0.2\0"u8)R(owner.ipaddr_aton(text,&address)==1,"DNS peer address");var pcb=owner.udp_new();R(pcb!=null,"DNS UDP allocate");listener=(nint)pcb;R(owner.udp_bind(pcb,&address,53)==0,"DNS UDP bind");owner.udp_recv(pcb,&Query,(void*)GCHandle.ToIntPtr(peerHandle));});
            pair.A.Invoke(owner=>{ip_addr address;fixed(byte* text="10.0.0.2\0"u8)R(owner.ipaddr_aton(text,&address)==1,"DNS server address");owner.dns_setserver(0,&address);});
            foreach(var (name,type,expected) in new[]{("v4.example.test",(byte)LWIP_DNS_ADDRTYPE_IPV4,"192.0.2.123"),("v6.example.test",(byte)LWIP_DNS_ADDRTYPE_IPV6,"2001:db8::7b"),("missing.example.test",(byte)LWIP_DNS_ADDRTYPE_IPV4,"")})
            {
                var answer=new Answer(pair.A.Core);var handle=GCHandle.Alloc(answer);bool submitted=false;
                try
                {
                    byte[] query=Encoding.UTF8.GetBytes(name+"\0");pair.A.Invoke(owner=>{ip_addr immediate;fixed(byte* text=query)R(owner.dns_gethostbyname_addrtype(text,&immediate,&Found,(void*)GCHandle.ToIntPtr(handle),type)==-5,"DNS asynchronous ERR_INPROGRESS");submitted=true;});
                    var limit=Stopwatch.StartNew();while(!Volatile.Read(ref answer.Completed)){pair.Check();if(peer.Error!=null)throw peer.Error;R(limit.Elapsed<TimeSpan.FromSeconds(10),"DNS bounded response");Thread.Yield();}
                    if(answer.Error!=null)throw answer.Error;R(answer.Missing==(expected.Length==0),"NXDOMAIN null callback vs successful answer");
                    if(expected.Length>0)
                    {
                        byte[] value=Encoding.UTF8.GetBytes(expected+"\0");pair.A.Invoke(owner=>{ip_addr wanted;ip_addr actual=answer.Value;fixed(byte* text=value)R(owner.ipaddr_aton(text,&wanted)==1,"expected DNS address");byte* formatted=stackalloc byte[80];R(owner.ipaddr_ntoa_r(&actual,formatted,80)!=null&&string.Equals(Marshal.PtrToStringUTF8((nint)formatted),expected,StringComparison.OrdinalIgnoreCase),"DNS address value");});
                        pair.Drain();int requests=Volatile.Read(ref peer.Queries);pair.A.Invoke(owner=>{ip_addr cached;fixed(byte* text=query)R(owner.dns_gethostbyname_addrtype(text,&cached,null,null,type)==0,"DNS cached synchronous success");});pair.Drain();R(peer.Queries==requests,"cached DNS answer emits no query");
                    }
                    Console.WriteLine($"PASS dns {name} {(expected.Length==0?"NXDOMAIN":"answer/cache "+expected)} explicit owner callback");
                }
                finally
                {
                    // The caller context cannot be freed while a resolver callback
                    // is pending. A failed fixture retains it through owner teardown.
                    if(!submitted||Volatile.Read(ref answer.Completed))handle.Free();
                    else pending.Add(handle);
                }
            }
            R(peer.Queries==3,"one wire query per unique name/type");
        }
        finally{pair.B.Invoke(owner=>{if(listener!=0)owner.udp_remove((udp_pcb*)listener);});peerHandle.Free();}
    }
    static void Main(string[] args)
    {
        using var capture=args.Length>0?new Pcap(args[0]):null;var pair=new VirtualPair(capture==null?null:capture.Write);var pending=new List<GCHandle>();
        try{if(args.Length>1&&args[1]=="autoip"){AutoIp(pair);}else if(args.Length>1&&args[1]=="dhcp"){Dhcp(pair);Console.WriteLine("PASS dhcp-fixtures: DORA renewal NAK expiry conflict recovery over translated Ethernet");}else{Dns(pair,pending);Console.WriteLine("PASS dns-fixtures: translated A/AAAA/cache/NXDOMAIN over two-owner Ethernet");}pair.Check();}
        finally{pair.Dispose();foreach(var handle in pending)handle.Free();}
    }
}

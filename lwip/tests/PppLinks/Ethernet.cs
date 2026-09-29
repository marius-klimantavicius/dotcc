using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;

/* Independently encoded RFC2516/RFC1661 peer; no copied discovery/FSM algorithm. */
unsafe static class Ethernet
{
    static void Require(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    sealed class Peer
    {
        public readonly List<byte[]> Frames=[];
        public readonly LwipStack Stack=new();
        public netif* Interface;
        public netif* PppInterface;
        public ppp_pcb_s* Session;
        public int Up,Datagrams;
        public Exception? Error;
        public GCHandle Root;
        public byte[] Unique=[];
    }
    static Peer? Active; // Test executes one owner; all callbacks still check explicit identity.
    static sbyte Output(Lwip owner,netif* iface,pbuf* packet)
    {
        var p=Active!;try{Require(ReferenceEquals(owner,p.Stack.Core)&&iface==p.Interface&&p.Frames.Count<256&&packet->tot_len<=2048,"Ethernet callback owner/bounds");byte[] data=new byte[packet->tot_len];fixed(byte* target=data)Require(owner.pbuf_copy_partial(packet,target,packet->tot_len,0)==packet->tot_len,"Ethernet complete pbuf");p.Frames.Add(data);return 0;}catch(Exception error){p.Error=error;return -1;}
    }
    static void Status(Lwip owner,ppp_pcb_s* pcb,int error,void* argument)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;Require(ReferenceEquals(owner,p.Stack.Core)&&pcb==p.Session,"PPPoE status owner");if(error==0)p.Up++;}
    static int U16(byte[] bytes,int index)=>(bytes[index]<<8)|bytes[index+1];
    static void Put16(byte[] bytes,int index,int value){bytes[index]=(byte)(value>>8);bytes[index+1]=(byte)value;}
    static byte[] Header(int code,int id,bool data)
    {byte[] frame=new byte[2048];frame[0]=frame[6]=2;frame[5]=1;frame[11]=2;Put16(frame,12,data?0x8864:0x8863);frame[14]=0x11;frame[15]=(byte)code;Put16(frame,16,id);return frame;}
    static int Append(byte[] frame,int at,int type,ReadOnlySpan<byte> data)
    {Put16(frame,at,type);Put16(frame,at+2,data.Length);data.CopyTo(frame.AsSpan(at+4));return at+4+data.Length;}
    static byte[] Tag(byte[] frame,int type)
    {Require(frame.Length>=20&&U16(frame,18)+20<=frame.Length,"discovery payload bounds");int limit=20+U16(frame,18);for(int at=20;at+4<=limit;){int length=U16(frame,at+2);Require(at+4+length<=limit,"tag bounds");if(U16(frame,at)==type)return frame.AsSpan(at+4,length).ToArray();at+=4+length;}throw new InvalidOperationException("Missing RFC2516 tag");}
    static void Inject(Peer p,Lwip owner,byte[] frame,int length,bool data)
    {pbuf* packet=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)length,pbuf_type.PBUF_RAM);Require(packet!=null,"PPPoE ingress allocation");fixed(byte* bytes=frame)Require(owner.pbuf_take(packet,bytes,(ushort)length)==0,"PPPoE ingress copy");if(data)owner.pppoe_data_input(p.Interface,packet);else owner.pppoe_disc_input(p.Interface,packet);if(p.Error!=null)throw p.Error;}
    static void Response(Peer p,Lwip owner,int protocol,int code,int id,ReadOnlySpan<byte> payload)
    {byte[] frame=Header(0,0x1234,true);Put16(frame,20,protocol);frame[22]=(byte)code;frame[23]=(byte)id;Put16(frame,24,payload.Length+4);payload.CopyTo(frame.AsSpan(26));Put16(frame,18,payload.Length+6);Inject(p,owner,frame,payload.Length+26,true);}
    static void Negotiate(Peer p,Lwip owner)
    {
        int sent=0;
        for(int index=3;index<p.Frames.Count;index++)
        {
            Require(index<200,"bounded PPP network control");byte[] frame=p.Frames[index];if(U16(frame,12)!=0x8864)continue;int protocol=U16(frame,20),code=frame[22],id=frame[23],length=U16(frame,24);Require(length>=4&&length+22<=frame.Length,"PPP control bounds");if(code!=1)continue;
            if(protocol==0xc021){Response(p,owner,protocol,2,id,frame.AsSpan(26,length-4));if((sent&1)==0){sent|=1;Response(p,owner,protocol,1,90,[]);}}
            else if(protocol==0x8021)
            {
                List<byte> rejected=[];for(int at=26;at<22+length;){int n=frame[at+1];Require(n>=2&&at+n<=22+length,"IPCP option bounds");if(frame[at]==2)rejected.AddRange(frame.AsSpan(at,n).ToArray());at+=n;}
                if(rejected.Count!=0){Response(p,owner,protocol,4,id,CollectionsMarshal.AsSpan(rejected));continue;}
                Response(p,owner,protocol,2,id,frame.AsSpan(26,length-4));if((sent&2)==0){sent|=2;Response(p,owner,protocol,1,91,[3,6,10,7,0,2]);}
            }
            else if(protocol==0x8057){Response(p,owner,protocol,2,id,frame.AsSpan(26,length-4));if((sent&4)==0){sent|=4;Response(p,owner,protocol,1,92,[1,10,0xee,0xdd,0xcc,0xbb,0xaa,0x99,0x88,2]);}}
            else if(protocol==0x80fd)Response(p,owner,protocol,4,id,frame.AsSpan(26,length-4));
        }
        Require(sent==7&&p.Up==2&&p.Session->phase==10,"independent peer completes IPCP and IPv6CP");
    }
    static int Checksum(ReadOnlySpan<byte> bytes)
    {uint sum=0;for(int i=0;i<bytes.Length;i+=2)sum+=(uint)((bytes[i]<<8)|(i+1<bytes.Length?bytes[i+1]:0));while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return (int)(~sum&65535);}
    static void Received(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;try{Require(ReferenceEquals(owner,p.Stack.Core)&&port==9001&&address->type==0&&packet->tot_len==73,"independent UDP peer");byte* bytes=stackalloc byte[73];Require(owner.pbuf_copy_partial(packet,bytes,73,0)==73,"UDP complete payload");for(int i=0;i<73;i++)Require(bytes[i]==(byte)(i*7),"independent UDP payload bytes");p.Datagrams++;}catch(Exception error){p.Error=error;}finally{owner.pbuf_free(packet);}}
    static void Exchange(Peer p,Lwip owner)
    {
        ip_addr local=default,remote=default;fixed(byte* a="10.7.0.1\0"u8,b="10.7.0.2\0"u8)Require(owner.ipaddr_aton(a,&local)==1&&owner.ipaddr_aton(b,&remote)==1,"PPPoE IP configuration");udp_pcb* udp=owner.udp_new();Require(udp!=null&&owner.udp_bind(udp,&local,9000)==0,"PPPoE UDP bind");owner.udp_recv(udp,&Received,(void*)GCHandle.ToIntPtr(p.Root));
        byte[] frame=Header(0,0x1234,true);Put16(frame,20,0x21);frame[22]=0x45;Put16(frame,24,101);frame[30]=64;frame[31]=17;frame[34]=frame[38]=10;frame[35]=frame[39]=7;frame[37]=2;frame[41]=1;Put16(frame,32,Checksum(frame.AsSpan(22,20)));Put16(frame,42,9001);Put16(frame,44,9000);Put16(frame,46,81);for(int i=0;i<73;i++)frame[50+i]=(byte)(i*7);Put16(frame,18,103);Inject(p,owner,frame,123,true);Require(p.Datagrams==1,"independently encoded UDP ingress");
        int start=p.Frames.Count;pbuf* packet=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,73,pbuf_type.PBUF_RAM);Require(packet!=null,"UDP response allocation");fixed(byte* bytes=frame)Require(owner.pbuf_take(packet,bytes+50,73)==0,"UDP response copy");Require(owner.udp_sendto_if_src(udp,packet,&remote,9001,p.PppInterface,&local)==0,"PPPoE UDP response");owner.pbuf_free(packet);Require(p.Frames.Count==start+1,"one UDP response frame");byte[] wire=p.Frames[start];Require(U16(wire,12)==0x8864&&U16(wire,16)==0x1234&&U16(wire,20)==0x21&&wire[31]==17&&U16(wire,42)==9000&&U16(wire,44)==9001&&wire.AsSpan(50,73).SequenceEqual(frame.AsSpan(50,73)),"independent UDP egress decode");owner.udp_remove(udp);
    }
    static void Case(Peer p,Lwip owner)
    {
        uint memory=owner.Globals.lwip_stats.mem.used,pool=owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used,timers=owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_SYS_TIMEOUT].Value->used;
        p.Interface->hwaddr_len=6;p.Interface->hwaddr[0]=2;p.Interface->hwaddr[5]=1;p.Interface->mtu=1500;p.Interface->linkoutput=&Output;
        // Names remain pinned for the complete session, including PADR retries.
        fixed(byte* service="service\0"u8,concentrator="concentrator\0"u8)
        {
            p.Session=owner.pppoe_create(p.PppInterface,p.Interface,service,concentrator,&Status,(void*)GCHandle.ToIntPtr(p.Root));Require(p.Session!=null,"PPPoE create");ip4_addr local=default,remote=default;fixed(byte* a="10.7.0.1\0"u8,b="10.7.0.2\0"u8)Require(owner.ip4addr_aton(a,&local)==1&&owner.ip4addr_aton(b,&remote)==1,"PPP addresses");p.Session->ipcp_wantoptions.ouraddr=local.addr;p.Session->ask_for_local=1;p.Session->ipcp_wantoptions.hisaddr=remote.addr;
            Require(owner.ppp_connect(p.Session,0)==0&&p.Frames.Count==1,"PADI starts discovery");byte[] padi=p.Frames[0];Require(U16(padi,12)==0x8863&&padi[15]==9&&U16(padi,16)==0,"PADI wire");p.Unique=Tag(padi,0x0103);Require(p.Unique.Length>0&&p.Unique.Length<=32,"host unique token");Require(Tag(padi,0x0101).AsSpan().SequenceEqual("service"u8)&&Tag(padi,0x0102).AsSpan().SequenceEqual("concentrator"u8),"service and concentrator names");
            byte[] frame=Header(7,0,false);int length=Append(frame,20,0x0101,"service"u8);length=Append(frame,length,0x0102,"concentrator"u8);length=Append(frame,length,0x0104,"cookie"u8);length=Append(frame,length,0x0103,p.Unique);Put16(frame,18,length-20);Inject(p,owner,frame,length,false);Require(p.Frames.Count==2&&p.Frames[1][15]==0x19&&Tag(p.Frames[1],0x0104).AsSpan().SequenceEqual("cookie"u8),"PADR echoes cookie");
            frame=Header(0x65,0x1234,false);length=Append(frame,20,0x0103,p.Unique);Put16(frame,18,length-20);Inject(p,owner,frame,length,false);Require(p.Frames.Count==3,"PADS begins LCP");byte[] lcp=p.Frames[2];Require(U16(lcp,12)==0x8864&&U16(lcp,16)==0x1234&&U16(lcp,20)==0xc021&&lcp[22]==1,"LCP request in session");int offered=U16(lcp,24);Response(p,owner,0xc021,4,lcp[23],lcp.AsSpan(26,offered-4));Require(p.Frames.Count>=4&&p.Frames[^1][22]==1&&p.Frames[^1][23]!=lcp[23],"LCP reject makes new request");
            Negotiate(p,owner);Exchange(p,owner);Require(owner.ppp_close(p.Session,0)==0&&p.Frames[^1][22]==5,"LCP termination request");Response(p,owner,0xc021,6,p.Frames[^1][23],[]);Require(p.Session->phase==0&&p.Frames[^1][15]==0xa7&&U16(p.Frames[^1],16)==0x1234,"termination acknowledgement produces PADT");owner.netif_poll(p.PppInterface);Require(owner.ppp_free(p.Session)==0,"PPPoE release");p.Session=null;
        }
        Require(owner.Globals.lwip_stats.mem.used==memory&&owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used==pool&&owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_SYS_TIMEOUT].Value->used==timers,"PPPoE heap/pbuf/timer baseline");
    }
    public static void Run()
    {
        var p=new Peer();Active=p;p.Interface=(netif*)NativeMemory.AllocZeroed((nuint)sizeof(netif));p.PppInterface=(netif*)NativeMemory.AllocZeroed((nuint)sizeof(netif));p.Root=GCHandle.Alloc(p);
        try{p.Stack.Start();p.Stack.Invoke(owner=>Case(p,owner));if(p.Error!=null)throw p.Error;}
        finally{p.Stack.Dispose();p.Root.Free();NativeMemory.Free(p.Interface);NativeMemory.Free(p.PppInterface);Active=null;}
        Console.WriteLine("PASS ppp-ethernet: independent discovery/service/cookie peer, LCP+IPCP+IPv6CP negotiation, bidirectional UDP, termination/PADT and resource baseline");
    }
}

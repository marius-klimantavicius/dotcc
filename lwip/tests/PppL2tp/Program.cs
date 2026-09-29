using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    static void Require(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    sealed class Peer
    {
        public readonly LwipStack Stack=new();
        public readonly List<byte[]> Frames=[],Packets=[];
        public netif* Outer;
        public netif* Inner;
        public ppp_pcb_s* Session;
        public int Tunnel,SessionId,Port,Up,Error,Datagrams;
        public Exception? Failure;
        public GCHandle Root;
    }
    static Peer? Active;
    static int U16(byte[] bytes,int at)=>(bytes[at]<<8)|bytes[at+1];
    static void Put16(byte[] bytes,int at,int value){bytes[at]=(byte)(value>>8);bytes[at+1]=(byte)value;}
    static int Checksum(ReadOnlySpan<byte> bytes)
    {uint sum=0;for(int i=0;i<bytes.Length;i+=2)sum+=(uint)((bytes[i]<<8)|(i+1<bytes.Length?bytes[i+1]:0));while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return (int)(~sum&65535);}
    static sbyte Output(Lwip owner,netif* iface,pbuf* packet,ip4_addr* address)
    {
        var p=Active!;try{Require(ReferenceEquals(owner,p.Stack.Core)&&iface==p.Outer&&p.Frames.Count<256&&packet->tot_len<=2048,"L2TP output identity/bounds");byte[] bytes=new byte[packet->tot_len];fixed(byte* target=bytes)Require(owner.pbuf_copy_partial(packet,target,packet->tot_len,0)==packet->tot_len,"complete outer frame");p.Frames.Add(bytes);if(bytes.Length>=36&&U16(bytes,28)==2){int at=34;if(U16(bytes,at)==0xff03)at+=2;p.Packets.Add(bytes[at..]);}return 0;}catch(Exception error){p.Failure=error;return -1;}
    }
    static sbyte Initialize(Lwip owner,netif* iface){iface->mtu=1500;iface->output=&Output;return 0;}
    static sbyte Input(Lwip owner,pbuf* packet,netif* iface)=>owner.ip_input(packet,iface);
    static void Status(Lwip owner,ppp_pcb_s* pcb,int error,void* argument)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;Require(ReferenceEquals(owner,p.Stack.Core)&&pcb==p.Session,"L2TP status owner");p.Error=error;if(error==0)p.Up++;}
    static byte[] Avp(Peer p,int index,int type)
    {byte[] f=p.Frames[index];Require(f.Length>=40&&U16(f,28)==0xc802&&U16(f,30)+28<=f.Length,"RFC2661 control bounds");int end=28+U16(f,30);for(int at=40;at+6<=end;){int length=U16(f,at)&1023;Require(length>=6&&at+length<=end&&U16(f,at+2)==0,"AVP bounds/vendor");if(U16(f,at+4)==type)return f.AsSpan(at+6,length-6).ToArray();at+=length;}throw new InvalidOperationException("Missing L2TP AVP");}
    static int Attribute(byte[] frame,int at,int type,ReadOnlySpan<byte> payload)
    {Put16(frame,at,0x8000|(payload.Length+6));Put16(frame,at+2,0);Put16(frame,at+4,type);payload.CopyTo(frame.AsSpan(at+6));return at+6+payload.Length;}
    static byte[] Control(Peer p,int message,int ns,int nr,out int length)
    {byte[] frame=new byte[2048];Put16(frame,0,0xc802);Put16(frame,4,p.Tunnel);Put16(frame,6,p.SessionId);Put16(frame,8,ns);Put16(frame,10,nr);length=12;if(message!=0){byte[] value=new byte[2];Put16(value,0,message);length=Attribute(frame,length,0,value);}return frame;}
    static void Inject(Peer p,Lwip owner,byte[] l2tp,int length)
    {byte[] frame=new byte[length+28];frame[0]=0x45;Put16(frame,2,frame.Length);frame[8]=64;frame[9]=17;frame[12]=frame[16]=10;frame[13]=frame[17]=8;frame[15]=2;frame[19]=1;Put16(frame,10,Checksum(frame.AsSpan(0,20)));Put16(frame,20,1701);Put16(frame,22,p.Port);Put16(frame,24,length+8);l2tp.AsSpan(0,length).CopyTo(frame.AsSpan(28));pbuf* packet=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)frame.Length,pbuf_type.PBUF_RAM);Require(packet!=null,"L2TP injection allocation");fixed(byte* bytes=frame)Require(owner.pbuf_take(packet,bytes,(ushort)frame.Length)==0,"L2TP injection copy");Require(owner.ip_input(packet,p.Outer)==0,"outer IP ingress");if(p.Failure!=null)throw p.Failure;}
    static byte[] Digest(int id,byte[] challenge)
    {byte[] bytes=new byte[challenge.Length+7];bytes[0]=(byte)id;"secret"u8.CopyTo(bytes.AsSpan(1));challenge.CopyTo(bytes,7);return MD5.HashData(bytes);}
    static void Data(Peer p,Lwip owner,int protocol,ReadOnlySpan<byte> payload,int bad=0)
    {byte[] frame=new byte[payload.Length+8];Put16(frame,0,2);Put16(frame,2,p.Tunnel^(bad==1?1:0));Put16(frame,4,p.SessionId^(bad==2?1:0));Put16(frame,6,protocol);payload.CopyTo(frame.AsSpan(8));Inject(p,owner,frame,frame.Length);}
    static void Response(Peer p,Lwip owner,int protocol,int code,int id,ReadOnlySpan<byte> payload)
    {byte[] frame=new byte[payload.Length+4];frame[0]=(byte)code;frame[1]=(byte)id;Put16(frame,2,frame.Length);payload.CopyTo(frame.AsSpan(4));Data(p,owner,protocol,frame);}
    static void Negotiate(Peer p,Lwip owner)
    {
        int sent=0;
        for(int index=0;index<p.Packets.Count;index++)
        {
            Require(index<200,"bounded inner PPP negotiation");byte[] frame=p.Packets[index];int protocol=U16(frame,0),code=frame[2],id=frame[3],length=U16(frame,4);Require(length>=4&&length+2<=frame.Length,"inner PPP control bounds");if(code!=1)continue;
            if(protocol==0xc021){Response(p,owner,protocol,2,id,frame.AsSpan(6,length-4));if((sent&1)==0){sent|=1;Response(p,owner,protocol,1,90,[]);}}
            else if(protocol==0x8021)
            {
                List<byte> rejected=[];for(int at=6;at<2+length;){int n=frame[at+1];Require(n>=2&&at+n<=2+length,"IPCP option bounds");if(frame[at]==2)rejected.AddRange(frame.AsSpan(at,n).ToArray());at+=n;}
                if(rejected.Count!=0){Response(p,owner,protocol,4,id,CollectionsMarshal.AsSpan(rejected));continue;}
                Response(p,owner,protocol,2,id,frame.AsSpan(6,length-4));if((sent&2)==0){sent|=2;Response(p,owner,protocol,1,91,[3,6,10,7,0,2]);}
            }
            else if(protocol==0x8057){Response(p,owner,protocol,2,id,frame.AsSpan(6,length-4));if((sent&4)==0){sent|=4;Response(p,owner,protocol,1,92,[1,10,0xee,0xdd,0xcc,0xbb,0xaa,0x99,0x88,2]);}}
            else if(protocol==0x80fd)Response(p,owner,protocol,4,id,frame.AsSpan(6,length-4));
        }
        Require(sent==7&&p.Up==2&&p.Session->phase==10,"authenticated tunnel runs both IP families");
    }
    static void Received(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;try{Require(ReferenceEquals(owner,p.Stack.Core)&&port==9001&&address->type==0&&packet->tot_len==73,"inner UDP peer");byte* bytes=stackalloc byte[73];Require(owner.pbuf_copy_partial(packet,bytes,73,0)==73,"inner UDP complete bytes");for(int i=0;i<73;i++)Require(bytes[i]==(byte)(i*7),"inner UDP data");p.Datagrams++;}catch(Exception error){p.Failure=error;}finally{owner.pbuf_free(packet);}}
    static void Exchange(Peer p,Lwip owner)
    {
        ip_addr local=default,remote=default;fixed(byte* a="10.7.0.1\0"u8,b="10.7.0.2\0"u8)Require(owner.ipaddr_aton(a,&local)==1&&owner.ipaddr_aton(b,&remote)==1,"inner IP addresses");udp_pcb* udp=owner.udp_new();Require(udp!=null&&owner.udp_bind(udp,&local,9000)==0,"inner UDP listener");owner.udp_recv(udp,&Received,(void*)GCHandle.ToIntPtr(p.Root));
        byte[] ip=new byte[101];ip[0]=0x45;Put16(ip,2,101);ip[8]=64;ip[9]=17;ip[12]=ip[16]=10;ip[13]=ip[17]=7;ip[15]=2;ip[19]=1;Put16(ip,10,Checksum(ip.AsSpan(0,20)));Put16(ip,20,9001);Put16(ip,22,9000);Put16(ip,24,81);for(int i=0;i<73;i++)ip[28+i]=(byte)(i*7);
        Data(p,owner,0x21,ip,1);Require(p.Datagrams==0,"wrong tunnel rejects data");Data(p,owner,0x21,ip,2);Require(p.Datagrams==0,"wrong session rejects data");Data(p,owner,0x21,ip);Require(p.Datagrams==1,"actual authenticated PPP UDP ingress");
        int start=p.Packets.Count;pbuf* packet=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,73,pbuf_type.PBUF_RAM);Require(packet!=null,"inner UDP output pbuf");fixed(byte* bytes=ip)Require(owner.pbuf_take(packet,bytes+28,73)==0,"inner output bytes");Require(owner.udp_sendto_if_src(udp,packet,&remote,9001,p.Inner,&local)==0,"inner UDP output");owner.pbuf_free(packet);Require(p.Packets.Count==start+1,"one inner UDP frame");byte[] wire=p.Packets[start];Require(U16(wire,0)==0x21&&wire[11]==17&&U16(wire,22)==9000&&U16(wire,24)==9001&&wire.AsSpan(30,73).SequenceEqual(ip.AsSpan(28,73)),"independent tunnel UDP egress decode");owner.udp_remove(udp);
    }
    static void Case(Peer p,Lwip owner,bool wrong)
    {
        uint memory=owner.Globals.lwip_stats.mem.used,pool=owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used,timers=owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_SYS_TIMEOUT].Value->used;
        ip4_addr local=default,mask=default,gateway=default;fixed(byte* a="10.8.0.1\0"u8,b="255.255.255.0\0"u8)Require(owner.ip4addr_aton(a,&local)==1&&owner.ip4addr_aton(b,&mask)==1,"outer addresses");Require(owner.netif_add(p.Outer,&local,&mask,&gateway,null,&Initialize,&Input)==p.Outer,"outer interface");owner.netif_set_up(p.Outer);owner.netif_set_link_up(p.Outer);
        ip_addr remote=default;fixed(byte* a="10.8.0.2\0"u8)Require(owner.ipaddr_aton(a,&remote)==1,"LNS address");
        fixed(byte* secret="secret"u8)
        {
            p.Session=owner.pppol2tp_create(p.Inner,p.Outer,&remote,1701,secret,6,&Status,(void*)GCHandle.ToIntPtr(p.Root));Require(p.Session!=null,"L2TP allocation");ip4_addr il=default,ir=default;fixed(byte* a="10.7.0.1\0"u8,b="10.7.0.2\0"u8)Require(owner.ip4addr_aton(a,&il)==1&&owner.ip4addr_aton(b,&ir)==1,"PPP addresses");p.Session->ipcp_wantoptions.ouraddr=il.addr;p.Session->ask_for_local=1;p.Session->ipcp_wantoptions.hisaddr=ir.addr;Require(owner.ppp_connect(p.Session,0)==0&&p.Frames.Count==1,"SCCRQ starts tunnel");p.Port=U16(p.Frames[0],20);Require(U16(Avp(p,0,0),0)==1,"SCCRQ message");p.Tunnel=U16(Avp(p,0,9),0);byte[] challenge=Avp(p,0,11);Require(challenge.Length==16,"client challenge");byte[] digest=Digest(2,challenge);if(wrong)digest[0]^=1;
            byte[] peerChallenge=Enumerable.Range(1,16).Select(i=>(byte)i).ToArray();byte[] frame=Control(p,2,0,1,out int length);length=Attribute(frame,length,9,[0x11,0x22]);length=Attribute(frame,length,13,digest);length=Attribute(frame,length,11,peerChallenge);Put16(frame,2,length);Inject(p,owner,frame,length);
            if(wrong)Require(p.Session->phase==0&&p.Error!=0,"wrong tunnel challenge aborts connection");
            else
            {
                Require(p.Frames.Count==3&&U16(Avp(p,1,0),0)==3&&Avp(p,1,13).AsSpan().SequenceEqual(Digest(3,peerChallenge))&&U16(Avp(p,2,0),0)==10,"mutual authentication SCCCN and ICRQ");p.SessionId=U16(Avp(p,2,14),0);frame=Control(p,11,1,3,out length);length=Attribute(frame,length,14,[0x33,0x44]);Put16(frame,2,length);Inject(p,owner,frame,length);Require(p.Frames.Count==4&&U16(Avp(p,3,0),0)==12,"ICCN connected");frame=Control(p,0,2,4,out length);Put16(frame,2,length);Inject(p,owner,frame,length);Require(p.Frames.Count==5&&U16(p.Frames[4],28)==2&&U16(p.Frames[4],30)==0x1122&&U16(p.Frames[4],32)==0x3344&&p.Session->phase==6,"ZLB acknowledgment starts PPP data");
                Negotiate(p,owner);Exchange(p,owner);Require(owner.ppp_close(p.Session,0)==0,"PPP close in tunnel");byte[] last=p.Packets[^1];Require(U16(last,0)==0xc021&&last[2]==5,"LCP terminate on data channel");Response(p,owner,0xc021,6,last[3],[]);Require(p.Session->phase==0&&U16(Avp(p,p.Frames.Count-1,0),0)==4,"LCP TermAck sends StopCCN");
            }
            owner.netif_poll(p.Inner);Require(owner.ppp_free(p.Session)==0,"L2TP free");p.Session=null;
        }
        owner.netif_set_down(p.Outer);owner.netif_remove(p.Outer);Require(owner.Globals.lwip_stats.mem.used==memory&&owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used==pool&&owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_SYS_TIMEOUT].Value->used==timers,"L2TP resource baselines");
    }
    static void Main()
    {
        for(int wrong=0;wrong<2;wrong++)
        {
            var p=new Peer();Active=p;p.Outer=(netif*)NativeMemory.AllocZeroed((nuint)sizeof(netif));p.Inner=(netif*)NativeMemory.AllocZeroed((nuint)sizeof(netif));p.Root=GCHandle.Alloc(p);
            try{p.Stack.Start();p.Stack.Invoke(owner=>Case(p,owner,wrong!=0));if(p.Failure!=null)throw p.Failure;}
            finally{p.Stack.Dispose();p.Root.Free();NativeMemory.Free(p.Outer);NativeMemory.Free(p.Inner);Active=null;}
        }
        Console.WriteLine("PASS ppp-l2tp: independent mutual/wrong tunnel challenge, IPCP/IPv6CP, UDP both directions, invalid session IDs, StopCCN and resource baselines");
    }
}

using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Managed.Networking;
using static Managed.Networking.Lwip;

// Positive independent peer: complete legal packets, prepared once, never mutated
// after injection. Reordering only changes delivery order of whole data segments.
static unsafe class Program
{
    static void R(bool value,string name){if(!value)throw new InvalidOperationException(name+"; errno="+Libc.errno);}
    static ushort Swap(ushort value)=>BinaryPrimitives.ReverseEndianness(value);
    static uint Word(byte[] p,int at)=>BinaryPrimitives.ReadUInt16BigEndian(p.AsSpan(at));
    static uint Dword(byte[] p,int at)=>BinaryPrimitives.ReadUInt32BigEndian(p.AsSpan(at));
    static void Put16(byte[] p,int at,uint value)=>BinaryPrimitives.WriteUInt16BigEndian(p.AsSpan(at),(ushort)value);
    static void Put32(byte[] p,int at,uint value)=>BinaryPrimitives.WriteUInt32BigEndian(p.AsSpan(at),value);
    static uint Sum(ReadOnlySpan<byte> p,uint value=0){for(int i=0;i<p.Length;i+=2)value+=(uint)((p[i]<<8)|(i+1<p.Length?p[i+1]:0));while((value>>16)!=0)value=(value&65535)+(value>>16);return value;}
    static byte[] Packet(ushort destination,uint seq,uint ack,byte flags,uint timestamp,uint echo,ReadOnlySpan<byte> data,bool syn)
    {
        int options=syn?24:12,tcpLength=20+options+data.Length;byte[] p=new byte[20+tcpLength];p[0]=0x45;Put16(p,2,(uint)p.Length);p[8]=64;p[9]=6;p[12]=p[16]=10;p[13]=p[17]=44;p[15]=2;p[19]=1;Put16(p,10,~Sum(p.AsSpan(0,20)));
        Put16(p,20,50001);Put16(p,22,destination);Put32(p,24,seq);Put32(p,28,ack);p[32]=(byte)(((20+options)/4)<<4);p[33]=flags;Put16(p,34,32768);int at=40;
        if(syn){p[at++]=2;p[at++]=4;Put16(p,at,1440);at+=2;p[at++]=4;p[at++]=2;p[at++]=1;p[at++]=3;p[at++]=3;p[at++]=2;p[at++]=1;p[at++]=1;}
        p[at++]=1;p[at++]=1;p[at++]=8;p[at++]=10;Put32(p,at,timestamp);at+=4;Put32(p,at,echo);at+=4;R(at==40+options,"coherent TCP options");data.CopyTo(p.AsSpan(at));Put16(p,36,~Sum(p.AsSpan(20),Sum(p.AsSpan(12,8),(uint)(6+tcpLength))));return p;
    }
    static int Option(byte[] p,int wanted){R(p.Length>=40&&p[9]==6,"TCP output header");int end=20+(p[32]>>4)*4;R(end<=p.Length,"TCP options bounds");for(int at=40;at<end;){int kind=p[at];if(kind==0)break;if(kind==1){at++;continue;}R(at+1<end&&p[at+1]>=2&&at+p[at+1]<=end,"TCP output option shape");if(kind==wanted)return at;at+=p[at+1];}return -1;}
    static ip_addr Ip4(uint networkHostOrder){ip_addr value=default;value.u_addr.ip4.addr=BinaryPrimitives.ReverseEndianness(networkHostOrder);return value;}
    sealed class Peer:IDisposable
    {
        internal readonly LwipStack Stack;readonly StreamWriter wire;GCHandle root;nint iface,listener,accepted;Exception? callbackError;internal readonly List<byte[]> Frames=new();readonly List<byte> received=new();int removed,reused,timed,canceled;uint clock=1000;readonly JsonElement native; (uint,uint,uint,uint,uint,uint,uint) baseline;
        internal Peer(LwipStack stack,JsonElement native,string path)
        {
            Stack=stack;this.native=native;wire=new StreamWriter(path);root=GCHandle.Alloc(this);clock=LwipSysArch.Now(stack.Core);LwipSysArch.SetClock(stack.Core,()=>Volatile.Read(ref clock));
            stack.Invoke(core=>{baseline=Usage(core);iface=(nint)NativeMemory.AllocZeroed((nuint)sizeof(netif));ip4_addr ip=new(){addr=0x01002c0a},mask=new(){addr=0x00ffffff},zero=default;R(core.netif_add((netif*)iface,&ip,&mask,&zero,(void*)GCHandle.ToIntPtr(root),&Init,&Input)!=null,"netif add");core.netif_set_up((netif*)iface);core.netif_set_link_up((netif*)iface);core.netif_set_default((netif*)iface);core.netif_set_remove_callback((netif*)iface,&Remove);});
        }
        static (uint,uint,uint,uint,uint,uint,uint) Usage(Lwip core)=>(core.Globals.lwip_stats.mem.used,core.Globals.memp_PBUF_POOL.stats->used,core.Globals.memp_TCP_PCB.stats->used,core.Globals.memp_UDP_PCB.stats->used,core.Globals.memp_NETCONN.stats->used,core.Globals.memp_NETBUF.stats->used,core.Globals.memp_TCP_PCB_LISTEN.stats->used);
        static Peer State(netif* n)=>(Peer)GCHandle.FromIntPtr((nint)n->state).Target!;
        static Peer Argument(void* p)=>(Peer)GCHandle.FromIntPtr((nint)p).Target!;
        static sbyte Init(Lwip core,netif* n){n->mtu=1500;n->flags=(byte)NETIF_FLAG_BROADCAST;n->output=&Output;return 0;}
        static sbyte Input(Lwip core,pbuf* p,netif* n)=>core.ip_input(p,n);
        static sbyte Output(Lwip core,netif* n,pbuf* p,ip4_addr* target)
        {
            var self=State(n);try{byte[] bytes=new byte[p->tot_len];fixed(byte* data=bytes)R(core.pbuf_copy_partial(p,data,p->tot_len,0)==p->tot_len,"output copy");self.Record("stack",bytes);self.Frames.Add(bytes);return 0;}catch(Exception error){self.callbackError=error;return (sbyte)err_enum_t.ERR_IF;}
        }
        static void Remove(Lwip core,netif* n){var self=State(n);try{R((nint)n==self.iface,"remove callback owner");self.removed++;}catch(Exception error){self.callbackError=error;}}
        void Record(string direction,byte[] bytes){wire.WriteLine(direction+" "+Convert.ToHexString(bytes).ToLowerInvariant());wire.Flush();}
        void Inject(Lwip core,byte[] bytes){Record("peer",bytes);var p=core.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)bytes.Length,pbuf_type.PBUF_RAM);R(p!=null,"input allocation");fixed(byte* data=bytes)R(core.pbuf_take(p,data,(ushort)bytes.Length)==0,"input copy");R(core.ip_input(p,(netif*)iface)==0,"original IP input");Check();}
        void Check(){if(callbackError!=null)throw new InvalidOperationException("enhanced callback",callbackError);}
        static sbyte Accept(Lwip core,void* arg,tcp_pcb* pcb,sbyte error)
        {
            var self=Argument(arg);try{R(error==0,"accept status");self.accepted=(nint)pcb;core.tcp_arg(pcb,arg);core.tcp_recv(pcb,&Receive);core.tcp_backlog_delayed(pcb);return 0;}catch(Exception failure){self.callbackError=failure;return (sbyte)err_enum_t.ERR_ABRT;}
        }
        static sbyte Receive(Lwip core,void* arg,tcp_pcb* pcb,pbuf* p,sbyte error)
        {
            var self=Argument(arg);try{R(error==0&&p!=null,"receive metadata");byte[] bytes=new byte[p->tot_len];fixed(byte* data=bytes)R(core.pbuf_copy_partial(p,data,p->tot_len,0)==p->tot_len,"receive copy");self.received.AddRange(bytes);core.tcp_recved(pcb,p->tot_len);return 0;}catch(Exception failure){self.callbackError=failure;return (sbyte)err_enum_t.ERR_ABRT;}finally{if(p!=null)core.pbuf_free(p);}
        }
        internal void TcpOptions()=>Stack.Invoke(core=>
        {
            var local=Ip4(0x0a2c0001);var pcb=core.tcp_new();R(pcb!=null&&core.tcp_bind(pcb,&local,50000)==0,"listener bind");listener=(nint)core.tcp_listen_with_backlog(pcb,1);R(listener!=0,"listen backlog");core.tcp_arg((tcp_pcb*)listener,(void*)GCHandle.ToIntPtr(root));core.tcp_accept((tcp_pcb*)listener,&Accept);
            const uint seq=0x10203040;Frames.Clear();Inject(core,Packet(50000,seq,0,2,100,0,[],true));R(Frames.Count==1&&(Frames[0][33]&0x12)==0x12,"SYNACK");var syn=Frames[0];int ws=Option(syn,3),ts=Option(syn,8),sack=Option(syn,4);R(ws>=0&&syn[ws+1]==3&&syn[ws+2]==native.GetProperty("native_rcv_scale").GetByte(),"native window scale");R(ts>=0&&syn[ts+1]==10&&Dword(syn,ts+6)==100&&sack>=0&&syn[sack+1]==2,"timestamp and SACK negotiation");uint serverSeq=Dword(syn,24),serverTime=Dword(syn,ts+2);
            Inject(core,Packet(50000,seq+1,serverSeq+1,16,101,serverTime,[],false));var child=(tcp_pcb*)accepted;R(child!=null&&(child->flags&TF_TIMESTAMP)!=0&&(child->flags&TF_WND_SCALE)!=0&&(child->flags&TF_SACK)!=0,"negotiated PCB options");R(child->snd_scale==2&&child->rcv_scale==native.GetProperty("native_rcv_scale").GetByte()&&child->rcv_wnd==native.GetProperty("native_tcp_window").GetUInt32(),"native PCB window values");R(((tcp_pcb_listen*)listener)->accepts_pending==1,"deferred accepted backlog");core.tcp_backlog_accepted(child);R(((tcp_pcb_listen*)listener)->accepts_pending==0,"accepted backlog released");
            byte[] first=Packet(50000,seq+1,serverSeq+1,24,102,serverTime,"abcd"u8,false),later=Packet(50000,seq+5,serverSeq+1,24,102,serverTime,"efgh"u8,false);Frames.Clear();Inject(core,later);R(received.Count==0&&child->ooseq!=null&&Frames.Count>0,"valid later segment held");sack=Option(Frames[^1],5);R(sack>=0&&Frames[^1][sack+1]==10&&Dword(Frames[^1],sack+2)==seq+5&&Dword(Frames[^1],sack+6)==seq+9,"exact emitted SACK block");Inject(core,first);R(received.SequenceEqual("abcdefgh"u8.ToArray())&&child->ooseq==null,"unchanged complete earlier segment drains queue");core.tcp_abort(child);accepted=0;R(core.tcp_close((tcp_pcb*)listener)==0,"listener close");listener=0;Check();
        });
        static void ReuseReceive(Lwip core,void* arg,udp_pcb* pcb,pbuf* p,ip_addr* address,ushort port)
        {
            var self=Argument(arg);try{R(address->type==0&&port==50001&&p->tot_len==4,"reuse metadata");byte* data=stackalloc byte[4];R(core.pbuf_copy_partial(p,data,4,0)==4&&new ReadOnlySpan<byte>(data,4).SequenceEqual("copy"u8),"reuse complete payload");self.reused++;}catch(Exception failure){self.callbackError=failure;}finally{core.pbuf_free(p);}
        }
        internal void UdpReuse()=>Stack.Invoke(core=>
        {
            var a=core.udp_new();var b=core.udp_new();R(a!=null&&b!=null,"reuse allocate");try{a->so_options|=(byte)SOF_REUSEADDR;b->so_options|=(byte)SOF_REUSEADDR;ip_addr any=default;R(core.udp_bind(a,&any,51000)==0&&core.udp_bind(b,&any,51000)==0,"reuse bind");core.udp_recv(a,&ReuseReceive,(void*)GCHandle.ToIntPtr(root));core.udp_recv(b,&ReuseReceive,(void*)GCHandle.ToIntPtr(root));byte[] p=new byte[32];p[0]=0x45;Put16(p,2,32);p[8]=64;p[9]=17;p[12]=p[16]=10;p[13]=p[17]=44;p[15]=2;p[19]=255;Put16(p,10,~Sum(p.AsSpan(0,20)));Put16(p,20,50001);Put16(p,22,51000);Put16(p,24,12);"copy"u8.CopyTo(p.AsSpan(28));Put16(p,26,~Sum(p.AsSpan(20),Sum(p.AsSpan(12,8),29)));Inject(core,p);R(reused==2,"both reusable PCBs receive broadcast");}finally{core.udp_remove(a);core.udp_remove(b);}
        });
        internal void Linger()
        {
            var owner=Stack.Core;int fd=owner.lwip_socket(AF_INET,SOCK_STREAM,0);R(fd>=0,"linger socket");Thread? thread=null;Exception? failure=null;int connected=-99;
            try
            {
                linger value=new(){l_onoff=1,l_linger=0},got=default;uint size=(uint)sizeof(linger);R(owner.lwip_setsockopt(fd,SOL_SOCKET,SO_LINGER,&value,size)==0&&owner.lwip_getsockopt(fd,SOL_SOCKET,SO_LINGER,&got,&size)==0&&got.l_onoff==1&&got.l_linger==0,"linger roundtrip");Stack.Invoke(_=>Frames.Clear());
                thread=new Thread(()=>{using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();try{sockaddr_in target=new(){sin_len=(byte)sizeof(sockaddr_in),sin_family=AF_INET,sin_port=Swap(50001),sin_addr=new(){s_addr=0x02002c0a}};connected=owner.lwip_connect(fd,(sockaddr*)&target,(uint)sizeof(sockaddr_in));}catch(Exception error){failure=error;}finally{owner.lwip_socket_thread_cleanup();}}){IsBackground=true};thread.Start();var elapsed=Stopwatch.StartNew();
                while(true){bool ready=false;Stack.Invoke(_=>ready=Frames.Count>0);if(ready)break;if(failure!=null)throw failure;R(elapsed.Elapsed<TimeSpan.FromSeconds(5),"socket SYN bounded wait");Thread.Yield();}
                Stack.Invoke(core=>{var syn=Frames[0];int ts=Option(syn,8);R(ts>=0&&(syn[33]&2)!=0,"socket SYN timestamp");Inject(core,Packet((ushort)Word(syn,20),0x20406080,Dword(syn,24)+1,18,500,Dword(syn,ts+2),[],true));});R(thread.Join(TimeSpan.FromSeconds(5)),"connect completed");if(failure!=null)throw failure;R(connected==0,"connect result");byte* bytes=stackalloc byte[14];"unacknowledged"u8.CopyTo(new Span<byte>(bytes,14));R(owner.lwip_send(fd,bytes,14,0)==14,"linger unacknowledged send");Stack.Invoke(core=>{var socket=core.lwip_socket_dbg_get_socket(fd);R(socket!=null&&socket->conn->pcb.tcp->unacked!=null,"actual unacknowledged original segment");});R(owner.lwip_close(fd)==0,"linger close");fd=-1;Stack.Invoke(_=>R(Frames.Any(p=>p[9]==6&&(p[33]&4)!=0),"original linger emits reset"));
            }
            finally{if(fd>=0)owner.lwip_close(fd);if(thread is {IsAlive:true})R(thread.Join(TimeSpan.FromSeconds(5)),"connect thread cleanup");}
        }
        static void Timer(Lwip core,void* argument){var self=Argument(argument);try{LwipSysArch.CheckCore(core);self.timed++;}catch(Exception error){self.callbackError=error;}}
        static void CanceledTimer(Lwip core,void* argument){var self=Argument(argument);self.canceled++;}
        internal void Timers()
        {
            var core=Stack.Core;void* arg=(void*)GCHandle.ToIntPtr(root);R(core.tcpip_timeout(0,&Timer,arg)==0&&core.tcpip_timeout(20,&CanceledTimer,arg)==0&&core.tcpip_untimeout(&CanceledTimer,arg)==0,"original timeout mailbox posting");Stack.Invoke(owner=>{Volatile.Write(ref clock,clock+100);owner.sys_check_timeouts();});Stack.Invoke(_=>{Check();R(timed==1&&canceled==0,"deterministic execution and cancellation");});
        }
        public void Dispose()
        {
            if(iface==0)return;Stack.Invoke(core=>{if(accepted!=0){core.tcp_abort((tcp_pcb*)accepted);accepted=0;}if(listener!=0){core.tcp_close((tcp_pcb*)listener);listener=0;}core.netif_set_down((netif*)iface);core.netif_remove((netif*)iface);});Stack.Invoke(core=>{Check();R(removed==1,"remove exactly once");R(Usage(core)==baseline,"natural heap/pbuf/TCP/UDP PCB/netconn/netbuf/listener baseline");});NativeMemory.Free((void*)iface);iface=0;root.Free();wire.Dispose();
        }
    }
    static void UdpInfo(Lwip owner,JsonElement native)
    {
        R(sizeof(in_pktinfo)==native.GetProperty("pktinfo_size").GetInt32()&&sizeof(cmsghdr)==native.GetProperty("cmsghdr_size").GetInt32()&&sizeof(netbuf)==native.GetProperty("netbuf_size").GetInt32(),"native-derived metadata struct sizes");netbuf sample=default;R((byte*)&sample.toaddr-(byte*)&sample==native.GetProperty("netbuf_toaddr_offset").GetInt32(),"native netbuf destination offset");sockaddr_in address=new(){sin_len=(byte)sizeof(sockaddr_in),sin_family=AF_INET,sin_port=Swap(52000),sin_addr=new(){s_addr=0x0100007f}};int fd=owner.lwip_socket(AF_INET,SOCK_DGRAM,0);R(fd>=0,"metadata socket");
        try
        {
            R(owner.lwip_bind(fd,(sockaddr*)&address,(uint)sizeof(sockaddr_in))==0,"metadata bind");int yes=1;R(owner.lwip_setsockopt(fd,IPPROTO_IP,IP_PKTINFO,&yes,4)==0,"IP_PKTINFO enable");timeval bound=new(){tv_sec=5};R(owner.lwip_setsockopt(fd,SOL_SOCKET,SO_RCVTIMEO,&bound,(uint)sizeof(timeval))==0,"metadata receive bound");byte* bytes=stackalloc byte[32];"first"u8.CopyTo(new Span<byte>(bytes,32));R(owner.lwip_sendto(fd,bytes,5,0,(sockaddr*)&address,(uint)sizeof(sockaddr_in))==5,"first datagram");"second-longer"u8.CopyTo(new Span<byte>(bytes,32));R(owner.lwip_sendto(fd,bytes,13,0,(sockaddr*)&address,(uint)sizeof(sockaddr_in))==13,"second datagram");R(owner.lwip_host_barrier()==0,"datagrams queued");
            byte* control=stackalloc byte[128];for(int i=0;i<2;i++){int count=0;R(owner.lwip_ioctl(fd,FIONREAD,&count)==0&&count==(i==0?5:13),"Linux FIONREAD next datagram");new Span<byte>(control,128).Clear();iovec vector=new(){iov_base=bytes,iov_len=32};msghdr message=new(){msg_iov=&vector,msg_iovlen=1,msg_control=control,msg_controllen=128};R(owner.lwip_recvmsg(fd,&message,0)==count,"recvmsg length");R(new ReadOnlySpan<byte>(bytes,count).SequenceEqual(i==0?"first"u8:"second-longer"u8),"recvmsg payload");R(message.msg_controllen==native.GetProperty("cmsg_space").GetUInt32(),"native CMSG_SPACE");var c=(cmsghdr*)control;R(c->cmsg_level==IPPROTO_IP&&c->cmsg_type==IP_PKTINFO&&c->cmsg_len==native.GetProperty("cmsg_length").GetUInt32(),"native control header");var info=(in_pktinfo*)(control+native.GetProperty("cmsg_data_offset").GetInt32());R(info->ipi_ifindex>0&&info->ipi_addr.s_addr==0x0100007f,"destination and interface metadata");}
        }
        finally{R(owner.lwip_close(fd)==0,"metadata socket close");}
        var connection=owner.netconn_new_with_proto_and_callback(netconn_type.NETCONN_UDP_IPV6,0,null);R(connection!=null,"IPv6 netconn");try{connection->recv_timeout=5000;connection->flags|=NETCONN_FLAG_PKTINFO;ip_addr loop=default;loop.type=6;((byte*)&loop.u_addr.ip6.addr[0])[15]=1;R(owner.netconn_bind(connection,&loop,52001)==0,"IPv6 metadata bind");var outgoing=owner.netbuf_new();R(outgoing!=null,"netbuf allocate");try{void* data=owner.netbuf_alloc(outgoing,4);R(data!=null,"netbuf payload");"info"u8.CopyTo(new Span<byte>(data,4));R(owner.netconn_sendto(connection,outgoing,&loop,52001)==0,"IPv6 metadata send");}finally{owner.netbuf_delete(outgoing);}netbuf* result=null;R(owner.netconn_recv(connection,&result)==0&&result!=null,"IPv6 metadata receive");try{R((result->flags&NETBUF_FLAG_DESTADDR)!=0&&result->toaddr.type==6&&new ReadOnlySpan<byte>(&result->toaddr.u_addr.ip6.addr[0],16).SequenceEqual(new ReadOnlySpan<byte>(&loop.u_addr.ip6.addr[0],16))&&result->toport_chksum==Swap(52001),"IPv6 destination and port");}finally{owner.netbuf_delete(result);}}finally{R(owner.netconn_delete(connection)==0,"IPv6 netconn release");}
    }
    static void Run(LwipStack stack,JsonElement native,string wire)
    {
        using var peer=new Peer(stack,native,wire);peer.TcpOptions();peer.UdpReuse();using(var entry=stack.Core.__DotCcEnter()){stack.Core.lwip_socket_thread_init();try{UdpInfo(stack.Core,native);peer.Linger();peer.Timers();}finally{stack.Core.lwip_socket_thread_cleanup();}}
    }
    static void Main(string[] args)
    {
        if(args.Length==1&&args[0]=="--assert-control")throw new InvalidOperationException("intentional assertion control");R(args.Length==2,"native report and wire directory required");using var document=JsonDocument.Parse(File.ReadAllText(args[0]));var native=document.RootElement.GetProperty("result");Directory.CreateDirectory(args[1]);using var a=new LwipStack();using var b=new LwipStack();LwipSysArch.SetClock(a.Core,()=>1000);LwipSysArch.SetClock(b.Core,()=>1000);a.Start();b.Start();Run(a,native,Path.Combine(args[1],"owner-a.wire"));Run(b,native,Path.Combine(args[1],"owner-b.wire"));a.Dispose();GC.Collect();GC.WaitForPendingFinalizers();Run(b,native,Path.Combine(args[1],"owner-b-after-a.wire"));Console.WriteLine("PASS enhanced-options: 10 original option paths, native layouts, legal independent peer, natural cleanup and survivor owner");
    }
}

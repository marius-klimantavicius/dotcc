using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    static void Require(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    static readonly byte[] Payload=Enumerable.Range(0,73).Select(i=>(byte)(i*7)).ToArray();
    sealed class Wire
    {
        readonly List<byte> frame=[];
        bool escaped;
        public int Mppe,VjFull,VjCompressed,Ip,LqrAck;
        public void Observe(ReadOnlySpan<byte> bytes)
        {
            foreach(byte input in bytes)
            {
                if(input==0x7e)
                {
                    if(frame.Count!=0)
                    {
                        uint fcs=0xffff;foreach(byte b in frame){fcs^=b;for(int bit=0;bit<8;bit++)fcs=(fcs>>1)^((fcs&1)!=0?0x8408u:0);}
                        Require(fcs==0xf0b8,"independent RFC1662 FCS residue");
                        int pos=frame.Count>2&&frame[0]==0xff&&frame[1]==3?2:0;int protocol=frame[pos++];if((protocol&1)==0)protocol=(protocol<<8)|frame[pos++];
                        if(protocol==0xc021&&frame[pos]==2){for(int k=pos+4;k+2<=frame.Count-2;){int length=frame[k+1];Require(length>=2&&k+length<=frame.Count-2,"LCP option bounds");if(frame[k]==4){Require(length==8&&frame[k+2]==0xc0&&frame[k+3]==0x25&&frame[k+7]==100,"LQR ACK protocol/period");LqrAck++;}k+=length;}}
                        if(protocol==0xfd){Mppe++;Require(CollectionsMarshal.AsSpan(frame)[pos..^2].IndexOf(Payload)<0,"MPPE wire excludes plaintext");}
                        if(protocol==0x2f)VjFull++;if(protocol==0x2d)VjCompressed++;if(protocol==0x21)Ip++;
                    }
                    frame.Clear();escaped=false;
                }
                else if(input==0x7d)escaped=true;
                else{byte b=escaped?(byte)(input^0x20):input;escaped=false;Require(frame.Count<4096,"bounded wire decoder");frame.Add(b);}
            }
        }
    }
    sealed class Peer:IDisposable
    {
        public readonly LwipStack Stack=new();
        public readonly int Index;
        public readonly Wire Wire=new();
        public readonly ConcurrentQueue<byte[]> Input=new();
        public Peer Other=null!;
        public ppp_pcb_s* Pcb;
        public netif* Interface;
        public udp_pcb* Udp;
        public tcp_pcb* Tcp;
        public tcp_pcb* Listener;
        public int Up,Error,Datagrams,TcpPackets,Connected,Pending;
        public Exception? CallbackError;
        public uint Clock,Memory,Pool,Timeouts;
        public GCHandle Root;
        public byte* User;
        public byte* Password;
        public Peer(int index)
        {
            Index=index;LwipSysArch.SetClock(Stack.Core,()=>Volatile.Read(ref Clock));Stack.Start();
            Interface=(netif*)NativeMemory.AllocZeroed((nuint)sizeof(netif));Root=GCHandle.Alloc(this);
            Stack.Invoke(core=>{Memory=core.Globals.lwip_stats.mem.used;Pool=core.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used;Timeouts=core.Globals.lwip_stats.memp[(int)memp_t.MEMP_SYS_TIMEOUT].Value->used;});
        }
        public void Create(byte auth,bool wrong,bool mppe)
        {
            User=(byte*)Marshal.StringToCoTaskMemUTF8("user");Password=(byte*)Marshal.StringToCoTaskMemUTF8(wrong&&Index==0?"wrong":"password");
            Stack.Invoke(core=>
            {
                Pcb=core.pppos_create(Interface,&Output,&Status,(void*)GCHandle.ToIntPtr(Root));Require(Pcb!=null,"PPPoS allocation");core.ppp_set_auth(Pcb,auth,User,Password);
                ip4_addr local=default,remote=default;fixed(byte* a=System.Text.Encoding.UTF8.GetBytes($"10.4.0.{Index+1}\0"),b=System.Text.Encoding.UTF8.GetBytes($"10.4.0.{2-Index}\0")){Require(core.ip4addr_aton(a,&local)==1&&core.ip4addr_aton(b,&remote)==1,"PPP address configuration");}
#if LWIP_LQR
                Pcb->lcp_wantoptions.neg_lqr=Pcb->lcp_allowoptions.neg_lqr=1;Pcb->lcp_wantoptions.lqr_period=Pcb->lcp_allowoptions.lqr_period=100;
#endif
                Pcb->ipcp_wantoptions.ouraddr=local.addr;Pcb->ask_for_local=1;Pcb->ipcp_wantoptions.hisaddr=remote.addr;if(Index==1)Pcb->settings.auth_required=1;if(mppe)core.ppp_set_mppe(Pcb,5);
            });
        }
        public void Pump()
        {
            Clock+=50;Stack.Poll();Stack.Invoke(core=>
            {
                core.sys_check_timeouts();
                int limit=4096;
                while(Input.TryDequeue(out byte[]? bytes))
                {
                    Require(--limit>0,"bounded PPP admission drain");Interlocked.Decrement(ref Pending);
                    fixed(byte* data=bytes)for(int offset=0;offset<bytes.Length;){int length=Math.Min(1+offset%7,bytes.Length-offset);core.pppos_input(Pcb,data+offset,length);offset+=length;}
                }
            });if(CallbackError!=null)throw CallbackError;
        }
        public bool Opened(){bool value=false;Stack.Invoke(_=>value=Pcb->ipcp_fsm.state==9&&Pcb->ipv6cp_fsm.state==9);return value;}
        public bool Dead(){bool value=false;Stack.Invoke(_=>value=Pcb->phase==0);return value;}
        public void Dispose()
        {
            Stack.Dispose();if(Root.IsAllocated)Root.Free();NativeMemory.Free(Interface);Marshal.FreeCoTaskMem((nint)User);Marshal.FreeCoTaskMem((nint)Password);
        }
    }
    static Peer Context(Lwip owner,ppp_pcb_s* pcb,void* argument)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;Require(ReferenceEquals(owner,p.Stack.Core)&&pcb==p.Pcb,"explicit PPP owner/callback identity");return p;}
    static uint Output(Lwip owner,ppp_pcb_s* pcb,void* bytes,uint length,void* argument)
    {
        var p=Context(owner,pcb,argument);try{Require(length<=2048,"bounded PPP output frame");byte[] copy=new ReadOnlySpan<byte>(bytes,checked((int)length)).ToArray();p.Wire.Observe(copy);Require(Interlocked.Increment(ref p.Other.Pending)<=2048,"bounded PPP peer queue");p.Other.Input.Enqueue(copy);return length;}catch(Exception failure){p.CallbackError=failure;return 0;}
    }
    static void Status(Lwip owner,ppp_pcb_s* pcb,int code,void* argument)
    {var p=Context(owner,pcb,argument);if(code==0)p.Up++;else p.Error=code;}
    static void Datagram(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;
        try{Require(ReferenceEquals(owner,p.Stack.Core)&&pcb==p.Udp&&port==9001&&address->type==0,"UDP callback ownership/address");CheckPayload(owner,packet);p.Datagrams++;}catch(Exception failure){p.CallbackError=failure;}finally{owner.pbuf_free(packet);}
    }
    static void CheckPayload(Lwip owner,pbuf* packet)
    {Require(packet->tot_len==73,"PPP payload length");byte* data=stackalloc byte[73];Require(owner.pbuf_copy_partial(packet,data,73,0)==73&&new ReadOnlySpan<byte>(data,73).SequenceEqual(Payload),"PPP payload bytes");}
    static sbyte TcpData(Lwip owner,void* argument,tcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;if(packet==null)return 0;
        try{Require(ReferenceEquals(owner,p.Stack.Core)&&pcb==p.Tcp&&error==0,"TCP callback identity");CheckPayload(owner,packet);owner.tcp_recved(pcb,73);p.TcpPackets++;}catch(Exception failure){p.CallbackError=failure;}finally{owner.pbuf_free(packet);}return 0;
    }
    static sbyte Accepted(Lwip owner,void* argument,tcp_pcb* pcb,sbyte error)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;Require(error==0&&p.Tcp==null&&ReferenceEquals(owner,p.Stack.Core),"TCP accept owner");p.Tcp=pcb;owner.tcp_bind_netif(pcb,p.Interface);owner.tcp_arg(pcb,argument);owner.tcp_recv(pcb,&TcpData);return 0;}
    static sbyte Connected(Lwip owner,void* argument,tcp_pcb* pcb,sbyte error)
    {var p=(Peer)GCHandle.FromIntPtr((nint)argument).Target!;Require(error==0&&p.Tcp==pcb&&ReferenceEquals(owner,p.Stack.Core),"TCP connect owner");p.Connected=1;return 0;}
    static ip_addr Address(Lwip owner,int index)
    {ip_addr address=default;fixed(byte* text=System.Text.Encoding.UTF8.GetBytes($"10.4.0.{index+1}\0"))Require(owner.ipaddr_aton(text,&address)==1,"IP address");return address;}
    static void Until(Peer a,Peer b,Func<bool> condition)
    {uint until=a.Clock+10000;do{Require((int)(until-a.Clock)>0,"bounded PPP protocol progress");a.Pump();b.Pump();}while(!condition());}
    static void One(byte auth,bool wrong,bool mppe)
    {
        using var a=new Peer(0);using var b=new Peer(1);a.Other=b;b.Other=a;a.Create(auth,wrong,mppe);b.Create(auth,wrong,mppe);
        b.Stack.Invoke(core=>Require(core.ppp_listen(b.Pcb)==0,"listen"));a.Stack.Invoke(core=>Require(core.ppp_connect(a.Pcb,0)==0,"connect"));
        Until(a,b,()=>a.Opened()&&b.Opened()||a.Error!=0||b.Error!=0);
        if(wrong)Require(a.Up==0&&b.Up==0&&(a.Error==7||b.Error==7),"wrong credential authentication failure");
        else
        {
            Require(a.Opened()&&b.Opened()&&a.Up==2&&b.Up==2,"both IPCP/IPv6CP negotiated once");
            b.Stack.Invoke(core=>{ip_addr address=Address(core,1);b.Udp=core.udp_new();Require(b.Udp!=null&&core.udp_bind(b.Udp,&address,9000)==0,"UDP listener");core.udp_recv(b.Udp,&Datagram,(void*)GCHandle.ToIntPtr(b.Root));});
            a.Stack.Invoke(core=>{if(mppe)Require(a.Pcb->ccp_gotoptions.mppe!=0,"MPPE negotiated");ip_addr local=Address(core,0),remote=Address(core,1);a.Udp=core.udp_new();Require(a.Udp!=null&&core.udp_bind(a.Udp,&local,9001)==0,"UDP sender");pbuf* packet=core.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,73,pbuf_type.PBUF_RAM);Require(packet!=null,"UDP pbuf");fixed(byte* bytes=Payload)Require(core.pbuf_take(packet,bytes,73)==0,"UDP payload");Require(core.udp_sendto_if_src(a.Udp,packet,&remote,9000,a.Interface,&local)==0,"PPP UDP send");core.pbuf_free(packet);});
            Until(a,b,()=>b.Datagrams==1);Require(a.Opened()&&b.Opened(),"valid datagram exchange preserves open link");
            b.Stack.Invoke(core=>{Require(b.Pcb->vj_enabled!=0,"server VJ negotiation");ip_addr address=Address(core,1);tcp_pcb* listener=core.tcp_new();Require(listener!=null&&core.tcp_bind(listener,&address,9002)==0,"TCP bind");core.tcp_bind_netif(listener,b.Interface);b.Listener=core.tcp_listen_with_backlog(listener,1);Require(b.Listener!=null,"TCP listen");core.tcp_arg(b.Listener,(void*)GCHandle.ToIntPtr(b.Root));core.tcp_accept(b.Listener,&Accepted);});
            a.Stack.Invoke(core=>{Require(a.Pcb->vj_enabled!=0,"client VJ negotiation");ip_addr local=Address(core,0),remote=Address(core,1);a.Tcp=core.tcp_new();Require(a.Tcp!=null&&core.tcp_bind(a.Tcp,&local,9003)==0,"TCP client bind");core.tcp_bind_netif(a.Tcp,a.Interface);core.tcp_arg(a.Tcp,(void*)GCHandle.ToIntPtr(a.Root));Require(core.tcp_connect(a.Tcp,&remote,9002,&Connected)==0,"TCP connect");});
            Until(a,b,()=>a.Connected!=0);
            for(int packet=1;packet<=3;packet++){a.Stack.Invoke(core=>{fixed(byte* bytes=Payload)Require(core.tcp_write(a.Tcp,bytes,73,1)==0&&core.tcp_output(a.Tcp)==0,"TCP payload send");});Until(a,b,()=>b.TcpPackets==packet);}
#if LWIP_LQR
            Require(a.Wire.LqrAck+b.Wire.LqrAck>=2,"LQR option acknowledged by both peers; report engine absent upstream");
#endif
            if(mppe)Require(a.Wire.Mppe>=3&&a.Wire.Ip==0,"negotiated encrypted data wire");else Require(a.Wire.VjFull+b.Wire.VjFull>0&&a.Wire.VjCompressed+b.Wire.VjCompressed>0,"negotiated VJ full/compressed wire");
        }
        foreach(var peer in new[]{a,b})peer.Stack.Invoke(core=>{if(peer.Udp!=null){core.udp_remove(peer.Udp);peer.Udp=null;}if(peer.Tcp!=null){core.tcp_abort(peer.Tcp);peer.Tcp=null;}if(peer.Listener!=null){Require(core.tcp_close(peer.Listener)==0,"listener close");peer.Listener=null;}});
        a.Stack.Invoke(core=>Require(core.ppp_close(a.Pcb,0)==0,"PPP close"));Until(a,b,()=>a.Dead()&&b.Dead());a.Pump();b.Pump();
        foreach(var peer in new[]{a,b})peer.Stack.Invoke(core=>{Require(core.ppp_free(peer.Pcb)==0,"PPP free");peer.Pcb=null;Require(core.Globals.lwip_stats.mem.used==peer.Memory&&core.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used==peer.Pool&&core.Globals.lwip_stats.memp[(int)memp_t.MEMP_SYS_TIMEOUT].Value->used==peer.Timeouts,"PPP owner resource baselines");});
        Console.WriteLine($"PPP auth={auth} wrong={wrong} mppe={mppe} UDP={b.Datagrams} TCP={b.TcpPackets}");
    }
    static void Main(string[] args)
    {if(args.Contains("--ethernet")){Ethernet.Run();return;}for(byte auth=1;auth<=16;auth*=2){One(auth,false,false);One(auth,true,false);}One(8,false,true);Console.WriteLine("PASS ppp-serial: six negotiated sessions, five credential failures, dual-stack control, UDP/TCP payload, independent FCS/VJ/MPPE wire checks and per-owner cleanup");}
}

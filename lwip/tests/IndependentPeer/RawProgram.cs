#if LWIP_NOSYS
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
using KernelSocket=System.Net.Sockets.Socket;
unsafe class Program
{
    static void R(bool condition,string name){if(!condition)throw new InvalidOperationException(name);}
    static byte Pattern(int index)=>(byte)(index*31+(index>>8));
    static KernelSocket Kernel(bool ipv6,SocketType type){var socket=new KernelSocket(ipv6?AddressFamily.InterNetworkV6:AddressFamily.InterNetwork,type,type==SocketType.Stream?ProtocolType.Tcp:ProtocolType.Udp){ReceiveTimeout=10000,SendTimeout=10000};if(ipv6)socket.DualMode=false;return socket;}
    static IPEndPoint Endpoint(bool ipv6,int port,bool kernel)=>new(IPAddress.Parse(ipv6?(kernel?"fd09::1":"fd09::2"):(kernel?"10.9.0.1":"10.9.0.2")),port);
    sealed class State(Lwip owner)
    {
        internal readonly Lwip Owner=owner;
        internal nint Connection,Listener,Udp;
        internal int Connected,Echoed,Fin;
        internal byte[]? Datagram;
        internal Exception? Error;
    }
    static State From(void*arg)=>(State)GCHandle.FromIntPtr((nint)arg).Target!;
    static void Failed(Lwip owner,void*arg,sbyte error){var state=From(arg);state.Connection=0;state.Error=new IOException("raw TCP error "+error);}
    static void Attach(Lwip owner,void*arg,tcp_pcb*pcb){var state=From(arg);R(ReferenceEquals(owner,state.Owner),"raw callback explicit owner");state.Connection=(nint)pcb;owner.tcp_arg(pcb,arg);owner.tcp_recv(pcb,&Receive);owner.tcp_err(pcb,&Failed);Volatile.Write(ref state.Connected,1);}
    static sbyte Connected(Lwip owner,void*arg,tcp_pcb*pcb,sbyte error)
    {try{R(error==0,"raw connected status");Attach(owner,arg,pcb);return 0;}catch(Exception failure){var state=From(arg);state.Error=failure;state.Connection=0;owner.tcp_abort(pcb);return -13;}}
    static sbyte Accepted(Lwip owner,void*arg,tcp_pcb*pcb,sbyte error)=>Connected(owner,arg,pcb,error);
    static sbyte Receive(Lwip owner,void*arg,tcp_pcb*pcb,pbuf*packet,sbyte error)
    {
        var state=From(arg);if(packet==null){Volatile.Write(ref state.Fin,1);return 0;}
        try{R(error==0&&ReferenceEquals(owner,state.Owner),"raw receive owner/status");int total=state.Echoed;for(var p=packet;p!=null;p=p->next)for(int i=0;i<p->len;i++)R(((byte*)p->payload)[i]==Pattern(total++),"raw kernel echo payload");owner.tcp_recved(pcb,packet->tot_len);Volatile.Write(ref state.Echoed,total);return 0;}
        catch(Exception failure){state.Error=failure;state.Connection=0;owner.tcp_abort(pcb);return -13;}
        finally{owner.pbuf_free(packet);}
    }
    static void UdpReceive(Lwip owner,void*arg,udp_pcb*pcb,pbuf*packet,ip_addr*address,ushort port)
    {
        var state=From(arg);try{R(ReferenceEquals(owner,state.Owner),"raw UDP owner");var bytes=new byte[packet->tot_len];fixed(byte*p=bytes)R(owner.pbuf_copy_partial(packet,p,packet->tot_len,0)==packet->tot_len,"raw UDP pbuf copy");Volatile.Write(ref state.Datagram,bytes);}catch(Exception failure){state.Error=failure;}finally{owner.pbuf_free(packet);}
    }
    static void Wait(State state,Tap tap,Func<bool> predicate)
    {var clock=Stopwatch.StartNew();while(!predicate()){if(state.Error!=null)throw state.Error;tap.Check();R(clock.Elapsed<TimeSpan.FromSeconds(10),"raw bounded protocol progress");Thread.Yield();}if(state.Error!=null)throw state.Error;}
    static void Tcp(LwipStack stack,Tap tap,bool ipv6,bool active)
    {
        int port=30000+(ipv6?100:0)+(active?1:2);using var peer=Kernel(ipv6,SocketType.Stream);var state=new State(stack.Core);var handle=GCHandle.Alloc(state);using var lease=stack.Core.__DotCcRetain();Exception? failure=null;Thread? thread=null;
        byte[] target=Encoding.UTF8.GetBytes((ipv6?"fd09::1":"10.9.0.1")+"\0"),local=Encoding.UTF8.GetBytes((ipv6?"fd09::2":"10.9.0.2")+"\0");byte type=(byte)(ipv6?lwip_ip_addr_type.IPADDR_TYPE_V6:lwip_ip_addr_type.IPADDR_TYPE_V4);
        try
        {
            if(active){peer.Bind(Endpoint(ipv6,port,true));peer.Listen(2);}else stack.Invoke(owner=>{ip_addr address;fixed(byte* text=local)R(owner.ipaddr_aton(text,&address)==1,"raw listen address");var pcb=owner.tcp_new_ip_type(type);R(pcb!=null,"raw listener allocate");state.Listener=(nint)pcb;R(owner.tcp_bind(pcb,&address,(ushort)port)==0,"raw bind");sbyte error;var listener=owner.tcp_listen_with_backlog_and_err(pcb,2,&error);R(listener!=null&&error==0,"raw listen");state.Listener=(nint)listener;owner.tcp_arg(listener,(void*)GCHandle.ToIntPtr(handle));owner.tcp_accept(listener,&Accepted);});
            thread=new Thread(()=>{KernelSocket? accepted=null;try{var connected=peer;if(active){R(peer.Poll(10000000,SelectMode.SelectRead),"kernel accept bound");accepted=peer.Accept();accepted.ReceiveTimeout=accepted.SendTimeout=10000;connected=accepted;}else peer.Connect(Endpoint(ipv6,port,false));var bytes=new byte[4096];int total=0;while(total<262144){int count=connected.Receive(bytes);R(count>0,"kernel receive");for(int i=0;i<count;i++)R(bytes[i]==Pattern(total+i),"kernel raw sender payload");int sent=0;while(sent<count){int done=connected.Send(bytes.AsSpan(sent,count-sent));R(done>0,"kernel echo");sent+=done;}total+=count;}connected.Shutdown(SocketShutdown.Send);}catch(Exception error){failure=error;state.Error=error;}finally{accepted?.Dispose();}}){IsBackground=true};thread.Start();
            if(active)stack.Invoke(owner=>{ip_addr address;fixed(byte*text=target)R(owner.ipaddr_aton(text,&address)==1,"raw target address");var pcb=owner.tcp_new_ip_type(type);R(pcb!=null,"raw client allocate");state.Connection=(nint)pcb;owner.tcp_arg(pcb,(void*)GCHandle.ToIntPtr(handle));owner.tcp_err(pcb,&Failed);R(owner.tcp_connect(pcb,&address,(ushort)port,&Connected)==0,"raw connect");});
            Wait(state,tap,()=>Volatile.Read(ref state.Connected)==1);var payload=new byte[4096];for(int sent=0;sent<262144;sent+=payload.Length){for(int i=0;i<payload.Length;i++)payload[i]=Pattern(sent+i);stack.Invoke(owner=>{fixed(byte*bytes=payload)R(owner.tcp_write((tcp_pcb*)state.Connection,bytes,(ushort)payload.Length,TCP_WRITE_FLAG_COPY)==0,"raw TCP enqueue");R(owner.tcp_output((tcp_pcb*)state.Connection)==0,"raw TCP output");});int expected=sent+payload.Length;Wait(state,tap,()=>Volatile.Read(ref state.Echoed)==expected);}Wait(state,tap,()=>Volatile.Read(ref state.Fin)==1);R(thread.Join(TimeSpan.FromSeconds(10)),"kernel raw TCP join");if(failure!=null)throw failure;
            stack.Invoke(owner=>{var pcb=(tcp_pcb*)state.Connection;owner.tcp_arg(pcb,null);owner.tcp_recv(pcb,null);owner.tcp_err(pcb,null);R(owner.tcp_close(pcb)==0,"raw TCP close");state.Connection=0;});Console.WriteLine($"PASS kernel TCP IPv{(ipv6?6:4)} lwIP-raw-{(active?"active":"passive")} echo=262144");
        }
        finally
        {
            stack.Invoke(owner=>{if(state.Connection!=0){var pcb=(tcp_pcb*)state.Connection;owner.tcp_arg(pcb,null);owner.tcp_err(pcb,null);owner.tcp_abort(pcb);state.Connection=0;}if(state.Listener!=0){R(owner.tcp_close((tcp_pcb*)state.Listener)==0,"raw listener close");state.Listener=0;}});peer.Dispose();if(thread?.IsAlive==true&&!thread.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("kernel raw peer cleanup");handle.Free();
        }
    }
    static void Udp(LwipStack stack,Tap tap,bool ipv6)
    {
        int port=30200+(ipv6?1:0);using var peer=Kernel(ipv6,SocketType.Dgram);peer.Bind(Endpoint(ipv6,port,true));var state=new State(stack.Core);var handle=GCHandle.Alloc(state);byte[] local=Encoding.UTF8.GetBytes((ipv6?"fd09::2":"10.9.0.2")+"\0"),target=Encoding.UTF8.GetBytes((ipv6?"fd09::1":"10.9.0.1")+"\0");
        try
        {
            stack.Invoke(owner=>{ip_addr address;fixed(byte*text=local)R(owner.ipaddr_aton(text,&address)==1,"raw UDP local");var pcb=owner.udp_new_ip_type((byte)(ipv6?lwip_ip_addr_type.IPADDR_TYPE_V6:lwip_ip_addr_type.IPADDR_TYPE_V4));R(pcb!=null,"raw UDP allocate");state.Udp=(nint)pcb;R(owner.udp_bind(pcb,&address,(ushort)port)==0,"raw UDP bind");owner.udp_recv(pcb,&UdpReceive,(void*)GCHandle.ToIntPtr(handle));ip_addr destination;fixed(byte*text=target)R(owner.ipaddr_aton(text,&destination)==1,"raw UDP target");var packet=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,123,pbuf_type.PBUF_RAM);R(packet!=null,"raw UDP packet");try{byte*bytes=stackalloc byte[123];for(int i=0;i<123;i++)bytes[i]=Pattern(i);R(owner.pbuf_take(packet,bytes,123)==0,"raw UDP fill");R(owner.udp_sendto(pcb,packet,&destination,(ushort)port)==0,"raw UDP send");}finally{owner.pbuf_free(packet);}});
            var received=new byte[123];EndPoint remote=Endpoint(ipv6,0,false);R(peer.ReceiveFrom(received,ref remote)==123,"kernel raw UDP receive");for(int i=0;i<123;i++)R(received[i]==Pattern(i),"kernel raw UDP payload");R(peer.SendTo(received,Endpoint(ipv6,port,false))==123,"kernel raw UDP reply");Wait(state,tap,()=>Volatile.Read(ref state.Datagram)!=null);R(state.Datagram!.AsSpan().SequenceEqual(received),"raw UDP echoed payload");Console.WriteLine($"PASS kernel UDP IPv{(ipv6?6:4)} raw bidirectional=123");
        }
        finally{stack.Invoke(owner=>{if(state.Udp!=0)owner.udp_remove((udp_pcb*)state.Udp);});handle.Free();}
    }
    static void Main(string[]args)
    {using var stack=new LwipStack();stack.Start();using(var tap=new Tap(stack,args[0]))foreach(bool ipv6 in new[]{false,true}){Tcp(stack,tap,ipv6,true);Tcp(stack,tap,ipv6,false);Udp(stack,tap,ipv6);}Console.WriteLine("PASS interop-kernel: NO_SYS raw TCP active/passive and UDP both families against independent Linux peer");}
}
#endif

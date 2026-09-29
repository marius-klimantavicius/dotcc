#if !LWIP_NOSYS
using System.Net;
using System.Net.Sockets;
using Managed.Networking;
using static Managed.Networking.Lwip;
using KernelSocket=System.Net.Sockets.Socket;
unsafe class Program
{
    static void R(bool condition,string name){if(!condition)throw new InvalidOperationException(name);}
    static byte Pattern(int index)=>(byte)(index*31+(index>>8));
    static uint Address(Lwip owner,sockaddr_storage* storage,bool ipv6,ushort port,bool kernel)
    {
        *storage=default;if(ipv6){var address=(sockaddr_in6*)storage;address->sin6_len=(byte)sizeof(sockaddr_in6);address->sin6_family=AF_INET6;address->sin6_port=owner.lwip_htons(port);byte* bytes=(byte*)&address->sin6_addr;bytes[0]=0xfd;bytes[1]=9;bytes[15]=(byte)(kernel?1:2);return (uint)sizeof(sockaddr_in6);}else{var address=(sockaddr_in*)storage;address->sin_len=(byte)sizeof(sockaddr_in);address->sin_family=AF_INET;address->sin_port=owner.lwip_htons(port);address->sin_addr.s_addr=owner.lwip_htonl(kernel?0x0a090001u:0x0a090002u);return (uint)sizeof(sockaddr_in);}
    }
    static KernelSocket Kernel(bool ipv6,SocketType type){var socket=new KernelSocket(ipv6?AddressFamily.InterNetworkV6:AddressFamily.InterNetwork,type,type==SocketType.Stream?ProtocolType.Tcp:ProtocolType.Udp){ReceiveTimeout=10000,SendTimeout=10000};if(ipv6)socket.DualMode=false;return socket;}
    static IPEndPoint Endpoint(bool ipv6,int port,bool kernel)=>new(IPAddress.Parse(ipv6?(kernel?"fd09::1":"fd09::2"):(kernel?"10.9.0.1":"10.9.0.2")),port);
    static void Bound(Lwip owner,int socket){timeval timeout=new(){tv_sec=10};R(owner.lwip_setsockopt(socket,SOL_SOCKET,SO_RCVTIMEO,&timeout,(uint)sizeof(timeval))==0,"lwIP recv bound");R(owner.lwip_setsockopt(socket,SOL_SOCKET,SO_SNDTIMEO,&timeout,(uint)sizeof(timeval))==0,"lwIP send bound");}
    static void Tcp(Lwip owner,Tap tap,bool ipv6,bool active)
    {
        int port=30000+(ipv6?100:0)+(active?1:2);using var peer=Kernel(ipv6,SocketType.Stream);int listener=-1,socket=-1;Exception? failure=null;Thread? thread=null;
        try
        {
            if(active){peer.Bind(Endpoint(ipv6,port,true));peer.Listen(2);}else{sockaddr_storage local;uint length=Address(owner,&local,ipv6,(ushort)port,false);listener=owner.lwip_socket(ipv6?AF_INET6:AF_INET,SOCK_STREAM,0);R(listener>=0,"lwIP listener");Bound(owner,listener);R(owner.lwip_bind(listener,(sockaddr*)&local,length)==0&&owner.lwip_listen(listener,2)==0,"lwIP listen");}
            thread=new Thread(()=>
            {
                KernelSocket? accepted=null;
                try
                {
                    var connected=peer;if(active){R(peer.Poll(10000000,SelectMode.SelectRead),"kernel accept bound");accepted=peer.Accept();accepted.ReceiveTimeout=accepted.SendTimeout=10000;connected=accepted;}else peer.Connect(Endpoint(ipv6,port,false));
                    var buffer=new byte[4096];int total=0;
                    while(total<262144){int count=connected.Receive(buffer);R(count>0,"kernel TCP receive");for(int i=0;i<count;i++)R(buffer[i]==Pattern(total+i),"kernel validates payload");int sent=0;while(sent<count){int done=connected.Send(buffer.AsSpan(sent,count-sent));R(done>0,"kernel echo send");sent+=done;}total+=count;}
                    connected.Shutdown(SocketShutdown.Send);
                }
                catch(Exception error){failure=error;}
                finally{accepted?.Dispose();}
            }){IsBackground=true};thread.Start();
            if(active){socket=owner.lwip_socket(ipv6?AF_INET6:AF_INET,SOCK_STREAM,0);R(socket>=0,"lwIP TCP socket");Bound(owner,socket);sockaddr_storage target;uint length=Address(owner,&target,ipv6,(ushort)port,true);R(owner.lwip_connect(socket,(sockaddr*)&target,length)==0,"lwIP active connect");}else{socket=owner.lwip_accept(listener,null,null);R(socket>=0,"lwIP passive accept");Bound(owner,socket);}
            byte* bytes=stackalloc byte[4096];int total=0;
            while(total<262144){for(int i=0;i<4096;i++)bytes[i]=Pattern(total+i);int sent=0;while(sent<4096){long done=owner.lwip_send(socket,bytes+sent,(ulong)(4096-sent),0);R(done>0,"lwIP TCP send");sent+=(int)done;}int received=0;while(received<4096){long count=owner.lwip_recv(socket,bytes+received,(ulong)(4096-received),0);R(count>0,"lwIP echo receive");received+=(int)count;}for(int i=0;i<4096;i++)R(bytes[i]==Pattern(total+i),"lwIP validates echo");total+=4096;tap.Check();}
            R(owner.lwip_recv(socket,bytes,1,0)==0,"kernel FIN");R(thread.Join(TimeSpan.FromSeconds(10)),"kernel TCP join");if(failure!=null)throw failure;
            Console.WriteLine($"PASS kernel TCP IPv{(ipv6?6:4)} lwIP-{(active?"active":"passive")} echo=262144");
        }
        finally{if(socket>=0)owner.lwip_close(socket);if(listener>=0)owner.lwip_close(listener);peer.Dispose();if(thread?.IsAlive==true&&!thread.Join(TimeSpan.FromSeconds(10)))throw new TimeoutException("kernel peer cleanup");}
    }
    static void Udp(Lwip owner,Tap tap,bool ipv6)
    {
        int port=30200+(ipv6?1:0);using var peer=Kernel(ipv6,SocketType.Dgram);peer.Bind(Endpoint(ipv6,port,true));int socket=owner.lwip_socket(ipv6?AF_INET6:AF_INET,SOCK_DGRAM,0);R(socket>=0,"lwIP UDP allocate");
        try
        {
            Bound(owner,socket);sockaddr_storage local,target;uint length=Address(owner,&local,ipv6,(ushort)port,false);Address(owner,&target,ipv6,(ushort)port,true);R(owner.lwip_bind(socket,(sockaddr*)&local,length)==0,"lwIP UDP bind");byte* bytes=stackalloc byte[123];for(int i=0;i<123;i++)bytes[i]=Pattern(i);R(owner.lwip_sendto(socket,bytes,123,0,(sockaddr*)&target,length)==123,"lwIP UDP kernel-bound");var received=new byte[123];EndPoint remote=Endpoint(ipv6,0,false);R(peer.ReceiveFrom(received,ref remote)==123,"kernel UDP receive");for(int i=0;i<123;i++)R(received[i]==Pattern(i),"kernel UDP payload");R(peer.SendTo(received,Endpoint(ipv6,port,false))==123,"kernel UDP lwIP-bound");new Span<byte>(bytes,123).Clear();R(owner.lwip_recv(socket,bytes,123,0)==123,"lwIP UDP receive");for(int i=0;i<123;i++)R(bytes[i]==Pattern(i),"lwIP UDP payload");tap.Check();Console.WriteLine($"PASS kernel UDP IPv{(ipv6?6:4)} bidirectional=123");
        }
        finally{owner.lwip_close(socket);}
    }
    static void Main(string[] args)
    {
        using var stack=new LwipStack();stack.Start();using(var tap=new Tap(stack,args[0])){using var entry=stack.Core.__DotCcEnter();stack.Core.lwip_socket_thread_init();try{foreach(bool ipv6 in new[]{false,true}){Tcp(stack.Core,tap,ipv6,true);Tcp(stack.Core,tap,ipv6,false);Udp(stack.Core,tap,ipv6);}}finally{stack.Core.lwip_socket_thread_cleanup();}}
        Console.WriteLine("PASS interop-kernel: independent Linux TCP active/passive and UDP both families through isolated test TAP; product uses translated lwIP");
    }
}

#endif

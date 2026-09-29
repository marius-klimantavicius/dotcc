#if !LWIP_NOSYS
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    static void Require(bool value,string reason){if(!value)throw new InvalidOperationException(reason);}
    static byte Pattern(int index)=>(byte)(index*31+(index>>8));
    static void Main()
    {
        using var a=new LwipStack();using var b=new LwipStack();a.Start();b.Start();
        using var packets=new BlockingCollection<(bool ToB,byte[] Frame)>(1024);
        nint ai=a.AddInterface("10.0.0.1","fd00::1",new byte[]{2,0,0,0,0,1},frame=>{if(!packets.TryAdd((true,frame)))throw new InvalidOperationException("Ethernet queue full");});
        nint bi=b.AddInterface("10.0.0.2","fd00::2",new byte[]{2,0,0,0,0,2},frame=>{if(!packets.TryAdd((false,frame)))throw new InvalidOperationException("Ethernet queue full");});
        Exception? transportError=null;
        var transport=new Thread(()=>
        {
            try{foreach(var packet in packets.GetConsumingEnumerable()){if(packet.ToB)b.Receive(bi,packet.Frame);else a.Receive(ai,packet.Frame);}}
            catch(Exception error){transportError=error;}
        }){IsBackground=true,Name="managed Ethernet link"};transport.Start();
        try
        {
            Exchange(a.Core,b.Core,false);Exchange(a.Core,b.Core,true);
            a.Invoke(_=>{});b.Invoke(_=>{});LwipFrames.ThrowErrors(a.Core);LwipFrames.ThrowErrors(b.Core);
            Console.WriteLine("PASS consumer: two independent stacks, Ethernet, IPv4/IPv6 TCP 262144 bytes and UDP 123 bytes each");
        }
        finally
        {
            packets.CompleteAdding();Require(transport.Join(TimeSpan.FromSeconds(5)),"frame worker drain");
            if(transportError!=null)throw new AggregateException("Frame input failed",transportError);
        }
        var ownerA=a.Core;var ownerB=b.Core;
        a.Dispose();Require(LwipSysArch.Counters(ownerA)==(0,0,0),"A host resources released");
        Exchange(ownerB,ownerB,false,22345);
        b.Dispose();Require(LwipSysArch.Counters(ownerB)==(0,0,0),"B host resources released");
        Console.WriteLine("PASS lifecycle: dispose A, B continues TCP/UDP traffic, both resource/worker/thread-semaphore counters zero");
    }
    static uint Address(Lwip owner,sockaddr_storage* address,bool ipv6,string ip,int port)
    {
        *address=default;byte[] text=Encoding.UTF8.GetBytes(ip+"\0");
        fixed(byte* source=text)
        {
            if(ipv6){var a=(sockaddr_in6*)address;a->sin6_len=(byte)sizeof(sockaddr_in6);a->sin6_family=AF_INET6;a->sin6_port=owner.lwip_htons((ushort)port);Require(owner.lwip_inet_pton(AF_INET6,source,&a->sin6_addr)==1,"IPv6 address");return(uint)sizeof(sockaddr_in6);}
            else{var a=(sockaddr_in*)address;a->sin_len=(byte)sizeof(sockaddr_in);a->sin_family=AF_INET;a->sin_port=owner.lwip_htons((ushort)port);Require(owner.lwip_inet_pton(AF_INET,source,&a->sin_addr)==1,"IPv4 address");return(uint)sizeof(sockaddr_in);}
        }
    }
    static void Timeout(Lwip core,int socket)
    {
        var value=new timeval{tv_sec=10,tv_usec=0};
        Require(core.lwip_setsockopt(socket,SOL_SOCKET,SO_RCVTIMEO,&value,(uint)sizeof(timeval))==0,"receive timeout");
        Require(core.lwip_setsockopt(socket,SOL_SOCKET,SO_SNDTIMEO,&value,(uint)sizeof(timeval))==0,"send timeout");
    }
    static void Exchange(Lwip a,Lwip b,bool ipv6,int port=12345)
    {
        using var bindingA=a.__DotCcEnter();a.lwip_socket_thread_init();
        int family=ipv6?AF_INET6:AF_INET;string target=ipv6?"fd00::2":"10.0.0.2";
        using var listening=new ManualResetEventSlim();Exception? peerError=null;
        var peer=new Thread(()=>
        {
            try
            {
                using var bindingB=b.__DotCcEnter();b.lwip_socket_thread_init();
                int listener=b.lwip_socket(family,SOCK_STREAM,0);Require(listener>=0,"listen socket");Timeout(b,listener);
                sockaddr_storage address;uint length=Address(b,&address,ipv6,target,port);
                Require(b.lwip_bind(listener,(sockaddr*)&address,length)==0,"bind");Require(b.lwip_listen(listener,2)==0,"listen");listening.Set();
                int accepted=b.lwip_accept(listener,null,null);Require(accepted>=0,"accept");Timeout(b,accepted);
                byte* buffer=stackalloc byte[4096];int received=0;
                while(received<262144){long count=b.lwip_recv(accepted,buffer,4096,0);Require(count>0,"receive TCP");for(int i=0;i<count;i++)Require(buffer[i]==Pattern(received++),"TCP payload");int echoed=0;while(echoed<count){long done=b.lwip_send(accepted,buffer+echoed,(ulong)(count-echoed),0);Require(done>0,"echo send");echoed+=(int)done;}}
                Require(b.lwip_close(accepted)==0,"accepted close");Require(b.lwip_close(listener)==0,"listener close");b.lwip_socket_thread_cleanup();
            }
            catch(Exception error){peerError=error;listening.Set();}
        }){IsBackground=true};peer.Start();
        Require(listening.Wait(TimeSpan.FromSeconds(10)),"listen ready");if(peerError!=null)throw peerError;
        int client=a.lwip_socket(family,SOCK_STREAM,0);Require(client>=0,"client socket");Timeout(a,client);
        sockaddr_storage remote;uint remoteLength=Address(a,&remote,ipv6,target,port);
        Require(a.lwip_connect(client,(sockaddr*)&remote,remoteLength)==0,"connect");
        byte* payload=stackalloc byte[4096];byte* echo=stackalloc byte[4096];
        for(int sent=0;sent<262144;sent+=4096)
        {
            for(int i=0;i<4096;i++)payload[i]=Pattern(sent+i);
            int done=0;while(done<4096){long count=a.lwip_send(client,payload+done,(ulong)(4096-done),0);Require(count>0,"TCP send");done+=(int)count;}
            done=0;while(done<4096){long count=a.lwip_recv(client,echo+done,(ulong)(4096-done),0);Require(count>0,"TCP echo receive");done+=(int)count;}
            Require(new ReadOnlySpan<byte>(payload,4096).SequenceEqual(new ReadOnlySpan<byte>(echo,4096)),"echo content");
        }
        Require(a.lwip_recv(client,echo,1,0)==0,"TCP FIN");Require(a.lwip_close(client)==0,"client close");Require(peer.Join(TimeSpan.FromSeconds(10)),"peer join");if(peerError!=null)throw peerError;
        using(b.__DotCcEnter())
        {
            b.lwip_socket_thread_init();int receive=b.lwip_socket(family,SOCK_DGRAM,0);Require(receive>=0,"UDP socket B");Timeout(b,receive);
            sockaddr_storage address;uint length=Address(b,&address,ipv6,target,(ushort)(port+1));Require(b.lwip_bind(receive,(sockaddr*)&address,length)==0,"UDP bind");
            using(a.__DotCcEnter())
            {
                int send=a.lwip_socket(family,SOCK_DGRAM,0);Require(send>=0,"UDP socket A");Address(a,&remote,ipv6,target,(ushort)(port+1));
                for(int i=0;i<123;i++)payload[i]=Pattern(i);Require(a.lwip_sendto(send,payload,123,0,(sockaddr*)&remote,length)==123,"UDP send");Require(a.lwip_close(send)==0,"UDP send close");
            }
            Require(b.lwip_recv(receive,echo,4096,0)==123,"UDP receive");Require(new ReadOnlySpan<byte>(payload,123).SequenceEqual(new ReadOnlySpan<byte>(echo,123)),"UDP content");Require(b.lwip_close(receive)==0,"UDP close");b.lwip_socket_thread_cleanup();
        }
        a.lwip_socket_thread_cleanup();Console.WriteLine($"PASS Ethernet IPv{(ipv6?6:4)} TCP echo=262144 UDP=123");
    }
}

#endif

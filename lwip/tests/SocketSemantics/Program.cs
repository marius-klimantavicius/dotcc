using System.Diagnostics;
using System.Text.Json;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    static int checks;
    static void R(bool condition,string name){if(!condition)throw new InvalidOperationException($"{name}; errno={Libc.errno}");checks++;}
    static uint Address(sockaddr_storage* storage,bool ipv6,ushort port)
    {
        *storage=default;
        if(ipv6){var address=(sockaddr_in6*)storage;address->sin6_len=(byte)sizeof(sockaddr_in6);address->sin6_family=AF_INET6;address->sin6_port=System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(port);((byte*)&address->sin6_addr)[15]=1;return (uint)sizeof(sockaddr_in6);}
        else{var address=(sockaddr_in*)storage;address->sin_len=(byte)sizeof(sockaddr_in);address->sin_family=AF_INET;address->sin_port=System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(port);address->sin_addr.s_addr=0x0100007f;return (uint)sizeof(sockaddr_in);}
    }
    static bool Bytes(void* pointer,ReadOnlySpan<byte> expected)=>new ReadOnlySpan<byte>(pointer,expected.Length).SequenceEqual(expected);
    static void Udp(Lwip owner,bool ipv6)
    {
        sockaddr_storage address;uint length=Address(&address,ipv6,(ushort)(28100+(ipv6?1:0)));int fd=owner.lwip_socket(ipv6?AF_INET6:AF_INET,SOCK_DGRAM,0);R(fd>=0,"UDP allocate");
        try
        {
            R(owner.lwip_bind(fd,(sockaddr*)&address,length)==0,"UDP bind");int flag=1;R(owner.lwip_ioctl(fd,FIONBIO,&flag)==0,"ioctl nonblocking");R((owner.lwip_fcntl(fd,F_GETFL,0)&O_NONBLOCK)!=0,"fcntl reflects ioctl");
            byte* data=stackalloc byte[16];R(owner.lwip_recv(fd,data,16,0)==-1&&Libc.errno==Libc.EWOULDBLOCK,"nonblocking empty receive errno");
            R(owner.lwip_fcntl(fd,F_SETFL,0)==0,"fcntl blocking");R((owner.lwip_fcntl(fd,F_GETFL,0)&O_NONBLOCK)==0,"fcntl flags cleared");
            timeval timeout=new(){tv_usec=20000};R(owner.lwip_setsockopt(fd,SOL_SOCKET,SO_RCVTIMEO,&timeout,(uint)sizeof(timeval))==0,"receive timeout option");R(owner.lwip_recv(fd,data,16,0)==-1&&Libc.errno==Libc.EWOULDBLOCK,"receive timeout errno");
            flag=1;R(owner.lwip_setsockopt(fd,SOL_SOCKET,SO_BROADCAST,&flag,4)==0,"broadcast set");flag=0;uint size=4;R(owner.lwip_getsockopt(fd,SOL_SOCKET,SO_BROADCAST,&flag,&size)==0&&flag!=0,"broadcast get");
            fd_set reads=default;reads.fd_bits[fd/8]|=(byte)(1<<(fd%8));timeout=default;R(owner.lwip_select(fd+1,&reads,null,null,&timeout)==0&&reads.fd_bits[fd/8]==0,"select empty immediate");
            pollfd poll=new(){fd=fd,events=(short)(POLLIN|POLLOUT)};R(owner.lwip_poll(&poll,1,0)==1&&poll.revents==POLLOUT,"poll writable only");
            "readiness"u8.CopyTo(new Span<byte>(data,16));R(owner.lwip_sendto(fd,data,9,0,(sockaddr*)&address,length)==9,"UDP self send");poll.events=POLLIN;R(owner.lwip_poll(&poll,1,2000)==1&&(poll.revents&POLLIN)!=0,"poll readable wake");
            int available=0;R(owner.lwip_ioctl(fd,FIONREAD,&available)==0&&available==9,"FIONREAD datagram size");reads=default;reads.fd_bits[fd/8]|=(byte)(1<<(fd%8));R(owner.lwip_select(fd+1,&reads,null,null,&timeout)==1&&(reads.fd_bits[fd/8]&(1<<(fd%8)))!=0,"select ready");
            R(owner.lwip_recv(fd,data,3,MSG_PEEK)==3&&Bytes(data,"rea"u8),"MSG_PEEK prefix");R(owner.lwip_recv(fd,data,16,0)==9&&Bytes(data,"readiness"u8),"peek preserves datagram");
            R(owner.lwip_close(fd)==0,"UDP close");int closed=fd;fd=-1;poll.fd=closed;poll.events=POLLIN;poll.revents=0;R(owner.lwip_poll(&poll,1,0)==1&&poll.revents==POLLIN,"closed valid slot poll matches native pending event");poll.fd=9999;R(owner.lwip_poll(&poll,1,0)==-1&&(poll.revents&POLLNVAL)!=0,"out-of-range poll matches native");R(owner.lwip_recv(closed,data,1,0)==-1&&Libc.errno==Libc.EBADF,"closed receive errno");
            Console.WriteLine($"PASS socket-udp IPv{(ipv6?6:4)} options ioctl fcntl timeout select poll peek errno");
        }
        finally{if(fd>=0)owner.lwip_close(fd);}
    }
    static void Tcp(Lwip owner,bool ipv6)
    {
        sockaddr_storage address;uint length=Address(&address,ipv6,(ushort)(28200+(ipv6?1:0)));int family=ipv6?AF_INET6:AF_INET;
        int listener=-1,client=-1,peer=-1;
        try
        {
            listener=owner.lwip_socket(family,SOCK_STREAM,0);R(listener>=0,"TCP listener allocate");R(owner.lwip_bind(listener,(sockaddr*)&address,length)==0,"TCP bind");R(owner.lwip_listen(listener,2)==0,"TCP listen");
            client=owner.lwip_socket(family,SOCK_STREAM,0);R(client>=0,"TCP client allocate");R(owner.lwip_connect(client,(sockaddr*)&address,length)==0,"TCP connect");peer=owner.lwip_accept(listener,null,null);R(peer>=0,"TCP accept");
            timeval timeout=new(){tv_sec=5};R(owner.lwip_setsockopt(client,SOL_SOCKET,SO_RCVTIMEO,&timeout,(uint)sizeof(timeval))==0,"client receive bound");R(owner.lwip_setsockopt(peer,SOL_SOCKET,SO_RCVTIMEO,&timeout,(uint)sizeof(timeval))==0,"peer receive bound");
            int flag=1;R(owner.lwip_setsockopt(client,IPPROTO_TCP,TCP_NODELAY,&flag,4)==0,"TCP_NODELAY set");flag=0;uint size=4;R(owner.lwip_getsockopt(client,IPPROTO_TCP,TCP_NODELAY,&flag,&size)==0&&flag==1,"TCP_NODELAY get");
            sockaddr_storage name;uint nameLength=(uint)sizeof(sockaddr_storage);R(owner.lwip_getpeername(client,(sockaddr*)&name,&nameLength)==0&&nameLength==length,"getpeername length");R(new ReadOnlySpan<byte>(&name,(int)length).SequenceEqual(new ReadOnlySpan<byte>(&address,(int)length)),"getpeername bytes");
            byte* data=stackalloc byte[32];"partial"u8.CopyTo(new Span<byte>(data,32));R(owner.lwip_send(client,data,7,0)==7,"TCP send");R(owner.lwip_shutdown(client,SHUT_WR)==0,"TCP half close");R(owner.lwip_recv(peer,data,2,MSG_WAITALL)==2&&Bytes(data,"pa"u8),"partial recv prefix");R(owner.lwip_recv(peer,data,5,MSG_WAITALL)==5&&Bytes(data,"rtial"u8),"partial recv suffix");R(owner.lwip_recv(peer,data,1,0)==0,"peer EOF after half close");
            "ack"u8.CopyTo(new Span<byte>(data,32));R(owner.lwip_send(peer,data,3,0)==3,"half-closed client can receive reply");R(owner.lwip_recv(client,data,3,MSG_WAITALL)==3&&Bytes(data,"ack"u8),"reply contents");R(owner.lwip_close(peer)==0,"peer close");peer=-1;R(owner.lwip_recv(client,data,1,0)==0,"client EOF");R(owner.lwip_close(client)==0,"client close");client=-1;R(owner.lwip_close(listener)==0,"listener close");listener=-1;
            Console.WriteLine($"PASS socket-tcp IPv{(ipv6?6:4)} options peername partial receive half-close EOF");
        }
        finally{if(peer>=0)owner.lwip_close(peer);if(client>=0)owner.lwip_close(client);if(listener>=0)owner.lwip_close(listener);}
    }
    static void CloseWaiter(Lwip owner,int expectedError)
    {
        sockaddr_storage address;uint length=Address(&address,false,28300);int fd=owner.lwip_socket(AF_INET,SOCK_DGRAM,0);R(fd>=0,"blocking UDP allocate");R(owner.lwip_bind(fd,(sockaddr*)&address,length)==0,"blocking UDP bind");
        Exception? failure=null;long result=0;int error=0;var thread=new Thread(()=>{using var entry=owner.__DotCcEnter();owner.lwip_socket_thread_init();try{byte value;result=owner.lwip_recv(fd,&value,1,0);error=Libc.errno;}catch(Exception caught){failure=caught;}finally{owner.lwip_socket_thread_cleanup();}}){IsBackground=true};
        try
        {
            thread.Start();var clock=Stopwatch.StartNew();
            while(true)
            {
                ulong token=LwipSysArch.Protect(owner);int waiting;try{waiting=owner.lwip_socket_dbg_get_socket(fd)->conn->mbox_threads_waiting;}finally{LwipSysArch.Unprotect(owner,token);}
                if(waiting>0)break;if(failure!=null)throw failure;R(clock.Elapsed<TimeSpan.FromSeconds(5),"caller reached translated blocking receive");Thread.Yield();
            }
            R(owner.lwip_close(fd)==0,"concurrent close releases waiter");fd=-1;R(thread.Join(TimeSpan.FromSeconds(5)),"receive caller joined");if(failure!=null)throw failure;R(result==-1&&error==expectedError,"concurrent close status matches native");
            Console.WriteLine($"PASS socket-concurrent-close actual blocked receive returned {result} errno={error}");
        }
        finally{if(fd>=0)owner.lwip_close(fd);if(thread.IsAlive&&!thread.Join(TimeSpan.FromSeconds(5)))throw new TimeoutException("socket cleanup waiter");}
    }
    static void Main(string[] args)
    {
        using var native=JsonDocument.Parse(File.ReadAllText(args[0]));int expectedError=native.RootElement.GetProperty("cases").EnumerateArray().Single(x=>x.GetProperty("case").GetString()=="socket-concurrent-close").GetProperty("errno").GetInt32();
        using var stack=new LwipStack();stack.Start();var owner=stack.Core;using(var entry=owner.__DotCcEnter()){owner.lwip_socket_thread_init();try{Udp(owner,false);Udp(owner,true);Tcp(owner,false);Tcp(owner,true);CloseWaiter(owner,expectedError);}finally{owner.lwip_socket_thread_cleanup();}}
        Console.WriteLine($"PASS socket-semantics: {checks} assertions matched native; closed all sockets before owner disposal");
    }
}

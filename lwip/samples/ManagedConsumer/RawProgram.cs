#if LWIP_NOSYS
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    private sealed class Transfer
    {
        internal int Received,Echoed,Udp;
        internal bool Connected;
        internal Exception? Error;
        internal nint Client,Accepted;
    }
    static void Require(bool value,string reason){if(!value)throw new InvalidOperationException(reason);}
    static byte Pattern(int index)=>(byte)(index*31+(index>>8));
    static Transfer State(void* argument)=>(Transfer)GCHandle.FromIntPtr((nint)argument).Target!;
    static sbyte Accept(Lwip owner,void* argument,tcp_pcb* pcb,sbyte error)
    {var state=State(argument);try{Require(error==0,"accept status");state.Accepted=(nint)pcb;owner.tcp_arg(pcb,argument);owner.tcp_recv(pcb,&ReceiveServer);return 0;}catch(Exception failure){state.Error=failure;owner.tcp_abort(pcb);return -13;}}
    static sbyte Connected(Lwip owner,void* argument,tcp_pcb* pcb,sbyte error)
    {var state=State(argument);if(error!=0){state.Error=new InvalidOperationException("connect status");return error;}state.Connected=true;owner.tcp_recv(pcb,&ReceiveClient);return 0;}
    static sbyte ReceiveServer(Lwip owner,void* argument,tcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var state=State(argument);if(packet==null){owner.tcp_arg(pcb,null);owner.tcp_recv(pcb,null);sbyte result=owner.tcp_close(pcb);if(result==0)state.Accepted=0;return result;}
        try
        {
            Require(error==0,"receive status");for(var p=packet;p!=null;p=p->next)
            {for(int i=0;i<p->len;i++)Require(((byte*)p->payload)[i]==Pattern(state.Received++),"server payload");Require(owner.tcp_write(pcb,p->payload,p->len,TCP_WRITE_FLAG_COPY)==0,"echo queue");}
            owner.tcp_recved(pcb,packet->tot_len);owner.tcp_output(pcb);return 0;
        }
        catch(Exception failure){state.Error=failure;state.Accepted=0;owner.tcp_abort(pcb);return -13;}
        finally{owner.pbuf_free(packet);}
    }
    static sbyte ReceiveClient(Lwip owner,void* argument,tcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var state=State(argument);if(packet==null)return 0;
        try{Require(error==0,"echo status");for(var p=packet;p!=null;p=p->next)for(int i=0;i<p->len;i++)Require(((byte*)p->payload)[i]==Pattern(state.Echoed++),"echo payload");owner.tcp_recved(pcb,packet->tot_len);owner.pbuf_free(packet);return 0;}
        catch(Exception failure){state.Error=failure;state.Client=0;owner.pbuf_free(packet);owner.tcp_abort(pcb);return -13;}
    }
    static void UdpReceive(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var state=State(argument);
        try{Require(packet->tot_len==123,"UDP length");byte* bytes=stackalloc byte[123];Require(owner.pbuf_copy_partial(packet,bytes,123,0)==123,"UDP copy");for(int i=0;i<123;i++)Require(bytes[i]==Pattern(i),"UDP payload");state.Udp++;}
        catch(Exception error){state.Error=error;}
        finally{owner.pbuf_free(packet);}
    }
    static void Main()
    {
        using var a=new LwipStack();using var b=new LwipStack();a.Start();b.Start();
        var packets=new Queue<(bool ToB,byte[] Frame)>();
        void Output(bool toB,byte[] frame){if(packets.Count==1024)throw new InvalidOperationException("Ethernet queue full");packets.Enqueue((toB,frame));}
        nint ai=a.AddInterface("10.0.0.1","fd00::1",new byte[]{2,0,0,0,0,1},frame=>Output(true,frame));
        nint bi=b.AddInterface("10.0.0.2","fd00::2",new byte[]{2,0,0,0,0,2},frame=>Output(false,frame));
        void Pump(){while(packets.TryDequeue(out var packet)){if(packet.ToB)b.Receive(bi,packet.Frame);else a.Receive(ai,packet.Frame);}a.Poll();b.Poll();}
        Exchange(a.Core,b.Core,false,Pump);Exchange(a.Core,b.Core,true,Pump);
        LwipFrames.ThrowErrors(a.Core);LwipFrames.ThrowErrors(b.Core);
        Console.WriteLine("PASS consumer: two independent raw stacks, Ethernet, IPv4/IPv6 TCP 262144 bytes and UDP 123 bytes each");
        var ownerA=a.Core;var ownerB=b.Core;a.Dispose();Require(LwipSysArch.Counters(ownerA)==(0,0,0),"A released");
        Exchange(ownerB,ownerB,false,b.Poll,22345);b.Dispose();Require(LwipSysArch.Counters(ownerB)==(0,0,0),"B released");
        Console.WriteLine("PASS lifecycle: dispose A, B continues TCP/UDP traffic, host counters zero");
    }
    static void Exchange(Lwip a,Lwip b,bool ipv6,Action pump,ushort port=12345)
    {
        var state=new Transfer();var handle=GCHandle.Alloc(state);void* argument=(void*)GCHandle.ToIntPtr(handle);
        tcp_pcb* listener=null;udp_pcb* receiver=null;udp_pcb* sender=null;
        try
        {
        byte type=(byte)(ipv6?lwip_ip_addr_type.IPADDR_TYPE_V6:lwip_ip_addr_type.IPADDR_TYPE_V4);
        byte[] text=Encoding.UTF8.GetBytes((ipv6?"fd00::2":"10.0.0.2")+"\0");ip_addr address;
        using var bindA=a.__DotCcEnter();fixed(byte* bytes=text)Require(a.ipaddr_aton(bytes,&address)==1,"address");
        using(b.__DotCcEnter())
        {listener=b.tcp_new_ip_type(type);Require(listener!=null,"listener allocation");Require(b.tcp_bind(listener,&address,port)==0,"listen bind");listener=b.tcp_listen_with_backlog(listener,2);Require(listener!=null,"listen");b.tcp_arg(listener,argument);b.tcp_accept(listener,&Accept);}
        var client=a.tcp_new_ip_type(type);Require(client!=null,"client allocation");state.Client=(nint)client;a.tcp_arg(client,argument);Require(a.tcp_connect(client,&address,port,&Connected)==0,"connect queue");
        int sent=0;byte* data=stackalloc byte[4096];var timer=Stopwatch.StartNew();
        while(state.Echoed<262144&&timer.Elapsed<TimeSpan.FromSeconds(15))
        {
            pump();if(state.Error!=null)throw state.Error;
            if(state.Connected&&state.Echoed==sent&&sent<262144)
            {for(int i=0;i<4096;i++)data[i]=Pattern(sent+i);Require(a.tcp_write(client,data,4096,TCP_WRITE_FLAG_COPY)==0,"TCP write");sent+=4096;Require(a.tcp_output(client)==0,"TCP output");}
        }
        Require(state.Received==262144&&state.Echoed==262144,$"bounded TCP echo complete sent={sent} received={state.Received} echoed={state.Echoed} connected={state.Connected}");a.tcp_arg(client,null);a.tcp_recv(client,null);Require(a.tcp_close(client)==0,"client close");state.Client=0;for(int i=0;i<100;i++)pump();
        using(b.__DotCcEnter())Require(b.tcp_close(listener)==0,"listener close");listener=null;
        using(b.__DotCcEnter()){receiver=b.udp_new_ip_type(type);Require(receiver!=null,"UDP allocation");Require(b.udp_bind(receiver,&address,(ushort)(port+1))==0,"UDP bind");b.udp_recv(receiver,&UdpReceive,argument);}
        sender=a.udp_new_ip_type(type);var packet=a.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,123,pbuf_type.PBUF_RAM);Require(sender!=null&&packet!=null,"UDP sender allocation");for(int i=0;i<123;i++)data[i]=Pattern(i);Require(a.pbuf_take(packet,data,123)==0,"UDP pbuf");Require(a.udp_sendto(sender,packet,&address,(ushort)(port+1))==0,"UDP send");a.pbuf_free(packet);
        timer.Restart();while(state.Udp==0&&timer.Elapsed<TimeSpan.FromSeconds(10))pump();if(state.Error!=null)throw state.Error;Require(state.Udp==1,"UDP delivery");a.udp_remove(sender);sender=null;using(b.__DotCcEnter())b.udp_remove(receiver);receiver=null;
        Console.WriteLine($"PASS raw Ethernet IPv{(ipv6?6:4)} TCP echo=262144 UDP=123");
        }
        finally
        {
            using(a.__DotCcEnter()){if(state.Client!=0){a.tcp_arg((tcp_pcb*)state.Client,null);a.tcp_abort((tcp_pcb*)state.Client);}if(sender!=null)a.udp_remove(sender);}
            using(b.__DotCcEnter()){if(state.Accepted!=0){b.tcp_arg((tcp_pcb*)state.Accepted,null);b.tcp_abort((tcp_pcb*)state.Accepted);}if(listener!=null)b.tcp_close(listener);if(receiver!=null)b.udp_remove(receiver);}
            handle.Free();
        }
    }
}
#endif

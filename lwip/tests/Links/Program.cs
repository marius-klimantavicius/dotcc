using System.Collections.Concurrent;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    sealed class State
    {
        public readonly List<byte> Output=[];
        public readonly ConcurrentQueue<byte[]> Received=new();
        public readonly AutoResetEvent Changed=new(false);
        public int Datagrams;
        public Exception? Error;
    }
    static readonly ConcurrentDictionary<Lwip,State> States=new();
    static void Require(bool value,string name){if(!value)throw new InvalidOperationException(name);}
    static sbyte Input(Lwip owner,pbuf* packet,netif* iface)
    {
        byte[] bytes=new byte[packet->tot_len];fixed(byte* target=bytes)Require(owner.pbuf_copy_partial(packet,target,(ushort)bytes.Length,0)==bytes.Length,"complete SLIP pbuf");
        owner.pbuf_free(packet);var state=States[owner];state.Received.Enqueue(bytes);state.Changed.Set();return 0;
    }
    static sbyte Initialize(Lwip owner,netif* iface)=>owner.slipif_init(iface);
    static sbyte IpInput(Lwip owner,pbuf* packet,netif* iface)=>owner.ip_input(packet,iface);
    static void Datagram(Lwip owner,void* argument,udp_pcb* pcb,pbuf* packet,ip_addr* address,ushort port)
    {
        var state=States[owner];try{Require(port==9001&&packet->tot_len==73,"SLIP UDP peer/length");byte* bytes=stackalloc byte[73];Require(owner.pbuf_copy_partial(packet,bytes,73,0)==73,"UDP complete bytes");for(int i=0;i<73;i++)Require(bytes[i]==(byte)(i*7),"UDP payload");Require(address->type==(state.Datagrams==0?0:6),"UDP IP family");Interlocked.Increment(ref state.Datagrams);}catch(Exception failure){state.Error=failure;}finally{owner.pbuf_free(packet);state.Changed.Set();}
    }
    static uint Sum(ReadOnlySpan<byte> bytes,uint sum=0){for(int i=0;i<bytes.Length;i+=2)sum+=(uint)(bytes[i]*256+(i+1<bytes.Length?bytes[i+1]:0));while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return sum;}
    static byte[] DatagramBytes(bool v6)
    {
        int offset=v6?40:20;byte[] bytes=new byte[offset+81];if(v6){bytes[0]=0x60;bytes[5]=81;bytes[6]=17;bytes[7]=64;bytes[8]=bytes[24]=0xfd;bytes[23]=2;bytes[39]=1;}
        else{bytes[0]=0x45;bytes[3]=(byte)bytes.Length;bytes[8]=64;bytes[9]=17;bytes[12]=bytes[16]=10;bytes[13]=bytes[17]=1;bytes[15]=2;bytes[19]=1;uint c=~Sum(bytes.AsSpan(0,20));bytes[10]=(byte)(c>>8);bytes[11]=(byte)c;}
        bytes[offset]=0x23;bytes[offset+1]=0x29;bytes[offset+2]=0x23;bytes[offset+3]=0x28;bytes[offset+5]=81;for(int i=0;i<73;i++)bytes[offset+8+i]=(byte)(i*7);
        if(v6){uint c=~Sum(bytes.AsSpan(40),Sum(bytes.AsSpan(8,32),81+17));bytes[46]=(byte)(c>>8);bytes[47]=(byte)c;}return bytes;
    }
    static byte[] Encode(byte[] bytes)
    {
        List<byte> wire=[0xc0];foreach(byte value in bytes){if(value==0xc0)wire.AddRange([0xdb,0xdc]);else if(value==0xdb)wire.AddRange([0xdb,0xdd]);else wire.Add(value);}wire.Add(0xc0);return wire.ToArray();
    }
    static byte[] Receive(Lwip owner,netif* iface,State state,bool nosys)
    {
        if(nosys)owner.slipif_poll(iface);
        var deadline=DateTime.UtcNow.AddSeconds(3);byte[]? bytes;
        while(!state.Received.TryDequeue(out bytes)){var remaining=deadline-DateTime.UtcNow;Require(remaining>TimeSpan.Zero&&state.Changed.WaitOne(remaining),"bounded SLIP receive");}
        return bytes!;
    }
    static pbuf* Packet(Lwip owner,byte[] bytes)
    {pbuf* p=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,(ushort)bytes.Length,pbuf_type.PBUF_RAM);Require(p!=null,"pbuf allocation");fixed(byte* data=bytes)Require(owner.pbuf_take(p,data,(ushort)bytes.Length)==0,"pbuf copy");return p;}
    static void Main(string[] args)
    {
        bool nosys=args[0]=="nosys";using var owner=new Lwip();using var binding=owner.__DotCcEnter();owner.lwip_init();uint baseline=owner.Globals.lwip_stats.mem.used,pool=owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used;var state=new State();States[owner]=state;
        var serial=new LwipSysArch.SerialChannel(bytes=>state.Output.AddRange(bytes),4096);LwipSysArch.AttachSerial(owner,7,serial);
        netif iface=default;ip4_addr address=default,mask=default,gateway=default;fixed(byte* a="10.1.0.1\0"u8,m="255.255.255.0\0"u8){Require(owner.ip4addr_aton(a,&address)==1&&owner.ip4addr_aton(m,&mask)==1,"SLIP addresses");}
        Require(owner.netif_add(&iface,&address,&mask,&gateway,(void*)7,&Initialize,&Input)==&iface && iface.mtu==1500,"SLIP initialize");owner.netif_set_up(&iface);owner.netif_set_link_up(&iface);
        long[] workers=LwipSysArch.FindWorkers(owner,"slipif_loop",(nint)(&iface));Require(workers.Length==(nosys?0:1),"profile SLIP worker applicability");
        byte[] payload=Enumerable.Range(0,300).Select(i=>(byte)i).ToArray();byte[] expected=Encode(payload);
        pbuf* first=Packet(owner,payload[..129]),second=Packet(owner,payload[129..]);owner.pbuf_cat(first,second);Require(iface.output(owner,&iface,first,null)==0,"IPv4 SLIP chained output");owner.pbuf_free(first);
        Require(state.Output.SequenceEqual(expected),"independent escaping vector");int split=Array.IndexOf(expected,(byte)0xdb);
        serial.Feed(expected.AsSpan(0,split+1));if(nosys){owner.slipif_poll(&iface);Require(state.Received.IsEmpty,"partial escape pending");}
        serial.Feed(expected.AsSpan(split+1,expected.Length-split-2));if(nosys){owner.slipif_poll(&iface);Require(state.Received.IsEmpty,"truncated frame pending");}
        serial.Feed(expected.AsSpan(expected.Length-1));Require(Receive(owner,&iface,state,nosys).SequenceEqual(payload),"split escaped frame roundtrip");
        serial.Feed([0xc0,0xc0,0xdb,0x01,0xc0]);Require(Receive(owner,&iface,state,nosys).SequenceEqual(new byte[]{1}),"native unknown escape tolerance and empty delimiter");
        state.Output.Clear();first=Packet(owner,"abc"u8.ToArray());Require(iface.output_ip6(owner,&iface,first,null)==0,"IPv6 SLIP output");owner.pbuf_free(first);Require(state.Output.SequenceEqual(new byte[]{0xc0,97,98,99,0xc0}),"IPv6 output bytes");
        iface.input=&IpInput;ip6_addr address6=default;fixed(byte* a="fd00::1\0"u8)Require(owner.ip6addr_aton(a,&address6)==1,"SLIP IPv6 address");owner.netif_ip6_addr_set(&iface,0,&address6);owner.netif_ip6_addr_set_state(&iface,0,IP6_ADDR_PREFERRED);
        ip_addr any=new(){type=46};udp_pcb* udp=owner.udp_new_ip_type(46);Require(udp!=null&&owner.udp_bind(udp,&any,9000)==0,"SLIP UDP listener");owner.udp_recv(udp,&Datagram,null);
        for(int family=0;family<2;family++){serial.Feed(Encode(DatagramBytes(family==1)));if(nosys)owner.slipif_poll(&iface);var limit=DateTime.UtcNow.AddSeconds(3);while(Volatile.Read(ref state.Datagrams)<family+1){if(state.Error!=null)throw state.Error;var remaining=limit-DateTime.UtcNow;Require(remaining>TimeSpan.Zero&&state.Changed.WaitOne(remaining),"SLIP bounded IP delivery");}if(state.Error!=null)throw state.Error;}
        owner.udp_remove(udp);
        serial.Feed([1,2,3,0xdb]);if(nosys)owner.slipif_poll(&iface);else{var until=DateTime.UtcNow.AddSeconds(3);while(serial.PendingBytes!=0){Require(DateTime.UtcNow<until,"partial SLIP serial bytes consumed");Thread.Yield();}}
        // Stop at the explicitly opted-in serial boundary before freeing decoder storage.
        if(!nosys){LwipSysArch.RequestServiceStop(owner,workers[0]);LwipSysArch.SerialAbort(owner,LwipSysArch.SerialOpen(owner,7));LwipSysArch.JoinWorker(owner,workers[0]);}
        slipif_priv* decoder=(slipif_priv*)iface.state;Require(decoder->q!=null,"partial decoder pbuf retained before cleanup");owner.pbuf_free(decoder->q);decoder->q=decoder->p=null;owner.netif_remove(&iface);owner.mem_free(decoder);iface.state=null;
        Require(owner.Globals.lwip_stats.mem.used==baseline&&owner.Globals.lwip_stats.memp[(int)memp_t.MEMP_PBUF_POOL].Value->used==pool,"decoder memory/pbuf baseline restored");
        serial.ThrowErrors();LwipSysArch.Stop(owner);Require(LwipSysArch.Counters(owner).Workers==0,"SLIP worker joined");States.TryRemove(owner,out _);state.Changed.Dispose();
        Console.WriteLine("PASS slip-framing: native-matched two receive frames, split escapes/truncation, 300-byte chained output, IPv4/IPv6 UDP input, partial-pbuf baseline cleanup, owner worker stop/join");
    }
}

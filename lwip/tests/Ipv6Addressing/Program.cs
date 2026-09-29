using System.Buffers.Binary;
using System.Net;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    static void R(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    static byte[] Ip(string value)=>IPAddress.Parse(value).GetAddressBytes();
    static ushort Checksum(ReadOnlySpan<byte> bytes){uint sum=0;for(int i=0;i<bytes.Length;i+=2)sum+=(uint)(bytes[i]<<8)|(uint)(i+1<bytes.Length?bytes[i+1]:0);while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return (ushort)~sum;}
    static byte[] Frame(byte protocol,byte[] payload,string source,string destination,byte hop=255)
    {
        var src=Ip(source);var dst=Ip(destination);var frame=new byte[54+payload.Length];new byte[]{0x33,0x33,dst[12],dst[13],dst[14],dst[15]}.CopyTo(frame,0);new byte[]{2,0,0,0,0,2}.CopyTo(frame,6);frame[12]=0x86;frame[13]=0xdd;frame[14]=0x60;BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(18),(ushort)payload.Length);frame[20]=protocol;frame[21]=hop;src.CopyTo(frame,22);dst.CopyTo(frame,38);payload.CopyTo(frame,54);
        var pseudo=new byte[40+payload.Length];src.CopyTo(pseudo,0);dst.CopyTo(pseudo,16);BinaryPrimitives.WriteUInt32BigEndian(pseudo.AsSpan(32),(uint)payload.Length);pseudo[39]=protocol;payload.CopyTo(pseudo,40);ushort checksum=Checksum(pseudo);BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(protocol==17?60:56),checksum==0?(ushort)65535:checksum);return frame;
    }
    static byte[] Advertisement(int prefix=1,uint valid=8,uint preferred=4,byte hop=255)
    {
        var ra=new byte[56];ra[0]=134;ra[4]=64;ra[5]=0x40;BinaryPrimitives.WriteUInt16BigEndian(ra.AsSpan(6),30);ra[16]=1;ra[17]=1;new byte[]{2,0,0,0,0,2}.CopyTo(ra,18);ra[24]=3;ra[25]=4;ra[26]=64;ra[27]=0xc0;BinaryPrimitives.WriteUInt32BigEndian(ra.AsSpan(28),valid);BinaryPrimitives.WriteUInt32BigEndian(ra.AsSpan(32),preferred);Ip($"2001:db8:{prefix}::").CopyTo(ra,40);return Frame(58,ra,"fe80::2","ff02::1",hop);
    }
    static byte[] NeighborConflict(byte[] target)
    {var na=new byte[32];na[0]=136;na[4]=0x20;target.CopyTo(na,8);na[24]=2;na[25]=1;new byte[]{2,0,0,0,0,2}.CopyTo(na,26);return Frame(58,na,"fe80::2","ff02::1");}
    static byte[] DhcpReply(byte[] request,string dns="2001:db8::53")
    {
        R(request.Length>=66&&request[62]==11,"DHCPv6 INFORMATION-REQUEST wire message");var udp=new byte[32];BinaryPrimitives.WriteUInt16BigEndian(udp,547);BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(2),546);BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(4),(ushort)udp.Length);udp[8]=7;request.AsSpan(63,3).CopyTo(udp.AsSpan(9));BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(12),23);BinaryPrimitives.WriteUInt16BigEndian(udp.AsSpan(14),16);Ip(dns).CopyTo(udp,16);string dest=new IPAddress(request.AsSpan(22,16)).ToString();var frame=Frame(17,udp,"fe80::2",dest,64);new byte[]{2,0,0,0,0,1}.CopyTo(frame,0);return frame;
    }
    static void Main(string[]args)
    {
        using var capture=new Pcap(args[0]);using var stack=new LwipStack();stack.Start();var frames=new List<byte[]>();var frameGate=new object();nint iface=stack.AddInterface("10.0.0.1","",new byte[]{2,0,0,0,0,1},frame=>{capture.Write(frame);lock(frameGate)frames.Add(frame);});
        void Inject(byte[] frame){capture.Write(frame);stack.Receive(iface,frame);stack.Invoke(_=>{});LwipFrames.ThrowErrors(stack.Core);}
        void Tick(){stack.Invoke(owner=>owner.nd6_tmr());stack.Poll();}
        byte State(int index){byte result=0;stack.Invoke(_=>result=((netif*)iface)->ip6_addr_state[index]);return result;}
        byte[] Address(int index){byte[] result=new byte[16];stack.Invoke(_=>new ReadOnlySpan<byte>(&((ip_addr*)&((netif*)iface)->ip6_addr)[index].u_addr.ip6.addr[0],16).CopyTo(result));return result;}
        try
        {
            stack.Invoke(owner=>{owner.netif_create_ip6_linklocal_address((netif*)iface,1);R(owner.dhcp6_enable_stateless((netif*)iface)==0,"enable stateless DHCP6");});
            for(int i=0;i<4&&State(0)!=0x30;i++)Tick();R(State(0)==0x30,"link-local DAD preferred");
            Inject(Advertisement());R((State(1)&8)!=0,"SLAAC address begins tentative");
            byte[] request;lock(frameGate)request=frames.Last(frame=>frame.Length>=66&&frame[20]==17&&BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(54))==546);
            bool Dns(string wanted="2001:db8::53"){bool same=false;stack.Invoke(owner=>same=new ReadOnlySpan<byte>(&owner.dns_getserver(0)->u_addr.ip6.addr[0],16).SequenceEqual(Ip(wanted)));return same;}
            byte[][] Requests(){lock(frameGate)return frames.Where(frame=>frame.Length>=66&&frame[20]==17&&BinaryPrimitives.ReadUInt16BigEndian(frame.AsSpan(54))==546).ToArray();}
            R(!Dns(),"DNS remains unset while valid DHCPv6 reply is delayed");int requests=Requests().Length;stack.Invoke(owner=>{for(int i=0;i<4;i++)owner.dhcp6_tmr();});R(Requests().Length==requests+1,"DHCPv6 request retransmitted on original timeout");R(Requests()[^1].AsSpan(63,3).SequenceEqual(request.AsSpan(63,3)),"DHCPv6 retry preserves transaction");
            Inject(DhcpReply(request));R(Dns(),"DHCPv6 DNS option applied");Inject(Advertisement());byte[] refreshed=Requests()[^1];R(Requests().Length==requests+2,"later RA triggers stateless information refresh");Inject(DhcpReply(refreshed,"2001:db8::54"));R(Dns("2001:db8::54"),"refreshed DHCPv6 DNS option replaces server");
            for(int i=0;i<4&&State(1)!=0x30;i++)Tick();R(State(1)==0x30,"SLAAC DAD succeeds");R(Address(1).AsSpan(0,8).SequenceEqual(Ip("2001:db8:1::").AsSpan(0,8)),"SLAAC advertised prefix");
            for(int i=0;i<8&&State(1)!=0x10;i++)Tick();R(State(1)==0x10,"preferred lifetime expiration deprecates");for(int i=0;i<10&&State(1)!=0;i++)Tick();R(State(1)==0,"valid lifetime expiration invalidates");
            Inject(Advertisement(2,30,20));R((State(1)&8)!=0,"second prefix DAD starts");Inject(NeighborConflict(Address(1)));R(State(1)==0x40,"DAD duplicate NA rejects address");
            Console.WriteLine("PASS ipv6-addressing: link-local DAD, SLAAC preferred/deprecated/expired, duplicate detection, DHCPv6 retry/refresh+DNS options");
        }
        finally{stack.Invoke(owner=>{owner.dhcp6_disable((netif*)iface);owner.dhcp6_cleanup((netif*)iface);});}
    }
}

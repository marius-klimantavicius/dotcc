using System.Buffers.Binary;
using Managed.Networking;
unsafe class Program
{
    static void R(bool condition,string name){if(!condition)throw new InvalidOperationException(name);}
    static ushort Checksum(ReadOnlySpan<byte> data){uint sum=0;for(int i=0;i<data.Length;i+=2)sum+=(uint)(data[i]<<8)|(uint)(i+1<data.Length?data[i+1]:0);while((sum>>16)!=0)sum=(sum&65535)+(sum>>16);return (ushort)~sum;}
    static byte[] Mac(int subnet,bool router)=>new byte[]{2,0,0,0,(byte)subnet,(byte)(router?1:2)};
    static byte[] Ip(int subnet,bool router)=>new byte[]{10,(byte)subnet,0,(byte)(router?1:2)};
    static byte[] Arp(int subnet)
    {
        var frame=new byte[42];Array.Fill(frame,(byte)255,0,6);Mac(subnet,false).CopyTo(frame,6);frame[12]=8;frame[13]=6;frame[15]=1;frame[16]=8;frame[18]=6;frame[19]=4;frame[21]=1;Mac(subnet,false).CopyTo(frame,22);Ip(subnet,false).CopyTo(frame,28);Ip(subnet,true).CopyTo(frame,38);return frame;
    }
    static byte[] Datagram(int destination,byte ttl,byte protocol=17)
    {
        var frame=new byte[14+20+8+24];Mac(1,true).CopyTo(frame,0);Mac(1,false).CopyTo(frame,6);frame[12]=8;frame[13]=0;frame[14]=0x45;BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(16),(ushort)(frame.Length-14));frame[18]=0x12;frame[19]=0x34;frame[22]=ttl;frame[23]=protocol;Ip(1,false).CopyTo(frame,26);Ip(destination,false).CopyTo(frame,30);BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(24),Checksum(frame.AsSpan(14,20)));BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(34),29001);BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(36),29002);BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(38),32);for(int i=42;i<frame.Length;i++)frame[i]=(byte)(i*17);
        if(protocol==6){frame.AsSpan(38,16).Clear();frame[46]=0x50;frame[47]=0x18;frame[48]=0x10;}
        else if(protocol==1){frame.AsSpan(34,8).Clear();frame[34]=8;frame[38]=0x12;frame[39]=0x34;frame[41]=1;}
        if(protocol==1)BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(36),Checksum(frame.AsSpan(34)));
        else{byte[] pseudo=new byte[12+32];frame.AsSpan(26,8).CopyTo(pseudo);pseudo[9]=protocol;pseudo[11]=32;frame.AsSpan(34).CopyTo(pseudo.AsSpan(12));BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(protocol==17?40:50),Checksum(pseudo));}
        return frame;
    }
    static void Main(string[] args)
    {
        using var capture=args.Length>0?new Pcap(args[0]):null;using var stack=new LwipStack();stack.Start();var outputs=new List<(int Port,byte[] Bytes)>();var interfaces=new nint[3];
        for(int i=0;i<3;i++){int subnet=i+1;interfaces[i]=stack.AddInterface($"10.{subnet}.0.1",$"fd{subnet:00}::1",Mac(subnet,true),bytes=>{capture?.Write(bytes);lock(outputs)outputs.Add((subnet,bytes));},"255.255.255.0","0.0.0.0");}
        void Inject(int port,byte[] frame){capture?.Write(frame);stack.Receive(interfaces[port-1],frame);stack.Invoke(_=>{});stack.Poll();stack.Invoke(_=>{});}
        for(int subnet=1;subnet<=3;subnet++)Inject(subnet,Arp(subnet));lock(outputs)outputs.Clear();
        byte[] data=Datagram(2,64);Inject(1,data);(int Port,byte[] Bytes) forwarded;lock(outputs){R(outputs.Count==1,"one routed output");forwarded=outputs[0];outputs.Clear();}
        R(forwarded.Port==2,"IPv4 longest matching connected route");R(forwarded.Bytes[22]==63,"forwarded TTL decremented");R(Checksum(forwarded.Bytes.AsSpan(14,20))==0,"forwarded IPv4 header checksum");R(forwarded.Bytes.AsSpan(0,6).SequenceEqual(Mac(2,false))&&forwarded.Bytes.AsSpan(6,6).SequenceEqual(Mac(2,true)),"egress Ethernet addresses");R(forwarded.Bytes.AsSpan(34).SequenceEqual(data.AsSpan(34)),"forwarded UDP checksum and payload");
        foreach(byte protocol in new byte[]{6,1})
        {
            byte[] packet=Datagram(2,64,protocol);Inject(1,packet);lock(outputs){R(outputs.Count==1&&outputs[0].Port==2,"protocol forwarding output");R(Checksum(outputs[0].Bytes.AsSpan(14,20))==0,"protocol forwarding IP checksum");R(outputs[0].Bytes.AsSpan(34).SequenceEqual(packet.AsSpan(34)),"nonzero TCP/ICMP checksum and payload preserved");outputs.Clear();}
        }
        for(int part=0;part<2;part++)
        {
            byte[] fragment=new byte[50];data.AsSpan(0,34).CopyTo(fragment);data.AsSpan(34+part*16,16).CopyTo(fragment.AsSpan(34));BinaryPrimitives.WriteUInt16BigEndian(fragment.AsSpan(16),36);BinaryPrimitives.WriteUInt16BigEndian(fragment.AsSpan(20),(ushort)(part==0?0x2000:2));fragment[24]=fragment[25]=0;BinaryPrimitives.WriteUInt16BigEndian(fragment.AsSpan(24),Checksum(fragment.AsSpan(14,20)));Inject(1,fragment);
            lock(outputs){R(outputs.Count==1&&outputs[0].Port==2,"fragment forwarding output");R(Checksum(outputs[0].Bytes.AsSpan(14,20))==0,"fragment forwarding IP checksum");R(outputs[0].Bytes.AsSpan(34).SequenceEqual(fragment.AsSpan(34)),"first/nonfirst fragment payload preserved");outputs.Clear();}
        }
        Inject(1,Datagram(2,1));lock(outputs){R(outputs.Count==1&&outputs[0].Port==1,"expired TTL returns toward sender");var icmp=outputs[0].Bytes;R(icmp[23]==1&&icmp[34]==11&&icmp[35]==0,"ICMP time exceeded type/code");R(Checksum(icmp.AsSpan(34))==0,"ICMP checksum");outputs.Clear();}
        Inject(1,Datagram(1,64));lock(outputs){R(outputs.Count==0,"same incoming interface forwarding prohibited");}
        stack.SetLinkState(interfaces[1],false);stack.SetDefaultInterface(interfaces[2]);Inject(1,Datagram(2,64));lock(outputs){R(outputs.All(item=>item.Port!=2),"link-down route cannot emit on disabled port");outputs.Clear();}
        stack.SetLinkState(interfaces[1],true);Inject(2,Arp(2));lock(outputs)outputs.Clear();Inject(1,Datagram(2,64));lock(outputs){R(outputs.Count==1&&outputs[0].Port==2,"link restoration recovers connected route");}
        Console.WriteLine("PASS routing-ipv4: three interfaces connected routing TTL/ICMP checksums same-link drop link-down/up recovery");
    }
}

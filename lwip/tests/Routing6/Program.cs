using System.Buffers.Binary;
using Managed.Networking;
unsafe class Program
{
    static void R(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    static ushort Sum(ReadOnlySpan<byte>bytes){uint sum=0;for(int i=0;i<bytes.Length;i+=2)sum+=(uint)(bytes[i]<<8)|(uint)(i+1<bytes.Length?bytes[i+1]:0);while(sum>>16!=0)sum=(sum&65535)+(sum>>16);return (ushort)~sum;}
    static void U16(byte[]f,int at,int n)=>BinaryPrimitives.WriteUInt16BigEndian(f.AsSpan(at),(ushort)n);
    static void U32(byte[]f,int at,uint n)=>BinaryPrimitives.WriteUInt32BigEndian(f.AsSpan(at),n);
    static byte[] Mac(int subnet,int host)=>new byte[]{2,0,0,0,(byte)subnet,(byte)host};
    static void Ip(byte[]f,int at,int subnet,int host){f.AsSpan(at,16).Clear();f[at]=0xfd;f[at+1]=(byte)subnet;f[at+15]=(byte)host;}
    static ushort TransportSum(byte[]f){int n=BinaryPrimitives.ReadUInt16BigEndian(f.AsSpan(18));byte[]pseudo=new byte[40+n];f.AsSpan(22,32).CopyTo(pseudo);U32(pseudo,32,(uint)n);pseudo[39]=f[20];f.AsSpan(54,n).CopyTo(pseudo.AsSpan(40));return Sum(pseudo);}
    static void SetChecksum(byte[]f,int offset){ushort sum=TransportSum(f);U16(f,54+offset,sum==0?65535:sum);}
    static byte[] Packet(int destination,int hop,int protocol=17,int length=32)
    {
        byte[]f=new byte[length+54];Mac(1,1).CopyTo(f,0);Mac(1,2).CopyTo(f,6);f[12]=0x86;f[13]=0xdd;f[14]=0x60;U16(f,18,length);f[20]=(byte)protocol;f[21]=(byte)hop;Ip(f,22,1,2);Ip(f,38,destination,2);for(int i=8;i<length;i++)f[54+i]=(byte)(i*17);
        if(protocol==17){U16(f,54,29001);U16(f,56,29002);U16(f,58,length);SetChecksum(f,6);}
        else if(protocol==6){f.AsSpan(54,20).Clear();U16(f,54,29001);U16(f,56,29002);f[66]=0x50;f[67]=0x18;f[68]=0x10;SetChecksum(f,16);}
        else{f[54]=128;f[58]=0x12;f[59]=0x34;f[61]=1;SetChecksum(f,2);}return f;
    }
    static byte[] Neighbor(int subnet)
    {
        byte[]f=new byte[86];f[0]=f[1]=0x33;f[2]=0xff;f[5]=1;Mac(subnet,2).CopyTo(f,6);f[12]=0x86;f[13]=0xdd;f[14]=0x60;f[19]=32;f[20]=58;f[21]=255;Ip(f,22,subnet,2);f[38]=0xff;f[39]=2;f[49]=1;f[50]=0xff;f[53]=1;f[54]=135;Ip(f,62,subnet,1);f[78]=1;f[79]=1;Mac(subnet,2).CopyTo(f,80);SetChecksum(f,2);return f;
    }
    static void Main(string[]args)
    {
        using var capture=new Pcap(args[0]);using var stack=new LwipStack();stack.Start();var outputs=new List<(int Port,byte[]Bytes)>();nint[]ifs=new nint[3];
        for(int i=0;i<3;i++){int port=i+1;ifs[i]=stack.AddInterface($"10.{port}.0.1",$"fd{port:00}::1",Mac(port,1),bytes=>{capture.Write(bytes);lock(outputs)outputs.Add((port,bytes));},"255.255.255.0","0.0.0.0");}
        void Inject(int port,byte[]f){capture.Write(f);stack.Receive(ifs[port-1],f);stack.Invoke(_=>{});stack.Poll();stack.Invoke(_=>{});}
        (int Port,byte[]Bytes) One(){lock(outputs){R(outputs.Count==1,"one IPv6 routing output");var item=outputs[0];outputs.Clear();return item;}}
        void Clear(){lock(outputs)outputs.Clear();}
        void Empty(){lock(outputs)R(outputs.Count==0,"IPv6 policy discards frame");}
        for(int p=1;p<=3;p++)Inject(p,Neighbor(p));Clear();
        foreach(int protocol in new[]{6,17,58}){byte[]f=Packet(2,64,protocol);Inject(1,f);var got=One();R(got.Port==2&&got.Bytes[21]==63,"IPv6 connected egress/hop");R(TransportSum(got.Bytes)==0&&got.Bytes.AsSpan(54).SequenceEqual(f.AsSpan(54)),"transport checksum/payload preserved");}
        byte[]original=Packet(2,64);for(int part=0;part<2;part++){byte[]f=Packet(2,64);Array.Resize(ref f,78);f[20]=44;U16(f,18,24);f.AsSpan(54,8).Clear();f[54]=17;U16(f,56,part==0?1:16);U32(f,58,0x12345678);original.AsSpan(54+part*16,16).CopyTo(f.AsSpan(62));Inject(1,f);var got=One();R(got.Port==2&&got.Bytes[21]==63&&got.Bytes.AsSpan(54).SequenceEqual(f.AsSpan(54)),"first/nonfirst fragment forwarding");}
        foreach(int hop in new[]{0,1,2,255}){Inject(1,Packet(2,hop));var got=One();if(hop<=1)R(got.Port==1&&got.Bytes[20]==58&&got.Bytes[54]==3&&got.Bytes[55]==0,"zero/one hop expires without wrap");else R(got.Port==2&&got.Bytes[20]==17&&got.Bytes[21]==hop-1,"positive hop decrement");R(TransportSum(got.Bytes)==0,"hop response transport checksum");}
        stack.Invoke(core=>((Lwip.netif*)ifs[1])->mtu=1280);Inject(1,Packet(2,64,17,1320));var mtu=One();R(mtu.Port==1&&mtu.Bytes[54]==2&&mtu.Bytes[55]==0&&BinaryPrimitives.ReadUInt32BigEndian(mtu.Bytes.AsSpan(58))==1280&&TransportSum(mtu.Bytes)==0,"ICMP6 PacketTooBig/checksum/MTU");
        byte[]same=Packet(1,64);same[53]=3;U16(same,60,0);SetChecksum(same,6);Inject(1,same);Empty();foreach(int at in new[]{22,38}){byte[]zone=Packet(2,64);zone.AsSpan(at,16).Clear();zone[at]=0xfe;zone[at+1]=0x80;zone[at+15]=2;U16(zone,60,0);SetChecksum(zone,6);Inject(1,zone);Empty();}
        stack.SetLinkState(ifs[1],false);stack.SetDefaultInterface(ifs[2]);Inject(1,Packet(2,64));lock(outputs)R(outputs.All(x=>x.Port!=2),"down link excluded");Clear();stack.SetLinkState(ifs[1],true);Inject(2,Neighbor(2));Clear();Inject(1,Packet(2,64));R(One().Port==2,"link recovery");stack.Invoke(core=>core.netif_set_default(null));Inject(1,Packet(99,64));var noRoute=One();R(noRoute.Port==1&&noRoute.Bytes[54]==1&&noRoute.Bytes[55]==0&&TransportSum(noRoute.Bytes)==0,"ICMP6 no route");
        Console.WriteLine("PASS routing-ipv6: UDP/TCP/ICMP6 checksum/fragment preservation hop0/1/2/255 MTU zones no-route link recovery");
    }
}

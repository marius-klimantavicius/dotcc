using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;

unsafe class Program
{
    static void Require(bool value,string name) { if(!value)throw new InvalidOperationException(name); }
    static void EqualHex(byte* actual,int length,string hex,string name)
    { Require(new ReadOnlySpan<byte>(actual,length).SequenceEqual(Convert.FromHexString(hex)),name); }
    static void Primitives(Lwip owner)
    {
        byte* digest=stackalloc byte[20];fixed(byte* abc="abc"u8)
        {
            owner.md4(abc,3,digest);EqualHex(digest,16,"a448017aaf21d8525fc10ae87aa6729d","MD4 vector");
            owner.md5(abc,3,digest);EqualHex(digest,16,"900150983cd24fb0d6963f7d28e17f72","MD5 vector");
            owner.sha1(abc,3,digest);EqualHex(digest,20,"a9993e364706816aba3e25717850c26c9cd0d89d","SHA1 vector");
        }
        byte* cipher=stackalloc byte[8];byte* back=stackalloc byte[8];des_context des=default;
        fixed(byte* key=Convert.FromHexString("133457799bbcdff1"),plain=Convert.FromHexString("0123456789abcdef"))
        {
            owner.des_setkey_enc(&des,key);owner.des_crypt_ecb(&des,plain,cipher);EqualHex(cipher,8,"85e813540f0ab405","DES vector");
            owner.des_setkey_dec(&des,key);owner.des_crypt_ecb(&des,cipher,back);Require(new ReadOnlySpan<byte>(plain,8).SequenceEqual(new ReadOnlySpan<byte>(back,8)),"DES inverse");
        }
        arc4_context rc4=default;byte[] message="Plaintext"u8.ToArray();
        fixed(byte* key="Key"u8,bytes=message)
        { owner.arc4_setup(&rc4,key,3);owner.arc4_crypt(&rc4,bytes,9);EqualHex(bytes,9,"bbf316e8d940af0ad3","ARC4 vector"); }
    }
    static void Chap(Lwip owner,ppp_pcb_s* pcb)
    {
        byte[] challenge=new byte[17],response=new byte[50];challenge[0]=16;response[0]=49;
        Convert.FromHexString("5b5d7c7d7b3f2f3e3c2c602132262628").CopyTo(challenge,1);
        Convert.FromHexString("21402324255e262a28295f2b3a337c7e").CopyTo(response,1);
        Convert.FromHexString("82309ecd8d708b5ea08faa3981cd83544233114a3d85d6df").CopyTo(response,25);
        byte* message=stackalloc byte[256];
        fixed(byte* c=challenge,r=response,user="User\0"u8,password="clientPass\0"u8)
        {
            var verify=owner.Globals.chapms2_digest.verify_response;
            Require(verify(owner,pcb,7,user,password,10,c,r,message,256)==1,"RFC2759 MSCHAPv2 verification");
            Require((Marshal.PtrToStringUTF8((nint)message)??"").StartsWith("S=407A5589115FD0D6209F510FE9C04566932CDA56",StringComparison.Ordinal),"RFC2759 mutual authentication response");
            r[25]^=1;Require(verify(owner,pcb,7,user,password,10,c,r,message,256)==0,"modified MSCHAPv2 response rejection");
        }
    }
    static void Mppe(Lwip owner,ppp_pcb_s* pcb)
    {
        fixed(byte* key=Convert.FromHexString("8b7cdc149b993a1ba118cb153f56dccb"))
        for(int stateful=0;stateful<2;stateful++)
        {
            ppp_mppe_state enc=default,dec=default;owner.mppe_set_key(pcb,&enc,key);owner.mppe_set_key(pcb,&dec,key);
            byte options=(byte)(2|(stateful==1?4:0));owner.mppe_init(pcb,&enc,options);owner.mppe_init(pcb,&dec,options);
            EqualHex(enc.session_key,16,"405cb2247a7956e6e211007ae27b22d4","RFC3079 initial session key");
            byte* bytes=stackalloc byte[73];byte* actual=stackalloc byte[75];
            for(int packet=0;packet<300;packet++)
            {
                for(int i=0;i<73;i++)bytes[i]=(byte)(packet+i*7);
                pbuf* original=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,73,pbuf_type.PBUF_RAM);Require(original!=null && owner.pbuf_take(original,bytes,73)==0,"plain packet allocation");
                pbuf* wire=original;Require(owner.mppe_compress(pcb,&enc,&wire,0x21)==0 && wire!=original,"MPPE encryption");
                Require(owner.pbuf_copy_partial(original,actual,73,0)==73 && new ReadOnlySpan<byte>(actual,73).SequenceEqual(new ReadOnlySpan<byte>(bytes,73)),"encryption preserves original pbuf");owner.pbuf_free(original);
                Require(owner.mppe_decompress(pcb,&dec,&wire)==0 && wire->tot_len==75,"MPPE decryption and rekey");
                Require(owner.pbuf_copy_partial(wire,actual,75,0)==75 && actual[0]==0 && actual[1]==0x21 && new ReadOnlySpan<byte>(actual+2,73).SequenceEqual(new ReadOnlySpan<byte>(bytes,73)),"MPPE complete protocol payload");owner.pbuf_free(wire);
            }
            pbuf* bad=owner.pbuf_alloc(pbuf_layer.PBUF_RAW,2,pbuf_type.PBUF_RAM);Require(bad!=null,"negative packet allocation");new Span<byte>(bad->payload,2).Clear();
            Require(owner.mppe_decompress(pcb,&dec,&bad)==(sbyte)err_enum_t.ERR_BUF,"unencrypted header rejection");owner.pbuf_free(bad);
        }
    }
    static void Main()
    {
        using var owner=new Lwip();using var binding=owner.__DotCcEnter();owner.lwip_init();Primitives(owner);
        ppp_pcb_s pcb=default;netif iface=default;pcb.netif=&iface;Chap(owner,&pcb);Mppe(owner,&pcb);LwipSysArch.Stop(owner);
        Console.WriteLine("PASS bundled-ppp: five primitive vectors; RFC2759 positive/tampered authentication; RFC3079 session key; 600 MPPE packets across stateful/stateless rollover and two malformed headers");
    }
}

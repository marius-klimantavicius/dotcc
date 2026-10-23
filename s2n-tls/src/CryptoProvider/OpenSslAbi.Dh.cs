using System.Formats.Asn1;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private sealed class BigNumber(BigInteger value){public BigInteger Value=value;}
    private static BigInteger Number(nint handle)=>Get<BigNumber>((void*)handle).Value;
    public static void* BN_bin2bn(byte* bytes,int count,void* output)
    {
        try{if(count<0)throw new ArgumentException();var value=new BigInteger(1,Input(bytes,(ulong)count).ToArray());if(output!=null){Get<BigNumber>(output).Value=value;return output;}return Add(new BigNumber(value));}catch{Error();return null;}
    }
    public static int BN_bn2bin(void* value,byte* output)
    {try{var number=Get<BigNumber>(value).Value.Abs();if(number.SignValue==0)return 0;byte[] bytes=number.ToByteArrayUnsigned();bytes.CopyTo(Output(output,bytes.Length));return bytes.Length;}catch{Error();return -1;}}
    public static int BN_num_bits(void* value){try{return Get<BigNumber>(value).Value.Abs().BitLength;}catch{Error();return 0;}}
    public static int BN_is_zero(void* value){try{return Get<BigNumber>(value).Value.SignValue==0?1:0;}catch{Error();return 0;}}
    public static int BN_is_one(void* value){try{return Get<BigNumber>(value).Value.Equals(BigInteger.One)?1:0;}catch{Error();return 0;}}
    public static int BN_cmp(void* first,void* second)
    {try{return Get<BigNumber>(first).Value.CompareTo(Get<BigNumber>(second).Value);}catch{Error();return 0;}}
    public static void* BN_dup(void* value){try{return Add(new BigNumber(Get<BigNumber>(value).Value));}catch{Error();return null;}}
    public static int BN_sub_word(void* value,ulong word)
    {try{var number=Get<BigNumber>(value);number.Value=number.Value.Subtract(new BigInteger(1,BitConverter.GetBytes(word).Reverse().ToArray()));return 1;}catch{Error();return 0;}}
    public static void BN_free(void* value){try{Remove(value);}catch{Error();}}
    private sealed class DhState : IDisposable
    {
        public nint P,Q,G,Public,Private;
        public int PrivateBits;
        public void Dispose(){foreach(nint p in new[]{P,Q,G,Public,Private}.Where(x=>x!=0).Distinct())Remove((void*)p);P=Q=G=Public=Private=0;}
        public DHParameters Parameters()=>new(Number(P),Number(G),Q==0?null:Number(Q),PrivateBits);
    }
    public static void* DH_new(){try{return Add(new DhState());}catch{Error();return null;}}
    public static void DH_free(void* dh){try{Remove(dh);}catch{Error();}}
    public static int DH_size(void* dh){try{return (Number(Get<DhState>(dh).P).BitLength+7)/8;}catch{Error();return 0;}}
    public static int DH_set0_pqg(void* dh,void* p,void* q,void* g)
    {
        try
        {
            var state=Get<DhState>(dh);if(p==null&&state.P==0||g==null&&state.G==0)throw new ArgumentException();
            if(p!=null)_=Get<BigNumber>(p);if(q!=null)_=Get<BigNumber>(q);if(g!=null)_=Get<BigNumber>(g);
            ReplaceNumber(ref state.P,p);ReplaceNumber(ref state.Q,q);ReplaceNumber(ref state.G,g);return 1;
        }
        catch{Error();return 0;}
    }
    private static void ReplaceNumber(ref nint destination,void* value){if(value==null)return;if(destination!=0&&destination!=(nint)value)Remove((void*)destination);destination=(nint)value;}
    public static int DH_set0_key(void* dh,void* publicKey,void* privateKey)
    {try{var state=Get<DhState>(dh);if(publicKey!=null)_=Get<BigNumber>(publicKey);if(privateKey!=null)_=Get<BigNumber>(privateKey);ReplaceNumber(ref state.Public,publicKey);ReplaceNumber(ref state.Private,privateKey);return 1;}catch{Error();return 0;}}
    public static void DH_get0_pqg(void* dh,void** p,void** q,void** g)
    {try{var state=Get<DhState>(dh);if(p!=null)*p=(void*)state.P;if(q!=null)*q=(void*)state.Q;if(g!=null)*g=(void*)state.G;}catch{Error();}}
    public static void DH_get0_key(void* dh,void** publicKey,void** privateKey)
    {try{var state=Get<DhState>(dh);if(publicKey!=null)*publicKey=(void*)state.Public;if(privateKey!=null)*privateKey=(void*)state.Private;}catch{Error();}}
    public static int DH_generate_key(void* dh)
    {
        try
        {
            var state=Get<DhState>(dh);var parameters=state.Parameters();
            if(state.Private==0)
            {
                var generator=new DHKeyPairGenerator();generator.Init(new DHKeyGenerationParameters(new SecureRandom(),parameters));var pair=generator.GenerateKeyPair();
                ReplaceNumber(ref state.Private,Add(new BigNumber(((DHPrivateKeyParameters)pair.Private).X)));ReplaceNumber(ref state.Public,Add(new BigNumber(((DHPublicKeyParameters)pair.Public).Y)));
            }
            else ReplaceNumber(ref state.Public,Add(new BigNumber(parameters.G.ModPow(Number(state.Private),parameters.P))));
            return 1;
        }
        catch{Error();return 0;}
    }
    public static int DH_compute_key(byte* output,void* publicKey,void* dh)
    {
        byte[]? secret=null;
        try
        {
            var state=Get<DhState>(dh);var parameters=state.Parameters();var peer=new DHPublicKeyParameters(Get<BigNumber>(publicKey).Value,parameters);
            var agreement=new DHBasicAgreement();agreement.Init(new DHPrivateKeyParameters(Number(state.Private),parameters));secret=agreement.CalculateAgreement(peer).ToByteArrayUnsigned();
            secret.CopyTo(Output(output,secret.Length));return secret.Length;
        }
        catch{Error();return -1;}
        finally{if(secret!=null)CryptographicOperations.ZeroMemory(secret);}
    }
    public static int DH_check(void* dh,int* codes)
    {
        try
        {
            if(codes==null)throw new ArgumentException();var d=Get<DhState>(dh);var p=Number(d.P);var g=Number(d.G);*codes=0;
            if(p.BitLength>32768){*codes=1;Error();return 0;}
            if(!p.TestBit(0))*codes|=1;if(g.CompareTo(BigInteger.One)<=0||g.CompareTo(p.Subtract(BigInteger.One))>=0)*codes|=8;
            if(d.Q!=0)
            {
                var q=Number(d.Q);if(q.SignValue<=0||q.CompareTo(p)>=0)*codes|=32;
                else
                {
                    if(g.CompareTo(BigInteger.One)>0&&g.CompareTo(p)<0&&!g.ModPow(q,p).Equals(BigInteger.One))*codes|=8;
                    if(!q.IsProbablePrime(128))*codes|=16;if(!p.Mod(q).Equals(BigInteger.One))*codes|=32;
                }
            }
            if(!p.IsProbablePrime(128))*codes|=1;else if(d.Q==0&&!p.ShiftRight(1).IsProbablePrime(128))*codes|=2;
            return 1;
        }
        catch{Error();return 0;}
    }
    public static void* DHparams_dup(void* dh)
    {
        DhState? target=null;
        try
        {
            var d=Get<DhState>(dh);target=new DhState{PrivateBits=d.PrivateBits};
            if(d.P!=0)target.P=(nint)Add(new BigNumber(Number(d.P)));if(d.Q!=0)target.Q=(nint)Add(new BigNumber(Number(d.Q)));if(d.G!=0)target.G=(nint)Add(new BigNumber(Number(d.G)));
            void* result=Add(target);target=null;return result;
        }
        catch{target?.Dispose();Error();return null;}
    }
    public static void* d2i_DHparams(void** output,byte** input,long length)
    {
        DhState? state=null;
        try
        {
            if(input==null||length<0)throw new ArgumentException();var reader=new AsnReader(Input(*input,(ulong)length).ToArray(),AsnEncodingRules.DER);var encoded=reader.ReadEncodedValue();var parameters=DHParameter.GetInstance(encoded.ToArray());
            state=new DhState{P=(nint)Add(new BigNumber(parameters.P)),G=(nint)Add(new BigNumber(parameters.G)),PrivateBits=parameters.L?.IntValueExact??0};void* handle=Add(state);state=null;
            if(output!=null){if(*output!=null)Remove(*output);*output=handle;}*input+=encoded.Length;return handle;
        }
        catch{state?.Dispose();Error();return null;}
    }
}

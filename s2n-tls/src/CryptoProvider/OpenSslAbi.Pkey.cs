using System.Formats.Asn1;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private const int RsaId=6,EcId=408,RsaPssId=912,X25519Id=1034;
    private static string CurveName(int nid) => nid switch {415=>"P-256",715=>"P-384",716=>"P-521",X25519Id=>"X25519",_=>throw new ArgumentException("Unsupported curve NID.")};
    private static int CurveNid(ECCurve curve) => curve.Oid.Value switch {"1.2.840.10045.3.1.7"=>415,"1.3.132.0.34"=>715,"1.3.132.0.35"=>716,_=>throw new ArgumentException("Unsupported named curve.")};
    private sealed class KeyState : IDisposable
    {
        public int Type,Curve;
        public RsaKey? Rsa;
        public EcdsaKey? Ecdsa;
        public EcKeyExchange? Exchange;
        public byte[]? Point;
        public RsassaPssParameters? PssRestrictions;
        public bool EcPrivate;
        public EcView? View;
        private int references=1;
        public KeyState Retain(){Interlocked.Increment(ref references);return this;}
        public void Dispose(){if(Interlocked.Decrement(ref references)!=0)return;if(View!=null){Remove((void*)View.Handle);View=null;}Rsa?.Dispose();Ecdsa?.Dispose();Exchange?.Dispose();}
        public byte[] OriginalPoint()=>Exchange?.ExportPoint()??(Ecdsa!=null
            ?((ECPublicKeyParameters)PublicKeyFactory.CreateKey(Ecdsa.ExportPublicDer())).Q.GetEncoded(false)
            :throw new InvalidOperationException("No original public point."));
        public byte[] PublicPoint()=>Point??OriginalPoint();
    }
    private enum KeyOperation {None,Sign,Verify,Encrypt,Decrypt,Paramgen,Keygen,Derive}
    private sealed class KeyContext : IDisposable
    {
        public KeyState? Key,Peer;
        public int Type,Curve,RsaBits=2048,Padding=1,SaltLength=-1;
        public DigestAlgorithm? Digest;
        public KeyOperation Operation;
        public KeyContext(KeyState key){Key=key.Retain();Type=key.Type;Curve=key.Curve;}
        public KeyContext(int type)=>Type=type;
        public void Dispose(){Key?.Dispose();Peer?.Dispose();Key=Peer=null;}
    }
    public static void* EVP_PKEY_new(){try{return Add(new KeyState());}catch{Error();return null;}}
    public static void EVP_PKEY_free(void* key){try{Remove(key);}catch{Error();}}
    public static int EVP_PKEY_set_type(void* key,int type)
    {
        try{var state=Get<KeyState>(key);if(state.Rsa!=null||state.Ecdsa!=null||state.Exchange!=null||state.Point!=null)throw new InvalidOperationException();if(type is not (RsaId or RsaPssId or EcId or X25519Id))throw new NotSupportedException();state.Type=type;state.Curve=type==X25519Id?X25519Id:0;return 1;}catch{Error();return 0;}
    }
    public static int EVP_PKEY_id(void* key){try{return Get<KeyState>(key).Type;}catch{Error();return 0;}}
    public static int EVP_PKEY_base_id(void* key)=>EVP_PKEY_id(key);
    public static int EVP_PKEY_bits(void* key)
    {try{var k=Get<KeyState>(key);return k.Rsa?.Bits??k.Curve switch{415=>256,715=>384,716=>521,X25519Id=>253,_=>throw new InvalidOperationException()};}catch{Error();return 0;}}
    public static int EVP_PKEY_size(void* key)
    {try{var k=Get<KeyState>(key);return k.Rsa?.Size??k.Curve switch{415=>72,715=>104,716=>139,X25519Id=>32,_=>throw new InvalidOperationException()};}catch{Error();return 0;}}
    public static void* EVP_PKEY_CTX_new(void* key,void* engine)
    {try{if(engine!=null)throw new ArgumentException();return Add(new KeyContext(Get<KeyState>(key)));}catch{Error();return null;}}
    public static void* EVP_PKEY_CTX_new_id(int type,void* engine)
    {try{if(engine!=null||type is not(RsaId or EcId or X25519Id))throw new ArgumentException();return Add(new KeyContext(type){Curve=type==X25519Id?type:0});}catch{Error();return null;}}
    public static void EVP_PKEY_CTX_free(void* ctx){try{Remove(ctx);}catch{Error();}}
    private static int KeyInit(void* ctx,KeyOperation operation)
    {
        try
        {
            var state=Get<KeyContext>(ctx);
            bool allowed=operation switch
            {
                KeyOperation.Sign or KeyOperation.Verify => state.Key!=null&&state.Type is (RsaId or RsaPssId or EcId),
                KeyOperation.Encrypt or KeyOperation.Decrypt => state.Key?.Rsa!=null&&state.Type==RsaId,
                KeyOperation.Paramgen => state.Type==EcId,
                KeyOperation.Keygen => state.Type is RsaId or EcId or X25519Id,
                KeyOperation.Derive => state.Key!=null&&state.Type is (EcId or X25519Id),
                _=>false
            };
            if(!allowed)throw new NotSupportedException();state.Operation=operation;return 1;
        }
        catch{Error();return 0;}
    }
    public static int EVP_PKEY_sign_init(void* ctx)=>KeyInit(ctx,KeyOperation.Sign);
    public static int EVP_PKEY_verify_init(void* ctx)=>KeyInit(ctx,KeyOperation.Verify);
    public static int EVP_PKEY_encrypt_init(void* ctx)=>KeyInit(ctx,KeyOperation.Encrypt);
    public static int EVP_PKEY_decrypt_init(void* ctx)
    {
        try
        {
            var state = Get<KeyContext>(ctx);
            if (state.Type != RsaId) { Error(); return -2; }
            state.Operation = KeyOperation.Decrypt;
            return 1;
        }
        catch { Error(); return -2; }
    }
    public static int EVP_PKEY_paramgen_init(void* ctx)=>KeyInit(ctx,KeyOperation.Paramgen);
    public static int EVP_PKEY_keygen_init(void* ctx)=>KeyInit(ctx,KeyOperation.Keygen);
    public static int EVP_PKEY_derive_init(void* ctx)=>KeyInit(ctx,KeyOperation.Derive);
    public static int EVP_PKEY_CTX_ctrl(void* ctx,int keyType,int operation,int command,int argument,void* pointer)
    {
        try
        {
            var c=Get<KeyContext>(ctx);if(keyType!=-1&&keyType!=c.Type)throw new ArgumentException("Control key type mismatch.");
            if(command==1){c.Digest=Get<DigestDescriptor>(pointer).Algorithm;return 1;}
            if(c.Type==EcId&&command==0x1001){_ = CurveName(argument);c.Curve=argument;return 1;}
            if(c.Type is RsaId or RsaPssId)
            {
                if(command==0x1001){if(argument is not(1 or 3 or 6))throw new NotSupportedException();if(c.Type==RsaPssId&&argument!=6)throw new ArgumentException();c.Padding=argument;return 1;}
                if(command==0x1002){if(argument!=-1)throw new NotSupportedException("Only digest-length PSS salts are requested by s2n.");c.SaltLength=argument;return 1;}
                if(command==0x1003){if(argument<512||argument>16384)throw new ArgumentException();c.RsaBits=argument;return 1;}
            }
            throw new NotSupportedException("Unknown PKEY control.");
        }
        catch{Error();return -2;}
    }
    public static int RSA_pkey_ctx_ctrl(void* ctx,int operation,int command,int argument,void* pointer)
    {
        try{if(Get<KeyContext>(ctx).Type is not(RsaId or RsaPssId))throw new ArgumentException();return EVP_PKEY_CTX_ctrl(ctx,-1,operation,command,argument,pointer);}catch{Error();return -2;}
    }
    private static void ValidatePss(KeyState key,KeyContext ctx)
    {
        if(key.Type!=RsaPssId)return;
        if(ctx.Padding!=6)throw new ArgumentException("RSA-PSS keys require PSS signatures.");
        if(key.PssRestrictions is not { } p)return;
        DigestAlgorithm digest=ctx.Digest??throw new InvalidOperationException();
        string oid=digest switch{DigestAlgorithm.Sha1=>"1.3.14.3.2.26",DigestAlgorithm.Sha224=>"2.16.840.1.101.3.4.2.4",DigestAlgorithm.Sha256=>"2.16.840.1.101.3.4.2.1",DigestAlgorithm.Sha384=>"2.16.840.1.101.3.4.2.2",DigestAlgorithm.Sha512=>"2.16.840.1.101.3.4.2.3",_=>throw new ArgumentException()};
        using var h=new Digest(digest);
        if(p.HashAlgorithm.Algorithm.Id!=oid||p.MaskGenAlgorithm.Algorithm.Id!="1.2.840.113549.1.1.8"||AlgorithmIdentifier.GetInstance(p.MaskGenAlgorithm.Parameters).Algorithm.Id!=oid||p.SaltLength.IntValueExact>h.Size||p.TrailerField.IntValueExact!=1)
            throw new ArgumentException("RSA-PSS key restrictions reject this signature.");
    }
    public static int EVP_PKEY_sign(void* ctx,byte* output,ulong* length,byte* digest,ulong count)
    {
        try
        {
            var c=Get<KeyContext>(ctx);var key=c.Key??throw new InvalidOperationException();if(c.Operation!=KeyOperation.Sign||length==null)throw new InvalidOperationException();
            if(output==null){*length=(ulong)(key.Rsa?.Size??(key.Curve==415?72:key.Curve==715?104:139));return 1;}
            ValidatePss(key,c);if(key.Rsa!=null&&c.Padding is not(1 or 6))throw new NotSupportedException();var hash=Input(digest,count);
            byte[] signature=key.Rsa!=null?key.Rsa.SignHash(c.Digest??throw new InvalidOperationException(),hash,c.Padding==6):SignEcHash(key,hash);
            if(*length<(ulong)signature.Length)throw new ArgumentException("Signature buffer too short.");signature.CopyTo(Output(output,signature.Length));*length=(ulong)signature.Length;return 1;
        }
        catch{Error();return 0;}
    }
    public static int EVP_PKEY_verify(void* ctx,byte* signature,ulong signatureSize,byte* digest,ulong count)
    {
        try
        {
            var c=Get<KeyContext>(ctx);var key=c.Key??throw new InvalidOperationException();if(c.Operation!=KeyOperation.Verify)throw new InvalidOperationException();ValidatePss(key,c);if(key.Rsa!=null&&c.Padding is not(1 or 6))throw new NotSupportedException();
            var hash=Input(digest,count);var sig=Input(signature,signatureSize);
            return (key.Rsa!=null?key.Rsa.VerifyHash(c.Digest??throw new InvalidOperationException(),hash,sig,c.Padding==6):VerifyEcHash(key,hash,sig))?1:0;
        }
        catch{Error();return 0;}
    }
    public static int EVP_PKEY_encrypt(void* ctx,byte* output,ulong* length,byte* input,ulong count)=>PkeyCrypt(ctx,output,length,input,count,true);
    public static int EVP_PKEY_decrypt(void* ctx,byte* output,ulong* length,byte* input,ulong count)
    {
        byte[]? result = null;
        try
        {
            KeyContext c;
            try { c = Get<KeyContext>(ctx); }
            catch { Error(); return -2; }
            if (c.Type != RsaId) { Error(); return -2; }
            if (c.Operation != KeyOperation.Decrypt) { Error(); return -1; }
            var key = c.Key?.Rsa;
            if (key == null || length == null) { Error(); return 0; }
            // EVP's AUTOARGLEN query and capacity checks precede all input reads
            // and private operations, even for malformed ciphertext.
            if (output == null) { *length = (ulong)key.Size; return 1; }
            if (*length < (ulong)key.Size) { Error(); return 0; }
            try
            {
                if (c.Padding == 3) result = key.DecryptRaw(Input(input,count));
                else if (c.Padding == 1)
                {
                    var cipher = Input(input,count);
                    if (cipher.Length > key.Size) throw new CryptographicException();
                    byte[] padded = new byte[key.Size];
                    cipher.CopyTo(padded.AsSpan(key.Size - cipher.Length));
                    try
                    {
                        result = new byte[key.Size];
                        if (!key.TryDecryptPkcs1(padded,result,out int written)) throw new CryptographicException();
                        Array.Resize(ref result,written);
                    }
                    finally { CryptographicOperations.ZeroMemory(padded); }
                }
                else throw new NotSupportedException();
                result.CopyTo(Output(output,result.Length));
                *length = (ulong)result.Length;
                return 1;
            }
            // rsa_pmeth preserves negative private-operation returns and leaves
            // the caller's length and output untouched on failure.
            catch { Error(); return -1; }
        }
        catch { Error(); return 0; }
        finally { if (result != null) CryptographicOperations.ZeroMemory(result); }
    }
    private static int PkeyCrypt(void* ctx,byte* output,ulong* length,byte* input,ulong count,bool encrypt)
    {
        byte[]? result=null;
        try
        {
            var c=Get<KeyContext>(ctx);var key=c.Key?.Rsa??throw new InvalidOperationException();if(c.Operation!=(encrypt?KeyOperation.Encrypt:KeyOperation.Decrypt)||length==null)throw new InvalidOperationException();
            if(output==null){*length=(ulong)key.Size;return 1;}
            if(encrypt){if(c.Padding!=1)throw new NotSupportedException();result=key.EncryptPkcs1(Input(input,count));}
            else if(c.Padding==3)result=key.DecryptRaw(Input(input,count));
            else if(c.Padding==1){result=new byte[key.Size];if(!key.TryDecryptPkcs1(Input(input,count),result,out int n))throw new CryptographicException();Array.Resize(ref result,n);}
            else throw new NotSupportedException();
            if(*length<(ulong)result.Length)throw new ArgumentException();result.CopyTo(Output(output,result.Length));*length=(ulong)result.Length;return 1;
        }
        catch{Error();return 0;}
        finally{if(result!=null)CryptographicOperations.ZeroMemory(result);}
    }
    public static int EVP_PKEY_paramgen(void* ctx,void** output)
    {
        try{var c=Get<KeyContext>(ctx);if(c.Operation!=KeyOperation.Paramgen||output==null||c.Type!=EcId||c.Curve==0)throw new ArgumentException();*output=Add(new KeyState{Type=c.Type,Curve=c.Curve});return 1;}catch{Error();return 0;}
    }
    public static int EVP_PKEY_keygen(void* ctx,void** output)
    {
        try
        {
            var c=Get<KeyContext>(ctx);if(c.Operation!=KeyOperation.Keygen||output==null)throw new ArgumentException();
            var state=new KeyState{Type=c.Type,Curve=c.Curve};
            try
            {
                if(c.Type==RsaId)state.Rsa=new RsaKey(RSA.Create(c.RsaBits));
                else state.Exchange=new EcKeyExchange(CurveName(c.Curve));
                *output=Add(state);return 1;
            }
            catch{state.Dispose();throw;}
        }
        catch{Error();return 0;}
    }
    private static Org.BouncyCastle.Asn1.X9.X9ECParameters EcParameters(int curve)
        => Org.BouncyCastle.Asn1.Nist.NistNamedCurves.GetByName(CurveName(curve))??throw new ArgumentException();
    private static byte[] SignEcHash(KeyState key,ReadOnlySpan<byte> hash)
    {
        if(key.Ecdsa!=null)return key.Ecdsa.SignHash(hash);
        if(key.Type!=EcId||key.Exchange==null)throw new CryptographicException("EC private key required.");
        using var signer=ECDsa.Create();signer.ImportFromPem(key.Exchange.ExportPrivatePem());
        return signer.SignHash(hash,DSASignatureFormat.Rfc3279DerSequence);
    }
    private static ECParameters EcPublicParameters(KeyState key)
    {
        byte[] point=key.PublicPoint();ValidateEcPoint(key.Curve,point);
        int width=(point.Length-1)/2;
        return new ECParameters{Curve=EcKeyExchange.Curve(CurveName(key.Curve)),Q=new ECPoint{X=point.AsSpan(1,width).ToArray(),Y=point.AsSpan(1+width,width).ToArray()}};
    }
    private static bool VerifyEcHash(KeyState key,ReadOnlySpan<byte> hash,ReadOnlySpan<byte> signature)
    {
        byte[] point=key.PublicPoint();
        // An infinity or off-curve public point cannot validate a signature.
        // Keep this mathematical rejection separate from API/setup failures.
        try{if(EcParameters(key.Curve).Curve.DecodePoint(point).IsInfinity)return false;}
        catch(ArgumentException){return false;}
        using var verifier=ECDsa.Create(EcPublicParameters(key));
        return verifier.VerifyHash(hash,signature,DSASignatureFormat.Rfc3279DerSequence);
    }
    public static int EVP_PKEY_set1_tls_encodedpoint(void* key,byte* point,ulong length)
    {
        if(length>int.MaxValue)return 0;
        try
        {
            var k=Get<KeyState>(key);
            if(k.Type is not(EcId or X25519Id)||k.Type==EcId&&k.Curve==0)return 0;
            byte[] bytes=Input(point,length).ToArray();
            if(k.Type==X25519Id)
            {
                if(bytes.Length!=32)throw new ArgumentException();
                // The pinned ECX setter assigns a new public-only key. Existing
                // contexts retain the EVP object and observe that replacement.
                k.Exchange?.Dispose();k.Exchange=null;k.Point=bytes;k.Curve=X25519Id;return 1;
            }
            if(k.Type!=EcId)throw new ArgumentException();
            var parameters=EcParameters(k.Curve);int width=(parameters.Curve.FieldSize+7)/8;
            // EC_KEY_oct2key creates its initially absent point before parsing.
            if(k.Point==null&&k.Exchange==null&&k.Ecdsa==null)k.Point=[0];
            if(bytes.Length==0)throw new ArgumentException();
            int tag=bytes[0],form=tag&~1;
            if(form is not (0 or 2 or 4 or 6)||((form==0||form==4)&&(tag&1)!=0))throw new ArgumentException();
            if(form==0)
            {if(bytes.Length!=1)throw new ArgumentException();k.Point=[0];return 1;}
            if(bytes.Length!=1+width*(form==2?1:2))throw new ArgumentException();
            var x=new Org.BouncyCastle.Math.BigInteger(1,bytes,1,width);
            if(x.CompareTo(parameters.Curve.Field.Characteristic)>=0)throw new ArgumentException();
            if(form!=2)
            {
                var y=new Org.BouncyCastle.Math.BigInteger(1,bytes,1+width,width);
                if(y.CompareTo(parameters.Curve.Field.Characteristic)>=0)throw new ArgumentException();
                if(form==6&&y.TestBit(0)!=((tag&1)!=0))throw new ArgumentException();
                // Native affine assignment occurs before the on-curve check.
                // A failed, in-range off-curve assignment changes public state.
                k.Point=(byte[])bytes.Clone();k.Point[0]=4;
                _=parameters.Curve.DecodePoint(k.Point);return 1;
            }
            // The pinned compressed-coordinate decoder clears the error queue
            // after its prefix, length and field-range checks.
            errors?.Clear();
            k.Point=parameters.Curve.DecodePoint(bytes).GetEncoded(false);return 1;
        }
        catch{Error();return 0;}
    }
    public static ulong EVP_PKEY_get1_tls_encodedpoint(void* key,byte** output)
    {
        try
        {
            if(output==null)throw new ArgumentException();var k=Get<KeyState>(key);
            if(k.Type is not(EcId or X25519Id))return 0;
            if(k.Point==null&&k.Exchange==null&&k.Ecdsa==null)return 0;
            byte[] point=k.PublicPoint();byte* allocated=(byte*)NativeMemory.Alloc((nuint)point.Length);
            point.CopyTo(Output(allocated,point.Length));*output=allocated;return (ulong)point.Length;
        }
        catch{Error();return 0;}
    }
    public static void CRYPTO_free(void* pointer,byte* file,int line)=>NativeMemory.Free(pointer);
    public static int EVP_PKEY_derive_set_peer(void* ctx,void* peer)
    {try{var c=Get<KeyContext>(ctx);var p=Get<KeyState>(peer);if(c.Operation!=KeyOperation.Derive||c.Curve!=p.Curve)throw new ArgumentException();c.Peer?.Dispose();c.Peer=p.Retain();return 1;}catch{Error();return 0;}}
    public static int EVP_PKEY_derive(void* ctx,byte* output,ulong* length)
    {
        byte[]? secret=null;
        try
        {
            var c=Get<KeyContext>(ctx);
            if(length==null||c.Operation!=KeyOperation.Derive||c.Key==null||c.Peer==null)throw new ArgumentException();
            int size=c.Curve switch{415=>32,715=>48,716=>66,X25519Id=>32,_=>throw new ArgumentException()};
            if(c.Type==X25519Id)
            {
                if(c.Key.Exchange==null)throw new CryptographicException("X25519 private key required.");
                if(output==null){*length=32;return 1;}
                secret=new byte[32];bool success=c.Key.Exchange.TryDeriveX25519(c.Peer.PublicPoint(),secret);
                secret.CopyTo(Output(output,32));
                // Native writes the actual all-zero agreement result, returns
                // failure without ERR, and preserves the caller's length.
                if(!success)return 0;
                *length=32;return 1;
            }
            if(output==null){*length=(ulong)size;return 1;}
            if(*length>int.MaxValue)throw new ArgumentException();
            if(c.Key.Exchange!=null)secret=c.Key.Exchange.Derive(c.Peer.PublicPoint());
            else if(c.Key.EcPrivate&&c.Key.Ecdsa!=null)
            {
                using var local=ECDiffieHellman.Create();local.ImportPkcs8PrivateKey(c.Key.Ecdsa.ExportPrivateDer(),out _);
                using var peer=ECDiffieHellman.Create(EcPublicParameters(c.Peer));
                secret=local.DeriveRawSecretAgreement(peer.PublicKey);
            }
            else throw new CryptographicException("EC private key required.");
            int written=Math.Min((int)*length,secret.Length);
            if(written==0)return 0;
            secret.AsSpan(0,written).CopyTo(Output(output,written));*length=(ulong)written;return 1;
        }
        catch{Error();return 0;}
        finally{if(secret!=null)CryptographicOperations.ZeroMemory(secret);}
    }
    private static void ValidateEcPoint(int curve,ReadOnlySpan<byte> point)
    {
        int width=curve switch{415=>32,715=>48,716=>66,_=>throw new ArgumentException()};
        if(point.Length!=1+width*2||point[0]!=4)throw new CryptographicException();
        using var check=ECDiffieHellman.Create(new ECParameters{Curve=EcKeyExchange.Curve(CurveName(curve)),Q=new ECPoint{X=point.Slice(1,width).ToArray(),Y=point.Slice(1+width,width).ToArray()}});
        check.ExportParameters(false);
    }
    // A stable borrowed EC_KEY/group identity belongs to the underlying key.
    // Each get1 adds a real KeyState owner; free drops only that acquired owner.
    private sealed class EcView : IDisposable
    {
        public readonly KeyState Key;
        public readonly nint Group;
        public nint Handle;
        public int References;
        public EcView(KeyState key){Key=key;Group=(nint)Add(new CurveDescriptor(key.Curve));}
        public void Dispose()=>Remove((void*)Group);
    }
    private static readonly object EcViewGate=new();
    private sealed record CurveDescriptor(int Nid);
    public static void* EVP_PKEY_get1_EC_KEY(void* key)
    {
        try
        {
            var k=Get<KeyState>(key);if(k.Type!=EcId)throw new ArgumentException();if(k.Curve==0)return null;
            lock(EcViewGate)
            {
                if(k.View==null)
                {
                    var view=new EcView(k);
                    try{view.Handle=(nint)Add(view);}catch{view.Dispose();throw;}
                    k.View=view;
                }
                checked{k.View.References++;}k.Retain();return (void*)k.View.Handle;
            }
        }
        catch{Error();return null;}
    }
    public static void EC_KEY_free(void* key)
    {
        if(key==null)return;
        try
        {
            lock(EcViewGate)
            {
                var view=Get<EcView>(key);if(view.References<=0)throw new ArgumentException();
                view.References--;view.Key.Dispose();
            }
        }
        catch{Error();}
    }
    public static void* EC_KEY_get0_group(void* key){try{return (void*)Get<EcView>(key).Group;}catch{Error();return null;}}
    public static int EC_GROUP_get_curve_name(void* group){try{return Get<CurveDescriptor>(group).Nid;}catch{Error();return 0;}}
    public static int EC_KEY_check_key(void* key)
    {try{var k=Get<EcView>(key).Key;byte[] point=k.PublicPoint();ValidateEcPoint(k.Curve,point);if((k.Exchange!=null||k.EcPrivate)&&!point.AsSpan().SequenceEqual(k.OriginalPoint()))throw new CryptographicException("EC public and private keys differ.");return 1;}catch{Error();return 0;}}

    private static KeyState ParsePrivate(ReadOnlySpan<byte> encoded)
    {
        // Use BC's real ASN.1/key decoder, then BCL key operations where compatible.
        var sequence=Asn1Sequence.GetInstance(encoded.ToArray());
        if(sequence.Count>=3&&sequence[1] is Asn1Sequence)
        {
            var info=PrivateKeyInfo.GetInstance(sequence);var algorithm=info.PrivateKeyAlgorithm;
            var bc=PrivateKeyFactory.CreateKey(info);
            if(bc is RsaPrivateCrtKeyParameters rsa)
                return new KeyState{Type=algorithm.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss)?RsaPssId:RsaId,Rsa=new RsaKey(RSA.Create(DotNetUtilities.ToRSAParameters(rsa))),PssRestrictions=algorithm.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss)&&algorithm.Parameters!=null?RsassaPssParameters.GetInstance(algorithm.Parameters):null};
            if(bc is ECPrivateKeyParameters)
            {
                var ec=ECDsa.Create();try{ec.ImportPkcs8PrivateKey(encoded,out _);var parameters=ec.ExportParameters(false);return new KeyState{Type=EcId,Curve=CurveNid(parameters.Curve),Point=[4,..parameters.Q.X!,..parameters.Q.Y!],Ecdsa=new EcdsaKey(ec),EcPrivate=true};}catch{ec.Dispose();throw;}
            }
            throw new NotSupportedException("Unsupported credential key type.");
        }
        // Traditional PKCS1 RSA and SEC1 EC encodings.
        if(sequence.Count>=9&&sequence[1] is DerInteger)
            return new KeyState{Type=RsaId,Rsa=RsaKey.FromPrivateDer(encoded,true)};
        var ecdsa=ECDsa.Create();try{ecdsa.ImportECPrivateKey(encoded,out _);var p=ecdsa.ExportParameters(false);return new KeyState{Type=EcId,Curve=CurveNid(p.Curve),Point=[4,..p.Q.X!,..p.Q.Y!],Ecdsa=new EcdsaKey(ecdsa),EcPrivate=true};}catch{ecdsa.Dispose();throw;}
    }
    private static KeyState ParsePublic(ReadOnlySpan<byte> encoded)
    {
        var info=SubjectPublicKeyInfo.GetInstance(encoded.ToArray());var bc=PublicKeyFactory.CreateKey(info);
        if(bc is RsaKeyParameters rsa)return new KeyState{Type=info.Algorithm.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss)?RsaPssId:RsaId,Rsa=new RsaKey(RSA.Create(DotNetUtilities.ToRSAParameters(rsa))),PssRestrictions=info.Algorithm.Algorithm.Equals(PkcsObjectIdentifiers.IdRsassaPss)&&info.Algorithm.Parameters!=null?RsassaPssParameters.GetInstance(info.Algorithm.Parameters):null};
        if(bc is ECPublicKeyParameters)
        {
            var ec=ECDsa.Create();try{ec.ImportSubjectPublicKeyInfo(encoded,out _);var p=ec.ExportParameters(false);return new KeyState{Type=EcId,Curve=CurveNid(p.Curve),Point=[4,..p.Q.X!,..p.Q.Y!],Ecdsa=new EcdsaKey(ec)};}catch{ec.Dispose();throw;}
        }
        throw new NotSupportedException();
    }
    public static void* d2i_AutoPrivateKey(void** output,byte** input,long length)=>DecodePrivate(0,output,input,length);
    public static void* d2i_PrivateKey(int type,void** output,byte** input,long length)=>DecodePrivate(type,output,input,length);
    private static void* DecodePrivate(int type,void** output,byte** input,long length)
    {
        KeyState? key=null;
        try
        {
            if(input==null||length<0)throw new ArgumentException();var reader=new AsnReader(Input(*input,(ulong)length).ToArray(),AsnEncodingRules.DER);var encoded=reader.ReadEncodedValue();key=ParsePrivate(encoded.Span);
            if(type!=0&&key.Type!=type)throw new CryptographicException("Key type hint mismatch.");void* handle=Add(key);key=null;
            if(output!=null){if(*output!=null)Remove(*output);*output=handle;}*input+=encoded.Length;return handle;
        }
        catch{key?.Dispose();Error();return null;}
    }
}

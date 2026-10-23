using System.Security.Cryptography;

namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    private enum CipherKind { Rc4, TripleDesCbc, Aes128Cbc, Aes256Cbc, Aes128Gcm, Aes256Gcm, ChaCha20Poly1305 }
    private sealed record CipherDescriptor(CipherKind Kind, int KeySize, int IvSize);
    private static readonly nint[] Ciphers = new[]
    {
        new CipherDescriptor(CipherKind.Rc4,16,0), new(CipherKind.TripleDesCbc,24,8), new(CipherKind.Aes128Cbc,16,16), new(CipherKind.Aes256Cbc,32,16),
        new(CipherKind.Aes128Gcm,16,12), new(CipherKind.Aes256Gcm,32,12), new(CipherKind.ChaCha20Poly1305,32,12)
    }.Select(x=>(nint)Add(x,true)).ToArray();
    public static void* EVP_rc4() => (void*)Ciphers[(int)CipherKind.Rc4];
    public static void* EVP_des_ede3_cbc() => (void*)Ciphers[(int)CipherKind.TripleDesCbc];
    public static void* EVP_aes_128_cbc() => (void*)Ciphers[(int)CipherKind.Aes128Cbc];
    public static void* EVP_aes_256_cbc() => (void*)Ciphers[(int)CipherKind.Aes256Cbc];
    public static void* EVP_aes_128_gcm() => AesGcm.IsSupported ? (void*)Ciphers[(int)CipherKind.Aes128Gcm] : null;
    public static void* EVP_aes_256_gcm() => AesGcm.IsSupported ? (void*)Ciphers[(int)CipherKind.Aes256Gcm] : null;
    public static void* EVP_chacha20_poly1305() => ChaCha20Poly1305.IsSupported ? (void*)Ciphers[(int)CipherKind.ChaCha20Poly1305] : null;
    private sealed class CipherContext : IDisposable
    {
        public CipherDescriptor? Descriptor;
        public byte[]? Key, Iv, Tag;
        public readonly MemoryStream Aad = new();
        public Rc4? Rc4;
        public CbcCipher? Cbc;
        public bool Encrypt, PayloadProcessed, Authenticated, Padding=true;
        public void Dispose()
        {
            Rc4?.Dispose();Cbc?.Dispose();Rc4=null;Cbc=null;
            if(Key!=null)CryptographicOperations.ZeroMemory(Key);if(Iv!=null)CryptographicOperations.ZeroMemory(Iv);if(Tag!=null)CryptographicOperations.ZeroMemory(Tag);
            Key=Iv=Tag=null;Descriptor=null;Aad.SetLength(0);Encrypt=PayloadProcessed=Authenticated=false;Padding=true;
        }
    }
    public static void* EVP_CIPHER_CTX_new() { try { return Add(new CipherContext()); } catch { Error();return null; } }
    public static void EVP_CIPHER_CTX_free(void* ctx) { try { Remove(ctx); } catch { Error(); } }
    public static int EVP_CIPHER_CTX_reset(void* ctx) { try { Get<CipherContext>(ctx).Dispose();return 1; } catch { Error();return 0; } }
    public static int EVP_CIPHER_CTX_set_padding(void* ctx,int padding)
    { try { if(padding!=0)throw new NotSupportedException("s2n owns record padding.");Get<CipherContext>(ctx).Padding=false;return 1; } catch { Error();return 0; } }
    public static int EVP_EncryptInit_ex(void* ctx,void* cipher,void* engine,byte* key,byte* iv) => CipherInit(ctx,cipher,engine,key,iv,true);
    public static int EVP_DecryptInit_ex(void* ctx,void* cipher,void* engine,byte* key,byte* iv) => CipherInit(ctx,cipher,engine,key,iv,false);
    private static int CipherInit(void* ctx,void* cipher,void* engine,byte* key,byte* iv,bool encrypt)
    {
        try
        {
            if(engine!=null)throw new ArgumentException();var state=Get<CipherContext>(ctx);
            if(cipher!=null){state.Dispose();state.Descriptor=Get<CipherDescriptor>(cipher);}
            var descriptor=state.Descriptor??throw new InvalidOperationException();
            if(key!=null)
            {
                if(state.Key!=null)CryptographicOperations.ZeroMemory(state.Key);state.Key=Input(key,(ulong)descriptor.KeySize).ToArray();
                state.Cbc?.Dispose();state.Cbc=null;state.Rc4?.Dispose();state.Rc4=null;
            }
            bool directionChanged=state.Encrypt!=encrypt;state.Encrypt=encrypt;
            if(directionChanged){state.Cbc?.Dispose();state.Cbc=null;}
            if(iv!=null)
            {
                state.Iv=Input(iv,(ulong)descriptor.IvSize).ToArray();state.Cbc?.SetIv(state.Iv);
                state.Aad.SetLength(0);state.PayloadProcessed=state.Authenticated=false;state.Tag=null;
            }
            state.Iv??=new byte[descriptor.IvSize];
            if(state.Key!=null)
            {
                if(descriptor.Kind==CipherKind.Rc4)state.Rc4??=new Rc4(state.Key);
                else if(descriptor.Kind is CipherKind.TripleDesCbc or CipherKind.Aes128Cbc or CipherKind.Aes256Cbc)
                    state.Cbc??=new CbcCipher(descriptor.Kind==CipherKind.TripleDesCbc,encrypt,state.Key,state.Iv);
            }
            return 1;
        }
        catch { Error();return 0; }
    }
    public static int EVP_CIPHER_CTX_ctrl(void* ctx,int type,int argument,void* pointer)
    {
        try
        {
            var state=Get<CipherContext>(ctx);var descriptor=state.Descriptor??throw new InvalidOperationException();
            if(descriptor.Kind is not (CipherKind.Aes128Gcm or CipherKind.Aes256Gcm or CipherKind.ChaCha20Poly1305))throw new ArgumentException();
            switch(type)
            {
                case 0x9: if(argument!=12)throw new ArgumentException("s2n AEAD uses 12-byte nonces.");return 1;
                case 0x10: if(argument!=16||!state.Encrypt||!state.Authenticated||state.Tag==null)throw new ArgumentException();state.Tag.CopyTo(Output(pointer,16));return 1;
                case 0x11: if(argument!=16||state.Encrypt)throw new ArgumentException();state.Tag=Input(pointer,16).ToArray();return 1;
                default: throw new NotSupportedException("Unsupported cipher control.");
            }
        }
        catch { Error();return 0; }
    }
    public static int EVP_EncryptUpdate(void* ctx,byte* output,int* length,byte* input,int count) => CipherUpdate(ctx,output,length,input,count,true);
    public static int EVP_DecryptUpdate(void* ctx,byte* output,int* length,byte* input,int count) => CipherUpdate(ctx,output,length,input,count,false);
    private static int CipherUpdate(void* ctx,byte* output,int* length,byte* input,int count,bool encrypt)
    {
        if(length!=null)*length=0;
        try
        {
            if(count<0||length==null)throw new ArgumentException();var state=Get<CipherContext>(ctx);
            if(state.Encrypt!=encrypt||state.Key==null)throw new InvalidOperationException();
            var bytes=Input(input,(ulong)count);
            if(state.Rc4!=null){state.Rc4.Process(bytes,Output(output,count));*length=count;return 1;}
            if(state.Cbc!=null)
            { if(state.Padding)throw new NotSupportedException("Disable EVP padding for TLS CBC.");state.Cbc.Process(bytes,Output(output,count));*length=count;return 1; }
            if(output==null){if(state.PayloadProcessed)throw new InvalidOperationException();state.Aad.Write(bytes);*length=count;return 1;}
            // This is the exact one-payload-Update pattern used by s2n's record wrappers.
            // General streaming EVP AEAD callers are not silently approximated.
            if(state.PayloadProcessed)throw new NotSupportedException("s2n AEAD requires one record payload Update.");
            state.PayloadProcessed=true;var algorithm=state.Descriptor!.Kind==CipherKind.ChaCha20Poly1305?AeadAlgorithm.ChaCha20Poly1305:AeadAlgorithm.AesGcm;
            if(encrypt)
            {
                state.Tag=new byte[16];Aead.Encrypt(algorithm,state.Key,state.Iv!,bytes,state.Aad.GetBuffer().AsSpan(0,(int)state.Aad.Length),Output(output,count),state.Tag);state.Authenticated=true;
            }
            else
            {
                if(state.Tag==null)throw new InvalidOperationException("Set the expected tag before payload Update.");
                state.Authenticated=Aead.TryDecrypt(algorithm,state.Key,state.Iv!,bytes,state.Aad.GetBuffer().AsSpan(0,(int)state.Aad.Length),state.Tag,Output(output,count));
                if(!state.Authenticated){Error();return 0;}
            }
            *length=count;return 1;
        }
        catch { Error();return 0; }
    }
    public static int EVP_EncryptFinal_ex(void* ctx,byte* output,int* length) => CipherFinal(ctx,length,true);
    public static int EVP_DecryptFinal_ex(void* ctx,byte* output,int* length) => CipherFinal(ctx,length,false);
    private static int CipherFinal(void* ctx,int* length,bool encrypt)
    {
        if(length!=null)*length=0;
        try
        {
            if(length==null)throw new ArgumentException();var state=Get<CipherContext>(ctx);if(state.Encrypt!=encrypt)throw new ArgumentException();
            if(state.Rc4!=null||state.Cbc!=null)return 1;
            if(!state.PayloadProcessed||!state.Authenticated)throw new CryptographicException("AEAD did not authenticate.");
            return 1;
        }
        catch { Error();return 0; }
    }
}

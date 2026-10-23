using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Managed.S2nTls.Crypto;

/// <summary>Authored OpenSSL-1.1.1 declaration adapter. No native crypto imports.</summary>
public static unsafe partial class OpenSslAbi
{
    private static class Registry { internal static readonly ConcurrentDictionary<nint, object> Table = new(); }
    private static ConcurrentDictionary<nint,object> Handles => Registry.Table;
    [ThreadStatic] private static Queue<ulong>? errors;
    private static int transientHandles;
    public static int LiveContexts => Volatile.Read(ref transientHandles);
    private static void Error()
    {
        var queue=errors??=new();if(queue.Count>=16)queue.Dequeue();queue.Enqueue(0x80000001UL);
    }
    public static ulong ERR_get_error() => errors is { Count: > 0 } ? errors.Dequeue() : 0;
    internal static void* Add(object state, bool permanent = false)
    {
        void* pointer = NativeMemory.AllocZeroed((nuint)IntPtr.Size);
        if (pointer == null) throw new OutOfMemoryException();
        if (!Handles.TryAdd((nint)pointer, state)) { NativeMemory.Free(pointer); throw new InvalidOperationException(); }
        if (!permanent) Interlocked.Increment(ref transientHandles);
        return pointer;
    }
    internal static T Get<T>(void* pointer) where T : class => pointer != null && Handles.TryGetValue((nint)pointer, out var value) && value is T typed
        ? typed : throw new ArgumentException("Invalid or expired provider handle.");
    internal static void Remove(void* pointer)
    {
        if (pointer == null) return;
        if (!Handles.TryRemove((nint)pointer, out var value)) throw new ArgumentException("Expired provider handle.");
        try { (value as IDisposable)?.Dispose(); }
        finally { NativeMemory.Free(pointer); Interlocked.Decrement(ref transientHandles); }
    }
    internal static ReadOnlySpan<byte> Input(void* pointer, ulong length)
    {
        if (length > int.MaxValue || pointer == null && length != 0) throw new ArgumentException("Invalid input buffer.");
        return new ReadOnlySpan<byte>(pointer, (int)length);
    }
    internal static Span<byte> Output(void* pointer, int length)
    {
        if (length < 0 || pointer == null && length != 0) throw new ArgumentException("Invalid output buffer.");
        return new Span<byte>(pointer, length);
    }
    internal static byte* String(string value)
    {
        byte[] text = Encoding.UTF8.GetBytes(value + "\0"); byte* data = (byte*)NativeMemory.Alloc((nuint)text.Length); text.CopyTo(new Span<byte>(data,text.Length)); return data;
    }
    private static readonly byte* ProviderName = String("DotCC managed s2n crypto / OpenSSL 1.1.1 declaration ABI / BCL + BouncyCastle");
    public static uint dotcc_crypto_provider_abi() => 1;
    // All eight selected s2n operation groups have native differential and JIT/AOT
    // evidence. This mask describes the declared s2n boundary, not all OpenSSL APIs.
    public static ulong dotcc_crypto_provider_capabilities() => 0xff;
    public static byte* dotcc_crypto_provider_name() => ProviderName;
    private static readonly Lazy<bool> Initialized = new(() =>
    {
        // Validate the real registered primitive implementations; no OpenSSL engine/config loading.
        foreach(var algorithm in Enum.GetValues<DigestAlgorithm>())
        {using var hash=new Digest(algorithm);using var clone=hash.Clone();if(!hash.FinalizeHash().AsSpan().SequenceEqual(clone.FinalizeHash()))return false;}
        if(!AesGcm.IsSupported||!ChaCha20Poly1305.IsSupported)return false;
        using var rc4=new Rc4(new byte[16]);byte[] plain=new byte[16],cipher=new byte[16];rc4.Process(plain,cipher);
        using var aes=new CbcCipher(false,true,new byte[16],new byte[16]);aes.Process(plain,cipher);
        using var des=new CbcCipher(true,true,new byte[24],new byte[8]);des.Process(plain,cipher);
        return true;
    });
    public static int OPENSSL_init_crypto(ulong options,void* settings)
    {try{if(settings!=null||(options&~0x0eUL)!=0)throw new NotSupportedException("Only digest/cipher/string registration is used by s2n.");return Initialized.Value?1:0;}catch{Error();return 0;}}

    private sealed record DigestDescriptor(DigestAlgorithm Algorithm);
    private static readonly nint[] Digests = Enum.GetValues<DigestAlgorithm>().Select(x => (nint)Add(new DigestDescriptor(x), true)).ToArray();
    public static void* EVP_md5() => (void*)Digests[(int)DigestAlgorithm.Md5];
    public static void* EVP_sha1() => (void*)Digests[(int)DigestAlgorithm.Sha1];
    public static void* EVP_sha224() => (void*)Digests[(int)DigestAlgorithm.Sha224];
    public static void* EVP_sha256() => (void*)Digests[(int)DigestAlgorithm.Sha256];
    public static void* EVP_sha384() => (void*)Digests[(int)DigestAlgorithm.Sha384];
    public static void* EVP_sha512() => (void*)Digests[(int)DigestAlgorithm.Sha512];
    public static void* EVP_md5_sha1() => (void*)Digests[(int)DigestAlgorithm.Md5Sha1];
    public static void* EVP_shake256() => (void*)Digests[(int)DigestAlgorithm.Shake256_64];
    private sealed class DigestContext : IDisposable
    {
        public nint Descriptor;
        public nint KeyContext;
        public Digest? Hash;
        public void Dispose() { Hash?.Dispose(); Hash = null; Descriptor = KeyContext = 0; }
    }
    public static void* EVP_MD_CTX_new() { try { return Add(new DigestContext()); } catch { Error(); return null; } }
    public static void EVP_MD_CTX_free(void* ctx) { try { Remove(ctx); } catch { Error(); } }
    public static int EVP_MD_CTX_reset(void* ctx) { try { Get<DigestContext>(ctx).Dispose(); return 1; } catch { Error(); return 0; } }
    public static void* EVP_MD_CTX_md(void* ctx) { try { return (void*)Get<DigestContext>(ctx).Descriptor; } catch { Error(); return null; } }
    public static int EVP_MD_size(void* descriptor) { try { using var hash = new Digest(Get<DigestDescriptor>(descriptor).Algorithm); return hash.Algorithm==DigestAlgorithm.Shake256_64?32:hash.Size; } catch { Error(); return -1; } }
    public static int EVP_DigestInit_ex(void* ctx, void* descriptor, void* engine)
    {
        try
        {
            if (engine != null) throw new ArgumentException("Native engines are not managed providers.");
            var state = Get<DigestContext>(ctx); var algorithm = Get<DigestDescriptor>(descriptor == null ? (void*)state.Descriptor : descriptor).Algorithm;
            state.Hash?.Dispose(); state.Hash = new Digest(algorithm); state.Descriptor = descriptor == null ? state.Descriptor : (nint)descriptor; return 1;
        }
        catch { Error(); return 0; }
    }
    public static int EVP_DigestUpdate(void* ctx, void* input, ulong length)
    { try { (Get<DigestContext>(ctx).Hash ?? throw new InvalidOperationException()).Update(Input(input,length)); return 1; } catch { Error(); return 0; } }
    public static int EVP_DigestFinal_ex(void* ctx, byte* output, uint* size)
    {
        try
        {
            var hash = Get<DigestContext>(ctx).Hash ?? throw new InvalidOperationException();
            if (output == null) throw new ArgumentException();
            byte[] result = hash.Algorithm==DigestAlgorithm.Shake256_64?hash.FinalizeXof(32):hash.FinalizeHash(); result.CopyTo(Output(output,result.Length)); if (size != null) *size = (uint)result.Length; return 1;
        }
        catch { if(size!=null)*size=0; Error(); return 0; }
    }
    public static int EVP_DigestFinalXOF(void* ctx,byte* output,ulong size)
    {
        try{if(size>int.MaxValue)throw new ArgumentException();var destination=Output(output,(int)size);var hash=Get<DigestContext>(ctx).Hash??throw new InvalidOperationException();hash.FinalizeXof((int)size).CopyTo(destination);return 1;}catch{Error();return 0;}
    }
    public static int EVP_MD_CTX_copy_ex(void* destination, void* source)
    {
        try
        {
            var from=Get<DigestContext>(source);var to=Get<DigestContext>(destination);
            if(from==to)return 1;
            var copy=from.Hash?.Clone();to.Dispose();to.Descriptor=from.Descriptor;to.KeyContext=from.KeyContext;to.Hash=copy;return 1;
        }
        catch { Error(); return 0; }
    }
    public static int RAND_bytes(byte* bytes,int count) { try { RandomNumberGenerator.Fill(Output(bytes,count));return 1; } catch { Error();return 0; } }
    public static int RAND_priv_bytes(byte* bytes,int count) => RAND_bytes(bytes,count);
    public static void EVP_MD_CTX_set_pkey_ctx(void* ctx,void* keyContext)
    {try{if(keyContext!=null)_=Get<KeyContext>(keyContext);Get<DigestContext>(ctx).KeyContext=(nint)keyContext;}catch{Error();}}
    public static int EVP_DigestSignFinal(void* ctx,byte* signature,ulong* length)
    {
        try
        {
            var state=Get<DigestContext>(ctx);using var hash=state.Hash?.Clone()??throw new InvalidOperationException();
            byte[] digest=hash.FinalizeHash();try{fixed(byte* d=digest)return EVP_PKEY_sign((void*)state.KeyContext,signature,length,d,(ulong)digest.Length);}finally{CryptographicOperations.ZeroMemory(digest);}
        }
        catch{Error();return 0;}
    }
    public static int EVP_DigestVerifyFinal(void* ctx,byte* signature,ulong length)
    {
        try
        {
            var state=Get<DigestContext>(ctx);using var hash=state.Hash?.Clone()??throw new InvalidOperationException();
            byte[] digest=hash.FinalizeHash();try{fixed(byte* d=digest)return EVP_PKEY_verify((void*)state.KeyContext,signature,length,d,(ulong)digest.Length);}finally{CryptographicOperations.ZeroMemory(digest);}
        }
        catch{Error();return 0;}
    }
    public static int EVP_EncodeBlock(byte* output,byte* input,int length)
    {
        try
        {
            string text=Convert.ToBase64String(Input(input,(ulong)length));int size=Encoding.ASCII.GetBytes(text,Output(output,text.Length));output[size]=0;return size;
        }
        catch { Error();return -1; }
    }
    public static int EVP_DecodeBlock(byte* output,byte* input,int length)
    {
        try
        {
            if(length<0)return -1;
            var encoded=Input(input,(ulong)length);int start=0,end=encoded.Length;
            // Match the pinned block API: leading space/tab only, then its
            // line-ending/EOF trim. Interior whitespace remains an error.
            while(start<end&&DecodeBlockValue(encoded[start])==0xe0)start++;
            while(end-start>3&&(DecodeBlockValue(encoded[end-1]) is 0xe0 or 0xf0 or 0xf1 or 0xf2))end--;
            int count=end-start;if(count%4!=0)return -1;
            var decoded=Output(output,count/4*3);int written=0;
            for(int offset=start;offset<end;offset+=4)
            {
                int a=DecodeBlockValue(encoded[offset]),b=DecodeBlockValue(encoded[offset+1]),
                    c=DecodeBlockValue(encoded[offset+2]),d=DecodeBlockValue(encoded[offset+3]);
                if(((a|b|c|d)&0x80)!=0)return -1;
                // '=' is zero in this native block API, including interior
                // slots. s2n handles its trailing padding/cursors itself.
                int word=(a<<18)|(b<<12)|(c<<6)|d;
                decoded[written++]=(byte)(word>>16);
                decoded[written++]=(byte)((word>>8)&0xff);
                decoded[written++]=(byte)(word&0xff);
            }
            // Native format rejection does not enqueue an OpenSSL error.
            return written;
        }
        catch { Error();return -1; }
    }
    private static int DecodeBlockValue(byte value)
    {
        if(value>='A'&&value<='Z')return value-'A';
        if(value>='a'&&value<='z')return value-'a'+26;
        if(value>='0'&&value<='9')return value-'0'+52;
        return value switch{(byte)'+'=>62,(byte)'/'=>63,(byte)'='=>0,
            (byte)' ' or (byte)'\t'=>0xe0,(byte)'\n'=>0xf0,(byte)'\r'=>0xf1,(byte)'-'=>0xf2,_=>0xff};
    }
}

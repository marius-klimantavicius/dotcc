using static Managed.Security.PicoTls;
using System.Runtime.InteropServices;
using Managed.Security;

// Every ptls header below is an actual public type from the translated product.
// Only the two provider-extension wrappers are authored here, just as in C.
[StructLayout(LayoutKind.Sequential)]
struct HashWithHandle { public st_ptls_hash_context_t header; public nint handle; }
[StructLayout(LayoutKind.Sequential)]
struct AeadWithHandle { public st_ptls_aead_context_t header; public nint handle; }
[StructLayout(LayoutKind.Sequential)]
struct Alignment<T> where T : unmanaged { public byte prefix; public T value; }

static unsafe class Program
{
    static readonly Dictionary<string, long> Actual = new(StringComparer.Ordinal);
    static void Put(string name, long value) => Actual.Add(name, value);
    static void Header<T>(string name) where T : unmanaged
    {
        Alignment<T> prefix = default;
        Put(name + ".size", sizeof(T));
        Put(name + ".align", (byte*)&prefix.value - (byte*)&prefix);
    }

    static void Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Expected native-layout.txt path");
        var native = File.ReadAllLines(args[0]).Select(line => line.Split('='))
            .ToDictionary(parts => parts[0], parts => long.Parse(parts[1]), StringComparer.Ordinal);
        Iovec(); Buffer(); Hash(); Cipher(); Aead(); KeyExchange(); Verification(); Extensions();
        if (!native.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(Actual.Keys))
            throw new InvalidOperationException("Native and actual translated ABI check inventories differ");
        foreach (var (name, expected) in native)
        {
            if (Actual[name] != expected)
                throw new InvalidOperationException($"ABI {name}: native={expected}, emitted storage={Actual[name]}");
        }
        Console.WriteLine($"PASS actual translated product ABI: {Actual.Count} size/alignment/member-address checks");
    }

    static void Iovec()
    {
        const string n = "ptls_iovec_t";
        Header<st_ptls_iovec_t>(n); st_ptls_iovec_t value = default;
        Put(n + ".base", (byte*)&value.@base - (byte*)&value);
        Put(n + ".len", (byte*)&value.len - (byte*)&value);
    }
    static void Buffer()
    {
        const string n = "ptls_buffer_t";
        Header<st_ptls_buffer_t>(n); st_ptls_buffer_t value = default;
        Put(n + ".base", (byte*)&value.@base - (byte*)&value);
        Put(n + ".capacity", (byte*)&value.capacity - (byte*)&value);
        Put(n + ".off", (byte*)&value.off - (byte*)&value);
        Put(n + ".is_allocated", (byte*)&value.is_allocated - (byte*)&value);
        Put(n + ".align_bits", (byte*)&value.align_bits - (byte*)&value);
    }
    static void Hash()
    {
        const string c = "ptls_hash_context_t", a = "ptls_hash_algorithm_t";
        Header<st_ptls_hash_context_t>(c); st_ptls_hash_context_t context = default;
        Put(c + ".update", (byte*)&context.update - (byte*)&context);
        Put(c + ".final", (byte*)&context.final - (byte*)&context);
        Put(c + ".clone_", (byte*)&context.clone_ - (byte*)&context);
        Header<st_ptls_hash_algorithm_t>(a); st_ptls_hash_algorithm_t algo = default;
        Put(a + ".name", (byte*)&algo.name - (byte*)&algo);
        Put(a + ".block_size", (byte*)&algo.block_size - (byte*)&algo);
        Put(a + ".digest_size", (byte*)&algo.digest_size - (byte*)&algo);
        Put(a + ".create", (byte*)&algo.create - (byte*)&algo);
        Put(a + ".empty_digest", (byte*)&algo.empty_digest[0] - (byte*)&algo);
    }
    static void Cipher()
    {
        const string c = "ptls_cipher_context_t", a = "ptls_cipher_algorithm_t";
        Header<st_ptls_cipher_context_t>(c); st_ptls_cipher_context_t context = default;
        Put(c + ".algo", (byte*)&context.algo - (byte*)&context);
        Put(c + ".do_dispose", (byte*)&context.do_dispose - (byte*)&context);
        Put(c + ".do_init", (byte*)&context.do_init - (byte*)&context);
        Put(c + ".do_transform", (byte*)&context.do_transform - (byte*)&context);
        Header<st_ptls_cipher_algorithm_t>(a); st_ptls_cipher_algorithm_t algo = default;
        Put(a + ".name", (byte*)&algo.name - (byte*)&algo);
        Put(a + ".key_size", (byte*)&algo.key_size - (byte*)&algo);
        Put(a + ".block_size", (byte*)&algo.block_size - (byte*)&algo);
        Put(a + ".iv_size", (byte*)&algo.iv_size - (byte*)&algo);
        Put(a + ".context_size", (byte*)&algo.context_size - (byte*)&algo);
        Put(a + ".setup_crypto", (byte*)&algo.setup_crypto - (byte*)&algo);
    }
    static void Aead()
    {
        const string c = "ptls_aead_context_t", a = "ptls_aead_algorithm_t", s = "ptls_aead_supplementary_encryption_t";
        Header<st_ptls_aead_context_t>(c); st_ptls_aead_context_t context = default;
        Put(c + ".algo", (byte*)&context.algo - (byte*)&context);
        Put(c + ".dispose_crypto", (byte*)&context.dispose_crypto - (byte*)&context);
        Put(c + ".do_get_iv", (byte*)&context.do_get_iv - (byte*)&context);
        Put(c + ".do_set_iv", (byte*)&context.do_set_iv - (byte*)&context);
        Put(c + ".do_encrypt_init", (byte*)&context.do_encrypt_init - (byte*)&context);
        Put(c + ".do_encrypt_update", (byte*)&context.do_encrypt_update - (byte*)&context);
        Put(c + ".do_encrypt_final", (byte*)&context.do_encrypt_final - (byte*)&context);
        Put(c + ".do_encrypt", (byte*)&context.do_encrypt - (byte*)&context);
        Put(c + ".do_encrypt_v", (byte*)&context.do_encrypt_v - (byte*)&context);
        Put(c + ".do_decrypt", (byte*)&context.do_decrypt - (byte*)&context);
        Header<st_ptls_aead_algorithm_t>(a); st_ptls_aead_algorithm_t algo = default;
        Put(a + ".name", (byte*)&algo.name - (byte*)&algo);
        Put(a + ".confidentiality_limit", (byte*)&algo.confidentiality_limit - (byte*)&algo);
        Put(a + ".integrity_limit", (byte*)&algo.integrity_limit - (byte*)&algo);
        Put(a + ".ctr_cipher", (byte*)&algo.ctr_cipher - (byte*)&algo);
        Put(a + ".ecb_cipher", (byte*)&algo.ecb_cipher - (byte*)&algo);
        Put(a + ".key_size", (byte*)&algo.key_size - (byte*)&algo);
        Put(a + ".iv_size", (byte*)&algo.iv_size - (byte*)&algo);
        Put(a + ".tag_size", (byte*)&algo.tag_size - (byte*)&algo);
        Put(a + ".tls12", (byte*)&algo.tls12 - (byte*)&algo);
        Put(a + ".align_bits", (byte*)&algo.align_bits - (byte*)&algo);
        Put(a + ".context_size", (byte*)&algo.context_size - (byte*)&algo);
        Put(a + ".setup_crypto", (byte*)&algo.setup_crypto - (byte*)&algo);
        Header<st_ptls_aead_supplementary_encryption_t>(s); st_ptls_aead_supplementary_encryption_t supplement = default;
        Put(s + ".ctx", (byte*)&supplement.ctx - (byte*)&supplement);
        Put(s + ".input", (byte*)&supplement.input - (byte*)&supplement);
        Put(s + ".output", (byte*)&supplement.output[0] - (byte*)&supplement);
    }
    static void KeyExchange()
    {
        const string c = "ptls_key_exchange_context_t", a = "ptls_key_exchange_algorithm_t";
        Header<st_ptls_key_exchange_context_t>(c); st_ptls_key_exchange_context_t context = default;
        Put(c + ".algo", (byte*)&context.algo - (byte*)&context);
        Put(c + ".pubkey", (byte*)&context.pubkey - (byte*)&context);
        Put(c + ".on_exchange", (byte*)&context.on_exchange - (byte*)&context);
        Header<st_ptls_key_exchange_algorithm_t>(a); st_ptls_key_exchange_algorithm_t algo = default;
        Put(a + ".id", (byte*)&algo.id - (byte*)&algo);
        Put(a + ".create", (byte*)&algo.create - (byte*)&algo);
        Put(a + ".exchange", (byte*)&algo.exchange - (byte*)&algo);
        Put(a + ".data", (byte*)&algo.data - (byte*)&algo);
        Put(a + ".name", (byte*)&algo.name - (byte*)&algo);
    }
    static void Verification()
    {
        const string n = "ptls_verify_certificate_t";
        Header<st_ptls_verify_certificate_t>(n); st_ptls_verify_certificate_t value = default;
        Put(n + ".cb", (byte*)&value.cb - (byte*)&value);
        Put(n + ".algos", (byte*)&value.algos - (byte*)&value);
    }
    static void Extensions()
    {
        const string h = "struct hash_with_handle", a = "struct aead_with_handle";
        Header<HashWithHandle>(h); HashWithHandle hash = default;
        Put(h + ".header", (byte*)&hash.header - (byte*)&hash);
        Put(h + ".handle", (byte*)&hash.handle - (byte*)&hash);
        Header<AeadWithHandle>(a); AeadWithHandle aead = default;
        Put(a + ".header", (byte*)&aead.header - (byte*)&aead);
        Put(a + ".handle", (byte*)&aead.handle - (byte*)&aead);
    }
}

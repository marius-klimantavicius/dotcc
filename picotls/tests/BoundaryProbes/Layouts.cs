using System.Runtime.InteropServices;

// Hand-authored mirrors for feasibility only. P2 must compare emitted dotcc types.
[StructLayout(LayoutKind.Sequential)] struct ptls_iovec_t { public nint @base; public nuint len; }
[StructLayout(LayoutKind.Sequential)] struct ptls_buffer_t { public nint @base; public nuint capacity, off; public byte is_allocated, align_bits; }
[StructLayout(LayoutKind.Sequential)] struct ptls_hash_context_t { public nint update, final, clone_; }
[StructLayout(LayoutKind.Sequential)] unsafe struct ptls_hash_algorithm_t { public nint name; public nuint block_size, digest_size; public nint create; public fixed byte empty_digest[64]; }
[StructLayout(LayoutKind.Sequential)] struct ptls_cipher_context_t { public nint algo, do_dispose, do_init, do_transform; }
[StructLayout(LayoutKind.Sequential)] struct ptls_cipher_algorithm_t { public nint name; public nuint key_size, block_size, iv_size, context_size; public nint setup_crypto; }
[StructLayout(LayoutKind.Sequential)] struct ptls_aead_context_t { public nint algo, dispose_crypto, do_get_iv, do_set_iv, do_encrypt_init, do_encrypt_update, do_encrypt_final, do_encrypt, do_encrypt_v, do_decrypt; }
[StructLayout(LayoutKind.Sequential)] struct Tls12 { public nuint fixed_iv_size, record_iv_size; }
[StructLayout(LayoutKind.Sequential)] struct ptls_aead_algorithm_t {
    public nint name; public ulong confidentiality_limit, integrity_limit; public nint ctr_cipher, ecb_cipher;
    public nuint key_size, iv_size, tag_size; public Tls12 tls12;
    // Native unsigned bitfield consumes bit zero; native offsetof proves the following byte placement.
    public byte non_temporal_storage, align_bits; public nuint context_size; public nint setup_crypto;
}
[StructLayout(LayoutKind.Sequential)] unsafe struct ptls_aead_supplementary_encryption_t { public nint ctx, input; public fixed byte output[16]; }
[StructLayout(LayoutKind.Sequential)] struct ptls_key_exchange_context_t { public nint algo; public ptls_iovec_t pubkey; public nint on_exchange; }
[StructLayout(LayoutKind.Sequential)] struct ptls_key_exchange_algorithm_t { public ushort id; public nint create, exchange, data, name; }
[StructLayout(LayoutKind.Sequential)] struct ptls_verify_certificate_t { public nint cb, algos; }
[StructLayout(LayoutKind.Sequential)] struct hash_with_handle { public ptls_hash_context_t header; public nint handle; }
[StructLayout(LayoutKind.Sequential)] struct aead_with_handle { public ptls_aead_context_t header; public nint handle; }
[StructLayout(LayoutKind.Sequential)] struct Alignment<T> where T : unmanaged { public byte prefix; public T value; }

static class Layouts {
    public static void Check(string path) {
        var native = File.ReadAllLines(path).Select(x => x.Split('=')).ToDictionary(x => x[0], x => int.Parse(x[1]));
        Check<ptls_iovec_t>(native); Check<ptls_buffer_t>(native); Check<ptls_hash_context_t>(native); Check<ptls_hash_algorithm_t>(native);
        Check<ptls_cipher_context_t>(native); Check<ptls_cipher_algorithm_t>(native); Check<ptls_aead_context_t>(native);
        Check<ptls_aead_algorithm_t>(native); Check<ptls_aead_supplementary_encryption_t>(native);
        Check<ptls_key_exchange_context_t>(native); Check<ptls_key_exchange_algorithm_t>(native); Check<ptls_verify_certificate_t>(native);
        Check<hash_with_handle>(native, "struct hash_with_handle"); Check<aead_with_handle>(native, "struct aead_with_handle");
        Console.WriteLine($"PASS native/C# mirror ABI: {native.Count} size/alignment/offset checks (not emitted dotcc types)");
    }
    static unsafe void Check<T>(Dictionary<string, int> native, string? name = null) where T : unmanaged {
        name ??= typeof(T).Name;
        foreach (var (key, expected) in native.Where(x => x.Key.StartsWith(name + ".", StringComparison.Ordinal))) {
            string field = key[(name.Length + 1)..];
            int actual = field switch { "size" => sizeof(T), "align" => (int)Marshal.OffsetOf<Alignment<T>>("value"), _ => (int)Marshal.OffsetOf<T>(field) };
            if (actual != expected) throw new Exception($"ABI {key}: native={expected}, C#={actual}");
        }
    }
}

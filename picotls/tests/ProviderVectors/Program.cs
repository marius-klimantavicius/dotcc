using System.Security.Cryptography;
using Managed.Security;

static unsafe class Program
{
    static int checks;
    internal static void Check(bool condition, string name)
    { if (!condition) throw new InvalidOperationException(name); checks++; }
    internal static byte[] Hex(string hex) => Convert.FromHexString(hex);
    internal static void Equal(ReadOnlySpan<byte> actual, ReadOnlySpan<byte> expected, string name)
    { Check(actual.SequenceEqual(expected), name); }
    internal static st_ptls_iovec_t Vector(byte* data, int size) => new() { @base = data, len = (ulong)size };
    static void Main()
    {
        Parallel.For(0, 8, _ => BclCryptoProvider.InitializeSymmetric());
        using (var scope = CallbackScope.Enter())
        {
            var suites = BclCryptoProvider.SymmetricCipherSuites;
            Check(suites[0]->id == 0x1301 && suites[1]->id == 0x1302 && suites[2] == null, "complete terminated suites");
            Hashes(scope); Hkdf(scope); BlockCiphers(scope); Aead(scope);
            scope.ThrowIfFailed();
        }
        ErrorAndLifetimeChecks(); AllocationFailures(); AsymmetricVectors.Run(); TicketVectors.Run(); HandshakeAllocationVectors.Run();
        Check(BclCryptoProvider.LiveManagedContexts == 0, "all provider GCHandles released");
        Console.WriteLine($"PASS BCL provider: {checks} checks over actual translated core; SHA256/SHA384, RFC5869 HKDF, AES128/256 ECB/CTR/GCM, overlap/vector/IV/supplementary, negatives, lifetime and asymmetric callbacks");
    }
    static byte[] Digest(st_ptls_hash_context_t* context, int mode, int size, CallbackScope scope)
    {
        var result = new byte[size];
        fixed (byte* output = result) context->final(context, output, (en_ptls_hash_final_mode_t)mode);
        scope.ThrowIfFailed(); return result;
    }
    static void Feed(st_ptls_hash_context_t* context, ReadOnlySpan<byte> bytes, CallbackScope scope)
    {
        fixed (byte* input = bytes) context->update(context, input, (ulong)bytes.Length);
        scope.ThrowIfFailed();
    }
    static void Hashes(CallbackScope scope)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            var algorithm = variant == 0 ? BclCryptoProvider.HashSha256 : BclCryptoProvider.HashSha384;
            int size = (int)algorithm->digest_size;
            byte[] expected = Hex(variant == 0 ? "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad" : "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7");
            var context = algorithm->create(); scope.ThrowIfFailed();
            var clone = (st_ptls_hash_context_t*)null;
            try
            {
                Feed(context, "ab"u8, scope); clone = context->clone_(context); scope.ThrowIfFailed();
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                Feed(context, "c"u8, scope); Feed(clone, "c"u8, scope);
                Equal(Digest(context, 2, size, scope), expected, "published SHA abc snapshot");
                Equal(Digest(clone, 0, size, scope), expected, "independent SHA clone/free"); clone = null;
                Equal(Digest(context, 1, size, scope), expected, "SHA reset result");
                Equal(Digest(context, 2, size, scope), new ReadOnlySpan<byte>(algorithm->empty_digest, size), "SHA reset empty state");
                Feed(context, "abc"u8, scope);
                context->final(context, null, (en_ptls_hash_final_mode_t)2); scope.ThrowIfFailed();
                Equal(Digest(context, 2, size, scope), expected, "null snapshot retains data");
                context->final(context, null, (en_ptls_hash_final_mode_t)1); scope.ThrowIfFailed();
                Equal(Digest(context, 2, size, scope), new ReadOnlySpan<byte>(algorithm->empty_digest, size), "null reset clears state");
            }
            finally
            {
                if (clone != null) clone->final(clone, null, (en_ptls_hash_final_mode_t)0);
                context->final(context, null, (en_ptls_hash_final_mode_t)0);
            }
        }
    }
    static void Hkdf(CallbackScope scope)
    {
        byte[] ikm = Enumerable.Repeat((byte)0x0b, 22).ToArray(), salt = Hex("000102030405060708090a0b0c"), info = Hex("f0f1f2f3f4f5f6f7f8f9");
        byte[] prk = new byte[32], okm = new byte[42];
        fixed (byte* i = ikm, s = salt, f = info, p = prk, o = okm)
        {
            Check(Picotls.ptls_hkdf_extract(BclCryptoProvider.HashSha256, p, Vector(s, salt.Length), Vector(i, ikm.Length)) == 0, "translated HKDF extract status");
            scope.ThrowIfFailed();
            Check(Picotls.ptls_hkdf_expand(BclCryptoProvider.HashSha256, o, (ulong)okm.Length, Vector(p, prk.Length), Vector(f, info.Length)) == 0, "translated HKDF expand status");
            scope.ThrowIfFailed();
        }
        Equal(prk, Hex("077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5"), "RFC5869 case1 PRK");
        Equal(okm, Hex("3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865"), "RFC5869 case1 OKM");
    }
    static void BlockCiphers(CallbackScope scope)
    {
        var plaintext = Hex("00112233445566778899aabbccddeeff");
        for (int variant = 0; variant < 2; variant++)
        {
            byte[] key = Enumerable.Range(0, variant == 0 ? 16 : 32).Select(x => (byte)x).ToArray();
            byte[] expected = Hex(variant == 0 ? "69c4e0d86a7b0430d8cdb78070b4c55a" : "8ea2b7ca516745bfeafc49904b496089");
            var algorithm = variant == 0 ? BclCryptoProvider.Aes128Ecb : BclCryptoProvider.Aes256Ecb;
            byte[] output = new byte[17];
            fixed (byte* k = key, p = plaintext, o = output)
            {
                var encrypt = Picotls.ptls_cipher_new(algorithm, 1, k); scope.ThrowIfFailed();
                var decrypt = Picotls.ptls_cipher_new(algorithm, 0, k); scope.ThrowIfFailed();
                try
                {
                    encrypt->do_transform(encrypt, o, p, 16); scope.ThrowIfFailed();
                    Equal(output.AsSpan(0, 16), expected, "FIPS197 AES ECB encrypt");
                    decrypt->do_transform(decrypt, o + 1, o, 16); scope.ThrowIfFailed();
                    Equal(output.AsSpan(1, 16), plaintext, "AES ECB decrypt partial overlap");
                }
                finally { Picotls.ptls_cipher_free(encrypt); Picotls.ptls_cipher_free(decrypt); }
            }
        }
        byte[] ctrPlaintext = Hex("6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e5130c81c46a35ce411e5fbc1191a0a52eff69f2445df4f9b17ad2b417be66c3710");
        byte[] iv = Hex("f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff");
        for (int variant = 0; variant < 2; variant++)
        {
            byte[] key = Hex(variant == 0 ? "2b7e151628aed2a6abf7158809cf4f3c" : "603deb1015ca71be2b73aef0857d77811f352c073b6108d72d9810a30914dff4");
            byte[] expected = Hex(variant == 0 ? "874d6191b620e3261bef6864990db6ce9806f66b7970fdff8617187bb9fffdff5ae4df3edbd5d35e5b4f09020db03eab1e031dda2fbe03d1792170a0f3009cee" : "601ec313775789a5b7a7f504bbf3d228f443e3ca4d62b59aca84e990cacaf5c52b0930daa23de94ce87017ba2d84988ddfc9c58db67aada613c2dd08457941a6");
            byte[] output = ctrPlaintext.ToArray();
            var algorithm = variant == 0 ? BclCryptoProvider.Aes128Ctr : BclCryptoProvider.Aes256Ctr;
            fixed (byte* k = key, v = iv, o = output)
            {
                var context = Picotls.ptls_cipher_new(algorithm, 1, k); scope.ThrowIfFailed();
                try
                {
                    context->do_init(context, v); context->do_transform(context, o, o, 13);
                    context->do_transform(context, o + 13, o + 13, 51); scope.ThrowIfFailed();
                    Equal(output, expected, "SP800-38A AES CTR in-place fragmented stream");
                    context->do_init(context, v); context->do_transform(context, o, o, 64); scope.ThrowIfFailed();
                    Equal(output, ctrPlaintext, "AES CTR reinit/decrypt");
                }
                finally { Picotls.ptls_cipher_free(context); }
            }
        }
    }
    static void Aead(CallbackScope scope)
    {
        st_ptls_iovec_t* vectors = stackalloc st_ptls_iovec_t[3];
        for (int variant = 0; variant < 2; variant++)
        {
            byte[] key = new byte[variant == 0 ? 16 : 32], iv = new byte[12], plaintext = new byte[16], output = new byte[48];
            byte[] expected = Hex(variant == 0 ? "0388dace60b6a392f328c2b971b2fe78ab6e47d42cec13bdf53a67b21257bddf" : "cea7403d4d606b6e074ec5d3baf39d18d0d1c8a799996bf0265b98b5d48ab919");
            var algorithm = variant == 0 ? BclCryptoProvider.Aes128Gcm : BclCryptoProvider.Aes256Gcm;
            fixed (byte* k = key, v = iv, p = plaintext, o = output)
            {
                var encrypt = Picotls.ptls_aead_new_direct(algorithm, 1, k, v); scope.ThrowIfFailed();
                var decrypt = Picotls.ptls_aead_new_direct(algorithm, 0, k, v); scope.ThrowIfFailed();
                try
                {
                    encrypt->do_encrypt(encrypt, o, p, 16, 0, null, 0, null); scope.ThrowIfFailed();
                    Equal(output.AsSpan(0, 32), expected, "AES GCM zero vector");
                    Check(decrypt->do_decrypt(decrypt, o + 1, o, 32, 0, null, 0) == 16, "AES GCM partial-overlap decrypt"); scope.ThrowIfFailed();
                    Equal(output.AsSpan(1, 16), plaintext, "AES GCM plaintext");
                    vectors[0] = Vector(p, 7); vectors[1] = Vector(null, 0); vectors[2] = Vector(p + 7, 9);
                    encrypt->do_encrypt_v(encrypt, o, vectors, 3, 0, null, 0); scope.ThrowIfFailed();
                    Equal(output.AsSpan(0, 32), expected, "AES GCM vector gather");
                    new Span<byte>(o, 16).Clear();
                    encrypt->do_encrypt(encrypt, o + 1, o, 16, 0, null, 0, null); scope.ThrowIfFailed();
                    Equal(output.AsSpan(1, 32), expected, "AES GCM partial-overlap encrypt");
                    expected.CopyTo(output, 0);
                    output.AsSpan(32, 16).Fill(0xa5);
                    output[31] ^= 1;
                    Check(decrypt->do_decrypt(decrypt, o + 32, o, 32, 0, null, 0) == ulong.MaxValue, "AES GCM corrupted tag rejected"); scope.ThrowIfFailed();
                    Equal(output.AsSpan(32, 16), plaintext, "failed decrypt output cleared");
                    output[31] ^= 1;
                    output[0] ^= 1;
                    Check(decrypt->do_decrypt(decrypt, o + 32, o, 32, 0, null, 0) == ulong.MaxValue, "AES GCM altered ciphertext rejected");
                    output[0] ^= 1;
                    Check(decrypt->do_decrypt(decrypt, o + 32, o, 32, 1, null, 0) == ulong.MaxValue, "AES GCM wrong sequence rejected");
                    byte aad = 1;
                    Check(decrypt->do_decrypt(decrypt, o + 32, o, 32, 0, &aad, 1) == ulong.MaxValue, "AES GCM altered AAD rejected"); scope.ThrowIfFailed();
                    for (int i = 0; i < 8; i++) iv[4 + i] = (byte)(i + 1);
                    encrypt->do_set_iv(encrypt, v);
                    encrypt->do_get_iv(encrypt, o); scope.ThrowIfFailed(); Equal(output.AsSpan(0, 12), iv, "AEAD IV get/set");
                    encrypt->do_encrypt(encrypt, o, p, 16, 0x0102030405060708, null, 0, null); scope.ThrowIfFailed();
                    Equal(output.AsSpan(0, 32), expected, "big-endian TLS nonce XOR");
                    encrypt->do_encrypt(encrypt, o, null, 0, 0x0102030405060708, null, 0, null); scope.ThrowIfFailed();
                    Equal(output.AsSpan(0, 16), Hex(variant == 0 ? "58e2fccefa7e3061367f1d57a4e7455a" : "530f8afbc74536b9a963b4f1c4cb738b"), "empty GCM vector");
                    var cipher = Picotls.ptls_cipher_new(algorithm->ctr_cipher, 1, k); scope.ThrowIfFailed();
                    try
                    {
                        st_ptls_aead_supplementary_encryption_t supplementary = default;
                        supplementary.ctx = cipher; supplementary.input = o;
                        encrypt->do_encrypt(encrypt, o, p, 16, 0x0102030405060708, null, 0, &supplementary); scope.ThrowIfFailed();
                        using var aes = System.Security.Cryptography.Aes.Create(); aes.Key = key;
                        Equal(new ReadOnlySpan<byte>(supplementary.output, 16), aes.EncryptEcb(expected.AsSpan(0, 16), PaddingMode.None), "supplementary reads ciphertext after encryption");
                    }
                    finally { Picotls.ptls_cipher_free(cipher); }
                }
                finally { Picotls.ptls_aead_free(encrypt); Picotls.ptls_aead_free(decrypt); }
            }
        }
    }
    static void AllocationFailures()
    {
        int baseline = BclCryptoProvider.LiveManagedContexts;
        for (int ordinal = 1; ordinal <= 2; ordinal++)
        {
            using var scope = CallbackScope.Enter();
            using var fault = ProviderFaultInjection.FailAllocation(ordinal);
            var hash = BclCryptoProvider.HashSha256->create();
            Check(hash == null, "injected hash context/handle allocation returns null");
            bool caught = false;
            try { scope.ThrowIfFailed(); } catch (OutOfMemoryException) { caught = true; }
            Check(caught && fault.AllocationsAttempted == ordinal && fault.DisposedStates == 1,
                "hash BCL state disposed on each ownership boundary failure");
            Check(BclCryptoProvider.LiveManagedContexts == baseline, "hash allocation failure handle balance");
        }
        st_ptls_hash_context_t* failedClone = null;
        for (int ordinal = 1; ordinal <= 2; ordinal++)
        {
            using var scope = CallbackScope.Enter();
            var original = BclCryptoProvider.HashSha256->create(); scope.ThrowIfFailed();
            try
            {
                using var fault = ProviderFaultInjection.FailAllocation(ordinal);
                failedClone = original->clone_(original);
                Check(failedClone != null && failedClone != original, "failed clone provides a distinct poison context for unchecked core cleanup");
                bool caught = false;
                try { scope.ThrowIfFailed(); } catch (OutOfMemoryException) { caught = true; }
                Check(caught && fault.DisposedStates == 1 && BclCryptoProvider.LiveManagedContexts == baseline + 1,
                    "failed clone releases unpublished state and preserves the original transcript owner");
                byte output = 0xa5;
                failedClone->update(failedClone, null, ulong.MaxValue);
                failedClone->final(failedClone, &output, (en_ptls_hash_final_mode_t)2);
                Check(output == 0xa5 && failedClone->clone_(failedClone) == failedClone, "poison callbacks produce no digest and clone only the poison context");
                failedClone->final(failedClone, null, (en_ptls_hash_final_mode_t)0);
                failedClone->final(failedClone, null, (en_ptls_hash_final_mode_t)0);
                caught = false;
                try { scope.ThrowIfFailed(); } catch (OutOfMemoryException) { caught = true; }
                Check(caught, "poison cleanup preserves the primary clone allocation failure");
            }
            finally { original->final(original, null, (en_ptls_hash_final_mode_t)0); }
            Check(BclCryptoProvider.LiveManagedContexts == baseline, "failed clone and original transcript cleanup balance handles");
        }
        using (var scope = CallbackScope.Enter())
        {
            failedClone->update(failedClone, null, 0);
            bool rejected = false;
            try { scope.ThrowIfFailed(); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected, "poison hash cannot become valid by entering a fresh callback scope");
            failedClone->final(failedClone, null, (en_ptls_hash_final_mode_t)0);
        }
        byte* key = stackalloc byte[32], iv = stackalloc byte[16];
        new Span<byte>(key, 32).Clear(); new Span<byte>(iv, 16).Clear();
        for (int kind = 0; kind < 2; kind++)
        {
            using var scope = CallbackScope.Enter();
            using var fault = ProviderFaultInjection.FailAllocation(1);
            if (kind == 0)
                Check(Picotls.ptls_cipher_new(BclCryptoProvider.Aes128Ctr, 1, key) == null, "core cipher allocation handles setup failure");
            else
                Check(Picotls.ptls_aead_new_direct(BclCryptoProvider.Aes128Gcm, 1, key, iv) == null, "core AEAD allocation handles setup failure");
            bool caught = false;
            try { scope.ThrowIfFailed(); } catch (OutOfMemoryException) { caught = true; }
            Check(caught && fault.DisposedStates == 1 && BclCryptoProvider.LiveManagedContexts == baseline,
                "AES BCL state disposed when handle publication fails");
        }
        // Directly inspect the core-owned allocation across failed setup. The
        // provider must preserve algo and leave freeing this allocation to us.
        var algorithm = BclCryptoProvider.Aes128Gcm;
        var allocation = (byte*)Libc.malloc(checked((int)algorithm->context_size + 16));
        Check(allocation != null, "core-owned tail test allocation");
        new Span<byte>(allocation, checked((int)algorithm->context_size + 16)).Fill(0xa5);
        var context = (st_ptls_aead_context_t*)allocation;
        context->algo = algorithm;
        try
        {
            using var scope = CallbackScope.Enter();
            using var fault = ProviderFaultInjection.FailAllocation(1);
            Check(algorithm->setup_crypto(context, 1, key, iv) != 0, "direct setup injected failure");
            Check(context->algo == algorithm, "setup failure preserves core algorithm pointer");
            Check(new ReadOnlySpan<byte>(allocation + (int)algorithm->context_size, 16).IndexOfAnyExcept((byte)0xa5) < 0,
                "setup failure preserves core allocation canary");
            bool caught = false;
            try { scope.ThrowIfFailed(); } catch (OutOfMemoryException) { caught = true; }
            Check(caught && fault.DisposedStates == 1, "direct setup disposes unpublished BCL state");
        }
        finally { Libc.free(allocation); }
        // Injection is confined to its creating thread and does not affect an
        // independently executing connection/provider context.
        using (var fault = ProviderFaultInjection.FailAllocation(1))
        {
            Exception? threadFailure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var scope = CallbackScope.Enter();
                    var hash = BclCryptoProvider.HashSha256->create(); scope.ThrowIfFailed();
                    hash->final(hash, null, (en_ptls_hash_final_mode_t)0); scope.ThrowIfFailed();
                }
                catch (Exception ex) { threadFailure = ex; }
            });
            thread.Start(); thread.Join();
            Check(threadFailure == null && fault.AllocationsAttempted == 0, "fault injection is thread-local");
        }
        Check(BclCryptoProvider.LiveManagedContexts == baseline, "all injected ownership failures balance handles");
    }
    static void ErrorAndLifetimeChecks()
    {
        int before = BclCryptoProvider.LiveManagedContexts;
        for (int i = 0; i < 1000; i++)
        {
            using var scope = CallbackScope.Enter();
            var hash = BclCryptoProvider.HashSha256->create(); scope.ThrowIfFailed();
            hash->final(hash, null, (en_ptls_hash_final_mode_t)0); scope.ThrowIfFailed();
        }
        Check(before == BclCryptoProvider.LiveManagedContexts, "1000 hash allocation/free cycles");
        using (var scope = CallbackScope.Enter())
        {
            var hash = BclCryptoProvider.HashSha256->create();
            hash->update(hash, null, ulong.MaxValue);
            bool caught = false;
            try { scope.ThrowIfFailed(); } catch (OverflowException) { caught = true; }
            Check(caught, "oversized hash callback length latched");
            hash->final(hash, null, (en_ptls_hash_final_mode_t)0); // cleanup despite latched failure
        }
        using (var scope = CallbackScope.Enter())
        {
            byte* iv = stackalloc byte[12]; new Span<byte>(iv, 12).Clear();
            var context = Picotls.ptls_aead_new_direct(BclCryptoProvider.Aes128Gcm, 1, null, iv);
            Check(context == null, "AEAD setup failure returns null through core");
            bool caught = false;
            try { scope.ThrowIfFailed(); } catch (ArgumentNullException) { caught = true; }
            Check(caught && BclCryptoProvider.LiveManagedContexts == before, "setup failure releases every state handle");
        }
        var original = new InvalidOperationException("first failure");
        using (var outer = CallbackScope.Enter())
        {
            using (var inner = CallbackScope.Enter()) { CallbackScope.Capture(original); CallbackScope.Capture(new Exception("later")); }
            bool same = false;
            try { outer.ThrowIfFailed(); } catch (Exception ex) { same = ReferenceEquals(ex, original); }
            Check(same, "nested callback scope preserves first exception");
        }
        Check(BclCryptoProvider.HashSha256->create() == null, "unscoped callback refuses crypto work");
        bool unscoped = false;
        try { CallbackScope.ThrowPendingUnscopedFailure(); } catch (InvalidOperationException) { unscoped = true; }
        Check(unscoped, "unscoped callback failure explicitly drainable");
    }
}

using static Managed.Security.PicoTls;
using System.Security.Cryptography;
using Managed.Security;

internal static partial class Program
{
    private static readonly object corruptionGate = new();
    private static int corruptMessage, corruptions;
    private static unsafe delegate*<st_ptls_aead_context_t*, void*, void*, ulong, ulong, void*, ulong, st_ptls_aead_supplementary_encryption_t*, void> originalEncrypt;
    private static readonly unsafe delegate*<st_ptls_aead_context_t*, int, void*, void*, int> CorruptSetupPointer = &CorruptSetup;
    private static readonly unsafe delegate*<st_ptls_aead_context_t*, void*, void*, ulong, ulong, void*, ulong, st_ptls_aead_supplementary_encryption_t*, void> CorruptEncryptPointer = &CorruptEncrypt;
    private static readonly unsafe delegate*<st_ptls_aead_context_t*, void*, st_ptls_iovec_t*, ulong, ulong, void*, ulong, void> CorruptEncryptVectorPointer = &CorruptEncryptVector;
    private static readonly unsafe delegate*<void*, ulong, void> TestRandomPointer = &TestRandom;
    private static readonly unsafe delegate*<st_ptls_get_time_t*, ulong> TestTimePointer = &TestTime;
    private static unsafe void TestRandom(void* output, ulong count)
    {
        try { CallbackScope.RequireActive(); RandomNumberGenerator.Fill(new Span<byte>(output, checked((int)count))); }
        catch (Exception error) { CallbackScope.Capture(error); }
    }
    private static unsafe ulong TestTime(st_ptls_get_time_t* self) => (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    private static unsafe int CorruptSetup(st_ptls_aead_context_t* context, int encryption, void* key, void* iv)
    {
        int result = (context->algo->key_size == 16 ? BclCryptoProvider.Aes128Gcm : BclCryptoProvider.Aes256Gcm)->setup_crypto(context, encryption, key, iv);
        if (result == 0 && encryption != 0)
        {
            originalEncrypt = context->do_encrypt;
            context->do_encrypt = CorruptEncryptPointer;
            context->do_encrypt_v = CorruptEncryptVectorPointer;
        }
        return result;
    }
    private static void CorruptPlaintext(Span<byte> bytes)
    {
        if (corruptions != 0 || bytes.IsEmpty || bytes[^1] != 22) return;
        // Mutate handshake plaintext before real GCM encryption. The resulting
        // record has a valid tag, so these cases exercise CertificateVerify and
        // Finished checks rather than merely duplicating bad-tag tests.
        int offset = 0;
        while (offset + 4 <= bytes.Length - 1)
        {
            int length = (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
            if (length > bytes.Length - 1 - offset - 4) return;
            if (bytes[offset] == corruptMessage && length != 0)
            { bytes[offset + 4 + length - 1] ^= 1; corruptions++; return; }
            offset += 4 + length;
        }
    }
    private static unsafe void CorruptEncrypt(st_ptls_aead_context_t* context, void* output, void* input, ulong length,
        ulong sequence, void* aad, ulong aadLength, st_ptls_aead_supplementary_encryption_t* supplementary)
    {
        byte[]? copy = null;
        try
        {
            copy = new ReadOnlySpan<byte>(input, checked((int)length)).ToArray(); CorruptPlaintext(copy);
            fixed (byte* bytes = copy) originalEncrypt(context, output, bytes, length, sequence, aad, aadLength, supplementary);
        }
        catch (Exception error) { CallbackScope.Capture(error); }
        finally { if (copy != null) CryptographicOperations.ZeroMemory(copy); }
    }
    private static unsafe void CorruptEncryptVector(st_ptls_aead_context_t* context, void* output, st_ptls_iovec_t* input,
        ulong count, ulong sequence, void* aad, ulong aadLength)
    {
        byte[]? copy = null;
        try
        {
            int length = 0;
            for (ulong i = 0; i < count; i++) length = checked(length + (int)input[i].len);
            copy = new byte[length]; int offset = 0;
            for (ulong i = 0; i < count; i++)
            {
                var bytes = new ReadOnlySpan<byte>(input[i].@base, checked((int)input[i].len));
                bytes.CopyTo(copy.AsSpan(offset)); offset += bytes.Length;
            }
            CorruptPlaintext(copy);
            fixed (byte* bytes = copy) originalEncrypt(context, output, bytes, (ulong)length, sequence, aad, aadLength, null);
        }
        catch (Exception error) { CallbackScope.Capture(error); }
        finally { if (copy != null) CryptographicOperations.ZeroMemory(copy); }
    }
    private static unsafe void AuthenticatedHandshakeCorruption()
    {
        lock (corruptionGate)
        {
            st_ptls_cipher_suite_t** serverSuites = stackalloc st_ptls_cipher_suite_t*[2];
            st_ptls_cipher_suite_t** clientSuites = stackalloc st_ptls_cipher_suite_t*[2];
            foreach (int type in new[] { 15, 20 })
                foreach (int suiteIndex in new[] { 0, 1 })
                {
                    using var signer = Identity("server-ecdsa");
                    using var verifier = Verifier();
                    int baseline = BclCryptoProvider.LiveManagedContexts;
                    using var scope = CallbackScope.Enter();
                    corruptMessage = type; corruptions = 0;
                    st_ptls_get_time_t time = new() { cb = TestTimePointer };
                    st_ptls_cipher_suite_t selected = *BclCryptoProvider.SymmetricCipherSuites[suiteIndex];
                    st_ptls_aead_algorithm_t algorithm = *selected.aead;
                    algorithm.setup_crypto = CorruptSetupPointer; selected.aead = &algorithm;
                    serverSuites[0] = &selected; serverSuites[1] = null;
                    clientSuites[0] = BclCryptoProvider.SymmetricCipherSuites[suiteIndex]; clientSuites[1] = null;
                    st_ptls_context_t clientContext = new() { random_bytes = TestRandomPointer, get_time = &time,
                        cipher_suites = clientSuites, key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges };
                    st_ptls_context_t serverContext = new() { random_bytes = TestRandomPointer, get_time = &time,
                        cipher_suites = serverSuites, key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges };
                    verifier.ApplyTo(&clientContext); signer.ApplyTo(&serverContext);
                    st_ptls_t* client = null; st_ptls_t* server = null;
                    st_ptls_buffer_t output = PicotlsBuffer.Create();
                    try
                    {
                        client = PicoTls.ptls_client_new(&clientContext); server = PicoTls.ptls_server_new(&serverContext);
                        Check(client != null && server != null, "real corruption-test core connections");
                        fixed (byte* name = "localhost\0"u8) Check(PicoTls.ptls_set_server_name(client, name, 9) == 0, "corruption-test endpoint");
                        ulong consumed = 0;
                        Check(PicoTls.ptls_handshake(client, &output, null, &consumed, null) == 0x202, "corruption-test ClientHello");
                        byte[] hello = new ReadOnlySpan<byte>(output.@base, checked((int)output.off)).ToArray(); output.off = 0;
                        consumed = (ulong)hello.Length;
                        int serverResult;
                        fixed (byte* bytes = hello)
                            serverResult = PicoTls.ptls_handshake(server, &output, bytes, &consumed, null);
                        scope.ThrowIfFailed();
                        Check(serverResult == 0, $"server emits authenticated corrupted flight (0x{serverResult:x})");
                        Check(corruptions == 1, "exact requested handshake message was modified before GCM");
                        byte[] flight = new ReadOnlySpan<byte>(output.@base, checked((int)output.off)).ToArray(); output.off = 0;
                        consumed = (ulong)flight.Length;
                        int result;
                        fixed (byte* bytes = flight) result = PicoTls.ptls_handshake(client, &output, bytes, &consumed, null);
                        scope.ThrowIfFailed();
                        // The pinned core maps a bad Finished to handshake_failure;
                        // the certificate verifier maps a bad signature to decrypt_error.
                        Check(result == (type == 15 ? 51 : 40), $"invalid {(type == 15 ? "CertificateVerify" : "Finished")} rejected (0x{result:x})");
                        Check(PicoTls.ptls_handshake_is_complete(client) == 0, "corrupted authenticated handshake never completes");
                    }
                    finally
                    {
                        PicoTls.ptls_buffer__release_memory(&output);
                        if (client != null) PicoTls.ptls_free(client);
                        if (server != null) PicoTls.ptls_free(server);
                    }
                    scope.ThrowIfFailed();
                    Check(BclCryptoProvider.LiveManagedContexts == baseline, "corruption failure releases all connection crypto state");
                }
        }
    }
}

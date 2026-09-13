using static Managed.Security.PicoTls;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Security;

static unsafe class AsymmetricVectors
{
    private static readonly delegate*<void*, ulong, void> RandomPointer = &Random;
    private static readonly delegate*<st_ptls_get_time_t*, ulong> TimePointer = &Time;
    private static void Random(void* output, ulong length)
    {
        try { RandomNumberGenerator.Fill(new Span<byte>(output, checked((int)length))); }
        catch (Exception error) { CallbackScope.Capture(error); }
    }
    private static ulong Time(st_ptls_get_time_t* self) => (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    internal static void Run()
    {
        BclCryptoProvider.InitializeAsymmetric();
        Exchange();
        using var rootKey = RSA.Create(2048);
        var now = DateTimeOffset.UtcNow;
        var rootRequest = new CertificateRequest("CN=provider vector root", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        using var root = rootRequest.CreateSelfSigned(now.AddDays(-7), now.AddDays(7));
        using var ecLeaf = Leaf(root, rootKey, now, false);
        using var rsaLeaf = Leaf(root, rootKey, now, true);
        using var expired = Leaf(root, rootKey, now, false, expired: true);
        using var clientLeaf = Leaf(root, rootKey, now, false, client: true);
        using var enciphermentOnly = Leaf(root, rootKey, now, true, keyUsage: X509KeyUsageFlags.KeyEncipherment);
        Sign(ecLeaf, 0x0403); Sign(rsaLeaf, 0x0804);
        bool signingRejected = false;
        try { using var invalid = new BclCryptoProvider.SigningIdentity(enciphermentOnly); }
        catch (CryptographicException) { signingRejected = true; }
        Program.Check(signingRejected, "signing identity rejects KeyUsage without digitalSignature");
        Certificates(root, rootKey, ecLeaf, rsaLeaf, expired, clientLeaf, enciphermentOnly);
        AllocationFailures(ecLeaf, root);
        Program.Check(BclCryptoProvider.LiveManagedContexts == 0, "asymmetric state handles fully released");
    }

    private static X509Certificate2 Leaf(X509Certificate2 root, RSA rootKey, DateTimeOffset now, bool rsa, bool expired = false, bool client = false,
        X509KeyUsageFlags keyUsage = X509KeyUsageFlags.DigitalSignature)
    {
        using RSA? rsaKey = rsa ? RSA.Create(2048) : null;
        using ECDsa? ecKey = rsa ? null : ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = rsa
            ? new CertificateRequest("CN=ignored-cn.invalid", rsaKey!, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
            : new CertificateRequest("CN=ignored-cn.invalid", ecKey!, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(keyUsage, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            { new(client ? "1.3.6.1.5.5.7.3.2" : "1.3.6.1.5.5.7.3.1") }, true));
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName("localhost"); san.AddDnsName("*.example.test"); san.AddIpAddress(IPAddress.Loopback);
        request.CertificateExtensions.Add(san.Build());
        using var issued = request.Create(root.SubjectName,
            X509SignatureGenerator.CreateForRSA(rootKey, RSASignaturePadding.Pkcs1),
            now.AddDays(-2), expired ? now.AddDays(-1) : now.AddDays(1), RandomNumberGenerator.GetBytes(16));
        return rsa ? issued.CopyWithPrivateKey(rsaKey!) : issued.CopyWithPrivateKey(ecKey!);
    }

    private static byte[] Generator() => Program.Hex("04" +
        "6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296" +
        "4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5");
    private static void Exchange()
    {
        using var scope = CallbackScope.Enter();
        var algorithm = BclCryptoProvider.P256KeyExchange;
        Program.Check(algorithm->id == 23 && BclCryptoProvider.AsymmetricKeyExchanges[1] == null, "P256 terminated algorithm table");
        byte[] generator = Generator();
        fixed (byte* peer = generator)
        {
            st_ptls_key_exchange_context_t* context = null;
            st_ptls_iovec_t secret = default;
            Program.Check(algorithm->create(algorithm, &context) == 0 && context != null, "create P256 context");
            try
            {
                Program.Check(context->pubkey.len == 65 && context->pubkey.@base[0] == 4, "provider SEC1 public key");
                byte[] expected = new ReadOnlySpan<byte>(context->pubkey.@base + 1, 32).ToArray();
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                Program.Check(context->on_exchange(&context, 0, &secret, Program.Vector(peer, generator.Length)) == 0, "raw P256 context agreement");
                // Peer private scalar 1 means d*(1*G) is this provider's public d*G.
                Program.Equal(new ReadOnlySpan<byte>(secret.@base, checked((int)secret.len)), expected, "P256 secret equals public X for peer G (no KDF)");
                new Span<byte>(secret.@base, (int)secret.len).Clear(); Libc.free(secret.@base); secret = default;
                Program.Check(context->on_exchange(&context, 1, null, default) == 0 && context == null, "key-exchange cleanup nulls context");
            }
            finally
            {
                if (context != null) context->on_exchange(&context, 1, null, default);
                Libc.free(secret.@base);
            }
            st_ptls_iovec_t publicKey = default;
            Program.Check(algorithm->exchange(algorithm, &publicKey, &secret, Program.Vector(peer, generator.Length)) == 0, "synchronous P256 agreement");
            try
            {
                Program.Check(publicKey.len == 65 && secret.len == 32, "synchronous output widths");
                Program.Equal(new ReadOnlySpan<byte>(secret.@base, 32), new ReadOnlySpan<byte>(publicKey.@base + 1, 32), "synchronous raw agreement has no KDF");
            }
            finally { new Span<byte>(secret.@base, (int)secret.len).Clear(); Libc.free(secret.@base); Libc.free(publicKey.@base); }
        }
        foreach (byte[] invalid in new[] { new byte[64], new byte[65], new byte[] { 4 }.Concat(new byte[64]).ToArray() })
        {
            st_ptls_key_exchange_context_t* context = null;
            Program.Check(algorithm->create(algorithm, &context) == 0, "invalid-peer fixture key context");
            fixed (byte* peer = invalid)
            {
                st_ptls_iovec_t secret = default;
                Program.Check(context->on_exchange(&context, 1, &secret, Program.Vector(peer, invalid.Length)) == 47,
                    "malformed/off-curve P256 peer rejected");
                Program.Check(context == null && secret.@base == null && secret.len == 0, "failed key exchange releases context and clears output");
            }
        }
        scope.ThrowIfFailed();
        Program.Check(BclCryptoProvider.LiveManagedContexts == 0, "key exchange lifecycle balanced");
    }

    private static void Sign(X509Certificate2 leaf, ushort algorithm)
    {
        using var identity = new BclCryptoProvider.SigningIdentity(leaf);
        using var scope = CallbackScope.Enter();
        byte[] message = "test CertificateVerify input: hash this supplied input exactly once"u8.ToArray();
        st_ptls_buffer_t buffer = PicotlsBuffer.Create();
        ushort selected = 0;
        ushort* offered = stackalloc ushort[2] { 0x0807, algorithm };
        try
        {
            int signingResult;
            fixed (byte* data = message)
                signingResult = identity.Callback->cb(identity.Callback, null, null, &selected, &buffer,
                    Program.Vector(data, message.Length), offered, 2);
            scope.ThrowIfFailed();
            Program.Check(signingResult == 0, $"provider certificate signing: 0x{signingResult:x}");
            Program.Check(selected == algorithm, "selected offered compatible signature scheme");
            var signature = new ReadOnlySpan<byte>(buffer.@base, checked((int)buffer.off));
            if (algorithm == 0x0403)
            {
                using var key = leaf.GetECDsaPublicKey()!;
                Program.Check(key.VerifyData(message, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence), "provider ECDSA DER validates in BCL");
                message[0] ^= 1;
                Program.Check(!key.VerifyData(message, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence), "changed signing input rejects");
            }
            else
            {
                using var key = leaf.GetRSAPublicKey()!;
                Program.Check(key.VerifyData(message, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss), "provider RSA-PSS validates in BCL");
                Program.Check(!key.VerifyData(message, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), "RSA PKCS1 signature substitution rejects");
            }
            ulong previous = buffer.off;
            offered[0] = 0x0807;
            fixed (byte* data = message)
                Program.Check(identity.Callback->cb(identity.Callback, null, null, &selected, &buffer,
                    Program.Vector(data, message.Length), offered, 1) == 40, "unoffered signature scheme rejects");
            Program.Check(buffer.off == previous, "failed signature selection does not append output");
            scope.ThrowIfFailed();
        }
        finally { PicoTls.ptls_buffer__release_memory(&buffer); }
    }

    private static void Certificates(X509Certificate2 root, RSA rootKey, X509Certificate2 ecLeaf, X509Certificate2 rsaLeaf,
        X509Certificate2 expired, X509Certificate2 clientLeaf, X509Certificate2 enciphermentOnly)
    {
        using var verifier = new BclCryptoProvider.CertificateVerifier([root], X509RevocationMode.NoCheck);
        st_ptls_get_time_t time = new() { cb = TimePointer };
        st_ptls_context_t context = new() { random_bytes = RandomPointer, get_time = &time,
            key_exchanges = BclCryptoProvider.AsymmetricKeyExchanges, cipher_suites = BclCryptoProvider.SymmetricCipherSuites };
        using var scope = CallbackScope.Enter();
        st_ptls_t* client = PicoTls.ptls_client_new(&context);
        st_ptls_t* server = PicoTls.ptls_server_new(&context);
        Program.Check(client != null && server != null, "real translated connection instances for certificate callback tests");
        try
        {
            CheckCertificate(verifier, client, ecLeaf, "localhost", 0);
            CheckCertificate(verifier, client, rsaLeaf, "127.0.0.1", 0);
            CheckCertificate(verifier, client, ecLeaf, "www.example.test", 0);
            CheckCertificate(verifier, client, ecLeaf, "wrong.invalid", 42);
            CheckCertificate(verifier, client, ecLeaf, "127.0.0.2", 42);
            CheckCertificate(verifier, client, ecLeaf, "ignored-cn.invalid", 42);
            CheckCertificate(verifier, client, ecLeaf, "deep.www.example.test", 42);
            CheckCertificate(verifier, client, expired, "localhost", 45);
            CheckCertificate(verifier, client, clientLeaf, "localhost", 42);
            CheckCertificate(verifier, client, enciphermentOnly, "localhost", 42);
            CheckCertificate(verifier, server, clientLeaf, "", 0);
            CheckCertificate(verifier, server, ecLeaf, "", 42);
            CheckCertificate(verifier, client, ecLeaf, "localhost", 0, signatureError: 51);
            CheckCertificate(verifier, client, rsaLeaf, "localhost", 0, signatureError: 51);
            CheckCertificate(verifier, client, ecLeaf, "localhost", 0, signatureError: 47);
            CheckCertificate(verifier, client, ecLeaf, "localhost", 0, signatureError: 51, malformedSignature: true);
            CheckCertificate(verifier, client, ecLeaf, "localhost", 0, cleanup: true);
            using var foreignKey = RSA.Create(2048);
            var request = new CertificateRequest("CN=untrusted root", foreignKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            using var foreignRoot = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
            using var foreignVerifier = new BclCryptoProvider.CertificateVerifier([foreignRoot], X509RevocationMode.NoCheck);
            CheckCertificate(foreignVerifier, client, ecLeaf, "localhost", 48);
            using var intermediateKey = RSA.Create(2048);
            var intermediateRequest = new CertificateRequest("CN=provider intermediate", intermediateKey,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            intermediateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, true));
            intermediateRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
            using var intermediate = intermediateRequest.Create(root.SubjectName,
                X509SignatureGenerator.CreateForRSA(rootKey, RSASignaturePadding.Pkcs1),
                DateTimeOffset.UtcNow.AddDays(-3), DateTimeOffset.UtcNow.AddDays(3), RandomNumberGenerator.GetBytes(16));
            using var chainedLeaf = Leaf(intermediate, intermediateKey, DateTimeOffset.UtcNow, false);
            CheckCertificate(verifier, client, chainedLeaf, "localhost", 48);
            CheckCertificate(verifier, client, chainedLeaf, "localhost", 48, intermediate: foreignRoot);
            CheckCertificate(verifier, client, chainedLeaf, "localhost", 0, intermediate: intermediate);
            CheckCertificate(foreignVerifier, client, chainedLeaf, "localhost", 48, intermediate: intermediate);
            scope.ThrowIfFailed();
        }
        finally { if (client != null) PicoTls.ptls_free(client); if (server != null) PicoTls.ptls_free(server); }
    }
    private static void CheckCertificate(BclCryptoProvider.CertificateVerifier verifier, st_ptls_t* tls,
        X509Certificate2 leaf, string name, int expected, int signatureError = 0, bool cleanup = false, bool malformedSignature = false,
        X509Certificate2? intermediate = null)
    {
        int baseline = BclCryptoProvider.LiveManagedContexts;
        byte[] der = leaf.RawData, hostname = System.Text.Encoding.UTF8.GetBytes(name + "\0");
        byte[] intermediateDer = intermediate?.RawData ?? [];
        delegate*<void*, ushort, st_ptls_iovec_t, st_ptls_iovec_t, int> verify = null;
        void* state = null;
        fixed (byte* certificateBytes = der, serverName = hostname, intermediateBytes = intermediateDer)
        {
            st_ptls_iovec_t* certificates = stackalloc st_ptls_iovec_t[2]
                { Program.Vector(certificateBytes, der.Length), Program.Vector(intermediateBytes, intermediateDer.Length) };
            int result = verifier.Callback->cb(verifier.Callback, tls, name.Length == 0 ? null : serverName, &verify, &state, certificates,
                intermediate == null ? 1ul : 2ul);
            Program.Check(result == expected, $"certificate chain/name/purpose result {name}: expected {expected}, got {result}");
        }
        if (expected != 0)
        {
            Program.Check(verify == null && state == null, "failed certificate validation publishes no state");
            Program.Check(BclCryptoProvider.LiveManagedContexts == baseline, "failed certificate callback leaves no handle");
            return;
        }
        Program.Check(verify != null && state != null && BclCryptoProvider.LiveManagedContexts == baseline + 1, "validated certificate returns owned signature state");
        try
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            if (cleanup)
            {
                int result = verify(state, 0, default, default); state = null;
                Program.Check(result == 0, "empty-input CertificateVerify abort cleanup");
            }
            else
            {
                byte[] message = "independent BCL CertificateVerify input"u8.ToArray();
                byte[] signature;
                ushort algorithm;
                using var ec = leaf.GetECDsaPrivateKey();
                if (ec != null)
                {
                    algorithm = 0x0403;
                    signature = ec.SignData(message, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
                }
                else
                {
                    using var rsa = leaf.GetRSAPrivateKey()!;
                    algorithm = 0x0804;
                    signature = rsa.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                }
                if (malformedSignature) signature = [0x30, 0x80, 0x02]; // Invalid DER length/structure.
                else if (signatureError == 47) algorithm = 0x0807; // Not the verified leaf's advertised scheme.
                else if (signatureError != 0) message[0] ^= 1;
                fixed (byte* data = message, sign = signature)
                {
                    int result = verify(state, algorithm, Program.Vector(data, message.Length), Program.Vector(sign, signature.Length));
                    state = null;
                    Program.Check(result == signatureError, "actual provider CertificateVerify result");
                }
            }
        }
        finally { if (state != null) verify(state, 0, default, default); }
        Program.Check(BclCryptoProvider.LiveManagedContexts == baseline, "CertificateVerify consumes and frees its key state");
    }

    private static void AllocationFailures(X509Certificate2 leaf, X509Certificate2 root)
    {
        bool completed = false;
        for (int ordinal = 1; ordinal <= 16; ordinal++)
        {
            using var fault = ProviderFaultInjection.FailAllocation(ordinal);
            using var scope = CallbackScope.Enter();
            var algorithm = BclCryptoProvider.P256KeyExchange;
            st_ptls_key_exchange_context_t* context = null;
            int result = algorithm->create(algorithm, &context);
            if (result == 0)
            {
                context->on_exchange(&context, 1, null, default); scope.ThrowIfFailed(); completed = true;
            }
            else
            {
                Program.Check(result == 0x201 && context == null, "P256 allocation failure returns no context");
                bool captured = false;
                try { scope.ThrowIfFailed(); } catch (OutOfMemoryException) { captured = true; }
                Program.Check(captured && fault.DisposedStates != 0, "P256 partial setup disposes state and captures allocation failure");
            }
            Program.Check(BclCryptoProvider.LiveManagedContexts == 0, "P256 allocation failure leaves no handles");
            if (completed) break;
        }
        Program.Check(completed, "enumerated all P256 allocation failure points");
        for (int kind = 0; kind < 2; kind++)
        {
            completed = false;
            for (int ordinal = 1; ordinal <= 16; ordinal++)
            {
                using var fault = ProviderFaultInjection.FailAllocation(ordinal);
                try
                {
                    using IDisposable registration = kind == 0
                        ? new BclCryptoProvider.SigningIdentity(leaf)
                        : new BclCryptoProvider.CertificateVerifier([root], X509RevocationMode.NoCheck);
                    completed = true;
                }
                catch (OutOfMemoryException) { Program.Check(fault.DisposedStates != 0, "registration allocation failure disposes crypto/policy state"); }
                Program.Check(BclCryptoProvider.LiveManagedContexts == 0, "registration partial setup leaves no handles");
                if (completed) break;
            }
            Program.Check(completed, "enumerated registration allocation failure points");
        }
    }
}

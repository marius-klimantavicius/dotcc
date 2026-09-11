using System.Formats.Asn1;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

static class Probe {
    public static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    public static void Equal(ReadOnlySpan<byte> actual, ReadOnlySpan<byte> expected, string message) => Assert(actual.SequenceEqual(expected), message);
    static byte[] Hex(string value) => Convert.FromHexString(value);
    public static void Main(string[] args) {
        Assert(args.Length == 1, "Provide native layout output path");
        Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}; OS: {RuntimeInformation.OSDescription}; architecture: {RuntimeInformation.ProcessArchitecture}; little-endian: {BitConverter.IsLittleEndian}");
        Layouts.Check(args[0]); HashContextProbe.Run(); ContextOwnershipProbe.Run(); Aead(); Agreement(); Signatures(); Certificates();
        Console.WriteLine("PASS all P1 boundary feasibility probes; no translated TLS/provider execution claimed");
    }
    static void Aead() {
        Assert(AesGcm.IsSupported, "AES-GCM is required");
        foreach (int keyLength in new[] { 16, 32 }) {
            using var aes = new AesGcm(new byte[keyLength], 16);
            byte[] ciphertext = new byte[16], tag = new byte[16], plaintext = new byte[16];
            aes.Encrypt(new byte[12], plaintext, ciphertext, tag);
            Equal(ciphertext, Hex(keyLength == 16 ? "0388dace60b6a392f328c2b971b2fe78" : "cea7403d4d606b6e074ec5d3baf39d18"), "AES-GCM known ciphertext");
            Equal(tag, Hex(keyLength == 16 ? "ab6e47d42cec13bdf53a67b21257bddf" : "d0d1c8a799996bf0265b98b5d48ab919"), "AES-GCM known tag");
            aes.Decrypt(new byte[12], ciphertext, tag, plaintext); Equal(plaintext, new byte[16], "AES-GCM decrypt vector");
            tag[0] ^= 1; plaintext.AsSpan().Fill(0xA5);
            bool failed = false;
            try { aes.Decrypt(new byte[12], ciphertext, tag, plaintext); } catch (AuthenticationTagMismatchException) { failed = true; }
            Assert(failed, "bad tag must fail"); Equal(plaintext, new byte[16], "BCL clears rejected plaintext on this target");
            aes.Encrypt(new byte[12], ReadOnlySpan<byte>.Empty, Span<byte>.Empty, tag); aes.Decrypt(new byte[12], ReadOnlySpan<byte>.Empty, tag, Span<byte>.Empty);
            // TLS record nonce: static IV XOR left-padded big-endian sequence.
            byte[] nonce = Hex("000102030405060708090a0b"); ulong sequence = 0x0102030405060708;
            for (int i = 0; i < 8; i++) nonce[11-i] ^= (byte)(sequence >> (8*i));
            Equal(nonce, Hex("00010203050705030d0f0d03"), "TLS nonce encoding");
            byte[] aad = [23, 3, 3, 0, 32];
            aes.Encrypt(nonce, new byte[16], ciphertext, tag, aad);
            aes.Decrypt(nonce, ciphertext, tag, plaintext, aad);
            aad[0] ^= 1; failed = false;
            try { aes.Decrypt(nonce, ciphertext, tag, plaintext, aad); } catch (AuthenticationTagMismatchException) { failed = true; }
            Assert(failed, "bad AAD must fail");
        }
        Console.WriteLine("PASS AES128/256-GCM fixed zero-key vectors, empty plaintext, TLS sequence nonce, AAD/tag rejection");
    }
    static byte[] Encode(ECParameters parameters) {
        Assert(parameters.Q.X?.Length == 32 && parameters.Q.Y?.Length == 32, "P256 coordinate widths");
        byte[] result = new byte[65]; result[0] = 4;
        parameters.Q.X!.CopyTo(result, 1); parameters.Q.Y!.CopyTo(result, 33); return result;
    }
    static ECDiffieHellman ImportPoint(ReadOnlySpan<byte> point) {
        if (point.Length != 65 || point[0] != 4) throw new CryptographicException("Expected 65-byte uncompressed P256 point");
        return ECDiffieHellman.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = new ECPoint { X = point.Slice(1, 32).ToArray(), Y = point.Slice(33, 32).ToArray() } });
    }
    static void Agreement() {
        byte[] gx = Hex("6b17d1f2e12c4247f8bce6e563a440f277037d812deb33a0f4a13945d898c296");
        byte[] gy = Hex("4fe342e2fe1a7f9b8ee7eb4a7c0f9e162bce33576b315ececbb6406837bf51f5");
        byte[] twoX = Hex("7cf27b188d034f7e8a52380304b51ac3c08969e277f21b35a60b48fc47669978");
        byte[] twoY = Hex("07775510db8ed040293d9ac69f7430dbba7dade63ce982299e04b79d227873d1");
        byte[] one = new byte[32]; one[^1] = 1;
        using var fixedKey = ECDiffieHellman.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, D = one, Q = new ECPoint { X = gx, Y = gy } });
        using var twiceGenerator = ECDiffieHellman.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = new ECPoint { X = twoX, Y = twoY } });
        Equal(fixedKey.DeriveRawSecretAgreement(twiceGenerator.PublicKey), twoX, "P256 fixed d=1, peer=2G raw big-endian X");
        using var alice = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        using var bob = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        using var importedAlice = ImportPoint(Encode(alice.ExportParameters(false)));
        using var importedBob = ImportPoint(Encode(bob.ExportParameters(false)));
        var left = alice.DeriveRawSecretAgreement(importedBob.PublicKey);
        var right = bob.DeriveRawSecretAgreement(importedAlice.PublicKey);
        Assert(left.Length == 32, "Raw P256 secret width"); Equal(left, right, "P256 peer agreement");
        foreach (byte[] bad in new[] { new byte[65], new byte[64], new byte[] { 4 }.Concat(new byte[64]).ToArray() }) {
            bool rejected = false;
            try { using var invalid = ImportPoint(bad); fixedKey.DeriveRawSecretAgreement(invalid.PublicKey); } catch (CryptographicException) { rejected = true; }
            Assert(rejected, "Reject malformed/off-curve P256 point");
        }
        CryptographicOperations.ZeroMemory(one); CryptographicOperations.ZeroMemory(left); CryptographicOperations.ZeroMemory(right);
        Console.WriteLine("PASS P256 raw big-endian fixed agreement, SEC1 65-byte points, generated peers, malformed/off-curve rejection");
    }
    static void Signatures() {
        byte[] input = "TLS CertificateVerify input is signed once using the selected scheme"u8.ToArray();
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] signature = ecdsa.SignData(input, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        var reader = new AsnReader(signature, AsnEncodingRules.DER); var sequence = reader.ReadSequence();
        Assert(sequence.ReadInteger() > 0 && sequence.ReadInteger() > 0, "positive DER r/s"); sequence.ThrowIfNotEmpty(); reader.ThrowIfNotEmpty();
        Assert(ecdsa.VerifyData(input, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence), "ECDSA DER verify");
        signature[^1] ^= 1;
        Assert(!ecdsa.VerifyData(input, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence), "ECDSA invalid signature");
        using var rsa = RSA.Create(2048);
        signature = rsa.SignData(input, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        Assert(rsa.VerifyData(input, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss), "RSA-PSS verify");
        Assert(!rsa.VerifyData(input, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), "RSA padding restriction");
        signature[^1] ^= 1;
        Assert(!rsa.VerifyData(input, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss), "RSA-PSS invalid signature");
        Console.WriteLine("PASS P256 ECDSA explicit DER, RSA2048-PSS/SHA256, altered signatures and wrong padding reject");
    }
    static void Certificates() {
        var now = DateTimeOffset.UtcNow;
        using var rootKey = RSA.Create(2048); using var leafKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var rootRequest = new CertificateRequest("CN=picotls P1 disposable root", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        using var root = rootRequest.CreateSelfSigned(now.AddYears(-2), now.AddYears(2));
        X509Certificate2 Leaf(DateTimeOffset before, DateTimeOffset after, string eku) {
            var request = new CertificateRequest("CN=cn-fallback.invalid", leafKey, HashAlgorithmName.SHA256);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid(eku) }, true));
            var san = new SubjectAlternativeNameBuilder(); san.AddDnsName("localhost"); san.AddDnsName("*.example.test"); san.AddIpAddress(IPAddress.Loopback);
            request.CertificateExtensions.Add(san.Build());
            // Cross-algorithm issuer needs an explicit RSA signature generator.
            using var cert = request.Create(root.SubjectName, X509SignatureGenerator.CreateForRSA(rootKey, RSASignaturePadding.Pkcs1), before, after, RandomNumberGenerator.GetBytes(16));
            return X509CertificateLoader.LoadCertificate(cert.RawData);
        }
        bool Trusted(X509Certificate2 leaf, bool includeRoot = true) {
            using var chain = new X509Chain();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            if (includeRoot) chain.ChainPolicy.CustomTrustStore.Add(root);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Isolated fixture has no revocation service.
            chain.ChainPolicy.DisableCertificateDownloads = true;
            chain.ChainPolicy.VerificationTime = now.UtcDateTime;
            chain.ChainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
            return chain.Build(leaf);
        }
        using var valid = Leaf(now.AddDays(-1), now.AddDays(1), "1.3.6.1.5.5.7.3.1");
        Assert(Trusted(valid), "explicit-root server-auth chain"); Assert(!Trusted(valid, false), "untrusted chain");
        Assert(valid.MatchesHostname("localhost", allowWildcards: true, allowCommonName: false), "DNS SAN");
        Assert(valid.MatchesHostname("127.0.0.1", allowWildcards: true, allowCommonName: false), "IP SAN");
        Assert(valid.MatchesHostname("www.example.test", allowWildcards: true, allowCommonName: false), "one-label wildcard");
        foreach (var badName in new[] { "wrong.invalid", "127.0.0.2", "cn-fallback.invalid", "deep.www.example.test" })
            Assert(!valid.MatchesHostname(badName, allowWildcards: true, allowCommonName: false), "wrong DNS/IP/CN/multilabel wildcard");
        using var expired = Leaf(now.AddDays(-3), now.AddDays(-2), "1.3.6.1.5.5.7.3.1"); Assert(!Trusted(expired), "expired chain");
        using var future = Leaf(now.AddDays(1), now.AddDays(2), "1.3.6.1.5.5.7.3.1"); Assert(!Trusted(future), "not-yet-valid chain");
        using var wrongEku = Leaf(now.AddDays(-1), now.AddDays(1), "1.3.6.1.5.5.7.3.2"); Assert(!Trusted(wrongEku), "client-only EKU rejected for server auth");
        using var publicKey = valid.GetECDsaPublicKey()!;
        byte[] data = "disposable CertificateVerify input"u8.ToArray();
        byte[] signature = leafKey.SignData(data, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        Assert(publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence), "leaf-key CertificateVerify feasibility");
        data[0] ^= 1;
        Assert(!publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence), "changed CertificateVerify input rejected");
        Console.WriteLine("PASS X509 custom-root chain, DNS/IP SAN, wildcard, leaf-key signature; untrusted/expired/future/wrong-EKU/wrong-name/invalid CertificateVerify reject; fixture revocation=NoCheck");
    }
}

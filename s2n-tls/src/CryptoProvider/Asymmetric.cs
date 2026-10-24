using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Managed.S2nTls.Crypto;

public sealed class RsaKey : IDisposable
{
    private readonly RSA rsa;
    private bool disposed;
    public int Size => (rsa.KeySize + 7) / 8;
    public int Bits => rsa.KeySize;
    public RsaKey(RSA key) => rsa = key;
    public static RsaKey FromPem(string pem)
    {
        var key = RSA.Create();
        try { key.ImportFromPem(pem); return new RsaKey(key); }
        catch { key.Dispose(); throw; }
    }
    public static RsaKey FromPrivateDer(ReadOnlySpan<byte> der, bool pkcs1 = false)
    {
        var key = RSA.Create();
        try
        {
            int consumed;
            if (pkcs1) key.ImportRSAPrivateKey(der, out consumed);
            else key.ImportPkcs8PrivateKey(der, out consumed);
            if (consumed != der.Length) throw new CryptographicException("Trailing private key data.");
            return new RsaKey(key);
        }
        catch { key.Dispose(); throw; }
    }
    public static RsaKey FromPublicDer(ReadOnlySpan<byte> der)
    {
        var key = RSA.Create();
        try
        {
            key.ImportSubjectPublicKeyInfo(der, out int consumed);
            if (consumed != der.Length) throw new CryptographicException("Trailing public key data.");
            return new RsaKey(key);
        }
        catch { key.Dispose(); throw; }
    }
    public byte[] ExportPublicDer() => rsa.ExportSubjectPublicKeyInfo();
    public byte[] ExportPrivateDer() => rsa.ExportPkcs8PrivateKey();
    public byte[] EncryptPkcs1(ReadOnlySpan<byte> plaintext)
    { ObjectDisposedException.ThrowIf(disposed, this); return rsa.Encrypt(plaintext, RSAEncryptionPadding.Pkcs1); }
    public bool TryDecryptPkcs1(ReadOnlySpan<byte> ciphertext, Span<byte> plaintext, out int written)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        try { return rsa.TryDecrypt(ciphertext, plaintext, RSAEncryptionPadding.Pkcs1, out written); }
        catch (CryptographicException) { CryptographicOperations.ZeroMemory(plaintext); written = 0; return false; }
    }
    // Keep BC's blinded private engine, CRT fault check, and output conversion.
    // Only its generic cipher input restriction differs from native raw RSA.
    private sealed class NativeRawRsaCore(RsaKeyParameters key) : RsaCoreEngine
    {
        public override BigInteger ConvertInput(byte[] input, int offset, int length)
        {
            int size = (key.Modulus.BitLength + 7) / 8;
            if (length < 0 || offset < 0 || length > size || offset > input.Length - length)
                throw new DataLengthException("Input exceeds native RSA modulus width.");
            var value = new BigInteger(1, input, offset, length);
            if (value.CompareTo(key.Modulus) >= 0)
                throw new DataLengthException("Input is outside the native RSA range [0,n).");
            return value;
        }
    }
    public byte[] DecryptRaw(ReadOnlySpan<byte> ciphertext)
    {
        // s2n_pkey_evp_decrypt performs its own constant-time PKCS1 unpadding.
        ObjectDisposedException.ThrowIf(disposed, this);
        var privateKey = (RsaKeyParameters)DotNetUtilities.GetRsaKeyPair(rsa).Private;
        var engine = new RsaBlindedEngine(new NativeRawRsaCore(privateKey));
        engine.Init(false, privateKey);
        byte[] block = ciphertext.ToArray();
        byte[]? raw = null;
        try
        {
            raw = engine.ProcessBlock(block, 0, block.Length);
            byte[] result = new byte[Size];
            raw.CopyTo(result, Size - raw.Length);
            return result;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(block);
            if (raw != null) CryptographicOperations.ZeroMemory(raw);
        }
    }
    private static void CheckDigest(DigestAlgorithm algorithm, ReadOnlySpan<byte> digest)
    {
        using var descriptor = new Digest(algorithm);
        if (digest.Length != descriptor.Size) throw new ArgumentException("Wrong digest length.");
    }
    public byte[] SignHash(DigestAlgorithm algorithm, ReadOnlySpan<byte> digest, bool pss = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this); CheckDigest(algorithm, digest);
        if (algorithm == DigestAlgorithm.Md5Sha1 || algorithm == DigestAlgorithm.Sha224)
        {
            if (pss) throw new ArgumentException("Unsupported legacy PSS digest.");
            byte[] payload = EncodeLegacyDigest(algorithm, digest);
            var engine = new Pkcs1Encoding(new RsaBlindedEngine());
            engine.Init(true, DotNetUtilities.GetRsaKeyPair(rsa).Private);
            try { return engine.ProcessBlock(payload, 0, payload.Length); }
            finally { CryptographicOperations.ZeroMemory(payload); }
        }
        return rsa.SignHash(digest, Digest.Name(algorithm), pss ? RSASignaturePadding.Pss : RSASignaturePadding.Pkcs1);
    }
    public bool VerifyHash(DigestAlgorithm algorithm, ReadOnlySpan<byte> digest, ReadOnlySpan<byte> signature, bool pss = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this); CheckDigest(algorithm, digest);
        if (algorithm == DigestAlgorithm.Md5Sha1 || algorithm == DigestAlgorithm.Sha224)
        {
            if (pss || signature.Length != Size) return false;
            var engine = new Pkcs1Encoding(new RsaBlindedEngine());
            engine.Init(false, DotNetUtilities.GetRsaPublicKey(rsa));
            byte[] expected = EncodeLegacyDigest(algorithm, digest);
            byte[]? actual = null;
            try
            {
                var bytes = signature.ToArray(); actual = engine.ProcessBlock(bytes, 0, bytes.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (InvalidCipherTextException) { return false; }
            finally { CryptographicOperations.ZeroMemory(expected); if (actual != null) CryptographicOperations.ZeroMemory(actual); }
        }
        return rsa.VerifyHash(digest, signature, Digest.Name(algorithm), pss ? RSASignaturePadding.Pss : RSASignaturePadding.Pkcs1);
    }
    private static byte[] EncodeLegacyDigest(DigestAlgorithm algorithm, ReadOnlySpan<byte> digest)
    {
        // SSLv3/TLS1.0/1.1 RSA signs the raw 36-byte MD5||SHA1 without DigestInfo.
        if (algorithm == DigestAlgorithm.Md5Sha1) return digest.ToArray();
        // DER DigestInfo(sha224, NULL, OCTET STRING[28]).
        return [.. Convert.FromHexString("302d300d06096086480165030402040500041c"), .. digest];
    }
    public void Dispose() { if (disposed) return; rsa.Dispose(); disposed = true; }
}

public sealed class FiniteFieldDh
{
    private readonly DHParameters parameters;
    private DHPrivateKeyParameters? privateKey;
    public int Size => (parameters.P.BitLength + 7) / 8;
    public FiniteFieldDh(ReadOnlySpan<byte> p, ReadOnlySpan<byte> g, ReadOnlySpan<byte> q = default)
    {
        var prime = new BigInteger(1, p.ToArray()); var generator = new BigInteger(1, g.ToArray());
        if (prime.BitLength < 512 || !prime.TestBit(0) || generator.CompareTo(BigInteger.One) <= 0 || generator.CompareTo(prime.Subtract(BigInteger.One)) >= 0)
            throw new ArgumentException("Invalid DH parameters.");
        parameters = new DHParameters(prime, generator, q.IsEmpty ? null : new BigInteger(1, q.ToArray()));
    }
    public byte[] GeneratePublicKey()
    {
        var generator = new DHKeyPairGenerator();
        generator.Init(new DHKeyGenerationParameters(new SecureRandom(), parameters));
        var pair = generator.GenerateKeyPair(); privateKey = (DHPrivateKeyParameters)pair.Private;
        return ((DHPublicKeyParameters)pair.Public).Y.ToByteArrayUnsigned();
    }
    public byte[] ImportPrivateKey(ReadOnlySpan<byte> privateBytes)
    {
        var value = new BigInteger(1, privateBytes.ToArray());
        if (value.CompareTo(BigInteger.One) <= 0 || value.CompareTo(parameters.P.Subtract(BigInteger.One)) >= 0)
            throw new ArgumentException("Invalid DH private key.");
        privateKey = new DHPrivateKeyParameters(value, parameters);
        return parameters.G.ModPow(value, parameters.P).ToByteArrayUnsigned();
    }
    public byte[] Derive(ReadOnlySpan<byte> peer, bool padded = false)
    {
        if (privateKey == null) throw new InvalidOperationException("DH key not generated.");
        var publicKey = new DHPublicKeyParameters(new BigInteger(1, peer.ToArray()), parameters);
        var agreement = new Org.BouncyCastle.Crypto.Agreement.DHBasicAgreement(); agreement.Init(privateKey);
        byte[] secret = agreement.CalculateAgreement(publicKey).ToByteArrayUnsigned();
        if (!padded || secret.Length == Size) return secret;
        byte[] result = new byte[Size]; secret.CopyTo(result, Size - secret.Length); CryptographicOperations.ZeroMemory(secret); return result;
    }
}

public sealed class EcKeyExchange : IDisposable
{
    private ECDiffieHellman? key;
    private X25519PrivateKeyParameters? x25519;
    private readonly string curve;
    public EcKeyExchange(string curve)
    {
        this.curve = curve;
        if (curve == "X25519") x25519 = new X25519PrivateKeyParameters(new SecureRandom());
        else key = ECDiffieHellman.Create(Curve(curve));
    }
    public static ECCurve Curve(string name) => name switch
    {
        "P-256" => ECCurve.NamedCurves.nistP256, "P-384" => ECCurve.NamedCurves.nistP384,
        "P-521" => ECCurve.NamedCurves.nistP521, _ => throw new ArgumentException("Unsupported curve.")
    };
    public byte[] ExportPoint()
    {
        if (x25519 != null) return x25519.GeneratePublicKey().GetEncoded();
        ObjectDisposedException.ThrowIf(key is null, this);
        var point = key.ExportParameters(false).Q; return [4, .. point.X!, .. point.Y!];
    }
    public string ExportPrivatePem()
    {
        if (x25519 != null)
        {
            // OpenSSL 1.1.1 expects RFC5208 version 0, without RFC5958's publicKey field.
            var info = new Org.BouncyCastle.Asn1.Pkcs.PrivateKeyInfo(
                new Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier(Org.BouncyCastle.Asn1.EdEC.EdECObjectIdentifiers.id_X25519),
                new Org.BouncyCastle.Asn1.DerOctetString(x25519.GetEncoded()));
            return PemEncoding.WriteString("PRIVATE KEY", info.GetDerEncoded());
        }
        ObjectDisposedException.ThrowIf(key is null, this);
        return key.ExportPkcs8PrivateKeyPem();
    }
    public string ExportPublicPem()
    {
        if (x25519 != null)
            return PemEncoding.WriteString("PUBLIC KEY", Org.BouncyCastle.X509.SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(x25519.GeneratePublicKey()).GetDerEncoded());
        ObjectDisposedException.ThrowIf(key is null, this);
        return key.ExportSubjectPublicKeyInfoPem();
    }
    public byte[] Derive(ReadOnlySpan<byte> peer)
    {
        if (x25519 != null)
        {
            if (peer.Length != 32) throw new CryptographicException("Invalid X25519 key length.");
            var result = new byte[32]; x25519.GenerateSecret(new X25519PublicKeyParameters(peer), result, 0); return result;
        }
        ObjectDisposedException.ThrowIf(key is null, this);
        int width = (key.KeySize + 7) / 8;
        if (peer.Length != 1 + 2 * width || peer[0] != 4) throw new CryptographicException("Expected uncompressed SEC1 point.");
        using var other = ECDiffieHellman.Create(new ECParameters
        {
            Curve = Curve(curve), Q = new ECPoint { X = peer.Slice(1, width).ToArray(), Y = peer.Slice(1 + width, width).ToArray() }
        });
        return key.DeriveRawSecretAgreement(other.PublicKey);
    }
    internal bool TryDeriveX25519(ReadOnlySpan<byte> peer,Span<byte> output)
    {
        ObjectDisposedException.ThrowIf(x25519 is null,this);
        if(peer.Length!=32||output.Length<32)throw new CryptographicException("Invalid X25519 key or output length.");
        try{x25519.GenerateSecret(new X25519PublicKeyParameters(peer),output);return true;}
        // The pinned BC method writes its calculated secret before this
        // documented exception for an all-zero agreement result.
        catch(InvalidOperationException){return false;}
    }
    public void Dispose() { key?.Dispose(); key = null; x25519 = null; }
}

public sealed class EcdsaKey : IDisposable
{
    private readonly ECDsa key;
    public EcdsaKey(ECDsa key) => this.key = key;
    public static EcdsaKey FromPem(string pem)
    {
        var key = ECDsa.Create();
        try { key.ImportFromPem(pem); return new EcdsaKey(key); }
        catch { key.Dispose(); throw; }
    }
    public byte[] ExportPrivateDer() => key.ExportPkcs8PrivateKey();
    public byte[] ExportPublicDer() => key.ExportSubjectPublicKeyInfo();
    public byte[] SignHash(ReadOnlySpan<byte> hash) => key.SignHash(hash, DSASignatureFormat.Rfc3279DerSequence);
    public bool VerifyHash(ReadOnlySpan<byte> hash, ReadOnlySpan<byte> signature) => key.VerifyHash(hash, signature, DSASignatureFormat.Rfc3279DerSequence);
    public void Dispose() => key.Dispose();
}

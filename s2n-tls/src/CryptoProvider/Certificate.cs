using System.Formats.Asn1;
using System.Security.Cryptography;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;

namespace Managed.S2nTls.Crypto;

/// <summary>DER/PEM and primitive PKIX validation. Hostname and s2n policy checks remain in translated s2n.</summary>
public sealed class Certificate
{
    internal X509Certificate Value { get; }
    private Certificate(X509Certificate value) => Value = value;
    public byte[] Der => Value.GetEncoded();
    public byte[] PublicKeyDer => Value.CertificateStructure.SubjectPublicKeyInfo.GetDerEncoded();
    public string SignatureAlgorithmOid => Value.SigAlgOid;
    public static Certificate ParseDer(ReadOnlySpan<byte> input, out int consumed)
    {
        // Delimit exactly one DER value, allowing s2n to enforce its own trailing-byte policy.
        try
        {
            var reader = new AsnReader(input.ToArray(), AsnEncodingRules.DER);
            var encoded = reader.ReadEncodedValue(); consumed = encoded.Length;
            return new Certificate(new X509CertificateParser().ReadCertificate(encoded.ToArray())??throw new CryptographicException("DER value is not a certificate."));
        }
        catch (AsnContentException error) { throw new CryptographicException("Invalid DER certificate.", error); }
    }
    public static Certificate ParsePem(string pem)
    {
        var item = PemEncoding.Find(pem);
        if (!pem.AsSpan(item.Label).SequenceEqual("CERTIFICATE")) throw new CryptographicException("Expected CERTIFICATE PEM.");
        byte[] der = Convert.FromBase64String(pem[item.Base64Data]);
        var cert = ParseDer(der, out int consumed);
        if (consumed != der.Length) throw new CryptographicException("Trailing certificate data.");
        return cert;
    }
    public bool VerifySignature(Certificate issuer)
    {
        try { Value.Verify(issuer.Value.GetPublicKey()); return true; }
        catch (Org.BouncyCastle.Security.GeneralSecurityException) { return false; }
    }
    public static bool VerifyChain(IReadOnlyList<Certificate> leafFirst, IReadOnlyList<Certificate> anchors, DateTime utcTime)
    {
        if (leafFirst.Count == 0 || anchors.Count == 0) return false;
        var trust = new HashSet<TrustAnchor>(anchors.Select(x => new TrustAnchor(x.Value, null)));
        var path = leafFirst.Select(x => x.Value).ToList();
        if (anchors.Any(a => a.Value.Equals(path[^1]))) path.RemoveAt(path.Count - 1);
        // A trusted leaf is still checked for its validity interval.
        if (path.Count == 0)
        {
            try { leafFirst[0].Value.CheckValidity(utcTime); return true; }
            catch (Org.BouncyCastle.Security.GeneralSecurityException) { return false; }
        }
        var options = new PkixParameters(trust) { IsRevocationEnabled = false, Date = utcTime };
        try { new PkixCertPathValidator().Validate(new PkixCertPath(path), options); return true; }
        catch (PkixCertPathValidatorException) { return false; }
    }
}

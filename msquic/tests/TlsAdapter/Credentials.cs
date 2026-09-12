using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// Shared certificate recipe from the raw TLS feasibility control.
internal sealed class Credentials : IDisposable
{
    public X509Certificate2 Root { get; } = CreateRoot("CN=dotcc QUIC feasibility root");
    public X509Certificate2 EcLeaf { get; }
    public X509Certificate2 RsaLeaf { get; }
    public Credentials()
    {
        using var ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var rsa = RSA.Create(2048);
        EcLeaf = Leaf(new CertificateRequest("CN=localhost", ec, HashAlgorithmName.SHA256), ec, null);
        RsaLeaf = Leaf(new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1), null, rsa);
    }
    public static X509Certificate2 CreateRoot(string subject)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(2));
    }
    private X509Certificate2 Leaf(CertificateRequest request, ECDsa? ec, RSA? rsa)
    {
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false));
        var san = new SubjectAlternativeNameBuilder(); san.AddDnsName("localhost");
        request.CertificateExtensions.Add(san.Build());
        using var issuerKey = Root.GetECDsaPrivateKey()!;
        using var certificate = request.Create(Root.SubjectName, X509SignatureGenerator.CreateForECDsa(issuerKey),
            DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow.AddDays(1), RandomNumberGenerator.GetBytes(16));
        return ec != null ? certificate.CopyWithPrivateKey(ec) : certificate.CopyWithPrivateKey(rsa!);
    }
    public void Dispose() { EcLeaf.Dispose(); RsaLeaf.Dispose(); Root.Dispose(); }
}

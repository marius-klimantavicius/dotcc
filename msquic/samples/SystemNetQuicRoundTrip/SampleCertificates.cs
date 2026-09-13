using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// Temporary credentials for the local demo. Nothing is installed or written to disk.
internal sealed class SampleCertificates : IDisposable
{
    public X509Certificate2 Root { get; }
    public X509Certificate2 Server { get; }

    public SampleCertificates()
    {
        using var rootKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var rootRequest = new CertificateRequest("CN=QUIC sample CA", rootKey, HashAlgorithmName.SHA256);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Root = rootRequest.CreateSelfSigned(now.AddMinutes(-5), now.AddDays(2));
        try
        {
            using var serverKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var request = new CertificateRequest("CN=localhost", serverKey, HashAlgorithmName.SHA256);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, true)); // TLS server authentication
            var names = new SubjectAlternativeNameBuilder();
            names.AddDnsName("localhost");
            request.CertificateExtensions.Add(names.Build());
            using var issued = request.Create(Root, now.AddMinutes(-5), now.AddDays(1), RandomNumberGenerator.GetBytes(16));
            Server = issued.CopyWithPrivateKey(serverKey);
        }
        catch { Root.Dispose(); throw; }
    }

    public void Dispose()
    {
        Server.Dispose();
        Root.Dispose();
    }
}

using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class Credentials
{
    public static void Create(string directory)
    {
        Directory.CreateDirectory(directory);
        if (Directory.EnumerateFileSystemEntries(directory).Any())
            throw new IOException("Credential output directory must be empty");
        using var rootKey = RSA.Create(2048);
        var now = DateTimeOffset.UtcNow;
        var rootRequest = new CertificateRequest("CN=dotcc disposable TLS root", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        using var root = rootRequest.CreateSelfSigned(now.AddDays(-7), now.AddDays(30));
        File.WriteAllBytes(Path.Combine(directory, "ca.cer"), root.RawData);
        File.WriteAllText(Path.Combine(directory, "ca.pem"), root.ExportCertificatePem());
        foreach (string identity in new[] { "server-rsa", "server-ecdsa", "client-rsa", "client-ecdsa", "server-expired", "server-wrong-name", "server-key-encipherment-rsa" })
        {
            bool rsa = identity.EndsWith("rsa", StringComparison.Ordinal);
            bool server = identity.StartsWith("server-", StringComparison.Ordinal);
            using RSA? rsaKey = rsa ? RSA.Create(2048) : null;
            using ECDsa? ecKey = rsa ? null : ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var request = rsa
                ? new CertificateRequest("CN=" + identity, rsaKey!, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                : new CertificateRequest("CN=" + identity, ecKey!, HashAlgorithmName.SHA256);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(identity == "server-key-encipherment-rsa"
                ? X509KeyUsageFlags.KeyEncipherment : X509KeyUsageFlags.DigitalSignature, true));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
                { new(server ? "1.3.6.1.5.5.7.3.1" : "1.3.6.1.5.5.7.3.2") }, true));
            if (server)
            {
                var san = new SubjectAlternativeNameBuilder();
                san.AddDnsName(identity == "server-wrong-name" ? "wrong.invalid" : "localhost");
                if (identity != "server-wrong-name") san.AddIpAddress(IPAddress.Loopback);
                request.CertificateExtensions.Add(san.Build());
            }
            using var issued = request.Create(root.SubjectName,
                X509SignatureGenerator.CreateForRSA(rootKey, RSASignaturePadding.Pkcs1),
                now.AddDays(-2), identity == "server-expired" ? now.AddDays(-1) : now.AddDays(7), RandomNumberGenerator.GetBytes(16));
            using var leaf = rsa ? issued.CopyWithPrivateKey(rsaKey!) : issued.CopyWithPrivateKey(ecKey!);
            File.WriteAllText(Path.Combine(directory, identity + ".pem"), leaf.ExportCertificatePem());
            string keyPath = Path.Combine(directory, identity + ".key.pem");
            string pfxPath = Path.Combine(directory, identity + ".pfx");
            File.WriteAllText(keyPath, rsa ? rsaKey!.ExportPkcs8PrivateKeyPem() : ecKey!.ExportPkcs8PrivateKeyPem());
            File.WriteAllBytes(pfxPath, leaf.Export(X509ContentType.Pfx, ""));
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(keyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                File.SetUnixFileMode(pfxPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
    }
}

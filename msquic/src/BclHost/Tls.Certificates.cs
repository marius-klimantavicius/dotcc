using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Security;

namespace Managed.Transport.Hosting;

public sealed unsafe partial class MsQuicHost
{
    private static int TlsVerifyCertificate(st_ptls_verify_certificate_t* self, st_ptls_t* tls, byte* name,
        delegate*<void*, ushort, st_ptls_iovec_t, st_ptls_iovec_t, int>* signature,
        void** signatureContext, st_ptls_iovec_t* certificates, ulong count)
    {
        if (signature == null || signatureContext == null) return 47;
        *signature = null; *signatureContext = null;
        bool transfer = false;
        try
        {
            var owner = TlsOwner(tls);
            var verifier = owner.Security.Credentials.Verifier!.Callback;
            int status = verifier->cb(verifier, tls, name, signature, signatureContext, certificates, count);
            // Deferred application approval cannot bypass explicit trust/name checks.
            if (status != 0 && (owner.Security.CredentialFlags & 0x20) == 0) return status;
            if (count is 0 or > BclCryptoProvider.MaximumCertificateCount || certificates == null) return status == 0 ? 42 : status;
            using var memory = new TlsMemory();
            var chain = new X509Certificate2Collection();
            try
            {
                int total = 0;
                for (int i = 0; i < (int)count; i++)
                {
                    if (certificates[i].len > BclCryptoProvider.MaximumCertificateBytes || certificates[i].@base == null) return 42;
                    total = checked(total + (int)certificates[i].len);
                    if (total > BclCryptoProvider.MaximumCertificateBytes) return 42;
                    chain.Add(X509CertificateLoader.LoadCertificate(new ReadOnlySpan<byte>(certificates[i].@base, (int)certificates[i].len)));
                }
                byte[] encodedChain = chain.Export(X509ContentType.Pkcs7) ?? throw new CryptographicException("Certificate chain export failed");
                QUIC_BUFFER certificate = new() { Length = (uint)certificates[0].len, Buffer = certificates[0].@base };
                QUIC_BUFFER portableChain = new() { Length = (uint)encodedChain.Length, Buffer = memory.Copy(encodedChain) };
                uint deferredStatus = status switch
                {
                    0 => Status.Success,
                    45 => Status.CertificateExpired,
                    48 => Status.CertificateUntrustedRoot,
                    116 => Status.CertificateMissing,
                    >= 0 and <= 255 => Status.TlsAlert((byte)status),
                    _ => Status.TlsError
                };
                byte accepted = owner.Security.Callbacks.CertificateReceived(owner.Connection,
                    &certificate, &portableChain, 0, deferredStatus);
                if (status != 0) return status;
                if (accepted == 0) return 42;
                transfer = true;
                return 0;
            }
            finally { foreach (var certificate in chain) certificate.Dispose(); }
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
        finally
        {
            if (!transfer && signature != null && signatureContext != null && *signature != null && *signatureContext != null)
            {
                var cleanup = *signature; void* context = *signatureContext;
                *signature = null; *signatureContext = null;
                cleanup(context, 0, default, default);
            }
        }
    }
}

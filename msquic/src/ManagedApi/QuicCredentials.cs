using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Managed.Transport.Hosting;

namespace Managed.Transport.Api;

/// <summary>An owning snapshot of portable peer certificate data.
/// PeerChainPkcs7 contains the peer-sent chain, not a rebuilt chain containing local
/// trust roots. Application approval cannot override failed native trust or name validation.</summary>
public sealed record QuicCertificateValidation(ReadOnlyMemory<byte> CertificateDer,
    ReadOnlyMemory<byte> PeerChainPkcs7, uint ValidationStatus);

/// <summary>Copied certificate, private-key and trust material for creating configurations.
/// Dispose independently after configuration creation has been invoked.</summary>
public sealed class QuicCredentials : IDisposable
{
    private readonly object gate = new();
    private readonly X509Certificate2? leaf;
    private readonly X509Certificate2[] certificates;
    private bool disposed;
    public bool IsServer { get; }
    public ushort CipherSuite { get; }
    public bool LoadAsynchronously { get; }
    internal X509RevocationMode RevocationMode { get; }
    internal Func<QuicCertificateValidation, CancellationToken, ValueTask<bool>>? CertificateValidation { get; }

    private QuicCredentials(bool server, X509Certificate2? leaf, X509Certificate2[] certificates,
        ushort cipherSuite, X509RevocationMode revocationMode,
        Func<QuicCertificateValidation, CancellationToken, ValueTask<bool>>? validation, bool asynchronous)
    {
        IsServer = server; this.leaf = leaf; this.certificates = certificates;
        CipherSuite = cipherSuite; RevocationMode = revocationMode;
        CertificateValidation = validation; LoadAsynchronously = asynchronous;
    }

    public static QuicCredentials Client(IEnumerable<X509Certificate2> trustRoots, X509RevocationMode revocationMode,
        ushort cipherSuite = 0, Func<QuicCertificateValidation, CancellationToken, ValueTask<bool>>? validateCertificate = null,
        bool loadAsynchronously = true)
    {
        ValidateSuite(cipherSuite);
        if (revocationMode is not (X509RevocationMode.NoCheck or X509RevocationMode.Offline or X509RevocationMode.Online))
            throw new ArgumentOutOfRangeException(nameof(revocationMode));
        var roots = CopyCertificates(trustRoots);
        if (roots.Length == 0) throw new ArgumentException("At least one explicit trust root is required.", nameof(trustRoots));
        try { return new(false, null, roots, cipherSuite, revocationMode, validateCertificate, loadAsynchronously); }
        catch { foreach (var root in roots) root.Dispose(); throw; }
    }

    public static QuicCredentials Server(X509Certificate2 leaf, IEnumerable<X509Certificate2>? intermediates = null,
        ushort cipherSuite = 0, bool loadAsynchronously = true)
    {
        ArgumentNullException.ThrowIfNull(leaf); ValidateSuite(cipherSuite);
        if (!leaf.HasPrivateKey) throw new ArgumentException("A server certificate with its private key is required.", nameof(leaf));
        X509Certificate2? copy = null;
        X509Certificate2[] chain = [];
        byte[]? encoded = null;
        try
        {
            encoded = leaf.Export(X509ContentType.Pkcs12);
            copy = X509CertificateLoader.LoadPkcs12(encoded, null, X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
            chain = CopyCertificates(intermediates ?? []);
            return new(true, copy, chain, cipherSuite, X509RevocationMode.NoCheck, null, loadAsynchronously);
        }
        catch { copy?.Dispose(); foreach (var certificate in chain) certificate.Dispose(); throw; }
        finally { if (encoded != null) CryptographicOperations.ZeroMemory(encoded); }
    }

    private static void ValidateSuite(ushort suite)
    { if (suite is not (0 or 0x1301 or 0x1302)) throw new ArgumentOutOfRangeException(nameof(suite), "Select AES-128-GCM/SHA-256, AES-256-GCM/SHA-384, or both (zero)."); }

    private static X509Certificate2[] CopyCertificates(IEnumerable<X509Certificate2> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var copies = new List<X509Certificate2>();
        try
        {
            foreach (var certificate in source)
            {
                ArgumentNullException.ThrowIfNull(certificate);
                var copy = X509CertificateLoader.LoadCertificate(certificate.RawData);
                try { copies.Add(copy); } catch { copy.Dispose(); throw; }
            }
            return copies.ToArray();
        }
        catch { foreach (var certificate in copies) certificate.Dispose(); throw; }
    }

    // The caller acquires this independent host-owned snapshot before its first await.
    internal MsQuicHost.CredentialRegistration Register(MsQuicHost host)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return IsServer ? host.CreateServerCredential(leaf!, certificates, CipherSuite)
                : host.CreateClientCredential(certificates, CipherSuite, RevocationMode);
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return; disposed = true;
            leaf?.Dispose(); foreach (var certificate in certificates) certificate.Dispose();
        }
    }
}

/// <summary>Owned imported ticket master. The first key passed to SetTicketKeys
/// encrypts; remaining configured keys decrypt. Removing an ID invalidates its tickets.</summary>
public sealed class QuicTicketKey : IDisposable
{
    private readonly object gate = new();
    private readonly byte[] id, material;
    private bool disposed;
    public QuicTicketKey(ReadOnlySpan<byte> id, ReadOnlySpan<byte> material)
    {
        if (id.Length != 16) throw new ArgumentException("Ticket key IDs contain exactly 16 bytes.", nameof(id));
        if (material.Length != 64) throw new ArgumentException("Ticket master material contains exactly 64 bytes.", nameof(material));
        this.id = id.ToArray(); this.material = material.ToArray();
    }
    public ReadOnlyMemory<byte> Id
    { get { lock (gate) { ObjectDisposedException.ThrowIf(disposed, this); return id.ToArray(); } } }
    internal void CopyTo(Span<byte> keyId, Span<byte> master)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            id.CopyTo(keyId); material.CopyTo(master);
        }
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return; disposed = true;
            CryptographicOperations.ZeroMemory(id); CryptographicOperations.ZeroMemory(material);
        }
    }
}

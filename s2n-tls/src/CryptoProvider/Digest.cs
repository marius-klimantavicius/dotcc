using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;

namespace Managed.S2nTls.Crypto;

public enum DigestAlgorithm { Md5, Sha1, Sha224, Sha256, Sha384, Sha512, Md5Sha1, Shake256_64 }

/// <summary>Copyable incremental state. Finalize consumes the state; Reset starts a new digest.</summary>
public sealed class Digest : IDisposable
{
    private IncrementalHash? primary, secondary;
    private IDigest? managed;
    private bool finalized, disposed;
    public DigestAlgorithm Algorithm { get; }
    public int Size => Algorithm switch
    {
        DigestAlgorithm.Md5 => 16, DigestAlgorithm.Sha1 => 20, DigestAlgorithm.Sha224 => 28,
        DigestAlgorithm.Sha256 => 32, DigestAlgorithm.Sha384 => 48, DigestAlgorithm.Sha512 => 64,
        DigestAlgorithm.Md5Sha1 => 36, DigestAlgorithm.Shake256_64 => 64, _ => throw new ArgumentOutOfRangeException()
    };
    public static HashAlgorithmName Name(DigestAlgorithm algorithm) => algorithm switch
    {
        DigestAlgorithm.Md5 => HashAlgorithmName.MD5, DigestAlgorithm.Sha1 => HashAlgorithmName.SHA1,
        DigestAlgorithm.Sha224 => new("SHA224"), DigestAlgorithm.Sha256 => HashAlgorithmName.SHA256,
        DigestAlgorithm.Sha384 => HashAlgorithmName.SHA384, DigestAlgorithm.Sha512 => HashAlgorithmName.SHA512,
        _ => throw new ArgumentException("Combined MD5+SHA1 has no single BCL hash identifier.")
    };
    public Digest(DigestAlgorithm algorithm) { Algorithm = algorithm; Initialize(); }
    private Digest(Digest source)
    {
        Algorithm = source.Algorithm;
        primary = source.primary?.Clone(); secondary = source.secondary?.Clone();
        managed = source.managed is IMemoable memo ? (IDigest)memo.Copy() : null;
        finalized = source.finalized;
    }
    private void Initialize()
    {
        if (Algorithm == DigestAlgorithm.Sha224) managed = new Sha224Digest();
        else if (Algorithm == DigestAlgorithm.Shake256_64) managed = new ShakeDigest(256);
        else if (Algorithm == DigestAlgorithm.Md5Sha1)
        {
            primary = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
            secondary = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        }
        else primary = IncrementalHash.CreateHash(Name(Algorithm));
        finalized = false;
    }
    public void Update(ReadOnlySpan<byte> bytes)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (finalized) throw new InvalidOperationException("Digest was finalized; reset before reuse.");
        primary?.AppendData(bytes); secondary?.AppendData(bytes); managed?.BlockUpdate(bytes);
    }
    public Digest Clone()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return new Digest(this);
    }
    public byte[] FinalizeHash()
    {
        if(Algorithm==DigestAlgorithm.Shake256_64)return FinalizeXof(Size);
        ObjectDisposedException.ThrowIf(disposed, this);
        if (finalized) throw new InvalidOperationException("Digest already finalized.");
        var result = new byte[Size];
        if (managed != null) managed.DoFinal(result, 0);
        else
        {
            primary!.GetHashAndReset(result);
            if (secondary != null) secondary.GetHashAndReset(result.AsSpan(16));
        }
        finalized = true;
        return result;
    }
    public byte[] FinalizeXof(int length)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(finalized)throw new InvalidOperationException("Digest already finalized.");
        if(length<0||managed is not IXof xof)throw new ArgumentException("Expected a SHAKE context and nonnegative output length.");
        var result=new byte[length];xof.OutputFinal(result,0,length);finalized=true;return result;
    }
    public void Reset()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        primary?.Dispose(); secondary?.Dispose(); primary = secondary = null; managed = null;
        Initialize();
    }
    public static byte[] Hash(DigestAlgorithm algorithm, ReadOnlySpan<byte> bytes)
    {
        using var hash = new Digest(algorithm); hash.Update(bytes); return hash.FinalizeHash();
    }
    public void Dispose()
    {
        if (disposed) return;
        primary?.Dispose(); secondary?.Dispose(); managed?.Reset();
        primary = secondary = null; managed = null; disposed = true;
    }
}

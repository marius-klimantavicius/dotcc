using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Managed.S2nTls.Crypto;

/// <summary>One continuing RC4 keystream; Process never resets the key schedule.</summary>
public sealed class Rc4 : IDisposable
{
    private RC4Engine? engine;
    public Rc4(ReadOnlySpan<byte> key)
    {
        if (key.IsEmpty || key.Length > 256) throw new ArgumentException("Invalid RC4 key length.");
        engine = new RC4Engine(); engine.Init(true, new KeyParameter(key));
    }
    public void Process(ReadOnlySpan<byte> input, Span<byte> output)
    {
        ObjectDisposedException.ThrowIf(engine is null, this);
        if (output.Length < input.Length) throw new ArgumentException("Output too small.");
        engine.ProcessBytes(input, output);
    }
    public void Dispose() { engine?.Reset(); engine = null; }
}

/// <summary>CBC without padding. The provider owns chaining; s2n owns record padding and explicit IVs.</summary>
public sealed class CbcCipher : IDisposable
{
    private readonly bool encrypt;
    private readonly byte[] key;
    private readonly byte[] iv;
    private Aes? aes;
    private DesEdeEngine? tripleDes;
    private bool disposed;
    public int BlockSize => iv.Length;
    public CbcCipher(bool tripleDes, bool encrypt, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv)
    {
        int block = tripleDes ? 8 : 16;
        if (iv.Length != block || (tripleDes ? key.Length != 24 : key.Length is not (16 or 32)))
            throw new ArgumentException("Invalid CBC key or IV length.");
        this.encrypt = encrypt; this.key = key.ToArray(); this.iv = iv.ToArray();
        if (tripleDes)
        {
            this.tripleDes = new DesEdeEngine(); this.tripleDes.Init(encrypt, new KeyParameter(key));
        }
        else { aes = Aes.Create(); aes.Key = this.key; }
    }
    public void SetIv(ReadOnlySpan<byte> value)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (value.Length != iv.Length) throw new ArgumentException("Invalid IV size.");
        value.CopyTo(iv);
    }
    public byte[] GetIv() => (byte[])iv.Clone();
    public void Process(ReadOnlySpan<byte> input, Span<byte> output)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (input.Length % BlockSize != 0 || output.Length < input.Length) throw new ArgumentException("CBC requires complete blocks.");
        if (input.IsEmpty) return;
        Span<byte> nextIv = stackalloc byte[BlockSize];
        if (!encrypt) input[^BlockSize..].CopyTo(nextIv);
        if (aes != null)
        {
            if (encrypt) aes.EncryptCbc(input, iv, output, PaddingMode.None);
            else aes.DecryptCbc(input, iv, output, PaddingMode.None);
        }
        else
        {
            Span<byte> block = stackalloc byte[8];
            Span<byte> saved = stackalloc byte[8];
            for (int i = 0; i < input.Length; i += 8)
            {
                var source = input.Slice(i, 8); var target = output.Slice(i, 8);
                source.CopyTo(saved);
                if (encrypt)
                {
                    for (int j = 0; j < 8; j++) block[j] = (byte)(source[j] ^ iv[j]);
                    tripleDes!.ProcessBlock(block, target); target.CopyTo(iv);
                }
                else
                {
                    tripleDes!.ProcessBlock(source, block);
                    for (int j = 0; j < 8; j++) target[j] = (byte)(block[j] ^ iv[j]);
                    saved.CopyTo(iv);
                }
            }
            CryptographicOperations.ZeroMemory(block); CryptographicOperations.ZeroMemory(saved);
        }
        if (encrypt) output.Slice(input.Length - BlockSize, BlockSize).CopyTo(iv);
        else nextIv.CopyTo(iv);
        CryptographicOperations.ZeroMemory(nextIv);
    }
    public void Dispose()
    {
        if (disposed) return;
        aes?.Dispose(); tripleDes = null;
        CryptographicOperations.ZeroMemory(key); CryptographicOperations.ZeroMemory(iv); disposed = true;
    }
}

public enum AeadAlgorithm { AesGcm, ChaCha20Poly1305 }
public static class Aead
{
    public const int TagSize = 16;
    public static void Encrypt(AeadAlgorithm algorithm, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> input, ReadOnlySpan<byte> aad, Span<byte> ciphertext, Span<byte> tag)
    {
        if (algorithm == AeadAlgorithm.AesGcm)
        { using var cipher = new AesGcm(key, TagSize); cipher.Encrypt(nonce, input, ciphertext, tag, aad); }
        else
        { using var cipher = new ChaCha20Poly1305(key); cipher.Encrypt(nonce, input, ciphertext, tag, aad); }
    }
    public static bool TryDecrypt(AeadAlgorithm algorithm, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> ciphertext, ReadOnlySpan<byte> aad, ReadOnlySpan<byte> tag, Span<byte> output)
    {
        try
        {
            if (algorithm == AeadAlgorithm.AesGcm)
            { using var cipher = new AesGcm(key, TagSize); cipher.Decrypt(nonce, ciphertext, tag, output, aad); }
            else
            { using var cipher = new ChaCha20Poly1305(key); cipher.Decrypt(nonce, ciphertext, tag, output, aad); }
            return true;
        }
        catch (AuthenticationTagMismatchException) { CryptographicOperations.ZeroMemory(output); return false; }
    }
}

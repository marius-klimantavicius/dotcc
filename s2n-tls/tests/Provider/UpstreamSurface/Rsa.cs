using System.Runtime.CompilerServices;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Managed.S2nTls.Crypto;

// Private candidate only. This file is not evaluated by the live test project.
public static unsafe partial class OpenSslAbi
{
    private sealed class TestRsaView : IDisposable
    {
        public readonly KeyState Key;
        public readonly RsaKeyParameters PublicKey;
        public nint Handle;
        public int References = 1;
        public TestRsaView(KeyState key)
        {
            Key = key.Retain();
            try { PublicKey = (RsaKeyParameters)PublicKeyFactory.CreateKey(key.Rsa!.ExportPublicDer()); }
            catch { Key.Dispose(); throw; }
        }
        public void Dispose() => Key.Dispose();
    }
    private static readonly ConditionalWeakTable<KeyState, TestRsaView> TestRsaViews = new();
    private static readonly object TestRsaGate = new();

    public static void* EVP_PKEY_get1_RSA(void* key)
    {
        try
        {
            var state = Get<KeyState>(key);
            if (state.Type is not (RsaId or RsaPssId)) throw new ArgumentException();
            // A correctly typed, empty native EVP_PKEY returns null without ERR.
            if (state.Rsa == null) return null;
            lock (TestRsaGate)
            {
                if (TestRsaViews.TryGetValue(state, out var existing))
                {
                    checked { existing.References++; }
                    return (void*)existing.Handle;
                }
                var view = new TestRsaView(state);
                try { view.Handle = (nint)Add(view); }
                catch { view.Dispose(); throw; }
                TestRsaViews.Add(state, view);
                return (void*)view.Handle;
            }
        }
        catch { Error(); return null; }
    }

    public static void RSA_free(void* rsa)
    {
        if (rsa == null) return;
        try
        {
            lock (TestRsaGate)
            {
                var view = Get<TestRsaView>(rsa);
                if (--view.References != 0) return;
                TestRsaViews.Remove(view.Key);
                Remove(rsa);
            }
        }
        catch { Error(); }
    }

    public static int RSA_size(void* rsa)
    {
        try { return Get<TestRsaView>(rsa).Key.Rsa!.Size; }
        catch { Error(); return 0; }
    }

    public static int RSA_public_encrypt(int length, byte* input, byte* output, void* rsa, int padding)
    {
        try
        {
            var view = Get<TestRsaView>(rsa);
            int size = view.Key.Rsa!.Size;
            if (length < 0 || output == null) throw new ArgumentException();
            byte[] result;
            if (padding == 1) result = view.Key.Rsa.EncryptPkcs1(Input(input, (ulong)length));
            else if (padding == 3)
            {
                if (length != size) throw new ArgumentException();
                byte[] bytes = Input(input, (ulong)length).ToArray();
                var value = new Org.BouncyCastle.Math.BigInteger(1, bytes);
                if (value.CompareTo(view.PublicKey.Modulus) >= 0) throw new ArgumentException();
                // BC's general RSA cipher deliberately excludes 0, 1 and n-1.
                // Native RSA_NO_PADDING permits every integer in [0,n).
                byte[] raw = value.ModPow(view.PublicKey.Exponent, view.PublicKey.Modulus).ToByteArrayUnsigned();
                result = new byte[size];
                raw.CopyTo(result, size - raw.Length);
            }
            else throw new NotSupportedException("This test boundary supports PKCS1 and raw RSA public encryption.");
            result.CopyTo(Output(output, size));
            return result.Length;
        }
        catch { Error(); return -1; }
    }
}

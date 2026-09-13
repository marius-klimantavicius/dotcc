using static Managed.Transport.MsQuic;
using System.Text;
using System.Text.Json;
using Managed.Transport;
using Managed.Transport.Hosting;

internal static unsafe class Program
{
    private static int checks;
    private static void Require(bool condition, string description)
    { if (!condition) throw new InvalidOperationException(description); checks++; }
    private static void Ok(uint status, string description) => Require(status == 0, description + ": " + status);
    private static byte[] Hex(string value) => Convert.FromHexString(value);
    private static byte[] Hex(JsonElement value, string property) => Hex(value.GetProperty(property).GetString()!);
    private static void Equal(ReadOnlySpan<byte> actual, ReadOnlySpan<byte> expected, string description)
        => Require(actual.SequenceEqual(expected), description);

    public static void Main()
    {
        using var vectors = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "vectors.json")));
        using (var host = new MsQuicHost())
        {
            Initial(host, vectors.RootElement.GetProperty("initial_v1"));
            HeaderProtection(host, vectors.RootElement.GetProperty("hp"));
            Kdf(host, vectors.RootElement.GetProperty("kbkdf"));
            Hashes(host);
            Aead(host);
            Retry(host);
            Require(host.OutstandingResources == 0, "All crypto tokens released");
        }
        Console.WriteLine($"packet-crypto: {checks} checks passed; resources drained");
    }

    private static void Initial(MsQuicHost host, JsonElement vector)
    {
        byte[] salt = Hex(vector, "salt"), cid = Hex(vector, "cid");
        byte[] keyLabel = "quic key\0"u8.ToArray(), ivLabel = "quic iv\0"u8.ToArray();
        byte[] hpLabel = "quic hp\0"u8.ToArray(), kuLabel = "quic ku\0"u8.ToArray();
        fixed (byte* s = salt, c = cid, kl = keyLabel, il = ivLabel, hl = hpLabel, ul = kuLabel)
        {
            QUIC_HKDF_LABELS labels = new() { KeyLabel = kl, IvLabel = il, HpLabel = hl, KuLabel = ul };
            CXPLAT_SECRET clientSecret = default, serverSecret = default;
            Ok(MsQuic.CxPlatTlsDeriveInitialSecrets(s, c, (byte)cid.Length, &clientSecret, &serverSecret), "Initial secrets");
            // RFC 9001 A.1, https://datatracker.ietf.org/doc/html/rfc9001#appendix-A.1
            Equal(new ReadOnlySpan<byte>(clientSecret.Secret, 32), Hex("c00cf151ca5be075ed0ebfb5c80323c42d6b7db67881289af4008f1f6c357aea"), "Client secret RFC");
            Equal(new ReadOnlySpan<byte>(serverSecret.Secret, 32), Hex("3c199828fd139efd216c155ad844cc81fb82fa8d7446fa7d78be803acdda951b"), "Server secret RFC");
            QUIC_PACKET_KEY* read = null, write = null, serverRead = null, serverWrite = null;
            QUIC_PACKET_KEY* oldKey = null, newKey = null;
            try
            {
                Ok(MsQuic.QuicPacketKeyCreateInitial(0, &labels, s, (byte)cid.Length, c, &read, &write), "Client Initial keys");
                Ok(MsQuic.QuicPacketKeyCreateInitial(1, &labels, s, (byte)cid.Length, c, &serverRead, &serverWrite), "Server Initial keys");
                Equal(new ReadOnlySpan<byte>(write->Iv, 12), Hex("fa044b2f42a3fd3b46fb255c"), "Client IV RFC");
                Equal(new ReadOnlySpan<byte>(read->Iv, 12), Hex("0ac1493ca1905853b0bba03e"), "Server IV RFC");
                Equal(new ReadOnlySpan<byte>(serverRead->Iv, 12), new ReadOnlySpan<byte>(write->Iv, 12), "Directional IVs");
                byte[] header = Hex(vector, "unprotected_header"), packet = new byte[1200];
                header.CopyTo(packet, 0); Hex(vector, "crypto_payload").CopyTo(packet, header.Length);
                byte[] original = (byte[])packet.Clone();
                byte* nonce = stackalloc byte[12];
                ulong packetNumber = 2;
                MsQuicHost.CombineIv(write->Iv, (byte*)&packetNumber, nonce);
                Equal(new ReadOnlySpan<byte>(nonce, 12), Hex("fa044b2f42a3fd3b46fb255e"), "Nonce combines network packet number");
                fixed (byte* p = packet)
                {
                    Ok(MsQuic.CxPlatEncrypt(write->PacketKey, nonce, (ushort)header.Length, p,
                        (ushort)(packet.Length - header.Length), p + header.Length), "Initial encryption");
                    Equal(packet.AsSpan(header.Length, 16), Hex(vector, "sample"), "Initial sample");
                    byte* mask = stackalloc byte[16];
                    Ok(MsQuic.CxPlatHpComputeMask(write->HeaderKey, 1, p + header.Length, mask), "Initial header mask");
                    Equal(new ReadOnlySpan<byte>(mask, 5), Hex(vector, "hp_mask"), "Initial mask RFC");
                    packet[0] ^= (byte)(mask[0] & 15);
                    for (int i = 1; i <= 4; i++) packet[17 + i] ^= mask[i];
                    Equal(packet, Hex(vector, "protected_packet"), "Full 1200-byte Initial RFC");
                    header.CopyTo(packet, 0);
                    Ok(MsQuic.CxPlatDecrypt(serverRead->PacketKey, nonce, (ushort)header.Length, p,
                        (ushort)(packet.Length - header.Length), p + header.Length), "Opposite role decrypts");
                    Equal(packet.AsSpan(0, packet.Length - 16), original.AsSpan(0, packet.Length - 16), "Original Initial restored");
                }
                CXPLAT_SECRET zero = default;
                Ok(MsQuic.QuicPacketKeyDerive(QUIC_PACKET_KEY_TYPE.QUIC_PACKET_KEY_1_RTT, &labels, &zero, null, 1, &oldKey), "1-RTT key derivation");
                Ok(MsQuic.QuicPacketKeyUpdate(&labels, oldKey, &newKey), "Translated key update");
                Equal(new ReadOnlySpan<byte>(newKey->TrafficSecret->Secret, 32), Hex(vector, "updated_zero_secret"), "Upstream key update control");
                Require(oldKey->HeaderKey != null && newKey->HeaderKey == null, "Key update retains existing header key policy");
                Require(new ReadOnlySpan<byte>(oldKey->TrafficSecret->Secret, 64).IndexOfAnyExcept((byte)0) < 0, "Old traffic secret cleared");
                QUIC_PACKET_KEY* nextKey = null;
                try
                {
                    Require(new ReadOnlySpan<byte>(newKey->TrafficSecret->Secret, 32).IndexOfAnyExcept((byte)0) >= 0,
                        "Updated secret is nonzero before the next update");
                    Ok(MsQuic.QuicPacketKeyUpdate(&labels, newKey, &nextKey), "Second translated key update");
                    Require(new ReadOnlySpan<byte>(newKey->TrafficSecret->Secret, 64).IndexOfAnyExcept((byte)0) < 0,
                        "Nonzero old traffic secret cleared by update");
                }
                finally { MsQuic.QuicPacketKeyFree(nextKey); }
            }
            finally
            {
                MsQuic.QuicPacketKeyFree(newKey); MsQuic.QuicPacketKeyFree(oldKey);
                MsQuic.QuicPacketKeyFree(serverWrite); MsQuic.QuicPacketKeyFree(serverRead);
                MsQuic.QuicPacketKeyFree(write); MsQuic.QuicPacketKeyFree(read);
            }
        }
    }

    private static void HeaderProtection(MsQuicHost host, JsonElement vectors)
    {
        foreach (var vector in vectors.EnumerateArray())
        {
            byte[] key = Hex(vector, "key"), sample = new byte[16 * 3], masks = new byte[sample.Length];
            CXPLAT_HP_KEY* handle = null;
            fixed (byte* k = key, s = sample, m = masks)
            {
                Ok(MsQuic.CxPlatHpKeyCreate((CXPLAT_AEAD_TYPE)(key.Length == 16 ? 0 : 1), k, &handle), "HP key");
                try
                {
                    Ok(MsQuic.CxPlatHpComputeMask(handle, 3, s, m), "Batched HP");
                    for (int i = 0; i < 3; i++) Equal(masks.AsSpan(i * 16, 5), Hex(vector, "mask_prefix"), "HP upstream vector");
                    byte[] expected = (byte[])masks.Clone();
                    sample.CopyTo(masks, 0);
                    Ok(MsQuic.CxPlatHpComputeMask(handle, 3, m, m), "In-place HP");
                    Equal(masks, expected, "In-place HP result");
                    Ok(MsQuic.CxPlatHpComputeMask(handle, 0, null, null), "Empty HP batch");
                    Require(MsQuic.CxPlatHpComputeMask(handle, 1, null, m) == Status.InvalidParameter, "Invalid HP sample rejected");
                }
                finally { MsQuic.CxPlatHpKeyFree(handle); }
            }
        }
    }

    private static void Kdf(MsQuicHost host, JsonElement vectors)
    {
        foreach (var vector in vectors.EnumerateArray())
        {
            byte[] key = Hex(vector, "key"), context = Hex(vector, "context"), expected = Hex(vector, "output");
            byte[] label = Encoding.ASCII.GetBytes(vector.GetProperty("label").GetString()! + "\0");
            byte[] output = new byte[expected.Length];
            fixed (byte* k = key, c = context, l = label, o = output)
                Ok(host.Table.CxPlatKbKdfDerive(host.Table.Context, k, (uint)key.Length, l, c,
                    (uint)context.Length, (uint)output.Length, o), "KBKDF");
            Equal(output, expected, "KBKDF upstream vector");
        }
    }

    private static void Hashes(MsQuicHost host)
    {
        // RFC 4231 section 4.2: https://datatracker.ietf.org/doc/html/rfc4231#section-4.2
        byte[] key = Enumerable.Repeat((byte)0x0b, 20).ToArray(), data = "Hi There"u8.ToArray();
        string[] expected = [
            "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7",
            "afd03944d84895626b0825f4ab46907f15f9dadbe4101ec682aa034c7cebc59cfaea9ea9076ede7f4af152e8b2fa9cb6",
            "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cde" +
            "daa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854"];
        fixed (byte* k = key, d = data)
        {
            for (int type = 0; type < 3; type++)
            {
                CXPLAT_HASH* hash = null;
                Ok(MsQuic.CxPlatHashCreate((CXPLAT_HASH_TYPE)type, k, (uint)key.Length, &hash), "HMAC create");
                try
                {
                    byte[] bytes = Hex(expected[type]), output = new byte[bytes.Length];
                    fixed (byte* o = output)
                    {
                        for (int repeat = 0; repeat < 3; repeat++)
                        {
                            Ok(MsQuic.CxPlatHashCompute(hash, d, (uint)data.Length, (uint)output.Length, o), "Independent HMAC compute");
                            Equal(output, bytes, "HMAC known vector");
                        }
                        Require(MsQuic.CxPlatHashCompute(hash, d, (uint)data.Length, (uint)output.Length - 1, o) == Status.InvalidParameter,
                            "HMAC output length checked");
                    }
                }
                finally { MsQuic.CxPlatHashFree(hash); }
            }
        }
    }

    private static void Aead(MsQuicHost host)
    {
        foreach (int keyLength in new[] { 16, 32 })
        {
            byte[] material = new byte[keyLength], nonce = new byte[12], aad = "packet header"u8.ToArray();
            CXPLAT_KEY* key = null;
            fixed (byte* k = material, iv = nonce, a = aad)
            {
                Ok(MsQuic.CxPlatKeyCreate((CXPLAT_AEAD_TYPE)(keyLength == 16 ? 0 : 1), k, &key), "AEAD key");
                try
                {
                    foreach (int length in new[] { 0, 1, 16, 1200, 65519 })
                    {
                        byte[] buffer = new byte[length + 16];
                        for (int i = 0; i < length; i++) buffer[i] = (byte)(i * 31 + 17);
                        byte[] original = (byte[])buffer.Clone();
                        fixed (byte* b = buffer)
                        {
                            Ok(MsQuic.CxPlatEncrypt(key, iv, (ushort)aad.Length, a, (ushort)buffer.Length, b), "AEAD encrypt");
                            byte[] ciphertext = (byte[])buffer.Clone();
                            buffer[^1] ^= 1;
                            byte[] invalid = (byte[])buffer.Clone();
                            Require(MsQuic.CxPlatDecrypt(key, iv, (ushort)aad.Length, a, (ushort)buffer.Length, b) == Status.TlsError, "Bad tag rejected");
                            Equal(buffer, invalid, "Failed authentication leaves caller bytes intact");
                            ciphertext.CopyTo(buffer, 0);
                            aad[0] ^= 1;
                            Require(MsQuic.CxPlatDecrypt(key, iv, (ushort)aad.Length, a, (ushort)buffer.Length, b) == Status.TlsError, "Wrong AAD rejected");
                            Equal(buffer, ciphertext, "Wrong AAD exposes no plaintext");
                            aad[0] ^= 1;
                            Ok(MsQuic.CxPlatDecrypt(key, iv, (ushort)aad.Length, a, (ushort)buffer.Length, b), "AEAD decrypt");
                            Equal(buffer.AsSpan(0, length), original.AsSpan(0, length), "AEAD round trip");
                            Require(MsQuic.CxPlatEncrypt(key, iv, 0, null, 15, b) == Status.InvalidParameter, "Short AEAD buffer rejected");
                        }
                        GC.Collect(); GC.WaitForPendingFinalizers();
                    }
                }
                finally { MsQuic.CxPlatKeyFree(key); }
                key = (CXPLAT_KEY*)123;
                Require(MsQuic.CxPlatKeyCreate((CXPLAT_AEAD_TYPE)2, k, &key) == Status.NotSupported && key == null, "Unsupported cipher clears output");
            }
        }
        MsQuic.CxPlatKeyFree(null); MsQuic.CxPlatHpKeyFree(null); MsQuic.CxPlatHashFree(null);
    }

    private static void Retry(MsQuicHost host)
    {
        // RFC 9001 A.4: the tag authenticates the ODCID-prefixed Retry packet.
        byte[] key = Hex("be0c690b9f66575a1d766b54e368c84e"), nonce = Hex("461599d35d632bf2239825bb");
        byte[] pseudo = Hex("088394c8f03e515708ff000000010008f067a5502a4262b5746f6b656e");
        byte[] tag = new byte[16];
        CXPLAT_KEY* handle = null;
        fixed (byte* k = key, iv = nonce, a = pseudo, t = tag)
        {
            Ok(MsQuic.CxPlatKeyCreate((CXPLAT_AEAD_TYPE)0, k, &handle), "Retry key");
            try
            {
                Ok(MsQuic.CxPlatEncrypt(handle, iv, (ushort)pseudo.Length, a, 16, t), "Retry integrity");
                Equal(tag, Hex("04a265ba2eff4d829058fb3f0f2496ba"), "Retry integrity RFC");
            }
            finally { MsQuic.CxPlatKeyFree(handle); }
        }
    }
}

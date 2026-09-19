using System.Globalization;
using System.Runtime.InteropServices;
using static Managed.Smb.LibSmb2;
using SmbStatvfs = Managed.Smb.LibSmb2.__DotCcTags.smb2_statvfs;

internal static unsafe class Program
{
    private static readonly Dictionary<string, string> Results = new(StringComparer.Ordinal);
    private static void Hex(string name, byte* data, int length) => Results.Add(name, Convert.ToHexStringLower(new ReadOnlySpan<byte>(data, length)));
    private static void Number(string name, ulong value) => Results.Add(name, value.ToString(CultureInfo.InvariantCulture));
    private static void Ok(int status) { if (status != 0) throw new InvalidOperationException($"Crypto status {status}"); }
    private static void Size<T>(string name) where T : unmanaged => Number($"abi.{name}.size", (ulong)sizeof(T));
    private static void Offset(string name, void* start, void* field) => Number("abi." + name, (ulong)((byte*)field - (byte*)start));
    private struct Completion { public uint Status; public ulong Cookie; }
    private static void Complete(smb2_context* context, int status, void* data, void* opaque)
    {
        var result = (Completion*)opaque;
        result->Status = unchecked((uint)status);
        result->Cookie = *(ulong*)data;
    }

    private static void Main()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This pinned ABI campaign currently targets Linux x64.");
        byte* abc = stackalloc byte[] { 97, 98, 99 };
        byte* digest = stackalloc byte[64];
        byte* pattern = stackalloc byte[257];
        for (int i = 0; i < 257; i++) pattern[i] = unchecked((byte)i);
        SHA256Context sha256 = default; SHA512Context sha512 = default;
        Ok(SHA256Reset(&sha256)); Ok(SHA256Input(&sha256, abc, 3)); Ok(SHA256Result(&sha256, digest)); Hex("sha256.abc", digest, 32);
        Ok(SHA512Reset(&sha512)); Ok(SHA512Input(&sha512, abc, 3)); Ok(SHA512Result(&sha512, digest)); Hex("sha512.abc", digest, 64);
        Ok(SHA256Reset(&sha256)); Ok(SHA256Input(&sha256, pattern, 13)); Ok(SHA256Input(&sha256, pattern + 13, 244)); Ok(SHA256Result(&sha256, digest)); Hex("sha256.pattern257", digest, 32);
        Ok(SHA512Reset(&sha512)); Ok(SHA512Input(&sha512, pattern, 13)); Ok(SHA512Input(&sha512, pattern + 13, 244)); Ok(SHA512Result(&sha512, digest)); Hex("sha512.pattern257", digest, 64);
        MD4_CTX md4 = default; MD4Init(&md4); MD4Update(&md4, abc, 3); MD4Final(digest, &md4); Hex("md4.abc", digest, 16);
        MD5Context md5 = default; MD5Init(&md5); MD5Update(&md5, abc, 3); MD5Final(digest, &md5); Hex("md5.abc", digest, 16);
        byte* key = stackalloc byte[131];
        byte* hi = stackalloc byte[] { 72, 105, 32, 84, 104, 101, 114, 101 };
        new Span<byte>(key, 20).Fill(0x0b);
        smb2_hmac_md5(hi, 8, key, 16, digest); Hex("hmac.md5", digest, 16);
        Ok(hmac(SHAversion.SHA256, hi, 8, key, 20, digest)); Hex("hmac.sha256", digest, 32);
        new Span<byte>(key, 131).Fill(0xaa);
        ReadOnlySpan<byte> longtext = "Test Using Larger Than Block-Size Key - Hash Key First"u8;
        fixed (byte* text = longtext) { Ok(hmac(SHAversion.SHA256, text, (ulong)longtext.Length, key, 131, digest)); }
        Hex("hmac.sha256.longkey", digest, 32);
        byte* block = stackalloc byte[16]; byte* output = stackalloc byte[16];
        for (int i = 0; i < 16; i++) { key[i] = (byte)i; block[i] = (byte)(i * 17); }
        AES128_ECB_encrypt(block, key, output); Hex("aes128.ecb", output, 16);
        byte* cmacKey = stackalloc byte[] {0x2b,0x7e,0x15,0x16,0x28,0xae,0xd2,0xa6,0xab,0xf7,0x15,0x88,0x09,0xcf,0x4f,0x3c};
        byte* cmacBlock = stackalloc byte[] {0x6b,0xc1,0xbe,0xe2,0x2e,0x40,0x9f,0x96,0xe9,0x3d,0x7e,0x11,0x73,0x93,0x17,0x2a};
        smb3_aes_cmac_128(cmacKey, cmacBlock, 0, output); Hex("aes128.cmac.empty", output, 16);
        smb3_aes_cmac_128(cmacKey, cmacBlock, 16, output); Hex("aes128.cmac.block", output, 16);
        byte* nonce = stackalloc byte[] {0,0,0,3,2,1,0,0xa0,0xa1,0xa2,0xa3,0xa4,0xa5};
        byte* aad = stackalloc byte[8]; byte* payload = stackalloc byte[23]; byte* ciphertext = stackalloc byte[23]; byte* tag = stackalloc byte[8];
        for (int i = 0; i < 16; i++) key[i] = (byte)(0xc0 + i);
        for (int i = 0; i < 8; i++) aad[i] = (byte)i;
        for (int i = 0; i < 23; i++) payload[i] = (byte)(i + 8);
        aes128ccm_encrypt(key, nonce, 13, aad, 8, payload, 23, tag, 8); Hex("aes128.ccm.ciphertext", payload, 23); Hex("aes128.ccm.tag", tag, 8);
        new ReadOnlySpan<byte>(payload, 23).CopyTo(new Span<byte>(ciphertext, 23));
        Number("aes128.ccm.decrypt_ok", aes128ccm_decrypt(key, nonce, 13, aad, 8, payload, 23, tag, 8) == 0 ? 1UL : 0); Hex("aes128.ccm.plaintext", payload, 23);
        new ReadOnlySpan<byte>(ciphertext, 23).CopyTo(new Span<byte>(payload, 23)); tag[0] ^= 1;
        Number("aes128.ccm.reject_tag", aes128ccm_decrypt(key, nonce, 13, aad, 8, payload, 23, tag, 8) != 0 ? 1UL : 0);
        Number("abi.pointer.size", (ulong)sizeof(void*));
        Size<SHA256Context>("SHA256Context"); Offset("SHA256Context.Message_Block", &sha256, sha256.Message_Block); Offset("SHA256Context.Computed", &sha256, &sha256.Computed);
        Size<SHA512Context>("SHA512Context"); Offset("SHA512Context.Message_Block", &sha512, sha512.Message_Block); Offset("SHA512Context.Computed", &sha512, &sha512.Computed);
        Size<MD4_CTX>("MD4_CTX"); Size<MD5Context>("struct MD5Context");
        smb2_iovec iov = default; Size<smb2_iovec>("struct smb2_iovec"); Offset("struct smb2_iovec.len", &iov, &iov.len); Offset("struct smb2_iovec.free", &iov, &iov.free);
        smb2_stat_64 stat = default; Size<smb2_stat_64>("struct smb2_stat_64"); Offset("struct smb2_stat_64.smb2_size", &stat, &stat.smb2_size); Offset("struct smb2_stat_64.smb2_attributes", &stat, &stat.smb2_attributes);
        SmbStatvfs statvfs = default; Size<SmbStatvfs>("struct smb2_statvfs"); Offset("struct smb2_statvfs.f_blocks", &statvfs, &statvfs.f_blocks);
        smb2_header header = default; Size<smb2_header>("struct smb2_header"); Offset("struct smb2_header.status", &header, &header.status); Offset("struct smb2_header.message_id", &header, &header.message_id); Offset("struct smb2_header.session_id", &header, &header.session_id); Offset("struct smb2_header.signature", &header, header.signature);
        smb2_pdu pdu = default; Size<smb2_pdu>("struct smb2_pdu"); Offset("struct smb2_pdu.cb", &pdu, &pdu.cb); Offset("struct smb2_pdu.cb_data", &pdu, &pdu.cb_data);
        Completion completed = default; ulong cookie = 0xfedcba9876543210;
        pdu.cb = &Complete; pdu.cb_data = &completed;
        pdu.cb(null, unchecked((int)0xc0000022u), &cookie, pdu.cb_data);
        Number("callback.status", completed.Status); Number("callback.cookie", completed.Cookie);
        Number("status.access_denied.errno", (ulong)nterror_to_errno(0xc0000022u));
        var expected = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "expected.txt"))
            .Where(line => line.Length != 0).Select(line => line.Split('=', 2))
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.Ordinal);
        foreach (var pair in Results.OrderBy(pair => pair.Key, StringComparer.Ordinal)) Console.WriteLine($"{pair.Key}={pair.Value}");
        var failures = expected.Where(pair => !Results.TryGetValue(pair.Key, out var actual) || pair.Value != actual)
            .Select(pair => $"{pair.Key}: expected {pair.Value}, actual {Results.GetValueOrDefault(pair.Key, "<missing>")}")
            .Concat(Results.Keys.Except(expected.Keys).Select(key => "Unexpected result: " + key)).ToArray();
        if (failures.Length != 0) throw new InvalidOperationException(string.Join(Environment.NewLine, failures));
    }
}

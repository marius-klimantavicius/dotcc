using static Managed.Security.PicoTls;
using System.Text;
using Managed.Security;

// Ports of the named cases in pinned upstream t/picotls.c. Calls run the actual
// translated implementation; no replacement codec or copied product types.
static unsafe class Program
{
    static int checks;
    static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); checks++; }
    static void Main()
    {
        using var scope = CallbackScope.Enter();
        IpAddress(); QuicInteger(); Base64(); JsonEscape(); Hmac(scope); LengthBlocks();
        scope.ThrowIfFailed();
        Console.WriteLine($"PASS upstream ports: 8 cases, {checks} checks");
    }
    static void IpAddress()
    {
        foreach (string name in new[] { "www.google.com", "www.google.com.", "www", "", "123", "1.1.1.1", "2001:db8::2:1" })
        {
            byte[] text = Encoding.UTF8.GetBytes(name + "\0");
            fixed (byte* p = text)
                Check((PicoTls.ptls_server_name_is_ipaddr(p) != 0) == (name is "1.1.1.1" or "2001:db8::2:1"), "is_ipaddr: " + name);
        }
    }
    static void QuicInteger()
    {
        (ulong value, byte[] encoded)[] patterns = [
            (0, [0]), (0, [0x40, 0]), (0, [0x80, 0, 0, 0]), (0, [0xc0, 0, 0, 0, 0, 0, 0, 0]),
            (9, [9]), (9, [0x40, 9]), (9, [0x80, 0, 0, 9]), (9, [0xc0, 0, 0, 0, 0, 0, 0, 9]),
            (0x1234, [0x52, 0x34]), (0x1234, [0x80, 0, 0x12, 0x34]), (0x1234, [0xc0, 0, 0, 0, 0, 0, 0x12, 0x34]),
            (0x12345678, [0x92, 0x34, 0x56, 0x78]), (0x12345678, [0xc0, 0, 0, 0, 0x12, 0x34, 0x56, 0x78]),
            (0x123456789abcdef, [0xc1, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef])
        ];
        foreach (var (value, encoded) in patterns)
        fixed (byte* start = encoded)
        {
            byte* cursor = start;
            Check(PicoTls.ptls_decode_quicint(&cursor, start + encoded.Length) == value, "quicint decode");
            Check(cursor == start + encoded.Length, "quicint cursor");
            // Additional bounded truncation coverage, including every prefix.
            for (int length = 0; length < encoded.Length; length++)
            {
                cursor = start;
                Check(PicoTls.ptls_decode_quicint(&cursor, start + length) == ulong.MaxValue, "quicint truncated");
                Check(cursor >= start && cursor <= start + length, "quicint truncated cursor bound");
            }
        }
        byte* buffer = stackalloc byte[9];
        foreach (ulong value in new ulong[] { 0, 1, 63, 64, 16383, 16384, 1073741823, 1073741824, (1UL << 62) - 1 })
        {
            new Span<byte>(buffer, 9).Fill(123);
            byte* end = PicoTls.dotcc_ptls_encode_quicint(buffer, value), cursor = buffer;
            Check(end - buffer is >= 1 and <= 8, "quicint encoded length");
            Check(PicoTls.ptls_decode_quicint(&cursor, buffer + 9) == value, "quicint round trip");
            Check(cursor == end && *cursor == 123, "quicint sentinel");
        }
    }
    static void Base64()
    {
        st_ptls_base64_decode_state_t state = default;
        st_ptls_buffer_t buffer = PicotlsBuffer.Create();
        try
        {
            foreach (byte[] text in new byte[][] { "aGVsbG8gd29ybGQ=\0"u8.ToArray(), "a$b\0"u8.ToArray(), [0x61, 0xff, 0x62, 0] })
            {
                buffer.off = 0;
                PicoTls.ptls_base64_decode_init(&state);
                fixed (byte* p = text)
                {
                    int result = PicoTls.ptls_base64_decode(p, &state, &buffer);
                    if (text.Length > 5)
                    {
                        Check(result == 0 && buffer.off == 11, "base64 valid length");
                        Check(new ReadOnlySpan<byte>(buffer.@base, 11).SequenceEqual("hello world"u8), "base64 decoded data");
                    }
                    else Check(result != 0, "base64 invalid alphabet");
                }
            }
        }
        finally { PicoTls.dotcc_ptls_buffer_dispose(&buffer); }
    }
    static void JsonEscape()
    {
        (string input, string expected)[] cases = [
            ("\" \\ / \b \f \n \r \t foo bar", "\\\" \\\\ \\/ \\b \\f \\n \\r \\t foo bar"),
            ("こんにちは、🌏！", "こんにちは、🌏！"),
            ("\0 \u001f \u007f", "\\u0000 \\u001f \\u007f")
        ];
        byte* output = stackalloc byte[256];
        foreach (var (input, expected) in cases)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input), want = Encoding.UTF8.GetBytes(expected);
            new Span<byte>(output, 256).Fill(0xa5);
            fixed (byte* p = bytes)
            {
                byte* end = PicoTls.ptls_jsonescape(output, p, (ulong)bytes.Length);
                Check(end - output == want.Length && *end == 0, "jsonescape length and terminator");
                Check(new ReadOnlySpan<byte>(output, want.Length).SequenceEqual(want), "jsonescape bytes");
                Check(end[1] == 0xa5, "jsonescape sentinel");
            }
        }
    }
    static void Hmac(CallbackScope scope)
    {
        // Upstream test_hmac_sha256: RFC 4231 case 1, repeated reset then free.
        byte[] key = Enumerable.Repeat((byte)0x0b, 20).ToArray();
        byte[] expected = Convert.FromHexString("b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7");
        st_ptls_hash_context_t* context;
        fixed (byte* p = key) context = PicoTls.ptls_hmac_create(BclCryptoProvider.HashSha256, p, (ulong)key.Length);
        scope.ThrowIfFailed();
        Check(context != null, "hmac create");
        byte* output = stackalloc byte[32];
        try
        {
            for (int repetition = 0; repetition < 3; repetition++)
            {
                fixed (byte* message = "Hi There"u8) context->update(context, message, 8);
                scope.ThrowIfFailed();
                st_ptls_hash_context_t* current = context;
                if (repetition == 2) context = null;
                current->final(current, output, (en_ptls_hash_final_mode_t)(repetition == 2 ? 0 : 1));
                scope.ThrowIfFailed();
                Check(new ReadOnlySpan<byte>(output, 32).SequenceEqual(expected), "hmac repeated reset/free");
            }
        }
        finally { if (context != null) context->final(context, null, (en_ptls_hash_final_mode_t)0); }
    }
    static void LengthBlocks()
    {
        // Exact upstream tls-block8 / tls-block16 bounds and quic/block sizes.
        foreach (var (capacity, size) in new (ulong, int)[] { (1, 255), (2, 65535), (ulong.MaxValue, 3), (ulong.MaxValue, 123) })
        {
            byte[] input = new byte[size];
            for (int i = 0; i < size; i++)
                input[i] = capacity == ulong.MaxValue ? (size == 3 ? "abc"u8[i] : (byte)0x55) : (byte)i;
            st_ptls_buffer_t buffer = PicotlsBuffer.Create();
            try
            {
                fixed (byte* p = input)
                {
                    Check(PicoTls.dotcc_ptls_buffer_push_block(&buffer, capacity, new() { @base = p, len = (ulong)size }) == 0, "upstream block push");
                    st_ptls_iovec_t body = default;
                    Check(PicoTls.dotcc_ptls_decode_block(new() { @base = buffer.@base, len = buffer.off }, capacity, &body) == 0, "upstream block decode");
                    Check(body.len == (ulong)size && new ReadOnlySpan<byte>(body.@base, size).SequenceEqual(input), "upstream block content");
                    Check(PicoTls.dotcc_ptls_decode_block(new() { @base = buffer.@base, len = buffer.off - 1 }, capacity, &body) == 50, "block truncation rejected");
                    Check(body.@base == null && body.len == 0, "failed decode publishes no body");
                }
                if (capacity != ulong.MaxValue)
                {
                    byte[] tooLong = new byte[size + 1];
                    fixed (byte* p = tooLong)
                        Check(PicoTls.dotcc_ptls_buffer_push_block(&buffer, capacity, new() { @base = p, len = (ulong)tooLong.Length }) == 0x20c, "upstream block overflow");
                }
            }
            finally { PicoTls.dotcc_ptls_buffer_dispose(&buffer); }
        }
        byte* extra = stackalloc byte[] { 1, 42, 99 };
        st_ptls_iovec_t decoded = default;
        Check(PicoTls.dotcc_ptls_decode_block(new() { @base = extra, len = 3 }, 1, &decoded) == 50, "block trailing bytes rejected");
        Check(decoded.@base == null && decoded.len == 0, "trailing data publishes no body");
        Check(PicoTls.dotcc_ptls_decode_block(new() { @base = extra, len = 3 }, 0, &decoded) == 47, "zero length-width rejected");
    }
}

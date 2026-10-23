using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Managed.S2nTls.Crypto;

internal static unsafe class Asn1StringChecks
{
    // Actual LP64 ASN1_STRING fields, independently measured by provider-layout.c.
    [StructLayout(LayoutKind.Sequential)]
    private struct StringFields { public int Length, Type; public byte* Data; public long Flags; }

    internal static void Run(Action<string, Action> test, Func<string[], string> oracle)
    {
        test("asn1/string-to-utf8-native-types-encodings-failures", () =>
        {
            int count = 0, contexts = OpenSslAbi.LiveContexts;
            void Compare(int? type, byte[] bytes, int? length = null)
            {
                void* value = null;
                try
                {
                    if (type != null)
                    {
                        byte[] encoded = [12, 6, (byte)'c', (byte)'a', (byte)'c', (byte)'h', (byte)'e', (byte)'d'];
                        fixed (byte* source = encoded)
                        {
                            byte* cursor = source;
                            value = OpenSslAbi.d2i_ASN1_UTF8STRING(null, &cursor, encoded.Length);
                            if (value == null) throw new Exception("Could not create real ASN1_STRING storage");
                        }
                        var fields = (StringFields*)value;
                        byte* data = (byte*)NativeMemory.Alloc(checked((nuint)bytes.Length + 1));
                        if (data == null) throw new OutOfMemoryException();
                        bytes.CopyTo(new Span<byte>(data, bytes.Length)); data[bytes.Length] = 0;
                        NativeMemory.Free(fields->Data);
                        fields->Data = data; fields->Length = length ?? bytes.Length; fields->Type = type.Value;
                        // The registered managed state's old Text is deliberately
                        // unchanged: native conversion reads these actual fields.
                    }
                    while (OpenSslAbi.ERR_get_error() != 0) { }
                    byte* output = (byte*)1;
                    int result = OpenSslAbi.ASN1_STRING_to_UTF8(&output, value);
                    bool error = OpenSslAbi.ERR_get_error() != 0;
                    while (OpenSslAbi.ERR_get_error() != 0) { }
                    string hex = result < 0 ? "" : Convert.ToHexString(new ReadOnlySpan<byte>(output, result + 1)).ToLowerInvariant();
                    string actual = $"{result} {(error ? 1 : 0)} {(output == (byte*)1 ? 1 : 0)} {hex}".TrimEnd();
                    if (result >= 0) NativeMemory.Free(output);
                    string expected = oracle(["asn1-utf8", type?.ToString(CultureInfo.InvariantCulture) ?? "null",
                        Convert.ToHexString(bytes).ToLowerInvariant(), (length ?? bytes.Length).ToString(CultureInfo.InvariantCulture)]);
                    count++;
                    if (actual != expected)
                        throw new Exception($"ASN1 type {type}, length {length ?? bytes.Length}, input {Convert.ToHexString(bytes)}: managed {actual}; native {expected}");
                }
                finally { OpenSslAbi.ASN1_STRING_free(value); }
            }

            Compare(29, Encoding.ASCII.GetBytes("invalid"));
            Compare(null, []);
            foreach (int type in Enumerable.Range(-4, 45).Concat([int.MinValue, int.MaxValue]))
                foreach (byte[] bytes in new byte[][] { [], [65], [65, 0], [0, 0, 0, 65] }) Compare(type, bytes);
            foreach (int type in new[] { 12, 18, 19, 20, 22, 23, 24, 26 })
                for (int value = 0; value < 256; value++) Compare(type, [(byte)value]);
            for (int value = 0; value < 256; value++)
            {
                Compare(30, [0, (byte)value]); Compare(30, [(byte)value, 0]);
                Compare(28, [0, 0, 0, (byte)value]);
            }
            foreach (string hex in new[] { "", "00", "410042", "c280", "dfbf", "e0a080", "ed9fbf", "eda080", "edbfbf", "efbfbf",
                "f0908080", "f48fbfbf", "f4908080", "f7bfbfbf", "f888808080", "fbbfbfbfbf", "fc8480808080", "fdbfbfbfbfbf",
                "80", "c0", "c080", "c180", "c2", "c241", "e0", "e08080", "e180", "e14180", "f0808080",
                "f09080", "f0904180", "f880808080", "fc8080808080", "fe808080808080", "ff" })
                Compare(12, Convert.FromHexString(hex));
            foreach (string hex in new[] { "", "00", "0041", "004100", "d800", "dfff", "d800dc00", "ffff", "0000", "00000041",
                "000000", "00000000", "0000004100", "0000d800", "0010ffff", "00110000", "001fffff", "00200000", "03ffffff", "04000000", "7fffffff", "80000000", "ffffffff" })
            {
                Compare(30, Convert.FromHexString(hex)); Compare(28, Convert.FromHexString(hex));
            }
            foreach (int type in new[] { 12, 18, 19, 20, 22, 23, 24, 26, 28, 29, 30 })
                foreach (byte[] bytes in new byte[][] { [], [65, 0, 66], [65, 66, 0, 67], [0, 0, 0, 65] }) Compare(type, bytes, -1);
            if (OpenSslAbi.LiveContexts != contexts) throw new Exception("ASN1 string conversion leaked registered storage");
            Console.WriteLine($"ASN1 native differential vectors: {count}");
        });
    }
}

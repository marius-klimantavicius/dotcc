using System.Text;
using Managed.S2nTls.Crypto;

internal static unsafe class Base64Checks
{
    internal static void Run(Action<string, Action> test, Func<string[], string> oracle)
    {
        void Compare(string operation, byte[] input)
        {
            byte[] guarded = new byte[128];
            guarded.AsSpan().Fill(0xa5);
            while (OpenSslAbi.ERR_get_error() != 0) { }
            int result;
            fixed (byte* source = input)
            fixed (byte* target = guarded)
                result = operation == "base64-encode"
                    ? OpenSslAbi.EVP_EncodeBlock(target + 8, source, input.Length)
                    : OpenSslAbi.EVP_DecodeBlock(target + 8, source, input.Length);
            ulong error = OpenSslAbi.ERR_get_error();
            string actual = $"{result} {error} {Convert.ToHexString(guarded).ToLowerInvariant()}";
            string expected = oracle([operation, Convert.ToHexString(input).ToLowerInvariant()]);
            if (actual != expected)
                throw new Exception($"{operation} input {Convert.ToHexString(input)}: managed {actual}; native {expected}");
        }
        test("base64/encode-native-terminator-and-guards", () =>
        {
            for (int length = 0; length <= 64; length++)
                Compare("base64-encode", Enumerable.Range(0, length).Select(i => (byte)(i * 37 + length)).ToArray());
        });
        test("base64/decode-native-padding-whitespace-reject-and-partial-writes", () =>
        {
            foreach (string text in new[] { "", "AAAA", "AA==", "A===", "====", "AA=A", "AAAA=AAA", "AAAA    AAAA",
                         "\nAAAA", "AAAA\n", "AAAA-", " \tAAAA\r\n-", "AAA", "A", "\t\t", "AAAA!!!!", "AAAAAAA!" })
                Compare("base64-decode", Encoding.ASCII.GetBytes(text));
            // Native is the oracle for every byte in each of the four slots,
            // then every leading/trailing byte, including NUL and high bytes.
            for (int position = 0; position < 4; position++)
                for (int value = 0; value <= 255; value++)
                {
                    byte[] input = Encoding.ASCII.GetBytes("AAAA");
                    input[position] = (byte)value;
                    Compare("base64-decode", input);
                }
            for (int value = 0; value <= 255; value++)
            {
                Compare("base64-decode", [(byte)value, (byte)'A', (byte)'A', (byte)'A', (byte)'A']);
                Compare("base64-decode", [(byte)'A', (byte)'A', (byte)'A', (byte)'A', (byte)value]);
            }
            for (int length = 0; length <= 40; length++)
                Compare("base64-decode", Encoding.ASCII.GetBytes(Convert.ToBase64String(Enumerable.Range(0, length).Select(i => (byte)(i * 19)).ToArray())));
        });
    }
}

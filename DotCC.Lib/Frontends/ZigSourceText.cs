using System;
using System.IO;
using System.Text;

namespace DotCC.Frontends;

/// <summary>Zig source bytes must reach member decoding without replacement characters.
/// Accept the native-compatible UTF-8 BOM explicitly; never auto-detect UTF-16.</summary>
internal static class ZigSourceText
{
    private static readonly UTF8Encoding Utf8 = new(false, true);

    internal static string Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var start = bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }) ? 3 : 0;
        try { return Utf8.GetString(bytes, start, bytes.Length - start); }
        catch (DecoderFallbackException error)
        {
            throw new CompileException($"zig: unsupported invalid UTF-8 source bytes {Convert.ToHexString(error.BytesUnknown ?? [])}"
                + $" at byte offset {start + error.Index} in '{path}'");
        }
    }
}

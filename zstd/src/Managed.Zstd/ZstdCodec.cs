using System.Text;
using VM = Managed.Zstd.Native.Zstd;

namespace Managed.Zstd;

/// <summary>An upstream zstd error, including the original stable error code.</summary>
public sealed class ZstdException : IOException
{
    public int ErrorCode { get; }
    internal ZstdException(int code, string message) : base(message) => ErrorCode = code;
}

internal static unsafe class NativeResult
{
    internal static ulong Check(ulong result)
    {
        if (VM.dz_is_error(result) != 0) throw new ZstdException(VM.dz_error_code(result), Text(VM.dz_error_name(result)));
        return result;
    }
    internal static string Text(byte* value)
    {
        if (value == null) return "Unknown zstd error";
        int length = 0;
        while (value[length] != 0) length = checked(length + 1);
        return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(value, length));
    }
    internal static void Buffers(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (source.Overlaps(destination)) throw new ArgumentException("Input and output buffers must not overlap.");
    }
}

/// <summary>Convenience operations over the real translated, single-thread zstd core.</summary>
public static unsafe class ZstdCodec
{
    public static uint VersionNumber { get { using var binding = ZstdRuntime.Enter(); return VM.dz_version_number(); } }
    public static string Version { get { using var binding = ZstdRuntime.Enter(); return NativeResult.Text(VM.dz_version()); } }
    public static int CompressBound(int sourceLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sourceLength);
        using var binding = ZstdRuntime.Enter();
        return checked((int)NativeResult.Check(VM.dz_compress_bound((ulong)sourceLength)));
    }
    /// <summary>Returns null for a valid frame whose decoded content size is unknown.</summary>
    public static ulong? GetFrameContentSize(ReadOnlySpan<byte> frame)
    {
        using var binding = ZstdRuntime.Enter();
        fixed (byte* data = frame)
        {
            ulong size = VM.dz_frame_content_size(data, (ulong)frame.Length);
            if (size == ulong.MaxValue) return null;
            if (size == ulong.MaxValue - 1) throw new InvalidDataException("Invalid or incomplete zstd frame header.");
            return size;
        }
    }
    public static uint GetFrameDictionaryId(ReadOnlySpan<byte> frame)
    {
        using var binding = ZstdRuntime.Enter();
        fixed (byte* data = frame) return VM.dz_frame_dictionary_id(data, (ulong)frame.Length);
    }
    public static byte[] Compress(ReadOnlySpan<byte> source, int level = 3, ZstdDictionary? dictionary = null)
    {
        using var compressor = new ZstdCompressor(level, dictionary);
        var output = new byte[CompressBound(source.Length)];
        int length = compressor.Compress(source, output);
        return output.AsSpan(0, length).ToArray();
    }
    /// <summary>Decodes concatenated and skippable frames with a hard output bound.
    /// Allocation never trusts an advertised frame size.</summary>
    public static byte[] Decompress(ReadOnlySpan<byte> frames, ZstdDictionary? dictionary = null, int maxOutputBytes = 256 * 1024 * 1024)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxOutputBytes);
        using var binding = ZstdRuntime.Enter();
        fixed (byte* data = frames)
        {
            ulong advertised = VM.dz_frame_content_size(data, (ulong)frames.Length);
            if (advertised < ulong.MaxValue - 1 && advertised > (ulong)maxOutputBytes)
                throw new InvalidDataException("Advertised frame content exceeds the configured output limit.");
        }
        using var decoder = new ZstdDecompressor(dictionary);
        using var output = new MemoryStream();
        var buffer = new byte[Math.Min(131072, Math.Max(1, maxOutputBytes))];
        int offset = 0;
        while (true)
        {
            var step = decoder.Process(frames[offset..], buffer);
            offset = checked(offset + step.Consumed);
            if (output.Length + step.Written > maxOutputBytes) throw new InvalidDataException("Decoded content exceeds the configured output limit.");
            output.Write(buffer, 0, step.Written);
            if (step.Completed && offset == frames.Length) return output.ToArray();
            if (step.Consumed == 0 && step.Written == 0)
                throw new InvalidDataException(offset == frames.Length ? "Truncated zstd frame." : "zstd decompression made no progress.");
        }
    }
}

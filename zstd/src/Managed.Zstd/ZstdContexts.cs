using System.Buffers;
using VM = Managed.Zstd.Native.Zstd;

namespace Managed.Zstd;

public enum ZstdEndDirective { Continue = VM.ZSTD_e_continue, Flush = VM.ZSTD_e_flush, End = VM.ZSTD_e_end }
public enum ZstdStrategy
{
    Fast = VM.ZSTD_fast, DoubleFast = VM.ZSTD_dfast, Greedy = VM.ZSTD_greedy,
    Lazy = VM.ZSTD_lazy, Lazy2 = VM.ZSTD_lazy2, BinaryTreeLazy = VM.ZSTD_btlazy2,
    BinaryTreeOptimal = VM.ZSTD_btopt, BinaryTreeUltra = VM.ZSTD_btultra, BinaryTreeUltra2 = VM.ZSTD_btultra2
}
public enum ZstdCompressionParameter
{
    CompressionLevel = VM.ZSTD_c_compressionLevel, WindowLog = VM.ZSTD_c_windowLog, HashLog = VM.ZSTD_c_hashLog, ChainLog = VM.ZSTD_c_chainLog,
    SearchLog = VM.ZSTD_c_searchLog, MinMatch = VM.ZSTD_c_minMatch, TargetLength = VM.ZSTD_c_targetLength, Strategy = VM.ZSTD_c_strategy,
    EnableLongDistanceMatching = VM.ZSTD_c_enableLongDistanceMatching, LdmHashLog = VM.ZSTD_c_ldmHashLog, LdmMinMatch = VM.ZSTD_c_ldmMinMatch,
    LdmBucketSizeLog = VM.ZSTD_c_ldmBucketSizeLog, LdmHashRateLog = VM.ZSTD_c_ldmHashRateLog,
    ContentSizeFlag = VM.ZSTD_c_contentSizeFlag, ChecksumFlag = VM.ZSTD_c_checksumFlag, DictionaryIdFlag = VM.ZSTD_c_dictIDFlag,
    Workers = VM.ZSTD_c_nbWorkers
}
public enum ZstdDecompressionParameter { WindowLogMax = VM.ZSTD_d_windowLogMax }
/// <summary>For compression, interpret Completed only with Flush or End; Continue
/// can return zero without finishing a frame. For decompression, Completed marks
/// a frame boundary. Remaining is an upstream hint, not a decoded length.</summary>
public readonly record struct ZstdStreamResult(int Consumed, int Written, ulong Remaining)
{
    public bool Completed => Remaining == 0;
}

/// <summary>Owns one compression context. Calls and Dispose serialize per context.
/// Each input/output span is pinned only until its synchronous call returns.</summary>
public sealed unsafe class ZstdCompressor : IDisposable
{
    private readonly object gate = new();
    private readonly ContextHandle handle;
    private bool faulted;
    public ZstdCompressor(int level = 3, ZstdDictionary? dictionary = null)
    {
        handle = new ContextHandle(compression: true);
        try
        {
            SetParameter(ZstdCompressionParameter.CompressionLevel, level);
            SetParameter(ZstdCompressionParameter.ChecksumFlag, 1);
            if (dictionary != null) LoadDictionary(dictionary);
        }
        catch { handle.Dispose(); throw; }
    }
    private ulong Check(ulong value)
    {
        try { return NativeResult.Check(value); } catch (ZstdException) { faulted = true; throw; }
    }
    private void Ready() { if (faulted) throw new InvalidOperationException("Reset the compression context after a zstd error."); }
    public void Reset()
    {
        lock (gate) { using var binding = handle.Enter(); NativeResult.Check(VM.dz_c_reset(handle.Pointer)); faulted = false; }
    }
    public void SetParameter(ZstdCompressionParameter parameter, int value)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            if (!Enum.IsDefined(parameter)) throw new ArgumentOutOfRangeException(nameof(parameter), "Only the declared compression parameters preserve per-call buffer ownership.");
            Check(VM.dz_c_parameter(handle.Pointer, (int)parameter, value));
        }
    }
    /// <summary>The source size is a promise for the next frame; a mismatch is an upstream error.</summary>
    public void SetPledgedSourceSize(ulong size)
    {
        lock (gate) { using var binding = handle.Enter(); Ready(); Check(VM.dz_c_pledge(handle.Pointer, size)); }
    }
    public void LoadDictionary(ZstdDictionary? dictionary)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            fixed (byte* data = dictionary?.Bytes) Check(VM.dz_c_dictionary(handle.Pointer, data, (ulong)(dictionary?.Length ?? 0)));
        }
    }
    public int Compress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready(); NativeResult.Buffers(source, destination);
            fixed (byte* input = source) fixed (byte* output = destination)
                return checked((int)Check(VM.dz_compress(handle.Pointer, output, (ulong)destination.Length, input, (ulong)source.Length)));
        }
    }
    public ZstdStreamResult Process(ReadOnlySpan<byte> source, Span<byte> destination, ZstdEndDirective directive = ZstdEndDirective.Continue)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            if (directive is < ZstdEndDirective.Continue or > ZstdEndDirective.End) throw new ArgumentOutOfRangeException(nameof(directive));
            if (destination.IsEmpty) throw new ArgumentException("An output buffer is required.", nameof(destination));
            NativeResult.Buffers(source, destination);
            ulong consumed = 0, written = 0;
            fixed (byte* input = source) fixed (byte* output = destination)
            {
                ulong remaining = Check(VM.dz_c_stream(handle.Pointer, output, (ulong)destination.Length, input, (ulong)source.Length, (int)directive, &consumed, &written));
                return new(checked((int)consumed), checked((int)written), remaining);
            }
        }
    }
    /// <summary>Writes one complete frame without closing either stream.</summary>
    public void Compress(Stream source, Stream destination, ulong? sourceSize = null, int bufferSize = 131072)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            Reset(); if (sourceSize.HasValue) SetPledgedSourceSize(sourceSize.Value);
            byte[] input = ArrayPool<byte>.Shared.Rent(bufferSize), output = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int count;
                while ((count = source.Read(input, 0, bufferSize)) != 0)
                {
                    int offset = 0;
                    while (offset < count)
                    {
                        var step = Process(input.AsSpan(offset, count - offset), output.AsSpan(0, bufferSize)); offset += step.Consumed;
                        destination.Write(output, 0, step.Written);
                        if (step.Consumed == 0 && step.Written == 0) throw new InvalidOperationException("zstd compression made no progress.");
                    }
                }
                ZstdStreamResult end;
                do { end = Process([], output.AsSpan(0, bufferSize), ZstdEndDirective.End); destination.Write(output, 0, end.Written); } while (!end.Completed);
            }
            finally { ArrayPool<byte>.Shared.Return(input); ArrayPool<byte>.Shared.Return(output); }
        }
    }
    public void Dispose() { lock (gate) { if (handle.IsClosed) return; using var binding = handle.Enter(); handle.Dispose(); } }
}

/// <summary>Owns one decompression context; accepts concatenated and skippable frames.</summary>
public sealed unsafe class ZstdDecompressor : IDisposable
{
    private readonly object gate = new();
    private readonly ContextHandle handle;
    private bool faulted;
    public ZstdDecompressor(ZstdDictionary? dictionary = null)
    {
        handle = new ContextHandle(compression: false);
        try { if (dictionary != null) LoadDictionary(dictionary); } catch { handle.Dispose(); throw; }
    }
    private ulong Check(ulong value)
    {
        try { return NativeResult.Check(value); } catch (ZstdException) { faulted = true; throw; }
    }
    private void Ready() { if (faulted) throw new InvalidOperationException("Reset the decompression context after a zstd error."); }
    public void Reset()
    {
        lock (gate) { using var binding = handle.Enter(); NativeResult.Check(VM.dz_d_reset(handle.Pointer)); faulted = false; }
    }
    public void SetParameter(ZstdDecompressionParameter parameter, int value)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            if (!Enum.IsDefined(parameter)) throw new ArgumentOutOfRangeException(nameof(parameter), "Only the declared decompression parameters preserve per-call buffer ownership.");
            Check(VM.dz_d_parameter(handle.Pointer, (int)parameter, value));
        }
    }
    public void LoadDictionary(ZstdDictionary? dictionary)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            fixed (byte* data = dictionary?.Bytes) Check(VM.dz_d_dictionary(handle.Pointer, data, (ulong)(dictionary?.Length ?? 0)));
        }
    }
    public int Decompress(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready(); NativeResult.Buffers(source, destination);
            fixed (byte* input = source) fixed (byte* output = destination)
                return checked((int)Check(VM.dz_decompress(handle.Pointer, output, (ulong)destination.Length, input, (ulong)source.Length)));
        }
    }
    public ZstdStreamResult Process(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        lock (gate)
        {
            using var binding = handle.Enter(); Ready();
            if (destination.IsEmpty) throw new ArgumentException("An output buffer is required.", nameof(destination));
            NativeResult.Buffers(source, destination);
            ulong consumed = 0, written = 0;
            fixed (byte* input = source) fixed (byte* output = destination)
            {
                ulong remaining = Check(VM.dz_d_stream(handle.Pointer, output, (ulong)destination.Length, input, (ulong)source.Length, &consumed, &written));
                return new(checked((int)consumed), checked((int)written), remaining);
            }
        }
    }
    /// <summary>Decodes complete frames to the output bound. Truncation and empty input fail.
    /// The source and destination remain open.</summary>
    public void Decompress(Stream source, Stream destination, long maxOutputBytes = 1024L * 1024 * 1024, int bufferSize = 131072)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(maxOutputBytes); ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        lock (gate)
        {
            using var binding = handle.Enter(); Ready(); Reset();
            byte[] input = ArrayPool<byte>.Shared.Rent(bufferSize), output = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                int count = 0, offset = 0; long total = 0; bool complete = false, eof = false;
                while (true)
                {
                    if (offset == count && !eof) { count = source.Read(input, 0, bufferSize); offset = 0; eof = count == 0; }
                    if (eof && complete) return;
                    var step = Process(input.AsSpan(offset, count - offset), output.AsSpan(0, bufferSize)); offset += step.Consumed; complete = step.Completed;
                    if (step.Written > maxOutputBytes - total) throw new InvalidDataException("Decoded content exceeds the configured output limit.");
                    destination.Write(output, 0, step.Written); total += step.Written;
                    if (step.Consumed == 0 && step.Written == 0)
                    {
                        if (eof) throw new InvalidDataException("Truncated zstd frame.");
                        // Completing a zero-output/skippable frame is progress if input was consumed.
                        throw new InvalidDataException("zstd decompression made no progress.");
                    }
                }
            }
            finally { ArrayPool<byte>.Shared.Return(input); ArrayPool<byte>.Shared.Return(output); }
        }
    }
    public void Dispose() { lock (gate) { if (handle.IsClosed) return; using var binding = handle.Enter(); handle.Dispose(); } }
}

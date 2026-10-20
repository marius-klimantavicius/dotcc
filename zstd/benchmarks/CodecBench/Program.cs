using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Managed.Zstd;

internal static class Program
{
    private sealed record Sample(int Iterations, double NanosecondsPerOperation, double AllocatedBytesPerOperation, int Gen0, int Gen1, int Gen2);
    private sealed record Result(string Name, string Kind, string Corpus, int SourceBytes, int FrameBytes, string SourceSha256, string FrameSha256, int Level, int WarmupCalls, List<Sample> Samples);
    private sealed class Workload(string name, string kind, string corpus, byte[] source, int level, Func<int> execute, Func<byte[]> frame, Action validate, IDisposable? owner = null) : IDisposable
    {
        internal readonly string Name = name, Kind = kind, Corpus = corpus;
        internal readonly byte[] Source = source;
        internal readonly int Level = level;
        internal readonly Func<int> Execute = execute;
        internal readonly Func<byte[]> Frame = frame;
        internal readonly Action Validate = validate;
        public void Dispose() => owner?.Dispose();
    }
    private static int sink;
    private static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static uint Next(ref uint state) { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
    private static byte[] Logs(int length, uint seed)
    {
        var bytes = new byte[length]; int offset = 0;
        while (offset < bytes.Length)
        {
            uint value = Next(ref seed);
            byte[] record = Encoding.UTF8.GetBytes($"2026-10-{1 + value % 28:00}T12:{value % 60:00}:00Z service=store request={value:x8} tenant={value % 37} method=GET path=/objects/{value % 113} status={(value % 9 == 0 ? 404 : 200)} elapsed_us={value % 10000} region=eu\n");
            int count = Math.Min(record.Length, bytes.Length - offset);
            record.AsSpan(0, count).CopyTo(bytes.AsSpan(offset)); offset += count;
        }
        return bytes;
    }
    private static byte[] Binary(int length, bool mixed)
    {
        var bytes = new byte[length]; uint state = 0x6a09e667;
        for (int i = 0; i < bytes.Length; i++) bytes[i] = mixed && (i / 1024) % 4 != 0 ? (byte)(i % 251) : (byte)Next(ref state);
        return bytes;
    }
    private static void Equal(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> actual)
    {
        if (!expected.SequenceEqual(actual)) throw new InvalidDataException("Benchmark roundtrip or deterministic frame validation failed.");
    }
    private static Workload Compression(string corpus, byte[] source, int level, ZstdDictionary? dictionary = null)
    {
        var compressor = new ZstdCompressor(level, dictionary);
        var destination = new byte[ZstdCodec.CompressBound(source.Length)]; int written = 0;
        var expected = ZstdCodec.Compress(source, level, dictionary);
        int Execute() { written = compressor.Compress(source, destination); return written ^ destination[0] ^ destination[written - 1]; }
        void Validate() { Equal(expected, destination.AsSpan(0, written)); Equal(source, ZstdCodec.Decompress(expected, dictionary, source.Length)); }
        return new($"compress/{corpus}/{source.Length}/level{level}", "context-reuse-compress", corpus, source, level, Execute, () => expected, Validate, compressor);
    }
    private static Workload Decompression(string corpus, byte[] source, int level, ZstdDictionary? dictionary = null)
    {
        var decoder = new ZstdDecompressor(dictionary);
        var frame = ZstdCodec.Compress(source, level, dictionary); var output = new byte[source.Length]; int written = 0;
        int Execute() { written = decoder.Decompress(frame, output); return written ^ output[0] ^ output[written - 1]; }
        void Validate() { if (written != source.Length) throw new InvalidDataException("Decoded length differs."); Equal(source, output); }
        return new($"decompress/{corpus}/{source.Length}/level{level}", "context-reuse-decompress", corpus, source, level, Execute, () => frame, Validate, decoder);
    }
    private static Workload Streaming(byte[] source, bool compress)
    {
        const int chunk = 32768;
        var frame = ZstdCodec.Compress(source); var output = new byte[ZstdCodec.CompressBound(source.Length)]; var decoded = new byte[source.Length];
        var encoder = compress ? new ZstdCompressor() : null; var decoder = compress ? null : new ZstdDecompressor(); int written = 0;
        int Execute()
        {
            int inputOffset = 0, outputOffset = 0;
            if (compress)
            {
                encoder!.Reset(); encoder.SetPledgedSourceSize((ulong)source.Length);
                while (inputOffset < source.Length)
                {
                    int end = Math.Min(inputOffset + chunk, source.Length);
                    while (inputOffset < end)
                    {
                        var step = encoder.Process(source.AsSpan(inputOffset, end - inputOffset), output.AsSpan(outputOffset, Math.Min(chunk, output.Length - outputOffset)));
                        inputOffset += step.Consumed; outputOffset += step.Written;
                        if (step.Consumed == 0 && step.Written == 0) throw new InvalidDataException("Compression made no progress.");
                    }
                }
                while (true)
                {
                    var step = encoder.Process([], output.AsSpan(outputOffset, Math.Min(chunk, output.Length - outputOffset)), ZstdEndDirective.End);
                    outputOffset += step.Written; if (step.Completed) break;
                    if (step.Written == 0) throw new InvalidDataException("End made no progress.");
                }
            }
            else
            {
                decoder!.Reset(); bool complete = false;
                while (!complete)
                {
                    var step = decoder.Process(frame.AsSpan(inputOffset, Math.Min(chunk, frame.Length - inputOffset)), decoded.AsSpan(outputOffset, Math.Min(chunk, decoded.Length - outputOffset)));
                    inputOffset += step.Consumed; outputOffset += step.Written; complete = step.Completed && inputOffset == frame.Length;
                    if (step.Consumed == 0 && step.Written == 0 && !complete) throw new InvalidDataException("Decompression made no progress.");
                }
            }
            written = outputOffset;
            return written ^ (compress ? output[0] : decoded[0]);
        }
        void Validate()
        {
            if (compress) Equal(source, ZstdCodec.Decompress(output.AsSpan(0, written), maxOutputBytes: source.Length));
            else { if (written != source.Length) throw new InvalidDataException("Streaming decoded length differs."); Equal(source, decoded); }
        }
        return new($"stream-{(compress ? "compress" : "decompress")}/text/{source.Length}/level3", compress ? "stream-context-reuse-compress" : "stream-context-reuse-decompress", "text", source, 3, Execute,
            () => compress ? output.AsSpan(0, written).ToArray() : frame, Validate, (IDisposable?)encoder ?? decoder);
    }
    private static Workload Convenience(byte[] source)
    {
        byte[] frame = [], output = [];
        int Execute() { frame = ZstdCodec.Compress(source); output = ZstdCodec.Decompress(frame, maxOutputBytes: source.Length); return frame.Length ^ output.Length ^ output[0]; }
        return new($"allocating-roundtrip/text/{source.Length}/level3", "allocating-api-roundtrip", "text", source, 3, Execute, () => frame, () => Equal(source, output));
    }
    private static IEnumerable<Workload> Workloads()
    {
        foreach (int length in new[] { 4096, 131072, 1048576 })
        {
            byte[] source = Logs(length, 0x12345678);
            foreach (int level in new[] { 1, 3, 9 }) { yield return Compression("text", source, level); yield return Decompression("text", source, level); }
        }
        foreach (bool mixed in new[] { true, false })
        {
            byte[] source = Binary(131072, mixed); string corpus = mixed ? "mixed-binary" : "incompressible";
            yield return Compression(corpus, source, 3); yield return Decompression(corpus, source, 3);
        }
        byte[] medium = Logs(131072, 0x12345678), large = Logs(1048576, 0x12345678);
        yield return Compression("text", medium, 19); yield return Decompression("text", medium, 19);
        yield return Streaming(large, true); yield return Streaming(large, false);
        var samples = Enumerable.Range(0, 128).Select(i => (ReadOnlyMemory<byte>)Logs(4096, (uint)i + 1)).ToArray();
        var dictionary = ZstdDictionary.Train(samples, 8192); byte[] small = Logs(4096, 0x9e3779b9);
        yield return Compression("dictionary-log", small, 3, dictionary); yield return Decompression("dictionary-log", small, 3, dictionary);
        yield return Convenience(medium);
    }
    private static double Run(Workload work, int iterations)
    {
        long start = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; i++) sink ^= work.Execute();
        return Stopwatch.GetElapsedTime(start).TotalMilliseconds;
    }
    private static Result Measure(Workload work, int warmupMs, int sampleMs, int samples)
    {
        sink ^= work.Execute(); work.Validate(); byte[] expectedFrame = work.Frame();
        int warmups = 0; long start = Stopwatch.GetTimestamp();
        do { sink ^= work.Execute(); warmups++; } while (warmups < 3 || Stopwatch.GetElapsedTime(start).TotalMilliseconds < warmupMs);
        work.Validate(); Equal(expectedFrame, work.Frame());
        int iterations = 1;
        while (true)
        {
            double elapsed = Run(work, iterations);
            if (elapsed >= sampleMs || iterations >= 1048576) break;
            int factor = (int)Math.Clamp(Math.Ceiling(sampleMs / Math.Max(0.001, elapsed)), 2, 16);
            iterations = (int)Math.Min(1048576L, (long)iterations * factor);
        }
        var measurements = new List<Sample>();
        for (int sample = 0; sample < samples; sample++)
        {
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
            long allocated = GC.GetAllocatedBytesForCurrentThread();
            double milliseconds = Run(work, iterations);
            allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
            measurements.Add(new(iterations, milliseconds * 1000000 / iterations, (double)allocated / iterations,
                GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2));
            work.Validate(); Equal(expectedFrame, work.Frame());
        }
        return new(work.Name, work.Kind, work.Corpus, work.Source.Length, expectedFrame.Length, Hash(work.Source), Hash(expectedFrame), work.Level, warmups, measurements);
    }
    private static void Report(string path, string status, List<Result> results, int warmup, int duration, int samples, string? error)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path); using var json = new Utf8JsonWriter(stream, new() { Indented = true });
        json.WriteStartObject(); json.WriteString("status", status); json.WriteString("error", error);
        json.WriteString("codec_version", ZstdCodec.Version); json.WriteString("framework", RuntimeInformation.FrameworkDescription); json.WriteString("os", RuntimeInformation.OSDescription);
        json.WriteString("architecture", RuntimeInformation.ProcessArchitecture.ToString()); json.WriteNumber("processor_count", Environment.ProcessorCount);
        json.WriteNumber("stopwatch_frequency", Stopwatch.Frequency); json.WriteBoolean("server_gc", System.Runtime.GCSettings.IsServerGC);
        json.WriteNumber("warmup_ms", warmup); json.WriteNumber("sample_ms", duration); json.WriteNumber("sample_count", samples);
        json.WriteString("tiered_compilation", Environment.GetEnvironmentVariable("DOTNET_TieredCompilation")); json.WriteNumber("sink", sink);
        json.WriteStartArray("cases");
        foreach (var result in results)
        {
            json.WriteStartObject(); json.WriteString("name", result.Name); json.WriteString("kind", result.Kind); json.WriteString("corpus", result.Corpus);
            json.WriteNumber("source_bytes", result.SourceBytes); json.WriteNumber("frame_bytes", result.FrameBytes); json.WriteNumber("level", result.Level);
            json.WriteString("source_sha256", result.SourceSha256); json.WriteString("frame_sha256", result.FrameSha256); json.WriteNumber("warmup_calls", result.WarmupCalls);
            json.WriteStartArray("samples");
            foreach (var sample in result.Samples)
            {
                json.WriteStartObject(); json.WriteNumber("iterations", sample.Iterations); json.WriteNumber("ns_per_op", sample.NanosecondsPerOperation); json.WriteNumber("allocated_bytes_per_op", sample.AllocatedBytesPerOperation);
                json.WriteNumber("gen0", sample.Gen0); json.WriteNumber("gen1", sample.Gen1); json.WriteNumber("gen2", sample.Gen2); json.WriteEndObject();
            }
            json.WriteEndArray(); json.WriteEndObject();
        }
        json.WriteEndArray(); json.WriteEndObject();
    }
    public static int Main(string[] args)
    {
        string output = args[0]; int warmup = int.Parse(args[1]), duration = int.Parse(args[2]), samples = int.Parse(args[3]);
        var results = new List<Result>();
        try
        {
            if (warmup < 1 || duration < 1 || samples < 3) throw new ArgumentOutOfRangeException(nameof(args));
            foreach (var work in Workloads())
            {
                using (work) results.Add(Measure(work, warmup, duration, samples));
                Report(output, "running", results, warmup, duration, samples, null);
                Console.WriteLine($"BENCH PASS {work.Name}");
            }
            if (results.Count != 29) throw new InvalidDataException("Workload inventory differs.");
            Report(output, "passed", results, warmup, duration, samples, null); return 0;
        }
        catch (Exception error) { Report(output, "failed", results, warmup, duration, samples, error.ToString()); Console.Error.WriteLine(error); return 1; }
    }
}

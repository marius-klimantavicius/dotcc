using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using Managed.Zstd;
using VM = Managed.Zstd.Native.Zstd;

internal static unsafe class Program
{
    private sealed record CaseResult(string Name, string Status, string? Detail = null);
    private static readonly List<CaseResult> Cases = [];
    private static int Main(string[] args)
    {
        if (args.Length < 2) { Console.Error.WriteLine("ManagedTests SUITE WORK [ORACLE]"); return 2; }
        Directory.CreateDirectory(args[1]);
        try
        {
            switch (args[0])
            {
                case "abi": Abi(args[1]); break;
                case "behavior": Behavior(); break;
                case "lifecycle": Lifecycle(); break;
                case "fixtures": Fixtures(args[1]); break;
                case "interop": Interop(args[1]); break;
                case "failure-control": Case("deliberate-assertion-failure", () => Require(false, "Deliberate assertion failure.")); break;
                default: throw new ArgumentException("Unknown suite: " + args[0]);
            }
        }
        catch (Exception error) { Cases.Add(new("harness", "failed", error.ToString())); }
        var failed = Cases.Count(item => item.Status == "failed");
        using var data = new MemoryStream();
        using (var writer = new Utf8JsonWriter(data, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject(); writer.WriteString("suite", args[0]); writer.WriteNumber("passed", Cases.Count - failed); writer.WriteNumber("failed", failed); writer.WriteNumber("skipped", 0); writer.WriteStartArray("cases");
            foreach (var item in Cases) { writer.WriteStartObject(); writer.WriteString("name", item.Name); writer.WriteString("status", item.Status); if (item.Detail != null) writer.WriteString("detail", item.Detail); writer.WriteEndObject(); }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        string json = Encoding.UTF8.GetString(data.ToArray());
        File.WriteAllText(Path.Combine(args[1], args[0] + "-report.json"), json);
        Console.WriteLine(json);
        return failed == 0 ? 0 : 1;
    }

    private static void Case(string name, Action action)
    {
        try { action(); Cases.Add(new(name, "passed")); }
        catch (Exception error) { Cases.Add(new(name, "failed", error.ToString())); }
    }
    private static void Require(bool value, string detail) { if (!value) throw new InvalidOperationException(detail); }
    private static void Equal(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> actual) => Require(expected.SequenceEqual(actual), "Payload bytes differ.");
    private static T Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T error) { return error; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
    private static void Error(Action action, VM.ZSTD_ErrorCode expected)
    {
        var error = Throws<ZstdException>(action);
        Require(error.ErrorCode == (int)expected && error.Message.Length > 0, $"Expected upstream {expected}, got code {error.ErrorCode}: {error.Message}");
    }
    internal static byte[] Payload(int length, uint seed = 17)
    {
        var data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            seed = unchecked(seed * 1664525 + 1013904223);
            data[i] = i % 113 < 91 ? (byte)(i % 23 + 65) : (byte)(seed >> 24);
        }
        return data;
    }
    internal static ReadOnlyMemory<byte>[] Samples() => Enumerable.Range(0, 256).Select(i =>
        (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(string.Join('\n', Enumerable.Range(0, 24).Select(j =>
            $"event={i:D5};record={j:D3};tenant={i % 17};name=Žąsis-東京;status={(i + j) % 7};message=archived telemetry sample payload {i * 31 + j}")))).ToArray();

    private static void Abi(string work)
    {
        Case("emitted-layout-matches-native", () =>
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(work, "native-abi.json")));
            var expected = document.RootElement.EnumerateArray().Select(value => value.GetUInt64()).ToArray();
            var actual = Enumerable.Range(0, 12).Select(i => VM.dz_abi(i)).ToArray();
            Require(expected.SequenceEqual(actual), "Emitted/native ABI layout differs: " + string.Join(',', actual));
        });
        Case("emitted-buffer-storage-roundtrip", () =>
        {
            var input = Payload(137); var output = new byte[137]; ulong consumed = 0, written = 0;
            fixed (byte* source = input) fixed (byte* destination = output)
                Require(VM.dz_abi_buffers(source, 137, destination, 137, &consumed, &written) == 1, "Buffer struct roundtrip failed.");
            Require(consumed == 137 && written == 137, "Stream positions differ."); Equal(input, output);
        });
        Case("version-and-error-contract", () =>
        {
            uint number = ZstdCodec.VersionNumber;
            Require(ZstdCodec.Version == $"{number / 10000}.{number / 100 % 100}.{number % 100}", "Version number/text differ.");
            Require(!string.IsNullOrWhiteSpace(ZstdCodec.Version), "Missing version text.");
            Error(() => ZstdCodec.Decompress(new byte[] { 0, 1, 2, 3, 4 }), VM.ZSTD_ErrorCode.ZSTD_error_prefix_unknown);
        });
    }

    private static void Behavior()
    {
        foreach (int size in new[] { 0, 1, 255, 131071, 131072, 262147 })
            Case("one-shot-size-" + size, () => { var source = Payload(size); Equal(source, ZstdCodec.Decompress(ZstdCodec.Compress(source))); });
        foreach (int level in new[] { -5, 1, 3, 9, 19, 22 })
            Case("compression-level-" + level, () => { var source = Payload(32771); Equal(source, ZstdCodec.Decompress(ZstdCodec.Compress(source, level))); });
        foreach (ZstdStrategy strategy in Enum.GetValues<ZstdStrategy>())
            Case("compression-strategy-" + strategy, () =>
            {
                using var compressor = new ZstdCompressor(); compressor.SetParameter(ZstdCompressionParameter.Strategy, (int)strategy);
                var source = Payload(65539); var frame = new byte[ZstdCodec.CompressBound(source.Length)];
                int count = compressor.Compress(source, frame); Equal(source, ZstdCodec.Decompress(frame.AsSpan(0, count)));
            });
        Case("streaming-tiny-buffers-and-flush", () =>
        {
            var source = Payload(262147); var frame = StreamCompress(source, 73, 127, flush: true);
            Equal(source, StreamDecompress(frame, 31, 47));
            Require(ZstdCodec.GetFrameContentSize(frame) == null, "Unpledged streaming frame unexpectedly declares size.");
        });
        Case("long-distance-matching", () =>
        {
            var source = Payload(524303); using var compressor = new ZstdCompressor(level: 9);
            compressor.SetParameter(ZstdCompressionParameter.EnableLongDistanceMatching, 1);
            compressor.SetParameter(ZstdCompressionParameter.WindowLog, 20);
            var frame = new byte[ZstdCodec.CompressBound(source.Length)]; int size = compressor.Compress(source, frame);
            Equal(source, ZstdCodec.Decompress(frame.AsSpan(0, size)));
        });
        Case("stream-copy-and-pledged-size", () =>
        {
            var source = Payload(524303); using var input = new MemoryStream(source); using var frame = new MemoryStream();
            using (var compressor = new ZstdCompressor()) compressor.Compress(input, frame, sourceSize: (ulong)source.Length, bufferSize: 257);
            Require(ZstdCodec.GetFrameContentSize(frame.ToArray()) == (ulong)source.Length, "Pledged source size missing.");
            frame.Position = 0; using var output = new MemoryStream(); using (var decoder = new ZstdDecompressor()) decoder.Decompress(frame, output, bufferSize: 191);
            Equal(source, output.ToArray());
        });
        Case("concatenated-and-skippable-frames", () =>
        {
            byte[] a = Payload(313), b = Payload(431, 71); var skip = new byte[11];
            BinaryPrimitives.WriteUInt32LittleEndian(skip, 0x184D2A50); BinaryPrimitives.WriteUInt32LittleEndian(skip.AsSpan(4), 3);
            var frames = ZstdCodec.Compress(a).Concat(skip).Concat(ZstdCodec.Compress(b)).ToArray();
            Equal(a.Concat(b).ToArray(), ZstdCodec.Decompress(frames));
        });
        Case("trained-dictionary-and-id", () =>
        {
            var dictionary = ZstdDictionary.Train(Samples(), 8192); var source = Samples()[39].ToArray();
            var frame = ZstdCodec.Compress(source, dictionary: dictionary);
            Require(dictionary.Id != 0 && ZstdCodec.GetFrameDictionaryId(frame) == dictionary.Id, "Dictionary ID missing.");
            Equal(source, ZstdCodec.Decompress(frame, dictionary));
            Error(() => ZstdCodec.Decompress(frame), VM.ZSTD_ErrorCode.ZSTD_error_dictionary_wrong);
            var other = dictionary.ToArray(); other[4] ^= 1;
            Error(() => ZstdCodec.Decompress(frame, new ZstdDictionary(other)), VM.ZSTD_ErrorCode.ZSTD_error_dictionary_wrong);
        });
        Case("oversized-advertised-content-size", () =>
        {
            byte[] frame = [0x28, 0xb5, 0x2f, 0xfd, 0xe0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0];
            BinaryPrimitives.WriteUInt64LittleEndian(frame.AsSpan(5), 1UL << 40);
            Require(ZstdCodec.GetFrameContentSize(frame) == 1UL << 40, "Oversized test frame header differs.");
            Throws<InvalidDataException>(() => ZstdCodec.Decompress(frame, maxOutputBytes: 1024));
        });
        Case("raw-dictionary-copied-ownership", () =>
        {
            var bytes = Payload(4096); var dictionary = new ZstdDictionary(bytes); bytes.AsSpan().Clear();
            var source = Payload(3001); var frame = ZstdCodec.Compress(source, dictionary: dictionary);
            Equal(source, ZstdCodec.Decompress(frame, dictionary)); Require(dictionary.Id == 0, "Raw dictionary unexpectedly has an ID.");
        });
        Case("malformed-truncated-corrupt-and-output-limit", () =>
        {
            var source = Payload(65539); var frame = ZstdCodec.Compress(source);
            Error(() => ZstdCodec.Decompress(new byte[] { 1, 2, 3, 4, 5, 6 }), VM.ZSTD_ErrorCode.ZSTD_error_prefix_unknown);
            Throws<InvalidDataException>(() => ZstdCodec.Decompress(frame.AsSpan(0, frame.Length - 1)));
            var corrupt = frame.ToArray(); corrupt[^1] ^= 0x80; Error(() => ZstdCodec.Decompress(corrupt), VM.ZSTD_ErrorCode.ZSTD_error_checksum_wrong);
            Throws<InvalidDataException>(() => ZstdCodec.Decompress(frame, maxOutputBytes: source.Length - 1));
        });
        Case("insufficient-destination-and-training-rejection", () =>
        {
            using var compressor = new ZstdCompressor(); var source = Payload(4097); var tooSmall = new byte[1];
            Error(() => compressor.Compress(source, tooSmall), VM.ZSTD_ErrorCode.ZSTD_error_dstSize_tooSmall); compressor.Reset();
            var frame = new byte[ZstdCodec.CompressBound(source.Length)]; int count = compressor.Compress(source, frame);
            Equal(source, ZstdCodec.Decompress(frame.AsSpan(0, count)));
            using var decoder = new ZstdDecompressor();
            Error(() => decoder.Decompress(frame.AsSpan(0, count), new byte[1]), VM.ZSTD_ErrorCode.ZSTD_error_dstSize_tooSmall);
            Throws<ZstdException>(() => ZstdDictionary.Train(new ReadOnlyMemory<byte>[] { new byte[3] }, 256));
        });
    }

    private static byte[] StreamCompress(byte[] input, int chunk, int capacity, bool flush = false)
    {
        using var compressor = new ZstdCompressor(); using var frame = new MemoryStream(); var output = new byte[capacity];
        int position = 0;
        while (position < input.Length)
        {
            int end = Math.Min(input.Length, position + chunk);
            while (position < end)
            {
                var step = compressor.Process(input.AsSpan(position, end - position), output);
                position += step.Consumed; frame.Write(output, 0, step.Written);
                Require(step.Consumed != 0 || step.Written != 0, "Compression stalled.");
            }
            if (flush)
            {
                ZstdStreamResult step;
                do { step = compressor.Process([], output, ZstdEndDirective.Flush); frame.Write(output, 0, step.Written); } while (!step.Completed);
            }
        }
        ZstdStreamResult last;
        do { last = compressor.Process([], output, ZstdEndDirective.End); frame.Write(output, 0, last.Written); } while (!last.Completed);
        return frame.ToArray();
    }
    private static byte[] StreamDecompress(byte[] frame, int chunk, int capacity)
    {
        using var decoder = new ZstdDecompressor(); using var output = new MemoryStream(); var buffer = new byte[capacity];
        int position = 0; bool complete = false;
        while (position < frame.Length || !complete)
        {
            var step = decoder.Process(frame.AsSpan(position, Math.Min(chunk, frame.Length - position)), buffer);
            position += step.Consumed; output.Write(buffer, 0, step.Written); complete = step.Completed;
            Require(step.Consumed != 0 || step.Written != 0 || complete, "Decompression stalled.");
        }
        return output.ToArray();
    }

    private static void Lifecycle()
    {
        Case("repeated-create-reset-dispose", () =>
        {
            var source = Payload(4103);
            for (int i = 0; i < 32; i++)
            {
                var c = new ZstdCompressor(); var d = new ZstdDecompressor();
                for (int j = 0; j < 3; j++) { c.Reset(); d.Reset(); var frame = new byte[ZstdCodec.CompressBound(source.Length)]; int size = c.Compress(source, frame); var dest = new byte[source.Length]; Equal(source, dest.AsSpan(0, d.Decompress(frame.AsSpan(0, size), dest))); }
                c.Dispose(); c.Dispose(); d.Dispose(); d.Dispose();
                Throws<ObjectDisposedException>(() => c.Reset()); Throws<ObjectDisposedException>(() => d.Reset());
                Throws<ObjectDisposedException>(() => c.Process([], new byte[1])); Throws<ObjectDisposedException>(() => d.Process([], new byte[1]));
            }
        });
        Case("error-reset-recovery", () =>
        {
            using var d = new ZstdDecompressor(); Throws<ZstdException>(() => d.Process(new byte[] { 0, 0, 0, 0 }, new byte[100]));
            Throws<InvalidOperationException>(() => d.Process([], new byte[1])); d.Reset();
            var data = Payload(100); var output = new byte[100]; Equal(data, output.AsSpan(0, d.Decompress(ZstdCodec.Compress(data), output)));
        });
        Case("independent-contexts-and-gc-pins", () =>
        {
            Parallel.For(0, 8, i =>
            {
                var source = Payload(65543, (uint)i + 1);
                for (int j = 0; j < 4; j++) { GC.Collect(); Equal(source, ZstdCodec.Decompress(ZstdCodec.Compress(source))); }
            });
        });
        Case("collection-during-translated-calls", () =>
        {
            using var stop = new CancellationTokenSource(); using var started = new ManualResetEventSlim();
            int collections = 0;
            var collector = Task.Run(() => { started.Set(); for (int i = 0; i < 32 && !stop.IsCancellationRequested; i++) { GC.Collect(); Interlocked.Increment(ref collections); Thread.Sleep(1); } });
            started.Wait();
            try
            {
                var source = Payload(524303);
                for (int i = 0; i < 8; i++) Equal(source, ZstdCodec.Decompress(ZstdCodec.Compress(source, level: 9)));
            }
            finally { stop.Cancel(); collector.GetAwaiter().GetResult(); }
            Require(collections > 0, "Concurrent collection was not exercised.");
        });
        Case("ambient-owner-restored-and-disposal-rebound", () =>
        {
            var compressor = new ZstdCompressor();
            using var other = new VM.Libc.RuntimeContext();
            using var nested = new VM.Libc.RuntimeContext();
            using (other.Enter())
            {
                using (nested.Enter())
                {
                    using var createdUnderNested = new ZstdCompressor();
                    Require(ReferenceEquals(VM.Libc.RuntimeContext.Current, nested), "Nested runtime binding changed.");
                }
                using var decoder = new ZstdDecompressor();
                var source = Payload(101); var frame = new byte[ZstdCodec.CompressBound(source.Length)];
                int count = compressor.Compress(source, frame); var output = new byte[source.Length];
                Equal(source, output.AsSpan(0, decoder.Decompress(frame.AsSpan(0, count), output)));
                compressor.Dispose();
                Require(ReferenceEquals(VM.Libc.RuntimeContext.Current, other), "API changed caller runtime binding.");
            }
            Require(VM.Libc.RuntimeContext.Current == null, "Ambient owner was not restored.");
        });
        Case("safehandle-finalization", () =>
        {
            int baseline = ContextHandle.ActiveHandles;
            var references = Enumerable.Range(0, 12).Select(_ => Abandon()).ToArray();
            for (int i = 0; i < 3; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }
            Require(references.All(r => !r.IsAlive), "Abandoned context remains rooted.");
            Require(ContextHandle.ActiveHandles == baseline, "SafeHandle finalization did not release all contexts.");
            Require(ContextHandle.ReleaseFailures == 0, "SafeHandle release encountered a failure.");
        });
        Case("parameter-rejection-and-buffer-validation", () =>
        {
            using var compressor = new ZstdCompressor(); Error(() => compressor.SetParameter(ZstdCompressionParameter.WindowLog, 1000), VM.ZSTD_ErrorCode.ZSTD_error_parameter_outOfBound); compressor.Reset();
            Error(() => compressor.SetParameter(ZstdCompressionParameter.Workers, 2), VM.ZSTD_ErrorCode.ZSTD_error_parameter_unsupported); compressor.Reset();
            Throws<ArgumentOutOfRangeException>(() => compressor.SetParameter((ZstdCompressionParameter)VM.ZSTD_c_stableInBuffer, 1));
            Throws<ArgumentOutOfRangeException>(() => compressor.SetParameter((ZstdCompressionParameter)VM.ZSTD_c_stableOutBuffer, 1));
            Throws<ArgumentOutOfRangeException>(() => compressor.SetParameter((ZstdCompressionParameter)int.MaxValue, 1));
            compressor.SetParameter(ZstdCompressionParameter.ChecksumFlag, 1);
            using var decoder = new ZstdDecompressor();
            Throws<ArgumentOutOfRangeException>(() => decoder.SetParameter((ZstdDecompressionParameter)VM.ZSTD_d_stableOutBuffer, 1));
            decoder.SetParameter(ZstdDecompressionParameter.WindowLogMax, 20);
            Throws<ArgumentOutOfRangeException>(() => compressor.Process([], new byte[4], (ZstdEndDirective)17));
            Throws<ArgumentException>(() => compressor.Process(new byte[1], []));
        });
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference Abandon() { var value = new ZstdCompressor(); value.Process(Payload(91), new byte[23]); return new WeakReference(value); }

    private static void Fixtures(string work)
    {
        Case("write-managed-fixtures", () =>
        {
            var data = Payload(262147); File.WriteAllBytes(Path.Combine(work, "payload.bin"), data);
            File.WriteAllBytes(Path.Combine(work, "managed.zst"), ZstdCodec.Compress(data));
            var samples = Samples(); Span<byte> count = stackalloc byte[8];
            using (var corpus = File.Create(Path.Combine(work, "samples.bin"))) using (var sizes = File.Create(Path.Combine(work, "sizes.bin")))
                foreach (var sample in samples) { corpus.Write(sample.Span); BinaryPrimitives.WriteUInt64LittleEndian(count, (ulong)sample.Length); sizes.Write(count); }
            var dictionary = ZstdDictionary.Train(samples, 8192); File.WriteAllBytes(Path.Combine(work, "managed.dict"), dictionary.ToArray());
            File.WriteAllBytes(Path.Combine(work, "managed-dict.zst"), ZstdCodec.Compress(data, dictionary: dictionary));
            using var frame = File.Create(Path.Combine(work, "managed-stream.zst")); using var input = new MemoryStream(data); using var c = new ZstdCompressor(); c.Compress(input, frame, bufferSize: 1031);
        });
    }
    private static void Interop(string work)
    {
        var source = File.ReadAllBytes(Path.Combine(work, "payload.bin"));
        Case("native-frame-managed-decode", () => Equal(source, ZstdCodec.Decompress(File.ReadAllBytes(Path.Combine(work, "native.zst")))));
        Case("native-dictionary-managed-decode", () => Equal(source, ZstdCodec.Decompress(File.ReadAllBytes(Path.Combine(work, "native-dict.zst")), new ZstdDictionary(File.ReadAllBytes(Path.Combine(work, "native.dict"))))));
        Case("managed-frame-native-decode", () => Equal(source, File.ReadAllBytes(Path.Combine(work, "native-decoded.bin"))));
        Case("managed-dictionary-native-decode", () => Equal(source, File.ReadAllBytes(Path.Combine(work, "native-dict-decoded.bin"))));
        Case("managed-stream-native-decode", () => Equal(source, File.ReadAllBytes(Path.Combine(work, "native-stream-decoded.bin"))));
        Case("native-dictionary-managed-encode", () => File.WriteAllBytes(Path.Combine(work, "managed-native-dict.zst"), ZstdCodec.Compress(source, dictionary: new ZstdDictionary(File.ReadAllBytes(Path.Combine(work, "native.dict"))))));
    }
}

// Compiled only into the generated private diagnostic executable, alongside
// absolute Compile references to the unmodified canonical translation.
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;

namespace FactoryAllocationDiagnostic
{
    internal static class Program
    {
        private static int Main(string[] args) => Managed.GhosttyVt.__RunFactoryAllocation(args);
    }
}

namespace Managed
{
    public static partial class GhosttyVt
    {
        internal static int __RunFactoryAllocation(string[] args) => __FactoryAllocation.Run(args);

        private static unsafe partial class __FactoryAllocation
        {
            private struct Result
            {
                internal bool Completed, BytesMatch;
                internal int Bytes, Iterations;
                internal long Allocated;
                internal ulong Checksum;
                internal string Sha256;
            }

            private static Result[] _results;
            private static ulong _sink;
            private const int Warmup = 256;

            internal static int Run(string[] args)
            {
                if (args.Length != 2)
                    throw new ArgumentException("Expected <output.json> <jit|nativeaot>");
                _results = new Result[GetterCount + FactoryCount];
                string error = null;
                long positiveControl = 0, fullWarmupAllocated = 0;
                int fullWarmupCompleted = 0;
                try
                {
                    if (RuntimeFeature.IsDynamicCodeSupported != (args[1] == "jit"))
                        throw new InvalidOperationException("Unexpected runtime dynamic-code capability");
                    positiveControl = CheckAllocationCounter();
                    if (positiveControl <= 0)
                        throw new InvalidOperationException("Managed allocation positive control failed");

                    // The complete first pass also compiles/initializes diagnostic
                    // code, all generic instantiations, getters and factories.
                    ProbeAll();
                    foreach (ref Result result in _results.AsSpan())
                    {
                        if (result.Completed) ++fullWarmupCompleted;
                        fullWarmupAllocated += result.Allocated;
                        result = default;
                    }
                    ProbeAll();
                }
                catch (Exception exception) { error = exception.ToString(); }

                int completed = 0, failures = 0;
                long allocated = 0;
                foreach (ref readonly Result result in _results.AsSpan())
                {
                    if (result.Completed) ++completed;
                    if (!result.Completed || !result.BytesMatch || result.Allocated != 0) ++failures;
                    allocated += result.Allocated;
                }
                using (var stream = File.Create(args[0]))
                using (var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                {
                    json.WriteStartObject();
                    json.WriteString("form", Form);
                    json.WriteString("source_fingerprint", SourceFingerprint);
                    json.WriteString("runtime", args[1]);
                    json.WriteString("framework", RuntimeInformation.FrameworkDescription);
                    json.WriteBoolean("dynamic_code_supported", RuntimeFeature.IsDynamicCodeSupported);
                    json.WriteString("tiered_compilation", Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default");
                    json.WriteNumber("allocation_counter_positive_control_bytes", positiveControl);
                    json.WriteNumber("global_warmup_completed", fullWarmupCompleted);
                    json.WriteNumber("global_warmup_allocated_bytes", fullWarmupAllocated);
                    json.WriteNumber("warmup_iterations_per_case", Warmup);
                    json.WriteNumber("getters", GetterCount);
                    json.WriteNumber("factories", FactoryCount);
                    json.WriteNumber("completed", completed);
                    json.WriteNumber("failures", failures);
                    json.WriteNumber("allocated_bytes", allocated);
                    json.WriteString("error", error);
                    json.WriteStartArray("results");
                    for (int i = 0; i < _results.Length; ++i)
                    {
                        ref readonly Result result = ref _results[i];
                        json.WriteStartObject();
                        json.WriteNumber("index", i);
                        json.WriteBoolean("completed", result.Completed);
                        json.WriteBoolean("bytes_match", result.BytesMatch);
                        json.WriteNumber("bytes", result.Bytes);
                        json.WriteNumber("iterations", result.Iterations);
                        json.WriteNumber("allocated_bytes", result.Allocated);
                        json.WriteNumber("checksum", result.Checksum);
                        json.WriteString("sha256", result.Sha256);
                        json.WriteEndObject();
                    }
                    json.WriteEndArray();
                    json.WriteEndObject();
                }
                Console.WriteLine($"{Form}/{args[1]}: {completed} completed; {failures} failures; {allocated} allocated bytes; positive control {positiveControl}");
                return error == null && failures == 0 && fullWarmupCompleted == _results.Length ? 0 : 1;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
            private static long CheckAllocationCounter()
            {
                long before = GC.GetAllocatedBytesForCurrentThread();
                var control = new byte[257];
                control[0] = 1;
                GC.KeepAlive(control);
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
            private static void ProbeGetter<T>(int index, delegate* managed<ReadOnlySpan<T>> getter) where T : unmanaged
            {
                // Destination allocation and hash/BCL setup are outside counting.
                var destination = new T[getter().Length];
                ulong checksum = 0;
                for (int i = 0; i < Warmup; ++i)
                {
                    getter().CopyTo(destination);
                    checksum = unchecked((checksum << 1) ^ Consume(MemoryMarshal.AsBytes(destination.AsSpan())) ^ (uint)i);
                }
                const int iterations = 10000;
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < iterations; ++i)
                {
                    getter().CopyTo(destination);
                    checksum = unchecked((checksum << 1) ^ Consume(MemoryMarshal.AsBytes(destination.AsSpan())) ^ (uint)i);
                }
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Volatile.Write(ref _sink, checksum);
                var bytes = MemoryMarshal.AsBytes(destination.AsSpan());
                _results[index] = new Result {
                    Completed = true, BytesMatch = bytes.SequenceEqual(MemoryMarshal.AsBytes(getter())),
                    Bytes = bytes.Length, Iterations = iterations, Allocated = allocated, Checksum = checksum,
                    Sha256 = Convert.ToHexString(SHA256.HashData(bytes))
                };
            }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
            private static void ProbeFactory<T>(int index, delegate* managed<T> create) where T : unmanaged
            {
                int bytes = Unsafe.SizeOf<T>();
                if (bytes > 1024 * 1024 || RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                    throw new InvalidOperationException("Factory value exceeds unmanaged 1 MiB budget");
                int iterations = bytes >= 4096 ? 1000 : 10000;
                ulong checksum = 0;
                for (int i = 0; i < Warmup; ++i)
                {
                    T value = create();
                    checksum = unchecked((checksum << 1) ^ ConsumeValue(in value) ^ (uint)i);
                }
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < iterations; ++i)
                {
                    T value = create();
                    checksum = unchecked((checksum << 1) ^ ConsumeValue(in value) ^ (uint)i);
                }
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Volatile.Write(ref _sink, checksum);
                _results[index] = new Result { Completed = true, BytesMatch = true,
                    Bytes = bytes, Iterations = iterations, Allocated = allocated, Checksum = checksum };
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static ulong ConsumeValue<T>(in T value) where T : unmanaged =>
                Consume(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1)));

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static ulong Consume(ReadOnlySpan<byte> bytes)
            {
                ulong checksum = 14695981039346656037UL;
                foreach (byte item in bytes) checksum = unchecked((checksum ^ item) * 1099511628211UL);
                return checksum;
            }
        }
    }
}

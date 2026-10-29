using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace DotCC.FunctionalTests;

// Shared by focused Release execution tests and the standalone product audit.
// Inspect the emitted PE, rather than inferring RVA backing from C# spelling.
internal static class AggregateFactoryAudit
{
    internal sealed record Payload(string Getter, string Backing, int Elements, int Bytes,
        int IlBytes, int NewArrayInstructions, int MatchingRvaFields,
        long AllocatedBytesAfterWarmup, int ReadIterations, string Sha256);

    internal sealed record FactoryIl(string Method, int IlBytes, int NewArrayInstructions,
        IReadOnlyList<string> ValueConstructions, IReadOnlyList<string> ReferenceConstructions,
        IReadOnlyList<string> ExceptionConstructions, IReadOnlyList<string> AllocationCalls,
        IReadOnlyList<string> Calls, IReadOnlyList<string> UnresolvedCalls, int IndirectCalls);

    internal sealed record Factory(string Method, string ReturnType, string Status, string? Error,
        int ValueBytes, int WarmupIterations, int Iterations, long? AllocatedBytesAfterWarmup,
        ulong? Checksum, IReadOnlyList<FactoryIl> CallGraph);

    private sealed record Measurement(int Bytes, int Warmup, int Iterations, long Allocated, ulong Checksum);
    private delegate T Create<T>() where T : unmanaged;
    private static ulong _factoryChecksum;

    private delegate ReadOnlySpan<T> Getter<T>() where T : unmanaged;
    private static readonly Regex PayloadName = new(
        @"^get_(?:B[0-9A-F]{64}|P(?:ushort|uint|ulong)_[0-9A-F]{64})$",
        RegexOptions.CultureInvariant);

    internal static IReadOnlyList<Payload> Inspect(byte[] image, Assembly assembly)
    {
        using var stream = new System.IO.MemoryStream(image, writable: false);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var methods = assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(method => PayloadName.IsMatch(method.Name))
            .OrderBy(method => method.DeclaringType!.FullName, StringComparer.Ordinal)
            .ThenBy(method => method.Name, StringComparer.Ordinal).ToArray();
        return methods.Select(method => method.ReturnType == typeof(ReadOnlySpan<byte>)
            ? Inspect<byte>(method, pe, metadata)
            : method.ReturnType == typeof(ReadOnlySpan<ushort>) ? Inspect<ushort>(method, pe, metadata)
            : method.ReturnType == typeof(ReadOnlySpan<uint>) ? Inspect<uint>(method, pe, metadata)
            : method.ReturnType == typeof(ReadOnlySpan<ulong>) ? Inspect<ulong>(method, pe, metadata)
            : throw new InvalidOperationException("Unexpected payload getter type: " + method)).ToArray();
    }

    internal static void RequireEmbeddedAllocationFree(IReadOnlyList<Payload> payloads)
    {
        if (payloads.Count == 0) throw new InvalidOperationException("No immutable payload getters found");
        foreach (var payload in payloads)
            if (payload.NewArrayInstructions != 0 || payload.MatchingRvaFields == 0 ||
                payload.AllocatedBytesAfterWarmup != 0)
                throw new InvalidOperationException("Payload was not an allocation-free RVA view: " + payload);
    }

    internal static MethodInfo[] FactoryMethods(Assembly assembly) => assembly.GetTypes()
        .Where(type => type.Name.StartsWith("__DotccAggregate", StringComparison.Ordinal))
        .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName))
        .OrderBy(method => method.DeclaringType!.FullName, StringComparer.Ordinal)
        .ThenBy(method => method.Name, StringComparer.Ordinal).ToArray();

    internal static IReadOnlyList<FactoryIl> InspectFactoryIl(Assembly assembly) =>
        FactoryMethods(assembly).SelectMany(CallGraph).DistinctBy(body => body.Method).ToArray();

    internal static IReadOnlyList<Factory> InspectFactories(Assembly assembly)
    {
        var results = new List<Factory>();
        foreach (var method in FactoryMethods(assembly).Where(method => method.Name == "Create" && method.GetParameters().Length == 0))
        {
            var graph = CallGraph(method);
            Measurement? measurement = null;
            string? error = null;
            string status = "pass";
            try
            {
                if (method.ReturnType == typeof(void) || !method.ReturnType.IsValueType ||
                    method.ReturnType.IsByRefLike || method.ReturnType.ContainsGenericParameters)
                    throw new NotSupportedException("Factory return is not a closed unmanaged value type");
                if (graph.Any(body => body.AllocationCalls.Count != 0))
                    throw new NotSupportedException("Allocation service call found; factory was not invoked");
                measurement = (Measurement)typeof(AggregateFactoryAudit)
                    .GetMethod(nameof(MeasureFactory), BindingFlags.Static | BindingFlags.NonPublic)!
                    .MakeGenericMethod(method.ReturnType).Invoke(null, [method])!;
                if (measurement.Allocated != 0 || graph.Any(body => body.NewArrayInstructions != 0 ||
                    body.ReferenceConstructions.Count != 0 || body.UnresolvedCalls.Count != 0 || body.IndirectCalls != 0))
                    status = "fail";
            }
            catch (Exception exception)
            {
                while (exception is TargetInvocationException { InnerException: { } inner }) exception = inner;
                status = exception is NotSupportedException or ArgumentException ? "unsupported" : "error";
                error = exception.GetType().FullName + ": " + exception.Message;
            }
            results.Add(new Factory(MethodName(method), method.ReturnType.FullName ?? method.ReturnType.Name,
                status, error, measurement?.Bytes ?? 0, measurement?.Warmup ?? 0, measurement?.Iterations ?? 0,
                measurement?.Allocated, measurement?.Checksum, graph));
        }
        return results;
    }

    internal static void RequireAllocationFreeFactories(IReadOnlyList<Factory> factories, IReadOnlyList<FactoryIl> bodies)
    {
        if (factories.Count == 0) throw new InvalidOperationException("No zero-argument Create factories found");
        foreach (var factory in factories)
            if (factory.Status != "pass" || factory.AllocatedBytesAfterWarmup != 0)
                throw new InvalidOperationException("Factory allocation audit did not pass: " + factory);
        foreach (var body in bodies)
            if (body.NewArrayInstructions != 0 || body.ReferenceConstructions.Count != 0 ||
                body.AllocationCalls.Count != 0 || body.UnresolvedCalls.Count != 0 || body.IndirectCalls != 0)
                throw new InvalidOperationException("Factory/helper IL contains allocation or unresolved calls: " + body);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static Measurement MeasureFactory<T>(MethodInfo method) where T : unmanaged
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            throw new NotSupportedException("Factory return contains managed references");
        int bytes = Unsafe.SizeOf<T>();
        if (bytes > 1024 * 1024) throw new NotSupportedException("Factory value exceeds the bounded 1 MiB audit stack budget");
        var create = method.CreateDelegate<Create<T>>();
        const int warmup = 256;
        int iterations = bytes >= 4096 ? 1000 : 10000;
        ulong checksum = 0;
        for (int i = 0; i < warmup; ++i)
        {
            T result = create();
            checksum = unchecked((checksum << 1) ^ Consume(in result) ^ (uint)i);
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; ++i)
        {
            // A normal T local has one reusable IL local slot. There is no
            // per-iteration stackalloc, boxing, reflection or array allocation.
            T result = create();
            checksum = unchecked((checksum << 1) ^ Consume(in result) ^ (uint)i);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Volatile.Write(ref _factoryChecksum, checksum);
        return new Measurement(bytes, warmup, iterations, allocated, checksum);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong Consume<T>(in T value) where T : unmanaged
    {
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1));
        ulong checksum = 14695981039346656037UL;
        foreach (byte item in bytes) checksum = unchecked((checksum ^ item) * 1099511628211UL);
        return checksum;
    }

    private static IReadOnlyList<FactoryIl> CallGraph(MethodInfo root)
    {
        var pending = new Stack<MethodBase>();
        var seen = new HashSet<MethodBase>();
        var result = new List<FactoryIl>();
        pending.Push(root);
        while (pending.TryPop(out var method))
        {
            if (!seen.Add(method)) continue;
            result.Add(InspectMethod(method));
            foreach (var instruction in Instructions(method.GetMethodBody()?.GetILAsByteArray() ?? []))
            {
                if (instruction.Code != OpCodes.Call && instruction.Code != OpCodes.Callvirt && instruction.Code != OpCodes.Newobj) continue;
                try
                {
                    var called = Resolve(method, instruction.Token);
                    if (called.Module.Assembly == root.Module.Assembly &&
                        (called.DeclaringType!.Name.StartsWith("__Dotcc", StringComparison.Ordinal) || PayloadName.IsMatch(called.Name)))
                        pending.Push(called);
                }
                catch (Exception) { /* InspectMethod reports the exact unresolved token. */ }
            }
        }
        return result;
    }

    private static FactoryIl InspectMethod(MethodBase method)
    {
        byte[] il = method.GetMethodBody()?.GetILAsByteArray() ?? [];
        int arrays = 0, indirect = 0;
        var values = new List<string>();
        var references = new List<string>();
        var exceptions = new List<string>();
        var allocations = new List<string>();
        var calls = new List<string>();
        var unresolved = new List<string>();
        foreach (var instruction in Instructions(il))
        {
            if (instruction.Code == OpCodes.Newarr) ++arrays;
            if (instruction.Code == OpCodes.Calli) ++indirect;
            if (instruction.Code != OpCodes.Newobj && instruction.Code != OpCodes.Call && instruction.Code != OpCodes.Callvirt) continue;
            try
            {
                var called = Resolve(method, instruction.Token);
                string name = MethodName(called);
                calls.Add(name);
                if (instruction.Code == OpCodes.Newobj)
                {
                    if (called.DeclaringType!.IsValueType) values.Add(name);
                    else if (typeof(Exception).IsAssignableFrom(called.DeclaringType)) exceptions.Add(name);
                    else references.Add(name);
                }
                if (IsAllocationService(called)) allocations.Add(name);
            }
            catch (Exception exception)
            {
                unresolved.Add("0x" + instruction.Token.ToString("X8") + ": " + exception.Message);
            }
        }
        return new FactoryIl(MethodName(method), il.Length, arrays, values, references, exceptions,
            allocations, calls.Distinct().ToArray(), unresolved, indirect);
    }

    private static MethodBase Resolve(MethodBase method, int token) => method.Module.ResolveMethod(token,
        method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null)
        ?? throw new InvalidOperationException("Unresolved method token");

    private static bool IsAllocationService(MethodBase method)
    {
        string owner = method.DeclaringType?.FullName ?? "";
        string name = method.Name;
        return name is "GlobalArrayFrom" or "GlobalArrayZeroed" or "GlobalAlignedArrayZeroed" or
                "GlobalZeroed" or "GlobalAlignedZeroed" or "GlobalAlignedFrom" or "malloc" or "calloc" or "realloc" or "aligned_alloc" ||
            (owner is "System.Runtime.InteropServices.NativeMemory" or "System.Runtime.InteropServices.Marshal") && name.StartsWith("Alloc", StringComparison.Ordinal) ||
            owner == "System.GC" && name.StartsWith("Allocate", StringComparison.Ordinal) ||
            owner == "System.Array" && name == "CreateInstance" || name is "ToArray" or "ToList";
    }

    private static string MethodName(MethodBase method) => method.DeclaringType?.FullName + "." + method.Name;

    // Fully compile the measurement loop before counting allocations. The
    // product audit also warms the full getter set before acceptance to keep
    // process-wide runtime/JIT initialization outside the counted access loop.
    // Getter code and the process default tiering policy remain unchanged.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private static Payload Inspect<T>(MethodInfo method, PEReader pe, MetadataReader metadata) where T : unmanaged
    {
        var getter = method.CreateDelegate<Getter<T>>();
        byte[] expected = MemoryMarshal.AsBytes(getter()).ToArray();
        byte[] il = method.GetMethodBody()!.GetILAsByteArray()!;
        var instructions = Instructions(il).ToArray();
        var fields = instructions.Where(instruction => instruction.Code == OpCodes.Ldtoken ||
                instruction.Code == OpCodes.Ldsflda)
            .Where(instruction => (instruction.Token & unchecked((int)0xff000000)) == 0x04000000)
            .Select(instruction => instruction.Token).Distinct()
            .Select(token => metadata.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(token & 0x00ffffff)));
        int matchingRvas = fields.Count(field => field.GetRelativeVirtualAddress() != 0 &&
            pe.GetSectionData(field.GetRelativeVirtualAddress()).GetContent(0, expected.Length)
                .SequenceEqual(expected));

        // Type initialization and delegate/destination allocation are outside
        // the measurement. Every measured iteration reads AND copies all data.
        var destination = new T[getter().Length];
        for (int i = 0; i < 256; ++i) getter().CopyTo(destination);
        const int iterations = 10000;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; ++i) getter().CopyTo(destination);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (!MemoryMarshal.AsBytes(destination.AsSpan()).SequenceEqual(expected))
            throw new InvalidOperationException("Payload copy changed bytes: " + method.Name);
        return new Payload(method.DeclaringType!.FullName + "." + method.Name, typeof(T).Name,
            destination.Length, expected.Length, il.Length,
            instructions.Count(instruction => instruction.Code == OpCodes.Newarr), matchingRvas,
            allocated, iterations, Convert.ToHexString(SHA256.HashData(expected)));
    }

    internal static IEnumerable<(OpCode Code, int Token)> Instructions(byte[] il)
    {
        var codes = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(code => unchecked((ushort)code.Value));
        for (int position = 0; position < il.Length;)
        {
            ushort value = il[position++];
            if (value == 0xfe) value = (ushort)(0xfe00 | il[position++]);
            var code = codes[value];
            int size = code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, position),
                _ => 4
            };
            yield return (code, size == 4 ? BitConverter.ToInt32(il, position) : 0);
            position += size;
        }
    }
}

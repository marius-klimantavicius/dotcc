using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Managed.Emulation.Execution;

// Emits bytes only: native publication/execution remains in ContractTests.
internal static class Program
{
    private const ulong Ip = 0x400000;
    private sealed record Case(string Name, JitInstruction[] Instructions, int Cap = 32);
    private static int checks;

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        Interlocked.Increment(ref checks);
    }

    private static int Main(string[] args)
    {
        string? writeGolden = null, verifyGolden = null;
        bool probe = false;
        for (int i = 0; i < args.Length; ++i)
        {
            switch (args[i])
            {
                case "--write-golden": writeGolden = args[++i]; break;
                case "--verify-golden": verifyGolden = args[++i]; break;
                case "--probe": probe = true; break;
                default: throw new ArgumentException("Unknown option: " + args[i]);
            }
        }
        var cases = Corpus();
        var golden = new Golden[cases.Count];
        for (int i = 0; i < cases.Count; ++i)
        {
            Case test = cases[i];
            byte[] code = X64BlockEmitter.Emit(test.Instructions, test.Cap);
            golden[i] = new(test.Name, code);
            for (int repeat = 0; repeat < 3; ++repeat)
                Check(code.AsSpan().SequenceEqual(X64BlockEmitter.Emit(test.Instructions, test.Cap)), test.Name + " repeat");
#if !LEGACY_EMITTER
            Check(code.Length <= X64BlockEmitter.CodeCapacity, test.Name + " bounded bytes");
#endif
        }
        // Independent invocations must not retain another worker's fixups or IR.
        Parallel.For(0, 4, worker =>
        {
            for (int repeat = 0; repeat < 4; ++repeat)
            for (int i = worker; i < cases.Count; i += 4)
                Check(golden[i].Code.AsSpan().SequenceEqual(X64BlockEmitter.Emit(cases[i].Instructions, cases[i].Cap)),
                    cases[i].Name + " concurrent");
        });
        if (writeGolden != null)
            File.WriteAllText(writeGolden, JsonSerializer.Serialize(golden, ProbeJsonContext.Default.GoldenArray));
        if (verifyGolden != null)
        {
            var expected = JsonSerializer.Deserialize(File.ReadAllText(verifyGolden), ProbeJsonContext.Default.GoldenArray)!;
            Check(expected.Length == golden.Length, "golden corpus length");
            for (int i = 0; i < golden.Length; ++i)
                Check(expected[i].Name == golden[i].Name && expected[i].Code.AsSpan().SequenceEqual(golden[i].Code),
                    golden[i].Name + " frozen-r16 exact bytes");
        }
        CheckRejections();
#if !LEGACY_EMITTER
        CheckSpanAndCompatibility(cases[0]);
#endif
        if (probe)
        {
            var rows = new AllocationRow[cases.Count];
            for (int i = 0; i < cases.Count; ++i) rows[i] = Measure(cases[i], 128);
#if LEGACY_EMITTER
            const string emitter = "frozen-r16";
            const int scratch = -1, compatibilityScratch = -1;
#else
            const string emitter = "bounded-stack";
            const int scratch = X64BlockEmitter.ScratchBytes;
            int compatibilityScratch = scratch + 32 * Unsafe.SizeOf<JitInstruction>();
#endif
            Console.WriteLine(JsonSerializer.Serialize(new ProbeReport(emitter, RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription, scratch, compatibilityScratch, checks, rows), ProbeJsonContext.Default.ProbeReport));
        }
        else Console.WriteLine("PASS emitter bytes/reuse/API checks=" + checks + " cases=" + cases.Count);
        return 0;
    }

    private static AllocationRow Measure(Case test, int iterations)
    {
        byte[] last = X64BlockEmitter.Emit(test.Instructions, test.Cap);
        for (int i = 0; i < 16; ++i) last = X64BlockEmitter.Emit(test.Instructions, test.Cap);
        int codeBytes = last.Length;
        long before = GC.GetAllocatedBytesForCurrentThread();
        long started = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; ++i) last = X64BlockEmitter.Emit(test.Instructions, test.Cap);
        long ticks = Stopwatch.GetTimestamp() - started;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(last);
        // x64 byte[] allocation: 24-byte header and eight-byte object alignment.
        long resultArrayBytes = (24L + codeBytes + 7) & ~7L;
#if !LEGACY_EMITTER
        // Runtime metadata/profiling may allocate once on this thread; retain
        // raw totals and bound that noise rather than asserting zero BCL work.
        Check(allocated <= resultArrayBytes * iterations + 4096, test.Name + $" bounded emission allocation actual={allocated} resultArrays={resultArrayBytes * iterations}");
#endif
        return new(test.Name, test.Instructions.Length, test.Cap, iterations, codeBytes, allocated,
            resultArrayBytes * iterations, (double)ticks * 1000 / Stopwatch.Frequency);
    }

    private static void Rejected<T>(JitInstruction[] instructions, int cap, string? parameter = null) where T : ArgumentException
    {
        try { X64BlockEmitter.Emit(instructions, cap); }
        catch (T error)
        {
            Check(error.GetType() == typeof(T), typeof(T).Name + " exact exception type");
            Check(error.ParamName == parameter, typeof(T).Name + " parameter");
            return;
        }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }

    private static void CheckRejections()
    {
        var nop = new JitInstruction(Ip, 1, JitOperation.Nop, 64, default, default);
        Rejected<ArgumentNullException>(null!, 32, "instructions");
        Rejected<ArgumentOutOfRangeException>([], 32, "instructions");
        Rejected<ArgumentOutOfRangeException>(new JitInstruction[33], 32, "instructions");
        Rejected<ArgumentOutOfRangeException>([nop], 0, "maxInstructions");
        Rejected<ArgumentOutOfRangeException>([nop], 33, "maxInstructions");
        Rejected<ArgumentException>([nop, nop], 32);
        Rejected<ArgumentException>([nop with { Length = 0 }], 32);
        Rejected<ArgumentException>([nop with { Operation = (JitOperation)999 }], 32);
        Rejected<ArgumentException>([nop with { Operation = JitOperation.ConditionalJump, Condition = 16 }], 32);
        Rejected<ArgumentException>([nop with { Operation = JitOperation.Mov, Destination = new(16, 0) }], 32);
        Rejected<ArgumentException>([nop with { Operation = JitOperation.Call }], 32);
        var call = new JitInstruction(Ip, 5, JitOperation.Call, 64, default, default,
            SourceWidth: 64, Address: new(4, -1, 0, -8));
        Rejected<ArgumentException>([call, nop with { Ip = Ip + 5 }], 32);
    }

#if !LEGACY_EMITTER
    private static void CheckSpanAndCompatibility(Case test)
    {
        Span<JitInstruction> buffer = stackalloc JitInstruction[34];
        test.Instructions.CopyTo(buffer[1..]);
        byte[] expected = X64BlockEmitter.Emit(test.Instructions, test.Cap);
        Check(expected.AsSpan().SequenceEqual(X64BlockEmitter.Emit(buffer.Slice(1, test.Instructions.Length), test.Cap)), "stack span slice");
        Check(expected.AsSpan().SequenceEqual(X64BlockEmitter.Emit(new List<JitInstruction>(test.Instructions), test.Cap)), "List compatibility");
        IReadOnlyList<JitInstruction> wrapped = Array.AsReadOnly(test.Instructions);
        Check(expected.AsSpan().SequenceEqual(X64BlockEmitter.Emit(wrapped, test.Cap)), "IReadOnlyList compatibility");
        Check(X64BlockEmitter.ScratchBytes == 16872, "exact primary scratch payload");
        Check(Unsafe.SizeOf<JitInstruction>() == 120, "compatibility copy element size");
        expected[0] ^= 0xff;
        Check(expected[0] != X64BlockEmitter.Emit(test.Instructions, test.Cap)[0], "result has independent ownership");
    }
#endif

    private static List<Case> Corpus()
    {
        var cases = new List<Case>();
        void Add(string name, JitInstruction instruction) => cases.Add(new(name, [instruction]));
        foreach (var op in new[] { JitOperation.Mov, JitOperation.Add, JitOperation.Sub, JitOperation.Cmp,
            JitOperation.And, JitOperation.Or, JitOperation.Xor, JitOperation.Test, JitOperation.Inc, JitOperation.Dec })
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (bool immediate in new[] { false, true })
            Add($"scalar/{op}/{width}/{immediate}", new(Ip, 4, op, width,
                new(3, 0, ByteOffset: width == 8 ? 1 : 0), new(immediate ? 0 : 15, 0x123456789abcdef0, immediate)));
        foreach (var op in new[] { JitOperation.Movzx, JitOperation.Movsx })
        foreach (int width in new[] { 16, 32, 64 })
        foreach (int sourceWidth in new[] { 8, 16 })
            Add($"extend/{op}/{width}/{sourceWidth}", new(Ip, 4, op, width, new(15, 0),
                new(3, 0, ByteOffset: sourceWidth == 8 ? 1 : 0), SourceWidth: sourceWidth));
        Add("extend/movsxd", new(Ip, 3, JitOperation.Movsx, 64, new(15, 0), new(14, 0), SourceWidth: 32));
        foreach (int width in new[] { 32, 64 })
        foreach (var address in new[] { new JitAddress(15, 14, 3, long.MinValue),
            new JitAddress(-1, -1, 0, long.MaxValue, true), new JitAddress(-1, -1, 0, -128) })
        {
            string label = $"address/{width}/{address}";
            Add(label + "/lea", new(Ip, 7, JitOperation.Lea, width, new(15, 0), default, Address: address));
            Add(label + "/load", new(Ip, 7, JitOperation.LoadMov, width, new(15, 0), default, SourceWidth: width, Address: address));
            Add(label + "/store", new(Ip, 7, JitOperation.StoreMov, width, default, new(15, 0), SourceWidth: width, Address: address));
            foreach (int sourceWidth in new[] { 8, 16 })
                Add(label + "/loadzx/" + sourceWidth, new(Ip, 7, JitOperation.LoadMovzx, width,
                    new(15, 0), default, SourceWidth: sourceWidth, Address: address));
        }
        foreach (var op in new[] { JitOperation.MemoryCmp, JitOperation.MemoryCmpReverse, JitOperation.MemoryTest })
        foreach (int width in new[] { 8, 16, 32, 64 })
        foreach (bool immediate in new[] { false, true })
        {
            if (op == JitOperation.MemoryCmpReverse && immediate) continue;
            var operand = new JitOperand(15, 0xfedcba9876543210, immediate);
            Add($"memory-flags/{op}/{width}/{immediate}", new(Ip, 4, op, width, operand, operand,
                SourceWidth: width, Address: new(15, 14, 3, -48)));
        }
        for (int reg = 0; reg < 16; ++reg)
        {
            Add("push/" + reg, new(Ip, 2, JitOperation.Push, 64, default, new(reg, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)));
            Add("pop/" + reg, new(Ip, 2, JitOperation.Pop, 64, new(reg, 0), default, SourceWidth: 64, Address: new(4, -1, 0, 0)));
        }
        for (int condition = 0; condition < 16; ++condition)
        foreach (int cap in new[] { 1, 31, 32 })
            cases.Add(new($"condition/{condition}/{cap}", [
                new(Ip, 4, JitOperation.Mov, 64, new(15, 0), new(0, 1, true)),
                new(Ip + 4, 2, JitOperation.ConditionalJump, 64, default, default, Target: Ip, Condition: condition) ], cap));
        foreach (int length in new[] { 1, 2, 31, 32 })
        foreach (bool call in new[] { false, true })
        foreach (int cap in new[] { 1, 31, 32 })
        {
            var body = new JitInstruction[length];
            for (int i = 0; i < length - 1; ++i)
                body[i] = new(Ip + (uint)i * 4, 4, JitOperation.MemoryCmp, 64, default,
                    new(0, ulong.MaxValue, true), SourceWidth: 64, Address: new(15, 14, 3, long.MinValue));
            body[^1] = new(Ip + (uint)(length - 1) * 4, call ? 5 : 1, call ? JitOperation.Call : JitOperation.Ret,
                64, default, default, Target: Ip, SourceWidth: 64, Address: new(4, -1, 0, call ? -8 : 0));
            cases.Add(new($"terminal/{length}/{call}/{cap}", body, cap));
        }
        foreach (var op in new[] { JitOperation.Nop, JitOperation.Jump, JitOperation.ConditionalJump, JitOperation.MemoryCmp })
        {
            var body = new JitInstruction[32];
            for (int i = 0; i < body.Length; ++i)
                body[i] = new(Ip + (uint)i * 4, 4, op, 64, default, new(0, ulong.MaxValue, true),
                    Target: Ip + 31 * 4, Condition: 15, SourceWidth: 64, Address: new(15, 14, 3, long.MinValue));
            cases.Add(new("maximum/" + op, body));
        }
        // Exact instruction IR shape of the existing conditional-trace contract.
        cases.Add(new("contract/conditional-trace", [
            new(Ip, 4, JitOperation.LoadMov, 64, new(0, 0), default, SourceWidth: 64, Address: new(3, -1, 0, -32)),
            new(Ip + 4, 4, JitOperation.ConditionalJump, 64, default, default, Target: Ip + 12, Condition: 4),
            new(Ip + 8, 4, JitOperation.Mov, 64, new(2, 0), new(0, 42, true)),
            new(Ip + 12, 4, JitOperation.StoreMov, 64, default, new(2, 0), SourceWidth: 64, Address: new(3, -1, 0, -24)),
            new(Ip + 16, 4, JitOperation.ConditionalJump, 64, default, default, Target: Ip + 8, Condition: 5) ]));
        cases.Add(new("retained/110001391260", RetainedHotBlock()));
        return cases;
    }

    // Independently transcribed from astra-20261027-r2/hot-blocks.asm.txt.
    private static JitInstruction[] RetainedHotBlock()
    {
        const ulong start = 0x110001391260;
        return [
            new(start, 1, JitOperation.Push, 64, default, new(5, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
            new(start + 1, 2, JitOperation.Push, 64, default, new(15, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
            new(start + 3, 1, JitOperation.Push, 64, default, new(3, 0), SourceWidth: 64, Address: new(4, -1, 0, -8)),
            new(start + 4, 4, JitOperation.Sub, 64, new(4, 0), new(0, 16, true)),
            new(start + 8, 5, JitOperation.Lea, 64, new(5, 0), default, Address: new(4, -1, 0, 32)),
            new(start + 13, 3, JitOperation.Mov, 64, new(3, 0), new(7, 0)),
            new(start + 16, 3, JitOperation.Mov, 64, new(15, 0), new(2, 0)),
            new(start + 19, 2, JitOperation.Test, 32, new(6, 0), new(6, 0)),
            new(start + 21, 2, JitOperation.ConditionalJump, 64, default, default, Target: start + 75, Condition: 12),
            new(start + 23, 3, JitOperation.LoadMov, 32, new(7, 0), default, SourceWidth: 32, Address: new(3, -1, 0, 16)),
            new(start + 26, 2, JitOperation.Cmp, 32, new(6, 0), new(7, 0)),
            new(start + 28, 2, JitOperation.ConditionalJump, 64, default, default, Target: start + 75, Condition: 3),
            new(start + 30, 4, JitOperation.LoadMov, 64, new(0, 0), default, SourceWidth: 64, Address: new(3, -1, 0, 8)),
            new(start + 34, 2, JitOperation.Mov, 32, new(6, 0), new(6, 0)),
            new(start + 36, 3, JitOperation.Add, 64, new(6, 0), new(0, 0)),
            new(start + 39, 4, JitOperation.StoreMov, 64, default, new(6, 0), SourceWidth: 64, Address: new(5, -1, 0, -24)),
            new(start + 43, 2, JitOperation.Mov, 32, new(6, 0), new(7, 0)),
            new(start + 45, 3, JitOperation.Add, 64, new(6, 0), new(0, 0)),
            new(start + 48, 4, JitOperation.Lea, 64, new(7, 0), default, Address: new(5, -1, 0, -24)),
            new(start + 52, 5, JitOperation.Call, 64, default, default, Target: 0x110001390f10,
                SourceWidth: 64, Address: new(4, -1, 0, -8)) ];
    }
}

internal sealed record Golden(string Name, byte[] Code);
internal sealed record AllocationRow(string Name, int Instructions, int Cap, int Iterations, int CodeBytes,
    long AllocatedBytes, long ResultArrayBytes, double ElapsedMilliseconds);
internal sealed record ProbeReport(string Emitter, string Runtime, string OS, int StackScratchBytes,
    int CompatibilityStackScratchBytes, int Checks, AllocationRow[] Rows);

[JsonSerializable(typeof(Golden[]))]
[JsonSerializable(typeof(ProbeReport))]
internal partial class ProbeJsonContext : JsonSerializerContext { }

using System.IO.Compression;
using System.Text.Json;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    private readonly record struct NativeInventoryExample(string Bytes, string Operation, int Width, bool Memory, int SourceWidth, uint Options);
    // A bounded encoding witness inventory, not an exhaustive encoding proof.
    // Run separately from contracts/timing. It decodes and validates IR only;
    // it neither emits nor executes native code or guest operation semantics.
    static int WriteNativeLoweringInventory(string input, string output)
    {
        using var file = File.OpenRead(input);
        using var compressed = new GZipStream(file, CompressionMode.Decompress);
        using var source = JsonDocument.Parse(compressed);
        using var program = new Blink();
        using var binding = program.__DotCcEnter();
        var counts = new long[2048];
        var witnesses = new Dictionary<int, List<NativeInventoryExample>>();
        var rejectedIr = new List<object>();
        byte[][] prefixes = [[], [0x66], [0x40], [0x44], [0x48], [0x4c], [0x4f],
            [0x67], [0x64], [0x65], [0x66,0x48], [0x67,0x48], [0x64,0x48],
            [0x65,0x48], [0xf0], [0xf2], [0xf3]];
        Span<byte> bytes = stackalloc byte[15];
        long candidates = 0, decodedCandidates = 0, nativeCandidates = 0, invalidIrCandidates = 0;
        foreach (byte[] prefix in prefixes)
        for (int map = 0; map < 4; ++map)
        for (int opcode = 0; opcode < 256; ++opcode)
        for (int modrm = 0; modrm < 256; ++modrm)
        for (int variant = 0; variant < 2; ++variant)
        {
            bytes.Fill(variant == 0 ? (byte)0 : (byte)0xff);
            prefix.AsSpan().CopyTo(bytes);
            int at = prefix.Length;
            if (map != 0) bytes[at++] = 0x0f;
            if (map >= 2) bytes[at++] = map == 2 ? (byte)0x38 : (byte)0x3a;
            bytes[at++] = (byte)opcode; bytes[at] = (byte)modrm;
            ++candidates;
            Blink.XedDecodedInst decoded = default;
            fixed (byte* p = bytes)
                if (program.DecodeInstruction(&decoded, p, 15, Blink.XED_MODE_LONG) != 0 || decoded.length is < 1 or > 15)
                    continue;
            ++decodedCandidates;
            ReadOnlySpan<byte> exact = bytes[..decoded.length];
            bool repSite = FullCanonicalEncoding.TryClassifyRep(Ip, exact, out var rep, out _);
            JitInstruction instruction = default;
            if (!repSite && (!FullTranslationJit.TestTryClassifyNative(Ip, exact, out instruction) || !FullBlockEmitter.CanInline(instruction))) continue;
            int dispatch = (int)((decoded.op.rde >> 40) & 2047);
            string? failure = null;
            if (repSite ? !FullRepLowering.IsCanonical(in rep) : !FullBlockEmitter.TestValidateNative(in instruction, out failure))
            {
                ++invalidIrCandidates;
                if (rejectedIr.Count < 32) rejectedIr.Add(new { dispatch, bytes = Convert.ToHexString(exact), failure });
                continue;
            }
            ++nativeCandidates; ++counts[dispatch];
            if (!witnesses.TryGetValue(dispatch, out var list)) witnesses.Add(dispatch, list = []);
            if (list.Count < 8)
            {
                string encoding = Convert.ToHexString(exact);
                // Examples are deliberately limited; repeated samples do not
                // establish distinct encodings or an arm's complete coverage.
                bool duplicate = false;
                foreach (var existing in list) if (existing.Bytes == encoding) { duplicate = true; break; }
                if (!duplicate) list.Add(repSite
                    ? new(encoding, "NativeRepKernel/" + rep.Operation, rep.Width, true, rep.Width, (uint)rep.Repeat)
                    : new(encoding, instruction.Operation.ToString(), instruction.Width,
                        instruction.Destination.IsMemory || instruction.Source.IsMemory, instruction.SourceWidth, instruction.Options));
            }
        }
        var arms = new List<object>();
        foreach (var arm in source.RootElement.GetProperty("dispatch").EnumerateArray())
        {
            var op = arm.GetProperty("opcode");
            int? number = op.ValueKind == JsonValueKind.Number ? op.GetInt32() : null;
            string? selected = arm.GetProperty("selected_handler").GetString();
            bool fault = selected == "OpUd";
            bool syscall = number == 0x105;
            long hits = number is int n ? counts[n] : 0;
            arms.Add(new { selected_dispatch = arm.Clone(), native_sample_candidates = hits,
                native_examples = number is int index && witnesses.TryGetValue(index, out var examples) ? examples : [],
                status = fault ? "NAMED_EXCEPTION_HELPER" : syscall ? "NAMED_SYSCALL_HELPER" :
                    hits != 0 ? "NATIVE_WITNESSES_OTHER_ENCODINGS_UNKNOWN" : "UNWITNESSED_IN_BOUNDED_SAMPLE",
                complete_native_encoding_coverage = false,
                helper_reason = fault ? "Architecturally invalid selected instruction: deliver guest #UD/SIGILL after leaving generated code." :
                    syscall ? "Linux syscall runtime/host IO boundary after native RET; one typed managed call per syscall attempt." : null,
                ordinary_helper_is_accepted_exception = false });
        }
        var report = new { schema = 1, kind = "native-lowering-encoding-witness-inventory", status = "INCOMPLETE",
            opcode_maps = "primary/0F/0F38/0F3A; two immediate/displacement fill patterns",
            mode = "flat long/protected, TF/RF clear; exact authoritative decoded instruction lengths",
            limitations = "Non-exhaustive bounded prefix/opcode/ModRM/padding sample; candidate counts include repeats. No semantic/emission/runtime proof. An unwitnessed arm is UNKNOWN in this sampler, not proof of missing implementation. All unsampled form cells retain their separate source-family/qualification status. Typed wrapper counts are not native coverage.",
            candidates, decodedCandidates, nativeCandidates, invalidIrCandidates, rejectedIr,
            dispatch_arms = arms.Count, arms };
        using var destination = new FileStream(output, FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(destination, report, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"native-inventory: {arms.Count} arms, {nativeCandidates} repeated candidate witnesses, {invalidIrCandidates} invalid IR classifications; INCOMPLETE");
        return invalidIrCandidates == 0 ? 0 : 1;
    }
}

namespace Managed.Emulation.Execution
{
    internal sealed unsafe partial class FullTranslationJit
    {
        internal static bool TestTryClassifyNative(ulong ip, ReadOnlySpan<byte> bytes, out JitInstruction instruction)
            => TryClassifyNative(ip, bytes, out instruction);
    }
    internal static partial class FullBlockEmitter
    {
        internal static unsafe bool TestValidateNative(in JitInstruction instruction, out string? failure)
        {
            // Compile-time IR/tuple SHAPE only. These nonzero sentinel values
            // are never published, emitted, dereferenced or executed, and confer
            // no RX ownership or production-profile qualification. The old tool
            // misreported every numeric x87 site as malformed because it omitted
            // the mandatory typed catalog binding. Real compilation authenticates
            // its actual catalog owner/entry under the authority gate.
            FullX87KernelKey? key = CanInlineCanonicalX87Arithmetic(instruction) ? CanonicalX87ArithmeticKernel(instruction) :
                CanInlineCanonicalX87Memory(instruction) ? CanonicalX87MemoryKernel(instruction) :
                CanInlineCanonicalX87MemoryArithmetic(instruction) ? CanonicalX87MemoryArithmeticKernel(instruction) :
                CanInlineCanonicalX87Control(instruction) && CanonicalX87ControlNeedsKernel(instruction) ? CanonicalX87ControlKernel(instruction) :
                CanInlineCanonicalX87Transcendental(instruction) ? CanonicalX87TranscendentalKernel(instruction) : null;
            FullX87KernelCall binding = key is { } k ? new((byte)k,(nint)0x1000,1,32) : default;
            Span<FullBlockInstruction> site = stackalloc FullBlockInstruction[1]; site[0] = new(instruction, X87Kernel:binding);
            try { Validate(site); failure = null; return true; }
            catch (ArgumentException error) { failure = error.Message; return false; }
        }
    }
}

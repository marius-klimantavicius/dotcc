using System.Globalization;
using Managed.Zydis;
using Core = Managed.Zydis.Zydis;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] == "demo") RunDemo();
            else if (args[0] is "--help" or "-h" or "help") PrintUsage();
            else RunCommand(args);
            return 0;
        }
        catch (Exception error) when (error is ZydisException or ArgumentException or FormatException or IOException)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }

    private static void RunCommand(string[] args)
    {
        if (args.Length < 3) throw new ArgumentException("Expected command, mode, and bytes. Use --help.");
        var mode = ParseMode(args[1]);
        if (args[0] == "rewrite-immediate")
        {
            if (args.Length != 5) throw new ArgumentException("rewrite-immediate MODE HEX OPERAND VALUE");
            var instruction = InstructionCodec.Decode(ParseBytes(args[2]), mode);
            int index = int.Parse(args[3], CultureInfo.InvariantCulture);
            var bytes = instruction.ToEncoderRequest().WithImmediate(index, ParseUnsigned(args[4])).Encode();
            Console.WriteLine(Convert.ToHexString(bytes));
            PrintInstructions(InstructionCodec.DecodeBuffer(bytes, mode), Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL);
            return;
        }
        if (args[0] is not ("disassemble" or "file" or "roundtrip"))
            throw new ArgumentException("Unknown command. Use --help.");
        if (args.Length > 5) throw new ArgumentException("Too many arguments. Use --help.");
        byte[] input = args[0] == "file" ? File.ReadAllBytes(args[2]) : ParseBytes(args[2]);
        ulong address = args.Length >= 4 ? ParseUnsigned(args[3]) : 0;
        var style = args.Length == 5 ? ParseStyle(args[4]) : Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL;
        var instructions = InstructionCodec.DecodeBuffer(input, mode, address);
        PrintInstructions(instructions, style);
        if (args[0] == "roundtrip")
        {
            foreach (var instruction in instructions)
            {
                var encoded = RoundTrip(instruction);
                Console.WriteLine($"encoded {Convert.ToHexString(encoded)}");
            }
        }
    }

    private static void RunDemo()
    {
        byte[] input = [0x48, 0x83, 0xC0, 0x2A, 0xC3]; // add rax, 42; ret
        var instructions = InstructionCodec.DecodeBuffer(input, runtimeAddress: 0x1000);
        Require(instructions.Count == 2 && instructions[0].Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_ADD,
            "64-bit buffer decode");
        PrintInstructions(instructions, Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL);
        PrintInstructions(instructions, Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_ATT);
        var original = instructions[0];
        Require(original.VisibleOperandCount == 2 && original.Operands.Length >= 2,
            "owning visible and hidden operands");
        Require(original.Metadata.operand_width == 64, "decoded metadata");
        Require(original.CpuFlags is { modified: not 0 }, "owning accessed-flag metadata");
        Require(InstructionCodec.GetMnemonicName(original.Mnemonic) == "add"
            && InstructionCodec.GetRegisterName(Core.ZydisRegister_.ZYDIS_REGISTER_RAX) == "rax", "upstream name utilities");
        foreach (var instruction in instructions) RoundTrip(instruction);

        var request = original.ToEncoderRequest();
        var changedBytes = request.WithMnemonic(Core.ZydisMnemonic_.ZYDIS_MNEMONIC_SUB).WithImmediate(1, 0x33).Encode();
        var changed = InstructionCodec.Decode(changedBytes);
        Require(changed.Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_SUB && ImmediateValue(changed, 1) == 0x33,
            "decoded request mutation");
        Require(request.Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_ADD && request.GetOperand(1).imm.u == 0x2A,
            "request mutation leaves original snapshot intact");
        Console.WriteLine($"mutated {Convert.ToHexString(changedBytes)}  {changed.Format()}");

        var created = EncoderRequest.Create(Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64, Core.ZydisMnemonic_.ZYDIS_MNEMONIC_MOV,
            [EncoderRequest.Register(Core.ZydisRegister_.ZYDIS_REGISTER_RAX), EncoderRequest.Immediate(0x1234)]);
        var assembled = InstructionCodec.Decode(created.Encode());
        Require(assembled.Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_MOV && ImmediateValue(assembled, 1) == 0x1234,
            "new request encode");
        RoundTrip(assembled);

        var mode16 = InstructionCodec.DecodeBuffer([0xB8, 0x34, 0x12, 0xC3], Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LEGACY_16);
        var mode32 = InstructionCodec.DecodeBuffer([0xB8, 0x78, 0x56, 0x34, 0x12, 0xC3], Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LEGACY_32);
        Require(mode16.Count == 2 && mode16[0].Length == 3 && mode16[0].Metadata.operand_width == 16, "16-bit mode");
        Require(mode32.Count == 2 && mode32[0].Length == 5 && mode32[0].Metadata.operand_width == 32, "32-bit mode");
        RoundTrip(mode16[0]);
        RoundTrip(mode32[0]);

        var branch = InstructionCodec.Decode([0xE9, 5, 0, 0, 0], runtimeAddress: 0x1000);
        Require(branch.TryGetAbsoluteAddress(0, out ulong target, out var addressStatus)
            && addressStatus.IsSuccess && target == 0x100A, "absolute branch address");
        var absoluteRequest = branch.ToEncoderRequest().WithImmediate(0, target);
        var relocated = InstructionCodec.Decode(absoluteRequest.EncodeAbsolute(0x1001), runtimeAddress: 0x1001);
        Require(relocated.TryGetAbsoluteAddress(0, out var relocatedTarget, out _) && relocatedTarget == target,
            "absolute encoding preserves target");
        Require(absoluteRequest.GetOperand(0).imm.u == target, "absolute encoding keeps immutable request");

        byte[] nops = InstructionCodec.CreateNops(37);
        var padding = InstructionCodec.DecodeBuffer(nops);
        Require(padding.All(instruction => instruction.Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_NOP)
            && padding.Sum(instruction => instruction.Length) == nops.Length, "upstream NOP filling");
        Require(InstructionCodec.CreateNops(0).Length == 0, "empty NOP fill");

        // Dropping and overwriting the caller buffer must not change snapshots.
        input.AsSpan().Fill(0xCC);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Require(original.Bytes.SequenceEqual(new byte[] { 0x48, 0x83, 0xC0, 0x2A })
            && ImmediateValue(original, 1) == 0x2A, "owned input/operands survive caller mutation and GC");
        Require(original.Format().Length > 0 && original.Format(Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_ATT).Length > 0,
            "format owned result after GC");
        RoundTrip(original);

        CheckOrdinaryErrors();
        Parallel.For(0, 64, index =>
        {
            var decoded = InstructionCodec.Decode(original.Bytes);
            var bytes = decoded.ToEncoderRequest().WithImmediate(1, (ulong)index).Encode();
            Require(ImmediateValue(InstructionCodec.Decode(bytes), 1) == (ulong)index, "concurrent independent calls");
            Require(original.Format().Length > 0, "concurrent shared result");
        });
        Console.WriteLine("PASS: decode 16/32/64, Intel/ATT, metadata/operands, encode/mutate/roundtrip, addresses, NOPs, errors, ownership, concurrency");
    }

    private static void CheckOrdinaryErrors()
    {
        Require(!InstructionCodec.TryDecode([], out var empty, out var emptyStatus)
            && empty is null && emptyStatus.Value == Core.ZYDIS_STATUS_NO_MORE_DATA, "empty decode status");
        Require(!InstructionCodec.TryDecode([0x0F], out var truncated, out var truncatedStatus)
            && truncated is null && truncatedStatus.Value == Core.ZYDIS_STATUS_NO_MORE_DATA, "truncated decode status");
        var nop = EncoderRequest.Create(Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64, Core.ZydisMnemonic_.ZYDIS_MNEMONIC_NOP);
        Require(!nop.TryEncode(Span<byte>.Empty, out int written, out var status) && written == 0 && status.IsFailure,
            "short encode destination");
        var invalid = EncoderRequest.Create(Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64, Core.ZydisMnemonic_.ZYDIS_MNEMONIC_INVALID);
        Require(!invalid.TryEncode(new byte[Core.ZYDIS_MAX_INSTRUCTION_LENGTH], out written, out status)
            && written == 0 && status.IsFailure, "invalid encoder request");
        try
        {
            InstructionCodec.DecodeBuffer([0x90, 0x0F]);
            throw new InvalidOperationException("Expected truncated-buffer failure.");
        }
        catch (ZydisException error)
        {
            Require(error.BufferOffset == 1 && error.Status.Value == Core.ZYDIS_STATUS_NO_MORE_DATA,
                "buffer failure reports original byte offset and status");
        }
        var registerInstruction = InstructionCodec.Decode([0x48, 0x89, 0xD8]);
        Require(!registerInstruction.TryGetAbsoluteAddress(0, out ulong address, out status)
            && address == 0 && status.IsFailure, "non-address operand");
    }

    private static byte[] RoundTrip(DecodedInstruction instruction)
    {
        var encoded = instruction.ToEncoderRequest().Encode();
        var decoded = InstructionCodec.Decode(encoded, instruction.MachineMode, instruction.RuntimeAddress);
        Require(decoded.Mnemonic == instruction.Mnemonic && decoded.VisibleOperandCount == instruction.VisibleOperandCount,
            "roundtrip instruction identity");
        for (int index = 0; index < instruction.VisibleOperandCount; ++index)
        {
            var before = instruction.Operands[index];
            var after = decoded.Operands[index];
            Require(before.type == after.type && before.size == after.size, "roundtrip operand shape");
            if (before.type == Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_REGISTER)
                Require(before.reg.value == after.reg.value, "roundtrip register");
            if (before.type == Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_IMMEDIATE)
                Require(before.imm.value.u == after.imm.value.u, "roundtrip immediate");
            if (before.type == Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_MEMORY)
                Require(before.mem.@base == after.mem.@base && before.mem.index == after.mem.index
                    && before.mem.scale == after.mem.scale && before.mem.disp.value == after.mem.disp.value,
                    "roundtrip memory addressing");
        }
        Require(decoded.ToEncoderRequest().Encode().AsSpan().SequenceEqual(encoded), "stable re-encoding");
        return encoded;
    }

    private static ulong ImmediateValue(DecodedInstruction instruction, int index)
    {
        var operand = instruction.Operands[index];
        Require(operand.type == Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_IMMEDIATE, "expected immediate operand");
        return operand.imm.value.u;
    }

    private static void PrintInstructions(IReadOnlyList<DecodedInstruction> instructions, Core.ZydisFormatterStyle_ style)
    {
        foreach (var instruction in instructions)
            Console.WriteLine($"{instruction.RuntimeAddress:X16}  {Convert.ToHexString(instruction.Bytes),-30}  {instruction.Format(style)}");
    }

    private static Core.ZydisMachineMode_ ParseMode(string value) => value switch
    {
        "16" => Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LEGACY_16,
        "32" => Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LEGACY_32,
        "64" => Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64,
        _ => throw new ArgumentException("Mode must be 16, 32 or 64.")
    };

    private static Core.ZydisFormatterStyle_ ParseStyle(string value) => value.ToLowerInvariant() switch
    {
        "intel" => Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL,
        "att" => Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_ATT,
        _ => throw new ArgumentException("Style must be intel or att.")
    };

    private static byte[] ParseBytes(string value)
        => Convert.FromHexString(string.Concat(value.Where(character => !char.IsWhiteSpace(character) && character != '_')));

    private static ulong ParseUnsigned(string value) => value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
        ? ulong.Parse(value.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
        : ulong.Parse(value, CultureInfo.InvariantCulture);

    private static void Require(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException("Consumer assertion failed: " + description);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("ZydisManagedConsumer [demo]");
        Console.WriteLine("  disassemble MODE HEX [ADDRESS] [intel|att]");
        Console.WriteLine("  file MODE PATH [ADDRESS] [intel|att]");
        Console.WriteLine("  roundtrip MODE HEX [ADDRESS] [intel|att]");
        Console.WriteLine("  rewrite-immediate MODE HEX OPERAND VALUE");
        Console.WriteLine("MODE: 16, 32 or 64. Quote HEX when it contains spaces. Numbers are decimal or 0x-prefixed hex.");
    }
}

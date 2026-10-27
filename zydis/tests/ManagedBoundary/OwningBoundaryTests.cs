using Managed.Zydis;
using Core = Managed.Zydis.Zydis;

internal static unsafe class OwningBoundaryTests
{
    public static void Run()
    {
        byte[] callerStorage = [0xCC, 0x48, 0x83, 0xC0, 0x2A, 0xCC]; // add rax, 42, inside a slice
        var decoded = InstructionCodec.Decode(callerStorage.AsSpan(1, 4));
        Check.Require(decoded.Metadata.cpu_flags == null && decoded.Metadata.fpu_flags == null,
            "owning metadata never exposes either borrowed flag pointer");
        Check.Require(decoded.CpuFlags is { modified: not 0 }, "flag values are retained separately from cleared pointers");

        var metadataCopy = decoded.Metadata;
        metadataCopy.mnemonic = Core.ZydisMnemonic_.ZYDIS_MNEMONIC_SUB;
        var operandCopy = decoded.Operands[1];
        operandCopy.imm.value.u = 0xEE;
        Check.Require(decoded.Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_ADD
            && decoded.Operands[1].imm.value.u == 42, "editing metadata and union copies cannot change an owning result");

        var rawRequest = decoded.ToEncoderRequest().Value;
        var request = new EncoderRequest(rawRequest);
        byte[] expected = [0x48, 0x83, 0xC0, 0x2A];
        Check.Require(request.Encode().AsSpan().SequenceEqual(expected), "baseline request emits the exact add encoding");
        rawRequest.mnemonic = Core.ZydisMnemonic_.ZYDIS_MNEMONIC_SUB;
        rawRequest.operands[1].imm.u = 0x55;
        var valueCopy = request.Value;
        valueCopy.operands[0].reg.value = Core.ZydisRegister_.ZYDIS_REGISTER_RBX;
        valueCopy.operands[1].imm.u = 0x66;
        Check.Require(request.Mnemonic == Core.ZydisMnemonic_.ZYDIS_MNEMONIC_ADD
            && request.GetOperand(0).reg.value == Core.ZydisRegister_.ZYDIS_REGISTER_RAX
            && request.GetOperand(1).imm.u == 42, "request constructor and Value copy own the emitted inline operand array");

        callerStorage.AsSpan().Fill(0xCC);
        Check.CompactGc();
        Check.Require(decoded.Bytes.SequenceEqual(new byte[] { 0x48, 0x83, 0xC0, 0x2A })
            && decoded.ToEncoderRequest().Encode().AsSpan().SequenceEqual(expected)
            && request.Encode().AsSpan().SequenceEqual(expected), "caller storage and edited struct copies cannot affect later encoding");

        byte[] storage = Check.GuardedBuffer(Core.ZYDIS_MAX_INSTRUCTION_LENGTH);
        Check.Require(request.TryEncode(storage.AsSpan(Check.GuardLength, Core.ZYDIS_MAX_INSTRUCTION_LENGTH),
            out int written, out var status) && status.IsSuccess && written == expected.Length,
            "encode into a borrowed destination slice reports the exact length");
        Check.Require(storage.AsSpan(Check.GuardLength, written).SequenceEqual(expected),
            "sliced output contains the exact encoding");
        Check.Guards(storage, Core.ZYDIS_MAX_INSTRUCTION_LENGTH);

        byte[] shortStorage = Check.GuardedBuffer(1);
        Check.Require(!request.TryEncode(shortStorage.AsSpan(Check.GuardLength, 1), out written, out status)
            && written == 0 && status.IsFailure, "short borrowed destination returns failure and zero written");
        Check.Guards(shortStorage, 1);
        Check.Require(request.Encode().AsSpan().SequenceEqual(expected), "failed bounded encode preserves the owning request");
    }
}

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raw = Managed.Zydis.Tests.ZydisQualification;

// These are actual emitted types. C sizeof/offsetof literals alone cannot prove
// CLR storage agrees with the compiler's C layout metadata.
internal static unsafe class ActualAbi
{
    internal static void Check(string nativePath)
    {
        var values = new List<string>();
        void Layout<T>(string name) where T : unmanaged
        {
            Aligned<T> aligned = default;
            values.Add($"layout:{name}:{sizeof(T)}:{(byte*)&aligned.Value - (byte*)&aligned}");
        }
        void Field<TOwner, TField>(string name, ref TOwner owner, ref TField field)
            where TOwner : unmanaged where TField : unmanaged =>
            values.Add($"offset:{name}:{(byte*)Unsafe.AsPointer(ref field) - (byte*)Unsafe.AsPointer(ref owner)}");
        void Bytes<T>(string name, ref T value) where T : unmanaged =>
            values.Add($"storage:{name}:{Convert.ToHexString(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref value), sizeof(T)))}");

        Layout<byte>("ZyanBool"); Layout<ulong>("ZyanUSize"); Layout<uint>("ZyanStatus");
        Layout<Raw.ZydisMachineMode_>("ZydisMachineMode");
        Layout<Raw.ZydisDecoder_>("ZydisDecoder"); Layout<Raw.ZydisDecoderContext_>("ZydisDecoderContext");
        Layout<Raw.ZydisDecodedInstruction_>("ZydisDecodedInstruction");
        Layout<Raw.ZydisDecodedOperand_>("ZydisDecodedOperand");
        Layout<Raw.ZydisEncoderOperand_>("ZydisEncoderOperand"); Layout<Raw.ZydisEncoderRequest_>("ZydisEncoderRequest");
        Layout<Raw.ZydisFormatter_>("ZydisFormatter"); Layout<Raw.ZydisFormatterContext_>("ZydisFormatterContext");
        Layout<Raw.ZydisFormatterBuffer_>("ZydisFormatterBuffer"); Layout<Raw.ZydisFormatterToken_>("ZydisFormatterToken");
        Layout<Raw.ZydisRegisterContext_>("ZydisRegisterContext");
        Layout<Raw.ZydisDisassembledInstruction_>("ZydisDisassembledInstruction");
        Layout<Raw.ZyanString_>("ZyanString"); Layout<nuint>("ZydisFormatterFunc");
        Raw.ZydisDecoder_ decoder = default;
        Raw.ZydisDecodedInstruction_ insn = default;
        Raw.ZydisDecodedOperand_ operand = default;
        Raw.ZydisEncoderOperand_ encoder = default;
        Raw.ZydisEncoderRequest_ request = default;
        Raw.ZydisFormatter_ formatter = default;
        Raw.ZydisFormatterContext_ context = default;
        Raw.ZydisFormatterBuffer_ buffer = default;
        Raw.ZydisFormatterToken_ token = default;
        Raw.ZydisDisassembledInstruction_ simple = default;
        Field("ZydisDecoder.machine_mode", ref decoder, ref decoder.machine_mode);
        Field("ZydisDecoder.stack_width", ref decoder, ref decoder.stack_width);
        Field("ZydisDecodedInstruction.mnemonic", ref insn, ref insn.mnemonic);
        Field("ZydisDecodedInstruction.attributes", ref insn, ref insn.attributes);
        Field("ZydisDecodedInstruction.avx", ref insn, ref insn.avx);
        Field("ZydisDecodedInstruction.raw", ref insn, ref insn.raw);
        Field("ZydisDecodedOperand.type", ref operand, ref operand.type);
        Field("ZydisDecodedOperand.reg", ref operand, ref operand.reg);
        Field("ZydisDecodedOperand.mem", ref operand, ref operand.mem);
        Field("ZydisDecodedOperand.ptr", ref operand, ref operand.ptr);
        Field("ZydisDecodedOperand.imm", ref operand, ref operand.imm);
        Field("ZydisEncoderOperand.reg", ref encoder, ref encoder.reg);
        Field("ZydisEncoderOperand.mem", ref encoder, ref encoder.mem);
        Field("ZydisEncoderOperand.mem.displacement", ref encoder, ref encoder.mem.displacement);
        Field("ZydisEncoderOperand.ptr", ref encoder, ref encoder.ptr);
        Field("ZydisEncoderOperand.imm", ref encoder, ref encoder.imm);
        Field("ZydisEncoderRequest.prefixes", ref request, ref request.prefixes);
        Field("ZydisEncoderRequest.operands", ref request, ref request.operands);
        Field("ZydisEncoderRequest.evex", ref request, ref request.evex);
        Field("ZydisEncoderRequest.mvex", ref request, ref request.mvex);
        values.Add($"offset:ZydisFormatter.func_print_mnemonic:{(byte*)&formatter.func_print_mnemonic - (byte*)&formatter}");
        Field("ZydisFormatterContext.user_data", ref context, ref Unsafe.AsRef<nuint>(&context.user_data));
        Field("ZydisFormatterBuffer.string", ref buffer, ref buffer.@string);
        Field("ZydisFormatterToken.next", ref token, ref token.next);
        Field("ZydisDisassembledInstruction.operands", ref simple, ref simple.operands);
        Field("ZydisDisassembledInstruction.text", ref simple, ref simple.text);

        operand.type = Raw.ZydisOperandType_.ZYDIS_OPERAND_TYPE_IMMEDIATE;
        operand.imm.is_signed = 1; operand.imm.is_relative = 1; operand.imm.value.u = 0xFEDCBA9876543210;
        Bytes("decoded-immediate", ref operand);
        encoder.type = Raw.ZydisOperandType_.ZYDIS_OPERAND_TYPE_MEMORY;
        encoder.reg.value = Raw.ZydisRegister_.ZYDIS_REGISTER_RAX; encoder.reg.is4 = 1;
        encoder.mem.@base = Raw.ZydisRegister_.ZYDIS_REGISTER_RIP; encoder.mem.index = Raw.ZydisRegister_.ZYDIS_REGISTER_RCX;
        encoder.mem.scale = 8; encoder.mem.displacement = -0x123456789; encoder.mem.size = 8;
        encoder.ptr.segment = 0xABCD; encoder.ptr.offset = 0x12345678; encoder.imm.u = 0xFFEEDDCCBBAA9988;
        Bytes("encoder-operand", ref encoder);
        var expected = File.ReadAllLines(nativePath).Where(line => line.StartsWith("layout:") ||
            line.StartsWith("offset:") || line.StartsWith("storage:"));
        if (!values.SequenceEqual(expected))
            throw new InvalidOperationException("Actual emitted CLR layouts/offsets/storage differ from native:\n" + string.Join("\n", values));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Aligned<T> where T : unmanaged { public byte Prefix; public T Value; }
}

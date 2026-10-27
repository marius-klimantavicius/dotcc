#nullable enable
using System;
using Core = Managed.Zydis.Zydis;

namespace Managed.Zydis;

/// <summary>
/// An immutable snapshot of the actual emitted upstream encoder request. Copy
/// <see cref="Value"/>, edit any upstream field and construct a new request to
/// use features beyond the convenience methods.
/// </summary>
public sealed class EncoderRequest
{
    private readonly Core.ZydisEncoderRequest_ value;

    public EncoderRequest(Core.ZydisEncoderRequest_ value)
    {
        if (value.operand_count > Core.ZYDIS_ENCODER_MAX_OPERANDS)
            throw new ArgumentOutOfRangeException(nameof(value), "Operand count exceeds upstream request storage.");
        this.value = value;
    }

    public Core.ZydisEncoderRequest_ Value => value;
    public int OperandCount => value.operand_count;
    public Core.ZydisMnemonic_ Mnemonic => value.mnemonic;
    public Core.ZydisMachineMode_ MachineMode => value.machine_mode;

    public static EncoderRequest Create(Core.ZydisMachineMode_ machineMode, Core.ZydisMnemonic_ mnemonic,
        ReadOnlySpan<Core.ZydisEncoderOperand_> operands = default)
    {
        if (operands.Length > Core.ZYDIS_ENCODER_MAX_OPERANDS)
            throw new ArgumentOutOfRangeException(nameof(operands));
        Core.ZydisEncoderRequest_ request = default;
        request.machine_mode = machineMode;
        request.mnemonic = mnemonic;
        request.operand_count = (byte)operands.Length;
        for (int i = 0; i < operands.Length; ++i) request.operands[i] = operands[i];
        return new(request);
    }

    public Core.ZydisEncoderOperand_ GetOperand(int index)
    {
        ValidateIndex(index);
        // Copy before indexing an inline array in a readonly field.
        var request = value;
        return request.operands[index];
    }

    public EncoderRequest WithOperand(int index, Core.ZydisEncoderOperand_ operand)
    {
        ValidateIndex(index);
        var request = value;
        request.operands[index] = operand;
        return new(request);
    }

    public EncoderRequest WithMnemonic(Core.ZydisMnemonic_ mnemonic)
    {
        var request = value;
        request.mnemonic = mnemonic;
        return new(request);
    }

    public EncoderRequest WithImmediate(int index, ulong immediate)
    {
        var operand = GetOperand(index);
        if (operand.type != Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_IMMEDIATE)
            throw new ArgumentException("Selected operand is not an immediate.", nameof(index));
        operand.imm.u = immediate;
        return WithOperand(index, operand);
    }

    public byte[] Encode() => InstructionCodec.Encode(this);

    /// <summary>
    /// Encode a request whose branch/memory addresses are absolute. Upstream's
    /// conversion to relative values operates on a private copy of the request.
    /// </summary>
    public byte[] EncodeAbsolute(ulong runtimeAddress) => InstructionCodec.EncodeAbsolute(this, runtimeAddress);

    public bool TryEncode(Span<byte> destination, out int written, out ZydisStatus status)
        => InstructionCodec.TryEncode(this, destination, out written, out status);

    public static Core.ZydisEncoderOperand_ Register(Core.ZydisRegister_ register)
    {
        Core.ZydisEncoderOperand_ operand = default;
        operand.type = Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_REGISTER;
        operand.reg.value = register;
        return operand;
    }

    public static Core.ZydisEncoderOperand_ Immediate(ulong immediate)
    {
        Core.ZydisEncoderOperand_ operand = default;
        operand.type = Core.ZydisOperandType_.ZYDIS_OPERAND_TYPE_IMMEDIATE;
        operand.imm.u = immediate;
        return operand;
    }

    private void ValidateIndex(int index)
    {
        if ((uint)index >= (uint)OperandCount) throw new ArgumentOutOfRangeException(nameof(index));
    }
}

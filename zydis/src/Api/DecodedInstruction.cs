#nullable enable
using System;
using Core = Managed.Zydis.Zydis;

namespace Managed.Zydis;

/// <summary>
/// An immutable, owning instruction snapshot. Operands include hidden operands;
/// visible operands are the leading <see cref="VisibleOperandCount"/> entries.
/// </summary>
public sealed unsafe class DecodedInstruction
{
    private readonly byte[] bytes;
    private readonly Core.ZydisDecodedOperand_[] operands;
    private readonly Core.ZydisDecodedInstruction_ metadata;

    internal DecodedInstruction(ReadOnlySpan<byte> input, Core.ZydisDecodedInstruction_ instruction,
        ReadOnlySpan<Core.ZydisDecodedOperand_> decodedOperands, ulong runtimeAddress)
    {
        CpuFlags = instruction.cpu_flags == null ? null : *instruction.cpu_flags;
        FpuFlags = instruction.fpu_flags == null ? null : *instruction.fpu_flags;
        // No translated address may escape into the managed owning snapshot.
        instruction.cpu_flags = null;
        instruction.fpu_flags = null;
        metadata = instruction;
        bytes = input[..instruction.length].ToArray();
        operands = decodedOperands[..instruction.operand_count].ToArray();
        RuntimeAddress = runtimeAddress;
    }

    public ulong RuntimeAddress { get; }
    public int Length => metadata.length;
    public Core.ZydisMnemonic_ Mnemonic => metadata.mnemonic;
    public Core.ZydisMachineMode_ MachineMode => metadata.machine_mode;
    public int VisibleOperandCount => metadata.operand_count_visible;
    public ReadOnlySpan<byte> Bytes => bytes;
    public ReadOnlySpan<Core.ZydisDecodedOperand_> Operands => operands;
    public ReadOnlySpan<Core.ZydisDecodedOperand_> VisibleOperands => operands.AsSpan(0, VisibleOperandCount);

    /// <summary>
    /// A copy of the actual emitted upstream metadata, including AVX, raw encoding
    /// and instruction category fields. cpu_flags/fpu_flags are cleared; use the
    /// separate owning flag snapshots below.
    /// </summary>
    public Core.ZydisDecodedInstruction_ Metadata => metadata;
    public Core.ZydisAccessedFlags_? CpuFlags { get; }
    public Core.ZydisAccessedFlags_? FpuFlags { get; }

    public string Format(Core.ZydisFormatterStyle_ style = Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL,
        ulong? runtimeAddress = null)
        => InstructionCodec.Format(this, style, runtimeAddress ?? RuntimeAddress);

    public EncoderRequest ToEncoderRequest() => InstructionCodec.ToEncoderRequest(this);

    public bool TryGetAbsoluteAddress(int operandIndex, out ulong address, out ZydisStatus status,
        ulong? runtimeAddress = null)
        => InstructionCodec.TryGetAbsoluteAddress(this, operandIndex, runtimeAddress ?? RuntimeAddress,
            out address, out status);

    internal Core.ZydisDecodedInstruction_ BorrowMetadata(Core.ZydisAccessedFlags_* cpu,
        Core.ZydisAccessedFlags_* fpu)
    {
        var result = metadata;
        result.cpu_flags = CpuFlags.HasValue ? cpu : null;
        result.fpu_flags = FpuFlags.HasValue ? fpu : null;
        return result;
    }
}

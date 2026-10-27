#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Core = Managed.Zydis.Zydis;

namespace Managed.Zydis;

/// <summary>
/// Stateless owning entrypoints over the translated upstream implementation.
/// Modes, styles, mnemonics and registers use Zydis's generated enum types.
/// Independent calls and immutable results can be used concurrently.
/// </summary>
public static unsafe class InstructionCodec
{
    public static DecodedInstruction Decode(ReadOnlySpan<byte> input,
        Core.ZydisMachineMode_ machineMode = Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64,
        ulong runtimeAddress = 0, Core.ZydisStackWidth_? stackWidth = null)
    {
        if (!TryDecode(input, out var instruction, out var status, machineMode, runtimeAddress, stackWidth))
            throw new ZydisException("decode", status);
        return instruction;
    }

    public static bool TryDecode(ReadOnlySpan<byte> input,
        [NotNullWhen(true)] out DecodedInstruction? instruction, out ZydisStatus status,
        Core.ZydisMachineMode_ machineMode = Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64,
        ulong runtimeAddress = 0, Core.ZydisStackWidth_? stackWidth = null)
    {
        instruction = null;
        Core.ZydisDecoder_ decoder = default;
        status = new(Core.ZydisDecoderInit(&decoder, machineMode, stackWidth ?? DefaultStackWidth(machineMode)));
        if (status.IsFailure) return false;

        Core.ZydisDecodedInstruction_ metadata = default;
        Core.ZydisDecodedOperand_* operands = stackalloc Core.ZydisDecodedOperand_[Core.ZYDIS_MAX_OPERAND_COUNT];
        new Span<Core.ZydisDecodedOperand_>(operands, Core.ZYDIS_MAX_OPERAND_COUNT).Clear();
        byte emptyBuffer = 0;
        fixed (byte* buffer = input)
        {
            // A non-null zero-length input asks upstream for its ordinary
            // NO_MORE_DATA status rather than an invalid-pointer error.
            status = new(Core.ZydisDecoderDecodeFull(&decoder, buffer == null ? &emptyBuffer : buffer,
                (ulong)input.Length, &metadata, operands));
            if (status.IsFailure) return false;
            if (metadata.length == 0 || metadata.length > input.Length
                || metadata.length > Core.ZYDIS_MAX_INSTRUCTION_LENGTH
                || metadata.operand_count > Core.ZYDIS_MAX_OPERAND_COUNT
                || metadata.operand_count_visible > metadata.operand_count)
                throw new InvalidOperationException("Translated decoder returned inconsistent instruction bounds.");
            instruction = new(input, metadata,
                new ReadOnlySpan<Core.ZydisDecodedOperand_>(operands, metadata.operand_count), runtimeAddress);
            return true;
        }
    }

    /// <summary>
    /// Decode the entire buffer. An invalid/truncated instruction fails with its
    /// byte offset; bytes are never silently skipped. Empty input is an empty list.
    /// Runtime addresses wrap at 64 bits, matching upstream address arithmetic.
    /// </summary>
    public static IReadOnlyList<DecodedInstruction> DecodeBuffer(ReadOnlySpan<byte> input,
        Core.ZydisMachineMode_ machineMode = Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64,
        ulong runtimeAddress = 0, Core.ZydisStackWidth_? stackWidth = null)
    {
        var result = new List<DecodedInstruction>();
        int offset = 0;
        while (offset < input.Length)
        {
            if (!TryDecode(input[offset..], out var instruction, out var status, machineMode,
                unchecked(runtimeAddress + (ulong)offset), stackWidth))
                throw new ZydisException("decode", status, offset);
            result.Add(instruction);
            offset += instruction.Length;
        }
        return result.AsReadOnly();
    }

    public static string Format(DecodedInstruction instruction,
        Core.ZydisFormatterStyle_ style = Core.ZydisFormatterStyle_.ZYDIS_FORMATTER_STYLE_INTEL,
        ulong? runtimeAddress = null)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        Core.ZydisFormatter_ formatter = default;
        new ZydisStatus(Core.ZydisFormatterInit(&formatter, style))
            .ThrowIfFailed("formatter initialization");
        var cpu = instruction.CpuFlags.GetValueOrDefault();
        var fpu = instruction.FpuFlags.GetValueOrDefault();
        var metadata = instruction.BorrowMetadata(&cpu, &fpu);
        // This is only a default-formatter buffer, with no user hooks or symbol
        // strings; a formatter overflow still remains an upstream failure.
        Span<byte> output = stackalloc byte[4096];
        output.Clear();
        Core.ZydisDecodedOperand_ emptyOperand = default;
        fixed (Core.ZydisDecodedOperand_* operands = instruction.Operands)
        fixed (byte* buffer = output)
        {
            var status = new ZydisStatus(Core.ZydisFormatterFormatInstruction(&formatter, &metadata,
                operands == null ? &emptyOperand : operands,
                (byte)instruction.VisibleOperandCount, buffer, (ulong)output.Length,
                runtimeAddress ?? instruction.RuntimeAddress, null));
            status.ThrowIfFailed("format");
            int length = output.IndexOf((byte)0);
            if (length < 0) throw new InvalidOperationException("Translated formatter returned unterminated text.");
            return Encoding.UTF8.GetString(output[..length]);
        }
    }

    public static EncoderRequest ToEncoderRequest(DecodedInstruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        var cpu = instruction.CpuFlags.GetValueOrDefault();
        var fpu = instruction.FpuFlags.GetValueOrDefault();
        var metadata = instruction.BorrowMetadata(&cpu, &fpu);
        Core.ZydisEncoderRequest_ request = default;
        Core.ZydisDecodedOperand_ emptyOperand = default;
        fixed (Core.ZydisDecodedOperand_* operands = instruction.Operands)
        {
            new ZydisStatus(Core.ZydisEncoderDecodedInstructionToEncoderRequest(&metadata,
                operands == null ? &emptyOperand : operands, (byte)instruction.VisibleOperandCount, &request))
                .ThrowIfFailed("decoded instruction to encoder request");
        }
        return new(request);
    }

    public static byte[] Encode(EncoderRequest request)
    {
        Span<byte> output = stackalloc byte[Core.ZYDIS_MAX_INSTRUCTION_LENGTH];
        if (!TryEncode(request, output, out int written, out var status))
            throw new ZydisException("encode", status);
        return output[..written].ToArray();
    }

    /// <summary>
    /// Returns zero written on failure; destination contents on failure are
    /// unspecified, as in upstream. The request is never modified.
    /// </summary>
    public static bool TryEncode(EncoderRequest request, Span<byte> destination,
        out int written, out ZydisStatus status)
        => TryEncodeCore(request, destination, null, out written, out status);

    public static byte[] EncodeAbsolute(EncoderRequest request, ulong runtimeAddress)
    {
        Span<byte> output = stackalloc byte[Core.ZYDIS_MAX_INSTRUCTION_LENGTH];
        if (!TryEncodeAbsolute(request, output, runtimeAddress, out int written, out var status))
            throw new ZydisException("encode absolute", status);
        return output[..written].ToArray();
    }

    public static bool TryEncodeAbsolute(EncoderRequest request, Span<byte> destination,
        ulong runtimeAddress, out int written, out ZydisStatus status)
        => TryEncodeCore(request, destination, runtimeAddress, out written, out status);

    private static bool TryEncodeCore(EncoderRequest request, Span<byte> destination,
        ulong? runtimeAddress, out int written, out ZydisStatus status)
    {
        ArgumentNullException.ThrowIfNull(request);
        written = 0;
        var value = request.Value;
        ulong length = (ulong)destination.Length;
        byte emptyBuffer = 0;
        fixed (byte* output = destination)
        {
            byte* buffer = output == null ? &emptyBuffer : output;
            status = new(runtimeAddress is ulong address
                ? Core.ZydisEncoderEncodeInstructionAbsolute(&value, buffer, &length, address)
                : Core.ZydisEncoderEncodeInstruction(&value, buffer, &length));
        }
        if (status.IsFailure) return false;
        if (length == 0 || length > (ulong)destination.Length || length > Core.ZYDIS_MAX_INSTRUCTION_LENGTH)
            throw new InvalidOperationException("Translated encoder returned inconsistent output bounds.");
        written = (int)length;
        return true;
    }

    public static bool TryGetAbsoluteAddress(DecodedInstruction instruction, int operandIndex,
        ulong runtimeAddress, out ulong address, out ZydisStatus status)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        if ((uint)operandIndex >= (uint)instruction.Operands.Length)
            throw new ArgumentOutOfRangeException(nameof(operandIndex));
        var operand = instruction.Operands[operandIndex];
        var cpu = instruction.CpuFlags.GetValueOrDefault();
        var fpu = instruction.FpuFlags.GetValueOrDefault();
        var metadata = instruction.BorrowMetadata(&cpu, &fpu);
        ulong value = 0;
        status = new(Core.ZydisCalcAbsoluteAddress(&metadata, &operand, runtimeAddress, &value));
        address = status.IsSuccess ? value : 0;
        return status.IsSuccess;
    }

    /// <summary>Calculate an address using the actual emitted register-context type.</summary>
    public static bool TryGetAbsoluteAddress(DecodedInstruction instruction, int operandIndex,
        ulong runtimeAddress, in Core.ZydisRegisterContext_ registerContext,
        out ulong address, out ZydisStatus status)
    {
        ArgumentNullException.ThrowIfNull(instruction);
        if ((uint)operandIndex >= (uint)instruction.Operands.Length)
            throw new ArgumentOutOfRangeException(nameof(operandIndex));
        var operand = instruction.Operands[operandIndex];
        var context = registerContext;
        var cpu = instruction.CpuFlags.GetValueOrDefault();
        var fpu = instruction.FpuFlags.GetValueOrDefault();
        var metadata = instruction.BorrowMetadata(&cpu, &fpu);
        ulong value = 0;
        status = new(Core.ZydisCalcAbsoluteAddressEx(&metadata, &operand, runtimeAddress, &context, &value));
        address = status.IsSuccess ? value : 0;
        return status.IsSuccess;
    }

    public static void FillNops(Span<byte> destination)
    {
        byte emptyBuffer = 0;
        fixed (byte* buffer = destination)
        {
            new ZydisStatus(Core.ZydisEncoderNopFill(buffer == null ? &emptyBuffer : buffer,
                (ulong)destination.Length)).ThrowIfFailed("NOP fill");
        }
    }

    public static byte[] CreateNops(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        var result = new byte[length];
        FillNops(result);
        return result;
    }

    public static string GetMnemonicName(Core.ZydisMnemonic_ mnemonic) => CopyName(Core.ZydisMnemonicGetString(mnemonic));
    public static string GetRegisterName(Core.ZydisRegister_ register) => CopyName(Core.ZydisRegisterGetString(register));

    private static string CopyName(byte* name)
    {
        if (name == null) throw new ArgumentOutOfRangeException(nameof(name), "Invalid upstream name identifier.");
        // The pointer is into upstream's immutable generated string table. Copy
        // while synchronously rooted inside this call, never retain the address.
        int length = 0;
        while (name[length] != 0) ++length;
        return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(name, length));
    }

    private static Core.ZydisStackWidth_ DefaultStackWidth(Core.ZydisMachineMode_ machineMode) => machineMode switch
    {
        Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_64 => Core.ZydisStackWidth_.ZYDIS_STACK_WIDTH_64,
        Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LONG_COMPAT_32 or Core.ZydisMachineMode_.ZYDIS_MACHINE_MODE_LEGACY_32 => Core.ZydisStackWidth_.ZYDIS_STACK_WIDTH_32,
        _ => Core.ZydisStackWidth_.ZYDIS_STACK_WIDTH_16
    };
}

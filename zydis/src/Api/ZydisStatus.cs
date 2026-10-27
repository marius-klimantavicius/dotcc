#nullable enable
using System;

namespace Managed.Zydis;

/// <summary>The original upstream ZyanStatus, including its module and error bits.</summary>
public readonly record struct ZydisStatus(uint Value)
{
    // Zycore/Status.h: ZYAN_SUCCESS tests that bit 31 is clear. This is the
    // upstream status representation, not a copied status-code enumeration.
    public bool IsSuccess => unchecked((int)Value) >= 0;
    public bool IsFailure => !IsSuccess;

    public void ThrowIfFailed(string operation)
    {
        if (IsFailure) throw new ZydisException(operation, this);
    }

    public override string ToString() => $"0x{Value:X8}";
}

/// <summary>An upstream failure. Try methods expose the identical status without throwing.</summary>
public sealed class ZydisException : Exception
{
    public ZydisStatus Status { get; }
    public int? BufferOffset { get; }

    public ZydisException(string operation, ZydisStatus status, int? bufferOffset = null)
        : base(bufferOffset is int offset
            ? $"Zydis {operation} failed at buffer offset {offset}: {status}."
            : $"Zydis {operation} failed: {status}.")
    {
        Status = status;
        BufferOffset = bufferOffset;
    }
}

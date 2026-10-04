#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

/// <summary>Owns storage backing the existing 24-byte unmanaged VaList ABI.</summary>
/// <remarks>
/// The workspace pointer must remain stable until disposal (translated code
/// passes stackalloc storage). Large packs allocate native memory instead.
/// Value and va_copy cursors borrow this lifetime. Do not copy an owning VaPack;
/// repeated Dispose calls on the same variable are safe.
/// </remarks>
public unsafe ref struct VaPack
{
    public const int MaximumPayloadBytes = VaArg.MaximumPayloadBytes;
    public const int StackLimitBytes = 64 * 1024;
    private byte* nativeStorage;
    private VaList value;

    public readonly VaList Value => value;
    public readonly bool UsesNativeMemory => nativeStorage != null;

    private static int Measure(ReadOnlySpan<VaArg> arguments, out int alignment, out int payloadBytes)
    {
        alignment = 8;
        long size = 0;
        foreach (var argument in arguments)
        {
            VaArg.ValidateLayout(argument.Size, argument.Alignment);
            alignment = Math.Max(alignment, argument.Alignment);
            size = (size + argument.Alignment - 1) & -(long)argument.Alignment;
            size += argument.Size;
            if (size > MaximumPayloadBytes)
                throw new ArgumentOutOfRangeException(nameof(arguments), "Variadic pack exceeds 64 MiB.");
        }
        payloadBytes = (int)size;
        // Empty packs still have a valid Begin address for va_start/va_copy.
        return Math.Max(payloadBytes, 1) + alignment - 1;
    }

    public static int GetStackBytes(ReadOnlySpan<VaArg> arguments)
    {
        int bytes = Measure(arguments, out _, out _);
        return bytes <= StackLimitBytes ? bytes : 0;
    }

    public VaPack(ReadOnlySpan<VaArg> arguments, byte* workspace, int workspaceBytes)
    {
        nativeStorage = null;
        value = default;
        int bytes = Measure(arguments, out int alignment, out int payloadBytes);
        byte* raw;
        if (bytes <= StackLimitBytes && workspace != null && workspaceBytes >= bytes)
            raw = workspace;
        else
        {
            raw = (byte*)NativeMemory.Alloc((nuint)bytes);
            if (raw == null) throw new OutOfMemoryException();
            nativeStorage = raw;
        }
        try
        {
            byte* data = (byte*)(((nuint)raw + (nuint)alignment - 1) & ~((nuint)alignment - 1));
            byte* next = data;
            foreach (var argument in arguments)
            {
                VaArg.ValidateLayout(argument.Size, argument.Alignment);
                nuint address = ((nuint)next + (nuint)argument.Alignment - 1) & ~((nuint)argument.Alignment - 1);
                // Bounds also protect against a caller concurrently replacing
                // descriptors in an array-backed input span between passes.
                if (address < (nuint)data || address > (nuint)data + (nuint)payloadBytes)
                    throw new ArgumentException("Variadic arguments changed while their pack was being constructed.");
                next = (byte*)address;
                argument.CopyTo(next, payloadBytes - checked((int)(next - data)));
                next += argument.Size;
            }
            value = new VaList { Begin = data, Next = data, End = next };
        }
        catch
        {
            if (nativeStorage != null) NativeMemory.Free(nativeStorage);
            nativeStorage = null;
            throw;
        }
    }

    public void Dispose()
    {
        byte* owned = nativeStorage;
        nativeStorage = null;
        value = default;
        if (owned != null) NativeMemory.Free(owned);
    }
}

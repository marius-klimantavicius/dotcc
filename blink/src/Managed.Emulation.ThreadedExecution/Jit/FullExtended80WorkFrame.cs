using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// R123: independent storage, never an overlay on MachineFpu or FullBlockFrame.
// The first ten bytes are the little-endian architectural binary80 encoding.
[StructLayout(LayoutKind.Explicit, Size = 16)]
internal struct FullExtended80
{
    [FieldOffset(0)] internal ulong Significand;
    [FieldOffset(8)] internal ushort SignExponent;
    [FieldOffset(10)] internal ushort Reserved10;
    [FieldOffset(12)] internal uint Reserved12;
    internal FullExtended80(ulong significand, ushort signExponent)
    { this = default; Significand = significand; SignExponent = signExponent; }
}

internal enum FullExtended80Operation : byte { Add, Subtract, Multiply, Divide }
internal enum FullExtended80Admission : uint
{
    Complete = 0, BadVersion = 1, BadSize = 2, ReservedPrecision = 3,
    Reserved4 = 4, EqualPayloadOppositeSignNaNUnqualified = 5
}

[System.Flags]
internal enum FullExtended80Classification : uint
{
    None = 0, NormalValid = 1, WrappedValid = 2,
    TinyBeforeRangeRounding = 4, Overflow = 8,
    NormalExact = 16, PreOperationException = 32,
    NaN = 64, Infinity = 128
}

// Caller must supply an exclusive, readable/writable 384-byte region even when
// Size is wrong. Version/Size are protocol checks, not pointer validation.
// Inputs are values, so Left==Right aliases and output-to-input reuse between
// invocations are legal. Neither input nor caller-owned reserved bytes change.
[StructLayout(LayoutKind.Explicit, Size = ByteSize)]
internal unsafe struct FullExtended80WorkFrame
{
    internal const uint CurrentVersion = 1;
    internal const int ByteSize = 384;
    [FieldOffset(0)] internal uint Version;
    [FieldOffset(4)] internal uint Size;
    [FieldOffset(8)] internal ushort ControlWord;
    [FieldOffset(10)] internal ushort Reserved10;
    [FieldOffset(12)] internal FullExtended80Admission Admission;
    [FieldOffset(16)] internal FullExtended80 Left;
    [FieldOffset(32)] internal FullExtended80 Right;
    [FieldOffset(48)] internal FullExtended80 Normal;
    [FieldOffset(64)] internal FullExtended80 Wrapped;
    // Exception flags 0..5 and C1 bit9 ONLY. These are deltas, never a full SW.
    // Normal assumes OE/UE masked. Wrapped uses unbounded precision rounding.
    // With unmasked pre-operation IE/DE/ZE parent must suppress completion.
    [FieldOffset(80)] internal ushort NormalStatus;
    [FieldOffset(82)] internal ushort WrappedStatus;
    [FieldOffset(84)] internal FullExtended80Classification Classification;
    // Used only by TryEmitContinuation: native address in same owning lifetime.
    [FieldOffset(88)] internal ulong Continuation;
    // Private native scratch [96,256); reserved caller storage [256,384).
    [FieldOffset(96)] internal fixed byte NativeScratch[160];
    [FieldOffset(256)] internal fixed byte Reserved256[128];
}

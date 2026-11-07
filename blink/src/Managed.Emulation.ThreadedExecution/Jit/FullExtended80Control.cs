using System.Runtime.InteropServices;

namespace Managed.Emulation.Execution;

// Compile-time selector only. Pop counts are documentary: parent owns TOP/tags.
internal enum FullExtended80Control : byte
{
    Fcom, Fcomp, Fcompp, Fucom, Fucomp, Fucompp,
    Fcomi, Fcomip, Fucomi, Fucomip, RoundToIntegral
}
internal enum FullExtended80Relation : uint { Greater, Less, Equal, Unordered }

// Cast Admission to this enum for control kernels; no WorkFrame ABI changes.
internal enum FullExtended80ControlAdmission : uint
{
    Complete = 0, BadVersion = 1, BadSize = 2
}

// Normal for comparisons ONLY; round uses the existing FullExtended80 instead.
// This is an image in Normal, not an overlay on guest state or another frame.
// Write masks describe mask-sensitive contributions. Denormal comparisons use
// AMD primary/parent hardware semantics; Intel omission is scoped in D/API.md.
[StructLayout(LayoutKind.Explicit, Size = 16)]
internal struct FullExtended80CompareResult
{
    [FieldOffset(0)] internal FullExtended80Relation Relation;
    [FieldOffset(4)] internal ushort StatusBits;
    [FieldOffset(6)] internal ushort StatusWriteMask;
    [FieldOffset(8)] internal uint EFlagsBits;
    [FieldOffset(12)] internal uint EFlagsWriteMask;
}

// WorkFrame384/v1 CONTROL interpretation; generic arithmetic/conversion
// Classification consumers MUST NOT interpret these bits. Wrapped is zero.
// NormalStatus = fully-masked IE/DE/PE/C1 delta; WrappedStatus = actual-mask
// priority event delta, NEVER exponent wrapping. Compare DE completes condition
// contributions even when unmasked, unlike round DE (see D/API.md).
[System.Flags]
internal enum FullExtended80ControlMetadata : uint
{
    None = 0, Comparison = 1, ValueValid = 2, Exact = 4,
    Invalid = 8, Denormal = 16, Precision = 32, RoundedMagnitudeUp = 64,
    SuppressCondition = 128, SuppressDestination = 256,
    CompleteThenPrecisionTrap = 512, DenormalComparison = 1024, SuppressPop = 2048
}

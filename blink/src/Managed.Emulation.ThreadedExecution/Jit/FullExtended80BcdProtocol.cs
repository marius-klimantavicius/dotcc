namespace Managed.Emulation.Execution;

// R152: separate selector/protocol. Never extend or reinterpret the R129 enum.
internal enum FullExtended80BcdOperation : byte { LoadPackedBcd, StorePackedBcd }

// Cast WorkFrame.Admission to this operation-specific protocol, not arithmetic.
internal enum FullExtended80BcdAdmission : uint
{
    Complete = 0, BadVersion = 1, BadSize = 2,
    // Intel/AMD define no numeric result for A..F digit nibbles. This is NOT IE.
    UndefinedPackedBcdDigits = 3
}

// Cast Classification84 to this enum. Numeric completion only; no memory/pop.
// NormalStatus80 is fully masked. WrappedStatus82 is the actual-mask event
// delta (including C1), NOT an exponent-wrapped arithmetic status.
[System.Flags]
internal enum FullExtended80BcdMetadata : uint
{
    None = 0, ValueValid = 1, Exact = 2, Invalid = 8,
    Precision = 128, RoundedMagnitudeUp = 256,
    SuppressDestinationAndPop = 512, CompleteThenPrecisionTrap = 1024
}

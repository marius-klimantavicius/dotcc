namespace Managed.Emulation.Execution;

// Compile-time selector. No runtime operation field or shared frame ABI change.
internal enum FullExtended80Conversion : byte
{
    LoadBinary32, LoadBinary64, LoadInt16, LoadInt32, LoadInt64,
    StoreBinary32, StoreBinary64, StoreInt16, StoreInt32, StoreInt64,
    TruncateInt16, TruncateInt32, TruncateInt64,
    LoadRaw80, StoreRaw80,
    PackedBcdLoadMissing, PackedBcdStoreMissing
}

// WorkFrame384/v1 conversion interpretation; cast Classification to this enum.
// Generic arithmetic Classification consumers MUST NOT interpret these bits.
// Normal has binary80 for loads/raw; otherwise its low 2/4/8 bytes contain the
// little-endian destination image, zero extended to 16 bytes. Wrapped is zero.
// NormalStatus = fully masked outcome; WrappedStatus = mask-sensitive event
// delta for the parent completion layer (NOT a wrapped exponent result).
[System.Flags]
internal enum FullExtended80ConversionMetadata : uint
{
    None = 0, ValueValid = 1, Exact = 2, RawCopy = 4,
    Invalid = 8, DenormalLoad = 16, Tiny = 32, Overflow = 64,
    Precision = 128, RoundedMagnitudeUp = 256,
    // These describe numeric completion after memory/stack/pending checks.
    SuppressDestination = 512, CompleteThenPrecisionTrap = 1024,
    CompleteThenDenormalTrap = 2048
}

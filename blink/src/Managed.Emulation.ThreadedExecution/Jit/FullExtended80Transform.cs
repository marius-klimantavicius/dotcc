namespace Managed.Emulation.Execution;

// Compile-time only. Parent owns stack push and all guest state effects.
internal enum FullExtended80Transform : byte { Sqrt, Extract }
internal enum FullExtended80TransformAdmission : uint
{ Complete = 0, BadVersion = 1, BadSize = 2, ReservedPrecision = 3 }

// WorkFrame384/v1 transform interpretation ONLY. See D/API.md.
// Extract Wrapped = NEW ST1 exponent value, NEVER arithmetic exponent wrapping.
// NormalStatus = fully-masked delta; WrappedStatus = actual priority delta.
// Suppressed images are zero; parent must honor the valid/suppression bits.
[System.Flags]
internal enum FullExtended80TransformMetadata : uint
{
    None = 0, Sqrt = 1, Extract = 2, NormalValid = 4, ExponentValid = 8,
    Exact = 16, Invalid = 32, Denormal = 64, ZeroDivide = 128,
    Precision = 256, RoundedMagnitudeUp = 512, SuppressDestination = 1024,
    SuppressPush = 2048, CompleteThenPrecisionTrap = 4096, C1Defined = 8192
}

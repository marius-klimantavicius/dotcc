namespace Managed.Emulation.Execution;

// Compile-time specialized entries, appended in the ONE parent catalog batch.
// 66 Scale,67 Remainder,68 RemainderNearest. No runtime operation field.
internal enum FullExtended80Reduction : byte { Scale, Remainder, RemainderNearest }

// Work384/v1 uses the existing arithmetic Normal/Wrapped/Classification images.
// All PC values ignored; full64 precision. RC affects Scale range rounding only.
// Additional operation-specific OUTPUT in private scratch (never caller reserve):
// 240 u16 CC bits,242 u16 CC write mask,244 u32 kind(1 Scale,2 Rem,4 RemNearest).
// Scale CC mask0200, bits0; selected status contributes its C1. Remainder ordinary
// mask4700; complete quotient modulo8 maps Q2/Q1/Q0 to C0/C3/C1, C2=0. Partial
// reduction follows the explicit AMD rule N=32+(D mod32), C2=1, other CC0.
// NaN/invalid/pre-operation suppression mask0200/bits0, undefined CC retained.
// Integer-native, all XMM preserved, R11 Work -> JMP [Work88]. Inputs/control,
// continuation and ALL256..383 preserved. Parent owns guest stack/metadata/MF.
internal static class FullExtended80ReductionProtocol
{
    internal const byte ScaleKey = 66, RemainderKey = 67, RemainderNearestKey = 68;
    internal const int ConditionBits = 240, ConditionMask = 242, Kind = 244;
}

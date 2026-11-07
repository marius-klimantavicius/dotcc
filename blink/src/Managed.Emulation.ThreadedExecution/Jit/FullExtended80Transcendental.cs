namespace Managed.Emulation.Execution;

// Emission-time selectors; no runtime operation field or semantic callback.
internal enum FullExtended80Transcendental : byte
{ F2xm1, Fyl2x, Fptan, Fpatan, Fyl2xp1, Fsincos, Fsin, Fcos }

[System.Flags]
internal enum FullExtended80TranscendentalMetadata : uint
{
    // Low byte is operation+1, not a generic arithmetic classification.
    NormalValid = 0x100, SecondValid = 0x200, Suppress = 0x400,
    OutOfRange = 0x800, CompleteThenPrecisionTrap = 0x1000,
    HostApproximation = 0x2000, CapturedActual = 0x4000, UndefinedCopyProfile = 0x8000, IntegerNative = 0x10000
}

internal static class FullExtended80TranscendentalProtocol
{
    internal const byte F2xm1Key=69, Fyl2xKey=70, FptanKey=71, FpatanKey=72,
        Fyl2xp1Key=73, FsincosKey=74, FsinKey=75, FcosKey=76;
    internal const int ProposedMaximumKernels=77;
    // Portable activation derives from the actual software numerical certificate
    // and owner/platform gates. The historical host-state blocker below remains
    // a separate OPEN finding; it is never waived or used as this backend.
    internal static bool GuestActivationReady =>
        FullCanonicalX87Activation.Blockers == FullCanonicalX87ActivationBlocker.None;
    internal const bool HostBackendActivationReady = false; // historical private-host foundation remains unqualified
    internal const string HostStateBoundaryBlocker = "R165UniversalHostX87StateRestorationMISSING";
    internal const uint UnsupportedHostCompletion=257;
    internal const int ConditionBits=232, ConditionMask=234, HostDiagnostic=236;
    // 236=5: captured logical TOP differs from completed7 (unary/binary) or6
    // (pair). This is an unsupported host-profile/protocol result, not guest
    // exception semantics and never an active fallback/termination route.
}

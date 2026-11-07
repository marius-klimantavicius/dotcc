namespace Managed.Emulation.Execution;

// Source candidate policy; qualification of exact numerical code, runtime
// correspondence and combined owner gates is a separate, mandatory obligation.
internal static class FullExtended80TranscendentalSoftwareProfile
{
    internal const bool Qualified = true;
    internal const string Id = "R182.Binary80.Gpr256.TruePi.FullQuotient.SineC1";
    internal const string Blocker = "R182FiniteSoftwareProfileAccepted;WindowsAndAsyncUnqualified";
    internal const int ScratchStart = 96, ScratchBytes = 160, PrecisionBits = 256;
    internal const uint ComputationMetadata = 0x10000;
    internal const int ExponentBias = 16383, RangeAdjustment = 24576;
    // Numerical envelope BEFORE final binary80 rounding; n=0 trig also retains
    // the small result's scale. See R182 proof/ERROR.md for source derivations.
    internal const int Expm1RelativeBits = 230, LogRelativeBits = 230,
        Log1pRelativeBits = 230, AtanRelativeBits = 230, TrigAbsoluteBits = 180,
        LargeTangentRelativeBits = 90;
    internal const bool TruePi = true, PrecisionControlIgnored = true;
    internal const bool FsincosC1FromSine = true, FptanC1FromTangent = true;
    // sNaN raises IE even alongside qNaN. Quiet preferred; same-class larger
    // payload; left tie sign; unsupported yields indefinite. NaN suppresses DE.
    internal const string NaNSelection = "UnsupportedIndefinite;IEOnAnySNaN;QuietFirst;LargerPayload;LeftTie";
    // Approximation cells set PE even if final candidate residual is zero.
    // Exact anchors use exact integer residuals; C1 is significand increment of
    // the actual candidate's final rounding, or masked infinity saturation.
    internal const string PrecisionPolicy = "ForcedOnApproximation;ExactResidualOnAnchors;ActualRoundingIncrementC1";
    // The immutable owner catalog supplies numerical trust. Runtime independently
    // validates raw/class/domain/special/shape/mask consistency and the retained
    // R165 exact anchors. Metadata alone authenticates neither code nor maths.
    internal const string NumericalTrust = "QualifiedImmutableTypedOwnerEntry;NoRuntimeGeneralReevaluation";
}

namespace Managed.Emulation.Execution;

[Flags]
internal enum FullCanonicalX87ActivationBlocker
{
    None = 0,
    StateOwnerNotBuilt = 1,
    PlatformUnverified = 2,
    NumericalAcceptanceDomainUnqualified = 4
}

// Production policy, not an execution-time per-op dispatch or a test receipt.
// c117 qualifies the isolated Linux family mechanisms. It does not establish
// numerical acceptance for arbitrary guest operands. This response implements
// the explicit PC01 and finite selected Linux context/helper policies.
internal static class FullCanonicalX87Activation
{
#if BLINK_FULL_X87_STATE_OWNER
    internal const bool StateOwnerBuilt = true;
#else
    internal const bool StateOwnerBuilt = false;
#endif
    internal const string PrecisionControlPolicy =
        "PC00=24;PC10=53;PC11=64;PC01=64SoftwareProfile;ConversionsAndRawTransportPreservePC01";
    internal const string Environment16Policy =
        "LinuxSoftwareProtected16RestoreOmittedFopZero;NotUniversalHardwarePrescription";
    internal const string ContextPolicy =
        "SelectedLinuxLongProtected;TFAndRFPreservedNoSingleStep;ACAllowedIfAM0;EligibleACAndOtherModesOutsideProfile";
    internal const string ReservedEncodingPolicy =
        "LinuxCanonicalReservedUD;54AdoptedAliasesRetained;31UnadoptedComparisonAliasesUD;SelectedObsoleteDBControlsUD";

    internal static FullCanonicalX87ActivationBlocker Blockers =>
        Evaluate(StateOwnerBuilt, OperatingSystem.IsLinux());

    // The environmental inputs cannot grant numerical or context qualification.
    // This pure relation is also used by the table-driven startup contracts.
    internal static FullCanonicalX87ActivationBlocker Evaluate(bool stateOwnerBuilt, bool linux)
    {
        var blockers = FullCanonicalX87ActivationBlocker.None;
        if (!stateOwnerBuilt) blockers |= FullCanonicalX87ActivationBlocker.StateOwnerNotBuilt;
        if (!linux) blockers |= FullCanonicalX87ActivationBlocker.PlatformUnverified;
        if (!FullExtended80TranscendentalSoftwareProfile.Qualified)
            blockers |= FullCanonicalX87ActivationBlocker.NumericalAcceptanceDomainUnqualified;
        return blockers;
    }

    internal static void RequireQualified()
    {
        var blockers = Blockers;
        if (blockers != FullCanonicalX87ActivationBlocker.None)
            throw new NotSupportedException("Canonical x87 production activation blocked: " + blockers +
                "; numerical profile=" + FullExtended80TranscendentalSoftwareProfile.Id +
                "; " + FullExtended80TranscendentalSoftwareProfile.Blocker);
    }
}

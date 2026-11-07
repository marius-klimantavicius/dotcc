namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // Explicitly synthetic version exhaustion; no production mutation hook.
    internal void TestSetMappingSequence203(ulong value) => mappingSequence = value;
    internal void TestSetMappingEpoch203(ulong value) => mappingAuthorityEpoch = value;
}

internal sealed unsafe partial class FullTranslationJit
{
    // Source-only deterministic work-cap exhaustion at the actual mapper seam.
    internal int? TestMappingBudget203;
    partial void MappingRevocationBudget(ref int remaining)
    { if (TestMappingBudget203 is int cap) remaining = Math.Min(remaining, cap); }
}

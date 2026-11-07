namespace Managed.Emulation.Execution;

// Contract assembly only; none of these seams is compiled into production.
internal sealed unsafe partial class FullCodeAuthority
{
    internal void TestTopologyExhaustion204(bool last = false) => mappingTopologyEpoch = last ? ulong.MaxValue - 1 : ulong.MaxValue;
}
internal sealed unsafe partial class FullWorkerNativeState
{
    internal ulong TestTopologyStamp204 => cachedTopologyEpoch;
}

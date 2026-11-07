namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullWorkerNativeState
{
    internal Action<FullCodeAuthority>? TestMappingPreserving200;
    partial void MappingCachePreserving(FullCodeAuthority authority)
        => TestMappingPreserving200?.Invoke(authority);
}

internal sealed unsafe partial class FullCodeAuthority
{
    internal void TestMappingSaturate200() { lock (Gate) Saturate(); }
}

public sealed unsafe partial class ThreadedGuestExecution
{
    internal sealed partial class FullControllerFixture
    {
        internal bool PrepareMapping200(FullBlockFrame* frame, bool reservation, bool ticket, out bool discarded)
            => Owner.PrepareNativeMappingCache(testWorkers[0], frame, reservation, ticket, out discarded);
    }
}

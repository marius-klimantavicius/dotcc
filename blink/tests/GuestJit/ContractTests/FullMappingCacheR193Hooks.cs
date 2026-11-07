namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullWorkerNativeState
{
    internal Action<FullCodeAuthority>? TestMappingRetagged;
    internal (ulong Mapping, ulong Runtime) TestMappingStamp => (cachedMappingEpoch, cachedRuntimeEpoch);
    partial void MappingCacheRetagged(FullCodeAuthority authority)
    {
        var action = TestMappingRetagged; TestMappingRetagged = null;
        action?.Invoke(authority);
    }
}

internal sealed unsafe partial class FullCodeAuthority
{
    // Counter edges are unreachable in a practical run, but must not wrap to
    // initial worker stamps. Contract-assembly-only fault injection.
    internal void TestMappingExhaustion() => mappingAuthorityEpoch = ulong.MaxValue;
}

public sealed unsafe partial class ThreadedGuestExecution
{
    internal sealed partial class FullControllerFixture
    {
        internal FullWorkerNativeState MappingState193 => testWorkers[0].FullNativeState!;
        internal void MappingReadOnly193(ulong address)
        {
            Select(0); using var scope = new FullDataScope(Owner, testWorkers[0]);
            Require(Program.ProtectVirtual(Owner.system, (long)address, 4096,
                1, false) == 0, "R193 real read-only mapping");
        }
    }
}

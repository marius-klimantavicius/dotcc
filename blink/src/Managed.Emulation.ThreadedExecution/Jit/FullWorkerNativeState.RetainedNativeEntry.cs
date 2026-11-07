namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullWorkerNativeState
{
    // Worker-owned stamps. Read-only: a stale runtime (even a mapping-neutral
    // reader pulse) must use R193's Gate-held retiming/discard protocol. This
    // cannot retag hints or promote a ticket, proof or Ready descriptor.
    internal bool RetainedMappingCacheCurrent(FullCodeAuthority authority, ulong runtime)
    {
        ulong topology = authority.MappingTopologyEpoch, mapping = authority.MappingAuthorityEpoch;
        return storage != null && runtime != 0 && runtime != ulong.MaxValue &&
            mapping != 0 && mapping != ulong.MaxValue && topology != 0 && topology != ulong.MaxValue &&
            cachedTopologyEpoch == topology && cachedRuntimeEpoch == runtime &&
            cachedMappingEpoch == mapping && authority.RetainedNativeEntryOpen &&
            authority.MappingCacheRuntimeEpoch == runtime && authority.MappingAuthorityEpoch == mapping &&
            authority.MappingTopologyEpoch == topology;
    }
}

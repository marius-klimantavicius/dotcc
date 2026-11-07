namespace Managed.Emulation.Execution;

internal sealed unsafe partial class FullCodeAuthority
{
    // An observation, never an execution permit. No registry/backing lookup and
    // no eligibility minting. The actual SC enrollment handshake remains the
    // authority; a later writer/reader advances runtime and drains that count.
    internal bool RetainedNativeEntryOpen => !Volatile.Read(ref saturated) &&
        Volatile.Read(ref epochExhausted) == 0 && Volatile.Read(ref mappingWriters) == 0 &&
        Volatile.Read(ref hostWriters) == 0 && Volatile.Read(ref copyWriters) == 0 &&
        PendingAtomicWorker == -1 && Volatile.Read(ref nativeTicketWorker) == -1 && SemanticReadersDrained();
}

namespace Managed.Emulation.Execution;

// Worker-local routing information, never executable authority. No allocation,
// descriptor, owner reference or native address escapes the validating gate.
internal struct JitEntryHint
{
    internal long Owner;
    internal ulong Ip, Generation;
    internal int Slot;
    internal bool HasData;
}

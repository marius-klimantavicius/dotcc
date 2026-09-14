namespace Managed.Emulation.Host;
public sealed partial class InstanceIo
{
    /// <summary>Atomically replace one descriptor within this owner. Source and
    /// target share an open description; descriptor flags remain per-number.</summary>
    public HostResult<int> DuplicateTo(int source, int target, bool closeOnExecFlag = false, bool rejectSame = false)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if (rejectSame && source == target) return Fail<int>(GuestError.Invalid);
            if (!Find(source, out var description)) return Fail<int>(GuestError.BadDescriptor);
            if (target < 0 || target >= descriptorLimit) return Fail<int>(GuestError.BadDescriptor);
            if (source == target) return HostResult<int>.Success(target);
            // Keep the source alive even when target already shares its description.
            ++description.References;
            if (descriptors.ContainsKey(target)) Close(target); // dup2 silently discards close errors.
            descriptors[target] = description;
            if (closeOnExecFlag) closeOnExec.Add(target); else closeOnExec.Remove(target);
            return HostResult<int>.Success(target);
        }
    }
}

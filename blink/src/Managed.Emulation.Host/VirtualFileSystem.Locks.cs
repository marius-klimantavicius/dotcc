namespace Managed.Emulation.Host;

public sealed partial class VirtualFileSystem
{
    private readonly Dictionary<Description, int> advisoryLocks = new();

    /// <summary>Private inode/open-description advisory locks. A contended
    /// blocking request is outside this profile and fails without waiting.</summary>
    public HostResult<int> AdvisoryLock(int descriptor, int operation)
    {
        lock (sync)
        {
            if (!TryDescription(descriptor, out var current)) return Fail<int>(GuestError.BadDescriptor);
            int kind = operation & ~4;
            if ((operation & ~15) != 0 || kind is not (1 or 2 or 8)) return Fail<int>(GuestError.Invalid);
            // Closing the last duplicate releases its description's locks.
            // Pruning before every acquisition bounds retained entries by the
            // descriptor ceiling, without a second descriptor lifetime table.
            var live = descriptors.Values.ToHashSet();
            foreach (var old in advisoryLocks.Keys.Where(d => !live.Contains(d)).ToArray()) advisoryLocks.Remove(old);
            if (kind == 8) { advisoryLocks.Remove(current); return HostResult<int>.Success(0); }
            if (advisoryLocks.TryGetValue(current, out int held) && held == kind) return HostResult<int>.Success(0);
            bool conflict = advisoryLocks.Any(pair => !ReferenceEquals(pair.Key, current) &&
                ReferenceEquals(pair.Key.Node, current.Node) && (kind == 2 || pair.Value == 2));
            // Unsupported blocking waits do not begin a conversion. Native
            // nonblocking conversion removes the old lock before retrying.
            if (conflict && (operation & 4) == 0) return Fail<int>(GuestError.Unsupported);
            advisoryLocks.Remove(current);
            if (conflict) return Fail<int>(GuestError.Again);
            advisoryLocks.Add(current, kind);
            return HostResult<int>.Success(0);
        }
    }
}

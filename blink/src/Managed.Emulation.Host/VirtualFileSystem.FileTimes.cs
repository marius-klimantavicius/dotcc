namespace Managed.Emulation.Host;

/// <summary>Exact POSIX time over the DateTime calendar range, with nanosecond
/// storage. Provider-generated current times retain their 100 ns quantum.</summary>
public readonly record struct VirtualFileTime(long Seconds, int Nanoseconds)
{
    public const long MinimumSeconds = -62135596800, MaximumSeconds = 253402300799;
    public bool IsValid => Seconds >= MinimumSeconds && Seconds <= MaximumSeconds
        && Nanoseconds >= 0 && Nanoseconds < 1_000_000_000;
    public long Ticks => checked(DateTime.UnixEpoch.Ticks + Seconds * TimeSpan.TicksPerSecond + Nanoseconds / 100);
    public int Subtick => Nanoseconds % 100;
    public static VirtualFileTime UtcNow {
        get {
            long ticks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
            return new(ticks / TimeSpan.TicksPerSecond, (int)(ticks % TimeSpan.TicksPerSecond * 100));
        }
    }
}

public enum FileTimeSelection { Explicit, Now, Omit }
public readonly record struct VirtualFileTimeUpdate(FileTimeSelection Selection, VirtualFileTime Time = default)
{
    public bool IsValid => Selection is FileTimeSelection.Now or FileTimeSelection.Omit
        || Selection == FileTimeSelection.Explicit && Time.IsValid;
}

public sealed partial class VirtualFileSystem
{
    public HostResult<int> SetTimes(int descriptor, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify)
    {
        lock (sync) return TryDescription(descriptor, out var file)
            ? SetNodeTimes(file.Node, access, modify) : Fail<int>(GuestError.BadDescriptor);
    }

    public HostResult<int> SetTimes(string path, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify, string cwd = "/")
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            var resolved = Resolve(path, cwd);
            if (!resolved.Succeeded) return Fail<int>(resolved.Error);
            return files.TryGetValue(resolved.Value, out var node) || directoryNodes.TryGetValue(resolved.Value, out node)
                ? SetNodeTimes(node, access, modify) : Fail<int>(GuestError.NoEntry);
        }
    }

    private static HostResult<int> SetNodeTimes(Node node, VirtualFileTimeUpdate access, VirtualFileTimeUpdate modify)
    {
        // Validate the pair before touching any metadata. OMIT preserves both
        // the requested field and, when selected twice, the change timestamp.
        if (!access.IsValid || !modify.IsValid) return Fail<int>(GuestError.Invalid);
        if (access.Selection == FileTimeSelection.Omit && modify.Selection == FileTimeSelection.Omit)
            return HostResult<int>.Success(0);
        if (node.Immutable) return Fail<int>(GuestError.ReadOnly);
        var now = VirtualFileTime.UtcNow;
        if (access.Selection != FileTimeSelection.Omit)
            node.AccessTime = access.Selection == FileTimeSelection.Now ? now : access.Time;
        if (modify.Selection != FileTimeSelection.Omit)
            node.ModifyTime = modify.Selection == FileTimeSelection.Now ? now : modify.Time;
        node.ChangeTime = now;
        return HostResult<int>.Success(0);
    }
}

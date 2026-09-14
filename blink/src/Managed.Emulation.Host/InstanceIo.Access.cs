namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    /// <summary>Private virtual-UID-zero access policy, using the shared path
    /// resolver and actual file modes. It does not reserve future I/O capacity.</summary>
    public HostResult<int> AccessAt(int directory, string path, int mode, int flags = 0)
    {
        lock (sync)
        {
            if (disposed) return Fail<int>(GuestError.BadDescriptor);
            if ((mode & ~7) != 0) return Fail<int>(GuestError.Invalid);
            if ((flags & ~(256 | 512)) != 0) return Fail<int>(GuestError.Unsupported);
            var result = StatAt(directory, path);
            if (!result.Succeeded) return Fail<int>(result.Error);
            var entry = result.Value;
            if ((mode & 2) != 0 && entry.Immutable && !entry.Directory) return Fail<int>(GuestError.ReadOnly);
            if ((mode & 1) != 0 && !entry.Directory && (entry.Mode & 0x49) == 0) return Fail<int>(GuestError.Access);
            return HostResult<int>.Success(0);
        }
    }
}

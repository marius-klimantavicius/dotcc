namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public sealed class DirectoryLease
    {
        internal readonly object Description;
        internal readonly InstanceIo Owner;
        internal readonly int Fd;
        internal bool Released;
        internal IReadOnlyList<VirtualDirectoryEntry> Snapshot;
        public IReadOnlyList<VirtualDirectoryEntry> Entries => Snapshot;
        internal DirectoryLease(InstanceIo owner, object description, int fd, VirtualDirectoryEntry[] entries)
        { Owner=owner; Description=description; Fd=fd; Snapshot=Array.AsReadOnly(entries); }
    }
    public HostResult<DirectoryLease> AcquireDirectory(int fd, int entryLimit, int nameBytesLimit)
    {
        lock (sync)
        {
            if (!Find(fd, out var description)) return Fail<DirectoryLease>(GuestError.BadDescriptor);
            if (description.Kind != Kind.File) return Fail<DirectoryLease>(GuestError.NotDirectory);
            var snapshot=files.DirectorySnapshot(description.Handle,entryLimit,nameBytesLimit);
            if (!snapshot.Succeeded) return Fail<DirectoryLease>(snapshot.Error);
            var lease=new DirectoryLease(this,description,fd,snapshot.Value);
            ++description.References; // Internal reference, not another guest descriptor.
            return HostResult<DirectoryLease>.Success(lease);
        }
    }
    public HostResult<int> DirectoryDescriptor(DirectoryLease lease)
    {
        lock (sync) return ReferenceEquals(lease.Owner,this) && !lease.Released && Find(lease.Fd,out var current) && ReferenceEquals(current,lease.Description)
            ? HostResult<int>.Success(lease.Fd) : Fail<int>(GuestError.BadDescriptor);
    }
    public HostResult<int> ReleaseDirectory(DirectoryLease lease, bool closeOriginal = true)
    {
        lock (sync)
        {
            if (!ReferenceEquals(lease.Owner,this) || lease.Released) return Fail<int>(GuestError.BadDescriptor);
            lease.Released=true;
            lease.Snapshot=Array.Empty<VirtualDirectoryEntry>();
            var description=(Description)lease.Description;
            bool current=Find(lease.Fd,out var found) && ReferenceEquals(found,description);
            if (closeOriginal && current) Close(lease.Fd);
            if (--description.References == 0) files.Close(description.Handle);
            return !closeOriginal || current ? HostResult<int>.Success(0) : Fail<int>(GuestError.BadDescriptor);
        }
    }
}

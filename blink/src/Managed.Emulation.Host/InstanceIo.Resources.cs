namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    /// <summary>Actual descriptor-table capacity, including standard streams.
    /// Values do not shrink as descriptors are acquired. A disposed owner has
    /// no queryable table and returns EBADF.</summary>
    public HostResult<int> DescriptorCapacity()
    {
        lock (sync) return disposed ? Fail<int>(GuestError.BadDescriptor)
            : HostResult<int>.Success(descriptorLimit);
    }
}

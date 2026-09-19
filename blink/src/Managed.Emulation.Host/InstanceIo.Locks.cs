namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public HostResult<int> AdvisoryLock(int descriptor, int operation)
    {
        lock (sync) return !Find(descriptor, out var description) ? Fail<int>(GuestError.BadDescriptor)
            : description.Kind == Kind.File ? files.AdvisoryLock(description.Handle, operation)
            : Fail<int>(GuestError.Unsupported);
    }
}

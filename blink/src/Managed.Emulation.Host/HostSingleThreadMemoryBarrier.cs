namespace Managed.Emulation.Host;

/// <summary>Private membarrier contract for a guest with exactly one execution
/// thread and no concurrent guest-memory executor. The BCL operation fences
/// the host process; this does not pin memory or qualify guest thread ownership.</summary>
public sealed class HostSingleThreadMemoryBarrier : IHostMemoryBarrier
{
    public const int Query = 0;
    public const int PrivateExpedited = 8;
    public const int RegisterPrivateExpedited = 16;
    public const int SupportedCommands = PrivateExpedited | RegisterPrivateExpedited;
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    private bool capabilityChecked;
    private GuestError capabilityError;
    public bool IsOwnerThread => Environment.CurrentManagedThreadId == ownerThread;
    public bool Registered { get; private set; }
    public ulong FenceCount { get; private set; }
    public int CapabilityFenceCount { get; private set; }

    private static GuestError Fence()
    {
        try { Interlocked.MemoryBarrierProcessWide(); return GuestError.None; }
        catch (NotSupportedException) { return GuestError.Unsupported; }
        catch (Exception) { return GuestError.Io; }
    }

    private GuestError EnsureCapability()
    {
        if (!capabilityChecked)
        {
            capabilityError = Fence();
            capabilityChecked = true;
            if (capabilityError == GuestError.None) ++CapabilityFenceCount;
        }
        return capabilityError;
    }

    public HostResult<int> Invoke(int command, uint flags, int cpuId)
    {
        if (!IsOwnerThread) return HostResult<int>.Failure((GuestError)1); // EPERM
        if (flags != 0) return HostResult<int>.Failure(GuestError.Invalid);
        // Linux ignores cpu_id for these commands when flags is zero.
        switch (command)
        {
            case Query:
                var queryError = EnsureCapability();
                if (queryError != GuestError.None) return HostResult<int>.Failure(queryError);
                return HostResult<int>.Success(SupportedCommands);
            case RegisterPrivateExpedited:
                var registrationError = EnsureCapability();
                if (registrationError != GuestError.None) return HostResult<int>.Failure(registrationError);
                Registered = true;
                return HostResult<int>.Success(0);
            case PrivateExpedited:
                if (!Registered) return HostResult<int>.Failure((GuestError)1);
                if (FenceCount == ulong.MaxValue) return HostResult<int>.Failure((GuestError)75); // EOVERFLOW
                var error = Fence();
                if (error != GuestError.None) return HostResult<int>.Failure(error);
                ++FenceCount;
                return HostResult<int>.Success(0);
            default:
                return HostResult<int>.Failure(GuestError.Invalid);
        }
    }
}

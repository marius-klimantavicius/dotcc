namespace Managed.Emulation.Host;

public interface IHostMemoryBarrier
{
    bool IsOwnerThread { get; }
    HostResult<int> Invoke(int command, uint flags, int cpuId);
}

/// <summary>Private registration shared only by explicitly attached execution
/// threads. The actual BCL fence covers the host process. Dispose after every
/// attached guest worker has stopped and detached.</summary>
public sealed class HostProcessMemoryBarrier : IHostMemoryBarrier, IDisposable
{
    private readonly object sync = new();
    private readonly HashSet<int> attached = new();
    private readonly int creator = Environment.CurrentManagedThreadId;
    private bool disposed, capabilityChecked, registered;
    private GuestError capabilityError;
    private ulong fences;
    private int capabilityFences;

    public bool IsOwnerThread { get { lock (sync) return !disposed && attached.Contains(Environment.CurrentManagedThreadId); } }
    public bool Registered { get { lock (sync) return registered; } }
    public ulong FenceCount { get { lock (sync) return fences; } }
    public int CapabilityFenceCount { get { lock (sync) return capabilityFences; } }
    public int Attachments { get { lock (sync) return attached.Count; } }

    public void AttachCurrentThread()
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (!attached.Add(Environment.CurrentManagedThreadId))
                throw new InvalidOperationException("Memory barrier context is already attached on this thread.");
        }
    }
    public void DetachCurrentThread()
    {
        lock (sync)
        {
            if (!attached.Remove(Environment.CurrentManagedThreadId))
                throw new InvalidOperationException("Memory barrier context is not attached on this thread.");
        }
    }
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
            if (capabilityError == GuestError.None) ++capabilityFences;
        }
        return capabilityError;
    }
    public HostResult<int> Invoke(int command, uint flags, int cpuId)
    {
        lock (sync)
        {
            if (disposed || !attached.Contains(Environment.CurrentManagedThreadId))
                return HostResult<int>.Failure((GuestError)1);
            if (flags != 0) return HostResult<int>.Failure(GuestError.Invalid);
            // Linux ignores cpu_id for this explicitly supported zero-flags set.
            switch (command)
            {
                case HostSingleThreadMemoryBarrier.Query:
                    var queryError = EnsureCapability();
                    return queryError == GuestError.None
                        ? HostResult<int>.Success(HostSingleThreadMemoryBarrier.SupportedCommands)
                        : HostResult<int>.Failure(queryError);
                case HostSingleThreadMemoryBarrier.RegisterPrivateExpedited:
                    var registrationError = EnsureCapability();
                    if (registrationError != GuestError.None) return HostResult<int>.Failure(registrationError);
                    registered = true;
                    return HostResult<int>.Success(0);
                case HostSingleThreadMemoryBarrier.PrivateExpedited:
                    if (!registered) return HostResult<int>.Failure((GuestError)1);
                    if (fences == ulong.MaxValue) return HostResult<int>.Failure((GuestError)75);
                    var error = Fence();
                    if (error != GuestError.None) return HostResult<int>.Failure(error);
                    ++fences;
                    return HostResult<int>.Success(0);
                default:
                    return HostResult<int>.Failure(GuestError.Invalid);
            }
        }
    }
    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            if (Environment.CurrentManagedThreadId != creator || attached.Count != 0)
                throw new InvalidOperationException("Only the creator may release a fully detached memory barrier context.");
            disposed = true;
        }
    }
}

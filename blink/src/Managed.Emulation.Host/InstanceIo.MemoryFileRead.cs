namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    private IHostMemoryFileRead? memoryFileRead;

    public void BindMemoryFileRead(IHostMemoryFileRead observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (memoryFileRead != null && !ReferenceEquals(memoryFileRead, observer))
                throw new InvalidOperationException("A memory-file read observer is already bound.");
            memoryFileRead = observer;
        }
    }

    public void UnbindMemoryFileRead(IHostMemoryFileRead observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (sync)
        {
            if (!ReferenceEquals(memoryFileRead, observer))
                throw new InvalidOperationException("The memory-file read observer does not match.");
            memoryFileRead = null;
        }
    }
}

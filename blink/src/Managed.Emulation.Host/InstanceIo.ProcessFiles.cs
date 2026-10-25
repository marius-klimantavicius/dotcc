namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    /// <summary>Install a bounded, immutable guest maps snapshot. The caller
    /// captures mappings before entering this owner's gate. Payload and node
    /// storage are charged to the existing combined memory-file budget.</summary>
    public HostResult<int> OpenProcessMaps(ReadOnlySpan<byte> snapshot, bool closeOnExecFlag = false,
        CancellationToken cancellation = default)
    {
        lock (sync)
        {
            if (snapshot.Length > 1 << 20) return Fail<int>(GuestError.NoMemory);
            var opened = CreateMemoryFile("guest-process-maps", closeOnExecFlag ? 1u : 0u, cancellation);
            if (!opened.Succeeded) return opened;
            bool installed = false;
            try
            {
                var description = descriptors[opened.Value];
                var written = WriteMemoryFile(description.MemoryFile!, snapshot, 0);
                if (!written.Succeeded) return Fail<int>(written.Error);
                description.StatusFlags = 0;
                description.ProcessSnapshot = true;
                installed = true;
                return opened;
            }
            finally { if (!installed) Close(opened.Value); }
        }
    }
}

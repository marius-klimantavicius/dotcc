namespace Managed.Emulation.Worker;

internal static class WorkerProfile
{
    internal const int ImageBytes = 2 * 1024 * 1024;
    internal const int MemoryBytes = 64 * 1024 * 1024;
    internal const int MaximumGuestWorkers = 16;
    internal const int ShutdownReserveMilliseconds = 1000;

    internal static InstanceOptions Snapshot(InstanceOptions options)
    {
        var copy = options.Snapshot();
        // The owning execution API currently fixes these limits. Never accept
        // another value while silently running with the owner's fixed setting.
        if (copy.MemoryLimit != MemoryBytes || copy.WorkingDirectory != "/" ||
            copy.DescriptorLimit > 128 || copy.PublishedPorts.Length != 1 ||
            copy.WallClockMilliseconds < 2000 || copy.Arguments.Length == 0)
            throw new ArgumentException("Worker profile requires 64 MiB guest memory, cwd /, at most 128 descriptors, one published port, nonempty argv, and at least 2 seconds wall time.");
        return copy;
    }
}

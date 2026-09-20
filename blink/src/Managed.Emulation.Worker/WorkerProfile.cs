namespace Managed.Emulation.Worker;

internal static class WorkerProfile
{
    internal const int ImageBytes = 16 * 1024 * 1024;
    internal const int MemoryBytes = 64 * 1024 * 1024;
    internal const int KestrelMemoryBytes = 128 * 1024 * 1024;
    internal const int MaximumGuestWorkers = 16;
    internal const int ShutdownReserveMilliseconds = 1000;

    internal static InstanceOptions Snapshot(InstanceOptions options)
    {
        var copy = options.Snapshot();
        // Forward one of the selected owner limits; never accept a value and
        // silently execute with a different memory ceiling.
        if ((copy.MemoryLimit != MemoryBytes && copy.MemoryLimit != KestrelMemoryBytes) || copy.WorkingDirectory != "/" ||
            copy.DescriptorLimit > 128 || copy.PublishedPorts.Length != 1 ||
            copy.WallClockMilliseconds < 2000 || copy.Arguments.Length == 0)
            throw new ArgumentException("Worker profile requires 64 or 128 MiB guest address-space/backing limits, cwd /, at most 128 descriptors, one published port, nonempty argv, and at least 2 seconds wall time.");
        return copy;
    }
}

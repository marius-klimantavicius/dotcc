using Managed.Emulation.Host;

namespace Managed.Emulation;

internal static class MachineDevices
{
    // Built-in devices are recreated for each worker and never copied into
    // persistent private storage or exported as regular image files.
    internal static void Mount(MountedFileSystem fileSystem, int descriptorLimit)
    {
        var devices = VirtualFileSystem.CreateDeviceFileSystem(descriptorLimit);
        try { fileSystem.Mount("/dev", devices, readOnly: true, ownsSource: true); }
        catch { devices.Dispose(); throw; }
    }
}

namespace Managed.Emulation.Host;

public sealed partial class VirtualFileSystem
{
    /// <summary>A private, read-only device namespace for mounting at /dev.
    /// Only urandom is provided. Entropy uses the existing BCL provider; no host
    /// path is opened, and no byte stream is retained as an image or snapshot.</summary>
    public static VirtualFileSystem CreateDeviceFileSystem(int descriptorLimit = 128)
    {
        var devices = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(),
            writableLimit: 0, descriptorLimit: descriptorLimit, imageLimit: 0, nodeLimit: 2);
        devices.ReserveImageName("/urandom");
        devices.files.Add("/urandom", new Node([], true, devices.nextInode++, 0x2000 | 0x124)
        {
            Entropy = new HostEnvironment()
        });
        return devices;
    }
}

using Managed.Emulation.Host;

static class EntropyChecks
{
    public static async Task Run()
    {
        using var root = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var mounted = new MountedFileSystem(root);
        mounted.Mount("/dev", VirtualFileSystem.CreateDeviceFileSystem(), readOnly: true, ownsSource: true);
        await using var io = new InstanceIo(mounted);
        var stat = Ok(io.Stat("/dev/urandom"));
        Check((stat.Mode & 0xf000) == 0x2000 && stat.Immutable && !stat.Directory && stat.Length == 0, "character device metadata");
        int directory = Ok(io.OpenFile("/dev", FileAccessMode.Read, allowDirectory: true, requireDirectory: true));
        int fd = Ok(io.OpenFileAt(directory, "./urandom", FileAccessMode.Read, closeOnExecFlag: true));
        Check(Ok(io.FStat(fd)).Inode == stat.Inode && Ok(io.GetDescriptorFlags(fd)) == 1, "openat/fstat/cloexec device");
        int duplicate = Ok(io.Duplicate(fd));
        Ok(io.Close(fd));
        byte[] first = new byte[64], second = new byte[64];
        Check(Ok(await io.ReadAsync(duplicate, first)) == first.Length, "read from surviving duplicate");
        Check(Ok(await io.ReadAsync(duplicate, second)) == second.Length && !first.SequenceEqual(second), "fresh entropy on next read");
        Check(Ok(await io.ReadAsync(duplicate, Memory<byte>.Empty)) == 0, "zero-length entropy read");
        byte[] large = new byte[1024];
        int count = Ok(await io.ReadAsync(duplicate, large));
        Check(count == HostEnvironment.MaximumEntropyChunk, "bounded entropy short read");
        var readable = Ok(await io.PollAsync([new(duplicate, 1)], 0));
        Check(readable.Count == 1 && readable.Events[0] == 1, "entropy ready for reading");
        int deviceDirectory = Ok(mounted.Open("/dev", FileAccessMode.Read, allowDirectory: true));
        Check(Ok(mounted.DirectorySnapshot(deviceDirectory, 8, 256)).Any(e => e.Name == "urandom" && e.Type == 2), "directory reports character device");
        Ok(mounted.Close(deviceDirectory));
        Check(mounted.WritableBytes == 0, "entropy has no stored payload");
        Ok(io.Close(directory));
        Ok(io.Close(duplicate));
        Check(io.OpenDescriptors == 3 && mounted.OpenDescriptors == 0, "device handles closed");
        Console.WriteLine("PASS BCL virtual urandom read/stat/openat/dup/short-read/poll/lifetime");
    }

    private static T Ok<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception($"Entropy operation: {result.Error}");
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
}

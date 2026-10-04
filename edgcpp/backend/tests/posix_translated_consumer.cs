#nullable enable
using System;
using System.IO;
using Edg.Runtime;
using Posix.Integration;
internal static unsafe class Program
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    public static int Main()
    {
#if INSTANCE
        using var first = new Core();
        using var second = new Core();
        int firstFd, secondFd;
        fixed (byte* path = "owner-first.bin\0"u8) firstFd = first.TestLeave((sbyte*)path);
        fixed (byte* path = "owner-second.bin\0"u8) secondFd = second.TestLeave((sbyte*)path);
        PosixContext a = first.TestRuntime, b = second.TestRuntime;
        Check(firstFd >= 0 && secondFd >= 0 && a.OpenDescriptorCount == 1 && b.OpenDescriptorCount == 1, "independent owner descriptors");
        Check(a.MappedViewCount == 1 && b.MappedViewCount == 1, "independent owner mappings");
        first.Dispose();
        Check(a.OpenDescriptorCount == 0 && a.MappedViewCount == 0, "first Dispose closes resources");
        byte value;
        Check(b.OpenDescriptorCount == 1 && b.MappedViewCount == 1 && b.Pread(secondFd, &value, 1, 0) == 1 && value == 77, "other owner remains usable");
        second.Shutdown();
        Check(b.OpenDescriptorCount == 0 && b.MappedViewCount == 0, "second Shutdown closes resources");
        File.Delete("owner-first.bin"); File.Delete("owner-second.bin");
#else
        int fd;
        fixed (byte* path = "owner-static.bin\0"u8) fd = Core.TestLeave((sbyte*)path);
        PosixContext runtime = Core.TestRuntime;
        Check(fd >= 0 && runtime.OpenDescriptorCount == 1 && runtime.MappedViewCount == 1, "static owner resources");
        Core.Shutdown();
        Check(runtime.OpenDescriptorCount == 0 && runtime.MappedViewCount == 0, "static Shutdown closes resources");
        File.Delete("owner-static.bin");
#endif
        Console.WriteLine("POSIX translated owner lifecycle passed");
        return 0;
    }
}

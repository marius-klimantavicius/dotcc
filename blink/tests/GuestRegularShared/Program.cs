using Managed.Emulation.Host;
using Managed.Emulation;
using System.Runtime.CompilerServices;

static class Program
{
    static T Value<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception(result.Error.ToString());
    static void Check(bool value, [CallerLineNumber] int line = 0) { if (!value) throw new Exception("regular shared invariant at " + line); }
    static unsafe byte Byte(MemoryFilePage page, int offset = 0) => ((byte*)page.Address)[offset];
    static int Create(InstanceIo io, string path, long length)
    {
        int fd = Value(io.OpenFile(path, FileAccessMode.Read | FileAccessMode.Write, create: true, exclusive: true));
        Value(io.TruncateFile(fd, length)); return fd;
    }
    static void Release(params MemoryFilePage[] pages)
    { foreach (var page in pages) Value(InstanceIo.ReleaseMemoryFilePage(page.Lease)); }

    static unsafe void Bridge(InstanceIo io)
    {
        int fd = Create(io, "/bridge", 4096);
        Value(io.WriteAt(fd, new byte[] { 81 }, 0));
        int reader = Value(io.OpenFile("/bridge", FileAccessMode.Read));
        Blink.io = io; Blink.ioCancellation = default;
        Check(Blink.blink_io_mapping_begin(reader) == 0 && Blink.blink_io_shared_file_readonly(reader) == 2);
        Value(io.Close(reader));
        int reused = Create(io, "/bridge-new", 4096); Check(reused == reader);
        void* address; ulong lease;
        Blink.ioCancellation = new CancellationToken(true);
        Check(Blink.blink_io_shared_file_page(reader, 0, &address, &lease) == -125);
        Blink.ioCancellation = default;
        Check(Blink.blink_io_shared_file_page(reader, 0, &address, &lease) == 0 && *(byte*)address == 81);
        Check(Blink.blink_io_mapping_end() == 0);
        Blink.io = null;
        Check(Blink.blink_io_shared_file_available(lease) == 1 && Blink.blink_io_shared_file_release(lease) == 0);
        Value(io.Close(fd)); Value(io.Close(reused)); Check(io.MemoryFilesUsage().Bytes == 0);
    }

    static async Task Coherence()
    {
        using var backing = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>> { ["/immutable"] = new byte[] { 99 } }, writableLimit: 1 << 20, descriptorLimit: 32);
        using var mounted = new MountedFileSystem(backing);
        await using var io = new InstanceIo(mounted, descriptorLimit: 12);
        int fd = Create(io, "/a", 8196);
        Value(io.WriteAt(fd, new byte[] { 71 }, 4101));
        int reader = Value(io.OpenFile("/a", FileAccessMode.Read)), writer = Value(io.OpenFile("/a", FileAccessMode.Write));
        Check(Value(io.SharedFileReadOnly(reader)) == 2 && !Value(io.FStat(reader)).Immutable);
        var a = Value(io.AcquireMemoryFilePage(reader, 4096));
        long firstBytes = io.MemoryFilesUsage().Bytes;
        var b = Value(io.AcquireMemoryFilePage(fd, 4096));
        Check(a.Address == b.Address && a.Lease != b.Lease && Byte(a, 5) == 71);
        Check(io.MemoryFilesUsage().Bytes == firstBytes + 128 && io.OpenDescriptors == 6);
        Check(io.AcquireMemoryFilePage(writer, 4096).Error == GuestError.BadDescriptor);
        Value(io.WriteAt(writer, new byte[] { 72 }, 4101)); Check(Byte(a, 5) == 72 && Byte(b, 5) == 72);
        Value(io.Seek(writer, 4102, SeekOrigin.Begin));
        Value(await io.WriteAsync(writer, new byte[] { 73 })); Check(Byte(a, 6) == 73);
        int append = Value(io.OpenFile("/a", FileAccessMode.Write, append: true));
        var end = Value(io.AcquireMemoryFilePage(reader, 8192));
        Value(await io.WriteAsync(append, new byte[] { 74 })); Check(Byte(end, 4) == 74);
        Value(io.WriteAt(append, new byte[] { 75 }, 0)); Check(Byte(end, 5) == 75);
        var tail = Value(io.AcquireMemoryFilePage(reader, 16384));
        Check(!InstanceIo.MemoryFilePageAvailable(tail.Lease));
        Value(io.TruncateFile(writer, 20480)); Check(InstanceIo.MemoryFilePageAvailable(tail.Lease) && Byte(tail) == 0);
        Value(io.WriteAt(writer, new byte[] { 76 }, 16384)); Check(Byte(tail) == 76);
        Value(io.TruncateFile("/a", 8193));
        Check(!InstanceIo.MemoryFilePageAvailable(tail.Lease) && Byte(end, 4) == 0 && Byte(tail) == 0);
        Value(io.TruncateFile(writer, 20480)); Check(Byte(tail) == 0 && Byte(end, 5) == 0);
        int truncated = Value(io.OpenFile("/a", FileAccessMode.Write, truncate: true));
        Check(!InstanceIo.MemoryFilePageAvailable(a.Lease) && Byte(a, 5) == 0);
        Value(io.TruncateFile(writer, 8196));
        Value(io.WriteAt(writer, new byte[] { 77 }, 4101)); Check(Byte(b, 5) == 77);
        // Last alias frees a resident page; next map reads the current file, not stale cache bytes.
        Release(a, b);
        var reloaded = Value(io.AcquireMemoryFilePage(reader, 4096)); Check(Byte(reloaded, 5) == 77);
        var transaction = Value(io.BeginMapping(reader));
        Value(io.Close(reader));
        int reused = Create(io, "/replacement", 8192); Check(reused == reader);
        Value(io.WriteAt(reused, new byte[] { 91 }, 4101));
        var pinned = Value(io.MappingMemoryFilePage(transaction, 4096));
        Check(pinned.Address == reloaded.Address && Byte(pinned, 5) == 77);
        Value(io.EndMapping(transaction));
        Value(io.RenameAt(-100, "/a", -100, "/moved"));
        Value(io.RenameAt(-100, "/replacement", -100, "/moved"));
        var independent = Value(io.AcquireMemoryFilePage(reused, 4096));
        Check(independent.Address != pinned.Address && Byte(independent, 5) == 91);
        Value(io.UnlinkAt(-100, "/moved"));
        Value(io.WriteAt(writer, new byte[] { 78 }, 4101)); Check(Byte(pinned, 5) == 78 && Byte(independent, 5) == 91);
        Value(io.Close(fd)); Value(io.Close(writer)); Value(io.Close(append)); Value(io.Close(truncated)); Value(io.Close(reused));
        Check(io.OpenDescriptors == 3 && backing.OpenDescriptors == 2); // Original internal readers retained, no guest fd slots.
        Check(Byte(pinned, 5) == 78 && Byte(independent, 5) == 91);
        Release(end, tail, reloaded, pinned, independent);
        Check(io.MemoryFilesUsage().Bytes == 0 && io.MemoryFilesUsage().Files == 0 && backing.OpenDescriptors == 0);
        int immutable = Value(io.OpenFile("/immutable", FileAccessMode.Read));
        Check(Value(io.SharedFileReadOnly(immutable)) == 1 && io.AcquireMemoryFilePage(immutable, 0).Error == GuestError.Unsupported);
        Value(io.Close(immutable));
        Bridge(io);
    }

    static async Task Bounds()
    {
        using var backing = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(), descriptorLimit: 32);
        await using var io = new InstanceIo(backing, descriptorLimit: 8);
        Value(io.SetMemoryFileLimit(6000));
        int fd = Create(io, "/bounded", 16384);
        Check(io.AcquireMemoryFilePage(fd, 0, new CancellationToken(true)).Error == GuestError.Canceled && io.MemoryFilesUsage().Bytes == 0);
        ulong budget = Value(io.BindMemoryFileBudget(10000, 5000));
        Check(io.AcquireMemoryFilePage(fd, 0).Error == GuestError.NoMemory && io.MemoryFilesUsage().Bytes == 0 && backing.OpenDescriptors == 1);
        Value(InstanceIo.AccountMemoryFileBudget(budget, 0));
        var a = Value(io.AcquireMemoryFilePage(fd, 0));
        var b = Value(io.AcquireMemoryFilePage(fd, 0));
        long charge = io.MemoryFilesUsage().Bytes;
        Check(io.AcquireMemoryFilePage(fd, 4096).Error == GuestError.NoMemory && io.MemoryFilesUsage().Bytes == charge);
        Value(io.WriteAt(fd, new byte[] { 51 }, 0)); Check(Byte(a) == 51 && Byte(b) == 51);
        Release(a, b);
        Check(io.MemoryFilesUsage().Bytes == 0 && backing.OpenDescriptors == 1);
        Check(io.AcquireMemoryFilePage(fd, 1).Error == GuestError.Invalid);
        Value(InstanceIo.AccountMemoryFileBudget(budget, 0, release: true));
        var held = Value(io.AcquireMemoryFilePage(fd, 0));
        await io.DisposeAsync();
        Check(backing.OpenDescriptors == 0 && Byte(held) == 51 && InstanceIo.MemoryFilePageAvailable(held.Lease));
        Release(held); Check(io.MemoryFilesUsage().Bytes == 0);
        Check(InstanceIo.ReleaseMemoryFilePage(held.Lease).Error == GuestError.Invalid);
        using var nodeBacking = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(), descriptorLimit: 32);
        await using var nodes = new InstanceIo(nodeBacking, descriptorLimit: 4);
        var retained = new List<MemoryFilePage>();
        for (int i = 0; i < 4; ++i)
        {
            int number = Create(nodes, "/node" + i, 4096);
            retained.Add(Value(nodes.AcquireMemoryFilePage(number, 0))); Value(nodes.Close(number));
        }
        int last = Create(nodes, "/node-last", 4096);
        Check(nodes.AcquireMemoryFilePage(last, 0).Error == GuestError.NoSpace);
        foreach (var page in retained) Release(page);
        Check(nodes.MemoryFilesUsage().Bytes == 0); Value(nodes.Close(last));
    }

    static async Task NamespaceIdentity()
    {
        using var left = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var right = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var mounted = new MountedFileSystem(left); mounted.Mount("/right", right);
        await using var io = new InstanceIo(mounted);
        int a = Create(io, "/a", 4096), b = Create(io, "/right/b", 4096);
        Check(Value(io.FStat(a)).Inode != Value(io.FStat(b)).Inode);
        var pa = Value(io.AcquireMemoryFilePage(a, 0)); var pb = Value(io.AcquireMemoryFilePage(b, 0));
        Value(io.WriteAt(a, new byte[] { 12 }, 0)); Value(io.WriteAt(b, new byte[] { 34 }, 0));
        Check(pa.Address != pb.Address && Byte(pa) == 12 && Byte(pb) == 34);
        Release(pa, pb); Value(io.Close(a)); Value(io.Close(b));
    }

    static async Task LargeFile()
    {
        const int length = 14 * 1024 * 1024 + 17;
        using var backing = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(), writableLimit: 20 << 20);
        await using var io = new InstanceIo(backing);
        int fd = Create(io, "/large", length);
        Value(io.WriteAt(fd, new byte[] { 17 }, length - 1));
        int reader = Value(io.OpenFile("/large", FileAccessMode.Read));
        var first = new List<MemoryFilePage>(); var second = new List<MemoryFilePage>();
        for (long offset = 0; offset < length; offset += 4096) first.Add(Value(io.AcquireMemoryFilePage(reader, offset)));
        long payload = io.MemoryFilesUsage().Bytes;
        for (long offset = 0; offset < length; offset += 4096)
        {
            var alias = Value(io.AcquireMemoryFilePage(reader, offset));
            Check(alias.Address == first[(int)(offset / 4096)].Address); second.Add(alias);
        }
        Check(io.MemoryFilesUsage().Bytes == payload + second.Count * 128 && io.MemoryFilesUsage().Bytes < 17 << 20);
        Value(io.WriteAt(fd, new byte[] { 18 }, length - 1));
        Check(Byte(first[^1], 16) == 18 && Byte(second[^1], 16) == 18 && Byte(first[^1], 17) == 0);
        foreach (var page in first) Release(page);
        foreach (var page in second) Release(page);
        Check(io.MemoryFilesUsage().Bytes == 0); Value(io.Close(fd)); Value(io.Close(reader));
    }

    static async Task HostBacked()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "regular-host-store"); Directory.CreateDirectory(root);
        try
        {
            using var backing = HostDirectoryFileSystem.OpenRoot(root, descriptorLimit: 16, writableLimit: long.MaxValue);
            using var mounted = new MountedFileSystem(backing);
            await using var io = new InstanceIo(mounted);
            // A sparse logical regular file must not allocate its logical extent.
            int fd = Create(io, "/sparse", 2L << 40);
            Value(io.WriteAt(fd, new byte[] { 52 }, 1L << 32));
            int reader = Value(io.OpenFile("/sparse", FileAccessMode.Read));
            var a = Value(io.AcquireMemoryFilePage(reader, 1L << 32));
            Check(Byte(a) == 52 && io.MemoryFilesUsage().Bytes == 1024 + 4224 + 128);
            Value(io.WriteAt(fd, new byte[] { 53 }, 1L << 32)); Check(Byte(a) == 53);
            Value(io.TruncateFile("/sparse", 0)); Check(!InstanceIo.MemoryFilePageAvailable(a.Lease) && Byte(a) == 0);
            Value(io.TruncateFile(fd, 2L << 40)); Check(InstanceIo.MemoryFilePageAvailable(a.Lease) && Byte(a) == 0);
            Value(io.WriteAt(fd, new byte[] { 54 }, 4096));
            var transaction = Value(io.BeginMapping(reader));
            Value(io.Close(fd)); Value(io.Close(reader)); Value(io.UnlinkAt(-100, "/sparse"));
            int reused = Create(io, "/sparse", 8192);
            Value(io.WriteAt(reused, new byte[] { 91 }, 4096));
            var old = Value(io.MappingMemoryFilePage(transaction, 4096));
            Check(Byte(old) == 54);
            Value(io.EndMapping(transaction));
            var independent = Value(io.AcquireMemoryFilePage(reused, 4096)); Check(Byte(independent) == 91);
            Release(a, old, independent); Value(io.Close(reused));
            Check(io.MemoryFilesUsage().Bytes == 0 && backing.OpenDescriptors == 0);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    static async Task Main()
    {
        await Coherence(); await Bounds(); await NamespaceIdentity(); await LargeFile(); await HostBacked();
        Console.WriteLine("managed regular shared: coherence/identity/EOF/quotas/cancel/disposal: PASS");
    }
}

namespace Managed.Emulation
{
    public static partial class Blink
    {
        internal static InstanceIo? io;
        internal static CancellationToken ioCancellation;
        private static int IoError(int error) => -error;
        private static int IoException(Exception error) => error is OutOfMemoryException ? -12 : -5;
        private static long IoResult<T>(HostResult<T> result) where T : System.Numerics.INumber<T>
            => result.Succeeded ? long.CreateChecked(result.Value) : -(int)result.Error;
    }
}

using Managed.Emulation.Host;
using Managed.Emulation;

static class Program
{
    static T Value<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception(result.Error.ToString());
    static void Check(bool condition) { if (!condition) throw new Exception("invariant failed"); }
    static unsafe void Put(MemoryFilePage page, int offset, byte value) => ((byte*)page.Address)[offset] = value;
    static unsafe byte Get(MemoryFilePage page, int offset) => ((byte*)page.Address)[offset];
    static unsafe void Bridge(InstanceIo io)
    {
        Blink.io = io;
        byte* name = stackalloc byte[250];
        for (int i = 0; i < 249; ++i) name[i] = 0xff;
        name[249] = 0; // Valid arbitrary Linux byte string, not UTF8.
        int fd = Blink.blink_host_memfd_create(name, 1);
        Check(fd >= 0);
        Value(io.TruncateFile(fd, 4096));
        void* address;
        ulong lease;
        Check(Blink.blink_io_shared_file_page(fd, 0, &address, &lease) == 0 && address != null);
        Blink.io = null;
        Check(Blink.blink_io_shared_file_available(lease) == 1);
        Check(Blink.blink_io_shared_file_release(lease) == 0); // Works unbound.
        Value(io.Close(fd));
    }
    static async Task Main()
    {
        await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/regular"] = new byte[1] }, descriptorLimit: 8);
        Value(io.SetMemoryFileLimit(32768));
        var canceled = new CancellationToken(true);
        Check(io.CreateMemoryFile("canceled", 0, canceled).Error == GuestError.Canceled);
        Check(io.MemoryFilesUsage().Bytes == 0);
        int fd = Value(io.CreateMemoryFile("doublemapper", 1));
        Value(io.TruncateFile(fd, 2L << 40));
        Check(Value(io.FStat(fd)).Length == 2L << 40 && io.MemoryFilesUsage().Bytes == 1024);
        Check(Value(io.GetDescriptorFlags(fd)) == 1 && Value(io.GetStatusFlags(fd)) == 2);
        int duplicate = Value(io.Duplicate(fd));
        Check(Value(io.GetDescriptorFlags(duplicate)) == 0);
        Value(io.Seek(fd, 4096, SeekOrigin.Begin));
        Check(Value(io.Seek(duplicate, 0, SeekOrigin.Current)) == 4096);
        var a = Value(io.AcquireMemoryFilePage(fd, 4096));
        var b = Value(io.AcquireMemoryFilePage(duplicate, 4096));
        Check(a.Address == b.Address && a.Lease != b.Lease && ((long)a.Address & 4095) == 0);
        Put(a, 7, 42);
        byte[] buffer = new byte[16];
        Check(Value(io.ReadAt(duplicate, buffer, 4096)) == 16 && buffer[7] == 42 && Get(b, 7) == 42);
        Value(io.WriteAt(fd, new byte[] { 73 }, 4103));
        Check(Get(a, 7) == 73);
        Value(io.TruncateFile(fd, 4096));
        Check(!InstanceIo.MemoryFilePageAvailable(a.Lease) && Get(a, 7) == 0);
        Value(io.TruncateFile(fd, 8192));
        Check(InstanceIo.MemoryFilePageAvailable(a.Lease) && Get(b, 7) == 0);
        Put(a, 10, 88);
        Value(io.TruncateFile(fd, 4104));
        Check(Get(b, 10) == 0 && InstanceIo.MemoryFilePageAvailable(a.Lease));
        Value(io.TruncateFile(fd, 8192));
        Check(Get(a, 10) == 0);
        Check(io.AcquireMemoryFilePage(fd, 0, canceled).Error == GuestError.Canceled);
        Value(io.Close(fd));
        int reused = Value(io.CreateMemoryFile("new", 0));
        Check(reused == fd);
        Value(io.TruncateFile(reused, 8192));
        var independent = Value(io.AcquireMemoryFilePage(reused, 4096));
        Check(independent.Address != a.Address && Get(independent, 7) == 0);
        Value(io.DuplicateTo(duplicate, reused)); // Closes independent description, retains its mapping.
        Value(io.Close(duplicate));
        Value(io.Close(reused));
        Put(a, 0, 99);
        Check(Get(b, 0) == 99 && Get(independent, 0) == 0);
        Value(InstanceIo.ReleaseMemoryFilePage(a.Lease));
        Check(Get(b, 0) == 99);
        Value(InstanceIo.ReleaseMemoryFilePage(b.Lease));
        Value(InstanceIo.ReleaseMemoryFilePage(independent.Lease));
        Check(io.MemoryFilesUsage().Bytes == 0 && io.MemoryFilesUsage().Files == 0);
        Bridge(io);
        int regular = Value(io.OpenFile("/regular", FileAccessMode.Read));
        Check(Value(io.SharedFileReadOnly(regular)) == 1);
        Check(io.AcquireMemoryFilePage(regular, 0).Error == GuestError.Unsupported);
        Value(io.Close(regular));
        int bounded = Value(io.CreateMemoryFile("bounded", 0));
        Value(io.TruncateFile(bounded, 1L << 40));
        var leases = new List<MemoryFilePage>();
        for (long offset = 0;; offset += 4096)
        {
            var result = io.AcquireMemoryFilePage(bounded, offset);
            if (!result.Succeeded) { Check(result.Error == GuestError.NoMemory); break; }
            leases.Add(result.Value);
        }
        Check(leases.Count > 0 && io.MemoryFilesUsage().Bytes <= io.MemoryFilesUsage().Limit);
        Value(io.Close(bounded));
        foreach (var lease in leases) Value(InstanceIo.ReleaseMemoryFilePage(lease.Lease));
        Check(io.MemoryFilesUsage().Bytes == 0);
        // The C owner and retained sparse storage share the public RAM cap.
        ulong budget = Value(io.BindMemoryFileBudget(16384, 10000));
        int charged = Value(io.CreateMemoryFile("charged", 0));
        Value(io.WriteAt(charged, new byte[] { 17 }, 0));
        var held = Value(io.AcquireMemoryFilePage(charged, 0));
        Value(InstanceIo.ReleaseMemoryFilePage(held.Lease));
        Check(io.WriteAt(charged, new byte[] { 18 }, 4096).Error == GuestError.NoMemory);
        Check(io.MemoryFilesUsage().Bytes == 1024 + 4224); // Payload stays charged after unmap.
        Check(InstanceIo.AccountMemoryFileBudget(budget, 16384).Error == GuestError.NoMemory);
        Value(io.Close(charged));
        Value(InstanceIo.AccountMemoryFileBudget(budget, 16000));
        Check(io.CreateMemoryFile("no-room", 0).Error == GuestError.NoMemory);
        Value(InstanceIo.AccountMemoryFileBudget(budget, 0, release: true));
        await using (var objects = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), descriptorLimit: 4))
        {
            var retainedObjects = new List<MemoryFilePage>();
            for (int i = 0; i < 4; ++i)
            {
                int number = Value(objects.CreateMemoryFile("detached", 0));
                Value(objects.TruncateFile(number, 4096));
                retainedObjects.Add(Value(objects.AcquireMemoryFilePage(number, 0)));
                Value(objects.Close(number));
            }
            Check(objects.CreateMemoryFile("bounded-node", 0).Error == GuestError.NoSpace);
            foreach (var item in retainedObjects) Value(InstanceIo.ReleaseMemoryFilePage(item.Lease));
            Check(objects.MemoryFilesUsage().Bytes == 0);
        }
        int drain = Value(io.CreateMemoryFile("drain", 0));
        Value(io.TruncateFile(drain, 4096));
        var retained = Value(io.AcquireMemoryFilePage(drain, 0));
        await io.DisposeAsync();
        Check(InstanceIo.MemoryFilePageAvailable(retained.Lease));
        Put(retained, 0, 17);
        Value(InstanceIo.ReleaseMemoryFilePage(retained.Lease));
        Check(io.MemoryFilesUsage().Bytes == 0);
        Console.WriteLine("managed sparse memfd lifetime, alias, fd, EOF, quota and cancellation passed");
    }
}

// Compile the authored bridge directly, independent of generated Blink sources.
namespace Managed.Emulation
{
    public static partial class Blink
    {
        internal static InstanceIo? io;
        internal static CancellationToken ioCancellation = default;
        private static int IoError(int error) => -error;
        private static int IoException(Exception exception) => exception is OutOfMemoryException ? -12 : -5;
        private static long IoResult<T>(HostResult<T> result) where T : System.Numerics.INumber<T>
            => result.Succeeded ? long.CreateChecked(result.Value) : -(int)result.Error;
    }
}

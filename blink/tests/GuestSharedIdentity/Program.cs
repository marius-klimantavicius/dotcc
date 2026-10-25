using Managed.Emulation;
using Managed.Emulation.Host;

static class Program
{
    static T Value<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new Exception(result.Error.ToString());
    static void Check(bool value) { if (!value) throw new Exception("mapping identity invariant"); }
    static unsafe byte Byte(MemoryFilePage page) => *(byte*)page.Address;

    static unsafe void Shared(InstanceIo io)
    {
        int a = Value(io.CreateMemoryFile("A", 0)), b = Value(io.CreateMemoryFile("B", 0));
        int target = Value(io.Duplicate(a));
        Check(io.OpenDescriptors == 6); // All guest slots occupied.
        Value(io.TruncateFile(a, 8192)); Value(io.TruncateFile(b, 8192));
        Value(io.WriteAt(a, new byte[] { 65 }, 0)); Value(io.WriteAt(a, new byte[] { 65 }, 4096));
        Value(io.WriteAt(b, new byte[] { 66 }, 0)); Value(io.WriteAt(b, new byte[] { 66 }, 4096));
        Blink.io = io;
        long baseline = io.MemoryFilesUsage().Bytes;
        Check(Blink.blink_io_mapping_begin(target) == 0 && io.OpenDescriptors == 6);
        Check(Blink.blink_io_mapping_begin(target) == -16);
        Check(io.MemoryFilesUsage().Bytes == baseline + 128);
        void* first; ulong firstLease;
        Check(Blink.blink_io_shared_file_page(target, 0, &first, &firstLease) == 0);
        Task.Run(() => { Value(io.DuplicateTo(b, target)); Value(io.Close(a)); }).GetAwaiter().GetResult();
        void* second; ulong secondLease;
        Check(Blink.blink_io_shared_file_page(target, 4096, &second, &secondLease) == 0);
        Check(*(byte*)first == 65 && *(byte*)second == 65);
        byte value = 0; long length;
        Check(Blink.blink_io_mapping_pread(target, &value, 1, 4096) == 1 && value == 65);
        Check(Blink.blink_io_mapping_read_at_length(target, &length) == 0 && length == 8192);
        Check(Blink.blink_io_shared_file_readonly(target) == 0);
        Check(Blink.blink_io_mapping_end() == 0 && Blink.blink_io_mapping_end() == -9);
        byte[] ordinary = new byte[1]; Check(Value(io.ReadAt(target, ordinary, 4096)) == 1 && ordinary[0] == 66);
        Check(InstanceIo.MemoryFilePageAvailable(secondLease));
        Value(InstanceIo.ReleaseMemoryFilePage(firstLease)); Value(InstanceIo.ReleaseMemoryFilePage(secondLease));
        Value(io.Close(b)); Value(io.Close(target));
        Check(io.MemoryFilesUsage().Bytes == 0 && io.MemoryFilesUsage().Files == 0);
        Blink.io = null;
    }

    static unsafe void Immutable(InstanceIo io)
    {
        for (int round = 0; round < 12; ++round)
        {
            int source = Value(io.OpenFile("/A", FileAccessMode.Read));
            Blink.io = io;
            Check(Blink.blink_io_mapping_begin(source) == 0);
            Value(io.Close(source));
            int replacement = Value(io.OpenFile("/B", FileAccessMode.Read)); Check(replacement == source);
            byte value = 0;
            Check(Blink.blink_io_mapping_pread(source, &value, 1, 0) == 1 && value == 65);
            Check(Blink.blink_io_mapping_pread(source, &value, 1, 4096) == 1 && value == 65);
            Check(Blink.blink_io_shared_file_readonly(source) == 1);
            Check(Blink.blink_io_mapping_end() == 0);
            Value(io.Close(replacement)); Check(io.MemoryFilesUsage().Bytes == 0);
            Blink.io = null;
        }
        int snapshot = Value(io.OpenProcessMaps("snapshot\n"u8));
        var captured = Value(io.BeginMapping(snapshot)); Value(io.Close(snapshot));
        Check(io.MappingSharedFileReadOnly(captured).Error == GuestError.Unsupported);
        Check(io.MappingMemoryFilePage(captured, 0).Error == GuestError.Unsupported);
        byte[] buffer = new byte[9]; Check(Value(io.MappingReadAt(captured, buffer, 0)) == 9);
        Value(io.EndMapping(captured)); Check(io.MemoryFilesUsage().Bytes == 0);
    }

    static async Task Main()
    {
        byte[] a = Enumerable.Repeat((byte)65, 8192).ToArray(), b = Enumerable.Repeat((byte)66, 8192).ToArray();
        await using var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/A"] = a, ["/B"] = b }, descriptorLimit: 6);
        Shared(io); Immutable(io);
        int fd = Value(io.CreateMemoryFile("rollback", 0)); Value(io.TruncateFile(fd, 1L << 40));
        Value(io.SetStatusFlags(fd, 2));
        var transaction = Value(io.BeginMapping(fd)); Value(io.Close(fd));
        var page = Value(io.MappingMemoryFilePage(transaction, 0)); Check(Byte(page) == 0);
        Check(io.MappingMemoryFilePage(transaction, 4096, new CancellationToken(true)).Error == GuestError.Canceled);
        Value(InstanceIo.ReleaseMemoryFilePage(page.Lease)); Value(io.EndMapping(transaction));
        Check(io.MemoryFilesUsage().Bytes == 0);
        int regular = Value(io.OpenFile("/A", FileAccessMode.Read));
        var pending = Value(io.BeginMapping(regular)); Value(io.Close(regular));
        await io.DisposeAsync(); Check(io.MemoryFilesUsage().Bytes == 0);
        Check(io.EndMapping(pending).Error == GuestError.BadDescriptor);
        await using (var bounded = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), descriptorLimit: 4))
        {
            ulong token = Value(bounded.BindMemoryFileBudget(8192, 7000));
            int number = Value(bounded.CreateMemoryFile("charged", 0));
            var held = Value(bounded.BeginMapping(number));
            Check(bounded.BeginMapping(number).Error == GuestError.NoMemory);
            Check(bounded.MemoryFilesUsage().Bytes == 1152 && bounded.OpenDescriptors == 4);
            Value(bounded.Close(number)); Value(bounded.EndMapping(held));
            Check(bounded.MemoryFilesUsage().Bytes == 0);
            Value(InstanceIo.AccountMemoryFileBudget(token, 0, release: true));
        }
        Console.WriteLine("managed mapping identity: concurrent dup2/close, immutable pin, quotas, readonly guards, cancel/dispose: PASS");
    }
}

namespace Managed.Emulation
{
    public static partial class Blink
    {
        internal static InstanceIo? io;
        internal static CancellationToken ioCancellation;
        private static int IoError(int error) => -error;
        private static int IoException(Exception exception) => exception is OutOfMemoryException ? -12 : -5;
        private static long IoResult<T>(HostResult<T> result) where T : System.Numerics.INumber<T>
            => result.Succeeded ? long.CreateChecked(result.Value) : -(int)result.Error;
    }
}

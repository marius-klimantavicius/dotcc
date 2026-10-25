using System.Text;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool condition) { if (!condition) throw new Exception("ordinary nonblock assertion"); }
static unsafe int Open(string path, int flags, int at = -100)
{
    byte[] bytes = Encoding.UTF8.GetBytes(path + "\0");
    fixed (byte* p = bytes) return Blink.blink_io_open_at(at, p, flags);
}
var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>> { ["/dotnet/host/fxr/10.0.12/seed"] = "abc"u8.ToArray() });
Blink.BindHostIo(io);
try
{
    int directory = Open("/dotnet/host/fxr", 2048 | 65536 | 524288);
    Check(directory >= 0);
    Check(Blink.blink_io_control(directory, 3, 0) == (2048 | 65536));
    Check(Blink.blink_io_control(directory, 1, 0) == 1);
    var lease = io.AcquireDirectory(directory, 128, 4096);
    Check(lease.Succeeded && lease.Value.Entries.Any(e => e.Name == "10.0.12"));
    Check(io.ReleaseDirectory(lease.Value, closeOriginal: false).Succeeded);
    int duplicate = Blink.blink_io_dup(directory);
    Check(duplicate >= 0 && Blink.blink_io_control(duplicate, 1, 0) == 0);
    Check(Blink.blink_io_control(duplicate, 4, 65536) == 0);
    Check(Blink.blink_io_control(directory, 3, 0) == 65536);
    Check(Blink.blink_io_control(directory, 4, 65536 | 2048) == 0);
    int file = Open("10.0.12/seed", 2048, directory);
    Check(file >= 0 && Blink.blink_io_control(file, 3, 0) == 2048);
    byte[] buffer = new byte[4];
    Check(io.ReadAsync(file, buffer).GetAwaiter().GetResult().Value == 3 && buffer.AsSpan(0, 3).SequenceEqual("abc"u8));
    Check(io.ReadAsync(file, buffer).GetAwaiter().GetResult().Value == 0);
    Check(Blink.blink_io_close(file) == 0);
    int writable = Open("/created", 2 | 64 | 128 | 2048);
    Check(writable >= 0);
    int copy = Blink.blink_io_dup(writable);
    Check(copy >= 0);
    Check(io.WriteAsync(writable, "a"u8.ToArray()).GetAwaiter().GetResult().Value == 1);
    Check(io.Seek(writable, 0, SeekOrigin.Begin).Succeeded);
    Check(Blink.blink_io_control(copy, 4, 2 | 1024 | 2048) == 0);
    Check(io.WriteAsync(copy, "b"u8.ToArray()).GetAwaiter().GetResult().Value == 1);
    Check(Blink.blink_io_control(writable, 3, 0) == (2 | 1024 | 2048));
    Check(Blink.blink_io_control(copy, 4, 0) == 0);
    Check(Blink.blink_io_control(writable, 3, 0) == 2); // Access mode stays O_RDWR.
    Check(io.Seek(writable, 0, SeekOrigin.Begin).Succeeded);
    Check(io.ReadAsync(writable, buffer).GetAwaiter().GetResult().Value == 2 && buffer.AsSpan(0, 2).SequenceEqual("ab"u8));
    foreach (int fd in new[] { directory, duplicate, writable, copy }) Check(Blink.blink_io_close(fd) == 0);
    Console.WriteLine("ordinary nonblock: directory enumeration, read/EOF, append and dup flags: PASS");
}
finally
{
    Blink.UnbindHostIo();
    io.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
Check(io.OpenDescriptors == 0);

// Focused ABI declarations only; the actual authored bridge is compiled unchanged.
namespace Managed.Emulation
{
    public static class Libc { [ThreadStatic] public static int errno; }
    public static partial class Blink
    {
        public unsafe struct blink_host_iovec { public void* iov_base; public ulong iov_len; }
    }
}

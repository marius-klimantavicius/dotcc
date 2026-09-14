using System.Text;
using Managed.Emulation.Host;

static void Equal<T>(T expected, T actual) where T : IEquatable<T>
{
    if (!expected.Equals(actual)) throw new Exception($"Expected {expected}, got {actual}");
}
static T Ok<T>(HostResult<T> result)
{
    if (!result.Succeeded) throw new Exception("Unexpected guest error " + result.Error);
    return result.Value;
}
static void Error<T>(GuestError expected, HostResult<T> result)
{
    if (result.Error != expected) throw new Exception($"Expected {expected}, got {result.Error}");
}
static string Read(VirtualFileSystem fs, int descriptor, int size)
{
    byte[] buffer = new byte[size];
    return Encoding.UTF8.GetString(buffer, 0, Ok(fs.Read(descriptor, buffer)));
}

byte[] imageBytes = Encoding.UTF8.GetBytes("alpha");
using var first = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>> { ["/data/instance"] = imageBytes }, writableLimit: 8, descriptorLimit: 4);
using var second = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>> { ["/data/instance"] = Encoding.UTF8.GetBytes("bravo") }, writableLimit: 8);
imageBytes[0] = (byte)'X'; // Image ownership is independent of the caller's storage.
int a = Ok(first.Open("instance", FileAccessMode.Read, cwd: "/data"));
int b = Ok(second.Open("/data/instance", FileAccessMode.Read));
Equal("al", Read(first, a, 2));
int duplicate = Ok(first.Duplicate(a));
Equal("pha", Read(first, duplicate, 8));
Equal("", Read(first, a, 1));
Ok(first.Close(a));
Ok(first.Seek(duplicate, 0, SeekOrigin.Begin));
Equal("alpha", Read(first, duplicate, 8));
Equal("bravo", Read(second, b, 8));
Error(GuestError.BadDescriptor, first.Close(a));
Error(GuestError.ReadOnly, first.Open("/data/instance", FileAccessMode.Write));
Error(GuestError.Access, first.Open("../../etc/passwd", FileAccessMode.Read, cwd: "/data"));
Error(GuestError.NoEntry, first.Open("/etc/passwd", FileAccessMode.Read));
Error(GuestError.NotDirectory, first.Open("/data/instance/../new", FileAccessMode.Read | FileAccessMode.Write, create: true));
Error(GuestError.NotDirectory, first.Open("/data/instance/", FileAccessMode.Read));
Error(GuestError.Invalid, first.Open("bad\0name", FileAccessMode.Read));
Error(GuestError.IsDirectory, first.Open("/data", FileAccessMode.Read));
Console.WriteLine("image ownership, isolation, lookup, duplicate cursor: PASS");

int writable = Ok(first.Open("/data/work", FileAccessMode.Read | FileAccessMode.Write, create: true, exclusive: true));
Equal(3, Ok(first.Write(writable, "abc"u8)));
Ok(first.Seek(writable, 5, SeekOrigin.Begin));
Equal(3, Ok(first.Write(writable, "defgh"u8))); // Bounded short write, zero-filled gap.
Error(GuestError.NoSpace, first.Write(writable, "x"u8));
Equal(8L, first.WritableBytes);
Ok(first.Seek(writable, 0, SeekOrigin.Begin));
Equal("abc\0\0def", Read(first, writable, 20));
Ok(first.Seek(writable, 0, SeekOrigin.Begin));
Equal(1, Ok(first.Write(writable, "A"u8))); // Overwrite still works at the quota.
Error(GuestError.Invalid, first.Seek(writable, -1, SeekOrigin.Begin));
Equal(0, Ok(first.Write(writable, ReadOnlySpan<byte>.Empty)));
Error(GuestError.Exists, first.Open("/data/work", FileAccessMode.Write, create: true, exclusive: true));
Error(GuestError.NoEntry, second.Stat("/data/work"));
Console.WriteLine("bounded writes, sparse gaps, overwrite, errors: PASS");

int more = Ok(first.Duplicate(writable));
int last = Ok(first.Duplicate(writable));
Equal(4, first.OpenDescriptors);
Error(GuestError.TooManyFiles, first.Open("/data/work", FileAccessMode.Write, truncate: true));
Equal(8L, Ok(first.Stat("/data/work")).Length); // Failure must not truncate.
Error(GuestError.TooManyFiles, first.Open("/data/new", FileAccessMode.Write, create: true));
Error(GuestError.NoEntry, first.Stat("/data/new"));
Ok(first.Close(more));
int truncated = Ok(first.Open("/data/work", FileAccessMode.Write, truncate: true, append: true));
Equal(0L, first.WritableBytes);
Ok(first.Seek(truncated, 100, SeekOrigin.Begin));
Equal(2, Ok(first.Write(truncated, "ok"u8))); // Append selects current end, not seek position.
Equal(2L, first.WritableBytes);
Ok(first.Seek(writable, 0, SeekOrigin.Begin));
Equal("ok", Read(first, writable, 8));
Error(GuestError.BadDescriptor, first.Read(truncated, new byte[1]));
first.Dispose();
first.Dispose();
Equal(0, first.OpenDescriptors);
Equal(0L, first.WritableBytes);
Error(GuestError.BadDescriptor, first.Read(last, new byte[1]));
Ok(second.Seek(b, 0, SeekOrigin.Begin));
Equal("bravo", Read(second, b, 8));
Console.WriteLine("descriptor exhaustion atomicity, append, disposal: PASS");

using var quota = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>>(), writableLimit: 5);
int q1 = Ok(quota.Open("/one", FileAccessMode.Write, create: true));
int q2 = Ok(quota.Open("/two", FileAccessMode.Write, create: true));
Equal(3, Ok(quota.Write(q1, "123"u8)));
Equal(2, Ok(quota.Write(q2, "456"u8)));
Equal(5L, quota.WritableBytes);
Ok(quota.Seek(q1, long.MaxValue, SeekOrigin.Begin));
Error(GuestError.Invalid, quota.Seek(q1, 1, SeekOrigin.Current));
Error(GuestError.NoSpace, quota.Write(q1, "x"u8));
try
{
    using var oversized = new VirtualFileSystem(new Dictionary<string, ReadOnlyMemory<byte>> { ["/big"] = new byte[2] }, imageLimit: 1);
    throw new Exception("Oversized image accepted");
}
catch (ArgumentException) { }
Console.WriteLine("aggregate quota, seek overflow, image limit: PASS");

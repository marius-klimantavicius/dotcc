using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Managed.Security.S2n;

// Original CLI host services only. The translated C still owns all TLS state,
// record processing, handshakes and application behavior.
public static unsafe partial class S2nTls
{
    [StructLayout(LayoutKind.Sequential)]
    private struct HostPollFd { public int Descriptor; public short Events; public short Returned; }
    [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
    private static extern long HostRead(int fd, void* buffer, ulong count);
    [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
    private static extern long HostWrite(int fd, void* buffer, ulong count);
    [DllImport("libc.so.6", EntryPoint = "poll", SetLastError = true)]
    private static extern int HostPoll(HostPollFd* descriptors, ulong count, int timeout);
    [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
    private static extern int HostIoctl(int fd, ulong request, void* argument);

    private static long StdfdResult(long result)
    {
        if (result < 0) Libc.errno = Marshal.GetLastPInvokeError();
        return result;
    }
    private static long StdfdFailure(Exception error)
    {
        Libc.errno = Libc.UpstreamExceptionErrno(error);
        return -1;
    }
    public static long UpstreamRead(int fd, void* buffer, ulong count)
    {
        try
        {
            if (!OperatingSystem.IsLinux()) { Libc.errno = 95; return -1; }
            return Libc.UpstreamIsNativeStandardDescriptor(fd) ? StdfdResult(HostRead(fd, buffer, count)) : Libc.read(fd, buffer, count);
        }
        catch (Exception error) { return StdfdFailure(error); }
    }
    public static long UpstreamWrite(int fd, void* buffer, ulong count)
    {
        try
        {
            if (!OperatingSystem.IsLinux()) { Libc.errno = 95; return -1; }
            return Libc.UpstreamIsNativeStandardDescriptor(fd) ? StdfdResult(HostWrite(fd, buffer, count)) : Libc.write(fd, buffer, count);
        }
        catch (Exception error) { return StdfdFailure(error); }
    }
    public static int UpstreamPoll(void* descriptors, ulong count, int timeout)
    {
        var leases = new List<SafeHandle>();
        try
        {
            if (!OperatingSystem.IsLinux()) { Libc.errno = 95; return -1; }
            if (count > int.MaxValue) { Libc.errno = 22; return -1; }
            if (count != 0 && descriptors == null) { Libc.errno = 14; return -1; }
            var caller = (HostPollFd*)descriptors;
            HostPollFd[] mapped = new HostPollFd[(int)count];
            bool[] invalid = new bool[(int)count];
            int invalidCount = 0;
            for (int i = 0; i < mapped.Length; i++)
            {
                mapped[i] = caller[i]; mapped[i].Returned = 0;
                if (caller[i].Descriptor < 0) continue;
                int status = Libc.UpstreamBorrowDescriptor(caller[i].Descriptor, out int host, out SafeHandle? lease);
                if (lease != null)
                {
                    try { leases.Add(lease); }
                    catch { lease.DangerousRelease(); throw; }
                }
                if (status == 2) { Libc.errno = 95; return -1; }
                if (status == 1) { invalid[i] = true; invalidCount++; host = -1; }
                mapped[i].Descriptor = host;
            }
            int result;
            fixed (HostPollFd* entries = mapped)
                result = (int)StdfdResult(HostPoll(entries, count, invalidCount != 0 ? 0 : timeout));
            if (result < 0) return result;
            for (int i = 0; i < mapped.Length; i++) caller[i].Returned = invalid[i] ? (short)32 : mapped[i].Returned;
            return result + invalidCount;
        }
        catch (Exception error) { return (int)StdfdFailure(error); }
        finally { foreach (SafeHandle lease in leases) lease.DangerousRelease(); }
    }

    public static unsafe partial class Libc
    {
        public static int UpstreamExceptionErrno(Exception error) => error switch
        {
            OutOfMemoryException => 12, ObjectDisposedException => 9, ArgumentException => 22,
            SocketException socket => SocketErrno(socket.SocketErrorCode),
            DllNotFoundException or EntryPointNotFoundException or PlatformNotSupportedException => 95,
            _ => 5
        };

        public static bool UpstreamIsNativeStandardDescriptor(int descriptor) =>
            descriptor is >= 0 and <= 2 && SlotByFd(descriptor)?.Kind is FileSlot.K.In or FileSlot.K.Out or FileSlot.K.Err;

        // Return 0 for a borrowed native descriptor, 1 for an invalid virtual
        // descriptor (POLLNVAL), and 2 for a truthful unsupported mapping.
        public static int UpstreamBorrowDescriptor(int descriptor, out int host, out SafeHandle? lease)
        {
            host = -1; lease = null;
            var slot = SlotByFd(descriptor);
            if (slot == null) return 1;
            if (descriptor is >= 0 and <= 2 && slot.Kind is FileSlot.K.In or FileSlot.K.Out or FileSlot.K.Err)
            { host = descriptor; return 0; }
            if (slot.Socket is not { } socket) return 2;
            SafeHandle handle = socket.SafeHandle;
            bool borrowed = false;
            try
            {
                handle.DangerousAddRef(ref borrowed);
                host = checked((int)handle.DangerousGetHandle());
                lease = handle;
                return 0;
            }
            catch (ObjectDisposedException)
            {
                if (borrowed) handle.DangerousRelease();
                return 1;
            }
            catch
            {
                if (borrowed) handle.DangerousRelease();
                throw;
            }
        }

        // The original C ioctl call has a pointer third argument. This exact
        // overload is selected ahead of the general variadic fallback.
        public static int ioctl(int descriptor, ulong request, void* argument)
        {
            SafeHandle? lease = null;
            try
            {
                if (!OperatingSystem.IsLinux()) { errno = 95; return -1; }
                int status = UpstreamBorrowDescriptor(descriptor, out int host, out lease);
                if (status != 0) { errno = status == 1 ? 9 : 95; return -1; }
                return (int)StdfdResult(HostIoctl(host, request, argument));
            }
            catch (Exception error) { return (int)StdfdFailure(error); }
            finally { lease?.DangerousRelease(); }
        }
    }
}

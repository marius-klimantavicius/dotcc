#nullable enable

using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

/// <summary>
/// The thin-POSIX surface beyond <c>&lt;unistd.h&gt;</c>: <c>&lt;fcntl.h&gt;</c>,
/// <c>&lt;poll.h&gt;</c>, <c>&lt;sys/stat.h&gt;</c>, <c>&lt;sys/time.h&gt;</c>.
/// (<c>&lt;sys/socket.h&gt;</c> is now real — see <c>SocketLib</c>.) Present so
/// portable Unix C (chibi-scheme's non-<c>_WIN32</c> path) compiles AND behaves
/// honestly at runtime:
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><c>stat</c>/<c>fstat</c> answer existence and the common fields
/// truthfully (File/Directory metadata; open slot).</item>
/// <item><c>gettimeofday</c> is faithful (UTC wall clock, µs truncated from
/// 100 ns ticks).</item>
/// <item><c>fcntl</c> tracks descriptor/status flags and controls socket blocking.</item>
/// <item><c>poll</c> observes sockets and regular files; unsupported console input
/// readiness fails with ENOTSUP instead of inventing availability.</item>
/// </list>
/// Struct-typed parameters (<c>struct stat*</c>, <c>struct pollfd*</c>,
/// <c>struct timeval*</c>) arrive as <c>void*</c>: the C structs are declared
/// in the synthetic headers and emitted into the program, so the runtime
/// addresses their fields by LAYOUT (documented offsets below), not by type.
/// </remarks>
public static unsafe partial class Libc
{
    // ---- <sys/stat.h> ----------------------------------------------------

    // struct stat layout (LP64, see include/sys/stat.h):
    //   0 st_dev(8) 8 st_ino(8) 16 st_mode(4) 20 pad(4) 24 st_nlink(8)
    //   32 st_uid(4) 36 st_gid(4) 40 st_size(8) 48 st_atime(8)
    //   56 st_mtime(8) 64 st_ctime(8)
    private const int StModeOff = 16, StSizeOff = 40, StAtimeOff = 48, StMtimeOff = 56, StCtimeOff = 64;
    private const uint S_IFDIR = 0x4000, S_IFREG = 0x8000;

    /// <summary><c>stat(path, buf)</c> — 0 when <paramref name="path"/> exists
    /// (file or directory), filling mode/size/times; -1 (ENOENT) otherwise.</summary>
    public static int stat(byte* path, void* buf)
    {
        var name = ResolvePath(path);
        try
        {
            if (File.Exists(name))
            {
                var fi = new FileInfo(name);
                FillStat(buf, S_IFREG, fi.Length, fi.LastWriteTimeUtc);
                return 0;
            }
            if (Directory.Exists(name))
            {
                FillStat(buf, S_IFDIR, 0, Directory.GetLastWriteTimeUtc(name));
                return 0;
            }
        }
        catch (IOException) { /* fall through to ENOENT */ }
        errno = ENOENT;
        return -1;
    }

    /// <summary><c>fstat(fd, buf)</c> — 0 for an open dotcc fd (mode = regular
    /// file, size from the backing stream where seekable); -1 otherwise.</summary>
    public static int fstat(int fd, void* buf)
    {
        var len = 0L;
        if (fd is 0 or 1 or 2)
        {
            // std streams: character devices in spirit; report a regular file
            // of size 0 (no caller of dotcc's surface distinguishes).
        }
        else
        {
            var slot = SlotByFd(fd);
            if (slot?.Stream == null) { errno = EBADF; return -1; }
            len = slot.Stream.CanSeek ? slot.Stream.Length : 0;
        }
        FillStat(buf, S_IFREG, len, global::System.DateTime.UtcNow);
        return 0;
    }

    /// <summary><c>lstat(path, buf)</c> — like <see cref="stat"/> but does not
    /// follow a final symlink. dotcc reports the same metadata either way (the
    /// fields it fills don't distinguish), so this aliases <c>stat</c>.</summary>
    public static int lstat(byte* path, void* buf) => stat(path, buf);

    private static void FillStat(void* buf, uint mode, long size, global::System.DateTime mtimeUtc)
    {
        var b = (byte*)buf;
        // struct stat is 96 bytes after the appended st_rdev/st_blksize/st_blocks
        // (see include/sys/stat.h) — clear all of it so those report 0.
        new Span<byte>(b, 96).Clear();
        *(uint*)(b + StModeOff) = mode;
        *(long*)(b + StSizeOff) = size;
        var unix = new DateTimeOffset(mtimeUtc).ToUnixTimeSeconds();
        *(long*)(b + StAtimeOff) = unix;
        *(long*)(b + StMtimeOff) = unix;
        *(long*)(b + StCtimeOff) = unix;
    }

    // ---- <sys/time.h> ----------------------------------------------------

    /// <summary><c>gettimeofday(tv, tz)</c> — UTC wall clock into
    /// <c>struct timeval { long tv_sec; long tv_usec; }</c>. The obsolete
    /// timezone argument is ignored (NULL on all modern callers). Always 0.</summary>
    public static int gettimeofday(void* tv, void* tz)
    {
        var t = (long*)tv;
        var ticks = (global::System.DateTime.UtcNow - global::System.DateTime.UnixEpoch).Ticks; // 100 ns units
        t[0] = ticks / TimeSpan.TicksPerSecond;
        t[1] = ticks % TimeSpan.TicksPerSecond / 10;
        return 0;
    }

    /// <summary><c>settimeofday(tv, tz)</c> — a managed process can't set the
    /// system clock; fail with EPERM (exactly what a non-root process gets).</summary>
    public static int settimeofday(void* tv, void* tz)
    {
        errno = EPERM;
        return -1;
    }

    /// <summary><c>getrusage(who, usage)</c> — per-process resource usage.
    /// Forwards to <c>getrusage(2)</c> on POSIX. On Windows there's no single
    /// rusage call, so we fill the same 144-byte Linux <c>struct rusage</c>
    /// layout (see include/sys/resource.h — the emitted struct is identical on
    /// every host) from GetProcessTimes (ru_utime/ru_stime, the CPU fields that
    /// matter) and, best-effort, GetProcessMemoryInfo (ru_maxrss = peak working
    /// set in KB, matching Linux's units); every other field stays 0.</summary>
    public static int getrusage(int who, void* usage)
    {
        if (!OperatingSystem.IsWindows())
        {
            if (PosixGetrusage(who, usage) == 0) { return 0; }
            errno = Marshal.GetLastPInvokeError();
            return -1;
        }

        // struct rusage as longs: [0]=ru_utime.tv_sec [1]=.tv_usec
        // [2]=ru_stime.tv_sec [3]=.tv_usec [4]=ru_maxrss, rest 0.
        new Span<byte>(usage, 144).Clear();
        long* ru = (long*)usage;
        IntPtr self = GetCurrentProcess();   // pseudo-handle (-1); no close needed
        if (GetProcessTimes(self, out _, out _, out long kernel100ns, out long user100ns))
        {
            (ru[0], ru[1]) = Ticks100nsToTimeval(user100ns);    // ru_utime
            (ru[2], ru[3]) = Ticks100nsToTimeval(kernel100ns);  // ru_stime
        }
        var mem = new PROCESS_MEMORY_COUNTERS { cb = (uint)sizeof(PROCESS_MEMORY_COUNTERS) };
        if (GetProcessMemoryInfo(self, ref mem, mem.cb))
        {
            ru[4] = (long)(mem.PeakWorkingSetSize / 1024);      // ru_maxrss in KB (Linux units)
        }
        return 0;
    }

    /// <summary>Split a Windows 100-ns FILETIME tick count into POSIX
    /// <c>timeval</c> (seconds, microseconds).</summary>
    private static (long Sec, long Usec) Ticks100nsToTimeval(long ticks100ns)
    {
        long usec = ticks100ns / 10;            // 100ns -> microseconds
        return (usec / 1_000_000, usec % 1_000_000);
    }

    [DllImport("libc", EntryPoint = "getrusage", SetLastError = true)]
    private static extern int PosixGetrusage(int who, void* usage);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetProcessTimes(
        IntPtr hProcess, out long creation, out long exit, out long kernel, out long user);

    // PROCESS_MEMORY_COUNTERS: cb + PageFaultCount are DWORD; the rest SIZE_T.
    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_MEMORY_COUNTERS
    {
        public uint cb;
        public uint PageFaultCount;
        public nuint PeakWorkingSetSize;
        public nuint WorkingSetSize;
        public nuint QuotaPeakPagedPoolUsage;
        public nuint QuotaPagedPoolUsage;
        public nuint QuotaPeakNonPagedPoolUsage;
        public nuint QuotaNonPagedPoolUsage;
        public nuint PagefileUsage;
        public nuint PeakPagefileUsage;
    }

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool GetProcessMemoryInfo(IntPtr hProcess, ref PROCESS_MEMORY_COUNTERS counters, uint size);

    // ---- <fcntl.h> ---------------------------------------------------------

    /// <summary>Read descriptor/status flags for an open dotcc descriptor.</summary>
    public static int fcntl(int fd, int cmd)
    {
        var slot = SlotByFd(fd);
        if (slot is null) { errno = EBADF; return -1; }
        return cmd switch
        {
            1 => slot.DescriptorFlags,
            3 => slot.StatusFlags,
            _ => FcntlError(EINVAL),
        };
    }

    /// <summary>Set flags; access mode is unchanged by F_SETFL, as on Linux.</summary>
    public static int fcntl(int fd, int cmd, int arg)
    {
        var slot = SlotByFd(fd);
        if (slot is null) return FcntlError(EBADF);
        if (cmd is 1 or 3) return fcntl(fd, cmd);
        if (cmd == 2) { slot.DescriptorFlags = arg & 1; return 0; }
        if (cmd != 4) return FcntlError(EINVAL);
        // O_APPEND and O_NONBLOCK are the supported mutable status flags.
        // Other Linux status flags require behavior the managed runtime lacks.
        if ((arg & (0x2000 | 0x4000 | 0x40000)) != 0) return FcntlError(ENOTSUP);
        if (slot.Kind is FileSlot.K.In or FileSlot.K.Out or FileSlot.K.Err && (arg & 0x800) != 0)
            return FcntlError(ENOTSUP);
        try
        {
            if (slot.Socket is { } sock) sock.Blocking = (arg & 0x800) == 0;
            if (slot.Pipe is { } pipe)
            {
                lock (pipe.Sync)
                {
                    slot.StatusFlags = (slot.StatusFlags & 3) | (arg & 0xc00);
                    global::System.Threading.Monitor.PulseAll(pipe.Sync);
                }
            }
            else slot.StatusFlags = (slot.StatusFlags & 3) | (arg & 0xc00);
            return 0;
        }
        catch (SocketException ex) { return FcntlError(SocketErrno(ex.SocketErrorCode)); }
        catch (ObjectDisposedException) { return FcntlError(EBADF); }
    }

    private static int FcntlError(int error) { errno = error; return -1; }
    public static int fcntl(int fd, int cmd, long arg) => fcntl(fd, cmd, unchecked((int)arg));
    public static int fcntl(int fd, int cmd, ulong arg) => fcntl(fd, cmd, unchecked((int)arg));

    // ---- <poll.h> ----------------------------------------------------------

    /// <summary>Poll Linux-layout pollfd entries (fd:4/events:2/revents:2).
    /// Regular files are immediately ready; sockets use BCL readiness. The
    /// timeout uses a monotonic clock and zero/negative descriptors are handled
    /// independently of the socket list. Console input readiness is unsupported.</summary>
    public static int poll(void* fds, ulong nfds, int timeout)
    {
        if (nfds > int.MaxValue) return FcntlError(EINVAL);
        if (nfds != 0 && fds == null) return FcntlError(EFAULT);
        var started = global::System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            var reads = new List<Socket>();
            var writes = new List<Socket>();
            var errors = new List<Socket>();
            int ready = 0;
            for (int i = 0; i < (int)nfds; i++)
            {
                byte* p = (byte*)fds + i * 8;
                int fd = *(int*)p;
                short events = *(short*)(p + 4);
                *(short*)(p + 6) = 0;
                if (fd < 0) continue;
                var slot = SlotByFd(fd);
                if (slot is null) { *(short*)(p + 6) = 0x20; ready++; continue; }
                if (slot.Kind == FileSlot.K.Pipe)
                {
                    short result = PollPipe(slot, events);
                    *(short*)(p + 6) = result;
                    if (result != 0) ready++;
                    continue;
                }
                if (slot.Socket is { } socket)
                {
                    // Read readiness also detects orderly EOF; errors are
                    // monitored regardless of the events requested by the caller.
                    if ((events & 1) != 0 || socket.SocketType == SocketType.Stream) reads.Add(socket);
                    if ((events & 4) != 0 || slot.Connecting) writes.Add(socket);
                    errors.Add(socket);
                    continue;
                }
                if (slot.Kind == FileSlot.K.In) return FcntlError(ENOTSUP);
                if (slot.Kind == FileSlot.K.File && slot.Stream is not { CanSeek: true })
                    return FcntlError(ENOTSUP);
                *(short*)(p + 6) = (short)(events & 5);
                if (*(short*)(p + 6) != 0) ready++;
            }
            long remaining = timeout < 0 ? long.MaxValue : Math.Max(0, (long)timeout - started.ElapsedMilliseconds);
            int waitMilliseconds = ready > 0 ? 0 : (int)Math.Min(remaining, 50);
            if (reads.Count + writes.Count + errors.Count == 0)
            {
                if (ready > 0) return ready;
                if (remaining == 0) return 0;
                global::System.Threading.Thread.Sleep(waitMilliseconds);
                continue;
            }
            try
            {
                Socket.Select(reads, writes, errors, waitMilliseconds * 1000);
                for (int i = 0; i < (int)nfds; i++)
                {
                    byte* p = (byte*)fds + i * 8;
                    int fd = *(int*)p;
                    if (fd < 0) continue;
                    var slot = SlotByFd(fd);
                    // Pipes may change readiness while Socket.Select blocks.
                    // Rescan before deciding that this timeout expired.
                    if (slot is null || slot.Kind == FileSlot.K.Pipe)
                    {
                        if (*(short*)(p + 6) != 0) ready--;
                        short pipeResult = slot is null ? (short)32 : PollPipe(slot, *(short*)(p + 4));
                        *(short*)(p + 6) = pipeResult;
                        if (pipeResult != 0) ready++;
                        continue;
                    }
                    if (slot?.Socket is not { } socket) continue;
                    short events = *(short*)(p + 4), result = 0;
                    bool readable = reads.Contains(socket), writable = writes.Contains(socket), exceptional = errors.Contains(socket);
                    if (slot.Connecting && (readable || writable || exceptional))
                    {
                        slot.PendingSocketError = SocketErrno((SocketError)(int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error)!);
                        slot.Connecting = false;
                    }
                    if (slot.PendingSocketError != 0) result |= 8 | 16; // POLLERR | POLLHUP
                    else if (exceptional) result |= (short)(events & 2); // urgent data
                    if (readable && (events & 1) != 0) result |= 1;
                    if (writable && (events & 4) != 0) result |= 4;
                    // Read shutdown is readable EOF. Linux POLLRDHUP is opt-in;
                    // POLLHUP describes a fully disconnected stream.
                    if (readable && socket.SocketType == SocketType.Stream && !slot.Listening && socket.Available == 0)
                    {
                        if (socket.Connected) result |= (short)(events & 0x2000);
                        else result |= 16;
                    }
                    *(short*)(p + 6) = result;
                    if (result != 0) ready++;
                }
            }
            catch (SocketException ex) { return FcntlError(SocketErrno(ex.SocketErrorCode)); }
            catch (ObjectDisposedException) { continue; } // rescan: concurrent close becomes POLLNVAL
            if (ready > 0) return ready;
            if (timeout >= 0 && started.ElapsedMilliseconds >= timeout) return 0;
            // EOF may wake select even when POLLIN wasn't requested.
            if (reads.Count + writes.Count + errors.Count != 0) global::System.Threading.Thread.Sleep(1);
        }
    }
}

using System.Runtime.InteropServices;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int checks, failures;
    private struct PollFd { public int Fd; public short Events, Revents; }
    private static void Equal(string name, long actual, long expected)
    {
        checks++;
        if (actual != expected)
        {
            failures++;
            Console.WriteLine($"FAIL {name} actual={actual} expected={expected}");
        }
    }

    // Deliberately reads the documented target byte layout. This direct-service
    // test is not a substitute for executing an actual emitted C sizeof probe.
    private readonly struct Metadata(byte[] bytes)
    {
        public ulong Device => BitConverter.ToUInt64(bytes, 0);
        public ulong Inode => BitConverter.ToUInt64(bytes, 8);
        public uint Mode => BitConverter.ToUInt32(bytes, 16);
        public ulong Links => BitConverter.ToUInt64(bytes, 24);
        public uint User => BitConverter.ToUInt32(bytes, 32);
        public uint Group => BitConverter.ToUInt32(bytes, 36);
        public long Size => BitConverter.ToInt64(bytes, 40);
        public long Access => BitConverter.ToInt64(bytes, 48);
        public long Modify => BitConverter.ToInt64(bytes, 56);
        public long Change => BitConverter.ToInt64(bytes, 64);
        public ulong SpecialDevice => BitConverter.ToUInt64(bytes, 72);
        public long BlockSize => BitConverter.ToInt64(bytes, 80);
        public long Blocks => BitConverter.ToInt64(bytes, 88);
    }
    private static bool Capture(int fd, out Metadata result)
    {
        byte* guarded = stackalloc byte[112];
        new Span<byte>(guarded, 112).Fill(0xa5);
        errno = 123;
        int rc = fstat(fd, guarded + 8);
        Equal("fstat", rc, 0);
        result = default;
        if (rc != 0) return false;
        Equal("success-errno", errno, 123);
        for (int i = 0; i < 8; i++)
        {
            Equal("stat-prefix", guarded[i], 0xa5);
            Equal("stat-suffix", guarded[104 + i], 0xa5);
        }
        result = new(new ReadOnlySpan<byte>(guarded + 8, 96).ToArray());
        Equal("fifo-type", (result.Mode & 0xf000) == 0x1000 ? 1 : 0, 1);
        Equal("owner-mode", result.Mode & 0xfff, 0x180);
        Equal("links", (long)result.Links, 1);
        Equal("uid", result.User, geteuid());
        Equal("gid", result.Group, getegid());
        Equal("size-is-not-queued-count", result.Size, 0);
        Equal("rdev", (long)result.SpecialDevice, 0);
        Equal("blocks", result.Blocks, 0);
        Equal("preferred-transfer-positive", result.BlockSize > 0 ? 1 : 0, 1);
        return true;
    }
    private static void Same(in Metadata a, in Metadata b)
    {
        Equal("same-device", a.Device == b.Device ? 1 : 0, 1);
        Equal("same-inode", a.Inode == b.Inode ? 1 : 0, 1);
        Equal("same-mode", a.Mode, b.Mode);
        Equal("same-links", (long)a.Links, (long)b.Links);
        Equal("same-uid", a.User, b.User); Equal("same-gid", a.Group, b.Group);
        Equal("same-size", a.Size, b.Size);
        Equal("same-access", a.Access, b.Access); Equal("same-modify", a.Modify, b.Modify);
        Equal("same-change", a.Change, b.Change);
        Equal("same-rdev", a.SpecialDevice == b.SpecialDevice ? 1 : 0, 1);
        Equal("same-blocksize", a.BlockSize, b.BlockSize); Equal("same-blocks", a.Blocks, b.Blocks);
    }
    private static void OriginalR36()
    {
        int* fd = stackalloc int[2];
        Equal("pipe", pipe(fd), 0);
        for (int end = 0; end < 2; end++)
        {
            byte* metadata = stackalloc byte[96];
            int rc = fstat(fd[end], metadata);
            Equal("fstat-pipe", rc, 0);
            if (rc == 0) Equal("fifo-mode", (*(uint*)(metadata + 16) & 0xf000) == 0x1000 ? 1 : 0, 1);
            errno = 0;
            Equal("seek-pipe", lseek(fd[end], 0, SEEK_CUR), -1); Equal("seek-errno", errno, ESPIPE);
            int flags = fcntl(fd[end], 3);
            Equal("get-flags", flags >= 0 ? 1 : 0, 1);
            Equal("set-nonblock", fcntl(fd[end], 4, flags | 0x800), 0);
            Equal("nonblock-roundtrip", (fcntl(fd[end], 3) & 0x800) != 0 ? 1 : 0, 1);
        }
        byte value = 0, marker = 0x5a;
        errno = 0;
        Equal("empty-nonblock-read", read(fd[0], &value, 1), -1); Equal("empty-read-errno", errno, EAGAIN);
        Equal("pipe-write", write(fd[1], &marker, 1), 1); Equal("pipe-read", read(fd[0], &value, 1), 1);
        Equal("pipe-byte", value, marker); Equal("close-writer", close(fd[1]), 0);
        Equal("drained-eof", read(fd[0], &value, 1), 0); Equal("close-reader", close(fd[0]), 0);
        Console.WriteLine($"R36 checks={checks} failures={failures}");
    }
    private static void Identity()
    {
        int* fd = stackalloc int[2]; int* other = stackalloc int[2];
        Equal("pipe", pipe(fd), 0); Equal("other-pipe", pipe(other), 0);
        for (int end = 0; end < 2; end++)
        {
            Equal("initial-descriptor-flags", fcntl(fd[end], 1), 0);
            Equal("set-noninheritable", fcntl(fd[end], 2, 1), 0);
            Equal("noninheritable-roundtrip", fcntl(fd[end], 1), 1);
        }
        long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!Capture(fd[0], out var initial) || !Capture(fd[1], out var writer) || !Capture(other[0], out var second)) return;
        Same(initial, writer);
        Equal("distinct-pipe", initial.Device != second.Device || initial.Inode != second.Inode ? 1 : 0, 1);
        Equal("timestamp-real", initial.Change > 0 && initial.Change <= before ? 1 : 0, 1);
        Equal("creation-access-modify", initial.Access, initial.Modify);
        Equal("creation-modify-change", initial.Modify, initial.Change);
        Equal("close-other-read", close(other[0]), 0); Equal("close-other-write", close(other[1]), 0);
        byte* ignored = stackalloc byte[96];
        errno = 0; Equal("valid-fd-null", fstat(fd[0], null), -1); Equal("null-errno", errno, EFAULT);
        errno = 0; Equal("invalid-fd", fstat(-1, ignored), -1); Equal("invalid-errno", errno, EBADF);
        errno = 0; Equal("ftruncate-pipe", ftruncate(fd[1], 1), -1); Equal("truncate-errno", errno, EINVAL);
        errno = 0; Equal("seek-pipe", lseek(fd[0], 0, SEEK_CUR), -1); Equal("seek-errno", errno, ESPIPE);
        Equal("nonblock", fcntl(fd[0], 4, 0x800), 0);
        byte value = 0, marker = 0x5a;
        errno = 0; Equal("empty-read", read(fd[0], &value, 1), -1); Equal("empty-errno", errno, EAGAIN);
        PollFd ready = new() { Fd = fd[0], Events = 1 };
        Equal("empty-poll", poll(&ready, 1, 0), 0);
        Thread.Sleep(2000);
        Equal("write", write(fd[1], &marker, 1), 1);
        if (!Capture(fd[0], out var queued) || !Capture(fd[1], out writer)) return;
        Same(queued, writer);
        Equal("queue-keeps-identity", queued.Inode == initial.Inode && queued.Device == initial.Device ? 1 : 0, 1);
        Equal("readable-poll", poll(&ready, 1, 0), 1); Equal("poll-in", (ready.Revents & 1) != 0 ? 1 : 0, 1);
        Equal("read", read(fd[0], &value, 1), 1); Equal("data", value, marker);
        if (!Capture(fd[0], out var drained)) return;
        Equal("write-keeps-access", queued.Access, initial.Access);
        Equal("write-updates-modify", queued.Modify > initial.Modify ? 1 : 0, 1);
        Equal("write-updates-change", queued.Change > initial.Change ? 1 : 0, 1);
        Equal("read-updates-access", drained.Access > queued.Access ? 1 : 0, 1);
        Equal("read-keeps-modify", drained.Modify, queued.Modify);
        Equal("read-keeps-change", drained.Change, queued.Change);
        Console.WriteLine($"TIMESTAMPS write_access_changed={(queued.Access != initial.Access ? 1 : 0)} write_modify_changed={(queued.Modify != initial.Modify ? 1 : 0)} write_change_changed={(queued.Change != initial.Change ? 1 : 0)} read_access_changed={(drained.Access != queued.Access ? 1 : 0)} read_modify_changed={(drained.Modify != queued.Modify ? 1 : 0)} read_change_changed={(drained.Change != queued.Change ? 1 : 0)}");
        Equal("close-writer", close(fd[1]), 0);
        if (!Capture(fd[0], out var afterClose)) return;
        Same(drained, afterClose);
        Equal("eof-poll", poll(&ready, 1, 0), 1); Equal("poll-hup", (ready.Revents & 16) != 0 ? 1 : 0, 1);
        Equal("eof", read(fd[0], &value, 1), 0);
        errno = 0; Equal("closed-writer-fstat", fstat(fd[1], ignored), -1); Equal("closed-writer-errno", errno, EBADF);
        Equal("close-reader", close(fd[0]), 0);
        errno = 0; Equal("closed-reader-fstat", fstat(fd[0], ignored), -1); Equal("closed-reader-errno", errno, EBADF);
    }
    private static int Main()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        OriginalR36(); Identity();
        Console.WriteLine($"SUMMARY checks={checks} failures={failures}");
        return failures == 0 ? 0 : 1;
    }
}

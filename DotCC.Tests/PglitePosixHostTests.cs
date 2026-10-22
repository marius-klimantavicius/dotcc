using System.Runtime.InteropServices;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PglitePosixHostTests
{
    private struct PasswordRecord
    {
        public byte* Name;
        public byte* Password;
        public uint Uid, Gid;
        public byte* Gecos;
        public byte* Directory;
        public byte* Shell;
    }

    private static void Linux()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            Assert.Skip("Linux x64 LP64 host service control");
    }
    private static int Shm(string name, int flags, uint mode = 0x180)
    { fixed (byte* p = Encoding.UTF8.GetBytes(name + "\0")) return shm_open(p, flags, mode); }
    private static int Unlink(string name)
    { fixed (byte* p = Encoding.UTF8.GetBytes(name + "\0")) return shm_unlink(p); }
    private static int Open(string name, int flags)
    { fixed (byte* p = Encoding.UTF8.GetBytes(name + "\0")) return open(p, flags); }

    [Fact]
    public void Shared_object_is_real_exclusive_permissioned_and_survives_unlink_and_descriptor_close()
    {
        Linux();
        string name = "/dotcc-posixhost-" + Guid.NewGuid().ToString("N");
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        int fd = -1, peer = -1; void* mapping = (void*)(nint)(-1);
        try
        {
            fd = Shm(name, 2 | 0x40 | 0x80);
            fd.ShouldBe(3); // independent of occupied native descriptors
            fcntl(fd, 1).ShouldBe(1);
            Shm(name, 2 | 0x40 | 0x80).ShouldBe(-1); errno.ShouldBe(EEXIST);
            File.GetUnixFileMode("/dev/shm" + name).ShouldBe(UnixFileMode.UserRead | UnixFileMode.UserWrite);
            ftruncate(fd, 4096).ShouldBe(0);
            mapping = mmap(null, 4096, 3, 1, fd, 0);
            ((nint)mapping).ShouldNotBe((nint)(-1));
            ((byte*)mapping)[123] = 0xc7;
            peer = Shm(name, 0);
            byte observed = 0; pread(peer, &observed, 1, 123).ShouldBe(1); observed.ShouldBe((byte)0xc7);
            Unlink(name).ShouldBe(0);
            Shm(name, 0).ShouldBe(-1); errno.ShouldBe(ENOENT);
            close(fd).ShouldBe(0); fd = -1;
            ((byte*)mapping)[124] = 0xc8;
            pread(peer, &observed, 1, 124).ShouldBe(1); observed.ShouldBe((byte)0xc8);
            close(peer).ShouldBe(0); peer = -1;
            ((byte*)mapping)[123].ShouldBe((byte)0xc7);
        }
        finally
        {
            if (fd >= 0) close(fd); if (peer >= 0) close(peer);
            if (mapping != (void*)(nint)(-1)) munmap(mapping, 4096).ShouldBe(0);
            Unlink(name);
        }
    }

    [Fact]
    public void Shared_descriptors_are_owner_local_and_disposal_closes_the_real_OS_handle()
    {
        Linux();
        string name = "/dotcc-posixhost-" + Guid.NewGuid().ToString("N");
        using var owner = new RuntimeContext();
        int native = -1;
        try
        {
            using (owner.Enter())
            {
                int fd = Shm(name, 2 | 0x40 | 0x80);
                fd.ShouldBe(3);
                // Observe the independently numbered real descriptor without
                // opening/duplicating it. The runtime must close this handle.
                string target = "/dev/shm" + name;
                foreach (string entry in Directory.EnumerateFiles("/proc/self/fd"))
                    if (new FileInfo(entry).LinkTarget == target) native = int.Parse(Path.GetFileName(entry));
                native.ShouldBeGreaterThan(3);
                using var foreign = new RuntimeContext();
                using (foreign.Enter())
                {
                    byte* attributes = stackalloc byte[60];
                    tcgetattr(fd, attributes).ShouldBe(-1); errno.ShouldBe(EBADF);
                    ((nint)mmap(null, 4096, 1, 1, fd, 0)).ShouldBe((nint)(-1)); errno.ShouldBe(EBADF);
                }
                NativeFcntl(native, 1).ShouldBeGreaterThanOrEqualTo(0);
            }
            owner.Dispose();
            NativeFcntl(native, 1).ShouldBe(-1); Marshal.GetLastPInvokeError().ShouldBe(EBADF);
            // Names have POSIX kernel lifetime, independent of owner disposal.
            File.Exists("/dev/shm" + name).ShouldBeTrue();
        }
        finally { Unlink(name); }
    }

    [Fact]
    public void Shared_open_matches_native_missing_exclusive_truncate_and_mapping_access_errors()
    {
        Linux();
        string name = "/dotcc-posixhost-" + Guid.NewGuid().ToString("N");
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        fixed (byte* p = Encoding.UTF8.GetBytes(name + "\0"))
        {
            NativeShmOpen(p, 0, 0).ShouldBe(-1); int expected = Marshal.GetLastPInvokeError();
            Shm(name, 0).ShouldBe(-1); errno.ShouldBe(expected);
            int fd = Shm(name, 2 | 0x40 | 0x80);
            try
            {
                ftruncate(fd, 4096).ShouldBe(0);
                NativeShmOpen(p, 2 | 0x40 | 0x80, 0x180).ShouldBe(-1); expected = Marshal.GetLastPInvokeError();
                Shm(name, 2 | 0x40 | 0x80).ShouldBe(-1); errno.ShouldBe(expected);
                int readOnly = Shm(name, 0);
                try
                {
                    ((nint)mmap(null, 4096, 3, 1, readOnly, 0)).ShouldBe((nint)(-1)); errno.ShouldBe(EACCES);
                    byte v = 1; pwrite(readOnly, &v, 1, 0).ShouldBe(-1); errno.ShouldBe(EBADF);
                }
                finally { close(readOnly); }
                int truncated = Shm(name, 2 | 0x200);
                try { new FileInfo("/dev/shm" + name).Length.ShouldBe(0); }
                finally { close(truncated); }
            }
            finally { close(fd); Unlink(name); }
            NativeShmUnlink(p).ShouldBe(-1); expected = Marshal.GetLastPInvokeError();
            Unlink(name).ShouldBe(-1); errno.ShouldBe(expected);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("relative")]
    [InlineData("/nested/name")]
    [InlineData("/.")]
    [InlineData("/..")]
    public void Shared_names_reject_nonportable_or_traversal_components(string name)
    { Shm(name, 0).ShouldBe(-1); errno.ShouldBe(EINVAL); Unlink(name).ShouldBe(-1); errno.ShouldBe(EINVAL); }

    [Fact]
    public void Shared_name_length_null_and_flags_are_validated()
    {
        Shm("/" + new string('x', 256), 0).ShouldBe(-1); errno.ShouldBe(ENAMETOOLONG);
        shm_open(null, 0, 0).ShouldBe(-1); errno.ShouldBe(EFAULT);
        shm_unlink(null).ShouldBe(-1); errno.ShouldBe(EFAULT);
        Shm("/unused", 3).ShouldBe(-1); errno.ShouldBe(EINVAL);
        Shm("/unused", 0x40000000).ShouldBe(-1); errno.ShouldBe(EINVAL);
    }

    [Fact]
    public void Equal_shared_descriptor_numbers_in_two_owners_refer_to_distinct_real_objects()
    {
        Linux(); string a = "/dotcc-posixhost-" + Guid.NewGuid().ToString("N");
        string b = "/dotcc-posixhost-" + Guid.NewGuid().ToString("N");
        using var first = new RuntimeContext(); using var second = new RuntimeContext();
        try
        {
            using var binding = first.Enter();
            int fd = Shm(a, 2 | 0x40 | 0x80); fd.ShouldBe(3);
            byte value = 17; pwrite(fd, &value, 1, 0).ShouldBe(1);
            using (second.Enter())
            {
                int other = Shm(b, 2 | 0x40 | 0x80); other.ShouldBe(fd);
                value = 29; pwrite(other, &value, 1, 0).ShouldBe(1);
                close(other).ShouldBe(0);
            }
            value = 0; pread(fd, &value, 1, 0).ShouldBe(1); value.ShouldBe((byte)17);
            close(fd).ShouldBe(0);
        }
        finally { Unlink(a); Unlink(b); }
    }

    [Fact]
    public void Shared_creation_with_zero_permissions_does_not_apply_a_default_mode()
    {
        Linux(); string name = "/dotcc-posixhost-" + Guid.NewGuid().ToString("N");
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        int fd = -1;
        try
        {
            fd = Shm(name, 2 | 0x40 | 0x80, 0); fd.ShouldBe(3);
            File.GetUnixFileMode("/dev/shm" + name).ShouldBe((UnixFileMode)0);
            fixed (byte* p = Encoding.UTF8.GetBytes(name + "\0"))
            {
                int native = NativeShmOpen(p, 0, 0), expected = Marshal.GetLastPInvokeError();
                int guest = Shm(name, 0);
                if (native < 0) { guest.ShouldBe(-1); errno.ShouldBe(expected); }
                else
                {
                    // A privileged host may bypass DAC; compare its real
                    // native behavior instead of claiming an EACCES control.
                    try { guest.ShouldBeGreaterThanOrEqualTo(0); }
                    finally { NativeClose(native); if (guest >= 0) close(guest); }
                }
            }
        }
        finally { if (fd >= 0) close(fd); Unlink(name); }
    }

    [Fact]
    public void Genuine_owner_console_kinds_read_the_actual_standard_terminal_handles()
    {
        Linux(); using var owner = new RuntimeContext(); using var binding = owner.Enter();
        byte* expected = stackalloc byte[60]; byte* actual = stackalloc byte[60];
        for (int fd = 0; fd < 3; fd++)
        {
            int status = NativeGetAttr(fd, expected), error = Marshal.GetLastPInvokeError();
            tcgetattr(fd, actual).ShouldBe(status);
            if (status != 0) errno.ShouldBe(error);
            else
            {
                (*(uint*)(actual + 12)).ShouldBe(*(uint*)(expected + 12));
                actual[17 + 4].ShouldBe(expected[17 + 4]);
            }
        }
    }

    [Fact]
    public void Reopened_owner_stdin_uses_its_real_file_handle_instead_of_OS_stdin()
    {
        Linux(); string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0"))
            fixed (byte* mode = "r+\0"u8)
                ((nint)freopen(name, mode, stdin)).ShouldNotBe(0);
            byte* attributes = stackalloc byte[60];
            tcgetattr(0, attributes).ShouldBe(-1); errno.ShouldBe(ENOTTY);
            tcsetattr(0, 0, attributes).ShouldBe(-1); errno.ShouldBe(ENOTTY);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Terminal_services_reject_real_regular_files_and_owned_pipes_and_bad_descriptors()
    {
        Linux(); using var owner = new RuntimeContext(); using var binding = owner.Enter();
        byte* attributes = stackalloc byte[60];
        tcgetattr(-1, attributes).ShouldBe(-1); errno.ShouldBe(EBADF);
        int fd = Open("/dev/null", 2); int* pair = stackalloc int[2]; pipe(pair).ShouldBe(0);
        try
        {
            NativeGetAttr(-1, attributes).ShouldBe(-1); Marshal.GetLastPInvokeError().ShouldBe(EBADF);
            tcgetattr(fd, attributes).ShouldBe(-1); errno.ShouldBe(ENOTTY);
            tcsetattr(fd, 0, attributes).ShouldBe(-1); errno.ShouldBe(ENOTTY);
            tcgetattr(pair[0], attributes).ShouldBe(-1); errno.ShouldBe(ENOTTY);
            tcsetattr(pair[1], 0, attributes).ShouldBe(-1); errno.ShouldBe(ENOTTY);
            tcgetattr(fd, null).ShouldBe(-1); errno.ShouldBe(EFAULT);
        }
        finally { close(fd); close(pair[0]); close(pair[1]); }
    }

    [Fact]
    public void Real_PTY_roundtrip_maps_the_owner_descriptor_and_restores_terminal_state()
    {
        Linux(); using var owner = new RuntimeContext(); using var binding = owner.Enter();
        int master = NativeOpenPty(2 | 0x100 | 0x80000);
        if (master < 0) Assert.Skip("Host PTY unavailable: errno " + Marshal.GetLastPInvokeError());
        int slave = -1;
        byte* original = stackalloc byte[60]; byte* observed = stackalloc byte[60];
        bool restore = false;
        try
        {
            NativeGrantPty(master).ShouldBe(0); NativeUnlockPty(master).ShouldBe(0);
            byte* path = stackalloc byte[256]; NativePtyName(master, path, 256).ShouldBe(0);
            slave = open(path, 2 | 0x100); slave.ShouldBe(3);
            tcgetattr(slave, original).ShouldBe(0); restore = true;
            new ReadOnlySpan<byte>(original, 60).CopyTo(new Span<byte>(observed, 60));
            *(uint*)(observed + 12) ^= 8; // ECHO, actual Linux c_lflag offset
            foreach (int action in new[] { 0, 1, 2 })
            {
                tcsetattr(slave, action, observed).ShouldBe(0);
                byte* check = stackalloc byte[60]; tcgetattr(slave, check).ShouldBe(0);
                (*(uint*)(check + 12) & 8).ShouldBe(*(uint*)(observed + 12) & 8);
                NativeGetAttr(master, check).ShouldBe(0);
                (*(uint*)(check + 12) & 8).ShouldBe(*(uint*)(observed + 12) & 8);
            }
            NativeSetAttr(master, 17, observed).ShouldBe(-1); int expected = Marshal.GetLastPInvokeError();
            tcsetattr(slave, 17, observed).ShouldBe(-1); errno.ShouldBe(expected);
            tcsetattr(slave, 0, original).ShouldBe(0);
            tcgetattr(slave, observed).ShouldBe(0);
            (*(uint*)(observed + 12)).ShouldBe(*(uint*)(original + 12));
        }
        finally
        {
            if (restore) tcsetattr(slave, 0, original).ShouldBe(0);
            if (slave >= 0) close(slave); NativeClose(master);
        }
    }

    [Fact]
    public void Reentrant_NSS_preserves_caller_records_buffers_and_numeric_errors()
    {
        Linux(); using var owner = new RuntimeContext(); using var binding = owner.Enter();
        byte* buffer = stackalloc byte[16386]; new Span<byte>(buffer, 16386).Fill(0xa5);
        PasswordRecord* records = stackalloc PasswordRecord[3];
        new Span<byte>(records, sizeof(PasswordRecord) * 3).Fill(0xa5);
        PasswordRecord* result = (PasswordRecord*)1;
        errno = EIO;
        getpwuid_r(getuid(), records + 1, buffer + 1, 1, &result).ShouldBe(ERANGE);
        ((nint)result).ShouldBe(0); errno.ShouldBe(EIO);
        getpwuid_r(getuid(), records + 1, buffer + 1, 16384, &result).ShouldBe(0);
        ((nint)result).ShouldBe((nint)(records + 1)); result->Uid.ShouldBe(getuid());
        foreach (nint pointer in new[] { (nint)result->Name, (nint)result->Password, (nint)result->Gecos, (nint)result->Directory, (nint)result->Shell })
        {
            pointer.ShouldBeGreaterThanOrEqualTo((nint)(buffer + 1));
            pointer.ShouldBeLessThan((nint)(buffer + 16385));
        }
        byte* nativeBuffer = stackalloc byte[16384]; PasswordRecord nativeRecord; PasswordRecord* nativeResult;
        NativePasswd(getuid(), &nativeRecord, nativeBuffer, 16384, &nativeResult).ShouldBe(0);
        Marshal.PtrToStringUTF8((nint)result->Name).ShouldBe(Marshal.PtrToStringUTF8((nint)nativeResult->Name));
        Marshal.PtrToStringUTF8((nint)result->Directory).ShouldBe(Marshal.PtrToStringUTF8((nint)nativeResult->Directory));
        result->Gid.ShouldBe(nativeResult->Gid);
        // A separate borrowed/static lookup must not overwrite caller storage.
        string saved = Marshal.PtrToStringUTF8((nint)result->Name)!;
        getpwuid(getuid()); Marshal.PtrToStringUTF8((nint)result->Name).ShouldBe(saved);
        buffer[0].ShouldBe((byte)0xa5); buffer[16385].ShouldBe((byte)0xa5);
        new ReadOnlySpan<byte>(records, sizeof(PasswordRecord)).ToArray().ShouldAllBe(x => x == 0xa5);
        new ReadOnlySpan<byte>(records + 2, sizeof(PasswordRecord)).ToArray().ShouldAllBe(x => x == 0xa5);
        int missingError = NativePasswd(uint.MaxValue, &nativeRecord, nativeBuffer, 16384, &nativeResult);
        (missingError == 0 || missingError == ENOENT).ShouldBeTrue();
        ((nint)nativeResult).ShouldBe(0);
        result = (PasswordRecord*)1;
        getpwuid_r(uint.MaxValue, records + 1, buffer + 1, 16384, &result).ShouldBe(missingError);
        ((nint)result).ShouldBe(0);
        errno.ShouldBe(EIO);
    }

    [Fact]
    public void Reentrant_NSS_null_arguments_return_numeric_EFAULT_without_writing_guest_errno()
    {
        byte* buffer = stackalloc byte[16]; PasswordRecord record; void* result = (void*)1;
        errno = EIO;
        getpwuid_r(0, &record, buffer, 16, null).ShouldBe(EFAULT);
        getpwuid_r(0, null, buffer, 16, &result).ShouldBe(EFAULT); ((nint)result).ShouldBe(0);
        getpwuid_r(0, &record, null, 16, &result).ShouldBe(EFAULT); errno.ShouldBe(EIO);
    }

    [DllImport("libc", EntryPoint = "shm_open", SetLastError = true)] private static extern int NativeShmOpen(byte* name, int flags, uint mode);
    [DllImport("libc", EntryPoint = "shm_unlink", SetLastError = true)] private static extern int NativeShmUnlink(byte* name);
    [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)] private static extern int NativeFcntl(int fd, int command);
    [DllImport("libc", EntryPoint = "close")] private static extern int NativeClose(int fd);
    [DllImport("libc", EntryPoint = "posix_openpt", SetLastError = true)] private static extern int NativeOpenPty(int flags);
    [DllImport("libc", EntryPoint = "grantpt", SetLastError = true)] private static extern int NativeGrantPty(int fd);
    [DllImport("libc", EntryPoint = "unlockpt", SetLastError = true)] private static extern int NativeUnlockPty(int fd);
    [DllImport("libc", EntryPoint = "ptsname_r")] private static extern int NativePtyName(int fd, byte* buffer, ulong size);
    [DllImport("libc", EntryPoint = "tcgetattr", SetLastError = true)] private static extern int NativeGetAttr(int fd, void* buffer);
    [DllImport("libc", EntryPoint = "tcsetattr", SetLastError = true)] private static extern int NativeSetAttr(int fd, int action, void* buffer);
    [DllImport("libc", EntryPoint = "getpwuid_r")] private static extern int NativePasswd(uint uid, PasswordRecord* record, byte* buffer, ulong size, PasswordRecord** result);
}

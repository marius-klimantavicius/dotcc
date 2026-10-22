using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PglitePosixIoTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct IOVec { public void* Base; public ulong Length; }

    private static int Open(string path, int flags)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* name = bytes) return open(name, flags);
    }

    [Fact]
    public void Positioned_scalar_io_preserves_offset_short_reads_eof_and_canaries()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, "0123456789"u8.ToArray());
            using var owner = new RuntimeContext();
            using var binding = owner.Enter();
            int fd = Open(path, 2);
            fd.ShouldBeGreaterThanOrEqualTo(0);
            try
            {
                lseek(fd, 4, 0).ShouldBe(4);
                byte* buffer = stackalloc byte[8];
                new Span<byte>(buffer, 8).Fill(0x5a);
                errno = EIO;
                pread(fd, buffer + 1, 3, 1).ShouldBe(3);
                new ReadOnlySpan<byte>(buffer + 1, 3).SequenceEqual("123"u8).ShouldBeTrue();
                buffer[0].ShouldBe((byte)0x5a);
                buffer[4].ShouldBe((byte)0x5a);
                errno.ShouldBe(EIO);
                lseek(fd, 0, 1).ShouldBe(4);
                fixed (byte* bytes = "xy"u8) pwrite(fd, bytes, 2, 6).ShouldBe(2);
                lseek(fd, 0, 1).ShouldBe(4);
                read(fd, buffer, 1).ShouldBe(1);
                buffer[0].ShouldBe((byte)'4');
                lseek(fd, 0, 1).ShouldBe(5);
                pread(fd, buffer, 8, 9).ShouldBe(1);
                buffer[0].ShouldBe((byte)'9');
                pread(fd, buffer, 8, 100).ShouldBe(0);
                File.ReadAllBytes(path).ShouldBe("012345xy89"u8.ToArray());
            }
            finally { close(fd).ShouldBe(0); }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Positioned_io_retains_64_bit_offsets_without_allocating_a_dense_file()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext();
            using var binding = owner.Enter();
            int fd = Open(path, 2);
            try
            {
                long offset = (1L << 32) + 19;
                byte value = 0xc3, result = 0;
                lseek(fd, 7, 0).ShouldBe(7);
                pwrite(fd, &value, 1, offset).ShouldBe(1);
                pread(fd, &result, 1, offset).ShouldBe(1);
                result.ShouldBe(value);
                lseek(fd, 0, 1).ShouldBe(7);
                new FileInfo(path).Length.ShouldBe(offset + 1);
            }
            finally { close(fd).ShouldBe(0); }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Positioned_vectors_scatter_gather_and_preserve_a_short_read_boundary()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext();
            using var binding = owner.Enter();
            int fd = Open(path, 2);
            try
            {
                byte* a = stackalloc byte[] { 10, 11, 12 };
                byte* b = stackalloc byte[] { 13, 14, 15, 16 };
                IOVec* vectors = stackalloc IOVec[3];
                vectors[0] = new() { Base = a, Length = 3 };
                vectors[1] = new() { Base = null, Length = 0 };
                vectors[2] = new() { Base = b, Length = 4 };
                lseek(fd, 2, 0).ShouldBe(2);
                pwritev(fd, vectors, 3, 5).ShouldBe(7);
                lseek(fd, 0, 1).ShouldBe(2);
                new Span<byte>(a, 3).Fill(99); new Span<byte>(b, 4).Fill(99);
                preadv(fd, vectors, 3, 8).ShouldBe(4);
                new ReadOnlySpan<byte>(a, 3).SequenceEqual(new byte[] { 13, 14, 15 }).ShouldBeTrue();
                b[0].ShouldBe((byte)16); b[1].ShouldBe((byte)99);
                lseek(fd, 0, 1).ShouldBe(2);
                preadv(fd, null, 0, 0).ShouldBe(0);
                pwritev(fd, null, 0, 0).ShouldBe(0);
            }
            finally { close(fd).ShouldBe(0); }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Positioned_services_report_invalid_arguments_access_modes_and_nonseekable_descriptors()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext();
            using var binding = owner.Enter();
            byte value = 1;
            pread(-1, &value, 1, 0).ShouldBe(-1); errno.ShouldBe(EBADF);
            int fd = Open(path, 2), ro = Open(path, 0), wo = Open(path, 1);
            try
            {
                pwrite(ro, &value, 1, 0).ShouldBe(-1); errno.ShouldBe(EBADF);
                pread(wo, &value, 1, 0).ShouldBe(-1); errno.ShouldBe(EBADF);
                pread(fd, &value, 1, -1).ShouldBe(-1); errno.ShouldBe(EINVAL);
                pread(fd, null, 1, 0).ShouldBe(-1); errno.ShouldBe(EFAULT);
                preadv(fd, null, 1, 0).ShouldBe(-1); errno.ShouldBe(EFAULT);
                preadv(fd, null, 1025, 0).ShouldBe(-1); errno.ShouldBe(EINVAL);
                IOVec entry = new() { Base = &value, Length = ulong.MaxValue };
                pwritev(fd, &entry, 1, 0).ShouldBe(-1); errno.ShouldBe(EINVAL);
                entry.Length = 1; entry.Base = null;
                pwritev(fd, &entry, 1, 0).ShouldBe(-1); errno.ShouldBe(EFAULT);
                pwrite(fd, &value, 1, long.MaxValue).ShouldBe(-1); errno.ShouldBe(EINVAL);
                int* descriptors = stackalloc int[2];
                pipe(descriptors).ShouldBe(0);
                try
                {
                    pread(descriptors[0], &value, 1, 0).ShouldBe(-1); errno.ShouldBe(ESPIPE);
                    pwrite(descriptors[1], &value, 1, 0).ShouldBe(-1); errno.ShouldBe(ESPIPE);
                }
                finally { close(descriptors[0]); close(descriptors[1]); }
            }
            finally { close(fd); close(ro); close(wo); }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Concurrent_positioned_writes_share_an_owner_without_racing_the_descriptor_position()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext();
            using var binding = owner.Enter();
            int fd = Open(path, 2);
            try
            {
                lseek(fd, 7, 0).ShouldBe(7);
                using var start = new Barrier(4);
                var failures = new ConcurrentQueue<Exception>();
                var workers = new Thread[4];
                for (int worker = 0; worker < workers.Length; worker++)
                {
                    int index = worker;
                    workers[worker] = new Thread(() =>
                    {
                        try
                        {
                            using var scope = owner.Enter();
                            long value;
                            start.SignalAndWait(TimeSpan.FromSeconds(10)).ShouldBeTrue();
                            for (int n = 0; n < 200; n++)
                            {
                                value = index * 200 + n + 1;
                                pwrite(fd, &value, 8, (value - 1) * 8).ShouldBe(8);
                            }
                        }
                        catch (Exception error) { failures.Enqueue(error); }
                    });
                    workers[worker].Start();
                }
                foreach (var worker in workers) worker.Join(TimeSpan.FromSeconds(20)).ShouldBeTrue();
                failures.ShouldBeEmpty();
                lseek(fd, 0, 1).ShouldBe(7);
                byte[] content = File.ReadAllBytes(path);
                content.Length.ShouldBe(800 * 8);
                for (int index = 0; index < 800; index++) BitConverter.ToInt64(content, index * 8).ShouldBe(index + 1);
            }
            finally { close(fd).ShouldBe(0); }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Identical_descriptor_numbers_in_different_owners_use_different_files()
    {
        string a = Path.GetTempFileName(), b = Path.GetTempFileName();
        try
        {
            using var first = new RuntimeContext(); using var second = new RuntimeContext();
            using var outer = first.Enter();
            int firstFd = Open(a, 2);
            try
            {
                byte value = 1;
                pwrite(firstFd, &value, 1, 0).ShouldBe(1);
                using (second.Enter())
                {
                    int secondFd = Open(b, 2);
                    try
                    {
                        secondFd.ShouldBe(firstFd);
                        value = 2;
                        pwrite(secondFd, &value, 1, 0).ShouldBe(1);
                    }
                    finally { close(secondFd); }
                }
                value = 0; pread(firstFd, &value, 1, 0).ShouldBe(1); value.ShouldBe((byte)1);
                File.ReadAllBytes(b).ShouldBe(new byte[] { 2 });
            }
            finally { close(firstFd); }
        }
        finally { File.Delete(a); File.Delete(b); }
    }

    [Fact]
    public void Linux_filesystem_sync_services_match_real_host_calls_and_validate_errors()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            int fd = Open(path, 2);
            try
            {
                byte value = 42; pwrite(fd, &value, 1, 0).ShouldBe(1);
                lseek(fd, 3, 0).ShouldBe(3);
                if (OperatingSystem.IsLinux())
                {
                    using var native = File.OpenHandle(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                    int expected = SyncRange(native, 0, 1, 7), error = Marshal.GetLastPInvokeError();
                    sync_file_range(fd, 0, 1, 7).ShouldBe(expected);
                    if (expected != 0) errno.ShouldBe(error);
                    expected = SyncFilesystem(native); error = Marshal.GetLastPInvokeError();
                    syncfs(fd).ShouldBe(expected);
                    if (expected != 0) errno.ShouldBe(error);
                }
                else
                {
                    syncfs(fd).ShouldBe(-1); errno.ShouldBe(ENOTSUP);
                    sync_file_range(fd, 0, 1, 2).ShouldBe(-1); errno.ShouldBe(ENOTSUP);
                }
                lseek(fd, 0, 1).ShouldBe(3);
                sync_file_range(fd, -1, 1, 0).ShouldBe(-1); errno.ShouldBe(EINVAL);
                sync_file_range(fd, 0, 1, 8).ShouldBe(-1); errno.ShouldBe(EINVAL);
                syncfs(-1).ShouldBe(-1); errno.ShouldBe(EBADF);
            }
            finally { close(fd).ShouldBe(0); }
        }
        finally { File.Delete(path); }
    }

    [DllImport("libc", EntryPoint = "syncfs", SetLastError = true)]
    private static extern int SyncFilesystem(SafeFileHandle handle);
    [DllImport("libc", EntryPoint = "sync_file_range", SetLastError = true)]
    private static extern int SyncRange(SafeFileHandle handle, long offset, long count, uint flags);
}

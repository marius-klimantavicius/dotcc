using System;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PipeMetadataTests
{
    private static byte[] Metadata(int fd)
    {
        byte[] guarded = new byte[112];
        Array.Fill(guarded, (byte)0xa5);
        fixed (byte* p = guarded)
        {
            errno = 123;
            fstat(fd, p + 8).ShouldBe(0);
            errno.ShouldBe(123);
            for (int i = 0; i < 8; i++)
            {
                p[i].ShouldBe((byte)0xa5);
                p[104 + i].ShouldBe((byte)0xa5);
            }
        }
        return guarded.AsSpan(8, 96).ToArray();
    }

    [Fact]
    public void Both_ends_describe_one_real_managed_FIFO_and_fill_exactly_target_96_bytes()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        pipe(fds).ShouldBe(0);
        byte[] metadata = Metadata(fds[0]);
        metadata.ShouldBe(Metadata(fds[1]));
        fixed (byte* p = metadata)
        {
            (*(ulong*)(p + 0)).ShouldBe(0UL); // no host device
            (*(ulong*)(p + 8)).ShouldBeGreaterThan(0UL); // virtual queue identity
            (*(uint*)(p + 16)).ShouldBe(0x1180U); // FIFO | 0600
            (*(uint*)(p + 20)).ShouldBe(0U); // logical ABI padding
            (*(ulong*)(p + 24)).ShouldBe(1UL);
            (*(uint*)(p + 32)).ShouldBe(geteuid());
            (*(uint*)(p + 36)).ShouldBe(getegid());
            (*(long*)(p + 40)).ShouldBe(0L);
            long created = *(long*)(p + 64);
            created.ShouldBeGreaterThanOrEqualTo(before);
            created.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            (*(long*)(p + 48)).ShouldBe(created);
            (*(long*)(p + 56)).ShouldBe(created);
            (*(ulong*)(p + 72)).ShouldBe(0UL);
            (*(long*)(p + 80)).ShouldBe(4096L); // managed PIPE_BUF
            (*(long*)(p + 88)).ShouldBe(0L);
        }
    }

    [Fact]
    public void FileIO_descriptor_flags_and_nonseekable_metadata_prerequisites_hold_for_both_ends()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        for (int end = 0; end < 2; end++)
        {
            fcntl(fds[end], 1).ShouldBe(0);
            fcntl(fds[end], 2, 1).ShouldBe(0); // _Py_set_inheritable(..., 0)
            Metadata(fds[end]);
            fcntl(fds[end], 1).ShouldBe(1);
            (fcntl(fds[end], 3) & 3).ShouldBe(end);
            lseek(fds[end], 0, SEEK_CUR).ShouldBe(-1);
            errno.ShouldBe(ESPIPE);
        }
    }

    [Fact]
    public void Data_and_endpoint_close_preserve_FIFO_identity_size_and_ownership()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        byte[] initial = Metadata(fds[0]);
        byte value = 0x5a;
        write(fds[1], &value, 1).ShouldBe(1);
        Metadata(fds[0]).ShouldBe(Metadata(fds[1]));
        byte[] queued = Metadata(fds[0]);
        queued.AsSpan(0, 48).ToArray().ShouldBe(initial.AsSpan(0, 48).ToArray());
        read(fds[0], &value, 1).ShouldBe(1);
        value.ShouldBe((byte)0x5a);
        byte[] drained = Metadata(fds[0]);
        close(fds[1]).ShouldBe(0);
        Metadata(fds[0]).ShouldBe(drained);
        read(fds[0], &value, 1).ShouldBe(0);
        lseek(fds[0], 0, SEEK_CUR).ShouldBe(-1);
        errno.ShouldBe(ESPIPE);
        ftruncate(fds[0], 1).ShouldBe(-1);
        errno.ShouldBe(EINVAL);
    }

    [Fact]
    public void Null_closed_invalid_and_foreign_descriptors_report_errors_without_writes()
    {
        using var owner = new RuntimeContext();
        using var foreign = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        byte* untouched = stackalloc byte[96];
        new Span<byte>(untouched, 96).Fill(0xa5);
        fstat(fds[0], null).ShouldBe(-1); errno.ShouldBe(EFAULT);
        fstat(-1, untouched).ShouldBe(-1); errno.ShouldBe(EBADF);
        using (foreign.Enter())
        {
            fstat(fds[0], untouched).ShouldBe(-1); errno.ShouldBe(EBADF);
            close(fds[0]).ShouldBe(-1); errno.ShouldBe(EBADF);
        }
        close(fds[0]).ShouldBe(0);
        fstat(fds[0], untouched).ShouldBe(-1); errno.ShouldBe(EBADF);
        for (int i = 0; i < 96; i++) untouched[i].ShouldBe((byte)0xa5);
        Metadata(fds[1]);
    }

    [Fact]
    public void Reused_and_colliding_owner_slots_keep_distinct_queue_identities()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        using var scope = first.Enter();
        int* a = stackalloc int[2];
        int* b = stackalloc int[2];
        pipe(a).ShouldBe(0);
        byte[] original = Metadata(a[0]);
        using (second.Enter())
        {
            pipe(b).ShouldBe(0);
            b[0].ShouldBe(a[0]); b[1].ShouldBe(a[1]);
            Metadata(b[0]).AsSpan(0, 16).ToArray().ShouldNotBe(original.AsSpan(0, 16).ToArray());
        }
        second.Dispose(); // closes only its own pipe, despite the same fd values
        Metadata(a[0]).ShouldBe(original);
        close(a[0]).ShouldBe(0); close(a[1]).ShouldBe(0);
        pipe(b).ShouldBe(0);
        b[0].ShouldBe(a[0]); b[1].ShouldBe(a[1]);
        Metadata(b[0]).AsSpan(0, 16).ToArray().ShouldNotBe(original.AsSpan(0, 16).ToArray());
    }

    [Fact]
    public void Closing_reader_leaves_writer_metadata_valid_and_no_filesystem_name_is_created()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        byte[] initial = Metadata(fds[1]);
        close(fds[0]).ShouldBe(0);
        Metadata(fds[1]).ShouldBe(initial);
        byte marker = 1;
        write(fds[1], &marker, 1).ShouldBe(-1); errno.ShouldBe(EPIPE);
        // Anonymous metadata is fd-only. It never creates a pathname entry.
        fixed (byte* name = Encoding.UTF8.GetBytes("/tmp/dotcc-no-pipe-entry-" + Guid.NewGuid().ToString("N") + '\0'))
        fixed (byte* metadata = initial)
        {
            lstat(name, metadata).ShouldBe(-1); errno.ShouldBe(ENOENT);
        }
    }
}

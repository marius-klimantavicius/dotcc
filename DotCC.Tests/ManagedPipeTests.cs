using System;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class ManagedPipeTests
{
    private struct PollFd { public int Fd; public short Events, Revents; }

    [Fact]
    public void Pipe_reads_available_bytes_and_reports_nonblocking_empty_then_drained_eof()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        fcntl(fds[0], 3).ShouldBe(0);
        fcntl(fds[1], 3).ShouldBe(1);
        fcntl(fds[0], 4, 0x800).ShouldBe(0);
        byte* bytes = stackalloc byte[8];
        read(fds[0], bytes, 8).ShouldBe(-1);
        errno.ShouldBe(EAGAIN);
        bytes[0] = 42; bytes[1] = 81;
        write(fds[1], bytes, 2).ShouldBe(2);
        read(fds[0], bytes, 8).ShouldBe(2);
        bytes[0].ShouldBe((byte)42); bytes[1].ShouldBe((byte)81);
        close(fds[1]).ShouldBe(0);
        read(fds[0], bytes, 8).ShouldBe(0);
        close(fds[0]).ShouldBe(0);
    }

    [Fact]
    public void Pipe_validates_arguments_and_endpoint_direction()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        pipe(null).ShouldBe(-1); errno.ShouldBe(EFAULT);
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        byte b;
        read(fds[1], &b, 1).ShouldBe(-1); errno.ShouldBe(EBADF);
        write(fds[0], &b, 1).ShouldBe(-1); errno.ShouldBe(EBADF);
        read(fds[0], null, 0).ShouldBe(0);
        write(fds[1], null, 0).ShouldBe(0);
        read(fds[0], null, 1).ShouldBe(-1); errno.ShouldBe(EFAULT);
        write(fds[1], null, 1).ShouldBe(-1); errno.ShouldBe(EFAULT);
        close(fds[0]).ShouldBe(0);
        write(fds[1], &b, 1).ShouldBe(-1); errno.ShouldBe(EPIPE);
    }

    [Fact]
    public void Bounded_pipe_preserves_atomic_small_writes_and_supports_partial_large_writes()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        fcntl(fds[1], 4, 0x800).ShouldBe(0);
        byte* data = stackalloc byte[65536];
        new Span<byte>(data, 65536).Fill(19);
        write(fds[1], data, 65536).ShouldBe(65536);
        PollFd writable = new() { Fd = fds[1], Events = 4 };
        poll(&writable, 1, 0).ShouldBe(0);
        write(fds[1], data, 1).ShouldBe(-1); errno.ShouldBe(EAGAIN);
        read(fds[0], data, 2000).ShouldBe(2000);
        poll(&writable, 1, 0).ShouldBe(0);
        write(fds[1], data, 4096).ShouldBe(-1); errno.ShouldBe(EAGAIN);
        write(fds[1], data, 8192).ShouldBe(2000);
        read(fds[0], data, 65536).ShouldBe(65536);
        poll(&writable, 1, 0).ShouldBe(1);
        writable.Revents.ShouldBe((short)4);
        for (int i = 0; i < 65536; ++i) data[i].ShouldBe((byte)19);
    }

    [Fact]
    public void Poll_reports_real_readiness_and_hangup_and_broken_reader()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        PollFd* checks = stackalloc PollFd[2];
        checks[0] = new() { Fd = fds[0], Events = 1 };
        checks[1] = new() { Fd = fds[1], Events = 4 };
        poll(checks, 2, 0).ShouldBe(1);
        checks[0].Revents.ShouldBe((short)0); checks[1].Revents.ShouldBe((short)4);
        byte b = 7;
        write(fds[1], &b, 1).ShouldBe(1);
        close(fds[1]).ShouldBe(0);
        poll(checks, 1, 0).ShouldBe(1);
        checks[0].Revents.ShouldBe((short)17);
        read(fds[0], &b, 1).ShouldBe(1);
        poll(checks, 1, 0).ShouldBe(1);
        checks[0].Revents.ShouldBe((short)16);
        close(fds[0]).ShouldBe(0);
        pipe(fds).ShouldBe(0);
        close(fds[0]).ShouldBe(0);
        checks[0] = new() { Fd = fds[1], Events = 4 };
        poll(checks, 1, 0).ShouldBe(1);
        checks[0].Revents.ShouldBe((short)12);
    }

    [Fact]
    public void Blocking_reader_wakes_on_write_and_blocked_writer_wakes_on_reader_close()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        int input = fds[0], output = fds[1];
        using var entered = new ManualResetEventSlim();
        long result = -99; int resultErrno = 0; byte received = 0;
        var reader = new Thread(() =>
        {
            using var scope = owner.Enter();
            byte b;
            entered.Set();
            result = read(input, &b, 1); received = b;
        });
        reader.Start();
        try
        {
            entered.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken).ShouldBeTrue();
            byte b = 38;
            write(output, &b, 1).ShouldBe(1);
            reader.Join(5000).ShouldBeTrue();
            result.ShouldBe(1); received.ShouldBe((byte)38);
        }
        finally { if (reader.IsAlive) { close(output); reader.Join(5000); } }
        byte* full = stackalloc byte[65536];
        write(output, full, 65536).ShouldBe(65536);
        entered.Reset();
        var writer = new Thread(() =>
        {
            using var scope = owner.Enter();
            byte b = 1;
            entered.Set();
            result = write(output, &b, 1); resultErrno = errno;
        });
        writer.Start();
        try
        {
            entered.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken).ShouldBeTrue();
            close(input).ShouldBe(0);
            writer.Join(5000).ShouldBeTrue();
            result.ShouldBe(-1); resultErrno.ShouldBe(EPIPE);
        }
        finally { if (writer.IsAlive) { close(output); writer.Join(5000); } }
    }
}

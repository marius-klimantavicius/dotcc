using System;
using System.Diagnostics;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class ManagedSelectTests
{
    private struct Timeval { public long Seconds, Microseconds; }

    [Fact]
    public void Fd_sets_cover_all_1024_bits_and_do_not_touch_adjacent_memory()
    {
        long* storage = stackalloc long[18];
        new Span<long>(storage, 18).Fill(-1);
        long* set = storage + 1;
        FD_ZERO(set);
        storage[0].ShouldBe(-1); storage[17].ShouldBe(-1);
        foreach (int fd in new[] { 0, 63, 64, 511, 1023 }) FD_SET(fd, set);
        for (int fd = 0; fd < 1024; fd++)
            FD_ISSET(fd, set).ShouldBe(fd is 0 or 63 or 64 or 511 or 1023 ? 1 : 0);
        FD_CLR(64, set); FD_ISSET(64, set).ShouldBe(0);
        FD_ISSET(63, set).ShouldBe(1); FD_ISSET(1023, set).ShouldBe(1);
        storage[0].ShouldBe(-1); storage[17].ShouldBe(-1);
    }

    [Fact]
    public void Select_counts_ready_bits_across_sets_and_clears_unready_descriptors()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2];
        pipe(fds).ShouldBe(0);
        var file = tmpfile();
        int regular = fileno(file);
        long* readset = stackalloc long[16]; long* writeset = stackalloc long[16];
        FD_ZERO(readset); FD_ZERO(writeset);
        FD_SET(fds[0], readset); FD_SET(regular, readset);
        FD_SET(fds[1], writeset); FD_SET(regular, writeset);
        Timeval timeout = default;
        select(regular + 1, readset, writeset, null, &timeout).ShouldBe(3);
        FD_ISSET(fds[0], readset).ShouldBe(0);
        FD_ISSET(regular, readset).ShouldBe(1);
        FD_ISSET(fds[1], writeset).ShouldBe(1);
        FD_ISSET(regular, writeset).ShouldBe(1);
    }

    [Fact]
    public void Select_validates_nfds_timeval_and_closed_descriptors()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        Timeval timeout = default;
        select(-1, null, null, null, &timeout).ShouldBe(-1); errno.ShouldBe(EINVAL);
        select(1025, null, null, null, &timeout).ShouldBe(-1); errno.ShouldBe(EINVAL);
        timeout.Microseconds = 1000000;
        select(0, null, null, null, &timeout).ShouldBe(-1); errno.ShouldBe(EINVAL);
        timeout.Microseconds = -1;
        select(0, null, null, null, &timeout).ShouldBe(-1); errno.ShouldBe(EINVAL);
        timeout = default;
        long* set = stackalloc long[16]; FD_ZERO(set); FD_SET(14, set);
        select(15, set, null, null, &timeout).ShouldBe(-1); errno.ShouldBe(EBADF);
    }

    [Fact]
    public void Empty_select_waits_and_consumes_timeout_including_submillisecond_requests()
    {
        Timeval timeout = new() { Microseconds = 30000 };
        var elapsed = Stopwatch.StartNew();
        select(0, null, null, null, &timeout).ShouldBe(0);
        elapsed.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(25);
        timeout.Seconds.ShouldBe(0); timeout.Microseconds.ShouldBe(0);
        timeout.Microseconds = 500;
        select(0, null, null, null, &timeout).ShouldBe(0);
        timeout.Microseconds.ShouldBe(0);
    }

    [Fact]
    public void Select_waits_for_pipe_writer_and_treats_drained_eof_as_readable()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2]; pipe(fds).ShouldBe(0);
        int input = fds[0], output = fds[1];
        using var start = new ManualResetEventSlim();
        long* set = stackalloc long[16]; FD_ZERO(set); FD_SET(input, set);
        var writer = new Thread(() =>
        {
            using var scope = owner.Enter();
            start.Wait();
            byte value = 55; write(output, &value, 1);
            close(output);
        });
        writer.Start();
        try
        {
            Timeval timeout = new() { Seconds = 5 };
            start.Set();
            select(input + 1, set, null, null, &timeout).ShouldBe(1);
            FD_ISSET(input, set).ShouldBe(1);
            byte value; read(input, &value, 1).ShouldBe(1); value.ShouldBe((byte)55);
            writer.Join(5000).ShouldBeTrue();
            timeout = default;
            select(input + 1, set, null, null, &timeout).ShouldBe(1);
            read(input, &value, 1).ShouldBe(0);
        }
        finally { start.Set(); writer.Join(5000); }
    }

    [Fact]
    public void Pipe_hangup_does_not_report_exceptional_data_or_spin_forever()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int* fds = stackalloc int[2]; pipe(fds).ShouldBe(0); close(fds[1]);
        long* except = stackalloc long[16]; FD_ZERO(except); FD_SET(fds[0], except);
        Timeval timeout = new() { Microseconds = 20000 };
        select(fds[0] + 1, null, null, except, &timeout).ShouldBe(0);
        FD_ISSET(fds[0], except).ShouldBe(0);
    }

    [Fact]
    public void Select_combines_loopback_socket_and_managed_pipe_readiness()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        int server = socket(2, 1, 0);
        byte* address = stackalloc byte[16];
        new Span<byte>(address, 16).Clear();
        *(ushort*)address = 2; address[4] = 127; address[7] = 1;
        bind(server, address, 16).ShouldBe(0); listen(server, 2).ShouldBe(0);
        uint length = 16; getsockname(server, address, &length).ShouldBe(0);
        int client = socket(2, 1, 0);
        connect(client, address, 16).ShouldBe(0);
        int accepted = accept(server, null, null);
        accepted.ShouldBeGreaterThanOrEqualTo(0);
        byte value = 22; send(accepted, &value, 1, 0).ShouldBe(1);
        long* readers = stackalloc long[16]; FD_ZERO(readers); FD_SET(client, readers);
        Timeval timeout = new() { Seconds = 5 };
        select(client + 1, readers, null, null, &timeout).ShouldBe(1);
        int* fds = stackalloc int[2]; pipe(fds).ShouldBe(0);
        write(fds[1], &value, 1).ShouldBe(1); FD_SET(fds[0], readers);
        timeout = default;
        select(Math.Max(client, fds[0]) + 1, readers, null, null, &timeout).ShouldBe(2);
        FD_ISSET(client, readers).ShouldBe(1); FD_ISSET(fds[0], readers).ShouldBe(1);
    }
}

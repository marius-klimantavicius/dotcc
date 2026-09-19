using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcSocketReadinessTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd { public int Fd; public short Events, Revents; }
    private const int Nonblock = 0x800;

    [Fact]
    public void Socket_creation_and_fcntl_control_nonblocking_and_descriptor_flags()
    {
        int fd = socket(2, 2 | Nonblock | 0x80000, 0);
        fd.ShouldBeGreaterThanOrEqualTo(0);
        try
        {
            fcntl(fd, 3).ShouldBe(2 | Nonblock);
            fcntl(fd, 1).ShouldBe(1);
            fcntl(fd, 2, 0).ShouldBe(0);
            fcntl(fd, 1).ShouldBe(0);
            byte value;
            recv(fd, &value, 1, 0).ShouldBe(-1);
            errno.ShouldBe(EAGAIN);
            fcntl(fd, 4, 0).ShouldBe(0);
            fcntl(fd, 3).ShouldBe(2);
            fcntl(fd, 4, Nonblock).ShouldBe(0);
            fcntl(fd, 3).ShouldBe(2 | Nonblock);
            fcntl(fd, 999, 0).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
        }
        finally { close(fd); }
        fcntl(fd, 3).ShouldBe(-1);
        errno.ShouldBe(EBADF);
    }

    [Fact]
    public void Poll_ignores_negative_fds_and_reports_invalid_fds_without_requested_events()
    {
        PollFd* fds = stackalloc PollFd[2];
        fds[0] = new PollFd { Fd = -1, Events = 5, Revents = 127 };
        fds[1] = new PollFd { Fd = int.MaxValue, Events = 0 };
        poll(fds, 2, 5000).ShouldBe(1);
        fds[0].Revents.ShouldBe((short)0);
        fds[1].Revents.ShouldBe((short)0x20);
    }

    [Fact]
    public void Poll_empty_set_waits_for_timeout()
    {
        var elapsed = Stopwatch.StartNew();
        poll(null, 0, 40).ShouldBe(0);
        elapsed.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(25);
    }

    [Fact]
    public void Poll_mixes_regular_files_with_unreadable_socket_and_counts_ready_entries()
    {
        var file = tmpfile();
        ((nint)file).ShouldNotBe(0);
        int fd = socket(2, 2, 0);
        fd.ShouldBeGreaterThanOrEqualTo(0);
        try
        {
            PollFd* fds = stackalloc PollFd[3];
            fds[0] = new PollFd { Fd = fileno(file), Events = 5 };
            fds[1] = new PollFd { Fd = fd, Events = 1 };
            fds[2] = new PollFd { Fd = -1, Events = 5 };
            poll(fds, 3, 5000).ShouldBe(1);
            fds[0].Revents.ShouldBe((short)5);
            fds[1].Revents.ShouldBe((short)0);
            fds[2].Revents.ShouldBe((short)0);
        }
        finally { close(fd); fclose(file); }
    }

    [Fact]
    public void Fcntl_append_affects_file_writes_after_seek_and_can_be_cleared()
    {
        var file = tmpfile();
        ((nint)file).ShouldNotBe(0);
        try
        {
            int fd = fileno(file);
            fcntl(fd, 3).ShouldBe(2);
            byte value = 1;
            write(fd, &value, 1).ShouldBe(1);
            fcntl(fd, 4, 0x400).ShouldBe(0);
            lseek(fd, 0, 0).ShouldBe(0);
            value = 2;
            write(fd, &value, 1).ShouldBe(1);
            lseek(fd, 0, 2).ShouldBe(2);
            fcntl(fd, 4, 0).ShouldBe(0);
            lseek(fd, 0, 0).ShouldBe(0);
            value = 3;
            write(fd, &value, 1).ShouldBe(1);
            lseek(fd, 0, 0).ShouldBe(0);
            byte* contents = stackalloc byte[2];
            read(fd, contents, 2).ShouldBe(2);
            contents[0].ShouldBe((byte)3);
            contents[1].ShouldBe((byte)2);
        }
        finally { fclose(file); }
    }

    [Fact]
    public void Nonblocking_connection_failure_is_reported_by_so_error_then_cleared()
    {
        int reserved = socket(2, 1, 0), client = socket(2, 1 | Nonblock, 0);
        try
        {
            byte* address = stackalloc byte[16];
            new Span<byte>(address, 16).Clear();
            *(ushort*)address = 2; address[4] = 127; address[7] = 1;
            bind(reserved, address, 16).ShouldBe(0); // bound, deliberately not listening
            uint length = 16;
            getsockname(reserved, address, &length).ShouldBe(0);
            fcntl(client, 3).ShouldBe(2 | Nonblock);
            connect(client, address, length).ShouldBe(-1);
            errno.ShouldBe(EINPROGRESS);
            PollFd p = new() { Fd = client, Events = 4 };
            poll(&p, 1, 5000).ShouldBe(1);
            int error = 0; uint errorSize = 4;
            getsockopt(client, 1, 4, &error, &errorSize).ShouldBe(0);
            error.ShouldBe(ECONNREFUSED);
            getsockopt(client, 1, 4, &error, &errorSize).ShouldBe(0);
            error.ShouldBe(0);
        }
        finally { close(client); close(reserved); }
    }

    [Fact]
    public void Open_append_preserves_readwrite_access_and_does_not_imply_create()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "dotcc-append-" + Guid.NewGuid().ToString("N"));
        var name = System.Text.Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* p = name)
        {
            open(p, 2 | 0x400).ShouldBe(-1);
            errno.ShouldBe(ENOENT);
            int fd = open(p, 2 | 0x400 | 0x40, 0x180);
            fd.ShouldBeGreaterThanOrEqualTo(0);
            try
            {
                fcntl(fd, 3).ShouldBe(2 | 0x400);
                byte value = 7, readBack = 0;
                write(fd, &value, 1).ShouldBe(1);
                lseek(fd, 0, 0).ShouldBe(0);
                read(fd, &readBack, 1).ShouldBe(1);
                readBack.ShouldBe(value);
            }
            finally { close(fd); System.IO.File.Delete(path); }
        }
    }

    [Theory]
    [InlineData(2, 16)]
    [InlineData(10, 28)]
    public void Nonblocking_connect_poll_send_recv_and_eof_roundtrip(int family, int size)
    {
        int server = socket(family, 1 | Nonblock, 0), client = -1, accepted = -1;
        server.ShouldBeGreaterThanOrEqualTo(0);
        try
        {
            byte* address = stackalloc byte[28];
            new Span<byte>(address, 28).Clear();
            *(ushort*)address = (ushort)family;
            if (family == 2) { address[4] = 127; address[7] = 1; }
            else address[23] = 1;
            bind(server, address, (uint)size).ShouldBe(0);
            listen(server, 4).ShouldBe(0);
            uint length = (uint)size;
            getsockname(server, address, &length).ShouldBe(0);
            length.ShouldBe((uint)size);
            client = socket(family, 1 | Nonblock, 0);
            client.ShouldBeGreaterThanOrEqualTo(0);
            fcntl(client, 3).ShouldBe(2 | Nonblock); // guard against a blocking baseline
            int connected = connect(client, address, length);
            (connected == 0 || connected == -1 && errno == EINPROGRESS).ShouldBeTrue();
            PollFd p = new() { Fd = client, Events = 4 };
            poll(&p, 1, 5000).ShouldBe(1);
            (p.Revents & 4).ShouldBe(4);
            int error = -1; uint errorSize = 4;
            getsockopt(client, 1, 4, &error, &errorSize).ShouldBe(0);
            error.ShouldBe(0);
            accepted = accept(server, null, null);
            accepted.ShouldBeGreaterThanOrEqualTo(0);
            fcntl(accepted, 3).ShouldBe(2); // Linux accept does not inherit O_NONBLOCK
            byte value = 91, received = 0;
            send(accepted, &value, 1, 0).ShouldBe(1);
            p.Events = 1;
            poll(&p, 1, 5000).ShouldBe(1);
            recv(client, &received, 1, 0).ShouldBe(1);
            received.ShouldBe(value);
            recv(client, &received, 1, 0).ShouldBe(-1);
            errno.ShouldBe(EAGAIN);
            shutdown(accepted, 1).ShouldBe(0);
            poll(&p, 1, 5000).ShouldBe(1);
            recv(client, &received, 1, 0).ShouldBe(0);
            byte* truncated = stackalloc byte[2]; length = 2;
            getpeername(client, truncated, &length).ShouldBe(0);
            length.ShouldBe((uint)size);
            (*(ushort*)truncated).ShouldBe((ushort)family);
        }
        finally { if (accepted >= 0) close(accepted); if (client >= 0) close(client); close(server); }
    }
}

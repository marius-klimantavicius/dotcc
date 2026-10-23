using System;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcSocketPairTests
{
    [Fact]
    public void Unnamed_stream_peers_exchange_binary_bytes_and_half_close()
    {
        if (!OperatingSystem.IsLinux()) return;
        int* pair = stackalloc int[2];
        socketpair(1, 1, 0, pair).ShouldBe(0);
        try
        {
            byte* sent = stackalloc byte[] { 0, 128, 255, 7 };
            byte* received = stackalloc byte[4];
            write(pair[0], sent, 4).ShouldBe(4);
            read(pair[1], received, 4).ShouldBe(4);
            new ReadOnlySpan<byte>(received, 4).SequenceEqual(new ReadOnlySpan<byte>(sent, 4)).ShouldBeTrue();
            send(pair[1], sent, 4, 0).ShouldBe(4);
            recv(pair[0], received, 4, 0).ShouldBe(4);
            shutdown(pair[0], 1).ShouldBe(0);
            read(pair[1], received, 4).ShouldBe(0);
        }
        finally { close(pair[0]).ShouldBe(0); close(pair[1]).ShouldBe(0); }
    }

    [Fact]
    public void Nonblocking_datagram_flags_and_failures_preserve_descriptor_contract()
    {
        if (!OperatingSystem.IsLinux()) return;
        int* pair = stackalloc int[] { 71, 72 };
        socketpair(2, 1, 0, pair).ShouldBe(-1);
        pair[0].ShouldBe(71); pair[1].ShouldBe(72);
        socketpair(1, 1, 0, null).ShouldBe(-1); errno.ShouldBe(EFAULT);
        socketpair(1, 2 | 0x800 | 0x80000, 0, pair).ShouldBe(0);
        try
        {
            fcntl(pair[0], 1).ShouldBe(1); // F_GETFD / FD_CLOEXEC
            (fcntl(pair[1], 3) & 0x800).ShouldBe(0x800); // F_GETFL / O_NONBLOCK
            byte* buffer = stackalloc byte[8];
            recv(pair[0], buffer, 8, 0).ShouldBe(-1); errno.ShouldBe(EAGAIN);
            buffer[0] = 5; buffer[1] = 9;
            send(pair[0], buffer, 2, 0).ShouldBe(2);
            recv(pair[1], buffer, 1, 0).ShouldBe(1); buffer[0].ShouldBe((byte)5);
            recv(pair[1], buffer, 8, 0).ShouldBe(-1); errno.ShouldBe(EAGAIN); // Datagram remainder discarded.
        }
        finally { close(pair[0]).ShouldBe(0); close(pair[1]).ShouldBe(0); }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcLingerTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Linger { public int OnOff, Seconds; }

    [Fact]
    public void Linger_roundtrips_and_short_set_rejects_without_changing_option()
    {
        int fd = socket(2, 1, 0);
        fd.ShouldBeGreaterThanOrEqualTo(0);
        try
        {
            Linger set = new() { OnOff = 1, Seconds = 7 }, got = default;
            setsockopt(fd, 1, 13, &set, 8).ShouldBe(0);
            uint length = 8;
            getsockopt(fd, 1, 13, &got, &length).ShouldBe(0);
            length.ShouldBe(8u); got.OnOff.ShouldBe(1); got.Seconds.ShouldBe(7);
            set.Seconds = 0;
            setsockopt(fd, 1, 13, &set, 7).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
            length = 8;
            getsockopt(fd, 1, 13, &got, &length).ShouldBe(0);
            got.Seconds.ShouldBe(7);
            got.Seconds = 1234; length = 4;
            getsockopt(fd, 1, 13, &got, &length).ShouldBe(0);
            length.ShouldBe(4u); got.OnOff.ShouldBe(1); got.Seconds.ShouldBe(1234);
            set.OnOff = 0;
            setsockopt(fd, 1, 13, &set, 8).ShouldBe(0);
            length = 8;
            getsockopt(fd, 1, 13, &got, &length).ShouldBe(0);
            got.OnOff.ShouldBe(0);
        }
        finally { close(fd); }
    }

    [Fact]
    public void Enabled_zero_linger_causes_peer_connection_reset_on_close()
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0)); listener.Listen(1);
        int fd = socket(2, 1, 0);
        fd.ShouldBeGreaterThanOrEqualTo(0);
        try
        {
            byte* address = stackalloc byte[16];
            new Span<byte>(address, 16).Clear();
            *(ushort*)address = 2;
            int port = ((IPEndPoint)listener.LocalEndPoint!).Port;
            address[2] = (byte)(port >> 8); address[3] = (byte)port;
            address[4] = 127; address[7] = 1;
            connect(fd, address, 16).ShouldBe(0);
            using var peer = listener.Accept();
            peer.ReceiveTimeout = 2000;
            Linger abortive = new() { OnOff = 1, Seconds = 0 };
            setsockopt(fd, 1, 13, &abortive, 8).ShouldBe(0);
            close(fd).ShouldBe(0); fd = -1;
            var error = Should.Throw<SocketException>(() => peer.Receive(new byte[1]));
            error.SocketErrorCode.ShouldBe(SocketError.ConnectionReset);
        }
        finally { if (fd >= 0) close(fd); }
    }
}

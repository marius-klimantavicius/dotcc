using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcNetworkServiceTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Iov { public byte* Base; public ulong Length; }

    [Theory]
    [InlineData("127.0.0.1", 2, 16)]
    [InlineData("::1", 10, 28)]
    public void Numeric_resolver_preserves_address_port_protocol_and_ownership(string text, int family, int length)
    {
        byte* hints = stackalloc byte[48];
        new Span<byte>(hints, 48).Clear();
        *(int*)hints = 4 | 1024;
        *(int*)(hints + 4) = family;
        *(int*)(hints + 8) = 1;
        byte* result = null;
        var host = System.Text.Encoding.UTF8.GetBytes(text + "\0");
        fixed (byte* node = host)
        fixed (byte* service = "445\0"u8)
            getaddrinfo(node, service, hints, &result).ShouldBe(0);
        try
        {
            ((nint)result).ShouldNotBe(0);
            (*(int*)(result + 4)).ShouldBe(family);
            (*(int*)(result + 8)).ShouldBe(1);
            (*(int*)(result + 12)).ShouldBe(6);
            (*(uint*)(result + 16)).ShouldBe((uint)length);
            ((nint)(*(void**)(result + 40))).ShouldBe(0);
            var address = *(byte**)(result + 24);
            (*(ushort*)address).ShouldBe((ushort)family);
            ((address[2] << 8) | address[3]).ShouldBe(445);
            new IPAddress(new ReadOnlySpan<byte>(address + (family == 2 ? 4 : 8), family == 2 ? 4 : 16)).ToString().ShouldBe(text);
        }
        finally { freeaddrinfo(result); }
        freeaddrinfo(null);
    }

    [Fact]
    public void Resolver_rejects_invalid_flags_names_and_ports_without_leaving_an_output()
    {
        byte* hints = stackalloc byte[48];
        new Span<byte>(hints, 48).Clear();
        byte* result = (byte*)123;
        *(int*)hints = 0x100000;
        fixed (byte* node = "127.0.0.1\0"u8)
        fixed (byte* service = "445\0"u8)
            getaddrinfo(node, service, hints, &result).ShouldBe(-1);
        ((nint)result).ShouldBe(0);
        *(int*)hints = 4;
        fixed (byte* node = "invalid!host\0"u8)
            getaddrinfo(node, null, hints, &result).ShouldBe(-2);
        ((nint)result).ShouldBe(0);
        fixed (byte* node = "127.0.0.1\0"u8)
        fixed (byte* service = "65536\0"u8)
            getaddrinfo(node, service, hints, &result).ShouldBe(-8);
        ((nint)result).ShouldBe(0);
        ((nint)gai_strerror(-1)).ShouldBe((nint)gai_strerror(-1));
        Marshal.PtrToStringUTF8((nint)gai_strerror(-2)).ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Localhost_dns_returns_owned_linked_results()
    {
        byte* result = null;
        fixed (byte* node = "localhost\0"u8)
        fixed (byte* service = "445\0"u8)
            getaddrinfo(node, service, null, &result).ShouldBe(0);
        try
        {
            int count = 0;
            for (var current = result; current != null; current = *(byte**)(current + 40))
            {
                (++count).ShouldBeLessThan(32);
                var family = *(int*)(current + 4);
                (family is 2 or 10).ShouldBeTrue();
                (*(int*)(current + 8)).ShouldBeOneOf(1, 2);
                var address = *(byte**)(current + 24);
                IPAddress.IsLoopback(new IPAddress(new ReadOnlySpan<byte>(address + (family == 2 ? 4 : 8), family == 2 ? 4 : 16))).ShouldBeTrue();
            }
            count.ShouldBeGreaterThan(0);
        }
        finally { freeaddrinfo(result); }
    }

    [Fact]
    public void Entropy_validates_lengths_flags_and_retains_buffer_guards()
    {
        byte* buffer = stackalloc byte[258];
        new Span<byte>(buffer, 258).Fill(0xa5);
        getrandom(buffer + 1, 256, 0).ShouldBe(256);
        buffer[0].ShouldBe((byte)0xa5);
        buffer[257].ShouldBe((byte)0xa5);
        var first = new Span<byte>(buffer + 1, 256).ToArray();
        arc4random_buf(buffer + 1, 256);
        new Span<byte>(buffer + 1, 256).SequenceEqual(first).ShouldBeFalse();
        getentropy(buffer, 257).ShouldBe(-1);
        errno.ShouldBe(EIO);
        getrandom(null, 1, 0).ShouldBe(-1);
        errno.ShouldBe(EFAULT);
        getrandom(buffer, 1, 8).ShouldBe(-1);
        errno.ShouldBe(EINVAL);
        getrandom(null, 0, 0).ShouldBe(0);
    }

    [Fact]
    public void Vector_socket_io_preserves_one_datagram_and_scatter_order()
    {
        using var peer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        peer.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        peer.ReceiveTimeout = 3000;
        int fd = socket(2, 2, 0);
        fd.ShouldBeGreaterThanOrEqualTo(0);
        try
        {
            byte* address = stackalloc byte[16];
            MakeAddress(address, ((IPEndPoint)peer.LocalEndPoint!).Port);
            connect(fd, address, 16).ShouldBe(0);
            byte* first = stackalloc byte[] { 1, 2, 3 };
            byte* second = stackalloc byte[] { 4, 5, 6, 7 };
            Iov* vectors = stackalloc Iov[3];
            vectors[0] = new() { Base = first, Length = 3 };
            vectors[1] = new() { Base = null, Length = 0 };
            vectors[2] = new() { Base = second, Length = 4 };
            writev(fd, vectors, 3).ShouldBe(7);
            byte[] received = new byte[16];
            EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            peer.ReceiveFrom(received, ref sender).ShouldBe(7);
            received.AsSpan(0, 7).SequenceEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }).ShouldBeTrue();
            peer.SendTo(new byte[] { 8, 9, 10, 11, 12 }, sender).ShouldBe(5);
            readv(fd, vectors, 3).ShouldBe(5);
            new Span<byte>(first, 3).SequenceEqual(new byte[] { 8, 9, 10 }).ShouldBeTrue();
            new Span<byte>(second, 4).SequenceEqual(new byte[] { 11, 12, 6, 7 }).ShouldBeTrue();
            writev(fd, vectors, -1).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
            vectors[0].Length = ulong.MaxValue;
            writev(fd, vectors, 1).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
        }
        finally { close(fd); }
    }

    private static void MakeAddress(byte* target, int port)
    {
        new Span<byte>(target, 16).Clear();
        *(ushort*)target = 2;
        target[2] = (byte)(port >> 8);
        target[3] = (byte)port;
        target[4] = 127;
        target[7] = 1;
    }
}

using System;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class SocketReuseTests
{
    private static int Option(int fd, int name)
    {
        int value = -1; uint length = 4;
        getsockopt(fd, 1, name, &value, &length).ShouldBe(0);
        length.ShouldBe(4u);
        return value;
    }

    [Fact]
    public void Linux_reuse_flags_are_independent_kernel_options()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        int fd = socket(2, 1, 0), one = 1, zero = 0;
        fd.ShouldBeGreaterThanOrEqualTo(0);
        setsockopt(fd, 1, 2, &one, 4).ShouldBe(0);
        Option(fd, 2).ShouldBe(1); Option(fd, 15).ShouldBe(0);
        setsockopt(fd, 1, 15, &one, 4).ShouldBe(0);
        setsockopt(fd, 1, 2, &zero, 4).ShouldBe(0);
        Option(fd, 2).ShouldBe(0); Option(fd, 15).ShouldBe(1);
        setsockopt(fd, 1, 15, &zero, 4).ShouldBe(0);
        Option(fd, 2).ShouldBe(0); Option(fd, 15).ShouldBe(0);
    }

    [Theory]
    [InlineData(2, false)]
    [InlineData(10, false)]
    [InlineData(2, true)]
    [InlineData(10, true)]
    public void Independent_owners_share_active_listener_only_with_explicit_reuseport(int family, bool reusePort)
    {
        if (!OperatingSystem.IsLinux()) return;
        if (family == 10 && !System.Net.Sockets.Socket.OSSupportsIPv6) Assert.Skip("IPv6 is unavailable on this host.");
        using var first = new RuntimeContext(); using var second = new RuntimeContext();
        byte* address = stackalloc byte[28]; new Span<byte>(address, 28).Clear();
        *(ushort*)address = (ushort)family;
        if (family == 2) { address[4] = 127; address[7] = 1; } else address[23] = 1;
        uint length = family == 2 ? 16u : 28u;
        int one = 1;
        using (first.Enter())
        {
            int listener = socket(family, 1, 0);
            listener.ShouldBeGreaterThanOrEqualTo(0);
            setsockopt(listener, 1, reusePort ? 15 : 2, &one, 4).ShouldBe(0);
            bind(listener, address, length).ShouldBe(0); listen(listener, 1).ShouldBe(0);
            getsockname(listener, address, &length).ShouldBe(0);
        }
        using (second.Enter())
        {
            int contender = socket(family, 1, 0);
            contender.ShouldBeGreaterThanOrEqualTo(0);
            setsockopt(contender, 1, reusePort ? 15 : 2, &one, 4).ShouldBe(0);
            int result = bind(contender, address, length);
            if (reusePort) { result.ShouldBe(0); listen(contender, 1).ShouldBe(0); }
            else { result.ShouldBe(-1); errno.ShouldBe(EADDRINUSE); }
        }
    }

    [Fact]
    public void Linux_reuse_buffers_validate_set_and_preserve_get_truncation()
    {
        if (!OperatingSystem.IsLinux()) return;
        using var owner = new RuntimeContext(); using var binding = owner.Enter();
        int fd = socket(2, 1, 0), one = 1;
        setsockopt(fd, 1, 2, &one, 3).ShouldBe(-1); errno.ShouldBe(EINVAL);
        setsockopt(fd, 1, 2, null, 4).ShouldBe(-1); errno.ShouldBe(EFAULT);
        setsockopt(fd, 1, 2, &one, 4).ShouldBe(0);
        byte* bytes = stackalloc byte[2] { 0, 123 }; uint length = 1;
        getsockopt(fd, 1, 2, bytes, &length).ShouldBe(0);
        length.ShouldBe(1u); bytes[0].ShouldBe((byte)1); bytes[1].ShouldBe((byte)123);
        length = 0; getsockopt(fd, 1, 2, null, &length).ShouldBe(0); length.ShouldBe(0u);
    }
}

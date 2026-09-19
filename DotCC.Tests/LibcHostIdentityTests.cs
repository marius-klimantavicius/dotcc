using System;
using System.Runtime.InteropServices;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcHostIdentityTests
{
    [Fact]
    public void Hostname_is_utf8_terminated_and_checks_capacity()
    {
        var expected = System.Net.Dns.GetHostName();
        int size = Encoding.UTF8.GetByteCount(expected) + 1;
        byte* buffer = stackalloc byte[size + 2];
        new Span<byte>(buffer, size + 2).Fill(0x5a);
        gethostname(buffer + 1, (ulong)size).ShouldBe(0);
        Marshal.PtrToStringUTF8((nint)(buffer + 1)).ShouldBe(expected);
        buffer[0].ShouldBe((byte)0x5a);
        buffer[size + 1].ShouldBe((byte)0x5a);
        gethostname(buffer + 1, (ulong)(size - 1)).ShouldBe(-1);
        errno.ShouldBe(ENAMETOOLONG);
        gethostname(null, 10).ShouldBe(-1);
        errno.ShouldBe(EFAULT);
    }

    [Fact]
    public void Login_uses_bcl_identity_and_returns_error_number_without_changing_errno()
    {
        var expected = Environment.UserName;
        int size = Encoding.UTF8.GetByteCount(expected) + 1;
        byte* buffer = stackalloc byte[size + 2];
        new Span<byte>(buffer, size + 2).Fill(0x5a);
        errno = 1234;
        getlogin_r(buffer + 1, (ulong)size).ShouldBe(0);
        Marshal.PtrToStringUTF8((nint)(buffer + 1)).ShouldBe(expected);
        getlogin_r(buffer + 1, (ulong)(size - 1)).ShouldBe(ERANGE);
        getlogin_r(null, 10).ShouldBe(EFAULT);
        errno.ShouldBe(1234);
        buffer[0].ShouldBe((byte)0x5a);
        buffer[size + 1].ShouldBe((byte)0x5a);
    }

    [Fact]
    public void Random_is_process_wide_repeatable_and_31_bit()
    {
        srandom(42);
        long first = random();
        long second = -1;
        var worker = new System.Threading.Thread(() => second = random());
        worker.Start();
        worker.Join(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        srandom(42);
        random().ShouldBe(first);
        random().ShouldBe(second);
        for (int i = 0; i < 1000; i++) random().ShouldBeInRange(0L, 0x7fffffffL);
    }
}

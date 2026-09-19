using System;
using System.Runtime.InteropServices;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcProtocolErrnoTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Protocol { public byte* Name; public byte** Aliases; public int Number; }
    private unsafe delegate void* Lookup(byte* name);

    [Theory]
    [InlineData("ENAMETOOLONG", 36, "File name too long")]
    [InlineData("ETXTBSY", 26, "Text file busy")]
    [InlineData("ENOTEMPTY", 39, "Directory not empty")]
    [InlineData("ELOOP", 40, "Too many levels of symbolic links")]
    [InlineData("ENODATA", 61, "No data available")]
    [InlineData("ENOLINK", 67, "Link has been severed")]
    [InlineData("ENETRESET", 102, "Network dropped connection on reset")]
    [InlineData("EWOULDBLOCK", 11, "Resource temporarily unavailable")]
    public void Linux_errno_numbers_and_messages_match(string name, int number, string message)
    {
        var field = typeof(DotCC.Libc.Libc).GetField(name);
        field.ShouldNotBeNull();
        field.GetRawConstantValue().ShouldBe(number);
        Marshal.PtrToStringUTF8((nint)strerror(number)).ShouldBe(message);
    }

    [Fact]
    public void Protocol_records_are_stable_borrowed_records_with_real_numbers_and_aliases()
    {
        var method = typeof(DotCC.Libc.Libc).GetMethod("getprotobyname");
        method.ShouldNotBeNull();
        var lookup = method.CreateDelegate<Lookup>();
        Protocol* first;
        fixed (byte* name = "tcp\0"u8) first = (Protocol*)lookup(name);
        ((nint)first).ShouldNotBe(0);
        first->Number.ShouldBe(6);
        Marshal.PtrToStringUTF8((nint)first->Name).ShouldBe("tcp");
        Marshal.PtrToStringUTF8((nint)first->Aliases[0]).ShouldBe("TCP");
        ((nint)first->Aliases[1]).ShouldBe(0);
        fixed (byte* name = "TCP\0"u8) ((nint)lookup(name)).ShouldBe((nint)first);
        fixed (byte* name = "udp\0"u8) ((Protocol*)lookup(name))->Number.ShouldBe(17);
        fixed (byte* name = "not-a-real-protocol\0"u8) ((nint)lookup(name)).ShouldBe(0);
        fixed (byte* name = "Tcp\0"u8) ((nint)lookup(name)).ShouldBe(0);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        first->Number.ShouldBe(6);
        Marshal.PtrToStringUTF8((nint)first->Name).ShouldBe("tcp");
    }
}

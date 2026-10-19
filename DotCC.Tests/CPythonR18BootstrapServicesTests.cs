using System.Text;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class CPythonR18BootstrapServicesTests
{
    [Theory]
    [InlineData(2147483713UL)]
    [InlineData(4294967361UL)]
    [InlineData(18446744073709551615UL)]
    public void Strftime_preserves_LP64_capacity_and_function_pointer_ABI(ulong capacity)
    {
        long epoch = 0;
        tm* calendar = gmtime(&epoch);
        byte* storage = stackalloc byte[66];
        new Span<byte>(storage, 66).Fill(0x5a);
        delegate*<byte*, ulong, byte*, tm*, ulong> format = &strftime;
        fixed (byte* spec = "%Y-%m-%d %H:%M:%S\0"u8)
        {
            errno = EDOM;
            Assert.Equal(19UL, format(storage + 1, capacity, spec, calendar));
            Assert.Equal(EDOM, errno);
        }
        Assert.Equal("1970-01-01 00:00:00", Encoding.ASCII.GetString(new ReadOnlySpan<byte>(storage + 1, 19)));
        Assert.Equal(0, storage[20]);
        Assert.Equal(0x5a, storage[0]);
        Assert.Equal(0x5a, storage[21]);
        Assert.Equal(0x5a, storage[65]);
    }

    [Fact]
    public void Strftime_checks_exact_NUL_capacity_and_preserves_failed_outputs()
    {
        long epoch = 0;
        tm* calendar = gmtime(&epoch);
        byte* storage = stackalloc byte[7];
        new Span<byte>(storage, 7).Fill(0x5a);
        fixed (byte* year = "%Y\0"u8)
        fixed (byte* empty = "\0"u8)
        {
            Assert.Equal(0UL, strftime(storage + 1, 0UL, year, calendar));
            Assert.Equal(0UL, strftime(storage + 1, 4UL, year, calendar));
            for (int i = 0; i < 7; i++) Assert.Equal(0x5a, storage[i]);
            Assert.Equal(4UL, strftime(storage + 1, 5UL, year, calendar));
            Assert.Equal("1970", Encoding.ASCII.GetString(new ReadOnlySpan<byte>(storage + 1, 4)));
            Assert.Equal(0, storage[5]);
            Assert.Equal(0x5a, storage[0]);
            Assert.Equal(0x5a, storage[6]);
            Assert.Equal(0UL, strftime(storage + 1, 1UL, empty, calendar));
            Assert.Equal(0, storage[1]);
            Assert.Equal(0UL, strftime(null, 5UL, year, calendar));
            Assert.Equal(0UL, strftime(storage + 1, 5UL, null, calendar));
            Assert.Equal(0UL, strftime(storage + 1, 5UL, year, null));
            Assert.Equal(0, strftime(storage + 1, -1, year, calendar));
            Assert.Equal(4, strftime(storage + 1, 5, year, calendar));
        }
    }

    [Fact]
    public void Wait_services_keep_explicit_rejection_and_status_ownership()
    {
        int status = 0x12345678;
        errno = 0;
        Assert.Equal(-1, waitpid(-1, &status, 1));
        Assert.Equal(EPERM, errno);
        Assert.Equal(0x12345678, status);
        errno = 0;
        Assert.Equal(-1, wait(&status));
        Assert.Equal(EPERM, errno);
        Assert.Equal(0x12345678, status);
    }
}

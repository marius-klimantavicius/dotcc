#nullable enable

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcCalendarTimezoneTests
{
    private static bool NativeHost => OperatingSystem.IsLinux()
        && RuntimeInformation.ProcessArchitecture == Architecture.X64;
    private static string Text(byte* value) => Marshal.PtrToStringUTF8((nint)value) ?? "";
    private static string Format(tm* value, string format)
    {
        byte* output = stackalloc byte[100];
        fixed (byte* pattern = Encoding.UTF8.GetBytes(format + '\0'))
            strftime(output, 100, pattern, value);
        return Text(output);
    }

    [DllImport("libc", EntryPoint = "localtime_r")]
    private static extern tm* HostLocaltime(long* epoch, tm* value);
    [DllImport("libc", EntryPoint = "mktime", SetLastError = true)]
    private static extern long HostMktime(tm* value);
    [DllImport("libc", EntryPoint = "strftime")]
    private static extern nuint HostStrftime(byte* output, nuint capacity, byte* format, tm* value);

    [Theory]
    [InlineData(0L, "+0000")]
    [InlineData(19800L, "+0530")]
    [InlineData(-12600L, "-0330")]
    [InlineData(20700L, "+0545")]
    [InlineData(-45900L, "-1245")]
    [InlineData(30L, "+0000")]
    [InlineData(-30L, "-0000")]
    [InlineData(19859L, "+0530")]
    [InlineData(-12659L, "-0330")]
    public void Numeric_offset_uses_supplied_seconds_and_preserves_sign(long seconds, string expected)
    {
        tm value = new() { tm_gmtoff = seconds, tm_isdst = 0 };
        Assert.Equal(expected, Format(&value, "%z"));
        value.tm_isdst = 1;
        Assert.Equal(expected, Format(&value, "%z"));
        value.tm_isdst = -1;
        Assert.Equal("[]", Format(&value, "[%z]"));
    }

    [Fact]
    public void Zone_name_uses_the_supplied_pointer_even_when_dst_is_unknown()
    {
        fixed (byte* name = "CALLER-0530\0"u8)
        {
            tm value = new() { tm_zone = name, tm_gmtoff = -12600, tm_isdst = -1 };
            Assert.Equal("CALLER-0530|", Format(&value, "%Z|%z"));
            value.tm_isdst = 1;
            Assert.Equal("CALLER-0530|-0330", Format(&value, "%Z|%z"));
        }
    }

    [Fact]
    public void Timezone_formats_respect_capacity_and_locale_validation()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        byte* output = stackalloc byte[8];
        for (int i = 0; i < 8; i++) output[i] = 42;
        fixed (byte* name = "X\0"u8)
        fixed (byte* format = "%Z%z\0"u8)
        fixed (byte* localeName = "C\0"u8)
        {
            tm value = new() { tm_zone = name, tm_isdst = 0, tm_gmtoff = 20700 };
            Assert.Equal(0UL, strftime(output, 6UL, format, &value));
            Assert.Equal((byte)42, output[0]);
            Assert.Equal(6UL, strftime(output, 7UL, format, &value));
            Assert.Equal("X+0545", Text(output));
            Assert.Equal((byte)42, output[7]);
            void* locale = newlocale(1 << 2 /* Linux LC_TIME */, localeName, null);
            Assert.True(locale != null);
            Assert.Equal(6UL, strftime_l(output, 8, format, &value, locale));
            Assert.Equal("X+0545", Text(output));
            freelocale(locale);
        }
    }

    [Theory]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(1705320000L)]
    [InlineData(1721044800L)]
    [InlineData(1730611800L)]
    [InlineData(1730615400L)]
    public void Local_names_fields_and_mktime_match_actual_host(long epoch)
    {
        if (!NativeHost) return;
        tm expected, actual;
        Assert.True(HostLocaltime(&epoch, &expected) != null);
        string expectedName = Text(expected.tm_zone);
        Assert.True(localtime_r(&epoch, &actual) == &actual);
        Assert.Equal(expectedName, Text(actual.tm_zone));
        Assert.Equal(expected.tm_gmtoff, actual.tm_gmtoff);
        Assert.Equal(expected.tm_isdst, actual.tm_isdst);
        Assert.Equal(expected.tm_hour, actual.tm_hour);
        Assert.Equal(expected.tm_mday, actual.tm_mday);
        Assert.Equal(expected.tm_yday, actual.tm_yday);
        Assert.Equal(expectedName, Format(&actual, "%Z"));
        Assert.Equal(HostMktime(&expected), mktime(&actual));
        Assert.Equal(expectedName, Text(actual.tm_zone));
        Assert.Equal(expected.tm_gmtoff, actual.tm_gmtoff);
        Assert.Equal(expected.tm_isdst, actual.tm_isdst);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Missing_name_fallback_matches_native_without_changing_the_supplied_tm(int dst)
    {
        if (!NativeHost) return;
        tm value = new() { tm_isdst = dst };
        byte* output = stackalloc byte[100];
        fixed (byte* pattern = "[%Z|%z]\0"u8)
        fixed (byte* empty = "\0"u8)
        {
            HostStrftime(output, 100, pattern, &value);
            Assert.Equal(Text(output), Format(&value, "[%Z|%z]"));
            Assert.True(value.tm_zone == null);
            value.tm_zone = empty;
            HostStrftime(output, 100, pattern, &value);
            Assert.Equal(Text(output), Format(&value, "[%Z|%z]"));
            Assert.True(value.tm_zone == empty);
        }
    }

    [Fact]
    public void Native_mktime_overflow_does_not_dereference_an_uninitialized_zone_field()
    {
        if (!NativeHost) return;
        tm value = new() { tm_year = int.MaxValue, tm_mon = int.MaxValue,
            tm_mday = 1, tm_isdst = -1, tm_zone = (byte*)1 };
        tm expected = value;
        Assert.Equal(-1L, HostMktime(&expected));
        Assert.NotEqual(0, Marshal.GetLastPInvokeError());
        Assert.Equal(-1L, mktime(&value));
        Assert.Equal((nint)expected.tm_zone, (nint)value.tm_zone);
    }

    [Fact]
    public void Returned_names_survive_owner_disposal_threads_gc_and_subsequent_calls()
    {
        var names = new ConcurrentBag<(nint Pointer, string Name)>();
        Parallel.For(0, 32, i =>
        {
            using var owner = new RuntimeContext();
            using var binding = owner.Enter();
            long epoch = i % 2 == 0 ? 1705320000 : 1721044800;
            tm value;
            Assert.True(localtime_r(&epoch, &value) != null);
            names.Add(((nint)value.tm_zone, Text(value.tm_zone)));
        });
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        foreach (var saved in names) Assert.Equal(saved.Name, Text((byte*)saved.Pointer));
        foreach (var group in System.Linq.Enumerable.GroupBy(names, entry => entry.Name))
            Assert.Single(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Select(group, entry => entry.Pointer)));
        Assert.Equal(56, sizeof(tm));
        Assert.Equal(40, Marshal.OffsetOf<tm>(nameof(tm.tm_gmtoff)).ToInt32());
        Assert.Equal(48, Marshal.OffsetOf<tm>(nameof(tm.tm_zone)).ToInt32());
    }
}

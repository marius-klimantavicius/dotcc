using System;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PosixQueryTests
{
    [Fact]
    public void Timezone_only_query_accepts_null_time_and_keeps_calendar_offset()
    {
        int* zone = stackalloc int[2];
        zone[0] = 12345; zone[1] = 12345;
        gettimeofday(null, zone).ShouldBe(0);
        zone[0].ShouldBe(-(int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        zone[1].ShouldBe(0);
        gettimeofday(null, null).ShouldBe(0);
        long* value = stackalloc long[2];
        long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        gettimeofday(value, null).ShouldBe(0);
        value[0].ShouldBeInRange(before, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        value[1].ShouldBeInRange(0, 999999);
    }

    [Fact]
    public void System_identity_and_page_size_match_host_queries_without_crossing_buffers()
    {
        using var owner = new RuntimeContext();
        using (owner.Enter())
        {
            sysconf(30).ShouldBe(Environment.SystemPageSize);
            sysconf(-123).ShouldBe(-1); errno.ShouldBe(EINVAL);
            byte* output = stackalloc byte[6 * 65 + 1];
            output[6 * 65] = 0x79;
            uname(output).ShouldBe(0);
            Encoding.UTF8.GetString(output + 65, strlen(output + 65)).ShouldBe(Environment.MachineName);
            output[6 * 65].ShouldBe((byte)0x79);
            for (int i = 0; i < 6; i++) output[65 * i + 64].ShouldBe((byte)0);
            uname(null).ShouldBe(-1); errno.ShouldBe(EFAULT);
            sched_yield().ShouldBe(0);
        }
    }

    [Fact]
    public void Reentrant_error_messages_use_caller_storage_and_report_small_buffers()
    {
        byte* output = stackalloc byte[128];
        strerror_r(EINVAL, output, 128).ShouldBe(0);
        strcmp(output, strerror(EINVAL)).ShouldBe(0);
        output[3] = 0x7f;
        strerror_r(EINVAL, output, 3).ShouldBe(ERANGE);
        output[2].ShouldBe((byte)0);
        output[3].ShouldBe((byte)0x7f);
        bzero(output, 3);
        output[0].ShouldBe((byte)0);
        output[3].ShouldBe((byte)0x7f);
    }

    [Fact]
    public void Unsupported_process_operations_report_failure_and_do_not_touch_outputs()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        long* record = stackalloc long[2] { 31, 42 };
        using (first.Enter())
        {
            errno = EIO;
            using (second.Enter())
            {
                getrlimit(7, record).ShouldBe(-1); errno.ShouldBe(ENOTSUP);
                record[0].ShouldBe(31); record[1].ShouldBe(42);
                setrlimit(7, record).ShouldBe(-1);
                setitimer(0, record, record).ShouldBe(-1);
                execve(null, null, null).ShouldBe(-1);
                setsid().ShouldBe(-1);
                ((nint)mmap(null, 4096, 3, 0x22, -1, 0)).ShouldBe((nint)(-1));
                pthread_cancel(1).ShouldBe(ENOTSUP);
                int previous = 99;
                pthread_setcancelstate(0, &previous).ShouldBe(ENOTSUP);
                previous.ShouldBe(99);
                dladdr(null, record).ShouldBe(0);
                record[0].ShouldBe(31);
            }
            errno.ShouldBe(EIO);
        }
    }
}

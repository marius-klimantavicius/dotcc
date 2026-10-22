using System;
using System.Threading;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PgliteProcessServicesTests
{
    [Fact]
    public void Page_size_matches_the_host_and_sysconf_without_changing_errno()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        errno = EIO;
        getpagesize().ShouldBe(Environment.SystemPageSize);
        getpagesize().ShouldBeGreaterThan(0);
        ((long)getpagesize()).ShouldBe(sysconf(30));
        errno.ShouldBe(EIO);
    }

    [Fact]
    public void Option_cursor_is_an_owner_scoped_lvalue_shared_by_its_threads()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        using var outer = first.Enter();
        optind.ShouldBe(1);
        ref int cursor = ref optind;
        cursor = 7;
        using (second.Enter())
        {
            optind.ShouldBe(1);
            optind = 3;
        }
        optind.ShouldBe(7);
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var worker = first.Enter();
                optind.ShouldBe(7);
                optind = 9;
            }
            catch (Exception error) { failure = error; }
        });
        thread.Start();
        thread.Join(TimeSpan.FromSeconds(5)).ShouldBeTrue();
        failure.ShouldBeNull();
        optind.ShouldBe(9);
        using (second.Enter()) optind.ShouldBe(3);
    }

    [Theory]
    [InlineData("r")]
    [InlineData("w")]
    [InlineData("re")]
    [InlineData("we")]
    public void Process_open_rejects_even_a_valid_command(string mode)
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        byte[] type = System.Text.Encoding.UTF8.GetBytes(mode + "\0");
        fixed (byte* command = "exit 0\0"u8)
        fixed (byte* pointer = type)
        {
            ((nint)popen(command, pointer)).ShouldBe(0);
            errno.ShouldBe(ENOTSUP);
        }
    }

    [Fact]
    public void Process_open_checks_arguments_and_does_not_invoke_a_shell()
    {
        fixed (byte* command = "exit 0\0"u8)
        fixed (byte* valid = "r\0"u8)
        fixed (byte* invalid = "r+\0"u8)
        {
            ((nint)popen(null, valid)).ShouldBe(0);
            errno.ShouldBe(EFAULT);
            ((nint)popen(command, null)).ShouldBe(0);
            errno.ShouldBe(EFAULT);
            ((nint)popen(command, invalid)).ShouldBe(0);
            errno.ShouldBe(EINVAL);
        }
    }

    [Fact]
    public void Process_close_rejects_foreign_handles_and_leaves_ordinary_files_open()
    {
        using var owner = new RuntimeContext();
        using var binding = owner.Enter();
        pclose(null).ShouldBe(-1);
        errno.ShouldBe(EBADF);
        pclose((FILE*)1).ShouldBe(-1);
        errno.ShouldBe(EBADF);
        FILE* file = tmpfile();
        ((nint)file).ShouldNotBe(0);
        try
        {
            pclose(file).ShouldBe(-1);
            errno.ShouldBe(ECHILD);
            fputc('x', file).ShouldBe((int)'x');
            rewind(file);
            fgetc(file).ShouldBe((int)'x');
        }
        finally { fclose(file).ShouldBe(0); }
    }
}

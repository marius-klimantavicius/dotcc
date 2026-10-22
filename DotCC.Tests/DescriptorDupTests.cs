using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class DescriptorDupTests
{
    private static int Open(string path)
    {
        byte[] name = Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* p = name) return open(p, 2);
    }

    [Fact]
    public void Duplicate_shares_offset_and_status_but_has_independent_close_and_descriptor_flags()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            int first = Open(path), second = dup(first);
            second.ShouldBeGreaterThanOrEqualTo(3); second.ShouldNotBe(first);
            byte a = (byte)'a', b = (byte)'b', c = (byte)'c';
            write(first, &a, 1).ShouldBe(1); write(second, &b, 1).ShouldBe(1);
            lseek(first, 0, 1).ShouldBe(2);
            fcntl(first, 2, 1).ShouldBe(0); fcntl(second, 1, 0).ShouldBe(0);
            fcntl(first, 4, 0x400).ShouldBe(0); (fcntl(second, 3, 0) & 0x400).ShouldBe(0x400);
            close(first).ShouldBe(0);
            write(second, &c, 1).ShouldBe(1); close(second).ShouldBe(0);
            File.ReadAllText(path).ShouldBe("abc");
            close(second).ShouldBe(-1); errno.ShouldBe(EBADF);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Dup2_replaces_target_after_validating_source_and_retains_redirected_stdio_until_last_close()
    {
        string path = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            byte[] name = Encoding.UTF8.GetBytes(path + "\0");
            fixed (byte* p = name) fixed (byte* mode = "wb\0"u8)
                ((nint)freopen(p, mode, stdout)).ShouldBe((nint)stdout);
            dup2(-1, 2).ShouldBe(-1); errno.ShouldBe(EBADF);
            dup2(1, 1).ShouldBe(1);
            dup2(1, 2).ShouldBe(2);
            fputc('x', stdout).ShouldBe((int)'x');
            fclose(stdout).ShouldBe(0);
            fputc('y', stderr).ShouldBe((int)'y'); fflush(stderr).ShouldBe(0);
            File.ReadAllText(path).ShouldBe("xy");
            fclose(stderr).ShouldBe(0);
            dup2(1, 4).ShouldBe(-1); errno.ShouldBe(EBADF);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Reopen_one_duplicate_and_dispose_foreign_owner_preserves_the_other_backing()
    {
        string firstPath = Path.GetTempFileName(), secondPath = Path.GetTempFileName();
        try
        {
            using var owner = new RuntimeContext();
            using (owner.Enter())
            {
                int descriptor = Open(firstPath), duplicate = dup(descriptor);
                FILE* stream;
                fixed (byte* mode = "w\0"u8) stream = fdopen(descriptor, mode);
                byte[] path = Encoding.UTF8.GetBytes(secondPath + "\0");
                fixed (byte* p = path) fixed (byte* mode = "w\0"u8)
                    ((nint)freopen(p, mode, stream)).ShouldBe((nint)stream);
                using (var foreign = new RuntimeContext())
                {
                    using (foreign.Enter()) dup(duplicate).ShouldBe(-1);
                }
                byte value = (byte)'a'; write(duplicate, &value, 1).ShouldBe(1);
                fputc('b', stream).ShouldBe((int)'b');
                fclose(stream).ShouldBe(0); close(duplicate).ShouldBe(0);
            }
            File.ReadAllText(firstPath).ShouldBe("a"); File.ReadAllText(secondPath).ShouldBe("b");
        }
        finally { File.Delete(firstPath); File.Delete(secondPath); }
    }
}

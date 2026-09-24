using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class FileOwnershipTests
{
    [Fact]
    public void Owners_reuse_fd_numbers_without_sharing_files_or_FILE_handles()
    {
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        FILE* firstFile;
        using (first.Enter())
        {
            firstFile = tmpfile();
            fileno(firstFile).ShouldBe(3);
            fputc(41, firstFile).ShouldBe(41);
        }
        using (second.Enter())
        {
            var secondFile = tmpfile();
            fileno(secondFile).ShouldBe(3);
            fputc(72, secondFile).ShouldBe(72);
            fclose(firstFile).ShouldBe(-1);
            errno.ShouldBe(EBADF);
            fileno(firstFile).ShouldBe(-1);
            rewind(secondFile);
            fgetc(secondFile).ShouldBe(72);
            fclose(secondFile).ShouldBe(0);
        }
        using (first.Enter())
        {
            rewind(firstFile);
            fgetc(firstFile).ShouldBe(41);
            fclose(firstFile).ShouldBe(0);
        }
    }

    [Fact]
    public void Unbound_legacy_descriptors_survive_owner_close_and_disposal()
    {
        var legacy = tmpfile();
        try
        {
            fputc(19, legacy).ShouldBe(19);
            using (var owner = new RuntimeContext())
            using (owner.Enter())
            {
                fcntl(fileno(legacy), 3).ShouldBe(-1);
                var owned = tmpfile();
                fclose(owned).ShouldBe(0);
            }
            rewind(legacy);
            fgetc(legacy).ShouldBe(19);
        }
        finally { fclose(legacy); }
    }

    [Fact]
    public void Disposing_an_unbound_owner_closes_its_backing_without_closing_current_owner()
    {
        string path = Path.GetTempFileName();
        var retired = new RuntimeContext();
        using var active = new RuntimeContext();
        try
        {
            using (retired.Enter())
            {
                fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0"))
                fixed (byte* mode = "w\0"u8)
                    ((nint)fopen(name, mode)).ShouldNotBe(0);
            }
            using (active.Enter())
            {
                var live = tmpfile();
                fputc(63, live).ShouldBe(63);
                Should.Throw<IOException>(() =>
                {
                    using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                });
                retired.Dispose();
                using (var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    exclusive.CanWrite.ShouldBeTrue();
                rewind(live);
                fgetc(live).ShouldBe(63);
                fclose(live).ShouldBe(0);
            }
        }
        finally { retired.Dispose(); File.Delete(path); }
    }

    [Fact]
    public void Standard_handles_and_redirection_are_owned_but_process_writers_remain_open()
    {
        string path = Path.GetTempFileName();
        var savedOut = Console.Out;
        using var capture = new StringWriter();
        var retired = new RuntimeContext();
        using var active = new RuntimeContext();
        try
        {
            Console.SetOut(capture);
            FILE* retiredOut;
            using (retired.Enter())
            {
                retiredOut = stdout;
                fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0"))
                fixed (byte* mode = "w\0"u8)
                    ((nint)freopen(name, mode, stdout)).ShouldBe((nint)retiredOut);
                fputc('A', stdout).ShouldBe('A');
            }
            using (active.Enter())
            {
                ((nint)stdout).ShouldNotBe((nint)retiredOut);
                fclose(retiredOut).ShouldBe(-1);
                retired.Dispose();
                fputc('B', stdout).ShouldBe('B');
            }
            active.Dispose();
            Console.Write('C');
            capture.ToString().ShouldBe("BC");
            File.ReadAllText(path).ShouldBe("A");
        }
        finally { retired.Dispose(); Console.SetOut(savedOut); File.Delete(path); }
    }
}

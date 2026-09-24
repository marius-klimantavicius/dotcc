using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class FileDurabilityTests
{
    private static byte[] CString(string text) => Encoding.UTF8.GetBytes(text + '\0');

    [Fact]
    public void Temporary_files_sync_permissions_and_truncation_follow_each_owner()
    {
        string root = Directory.CreateTempSubdirectory("dotcc-durable-").FullName;
        string a = Directory.CreateDirectory(Path.Combine(root, "a")).FullName;
        string b = Directory.CreateDirectory(Path.Combine(root, "b")).FullName;
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        try
        {
            using (first.Enter())
            fixed (byte* directory = CString(a))
            fixed (byte* template = CString("file-XXXXXX"))
            {
                chdir(directory).ShouldBe(0);
                int descriptor = mkstemp(template);
                descriptor.ShouldBeGreaterThanOrEqualTo(3);
                string filename = Encoding.UTF8.GetString(template, strlen(template));
                byte* contents = stackalloc byte[] { 1, 2, 3, 4 };
                write(descriptor, contents, 4).ShouldBe(4);
                fsync(descriptor).ShouldBe(0);
                File.ReadAllBytes(Path.Combine(a, filename)).ShouldBe(new byte[] { 1, 2, 3, 4 });
                if (!OperatingSystem.IsWindows())
                {
                    File.GetUnixFileMode(Path.Combine(a, filename)).ShouldBe(UnixFileMode.UserRead | UnixFileMode.UserWrite);
                    fchmod(descriptor, 0x100).ShouldBe(0);
                    File.GetUnixFileMode(Path.Combine(a, filename)).ShouldBe(UnixFileMode.UserRead);
                    fchmod(descriptor, 0x180).ShouldBe(0);
                }
                using (second.Enter())
                fixed (byte* otherDirectory = CString(b))
                fixed (byte* otherTemplate = CString("file-XXXXXX"))
                {
                    chdir(otherDirectory).ShouldBe(0);
                    int other = mkstemp(otherTemplate);
                    other.ShouldBe(descriptor);
                    close(other).ShouldBe(0);
                    fsync(other).ShouldBe(-1);
                    errno.ShouldBe(EBADF);
                }
                fsync(descriptor).ShouldBe(0);
                truncate(template, 2).ShouldBe(0);
                File.ReadAllBytes(Path.Combine(a, filename)).ShouldBe(new byte[] { 1, 2 });
                close(descriptor).ShouldBe(0);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Ftello_preserves_large_positions_and_rejects_closed_streams()
    {
        string root = Directory.CreateTempSubdirectory("dotcc-ftello-").FullName;
        using var owner = new RuntimeContext();
        try
        {
            using (owner.Enter())
            fixed (byte* name = CString(Path.Combine(root, "file")))
            fixed (byte* mode = CString("w+b"))
            {
                FILE* stream = fopen(name, mode);
                ((nint)stream).ShouldNotBe(0);
                fseek(stream, 1L << 33, 0).ShouldBe(0);
                ftello(stream).ShouldBe(1L << 33);
                fclose(stream).ShouldBe(0);
                ftello(stream).ShouldBe(-1);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Invalid_templates_and_nonfile_sync_fail_without_creating_files()
    {
        using var owner = new RuntimeContext();
        using (owner.Enter())
        fixed (byte* template = CString("wrong-XXXXX"))
        {
            mkstemp(template).ShouldBe(-1); errno.ShouldBe(EINVAL);
            fsync(-1).ShouldBe(-1); errno.ShouldBe(EBADF);
            int* descriptors = stackalloc int[2];
            pipe(descriptors).ShouldBe(0);
            fsync(descriptors[0]).ShouldBe(-1); errno.ShouldBe(EINVAL);
            close(descriptors[0]); close(descriptors[1]);
        }
    }
}

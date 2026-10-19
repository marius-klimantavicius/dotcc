using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class FtruncatePositionTests
{
    private static byte[] CString(string value) => Encoding.UTF8.GetBytes(value + '\0');

    [Theory]
    [InlineData(5L, 5L, 0L)]
    [InlineData(8L, 6L, 3L)]
    [InlineData(8L, 2L, 3L)]
    [InlineData(8L, 11L, 8L)]
    [InlineData(8L, 11L, 20L)]
    [InlineData(8L, 25L, 20L)]
    [InlineData(2147483650L, 2147483650L, 2147483649L)]
    [InlineData(2147483649L, 2147483650L, 2147483651L)]
    public void Resize_preserves_offset_and_the_next_write_uses_that_offset(long initial, long position, long length)
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-ftruncate-" + Guid.NewGuid().ToString("N"));
        using var owner = new RuntimeContext();
        try
        {
            using (owner.Enter())
            fixed (byte* name = CString(path))
            {
                int fd = open(name, 2 | 0x40 | 0x80, 0x180);
                fd.ShouldBeGreaterThanOrEqualTo(3);
                try
                {
                    ftruncate(fd, initial).ShouldBe(0);
                    lseek(fd, position, SEEK_SET).ShouldBe(position);
                    errno = 123;
                    ftruncate(fd, length).ShouldBe(0);
                    errno.ShouldBe(123);
                    lseek(fd, 0, SEEK_CUR).ShouldBe(position);
                    fsync(fd).ShouldBe(0);
                    new FileInfo(path).Length.ShouldBe(length);
                    byte marker = 0x5a;
                    write(fd, &marker, 1).ShouldBe(1);
                    lseek(fd, 0, SEEK_CUR).ShouldBe(position + 1);
                    fsync(fd).ShouldBe(0);
                    new FileInfo(path).Length.ShouldBe(Math.Max(length, position + 1));
                    lseek(fd, position, SEEK_SET).ShouldBe(position);
                    byte observed = 0;
                    read(fd, &observed, 1).ShouldBe(1);
                    observed.ShouldBe(marker);
                }
                finally { close(fd).ShouldBe(0); }
            }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Stdio_and_descriptor_share_the_preserved_position_after_explicit_flush()
    {
        string path = Path.Combine(Path.GetTempPath(), "dotcc-ftruncate-stdio-" + Guid.NewGuid().ToString("N"));
        using var owner = new RuntimeContext();
        try
        {
            using (owner.Enter())
            fixed (byte* name = CString(path))
            fixed (byte* mode = CString("w+b"))
            {
                FILE* stream = fopen(name, mode);
                ((nint)stream).ShouldNotBe(0);
                try
                {
                    setvbuf(stream, null, 0, 16).ShouldBe(0);
                    for (int i = 0; i < 6; i++) fputc('a' + i, stream).ShouldBe('a' + i);
                    ftello(stream).ShouldBe(6);
                    fflush(stream).ShouldBe(0);
                    int fd = fileno(stream);
                    ftruncate(fd, 3).ShouldBe(0);
                    ftello(stream).ShouldBe(6);
                    lseek(fd, 0, SEEK_CUR).ShouldBe(6);
                    fputc('z', stream).ShouldBe('z');
                    fflush(stream).ShouldBe(0);
                    ftello(stream).ShouldBe(7);
                    File.ReadAllBytes(path).ShouldBe(new byte[] { (byte)'a', (byte)'b', (byte)'c', 0, 0, 0, (byte)'z' });
                    fseek(stream, 0, SEEK_SET).ShouldBe(0);
                    fgetc(stream).ShouldBe('a');
                    ftruncate(fd, 0).ShouldBe(0);
                    ftello(stream).ShouldBe(1);
                    fgetc(stream).ShouldBe(-1);
                }
                finally { fclose(stream).ShouldBe(0); }
            }
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Colliding_owner_descriptors_resize_only_the_current_owners_file()
    {
        string root = Directory.CreateTempSubdirectory("dotcc-ftruncate-owner-").FullName;
        using var first = new RuntimeContext();
        using var second = new RuntimeContext();
        try
        {
            using (first.Enter())
            fixed (byte* a = CString(Path.Combine(root, "a")))
            fixed (byte* b = CString(Path.Combine(root, "b")))
            {
                int fd = open(a, 2 | 0x40 | 0x80, 0x180);
                try
                {
                    fd.ShouldBeGreaterThanOrEqualTo(3);
                    ftruncate(fd, 9).ShouldBe(0);
                    lseek(fd, 7, SEEK_SET).ShouldBe(7);
                    using (second.Enter())
                    {
                        int other = open(b, 2 | 0x40 | 0x80, 0x180);
                        other.ShouldBe(fd);
                        try
                        {
                            lseek(other, 11, SEEK_SET).ShouldBe(11);
                            ftruncate(other, 2).ShouldBe(0);
                            lseek(other, 0, SEEK_CUR).ShouldBe(11);
                        }
                        finally { close(other).ShouldBe(0); }
                    }
                    lseek(fd, 0, SEEK_CUR).ShouldBe(7);
                    ftruncate(fd, 4).ShouldBe(0);
                    lseek(fd, 0, SEEK_CUR).ShouldBe(7);
                    new FileInfo(Path.Combine(root, "a")).Length.ShouldBe(4);
                    new FileInfo(Path.Combine(root, "b")).Length.ShouldBe(2);
                }
                finally { close(fd).ShouldBe(0); }
            }
        }
        finally { Directory.Delete(root, true); }
    }
}

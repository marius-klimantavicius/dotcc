using System.Runtime.InteropServices;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcFileMetadataTests
{
    private static byte[] Z(string path) => Encoding.UTF8.GetBytes(path + "\0");

    [Fact]
    public void Source_permissions_survive_stat_and_permission_derived_atomic_creation()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64) return;
        string root = Directory.CreateTempSubdirectory("dotcc-metadata-").FullName;
        try
        {
            string source = Path.Combine(root, "source"), output = Path.Combine(root, "output");
            File.WriteAllText(source, "value=42\n"); File.SetUnixFileMode(source, (UnixFileMode)0x120); // 0440
            byte* metadata = stackalloc byte[96];
            fixed (byte* p = Z(source)) stat(p, metadata).ShouldBe(0);
            uint mode = *(uint*)(metadata + 16);
            (mode & 0xfff).ShouldBe((uint)File.GetUnixFileMode(source));
            fixed (byte* p = Z(output))
            {
                int fd = open(p, 2 | 0x40 | 0x80, (int)mode | 0x80); fd.ShouldBeGreaterThanOrEqualTo(3);
                close(fd).ShouldBe(0);
            }
            (File.GetUnixFileMode(output) & UnixFileMode.UserRead).ShouldBe(UnixFileMode.UserRead);
            using var reader = File.OpenRead(output); reader.Length.ShouldBe(0);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Lstat_reports_the_link_and_stat_follows_or_reports_dangling_target()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64) return;
        string root = Directory.CreateTempSubdirectory("dotcc-metadata-").FullName;
        try
        {
            string target = Path.Combine(root, "target"), link = Path.Combine(root, "link");
            File.WriteAllText(target, "value"); File.CreateSymbolicLink(link, "target");
            byte* metadata = stackalloc byte[96];
            fixed (byte* p = Z(link))
            {
                stat(p, metadata).ShouldBe(0); (*(uint*)(metadata + 16) & 0xf000).ShouldBe(0x8000u);
                lstat(p, metadata).ShouldBe(0); (*(uint*)(metadata + 16) & 0xf000).ShouldBe(0xa000u);
                (*(long*)(metadata + 40)).ShouldBe(6);
                File.Delete(target); stat(p, metadata).ShouldBe(-1); errno.ShouldBe(2);
                lstat(p, metadata).ShouldBe(0);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Fstat_uses_the_owner_handle_after_unlink_and_preserves_buffer_bounds()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64) return;
        string root = Directory.CreateTempSubdirectory("dotcc-metadata-").FullName;
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            string path = Path.Combine(root, "file");
            byte* metadata = stackalloc byte[104]; new Span<byte>(metadata, 104).Fill(0xa5);
            fixed (byte* p = Z(path))
            {
                int fd = open(p, 2 | 0x40 | 0x80, 0x180); fd.ShouldBeGreaterThanOrEqualTo(3);
                try
                {
                    stat(p, metadata).ShouldBe(0); ulong inode = *(ulong*)(metadata + 8); inode.ShouldNotBe(0ul);
                    File.Delete(path); fstat(fd, metadata).ShouldBe(0);
                    (*(ulong*)(metadata + 8)).ShouldBe(inode); (*(ulong*)(metadata + 24)).ShouldBe(0ul);
                    new ReadOnlySpan<byte>(metadata + 96, 8).ToArray().ShouldAllBe(x => x == 0xa5);
                }
                finally { close(fd); }
                fstat(fd, metadata).ShouldBe(-1); errno.ShouldBe(9);
            }
        }
        finally { Directory.Delete(root, true); }
    }
}

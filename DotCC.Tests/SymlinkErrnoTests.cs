using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class SymlinkErrnoTests
{
    private sealed class Scope : IDisposable
    {
        internal readonly string Root;
        internal readonly RuntimeContext Owner = new();
        private readonly IDisposable binding;
        internal Scope()
        {
            if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
                Assert.Skip("Linux x64 native symlink ABI controls");
            Root = Directory.CreateTempSubdirectory("dotcc-symlink-").FullName;
            binding = Owner.Enter();
            fixed (byte* p = C(Root)) chdir(p).ShouldBe(0);
        }
        public void Dispose() { binding.Dispose(); Owner.Dispose(); Directory.Delete(Root, true); }
    }

    private static byte[] C(string value) => Encoding.UTF8.GetBytes(value + '\0');
    private static int Link(string target, string name)
    { fixed (byte* t = C(target)) fixed (byte* p = C(name)) return symlink(t, p); }
    private static long Read(string name, byte* buffer, ulong capacity)
    { fixed (byte* p = C(name)) return readlink(p, buffer, capacity); }

    [Fact]
    public void Traversal_errors_come_from_the_host_and_never_change_the_output_buffer()
    {
        using var scope = new Scope();
        File.WriteAllText(Path.Combine(scope.Root, "regular"), "value");
        Directory.CreateDirectory(Path.Combine(scope.Root, "directory"));
        Link("loop", "loop").ShouldBe(0);
        byte* buffer = stackalloc byte[16]; new Span<byte>(buffer, 16).Fill(0xa5);
        foreach (var row in new[] { ("missing", ENOENT), ("missing/link", ENOENT),
                 ("regular/link", ENOTDIR), ("loop/link", ELOOP), ("regular", EINVAL),
                 ("directory", EINVAL), ("", ENOENT) })
        {
            errno = 0; Read(row.Item1, buffer, 16).ShouldBe(-1); errno.ShouldBe(row.Item2);
        }
        foreach (var row in new[] { ("missing/link", ENOENT), ("regular/link", ENOTDIR), ("loop/link", ELOOP), ("", ENOENT) })
        {
            errno = 0; Link("target", row.Item1).ShouldBe(-1); errno.ShouldBe(row.Item2);
        }
        for (int i = 0; i < 16; i++) buffer[i].ShouldBe((byte)0xa5);
    }

    [Fact]
    public void Existing_regular_directory_broken_and_loop_entries_are_EEXIST()
    {
        using var scope = new Scope();
        File.WriteAllText(Path.Combine(scope.Root, "regular"), "value");
        Directory.CreateDirectory(Path.Combine(scope.Root, "directory"));
        Link("missing", "broken").ShouldBe(0); Link("loop", "loop").ShouldBe(0);
        foreach (string name in new[] { "regular", "directory", "broken", "loop" })
        {
            errno = 0; Link("new-target", name).ShouldBe(-1); errno.ShouldBe(EEXIST);
        }
        new FileInfo(Path.Combine(scope.Root, "broken")).LinkTarget.ShouldBe("missing");
        new FileInfo(Path.Combine(scope.Root, "loop")).LinkTarget.ShouldBe("loop");
    }

    [Theory]
    [InlineData(32UL, 9L, 123)]
    [InlineData(3UL, 3L, 123)]
    [InlineData(0UL, -1L, EINVAL)]
    [InlineData(2147483647UL, 9L, 123)]
    [InlineData(2147483648UL, -1L, EINVAL)]
    [InlineData(4294967295UL, -1L, EINVAL)]
    [InlineData(4294967296UL, -1L, EINVAL)]
    [InlineData(4294967299UL, 3L, 123)]
    [InlineData(ulong.MaxValue, -1L, EINVAL)]
    public void Native_size_t_extent_and_no_NUL_contract_hold(ulong capacity, long count, int error)
    {
        using var scope = new Scope();
        Link("../target", "link").ShouldBe(0);
        byte* guarded = stackalloc byte[32]; new Span<byte>(guarded, 32).Fill(0xa5);
        errno = 123; Read("link", guarded + 4, capacity).ShouldBe(count); errno.ShouldBe(error);
        for (int i = 0; i < 32; i++)
            guarded[i].ShouldBe(count > 0 && i >= 4 && i < 4 + count ? "../target"u8[i - 4] : (byte)0xa5);
    }

    [Fact]
    public void Denied_directory_search_is_EACCES_and_keeps_the_buffer_intact()
    {
        using var scope = new Scope();
        string denied = Directory.CreateDirectory(Path.Combine(scope.Root, "denied")).FullName;
        File.SetUnixFileMode(denied, 0);
        try
        {
            byte* buffer = stackalloc byte[16]; new Span<byte>(buffer, 16).Fill(0xa5);
            errno = 0; Read("denied/link", buffer, 16).ShouldBe(-1); errno.ShouldBe(EACCES);
            errno = 0; Link("target", "denied/link").ShouldBe(-1); errno.ShouldBe(EACCES);
            for (int i = 0; i < 16; i++) buffer[i].ShouldBe((byte)0xa5);
        }
        finally { File.SetUnixFileMode(denied, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute); }
    }

    [Fact]
    public void Null_pointers_and_zero_capacity_return_the_native_error_without_an_exception()
    {
        using var scope = new Scope(); Link("target", "link").ShouldBe(0);
        byte* buffer = stackalloc byte[16]; new Span<byte>(buffer, 16).Fill(0xa5);
        readlink(null, buffer, 16).ShouldBe(-1); errno.ShouldBe(EFAULT);
        fixed (byte* link = "link\0"u8) { readlink(link, null, 16).ShouldBe(-1); errno.ShouldBe(EFAULT); }
        readlink(null, null, 0).ShouldBe(-1); errno.ShouldBe(EINVAL);
        fixed (byte* p = "new-link\0"u8) { symlink(null, p).ShouldBe(-1); errno.ShouldBe(EFAULT); symlink(p, null).ShouldBe(-1); errno.ShouldBe(EFAULT); }
        for (int i = 0; i < 16; i++) buffer[i].ShouldBe((byte)0xa5);
    }

    [Fact]
    public void Target_bytes_stay_lexical_and_other_owners_cannot_redirect_the_caller()
    {
        string processCwd = Directory.GetCurrentDirectory();
        RuntimeContext? previous = RuntimeContext.Current;
        using (var scope = new Scope())
        {
            byte* target = stackalloc byte[] { (byte)'.', (byte)'/', 0xff, 0xc3, 0 };
            fixed (byte* p = "bytes\0"u8) symlink(target, p).ShouldBe(0);
            byte* buffer = stackalloc byte[16]; new Span<byte>(buffer, 16).Fill(0xa5);
            Read("bytes", buffer, 16).ShouldBe(4);
            for (int i = 0; i < 4; i++) buffer[i].ShouldBe(target[i]);
            buffer[4].ShouldBe((byte)0xa5);
            Link("../missing/./target", "relative").ShouldBe(0);
            using (var second = new RuntimeContext())
            using (second.Enter())
            {
                string other = Directory.CreateDirectory(Path.Combine(scope.Root, "other")).FullName;
                fixed (byte* p = C(other)) chdir(p).ShouldBe(0);
                Read("relative", buffer, 16).ShouldBe(-1); errno.ShouldBe(ENOENT);
                Link("different", "relative").ShouldBe(0);
            }
            RuntimeContext.Current.ShouldBeSameAs(scope.Owner);
            new FileInfo(Path.Combine(scope.Root, "relative")).LinkTarget.ShouldBe("../missing/./target");
            Read("relative", buffer, 16).ShouldBe(16); // truncated literal bytes
            new ReadOnlySpan<byte>(buffer, 16).SequenceEqual("../missing/./tar"u8).ShouldBeTrue();
            Directory.GetCurrentDirectory().ShouldBe(processCwd);
        }
        RuntimeContext.Current.ShouldBeSameAs(previous);
    }
}

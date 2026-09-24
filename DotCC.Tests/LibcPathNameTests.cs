using System;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class LibcPathNameTests
{
    [Theory]
    [InlineData("", ".", ".")]
    [InlineData("/", "/", "/")]
    [InlineData("//", "//", "/")]
    [InlineData("///", "/", "/")]
    [InlineData("a", ".", "a")]
    [InlineData("a///", ".", "a")]
    [InlineData("a//b///", "a", "b")]
    [InlineData("/a/b", "/a", "b")]
    [InlineData("//a", "//", "a")]
    [InlineData("///a", "/", "a")]
    [InlineData("//a/b", "//a", "b")]
    [InlineData(".", ".", ".")]
    [InlineData("..", ".", "..")]
    [InlineData("a/../b", "a/..", "b")]
    [InlineData("a\\b", ".", "a\\b")]
    [InlineData("ą/ž", "ą", "ž")]
    public void Paths_follow_POSIX_byte_string_rules(string path, string directory, string component)
    {
        var directoryBytes = Encoding.UTF8.GetBytes(path + "\0");
        var componentBytes = Encoding.UTF8.GetBytes(path + "\0");
        fixed (byte* d = directoryBytes, b = componentBytes)
        {
            Decode(dirname(d)).ShouldBe(directory);
            Decode(basename(b)).ShouldBe(component);
        }
    }

    [Fact]
    public void Results_alias_input_and_never_write_past_its_terminator()
    {
        byte* path = stackalloc byte[] { (byte)'a', (byte)'/', (byte)'b', (byte)'/', 0, 0xA5 };
        (basename(path) == path + 2).ShouldBeTrue();
        path[3].ShouldBe((byte)0);
        path[5].ShouldBe((byte)0xA5);
        (dirname(path) == path).ShouldBeTrue();
        path[1].ShouldBe((byte)0);
        path[5].ShouldBe((byte)0xA5);
    }

    [Fact]
    public void Empty_and_null_results_survive_GC_and_do_not_overwrite_empty_input()
    {
        byte* empty = stackalloc byte[] { 0, 0xA5 };
        byte* first = dirname(null);
        byte* second = basename(empty);
        empty[0].ShouldBe((byte)0);
        empty[1].ShouldBe((byte)0xA5);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Decode(first).ShouldBe(".");
        Decode(second).ShouldBe(".");
        Decode(basename(null)).ShouldBe(".");
        Decode(dirname(empty)).ShouldBe(".");
    }

    private static string Decode(byte* value) => Encoding.UTF8.GetString(value, strlen(value));
}

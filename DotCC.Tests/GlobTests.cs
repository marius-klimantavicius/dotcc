using System;
using System.IO;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class GlobTests
{
    private sealed class Program : IProgramInstance, IDisposable
    {
        public RuntimeContext __DotCcRuntime { get; } = new();
        public string? ErrorPath;
        public int ErrorCode, CallbackResult;
        public bool CorrectOwner, Throw;
        public void Dispose() => __DotCcRuntime.Dispose();
    }
    private static int Error(Program instance, byte* path, int code)
    {
        instance.CorrectOwner = ReferenceEquals(RuntimeContext.Current, instance.__DotCcRuntime);
        instance.ErrorPath = Encoding.UTF8.GetString(path, strlen(path));
        instance.ErrorCode = code;
        if (instance.Throw) throw new InvalidOperationException("glob callback");
        return instance.CallbackResult;
    }
    private static void Change(string path)
    {
        fixed (byte* bytes = Encoding.UTF8.GetBytes(path + "\0")) chdir(bytes).ShouldBe(0);
    }
    private static string[] Results(void* result)
    {
        nuint count = *(nuint*)result;
        byte** paths = *(byte***)(8 + (byte*)result);
        if (paths == null) return Array.Empty<string>();
        ((nint)paths[count]).ShouldBe(0);
        var values = new string[(int)count];
        for (int i = 0; i < values.Length; i++) values[i] = Encoding.UTF8.GetString(paths[i], strlen(paths[i]));
        return values;
    }
    private static string Tree()
    {
        string root = Directory.CreateTempSubdirectory("dotcc-glob-").FullName;
        foreach (string name in new[] { "b.conf", "a.conf", "é.conf", ".hidden.conf", "literal*.conf", "[broken.conf" }) File.WriteAllText(Path.Combine(root, name), "");
        Directory.CreateDirectory(Path.Combine(root, "sub"));
        File.WriteAllText(Path.Combine(root, "sub", "c.conf"), "");
        return root;
    }
    [Theory]
    [InlineData("*.conf", 0, "[broken.conf|a.conf|b.conf|literal*.conf|é.conf")]
    [InlineData("?.conf", 0, "a.conf|b.conf")]
    [InlineData("[a-b].conf", 0, "a.conf|b.conf")]
    [InlineData("[[:alpha:]].conf", 0, "a.conf|b.conf")]
    [InlineData("[!b].conf", 0, "a.conf")]
    [InlineData("literal\\*.conf", 0, "literal*.conf")]
    [InlineData("[broken.conf", 0, "[broken.conf")]
    [InlineData("sub/*.conf", 0, "sub/c.conf")]
    [InlineData("sub//*.conf", 0, "sub//c.conf")]
    [InlineData("*.conf", 128, ".hidden.conf|[broken.conf|a.conf|b.conf|literal*.conf|é.conf")]
    [InlineData("sub", 2, "sub/")]
    [InlineData("*/", 0, "sub/")]
    [InlineData("missing*", 16, "missing*")]
    [InlineData("*", 8192, "sub")]
    public void C_locale_patterns_match_native_results(string pattern, int flags, string expected)
    {
        string root = Tree();
        try
        {
            using var program = new Program();
            using var binding = program.__DotCcRuntime.Enter();
            Change(root);
            byte* result = stackalloc byte[72];
            fixed (byte* bytes = Encoding.UTF8.GetBytes(pattern + "\0")) glob(bytes, flags, null, result).ShouldBe(0);
            Results(result).ShouldBe(expected.Split('|'));
            globfree(result); Results(result).ShouldBeEmpty();
            globfree(result);
        }
        finally { Directory.Delete(root, true); }
    }
    [Fact]
    public void Explicit_owner_controls_paths_callback_and_output_allocation_and_restores_binding()
    {
        string root = Tree();
        try
        {
            using var first = new Program(); using var second = new Program();
            using (first.__DotCcRuntime.Enter()) Change(root);
            using var binding = second.__DotCcRuntime.Enter();
            byte* result = stackalloc byte[72];
            fixed (byte* pattern = "*.conf\0"u8) GlobManaged(first, pattern, 0, &Error, result).ShouldBe(0);
            ReferenceEquals(RuntimeContext.Current, second.__DotCcRuntime).ShouldBeTrue();
            Results(result).Length.ShouldBe(5);
            first.__DotCcRuntime.NativeAllocations.Count.ShouldBe(6);
            second.__DotCcRuntime.NativeAllocations.ShouldBeEmpty();
            bool rejected = false;
            try { globfree(result); } catch (InvalidOperationException) { rejected = true; }
            rejected.ShouldBeTrue();
            using (first.__DotCcRuntime.Enter()) globfree(result);
            first.__DotCcRuntime.NativeAllocations.ShouldBeEmpty();
            fixed (byte* pattern = "absent/*.conf\0"u8) GlobManaged(first, pattern, 0, &Error, result).ShouldBe(3);
            first.CorrectOwner.ShouldBeTrue(); first.ErrorPath.ShouldBe("absent"); first.ErrorCode.ShouldBe(ENOENT);
            first.Throw = true;
            bool threw = false;
            try { fixed (byte* pattern = "absent/*.conf\0"u8) GlobManaged(first, pattern, 0, &Error, result); }
            catch (InvalidOperationException exception) { threw = exception.Message == "glob callback"; }
            threw.ShouldBeTrue();
            ReferenceEquals(RuntimeContext.Current, second.__DotCcRuntime).ShouldBeTrue();
            first.__DotCcRuntime.NativeAllocations.ShouldBeEmpty();
            first.Throw = false;
            first.CallbackResult = 1;
            fixed (byte* pattern = "absent/*.conf\0"u8) GlobManaged(first, pattern, 0, &Error, result).ShouldBe(2);
            ReferenceEquals(RuntimeContext.Current, second.__DotCcRuntime).ShouldBeTrue();
        }
        finally { Directory.Delete(root, true); }
    }
    [Theory]
    [InlineData(8)] [InlineData(32)] [InlineData(512)] [InlineData(1024)] [InlineData(2048)] [InlineData(4096)] [InlineData(16384)]
    public void Unsupported_flags_fail_explicitly(int flags)
    {
        byte* result = stackalloc byte[72];
        fixed (byte* pattern = "*\0"u8) glob(pattern, flags, null, result).ShouldBe(2);
        errno.ShouldBe(ENOTSUP); Results(result).ShouldBeEmpty();
    }
    [Fact]
    public void No_matches_null_arguments_and_directory_errors_have_defined_results()
    {
        string root = Tree();
        try
        {
            using var program = new Program(); using var binding = program.__DotCcRuntime.Enter(); Change(root);
            byte* result = stackalloc byte[72];
            glob(null, 0, null, result).ShouldBe(2); errno.ShouldBe(EFAULT);
            fixed (byte* pattern = "missing*\0"u8) glob(pattern, 0, null, result).ShouldBe(3);
            fixed (byte* pattern = "absent/*\0"u8) glob(pattern, 1, null, result).ShouldBe(2);
            errno.ShouldBe(ENOENT);
            fixed (byte* pattern = "a.conf/*\0"u8) GlobManaged(program, pattern, 1, &Error, result).ShouldBe(3);
            program.ErrorPath.ShouldBeNull();
            Results(result).ShouldBeEmpty();
        }
        finally { Directory.Delete(root, true); }
    }
}

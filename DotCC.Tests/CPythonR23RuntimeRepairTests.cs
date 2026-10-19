using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class CPythonR23RuntimeRepairTests
{
    private static int calls, thread;
    private static RuntimeContext? expected;
    private static void Handler(int sig)
    {
        sig.ShouldBe(2);
        RuntimeContext.Current.ShouldBeSameAs(expected);
        Environment.CurrentManagedThreadId.ShouldBe(thread);
        calls++;
    }

    [Fact]
    public void Raise_finishes_one_handler_on_raising_thread_and_restores_foreign_context()
    {
        using var registered = new RuntimeContext();
        using var raising = new RuntimeContext();
        expected = registered; calls = 0; thread = Environment.CurrentManagedThreadId;
        try
        {
            using (registered.Enter())
            {
                ((nint)signal(2, &Handler)).ShouldBe(0);
                ((nint)signal(2, &Handler)).ShouldBe((nint)(delegate*<int, void>)&Handler);
            }
            Should.Throw<InvalidOperationException>(() => registered.Dispose());
            using (raising.Enter())
            {
                errno = 74;
                raise(2).ShouldBe(0);
                calls.ShouldBe(1);
                RuntimeContext.Current.ShouldBeSameAs(raising);
                errno.ShouldBe(74);
            }
        }
        finally { signal(2, null); }
    }

    [Fact]
    public void Ignore_and_error_dispositions_are_values_not_callback_addresses()
    {
        try
        {
            ((nint)signal(2, (delegate*<int, void>)1)).ShouldBe(0);
            errno = 73;
            raise(2).ShouldBe(0);
            errno.ShouldBe(73);
            ((nint)signal(2, (delegate*<int, void>)(nint)(-1))).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
            ((nint)signal(-1, null)).ShouldBe(-1);
            errno.ShouldBe(EINVAL);
            ((nint)signal(2, null)).ShouldBe(1);
        }
        finally { signal(2, null); }
    }

    [Fact]
    public void Opendir_distinguishes_file_ancestor_symlinks_missing_and_loop()
    {
        if (!OperatingSystem.IsLinux()) return;
        string root = Path.Combine(Path.GetTempPath(), "dotcc-r23-unit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string file = Path.Combine(root, "item"); File.WriteAllText(file, "x");
        File.CreateSymbolicLink(Path.Combine(root, "file-link"), "item");
        File.CreateSymbolicLink(Path.Combine(root, "dangling"), "missing");
        File.CreateSymbolicLink(Path.Combine(root, "loop"), "loop");
        try
        {
            foreach (var (suffix, error) in new[] { ("item", ENOTDIR), ("item/child", ENOTDIR), ("item/", ENOTDIR), ("item/..", ENOTDIR), ("file-link/child", ENOTDIR), ("missing/child", ENOENT), ("dangling", ENOENT), ("loop", ELOOP) })
            fixed (byte* path = Encoding.UTF8.GetBytes(Path.Combine(root, suffix) + "\0"))
            {
                errno = 0;
                ((nint)opendir(path)).ShouldBe(0);
                errno.ShouldBe(error, suffix);
            }
        }
        finally { Directory.Delete(root, true); }
    }
}

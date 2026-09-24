using System;
using System.IO;
using System.Text;
using Shouldly;
using Xunit;
using static DotCC.Libc.Libc;

namespace DotCC.Tests;

[Collection("Runtime")]
public sealed unsafe class PathOwnershipTests
{
    private static int Change(string path)
    {
        fixed (byte* value = Encoding.UTF8.GetBytes(path + "\0")) return chdir(value);
    }
    private static string Cwd()
    {
        byte* buffer = stackalloc byte[4096];
        ((nint)getcwd(buffer, 4096)).ShouldNotBe(0);
        return Encoding.UTF8.GetString(buffer, strlen(buffer));
    }

    [Fact]
    public void Owner_captures_creation_directory_before_first_binding()
    {
        string saved = Directory.GetCurrentDirectory();
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            Directory.SetCurrentDirectory(root);
            using var owner = new RuntimeContext();
            Directory.SetCurrentDirectory(saved);
            using (owner.Enter()) Cwd().ShouldBe(root);
            Directory.GetCurrentDirectory().ShouldBe(saved);
        }
        finally { Directory.SetCurrentDirectory(saved); Directory.Delete(root, true); }
    }

    [Fact]
    public void Two_owners_resolve_file_open_stat_rename_remove_and_directory_operations_independently()
    {
        string processCwd = Directory.GetCurrentDirectory();
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            using var first = new RuntimeContext(); using var second = new RuntimeContext();
            string a = Directory.CreateDirectory(Path.Combine(root, "a")).FullName;
            string b = Directory.CreateDirectory(Path.Combine(root, "b")).FullName;
            byte* status = stackalloc byte[128];
            foreach (var pair in new[] { (first, a, 'A'), (second, b, 'B') })
            using (pair.Item1.Enter())
            {
                Change(pair.Item2).ShouldBe(0);
                fixed (byte* name = "data\0"u8)
                fixed (byte* mode = "w\0"u8)
                {
                    var file = fopen(name, mode); ((nint)file).ShouldNotBe(0);
                    fputc(pair.Item3, file).ShouldBe(pair.Item3); fclose(file).ShouldBe(0);
                    stat(name, status).ShouldBe(0);
                    access(name, 0).ShouldBe(0);
                    int fd = open(name, 0); fd.ShouldBeGreaterThanOrEqualTo(3);
                    byte value; read(fd, &value, 1).ShouldBe(1); value.ShouldBe((byte)pair.Item3);
                    close(fd).ShouldBe(0);
                    byte* absolute = realpath(name, null);
                    Encoding.UTF8.GetString(absolute, strlen(absolute)).ShouldBe(Path.Combine(pair.Item2, "data"));
                    free(absolute);
                }
                fixed (byte* oldName = "data\0"u8)
                fixed (byte* newName = "renamed\0"u8) rename(oldName, newName).ShouldBe(0);
                fixed (byte* directory = "sub\0"u8) { mkdir(directory, 0x1c0).ShouldBe(0); rmdir(directory).ShouldBe(0); }
                Cwd().ShouldBe(pair.Item2);
            }
            File.ReadAllText(Path.Combine(a, "renamed")).ShouldBe("A");
            File.ReadAllText(Path.Combine(b, "renamed")).ShouldBe("B");
            using (first.Enter()) { fixed (byte* name = "renamed\0"u8) remove(name).ShouldBe(0); }
            File.Exists(Path.Combine(b, "renamed")).ShouldBeTrue();
            Directory.GetCurrentDirectory().ShouldBe(processCwd);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Directory_tokens_are_owner_scoped_and_retirement_preserves_other_owner()
    {
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            using var first = new RuntimeContext(); using var second = new RuntimeContext();
            void* firstDir;
            using (first.Enter())
            {
                Change(root).ShouldBe(0);
                fixed (byte* name = ".\0"u8) firstDir = opendir(name);
                ((nint)firstDir).ShouldNotBe(0);
            }
            using (second.Enter())
            {
                Change(root).ShouldBe(0);
                void* live; fixed (byte* name = ".\0"u8) live = opendir(name);
                closedir(firstDir).ShouldBe(-1); errno.ShouldBe(EBADF);
                ((nint)readdir(firstDir)).ShouldBe(0); errno.ShouldBe(EBADF);
                first.Dispose();
                ((nint)readdir(live)).ShouldNotBe(0);
                rewinddir(live);
                byte* entry = (byte*)readdir(live);
                Encoding.UTF8.GetString(entry, strlen(entry)).ShouldBe(".");
                closedir(live).ShouldBe(0);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Native_path_calls_resolve_owner_directory_and_symlink_targets_stay_relative()
    {
        if (OperatingSystem.IsWindows()) Assert.Skip("POSIX native path calls");
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            Change(root).ShouldBe(0); File.WriteAllText(Path.Combine(root, "data"), "value");
            fixed (byte* target = "data\0"u8)
            fixed (byte* hard = "hard\0"u8)
            fixed (byte* symbolic = "symbolic\0"u8)
            fixed (byte* fifo = "fifo\0"u8)
            {
                link(target, hard).ShouldBe(0);
                chown(target, uint.MaxValue, uint.MaxValue).ShouldBe(0);
                chmod(target, 0x180).ShouldBe(0);
                symlink(target, symbolic).ShouldBe(0);
                byte* value = stackalloc byte[16];
                long length = readlink(symbolic, value, 16);
                length.ShouldBe(4); Encoding.UTF8.GetString(value, (int)length).ShouldBe("data");
                mkfifo(fifo, 0x180).ShouldBe(0);
                File.Exists(Path.Combine(root, "fifo")).ShouldBeTrue();
                File.ReadAllText(Path.Combine(root, "hard")).ShouldBe("value");
                File.ReadAllText(Path.Combine(root, "symbolic")).ShouldBe("value");
                unlink(hard).ShouldBe(0);
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Chdir_resolves_symlink_before_parent_components_and_does_not_change_on_failure()
    {
        if (OperatingSystem.IsWindows()) Assert.Skip("Symbolic link privilege is host dependent");
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            string nested = Directory.CreateDirectory(Path.Combine(root, "physical", "nested")).FullName;
            Directory.CreateSymbolicLink(Path.Combine(root, "shortcut"), nested);
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            Change(root).ShouldBe(0); Change("shortcut/..").ShouldBe(0);
            Cwd().ShouldBe(Path.Combine(root, "physical"));
            Change("missing").ShouldBe(-1); errno.ShouldBe(ENOENT);
            Cwd().ShouldBe(Path.Combine(root, "physical"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Relative_loader_paths_use_owner_directory_but_bare_sonames_keep_loader_search()
    {
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            Change(root).ShouldBe(0);
            fixed (byte* name = "./missing-dotcc-module.so\0"u8) ((nint)dlopen(name, 0)).ShouldBe(0);
            byte* error = dlerror(); Encoding.UTF8.GetString(error, strlen(error)).ShouldContain(root);
            fixed (byte* name = "missing-dotcc-soname.so\0"u8) ((nint)dlopen(name, 0)).ShouldBe(0);
            error = dlerror(); Encoding.UTF8.GetString(error, strlen(error)).ShouldNotContain(root);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Child_process_uses_owner_directory_without_changing_host_directory()
    {
        string processCwd = Directory.GetCurrentDirectory();
        string root = Directory.CreateTempSubdirectory("dotcc-path-").FullName;
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            Change(root).ShouldBe(0);
            fixed (byte* command = "echo owned > child.txt\0"u8) system(command).ShouldBe(0);
            File.ReadAllText(Path.Combine(root, "child.txt")).Trim().ShouldBe("owned");
            Directory.GetCurrentDirectory().ShouldBe(processCwd);
        }
        finally { Directory.Delete(root, true); }
    }
}

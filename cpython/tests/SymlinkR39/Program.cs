using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int checks, failures;
    private static readonly Dictionary<string, string> native = new();
    private static byte[] C(string value) => Encoding.UTF8.GetBytes(value + '\0');
    private static void Check(string name, bool value)
    {
        checks++;
        if (!value) { failures++; Console.WriteLine($"FAIL {name} errno={errno}"); }
    }
    private static int Link(string target, string path)
    { fixed (byte* t = C(target)) fixed (byte* p = C(path)) return symlink(t, p); }
    private static long Read(string path, byte* buffer, ulong size)
    { fixed (byte* p = C(path)) return readlink(p, buffer, size); }
    private static int Change(string path)
    { fixed (byte* p = C(path)) return chdir(p); }

    private static void Error(string name, Func<long> action)
    {
        errno = 0;
        try
        {
            long result = action();
            string row = $"ERROR {name} {result} {errno}";
            Console.WriteLine(row);
            Check(name, native[name] == row);
        }
        catch (Exception e) { Check(name + "-escaped-" + e.GetType().Name, false); }
    }

    private static void ReadCase(string name, ulong size)
    {
        byte[] buffer = new byte[32]; Array.Fill(buffer, (byte)0x5a);
        errno = 123;
        try
        {
            long result;
            fixed (byte* b = buffer) result = Read("link", b + 4, size);
            string row = $"READ {name} {result} {errno} {Convert.ToHexStringLower(buffer)}";
            Console.WriteLine(row);
            Check(name, native[name] == row);
        }
        catch (Exception e) { Check(name + "-escaped-" + e.GetType().Name, false); }
    }

    private static void Boundary(string root)
    {
        File.WriteAllText(Path.Combine(root, "regular"), "");
        Directory.CreateDirectory(Path.Combine(root, "directory"));
        string denied = Directory.CreateDirectory(Path.Combine(root, "denied")).FullName;
        Check("setup-link", Link("../target", "link") == 0);
        Check("setup-broken", Link("absent", "broken") == 0);
        Check("setup-loop", Link("loop", "loop") == 0);
        byte* buffer = stackalloc byte[32];
        new Span<byte>(buffer, 32).Fill(0x5a);
        nint address = (nint)buffer;
        foreach (var row in new[] { ("missing", "absent"), ("missing-parent", "absent/link"),
                 ("not-directory", "regular/link"), ("loop-parent", "loop/link"),
                 ("regular", "regular"), ("directory", "directory"), ("empty", "") })
            Error("read-" + row.Item1, () => Read(row.Item2, (byte*)address, 32));
        foreach (var row in new[] { ("missing-parent", "absent/link"), ("not-directory", "regular/link"),
                 ("loop-parent", "loop/link"), ("existing-regular", "regular"),
                 ("existing-directory", "directory"), ("existing-broken", "broken"),
                 ("existing-loop", "loop") })
            Error("create-" + row.Item1, () => Link("value", row.Item2));
        Error("create-empty-target", () => Link("", "empty-target"));
        Error("create-empty-path", () => Link("value", ""));
        Error("read-null-path", () => readlink(null, (byte*)address, 32));
        Error("read-null-buffer", () => Read("link", null, 32));
        Error("read-zero-null", () => readlink(null, null, 0));
        Error("create-null-target", () => { fixed (byte* p = "null-target\0"u8) return symlink(null, p); });
        Error("create-null-path", () => { fixed (byte* p = "value\0"u8) return symlink(p, null); });
        File.SetUnixFileMode(denied, 0);
        try
        {
            Error("read-denied", () => Read("denied/link", (byte*)address, 32));
            Error("create-denied", () => Link("value", "denied/link"));
        }
        finally { File.SetUnixFileMode(denied, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute); }
        for (int i = 0; i < 32; i++) Check("error-buffer-untouched", buffer[i] == 0x5a);
        foreach (var row in new[] { ("full", 32UL), ("truncated", 3UL), ("zero", 0UL),
                 ("int-max", (ulong)int.MaxValue), ("int-max-plus-one", (ulong)int.MaxValue + 1),
                 ("uint-max", (ulong)uint.MaxValue), ("uint-max-plus-one", (ulong)uint.MaxValue + 1),
                 ("uint-max-plus-four", (ulong)uint.MaxValue + 4), ("size-max", ulong.MaxValue) })
            ReadCase(row.Item1, row.Item2);
    }

    // Direct service equivalents of the retained, unchanged R37 C's 17 checks.
    // This is not an emitted-C proof; A31 must run both actual C fixtures later.
    private static void R37()
    {
        int before = failures;
        int fd; fixed (byte* p = "target\0"u8) fd = open(p, 1 | 0x40 | 0x80, 0x180);
        Check("r37-create", fd >= 0);
        fixed (byte* p = "hello"u8) Check("r37-write", write(fd, p, 5) == 5);
        Check("r37-close", close(fd) == 0);
        Check("r37-symlink", Link("target", "r37-link") == 0);
        byte* b = stackalloc byte[32]; new Span<byte>(b, 32).Fill(0x5a);
        Check("r37-read", Read("r37-link", b, 32) == 6 && new ReadOnlySpan<byte>(b, 6).SequenceEqual("target"u8) && b[6] == 0x5a);
        new Span<byte>(b, 32).Fill(0x5a);
        Check("r37-truncated", Read("r37-link", b, 3) == 3 && new ReadOnlySpan<byte>(b, 3).SequenceEqual("tar"u8) && b[3] == 0x5a);
        byte* st = stackalloc byte[96];
        fixed (byte* p = "r37-link\0"u8)
        {
            Check("r37-lstat", lstat(p, st) == 0 && (*(uint*)(st + 16) & 0xf000) == 0xa000 && *(long*)(st + 40) == 6);
            Check("r37-stat", stat(p, st) == 0 && (*(uint*)(st + 16) & 0xf000) == 0x8000 && *(long*)(st + 40) == 5);
        }
        Check("r37-broken", Link("missing", "r37-broken") == 0);
        fixed (byte* p = "r37-broken\0"u8)
            Check("r37-lstat-broken", lstat(p, st) == 0 && (*(uint*)(st + 16) & 0xf000) == 0xa000 && *(long*)(st + 40) == 7);
        Check("r37-read-broken", Read("r37-broken", b, 32) == 7 && new ReadOnlySpan<byte>(b, 7).SequenceEqual("missing"u8));
        fixed (byte* p = "r37-broken\0"u8) { errno = 0; Check("r37-stat-broken", stat(p, st) == -1 && errno == ENOENT); }
        errno = 0; Check("r37-duplicate", Link("target", "r37-link") == -1 && errno == EEXIST);
        errno = 0; Check("r37-duplicate-broken", Link("missing", "r37-broken") == -1 && errno == EEXIST);
        errno = 0; Check("r37-read-regular", Read("target", b, 32) == -1 && errno == EINVAL);
        errno = 0; Check("r37-read-missing", Read("absent", b, 32) == -1 && errno == ENOENT);
        errno = 0; Check("r37-create-missing-parent", Link("target", "absent/r37-link") == -1 && errno == ENOENT);
        Console.WriteLine($"R37 checks=17 failures={failures - before}");
    }

    private static int Main(string[] args)
    {
        if (args.Length != 2 || !OperatingSystem.IsLinux()) return 2;
        foreach (string line in File.ReadLines(args[1]))
            if (line.StartsWith("ERROR ") || line.StartsWith("READ ")) native.Add(line.Split(' ')[1], line);
        Check("native-inventory", native.Count == 32);
        string processCwd = Directory.GetCurrentDirectory();
        string legacy = Directory.CreateDirectory(Path.Combine(args[0], "legacy")).FullName;
        string owned = Directory.CreateDirectory(Path.Combine(args[0], "owned")).FullName;
        string other = Directory.CreateDirectory(Path.Combine(args[0], "other")).FullName;
        Check("initial-legacy-context", RuntimeContext.Current == null);
        try { Directory.SetCurrentDirectory(legacy); Boundary(legacy); R37(); }
        finally { Directory.SetCurrentDirectory(processCwd); }
        using var owner = new RuntimeContext(); using var second = new RuntimeContext();
        using (owner.Enter())
        {
            Check("owner-change", Change(owned) == 0);
            Boundary(owned); R37();
            Check("lexical-target", new FileInfo(Path.Combine(owned, "link")).LinkTarget == "../target");
            using (second.Enter())
            {
                Check("other-change", Change(other) == 0);
                byte* b = stackalloc byte[16];
                errno = 0; Check("other-missing", Read("link", b, 16) == -1 && errno == ENOENT);
                Check("other-create", Link("other-target", "link") == 0);
            }
            Check("owner-restored", ReferenceEquals(RuntimeContext.Current, owner));
            Check("other-target", new FileInfo(Path.Combine(other, "link")).LinkTarget == "other-target");
            byte* restored = stackalloc byte[16];
            Check("owner-read-restored", Read("link", restored, 16) == 9 && new ReadOnlySpan<byte>(restored, 9).SequenceEqual("../target"u8));
            Check("process-cwd-unchanged", Directory.GetCurrentDirectory() == processCwd);
        }
        Check("legacy-context-restored", RuntimeContext.Current == null);
        Console.WriteLine($"SUMMARY checks={checks} failures={failures}");
        return failures == 0 ? 0 : 1;
    }
}

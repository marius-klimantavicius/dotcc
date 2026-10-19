using System.Runtime.InteropServices;
using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    [DllImport("metadata_oracle")] static extern void target_layout(ulong* result);
    [DllImport("metadata_oracle")] static extern void host_layout(ulong* result);
    [DllImport("metadata_oracle")] static extern int host_values(byte* path, int nofollow, ulong* result);
    static readonly string[] Fields = ["dev", "ino", "mode", "nlink", "uid", "gid", "size", "atime", "mtime", "ctime", "rdev", "blksize", "blocks"];
    static readonly List<string> Failures = [];
    static int checks;
    static ulong[] offsets = [];
    static int size;
    static void Check(bool condition, string name) { checks++; if (!condition) Failures.Add(name); }
    static byte[] Z(string value) => Encoding.UTF8.GetBytes(value + "\0");
    static ulong Field(byte* buffer, int i) => i is 2 or 4 or 5 ? *(uint*)(buffer + offsets[i]) : *(ulong*)(buffer + offsets[i]);
    static ulong[] Compare(string path, bool nofollow = false, int fd = -1)
    {
        var bytes = new byte[size + 16]; Array.Fill(bytes, (byte)0xa5);
        var expected = new ulong[13]; var actual = new ulong[13];
        fixed (byte* p = Z(path)) fixed (byte* b = bytes) fixed (ulong* e = expected)
        {
            Check(host_values(p, nofollow ? 1 : 0, e) == 0, "host stat " + path);
            errno = 71;
            int r = fd >= 0 ? fstat(fd, b) : nofollow ? lstat(p, b) : stat(p, b);
            Check(r == 0, "runtime stat " + path + " errno=" + errno);
            Check(errno == 71, "successful stat preserves errno");
            for (int i = 0; i < actual.Length; i++)
            {
                actual[i] = Field(b, i);
                Check(actual[i] == expected[i], $"{path} {Fields[i]} actual={actual[i]} host={expected[i]} fd={fd} nofollow={nofollow}");
            }
            Check(new ReadOnlySpan<byte>(b + size, 16).ToArray().All(x => x == 0xa5), "stat buffer tail guard");
        }
        return actual;
    }
    static int Open(string path, int mode)
    {
        fixed (byte* p = Z(path)) { int fd = open(p, 2 | 0x40 | 0x80, mode); Check(fd >= 0, "open " + path); return fd; }
    }
    static void Error(string path, bool nofollow, int error)
    {
        byte* b = stackalloc byte[size]; new Span<byte>(b, size).Fill(0xa5);
        fixed (byte* p = Z(path))
        {
            errno = 0; Check((nofollow ? lstat(p, b) : stat(p, b)) == -1 && errno == error, "error " + path + " expected=" + error + " actual=" + errno);
        }
        Check(new ReadOnlySpan<byte>(b, size).ToArray().All(x => x == 0xa5), "failed stat leaves output untouched");
    }
    static int Main(string[] args)
    {
        ulong* target = stackalloc ulong[15]; ulong* host = stackalloc ulong[15];
        target_layout(target); host_layout(host);
        Console.WriteLine("target-layout " + string.Join(" ", new ReadOnlySpan<ulong>(target, 15).ToArray()));
        Console.WriteLine("host-layout " + string.Join(" ", new ReadOnlySpan<ulong>(host, 15).ToArray()));
        size = checked((int)target[0]); offsets = new ReadOnlySpan<ulong>(target + 2, 13).ToArray();
        Check(size == 96 && target[1] == 8, "actual target header size/alignment");
        string root = args[0]; Directory.CreateDirectory(root);
        byte* statBuffer = stackalloc byte[size];
        foreach (int mode in new[] { 0x180, 0x1a4, 0x1b6 })
        {
            string path = Path.Combine(root, "file-" + mode); int fd = Open(path, mode);
            fixed (byte* text = "metadata\0"u8) Check(write(fd, text, 8) == 8, "write bytes");
            var before = Compare(path); Compare(path, fd: fd);
            Check(fchmod(fd, 0x1a0) == 0, "fchmod 0640"); Compare(path, fd: fd); Compare(path);
            string alias = path + ".hard"; fixed (byte* p = Z(path)) fixed (byte* a = Z(alias)) Check(link(p, a) == 0, "hard link");
            var linked = Compare(alias); Check(linked[0] == before[0] && linked[1] == before[1] && linked[3] == 2, "hardlink identity and count");
            string renamed = path + ".renamed"; File.Move(path, renamed); Compare(renamed, fd: fd);
            File.Delete(renamed); Compare(alias, fd: fd);
            File.Delete(alias);
            Check(fstat(fd, statBuffer) == 0 && Field(statBuffer, 1) == before[1] && Field(statBuffer, 3) == 0 && Field(statBuffer, 6) == 8, "fstat unlinked owning handle");
            Check(close(fd) == 0, "close"); errno = 0; Check(fstat(fd, statBuffer) == -1 && errno == 9, "closed descriptor EBADF");
        }
        string dir = Path.Combine(root, "dir"); Directory.CreateDirectory(dir); Compare(dir);
        string file = Path.Combine(root, "source"); File.WriteAllText(file, "source");
        string sym = Path.Combine(root, "symlink"); File.CreateSymbolicLink(sym, "source"); Compare(sym); var symlink = Compare(sym, true);
        Check((symlink[2] & 0xf000) == 0xa000 && symlink[6] == 6, "lstat symlink type and target byte size");
        File.Delete(file); Error(sym, false, 2); Compare(sym, true);
        string dsym = Path.Combine(root, "dir-symlink"); Directory.CreateSymbolicLink(dsym, "dir"); Compare(dsym); Compare(dsym, true);
        string loop = Path.Combine(root, "loop"); File.CreateSymbolicLink(loop, "loop"); Error(loop, false, 40); Compare(loop, true);
        Error(Path.Combine(root, "absent"), false, 2); Error(Path.Combine(root, "absent"), true, 2);
        string plain = Path.Combine(root, "plain"); File.WriteAllText(plain, "x"); Error(plain + "/child", false, 20); Error("", false, 2);
        File.SetLastAccessTimeUtc(plain, new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(plain, new DateTime(2002, 3, 4, 5, 6, 7, DateTimeKind.Utc)); Compare(plain);
        string denied = Path.Combine(root, "denied"); Directory.CreateDirectory(denied); File.WriteAllText(Path.Combine(denied, "item"), "x");
        File.SetUnixFileMode(denied, (UnixFileMode)0);
        try { Error(Path.Combine(denied, "item"), false, 13); Error(Path.Combine(denied, "item"), true, 13); }
        finally { File.SetUnixFileMode(denied, (UnixFileMode)0x1c0); }
        errno = 0; Check(stat(null, statBuffer) == -1 && errno == 14, "null stat path EFAULT");
        errno = 0; fixed (byte* p = Z(plain)) Check(stat(p, null) == -1 && errno == 14, "null stat buffer EFAULT");
        errno = 0; Check(lstat(null, statBuffer) == -1 && errno == 14, "null lstat path EFAULT");
        errno = 0; Check(fstat(0, null) == -1 && errno == 14, "null fstat buffer EFAULT");
        // Generic permission-derived atomic output: the same stat-mode | 0200
        // contract used by importlib, followed by real creation and reopening.
        string source = Path.Combine(root, "mode-source"); File.WriteAllText(source, "source"); File.SetUnixFileMode(source, (UnixFileMode)0x120);
        int createMode = checked((int)Compare(source)[2]) | 0x80;
        string temporary = Path.Combine(root, "mode-output.tmp"); int outputFd = Open(temporary, createMode);
        fixed (byte* text = "value = 42\n"u8) Check(write(outputFd, text, 11) == 11, "permission-derived output write");
        Check(close(outputFd) == 0, "permission-derived output close");
        string final = Path.Combine(root, "mode-output"); File.Move(temporary, final);
        Check((File.GetUnixFileMode(final) & UnixFileMode.UserRead) != 0, "physical permission-derived output readable");
        Compare(final); Check(File.ReadAllText(final) == "value = 42\n", "real reopen permission-derived output bytes");
        Compare("/dev/null");
        using (var ownerA = new RuntimeContext()) using (var ownerB = new RuntimeContext())
        {
            int a, b; string pa = Path.Combine(root, "owner-a"), pb = Path.Combine(root, "owner-b");
            using (ownerA.Enter()) { a = Open(pa, 0x180); Compare(pa, fd: a); }
            using (ownerB.Enter()) { byte* output = stackalloc byte[size]; errno = 0; Check(fstat(a, output) == -1 && errno == 9, "foreign owner fd EBADF"); b = Open(pb, 0x1a4); Compare(pb, fd: b); }
            Check(a == b, "owners reuse descriptor index");
            using (ownerA.Enter()) { Compare(pa, fd: a); Check(close(a) == 0, "owner A close"); }
            using (ownerB.Enter()) { GC.Collect(2, GCCollectionMode.Forced, true, true); Compare(pb, fd: b); }
            var worker = new Thread(() => { using (ownerB.Enter()) Compare(pb, fd: b); }); worker.Start(); worker.Join();
        }
        Console.WriteLine($"checks={checks} failures={Failures.Count}"); foreach (string failure in Failures) Console.WriteLine("FAIL " + failure);
        return Failures.Count == 0 ? 0 : 1;
    }
}

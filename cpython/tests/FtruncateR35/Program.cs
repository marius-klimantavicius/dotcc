using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int checks, failures;
    private static readonly Dictionary<string, (int Result, int Error)> nativeErrors = new();
    private static byte[] CString(string value) => Encoding.UTF8.GetBytes(value + '\0');
    private static void Equal(string name, long actual, long expected)
    {
        checks++;
        if (actual != expected)
        {
            failures++;
            Console.WriteLine($"FAIL {name} actual={actual} expected={expected}");
        }
    }

    private static void ResizeCase(int fd, long initial, long position, long length)
    {
        Equal("initial-length", ftruncate(fd, initial), 0);
        Equal("set-position", lseek(fd, position, SEEK_SET), position);
        errno = 123;
        Equal("resize", ftruncate(fd, length), 0);
        Equal("success-errno", errno, 123);
        Equal("preserved-position", lseek(fd, 0, SEEK_CUR), position);
        byte* status = stackalloc byte[96];
        Equal("stat", fstat(fd, status), 0);
        Equal("length", *(long*)(status + 40), length);
        byte marker = 0x5a, observed = 0;
        Equal("write", write(fd, &marker, 1), 1);
        Equal("write-position", lseek(fd, 0, SEEK_CUR), position + 1);
        Equal("stat-after-write", fstat(fd, status), 0);
        Equal("length-after-write", *(long*)(status + 40), Math.Max(length, position + 1));
        Equal("seek-marker", lseek(fd, position, SEEK_SET), position);
        Equal("read-marker", read(fd, &observed, 1), 1);
        Equal("marker", observed, marker);
    }

    private static void ErrorCase(string name, int fd, long length)
    {
        errno = 0;
        int result = ftruncate(fd, length), error = errno;
        Console.WriteLine($"ERROR {name} {result} {error}");
        var native = nativeErrors[name];
        Equal(name + "-result", result, native.Result);
        Equal(name + "-errno", error, native.Error);
    }

    private static int Main(string[] args)
    {
        if (args.Length != 2) return 2;
        foreach (string line in File.ReadLines(args[1]))
        {
            if (!line.StartsWith("ERROR ", StringComparison.Ordinal)) continue;
            string[] fields = line.Split(' ');
            nativeErrors.Add(fields[1], (int.Parse(fields[2]), int.Parse(fields[3])));
        }
        Equal("native-error-inventory", nativeErrors.Count, 10);
        using var owner = new RuntimeContext();
        using (owner.Enter())
        fixed (byte* path = CString(args[0]))
        {
            int fd = open(path, 2 | 0x40 | 0x80, 0x180);
            if (fd < 0) return 3;
            try
            {
                ResizeCase(fd, 5, 5, 0);
                ResizeCase(fd, 8, 6, 3);
                ResizeCase(fd, 8, 2, 3);
                ResizeCase(fd, 8, 11, 8);
                ResizeCase(fd, 8, 11, 20);
                ResizeCase(fd, 8, 25, 20);
                ResizeCase(fd, 2147483650L, 2147483650L, 2147483649L);
                ResizeCase(fd, 2147483649L, 2147483650L, 2147483651L);
                Equal("reset-length", ftruncate(fd, 8), 0);
                Equal("reset-position", lseek(fd, 6, SEEK_SET), 6);
                ErrorCase("negative", fd, -1);
                ErrorCase("minimum", fd, long.MinValue);
                ErrorCase("maximum-length", fd, long.MaxValue);
                Equal("error-position", lseek(fd, 0, SEEK_CUR), 6);
                byte* status = stackalloc byte[96];
                Equal("error-stat", fstat(fd, status), 0);
                Equal("error-length", *(long*)(status + 40), 8);
                Equal("truncate-after-errors", ftruncate(fd, 0), 0);
                Equal("preserved-position-after-errors", lseek(fd, 0, SEEK_CUR), 6);
                int ro = open(path, 0);
                if (ro < 0) return 4;
                try { ErrorCase("readonly", ro, 0); }
                finally { Equal("close-readonly", close(ro), 0); }
                int* descriptors = stackalloc int[2];
                Equal("pipe", pipe(descriptors), 0);
                try
                {
                    ErrorCase("pipe-read", descriptors[0], 0);
                    ErrorCase("pipe-write", descriptors[1], 0);
                }
                finally
                {
                    Equal("close-pipe-read", close(descriptors[0]), 0);
                    Equal("close-pipe-write", close(descriptors[1]), 0);
                }
                int sock = socket(1, 1, 0);
                if (sock < 0) return 5;
                try { ErrorCase("socket", sock, 0); }
                finally { Equal("close-socket", close(sock), 0); }
            }
            finally { Equal("close", close(fd), 0); }
            ErrorCase("closed", fd, 0);
            ErrorCase("invalid", -1, 0);
            ErrorCase("invalid-negative", -1, -1);
            Equal("unlink", unlink(path), 0);
        }
        Console.WriteLine($"SUMMARY checks={checks} failures={failures}");
        return failures == 0 ? 0 : 1;
    }
}

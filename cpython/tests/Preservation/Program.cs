using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int checks;
    private static void Check(bool value, string message)
    {
        checks++;
        if (!value) throw new InvalidOperationException(message);
    }
    private static byte[] PathBytes(string path) => Encoding.UTF8.GetBytes(path + "\0");
    private static FILE* Open(string path, string mode)
    {
        fixed (byte* name = PathBytes(path), access = PathBytes(mode)) return fopen(name, access);
    }
    private static FILE* Reopen(string path, string mode, FILE* file)
    {
        fixed (byte* name = PathBytes(path), access = PathBytes(mode)) return freopen(name, access, file);
    }

    public static int Main()
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-r7-stdio-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using var runtime = new RuntimeContext();
            using var binding = runtime.Enter();
            byte* buffer = stackalloc byte[64];
            string oldPath = Path.Combine(directory, "old"), newPath = Path.Combine(directory, "new");
            foreach (bool samePath in new[] { false, true })
            {
                FILE* file = Open(oldPath, "w+");
                Check(file != null, "open old backing");
                Check(setvbuf(file, buffer, 0, 64) == 0, "install borrowed buffer");
                Check(fputc('A', file) == 'A' && buffer[0] == 'A', "buffer old output in borrowed storage");
                Check(Reopen(samePath ? oldPath : newPath, "w+", file) == file, "reopen keeps FILE identity");
                Check(fputc('B', file) == 'B' && buffer[0] == 'B', "borrowed buffer remains valid on new association");
                Check(fclose(file) == 0, "close new backing");
                Check(File.ReadAllText(oldPath) == (samePath ? "B" : "A"), "flush before replacement or same-path truncation");
                if (!samePath) Check(File.ReadAllText(newPath) == "B", "no pending old bytes redirected");
            }
            foreach (bool invalidMode in new[] { false, true })
            {
                FILE* file = Open(oldPath, "w+");
                Check(file != null && setvbuf(file, buffer, 0, 64) == 0, "failure control buffered open");
                int descriptor = fileno(file);
                Check(fputc('A', file) == 'A', "buffer before failed reopen");
                errno = 0;
                Check(Reopen(invalidMode ? newPath : Path.Combine(directory, "missing", "child"), invalidMode ? "invalid" : "w+", file) == null,
                    "failed reopen reports NULL");
                Check(errno == (invalidMode ? EINVAL : ENOENT), "failed reopen errno");
                errno = 0;
                Check(fcntl(descriptor, 1) == -1 && errno == EBADF, "failed reopen closes old descriptor");
                Check(File.ReadAllText(oldPath) == "A", "failed reopen still drains old backing");
                // Runtime stale-handle protection is stronger than ISO C; check
                // that descriptor reuse cannot accidentally revive the alias.
                FILE* replacement = Open(newPath, "w+");
                Check(replacement != null && fileno(replacement) == descriptor, "descriptor reuse control");
                Check(fileno(file) == -1 && fputc('X', file) == -1 && errno == EBADF, "stale FILE cannot write reused descriptor");
                Check(fputc('B', replacement) == 'B' && fclose(replacement) == 0, "replacement remains usable");
                Check(File.ReadAllText(newPath) == "B", "replacement not corrupted by stale FILE");
            }
            FILE* full = Open("/dev/full", "w");
            Check(full != null && setvbuf(full, buffer, 0, 64) == 0 && fputc('A', full) == 'A', "old flush-error association");
            Check(Reopen(newPath, "w+", full) == full && ferror(full) == 0 && feof(full) == 0, "reopen succeeds after old flush failure and clears indicators");
            Check(fputc('B', full) == 'B' && fclose(full) == 0, "write new association after flush failure");
            Check(File.ReadAllText(newPath) == "B", "failed old byte never redirected");
            FILE* readOnly = Open(oldPath, "r");
            Check(readOnly != null && fgetc(readOnly) == 'A' && fgetc(readOnly) == -1 && feof(readOnly) != 0, "old EOF control");
            Check(ungetc('Z', readOnly) == 'Z', "old pushback control");
            Check(Reopen(newPath, "r", readOnly) == readOnly && feof(readOnly) == 0 && ferror(readOnly) == 0, "reopen resets input state");
            Check(fgetc(readOnly) == 'B', "old pushback discarded");
            Check(fputc('X', readOnly) == -1 && errno == EBADF && ferror(readOnly) != 0, "new access mode enforced");
            Check(fclose(readOnly) == 0, "close read-only association");
            Console.WriteLine($"stdio-r7 passed checks={checks} old=A new=B failure-closes=1 flush-error-resets=1");
            return 0;
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}

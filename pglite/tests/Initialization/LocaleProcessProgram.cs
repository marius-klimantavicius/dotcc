using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class LocaleProcessProgram
{
    private static int assertions;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message + $" (errno={errno})");
        assertions++;
    }

    private static byte[] ReadAll(FILE* stream)
    {
        using var output = new MemoryStream();
        int value;
        while ((value = fgetc(stream)) != -1) output.WriteByte((byte)value);
        Check(feof(stream) != 0 && ferror(stream) == 0, "lost clean EOF");
        return output.ToArray();
    }

    private static FILE* Open(string command = "locale -a", string mode = "r")
    {
        fixed (byte* c = Encoding.UTF8.GetBytes(command + "\0"))
        fixed (byte* m = Encoding.UTF8.GetBytes(mode + "\0"))
            return popen(c, m);
    }

    private static FILE* Control(string executable, string mode)
    {
        Environment.SetEnvironmentVariable("DOTCC_LOCALE_PROCESS_CONTROL", mode);
        fixed (byte* read = "r\0"u8) return OpenLocaleUtility(executable, read);
    }

    public static int Main(string[] args)
    {
        string fixture = Path.GetFullPath(args[0]);
        string temporary = Path.Combine(Path.GetTempPath(), "dotcc-locale-process-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        string? original = Environment.GetEnvironmentVariable("DOTCC_LOCALE_PROCESS_CONTROL");
        try
        {
            byte[] expected;
            var start = new ProcessStartInfo("/usr/bin/locale") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("-a");
            using (var process = Process.Start(start)!)
            using (var output = new MemoryStream())
            {
                process.StandardOutput.BaseStream.CopyTo(output);
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Check(process.ExitCode == 0 && errors.Length == 0, "native locale control failed");
                expected = output.ToArray();
            }
            using (var owner = new RuntimeContext())
            {
                using var binding = owner.Enter();
                foreach (string mode in new[] { "r", "re" })
                {
                    FILE* stream = Open(mode: mode);
                    Check(stream != null, "locale spawn failed");
                    Check(owner.LocaleProcessCount == 1, "child registration missing");
                    Check(fcntl(fileno(stream), 1 /* Linux F_GETFD */) == (mode == "re" ? 1 : 0), "close-on-exec mode lost");
                    using (var foreign = new RuntimeContext())
                    using (foreign.Enter())
                    {
                        Check(pclose(stream) == -1 && errno == EBADF, "foreign owner closed child");
                    }
                    Check(ReadAll(stream).SequenceEqual(expected), "locale bytes differ from native utility");
                    Check(pclose(stream) == 0, "actual locale wait status changed");
                    Check(owner.LocaleProcessCount == 0, "completed child retained");
                    Check(pclose(stream) == -1 && errno == EBADF, "stale FILE accepted");
                }
                foreach (string command in new[] { "locale", "/usr/bin/locale -a", "locale -a; exit 0", "locale -a ", "locale -a\n" })
                    Check(Open(command) == null && errno == ENOTSUP, "command policy broadened");
                foreach (string mode in new[] { "w", "we" })
                    Check(Open(mode: mode) == null && errno == ENOTSUP, "write command enabled");
                Check(Open(mode: "r+") == null && errno == EINVAL, "invalid mode accepted");
                FILE* ordinary = tmpfile();
                Check(ordinary != null && pclose(ordinary) == -1 && errno == ECHILD, "ordinary FILE consumed");
                Check(fputc('x', ordinary) == 'x' && fclose(ordinary) == 0, "ordinary FILE was closed");
                fixed (byte* read = "r\0"u8)
                    Check(OpenLocaleUtility(Path.Combine(temporary, "absent"), read) == null && errno == ENOENT, "spawn error not preserved");

                string errors = Path.Combine(temporary, "stderr");
                fixed (byte* path = Encoding.UTF8.GetBytes(errors + "\0"))
                fixed (byte* mode = "wb\0"u8)
                    Check(freopen(path, mode, stderr) != null, "stderr redirection failed");
                FILE* bytes = Control(fixture, "bytes");
                Check(bytes != null, "native byte fixture spawn failed");
                Check(ReadAll(bytes).SequenceEqual(new byte[] { 97, 0, 122, 10 }), "binary stdout changed");
                Check(pclose(bytes) == (7 << 8), "nonzero POSIX status changed");
                Check(fflush(stderr) == 0, "stderr flush failed");
                Check(File.ReadAllBytes(errors).SequenceEqual(Enumerable.Repeat((byte)'e', 100000)), "stderr drain lost bytes or deadlocked");
                FILE* signaled = Control(fixture, "signal");
                Check(signaled != null, "native signal fixture spawn failed");
                ReadAll(signaled);
                Check(pclose(signaled) == 15, "signal wait status was reconstructed as exit status");
                Check(owner.LocaleProcessCount == 0, "control children leaked");
            }
            for (int i = 0; i < 3; i++)
            {
                var owner = new RuntimeContext();
                int pid;
                using (owner.Enter())
                {
                    FILE* abandoned = Control(fixture, "block");
                    Check(abandoned != null, "blocked fixture spawn failed");
                    var line = new StringBuilder();
                    int b;
                    while ((b = fgetc(abandoned)) != '\n' && b != -1) line.Append((char)b);
                    pid = int.Parse(line.ToString());
                    Check(owner.LocaleProcessCount == 1, "abandoned child not registered");
                }
                owner.Dispose();
                Check(owner.LocaleProcessCount == 0, "abandoned child table leaked");
                int status;
                Check(Wait(pid, &status, 1) == -1 && Marshal.GetLastPInvokeError() == ECHILD, "owner left a live child or zombie");
                owner.Dispose();
            }
            // Native host code may already have reaped a child. Disposal of
            // that old owner must not publish ECHILD into a different binding.
            var retired = new RuntimeContext();
            int retiredPid;
            using (retired.Enter())
            {
                FILE* child = Control(fixture, "block");
                Check(child != null, "retired-owner fixture spawn failed");
                var line = new StringBuilder();
                int b;
                while ((b = fgetc(child)) != '\n' && b != -1) line.Append((char)b);
                retiredPid = int.Parse(line.ToString());
            }
            int retiredStatus;
            Check(Kill(retiredPid, 9) == 0 && Wait(retiredPid, &retiredStatus, 0) == retiredPid, "native reap control failed");
            using (var other = new RuntimeContext())
            using (other.Enter())
            {
                errno = EIO;
                retired.Dispose();
                Check(errno == EIO, "retired owner cleanup changed the currently bound owner's errno");
            }
            Console.WriteLine($"PASS {assertions} actual OS locale/process byte/status/ownership/cleanup assertions");
            return 0;
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTCC_LOCALE_PROCESS_CONTROL", original);
            Directory.Delete(temporary, true);
        }
    }

    [DllImport("libc", EntryPoint = "waitpid", SetLastError = true)]
    private static extern int Wait(int pid, int* status, int options);
    [DllImport("libc", EntryPoint = "kill", SetLastError = true)]
    private static extern int Kill(int pid, int signal);
}

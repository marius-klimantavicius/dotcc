using System.Runtime.InteropServices;
using System.Text;
using Managed.PGlite.Initialization;
using Fork = Managed.PGlite.Probes.FullLibcControl;
using Runtime = Managed.PGlite.Probes.FullLibcControl.Libc;
using Fixture = Managed.PGlite.Tests.BootstrapFixture;

internal static unsafe class ProcessServices
{
    private static InitializationProcessTransport? active;
    private static int assertions;

    private static void Check(bool value, string message)
    {
        assertions++;
        if (!value) throw new InvalidOperationException(message);
    }

    private static long SystemCallback(byte* command) => active!.System(Marshal.PtrToStringUTF8((nint)command)!);
    private static Runtime.FILE* OpenCallback(byte* command, byte* mode) =>
        (Runtime.FILE*)active!.Open(Marshal.PtrToStringUTF8((nint)command)!, Marshal.PtrToStringUTF8((nint)mode)!);
    private static int CloseCallback(Runtime.FILE* stream) => active!.Close((nint)stream);

    private sealed class Files : IInitdbFiles
    {
        public nint Open(string path, string mode)
        {
            byte[] name = Encoding.UTF8.GetBytes(path + "\0"), type = Encoding.UTF8.GetBytes(mode + "\0");
            fixed (byte* p = name) fixed (byte* m = type) return (nint)Runtime.fopen(p, m);
        }
        public int Flush(nint stream) => Runtime.fflush((Runtime.FILE*)stream);
        public int Close(nint stream) => Runtime.fclose((Runtime.FILE*)stream);
        public void SetErrno(int error) => Runtime.errno = error;
    }

    private sealed class FaultFiles(bool throwFlush, bool throwFirstClose) : IInitdbFiles
    {
        private readonly Files actual = new();
        public int CloseCalls { get; private set; }
        public nint Open(string path, string mode) => actual.Open(path, mode);
        public int Flush(nint stream) => throwFlush ? throw new IOException("Injected host flush exception") : actual.Flush(stream);
        public int Close(nint stream)
        {
            if (++CloseCalls == 1 && throwFirstClose) throw new IOException("Injected host close exception");
            return actual.Close(stream);
        }
        public void SetErrno(int error) => actual.SetErrno(error);
    }

    private static int InvokeFixture(BootstrapInvocation request, int status)
    {
        using var owner = new Fixture.Libc.RuntimeContext();
        using var binding = owner.Enter();
        byte[] input = Encoding.UTF8.GetBytes(request.InputPath + "\0"), output = Encoding.UTF8.GetBytes(request.OutputPath + "\0");
        fixed (byte* i = input) fixed (byte* o = output)
            return request.Operation == BootstrapOperation.Version ? Fixture.fixture_version(o, status) : Fixture.fixture_copy(i, o, status);
    }

    private static void Reject(string command, string executable)
    {
        bool rejected = false;
        try { InitdbCommand.Parse(command, executable); }
        catch (Exception error) when (error is ArgumentException or NotSupportedException) { rejected = true; }
        Check(rejected, "Accepted unsupported shell command: " + command);
    }

    public static int Main()
    {
        string temporary = Path.Combine(Path.GetTempPath(), "dotcc-initdb-contract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            using var owner = new Runtime.RuntimeContext();
            using var binding = owner.Enter();
            Runtime.errno = 123;
            Check(Runtime.getpagesize() == Environment.SystemPageSize && Runtime.errno == 123, "Host page size/errno");
            using (var ioOwner = new Fixture.Libc.RuntimeContext())
            using (ioOwner.Enter())
            {
                byte[] path = Encoding.UTF8.GetBytes(Path.Combine(temporary, "positioned-file") + "\0");
                fixed (byte* name = path)
                    Check(Fixture.fixture_positioned_io(name) == 0, "Actual emitted positioned scalar/vector I/O and Linux filesystem synchronization");
                File.Delete(Path.Combine(temporary, "positioned-file"));
            }
            fixed (byte* command = "exit 0\0"u8) fixed (byte* mode = "r\0"u8)
            {
                Check(Fork.pgl_popen(command, mode) == null && Runtime.errno == Runtime.ENOTSUP, "Actual fork must reject unconfigured popen");
                Check(Fork.pgl_pclose(null) == -1 && Runtime.errno == Runtime.EBADF, "Actual fork pclose must fail honestly");
            }
            string executable = "/private/translated postgres";
            var parsed = InitdbCommand.Parse('"' + executable + "\" --boot -c name='a'\\''b' -c empty=''", executable);
            Check(parsed.Arguments[^3] == "name=a'b" && parsed.Arguments[^1] == "empty=", "POSIX argument quote preservation");
            parsed = InitdbCommand.Parse('"' + executable + "\" --boot -c 'name=a\nb'", executable);
            Check(parsed.Arguments[^1] == "name=a\nb", "Quoted newlines remain argument bytes");
            foreach (string command in new[] { "\"/other/postgres\" -V", '"' + executable + "\" -V; touch x",
                '"' + executable + "\" --boot $(false)", '"' + executable + "\" --boot \"$HOME\"",
                '"' + executable + "\" --boot | cat", '"' + executable + "\" --single template1 > /tmp/output",
                '"' + executable + "\" --check < /dev/null > /dev/null", '"' + executable + "\" --boot 'unterminated",
                '"' + executable + "\" --boot\nnative-other" })
                Reject(command, executable);

            int calls = 0, status = 7;
            byte[]? received = null;
            active = new(executable, temporary, new Files(), request =>
            {
                calls++;
                int result = InvokeFixture(request, status);
                received = File.ReadAllBytes(request.OutputPath);
                Check(request.Arguments[0] == executable, "Exact argv[0] retained");
                return result;
            });
            using (active)
            {
                Fork.pgl_set_system_fn(&SystemCallback);
                Fork.pgl_set_popen_fn(&OpenCallback);
                Fork.pgl_set_pclose_fn(&CloseCallback);
                byte[] check = Encoding.UTF8.GetBytes('"' + executable + "\" --check -c max_connections=100 < \"/dev/null\" > \"/dev/null\" 2>&1\0");
                fixed (byte* command = check)
                    Check(Fork.pgl_system(command) == (7 << 8), "Actual fork system forwards the encoded failed backend exit");
                Check(calls == 1, "--check was invoked exactly once");
                byte[] boot = Encoding.UTF8.GetBytes('"' + executable + "\" --boot -F -c log_checkpoints=false\0");
                Runtime.FILE* stream;
                fixed (byte* command = boot) fixed (byte* mode = "w\0"u8) stream = Fork.pgl_popen(command, mode);
                Check(stream != null && calls == 1, "Bootstrap execution waits until EOF");
                byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("BKI and SQL bytes: Ž猫\0\n", 12000)));
                fixed (byte* data = bytes)
                    Check(Runtime.fwrite(data, 1, bytes.Length, stream) == bytes.Length, "Real frontend FILE preserves all bytes");
                Check(Fork.pgl_pclose(stream) == (7 << 8), "pclose returns wait status from actual translated I/O fixture");
                Check(calls == 2 && received!.SequenceEqual(bytes), "Exact UTF-8/NUL/large input reached translated code");
                Check(Fork.pgl_pclose(stream) == -1 && Runtime.errno == Runtime.EBADF && calls == 2, "A retired pipe cannot execute twice");
                status = 0;
                byte[] version = Encoding.UTF8.GetBytes('"' + executable + "\" -V\0");
                fixed (byte* command = version) fixed (byte* mode = "r\0"u8) stream = Fork.pgl_popen(command, mode);
                Check(stream != null && calls == 3, "Version execution precedes the read pipe");
                byte* line = stackalloc byte[100];
                Check(Runtime.fgets(line, 100, stream) != null && Marshal.PtrToStringUTF8((nint)line) == "translated I/O fixture\n", "Real translated output reaches frontend fgets");
                Check(Fork.pgl_pclose(stream) == 0, "Successful real invocation exits zero");
                Check(active.Open('"' + executable + "\" --boot", "r") == 0 && Runtime.errno == Runtime.ENOTSUP, "Wrong pipe direction rejected");
                Check(active.System("native-postgres --check") == -1 && Runtime.errno == Runtime.ENOTSUP, "Unknown executable rejected");
                Fork.pgl_set_system_fn(null); Fork.pgl_set_popen_fn(null); Fork.pgl_set_pclose_fn(null);
            }
            active = null;
            foreach (bool closeFailure in new[] { false, true })
            {
                var faults = new FaultFiles(!closeFailure, closeFailure);
                using (var transport = new InitializationProcessTransport(executable, temporary, faults,
                    _ => throw new InvalidOperationException("Partial input must never enter a backend.")))
                {
                    nint stream = transport.Open('"' + executable + "\" --boot", "w");
                    Check(stream != 0, "Fault-path test uses a real frontend FILE");
                    Check(transport.Close(stream) == -1 && transport.LastError is IOException && Runtime.errno == Runtime.EIO,
                        "Host flush/close exception is contained without running a partial input");
                }
                Check(faults.CloseCalls == (closeFailure ? 2 : 1), "Flush exceptions still close; close exceptions retry during drain");
            }
            Check(!Directory.EnumerateFileSystemEntries(temporary).Any(), "Transport removes its owned spool directory");
            Console.WriteLine($"PASS {assertions} actual-fork/runtime/translated-I/O transport assertions (no database execution claimed)");
            return 0;
        }
        finally
        {
            Fork.pgl_set_system_fn(null); Fork.pgl_set_popen_fn(null); Fork.pgl_set_pclose_fn(null);
            Directory.Delete(temporary, true);
        }
    }
}

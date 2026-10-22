using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using PG = Managed.PGlite.Probes.PosixHost;

internal static unsafe class Program
{
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    public static int Main(string[] args)
    {
        try
        {
            Require(args.Length == 2 || args.Length == 3, "Expected NATIVE_JSON MANAGED_JSON [files]");
            if (args.Length == 3)
            {
                fixed (byte* database = "passwd\0"u8)
                fixed (byte* service = "files\0"u8)
                    Require(args[2] == "files" && ConfigureNss(database, service) == 0, "test-process-only real files NSS selection");
            }
            using var expected = JsonDocument.Parse(File.ReadAllText(args[0]));
            using var owner = new PG.Libc.RuntimeContext(); using var binding = owner.Enter();
            ulong[] native = expected.RootElement.GetProperty("layout").EnumerateArray().Select(x => x.GetUInt64()).ToArray();
            ulong[] actual = Enumerable.Range(0, 24).Select(i => PG.dph_layout((uint)i)).ToArray();
            Require(actual.SequenceEqual(native), "24 actual native/emitted header sizes, alignments and offsets");
            PG.passwd passwd = default; PG.termios terminal = default;
            Require((ulong)sizeof(PG.passwd) == native[0] && (ulong)sizeof(PG.termios) == native[9], "actual CLR record sizes");
            ulong[] passwdOffsets = [(ulong)((byte*)&passwd.pw_name - (byte*)&passwd), (ulong)((byte*)&passwd.pw_passwd - (byte*)&passwd),
                (ulong)((byte*)&passwd.pw_uid - (byte*)&passwd), (ulong)((byte*)&passwd.pw_gid - (byte*)&passwd),
                (ulong)((byte*)&passwd.pw_gecos - (byte*)&passwd), (ulong)((byte*)&passwd.pw_dir - (byte*)&passwd), (ulong)((byte*)&passwd.pw_shell - (byte*)&passwd)];
            Require(passwdOffsets.SequenceEqual(native.Skip(2).Take(7)), "actual CLR passwd field addresses");
            ulong[] termiosOffsets = [(ulong)((byte*)&terminal.c_iflag - (byte*)&terminal), (ulong)((byte*)&terminal.c_oflag - (byte*)&terminal),
                (ulong)((byte*)&terminal.c_cflag - (byte*)&terminal), (ulong)((byte*)&terminal.c_lflag - (byte*)&terminal),
                (ulong)((byte*)&terminal.c_line - (byte*)&terminal), (ulong)(terminal.c_cc - (byte*)&terminal),
                (ulong)((byte*)&terminal.c_ispeed - (byte*)&terminal), (ulong)((byte*)&terminal.c_ospeed - (byte*)&terminal)];
            Require(termiosOffsets.SequenceEqual(native.Skip(11).Take(8)), "actual CLR termios field addresses: " + string.Join(",", termiosOffsets));
            ulong digest = 0;
            int status = PG.dph_passwd(&digest);
            Require(status == 0, "actual emitted NSS assertion at posix_host.c:" + status);
            Require(digest == expected.RootElement.GetProperty("nss_digest").GetUInt64(), "native/emitted NSS uid/gid/name/home/shell digest");
            int missing = -1; status = PG.dph_missing(&missing);
            Require(status == 0 && missing == expected.RootElement.GetProperty("missing_error").GetInt32(), "native/emitted no-entry NULL and exact numeric error");
            if (args.Length == 3) Require(missing == 0, "real files NSS returns successful no-entry 0,NULL");
            fixed (byte* name = Encoding.UTF8.GetBytes("/dotcc-posixhost-emitted-" + Guid.NewGuid().ToString("N") + "\0"))
            { status = PG.dph_shared(name); Require(status == 0, "actual emitted shared memory assertion at posix_host.c:" + status); }
            status = PG.dph_terminal_errors(); Require(status == 0, "actual emitted terminal errors at posix_host.c:" + status);
            bool pty = Terminal();
            Require(pty == expected.RootElement.GetProperty("pty_available").GetBoolean(), "native/emitted PTY availability");
            using (var output = File.Create(args[1]))
            using (var writer = new Utf8JsonWriter(output))
            {
                writer.WriteStartObject(); writer.WriteStartArray("layout");
                foreach (ulong value in actual) writer.WriteNumberValue(value);
                writer.WriteEndArray(); writer.WriteNumber("nss_digest", digest); writer.WriteNumber("missing_error", missing);
                writer.WriteBoolean("pty_available", pty); writer.WriteBoolean("shared", true); writer.WriteBoolean("errors", true);
                writer.WriteBoolean("actual_clr_storage", true); writer.WriteString("source", "actual emitted posix_host.c");
                writer.WriteEndObject();
            }
            Console.WriteLine("PASS 24 native/emitted layouts, actual CLR storage, real shared memory/mappings, NSS buffers and terminal controls; PTY=" + pty);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static bool Terminal()
    {
        int master = OpenPty(2 | 0x100 | 0x80000), slave = -1;
        if (master < 0) { Console.WriteLine("PTY unavailable: errno " + Marshal.GetLastPInvokeError()); return false; }
        try
        {
            Require(Grant(master) == 0 && Unlock(master) == 0, "real PTY grant/unlock");
            byte* path = stackalloc byte[256]; Require(Name(master, path, 256) == 0, "real PTY slave name");
            slave = PG.Libc.open(path, 2 | 0x100); Require(slave >= 3, "owner opens PTY slave with O_NOCTTY");
            int status = PG.dph_terminal(slave);
            Require(status == 0, "actual emitted PTY roundtrip/restoration at posix_host.c:" + status);
            return true;
        }
        finally { if (slave >= 0) PG.Libc.close(slave); Close(master); }
    }
    [DllImport("libc", EntryPoint = "posix_openpt", SetLastError = true)] private static extern int OpenPty(int flags);
    [DllImport("libc", EntryPoint = "grantpt")] private static extern int Grant(int fd);
    [DllImport("libc", EntryPoint = "unlockpt")] private static extern int Unlock(int fd);
    [DllImport("libc", EntryPoint = "ptsname_r")] private static extern int Name(int fd, byte* buffer, ulong size);
    [DllImport("libc", EntryPoint = "close")] private static extern int Close(int fd);
    [DllImport("libc", EntryPoint = "__nss_configure_lookup")] private static extern int ConfigureNss(byte* database, byte* service);
}

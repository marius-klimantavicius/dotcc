using System.Text.Json;
using ABI = Managed.PGlite.Probes.Boundary;

internal static unsafe class Program
{
    private static int cleanupOrder;
    private static void FirstCleanup() { cleanupOrder = cleanupOrder * 10 + 1; }
    private static void SecondCleanup() { cleanupOrder = cleanupOrder * 10 + 2; }
    private static long Callback(void* buffer, ulong length)
    {
        // Exact managed function pointer shape observed from emitted C. This
        // catches failures so an exception cannot escape into translated C.
        try
        {
            byte* bytes = (byte*)buffer;
            if (bytes[0] != 0xce || bytes[1] != 0xbb || bytes[2] != 0) return -2;
            GC.Collect(); // The fixed caller's buffer must stay valid.
            bytes[3] = 42;
            return length == 0 ? -1 : checked((long)length);
        }
        catch { return -3; }
    }

    private static void Require(bool value, string operation)
    {
        if (!value) throw new InvalidOperationException("Boundary assertion: " + operation);
    }

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2) throw new ArgumentException("Expected NATIVE_REPORT MANAGED_REPORT.");
            var layout = ABI.dpg_fixture_get_layout();
            using var native = JsonDocument.Parse(File.ReadAllText(args[0]));
            var fields = new Dictionary<string, uint>
            {
                ["pointer"] = layout.pointer_bytes, ["long"] = layout.long_bytes,
                ["size_t"] = layout.size_bytes, ["ssize_t"] = layout.ssize_bytes,
                ["time_t"] = layout.time_bytes, ["callback"] = layout.callback_bytes,
                ["sample"] = layout.sample_bytes, ["payload_offset"] = layout.sample_payload_offset,
            };
            foreach (var field in fields)
                Require(native.RootElement.GetProperty(field.Key).GetUInt32() == field.Value, "native/emitted " + field.Key);
            Require(layout.pointer_bytes == 8 && layout.long_bytes == 8 && layout.size_bytes == 8 && layout.ssize_bytes == 8,
                "selected LP64 boundary profile");
            // Deliberate ABI distinction: DotCC jump buffers are identity slots,
            // not the host libc register-save record. Never compare their bytes.
            Require(layout.jump_bytes == 8, "DotCC jump identity slot");
            Require(ABI.dpg_fixture_nested_jump() == 1, "real translated nested nonlocal jumps");
            Require(ABI.dpg_fixture_memory() == 1, "translated allocation and byte preservation");
            byte[] buffer = [0xce, 0xbb, 0, 0];
            fixed (byte* bytes = buffer)
            {
                const ulong wide = (1UL << 33) + 9;
                Require(ABI.dpg_fixture_invoke(&Callback, bytes, wide) == (long)wide && bytes[3] == 42,
                    "actual managed/emitted C callback, 64-bit length, pinned bytes through GC");
                Require(ABI.dpg_fixture_invoke(&Callback, bytes, 0) == -1, "signed callback error return");
                ABI.pgl_set_rw_cbs(&Callback, &Callback);
                try
                {
                    Require(ABI.pgl_recv(1, bytes, wide, 0) == (long)wide && bytes[3] == 42,
                        "actual fork pgl_recv body and registered managed callback");
                    Require(ABI.pgl_send(1, bytes, 0, 0) == -1, "actual fork pgl_send signed callback error");
                }
                finally { ABI.pgl_set_rw_cbs(null, null); }
            }
            Require(ABI.pgl_atexit(&FirstCleanup) == 0 && ABI.pgl_atexit(&SecondCleanup) == 0,
                "actual fork cleanup registration");
            ABI.pgl_run_atexit_funcs();
            Require(cleanupOrder == 21, "actual fork cleanup order");
            ABI.pgl_run_atexit_funcs();
            Require(cleanupOrder == 21, "actual fork cleanup drains only once");
            bool terminal = false;
            bool timersSupported;
            bool signalQuerySupported = ABI.dpg_fixture_signal_query() == 0;
            using (var owner = new ABI.Libc.RuntimeContext())
            {
                using (owner.Enter())
                {
                    timersSupported = ABI.dpg_fixture_clear_timer() == 0;
                    try { ABI.dpg_fixture_exit(99); throw new InvalidOperationException("exit unexpectedly returned"); }
                    catch (ABI.Libc.RuntimeTerminationException exit) when (ReferenceEquals(exit.Owner, owner))
                    { terminal = owner.Termination is { Status: 99 }; }
                }
                // Do not resume C after the owner has become terminal.
            }
            Require(terminal, "synthetic yield records a terminal request in the current runtime");
            // Serialize without reflection so the same harness can be AOT-rooted.
            using var output = File.Create(args[1]);
            using var writer = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject(); writer.WriteString("scope", "minimal ABI plus exact fork callback/cleanup slices; no PGlite SQL execution");
            foreach (var field in fields) writer.WriteNumber(field.Key, field.Value);
            writer.WriteNumber("jmp_buf", layout.jump_bytes);
            writer.WriteNumber("native_jmp_buf", native.RootElement.GetProperty("jmp_buf").GetUInt32());
            writer.WriteBoolean("nested_jump", true); writer.WriteBoolean("memory", true);
            writer.WriteBoolean("callback_roundtrip", true); writer.WriteBoolean("fork_callbacks", true);
            writer.WriteBoolean("fork_cleanup", true); writer.WriteBoolean("yield_records_terminal", terminal);
            writer.WriteBoolean("timers_supported", timersSupported);
            writer.WriteBoolean("signal_query_supported", signalQuerySupported); writer.WriteEndObject(); writer.Flush();
            Console.WriteLine("PASS LP64 ABI, nested jumps, allocation, actual fork callback/cleanup slices; synthetic exit and timer blockers characterized; SQL workflow unrun");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}

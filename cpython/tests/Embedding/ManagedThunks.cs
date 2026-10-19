using System.Collections.Concurrent;
using System.Text;
using Managed.CPython;
using Probe = Managed.CPython.Probes.CallbackProbe;

unsafe class Program
{
    private static int passed;
    private static void Check(bool condition)
    { if (!condition) throw new InvalidOperationException("Managed/C thunk probe assertion failed"); passed++; }

    static void Main()
    {
        delegate*<long, long, long*, byte*, ulong, int> thunk = &CallbackThunks.Int64;
        Span<byte> buffer = stackalloc byte[1024];
        long value = -1;
        long token = CallbackRoots.Add(argument => checked(argument * 2));
        fixed (byte* error = buffer)
        {
            Check(Probe.dcp_probe_call(thunk, token, 21, &value, error, 1024) == 0 && value == 42 && error[0] == 0);
            value = 123;
            Check(Probe.dcp_probe_call(thunk, token, long.MaxValue, &value, error, 1024) == -1 && value == 123);
            Check(Read(buffer).StartsWith("OverflowException:", StringComparison.Ordinal));
        }
        CallbackRoots.RemoveAfterDrain(token);
        fixed (byte* error = buffer)
        {
            Check(Probe.dcp_probe_call(thunk, token, 1, &value, error, 1024) == -1);
            Check(Read(buffer) == "Expired managed callback.");
        }

        token = CallbackRoots.Add(_ => throw new InvalidOperationException("λ🐍\0" + new string('λ', 2000)));
        buffer.Fill(0xaa);
        fixed (byte* error = buffer)
        {
            Check(Probe.dcp_probe_call(thunk, token, 1, &value, error, 41) == -1);
            string message = Read(buffer[..41]); // Strict UTF-8, complete prefix.
            Check(message.StartsWith("InvalidOperationException: λ🐍\\0", StringComparison.Ordinal));
            Check(buffer[41] == 0xaa); // No write past advertised C capacity.
        }
        CallbackRoots.RemoveAfterDrain(token);

        token = CallbackRoots.Add(_ => throw new InvalidOperationException("\ud800"));
        fixed (byte* error = buffer)
        {
            Check(Probe.dcp_probe_call(thunk, token, 1, &value, error, 1024) == -1);
            Check(Read(buffer) == "Managed callback failed while formatting its error.");
        }
        CallbackRoots.RemoveAfterDrain(token);

        token = CallbackRoots.Add(_ => throw new ThrowingMessage());
        fixed (byte* error = buffer)
        {
            Check(Probe.dcp_probe_call(thunk, token, 1, &value, error, 1024) == -1);
            Check(Read(buffer) == "Managed callback failed before returning a result.");
            Check(Probe.dcp_probe_call(thunk, token, 1, &value, error, 1) == -1 && error[0] == 0);
            Check(Probe.dcp_probe_call(thunk, token, 1, &value, null, 0) == -1);
        }
        CallbackRoots.RemoveAfterDrain(token);

        int owner = Environment.CurrentManagedThreadId;
        var threads = new ConcurrentDictionary<int, byte>();
        using var arrived = new CountdownEvent(2);
        token = CallbackRoots.Add(argument =>
        {
            threads.TryAdd(Environment.CurrentManagedThreadId, 0);
            arrived.Signal();
            if (!arrived.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Both real workers must enter managed code.");
            GC.Collect(); // Registry retains callback/context while C workers run.
            return argument * 2;
        });
        Check(Probe.dcp_probe_workers(thunk, token) == 86);
        Check(threads.Count == 2 && !threads.ContainsKey(owner));
        CallbackRoots.RemoveAfterDrain(token);
        Console.WriteLine($"PASS {passed} actual managed/emitted-C thunk assertions, including two real workers; no CPython interpreter execution claimed");
    }

    private static string Read(ReadOnlySpan<byte> data)
    {
        int end = data.IndexOf((byte)0);
        Check(end >= 0);
        return PythonText.DecodeUtf8(data[..end]);
    }

    private sealed class ThrowingMessage : Exception
    { public override string Message => throw new InvalidOperationException("Message getter failure"); }
}

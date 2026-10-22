using System.Text;
using Managed.PGlite.Initialization;
using Backend = Managed.PGlite.Translated.PGlite;

string temporary = Path.Combine(Path.GetTempPath(), "dotcc-backend-adapter-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(Path.Combine(temporary, "lib/postgresql"));
File.WriteAllText(Path.Combine(temporary, "lib/postgresql/plpgsql"), "explicit byte-fixture catalog descriptor; not PostgreSQL");
try
{
    int assertions = 0;
    var backend = new InitializationBackend(temporary);
    byte[] bytes = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("Ž猫 byte input\0\n", 2000)));
    string input = Path.Combine(temporary, "input"), output = Path.Combine(temporary, "output"), cleanup = Path.Combine(temporary, "cleanup");
    File.WriteAllBytes(input, bytes);
    string? localeBefore = Environment.GetEnvironmentVariable("LC_ALL"), dataBefore = Environment.GetEnvironmentVariable("PGDATA");
    foreach (var (status, merge) in new[] { (7, false), (0, true), (3, false) })
    {
        var request = new BootstrapInvocation(BootstrapOperation.Bootstrap,
            ["/fixture/backend", "--byte-fixture", cleanup, Path.Combine(temporary, "lib/postgresql/plpgsql"), status.ToString()],
            input, output, merge);
        if (backend.Invoke(request) != status) throw new InvalidOperationException("Lost actual pgl_exit status.");
        assertions++;
        byte[] expected = merge ? bytes.Concat("fixture stderr\n"u8.ToArray()).ToArray() : bytes;
        if (!File.ReadAllBytes(output).SequenceEqual(expected)) throw new InvalidOperationException("Changed actual stdin/stdout bytes.");
        assertions++;
        if (!merge && File.ReadAllText(request.ErrorPath) != "fixture stderr\n") throw new InvalidOperationException("Changed stderr redirection.");
        assertions++;
        if (File.ReadAllText(cleanup) != "actual fork atexit drained\n") throw new InvalidOperationException("Lost original fork cleanup or retained argv.");
        assertions++;
        if (Backend.Libc.RuntimeContext.Current != null || InitializationBackend.TryPollSignals()) throw new InvalidOperationException("Leaked backend binding/signal owner.");
        assertions++;
        if (Environment.GetEnvironmentVariable("LC_ALL") != localeBefore || Environment.GetEnvironmentVariable("PGDATA") != dataBefore)
            throw new InvalidOperationException("Child backend environment escaped into initdb parent.");
        assertions++;
        File.Delete(output); File.Delete(request.ErrorPath); File.Delete(cleanup);
    }
    Console.WriteLine($"PASS {assertions} backend adapter actual-pglitec byte/exit/cleanup/fresh-state assertions; PostgreSQL initialization unrun");
    return 0;
}
finally { Directory.Delete(temporary, true); }

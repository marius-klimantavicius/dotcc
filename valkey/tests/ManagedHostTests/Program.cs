using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Managed.Valkey;

internal sealed record Options(string Directory, string Report, int Port, int PeerPort, bool Serve, bool AppendOnly, string Suite, int NativePort, string? Exchange, long Expiry)
{
    internal static Options Parse(string[] args)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dotcc-valkey-integration-" + Guid.NewGuid().ToString("N"));
        string? report = null;
        int port = 0, peer = 0, native = 0;
        bool serve = false, aof = false;
        string suite = "all";
        string? exchange = null;
        long expiry = 0;
        for (int i = 0; i < args.Length; i++)
        {
            string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Missing option value.");
            switch (args[i])
            {
                case "--directory": directory = Path.GetFullPath(Value()); break;
                case "--report": report = Path.GetFullPath(Value()); break;
                case "--port": port = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--peer-port": peer = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--native-port": native = int.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--suite": suite = Value(); break;
                case "--exchange": exchange = Value(); break;
                case "--expiry": expiry = long.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--serve": serve = true; break;
                case "--appendonly": aof = true; break;
                default: throw new ArgumentException("Options: --directory PATH --report PATH --port N --peer-port N --suite all|rdb|aof --serve --appendonly --native-port N");
            }
        }
        if (suite is not ("all" or "rdb" or "aof")) throw new ArgumentException("Unknown suite.");
        if (exchange is not (null or "export" or "import")) throw new ArgumentException("Exchange must be export or import.");
        if (exchange is not null && (serve || suite == "all" || expiry <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()))
            throw new ArgumentException("Exchange requires --suite rdb|aof and a future absolute --expiry in milliseconds.");
        // Port zero disables Valkey TCP. Reserve two actual free loopback ports,
        // then release immediately before construction; external races fail visibly.
        using var first = new TcpListener(IPAddress.Loopback, port);
        using var second = new TcpListener(IPAddress.Loopback, peer);
        first.Start(); second.Start();
        port = ((IPEndPoint)first.LocalEndpoint).Port;
        peer = ((IPEndPoint)second.LocalEndpoint).Port;
        if (port == peer || native != 0 && (native == port || native == peer)) throw new ArgumentException("Ports must differ.");
        return new(directory, report ?? Path.Combine(directory, "results.json"), port, peer, serve, aof, suite, native, exchange, expiry);
    }
}

internal static class Program
{
    private sealed record Result(string Name, string Status, double Milliseconds, string? Detail);
    private static readonly List<Result> results = [];
    private static readonly List<ValkeyServer> servers = [];
    private static readonly byte[] binary = Enumerable.Range(0, 262144).Select(i => (byte)(i * 17)).ToArray();
    private const string Library = "#!lua name=managed_integration\nredis.register_function('managed_read', function(keys,args) return redis.call('GET', keys[1]) end)";
    private static CancellationToken cancellation;
    private static string originalDirectory = "";
    private static string? caseEvidence;

    internal static async Task<int> Main(string[] args)
    {
        Options options = Options.Parse(args);
        if (!options.Serve && options.Exchange != "import" && Directory.Exists(options.Directory) && Directory.EnumerateFileSystemEntries(options.Directory).Any())
            throw new ArgumentException("Integration checks require a new or empty data directory.");
        Directory.CreateDirectory(options.Directory);
        originalDirectory = Directory.GetCurrentDirectory();
        using var timeout = new CancellationTokenSource(options.Serve ? Timeout.InfiniteTimeSpan : TimeSpan.FromMinutes(4));
        cancellation = timeout.Token;
        int status = 0;
        try
        {
            if (options.Serve) await Serve(options);
            else if (options.Exchange is not null) await Exchange(options);
            else
            {
                if (options.Suite is "all" or "rdb") await OwnersAndRdb(options);
                if (options.Suite is "all" or "aof") { await Aof(options); await EverySecond(options); }
                if (options.NativePort != 0) await CompareNative(options);
            }
        }
        catch (Exception error)
        {
            status = 1;
            Console.Error.WriteLine(error);
            if (results.Count == 0 || results[^1].Status != "failed") results.Add(new("unhandled", "failed", 0, error.ToString()));
        }
        finally
        {
            foreach (var server in servers.AsEnumerable().Reverse())
            {
                try
                {
                    await server.StopAsync(ValkeyShutdownMode.NoSave).WaitAsync(TimeSpan.FromSeconds(20));
                    Check.True(!server.IsQuarantined && !server.IsRunning, "Owner cleanup did not finish.");
                }
                catch (Exception error) { status = 1; results.Add(new("cleanup:" + server.InstanceId, "failed", 0, error.ToString())); }
            }
            if (Directory.GetCurrentDirectory() != originalDirectory)
            { status = 1; results.Add(new("process-cwd", "failed", 0, "Managed owner changed process current directory.")); }
            WriteReport(options, status);
        }
        return status;
    }
    private static async Task Case(string name, Func<Task> body)
    {
        var watch = Stopwatch.StartNew();
        caseEvidence = null;
        try { await body(); results.Add(new(name, "passed", watch.Elapsed.TotalMilliseconds, caseEvidence)); Console.WriteLine("PASS " + name); }
        catch (Exception error) { results.Add(new(name, "failed", watch.Elapsed.TotalMilliseconds, error.ToString())); throw; }
    }
    private static ValkeyServer Start(ValkeyOptions options)
    {
        var server = new ValkeyServer(options); servers.Add(server); return server;
    }
    private static async Task Ready(params ValkeyServer[] owners)
    {
        await Task.WhenAll(owners.Select(owner => owner.Ready)).WaitAsync(cancellation);
        foreach (var owner in owners) Check.True(owner.IsRunning && !owner.IsQuarantined, "Readiness requires a live, usable owner.");
    }
    private static async Task Stopped(ValkeyServer owner, ValkeyShutdownMode mode)
    {
        await owner.StopAsync(mode).WaitAsync(cancellation);
        await owner.Completion.WaitAsync(cancellation);
        Check.True(!owner.IsRunning && !owner.IsQuarantined, "Stop did not release owner resources.");
        await owner.DisposeAsync();
    }
    private static async Task OwnersAndRdb(Options options)
    {
        var firstOptions = new ValkeyOptions { DataDirectory = Path.Combine(options.Directory, "rdb-first"), Port = options.Port, Password = "owner-one", DisposeMode = ValkeyShutdownMode.NoSave };
        var peerOptions = firstOptions with { DataDirectory = Path.Combine(options.Directory, "rdb-peer"), Port = options.PeerPort, Password = "owner-two" };
        ValkeyServer first = Start(firstOptions), peer = Start(peerOptions);
        await Case("owners:start-and-readiness", () => Ready(first, peer));
        Check.True(first.InstanceId != peer.InstanceId, "Owners have identical identities.");
        await using var client = await RespConnection.Connect(await first.Endpoint, cancellation, firstOptions.Password);
        await using var other = await RespConnection.Connect(await peer.Endpoint, cancellation, peerOptions.Password);
        await Case("owners:occupied-port-startup-failure-and-cleanup", async () =>
        {
            // This owner intentionally faults, so it is not registered with the
            // normal-success cleanup loop. Its real listener initialization runs.
            var failed = new ValkeyServer(peerOptions with { DataDirectory = Path.Combine(options.Directory, "failed-startup") });
            Task<ValkeyPersistenceStatus> queuedSnapshot = failed.GetPersistenceStatusAsync(cancellation);
            try
            {
                async Task<ValkeyException> Fails(Task task, string operation)
                {
                    try { await task.WaitAsync(TimeSpan.FromSeconds(20), cancellation); }
                    catch (ValkeyException error) when (error is not ValkeyCleanupException) { return error; }
                    throw new InvalidOperationException(operation + " unexpectedly succeeded for an occupied-port owner.");
                }
                ValkeyException startup = await Fails(failed.Ready, "Ready");
                await Fails(failed.Completion, "Completion");
                await Fails(queuedSnapshot, "Startup persistence request");
                Check.True(!failed.IsRunning && !failed.IsQuarantined, "Failed startup did not release the partially initialized owner.");
                Check.Equal(await other.Command("PING"), "PONG");
                await Fails(failed.StopAsync(ValkeyShutdownMode.NoSave), "StopAsync");
                await Fails(failed.DisposeAsync().AsTask(), "DisposeAsync");
                Check.Equal(await client.Command("PING"), "PONG");
                RecordEvidence("Ready, Completion, queued snapshot, Stop and Dispose faulted; cleanup completed without quarantine: " + startup.Message);
            }
            finally
            {
                try { await failed.StopAsync(ValkeyShutdownMode.NoSave).WaitAsync(TimeSpan.FromSeconds(20)); }
                catch (ValkeyException) when (!failed.IsQuarantined) { }
            }
        });
        await Case("owners:auth-and-data-isolation", async () =>
        {
            await using var anonymous = await RespConnection.Connect(await first.Endpoint, cancellation);
            Check.Error(await anonymous.Command("GET", "owner"), "NOAUTH");
            Check.Error(await anonymous.Command("AUTH", peerOptions.Password!), "WRONGPASS");
            Check.Equal(await anonymous.Command("AUTH", firstOptions.Password!), "OK");
            Check.Equal(await client.Command("SET", "owner", "first"), "OK");
            Check.Equal(await other.Command("SET", "owner", "peer"), "OK");
            Check.Equal(await client.Command("GET", "owner"), "first");
            Check.Equal(await other.Command("GET", "owner"), "peer");
        });
        await Case("resp:partial-binary-pipeline-resp3", async () =>
        {
            Check.Equal(await client.Fragmented(["SET"u8.ToArray(), "binary"u8.ToArray(), binary], 257), "OK");
            Check.Binary(await client.Command("GET", "binary"), binary);
            var pipelined = await client.Pipeline(["SET", "pipeline", "40"], ["INCR", "pipeline"], ["GET", "pipeline"]);
            Check.Equal(pipelined[0], "OK"); Check.Equal(pipelined[1], 41L); Check.Equal(pipelined[2], "41");
            Check.True(await client.Command("HELLO", "3") is RespMap, "HELLO 3 must return a RESP3 map.");
            Check.Equal(await client.Command("GET", "missing"), null);
            Check.True(await client.Command("HELLO", "2") is object?[], "HELLO 2 must return a RESP2 array.");
        });
        await Case("commands:admitted-family-transitions", async () =>
            await CommandCoverage.Run(client, await first.Endpoint, firstOptions.Password!, cancellation));
        await Case("commands:collections-transactions-expiry", async () =>
        {
            Check.Equal(await client.Command("RPUSH", "list", "one", "two"), 2L);
            Check.Equal(await client.Command("HSET", "hash", "field", "value"), 1L);
            Check.Equal(await client.Command("MULTI"), "OK");
            Check.Equal(await client.Command("SET", "transaction", "41"), "QUEUED");
            Check.Equal(await client.Command("INCR", "transaction"), "QUEUED");
            var transaction = await client.Command("EXEC") as object?[] ?? throw new InvalidOperationException("Missing EXEC array.");
            Check.True(transaction.Length == 2, "Incorrect EXEC count."); Check.Equal(transaction[0], "OK"); Check.Equal(transaction[1], 42L);
            Check.Equal(await client.Command("SET", "ttl", "kept", "PX", "3600000"), "OK");
        });
        await Case("lua:callbacks-cache-functions-and-owner-isolation", async () =>
        {
            const string script = "return redis.call('GET', KEYS[1])";
            string sha = Check.Text(await client.Command("SCRIPT", "LOAD", script));
            Check.Equal(await client.Command("EVALSHA", sha, "1", "owner"), "first");
            Check.Error(await other.Command("EVALSHA", sha, "1", "owner"), "NOSCRIPT");
            Check.Equal(await client.Command("EVAL", "return cjson.decode('{\"value\":42}').value + bit.band(7,3)", "0"), 45L);
            Check.Error(await client.Command("EVAL", "error('protected-script-error')", "0"), "protected-script-error");
            Check.Equal(await client.Command("PING"), "PONG");
            Check.Equal(await client.Command("FUNCTION", "LOAD", Library), "managed_integration");
            Check.Equal(await client.Command("FCALL", "managed_read", "1", "owner"), "first");
            Check.Error(await other.Command("FCALL", "managed_read", "1", "owner"), "not found");
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            Check.Equal(await client.Command("EVALSHA", sha, "1", "owner"), "first");
            Check.Equal(await other.Command("EVAL", script, "1", "owner"), "peer");
        });
        await Case("profile:fork-commands-and-configurations-rejected", async () =>
        {
            string[][] denied = [["BGSAVE"], ["BGREWRITEAOF"], ["CONFIG", "SET", "save", "60 1"],
                ["CONFIG", "SET", "appendonly", "yes"], ["CONFIG", "SET", "auto-aof-rewrite-percentage", "100"],
                ["CONFIG", "SET", "io-threads", "2"], ["CONFIG", "SET", "daemonize", "yes"],
                ["REPLICAOF", "127.0.0.1", "1"], ["MODULE", "LOAD", "does-not-exist.so"], ["SCRIPT", "DEBUG", "yes"]];
            foreach (string[] command in denied) Check.Error(await client.Command(command));
            string priorMemory = Canonical(await client.Command("CONFIG", "GET", "maxmemory"));
            Check.Error(await client.Command("CONFIG", "SET", "maxmemory", "12345", "save", "60 1"));
            Check.Equal(Canonical(await client.Command("CONFIG", "GET", "maxmemory")), priorMemory);
            Check.Equal(await client.Command("MULTI"), "OK");
            Check.Error(await client.Command("BGSAVE"));
            Check.Error(await client.Command("EXEC"), "EXECABORT");
            Check.Error(await client.Command("EVAL", "return redis.pcall('BGREWRITEAOF')", "0"));
            Check.Equal(await client.Command("PING"), "PONG"); Check.Equal(await other.Command("GET", "owner"), "peer");
            Check.True(!Directory.EnumerateFiles(firstOptions.DataDirectory, "*.rdb").Any(), "Rejected background save created an RDB.");
        });
        await Case("rdb:failed-save-keeps-server-running-then-retry", async () =>
        {
            string blocked = Path.Combine(firstOptions.DataDirectory, firstOptions.RdbFileName);
            Directory.CreateDirectory(blocked);
            bool refused = false;
            try { await first.StopAsync(ValkeyShutdownMode.Save).WaitAsync(cancellation); }
            catch (ValkeyStopException) { refused = true; }
            Check.True(refused && first.IsRunning && !first.Completion.IsCompleted, "Failed SAVE must preserve a running owner.");
            Check.Equal(await client.Command("PING"), "PONG");
            Directory.Delete(blocked);
            await Stopped(first, ValkeyShutdownMode.Save);
            Check.True(File.Exists(Path.Combine(firstOptions.DataDirectory, firstOptions.RdbFileName)), "Foreground SAVE produced no RDB.");
            Check.Equal(await other.Command("PING"), "PONG");
        });
        await Case("rdb:fresh-owner-load-data-types-ttl-and-functions", async () =>
        {
            var restarted = Start(firstOptions); await Ready(restarted);
            await using var loaded = await RespConnection.Connect(await restarted.Endpoint, cancellation, firstOptions.Password);
            Check.Equal(await loaded.Command("GET", "owner"), "first");
            Check.Binary(await loaded.Command("GET", "binary"), binary);
            Check.Equal(await loaded.Command("LINDEX", "list", "1"), "two");
            Check.Equal(await loaded.Command("HGET", "hash", "field"), "value");
            Check.Equal(await loaded.Command("GET", "transaction"), "42");
            long ttl = (long)(await loaded.Command("PTTL", "ttl") ?? -1L);
            Check.True(ttl is > 0 and <= 3600000, "RDB lost absolute expiry.");
            Check.Equal(await loaded.Command("FCALL", "managed_read", "1", "owner"), "first");
            Check.Equal(await other.Command("GET", "owner"), "peer");
            await Stopped(restarted, ValkeyShutdownMode.NoSave);
        });
        await Case("bio:lazyfree-worker-completion-and-shutdown-drain", async () =>
        {
            var owner = Start(firstOptions with { DataDirectory = Path.Combine(options.Directory, "bio-lazyfree") });
            await Ready(owner);
            await using var connection = await RespConnection.Connect(await owner.Endpoint, cancellation, firstOptions.Password);
            var before = Info(await connection.Command("INFO", "memory"));
            long freedBefore = Number(before, "lazyfreed_objects");
            const string createHash = "for i=1,2048 do redis.call('HSET',KEYS[1],tostring(i),string.rep('x',80)) end; return redis.call('HLEN',KEYS[1])";
            Check.Equal(await connection.Command("EVAL", createHash, "1", "large-hash"), 2048L);
            Check.Equal(await connection.Command("UNLINK", "large-hash"), 1L);
            long completed = freedBefore, pending = -1;
            await Until(async () =>
            {
                var memory = Info(await connection.Command("INFO", "memory"));
                completed = Number(memory, "lazyfreed_objects");
                pending = Number(memory, "lazyfree_pending_objects");
                Check.Equal(await other.Command("GET", "owner"), "peer");
                return completed > freedBefore && pending == 0;
            }, "Lazyfree BIO worker did not report completed work.");
            RecordEvidence($"lazyfreed_objects {freedBefore}->{completed}; lazyfree_pending_objects={pending}");
            Check.Equal(await connection.Command("EVAL", createHash, "1", "shutdown-hash"), 2048L);
            Check.Equal(await connection.Command("UNLINK", "shutdown-hash"), 1L);
            // No pending-counter timing assumption: stop must drain queued work
            // or join the worker if it already finished before this continuation.
            await Stopped(owner, ValkeyShutdownMode.NoSave);
            Check.Equal(await other.Command("GET", "owner"), "peer");
        });
        await Case("lifecycle:resp-shutdown-preserves-peer", async () =>
        {
            var owner = Start(firstOptions with { DataDirectory = Path.Combine(options.Directory, "resp-shutdown") });
            await Ready(owner);
            await using var connection = await RespConnection.Connect(await owner.Endpoint, cancellation, firstOptions.Password);
            bool closed = false;
            try
            {
                object? reply = await connection.Command("SHUTDOWN", "NOSAVE");
                throw new InvalidOperationException($"SHUTDOWN should close the connection, received {reply}.");
            }
            catch (IOException) { closed = true; }
            Check.True(closed, "SHUTDOWN left its client connection open.");
            await owner.Completion.WaitAsync(cancellation);
            Check.True(!owner.IsRunning && !owner.IsQuarantined, "RESP SHUTDOWN did not cleanly finish the owner.");
            Check.Equal(await other.Command("GET", "owner"), "peer");
            await owner.DisposeAsync();
        });
        await Case("lifecycle:repeated-dispose-rebind-and-owner-cleanup", async () =>
        {
            var repeat = firstOptions with { DataDirectory = Path.Combine(options.Directory, "repeat") };
            for (int i = 0; i < 3; i++)
            {
                var owner = Start(repeat); await Ready(owner);
                await using var connection = await RespConnection.Connect(await owner.Endpoint, cancellation, repeat.Password);
                Check.Equal(await connection.Command("GET", "transient"), null);
                Check.Equal(await connection.Command("SET", "transient", "new"), "OK");
                using var cancelled = new CancellationTokenSource();
                cancelled.Cancel();
                bool cancelledBeforeQueue = false;
                try { await owner.StopAsync(ValkeyShutdownMode.NoSave, cancelled.Token); }
                catch (OperationCanceledException) { cancelledBeforeQueue = true; }
                Check.True(cancelledBeforeQueue && owner.IsRunning, "Pre-cancelled stop queued a shutdown.");
                Check.Equal(await connection.Command("PING"), "PONG");
                await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => owner.StopAsync(ValkeyShutdownMode.NoSave))).WaitAsync(cancellation);
                await owner.Completion.WaitAsync(cancellation); await owner.DisposeAsync();
                Check.True(!owner.IsRunning && !owner.IsQuarantined, "Repeated owner was retained.");
                Check.Equal(await other.Command("GET", "owner"), "peer");
            }
            await Stopped(peer, ValkeyShutdownMode.NoSave);
            Check.Equal(Directory.GetCurrentDirectory(), originalDirectory);
            foreach (string path in Directory.EnumerateFiles(options.Directory, "*", SearchOption.AllDirectories))
            { using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
        });
    }
    private static async Task Aof(Options options)
    {
        var settings = new ValkeyOptions { DataDirectory = Path.Combine(options.Directory, "aof"), Port = options.Port, AppendOnly = true, AppendFsync = ValkeyAppendFsync.Always, DisposeMode = ValkeyShutdownMode.NoSave };
        await Case("aof:fresh-manifest-append-and-existing-replay", async () =>
        {
            for (int cycle = 0; cycle < 3; cycle++)
            {
                var owner = Start(settings); await Ready(owner);
                await using var connection = await RespConnection.Connect(await owner.Endpoint, cancellation);
                if (cycle == 0)
                {
                    Check.Equal(await connection.Bytes("SET"u8.ToArray(), "binary"u8.ToArray(), binary), "OK");
                    Check.Equal(await connection.Command("FUNCTION", "LOAD", Library), "managed_integration");
                    Check.Equal(await connection.Command("SET", "owner", "aof"), "OK");
                    Check.Equal(await connection.Command("EVAL", "redis.call('SET','script-effect','persisted'); return 1", "0"), 1L);
                    Check.Equal(await connection.Command("MULTI"), "OK");
                    Check.Equal(await connection.Command("SET", "tx", "40"), "QUEUED");
                    Check.Equal(await connection.Command("INCR", "tx"), "QUEUED");
                    var transaction = await connection.Command("EXEC") as object?[] ?? throw new InvalidOperationException("Missing AOF EXEC array.");
                    Check.True(transaction.Length == 2, "Incorrect AOF EXEC count.");
                    Check.Equal(transaction[0], "OK"); Check.Equal(transaction[1], 41L);
                    Check.Equal(await connection.Command("SET", "ttl", "kept", "PX", "3600000"), "OK");
                }
                else
                {
                    Check.Binary(await connection.Command("GET", "binary"), binary);
                    Check.Equal(await connection.Command("GET", "counter"), cycle.ToString(CultureInfo.InvariantCulture));
                    Check.Equal(await connection.Command("FCALL", "managed_read", "1", "owner"), "aof");
                    Check.Equal(await connection.Command("GET", "script-effect"), "persisted");
                    Check.Equal(await connection.Command("GET", "tx"), "41");
                    long ttl = (long)(await connection.Command("PTTL", "ttl") ?? -1L);
                    Check.True(ttl is > 0 and <= 3600000, "AOF replay lost absolute expiry.");
                }
                Check.Equal(await connection.Command("INCR", "counter"), (long)cycle + 1);
                await Stopped(owner, ValkeyShutdownMode.NoSave);
            }
            string[] manifests = Directory.GetFiles(settings.DataDirectory, "*.manifest", SearchOption.AllDirectories);
            Check.True(manifests.Length == 1 && new FileInfo(manifests[0]).Length > 0, "Multipart AOF manifest missing.");
            Check.True(Directory.EnumerateFiles(settings.DataDirectory, "*.aof", SearchOption.AllDirectories).Any(path => new FileInfo(path).Length > 0), "AOF append files missing.");
        });
    }
    private static async Task EverySecond(Options options)
    {
        var settings = new ValkeyOptions { DataDirectory = Path.Combine(options.Directory, "aof-everysecond"), Port = options.Port,
            AppendOnly = true, AppendFsync = ValkeyAppendFsync.EverySecond, DisposeMode = ValkeyShutdownMode.NoSave };
        await Case("bio:aof-everysecond-fsync-completion-and-replay", async () =>
        {
            var owner = Start(settings); await Ready(owner);
            await using var connection = await RespConnection.Connect(await owner.Endpoint, cancellation);
            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                try { await owner.GetPersistenceStatusAsync(cancelled.Token); throw new InvalidOperationException("Pre-cancelled persistence snapshot completed."); }
                catch (OperationCanceledException) when (cancelled.IsCancellationRequested) { }
            }
            var baseline = await owner.GetPersistenceStatusAsync(cancellation);
            Check.True(baseline.AppendOnlyEnabled, "EverySecond owner did not enable AOF.");
            string[] values = Enumerable.Range(0, 256).Select(i => new string('x', 1024) + i.ToString(CultureInfo.InvariantCulture)).ToArray();
            string[][] writes = values.Select((value, i) => new[] { "SET", "periodic:" + i, value }).ToArray();
            foreach (var reply in await connection.Pipeline(writes)) Check.Equal(reply, "OK");
            var target = await owner.GetPersistenceStatusAsync(cancellation);
            Check.True(target.PrimaryOffset > baseline.PrimaryOffset, "Writes did not advance the replication stream offset.");
            var completed = target;
            await Until(async () =>
            {
                completed = await owner.GetPersistenceStatusAsync(cancellation);
                Check.True(!completed.BackgroundFsyncFailed, "Background AOF fsync reported failure.");
                var persistence = Info(await connection.Command("INFO", "persistence"));
                Check.Equal(persistence["aof_last_write_status"], "ok");
                return completed.FsyncedOffset >= target.PrimaryOffset &&
                    Number(persistence, "aof_pending_bio_fsync") == 0 && Number(persistence, "aof_buffer_length") == 0;
            }, "Periodic AOF background fsync did not publish completion through the target stream offset.");
            Check.True(completed.AofCurrentBytes > baseline.AofCurrentBytes, "AOF bytes did not increase after writes.");
            RecordEvidence($"PrimaryOffset {baseline.PrimaryOffset}->{target.PrimaryOffset}; FsyncedOffset={completed.FsyncedOffset}; AofCurrentBytes {baseline.AofCurrentBytes}->{completed.AofCurrentBytes}; pending_bio_fsync=0; last_write_status=ok");
            await Stopped(owner, ValkeyShutdownMode.NoSave);
            try { await owner.GetPersistenceStatusAsync(cancellation); throw new InvalidOperationException("Stopped owner returned a persistence snapshot."); }
            catch (ValkeyException) { }
            var restarted = Start(settings); await Ready(restarted);
            await using var replay = await RespConnection.Connect(await restarted.Endpoint, cancellation);
            var actual = await replay.Pipeline(values.Select((_, i) => new[] { "GET", "periodic:" + i }).ToArray());
            Check.True(actual.Length == values.Length, "AOF replay returned an incorrect number of values.");
            for (int i = 0; i < values.Length; i++) Check.Equal(actual[i], values[i]);
            await Stopped(restarted, ValkeyShutdownMode.NoSave);
        });
    }
    private static Dictionary<string, string> Info(object? reply)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (string raw in Check.Text(reply).Split('\n'))
        {
            string line = raw.TrimEnd('\r');
            int separator = line.IndexOf(':');
            if (separator > 0 && line[0] != '#') fields[line[..separator]] = line[(separator + 1)..];
        }
        return fields;
    }
    private static long Number(Dictionary<string, string> fields, string name) =>
        fields.TryGetValue(name, out string? value) ? long.Parse(value, CultureInfo.InvariantCulture) : throw new InvalidOperationException("Missing INFO field " + name);
    private static async Task Until(Func<Task<bool>> condition, string failure)
    {
        var deadline = Stopwatch.StartNew();
        do
        {
            if (await condition()) return;
            await Task.Delay(20, cancellation);
        } while (deadline.Elapsed < TimeSpan.FromSeconds(15));
        throw new TimeoutException(failure);
    }
    private static void RecordEvidence(string detail)
    {
        Console.WriteLine(detail);
        caseEvidence = caseEvidence is null ? detail : caseEvidence + "\n" + detail;
    }

    private static async Task Exchange(Options options)
    {
        bool appendOnly = options.Suite == "aof";
        var settings = new ValkeyOptions { DataDirectory = options.Directory, Port = options.Port,
            AppendOnly = appendOnly, AppendFsync = ValkeyAppendFsync.Always, DisposeMode = ValkeyShutdownMode.NoSave };
        await Case($"exchange:{options.Exchange}:{options.Suite}:actual-owner-start", async () =>
        {
            var owner = Start(settings); await Ready(owner);
            await using var connection = await RespConnection.Connect(await owner.Endpoint, cancellation);
            if (options.Exchange == "export") await SeedExchange(connection, options.Expiry);
            await VerifyExchange(connection, options.Expiry);
            await Stopped(owner, appendOnly || options.Exchange == "import" ? ValkeyShutdownMode.NoSave : ValkeyShutdownMode.Save);
        });
    }

    private static async Task SeedExchange(RespConnection connection, long expiry)
    {
        Check.Equal(await connection.Command("DBSIZE"), 0L);
        Check.Equal(await connection.Bytes("SET"u8.ToArray(), "binary"u8.ToArray(), binary), "OK");
        Check.Equal(await connection.Command("SET", "owner", "exchange"), "OK");
        Check.Equal(await connection.Command("SET", "ttl", "future", "PXAT", expiry.ToString(CultureInfo.InvariantCulture)), "OK");
        Check.Equal(await connection.Command("RPUSH", "list", "one", "two"), 2L);
        Check.Equal(await connection.Command("HSET", "hash", "field", "value"), 1L);
        Check.Equal(await connection.Command("SADD", "set", "beta", "alpha"), 2L);
        Check.Equal(await connection.Command("ZADD", "sorted", "1.25", "alpha", "2.5", "beta"), 2L);
        Check.Equal(await connection.Command("XADD", "stream", "1-0", "field", "value"), "1-0");
        Check.Equal(await connection.Command("MULTI"), "OK");
        Check.Equal(await connection.Command("SET", "tx", "40"), "QUEUED");
        Check.Equal(await connection.Command("INCR", "tx"), "QUEUED");
        var transaction = await connection.Command("EXEC") as object?[] ?? throw new InvalidOperationException("Exchange EXEC did not return an array.");
        Check.True(transaction.Length == 2, "Exchange EXEC count differs.");
        Check.Equal(transaction[0], "OK"); Check.Equal(transaction[1], 41L);
        Check.Equal(await connection.Command("EVAL", "redis.call('SET','script-effect','persisted'); return 1", "0"), 1L);
        Check.Equal(await connection.Command("FUNCTION", "LOAD", Library), "managed_integration");
    }

    private static async Task VerifyExchange(RespConnection connection, long expiry)
    {
        Check.Equal(await connection.Command("DBSIZE"), 10L);
        Check.Binary(await connection.Command("GET", "binary"), binary);
        Check.Equal(await connection.Command("GET", "owner"), "exchange");
        Check.Equal(await connection.Command("GET", "ttl"), "future");
        Check.Equal(await connection.Command("PEXPIRETIME", "ttl"), expiry);
        Check.True((long)(await connection.Command("PTTL", "ttl") ?? -1L) > 0, "Exchange expiry elapsed or disappeared.");
        Check.Equal(await connection.Command("LINDEX", "list", "0"), "one");
        Check.Equal(await connection.Command("LINDEX", "list", "1"), "two");
        Check.Equal(await connection.Command("LLEN", "list"), 2L);
        Check.Equal(await connection.Command("HGET", "hash", "field"), "value");
        Check.Equal(await connection.Command("HLEN", "hash"), 1L);
        Check.Equal(await connection.Command("SCARD", "set"), 2L);
        Check.Equal(await connection.Command("SISMEMBER", "set", "alpha"), 1L);
        Check.Equal(await connection.Command("SISMEMBER", "set", "beta"), 1L);
        Check.Equal(await connection.Command("ZCARD", "sorted"), 2L);
        Check.Equal(await connection.Command("ZSCORE", "sorted", "alpha"), "1.25");
        Check.Equal(await connection.Command("ZSCORE", "sorted", "beta"), "2.5");
        Check.Equal(await connection.Command("XLEN", "stream"), 1L);
        var stream = await connection.Command("XRANGE", "stream", "-", "+") as object?[] ?? throw new InvalidOperationException("Missing stream reply.");
        Check.True(stream.Length == 1 && stream[0] is object?[] { Length: 2 }, "Stream entry count differs.");
        var entry = (object?[])stream[0]!;
        Check.Equal(entry[0], "1-0");
        var fields = entry[1] as object?[] ?? throw new InvalidOperationException("Missing stream field array.");
        Check.True(fields.Length == 2, "Stream field count differs."); Check.Equal(fields[0], "field"); Check.Equal(fields[1], "value");
        foreach (var pair in new[] { ("binary", "string"), ("ttl", "string"), ("list", "list"), ("hash", "hash"), ("set", "set"), ("sorted", "zset"), ("stream", "stream") })
            Check.Equal(await connection.Command("TYPE", pair.Item1), pair.Item2);
        Check.Equal(await connection.Command("GET", "tx"), "41");
        Check.Equal(await connection.Command("GET", "script-effect"), "persisted");
        Check.Equal(await connection.Command("FCALL", "managed_read", "1", "owner"), "exchange");
        Check.Equal(await connection.Command("EVAL", "return redis.call('HGET',KEYS[1],'field')", "1", "hash"), "value");
    }

    private static async Task CompareNative(Options options)
    {
        await Case("native-differential:admitted-family-transitions", async () =>
        {
            var endpoint = new IPEndPoint(IPAddress.Loopback, options.NativePort);
            await using var native = await RespConnection.Connect(endpoint, cancellation);
            await CommandCoverage.Run(native, endpoint, null, cancellation);
        });
        await Case("native-differential:actual-resp-command-and-lua-results", async () =>
        {
            var server = Start(new() { DataDirectory = Path.Combine(options.Directory, "comparison"), Port = options.Port, DisposeMode = ValkeyShutdownMode.NoSave });
            await Ready(server);
            await using var managed = await RespConnection.Connect(await server.Endpoint, cancellation);
            await using var native = await RespConnection.Connect(new(IPAddress.Loopback, options.NativePort), cancellation);
            string[][] corpus = [["PING"], ["SET", "comparison", "hello"], ["GET", "comparison"], ["INCRBY", "number", "2147483648"],
                ["HSET", "h", "f", "v"], ["HGET", "h", "f"], ["RPUSH", "l", "a", "b"], ["LRANGE", "l", "0", "-1"],
                ["EVAL", "return {redis.call('GET', KEYS[1]),cjson.decode('{\"n\":42}').n,bit.band(7,3)}", "1", "comparison"],
                ["MULTI"], ["SET", "tx", "9"], ["INCR", "tx"], ["EXEC"]];
            foreach (string[] command in corpus)
                Check.Equal(Canonical(await managed.Command(command)), Canonical(await native.Command(command)));
            await Stopped(server, ValkeyShutdownMode.NoSave);
        });
    }
    private static string Canonical(object? value) => value switch
    {
        null => "null", byte[] bytes => "bytes:" + Convert.ToBase64String(bytes), string text => "text:" + text,
        long number => "int:" + number.ToString(CultureInfo.InvariantCulture), object?[] values => "[" + string.Join(",", values.Select(Canonical)) + "]",
        RespError error => "error:" + error.Message, _ => throw new InvalidOperationException("Unsupported differential reply.")
    };
    private static async Task Serve(Options options)
    {
        var server = Start(new() { DataDirectory = options.Directory, Port = options.Port, AppendOnly = options.AppendOnly, DisposeMode = ValkeyShutdownMode.NoSave });
        await Ready(server);
        Console.WriteLine("MANAGED_VALKEY_READY " + (await server.Endpoint).Port.ToString(CultureInfo.InvariantCulture));
        Console.Out.Flush();
        while (true)
        {
            Task<string?> line = Console.In.ReadLineAsync();
            Task finished = await Task.WhenAny(line, server.Completion);
            if (finished == server.Completion) { await server.Completion; break; }
            string? command = await line;
            if (command is null or "STOP NOSAVE") { await Stopped(server, ValkeyShutdownMode.NoSave); break; }
            if (command == "STOP SAVE") { await Stopped(server, ValkeyShutdownMode.Save); break; }
            Console.Error.WriteLine("Expected STOP SAVE or STOP NOSAVE.");
        }
        results.Add(new("serve:lifecycle", "passed", 0, null));
    }
    private static void WriteReport(Options options, int status)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(options.Report)!);
        using var file = File.Create(options.Report);
        using var writer = new Utf8JsonWriter(file, new() { Indented = true });
        writer.WriteStartObject(); writer.WriteNumber("schema_version", 1); writer.WriteString("backend", "managed-translated-valkey");
        writer.WriteString("execution", RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "NativeAOT");
        writer.WriteString("status", status == 0 ? "passed" : "failed"); writer.WriteString("data_directory", options.Directory);
        writer.WriteNumber("port", options.Port); writer.WriteNumber("peer_port", options.PeerPort);
        writer.WriteString("suite", options.Suite);
        if (options.Exchange is not null) { writer.WriteString("exchange", options.Exchange); writer.WriteNumber("absolute_expiry", options.Expiry); }
        writer.WriteStartArray("checks");
        foreach (var result in results)
        {
            writer.WriteStartObject(); writer.WriteString("name", result.Name); writer.WriteString("status", result.Status);
            writer.WriteNumber("milliseconds", result.Milliseconds); if (result.Detail is not null) writer.WriteString("detail", result.Detail); writer.WriteEndObject();
        }
        writer.WriteEndArray(); writer.WriteEndObject();
    }
}

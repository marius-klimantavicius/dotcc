using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Text;
using Managed.Valkey;

int port = 6379, peerPort = 6380;
string directory = Path.Combine(Path.GetTempPath(), "dotcc-valkey-" + Guid.NewGuid().ToString("N"));
string? password = null;
bool appendOnly = false;
for (int i = 0; i < args.Length; ++i)
{
    string Value() => ++i < args.Length ? args[i] : throw new ArgumentException("Missing option value.");
    switch (args[i])
    {
        case "--port": port = int.Parse(Value(), CultureInfo.InvariantCulture); break;
        case "--peer-port": peerPort = int.Parse(Value(), CultureInfo.InvariantCulture); break;
        case "--directory": directory = Path.GetFullPath(Value()); break;
        case "--password": password = Value(); break;
        case "--appendonly": appendOnly = true; break;
        default: throw new ArgumentException("Options: --port N --peer-port N --directory PATH --password VALUE --appendonly");
    }
}
if (port == peerPort) throw new ArgumentException("The two instances require distinct TCP ports.");
var firstOptions = new ValkeyOptions
{
    DataDirectory = Path.Combine(directory, "first"), Port = port, Password = password,
    AppendOnly = appendOnly, DisposeMode = appendOnly ? ValkeyShutdownMode.NoSave : ValkeyShutdownMode.Save
};
var peerOptions = firstOptions with { DataDirectory = Path.Combine(directory, "peer"), Port = peerPort };
using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
ValkeyServer? first = null, peer = null, restarted = null;
ExceptionDispatchInfo? primaryFailure = null;
try
{
    first = new ValkeyServer(firstOptions);
    peer = new ValkeyServer(peerOptions);
    await Task.WhenAll(first.Ready, peer.Ready).WaitAsync(timeout.Token);
    await using var client = await RespClient.ConnectAsync(await first.Endpoint, password, timeout.Token);
    await using var other = await RespClient.ConnectAsync(await peer.Endpoint, password, timeout.Token);
    string key = "managed-consumer:" + Guid.NewGuid().ToString("N");
    Check(await client.CommandAsync("SET", key, "first"), "OK");
    Check(await other.CommandAsync("SET", key, "peer"), "OK");
    Check(await client.CommandAsync("GET", key), "first");
    Check(await other.CommandAsync("GET", key), "peer");
    byte[] binary = [0, 255, 13, 10, 128, 1];
    Check(await client.CommandBytesAsync("SET"u8.ToArray(), Encoding.UTF8.GetBytes(key + ":binary"), binary), "OK");
    if (await client.CommandAsync("GET", key + ":binary") is not byte[] actual || !actual.SequenceEqual(binary))
        throw new InvalidOperationException("Binary GET/SET mismatch.");
    Check(await client.CommandAsync("RPUSH", key + ":list", "one", "two"), 2L);
    Check(await client.CommandAsync("LPOP", key + ":list"), "one");
    Check(await client.CommandAsync("HSET", key + ":hash", "field", "value"), 1L);
    Check(await client.CommandAsync("HGET", key + ":hash", "field"), "value");
    Check(await client.CommandAsync("MULTI"), "OK");
    Check(await client.CommandAsync("SET", key + ":transaction", "41"), "QUEUED");
    Check(await client.CommandAsync("INCR", key + ":transaction"), "QUEUED");
    if (await client.CommandAsync("EXEC") is not object?[] transaction || transaction.Length != 2)
        throw new InvalidOperationException("EXEC did not return two results.");
    Check(transaction[0], "OK"); Check(transaction[1], 42L);
    Check(await client.CommandAsync("EVAL", "return redis.call('GET', KEYS[1])", "1", key), "first");
    Check(await client.CommandAsync("EVAL", "return cjson.decode('{\"value\":42}').value + bit.band(7,3)", "0"), 45L);
    await first.StopAsync(firstOptions.DisposeMode).WaitAsync(timeout.Token);
    Check(await other.CommandAsync("PING"), "PONG");
    restarted = new ValkeyServer(firstOptions);
    await restarted.Ready.WaitAsync(timeout.Token);
    await using var loaded = await RespClient.ConnectAsync(await restarted.Endpoint, password, timeout.Token);
    Check(await loaded.CommandAsync("GET", key), "first");
    Check(await loaded.CommandAsync("GET", key + ":transaction"), "42");
    Check(await other.CommandAsync("GET", key), "peer");
    await restarted.StopAsync(firstOptions.DisposeMode).WaitAsync(timeout.Token);
    await peer.StopAsync(peerOptions.DisposeMode).WaitAsync(timeout.Token);
    Console.WriteLine($"PASS: TCP, binary values, list/hash, MULTI, Lua, two owners and {(appendOnly ? "AOF" : "RDB")} restart.");
    Console.WriteLine("Persistence files: " + directory);
}
catch (Exception error) { primaryFailure = ExceptionDispatchInfo.Capture(error); }
finally
{
    // This sample owns its test data and explicitly chooses NOSAVE on a failed
    // check; the library itself never silently converts a failed SAVE to NOSAVE.
    var failures = new List<Exception>();
    foreach (var server in new[] { restarted, first, peer })
    {
        if (server is null) continue;
        try { await server.StopAsync(ValkeyShutdownMode.NoSave).WaitAsync(TimeSpan.FromSeconds(20)); }
        catch (Exception error) { failures.Add(error); }
    }
    if (primaryFailure is null && failures.Count != 0)
        throw new AggregateException("Sample cleanup failed.", failures);
    if (primaryFailure is not null)
        foreach (var error in failures)
            if (!ReferenceEquals(error, primaryFailure.SourceException))
                Console.Error.WriteLine("Additional cleanup failure: " + error.Message);
}
primaryFailure?.Throw();

static void Check(object? actual, object expected)
{
    if (actual is byte[] bytes) actual = Encoding.UTF8.GetString(bytes);
    if (!Equals(actual, expected)) throw new InvalidOperationException($"Expected {expected}, received {actual ?? "(null)"}.");
}

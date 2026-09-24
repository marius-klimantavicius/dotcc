using System.Globalization;
using System.Net;

/// <summary>State transitions across admitted command families, not exhaustive command qualification.</summary>
internal static class CommandCoverage
{
    internal static async Task Run(RespConnection client, IPEndPoint endpoint, string? password, CancellationToken cancellation)
    {
        const string prefix = "coverage:";
        string user = "coverage_" + Guid.NewGuid().ToString("N");
        string[] keys = ["number", "bits", "inverted", "hash", "list", "moved", "set", "set2", "sorted", "stream", "hll", "hll2", "hll-union", "geo", "watched"];
        keys = keys.Select(key => prefix + key).ToArray();
        bool userCreated = false;
        try
        {
            Check.Equal(await client.Command("SET", prefix + "number", "10"), "OK");
            Check.Equal(await client.Command("INCRBY", prefix + "number", "7"), 17L);
            Check.Equal(await client.Command("DECRBY", prefix + "number", "2"), 15L);
            Check.Equal(await client.Command("INCRBYFLOAT", prefix + "number", "0.5"), "15.5");
            Check.Error(await client.Command("INCR", prefix + "number"), "integer");
            Check.Equal(await client.Command("GET", prefix + "number"), "15.5");

            Check.Equal(await client.Command("SETBIT", prefix + "bits", "0", "1"), 0L);
            Check.Equal(await client.Command("SETBIT", prefix + "bits", "7", "1"), 0L);
            Check.Equal(await client.Command("BITCOUNT", prefix + "bits"), 2L);
            Check.Equal(await client.Command("BITOP", "NOT", prefix + "inverted", prefix + "bits"), 1L);
            Check.Binary(await client.Command("GET", prefix + "inverted"), [0x7e]);

            Check.Equal(await client.Command("HSET", prefix + "hash", "count", "4", "name", "old"), 2L);
            Check.Equal(await client.Command("HINCRBY", prefix + "hash", "count", "3"), 7L);
            Sequence(await client.Command("HMGET", prefix + "hash", "name", "count", "absent"), "old", "7", null);
            Check.Equal(await client.Command("HDEL", prefix + "hash", "name"), 1L);
            Check.Equal(await client.Command("HEXISTS", prefix + "hash", "name"), 0L);

            Check.Equal(await client.Command("RPUSH", prefix + "list", "a", "b", "c"), 3L);
            Check.Equal(await client.Command("LMOVE", prefix + "list", prefix + "moved", "RIGHT", "LEFT"), "c");
            Sequence(await client.Command("LRANGE", prefix + "list", "0", "-1"), "a", "b");
            Check.Equal(await client.Command("LPOP", prefix + "list"), "a");
            Check.Equal(await client.Command("LINDEX", prefix + "list", "0"), "b");

            Check.Equal(await client.Command("SADD", prefix + "set", "a", "b", "c"), 3L);
            Check.Equal(await client.Command("SREM", prefix + "set", "a"), 1L);
            Check.Equal(await client.Command("SADD", prefix + "set2", "b", "d"), 2L);
            Sequence(await client.Command("SINTER", prefix + "set", prefix + "set2"), "b");
            Check.Equal(await client.Command("SISMEMBER", prefix + "set", "a"), 0L);

            Check.Equal(await client.Command("ZADD", prefix + "sorted", "1", "a", "2", "b", "3", "c"), 3L);
            Check.Equal(await client.Command("ZINCRBY", prefix + "sorted", "3", "a"), "4");
            Sequence(await client.Command("ZRANGE", prefix + "sorted", "0", "-1", "WITHSCORES"), "b", "2", "c", "3", "a", "4");
            Check.Equal(await client.Command("ZRANK", prefix + "sorted", "a"), 2L);
            Check.Equal(await client.Command("ZREM", prefix + "sorted", "b"), 1L);

            Check.Equal(await client.Command("XADD", prefix + "stream", "1-0", "field", "first"), "1-0");
            Check.Equal(await client.Command("XADD", prefix + "stream", "2-0", "field", "second"), "2-0");
            Check.Equal(await client.Command("XGROUP", "CREATE", prefix + "stream", "group", "0"), "OK");
            var read = Array(await client.Command("XREADGROUP", "GROUP", "group", "consumer", "COUNT", "1", "STREAMS", prefix + "stream", ">"), 1);
            var stream = Array(read[0], 2);
            Check.Equal(stream[0], prefix + "stream");
            var entry = Array(Array(stream[1], 1)[0], 2);
            Check.Equal(entry[0], "1-0"); Sequence(entry[1], "field", "first");
            var pending = Array(await client.Command("XPENDING", prefix + "stream", "group"), 4);
            Check.Equal(pending[0], 1L); Check.Equal(pending[1], "1-0");
            Check.Equal(await client.Command("XACK", prefix + "stream", "group", "1-0"), 1L);
            Check.Equal(Array(await client.Command("XPENDING", prefix + "stream", "group"), 4)[0], 0L);
            Check.Equal(await client.Command("XGROUP", "DESTROY", prefix + "stream", "group"), 1L);

            Check.Equal(await client.Command("PFADD", prefix + "hll", "a", "b", "c", "a"), 1L);
            Check.Equal(await client.Command("PFCOUNT", prefix + "hll"), 3L);
            Check.Equal(await client.Command("PFADD", prefix + "hll2", "c", "d"), 1L);
            Check.Equal(await client.Command("PFMERGE", prefix + "hll-union", prefix + "hll", prefix + "hll2"), "OK");
            Check.Equal(await client.Command("PFCOUNT", prefix + "hll-union"), 4L);

            Check.Equal(await client.Command("GEOADD", prefix + "geo", "13.361389", "38.115556", "Palermo", "15.087269", "37.502669", "Catania"), 2L);
            double distance = double.Parse(Check.Text(await client.Command("GEODIST", prefix + "geo", "Palermo", "Catania", "km")), CultureInfo.InvariantCulture);
            Check.True(distance is > 160 and < 170, "Unexpected Palermo/Catania geographic distance.");
            Sequence(await client.Command("GEOSEARCH", prefix + "geo", "FROMMEMBER", "Palermo", "BYRADIUS", "200", "km", "ASC"), "Palermo", "Catania");

            var scanned = new HashSet<string>(StringComparer.Ordinal);
            string cursor = "0";
            int iterations = 0;
            do
            {
                var page = Array(await client.Command("SCAN", cursor, "MATCH", prefix + "*", "COUNT", "2"), 2);
                cursor = Check.Text(page[0]);
                foreach (object? key in page[1] as object?[] ?? throw new InvalidOperationException("SCAN returned no key array.")) scanned.Add(Check.Text(key));
                Check.True(++iterations <= 1000, "SCAN did not terminate.");
            } while (cursor != "0");
            Check.True(keys.Where(key => key != prefix + "watched").All(scanned.Contains), "SCAN omitted an existing coverage key.");

            await using var writer = await RespConnection.Connect(endpoint, cancellation, password);
            Check.Equal(await client.Command("WATCH", prefix + "watched"), "OK");
            Check.Equal(await writer.Command("SET", prefix + "watched", "concurrent"), "OK");
            Check.Equal(await client.Command("MULTI"), "OK");
            Check.Equal(await client.Command("SET", prefix + "watched", "transaction"), "QUEUED");
            Check.Equal(await client.Command("EXEC"), null);
            Check.Equal(await client.Command("GET", prefix + "watched"), "concurrent");

            Check.Equal(await client.Command("ACL", "SETUSER", user, "reset", "on", ">coverage-password", "~coverage:*", "+get"), "OK");
            userCreated = true;
            await using var restricted = await RespConnection.Connect(endpoint, cancellation);
            Check.Equal(await restricted.Command("AUTH", user, "coverage-password"), "OK");
            Check.Equal(await restricted.Command("GET", prefix + "number"), "15.5");
            Check.Error(await restricted.Command("SET", prefix + "number", "99"), "NOPERM");
            Check.Error(await restricted.Command("GET", "owner"), "NOPERM");
        }
        finally
        {
            if (userCreated) Check.Equal(await client.Command("ACL", "DELUSER", user), 1L);
            // The RDB/AOF assertions elsewhere rely on their original exact key count.
            await client.Command(["DEL", .. keys]);
        }
    }

    private static object?[] Array(object? value, int length)
    {
        var result = value as object?[] ?? throw new InvalidOperationException("Expected RESP array.");
        Check.Equal(result.Length, length);
        return result;
    }
    private static void Sequence(object? value, params object?[] expected)
    {
        var actual = Array(value, expected.Length);
        for (int i = 0; i < expected.Length; ++i) Check.Equal(actual[i], expected[i]);
    }
}

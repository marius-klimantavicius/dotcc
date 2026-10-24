using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace NpgsqlAcceptance;

internal sealed partial class Suite(NpgsqlConnection connection, Checks check, string workRoot)
{
    NpgsqlCommand Command(string sql) => new(sql, connection) { CommandTimeout = 0 };

    async Task<object?> Scalar(string sql)
    {
        await using var cmd = Command(sql);
        return await cmd.ExecuteScalarAsync();
    }

    async Task<int> Execute(string sql)
    {
        await using var cmd = Command(sql);
        return await cmd.ExecuteNonQueryAsync();
    }

    internal IEnumerable<AcceptanceCase> CodecCases()
    {
        yield return new("codecs.null", Null);
        yield return new("codecs.text-unicode", Text);
        yield return new("codecs.bytea", Bytea);
        yield return new("codecs.integer-float-bool", Numbers);
        yield return new("codecs.numeric-all-upstream-vectors", Numeric);
        yield return new("codecs.numeric-overflow-recovery", NumericOverflow);
        yield return new("codecs.arrays", Arrays);
        yield return new("codecs.date-time-uuid", DateTimeUuid);
        yield return new("codecs.large-fragmented-values", LargeValues);
        yield return new("codecs.sequential-getbytes", SequentialBytes);
    }

    async Task Roundtrip<T>(T value, string literal, string pgType, NpgsqlDbType type)
    {
        // Literal read independently exercises the server's encoder. Parameter
        // echo and SQL equality exercise Npgsql's writer as well as its reader.
        await using (var cmd = Command($"SELECT '{literal.Replace("'", "''")}'::{pgType}"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            check.True(await reader.ReadAsync(), pgType + " literal row");
            check.Equal(value, reader.GetFieldValue<T>(0), pgType + " literal typed value");
            check.Equal(pgType, reader.GetDataTypeName(0), pgType + " literal metadata");
            check.False(await reader.ReadAsync(), pgType + " literal end");
        }
        await using (var cmd = Command($"SELECT $1, $1 = '{literal.Replace("'", "''")}'::{pgType}, pg_typeof($1)::text"))
        {
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = type, Value = value });
            await using var reader = await cmd.ExecuteReaderAsync();
            check.True(await reader.ReadAsync(), pgType + " parameter row");
            check.Equal(value, reader.GetFieldValue<T>(0), pgType + " parameter typed value");
            check.True(reader.GetBoolean(1), pgType + " server equality");
            check.Equal(pgType, reader.GetString(2), pgType + " server parameter type");
            check.False(await reader.ReadAsync(), pgType + " parameter end");
        }
    }

    async Task Null()
    {
        using var cmd = Command("SELECT @p1, @p2::text");
        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String) { Value = DBNull.Value });
        cmd.Parameters.Add(new NpgsqlParameter { ParameterName = "p2", Value = DBNull.Value });
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            check.True(await reader.ReadAsync(), "null row");
            for (int i = 0; i < 2; i++)
            {
                check.True(reader.IsDBNull(i), "IsDBNull");
                check.True(await reader.IsDBNullAsync(i), "IsDBNullAsync");
                check.Equal<object>(DBNull.Value, reader.GetValue(i), "GetValue DBNull");
                check.Equal<object>(DBNull.Value, reader.GetFieldValue<object>(i), "GetFieldValue DBNull");
                check.Equal<object>(DBNull.Value, reader.GetProviderSpecificValue(i), "provider DBNull");
                check.Throws<InvalidCastException>(() => reader.GetString(i), "null GetString");
                check.Throws<InvalidCastException>(() => reader.GetStream(i), "null GetStream");
            }
        }
        using var invalid = Command("SELECT @p::text");
        invalid.Parameters.AddWithValue("p", NpgsqlDbType.Text, null!);
        await check.ThrowsAsync<InvalidOperationException>(async () => { await invalid.ExecuteScalarAsync(); }, "untyped null value");
    }

    async Task Text()
    {
        foreach (string value in new[] { "foo", "", "Vilnius Žąsis — 日本語 🐘 e\u0301 ' quoted \\ @p; $1" })
            await Roundtrip(value, value, "text", NpgsqlDbType.Text);
        using var cmd = Command("SELECT @p::text");
        var parameter = new NpgsqlParameter("p", "SomeText") { Size = 4 };
        cmd.Parameters.Add(parameter);
        check.Equal<object?>("Some", await cmd.ExecuteScalarAsync(), "text truncation");
        parameter.Value = "AnotherValue";
        check.Equal<object?>("Anot", await cmd.ExecuteScalarAsync(), "Size persists after value change");
        // Match upstream TextTests.Truncate: explicitly reassign the value before
        // increasing Size, even though the CLR string value is unchanged.
        parameter.Value = "AnotherValue";
        foreach (int size in new[] { 22, 0, -1 })
        {
            parameter.Size = size;
            check.Equal<object?>("AnotherValue", await cmd.ExecuteScalarAsync(), "text Size " + size);
        }
        check.Throws<ArgumentException>(() => parameter.Size = -2, "negative Size");
    }

    async Task Bytea()
    {
        await Roundtrip(new byte[] { 1, 2, 3, 4, 5 }, "\\x0102030405", "bytea", NpgsqlDbType.Bytea);
        await Roundtrip(Array.Empty<byte>(), "\\x", "bytea", NpgsqlDbType.Bytea);
        byte[] value = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        await Roundtrip(value, "\\x" + Convert.ToHexString(value).ToLowerInvariant(), "bytea", NpgsqlDbType.Bytea);
    }

    async Task Numbers()
    {
        await Roundtrip(short.MinValue, "-32768", "smallint", NpgsqlDbType.Smallint);
        await Roundtrip(short.MaxValue, "32767", "smallint", NpgsqlDbType.Smallint);
        await Roundtrip(int.MinValue, "-2147483648", "integer", NpgsqlDbType.Integer);
        await Roundtrip(int.MaxValue, "2147483647", "integer", NpgsqlDbType.Integer);
        await Roundtrip(long.MinValue, "-9223372036854775808", "bigint", NpgsqlDbType.Bigint);
        await Roundtrip(long.MaxValue, "9223372036854775807", "bigint", NpgsqlDbType.Bigint);
        await Roundtrip(1.25f, "1.25", "real", NpgsqlDbType.Real);
        await Roundtrip(-123.125d, "-123.125", "double precision", NpgsqlDbType.Double);
        await Roundtrip(true, "true", "boolean", NpgsqlDbType.Boolean);
        await Roundtrip(false, "false", "boolean", NpgsqlDbType.Boolean);
        await Roundtrip(new[] { true, false }, "{t,f}", "boolean[]", NpgsqlDbType.Array | NpgsqlDbType.Boolean);
    }

    async Task Numeric()
    {
        foreach (var (sql, expected) in NumericVectors.Cases)
        {
            using (var cmd = Command("SELECT " + sql))
            {
                var value = (decimal)(await cmd.ExecuteScalarAsync())!;
                check.Equal(decimal.GetBits(expected), decimal.GetBits(value), "numeric Read bits " + sql);
            }
            using (var cmd = Command("SELECT @p, @p = " + sql))
            {
                cmd.Parameters.AddWithValue("p", expected);
                await using var reader = await cmd.ExecuteReaderAsync();
                check.True(reader.Read(), "numeric Write row " + sql);
                check.Equal(decimal.GetBits(expected), decimal.GetBits(reader.GetFieldValue<decimal>(0)), "numeric Write bits " + sql);
                check.True(reader.GetFieldValue<bool>(1), "numeric Write SQL equality " + sql);
            }
        }
        using var zero = Command("SELECT @p");
        zero.Parameters.Add(new NpgsqlParameter("p", DbType.Decimal, 10, null, ParameterDirection.Input,
            false, 10, 2, DataRowVersion.Default, 0.00M));
        check.Equal((byte)2, ((decimal)(await zero.ExecuteScalarAsync())!).Scale, "numeric zero scale");
    }

    async Task NumericOverflow()
    {
        using var cmd = Command("SELECT (0.20285714285714285714285714285)::numeric, generate_series FROM generate_series(1, 2)");
        await using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
        {
            int i = 1;
            while (await reader.ReadAsync())
            {
                var error = check.Throws<OverflowException>(() => reader.GetDecimal(0), "numeric read overflow");
                check.Equal("Numeric value does not fit in a System.Decimal", error.Message, "numeric overflow message");
                check.Equal(i++, reader.GetInt32(1), "next field after overflow");
                check.Equal(ConnectionState.Open | ConnectionState.Fetching, connection.FullState, "overflow FullState");
                check.Equal(ConnectionState.Open, connection.State, "overflow State");
            }
            check.Equal(3, i, "both overflow rows consumed");
        }
        check.Equal<object?>(42, await Scalar("SELECT 42"), "query after numeric overflow");
    }

    async Task Arrays()
    {
        await Roundtrip(new[] { 1, 2, 3 }, "{1,2,3}", "integer[]", NpgsqlDbType.Array | NpgsqlDbType.Integer);
        await Roundtrip(Array.Empty<int>(), "{}", "integer[]", NpgsqlDbType.Array | NpgsqlDbType.Integer);
        await Roundtrip(new[,] { { 1, 2, 3 }, { 7, 8, 9 } }, "{{1,2,3},{7,8,9}}", "integer[]", NpgsqlDbType.Array | NpgsqlDbType.Integer);
        await Roundtrip(new string?[] { "a", null, "Žąsis", "NULL", "a,b", "{\\\"}" },
            "{\"a\",NULL,\"Žąsis\",\"NULL\",\"a,b\",\"{\\\\\\\"}\"}", "text[]", NpgsqlDbType.Array | NpgsqlDbType.Text);
        foreach (int?[] expected in new int?[][] { [1, 2, null, 3], [1, 5, null, 9] })
        {
            using var cmd = Command("SELECT @p1");
            cmd.Parameters.AddWithValue("p1", NpgsqlDbType.Array | NpgsqlDbType.Integer, expected);
            await using var reader = await cmd.ExecuteReaderAsync();
            check.True(await reader.ReadAsync(), "nullable array row");
            check.Equal(expected, reader.GetFieldValue<int?[]>(0), "nullable array values");
            var error = check.Throws<InvalidCastException>(() => reader.GetFieldValue<int[]>(0), "nullable as nonnullable array");
            check.Equal("Cannot read a non-nullable collection of elements because the returned array contains nulls. Call GetFieldValue with a nullable collection type instead.",
                error.Message, "nullable array exact exception message");
        }
    }

    async Task DateTimeUuid()
    {
        await Roundtrip(new DateOnly(2020, 10, 1), "2020-10-01", "date", NpgsqlDbType.Date);
        await Roundtrip(new DateOnly(1900, 1, 1), "1900-01-01", "date", NpgsqlDbType.Date);
        await Roundtrip(new TimeOnly(10, 45, 34, 500), "10:45:34.5", "time without time zone", NpgsqlDbType.Time);
        await Roundtrip(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Unspecified),
            "1998-04-12 13:26:38", "timestamp without time zone", NpgsqlDbType.Timestamp);
        await Roundtrip(new DateTime(1998, 4, 12, 13, 26, 38, DateTimeKind.Utc),
            "1998-04-12 13:26:38+00", "timestamp with time zone", NpgsqlDbType.TimestampTz);
        await Roundtrip(new Guid("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"),
            "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11", "uuid", NpgsqlDbType.Uuid);
        await Roundtrip(new TimeSpan(2, 3, 4, 5, 600), "2 days 03:04:05.6", "interval", NpgsqlDbType.Interval);
    }

    async Task LargeValues()
    {
        // These all cross Npgsql's default 8192-byte read/write buffers, including
        // UTF-8 sequences spanning flushes. Exact content equality is mandatory.
        foreach (int size in new[] { 8191, 8192, 8193, 262147 })
        {
            string text = string.Concat(Enumerable.Repeat("ABCDEéééŽ🦆", size / 12 + 1)) + "tail'\\";
            byte[] bytes = Enumerable.Range(0, size).Select(i => (byte)((i * 73 + 19) & 255)).ToArray();
            using var cmd = Command("SELECT @t, @b, octet_length(@t), octet_length(@b)");
            cmd.Parameters.AddWithValue("t", NpgsqlDbType.Text, text);
            cmd.Parameters.AddWithValue("b", NpgsqlDbType.Bytea, bytes);
            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            check.True(await reader.ReadAsync(), "large row " + size);
            check.Equal(text, reader.GetString(0), "large Unicode text " + size);
            check.Equal(bytes, reader.GetFieldValue<byte[]>(1), "large bytea " + size);
            check.Equal(System.Text.Encoding.UTF8.GetByteCount(text), reader.GetInt32(2), "UTF-8 byte count " + size);
            check.Equal(size, reader.GetInt32(3), "bytea byte count " + size);
            check.False(await reader.ReadAsync(), "large result end " + size);
        }
        using var many = Command("SELECT i, repeat('Ž', 41) FROM generate_series(1, 2049) AS g(i)");
        await using var rows = await many.ExecuteReaderAsync();
        int count = 0;
        while (await rows.ReadAsync())
        {
            check.Equal(++count, rows.GetInt32(0), "fragmented many-row sequence");
            check.Equal(new string('Ž', 41), rows.GetString(1), "fragmented many-row text");
        }
        check.Equal(2049, count, "fragmented many-row count");
    }

    async Task SequentialBytes()
    {
        byte[] expected = Enumerable.Range(0, 32771).Select(i => (byte)(i % 251)).ToArray();
        using var cmd = Command("SELECT @p, 'foo', @p, 'bar', @p, @p");
        cmd.Parameters.AddWithValue("p", expected);
        await using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
        {
            check.True(reader.Read(), "GetBytes row");
            byte[] actual = new byte[expected.Length];
            check.Equal(2L, reader.GetBytes(0, 0, actual, 0, 2), "GetBytes prefix length");
            check.Equal(expected[0], actual[0], "GetBytes prefix 0");
            check.Equal(expected[1], actual[1], "GetBytes prefix 1");
            check.Equal((long)expected.Length, reader.GetBytes(0, 0, null, 0, 0), "GetBytes length");
            check.Throws<InvalidOperationException>(() => reader.GetBytes(0, 0, actual, 4, 1), "no sequential seek back");
            check.Equal((long)expected.Length - 2, reader.GetBytes(0, 2, actual, 2, actual.Length - 2), "GetBytes remainder");
            check.Equal(expected, actual, "GetBytes full content");
            check.Equal("foo", reader.GetString(1), "field following bytea");
            reader.GetBytes(2, 0, actual, 0, 2);
            reader.GetBytes(4, 0, actual, 0, 2);
            check.Equal(1L, reader.GetBytes(4, expected.Length - 1, actual, 0, 2), "last byte clipping");
            check.Equal(expected[^1], actual[0], "last byte content");
            check.Throws<IndexOutOfRangeException>(() => reader.GetBytes(4, 0, actual, 0, actual.Length + 1), "output buffer bound");
            reader.GetBytes(5, 0, actual, 0, 2);
        }
        check.Equal<object?>(42, await Scalar("SELECT 42"), "early partial bytea reader disposal");
    }
}

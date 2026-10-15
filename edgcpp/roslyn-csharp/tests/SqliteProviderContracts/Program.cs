using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

internal static unsafe class ProviderContractChecks
{
    private const string WorkloadSha256 = "33b6ea1f6d7dd02833c339f3d0e4a6612a75449b1c2be1b6e9b710e216f80502";
    private const string BuildSha256 = "c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435";
    private const string LibrarySha256 = "39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Dictionary<string, int> Calls = new(StringComparer.Ordinal);
    private static CapturedDatabase* database;
    private static void* statement;
    private static void* allocation;

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
    private static void Called(string name) => Calls[name] = Calls.GetValueOrDefault(name) + 1;
    private static string Digest(string path)
    {
        using var input = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(input));
    }
    private static JsonObject Receipt(string path) => new()
    {
        ["path"] = Path.GetFullPath(path), ["bytes"] = new FileInfo(path).Length, ["sha256"] = Digest(path)
    };
    private static string Bits(double value) => unchecked((ulong)BitConverter.DoubleToInt64Bits(value)).ToString("x16", CultureInfo.InvariantCulture);
    private static long UnixNanoseconds() => checked((DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) * 100);
    private static void RequireContracts(JsonObject contracts, string message) =>
        Require(contracts.All(pair => pair.Value!.GetValue<bool>()), message);

    private static void Clock(JsonElement work, JsonArray cases, JsonObject report)
    {
        var filename = Encoding.UTF8.GetBytes(":memory:\0");
        CapturedDatabase* opened = null;
        int openCode;
        fixed (byte* name = filename)
        {
            Called("sqlite3_open_v2"); openCode = Api.sqlite3_open_v2((sbyte*)name, &opened, 6, null);
        }
        database = opened; Require(openCode == 0 && database != null, "Time-query open failed.");
        var sql = work.GetProperty("sql").GetString()!;
        Require(!sql.Contains('\0'), "Embedded NUL SQL.");
        var encoded = Encoding.UTF8.GetBytes(sql + "\0");
        void* prepared = null; sbyte* tail = null;
        int prepareCode;
        fixed (byte* bytes = encoded)
        {
            Called("sqlite3_prepare_v2"); prepareCode = Api.sqlite3_prepare_v2(database, (sbyte*)bytes, encoded.Length, &prepared, &tail);
            statement = prepared;
            Require(prepareCode == 0 && statement != null && (byte*)tail == bytes + encoded.Length - 1, "Exact one-statement prepare failed.");
        }
        // The only SQLite operation between endpoint realtime samples is ROW step.
        // Endpoint order is audit data, not an assumption of wall-clock monotonicity.
        var before = UnixNanoseconds();
        Called("sqlite3_step"); var rowCode = Api.sqlite3_step(statement);
        var after = UnixNanoseconds();
        Called("sqlite3_column_count"); var columnCount = Api.sqlite3_column_count(statement);
        Require(rowCode == 100 && columnCount == 2, "Expected exactly two time columns.");
        Called("sqlite3_column_type"); var epochKind = Api.sqlite3_column_type(statement, 0);
        Called("sqlite3_column_type"); var julianKind = Api.sqlite3_column_type(statement, 1);
        Called("sqlite3_column_int64"); var epoch = Api.sqlite3_column_int64(statement, 0);
        Called("sqlite3_column_double"); var julian = Api.sqlite3_column_double(statement, 1);
        var tolerance = work.GetProperty("toleranceSeconds").GetDouble();
        var low = Math.Min(before, after) / 1e9; var high = Math.Max(before, after) / 1e9;
        var julianEpoch = (julian - work.GetProperty("unixEpochJulianDay").GetDouble()) * work.GetProperty("secondsPerDay").GetDouble();
        var contracts = new JsonObject
        {
            ["unixepochInteger"] = epochKind == 1,
            ["juliandayRealFinite"] = julianKind == 2 && double.IsFinite(julian),
            ["unixepochWithinHostWindow"] = Math.Floor(low - tolerance) <= epoch && epoch <= Math.Floor(high + tolerance),
            ["juliandayWithinHostWindow"] = double.IsFinite(julianEpoch) && low - tolerance <= julianEpoch && julianEpoch <= high + tolerance,
            ["statementTimeConsistent"] = double.IsFinite(julianEpoch) && epoch - tolerance <= julianEpoch && julianEpoch < epoch + 1 + tolerance
        };
        Called("sqlite3_step"); var done = Api.sqlite3_step(statement);
        var owned = statement; statement = null;
        Called("sqlite3_finalize"); var finished = Api.sqlite3_finalize(owned);
        Called("sqlite3_close"); var closed = Api.sqlite3_close(database);
        if (closed == 0) database = null;
        cases.Add((JsonNode)new JsonObject { ["name"] = "wall-clock-window", ["openCode"] = openCode, ["prepareCode"] = prepareCode,
            ["rowStepCode"] = rowCode, ["doneStepCode"] = done, ["finalizeCode"] = finished, ["closeCode"] = closed, ["contracts"] = contracts });
        report["timeAudit"] = new JsonObject
        {
            ["hostBeforeUnixNanoseconds"] = before.ToString(CultureInfo.InvariantCulture),
            ["hostAfterUnixNanoseconds"] = after.ToString(CultureInfo.InvariantCulture), ["hostClockWentBackwards"] = after < before,
            ["unixepochDecimal"] = epoch.ToString(CultureInfo.InvariantCulture), ["juliandayBinary64Hex"] = Bits(julian),
            ["juliandayAsUnixSecondsBinary64Hex"] = Bits(julianEpoch), ["toleranceSeconds"] = tolerance,
            ["bounds"] = "minimum/maximum endpoint samples; no monotonicity assumption"
        };
        Require(done == 101 && finished == 0 && closed == 0, "Time-query completion/close failed.");
        RequireContracts(contracts, "Finite wall-clock contract failed.");
    }

    private static void Randomness(JsonElement work, JsonArray cases, JsonObject report)
    {
        var count = work.GetProperty("requestedBytes").GetInt32(); var guard = work.GetProperty("guardBytes").GetInt32();
        Require(count == 64 && guard == 16, "Unexpected guarded randomness extent.");
        var prefix = work.GetProperty("prefixByte").GetByte(); var suffix = work.GetProperty("suffixByte").GetByte();
        var initial = work.GetProperty("initialInteriorByte").GetByte();
        var storage = (byte*)NativeMemory.Alloc((nuint)(count + 2 * guard));
        if (storage == null) throw new OutOfMemoryException();
        try
        {
            new Span<byte>(storage, guard).Fill(prefix);
            new Span<byte>(storage + guard, count).Fill(initial);
            new Span<byte>(storage + guard + count, guard).Fill(suffix);
            Called("sqlite3_randomness"); Api.sqlite3_randomness(count, storage + guard);
            var observed = new ReadOnlySpan<byte>(storage + guard, count).ToArray();
            var prefixEqual = true; var suffixEqual = true; var unchangedInterior = true;
            for (var i = 0; i < guard; ++i) { prefixEqual &= storage[i] == prefix; suffixEqual &= storage[guard + count + i] == suffix; }
            foreach (var value in observed) unchangedInterior &= value == initial;
            var contracts = new JsonObject { ["callCompleted"] = true, ["prefixGuardUnchanged"] = prefixEqual,
                ["suffixGuardUnchanged"] = suffixEqual, ["observedExtentMatchesRequest"] = observed.Length == count };
            cases.Add((JsonNode)new JsonObject { ["name"] = "guarded-randomness", ["requestedBytes"] = count, ["contracts"] = contracts });
            report["randomnessAudit"] = new JsonObject { ["bytes"] = observed.Length, ["hex"] = Convert.ToHexStringLower(observed),
                ["differentFromInitialInterior"] = !unchangedInterior, ["scope"] = "No output vector is forbidden; no statistical quality or security claim." };
            RequireContracts(contracts, "Randomness buffer guard/extent contract failed.");
        }
        finally { NativeMemory.Free(storage); }
    }

    private static void Allocation(JsonElement work, JsonArray cases, JsonObject report)
    {
        var first = work.GetProperty("initialBytes").GetInt32(); var grown = work.GetProperty("grownBytes").GetInt32();
        Require(first == 32 && grown == 96, "Unexpected allocation extents.");
        var prefix = new byte[first]; var tail = new byte[grown - first];
        for (var i = 0; i < prefix.Length; ++i) prefix[i] = unchecked((byte)(i * work.GetProperty("prefixMultiplier").GetInt32() + work.GetProperty("prefixAddend").GetInt32()));
        for (var i = 0; i < tail.Length; ++i) tail[i] = unchecked((byte)(i * work.GetProperty("tailMultiplier").GetInt32() + work.GetProperty("tailAddend").GetInt32()));
        Called("sqlite3_malloc64"); allocation = Api.sqlite3_malloc64((ulong)first);
        Require(allocation != null, "malloc64 refused bounded allocation.");
        var initialAddress = (nuint)allocation;
        Called("sqlite3_msize"); var initialSize = Api.sqlite3_msize(allocation);
        Require(initialSize >= (ulong)first, "Initial msize is below the requested extent.");
        prefix.CopyTo(new Span<byte>(allocation, first));
        var initialRoundtrip = new ReadOnlySpan<byte>(allocation, first).SequenceEqual(prefix);
        Called("sqlite3_realloc64"); var replacement = Api.sqlite3_realloc64(allocation, (ulong)grown);
        Require(replacement != null, "realloc64 refused bounded growth; original allocation retained for cleanup.");
        allocation = replacement; // The prior pointer is invalid after successful growth.
        Called("sqlite3_msize"); var grownSize = Api.sqlite3_msize(allocation);
        Require(grownSize >= (ulong)grown, "Grown msize is below the requested extent.");
        var preserved = new ReadOnlySpan<byte>(allocation, first).SequenceEqual(prefix);
        tail.CopyTo(new Span<byte>((byte*)allocation + first, grown - first));
        var tailRoundtrip = new ReadOnlySpan<byte>((byte*)allocation + first, grown - first).SequenceEqual(tail);
        var address = (nuint)allocation; var owned = allocation; allocation = null;
        Called("sqlite3_free"); Api.sqlite3_free(owned); // Ownership cleared before the one free.
        var contracts = new JsonObject { ["allocated"] = true, ["initialMsizeAtLeastRequested"] = initialSize >= (ulong)first,
            ["initialWriteReadEqual"] = initialRoundtrip, ["grownMsizeAtLeastRequested"] = grownSize >= (ulong)grown,
            ["prefixPreservedAfterGrowth"] = preserved, ["newTailWriteReadEqual"] = tailRoundtrip, ["freedExactlyOnce"] = true };
        cases.Add((JsonNode)new JsonObject { ["name"] = "allocation-growth-preserves-bytes", ["initialBytes"] = first, ["grownBytes"] = grown, ["contracts"] = contracts });
        report["allocationAudit"] = new JsonObject { ["initialAddressHex"] = initialAddress.ToString("x", CultureInfo.InvariantCulture),
            ["grownAddressHex"] = address.ToString("x", CultureInfo.InvariantCulture), ["addressChanged"] = initialAddress != address,
            ["initialMsize"] = initialSize, ["grownMsize"] = grownSize,
            ["prefixHex"] = Convert.ToHexStringLower(prefix), ["newTailHex"] = Convert.ToHexStringLower(tail) };
        RequireContracts(contracts, "Allocation preservation/size contract failed.");
    }

    private static JsonArray Libraries()
    {
        var paths = File.ReadLines("/proc/self/maps").Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(parts => parts.Length >= 6 && parts[^1].StartsWith('/') && parts[^1].Contains(".so", StringComparison.Ordinal))
            .Select(parts => parts[^1]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        Require(!paths.Any(path => Path.GetFileName(path).Contains("sqlite", StringComparison.OrdinalIgnoreCase)), "Generated execution loaded a native SQLite application library.");
        var values = new JsonArray(); foreach (var path in paths) values.Add((JsonNode)Receipt(path)); return values;
    }

    private static int Main(string[] args)
    {
        if (args.Length != 3) throw new ArgumentException("Checks WORKLOAD_JSON NEW_ARTIFACT_DIRECTORY SEPARATE_NATIVE_REPORT");
        var folder = Path.GetFullPath(args[1]); Require(!Directory.Exists(folder) && !File.Exists(folder), "Choose fresh artifacts."); Directory.CreateDirectory(folder);
        var cases = new JsonArray(); var frozen = new Dictionary<string, string>(StringComparer.Ordinal);
        var report = new JsonObject { ["status"] = "incomplete", ["role"] = "generated-csharp-application-only", ["generatedExecution"] = true,
            ["nativeApplicationFallbackPermitted"] = false, ["cases"] = cases, ["connectionsClosed"] = false,
            ["scope"] = "Three finite API contracts; selected provider exports are not invocation evidence." };
        try
        {
            Require(Digest(args[0]) == WorkloadSha256, "The frozen finite workload changed.");
            using var document = JsonDocument.Parse(File.ReadAllText(args[0])); var work = document.RootElement;
            Require(work.GetProperty("workloadVersion").GetInt32() == 1, "Unsupported workload version.");
            report["workload"] = Receipt(args[0]); report["remainingScope"] = JsonNode.Parse(work.GetProperty("remainingScope").GetRawText()); frozen[args[0]] = WorkloadSha256;
            var native = JsonNode.Parse(File.ReadAllText(args[2]))!;
            Require(native["status"]!.GetValue<string>() == "passed-native-provider-contracts" && native["role"]!.GetValue<string>() == "separate-native-test-oracle-only" &&
                native["connectionsClosed"]!.GetValue<bool>() && native["allocationReleased"]!.GetValue<bool>() && native["pinnedFilesUnchanged"]!.GetValue<bool>() &&
                native["workload"]!["sha256"]!.GetValue<string>() == WorkloadSha256 && native["buildReceipt"]!["sha256"]!.GetValue<string>() == BuildSha256 &&
                native["library"]!["sha256"]!.GetValue<string>() == LibrarySha256 && native["sqliteVersionNumber"]!.GetValue<int>() == 3054000,
                "Use completed exact pinned native provider-contract observations.");
            report["nativeObservationReceipt"] = Receipt(args[2]); frozen[args[2]] = Digest(args[2]);
            _ = Libraries(); Called("sqlite3_libversion_number"); report["sqliteVersionNumber"] = Api.sqlite3_libversion_number();
            Require(report["sqliteVersionNumber"]!.GetValue<int>() == work.GetProperty("sqliteVersionNumber").GetInt32(), "Expected pinned generated SQLite3.54.0.");
            Called("sqlite3_initialize"); report["initializationCode"] = Api.sqlite3_initialize();
            Require(report["initializationCode"]!.GetValue<int>() == 0, "Initialization failed.");
            Clock(work.GetProperty("time"), cases, report); Randomness(work.GetProperty("randomness"), cases, report); Allocation(work.GetProperty("allocation"), cases, report);
            Require(JsonNode.DeepEquals(cases, native["cases"]), "Finite native/generated contract observations differ; nondeterministic values are separate audits.");
            report["nativeComparison"] = "exact-finite-contracts-equal-nondeterministic-audits-separate";
            report["loadedLibraries"] = Libraries(); report["status"] = "passed-generated-provider-contracts";
        }
        catch (Exception problem) { report["error"] = problem.ToString(); report["status"] = "incomplete"; }
        finally
        {
            var cleanup = new JsonArray();
            if (allocation != null)
            {
                var owned = allocation; allocation = null; Called("sqlite3_free"); Api.sqlite3_free(owned);
                cleanup.Add((JsonNode)new JsonObject { ["allocationFreed"] = true });
            }
            if (statement != null)
            {
                var owned = statement; statement = null; Called("sqlite3_finalize"); var code = Api.sqlite3_finalize(owned);
                cleanup.Add((JsonNode)new JsonObject { ["finalizeCode"] = code });
            }
            if (database != null)
            {
                Called("sqlite3_close"); var code = Api.sqlite3_close(database); if (code == 0) database = null;
                cleanup.Add((JsonNode)new JsonObject { ["closeCode"] = code });
            }
            if (cleanup.Count != 0) report["cleanup"] = cleanup;
            var called = new JsonObject(); foreach (var pair in Calls.OrderBy(pair => pair.Key, StringComparer.Ordinal)) called[pair.Key] = pair.Value;
            report["directApiInvocations"] = called; report["connectionsClosed"] = database == null; report["allocationReleased"] = allocation == null;
            report["pinnedFilesUnchanged"] = frozen.All(pair => File.Exists(pair.Key) && Digest(pair.Key) == pair.Value);
            if (database != null || allocation != null || !report["pinnedFilesUnchanged"]!.GetValue<bool>()) report["status"] = "incomplete";
            using var writer = new StreamWriter(new FileStream(Path.Combine(folder, "provider-contracts-report.json"), FileMode.CreateNew, FileAccess.Write));
            writer.Write(report.ToJsonString(JsonOptions) + "\n");
        }
        Console.WriteLine(new JsonObject { ["status"] = report["status"]!.GetValue<string>(), ["cases"] = cases.Count }.ToJsonString());
        return report["status"]!.GetValue<string>() == "passed-generated-provider-contracts" ? 0 : 1;
    }
}

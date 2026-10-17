using System.Security.Cryptography;
using System.Text.Json.Nodes;

return SelectedConstantStaticChecks.Run(args);

internal static unsafe class SelectedConstantStaticChecks
{
    private const string SourceInputHash = "f0b4b51c0ca42bf8ac5a0a3da168e07022f206caa70ff410e8cca7971d729621";
    private const string ProviderInputHash = "8f7fc060b41ce984af43cfeffd02c5a958240fce13aa0e086080001297cffa3b";
    private const string SourceFactsHash = "6189087b70aa80bd849972ebc27560123337956f8f4a459c0a586bd1e88432bc";
    private const string ProviderFactsHash = "cfac1c448fb97c3b317507b28975cbd8505471a65187622bcde805ebd750135f";
    private const string NativeHash = "1c4616fb4828374acd9c0f91ef8d400b48b1d7ef55ce53ca75fcfc85a4f201b7";
    private const string OriginalPreparationHash = "d77e3d883b012ef63b684493363b6fd7016fb774dc28f765d34fd14ffb3fcda1";
    private static readonly string[] SourceHashes =
    [
        "beaa6ea525e68da17223509e939ae3a4e5f673d091f6ab4770d6dcd7c2493101",
        "7f0b80e97c2e088262b3bea6bd3146649cbf57b6d86433c04b21ef2ec9ba1c18",
        "829466f4b587eb31ac35ab578f0e1750f1e925c3f40a498f515f5927591ec3ce",
        "441e296457792fac82be7f0e2894efd4ee35a33323932846d0dbcee7eabecc79"
    ];

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2)
            throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON PREPARATION_PROVENANCE_JSON");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0])) ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1])) ?? throw new InvalidDataException("Missing capture provenance.");
        Check(Hash(arguments[0]) == NativeHash && Text(provenance, "native", "sha256") == NativeHash &&
            Hashes(provenance, "sources").SetEquals(SourceHashes) &&
            Hashes(provenance, "inputs").SetEquals([SourceInputHash, ProviderInputHash]) &&
            Hashes(provenance, "facts").SetEquals([SourceFactsHash, ProviderFactsHash]) &&
            provenance["sourceUnchanged"]?.GetValue<bool>() == true &&
            provenance["valueCount"]?.GetValue<int>() == 64 && provenance["eventCapacity"]?.GetValue<int>() == 64,
            "The immutable source, input, facts or independent native observation identities differ.");
        Check(Text(provenance, "originalPreparation", "sha256") == OriginalPreparationHash &&
            Hash(Text(provenance, "originalPreparation", "path")) == OriginalPreparationHash,
            "The additive acceptance provenance does not retain the unchanged original preparation evidence.");
        Check(native["runs"] is JsonArray { Count: 2 }, "The native protocol requires two complete fixture observations.");

        var original = ReadAddresses();
        Check(original.SharedA != 0 && original.SharedB != 0 && original.DifferentA != 0 && original.DifferentB != 0,
            "An actual constant-array wrapper returned null.");
        var sharedBytes = Bytes(original.SharedA).ToArray();
        var differentBytes = Bytes(original.DifferentA).ToArray();
        var runs = new JsonArray();
        for (var run = 0; run < 2; ++run)
        {
            var result = CapturedBindings.Fixture();
            var count = CapturedBindings.Count();
            var overflow = CapturedBindings.Overflow();
            var live = CapturedBindings.Live();
            Check(count is >= 0 and <= 64 && overflow == 0 && live == 0,
                "The actual scalar/event protocol exceeded its bounds.");
            var values = new JsonArray();
            for (var index = 0; index < 64; ++index) values.Add((JsonNode)JsonValue.Create(CapturedBindings.Value(index))!);
            var events = new JsonArray();
            for (var index = 0; index < count; ++index) events.Add((JsonNode)JsonValue.Create(CapturedBindings.Event(index))!);
            var observation = new JsonObject
            {
                ["result"] = result, ["live"] = live, ["overflow"] = overflow, ["values"] = values, ["events"] = events
            };
            Check(JsonNode.DeepEquals(observation, native["runs"]![run]),
                "The complete generated 64-value/event observation differs from the independent native run.");
            Check(Bytes(original.SharedA).SequenceEqual(sharedBytes) && Bytes(original.SharedB).SequenceEqual(sharedBytes) &&
                Bytes(original.DifferentA).SequenceEqual(differentBytes) && Bytes(original.DifferentB).SequenceEqual(differentBytes),
                "A selected call changed a named constant array's actual bytes.");
            runs.Add((JsonNode)observation);
        }

        var repeated = ReadAddresses();
        var current = ReadAddresses();
        var sharedAcrossTus = original.SharedA == original.SharedB;
        var differentAcrossTus = original.DifferentA == original.DifferentB;
        var sharedStable = original.SharedA == repeated.SharedA && original.SharedB == repeated.SharedB &&
            repeated.SharedA == current.SharedA && repeated.SharedB == current.SharedB;
        var differentStable = original.DifferentA == repeated.DifferentA && original.DifferentB == repeated.DifferentB &&
            repeated.DifferentA == current.DifferentA && repeated.DifferentB == current.DifferentB;
        var namedArraysDistinct = original.SharedA != original.DifferentA && original.SharedB != original.DifferentB;
        var unchanged = Bytes(current.SharedA).SequenceEqual(sharedBytes) && Bytes(current.SharedB).SequenceEqual(sharedBytes) &&
            Bytes(current.DifferentA).SequenceEqual(differentBytes) && Bytes(current.DifferentB).SequenceEqual(differentBytes);
        var contentsEqual = Bytes(original.SharedA).SequenceEqual(Bytes(original.SharedB)) &&
            Bytes(original.SharedA).SequenceEqual(Bytes(original.DifferentA)) && Bytes(original.SharedA).SequenceEqual(Bytes(original.DifferentB));
        var terminated = Bytes(original.SharedA)[49] == 0 && Bytes(original.SharedB)[49] == 0 &&
            Bytes(original.DifferentA)[49] == 0 && Bytes(original.DifferentB)[49] == 0;
        Check(sharedAcrossTus && differentAcrossTus && sharedStable && differentStable && namedArraysDistinct &&
            unchanged && contentsEqual && terminated,
            "Actual cross-TU storage identity, distinct named storage, fresh contents or terminator differ.");
        var storage = new JsonObject
        {
            ["byteCount"] = 50, ["sharedAcrossTus"] = sharedAcrossTus, ["differentAcrossTus"] = differentAcrossTus,
            ["sharedStableAcrossCalls"] = sharedStable, ["differentStableAcrossCalls"] = differentStable,
            ["namedArraysDistinct"] = namedArraysDistinct, ["allBytesUnchanged"] = unchanged,
            ["allFourContentsEqual"] = contentsEqual, ["terminatedAt49"] = terminated,
            ["sharedBytes"] = JsonBytes(sharedBytes), ["differentBytes"] = JsonBytes(differentBytes)
        };
        Check(JsonNode.DeepEquals(storage, native["storage"]), "Complete actual native/generated constant storage observations differ.");
        var physical = CapturedBindings.Physical(original);
        var observations = new JsonObject { ["runs"] = runs, ["storage"] = storage };
        Check(JsonNode.DeepEquals(observations, native), "The entire independent native protocol differs.");
        Console.WriteLine(observations.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["selectedConstantStatic"] = new JsonObject
            {
                ["physical"] = physical, ["freshFullByteReads"] = true,
                ["sameNamedStorageSharedAcrossTus"] = sharedAcrossTus && differentAcrossTus,
                ["differentNamedStorageNotInterned"] = namedArraysDistinct,
                ["writesPerformedByChecker"] = false,
                ["sourceOdrValidationClaim"] = false,
                ["runtimeInitializationExecutionCountClaim"] = false,
                ["lifetimeLiveZeroMeaning"] = "scalar harness protocol only"
            }
        }.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["suppliedCaptureProvenance"] = new JsonObject
            {
                ["path"] = Path.GetFullPath(arguments[1]), ["sha256"] = Hash(arguments[1]),
                ["sourceInputSha256"] = SourceInputHash, ["providerInputSha256"] = ProviderInputHash,
                ["sourceFactsSha256"] = SourceFactsHash, ["providerFactsSha256"] = ProviderFactsHash,
                ["nativeObservationsSha256"] = NativeHash,
                ["originalPreparationSha256"] = OriginalPreparationHash,
                ["attribution"] = "supplied evidence; outer receipt binds both inputs, actual constant/storage representative mappings, GeneratedProject and executable"
            }
        }.ToJsonString());
        return 0;
    }

    private static ConstantAddresses ReadAddresses()
    {
        var addresses = new ConstantAddresses(CapturedBindings.SharedA(), CapturedBindings.SharedB(),
            CapturedBindings.DifferentA(), CapturedBindings.DifferentB());
        Check(addresses.SharedA != 0 && addresses.SharedB != 0 && addresses.DifferentA != 0 && addresses.DifferentB != 0,
            "An actual repeated constant-array wrapper returned null.");
        return addresses;
    }

    private static ReadOnlySpan<byte> Bytes(nint address) => new((void*)address, 50);

    private static JsonArray JsonBytes(ReadOnlySpan<byte> bytes)
    {
        var values = new JsonArray();
        foreach (var value in bytes) values.Add((JsonNode)JsonValue.Create((int)value)!);
        return values;
    }

    private static HashSet<string> Hashes(JsonNode value, string section)
    {
        var rows = value[section] as JsonArray ?? throw new InvalidDataException("Missing " + section + ".");
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
            Check(result.Add(row?["sha256"]?.GetValue<string>() ?? throw new InvalidDataException("Missing content hash.")),
                "Duplicate content identity in " + section + ".");
        return result;
    }

    private static string Text(JsonNode value, string section, string field) =>
        value[section]?[field]?.GetValue<string>() ?? throw new InvalidDataException("Missing " + section + "." + field);

    private static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

internal sealed record ConstantAddresses(nint SharedA, nint SharedB, nint DifferentA, nint DifferentB);

// Implemented against the inspected publication in CapturedBindings.cs.
internal static partial class CapturedBindings
{
    internal static partial int Fixture();
    internal static partial int Count();
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Value(int index);
    internal static partial int Event(int index);
    internal static partial nint SharedA();
    internal static partial nint SharedB();
    internal static partial nint DifferentA();
    internal static partial nint DifferentB();
    internal static partial JsonObject Physical(ConstantAddresses addresses);
}

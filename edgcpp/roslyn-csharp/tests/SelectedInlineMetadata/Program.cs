using System.Security.Cryptography;
using System.Text.Json.Nodes;

return SelectedInlineMetadataChecks.Run(args);

internal static class SelectedInlineMetadataChecks
{
    private const string SourceInputHash = "562c062be0e1f0a82daf1e970f79ed817f836900c788d2e87dda79a230393c11";
    private const string ProviderInputHash = "a76827db7c5a84cb88d27306c1cfe97e7e354e827005d94a79e02f2b9d25f1d7";
    private const string SourceFactsHash = "c6f41182f6f0abebc2c4344c38a644992f09a75edf195f4325de16975df5038c";
    private const string ProviderFactsHash = "607dac7a75d2a337a216149a0d5241348be8a44c9928c3160ccc77e3632aa555";
    private const string NativeHash = "20dc7348d6e6b8a310ee4b492a648f68b108c2f308e6e2e68f8ab776917774df";
    private const string OriginalPreparationHash = "f9061d80936345d3fa0c8c9cc1d89d7891574c238c2243c7aaa82d575ef796c5";
    private static readonly string[] SourceHashes =
    [
        "6f9629198dff6f2d53206df94f61ccc5419306a06cc8d4b5f77df16c8aadea67",
        "0bdca001c077dd40568544cbe5c93c98841d16ea607f2810c5c0fee86a038fc7",
        "bd5c61850047a170f13907a7ba0b9a56bed9567313ac44794f42d1006dcde5c9",
        "72d1a0635c10dfe01b1df4458122816d0c91681e25823c5248729b0c29b57335"
    ];

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2)
            throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON ACCEPTANCE_PROVENANCE_JSON");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0])) ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1])) ?? throw new InvalidDataException("Missing capture provenance.");
        Check(Hash(arguments[0]) == NativeHash && Text(provenance, "native", "sha256") == NativeHash &&
            Hashes(provenance, "sources").SetEquals(SourceHashes) &&
            Hashes(provenance, "inputs").SetEquals([SourceInputHash, ProviderInputHash]) &&
            Hashes(provenance, "facts").SetEquals([SourceFactsHash, ProviderFactsHash]) &&
            Text(provenance, "originalPreparation", "sha256") == OriginalPreparationHash &&
            Hash(Text(provenance, "originalPreparation", "path")) == OriginalPreparationHash &&
            provenance["sourceUnchanged"]?.GetValue<bool>() == true && provenance["valueCount"]?.GetValue<int>() == 9 &&
            provenance["eventCapacity"]?.GetValue<int>() == 64,
            "The unchanged source, immutable input/facts/native observations or original preparation identities differ.");
        Check(native["runs"] is JsonArray { Count: 2 } && native["counterUnit"]?.GetValue<string>() == "Member",
            "The native protocol requires two full observations with Member counter units.");
        Check(CapturedBindings.Created() == 0 && CapturedBindings.Destroyed() == 0 && CapturedBindings.Live() == 0 &&
            CapturedBindings.ArgumentCount() == 0,
            "Automatic object construction occurred before the actual selected fixture calls.");

        var runs = new JsonArray();
        var lifetimes = new JsonArray();
        for (var run = 0; run < 2; ++run)
        {
            var beforeCreated = CapturedBindings.Created();
            var beforeDestroyed = CapturedBindings.Destroyed();
            var beforeLive = CapturedBindings.Live();
            var beforeArguments = CapturedBindings.ArgumentCount();
            var result = CapturedBindings.Fixture();
            var created = CapturedBindings.Created();
            var destroyed = CapturedBindings.Destroyed();
            var live = CapturedBindings.Live();
            var overflow = CapturedBindings.Overflow();
            var count = CapturedBindings.Count();
            var argumentsEvaluated = CapturedBindings.ArgumentCount() - beforeArguments;
            Check(count is >= 0 and <= 64 && overflow == 0 && beforeLive == 0 && live == 0 &&
                created - beforeCreated == 2 && destroyed - beforeDestroyed == 2 && created == destroyed &&
                CapturedBindings.Value(8) == beforeLive && argumentsEvaluated == 2,
                "The actual two automatic Member lifetimes did not finish once with the retained prior-live value.");
            var values = new JsonArray();
            for (var index = 0; index < 9; ++index) values.Add((JsonNode)JsonValue.Create(CapturedBindings.Value(index))!);
            var events = new JsonArray();
            for (var index = 0; index < count; ++index) events.Add((JsonNode)JsonValue.Create(CapturedBindings.Event(index))!);
            var observation = new JsonObject
            {
                ["result"] = result, ["created"] = created, ["destroyed"] = destroyed,
                ["live"] = live, ["overflow"] = overflow, ["values"] = values, ["events"] = events
            };
            Check(JsonNode.DeepEquals(observation, native["runs"]![run]),
                "The complete generated normal member-cleanup observation differs from native.");
            runs.Add((JsonNode)observation);
            lifetimes.Add((JsonNode)new JsonObject
            {
                ["createdMembers"] = created - beforeCreated, ["destroyedMembers"] = destroyed - beforeDestroyed,
                ["priorLiveMembers"] = beforeLive, ["liveMembersAfterReturn"] = live,
                ["constructorArgumentEvaluations"] = argumentsEvaluated,
                ["wholeHasSeparateLiveUnit"] = false,
                ["actualNormalCleanupTraceMatchesNative"] = true
            });
        }
        var beforePhysicalCount = CapturedBindings.Count();
        var physical = CapturedBindings.Physical();
        Check(CapturedBindings.Created() == 4 && CapturedBindings.Destroyed() == 4 && CapturedBindings.Live() == 0 &&
            CapturedBindings.ArgumentCount() == 4 && CapturedBindings.Count() == beforePhysicalCount,
            "Raw carrier measurements unexpectedly invoked an application lifetime or argument helper.");
        Check(JsonNode.DeepEquals(physical.NativeCompatible, native["physical"]),
            "Actual emitted source-TU carrier measurements differ from native.");
        var observations = new JsonObject
        {
            ["runs"] = runs, ["physical"] = physical.NativeCompatible, ["counterUnit"] = "Member"
        };
        Check(JsonNode.DeepEquals(observations, native), "The complete native JSON protocol differs.");
        Console.WriteLine(observations.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["selectedInlineMetadata"] = new JsonObject
            {
                ["memberLifetimes"] = lifetimes, ["physical"] = physical.Additional,
                ["fixtureCalls"] = 2, ["manualConstructorOrDestructorCalls"] = false,
                ["constructorArgumentCounterAfterRuns"] = CapturedBindings.ArgumentCount(),
                ["onceOnlyConstructorArgumentsMatchNativeTrace"] = true,
                ["globalWholeOrStartupLifecycleInvoked"] = false,
                ["exceptionPropagationExercised"] = false, ["sourceOdrValidationClaim"] = false,
                ["metadataClassificationAttribution"] = "outer receipt binds actual retained table rows and selected representative policy; table identities are not an execution schedule"
            }
        }.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["suppliedCaptureProvenance"] = new JsonObject
            {
                ["path"] = Path.GetFullPath(arguments[1]), ["sha256"] = Hash(arguments[1]),
                ["sourceInputSha256"] = SourceInputHash, ["providerInputSha256"] = ProviderInputHash,
                ["sourceFactsSha256"] = SourceFactsHash, ["providerFactsSha256"] = ProviderFactsHash,
                ["nativeObservationsSha256"] = NativeHash, ["originalPreparationSha256"] = OriginalPreparationHash,
                ["attribution"] = "supplied evidence; the outer receipt binds selected inputs, actual table classification, GeneratedProject and executable"
            }
        }.ToJsonString());
        return 0;
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

internal sealed record PhysicalObservation(JsonObject NativeCompatible, JsonObject Additional);

// Implemented against the actual publication in CapturedBindings.cs.
internal static partial class CapturedBindings
{
    internal static partial int Fixture();
    internal static partial int Count();
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Created();
    internal static partial int Destroyed();
    internal static partial int ArgumentCount();
    internal static partial int Value(int index);
    internal static partial int Event(int index);
    internal static partial PhysicalObservation Physical();
}

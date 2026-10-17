using System.Security.Cryptography;
using System.Text.Json.Nodes;

return SelectedSuppressedProviderChecks.Run(args);

internal static class SelectedSuppressedProviderChecks
{
    private const string NativeEvidenceSha256 = "1f4fe3ea6986f61fa2d4c93956b4850a3f4ea0267abb5745d3809dd4ee8f0356";
    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2)
            throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSONL EXPECTED_NATIVE_SHA256");
        var nativePath = Path.GetFullPath(arguments[0]);
        var expectedHash = arguments[1];
        Check(expectedHash.Length == 64 && expectedHash.All(character =>
            character is >= '0' and <= '9' or >= 'a' and <= 'f'), "Expected native SHA-256 must be explicit lowercase hexadecimal.");
        Check(expectedHash == NativeEvidenceSha256, "This checker requires the unchanged capture-2 native oracle.");
        Check(new FileInfo(nativePath).Length <= 1024 * 1024, "Native observation file exceeds one MiB.");
        var nativeBytes = File.ReadAllBytes(nativePath);
        var actualHash = Convert.ToHexStringLower(SHA256.HashData(nativeBytes));
        Check(actualHash == expectedHash, "Native observation bytes differ from the explicit evidence pin.");
        var lines = System.Text.Encoding.UTF8.GetString(nativeBytes).Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        Check(lines.Length == 2, "The native oracle must contain exactly two full JSON observations.");
        var native = lines.Select(line => JsonNode.Parse(line) as JsonObject ??
            throw new InvalidDataException("Native observation is not an object.")).ToArray();
        Check(CapturedBindings.ValueCount() == 9, "The selected fixture value protocol differs.");
        Check(CapturedBindings.Live() == 0 && CapturedBindings.Constructed() == 0 && CapturedBindings.Destroyed() == 0,
            "Automatic fixture construction occurred before the actual fixture calls.");

        var observations = new List<JsonObject>();
        var lifetimeObservations = new JsonArray();
        for (var run = 0; run < 2; ++run)
        {
            var priorLive = CapturedBindings.Live();
            var result = CapturedBindings.Fixture();
            var observation = Observe(result);
            Check(JsonNode.DeepEquals(observation, native[run]),
                "The complete generated observation differs from the pinned native oracle.");
            var constructed = CapturedBindings.Constructed();
            var destroyed = CapturedBindings.Destroyed();
            Check(priorLive == 0 && CapturedBindings.Value(8) == priorLive && CapturedBindings.Live() == 0 &&
                constructed > 0 && constructed == destroyed,
                "The actual automatic object scopes did not finish with balanced per-call counters.");
            observations.Add(observation);
            lifetimeObservations.Add((JsonNode)new JsonObject
            {
                ["invocation"] = run + 1, ["priorLive"] = priorLive,
                ["constructed"] = constructed, ["destroyed"] = destroyed,
                ["liveAfterReturn"] = CapturedBindings.Live(), ["nativeObservationEqual"] = true,
                ["counterScope"] = "fixture resets constructed/destroyed per invocation; live is preserved"
            });
        }

        var lastResult = observations[1]["result"]!.GetValue<int>();
        var beforePhysical = Observe(lastResult);
        var physical = CapturedBindings.Physical();
        Check(JsonNode.DeepEquals(beforePhysical, Observe(lastResult)),
            "Raw physical measurements changed application scalar/event storage or invoked an application lifetime.");
        foreach (var observation in observations) Console.WriteLine(observation.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["selectedSuppressedProvider"] = new JsonObject
            {
                ["nativeObservationsPath"] = nativePath, ["nativeObservationsSha256"] = actualHash,
                ["fixtureCalls"] = 2, ["lifetimeObservations"] = lifetimeObservations,
                ["physical"] = physical, ["manualConstructorOrDestructorCalls"] = false,
                ["sourceOdrOrBodyEquivalenceClaim"] = false,
                ["attribution"] = "The outer receipt binds the supplied native evidence, both captures, selected provider policy, GeneratedProject, Runtime and executable."
            }
        }.ToJsonString());
        return 0;
    }

    private static JsonObject Observe(int result)
    {
        Check(CapturedBindings.ValueCount() == 9, "The actual value count changed.");
        var count = CapturedBindings.Count();
        Check(count is >= 0 and <= 64 && CapturedBindings.Overflow() == 0,
            "The actual bounded event protocol overflowed.");
        var values = new JsonArray();
        for (var index = 0; index < 9; ++index)
            values.Add((JsonNode)JsonValue.Create(CapturedBindings.Value(index))!);
        var events = new JsonArray();
        for (var index = 0; index < count; ++index)
            events.Add((JsonNode)JsonValue.Create(CapturedBindings.Event(index))!);
        return new JsonObject
        {
            ["result"] = result, ["values"] = values, ["events"] = events, ["count"] = count,
            ["overflow"] = CapturedBindings.Overflow(), ["live"] = CapturedBindings.Live(),
            ["constructed"] = CapturedBindings.Constructed(), ["destroyed"] = CapturedBindings.Destroyed()
        };
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

// Implementations are bound to the actual publication in CapturedBindings.cs.
internal static partial class CapturedBindings
{
    internal static partial int Fixture();
    internal static partial int ValueCount();
    internal static partial int Value(int index);
    internal static partial int Count();
    internal static partial int Event(int index);
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Constructed();
    internal static partial int Destroyed();
    internal static partial JsonObject Physical();
}

using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;

return ClassConstructionChecks.Run(args);

internal static class ClassConstructionChecks
{
    private const string InputHash = "b7209c64bd66bc62e49aad1180435be847be61996103cdfc62e6b63c9ffdc13f";
    private const string SourceHash = "753329ca1be6745cbec2dfe1a1d8fcefc1fefd6aed7884a8b170bda68f96b395";
    private const string FactsHash = "2fcc8d7482fda42f39da6f155a6f43bc660cb63fc58defb6a1938a81a6697c79";
    private const string NativeHash = "b5647574c1cd84fc04df72925494e017c94e998bb034a069ab27a9ca9cffa051";

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2) throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0])) ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1])) ?? throw new InvalidDataException("Missing capture provenance.");
        Check(Hash(arguments[0]) == NativeHash && Text(provenance, "nativeObservations", "sha256") == NativeHash &&
            Text(provenance, "input", "sha256") == InputHash && Text(provenance, "source", "sha256") == SourceHash &&
            Text(provenance, "publicFacts", "sha256") == FactsHash,
            "The native observations, immutable capture or source/facts identities differ.");
        Check(provenance["sourceUnchanged"]?.GetValue<bool>() == true && provenance["probeVersion"]?.GetValue<int>() == 7,
            "The supplied capture source/probe evidence differs.");
        Check(JsonNode.DeepEquals(provenance["actualNativeObservations"], native), "Capture provenance does not bind these exact native observations.");
        Check(native["runs"] is JsonArray { Count: 2 }, "The native protocol requires two complete observations.");

        var runs = new JsonArray();
        var ownership = new JsonArray();
        for (var run = 0; run < 2; ++run)
        {
            var before = ManagedCppException.GetDiagnostics();
            var beforeConstruction = ManagedCppException.GetConstructionDiagnostics();
            var beforeRecord = ManagedCppException.GetRecordDiagnostics();
            var beforeReferences = ManagedCppException.GetRethrowDiagnostics();
            Check(before.LiveOwners == 0 && beforeConstruction.LiveOwners == 0 && beforeConstruction.PendingOwners == 0 &&
                beforeRecord.LiveOwners == 0 && beforeReferences.LiveReferences == 0, "The fixture entered with live exception resources.");

            var result = CapturedBindings.Fixture();

            var after = ManagedCppException.GetDiagnostics();
            var construction = ManagedCppException.GetConstructionDiagnostics();
            var record = ManagedCppException.GetRecordDiagnostics();
            var references = ManagedCppException.GetRethrowDiagnostics();
            var count = CapturedBindings.Count();
            var overflow = CapturedBindings.Overflow();
            var live = CapturedBindings.Live();
            Check(count is >= 0 and <= 64 && overflow == 0 && live == 0 && CapturedBindings.Value(8) == 0,
                "The actual generated event/live/overflow/prior-live protocol failed.");
            var values = new JsonArray();
            for (var index = 0; index < 9; ++index) values.Add((JsonNode)JsonValue.Create(CapturedBindings.Value(index))!);
            var events = new JsonArray();
            for (var index = 0; index < count; ++index) events.Add((JsonNode)JsonValue.Create(CapturedBindings.Event(index))!);
            var observation = new JsonObject { ["result"] = result, ["overflow"] = overflow, ["live"] = live,
                ["values"] = values, ["events"] = events };
            Check(JsonNode.DeepEquals(observation, native["runs"]![run]), "Generated construction observations differ from the independent native run.");
            runs.Add((JsonNode)observation);

            Check(after.Created - before.Created == 3 && after.Committed - before.Committed == 2 &&
                after.Selected - before.Selected == 2 && after.Caught - before.Caught == 2 &&
                after.Released - before.Released == 3 && after.PendingReleased - before.PendingReleased == 1 &&
                after.LiveOwners == 0 && after.ProtocolRejections == before.ProtocolRejections,
                "Primary ownership did not balance two completed payloads and one failed pending allocation.");
            Check(construction.Created - beforeConstruction.Created == 3 && construction.AddressRequested - beforeConstruction.AddressRequested == 3 &&
                construction.Committed - beforeConstruction.Committed == 2 && construction.Selected - beforeConstruction.Selected == 2 &&
                construction.Caught - beforeConstruction.Caught == 2 && construction.PendingReleased - beforeConstruction.PendingReleased == 1 &&
                construction.DestructorStarted - beforeConstruction.DestructorStarted == 2 && construction.DestructorCompleted - beforeConstruction.DestructorCompleted == 2 &&
                construction.Released - beforeConstruction.Released == 3 && construction.LiveOwners == 0 && construction.PendingOwners == 0 &&
                construction.PeakPendingOwners == 2 && construction.LastPayloadSize == 16 && construction.LastPayloadAlignment == 8 && construction.LastPayloadWasAligned,
                "Direct-construction resource or complete 16/8 storage diagnostics differ.");
            Check(references.Rethrown == beforeReferences.Rethrown && references.HandlingEntriesReleased - beforeReferences.HandlingEntriesReleased == 2 &&
                references.LiveReferences == 0, "The construction profile introduced a rethrow or leaked handling references.");
            Check(record.Created == beforeRecord.Created && record.EvaluationCompleted == beforeRecord.EvaluationCompleted &&
                record.Committed == beforeRecord.Committed && record.DestructorStarted == beforeRecord.DestructorStarted &&
                record.DestructorCompleted == beforeRecord.DestructorCompleted && record.Released == beforeRecord.Released && record.LiveOwners == 0,
                "The no-copy construction profile unexpectedly used copy-owner lifecycle operations.");

            var completedOwner = construction.LastCommittedOwner;
            var failedOwner = construction.LastPendingReleasedOwner;
            Check(completedOwner > 0 && completedOwner != beforeConstruction.LastCommittedOwner &&
                completedOwner == construction.LastCreatedOwner && completedOwner == construction.LastDestructorOwner &&
                completedOwner == construction.LastReleasedOwner && completedOwner == references.LastCreatedPrimaryOwner &&
                completedOwner == references.LastReleasedPrimaryOwner && failedOwner > 0 && failedOwner != completedOwner &&
                failedOwner != beforeConstruction.LastPendingReleasedOwner,
                "The latest completed owner and failed pending owner identities are not distinct and balanced.");
            var assertions = after.CleanupAssertions - before.CleanupAssertions;
            Check(assertions > 0 && after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions == assertions,
                "Not every encoded exceptional cleanup observed first-pass selection.");
            Check(construction.LastCreatedSequence > beforeConstruction.LastReleasedSequence &&
                construction.LastAddressSequence > construction.LastCreatedSequence &&
                construction.LastCommittedSequence > construction.LastAddressSequence &&
                construction.LastSelectedSequence > construction.LastCommittedSequence &&
                construction.LastFirstCleanupSequence > construction.LastSelectedSequence &&
                construction.LastPendingReleasedSequence > construction.LastFirstCleanupSequence &&
                construction.LastDestructorStartedSequence > construction.LastPendingReleasedSequence &&
                construction.LastDestructorCompletedSequence > construction.LastDestructorStartedSequence &&
                construction.LastReleasedSequence > construction.LastDestructorCompletedSequence &&
                after.LastSelectedSequence == construction.LastSelectedSequence &&
                after.LastFirstCleanupSequence == construction.LastFirstCleanupSequence &&
                after.LastReleasedSequence == construction.LastReleasedSequence,
                "Latest completed-owner selection/cleanup/destruction/free and failed-pending retirement were not observed in order.");

            ownership.Add((JsonNode)new JsonObject
            {
                ["created"] = construction.Created - beforeConstruction.Created,
                ["addressRequested"] = construction.AddressRequested - beforeConstruction.AddressRequested,
                ["committed"] = construction.Committed - beforeConstruction.Committed,
                ["selected"] = construction.Selected - beforeConstruction.Selected,
                ["caught"] = construction.Caught - beforeConstruction.Caught,
                ["pendingReleased"] = construction.PendingReleased - beforeConstruction.PendingReleased,
                ["destructorStarted"] = construction.DestructorStarted - beforeConstruction.DestructorStarted,
                ["destructorCompleted"] = construction.DestructorCompleted - beforeConstruction.DestructorCompleted,
                ["released"] = construction.Released - beforeConstruction.Released,
                ["handlingEntriesReleased"] = references.HandlingEntriesReleased - beforeReferences.HandlingEntriesReleased,
                ["liveOwners"] = construction.LiveOwners, ["pendingOwners"] = construction.PendingOwners,
                ["liveReferences"] = references.LiveReferences, ["processPeakPendingOwners"] = construction.PeakPendingOwners,
                ["copyOwnerOperationsObserved"] = false, ["latestCompletedOwner"] = completedOwner, ["failedPendingOwner"] = failedOwner,
                ["payloadSize"] = construction.LastPayloadSize, ["payloadAlignment"] = construction.LastPayloadAlignment,
                ["payloadWasAligned"] = construction.LastPayloadWasAligned,
                ["cleanupAssertions"] = assertions,
                ["selectionBeforeCleanupAssertions"] = after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions,
                ["protocolRejections"] = after.ProtocolRejections - before.ProtocolRejections,
                ["sequenceScope"] = "Last* fields describe the latest completed owner plus the latest failed pending owner; aggregate counters are per invocation, not a per-owner event history.",
                ["latestCreatedSequence"] = construction.LastCreatedSequence,
                ["latestAddressSequence"] = construction.LastAddressSequence,
                ["latestCommittedSequence"] = construction.LastCommittedSequence,
                ["latestSelectedSequence"] = construction.LastSelectedSequence,
                ["latestFirstCleanupSequence"] = construction.LastFirstCleanupSequence,
                ["failedPendingReleasedSequence"] = construction.LastPendingReleasedSequence,
                ["latestDestructorStartedSequence"] = construction.LastDestructorStartedSequence,
                ["latestDestructorCompletedSequence"] = construction.LastDestructorCompletedSequence,
                ["latestReleasedSequence"] = construction.LastReleasedSequence
            });
        }
        Check(JsonNode.DeepEquals(runs[0], runs[1]), "Repeated generated construction observations differ.");
        var observations = new JsonObject { ["runs"] = runs };
        Check(JsonNode.DeepEquals(observations, native), "Full generated/native construction protocol differs.");
        var physical = CapturedBindings.MeasurePhysical();
        Check(Int(physical, "completePayloadSize") == 16 && Int(physical, "canaryOffset") == 0 && Int(physical, "valueOffset") == 8 &&
            Int(physical, "sequentialPrefixOffset") == 8 && Int(physical, "elementStride") == 16,
            "The published complete payload layout differs from the retained 16/8 record.");
        Console.WriteLine(observations.ToJsonString());
        Console.WriteLine(new JsonObject { ["classConstructionOwnership"] = ownership }.ToJsonString());
        Console.WriteLine(new JsonObject { ["classConstructionPhysical"] = physical }.ToJsonString());
        Console.WriteLine(new JsonObject { ["suppliedCaptureProvenance"] = new JsonObject
        {
            ["inputSha256"] = InputHash, ["sourceSha256"] = SourceHash, ["publicFactsSha256"] = FactsHash,
            ["nativeObservationsSha256"] = NativeHash, ["captureProvenanceSha256"] = Hash(arguments[1]),
            ["compiledProjectAttribution"] = "Outer receipt binds actual generated project, completed source/build and checker binary; supplied hashes alone do not identify compiled code."
        } }.ToJsonString());
        return 0;
    }
    private static int Int(JsonObject value, string name) => value[name]?.GetValue<int>() ?? throw new InvalidDataException("Missing physical fact " + name);
    private static string? Text(JsonNode value, string owner, string field) => value[owner]?[field]?.GetValue<string>();
    private static string Hash(string path) { using var file = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(file)); }
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}

// Names are bound to the completed class-construction-cli-generated-2 publication.
internal static unsafe class CapturedBindings
{
    internal static int Fixture() => global::Translated.Program.fixture();
    internal static int Count() => global::Translated.Program.lifetime_count();
    internal static int Overflow() => global::Translated.Program.lifetime_overflow();
    internal static int Live() => global::Translated.Program.lifetime_live();
    internal static int Event(int index) => global::Translated.Program.lifetime_event(index);
    internal static int Value(int index) => global::Translated.Program.lifetime_value(index);

    [StructLayout(LayoutKind.Sequential)]
    private struct PayloadPrefix
    {
        public byte Prefix;
        public global::Translated.ConstructedPayload__2 Payload;
    }

    internal static JsonObject MeasurePhysical()
    {
        global::Translated.ConstructedPayload__2* payloads = stackalloc global::Translated.ConstructedPayload__2[2];
        PayloadPrefix prefix = default;
        var start = (byte*)payloads;
        var size = sizeof(global::Translated.ConstructedPayload__2);
        var canaryOffset = checked((int)((byte*)(&payloads[0].canary) - start));
        var valueOffset = checked((int)((byte*)(&payloads[0].value) - start));
        var prefixOffset = checked((int)((byte*)(&prefix.Payload) - (byte*)(&prefix)));
        var stride = checked((int)((byte*)(payloads + 1) - start));
        if (((nuint)payloads) % 8 != 0 || ((nuint)(payloads + 1)) % 8 != 0 ||
            ((nuint)(&prefix.Payload)) % 8 != 0)
            throw new InvalidOperationException("Actual complete payload storage is not naturally 8-byte aligned.");
        return new JsonObject
        {
            ["completePayloadSize"] = size, ["canaryOffset"] = canaryOffset,
            ["valueOffset"] = valueOffset, ["sequentialPrefixOffset"] = prefixOffset,
            ["elementStride"] = stride, ["actualPayloadAddressesAligned"] = true
        };
    }
}

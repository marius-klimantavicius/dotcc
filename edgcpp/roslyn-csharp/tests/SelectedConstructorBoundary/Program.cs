using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;

return SelectedConstructorBoundaryChecks.Run(args);

internal static class SelectedConstructorBoundaryChecks
{
    private const string ProviderInputHash = "1f56f6f6e9dce3d1eabd25137ecfbaefc54e9f9dbe5a6e631a7162aa9385ca33";
    private const string SourceInputHash = "4825d6f3e6fb2e6165f7400fed46682fe34e5aa1fa516af442c372bf71f637d9";
    private const string ProviderFactsHash = "83228eb5882c6626ad494997c982629bf321f7de0c2cdaa980c3568c00d35a1b";
    private const string SourceFactsHash = "69735836670470dfd8eb6c4e83e1458f919b6f16efce9ccaa6ac4398b7e46dd4";
    private const string NativeHash = "db93f0ef2b94acb9a04aa3207991281985fb908cb613a426363426d4a3f17589";
    private static readonly string[] SourceHashes =
    [
        "1e22355983cae4fa62f578a7cead99aa3c41d8bb695c7649e5f9907aae420182",
        "5ccf9ec2270b9514584a694419ab32c1cf0a20aab639e9519b4c2cf43b9b97d0",
        "48e5f2b6507fe24809b89f175f470e3049319a079feafcf654963b70db935b2d",
        "8ed34815a5d47f8c8a39eeaedecd24e3805ae2f290c2d47fc4fd9bd12798bf90"
    ];

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2)
            throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON PREPARATION_PROVENANCE_JSON");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0])) ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1])) ?? throw new InvalidDataException("Missing preparation provenance.");
        Check(Hash(arguments[0]) == NativeHash && Text(provenance, "native", "sha256") == NativeHash,
            "The immutable independent native observation identity differs.");
        Check(Hashes(provenance, "sources").SetEquals(SourceHashes) && Hashes(provenance, "sources").Count == 4 &&
            Hashes(provenance, "inputs").SetEquals([ProviderInputHash, SourceInputHash]) &&
            Hashes(provenance, "facts").SetEquals([ProviderFactsHash, SourceFactsHash]) &&
            provenance["sourceUnchanged"]?.GetValue<bool>() == true && provenance["valueCount"]?.GetValue<int>() == 9 &&
            provenance["eventCapacity"]?.GetValue<int>() == 64,
            "The two captures, their facts or the unchanged source protocol identities differ.");

        var before = CapturedBindings.Diagnostics();
        Check(before.State == ManagedSelectedProgramLifetimeState.New && before.InitializeCalls == 0 &&
            before.ProtocolRejections == 0 && before.PendingEntries == 0 && CapturedBindings.ProviderSerialCounter() == 0,
            "The generated explicit program lifetime was not fresh.");
        RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState.New);

        CapturedBindings.Initialize();
        var ready = CapturedBindings.Diagnostics();
        CheckReady(ready, 1);
        var startup = ObserveReady(null);
        Check(JsonNode.DeepEquals(startup, native["startup"]), "Generated startup storage differs from native.");
        var objects = CapturedBindings.SnapshotObjects();

        CapturedBindings.Initialize();
        var repeatedReady = CapturedBindings.Diagnostics();
        CheckReady(repeatedReady, 2);
        Check(repeatedReady == (ready with { InitializeCalls = 2 }) &&
            JsonNode.DeepEquals(ObserveReady(null), startup) && CapturedBindings.ObjectsUnchanged(objects),
            "Repeated initialize changed the actual constructor objects, protocol or lifetime state.");

        var first = ObserveReady(CapturedBindings.Fixture());
        Check(CapturedBindings.ObjectsUnchanged(objects), "The first selected call changed either actual global object's bytes.");
        var second = ObserveReady(CapturedBindings.Fixture());
        Check(CapturedBindings.ObjectsUnchanged(objects), "The second selected call changed either actual global object's bytes.");
        Check(JsonNode.DeepEquals(first, native["first"]) && JsonNode.DeepEquals(second, native["second"]),
            "The complete generated constructor-boundary observations differ from native.");
        // Physical fields and object bytes are inspected while Ready. Final
        // observations below read only independent application scalar storage.
        var physical = CapturedBindings.Physical(objects);
        Check(JsonNode.DeepEquals(physical.NativeCompatible, native["physical"]),
            "Actual generated and native complete-carrier physical observations differ.");

        CapturedBindings.Shutdown();
        var ended = CapturedBindings.Diagnostics();
        CheckEnded(ended, 1, 1, ready);
        var shutdown = Observation(null, CapturedBindings.SnapshotStorage());
        Check(JsonNode.DeepEquals(shutdown, native["shutdown"]), "Final independent protocol storage differs from native.");
        RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState.Shutdown);

        CapturedBindings.Shutdown();
        var repeatedEnd = CapturedBindings.Diagnostics();
        CheckEnded(repeatedEnd, 2, 2, ready);
        Check(repeatedEnd == (ended with { ShutdownCalls = 2, ProtocolRejections = 2 }) &&
            JsonNode.DeepEquals(Observation(null, CapturedBindings.SnapshotStorage()), shutdown),
            "Repeated shutdown changed application storage or repeated a lifecycle effect.");

        var observations = new JsonObject
        {
            ["startup"] = startup, ["first"] = first, ["second"] = second,
            ["shutdown"] = shutdown, ["physical"] = physical.NativeCompatible
        };
        Check(JsonNode.DeepEquals(observations, native), "The complete native protocol differs.");
        Console.WriteLine(observations.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["selectedConstructorBoundary"] = new JsonObject
            {
                ["physical"] = physical.Additional,
                ["initializeCalls"] = repeatedEnd.InitializeCalls,
                ["startupInvocations"] = repeatedEnd.StartupInvocations,
                ["startupCompletions"] = repeatedEnd.StartupCompletions,
                ["registerCalls"] = repeatedEnd.RegisterCalls,
                ["registered"] = repeatedEnd.Registered,
                ["pendingEntries"] = repeatedEnd.PendingEntries,
                ["shutdownCalls"] = repeatedEnd.ShutdownCalls,
                ["shutdownInvocations"] = repeatedEnd.ShutdownInvocations,
                ["destructorStarts"] = repeatedEnd.DestructorStarts,
                ["destructorCompletions"] = repeatedEnd.DestructorCompletions,
                ["protocolRejections"] = repeatedEnd.ProtocolRejections,
                ["startupStartSequence"] = repeatedEnd.StartupStartSequence,
                ["startupCompleteSequence"] = repeatedEnd.StartupCompleteSequence,
                ["shutdownStartSequence"] = repeatedEnd.ShutdownStartSequence,
                ["shutdownCompleteSequence"] = repeatedEnd.ShutdownCompleteSequence,
                ["beforeInitializeGuardObserved"] = true, ["afterShutdownGuardObserved"] = true,
                ["repeatInitializeNoApplicationEffects"] = true, ["repeatShutdownNoApplicationEffects"] = true,
                ["liveZeroMeaning"] = "scalar protocol only; no class ownership or destruction claim",
                ["postShutdownInspection"] = "independent scalar/event/value storage only"
            }
        }.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["suppliedCaptureProvenance"] = new JsonObject
            {
                ["path"] = Path.GetFullPath(arguments[1]), ["sha256"] = Hash(arguments[1]),
                ["providerInputSha256"] = ProviderInputHash, ["sourceInputSha256"] = SourceInputHash,
                ["providerFactsSha256"] = ProviderFactsHash, ["sourceFactsSha256"] = SourceFactsHash,
                ["nativeObservationsSha256"] = NativeHash,
                ["attribution"] = "supplied evidence; the outer receipt binds both selected inputs, GeneratedProject and executable"
            }
        }.ToJsonString());
        return 0;
    }

    private static void CheckReady(ManagedSelectedProgramLifetimeDiagnostics value, long initializeCalls)
    {
        Check(value.State == ManagedSelectedProgramLifetimeState.Ready && value.OwningManagedThreadId > 0 &&
            value.InitializeCalls == initializeCalls && value.StartupInvocations == 1 && value.StartupCompletions == 1 &&
            value.ShutdownCalls == 0 && value.ShutdownInvocations == 0 && value.ShutdownCompletions == 0 &&
            value.ProtocolRejections == 1 && value.StartupStartSequence > 0 &&
            value.StartupCompleteSequence > value.StartupStartSequence,
            "Actual explicit startup did not complete once with the expected public guard rejection.");
        CheckNoDestructorRegistrationsOrFailures(value);
    }

    private static void CheckEnded(ManagedSelectedProgramLifetimeDiagnostics value, long shutdownCalls, long protocolRejections,
        ManagedSelectedProgramLifetimeDiagnostics ready)
    {
        Check(value.State == ManagedSelectedProgramLifetimeState.Shutdown && value.OwningManagedThreadId == ready.OwningManagedThreadId &&
            value.InitializeCalls == 2 && value.StartupInvocations == 1 && value.StartupCompletions == 1 &&
            value.ShutdownCalls == shutdownCalls && value.ShutdownInvocations == 1 && value.ShutdownCompletions == 1 &&
            value.ProtocolRejections == protocolRejections && value.StartupStartSequence == ready.StartupStartSequence &&
            value.StartupCompleteSequence == ready.StartupCompleteSequence && value.ShutdownStartSequence > value.StartupCompleteSequence &&
            value.ShutdownCompleteSequence > value.ShutdownStartSequence,
            "Actual explicit shutdown was not a single empty drain after successful startup.");
        CheckNoDestructorRegistrationsOrFailures(value);
    }

    private static void CheckNoDestructorRegistrationsOrFailures(ManagedSelectedProgramLifetimeDiagnostics value) => Check(
        value.RegisterCalls == 0 && value.Registered == 0 && value.RegistrationFailures == 0 && value.PendingEntries == 0 &&
        value.DestructorStarts == 0 && value.DestructorCompletions == 0 && value.StartupFailures == 0 && value.ShutdownFailures == 0 &&
        value.LastRegisteredEntry == 0 && value.LastStartedEntry == 0 && value.LastCompletedEntry == 0 &&
        value.LastRegisteredObjectAddress == 0 && value.LastRegisteredDsoAddress == 0 && value.LastDestroyedObjectAddress == 0 &&
        value.LastDestroyedDsoAddress == 0 && value.LastRegistrationSequence == 0 && value.LastDestructorStartSequence == 0 &&
        value.LastDestructorCompleteSequence == 0,
        "The trivial-destruction source acquired a synthetic registration, destructor or failure effect.");

    private static void RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState expected)
    {
        var before = CapturedBindings.Diagnostics();
        Check(before.State == expected, "The public guard check began in a different actual state.");
        var storage = Observation(null, CapturedBindings.SnapshotStorage());
        var providerCounter = CapturedBindings.ProviderSerialCounter();
        var rejected = false;
        try { _ = CapturedBindings.Fixture(); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected && CapturedBindings.Diagnostics() == (before with { ProtocolRejections = before.ProtocolRejections + 1 }) &&
            CapturedBindings.ProviderSerialCounter() == providerCounter &&
            JsonNode.DeepEquals(storage, Observation(null, CapturedBindings.SnapshotStorage())),
            "A public fixture boundary call was not rejected without application effects.");
    }

    private static JsonObject ObserveReady(int? result)
    {
        var count = CapturedBindings.Count();
        Check(count is >= 0 and <= 64, "The actual event protocol exceeded its bound.");
        var values = new int[9];
        var events = new int[count];
        for (var index = 0; index < values.Length; ++index) values[index] = CapturedBindings.Value(index);
        for (var index = 0; index < events.Length; ++index) events[index] = CapturedBindings.Event(index);
        return Observation(result, new(CapturedBindings.Calls(), CapturedBindings.Live(), CapturedBindings.Overflow(), values, events));
    }

    private static JsonObject Observation(int? result, GeneratedStorageSnapshot storage)
    {
        Check(storage.Values.Length == 9 && storage.Events.Length <= 64 && storage.Live == 0 && storage.Overflow == 0,
            "The independent scalar/array protocol differs or exceeds its bounds.");
        var values = new JsonArray();
        foreach (var value in storage.Values) values.Add((JsonNode)JsonValue.Create(value)!);
        var events = new JsonArray();
        foreach (var value in storage.Events) events.Add((JsonNode)JsonValue.Create(value)!);
        return new JsonObject { ["result"] = result is int number ? JsonValue.Create(number) : null,
            ["calls"] = storage.Calls, ["live"] = storage.Live, ["overflow"] = storage.Overflow,
            ["values"] = values, ["events"] = events };
    }

    private static HashSet<string> Hashes(JsonNode value, string section)
    {
        var rows = value[section] as JsonArray ?? throw new InvalidDataException("Missing " + section + ".");
        var hashes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
            Check(hashes.Add(row?["sha256"]?.GetValue<string>() ?? throw new InvalidDataException("Missing content hash.")),
                "Duplicate content identity in " + section + ".");
        return hashes;
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

internal sealed record GeneratedStorageSnapshot(int Calls, int Live, int Overflow, int[] Values, int[] Events);
internal sealed record GeneratedObjectSnapshot(nint FirstAddress, nint SecondAddress, byte[] FirstBytes, byte[] SecondBytes, int ProviderSerialCounter);
internal sealed record PhysicalObservation(JsonObject NativeCompatible, JsonObject Additional);

// Bindings use the actual selected-constructor-boundary-generated-1 publication.
internal static partial class CapturedBindings
{
    internal static partial void Initialize();
    internal static partial void Shutdown();
    internal static partial int Fixture();
    internal static partial int Count();
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Calls();
    internal static partial int Value(int index);
    internal static partial int Event(int index);
    internal static partial ManagedSelectedProgramLifetimeDiagnostics Diagnostics();
    internal static partial int ProviderSerialCounter();
    internal static partial GeneratedStorageSnapshot SnapshotStorage();
    internal static partial GeneratedObjectSnapshot SnapshotObjects();
    internal static partial bool ObjectsUnchanged(GeneratedObjectSnapshot snapshot);
    internal static partial PhysicalObservation Physical(GeneratedObjectSnapshot snapshot);
}

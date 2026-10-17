using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;

return SelectedInlineComdatChecks.Run(args);

internal static class SelectedInlineComdatChecks
{
    private const string SourceInputHash = "05ad8276da19d56c1593fff0d81ee8ea3d6efc7f27d7274dbe35af857b067238";
    private const string ProviderInputHash = "1f9efec52e75dd1f818bf3b911c8151ae45f480dd92fdb16924683f78515e428";
    private const string SourceFactsHash = "c97e975c7a5b965d5cfcccd745737f38eb1c1b1fa41fc5d3ed49c1d8015d39a8";
    private const string ProviderFactsHash = "c68eeea600f6ab4ee1a052d78b8e1b9e915827841a740e7edea8046a4973f143";
    private const string NativeHash = "334e5005b734df9ff7d4cbc6b0d66c6374ca3c23ba50bd954a94e16d143754f6";
    private static readonly string[] SourceHashes =
    [
        "f096af55500399cb08f5e890156aa33719b53790e3c31973f6d0560ae4511f69",
        "40897080994eca046ca89fa9307af08575522d0da62c4ff4ac0fe5f2b03ffae8",
        "19c4060068be423beed61dd32a1546340126672b97de8d8b0f3735706b18ea2b",
        "931bcd69fa10c8906c1ca6434a5193f062e19ff26d947791adcc3085b1f5a3ff"
    ];

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2)
            throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON PREPARATION_PROVENANCE_JSON");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0])) ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1])) ?? throw new InvalidDataException("Missing preparation provenance.");
        Check(Hash(arguments[0]) == NativeHash && Text(provenance, "native", "sha256") == NativeHash &&
            Hashes(provenance, "sources").SetEquals(SourceHashes) &&
            Hashes(provenance, "inputs").SetEquals([SourceInputHash, ProviderInputHash]) &&
            Hashes(provenance, "facts").SetEquals([SourceFactsHash, ProviderFactsHash]) &&
            provenance["sourceUnchanged"]?.GetValue<bool>() == true && provenance["valueCount"]?.GetValue<int>() == 9 &&
            provenance["eventCapacity"]?.GetValue<int>() == 64,
            "The immutable captures, source/facts identities or independent native observations differ.");

        var initial = CapturedBindings.Diagnostics();
        Check(initial.State == ManagedSelectedProgramLifetimeState.New && initial.InitializeCalls == 0 &&
            initial.ProtocolRejections == 0 && initial.PendingEntries == 0,
            "The generated explicit program lifetime was not fresh.");
        RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState.New);

        CapturedBindings.Initialize();
        var ready = CapturedBindings.Diagnostics();
        CheckReady(ready, 1);
        var objects = CapturedBindings.SnapshotObjects();
        var dso = ready.LastRegisteredDsoAddress;
        Check(objects.FirstAddress != 0 && objects.SecondAddress != 0 && objects.FirstAddress != objects.SecondAddress &&
            ready.LastRegisteredEntry == 2 && ready.LastRegisteredObjectAddress == objects.SecondAddress && dso != 0 &&
            ready.StartupStartSequence > 0 && ready.LastRegistrationSequence > ready.StartupStartSequence &&
            ready.StartupCompleteSequence > ready.LastRegistrationSequence,
            "Actual registration identities or startup/registration/completion ordering differ.");
        var startup = ObserveReady(null);
        Check(JsonNode.DeepEquals(startup, native["startup"]), "Actual generated startup differs from native.");

        CapturedBindings.Initialize();
        var repeatedReady = CapturedBindings.Diagnostics();
        CheckReady(repeatedReady, 2);
        Check(repeatedReady == (ready with { InitializeCalls = 2 }) &&
            JsonNode.DeepEquals(ObserveReady(null), startup) && CapturedBindings.ObjectsUnchanged(objects),
            "Repeat initialization changed constructors, registrations, objects or application counters.");

        var first = ObserveReady(CapturedBindings.Fixture());
        Check(CapturedBindings.ObjectsUnchanged(objects), "The first call changed a global object's bytes.");
        var second = ObserveReady(CapturedBindings.Fixture());
        Check(CapturedBindings.ObjectsUnchanged(objects), "The second call changed a global object's bytes.");
        Check(JsonNode.DeepEquals(first, native["first"]) && JsonNode.DeepEquals(second, native["second"]),
            "Actual generated automatic-scope calls or event/value observations differ from native.");
        var afterScopes = CapturedBindings.SnapshotStorage();
        Check(afterScopes.Created == 6 && afterScopes.Destroyed == 4 && afterScopes.Live == 2 && afterScopes.Calls == 2 &&
            CapturedBindings.Diagnostics() == repeatedReady,
            "Four automatic D1 calls did not finish before the snapshot, or incorrectly used the global registry.");
        var physical = CapturedBindings.Physical(objects);
        Check(JsonNode.DeepEquals(physical.NativeCompatible, native["physical"]),
            "The actual complete-carrier physical observations differ from native.");

        CapturedBindings.Shutdown();
        var ended = CapturedBindings.Diagnostics();
        CheckEnded(ended, 1, 1, ready, objects.FirstAddress, objects.SecondAddress, dso);
        // Direct independent storage only. No source root or class member is
        // read after its actual registered global D1 has run.
        var finalStorage = CapturedBindings.SnapshotStorage();
        var shutdown = Observation(null, finalStorage);
        Check(finalStorage.Created == 6 && finalStorage.Destroyed == 6 && finalStorage.Live == 0 &&
            JsonNode.DeepEquals(shutdown, native["shutdown"]),
            "Global reverse destruction did not finish once after the four automatic scope destructions.");
        RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState.Shutdown);

        CapturedBindings.Shutdown();
        var repeatedEnd = CapturedBindings.Diagnostics();
        CheckEnded(repeatedEnd, 2, 2, ready, objects.FirstAddress, objects.SecondAddress, dso);
        Check(repeatedEnd == (ended with { ShutdownCalls = 2, ProtocolRejections = 2 }) &&
            JsonNode.DeepEquals(Observation(null, CapturedBindings.SnapshotStorage()), shutdown),
            "Repeat shutdown changed application storage or repeated a finalizer.");

        var observations = new JsonObject
        {
            ["startup"] = startup, ["first"] = first, ["second"] = second,
            ["shutdown"] = shutdown, ["physical"] = physical.NativeCompatible
        };
        Check(JsonNode.DeepEquals(observations, native), "The complete four-snapshot native protocol differs.");
        Console.WriteLine(observations.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["selectedInlineComdat"] = new JsonObject
            {
                ["physical"] = physical.Additional,
                ["created"] = finalStorage.Created, ["destroyed"] = finalStorage.Destroyed, ["live"] = finalStorage.Live,
                ["automaticDestructionsBeforeShutdown"] = afterScopes.Destroyed,
                ["registeredGlobalDestructions"] = repeatedEnd.DestructorCompletions,
                ["initializeCalls"] = repeatedEnd.InitializeCalls, ["startupInvocations"] = repeatedEnd.StartupInvocations,
                ["registerCalls"] = repeatedEnd.RegisterCalls, ["registered"] = repeatedEnd.Registered,
                ["pendingEntries"] = repeatedEnd.PendingEntries, ["shutdownCalls"] = repeatedEnd.ShutdownCalls,
                ["shutdownInvocations"] = repeatedEnd.ShutdownInvocations, ["destructorStarts"] = repeatedEnd.DestructorStarts,
                ["destructorCompletions"] = repeatedEnd.DestructorCompletions, ["protocolRejections"] = repeatedEnd.ProtocolRejections,
                ["lastRegisteredEntry"] = repeatedEnd.LastRegisteredEntry, ["lastCompletedEntry"] = repeatedEnd.LastCompletedEntry,
                ["moduleIdentityObservation"] = "opaque registration and retirement diagnostics only",
                ["moduleIdentityStable"] = ended.LastDestroyedDsoAddress == dso,
                ["startupStartSequence"] = repeatedEnd.StartupStartSequence,
                ["lastRegistrationSequence"] = repeatedEnd.LastRegistrationSequence,
                ["startupCompleteSequence"] = repeatedEnd.StartupCompleteSequence,
                ["shutdownStartSequence"] = repeatedEnd.ShutdownStartSequence,
                ["lastDestructorStartSequence"] = repeatedEnd.LastDestructorStartSequence,
                ["lastDestructorCompleteSequence"] = repeatedEnd.LastDestructorCompleteSequence,
                ["shutdownCompleteSequence"] = repeatedEnd.ShutdownCompleteSequence,
                ["beforeInitializeGuardObserved"] = true, ["afterShutdownGuardObserved"] = true,
                ["repeatInitializeNoApplicationEffects"] = true, ["repeatShutdownNoApplicationEffects"] = true,
                ["sourceOdrValidationClaim"] = false, ["manualConstructorOrDestructorCalls"] = false
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
                ["attribution"] = "supplied evidence; the outer receipt binds the explicit manifest-order policy, representative mappings, both inputs and executable"
            }
        }.ToJsonString());
        return 0;
    }

    private static void CheckReady(ManagedSelectedProgramLifetimeDiagnostics value, long initializeCalls)
    {
        Check(value.State == ManagedSelectedProgramLifetimeState.Ready && value.OwningManagedThreadId > 0 &&
            value.InitializeCalls == initializeCalls && value.StartupInvocations == 1 && value.StartupCompletions == 1 &&
            value.RegisterCalls == 2 && value.Registered == 2 && value.PendingEntries == 2 && value.ShutdownCalls == 0 &&
            value.ShutdownInvocations == 0 && value.DestructorStarts == 0 && value.DestructorCompletions == 0 &&
            value.ShutdownCompletions == 0 && value.ProtocolRejections == 1,
            "Successful explicit startup did not register exactly the two global destructors.");
        CheckNoFailures(value);
    }

    private static void CheckEnded(ManagedSelectedProgramLifetimeDiagnostics value, long shutdownCalls, long protocolRejections,
        ManagedSelectedProgramLifetimeDiagnostics ready, nint first, nint second, nint dso)
    {
        Check(value.State == ManagedSelectedProgramLifetimeState.Shutdown && value.OwningManagedThreadId == ready.OwningManagedThreadId &&
            value.InitializeCalls == 2 && value.StartupInvocations == 1 && value.StartupCompletions == 1 && value.RegisterCalls == 2 &&
            value.Registered == 2 && value.PendingEntries == 0 && value.ShutdownCalls == shutdownCalls && value.ShutdownInvocations == 1 &&
            value.DestructorStarts == 2 && value.DestructorCompletions == 2 && value.ShutdownCompletions == 1 &&
            value.ProtocolRejections == protocolRejections && value.LastRegisteredEntry == 2 && value.LastStartedEntry == 1 &&
            value.LastCompletedEntry == 1 && value.LastRegisteredObjectAddress == second && value.LastDestroyedObjectAddress == first &&
            value.LastRegisteredDsoAddress == dso && value.LastDestroyedDsoAddress == dso,
            "Global destructor drain did not retire the two exact objects in reverse order exactly once.");
        Check(value.StartupStartSequence == ready.StartupStartSequence && value.LastRegistrationSequence == ready.LastRegistrationSequence &&
            value.StartupCompleteSequence == ready.StartupCompleteSequence && value.ShutdownStartSequence > value.StartupCompleteSequence &&
            value.LastDestructorStartSequence > value.ShutdownStartSequence && value.LastDestructorCompleteSequence > value.LastDestructorStartSequence &&
            value.ShutdownCompleteSequence > value.LastDestructorCompleteSequence,
            "The observed startup/registration/shutdown/destructor sequence differs.");
        CheckNoFailures(value);
    }

    private static void CheckNoFailures(ManagedSelectedProgramLifetimeDiagnostics value) => Check(
        value.StartupFailures == 0 && value.RegistrationFailures == 0 && value.ShutdownFailures == 0,
        "The positive fixture encountered an unexpected initialization, registration or shutdown failure.");

    private static void RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState expected)
    {
        var before = CapturedBindings.Diagnostics();
        Check(before.State == expected, "The public boundary check began in a different actual state.");
        var storage = Observation(null, CapturedBindings.SnapshotStorage());
        var rejected = false;
        try { _ = CapturedBindings.Fixture(); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected && CapturedBindings.Diagnostics() == (before with { ProtocolRejections = before.ProtocolRejections + 1 }) &&
            JsonNode.DeepEquals(storage, Observation(null, CapturedBindings.SnapshotStorage())),
            "The public fixture was not rejected without application effects.");
    }

    private static JsonObject ObserveReady(int? result)
    {
        var count = CapturedBindings.Count();
        Check(count is >= 0 and <= 64, "The actual event count exceeded the finite protocol.");
        var values = new int[9];
        var events = new int[count];
        for (var index = 0; index < values.Length; ++index) values[index] = CapturedBindings.Value(index);
        for (var index = 0; index < events.Length; ++index) events[index] = CapturedBindings.Event(index);
        return Observation(result, new(CapturedBindings.Calls(), CapturedBindings.CreatedStorage(), CapturedBindings.DestroyedStorage(),
            CapturedBindings.Live(), CapturedBindings.Overflow(), values, events));
    }

    private static JsonObject Observation(int? result, ApplicationSnapshot storage)
    {
        Check(storage.Values.Length == 9 && storage.Events.Length <= 64 && storage.Overflow == 0,
            "The independent scalar/array protocol exceeded its bounds.");
        var values = new JsonArray();
        foreach (var value in storage.Values) values.Add((JsonNode)JsonValue.Create(value)!);
        var events = new JsonArray();
        foreach (var value in storage.Events) events.Add((JsonNode)JsonValue.Create(value)!);
        return new JsonObject { ["result"] = result is int number ? JsonValue.Create(number) : null,
            ["calls"] = storage.Calls, ["created"] = storage.Created, ["destroyed"] = storage.Destroyed,
            ["live"] = storage.Live, ["overflow"] = storage.Overflow, ["values"] = values, ["events"] = events };
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

internal sealed record ApplicationSnapshot(int Calls, int Created, int Destroyed, int Live, int Overflow, int[] Values, int[] Events);
internal sealed record ObjectSnapshot(nint FirstAddress, nint SecondAddress, byte[] FirstBytes, byte[] SecondBytes);
internal sealed record PhysicalObservation(JsonObject NativeCompatible, JsonObject Additional);

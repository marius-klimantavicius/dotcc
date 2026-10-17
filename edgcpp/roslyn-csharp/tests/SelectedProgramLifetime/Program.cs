using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;

return SelectedProgramLifetimeChecks.Run(args);

internal static class SelectedProgramLifetimeChecks
{
    private const string InputHash = "485b8f2be85a8f0eba3a5bad94e3d5fc141101f2ead384d102ca809d97902b2a";
    private const string SourceHash = "bbc48b4cab85a6514e884edfebe6cc830f97fe38deeb4bd64f05e3854d8c39cc";
    private const string NativeSourceHash = "9dd2e39607e47ff6b044ec102b90469b54223bb1429dc21830556516f10d1151";
    private const string FactsHash = "57e935a5385927df22c1a4ddd0b9a172aa7dd0b71bb56996dea67f8333e5eb7d";
    private const string NativeHash = "80d483e1c86476912a2e1ced53f7f085ace6b7ef38ecf2e195aba433fbe0793e";

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2)
            throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0])) ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1])) ?? throw new InvalidDataException("Missing capture provenance.");
        Check(Hash(arguments[0]) == NativeHash && Text(provenance, "native", "sha256") == NativeHash &&
            Text(provenance, "input", "sha256") == InputHash && Text(provenance, "facts", "sha256") == FactsHash,
            "The supplied native observations, capture or public facts identity differs.");
        Check(provenance["sourceUnchanged"]?.GetValue<bool>() == true &&
            provenance["valueCount"]?.GetValue<int>() == 9 && provenance["eventCapacity"]?.GetValue<int>() == 64,
            "The supplied preparation evidence does not bind the finite unchanged source protocol.");
        var sources = provenance["sources"] as JsonArray ?? throw new InvalidDataException("Missing source identities.");
        Check(sources.Count == 2 && sources[0]?["sha256"]?.GetValue<string>() == SourceHash &&
            sources[1]?["sha256"]?.GetValue<string>() == NativeSourceHash,
            "The fixture and independent native observer identities differ.");

        var before = CapturedBindings.Diagnostics();
        Check(before.State == ManagedSelectedProgramLifetimeState.New && before.InitializeCalls == 0 &&
            before.RegisterCalls == 0 && before.PendingEntries == 0 && before.ProtocolRejections == 0,
            "The generated program did not begin with a fresh explicit lifetime instance.");
        RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState.New);

        CapturedBindings.Initialize();
        var ready = CapturedBindings.Diagnostics();
        CheckReady(ready, initializeCalls: 1);
        var firstObject = CapturedBindings.FirstObjectAddress();
        var secondObject = CapturedBindings.SecondObjectAddress();
        // The module slot is intentionally private generated implementation
        // storage. Observe its opaque identity through actual registration and
        // retirement diagnostics, without inventing an application accessor.
        var dso = ready.LastRegisteredDsoAddress;
        Check(firstObject != 0 && secondObject != 0 && firstObject != secondObject && dso != 0 &&
            ready.LastRegisteredEntry == 2 && ready.LastRegisteredObjectAddress == secondObject &&
            ready.LastRegisteredDsoAddress == dso,
            "The two actual registrations do not name distinct generated objects and the exact module slot.");
        Check(ready.StartupStartSequence > 0 && ready.LastRegistrationSequence > ready.StartupStartSequence &&
            ready.StartupCompleteSequence > ready.LastRegistrationSequence,
            "Actual registration did not occur between explicit startup entry and completion.");
        var startup = ObserveReady(null);
        Check(JsonNode.DeepEquals(startup, native["startup"]), "Generated startup differs from actual native startup.");
        var physical = CapturedBindings.Physical();

        CapturedBindings.Initialize();
        var repeatedReady = CapturedBindings.Diagnostics();
        CheckReady(repeatedReady, initializeCalls: 2);
        Check(repeatedReady.StartupStartSequence == ready.StartupStartSequence &&
            repeatedReady.LastRegistrationSequence == ready.LastRegistrationSequence &&
            repeatedReady.StartupCompleteSequence == ready.StartupCompleteSequence &&
            repeatedReady.LastRegisteredEntry == ready.LastRegisteredEntry &&
            repeatedReady.LastRegisteredObjectAddress == ready.LastRegisteredObjectAddress &&
            repeatedReady.LastRegisteredDsoAddress == ready.LastRegisteredDsoAddress &&
            JsonNode.DeepEquals(ObserveReady(null), startup),
            "Successful repeat initialization repeated an application effect or registration.");

        var first = ObserveReady(CapturedBindings.Fixture());
        var second = ObserveReady(CapturedBindings.Fixture());
        Check(JsonNode.DeepEquals(first, native["first"]) && JsonNode.DeepEquals(second, native["second"]),
            "Generated selected-root calls differ from the complete independent native observations.");

        CapturedBindings.Shutdown();
        var ended = CapturedBindings.Diagnostics();
        CheckEnded(ended, shutdownCalls: 1, firstObject, secondObject, dso, ready, expectedProtocolRejections: 1);
        // Protocol functions are advertised roots and remain guarded. Once
        // shutdown has completed, inspect only the published independent scalar
        // and event/value storage; never call a guarded source getter here.
        var shutdownStorage = CapturedBindings.SnapshotStorage();
        var shutdown = Observation(null, shutdownStorage);
        Check(JsonNode.DeepEquals(shutdown, native["shutdown"]),
            "Actual generated reverse destruction or final independent scalar storage differs from native.");
        RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState.Shutdown);

        CapturedBindings.Shutdown();
        var repeatedEnd = CapturedBindings.Diagnostics();
        CheckEnded(repeatedEnd, shutdownCalls: 2, firstObject, secondObject, dso, ready, expectedProtocolRejections: 2);
        Check(repeatedEnd.ShutdownStartSequence == ended.ShutdownStartSequence &&
            repeatedEnd.LastDestructorStartSequence == ended.LastDestructorStartSequence &&
            repeatedEnd.LastDestructorCompleteSequence == ended.LastDestructorCompleteSequence &&
            repeatedEnd.ShutdownCompleteSequence == ended.ShutdownCompleteSequence &&
            JsonNode.DeepEquals(Observation(null, CapturedBindings.SnapshotStorage()), shutdown),
            "Successful repeat shutdown repeated an application destructor or changed final storage.");

        var observations = new JsonObject
        {
            ["startup"] = startup, ["first"] = first, ["second"] = second, ["shutdown"] = shutdown
        };
        Check(JsonNode.DeepEquals(observations, native), "The complete four-snapshot native protocol differs.");
        Console.WriteLine(observations.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["selectedProgramLifetime"] = new JsonObject
            {
                ["ready"] = DiagnosticObservation(repeatedReady),
                ["shutdown"] = DiagnosticObservation(repeatedEnd),
                ["actualObjectAddressesDistinct"] = firstObject != secondObject,
                ["registeredSecondObject"] = ready.LastRegisteredObjectAddress == secondObject,
                ["lastDestroyedFirstObject"] = ended.LastDestroyedObjectAddress == firstObject,
                ["moduleSlotStable"] = ready.LastRegisteredDsoAddress == dso && ended.LastDestroyedDsoAddress == dso,
                ["moduleAddressObservation"] = "opaque registration/retirement diagnostics only",
                ["physical"] = physical,
                ["repeatInitializeNoApplicationEffects"] = true,
                ["repeatShutdownNoApplicationEffects"] = true,
                ["fixtureBeforeInitializeRejectedWithoutApplicationEffects"] = true,
                ["fixtureAfterShutdownRejectedWithoutApplicationEffects"] = true,
                ["postShutdownInspection"] = "independent generated scalar/event/value storage only",
                ["registrationFailureOrStartupFailureExercised"] = false
            }
        }.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["suppliedCaptureProvenance"] = new JsonObject
            {
                ["path"] = Path.GetFullPath(arguments[1]), ["sha256"] = Hash(arguments[1]),
                ["inputSha256"] = InputHash, ["fixtureSourceSha256"] = SourceHash,
                ["nativeObserverSourceSha256"] = NativeSourceHash, ["publicFactsSha256"] = FactsHash,
                ["nativeObservationsSha256"] = NativeHash,
                ["attribution"] = "supplied evidence; the outer receipt binds the compiled GeneratedProject and executable"
            }
        }.ToJsonString());
        return 0;
    }

    private static void CheckReady(ManagedSelectedProgramLifetimeDiagnostics value, long initializeCalls)
    {
        Check(value.State == ManagedSelectedProgramLifetimeState.Ready && value.OwningManagedThreadId > 0 &&
            value.InitializeCalls == initializeCalls && value.StartupInvocations == 1 && value.StartupCompletions == 1 &&
            value.StartupFailures == 0 && value.RegisterCalls == 2 && value.Registered == 2 && value.RegistrationFailures == 0 &&
            value.PendingEntries == 2 && value.ShutdownCalls == 0 && value.ShutdownInvocations == 0 &&
            value.DestructorStarts == 0 && value.DestructorCompletions == 0 && value.ShutdownCompletions == 0 &&
            value.ShutdownFailures == 0 && value.ProtocolRejections == 1,
            "Explicit successful startup did not retain exactly the two actual destructor registrations.");
    }

    private static void CheckEnded(ManagedSelectedProgramLifetimeDiagnostics value, long shutdownCalls,
        nint firstObject, nint secondObject, nint dso, ManagedSelectedProgramLifetimeDiagnostics ready,
        long expectedProtocolRejections)
    {
        Check(value.State == ManagedSelectedProgramLifetimeState.Shutdown && value.OwningManagedThreadId == ready.OwningManagedThreadId &&
            value.InitializeCalls == 2 && value.StartupInvocations == 1 && value.StartupCompletions == 1 && value.StartupFailures == 0 &&
            value.RegisterCalls == 2 && value.Registered == 2 && value.RegistrationFailures == 0 && value.PendingEntries == 0 &&
            value.ShutdownCalls == shutdownCalls && value.ShutdownInvocations == 1 && value.DestructorStarts == 2 &&
            value.DestructorCompletions == 2 && value.ShutdownCompletions == 1 && value.ShutdownFailures == 0 &&
            value.ProtocolRejections == expectedProtocolRejections,
            "Successful explicit shutdown did not drain the two registrations exactly once.");
        Check(value.LastRegisteredEntry == 2 && value.LastRegisteredObjectAddress == secondObject &&
            value.LastRegisteredDsoAddress == dso && value.LastStartedEntry == 1 && value.LastCompletedEntry == 1 &&
            value.LastDestroyedObjectAddress == firstObject && value.LastDestroyedDsoAddress == dso,
            "Last registration/retirement observations do not preserve reverse order and exact object/module identities.");
        Check(value.StartupStartSequence == ready.StartupStartSequence && value.LastRegistrationSequence == ready.LastRegistrationSequence &&
            value.StartupCompleteSequence == ready.StartupCompleteSequence && value.ShutdownStartSequence > value.StartupCompleteSequence &&
            value.LastDestructorStartSequence > value.ShutdownStartSequence &&
            value.LastDestructorCompleteSequence > value.LastDestructorStartSequence &&
            value.ShutdownCompleteSequence > value.LastDestructorCompleteSequence,
            "Explicit ready/shutdown/destructor completion sequence differs.");
    }

    private static void RejectFixtureWithoutApplicationEffects(ManagedSelectedProgramLifetimeState expectedState)
    {
        var before = CapturedBindings.Diagnostics();
        Check(before.State == expectedState, "The advertised-root boundary check began in a different actual state.");
        var storage = Observation(null, CapturedBindings.SnapshotStorage());
        var rejected = false;
        try
        {
            _ = CapturedBindings.Fixture();
        }
        catch (InvalidOperationException)
        {
            rejected = true;
        }
        var after = CapturedBindings.Diagnostics();
        Check(rejected && after == (before with { ProtocolRejections = before.ProtocolRejections + 1 }),
            "The advertised fixture was not rejected at the actual lifetime boundary, or changed other runtime state.");
        Check(JsonNode.DeepEquals(storage, Observation(null, CapturedBindings.SnapshotStorage())),
            "A rejected advertised fixture call entered its application body or changed scalar/event/value storage.");
    }

    private static JsonObject ObserveReady(int? result)
    {
        var count = CapturedBindings.Count();
        Check(count is >= 0 and <= 64, "Actual generated event count exceeded the captured protocol.");
        var values = new int[9];
        var events = new int[count];
        for (var index = 0; index < values.Length; ++index) values[index] = CapturedBindings.Value(index);
        for (var index = 0; index < events.Length; ++index) events[index] = CapturedBindings.Event(index);
        return Observation(result, new(CapturedBindings.Live(), CapturedBindings.Constructed(), CapturedBindings.Destroyed(),
            CapturedBindings.Calls(), CapturedBindings.Overflow(), values, events));
    }

    private static JsonObject Observation(int? result, GeneratedStorageSnapshot storage)
    {
        Check(storage.Values.Length == 9 && storage.Events.Length <= 64 && storage.Overflow == 0,
            "The independent generated scalar storage protocol exceeded its bounds.");
        var values = new JsonArray();
        foreach (var value in storage.Values) values.Add((JsonNode)JsonValue.Create(value)!);
        var events = new JsonArray();
        foreach (var value in storage.Events) events.Add((JsonNode)JsonValue.Create(value)!);
        return new JsonObject
        {
            ["result"] = result is int number ? JsonValue.Create(number) : null,
            ["live"] = storage.Live, ["constructed"] = storage.Constructed, ["destroyed"] = storage.Destroyed,
            ["calls"] = storage.Calls, ["overflow"] = storage.Overflow, ["values"] = values, ["events"] = events
        };
    }

    private static JsonObject DiagnosticObservation(ManagedSelectedProgramLifetimeDiagnostics value) => new()
    {
        ["state"] = value.State.ToString(), ["initializeCalls"] = value.InitializeCalls,
        ["startupInvocations"] = value.StartupInvocations, ["startupCompletions"] = value.StartupCompletions,
        ["startupFailures"] = value.StartupFailures, ["registerCalls"] = value.RegisterCalls,
        ["registered"] = value.Registered, ["registrationFailures"] = value.RegistrationFailures,
        ["shutdownCalls"] = value.ShutdownCalls, ["shutdownInvocations"] = value.ShutdownInvocations,
        ["destructorStarts"] = value.DestructorStarts, ["destructorCompletions"] = value.DestructorCompletions,
        ["shutdownCompletions"] = value.ShutdownCompletions, ["shutdownFailures"] = value.ShutdownFailures,
        ["protocolRejections"] = value.ProtocolRejections, ["pendingEntries"] = value.PendingEntries,
        ["lastRegisteredEntry"] = value.LastRegisteredEntry, ["lastStartedEntry"] = value.LastStartedEntry,
        ["lastCompletedEntry"] = value.LastCompletedEntry, ["startupStartSequence"] = value.StartupStartSequence,
        ["lastRegistrationSequence"] = value.LastRegistrationSequence, ["startupCompleteSequence"] = value.StartupCompleteSequence,
        ["shutdownStartSequence"] = value.ShutdownStartSequence, ["lastDestructorStartSequence"] = value.LastDestructorStartSequence,
        ["lastDestructorCompleteSequence"] = value.LastDestructorCompleteSequence, ["shutdownCompleteSequence"] = value.ShutdownCompleteSequence
    };

    private static string Text(JsonNode node, string section, string field) =>
        node[section]?[field]?.GetValue<string>() ?? throw new InvalidDataException("Missing " + section + "." + field);

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

internal sealed record GeneratedStorageSnapshot(int Live, int Constructed, int Destroyed, int Calls, int Overflow, int[] Values, int[] Events);

// Implemented against the inspected production publication in CapturedBindings.
internal static partial class CapturedBindings
{
    internal static partial void Initialize();
    internal static partial void Shutdown();
    internal static partial int Fixture();
    internal static partial int Count();
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Constructed();
    internal static partial int Destroyed();
    internal static partial int Calls();
    internal static partial int Value(int index);
    internal static partial int Event(int index);
    internal static partial GeneratedStorageSnapshot SnapshotStorage();
    internal static partial ManagedSelectedProgramLifetimeDiagnostics Diagnostics();
    internal static partial nint FirstObjectAddress();
    internal static partial nint SecondObjectAddress();
    internal static partial JsonObject Physical();
}

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using Payload = Translated.Payload__3;

if (args.Length != 1) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON");
var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations");
var runs = new JsonArray();
var ownership = new JsonArray();
for (var run = 0; run < 2; ++run)
{
    var before = ManagedCppException.GetDiagnostics();
    var beforeRecord = ManagedCppException.GetRecordDiagnostics();
    var beforeReferences = ManagedCppException.GetRethrowDiagnostics();
    if (before.LiveOwners != 0 || beforeRecord.LiveOwners != 0 || beforeReferences.LiveReferences != 0)
        throw new InvalidOperationException("Fixture entered with a live payload owner or reference");

    var result = Application.fixture();

    var after = ManagedCppException.GetDiagnostics();
    var record = ManagedCppException.GetRecordDiagnostics();
    var references = ManagedCppException.GetRethrowDiagnostics();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var live = Application.lifetime_live();
    if (count is < 0 or > 64 || overflow != 0 || live != 0 || Application.lifetime_value(8) != 0)
        throw new InvalidOperationException("Class payload event, live, overflow or prior-live invariant failed");
    var values = new JsonArray();
    for (var index = 0; index < 9; ++index)
        values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
    var events = new JsonArray();
    for (var index = 0; index < count; ++index)
        events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
    runs.Add((JsonNode)new JsonObject
    {
        ["result"] = result, ["overflow"] = overflow, ["live"] = live,
        ["values"] = values, ["events"] = events
    });

    if (after.Created - before.Created != 1 || after.Committed - before.Committed != 1 ||
        after.Selected - before.Selected != 1 || after.Caught - before.Caught != 1 ||
        after.Released - before.Released != 1 || after.LiveOwners != 0 ||
        after.PendingReleased != before.PendingReleased || after.ProtocolRejections != before.ProtocolRejections ||
        references.Rethrown != beforeReferences.Rethrown || references.HandlingEntriesReleased - beforeReferences.HandlingEntriesReleased != 1 ||
        references.LiveReferences != 0)
        throw new InvalidOperationException("The generated class throw did not complete one balanced primary and handling lifecycle");
    if (record.Created - beforeRecord.Created != 1 || record.EvaluationCompleted - beforeRecord.EvaluationCompleted != 1 ||
        record.Committed - beforeRecord.Committed != 1 || record.DestructorStarted - beforeRecord.DestructorStarted != 1 ||
        record.DestructorCompleted - beforeRecord.DestructorCompleted != 1 || record.Released - beforeRecord.Released != 1 ||
        record.LiveOwners != 0 || record.LastPayloadSize != 16 || record.LastPayloadAlignment != 8 || !record.LastPayloadWasAligned)
        throw new InvalidOperationException("Record storage, evaluation, destructor or release observations differ from the admitted 16/8 owner");
    if (record.LastPrimaryOwner <= 0 || record.LastPrimaryOwner == beforeRecord.LastPrimaryOwner ||
        record.LastPrimaryOwner != references.LastCreatedPrimaryOwner || record.LastPrimaryOwner != references.LastReleasedPrimaryOwner)
        throw new InvalidOperationException("The observed record primary allocation and final release identities differ");

    var assertions = after.CleanupAssertions - before.CleanupAssertions;
    if (assertions < 1 || after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions != assertions ||
        record.LastCreatedSequence <= beforeRecord.LastReleasedSequence ||
        record.LastEvaluationCompletedSequence <= record.LastCreatedSequence ||
        record.LastCommittedSequence <= record.LastEvaluationCompletedSequence ||
        record.LastSelectedSequence <= record.LastCommittedSequence ||
        record.LastFirstCleanupSequence <= record.LastSelectedSequence ||
        record.LastDestructorStartedSequence <= record.LastFirstCleanupSequence ||
        record.LastDestructorCompletedSequence <= record.LastDestructorStartedSequence ||
        record.LastReleasedSequence <= record.LastDestructorCompletedSequence ||
        after.LastSelectedSequence != record.LastSelectedSequence ||
        after.LastFirstCleanupSequence != record.LastFirstCleanupSequence ||
        after.LastReleasedSequence != record.LastReleasedSequence)
        throw new InvalidOperationException("Record evaluation, commit, first-pass selection, cleanup, destruction and free were not observed in order");

    ownership.Add((JsonNode)new JsonObject
    {
        ["created"] = record.Created - beforeRecord.Created,
        ["evaluationCompleted"] = record.EvaluationCompleted - beforeRecord.EvaluationCompleted,
        ["committed"] = record.Committed - beforeRecord.Committed,
        ["selected"] = after.Selected - before.Selected, ["caught"] = after.Caught - before.Caught,
        ["destructorStarted"] = record.DestructorStarted - beforeRecord.DestructorStarted,
        ["destructorCompleted"] = record.DestructorCompleted - beforeRecord.DestructorCompleted,
        ["released"] = record.Released - beforeRecord.Released,
        ["handlingEntriesReleased"] = references.HandlingEntriesReleased - beforeReferences.HandlingEntriesReleased,
        ["pendingReleased"] = after.PendingReleased - before.PendingReleased,
        ["liveOwners"] = after.LiveOwners, ["recordLiveOwners"] = record.LiveOwners,
        ["liveReferences"] = references.LiveReferences,
        ["primaryOwner"] = record.LastPrimaryOwner,
        ["payloadSize"] = record.LastPayloadSize, ["payloadAlignment"] = record.LastPayloadAlignment,
        ["payloadWasAligned"] = record.LastPayloadWasAligned,
        ["unwindObservations"] = after.UnwindObservations - before.UnwindObservations,
        ["cleanupAssertions"] = assertions,
        ["selectionBeforeCleanupAssertions"] = after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions,
        ["protocolRejections"] = after.ProtocolRejections - before.ProtocolRejections,
        ["createdSequence"] = record.LastCreatedSequence,
        ["evaluationCompletedSequence"] = record.LastEvaluationCompletedSequence,
        ["committedSequence"] = record.LastCommittedSequence,
        ["selectedSequence"] = record.LastSelectedSequence,
        ["firstCleanupSequence"] = record.LastFirstCleanupSequence,
        ["destructorStartedSequence"] = record.LastDestructorStartedSequence,
        ["destructorCompletedSequence"] = record.LastDestructorCompletedSequence,
        ["releasedSequence"] = record.LastReleasedSequence
    });
}
if (!JsonNode.DeepEquals(runs[0], runs[1]))
    throw new InvalidOperationException("Repeated generated class-payload observations differ");
var observations = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observations, native))
    throw new InvalidOperationException("Generated class-payload observations differ from the separate native oracle");
Console.WriteLine(observations.ToJsonString());
Console.WriteLine(new JsonObject { ["recordExceptionOwnership"] = ownership }.ToJsonString());
Console.WriteLine(new JsonObject { ["classPayloadPhysical"] = ClassPayloadPhysical.Measure() }.ToJsonString());

internal static unsafe class ClassPayloadPhysical
{
    [StructLayout(LayoutKind.Sequential)]
    private struct AlignmentProbe
    {
        public byte Prefix;
        public Payload Value;
    }

    internal static JsonObject Measure()
    {
        Payload payload = default;
        var canaryOffset = (byte*)&payload.canary - (byte*)&payload;
        var valueOffset = (byte*)&payload.value - (byte*)&payload;
        var prefix = new AlignmentProbe { Prefix = 0, Value = default };
        var naturalAlignment = (byte*)&prefix.Value - (byte*)&prefix;
        if (sizeof(Payload) != 16 || canaryOffset != 0 || valueOffset != 8 || naturalAlignment != 8)
            throw new InvalidOperationException("Published complete Payload__3 size, field offsets or natural CLR alignment differ from the captured 16/8 representation");

        var providers = BorrowedNativeRttiData.GetDiagnostics();
        if (providers.Count != 1)
            throw new InvalidOperationException("The class payload requires exactly one observed borrowed RTTI data descriptor");
        var provider = providers[0];
        const string providerPath = "/usr/lib/x86_64-linux-gnu/libstdc++.so.6.0.33";
        const string providerHash = "1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11";
        const string exportName = "_ZTVN10__cxxabiv117__class_type_infoE";
        const ulong exportRva = 2546968;
        if (provider.CanonicalLibraryPath != providerPath || provider.LibrarySha256 != providerHash ||
            provider.ExportName != exportName || provider.PinnedProviderElfSymbolVersion != "CXXABI_1.3" ||
            provider.ExportRelativeVirtualAddress != exportRva || provider.SymbolSizeBytes != 88 ||
            !provider.LoadedOwnerVerified || !provider.ReadOnlyMappingVerified || provider.MappingPermissions != "r--p" ||
            provider.SymbolBaseAddress <= 0 || provider.ModuleLoadBaseAddress <= 0 ||
            (ulong)(nuint)provider.SymbolBaseAddress != checked((ulong)(nuint)provider.ModuleLoadBaseAddress + exportRva) ||
            provider.MappingStartAddress > provider.SymbolBaseAddress ||
            checked((ulong)(nuint)provider.SymbolBaseAddress + 88) > (ulong)(nuint)provider.MappingEndAddress)
            throw new InvalidOperationException("The observed class RTTI descriptor differs from its accepted pinned provider/owner/read-only contract");

        var generatedTypeinfo = Application.storage;
        if (generatedTypeinfo == null || ((nuint)generatedTypeinfo & 7) != 0)
            throw new InvalidOperationException("Generated class typeinfo storage is null or misaligned");
        var storedVptr = (nint)generatedTypeinfo->@base.__vptr;
        if (storedVptr != checked(provider.SymbolBaseAddress + 16))
            throw new InvalidOperationException("Generated class typeinfo did not retain the borrowed export base plus captured byte offset 16 exactly once");

        return new JsonObject
        {
            ["completePayloadSize"] = sizeof(Payload), ["canaryOffset"] = canaryOffset,
            ["valueOffset"] = valueOffset, ["sequentialPrefixOffset"] = naturalAlignment,
            ["subobjectCarrierClaimed"] = false,
            ["generatedTypeinfoAddress"] = Address((nint)generatedTypeinfo),
            ["storedTypeinfoVptr"] = Address(storedVptr), ["capturedAddressByteOffset"] = 16,
            ["provider"] = new JsonObject
            {
                ["canonicalLibraryPath"] = provider.CanonicalLibraryPath, ["librarySha256"] = provider.LibrarySha256,
                ["exportName"] = provider.ExportName, ["pinnedDefaultExportMetadataVersion"] = provider.PinnedProviderElfSymbolVersion,
                ["exportRva"] = provider.ExportRelativeVirtualAddress, ["elfSymbolSizeBytes"] = provider.SymbolSizeBytes,
                ["symbolBaseAddress"] = Address(provider.SymbolBaseAddress), ["moduleLoadBaseAddress"] = Address(provider.ModuleLoadBaseAddress),
                ["mappingPermissions"] = provider.MappingPermissions,
                ["loadedOwnerVerified"] = provider.LoadedOwnerVerified, ["readOnlyMappingVerified"] = provider.ReadOnlyMappingVerified,
                ["addressOnly"] = true, ["nativeRttiSlotInvoked"] = false
            }
        };
    }

    private static string Address(nint value) => "0x" + ((nuint)value).ToString("x", CultureInfo.InvariantCulture);
}

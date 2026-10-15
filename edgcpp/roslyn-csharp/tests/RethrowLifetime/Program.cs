using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;

if (args.Length != 1) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON");
var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations");
var runs = new JsonArray();
var ownership = new JsonArray();
for (var run = 0; run < 2; ++run)
{
    var before = ManagedCppException.GetDiagnostics();
    var beforeRethrow = ManagedCppException.GetRethrowDiagnostics();
    if (before.LiveOwners != 0 || beforeRethrow.LiveReferences != 0)
        throw new InvalidOperationException("Fixture entered with live exception ownership");
    var result = Application.fixture();
    var after = ManagedCppException.GetDiagnostics();
    var afterRethrow = ManagedCppException.GetRethrowDiagnostics();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var live = Application.lifetime_live();
    if (count is < 0 or > 64 || overflow != 0 || live != 0)
        throw new InvalidOperationException("Rethrow lifetime event bounds or live/overflow invariant failed");
    var values = new JsonArray();
    for (var index = 0; index < 5; ++index) values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
    if (Application.lifetime_value(4) != 0) throw new InvalidOperationException("Fixture entered with leaked prior-run objects");
    var events = new JsonArray();
    for (var index = 0; index < count; ++index) events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
    runs.Add((JsonNode)new JsonObject { ["result"] = result, ["overflow"] = overflow, ["live"] = live, ["values"] = values, ["events"] = events });

    if (after.Created - before.Created != 1 || after.Committed - before.Committed != 1 || after.Selected - before.Selected != 2 ||
        after.Caught - before.Caught != 2 || after.Released - before.Released != 1 || after.LiveOwners != 0 ||
        after.PendingReleased != before.PendingReleased || after.ProtocolRejections != before.ProtocolRejections)
        throw new InvalidOperationException("Rethrow did not preserve one primary allocation/commit/free across two handler entries");
    if (afterRethrow.Rethrown - beforeRethrow.Rethrown != 1 ||
        afterRethrow.HandlingEntriesReleased - beforeRethrow.HandlingEntriesReleased != 2 || afterRethrow.LiveReferences != 0 ||
        afterRethrow.LastCreatedPrimaryOwner <= beforeRethrow.LastCreatedPrimaryOwner ||
        afterRethrow.LastCreatedPrimaryOwner != afterRethrow.LastRethrownPrimaryOwner ||
        afterRethrow.LastCreatedPrimaryOwner != afterRethrow.LastReleasedPrimaryOwner)
        throw new InvalidOperationException("Rethrow propagation did not share and retire exactly the same primary owner");
    var assertions = after.CleanupAssertions - before.CleanupAssertions;
    if (assertions < 2 || after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions != assertions ||
        after.LastSelectedSequence <= before.LastReleasedSequence || after.LastFirstCleanupSequence <= after.LastSelectedSequence ||
        after.LastReleasedSequence <= after.LastFirstCleanupSequence)
        throw new InvalidOperationException("Rethrow handler selection did not precede cleanup and final release");
    ownership.Add((JsonNode)new JsonObject
    {
        ["created"] = after.Created - before.Created, ["committed"] = after.Committed - before.Committed,
        ["selected"] = after.Selected - before.Selected, ["caught"] = after.Caught - before.Caught,
        ["released"] = after.Released - before.Released, ["pendingReleased"] = after.PendingReleased - before.PendingReleased,
        ["liveOwners"] = after.LiveOwners, ["rethrown"] = afterRethrow.Rethrown - beforeRethrow.Rethrown,
        ["handlingEntriesReleased"] = afterRethrow.HandlingEntriesReleased - beforeRethrow.HandlingEntriesReleased,
        ["liveReferences"] = afterRethrow.LiveReferences, ["primaryOwner"] = afterRethrow.LastCreatedPrimaryOwner,
        ["rethrownPrimaryOwner"] = afterRethrow.LastRethrownPrimaryOwner, ["releasedPrimaryOwner"] = afterRethrow.LastReleasedPrimaryOwner,
        ["unwindObservations"] = after.UnwindObservations - before.UnwindObservations,
        ["cleanupAssertions"] = assertions, ["selectionBeforeCleanupAssertions"] = after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions,
        ["protocolRejections"] = after.ProtocolRejections - before.ProtocolRejections,
        ["selectionSequence"] = after.LastSelectedSequence, ["firstCleanupSequence"] = after.LastFirstCleanupSequence,
        ["releaseSequence"] = after.LastReleasedSequence
    });
}
if (!JsonNode.DeepEquals(runs[0], runs[1])) throw new InvalidOperationException("Repeated generated rethrow observations differ");
var observation = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observation, native)) throw new InvalidOperationException("Generated rethrow lifetimes differ from the separate native oracle");
Console.WriteLine(observation.ToJsonString());
Console.WriteLine(new JsonObject { ["exceptionOwnership"] = ownership }.ToJsonString());

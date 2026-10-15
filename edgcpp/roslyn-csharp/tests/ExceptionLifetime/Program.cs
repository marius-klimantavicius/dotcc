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
    if (before.LiveOwners != 0) throw new InvalidOperationException("Fixture entered with a live exception payload owner");
    var result = Application.fixture();
    var after = ManagedCppException.GetDiagnostics();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var live = Application.lifetime_live();
    if (count is < 0 or > 64 || overflow != 0 || live != 0)
        throw new InvalidOperationException("Exceptional lifetime event bounds or live/overflow invariant failed");
    var values = new JsonArray();
    for (var index = 0; index < 5; ++index) values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
    if (Application.lifetime_value(4) != 0) throw new InvalidOperationException("Fixture entered with leaked prior-run objects");
    var events = new JsonArray();
    for (var index = 0; index < count; ++index) events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
    runs.Add((JsonNode)new JsonObject { ["result"] = result, ["overflow"] = overflow, ["live"] = live, ["values"] = values, ["events"] = events });
    if (after.Created - before.Created != 1 || after.Committed - before.Committed != 1 || after.Selected - before.Selected != 1 ||
        after.Caught - before.Caught != 1 || after.Released - before.Released != 1 || after.LiveOwners != 0 ||
        after.PendingReleased != before.PendingReleased || after.ProtocolRejections != before.ProtocolRejections)
        throw new InvalidOperationException("The finite throw did not create, commit, select, catch and release exactly one payload owner");
    var assertions = after.CleanupAssertions - before.CleanupAssertions;
    if (assertions <= 0 || after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions != assertions ||
        after.LastSelectedSequence <= 0 || after.LastFirstCleanupSequence <= after.LastSelectedSequence ||
        after.LastReleasedSequence <= after.LastFirstCleanupSequence)
        throw new InvalidOperationException("First-pass handler selection was not observed before exceptional cleanup and release");
    ownership.Add((JsonNode)new JsonObject
    {
        ["created"] = after.Created - before.Created, ["committed"] = after.Committed - before.Committed,
        ["selected"] = after.Selected - before.Selected, ["caught"] = after.Caught - before.Caught,
        ["released"] = after.Released - before.Released, ["pendingReleased"] = after.PendingReleased - before.PendingReleased,
        ["liveOwners"] = after.LiveOwners, ["unwindObservations"] = after.UnwindObservations - before.UnwindObservations,
        ["cleanupAssertions"] = assertions, ["selectionBeforeCleanupAssertions"] = after.SelectionBeforeCleanupAssertions - before.SelectionBeforeCleanupAssertions,
        ["protocolRejections"] = after.ProtocolRejections - before.ProtocolRejections,
        ["selectionSequence"] = after.LastSelectedSequence, ["firstCleanupSequence"] = after.LastFirstCleanupSequence,
        ["releaseSequence"] = after.LastReleasedSequence
    });
}
if (!JsonNode.DeepEquals(runs[0], runs[1])) throw new InvalidOperationException("Repeated generated exceptional lifetime observations differ");
var observation = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observation, native)) throw new InvalidOperationException("Generated exceptional lifetimes differ from the separate native oracle");
// Preserve the native-compatible observation line; ownership diagnostics are
// separate because the native application does not expose this runtime carrier.
Console.WriteLine(observation.ToJsonString());
Console.WriteLine(new JsonObject { ["exceptionOwnership"] = ownership }.ToJsonString());

using System.Text.Json.Nodes;
using Application = Translated.Program;

if (args.Length != 1) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON");
var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations");
var runs = new JsonArray();
for (var run = 0; run < 2; ++run)
{
    var result = Application.fixture();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var live = Application.lifetime_live();
    if (count is < 0 or > 64 || overflow != 0 || live != 0)
        throw new InvalidOperationException("Lifetime event bounds or live/overflow invariant failed");
    var values = new JsonArray();
    for (var index = 0; index < 9; ++index) values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
    if (Application.lifetime_value(8) != 0) throw new InvalidOperationException("Fixture entered with leaked prior-run objects");
    var events = new JsonArray();
    for (var index = 0; index < count; ++index) events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
    runs.Add((JsonNode)new JsonObject { ["result"] = result, ["overflow"] = overflow, ["live"] = live, ["values"] = values, ["events"] = events });
}
if (!JsonNode.DeepEquals(runs[0], runs[1])) throw new InvalidOperationException("Repeated generated lifetime observations differ");
var observation = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observation, native)) throw new InvalidOperationException("Generated lifetime observations differ from the separate native oracle");
Console.WriteLine(observation.ToJsonString());

using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using Cell = Translated.Cell;

if (args.Length != 1) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON");
RuntimeContract.RequireLayout(8, true);
var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations");
var runs = new JsonArray();
for (var run = 0; run < 2; ++run)
{
    var result = Application.fixture();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var protocolLive = Application.lifetime_live();
    var priorProtocolLive = Application.lifetime_value(8);
    if (count is < 0 or > 64 || overflow != 0 || protocolLive != 0 || priorProtocolLive != 0)
        throw new InvalidOperationException("Mutable-reference event bounds, overflow or zero-live protocol check failed; Cell has no counted resources");
    var values = new JsonArray();
    for (var index = 0; index < 9; ++index)
        values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
    var events = new JsonArray();
    for (var index = 0; index < count; ++index)
        events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
    runs.Add((JsonNode)new JsonObject
    {
        ["result"] = result, ["overflow"] = overflow, ["live"] = protocolLive,
        ["values"] = values, ["events"] = events
    });
}
if (!JsonNode.DeepEquals(runs[0], runs[1]))
    throw new InvalidOperationException("Repeated generated mutable-reference observations differ");
var observations = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observations, native))
    throw new InvalidOperationException("Generated mutable-reference observations differ from the separate native oracle");
Console.WriteLine(observations.ToJsonString());
Console.WriteLine(new JsonObject { ["mutableReferencePhysical"] = MutableReferencePhysical.Measure() }.ToJsonString());

internal static unsafe class MutableReferencePhysical
{
    [StructLayout(LayoutKind.Sequential)]
    private struct AlignmentProbe
    {
        public byte Prefix;
        public Cell Value;
    }

    internal static JsonObject Measure()
    {
        Cell cell = default;
        var firstOffset = (byte*)&cell.first - (byte*)&cell;
        var secondOffset = (byte*)&cell.second - (byte*)&cell;
        var prefix = new AlignmentProbe { Prefix = 0, Value = default };
        var prefixOffset = (byte*)&prefix.Value - (byte*)&prefix;
        if (sizeof(Cell) != 8 || firstOffset != 0 || secondOffset != 4 || prefixOffset != 4)
            throw new InvalidOperationException("Published Cell size, actual field offsets or natural CLR alignment differ from the captured 8/4 representation");
        return new JsonObject
        {
            ["cellSize"] = sizeof(Cell), ["firstOffset"] = firstOffset,
            ["secondOffset"] = secondOffset, ["sequentialPrefixOffset"] = prefixOffset
        };
    }
}

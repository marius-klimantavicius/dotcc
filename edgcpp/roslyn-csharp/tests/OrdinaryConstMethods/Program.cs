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
    if (count is < 0 or > 64 || overflow != 0 || protocolLive != 0 || Application.lifetime_value(8) != 0)
        throw new InvalidOperationException("Ordinary const-method event bounds, overflow or zero-live protocol check failed; Cell has no counted resources");
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
    throw new InvalidOperationException("Repeated generated ordinary const-method observations differ");
var observations = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observations, native))
    throw new InvalidOperationException("Generated ordinary const-method observations differ from the separate native oracle");
Console.WriteLine(observations.ToJsonString());
Console.WriteLine(new JsonObject { ["ordinaryConstMethodPhysical"] = OrdinaryConstMethodPhysical.Measure() }.ToJsonString());

internal static unsafe class OrdinaryConstMethodPhysical
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
        var kindOffset = (byte*)&cell.kind - (byte*)&cell;
        var prefix = new AlignmentProbe { Prefix = 0, Value = default };
        var prefixOffset = (byte*)&prefix.Value - (byte*)&prefix;
        cell.kind = 201;
        byte unsignedHighBitValue = cell.kind;
        if (sizeof(Cell) != 1 || kindOffset != 0 || prefixOffset != 1 ||
            unsignedHighBitValue != 201 || *(byte*)&cell != 201)
            throw new InvalidOperationException("Published Cell size, kind offset, natural CLR alignment or unsigned-byte storage differs from the captured representation");
        return new JsonObject
        {
            ["cellSize"] = sizeof(Cell), ["kindOffset"] = kindOffset,
            ["sequentialPrefixOffset"] = prefixOffset, ["unsignedHighBitValue"] = unsignedHighBitValue
        };
    }
}

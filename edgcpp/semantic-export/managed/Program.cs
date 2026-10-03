using System.Text.Json;
using Edg.Semantic;

try
{
    if (args.Length < 2 || args[0] is not ("inspect" or "assert-probe"))
    {
        Console.Error.WriteLine("Usage: Edg.Semantic inspect|assert-probe <package directory|package.json|graph.json> [--strict-emission]");
        return 2;
    }
    if (args.Skip(2).Any(a => a != "--strict-emission")) throw new ArgumentException("Unknown argument");
    using var graph = SemanticGraph.Load(args[1], args.Contains("--strict-emission"));
    var assertions = args[0] == "assert-probe" ? ProbeAssertions.Verify(graph) : Array.Empty<string>();
    using var output = new Utf8JsonWriter(Console.OpenStandardOutput(), new JsonWriterOptions { Indented = true });
    output.WriteStartObject();
    output.WriteString("schema", "edg-semantic-inspection/1");
    output.WriteString("graph", graph.GraphPath);
    output.WriteBoolean("completeForEmission", graph.CompleteForEmission);
    output.WriteNumber("nodes", graph.Nodes.Count);
    output.WriteNumber("diagnostics", graph.Diagnostics.Count());
    output.WriteNumber("macroInvocations", graph.MacroInvocations.Count());
    output.WriteStartObject("nodeKinds");
    foreach (var group in graph.Nodes.Values.GroupBy(n => n.Kind).OrderBy(g => g.Key)) output.WriteNumber(group.Key, group.Count());
    output.WriteEndObject();
    output.WriteStartArray("directCalls");
    foreach (var call in graph.OfKind("expr-node"))
    {
        var target = graph.DirectCallTarget(call);
        if (target == null) continue;
        output.WriteStartObject();
        output.WriteNumber("expression", call.Id);
        output.WriteNumber("routine", target.Id);
        output.WriteString("name", target.Name);
        output.WriteBoolean("virtual", call.Flag("virtualCall"));
        output.WriteEndObject();
    }
    output.WriteEndArray();
    output.WriteStartArray("assertions");
    foreach (var assertion in assertions) output.WriteStringValue(assertion);
    output.WriteEndArray();
    output.WriteEndObject();
    output.Flush();
    Console.WriteLine();
    return 0;
}
catch (Exception error) when (error is InvalidDataException or IOException or JsonException or InvalidOperationException or KeyNotFoundException or FormatException or ArgumentException or OverflowException)
{
    Console.Error.WriteLine(error.GetType().Name + ": " + error.Message);
    return 1;
}

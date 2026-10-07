using System.Text.Json;
using Edg.Semantic;

try
{
    if (args.Length < 2 || args[0] is not ("inspect" or "assert-probe" or "emit-csharp" or "emit-project"))
    {
        Console.Error.WriteLine("Usage: Edg.Semantic inspect|assert-probe <package directory|package.json|graph.json> [--strict-emission]");
        Console.Error.WriteLine("       Edg.Semantic emit-csharp <package directory|package.json> --output FILE [--class NAME]");
        Console.Error.WriteLine("       Edg.Semantic emit-project <package directory|package.json> --output-directory DIRECTORY");
        return 2;
    }
    if (args[0] == "emit-project")
    {
        if (args.Length != 4 || args[2] != "--output-directory")
            throw new ArgumentException("Usage: Edg.Semantic emit-project PACKAGE --output-directory DIRECTORY");
        using var projectGraph = SemanticGraph.LoadForCEmission(args[1]);
        new CProjectEmitter(projectGraph).Emit(args[3]);
        Console.WriteLine("Emitted C project to " + Path.GetFullPath(args[3]));
        return 0;
    }
    if (args[0] == "emit-csharp")
    {
        string? outputPath = null;
        string className = "SemanticModule";
        for (int i = 2; i < args.Length; i++)
        {
            if (i + 1 >= args.Length) throw new ArgumentException("Missing option value: " + args[i]);
            switch (args[i])
            {
                case "--output" when outputPath == null: outputPath = args[++i]; break;
                case "--class": className = args[++i]; break;
                default: throw new ArgumentException("Unknown or duplicate option: " + args[i]);
            }
        }
        if (outputPath == null) throw new ArgumentException("emit-csharp requires --output FILE");
        using var emissionGraph = SemanticGraph.LoadForCEmission(args[1]);
        var source = new CSharpEmitter(emissionGraph, className).Emit();
        var destination = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, source);
        Console.WriteLine($"Emitted {destination} using bounded profile {CSharpEmitter.Profile}; completeForEmission remains {emissionGraph.CompleteForEmission.ToString().ToLowerInvariant()}.");
        return 0;
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

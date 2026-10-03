namespace Edg.Semantic;

internal static class ProbeAssertions
{
    public static IReadOnlyList<string> Verify(SemanticGraph graph)
    {
        var passed = new List<string>();
        void Require(bool condition, string description)
        {
            if (!condition) throw new InvalidDataException("Probe assertion failed: " + description);
            passed.Add(description);
        }
        var cpp = graph.OfKind("routine").Any(n => n.Name == "work");
        var entry = graph.OfKind("routine").Single(n => n.Name == (cpp ? "work" : "sum") && !n.Flag("prototypeInstance"));
        var scope = entry.Reference("bodyScope");
        Require(scope != null && scope.Reference("body")?.Kind == "statement", "Entry routine links to structured body");
        var parameters = graph.Linked(scope!.ReferenceId("parameters")).ToArray();
        Require(parameters.Length == (cpp ? 1 : 2) && parameters.All(p => p.Kind == "variable" && p.Flag("parameter") && p.Type != null), "Parameters retain symbol and type identities");
        Require(graph.OfKind("statement").Any(n => n.Form == "stmk_for") && graph.OfKind("statement").Any(n => n.Form == "stmk_if"), "For and if statements survive without lowering");
        Require(graph.OfKind("expr-node").Any(n => n.Type != null && n.Raw.TryGetProperty("location", out var loc) && loc.GetProperty("expansion").GetProperty("line").GetInt32() > 0), "Typed expressions retain source locations");
        Require(graph.SourceOrder(scope).Any(), "Function scope preserves source sequence");
        Require(graph.OfKind("macro").Any(n => n.Name == "API_VERSION"), "Public macro definition retained");
        Require(graph.MacroInvocations.Any(), "Macro invocation records retained");
        if (!cpp)
        {
            Require(graph.OfKind("field").Any(n => n.Name == "value" && n.Type != null), "C member name and type retained");
            Require(graph.OfKind("statement").Any(n => n.Text("sourceForm") == "continue"), "Continue label origin retained");
            return passed;
        }
        var calls = graph.OfKind("expr-node").Select(graph.DirectCallTarget).Where(n => n != null).Cast<SemanticNode>().ToArray();
        var chosen = calls.Where(n => n.Name == "choose").ToArray();
        Require(chosen.Length > 0 && chosen.All(n => IntegerKind(ReturnType(n)) == "ik_int"), "Overload selection resolves choose(int), not choose(long)");
        Require(calls.Any(n => n.Name == "twice" && n.Flag("templateInstance") && !n.Flag("prototypeInstance") && graph.Linked(n.ReferenceId("templateArguments")).Any(a => IntegerKind(a.Type) == "ik_int")), "Direct call binds instantiated twice<int>");
        var box = graph.OfKind("type").Single(n => n.Name == "Box" && n.Flag("templateInstance") && !n.Flag("prototypeInstance") && !n.Flag("nonreal"));
        var boxInfo = box.Reference("recordInfo")!;
        Require(graph.Linked(boxInfo.ReferenceId("templateArguments")).Any(a => Canonical(a.Type)?.Form == "tk_pointer") &&
            boxInfo.Reference("template")?.Text("text")?.Contains("Box < T * >", StringComparison.Ordinal) == true &&
            calls.Any(n => n.Name == "read" && n.ReferenceId("parentScope") == boxInfo.ReferenceId("scope") && !n.Flag("prototypeInstance")),
            "Box<int*>::read resolves to the instantiated pointer partial specialization");
        var destructions = graph.OfKind("dynamic-init").Where(n => n.Reference("destructor") != null).ToArray();
        foreach (var name in new[] { "first", "inner", "conditional" })
            Require(destructions.Any(n => n.Reference("variable")?.Name == name && n.Reference("lifetime") != null), "RAII destructor and lifetime recorded for " + name);
        Require(graph.OfKind("object-lifetime").Any(n => n.Reference("parentLifetime") != null), "Nested destruction boundaries retained");
        Require(graph.OfKind("statement").Any(n => n.Form == "stmk_try_block") && graph.OfKind("handler").Any(), "Structured exception handler retained");
        Require(graph.MacroInvocations.Any(m => m.GetProperty("parentIndex").GetInt64() > 0), "Nested macro expansion provenance retained");
        Require(graph.OfKind("macro").Any(n => n.Name == "API_VERSION" && n.Flag("undef")), "Macro undefinition retained");
        return passed;
    }

    private static SemanticNode? ReturnType(SemanticNode routine) => Canonical(routine.Type)?.Reference("returnType");
    private static string? IntegerKind(SemanticNode? type) => Canonical(type)?.Text("integerKind");
    private static SemanticNode? Canonical(SemanticNode? type)
    {
        var seen = new HashSet<ulong>();
        while (type?.Form == "tk_typeref" && seen.Add(type.Id)) type = type.Reference("underlyingType");
        return type;
    }
}

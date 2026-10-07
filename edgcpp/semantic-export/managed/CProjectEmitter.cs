using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Edg.Semantic;

/// <summary>Project C emission from named EDG semantic records.</summary>
public sealed partial class CProjectEmitter
{
    private readonly SemanticGraph graph;
    private readonly Dictionary<ulong, string> names = new();
    private readonly Dictionary<ulong, string> typeNames = new();
    private readonly HashSet<string> memberNames = new(StringComparer.Ordinal);
    private readonly HashSet<ulong> recordTypes = new();
    private readonly HashSet<ulong> arrayTypes = new();
    private readonly Dictionary<ulong, SemanticNode> globals = new();
    private readonly Dictionary<ulong, byte[]> strings = new();
    private readonly HashSet<ulong> externalRoutines = new();
    private readonly Dictionary<ulong, SemanticNode> localStaticInitializers = new();
    private Dictionary<ulong, SemanticNode[]> localsByRoutine = new();
    private SemanticNode? currentRoutine;
    private SemanticNode? returnType;
    private readonly StringBuilder body = new();
    private int indent;
    private int switchSerial;
    private readonly Stack<string> breakTargets = new();
    private readonly Stack<string> continueTargets = new();
    private readonly Dictionary<ulong, string> labelNames = new();
    private readonly HashSet<ulong> activeStatements = new();
    private readonly HashSet<ulong> unstructuredControls = new();

    public CProjectEmitter(SemanticGraph graph) { this.graph = graph; }

    public void Emit(string destination)
    {
        if (graph.Raw.GetProperty("sourceLanguage").GetString() != "C" ||
            graph.Raw.GetProperty("pointerBytes").GetInt32() != 8 || !graph.Raw.GetProperty("littleEndian").GetBoolean())
            throw new InvalidDataException("The C project profile requires unlowered little-endian LP64 C.");
        if (graph.Diagnostics.Any())
            throw new InvalidDataException("C project export still has adapter diagnostics: " + string.Join(", ", graph.Diagnostics.Select(d => d.GetProperty("message").GetString()).Distinct()));
        var routines = graph.OfKind("routine").Where(n => n.Flag("defined") && n.ReferenceId("bodyScope") != 0).ToArray();
        foreach (var variable in graph.OfKind("variable"))
            if (variable.Flag("threadLocal") || variable.ReferenceId("cleanupRoutine") != 0)
                Fail(variable, "TLS and cleanup attributes require separate project lowering");
        foreach (var n in graph.OfKind("routine")) Allocate(n, memberNames);
        var referencedVariables = graph.OfKind("expr-node").Where(n => n.Form == "variable").Select(n => n.ReferenceId("symbol")).Concat(graph.OfKind("constant").Where(n => n.Form == "ck_address").Select(n => n.ReferenceId("variable"))).ToHashSet();
        foreach (var n in graph.OfKind("variable").Where(n => n.Text("storageClassName") != "sc_extern" || n.Text("initializationKind") != "initk_none" || referencedVariables.Contains(n.Id)))
            if (n.Text("storageClassName") is "sc_static" || n.ReferenceId("enclosingRoutine") == 0)
            { globals[n.Id] = n; Allocate(n, memberNames); }
        foreach (var group in graph.OfKind("variable").Where(n => !globals.ContainsKey(n.Id)).GroupBy(n => n.ReferenceId("enclosingRoutine")))
        {
            var used = new HashSet<string>(memberNames, StringComparer.Ordinal);
            foreach (var n in group) Allocate(n, used);
        }
        foreach (var n in graph.OfKind("local-static-variable-init"))
            localStaticInitializers[n.ReferenceId("variable")] = n;
        foreach (var n in graph.OfKind("label")) labelNames[n.Id] = "__label_" + n.Id;
        localsByRoutine = graph.OfKind("variable").Where(n => !n.Flag("parameter") && !IsGlobal(n)).GroupBy(n => n.ReferenceId("enclosingRoutine")).ToDictionary(g => g.Key, g => g.ToArray());
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        int index = 0;
        foreach (var routine in routines)
        {
            body.Clear(); indent = 1;
            EmitRoutine(routine);
            files[$"Functions/{index++:D4}-{SafeFile(routine.Name)}.cs"] = Header + body + Footer;
        }
        body.Clear(); indent = 1;
        EmitGlobals();
        files["Globals.cs"] = Header + body + Footer;
        files["Runtime.cs"] = RuntimeSource();
        body.Clear(); indent = 1;
        foreach (var id in recordTypes.Order().ToArray()) EmitRecord(graph.Get(id)!);
        foreach (var id in arrayTypes.Order().ToArray()) EmitArray(graph.Get(id)!);
        files["Types.cs"] = Header + body + Footer;
        body.Clear(); indent = 1;
        foreach (var helper in ExpressionHelpers) Line(helper);
        files["ExpressionHelpers.cs"] = Header + body + Footer;
        if (UsesFloat80)
        {
            using var resource = typeof(CProjectEmitter).Assembly.GetManifestResourceStream("Edg.Semantic.Float80.cs")
                ?? throw new InvalidOperationException("Missing binary80 runtime resource");
            using var reader = new StreamReader(resource);
            files["Float80.cs"] = reader.ReadToEnd();
        }
        files["Generated.csproj"] = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings><IsAotCompatible>true</IsAotCompatible>
    <CheckForOverflowUnderflow>false</CheckForOverflowUnderflow>
    <NoWarn>$(NoWarn);CS0162;CS0164;CS0219;CS0414;CS0649;CS8981</NoWarn>
  </PropertyGroup>
</Project>
""";
        var full = Path.GetFullPath(destination);
        if (Directory.Exists(full)) throw new IOException("Output directory must be fresh: " + full);
        Directory.CreateDirectory(full);
        foreach (var file in files)
        {
            var path = Path.Combine(full, file.Key); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, file.Value);
        }
        using var manifest = new Utf8JsonWriter(File.Create(Path.Combine(full, "semantic-emission.json")), new JsonWriterOptions { Indented = true });
        manifest.WriteStartObject(); manifest.WriteString("schema", "edg-c-project-emission/1");
        manifest.WriteString("graph", graph.GraphPath); manifest.WriteNumber("routines", routines.Length);
        manifest.WriteNumber("globals", globals.Count); manifest.WriteNumber("records", recordTypes.Count);
        manifest.WriteNumber("arrays", arrayTypes.Count); manifest.WriteStartArray("externalRoutines");
        foreach (var id in externalRoutines.Order()) manifest.WriteStringValue(graph.Get(id)!.Name);
        manifest.WriteEndArray(); manifest.WriteEndObject(); manifest.Flush();
    }

    private const string Header = "// <auto-generated/>\n// From the EDG semantic graph.\nusing System;\nusing System.Runtime.CompilerServices;\nusing System.Runtime.InteropServices;\nnamespace Edg.Semantic.Generated;\npublic static unsafe partial class SqliteCore\n{\n";
    private const string Footer = "}\n";
    private static string SafeFile(string? name) => new((name ?? "anonymous").Select(c => char.IsAsciiLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
    private static string Identifier(string name) => "@" + name;
    private string Allocate(SemanticNode node, HashSet<string> used)
    {
        if (names.TryGetValue(node.Id, out var value)) return value;
        var name = SafeFile(node.Name); if (name.Length == 0) name = "anonymous"; if (name == "_") name = "__source_discard";
        if (!used.Add(name)) { name += "_" + node.Id; while (!used.Add(name)) name += "_"; }
        return names[node.Id] = Identifier(name);
    }
    private string Name(SemanticNode node) => names.TryGetValue(node.Id, out var name) ? name : Allocate(node, memberNames);
    private static ulong Num(SemanticNode node, string field) => node.Raw.GetProperty(field).GetUInt64();
    private static SemanticNode Need(SemanticNode node, string field) => node.Reference(field) ?? throw new InvalidDataException($"node {node.Id} ({node.Kind}/{node.Form}) missing {field}");
    private static void Fail(SemanticNode node, string message) => throw new InvalidDataException($"node {node.Id} ({node.Kind}/{node.Form}, {node.Name}): {message}");
    private SemanticNode Canonical(SemanticNode node)
    {
        var seen = new HashSet<ulong>();
        while (node.Form == "tk_typeref") { if (!seen.Add(node.Id)) Fail(node, "cyclic typedef"); node = Need(node, "underlyingType"); }
        return node;
    }
    private bool IsGlobal(SemanticNode node) => globals.ContainsKey(node.Id);
    private string VariableValue(SemanticNode node)
    {
        var type = Canonical(Need(node, "type"));
        if (IsGlobal(node)) return type.Form == "tk_array" ? "((" + TypeName(Need(type, "elementType")) + "*)" + Name(node) + ")" : "(*" + Name(node) + ")";
        return Name(node);
    }
    private string VariableAddress(SemanticNode node) => IsGlobal(node) ? Name(node) : "(&" + Name(node) + ")";
    private void Line(string value = "") { if (value.Length != 0) body.Append(' ', indent * 4).Append(value); body.AppendLine(); }

    private void EmitRoutine(SemanticNode routine)
    {
        currentRoutine = routine;
        var type = Canonical(Need(routine, "type")); var sig = Need(type, "signature");
        returnType = Need(type, "returnType");
        var scope = Need(routine, "bodyScope");
        var parameters = graph.Linked(scope.ReferenceId("parameters")).ToArray();
        var args = parameters.Select(n => TypeName(Need(n, "type")) + " " + Name(n)).ToList();
        if (sig.Flag("variadic")) args.Add("ReadOnlySpan<__CArg> __args");
        Line($"// semantic routine {routine.Id}: {routine.Name}");
        Line("public static " + TypeName(returnType) + " " + Name(routine) + "(" + string.Join(", ", args) + ")");
        Line("{"); indent++;
        Line("unchecked"); Line("{"); indent++;
        if (sig.Flag("variadic")) { Line("fixed (__CArg* __argData = __args)"); Line("{"); indent++; }
        foreach (var local in localsByRoutine.GetValueOrDefault(routine.Id, Array.Empty<SemanticNode>()))
            Line(TypeName(Need(local, "type")) + " " + Name(local) + " = default;");
        PlanJumpScopes(Need(scope, "body"));
        EmitStatement(Need(scope, "body"));
        // EDG retains unreachable tails; C requires a return value only when used.
        if (Canonical(returnType).Form != "tk_void") Line("return default;");
        if (sig.Flag("variadic")) { indent--; Line("}"); }
        indent--; Line("}"); indent--; Line("}");
    }

    // C permits a goto to enter an if/loop body; C# does not. Lower only
    // controls whose lexical scope a jump actually enters, preserving all others.
    private void PlanJumpScopes(SemanticNode root)
    {
        unstructuredControls.Clear();
        var targets = new Dictionary<ulong, (ulong Scope, ulong Control)[]>();
        var jumps = new List<(ulong Target, (ulong Scope, ulong Control)[] Path)>();
        var path = new List<(ulong Scope, ulong Control)>();
        var visiting = new HashSet<ulong>();
        void ControlledVisit(SemanticNode child, ulong control)
        {
            path.Add((child.Id, control)); Visit(child); path.RemoveAt(path.Count - 1);
        }
        void Visit(SemanticNode n)
        {
            if (!visiting.Add(n.Id)) Fail(n, "cyclic statement tree during jump analysis");
            switch (n.Form)
            {
                case "stmk_block":
                    foreach (var child in graph.Linked(n.ReferenceId("statements"))) Visit(child);
                    break;
                case "stmk_if":
                    ControlledVisit(Need(n, "then"), n.Id);
                    if (n.Reference("else") is { } other) ControlledVisit(other, n.Id);
                    break;
                case "stmk_for":
                    if (Need(n, "loopInfo").Reference("initialization") is { } initial) Visit(initial);
                    ControlledVisit(Need(n, "body"), n.Id); break;
                case "stmk_while": case "stmk_end_test_while":
                    ControlledVisit(Need(n, "body"), n.Id); break;
                case "stmk_switch":
                    foreach (var entry in graph.Linked(Need(n, "switchInfo").ReferenceId("cases")))
                        jumps.Add((Need(entry, "statement").Id, path.ToArray()));
                    if (Need(n, "switchInfo").Reference("defaultCase") is { } fallback)
                        jumps.Add((Need(fallback, "statement").Id, path.ToArray()));
                    Visit(Need(n, "body")); break;
                case "stmk_switch_case": targets[n.Id] = path.ToArray(); break;
                case "stmk_label": targets[Need(n, "label").Id] = path.ToArray(); break;
                case "stmk_goto":
                    var label = Need(n, "label");
                    if (!label.Flag("break") && !label.Flag("continue")) jumps.Add((label.Id, path.ToArray()));
                    break;
            }
            visiting.Remove(n.Id);
        }
        Visit(root);
        foreach (var jump in jumps)
            if (targets.TryGetValue(jump.Target, out var targetPath))
                foreach (var scope in targetPath)
                    if (!jump.Path.Contains(scope)) unstructuredControls.Add(scope.Control);
    }

    private void EmitStatement(SemanticNode statement)
    {
        if (!activeStatements.Add(statement.Id)) Fail(statement, "cyclic statement tree");
        switch (statement.Form)
        {
            case "stmk_block":
                foreach (var child in graph.Linked(statement.ReferenceId("statements"))) EmitStatement(child);
                break;
            case "stmk_decl": break; // Declarations are hoisted for C's jumps into nested scopes.
            case "stmk_init":
                var init = Need(statement, "initialization");
                if (init.Reference("variable") is { } variable && !IsGlobal(variable)) EmitInitialization(VariableAddress(variable), Need(variable, "type"), init);
                break;
            case "stmk_expr":
                if (statement.Reference("expression") is { } expression) Line(StatementExpr(expression) + ";");
                break;
            case "stmk_return":
                if (statement.Reference("expression") is { } result)
                    Line("return " + ConvertValue(Expr(result), Need(result, "type"), returnType!) + ";");
                else Line("return;");
                break;
            case "stmk_if":
                if (unstructuredControls.Contains(statement.Id))
                {
                    var alternate = "__else_" + statement.Id; var done = "__endif_" + statement.Id;
                    Line("if (!(" + Condition(Need(statement, "expression")) + ")) goto " + alternate + ";");
                    EmitStatement(Need(statement, "then")); Line("goto " + done + ";");
                    Line(alternate + ":;");
                    if (statement.Reference("else") is { } alternative) EmitStatement(alternative);
                    Line(done + ":;"); break;
                }
                Line("if (" + Condition(Need(statement, "expression")) + ")"); Controlled(Need(statement, "then"));
                if (statement.Reference("else") is { } other) { Line("else"); Controlled(other); }
                break;
            case "stmk_for":
                var loop = Need(statement, "loopInfo");
                if (loop.Reference("initialization") is { } initial) EmitStatement(initial);
                var forBreak = "__break_" + statement.Id; var forContinue = "__continue_" + statement.Id;
                var forCondition = statement.Reference("expression") is { } test ? Condition(test) : "";
                var forStep = loop.Reference("increment") is { } step ? StatementExpr(step) : "";
                if (unstructuredControls.Contains(statement.Id))
                {
                    var head = "__loop_" + statement.Id;
                    Line(head + ":;");
                    if (forCondition.Length != 0) Line("if (!(" + forCondition + ")) goto " + forBreak + ";");
                    breakTargets.Push(forBreak); continueTargets.Push(forContinue);
                    EmitStatement(Need(statement, "body")); continueTargets.Pop(); breakTargets.Pop();
                    Line(forContinue + ":;"); if (forStep.Length != 0) Line(forStep + ";");
                    Line("goto " + head + ";"); Line(forBreak + ":;"); break;
                }
                Line("for (; " + forCondition + "; " + forStep + ")"); Line("{"); indent++;
                breakTargets.Push(forBreak); continueTargets.Push(forContinue); EmitStatement(Need(statement, "body")); continueTargets.Pop(); breakTargets.Pop();
                Line(forContinue + ":;");
                indent--; Line("}"); Line(forBreak + ":;"); break;
            case "stmk_while": case "stmk_end_test_while":
                var breakName = "__break_" + statement.Id; var continueName = "__continue_" + statement.Id;
                if (unstructuredControls.Contains(statement.Id))
                {
                    var head = "__loop_" + statement.Id;
                    Line(head + ":;");
                    if (statement.Form == "stmk_while") Line("if (!(" + Condition(Need(statement, "expression")) + ")) goto " + breakName + ";");
                    breakTargets.Push(breakName); continueTargets.Push(continueName);
                    EmitStatement(Need(statement, "body")); continueTargets.Pop(); breakTargets.Pop();
                    Line(continueName + ":;");
                    Line((statement.Form == "stmk_end_test_while" ? "if (" + Condition(Need(statement, "expression")) + ") " : "") + "goto " + head + ";");
                    Line(breakName + ":;"); break;
                }
                if (statement.Form == "stmk_while") Line("while (" + Condition(Need(statement, "expression")) + ")"); else Line("do");
                Line("{"); indent++; breakTargets.Push(breakName); continueTargets.Push(continueName);
                EmitStatement(Need(statement, "body")); continueTargets.Pop(); breakTargets.Pop(); Line(continueName + ":;"); indent--; Line("}");
                if (statement.Form == "stmk_end_test_while") Line("while (" + Condition(Need(statement, "expression")) + ");");
                Line(breakName + ":;"); break;
            case "stmk_goto":
                var label = Need(statement, "label");
                if (label.Flag("break") && breakTargets.Count != 0) Line("goto " + breakTargets.Peek() + ";");
                else if (label.Flag("continue") && continueTargets.Count != 0) Line("goto " + continueTargets.Peek() + ";");
                else Line("goto " + labelNames[label.Id] + ";");
                break;
            case "stmk_label":
                var target = Need(statement, "label");
                if (!target.Flag("break") && !target.Flag("continue")) Line(labelNames[target.Id] + ":;");
                break;
            case "stmk_switch": EmitSwitch(statement); break;
            case "stmk_switch_case": Line("__case_" + statement.Id + ":;"); break;
            case "stmk_empty": Line(";"); break;
            default: Fail(statement, "unsupported project statement"); break;
        }
        activeStatements.Remove(statement.Id);
    }
    private void Controlled(SemanticNode statement) { Line("{"); indent++; EmitStatement(statement); indent--; Line("}"); }
    private void EmitSwitch(SemanticNode statement)
    {
        var descriptor = Need(statement, "switchInfo"); var expression = Need(statement, "expression");
        var name = "__switch_" + switchSerial++; var end = "__break_" + statement.Id;
        Line(TypeName(Need(expression, "type")) + " " + name + " = " + Expr(expression) + ";");
        foreach (var entry in graph.Linked(descriptor.ReferenceId("cases")))
        {
            if (entry.Reference("caseValue") is not { } value) continue;
            var condition = name + " == " + Constant(value);
            if (entry.Reference("rangeEnd") is { } last) condition = name + " >= " + Constant(value) + " && " + name + " <= " + Constant(last);
            Line("if (" + condition + ") goto __case_" + Need(entry, "statement").Id + ";");
        }
        var defaultCase = descriptor.Reference("defaultCase");
        Line("goto " + (defaultCase == null ? end : "__case_" + Need(defaultCase, "statement").Id) + ";");
        breakTargets.Push(end); EmitSwitchBody(Need(statement, "body")); breakTargets.Pop(); Line(end + ":;");
    }
    private void EmitSwitchBody(SemanticNode statement)
    {
        // Switch dispatch can jump into C lexical blocks. Locals were hoisted,
        // so flatten these scopes to make the same label targets legal in C#.
        if (statement.Form == "stmk_block") { foreach (var child in graph.Linked(statement.ReferenceId("statements"))) EmitSwitchBody(child); }
        else EmitStatement(statement);
    }
}

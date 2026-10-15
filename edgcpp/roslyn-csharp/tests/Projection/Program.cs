using RoslynObj;
using RoslynObj.CSharp;
using System.Text.Json;
using System.Globalization;

if (args.Length < 2) throw new ArgumentException("inspect OBJECT | emit OBJECT OUTPUT RUNTIME_PROJECT");
using var input = RoslynObject.Open(args[1]);
if (args[0] == "inspect")
{
    foreach (var function in input.TranslationUnit.Functions)
    {
        Console.WriteLine($"FUNCTION {function.Name} state={function.BodyState} defined={function.IsDefined} instance={function.Instantiation is not null}");
        foreach (var parameter in function.Parameters) Console.WriteLine($"  PARAM {parameter.Name} {parameter.Type} role={parameter.Role} symbol={parameter.Symbol?.Name}");
        if (function.Body is { } body)
        {
            foreach (var local in body.Locals) Console.WriteLine($"  LOCAL {local.Name} {local.Type} constant={local.ConstantInitializer?.Value.Kind} init={local.Initializer?.Kind}");
            Inspect(body.Root, "  ", new HashSet<SemanticNode>());
        }
    }
    foreach (var type in input.TranslationUnit.Types.OfType<ArithmeticType>())
        Console.WriteLine($"TYPE {type.Name} {type.Kind} {type.Arithmetic.ArithmeticKind} bits={type.Arithmetic.Bits} signed={type.Arithmetic.Signed} size={type.Size}");
}
else if (args[0] == "verify")
{
    var definitions = input.TranslationUnit.Functions.Where(f => f.BodyState.Name == "definition").ToArray();
    var concreteInstances = definitions.Where(f => f.Instantiation is not null).ToArray();
    var records = input.TranslationUnit.Types.OfType<RecordType>().Where(t => t.Layout is not null).ToArray();
    if (args[2] == "arithmetic")
    {
        if (definitions.Length != 2 || concreteInstances.Length != 0) throw new InvalidOperationException("Unexpected C fixture body classification.");
        var pair = records.Single(t => t.Name == "Pair");
        if (pair.Size != 8 || pair.Alignment != 4) throw new InvalidOperationException("Pair size/alignment did not match the captured fixture contract.");
        var fields = pair.Layout!.Fields;
        if (fields.Count != 2 || fields[0].Field.Name != "value" || fields[0].Offset != 0 || fields[1].Field.Name != "tag" || fields[1].Offset != 4)
            throw new InvalidOperationException("Pair field layout did not match captured EDG facts.");
    }
    else if (args[2] == "resolved-template")
    {
        if (definitions.Length != 5 || concreteInstances.Length != 4 || concreteInstances.Any(f => f.Parameters.Any(p => p.Type.IsDependent)))
            throw new InvalidOperationException("Expected four concrete EDG template bodies and one fixture body.");
        if (!concreteInstances.SelectMany(f => f.Parameters).OfType<Parameter>().Select(p => p.Type).OfType<ArithmeticType>().Any(t => t.Arithmetic.Signed == true) ||
            !concreteInstances.SelectMany(f => f.Parameters).Select(p => p.Type).OfType<ArithmeticType>().Any(t => t.Arithmetic.Signed == false))
            throw new InvalidOperationException("Both selected signed/unsigned concrete instantiations must be present.");
    }
    else throw new ArgumentException("Unknown fixture contract.");
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        fixture = args[2], artifact = input.Manifest.ArtifactIdentity,
        target = new { abi = input.Manifest.Target.Abi, addressableUnitBits = input.Manifest.Target.AddressableUnitBits,
            endianness = input.Manifest.Target.Endianness,
            pointerBits = input.Manifest.Target.PointerBits.ToString(CultureInfo.InvariantCulture),
            pointerAlignment = input.Manifest.Target.PointerAlignment.ToString(CultureInfo.InvariantCulture) },
        executableDefinitions = definitions.Length,
        concreteTemplateBodies = concreteInstances.Length,
        functions = definitions.Select(f => new { name = f.Name, state = f.BodyState.Name, instantiated = f.Instantiation is not null,
            parameters = f.Parameters.Select(ParameterContract) }),
        records = records.Select(r => new { name = r.Name, size = r.Size?.ToString(), alignment = r.Alignment?.ToString(),
            fields = r.Layout!.Fields.Select(f => new { name = f.Field.Name, offset = f.Offset?.ToString() }) }),
        semanticInput = "public RoslynObj only; source files are never passed to the emitter"
    }, new JsonSerializerOptions { WriteIndented = true }));
}
else if (args[0] == "emit")
{
    var result = new CSharpEmitter().Emit([input], new CSharpEmissionOptions { Executable = true, EntryPointName = "fixture", PrintEntryResult = true, RuntimeProjectPath = args[3] });
    foreach (var diagnostic in result.Diagnostics) Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} [{diagnostic.Locator}]");
    if (result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return 1;
    Directory.CreateDirectory(args[2]);
    foreach (var file in result.Files) File.WriteAllText(Path.Combine(args[2], file.RelativePath), file.Content);
    Console.WriteLine($"Emitted {result.EmittedFunctionCount} functions, {result.EmittedRecordCount} records.");
}
else throw new ArgumentException("Expected inspect or emit.");
return 0;

static object ParameterContract(Parameter parameter)
{
    var type = parameter.Type;
    while (type is TypeReference reference) type = reference.UnderlyingType;
    var arithmetic = (type as ArithmeticType)?.Arithmetic;
    return new { name = parameter.Name, kind = type.Kind.Name, dependent = type.IsDependent,
        bits = arithmetic?.Bits, signed = arithmetic?.Signed,
        storageBytes = type.Size?.ToString(CultureInfo.InvariantCulture) };
}

static void Inspect(SemanticNode node, string indent, HashSet<SemanticNode> path)
{
    if (!path.Add(node)) { Console.WriteLine(indent + "<cycle>"); return; }
    string detail = node switch
    {
        OperatorOperation op => $" {op.OperatorKind} LTR={op.Operator.EvalLeftToRight} RTL={op.Operator.EvalRightToLeft}",
        ReferenceOperation reference => $" target={reference.Target?.Name} targetType={reference.Target?.GetType().Name}",
        ConstantOperation constant => $" value={constant.Constant.Value.Kind} {(constant.Constant.Value is IntegerValue integer ? integer.Value.ToString() : "")}",
        InitializationOperation init => $" init={init.Initialization.Kind} target={init.Target?.Name}",
        JumpOperation jump => $" jump={jump.JumpKind} target={jump.Target?.Name}",
        _ => ""
    };
    Console.WriteLine($"{indent}{node.GetType().Name} {node.Kind}{detail}{(node is ExpressionOperation e ? $" type={e.Type.Kind}/{e.Type} category={e.ValueCategory}" : "")}");
    if (node is ExpressionOperation expression)
        foreach (var relation in expression.OperandRelations) Console.WriteLine($"{indent}  operand {relation.Role}/{relation.Ordinal}: {relation.Expression.Kind}");
    foreach (var relation in node.ChildRelations)
    {
        Console.WriteLine($"{indent}  CHILD {relation.Role}/{relation.Ordinal}");
        Inspect(relation.Child, indent + "    ", path);
    }
    path.Remove(node);
}

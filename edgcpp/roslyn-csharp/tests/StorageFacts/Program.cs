using System.Diagnostics;
using System.Text.Json;
using RoslynObj;

if (args.Length != 2 || File.Exists(args[1])) throw new ArgumentException("StorageFacts OBJECT NEW_REPORT");
var watch = Stopwatch.StartNew();
using var input = RoslynObject.Open(args[0], new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
var staticLocals = input.TranslationUnit.Variables.Where(variable => variable.Details.StorageDuration?.Name == "static"
    && (variable.InitializationKind.Name == "initk_function_local" || variable.LocalStaticInitializations.Count > 0)).ToArray();
var arrays = input.TranslationUnit.Types.OfType<ArrayType>().Where(array => array.Size is null || array.Size <= 0 || array.Bound is null || array.Bound <= 0).ToArray();
var selected = input.TranslationUnit.Functions.Where(function => function.Name is "sqlite3Multiply128" or "sqlite3Multiply160").ToArray();
var arraySet = arrays.ToHashSet();
var arrayFields = input.TranslationUnit.Types.OfType<RecordType>().SelectMany(record => record.Fields
    .Select(field => (Record: record, Field: field, Raw: Raw(field.Type))))
    .Where(item => item.Raw is ArrayType array && arraySet.Contains(array)).ToLookup(item => (ArrayType)item.Raw);
var output = new
{
    Scope = "Public retained storage/type facts only. Catalog type occurrence does not establish required value storage or execution order.",
    input.Manifest.ArtifactIdentity,
    StaticLocals = staticLocals.Select(variable => new
    {
        variable.Name, Storage = variable.SymbolDetails.Storage?.Name, Duration = variable.Details.StorageDuration?.Name,
        Kind = variable.InitializationKind.Name, variable.IsThreadLocal, Type = TypeFacts(variable.Type),
        Constant = variable.ConstantInitializer?.Value.Kind.Name, Initializer = variable.Initializer?.Kind.Name,
        Recipes = variable.LocalStaticInitializations.Select(recipe => new { Kind = recipe.Kind.Name,
            Initializer = recipe.Initialization?.Kind.Name, Constant = recipe.Constant?.Value.Kind.Name,
            Cleanups = recipe.Lifetime?.Cleanups.Count }).ToArray()
    }).ToArray(),
    NonvalueArrayCatalog = arrays.Select((array, index) => new
    {
        Ordinal = index + 1, Type = TypeFacts(array), Bound = array.Bound?.ToString(), Kind = array.Array.BoundKind.Name,
        HasVla = array.VlaDimension is not null, Element = TypeFacts(array.ElementType),
        Fields = arrayFields[array].Select(item => new
            { Record = item.Record.Name, RecordSize = item.Record.Size?.ToString(), Field = item.Field.Name, FieldType = TypeFacts(item.Field.Type),
                Offset = item.Record.Layout?.Fields.FirstOrDefault(layout => layout.Field.Equals(item.Field))?.Offset?.ToString() }).ToArray()
    }).ToArray(),
    Integer128Functions = selected.Select(function => new
    {
        function.Name,
        Locals = function.Body?.Locals.Select(variable => new { variable.Name, Type = TypeFacts(variable.Type) }).ToArray(),
        Operations = function.Body?.Root.DescendantOperations().OfType<ExpressionOperation>().Select(operation => new
        {
            Kind = operation is OperatorOperation op ? op.OperatorKind.Name : operation.Kind.Name,
            Type = TypeFacts(operation.Type), Category = operation.ValueCategory.ToString(),
            Operands = operation.Operands.Select(operand => TypeFacts(operand.Type)).ToArray(),
            Constant = operation is ConstantOperation { Constant.Value: IntegerValue integer } ? integer.Value.ToString() : null,
            Reference = operation is ReferenceOperation reference ? reference.Target?.Name : null
        }).ToArray()
    }).ToArray(),
    ElapsedSeconds = watch.Elapsed.TotalSeconds
};
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
File.WriteAllText(args[1], JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"static-locals={staticLocals.Length}; nonvalue-array-catalog={arrays.Length}; int128-functions={selected.Length}");

static CppType Raw(CppType type) { while (type is TypeReference reference) type = reference.UnderlyingType; return type; }
static object TypeFacts(CppType type)
{
    var raw = Raw(type);
    return new { Kind = type.Kind.Name, type.Name, Size = type.Size?.ToString(), Alignment = type.Alignment?.ToString(),
        RawKind = raw.Kind.Name, RawSize = raw.Size?.ToString(), RawAlignment = raw.Alignment?.ToString(),
        ArithmeticKind = raw is ArithmeticType arithmetic ? arithmetic.Arithmetic.ArithmeticKind.Name : null,
        Signed = raw is ArithmeticType integer ? integer.Arithmetic.Signed : null,
        Pointee = raw is PointerType pointer ? Raw(pointer.PointeeType).Kind.Name : null };
}

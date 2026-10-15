using System.Text.Json;
using RoslynObj;

if (args.Length < 2 || File.Exists(args[1])) throw new ArgumentException("Probe OBJECT NEW_REPORT");
using var input = RoslynObject.Open(args[0], new ReadOptions { CachedRowCount = 4096, CachedPageCount = 128, PageSize = 128 });
string[] names = ["sqlite3_sourceid", "sqlite3ExprListAppend", "sqlite3ExprListAppendNew", "sqlite3ExprListAppendGrow",
    "explainIndexColumnName", "sqlite3PreferredTableName", "fts3ReadExprList", "sqlite3Fts5ConfigDeclareVtab",
    "fts5ContentlessUpdate", "rbuCreateImposterTable2", "rbuCreateImposterTable", "rbuObjIterPrepareAll"];
if (args.Length > 2) names = args.Skip(2).ToArray();
var isSqlite = input.TranslationUnit.Functions.Any(function => function.Name == "sqlite3_sourceid");
var functions = input.TranslationUnit.Functions.Where(function => function.Body is not null &&
    (!isSqlite || names.Contains(function.Name))).ToArray();
var report = new
{
    Scope = "Public constant occurrence/type/category and static recipe inventory only; no execution scheduling",
    input.Manifest.ArtifactIdentity,
    Functions = functions.Select(function => new { function.Name,
        Constants = function.Body!.Root.DescendantOperations().OfType<ConstantOperation>()
            .Where(operation => operation.Constant.Value is StringValue or AggregateValue).Select(operation => new
            {
                Value = operation.Constant.Value.Kind.Name, OperationType = TypeFacts(operation.Type),
                Category = operation.ValueCategory.ToString(), ConstantType = operation.Constant.Type is { } type ? TypeFacts(type) : null,
                Storage = operation.Constant.AssociatedStorageVariable?.Name,
                String = operation.Constant.Value is StringValue text ? new { Length = text.Length.ToString(),
                    UnitBits = text.StorageUnitBits, Units = text.UnitKind.Name, StoredUnits = text.StorageUnits } : null,
                Aggregate = operation.Constant.Value is AggregateValue aggregate ? new { Type = TypeFacts(aggregate.Details.Type),
                    Elements = aggregate.Elements.Select(element => new { Value = element.Value.Kind.Name,
                        Type = element.Type is { } elementType ? TypeFacts(elementType) : null }).ToArray() } : null
            }).ToArray(),
        StaticLocals = function.Body!.Locals.Where(variable => variable.Details.StorageDuration?.Name == "static" ||
                variable.InitializationKind.Name == "initk_function_local" || variable.LocalStaticInitializations.Count != 0)
            .Select(variable => new { variable.Name, Kind = variable.InitializationKind.Name, Duration = variable.Details.StorageDuration?.Name,
                Storage = variable.SymbolDetails.Storage?.Name, variable.IsThreadLocal, Cleanup = variable.CleanupRoutine?.Name,
                Type = TypeFacts(variable.Type), RawConstant = variable.Details.ConstantInitializer is { } raw ? new { Kind = raw.Value.Kind.Name, Type = raw.Type is { } rawType ? TypeFacts(rawType) : null } : null,
                Initializer = variable.Initializer?.Kind.Name, Constant = variable.ConstantInitializer?.Value.Kind.Name,
                Recipes = variable.LocalStaticInitializations.Select(recipe => new { Kind = recipe.Kind.Name,
                    Constant = recipe.Constant?.Value.Kind.Name, Initialization = recipe.Initialization?.Kind.Name,
                    Cleanups = recipe.Lifetime?.Cleanups.Count }).ToArray() }).ToArray()
    }).ToArray()
};
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
File.WriteAllText(args[1], JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine("functions=" + functions.Length);
static object TypeFacts(CppType type) => new { Kind = type.Kind.Name, type.Name, Size = type.Size?.ToString(), Alignment = type.Alignment?.ToString() };

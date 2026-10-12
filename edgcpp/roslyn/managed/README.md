# RoslynObj managed reader

`RoslynObj` targets .NET 10/C# 14 and pins `Microsoft.Data.Sqlite` 10.0.12.
It has no dependency on Microsoft.CodeAnalysis and performs no C++ semantic
analysis or translation. Each opened object contains one independent EDG
translation unit.

```csharp
using RoslynObj;

using var obj = RoslynObject.Open("sample.roslynobj");
foreach (var function in obj.TranslationUnit.Functions)
{
    Console.WriteLine($"{function.Name}: {function.ReturnType.Name}");
    foreach (var parameter in function.Parameters)
        Console.WriteLine($"{parameter.Name}: {parameter.Type.Name}");

    if (function.Body is not { } body)
        continue;
    foreach (var call in body.DescendantOperations().OfType<CallOperation>())
        Console.WriteLine(call.TargetMethod?.Name ?? "<indirect>");
    foreach (var region in body.ExceptionHandling.CleanupRegions)
        foreach (var action in region.Actions)
            Console.WriteLine($"{action.Kind}: {action.TargetMethod?.Name}");
}
```

Current parameters include EDG's ABI parameters. A declaration-only signature
can expose a parameter type with a null `Symbol` and `Name`. Optional source
parameters, declaration defaults, source-selected call targets and original
source types are available only through retained associations. Optional lists
are empty and optional singular relationships are null.

`DescendantOperations` includes statements, expressions and initialization
operations, visits each captured identity once, and follows executable structural
relationships. It excludes backing metadata expressions and does not traverse
callees, types or exceptional cleanup as executable children. `Children`, typed
operands, semantic references and source associations remain available separately.

Types, templates, layouts, partial EH, macros and source positions have ordinary
object properties. `Details` and typed auxiliary records expose the complete
normalized column and ordered-row projection. [field-bindings.tsv](field-bindings.tsv)
is generated from the shared schema and records each scalar/foreign-key property.
Internal SQLite identities are unnecessary for navigation. Entity equality uses
artifact identity plus captured entity identity; `RoslynWorkspace` opens many
objects without linking or merging them.

Exact integers, target sizes, offsets and EH states use `BigInteger`. Floats
retain class, sign, radix, significand, exponent and supplied NaN facts; they are
never converted through SQLite REAL or host double. `StringValue.StorageUnits`
preserves the declared storage units and length. `UnitKind` distinguishes target
bytes from target characters. `TargetBytes` is empty for character-unit values;
`CodeUnits` aliases the stored units without implying Unicode decoding.
`TargetProfile.PointerBits` is a bit width; alignment and layout quantities use
target addressable units.

The owner holds a read-only SQLite transaction for the lifetime of the snapshot.
Reads and disposal are synchronized; collections are immutable paged views.
Sequential enumeration uses keyset paging and fetches related rows in bounded
batches. `ReadOptions` controls row/page cache counts and page size; cache eviction
changes cost without dropping data. `PagedList.Enumerate` and operation traversal
accept cancellation. Database-backed reads after disposal throw
`ObjectDisposedException`. Already detached scalar values remain ordinary values.
`Statistics` exposes query, row and cache counters for measurements.

Opening checks the supported schema version, complete header, stable kind catalog
and required representation capabilities. The reader fails on incompatible
required kinds instead of returning an opaque unknown operation. Informational
EDG configuration and sparse availability records are accessible separately.

From the repository root:

```sh
python3 edgcpp/roslyn/managed/tools/generate_bindings.py --check
dotnet build edgcpp/roslyn/managed/RoslynObj/RoslynObj.csproj
dotnet run --project edgcpp/roslyn/managed/RoslynObj.Tests/RoslynObj.Tests.csproj -- \
  edgcpp/artifacts/roslynobj/basic.roslynobj \
  edgcpp/artifacts/roslynobj/m0.roslynobj
```

Without object paths, the console test project runs the valid shared-schema
fixture and reader behavior/scale checks. With paths, it also navigates real EDG
outputs. The named integration fixtures check selected calls, exceptional actions
and retained macro definitions/invocations. These tests verify projection and
reader behavior; they do not re-evaluate EDG language decisions.

After a supervisor-owned schema change, regenerate committed bindings with
`python3 edgcpp/roslyn/managed/tools/generate_bindings.py`. Normal library builds
consume those generated sources and do not require Python.

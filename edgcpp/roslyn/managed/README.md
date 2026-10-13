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
can expose a parameter descriptor without a variable `Symbol`. Retained signature
names remain available; absent names are null. Optional source
parameters, declaration defaults, source-selected call targets and original
source types are available only through retained associations. Optional lists
are empty and optional singular relationships are null.

`DescendantOperations` includes statements, expressions and initialization
operations, visits each captured identity once, and follows executable structural
relationships. It excludes backing metadata expressions and does not traverse
callee routine definitions, types or exceptional cleanup as executable children.
Evaluated callee expressions remain structural children. `Children`, typed
operands, semantic references and source associations remain available separately.

`As<T>()` returns the same captured entity through its named façade, or null when
the façade does not match. `SemanticVisitor` dispatches to named declaration and
operation hooks, then through their base hooks. Calling the base hook continues
through captured structural children, including the shape of metadata prototypes.
Visitors visit each structural occurrence; `DescendantOperations` provides the
separate identity-deduplicated executable traversal.

Types, templates, layouts, partial EH, macros and source positions have ordinary
object properties. `Details` and typed auxiliary records expose the complete
normalized column and ordered-row projection. [field-bindings.tsv](field-bindings.tsv)
is generated from the shared schema and records each scalar/foreign-key property.
Internal SQLite identities are unnecessary for navigation. Entity equality uses
artifact identity plus captured entity identity; `RoslynWorkspace` opens many
objects without linking or merging them.

`SemanticModel.GetReferences(symbol)` returns captured reference uses and selected
call targets, with their owner nodes and optional source origins. Literal pointer,
member-pointer and label-difference targets follow exact captured relationships;
a constant initializer without an expression retains its Constant occurrence as
the reference owner. Declaration
objects are semantic nodes, so `GetSymbol(declaration)` and `GetType(declaration)`
follow retained descriptor links without reconstructing C++ lookup. A type's
`DeclarationSymbols` exposes the actual declarations associated with a descriptor;
`DeclarationSymbol` is optional and returns a symbol only when that association is
unique. The type descriptor stays separate.
Template headers, parameter placeholders and defaults navigate through typed
objects. Function signatures and requires-expression parameters share canonical
`ParameterType` descriptors, including retained declared types and default facts.
`CallArgument.Parameter` follows the actual current formal descriptor; variadic
trailing arguments have none. Implicit source receivers remain separate, while
an explicit-object parameter is an ordinary argument with its recorded descriptor.
Variables expose their ordered `EntitiesDefinedInInitializer`, including actual
closure types defined by static-member initializers.
`VariableSymbol.DefinitionType` is the optional retained declaration type.
Inherited `Symbol.DeclaredType` refers to the current common-symbol type descriptor.
`FunctionSymbol.Signature` follows the current callable routine type, including
hidden ABI parameters. `DefinitionType` is the optional retained written type;
the reader does not recover an original signature from lowered parameters.
Member object qualifiers and `this` pointer qualifiers navigate separately.
`ExceptionSpecification` keeps ordered throw types, explicit noexcept arguments,
indeterminate state and deferred prototype relationships. Concrete consteval
bodies remain metadata, and `IsDefined` stays true for discarded definitions.
GNU routine supplements expose exact aliases, ifunc resolvers, inline partners
and multiversion relationships; targeted versions retain recorded dispatch order.
`IsPureVirtual` and `IsPureFunction` describe separate C++ and GNU facts.

`SemanticEntity.SourceCorrespondence` exposes its actual retained common
metadata, including recorded parent scope, enclosing routine and semantic flags.
Associated type descriptors, symbols and primary declarations share that exact
correspondence. Copied wrappers keep distinct identities. `Name` projects the
retained source name; `CurrentName`, `FabricatedName` and `MangledEncoding` remain
separate projections. Fabricated unnamed entities have no source name. Symbol
search compares that stored source-name projection without language lookup.
Primary declarations delegate common metadata; secondary occurrences retain their
own null or empty fields without falling back to the primary declaration.

`Scope.Children` follows the actual ordered child-scope list.
`ChildScopesByParent` is the separate reverse-parent relation; a scope may be
absent from the recorded child list while retaining its parent. Using declarations
expose exact qualifier/target/set links and captured lexical owners. Lowering can
remove written using forms. Microsoft proxy types keep their descriptor metadata
and namespace/type associations without an invented type declaration or concrete
placeholder layout.
`NameReference` and `NameQualifier` expose actual qualifiers, special forms and
retained original template arguments. An empty original list stays empty even
when the name records an explicit argument count; instance arguments are separate.

`RecordType.BasesInPostorder` and `BasesInPreorder` preserve the available EDG
orders. Base derivations and overrides retain contextual subobject identities;
they do not imply a unique parent.
`DeclaredVirtualMethods` follows retained virtual routines owned by the record.
Inherited relationships remain accessible through actual `BaseSubobjects` and
`Overrides`. Class table and VTT variables expose their exact constant
initializers; the reader does not construct a flattened virtual slot map.

Layouts expose effective and nonvirtual sizes, ordered fields, declared bit-field
widths and stored widths separately. Optional
ABI indices and Microsoft container offsets stay null when their build does not
retain them. All layout quantities use exact target values. `CppType.Size` and `Alignment`
are nullable when the captured availability fact does not establish a concrete
quantity. `EffectiveSize` and `EffectiveAlignment` expose the raw retained stock
helper quantities. Dynamic UPC arrays keep their THREADS coefficient in
`ThreadsSizeMultiplier` and `ArrayType.ThreadsMultiplier`; ordinary size and
bound are absent. An incomplete array bound remains distinct from explicit zero.
Every retained template has a `TemplateDescriptor`. A true template declaration
has a separate `TemplateSymbol` whose `Descriptor` preserves that relationship.
Template-template placeholders instead point to their actual parameter symbols.
Parameters participate in the common `Symbol` hierarchy; `TemplateKind` and
`ParameterKind` expose their retained subtype classifications. Placeholder
associations follow exact stored pointers, including multiple retained declarations.
Named type symbols delegate attributes to their authoritative type descriptor.
Attributes and arguments preserve their actual descriptor identities independently
from ordered owner associations. `Attribute.Owners` and
`AttributeArgument.OwningAttributes` retain sharing; an attribute's optional
`Owner` is available only for a unique association. Groups retain their own
identity and expose their captured members.
Switches navigate canonical `SwitchDescriptor` and `SwitchCaseEntry` identities.
`Cases` preserves the captured declaration order and `SortedCases` the separate
retained numeric order; the reader does not sort them. `HasSortedCases` records
only a nonnull sorted-list head. Source VLA expressions expose their actual
`VlaDimensions`; executable traversal follows retained evaluated dimensions
without reevaluating dimensions established by typedef or size statements.
Conversions project the actual operator, operand type and result type.
UPC loops expose their evaluated `Affinity` through the same structural graph.
Statements expose their designated `SourceSequenceEntry` when retained. Named
initialization and statement-expression result statements have their own visitor
hooks; result values follow the actual statement expression or initialization.

Lambda initializations and lambda expressions share one `LambdaDefinition` with
its actual capture list. Assembly operations similarly delegate to their captured
`AssemblyDefinition`; module/import objects retain ordered translation-unit imports.

`ComplexValue.Real` and `Imaginary` navigate the canonical floating components.
The real component remains null when the retained value has none. Each component
keeps exact target significand, exponent, sign and special-value facts regardless
of the producer's host floating storage.

`Scope.SourceSequence` exposes the retained physical source-entry list.
`Scope.SourceEntries` follows sublist entries at their actual splice point;
`SourceSublists` is the separate storage index. Source-sequence owners and a
declaration's optional lexical scope describe different relationships. Secondary
declarations retain their own source positions, written types, attributes and
flags. `RecordType.SourceDeclarations` follows the retained member occurrence
list, including friends and repeated targets; those positionless occurrences do
not acquire source positions from the target's primary declaration.

Source files use canonical physical paths. Include occurrences keep their display
names, import spelling and context flags, while `LineDirective` describes a retained
line remapping. Positions expose physical and logical lines, presumed names and
the actual spelling position when retained. Optional source metadata stays null
or empty when its resolved build configuration does not retain it.

Exact integers, target sizes, offsets and EH states use `BigInteger`. Floats
retain class, sign, radix, significand, exponent and supplied NaN facts; they are
never converted through SQLite REAL or host double. `StringValue.StorageUnits`
preserves the declared storage units and length. `UnitKind` distinguishes target
bytes from target characters. `TargetBytes` is empty for character-unit values;
`CodeUnits` and `Length` are compatibility aliases for stored units and their
count, without implying Unicode decoding. `ByteLength` and `StorageUnitBits`
make target-byte storage explicit. `AddressValue.ByteOffset` is a signed target
byte offset; `SubobjectPath.ElementOffset` is a signed array element count.
`SubobjectPath.Steps` follows the recorded canonical chain.
`TargetProfile.PointerBits` is a bit width; alignment and layout quantities use
target addressable units.

Blocks and try statements navigate their separate retained descriptors. A
lifetime's optional `BoundEntity` follows its actual stored binding, including a
cleared binding after lowering. `Children` records reverse creation order;
`HasImplicitChild` identifies children that may be absent from that list.
Lifetime cleanup entries expose recorded initialization recipes, including
exceptional-only destruction. `FunctionBody.Lifetimes` follows captured scope,
body and recipe references without assigning function ownership. Goto
`CommonLifetime` is the recorded common boundary; lifetime parents can represent
continuation predecessors after labels.
`Initialization.Master` and `NextInDestructionList` preserve the exact recipe
associations separately from evaluated children. Temporary expressions follow
their actual `Initialization.Target`; an absent target remains null.

Partial EH with tables exposes numeric states and decoded regions. A tableless
partial-EH object instead retains actual `CleanupStart` and `CompletedInitializer`
recipes as metadata. Its state numbers and initial/null state scalars are null;
the reader does not infer regions, storage or extra evaluated children.

`Scope.LocalStaticInitializations` preserves its storage-list order. File and
namespace execution order remains `Scope.DynamicInitializations`.
Native `ForEachOperation.Initializations` follows the plural evaluated initializer
edges in their recorded order. It does not select a synthetic singleton recipe.
`Scope.ConstructorInitializers` records construction or destruction order;
`EffectiveInitialization` follows a field default recipe only when the retained
initializer explicitly requests it. Array cleanup handles remain exact constants,
while each cleanup action carries its own captured storage location.
`CleanupArray.StorageLocations` and `CountLocations` project those using-action
relationships; their singular properties are available only when the recorded
location is unique. Thrown-object and receiver-relative addressing remain
separate kinds. `CleanupAction.IsUnordered` is nullable because a lowered cleanup
table does not retain that initialization flag.

The owner holds a read-only SQLite transaction for the lifetime of the snapshot.
Reads and disposal are synchronized; collections are immutable paged views.
Sequential enumeration uses keyset paging and fetches related rows in bounded
batches. `ReadOptions` controls row/page cache counts and page size; cache eviction
changes cost without dropping data. `PagedList.Enumerate` and operation traversal
accept cancellation. Database-backed reads after disposal throw
`ObjectDisposedException`. Already detached scalar values remain ordinary values.
`Statistics` exposes query, row and cache counters plus the largest batch size.
Companion and child-prefetch batches contain at most 900 keys regardless of the
selected page and cache sizes.

Opening checks the supported schema version, complete header, stable kind catalog
and required representation capabilities. The reader fails on incompatible
required kinds instead of returning an opaque unknown operation. Informational
EDG configuration and sparse availability records are accessible separately.
During development, a required `normalized_semantic_schema_v1` capability carries
the generated schema fingerprint covering framed SQL and the full stable kind
catalog. Opening rejects missing or mismatched
fingerprints before publishing a model, including older development objects with
the same numeric format version. This gate is not a final public-format promise.

From the repository root:

```sh
python3 edgcpp/roslyn/managed/tools/generate_bindings.py --check
dotnet build edgcpp/roslyn/managed/RoslynObj/RoslynObj.csproj
dotnet run --project edgcpp/roslyn/managed/RoslynObj.Tests/RoslynObj.Tests.csproj -- \
  edgcpp/artifacts/roslynobj/closure251-phantom29/native/basic.roslynobj \
  edgcpp/artifacts/roslynobj/closure251-phantom29/native/m0.roslynobj
```

Without object paths, the console test project runs the valid shared-schema
fixture and reader behavior/scale checks. With paths, it also navigates real EDG
outputs. The named integration fixtures check selected calls, exceptional actions
and retained macro definitions/invocations. These tests verify projection and
reader behavior; they do not re-evaluate EDG language decisions.

Use `--benchmark` followed by object paths to measure full public entity and body
navigation on each object twice. The cold pass starts with empty reader caches;
the warm pass reuses the bounded caches. The harness reports commands, rows, wall
time, process working set and cache counts without flushing operating-system caches.

After a supervisor-owned schema change, regenerate committed bindings with
`python3 edgcpp/roslyn/managed/tools/generate_bindings.py`. Normal library builds
consume those generated sources and do not require Python.

# Independent C# semantic API and source provenance

Design proposal only, 2026-10-12. No implementation is authorized by this document. This is a new `.roslynobj` reader API, with no dependency on `Microsoft.CodeAnalysis`, no C++ parser or semantic analyzer in managed code, and no translation or linking component. Evidence is the pristine `edgcpp/compiler` revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`; citations below pin that revision. Names and C# signatures are proposed contracts, not existing implementation.

## Object model

`RoslynObject` owns a read-only SQLite snapshot containing exactly one primary translation unit, exposed by `obj.TranslationUnit`. A `TranslationUnit` is the semantic boundary: language mode, target, compiler configuration, source occurrence history, declarations, instantiations and executable bodies belong to it. A workspace may open many independent objects; this does not create a merged database or linked program. EDG-internal imported secondary material retains its origin within its primary object and is distinct from cross-object linking. Identical names, mangled names, paths, source hashes or type shapes never merge entities across objects. There is no `Compilation` abstraction implying a linked C++ program.

The ordinary representation is EDG's source-shaped, resolved IL before optional IL lowering. It already contains semantic elaborations: conversions, selected routines, compiler-generated entities, dynamic initializations and lifetime records. “Source-shaped” does not mean a lossless concrete syntax tree. A dependent prototype remains dependent; no reader algorithm fills in template semantics. The chosen producer phase and retention switches are part of the manifest.

| API family | Meaning | Identity and navigation |
| --- | --- | --- |
| `SemanticNode` | A declaration occurrence, statement, expression or other retained source-shaped semantic construct | Structural children by named role and ordinal; semantic references separately |
| `Symbol` | An EDG-resolved declared entity, such as a variable, parameter, field, function, namespace, template or named type | Declaration/definition occurrences, owner, type, linkage and instantiation associations |
| `CppType` | A retained type descriptor, including qualified/typedef wrappers, pointers, references, arrays, routine types, class types and dependent types | Type graph; named-type declaration link is separate from type identity |
| `Operation` | A typed executable view over a retained expression, statement or dynamic-initialization record | Same backing entity identity; no second invented lowering or duplicated operation tree |
| `ObjectLifetime`, `DynamicInitialization`, `ExceptionRegion` | EDG semantic execution/lifetime associations | Explicit graph relations with recorded ordering and boundaries |
| `SourceOccurrence`, `SourcePosition`, `MacroDefinition`, `MacroInvocation` | Source history and provenance | Separate from semantic containment and executable evaluation |

For example, a variable symbol may have several declaration nodes, one definition and one initializer operation. `SemanticNode` is an API navigation abstraction: a declaration node projects the normalized declaration occurrence, without requiring a duplicate row in the executable `semantic_node` table. A class symbol is associated with a class type; a typedef symbol names a type wrapper, whose underlying type is another type entity. Dropping that wrapper globally loses spelling and qualifiers. EDG expressly represents typedefs and added qualifiers with `tk_typeref`; type queries must expose both the declared type and any EDG-derived underlying type without treating them as interchangeable. [Type-reference representation](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/int_overview.rst#L860-L873)

## Containment, sharing and traversal

The storage graph may share an entity. `SemanticNode` therefore has `GetStructuralParents()` rather than an unconditional single `Parent`. `GetChildren()` returns `ChildLink(Role, Ordinal, Node)`. A source occurrence has its own identity and may have a unique containing occurrence where the retained source information establishes one. Multiple declarations and repeated template use must not be forced into one source occurrence merely because they refer to the same entity.

`DescendantNodes()` follows structural roles, visits each node identity once and has deterministic role/ordinal order. `DescendantOccurrences()` follows occurrence containment and visits each occurrence; it is explicitly allowed to return the same semantic entity more than once. Neither follows symbol references, types, call targets, template origins, base classes or lifetimes. `SemanticVisitor` dispatches by node kind; `OperationVisitor` dispatches by operation kind. Separate typed graph queries traverse references and accept cycle handling and cancellation explicitly. No default traversal recursively expands a called function or type definition.

Lexical order, operation operand order and EDG destruction-list order are different orders. `ChildLink.Ordinal` is not an assertion about C++ evaluation sequencing. Source-sequence lists provide declaration/statement order and secondary declarations, not punctuation-complete syntax. The primary/secondary distinction also has documented exceptions, so expose recorded declaration role independently from an `IsDefinition` fact. [Source-sequence and secondary-declaration records](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L2088-L2165)

## IDs, ownership and lifetime

An artifact has an immutable identity recorded in its manifest. Entity IDs are opaque integers unique within that artifact; every semantic entity belongs to the object's singleton translation unit, without a repeated ownership column on every row. A durable identity is `(ArtifactId, EntityId)`, with artifact ownership checked on lookup. `NodeId`, `SymbolId`, `TypeId`, `LifetimeId`, `SourceOccurrenceId` and `MacroInvocationId` are strongly typed handles, even when they share the same underlying entity key. IDs are not native pointers, source offsets, names or stable identities across recompilations. Artifact copies retain identity; a newly produced artifact receives a new identity.

Reader values are immutable. Lightweight handles load typed records and indexed relation pages on demand. Repeated lookups have value identity without promising managed reference equality. Opening does not load every body, type, macro or source buffer. Batch lookup and bounded paging avoid a separate SQL statement per child during traversal; cancellation is available on potentially large enumeration/materialization operations.

`RoslynObject : IDisposable` owns connections and caches. Handles may be shared across threads for concurrent reads while the owner is alive; cursor/enumerator instances are not shared between threads. Disposal is coordinated with active reads, prevents new reads, and releases connections after outstanding reads finish or cancel. Subsequent payload access throws `ObjectDisposedException`; copied IDs and already materialized detached records remain usable as ordinary values. No operation silently reopens the file. A reader observes one fixed snapshot for its entire lifetime, including lazy reads; replacement or mutation of the backing file must not mix versions. A `ReadOptions` memory budget controls caches, not semantic completeness.

## Facts and unknowns

The API uses `Fact<T>` with either `Available(T)` or `Unavailable(FactUnavailableReason)`. Reasons include `Dependent`, `NotRetained`, `NotComputedAtCapture`, `LostBeforeCapture`, `UnsupportedByProducer`, and `NotApplicable`. A known absence is `Available(Optional<T>.None)`, not unavailable. A known false Boolean is available false. An empty collection means known empty only if its collection availability is available. These reasons describe upstream limitations or optional metadata, never missing required exporter adapters. Every required executable kind and payload in a supported schema must be understood. An unsupported required schema version, kind or capability is an incompatible-reader error; the reader must not successfully expose an allegedly complete object using an `Unknown` or opaque payload fallback. A producer missing a required adapter must fail export.

A uniform capability manifest and sparse exceptions implement these semantics without storing an availability row for every scalar. For example, a TU with no retained macro arguments has one capability-level explanation; an exceptional unavailable range has its own fact exception. `GetRequiredValue()` throws a fact-unavailable exception containing the entity, fact and reason. Queries must not quietly skip unknown values when that would turn incomplete results into a completeness claim.

The reader reports recorded EDG facts. It does not redo overload selection, name lookup, template deduction, constant evaluation, ABI layout, virtual override analysis or destructor scheduling. A source position of zero is an EDG sentinel; it is not line zero in a real file. A false expression `compiler_generated` flag does not establish that the expression was explicitly written: EDG documents contexts where generation is implicit in the containing construct. Preserve the flag and expose a richer origin fact only when an explicit retained association supports it. [Expression flag and ranges](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L15083-L15118)

## Proposed reader surface

```csharp
using RoslynObj;

using var obj = RoslynObject.Open("sample.roslynobj", ReadOptions.Default);
TranslationUnit unit = obj.TranslationUnit;
Console.WriteLine(unit.Manifest.LanguageMode);
foreach (FunctionSymbol function in unit.Symbols.Functions())
{
    Fact<Optional<FunctionBody>> body = function.Body;
    // Preserve and report unavailability instead of treating it as no body.
}
```

The principal entry points are `unit.Root`, `unit.Symbols`, `unit.Types`, `unit.Instantiations`, `unit.Sources`, `unit.Macros`, `unit.Lifetimes` and typed `unit.Get(NodeId/SymbolId/TypeId/...)`. `SymbolIndex.FindByName` is a search over retained names, not C++ lookup in an arbitrary context. `unit.References.To(symbol.Id)` returns the retained resolved uses in this TU with a coverage fact. It makes no whole-program completeness claim. The initial API does not promise a retained EDG warning/diagnostic collection; object-open and exporter failures are separate operational diagnostics.

Operation views include call, conversion, member access, initialization, allocation/deallocation, throw, structured control, coroutine and target-extension kinds wherever the producer records them. An operation exposes its type, value category, implicit/conversion flags, selected reference, operands, origin and lifetime associations as appropriate. An indirect call has an indirect target expression; a virtual call exposes the selected declaration and retained dispatch facts. Neither fabricates a unique runtime callee.

Type queries expose qualifiers, spelled wrapper chain, underlying type, pointer/reference kind, array extent, routine parameters/return/qualifiers/exception specification, class/enum information, and dependent constituents. `TypeIdentity` is a recorded identity. Structural shape or equal source spelling is not type equivalence; any `EquivalentTo` relation must be emitted from EDG knowledge, with a documented comparison meaning.

Instantiation queries expose primary template, specialization, ordered arguments, instantiated member origin, EDG instantiation kind and any available request/source locations. Layout queries return target-qualified sizes/alignments, fields, bitfields and base-subobject facts. Virtual queries return recorded override relationships, slots, table facts and adjustments only when retained at the chosen phase. Lifetime queries expose the dynamic initialization, destructor, lifetime membership, list order, parent/sublist links and jump/exception associations. They deliberately do not add a derived “all destructor calls on this exit” service: that would be a cleanup planner. EDG documents these lifetimes as the representation from which lowering generates destructions. [Lifetime representation and destruction lists](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/il.rst#L1457-L1587)

Useful query shapes, shown as proposed C# pseudocode:

```csharp
// Explain the selected call and where its spelling originated.
CallOperation call = unit.Get(callNodeId).AsOperation().AsCall();
Fact<Optional<FunctionSymbol>> selected = call.SelectedFunction;
Fact<CppType> resultType = call.Type;
Fact<ValueCategory> category = call.ValueCategory;
Fact<SourcePosition> origin = call.Origin.Anchor;
if (origin.TryGetValue(out SourcePosition position))
{
    Fact<ImmutableArray<MacroInvocation>> expansions =
        unit.Macros.GetExpansionChain(position);
}

// Inspect a realized specialization and its target layout.
TemplateInstantiation instance = unit.Instantiations.Get(instanceId);
var arguments = instance.Arguments; // Ordered, typed argument variants.
Fact<ClassLayout> layout = instance.Result.AsClassType().Layout;

// Follow EDG's recorded lifetime associations without executing a plan.
ReturnOperation exit = unit.Get(returnNodeId).AsOperation().AsReturn();
var cleanupAnchor = exit.DestructionAnchor;
var lifetime = exit.EnclosingLifetime;
// These facts may point into existing lifetime/destruction records.
// No enumeration here claims to synthesize the runtime destruction sequence.

// Follow declarations without conflating repeated header inclusion or TUs.
if (selected.TryGetValue(out Optional<FunctionSymbol> target) && target.HasValue)
{
    foreach (DeclarationOccurrence declaration in target.Value.Declarations
                 .GetRequiredValue())
        Console.WriteLine(declaration.Origin.Anchor); // Fact-aware formatting.
}
```

These examples define the intended query value, not a promise that every property is available for every language construct or phase. A body present as a dependent prototype has an explicit semantic state and is not advertised as an executable specialization.

## Exact source and macro evidence

| Requested information | Stock upstream capability at ordinary backend entry | Contract and limitation |
| --- | --- | --- |
| Node/operator position | `a_source_position`; extra declaration, statement and expression ranges with `EXTRA_SOURCE_POSITIONS_IN_IL=TRUE` | Token-level precision for retained positions, not a record for every token; ranges may be hypothetical for implicit nodes or null |
| Logical and physical source identity | `a_source_file` tree and sequence mapping, including `#line` remaps | Keep include occurrences and mapping segments distinct from physical documents; a display filename is not necessarily a physical pathname |
| Macro spelling origin | `FULLY_RESOLVED_MACRO_POSITIONS=TRUE` adds `orig_seq/orig_column` | An original coordinate plus top-level invocation coordinate; this is not an exhaustive multi-input token lineage |
| Nested expansion chain/tree | `RECORD_MACRO_INVOCATIONS=TRUE` and `MACRO_INVOCATION_TREE_IN_IL=TRUE` | Parent relation, referenced definition, original invocation start and optional end; omit packing placeholders from public semantic records |
| Definition/redefinition/undef text | `RECORD_MACROS_IN_IL=TRUE`, `a_macro.text`, replacement range when extra positions enabled | Store EDG's textual representation, with source/command-line/predefined origin. Changing definitions produce separate records. Dynamic builtins such as `__LINE__` have empty definition text |
| Invocation arguments | `RECORD_MACRO_ARGS=TRUE`, `a_macro_invocation_record.arguments` | One canonical-whitespace argument-list string for function-like invocation. This is not individual argument records, original trivia, per-argument preexpanded text, or substitution mapping |
| Every token and exact expanded token text | Lexer/current-token and selective cache structures exist while parsing | No complete retained IL token stream or stock per-token exporter hook established. Unavailable in the baseline |
| Token paste/stringize and argument-to-result lineage | Temporary macro maps and argument/concatenation machinery exist in lexer/preprocessor | Not an IL-level retained relation. Unavailable in the baseline; do not infer from endpoints or parse definition text to invent it |
| All whitespace/comments, inactive directives and excluded branches | Not preserved as a complete IL trivia stream | Optional original source byte archive can preserve text; it is not a semantic token/trivia binding or an active-branch map |

`a_source_position.seq/column` inside expansion denotes the beginning of the top-level invocation, while its optional original pair denotes copied text in a macro definition or argument. `macro_context` identifies the associated invocation. The original invocation end, when present, refers to the last character of an object-like macro name or the closing parenthesis of a function-like invocation. Macro records with negative parent indices are storage placeholders, not expansion events. [Position fields and flag dependencies](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/basics.h#L786-L936), [macro declaration and invocation records](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L17444-L17540), [redefinitions, nesting and placeholders](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/il.rst#L3125-L3229)

EDG columns are one-based logical character positions, with a tab counting as one character. They are not byte offsets, display columns or guaranteed .NET UTF-16 indices. Preserve the native coordinates. Expose `ByteSpan` or a C# text span only as an available derived mapping when the exact encoding and text make conversion valid. A source range can cross macro contexts or documents; do not force it into one contiguous text slice. Preserve endpoint roles and original inclusivity rather than silently treating EDG ranges as half-open byte ranges. [Column semantics](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/int_overview.rst#L875-L910), [file and remapping structure](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L135-L250)

A file's repeated inclusion gets separate source-history occurrences. A `#line` logical filename may have no physical `full_name`. Command-line, predefined and unknown positions get explicit origin kinds rather than fabricated documents. [Special-position sentinels](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/basics.h#L939-L960)

## Retention switches, hooks and hard boundaries

The ordinary independent-TU provenance configuration requests all six switches in the table: `EXTRA_SOURCE_POSITIONS_IN_IL`, `RECORD_MACROS_IN_IL`, `FULLY_RESOLVED_MACRO_POSITIONS`, `RECORD_MACRO_INVOCATIONS`, `MACRO_INVOCATION_TREE_IN_IL`, and `RECORD_MACRO_ARGS`. The resolved build values must be recorded rather than inferred from defaults. Macro invocation recording requires macro definitions and resolved original positions; argument recording requires invocation recording. `MACRO_INVOCATION_TREE_IN_IL` conflicts with `COMPILE_MULTIPLE_TRANSLATION_UNITS`, whose purpose is merging IL from multiple input files. Compile independently supplied inputs separately and store each resulting primary TU in its own object. [Dependencies](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/basics.h#L786-L846), [merged-input incompatibility](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/host_envir.h#L370-L390)

Legacy exported-template processing may introduce EDG-internal secondary translation units independently of `COMPILE_MULTIPLE_TRANSLATION_UNITS`. That profile is not prohibited by the one-primary-TU artifact rule. Macro-context survival/remapping during secondary-to-primary copying has not been established; its compatibility profile must mark macro-tree/argument provenance unavailable unless that mechanism is demonstrated. This optional provenance limitation must not reduce executable/type coverage. [Internal secondary-TU distinction](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/trans_unit.c#L752-L770)

Enable `GENERATE_SOURCE_SEQUENCE_LISTS` for declaration ordering. Optional IL lowering suppresses or fails to update that source information in ordinary configurations; the special preservation switch is documented as unusual and less well debugged. This supports taking the source-shaped snapshot before optional lowering, rather than promising reconstruction afterward. [Source-sequence configuration](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/host_envir.h#L1546-L1590)

The current backend boundary cannot recover transient lexer information. `fe_wrapup` frees front-end-only storage, or all regions when the IL is to be reread. Token spelling can disappear even before the next `get_token` returns because string-literal lookahead crosses lines. Nonreusable token caches are discarded after use; reusable caches exist for selected delayed constructs, not the full TU. `a_macro_text_map` describes mapping over transient definition/argument/expansion buffers but is not a complete persisted token-provenance contract. [Wrapup freeing](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/fe_wrapup.c#L842-L854), [spelling lifetime and token caching](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/lex_pp.rst#L324-L466), [temporary text maps](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/lexical.h#L2118-L2182)

Inspection of stock lexical, macro, preprocessing and initialization interfaces plus the customization documentation did not establish an existing callback for exporting every token/comment/macro-argument event. Internal `get_token`, cache APIs and macro-text maps are not such callbacks. Do not add an interception patch, symbol-renaming wrapper, private lexical pass, replacement preprocessor or core change under the label of a build hook. A future full-token proposal requires identifying a real supported hook and proving event timing, rescan identity and ownership first; it is outside the approved baseline. The documented replaceable-code facilities do not themselves establish that hook. [Documented customization boundary](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/build.rst#L624-L644)

The `--comments`/`-C` switch preserves comments in preprocessing output only. Source-sequence comments in broad descriptions do not establish a retained general comment entity or complete trivia stream; even the source-generating backend documents elimination of comments. A separate preprocessing run is not substituted for the semantic run and does not supply missing semantic-token associations. [Preprocessing option](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/ext_intf.rst#L587-L595), [documented source-output losses](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/cp_gen_be.rst#L11-L21)

Optional source archival uses external attachments for exact bytes; `.roslynobj` stores only normalized attachment references, encoding, hash and acquisition provenance. It stores no raw-byte BLOB, binary payload or encoded substitute. `FrozenCompilationInput` is valid only when the build environment guarantees the archived bytes were the compilation inputs. A backend reread of a path after compilation is labeled `PostCompilationReread`, not exact compilation input. Bytes unavailable from a file, PCH or module are explicitly unavailable. Archived whitespace/comments remain accessible as text without claiming a tokenized trivia tree; disabled preprocessor branches remain text without invented branch decisions. Reconstructing exact input from line/column positions or normalized macro strings is impossible.

## Required relational support

The central normalized schema should support these relations with typed columns and foreign keys rather than serialized semantic payloads:

1. Artifact/TU ownership, typed entity identities, phase/configuration/capabilities, and sparse fact-unavailability exceptions.
2. Structural child links `(owner, role, ordinal, child)`; source occurrence containment; symbol declaration/definition occurrences; separate typed reference relations.
3. Expression/statement/dynamic-initialization rows and operation views, type constituents/wrappers, selected symbols and value categories; instantiation arguments/origins; target layout, virtual and lifetime associations.
4. Physical source document metadata and optional external byte-attachment references; source-history/include occurrences; sequence-to-file/line remapping segments; retained positions with expansion/original coordinate pairs and macro invocation; ranges referencing endpoint positions.
5. Macro directive/definition versions and their EDG text, flags and replacement range; actual macro invocation rows with parent, definition, original endpoints, invocation order and optional canonical argument-list text. No rows for EDG packing placeholders.
6. Optional source archive provenance and encoding/mapping availability. A full token table, per-argument expansion table, paste/stringize lineage table or trivia binding table is not required by the baseline because its population has no established upstream capture mechanism.

Do not invent source positions, expanded spelling, argument boundaries or origin associations merely to populate a relation. Preserve ordinary browsability: macro definition and canonical argument strings are scalar textual facts, not JSON transports. Text storage is not a substitute for normalized semantic relationships.

## Acceptance questions after plan approval

Validation should exercise useful language/provenance questions: distinguish a redeclaration from its definition; retain a typedef wrapper; inspect a selected overload and conversion; show a template's prototype separately from an instance; follow virtual/lifetime records; trace nested object-like/function-like macro origins; preserve redefinition/undef and dynamic builtin distinctions; show canonical argument text without claiming original whitespace; map a repeated include and `#line`; distinguish logical columns from bytes for Unicode and tabs; and report unavailable token/trivia detail honestly. Check bounded lazy reads and concurrent read/disposal behavior as reader functionality. No corruption campaign or independent semantic correctness verifier is proposed.

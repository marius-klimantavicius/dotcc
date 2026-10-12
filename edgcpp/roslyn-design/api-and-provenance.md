# Idiomatic C# semantic API and source provenance

Revised design proposal only, 2026-10-12. No implementation is authorized by this document. This is a new `.roslynobj` reader API with no dependency on `Microsoft.CodeAnalysis`, no managed C++ semantic analyzer, and no translator. Provenance evidence below pins pristine `edgcpp/compiler` revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`. Names and signatures describe the proposed API, not implemented code.

## Public model

The public API presents immutable C# objects with properties and ordinary collections. A caller writes `unit.Functions`, `function.Body`, `call.TargetMethod` and `constructedType.TypeArguments`. Database IDs, table names, foreign keys and per-field status wrappers are not the navigation model. Query methods take nodes, symbols and types, rather than IDs.

`RoslynObject` owns a read-only SQLite snapshot containing exactly one primary translation unit, exposed as `obj.TranslationUnit`. A workspace may open many independent objects. It does not merge declarations, types or instantiations between them. EDG-internal imported secondary material retains its origin within the primary object; that differs from linking separately compiled objects. Matching names, mangled names, paths, hashes or shapes never establish cross-object C++ identity.

The proposed primary execution model follows the EDG lowering profile in [lowering-and-integration.md](lowering-and-integration.md): ordinary IL lowering with partial exception lowering and retained EDG cleanup tables. The reader exposes executable operations and surviving symbols/types directly. Source correspondence, original constructs and origins are available only where EDG retained them. It does not reconstruct an original C++ tree from lowered operations. Compatibility profiles can retain different high-level constructs; the manifest identifies their execution representation.

**Open approval decision: original query fidelity.** The user's request to identify the exact source-selected overload is not automatically weakened by choosing nullable APIs. General lowering can destroy that relationship, original virtual-call classification, source signatures and default-argument provenance. Returning null or an empty optional collection is an honest API behavior, but does not satisfy a still-required source query. The tradeoff between that fidelity and the proposed lowered execution model remains pending explicit user resolution; no approval of its loss is inferred from the ergonomic API change or from silence. The examples below demonstrate the proposed lowered option subject to that decision.

| Public object | Ordinary navigation |
| --- | --- |
| `TranslationUnit` | `Functions`, `Variables`, `Namespaces`, `Types`, `Templates`, `Declarations`, `Macros`, `SemanticModel` |
| `FunctionSymbol` | `Name`, current-signature `Parameters`/`ReturnType`, optional `SourceParameters`, `ContainingType`, `Body`, `Template`, `Instantiation`, `Declarations`, `Source` |
| `FunctionBody` | `Root`, `Locals`, `ExceptionHandling`, `Lifetimes`; `DescendantOperations()` for recursive executable traversal |
| `SemanticNode` | `Children`, `StructuralParents`, `Source`; typed `As…()` helpers and visitors |
| `Operation` | `Type`, `Source`, `Operands`, retained execution-specific properties |
| `CallOperation` | `TargetMethod`, `SourceTargetMethod`, optional `Receiver`, `Arguments`, execution `CallKind`, optional `SourceDispatchKind`, `Type` |
| `CppType` and specialized type objects | `Name`, `Members`, `Fields`, `Methods`, qualifiers and typed constituents; `Layout`, `OriginalType`, `LoweringOrigin` when retained |
| `ConstructedType` | `OriginalDefinition`, `TypeArguments`, full typed `TemplateArguments`, `Template`, `Members`, `Layout` |
| `TemplateSymbol`, `TemplateInstantiation` | `Parameters`, `Instances`, `OriginalDefinition`, `TypeArguments`/typed `Arguments`, `Result`, retained source origin |
| `ExceptionHandlingInfo`, `CleanupRegion` | `CleanupRegions`, `Actions`, `NextRegion`, retained condition and storage relationships |
| `SourceInfo`, `MacroInvocation` | source positions/ranges, `MacroExpansion`; expansion `Definition`, `Parent`, `Children`, `ArgumentText` |

Collections are immutable `IReadOnlyList<T>` values or equivalent read-only views. Unsupported, unavailable or absent optional collections return empty collections, never null. Optional scalar/object relationships use nullable values or `Optional<T>`. Ordinary members and lists are not wrapped in `Fact<T>`. Required properties such as an accepted expression's execution type remain ordinary values; a missing exporter adapter cannot turn a required value into an optional one.

The object distinctions remain meaningful. A symbol denotes a declared entity; declaration nodes denote its retained declaration occurrences; a type describes an EDG type; an operation describes execution. A variable can have several declaration occurrences and one initializer operation. A named type symbol can point to a type descriptor without those objects becoming identical. `SemanticNode` is a navigation abstraction: declaration nodes can project normalized declaration rows without a redundant executable-node row. An operation view uses its underlying expression/statement/initialization identity rather than duplicating the graph.

`CallOperation.TargetMethod` is the lowered selected callee, including a generated wrapper or thunk when that is what EDG calls. `SourceTargetMethod` is nullable and exists only when an explicit retained mapping identifies the original source overload. An indirect call exposes its target expression. `CallKind` describes the retained execution form (such as direct or indirect); it does not reconstruct whether an indirect call originated as virtual dispatch. `SourceDispatchKind` is nullable and requires an explicit surviving source association. Stock virtual-call lowering reuses the original routine-reference node as a new call and overwrites/replaces the original call expression. Recognizing table-access patterns is not an authorized substitute for the lost reference. [Virtual-call rewriting](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/lower_il.c#L12524-L12544)

`FunctionSymbol.Parameters` and `ReturnType` describe the current stored signature, including receiver, return-slot, VTT or other ABI parameters where present. A parameter's explicit EDG role can distinguish those from source parameters; the reader does not remove an assumed first parameter or infer a source signature from names/types. `SourceParameters` is an optional retained-source collection, empty when unavailable. Likewise, `Receiver` is populated only from a retained explicit association or tagged ABI role. Current call operands preserve executed default-argument expressions when lowered into arguments, without automatically preserving that they came from a default.

`ConstructedType.OriginalDefinition` means template definition, never prelowering type. Optional `OriginalType` or `LoweringOrigin` is limited to an explicitly supported, backend-accessible association; names and shapes do not establish one. In particular, `a_type.variant.typeref.orig_type` is documented for lowering-internal use only, and `a_type.variant.routine.unlowered_type` is expressly not for backend use. They cannot populate a generic `OriginalType` property. Until an allowed retained association is established for a type family, that property is null. [Private typeref original](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L10741-L10751), [private routine original](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L10243-L10255)

## Normal navigation examples

The following examples illustrate the proposed API surface. Enumeration is ordinary C#; nullable relationships need only the usual guards.

```csharp
using RoslynObj;

using var obj = RoslynObject.Open("sample.roslynobj");
var unit = obj.TranslationUnit;
var model = unit.SemanticModel;

foreach (var function in unit.Functions)
{
    Console.WriteLine($"{function.Name}: {function.ReturnType.Name}");
    foreach (var parameter in function.Parameters)
        Console.WriteLine($"  {parameter.Name}: {parameter.Type.Name}");

    if (function.Body is not { } body)
        continue; // For example, an external declaration has no body here.

    foreach (var call in body.DescendantOperations().OfType<CallOperation>())
    {
        var target = call.TargetMethod;
        Console.WriteLine($"calls {target?.Name ?? "<indirect>"}");
        Console.WriteLine($"result: {call.Type.Name}");
        foreach (var argument in call.Arguments)
            Console.WriteLine(argument.Value.Type.Name);

        // These methods accept model objects, not database IDs.
        var selectedSymbol = model.GetSymbol(call);
        var resultType = model.GetType(call);
        var sourceOverload = call.SourceTargetMethod; // Optional retained origin.
        if (target is not null)
            foreach (var use in model.GetReferences(target))
                Console.WriteLine(use.Source?.Position);
    }
}
```

`GetSymbol(node)` returns the retained associated/selected symbol or null. `GetType(node)` and `GetOperation(node)` similarly return the corresponding object when applicable. They do not perform C++ lookup, parsing or inference. `GetReferences(symbol)` enumerates retained uses in that object's translation unit. `FindSymbols(name)` is a name search, not C++ lookup in an arbitrary source context.

```csharp
// Inspect members, target layout and a retained template relationship.
foreach (var type in unit.Types)
{
    foreach (var member in type.Members)
        Console.WriteLine(member.Name);

    if (type.Layout is { } layout)
    {
        Console.WriteLine($"size {layout.Size}, alignment {layout.Alignment}");
        foreach (var field in layout.Fields)
            Console.WriteLine($"{field.Field.Name}: {field.Offset}");
    }

    if (type is ConstructedType constructed)
    {
        Console.WriteLine(constructed.OriginalDefinition.Name);
        foreach (var argument in constructed.TypeArguments)
            Console.WriteLine(argument.Name);
    }
}

foreach (var function in unit.Functions)
{
    if (function.Template is { } template)
        foreach (var parameter in template.Parameters)
            Console.WriteLine(parameter.Name);

    if (function.Instantiation is { } instance)
    {
        Console.WriteLine(instance.OriginalDefinition.Name);
        foreach (var argument in instance.Arguments)
            Console.WriteLine(argument.Kind); // Type, value, template or pack.
    }
}
```

Template arguments have typed variants; `ConstructedType.TypeArguments` is a type-only convenience collection and never substitutes for the full ordered heterogeneous `TemplateArguments` list. `ConstructedType` is exposed only when its defining relationship is retained; otherwise the type remains an ordinary type with optional origin metadata. Uninstantiated patterns and actual instantiated bodies remain distinct. Missing unused bodies do not cause the reader to instantiate a template.

```csharp
// Inspect EDG's already-lowered exception/cleanup representation.
foreach (var function in unit.Functions)
{
    if (function.Body is not { } body)
        continue;

    foreach (var region in body.ExceptionHandling.CleanupRegions)
    {
        Console.WriteLine(region.NextRegion?.Ordinal);
        foreach (var action in region.Actions)
        {
            Console.WriteLine(action.Kind);
            Console.WriteLine(action.TargetMethod?.Name);
            Console.WriteLine(action.Storage); // A symbolic location, not host memory.
        }
    }

    foreach (var state in body.DescendantOperations().OfType<CleanupStateOperation>())
        Console.WriteLine(state.Region?.Ordinal);

    // Supplemental lifetime metadata may be absent after lowering.
    foreach (var lifetime in body.Lifetimes)
        Console.WriteLine(lifetime.Kind);
}

// Inspect available macro provenance through normal object relationships.
foreach (var invocation in unit.Macros.Invocations)
{
    Console.WriteLine(invocation.Definition.Name);
    Console.WriteLine(invocation.Definition.Text);
    Console.WriteLine(invocation.ArgumentText); // Canonical text, not original trivia.
    for (MacroInvocation? current = invocation; current is not null; current = current.Parent)
        Console.WriteLine(current.Position);
}
```

`FunctionBody.ExceptionHandling` is a nonnull immutable view whose collections are empty when unavailable or inapplicable. Normal destructor calls inserted into the body are normal `CallOperation` objects. Exception regions/actions and cleanup-state operations expose the normalized records EDG retained; the API does not regenerate lifetime chains, execute the cleanup machinery or emit runtime code. Partial-EH table regions, array cleanup records, conditions, VTT/VLA details and symbolic storage references require typed properties for the selected profile. Original lifetime descriptors may already be freed despite a retention option; `body.Lifetimes` is supplementary and can be empty. The lowering document is authoritative for which records survive.

For a particular operation, `operation.Source?.MacroExpansion` gives the available expansion link. `MacroInvocation.Children`, symbol `Declarations`, template `Instances` and similar optional collections remain easy to enumerate when nothing was retained. No caller must query capability metadata before ordinary traversal.

## Optional detail status and required completeness

Advanced callers can inspect `unit.Capabilities` or call `model.GetAvailability(entity, SemanticProperty.Source)` to distinguish known absence, unsupported optional detail, disabled retention, a dependent fact, or information lost at capture. This separate metadata does not alter the normal property signatures. For example, an empty `unit.Macros.Invocations` can mean no macros or a profile without invocation retention; a caller needing that distinction asks for it explicitly.

Sparse availability metadata and manifest-level capabilities avoid storing a status row per ordinary scalar. Status is never an excuse for dropping a required executable payload or semantic dependency. A producer missing a required adapter fails export. A reader lacking a required schema version/kind/capability reports an incompatible object; it does not publish a supposedly complete model with an opaque `UnknownOperation` fallback. Optional unsupported collection properties, by contrast, correctly return empty collections.

The reader reports EDG facts and recorded relationships. It does not redo overload selection, type equivalence, template deduction, constant evaluation, layout or virtual analysis. Preserve raw flags such as `compiler_generated` independently from richer origin relationships: false does not prove that source explicitly contained a construct. EDG documents contexts in which generation is implied by the surrounding construct. Optional `Source`, `OriginalType` and `LoweringOrigin` remain absent unless supported by retained evidence. [Expression flags and ranges](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/il_def.h#L15083-L15118)

## Traversal and identity

`Children` follows the designated structural relationships of the retained representation. `DescendantOperations()` follows executable containment in deterministic role/ordinal order and visits each underlying operation identity once. It never implicitly follows types, callees, template origins or cleanup graphs as if they were sequential executable children. Explicit `Operands`, `StructuralParents`, references and exception-region properties handle graph relationships. Child order does not assert C++ evaluation sequencing.

`FunctionBody.Root.Children` belongs to execution containment. `function.Declarations` and `unit.Declarations` are separate declaration-occurrence collections, not extra children appended to that body. A local declaration can reference the same initialization represented in the execution graph; following the declaration reference does not execute or enumerate that initialization a second time. Declaration-node `Children` contains only designated declaration structure, while symbol/type/initializer associations remain explicit reference properties. This distinction avoids duplicate operations while preserving natural navigation from declarations to their symbols and initializers.

Shared entities need not have a unique parent; `StructuralParents` is a collection. Source occurrence navigation, when available, has separate occurrence identity. A retained source occurrence can be associated with several generated execution nodes. Source correspondence does not replace execution containment or synthesize an original parent tree. A source-occurrence visitor can revisit one semantic entity through distinct occurrences, while ordinary operation traversal remains identity-based.

Equality follows artifact identity plus internal entity identity, not name or managed reference equality. IDs exist in the storage/advanced debugging layer; ordinary APIs resolve relationships to objects. A new compilation gets a new artifact identity. Multiple declarations and references to one symbol return equal symbol objects; equal-looking entities in different objects remain distinct.

## Lazy loading, disposal and thread safety

Public model values and collections are immutable. Properties lazily load typed records and bounded relation pages, with batched fetching so a traversal does not require one query per edge. Opening does not eagerly materialize all bodies, types, macros or external source attachments. A read-options memory budget controls caches without reducing semantic completeness. Potentially large queries/traversals accept cancellation where appropriate; ordinary collection properties retain their simple signatures.

`RoslynObject : IDisposable` owns connections and caches. Handles and collection views support concurrent reads while the owner is alive; individual enumerators are not shared between threads. Disposal prevents new reads and coordinates completion/cancellation of active reads before releasing resources. Subsequent database-backed access throws `ObjectDisposedException`; detached materialized values remain ordinary values. No property silently reopens the object. Every lazy read observes the same fixed snapshot. The initial API does not promise a retained EDG warning collection; object-open and exporter failures are operational diagnostics.

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

The selected lowered profile sets source-sequence generation off; source-position/macro retention is a separate facility. `GENERATE_SOURCE_SEQUENCE_LISTS` is not a promise of source-order reconstruction after lowering: ordinary lowering suppresses or fails to update those lists. Enabling it with lowering requires the compatibility escape, and `PRESERVE_SOURCE_SEQUENCE_LISTS_WITH_IL_LOWERING` is documented as unusual and less well debugged. The API therefore exposes surviving declarations and positions without promising a complete lexical declaration sequence, original structured C++ tree or one-to-one source operation correspondence. A future preservation profile needs evidence that its retained lists remain usable. Exact settings are owned by [lowering-and-integration.md](lowering-and-integration.md). [Upstream source-sequence configuration](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/host_envir.h#L1546-L1590)

The current backend boundary cannot recover transient lexer information. `fe_wrapup` frees front-end-only storage, or all regions when the IL is to be reread. Token spelling can disappear even before the next `get_token` returns because string-literal lookahead crosses lines. Nonreusable token caches are discarded after use; reusable caches exist for selected delayed constructs, not the full TU. `a_macro_text_map` describes mapping over transient definition/argument/expansion buffers but is not a complete persisted token-provenance contract. [Wrapup freeing](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/fe_wrapup.c#L842-L854), [spelling lifetime and token caching](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/lex_pp.rst#L324-L466), [temporary text maps](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/src/lexical.h#L2118-L2182)

Inspection of stock lexical, macro, preprocessing and initialization interfaces plus the customization documentation did not establish an existing callback for exporting every token/comment/macro-argument event. Internal `get_token`, cache APIs and macro-text maps are not such callbacks. Do not add an interception patch, symbol-renaming wrapper, private lexical pass, replacement preprocessor or core change under the label of a build hook. A future full-token proposal requires identifying a real supported hook and proving event timing, rescan identity and ownership first; it is outside the approved baseline. The documented replaceable-code facilities do not themselves establish that hook. [Documented customization boundary](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/build.rst#L624-L644)

The `--comments`/`-C` switch preserves comments in preprocessing output only. Source-sequence comments in broad descriptions do not establish a retained general comment entity or complete trivia stream; even the source-generating backend documents elimination of comments. A separate preprocessing run is not substituted for the semantic run and does not supply missing semantic-token associations. [Preprocessing option](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/ext_intf.rst#L587-L595), [documented source-output losses](https://github.com/edgcpp/compiler/blob/89e67e07c7f6d0fc38622f9778d8e79f40deaa0f/doc/source/cp_gen_be.rst#L11-L21)

Optional source archival uses external attachments for exact bytes; `.roslynobj` stores only normalized attachment references, encoding, hash and acquisition provenance. It stores no raw-byte BLOB, binary payload or encoded substitute. `FrozenCompilationInput` is valid only when the build environment guarantees the archived bytes were the compilation inputs. A backend reread of a path after compilation is labeled `PostCompilationReread`, not exact compilation input. Bytes unavailable from a file, PCH or module are explicitly unavailable. Archived whitespace/comments remain accessible as text without claiming a tokenized trivia tree; disabled preprocessor branches remain text without invented branch decisions. Reconstructing exact input from line/column positions or normalized macro strings is impossible.

## Required relational support

The central normalized schema should support these relations with typed columns and foreign keys rather than serialized semantic payloads:

1. Artifact/TU ownership, typed entity identities, phase/configuration/capabilities, and sparse optional-detail availability metadata behind the simple public properties.
2. Structural child links `(owner, role, ordinal, child)`; source occurrence containment; symbol declaration/definition occurrences; separate typed reference relations.
3. Lowered expression/statement/initialization rows and operation views, type constituents/wrappers, selected lowered callees and separate retained source targets, value categories; instantiation arguments/origins distinct from lowering origins; target layout and virtual facts; normalized exception region/array cleanup tables, typed actions, symbolic storage and cleanup-state relationships. Lifetime descriptors are supplemental only when still live.
4. Physical source document metadata and optional external byte-attachment references; source-history/include occurrences; sequence-to-file/line remapping segments; retained positions with expansion/original coordinate pairs and macro invocation; ranges referencing endpoint positions.
5. Macro directive/definition versions and their EDG text, flags and replacement range; actual macro invocation rows with parent, definition, original endpoints, invocation order and optional canonical argument-list text. No rows for EDG packing placeholders.
6. Optional source archive provenance and encoding/mapping availability. A full token table, per-argument expansion table, paste/stringize lineage table or trivia binding table is not required by the baseline because its population has no established upstream capture mechanism.

Do not invent source positions, expanded spelling, argument boundaries or origin associations merely to populate a relation. Preserve ordinary browsability: macro definition and canonical argument strings are scalar textual facts, not JSON transports. Text storage is not a substitute for normalized semantic relationships.

## Acceptance questions after plan approval

Validation should exercise ordinary object navigation: enumerate `Functions`, navigate bodies/calls/selected lowered callees, distinguish any retained source-overload relationship, inspect types/members/layout and template instances, traverse lowered cleanup-region/action/state relationships, and enumerate empty unsupported optional collections without mandatory status handling. Check available redeclaration, type-wrapper and origin metadata without reconstructing anything lowering discarded. For source provenance, trace nested object-like/function-like macro origins; preserve redefinition/undef and dynamic builtin distinctions; show canonical argument text without claiming original whitespace; map a repeated include and `#line`; and distinguish logical columns from bytes for Unicode and tabs. Advanced capability queries should explain unavailable token/trivia detail when requested. Check bounded lazy reads and concurrent read/disposal behavior as reader functionality. No corruption campaign or independent semantic correctness verifier is proposed.

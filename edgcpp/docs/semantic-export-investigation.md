# EDG semantic export for a C# implementation of the backend

Investigation date: 2026-10-03. Historical finding: feasible and recommended for a
bounded prototype. Implementation has since progressed; see the
[export/reader checkpoint](semantic-export-implementation.md) and
[managed C emission checkpoint](semantic-export-c-emission.md).
This is an alternative architecture, not an
authorization to replace the current emitter. Investigation changed no compiler
source or build and used the existing `build/bin/cpfe-cp` only.

## Finding

Yes. EDG can provide almost exactly the semantic foundation requested: a
structured, typed representation with resolved names and overloads, instantiated
templates, explicit implicit operations, source correspondence and destruction
lifetimes. The appropriate input is its **unlowered high-level IL**, augmented
with its source-sequence lists, preprocessing provenance and retained source
text. A small native exporter can save that information in a versioned graph;
the actual transformations and C# emission can then run entirely in C#.

This does not require translating EDG itself into C#, nor exposing its raw
memory through P/Invoke. EDG need only run as a build tool. Its existing C++
source-regenerating backend is direct evidence that resolved semantics and
recognizable source structure can coexist. It is a better reference for this
exporter than starting from the C-generating backend's lowered output.

The qualification is important: this is a **semantic graph with source-form
information**, not an already lossless Roslyn-style concrete syntax tree. We
must retain source text and explicitly preserve the distinction between the
written form and implicit semantic operations. We must also implement C++'s
runtime operations in the managed backend or keep selected EDG lowering as an
optional supplementary route. Moving to C# does not remove that work.

Primary evidence is the local [IL overview](../upstream/doc/source/il.rst)
(lines 7–37, 149–208), [C++ regeneration documentation](../upstream/doc/source/cp_gen_be.rst)
(lines 5–62), and the actual [IL declarations](../upstream/src/il_def.h).
Line numbers below refer to this inspected checkout; documentation occasionally
describes historical configurations, so source assertions take precedence.

## What was checked experimentally

An isolated [probe](../artifacts/campaign/persistent-continuation/semantic-export-investigation-001/probe.cpp)
contains overloads, `twice<int>`, a class-template partial specialization,
nested RAII objects, a `for`/`if`/`break`, and nested API macros. Existing
`cpfe-cp --c++17 --instantiate used --il_display` completed successfully and
saved both native EDG IL and regenerated C++.

The [regenerated source](../artifacts/campaign/persistent-continuation/semantic-export-investigation-001/probe.int.c)
keeps `Guard`, `choose`, `work`, local names, `for`, `if`, `break` and the
template references. It retains the source form `2 + 3` rather than only `5`.
Macros are expanded and the ordinary comment is absent. The
[unlowered IL display](../artifacts/campaign/persistent-continuation/semantic-export-investigation-001/stdout-002.txt)
records the selected `choose(int)` routine (around line 6274), instantiated
`twice` routine (6299), class-template argument and partial-specialization
argument lists, two block lifetimes and `~Guard` destruction entries (5546–5615),
and structured `stmk_if`/`stmk_for` nodes (6243, 6558).

This is evidence for the extraction boundary, **not** a test of a new exporter
or managed emitter. This existing executable did not record the optional macro
tables in its display; macro recording was verified in source/documentation,
not enabled in a new build. Commands, output, the initial missing predefined
macro-file diagnostic and the successful retry are retained with a
[hash manifest](../artifacts/campaign/persistent-continuation/semantic-export-investigation-001/evidence.json).
The successful self-contained probe disabled the predefined macro file through
the existing `use_predefined_macro_file` configuration flag. It did not exercise
platform headers or project ABI compatibility.

## Available information and its limits

| Requirement | EDG evidence and proposed export |
| --- | --- |
| Names, declarations, members, scopes | `a_source_correspondence` carries source name, declaring scope, linkage and declaration location. Source-sequence secondary declarations distinguish a forward declaration from its later definition and retain each occurrence's written type. Export symbol identity separately from declaration occurrences. See `il.rst:390` and `cp_gen_be.rst:28`. |
| Structured statements | Statements remain trees; source-sequence lists preserve declaration/statement ordering. Export blocks, loops, conditionals, switches, labels and lexical declarations, not just a CFG. Use a derived CFG for analysis. |
| Exact types and conversions | Expression nodes carry types, `orig_lvalue_type`, lvalue/xvalue flags and conversion operators. Declared type spelling can differ from canonical type. Some lvalue-to-rvalue/function-to-pointer conversions are represented by flags rather than extra nodes: normalize deliberately, do not infer from node kind alone. See `il_def.h:14915` and `il.rst:2230`. |
| Overload/member resolution | Direct calls identify a resolved routine; field accesses identify fields. Unlowered member calls distinguish `x.f()` from `p->f()` and preserve virtual-call information. See `il_def.h:14247`. A virtual call or function-pointer call still has a runtime target; semantic resolution does not promise a single actual callee. Export static target/dispatch contract plus receiver or pointer expression. |
| Templates | Concrete instances contain substituted types, argument lists and links to templates. Class instances have both primary-template and partial-specialization argument lists (`il_def.h:9101`). Preserve template definition/prototype separately from executable specializations. |
| Object initialization/destruction | Dynamic initialization entries identify constructors/destructors and object lifetimes carry ordered destruction lists, parent lifetimes and cleanup cursors. See the detailed lifetime contract below. |
| Source locations/ranges | `EXTRA_SOURCE_POSITIONS_IN_IL` preserves expression, initializer, identifier, specifier and declarator ranges. Map sequence numbers through `conv_seq_to_file_and_line`; preserve expansion and spelling origins separately. See `il.rst:503` and `basics.h:897`. |
| Constants and original expressions | Constant values can retain a backing expression (`a_constant.expr`); export both. `REDUCE_BACKING_EXPRESSION_USE` can omit some implicit-conversion backing expressions in large aggregate initializers (`host_envir.h:1471`). Source bytes remain necessary for exact literal spelling. |
| Macros | Optional `a_macro` records include definitions, undefinitions, source positions, command-line/predefined origin, object-like status and normalized text. Invocation records include nesting, definition identity, ranges and optionally normalized arguments (`il_def.h:17442`). |
| Layout/ABI | Export EDG's chosen sizes, alignment, bitfield offsets, base relations, calling conventions and target configuration. Keep these facts separate from source declaration order and spelling. Abstract virtual dispatch/member pointers still require an implementation. |

The native frontend remains responsible for parsing, lookup, overload
selection, access checking, template deduction/specialization and constant
evaluation. The managed backend consumes their results. It must not repeat
C++ overload resolution using names and argument types.

Even unlowered IL is not a literal syntax-node-for-node copy. In the probe,
`break` is a compiler-generated `stmk_goto` to a label marked `break_label`;
`cp_gen_be.c:22539` restores `break` from that semantic marker (and similarly
restores `continue`). The neutral source view should normalize these known
forms into `Break`/`Continue` while retaining their target and lifetime facts.
This is a local, documented reconstruction, not general recovery of structured
control flow from arbitrary jumps.

### Template completeness is relative to a build

An unused dependent template cannot have all methods and types fully resolved
without actual arguments. Keep such definitions as source/prototype information
with `resolution: dependent`; only concrete instantiated bodies enter executable
emission. The IL explicitly distinguishes these cases
([template representation](../upstream/doc/source/il.rst), lines 3030–3117).

Use an explicit instantiation policy such as `--instantiate used`, plus the
project's explicit-instantiation definitions. Do not use the documented default
`none` and assume every external template body was emitted. `all` is an EDG
emission policy, not enumeration of every possible argument combination.
`extern template`, missing definitions, imported modules and unresolved external
dependencies must appear in the package as such. A library can have an explicit
manifest of template instances required by its intended managed API. New
specializations require EDG to run again.

Keep instance discovery separate from source-sequence traversal. Enabling
`CLASS_TEMPLATE_INSTANTIATIONS_IN_SOURCE_SEQUENCE_LISTS` and its nonclass
equivalent is deprecated and guarded by explicit compatibility switches in
[host_envir.h](../upstream/src/host_envir.h) at lines 1592–1647. Ordinary semantic
graph traversal discovers instances without using that deprecated mode.

### Lifetimes are especially suitable for export

EDG's unlowered IL deliberately describes cleanup obligations without imposing
an exception runtime. `an_object_lifetime` contains ordered destructions and
parent/sublist links. Initializations determine the active destruction cursor;
`goto` has a `common_lifetime`; full-expression lifetimes and temporary lifetime
extension are already represented. This is explained with examples in
[il.rst](../upstream/doc/source/il.rst), lines 1457–1628.

Export those primitive facts intact and derive an owned managed lifetime model:
construction events, successful-construction guards, normal/exceptional cleanup,
array construction progress, scope/label exits, full-expression boundaries,
constructor failure and ownership transfer of returned temporaries. Preserve
the selected destructor and whether the cleanup concerns a complete object,
base, member or array. An explicit cleanup-edge view is useful, but must be
derived from the cursor and path state, not from “destroy every local at the
end of its lexical block.” The latter is wrong for partial construction,
backward jumps, temporaries and conditional construction.

Managed lowering can then use `try/finally` and the existing exception runtime
while retaining recognizable body structure. EDG C lowering and
`lower_init.c`/`lower_eh.c` remain implementation references. The native C backend
is a differential oracle for cleanup traces.

These EDG lifetime records primarily concern objects requiring destruction.
They are not a universal lifetime/safety model for every C object, VLA or
allocation. Export storage duration, scopes and other relevant operations too.
Do not turn the documentation's simplifying discussion of trivial objects into
a claim that all C/C++ object-lifetime rules disappear.

## Recommended boundary and alternatives

```text
original source + headers + target/dialect/preprocessor configuration
  -> EDG semantic analysis and required instantiation, with lowering disabled
  -> semantic_export_be.c: source/semantic/lifetime/ABI graph + source bundle
  -> managed package reader, validation, per-TU linking and transformations
  -> C# emitter + existing runtime/libc adapters + configurable file packaging
  -> normal .NET build or NativeAOT publish
```

Names are proposals. The exporter should be narrow native code; formatting,
naming policy, source grouping, lowering and C# generation belong in C#.

| Option | Assessment |
| --- | --- |
| Dedicated backend with `DO_IL_LOWERING=0` | **Recommended.** Existing backend callback after semantic wrapup provides unlowered IL, including instantiated bodies and lifetime facts. Use EDG region walkers/read APIs and source-sequence helpers. No need to modify parsing/semantic behavior. |
| Native library embedding | Supported through `MAKE_FRONT_END_CALLABLE` and `EDG_MAIN`/`edg_main`. Useful if an in-process host is later needed, but not a Roslyn-like persistent public object model. The entry calls `fe_cleanup()` before returning; export/copy inside the backend callback. Use a narrow C ABI owned by us rather than exposing EDG structs to managed code. See `admin.rst:24` and `cfe.c:219`. |
| Existing EDG binary IL as durable managed format | Good private cache or bridge to a native exporter built with the same frontend, **poor public interchange format**. The writer serializes configured native struct bytes and remapped pointers, and embeds compiler version information (`il_write.c:411`, `677–723`). A C# reader would be coupled to host layout, compile-time options, version and IL variants. |
| Parse `--il_display` output | Useful investigation fixture only. It is a diagnostic rendering with addresses, not a stable typed schema. |
| Serialize the current backend-owned graph | Insufficient alone: `backend/dot_gen_ir.h:1` explicitly owns physical types only, not operations, symbols or lifetimes. Reuse its validation lessons, but it is not already the requested semantic tree. |
| Export only after the existing C/C++ lowering | Easier short-term reuse, but gives up much of the source-oriented advantage. Can be a compatibility/ABI sidecar, not the canonical source view. |

The callback timing matters. Lowering currently occurs as top-level functions
finish and again for file scope, not as one optional pass immediately before
`back_end()`: see [lower_il.rst](../upstream/doc/source/lower_il.rst), line 195,
and [scope_stk.c](../upstream/src/scope_stk.c), line 8960. A callback inserted
late in the existing lowering configuration cannot recover discarded data.
`fe_wrapup.c:551` can detach lifetime information and `617` clears parent
membership information after lowering. Source-sequence preservation with
lowering is explicitly described as unusual and less tested in
`host_envir.h:1562`; backing expressions are not maintained by lowering either.

Start with a separate no-lowering target. If migration needs EDG ABI lowering,
save the canonical source graph before mutation and produce a separately
identified implementation graph with origin mappings. Do not assume native
pointers or traversal order remain stable across independent frontend runs.
A same-run dual export requires carefully timed per-region capture, or an owned
copy; a late snapshot is insufficient. This is extra engineering, not required
for the first plain-C prototype.

### Proposed configuration checklist

Resolve the actual effective configuration and record it in each package:

- `DO_IL_LOWERING=0`; use the real backend callback, not `--no_code_gen`.
  `--no_il_lowering` in a lowering build also suppresses its backend; it is useful
  for obtaining private IL, not directly invoking the new exporter.
- `GENERATE_SOURCE_SEQUENCE_LISTS=1`, `EXTRA_SOURCE_POSITIONS_IN_IL=1`,
  `ALL_TEMPLATE_INFO_IN_IL=1`, retained prototype/template information and
  backing expressions; consider `RECORD_TEMPLATE_STRINGS=1` and
  `MAINTAIN_CLASS_MEMBER_LIST=1`. Keep semantic instances outside source ordering.
- `RECORD_MACROS_IN_IL=1`, `FULLY_RESOLVED_MACRO_POSITIONS=1`,
  `MACRO_INVOCATION_TREE_IN_IL=1`, `RECORD_MACRO_ARGS=1` for full macro provenance.
  Their dependency assertions are in `basics.h:788–846`.
- Disable removal of unneeded declarations for API/source export. Keep liveness
  as metadata and let the managed build select executable roots separately.
- Preserve target/dialect options and needed GNU/Microsoft extensions from the
  existing profile. `DOING_SOURCE_ANALYSIS` defaults differently without
  lowering and relaxes some ABI checks (`host_envir.h:1444`); audit that choice
  before using exported layouts for executable code.

This list is a prototype configuration proposal, not a build-tested combination.
Some historical switches changed: for example,
`FULL_SOURCE_POS_IN_IL_STATEMENT=0` is now rejected (`host_envir.h:1408`).
Do not copy an old documentation configuration blindly.

## Macros, original text and public APIs

Preserve the original bytes of every input/header, with content hashes,
encodings and physical path identities. Keep presumed locations affected by
`#line` separate from physical locations. EDG columns are not necessarily byte
offsets: Unicode characters, line splicing and macro expansions require an
explicit mapping. A node produced by a macro may not correspond to one
contiguous source slice. Capture token/expansion provenance where available,
and mark missing or approximate ranges rather than inventing precision.

The macro package should include definition/undefinition events in order,
identity of each definition version, replacement text, parameter information,
invocation nesting, argument text and expansion-to-semantic-node links. EDG's
macro argument strings normalize whitespace; original source is still required
for exact spelling. Builtins such as `__LINE__` have per-use values and no fixed
replacement text. Command-line definitions need separate provenance.

**Recording a macro is different from translating it into a managed API.**

- Object-like constants can often become source-named C# constants or readonly
  storage. Determine their type/value through EDG in the actual use context or
  a dedicated probe; a macro definition by itself has no universal C type.
- String, address, `sizeof` and target-dependent constants need corresponding
  representation/initialization, not unconditional `const int`.
- Function-like macros may require typed wrappers or per-use expansion. Replacing
  `#define TWICE(x) ((x)+(x))` with a function changes side effects for `i++`.
  Statement macros, type-generating macros, token pasting/stringification,
  caller-scope control flow and variadic forms cannot all become ordinary C#
  methods. Preserve their public API description and use checked adapters where
  a callable API is meaningful.
- Keep expansion structure for faithful translated bodies; optionally preserve
  a macro-origin annotation or use a proven equivalent named constant/wrapper.
  Do not reintroduce the textual macro name when its expansion has different
  evaluation or type semantics in C#.

Inactive `#if` branches have source text, not resolved semantics for the current
configuration. Comments/trivia are not a lossless IL layer in the inspected
path. Retained source permits a future token/trivia attachment pass; it does
not make skipped branches semantically analyzed. Macro provenance also cannot
be recovered if we feed EDG only an externally preprocessed `.i`/`.ii` file.
Use original sources and let EDG preprocess when this fidelity is requested.

## A durable graph contract

Use separate collections with references by IDs, not recursive JSON objects or
native addresses. Declarations, types, symbols and templates form cycles and
shared references. Begin with JSON or JSON Lines for inspectability, with
per-function chunks and tables; add an equivalent compressed/binary encoding
only after measuring project size and load time.

| Collection | Essential content |
| --- | --- |
| Package | Schema version, producer revision/config hash, target ABI and widths, source dialect/extensions, original invocation and include/define inputs, dependency hashes, capabilities and completeness diagnostics. |
| Sources/preprocessing | Source bytes/content hashes, encodings, include instances, physical/presumed/expansion locations, definitions and invocations, optional retained tokens/trivia. |
| Symbols | Stable package-local ID, kind, source/qualified/linker names, linkage and TU identity, declared/canonical type, declaring scope, declaration occurrences, definition, attributes/visibility. |
| Types | Interned qualified and canonical type graph, spelling aliases, records/enums, members, bases, function/ref/member-pointer types, extents, layout and calling convention. Preserve typedef spellings separately. |
| Syntax/operations | Structured statements and expression operations, ordered children/source occurrences, type and value-category facts, semantic references, constants/backing expressions, explicit/implicit origin, dispatch and sequencing semantics. |
| Templates | Definitions/prototypes with dependent status, concrete instances, primary/selected template references, deduced and written arguments, instantiation provenance/body completeness. |
| Lifetimes | Regions, construction events, destruction lists, parent/sublist links, active cleanup cursors, scope-exit targets, temporary extension, storage ownership and EH relationships. |
| Linking | External definitions/references, inline/COMDAT selection rules, internal identity, initialization requirements and ABI fingerprints. |

Illustrative **proposed** records for part of the probe, not actual exported
JSON or a complete executable package:

```json
{
  "schema": "edg-semantic/1",
  "nodes": [
    {
      "id": "tu0:expr17", "kind": "Call",
      "source": { "file": "source0", "line": 17, "column": 14 },
      "type": "type:int", "valueCategory": "prvalue",
      "dispatch": "direct", "target": "tu0:symbol:choose_int",
      "arguments": ["tu0:expr18"], "implicit": false
    },
    {
      "id": "tu0:expr18", "kind": "Call",
      "type": "type:int", "valueCategory": "prvalue",
      "dispatch": "direct", "target": "tu0:symbol:twice_int",
      "templateInstance": "tu0:instance:twice_int",
      "arguments": ["tu0:expr19"]
    }
  ],
  "instances": [
    {
      "id": "tu0:instance:twice_int", "definition": "tu0:template:twice",
      "arguments": [{ "kind": "type", "type": "type:int" }],
      "resolution": "concrete", "bodyAvailable": true
    }
  ],
  "lifetimes": [
    {
      "id": "tu0:lifetime:loop_body", "kind": "block",
      "parent": "tu0:lifetime:work",
      "destructions": [{ "initialization": "tu0:init:inner", "destructor": "tu0:symbol:Guard_dtor" }]
    }
  ]
}
```

Use deterministic IDs for a fixed package/build, independent of addresses.
Separate those IDs from cross-package semantic linkage keys and from optional
edit-stable source IDs. A source location alone is not a unique identity:
macros, instantiations, anonymous entities and repeated includes can share it.
Physical type structural equality is not C++ nominal identity.

Reject unsupported required node kinds/capabilities explicitly. Validate all
edges, completed definitions, qualified types, source identities and lifetime
references before emission. Use typed managed records and a source-generated
serializer or manual reader; no runtime code generation is necessary. The graph
and Roslyn emitter are build-time tools, so generated projects need not ship
EDG, the graph reader or Roslyn. Existing NativeAOT runtime constraints remain.

## Multiple files and platforms

EDG has an actual multi-TU merge mode with duplicate elimination and conflict
checks ([trans_unit.rst](../upstream/doc/source/trans_unit.rst), lines 7–43).
However, the inspected source **rejects combining
`COMPILE_MULTIPLE_TRANSLATION_UNITS` with `MACRO_INVOCATION_TREE_IN_IL`**
(`host_envir.h:387`). Do not remove that assertion or claim the two requirements
are already compatible.

Prefer one semantic package per TU and a managed assembly/link step when full
macro history is needed. Preserve TU boundaries, internal/static/anonymous
identity, source configuration and external linkage. Merge compatible external
declarations/definitions using language-aware identity and ODR/COMDAT rules;
do not unify everything named `X` or everything with the same layout. C tags
in different TUs and C++ nominal types need different treatment. Conflicting
target profiles or macro-dependent definitions must be diagnosed. This merger
is real work; existing driver symbol/COMDAT handling can inform it but does not
establish a complete semantic linker.

EDG's multi-TU mode with macro-tree recording disabled remains a useful optional
comparison/whole-program mode. Separate repeated compilations under
`COMPILE_MULTIPLE_SOURCE_FILES` are not that merge mode.

An exported graph describes one target/preprocessing configuration. It does not
make platform-specific `#if` branches dynamically selectable after translation.
Continue the existing portable target headers/profile and BCL/runtime-selected
platform glue. If another source configuration is required, export it separately
and adapt it deliberately; do not combine incompatible layouts into one type.
Generated file grouping remains independent: preserve `none`, `source`,
`function` and size splitting. An amalgamation can retain original symbol names
and source navigation while producing many manageable C# files.

## Minimal prototype and migration

1. Add a separately built native exporter target using supported unlowered
   configuration, plus a small C# reader/validator. First export C declarations,
   types, source order, structured function bodies and locations. Retain all
   original sources and an effective-build manifest. No current emitter changes.
2. Prove semantics and fidelity on small C cases, the existing C++ probe,
   nested/conditional RAII with exception traces, overload/template specialization
   cases and a macro-header API corpus. Check that the reader selects the actual
   semantic references; merely regenerating text is insufficient.
3. Add managed C# emission for the basic C subset using the existing runtime,
   target layout, globals/TLS/instance policy and named bitfield presentation.
   Golden side-by-side output and differential JIT/NativeAOT tests are acceptance
   conditions. Do not replace direct assignments with generic helpers.
4. Translate SQLite through both routes. Verify the existing project checks,
   public API names, source correspondence of representative functions, package
   size, translation time, memory use and split-output navigation. Run meaningful
   failure reductions through the native C oracle.
5. Move C++ features by project need: lifetimes/EH, classes and dispatch,
   allocation/member pointers, templates and library consumers. Keep old output
   as a behavioral reference until each migrated area has parity. Preserve
   origin IDs through every managed rewrite for diagnostic/source review.

The first go/no-go gate is an actual exported graph showing resolved direct
calls, concrete specialization identity, macro nesting/spelling provenance and
correct normal/exceptional destruction traces, together with readable managed
output for a representative C function. The current read-only probe establishes
availability of much of that information, not completion of the gate.

Reuse the existing runtime/libc glue, target profiles, project driver, API export
policy, bitfield/layout tests, JIT/NativeAOT checks and source-fidelity fixtures.
Most work now in the native emitter would move to managed passes; it is not
necessary to discard working runtime behavior or pause the current project
pipeline while the alternative is evaluated.

## Remaining risks and integration facts

- Export breadth is substantial: the graph must cover each project's actual IL
  variants, including extensions. “EDG parses it” does not imply our exporter or
  managed emitter supports it. Unknown semantics must fail with source context.
- EDG upgrades/configuration changes require rebuilding and auditing the native
  adapter. A versioned neutral contract isolates C# from raw structs but does not
  automatically make producer changes compatible. Pin revision and capabilities.
- Preserving all headers, prototypes, backing expressions and macro expansions
  can be large. Intern shared graphs; chunk bodies; separate API discovery from
  reachability-based executable emission. Measure SQLite before choosing binary
  encoding or aggressive pruning.
- Native IL regions can be released between functions; walk/read them using
  EDG's supported region interfaces (`il.rst:43–162`, `il_walk.h:71`). Never keep
  dangling native pointers in the saved graph. Library mode also requires copying
  before cleanup; reentrancy/threading needs configuration-specific validation.
- Assembly, vendor builtins, preprocessing tricks and unsupported native ABI
  interfaces still need explicit policy or adapters. Source faithfulness cannot
  mean emitting unsupported semantics verbatim in C#.
- The local [LICENSE.txt](../upstream/LICENSE.txt) and source SPDX headers identify
  Apache-2.0 with LLVM exception. Library embedding is documented technical
  functionality, not an assumed separately licensed SDK. Preserve the checkout's
  license/notices when packaging the tool; embedded project/header text has its
  own provenance and notices. This investigation makes no broader legal claim.

Recommendation: build the isolated export/managed-reader prototype. The source
and the existing C++ regeneration probe support the architecture strongly. The
most valuable early tests are lifetime fidelity, full macro provenance and
per-TU identity, followed by SQLite source correspondence and execution parity.

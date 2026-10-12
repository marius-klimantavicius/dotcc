# Proposed normalized relational contract

**Implementation approved by the user, 2026-10-12:** phases 3–4 are now authorized through M0–M5. Historical proposal language below records the reviewed design; current evidence and remaining work are tracked in `../roslyn/docs/IMPLEMENTATION.md`.

**Subsequent user scope change (2026-10-12):** C++/CLI and C++/CX implementation and metadata/profile acceptance are excluded. Native Microsoft C++ extensions remain required. Managed-dialect sections below are historical design material and do not impose current implementation gates.

This is the phase-2 design, not an implemented schema. Names below specify the
logical contract; implementation milestone M1 freezes the full DDL and the
field-by-field upstream adapter inventory together. Adding a missed executable
variant requires an explicit typed relation/column and corresponding API support,
not an opaque extension payload.

The primary profile now stores **lowered execution with partial EH**, together
with the type/declaration/source relationships stock EDG retains. Earlier
unlowered-only assumptions are superseded. Compatibility profiles can store
unlowered forms when stock EDG cannot lower a dialect; the manifest states the
profile and remaining execution requirements. No two snapshots are implicitly
combined, and no source shape is inferred from similar names or generated code.

## Identity and normalization rules

One database represents one primary translation unit. Its header records format
version, artifact identity, EDG revision/version, semantic stage, and the actual
target/feature profile. An EDG compatibility mode that internally imports
secondary material must retain its origin; that is different from this library
linking separately compiled objects.

- `entity(id INTEGER PRIMARY KEY, kind_id)` supplies file-local graph identity.
  Each typed entity table uses `id` as its primary key and foreign key. IDs are
  assigned by the exporter, never EDG addresses, source offsets, names, or hashes.
- Integer IDs are local to an artifact. Managed equality includes the artifact
  identity and entity ID; loading a distinct compilation with identical source
  text does not establish C++ entity equivalence.
- An entity has one logical identity, but may have many declaration occurrences,
  references, initialization uses, or source origins. Symbols, literal storage
  objects, and template instances are not value-interned.
- Every reference is a foreign key to a known table or to a deliberately declared
  entity supertype. Avoid uncheckable `(table_name, arbitrary_id)` pairs.
- Every semantic list is an ordered relation keyed by `(owner_id, ordinal)` or
  `(owner_id, role_id, ordinal)`. A list is never a comma-delimited name/ID string.
- Singular fields belong in typed rows. Do not put all semantic fields into a
  universal entity/attribute/value table. A role-discriminated relation is
  appropriate for a genuinely homogeneous list such as expression operands.
- Each scalar dependency lives once. Source filename → source file; spelling →
  text atom; literal occurrence → literal value; member layout → field and record
  layout. Derived display names, counts, and convenience API projections need
  not be materialized. This excludes only genuinely derived base-list views:
  EDG's contextual indirect base-subobject entries, offsets, derivation paths and
  override lists must be retained, not recomputed from direct inheritance.
- Format enums have explicit stable IDs and names. They are independent of EDG
  enum ordinals, which change with `#if` configuration. Mapping is explicit and
  exhaustive per upstream revision/profile.
- Boolean facts use scalar integer columns, not packed semantic bitfields.
  Small discriminants reference schema-owned kind tables. Repeated memberships
  use typed join tables rather than comma-separated flags.
- Optional detail tables keep common rows compact. Missing rows must have a
  defined contract meaning; absence alone cannot ambiguously mean both `false`
  and `not captured`.

All value columns are `INTEGER`, `TEXT`, or null as appropriate. `REAL` is not an
exact constant transport. No semantic `BLOB`, JSON, XML, serialized CLR object,
native struct, or base64/hex-encoded struct is permitted. A canonical decimal
integer stored as text is an atomic number, not a packed structured value.

## File header and shared atoms

| Relation | Key and essential scalar/foreign-key fields |
|---|---|
| `object_header` | Singleton key; format major/minor, artifact ID, producer/revision text IDs, capture/lowering profile ID, target-profile ID, main-source-file ID |
| `translation_unit` | Singleton entity; global scope ID, primary input ID, language mode ID |
| `target_profile` | ABI family, addressable-unit bits, endianness, pointer representation facts EDG exposes |
| `arithmetic_kind` | Stable kind, target bit width/alignment, signedness, precision/radix where applicable |
| `build_capability` | Stable capability key, enabled/state value; actual exported feature configuration |
| `invocation_argument` | Ordinal, argument text ID; one command-line argument per row, not a command string |
| `text_atom` | Integer key, exact text with binary comparison, unique contents |
| `name` | Entity, spelling text ID if named, name category; empty/anonymous distinct |
| `qualified_name_part` | Name ID, ordinal, component name ID; optional retained EDG source-name structure |
| `fact_availability` | Entity ID, schema-defined fact ID, state/reason ID; sparse exceptional states |

The target profile records enough to interpret exported sizes/offsets and scalar
values even though phases 1–4 do not link objects. This is more essential than a
human-friendly list of language switches. Do not store host `sizeof(void*)` as
the target pointer width. Target facts come from EDG's actual target configuration.
Target quantities must not overflow a signed SQLite integer or managed `long`:
the frozen scalar contract provides an exact wide-integer representation where
the EDG field's range requires it. This applies to extents/offsets as well as
literal values; it is not permission to approximate large quantities with REAL.

Availability defaults are specified per field and profile, but do not wrap the
ordinary public API. Collections such as `unit.Functions`, `type.Members` and
`method.TypeArguments` are read-only properties and return empty when absent or
unsupported. Optional singular relationships return null/optional values.
Capabilities and sparse retention metadata let interested callers distinguish
known-empty, dependent, not applicable, lost/not retained, and unsupported optional
data. IDs stay inside the loader; navigating to a member or instance follows an
object relationship, not a `Get(id)` call. Missing required executable adapters,
database I/O failures and incompatible required schema are errors, not reasons
to silently return empty program bodies.

## Scopes, declarations, symbols, and types

| Relation family | Required content |
|---|---|
| `scope` | Kind, semantic parent, associated namespace/routine/record entity, lifetime if EDG supplies one |
| `scope_member` | Scope, ordinal, member symbol; scope membership is not declaration occurrence order |
| `declaration` | Entity kind, symbol, lexical scope, source origin, retained generation flags and explicit/implicit state only when actually known |
| `declaration_sequence` | Scope/container, ordinal, declaration occurrence only if a profile supplies a valid retained source sequence; not populated from guessed source order in the main lowered profile |
| `symbol` | Symbol kind, name, semantic containing entity, declared type, EDG linkage/storage/access facts |
| `namespace_symbol` | Namespace alias target/inline/anonymous facts as applicable |
| `routine_symbol` | Lowered routine type, retained containing record, body state/root, special-function category and retained semantic flags; explicit alternate-entry/thunk/source-origin links where EDG provides them |
| `routine_parameter` | Routine, ordinal, parameter symbol, default initializer reference if retained |
| `variable_symbol` | Type, storage duration, initializer, thread-local/inline facts, associated lifetime/init record |
| `field_symbol` | Record, type, access, mutable/static distinctions, default initializer, bit-field declaration |
| `enum_symbol`, `enumerator` | Underlying type, scoped status, ordered enumerators, EDG evaluated values |
| `alias_symbol`, `using_declaration` | EDG-resolved target/type and retained source occurrence |
| `type` | Type kind, EDG identity/canonical correspondence if exposed; declared/sugar type preserved separately from any underlying type |
| `qualified_type` | Unqualified type, cv/restrict/address-space and extension qualifiers |
| `arithmetic_type` | Arithmetic kind; signedness/width facts are owned by the arithmetic descriptor |
| `pointer_type`, `reference_type` | Pointee/referred type; pointer/ref qualifiers, lvalue/rvalue reference kind |
| `member_pointer_type` | Declaring class and member type; no invented lowered native encoding |
| `array_type` | Element type, bound kind, constant bound or EDG bound expression, variable/dependent/incomplete state |
| `routine_type` | Result, calling convention, language linkage, variadic state, member/ref qualifiers, exception specification |
| `routine_type_parameter` | Routine type, ordinal, parameter type and EDG-retained facts, including hidden receiver/return-slot/VTT/ABI parameter roles |
| `record_type` | Defining class/struct/union symbol, definition state, template instance if any |
| `typedef_type` / `type_reference` | Retained named/sugared type relation and underlying/resolved type |
| Specialized type tables | Vector, complex/imaginary, fixed-point, nullptr, template/dependent/decltype, extension-managed types and every other upstream type family in the inventory |
| `attribute`, typed attribute details | Owner and order, attribute kind, EDG-retained scalar/type/symbol/expression operands |

EDG entities can share source correspondence without becoming the same entity.
Multiple declarations of a routine point to one routine symbol when EDG says so.
Function types and named routine symbols are separate. `T`, `const T`, `T&`, and
an alias spelling of `T` are not flattened into a single display-name key.

Class/member identities can survive while their representation changes: generated
base/vptr fields and lowered signatures belong to the stored execution stage.
Populate the high-level model from retained semantic ownership and flags, not
the post-promotion list that happens to hold a member. Original declared types,
argument defaults and source signatures are separate optional facts only where
backend-accessible EDG data actually retain them. A saved lowering-internal
pointer is not automatically a supported original-type interface.

Dependent/prototype types remain queryable metadata. Canonicalization is an EDG
relationship, not a C# structural-equality algorithm. If upstream discards a
source spelling, the model cannot promise it back.

### Records, inheritance, and dispatch

| Relation | Required content |
|---|---|
| `record_layout` | Record type, completeness/availability, EDG size and alignment, nonvirtual facts where exposed |
| `field_layout` | Field and record layout, EDG target-addressable-unit offset; bit offset/width/container facts where present |
| `direct_base` | Derived type, ordinal, base type, access, virtual flag |
| `base_subobject` | Most-derived type and EDG subobject identity, base type, offset or dynamic/unavailable state |
| `base_derivation_step` | EDG derivation/path identity, ordinal, base relationship; distinguish repeated bases |
| `virtual_member` | Record, member, inherited/introduction/override/final/pure facts that EDG retains |
| `override_relation` | Overrider, overridden member, applicable EDG subobject/derivation context |
| `virtual_dispatch_detail` | EDG-generated slot/adjustment/table/thunk facts present in this profile; no inferred source-call target |
| `lowered_type_relationship` | Backend-supported retained declared/storage/subobject-partner links with their actual relation kind; exclude private `typeref.orig_type` and routine `unlowered_type` copies |
| `abi_routine_relationship` | Primary/alternate constructor-destructor entries, adjustment thunks and wrapper targets supplied by EDG |
| `virtual_table`, `virtual_table_entry` | Generated table variable and ordered typed entries, selected routine/adjustment/RTTI/VTT dependencies |

An EDG target layout offset is a target addressable-unit count with documented
units; do not imply a .NET layout or a globally constant virtual-base offset.
The same base type can occupy different subobjects. API queries must retain that
distinction. A selected virtual declaration is not a statically known ultimate
runtime callee.

Moreover, stock virtual-call lowering replaces the original selected-routine node
with an indirect call (`src/lower_il.c:12524–12542`). The schema can preserve all
resulting dispatch operations/table entities without having a per-call link to
the original source-selected overload. The user accepted this source-query
limitation on 2026-10-12 in favor of lowered execution. Store a source-target
link only when EDG explicitly retains it; otherwise it is absent. Do not infer
the lost link from a slot, signature or mangled spelling. All resulting
execution operations and their semantic dependencies remain required.

## Executable graph

`semantic_node(id, node_kind_id, origin_id, compiler_generated, synthesis_kind_id)` is the common
operation/statement identity. `expression(id, type_id, value_category_id)` and
`statement(id, statement_kind_id)` add their common facts. Detailed facts live
in typed payload tables. An expression's EDG category and implicit conversions
at capture are preserved; they are not inferred from spelling or the selected
C# type. They need not equal the pre-lowering expression's type or category.

Preserve EDG's raw generation flag separately from any richer synthesis-origin
fact. A false `compiler_generated` flag does not prove explicit source spelling;
some generation is implied by the containing construct. A declaration occurrence
can project as a `SemanticNode` without duplicating its stored identity/payload.

Every payload field is classified as a containment operand, semantic reference,
source/backing-expression reference, scalar, or irrelevant parser state.
The API's normal traversal follows the captured executable structure. Retained
source/backing-expression traversal is separate, so metadata cannot accidentally
become a second execution of a lowered operation. A lowered range-for is an
ordinary loop; expose a source range-for only if EDG retained a supported
association. Shared nodes and cycles in semantic references are legal.

| Relation family | Required content |
|---|---|
| `expression_operand` | Owner, role, ordinal, operand; semantic evaluation/sequencing category is separate from list order |
| `operator_expression` | Expression, stable EDG operation code, EDG-specific operation facts |
| `call_expression` | Callee expression, exact lowered routine if direct/resolved, execution direct/indirect call kind, receiver only when explicitly identified, function type, result/return-slot facts; optional original selected member and source-dispatch kind only when retained |
| `call_argument` | Call, ordinal, expression, selected parameter association where available; retain every executed operand, including expanded default/implicit arguments, without promising surviving default-argument provenance |
| `conversion_expression` | Operand, source/target type, conversion category, EDG-selected conversion function/base path |
| `reference_expression` | Referenced entity, reference kind and EDG access/adjustment context |
| `constant_expression` | Constant occurrence/value, original type and retained backing expression |
| `block_statement`, `block_item` | Scope/lifetime, ordered declaration/statement children |
| `if_statement`, `loop_statement` | Init/condition/test/body/increment roles, EDG flags, scope and lifetime boundaries |
| `range_for` | For retained unlowered metadata/compatibility forms only; the main profile uses EDG's ordinary loop and selected iterator operations |
| `switch_statement`, `switch_case` | Selector, ordered case/default nodes, selected constant/range values and target labels |
| `jump_statement` | Return/break/continue/goto category, value/label/target relations and EDG lifetime transition information |
| `label_statement` | Label symbol, target statement, source origin |
| `try_statement`, `handler` | Protected body, ordered catch handlers, caught type/parameter, handler lifetime, function-try distinctions |
| `throw_expression` | Operand, EDG exception initialization/type/cleanup and rethrow state |
| `new_expression`, `delete_expression` | For surviving unlowered forms: selected routines/type/count/initialization/failure cleanup; main-profile lowering's calls, arithmetic, cookie/runtime helpers and cleanup operations use their actual typed payloads |
| `lambda_expression`, `lambda_capture` | Retained closure/capture associations if available; lowered closure storage, constructor and call operator are ordinary executable/type entities |
| `coroutine`, `coroutine_operation` | EDG promise/suspend/await/yield/return protocol, selected functions, placeholders and allocation/deallocation facts |
| `builtin_expression`, typed builtin details | Stable builtin opcode, operands, target types and order/alignment/volatile/atomic facts actually supplied |
| `asm_statement`, `asm_operand`, `asm_clobber`, `asm_label` | Upstream-retained assembly text and separately normalized constraints/operands/clobbers/labels |
| Extension payload tables | GNU statement expressions/computed goto, Microsoft SEH/managed operations/properties, vector/UPC and other enabled families from the upstream inventory |
| `lowered_eh_operation` and typed details | Prologue/epilogue, cleanup-state, caught/thrown-object address, internal-try, and other active partial-EH kinds; region/handler/object references |
| Metadata expression tables | Surviving unevaluated/type traits/requires/reflection/dependent forms with EDG results or explicit dependent state |

This is a logical family catalog, not permission to implement only these example
operators. The exhaustive enum/union inventory in
[semantic-inventory.md](semantic-inventory.md) defines the coverage obligation.
Preserve the complete graph EDG produces for `new`, await and initialization;
do not flatten several operations into an invented generic call and lose their
protocol. Do not encode an extension's semantics as an uninterpreted text field.
Raw assembly and original source fragments are text by nature; they are not a
substitute for typed semantic operands.

No separately computed CFG is required initially. Preserve EDG's structured
statements, explicit jumps, normal-path cleanup calls and EH state/region graph.
A target consumer maps these to C# control flow and CLR unwinding without
replanning C++ lifetime rules. A generic `GetControlFlowGraph()` API must not
imply that a distinct CFG was exported by EDG.

## Initialization, lifetime, and cleanup

| Relation | Required content |
|---|---|
| `initialization` | EDG initialization kind, target/entity/type, expression or constant aggregate, selected constructor/destructor, temporary/materialization facts |
| `initialization_operand` | Ordered constructor/default/member/base/array-element component references with typed role |
| `aggregate_initializer`, `aggregate_element` | Target type, ordered selected members/elements/designators, repeats and dynamic-init references |
| `lifetime` | EDG lifetime identity, parent, parent destruction-sublist checkpoint, scope/owner, kind and explicit beginning/end associations |
| `lifetime_cleanup` | Lifetime, EDG destruction order and unordered marker, initialization/object, selected destructor, conditional-construction/array/extension facts |
| `lifetime_transition` | Statement/expression owning an EDG transition, source/destination lifetime endpoints and transition kind where represented |
| `temporary` | Identity, type, initialization, lifetime, lifetime extension association |
| `construction_dependency` | EDG-provided dependencies/guards for partially completed initialization and cleanup |
| `static_initialization` | Translation-unit order, variable, initialization, destruction registration and storage-duration facts exposed by EDG |
| `exception_specification` | Kind, effective EDG noexcept flags/result, separately retained argument expression and its dependency state, dynamic exception types in ordered rows |

These relations preserve EDG's reverse-destruction chains and conditional
obligations as records. A database row is not necessarily a statement that
executes unconditionally. In particular, a default-constructed temporary, failed
constructor, array prefix, return value, or lifetime-extended reference must not
be treated as a simple block-local destructor call.

These descriptive lifetime records are supplemental in the lowered profile.
Stock end-of-lowering cleanup frees `destructible_entity_descr` even when lifetime
records are retained (`src/lower_il.c:22882–22922`). Do not traverse it, patch its
retention, or rebuild it from C++ declarations. Executable cleanup uses EDG's
surviving normal-path calls and the partial-EH interface below.

### EDG-generated partial-EH interface

| Relation | Required content |
|---|---|
| `function_eh` | Function, prologue/epilogue operations, generated region/array table identities, initial/sentinel state conventions |
| `cleanup_region` | Owning function, EDG region index, next-region foreign key, action and condition; normalize supplemental rows without changing referenced indices |
| `cleanup_action` | EDG action kind, selected destructor/deallocation/guard operation, symbolic object location, array descriptor and ABI supplements where applicable |
| `cleanup_storage_location` | Direct variable, indirect variable, receiver-relative or static-address kind; referenced variable and exact offset; no fabricated physical stack address |
| `cleanup_array` | EDG table identity/index, storage locator, element size, live element-count source and applicable VLA facts |
| `cleanup_condition` | EDG guard/condition locator and supplied comparison/state semantics |
| `cleanup_abi_detail` | VTT, base-subobject, delegated-constructor and other target-dependent payloads, in typed fields/relations |
| `cleanup_state_operation` | Captured operation, referenced region/state and required temporary/operand relationships |
| `eh_handler_region` | EDG-provided protected-region/handler associations and boundaries |

Decode the stock table representation according to `lower_eh.c` and the EDG
runtime contract. This is lossless representation adaptation, not a new cleanup
solver. Flag meanings depend on ABI and entry kind; the same numeric bit may
denote different facts. Preserve real conditional/unordered/partial-array actions
and supplemental entries. The `ck_stack_offset` payload is still a variable plus
offset, not a measured host stack position (`src/lower_eh.c:3210–3306`).

Expose this naturally as `body.ExceptionHandling.CleanupRegions`, region actions
and next-region references, with object properties for routines and storage.
The consumer does not decode EDG table indexes or bit masks itself. Target
exception transport and actually performing an action remain translator/runtime
work; they need no new decision about which C++ destructor was selected.

Normalize generated tables once. Preserve their symbol identity and all executable
references, but do not persist both a positional constant-array dump and a second
typed copy of the same facts. A generated-table symbol can reference its typed
initializer representation. If an ordinary address/value query exposes that
table, its model view must project the same authoritative rows. Required ordinary
constants not fully represented by that projection stay in the constant tables.

## Templates and synthesized material

| Relation | Required content |
|---|---|
| `template_definition` | Template entity, owner/name, retained pattern declaration/type/body, source origin |
| `template_parameter` | Template, ordinal/depth, parameter kind, pack state, default argument and constraint metadata retained by EDG |
| `template_argument` | Argument entity and discriminant; one typed payload: type, value, template, expression, pack |
| `template_argument_pack_element` | Pack argument, ordinal, argument entity |
| `template_instance` | Concrete entity/type, originating template and selected specialization links if retained, instantiation state and source point |
| `template_instance_argument` | Instance, argument-list role (primary template versus selected partial specialization), ordinal, EDG-selected argument entity |
| `template_specialization` | Primary template, specialization entity, explicit/partial kind and retained selection correspondence |
| `constraint_metadata` | EDG-retained normalized/result relationships only; no new satisfaction solver |
| `synthesized_origin` | Synthesized entity, kind, causing declaration/expression/type where EDG supplies it |

Do not serialize front-end template substitution worklists, lookup candidates,
rescan caches, or every speculative instantiation. Preserve retained semantic
entities and origins; an uninstantiated template may legitimately have no
executable body. The backend cannot follow front-end-region pointers after
`fe_wrapup` frees that memory. Origin availability is therefore part of the
retention-profile contract, not a license to guess based on matching names.

## Exact constants and interning

Separate constant *occurrence* from constant *value*. An occurrence owns its
source position, original/current type, conversion facts, and optional backing
expression. Several occurrences can share a normalized value without sharing
address identity or source locations.

| Value family | Relational representation |
|---|---|
| Integer | Canonical signed magnitude/decimal scalar text atom; interpretation/width comes from the constant type |
| Floating point | Kind/class, sign (including negative zero), exact finite significand and exponent/radix; NaN payload/quiet state when retained; no SQLite REAL rounding |
| Fixed point | Exact scaled integer scalar and scale/type descriptor |
| Complex/imaginary | References to scalar real/imaginary component values |
| String | Interned exact value with character/element representation and explicit length; lossless text fast path or ordered scalar code-unit rows |
| Address | Referenced base entity, address kind, exact offset, relevant base/field derivation; no host pointer value |
| Pointer-to-member | Selected member, containing type, EDG base derivation/adjustment facts |
| Aggregate | Typed ordered element/designator/repeat relations; share values but preserve occurrence identity |
| Reflection and other kinds | Dedicated typed detail matching upstream discriminator; never a printer dump |

For strings, keep EDG's target bytes/code units, including embedded and trailing
NULs and the possibility of no terminator. `a_constant.variant.string.length`
and `.value` explicitly have these semantics (`src/il_def.h:5048–5068`). A `TEXT`
fast path is permitted only for a proven lossless known-encoding representation;
record element width/encoding so the original target units are reconstructible.
Otherwise use `(value_id, index, unsigned_code_unit)` rows; repeated identical
unit runs may use a typed `(start, count, unit_value)` relation if benchmarks
justify it. Never “solve” invalid Unicode using hidden base64. Source spelling,
decoded value, and a literal's storage object are three distinct things.

EDG integer and floating helpers are the conversion authority. Do not truncate
integers through host `long` or managed `long`. `fp_to_hex_constant_string`
supports finite C99 hexadecimal values when configured; its special-value helper
returns generic NaN (`src/float_pt.c:3010–3068,3644–3666`), so it alone does not
prove exact NaN payload preservation. M0/M1 must settle a lossless adapter for
the enabled EDG numeric representations, including exceptional values, before
calling the scalar contract complete. If an EDG value has information that an
adapter drops, that is an exporter defect, not upstream uncertainty.

Intern text and literal content using in-memory hashes followed by exact
comparison; hashes are never semantic identity or sole collision evidence.
Interning a literal value must not merge distinct writable objects, distinct
addressable string objects, or EDG-selected address identities.

## Source and macro provenance

| Relation | Required content |
|---|---|
| `source_file` | Path/name text IDs and EDG file identity; content digest only if genuinely captured/verified |
| `include_occurrence` | Parent occurrence, included file, inclusion position and EDG include facts |
| `source_mapping_segment` | EDG sequence interval, source/include occurrence, logical line mapping and physical/presumed filename distinction, including `#line` remap occurrences |
| `source_position` | EDG expansion sequence/column, optional original sequence/column, macro context; logical coordinates resolved through mapping segments |
| `source_range` | Start/end positions with their actual endpoint/coordinate conventions |
| `source_origin` | Spelling and expansion range references where provided; synthesized/absent reason |
| `macro_definition` | Definition identity, name, define/undef text and replacement-list range retained by EDG |
| `macro_invocation` | Invocation identity, macro definition, logical parent invocation, invocation range, retained original-location facts |
| `macro_argument_text` | Invocation and EDG's retained canonical-whitespace argument-list text atom |
| Optional source attachment | Externally supplied exact snapshot identity/content reference, or explicitly unverified reread; not silently equivalent to compiler input |

The argument-list string is preserved source text, not the authoritative
representation of an argument array. Do not parse it and claim an EDG token
mapping. Negative macro invocation parent records used as storage placeholders
are decoded using EDG's convention; only actual logical invocations become rows.

EDG positions are not automatically byte offsets or C# UTF-16 `TextSpan`s.
Token-level locations are exposed where EDG retains them, with their coverage
and coordinate system, not as an invented complete token stream. A macro can
contribute several origins; a node may lack an exact spelling range. See
[api-and-provenance.md](api-and-provenance.md) for the verified boundary.

## Inclusion, traversal, and memory policy

1. Discover file/namespace/class declarations and all retained routine bodies,
   static/thread initialization, instantiated and synthesized material, source
   sequences where a profile actually preserves them, and requested provenance.
   Include promoted and orphaned entities, synthetic routines/tables, local
   declarations and local classes through their owning memory regions and
   retained semantic-owner links. Post-promotion class lists alone are incomplete.
2. Assign identities before descending. Follow only the field-level semantic
   whitelist into types, constants, templates, layout, initializations, lifetimes,
   and selected references. Cycles are handled by identity maps, not recursion
   duplication. EDG's parser/symbol worklists are not a traversal root.
3. Serialize live IL at the supported backend boundary. Initially use in-memory
   IL; reserve memory-region-aware identity keys and a routine-at-a-time writer
   organization. If file-backed IL is added later, key identities by stable EDG
   region/entity information, never reusable pointer addresses across unloads.
4. Emit each logical entity and payload once. Keep per-routine transient queues
   bounded where possible; the format must not require creating an additional
   full managed/in-memory mirror before SQLite writes.
5. Preserve constants' backing expressions when retained; handle indirect local
   expression references with EDG's supported region-aware helpers. Do not
   accidentally omit lambdas or local-class methods because they use different
   region placement rules (`doc/source/il.rst:60–123`).
6. Publish only after traversal has resolved all required references and the
   transaction commits. A translation unit with frontend errors is not represented
   as a successful complete executable object in this first contract. Ordinary
   warnings do not disqualify a successfully compiled unit; their full text is
   not promised as retained backend-accessible metadata.

Minimality means one representation per meaningful fact, not dead-code removal.
There is no include-path blacklist or system-header body exemption. Optional
future packaging can separate source attachments; executable and type completeness
is not controlled by a size-saving toggle.

## SQLite writer, reader, and size measurements

Use prepared statements and one transaction per output, a temporary sibling file,
and atomic publication after close. Failure leaves no partially published new
object and preserves any existing destination. SQLite errors are exporter errors;
they do not question EDG semantics. Foreign keys are enabled by the writer;
deferral permits identity-first cyclic graph insertion. Use `STRICT` tables with
a documented minimum SQLite version. Ordinary integer-primary-key entity tables
use rowid storage; benchmark `WITHOUT ROWID` for composite-key ordered relations.

Create only indexes required by the initial API: entity lookup, owner/ordinal
enumeration, declaration-to-symbol, type member lookup, macro parent, and lifetime
membership. Add reverse-reference indexes only for implemented queries. No
duplicate cached display-name column on every row and no duplicated whole-source
text at every node. Temporary WAL/journal files are not part of the final artifact.

The C# reader uses read-only connections, parameterized queries, identity caches,
and batched child/relation loads. It reads required schema/version/capability
metadata rather than probing arbitrary columns at runtime. Supported minor
extensions are additive; incompatible meaning changes require a major version.
Old-object rewriting/migration and cross-unit linking are not phase-4 features.

Size is measured, not promised from normalization alone. Track database bytes,
table/index bytes, row counts, string/value interning ratio, frontend/export wall
time, peak RSS, managed opening cost, and cold/warm traversal cost. Compare
repeated literals/names, template-heavy headers, large aggregates, and macro-heavy
units. In particular, normalized non-text code units can be expensive: evaluate
the text fast path and exact-value sharing before adding compression structures.
No numeric size cap is invented before a baseline exists.

SQLite references supporting these design choices:
[storage classes and numeric precision](https://www.sqlite.org/datatype3.html),
[foreign-key enforcement](https://www.sqlite.org/foreignkeys.html), and
[composite-key storage tradeoffs](https://www.sqlite.org/withoutrowid.html).
These are storage decisions only; EDG remains the semantic authority.

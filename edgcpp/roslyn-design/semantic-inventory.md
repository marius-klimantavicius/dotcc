# `.roslynobj` semantic inventory — design only

Status: proposed contract, not an implementation or a demonstrated exporter. Implementation requires the user's approval. This document was derived independently from pristine EDG commit `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` checked out at `/tmp/roslynobj-edg-upstream-20261012`. All `src/...:line` citations below refer to that checkout and commit. The allowed preliminary copies of `il_def.h` and `c_gen_be.c` were confirmed byte-identical to the pristine files. No previous object backend, exporter, linker, artifacts, or campaign findings were used.

## 1. Authority and semantic boundary

The payload represents EDG's completed semantic decisions before optional C++ IL lowering. It is an executable semantic graph with ordered source-shaped statement/expression containment, explicit typed cross-references, and shared nodes/declarations/types/constants. Containment views need not give a shared node one unique structural parent. It is neither source text to recompile nor a parser/symbol-table dump. A consumer implements the already selected operations; it must not rerun lookup, overload resolution, template substitution, implicit-conversion selection, constructor selection, access checking, or class layout.

The object header identifies the semantic phase, inherited by its records without a duplicate phase column per row. Any genuinely mixed-stage retained data must have an explicit family/role rule or exceptional annotation; backing-expression roles already identify their documented unlowered form. Native EDG layout facts remain facts about the configured EDG target. They must not silently become managed-runtime offsets. Every optional fact distinguishes **known**, **not applicable**, **unavailable in this build/phase**, and **dependent/unresolved**; a numeric zero or null pointer is not a universal absence marker. Invalid/error/reclaimed entries cannot be accepted as executable nodes. Compile-time dependent entries can be retained as provenance but cannot masquerade as resolved executable code.

The C generator provides a coverage baseline, not a complete definition of this contract:

| Baseline source | What it establishes | Additional preserved semantics required |
|---|---|---|
| `src/c_gen_be.c:5760`, `dump_expr`; assertions at 5789–5808 | Expression/operator visitor and C-compatible assumptions | Full unlowered C++ expression union, value categories, call dispatch, temporaries, semantic sequencing |
| `src/c_gen_be.c:7093–7114`, `dump_expr` | `new_delete`, `gcnew`, conditions, typeid, reuse-value, sizeof-pack and braced-init are rejected; await/yield are merely printed | Their actual supplements, selected routines, nested lifetimes and protocol calls |
| `src/c_gen_be.c:9530`, `dump_dynamic_init` | Handles a restricted constant/aggregate/expression set | Constructors, copy/return materialization, zero/no-init with destruction, lambdas and bitwise-copy |
| `src/c_gen_be.c:9681`, `dump_statement` | Statement visitor, including labels, switch, VLA, extensions | Range-for/for-each supplements and unlowered cleanup semantics; declaration entities that C emission ignores |
| `src/c_gen_be.c:9980–10010` | Unlowered try printing is explicitly a debugging aid | Handler parameter initialization, destruction, try lifetime and ordered handlers |
| `src/c_gen_be.c:10089–10104` | Coroutine assumes generated body; co_return expression already calls promise operations | Coroutine descriptor, await protocol edges and lifecycle obligations |
| `src/c_gen_be.c:7902`, `dump_initializer_part`; 8489, `dump_initializer`; 8555, `dump_compound_literal` | Constants are also initialization programs | Repetition/designators, dynamic aggregate elements, constructor and cleanup dependencies |
| `src/c_gen_be.c:11704`, `generate_C_output_file` | Distinct declaration and definition passes, static/dynamic initialization | Namespace/class/function regions before C flattening; all retained executable definitions, not just C-emission-needed ones |

An especially consequential phase distinction: `eok_assign` for unlowered C++ class assignment excludes tail padding, whereas lowered/C structure assignment copies `sizeof(struct)` bytes. `eok_bassign` is a lowering-only block-copy operation. Do not normalize them into one undifferentiated copy (`src/il_def.h:14114–14151`).

## 2. Typed relation families

Names below describe normalized families, not finalized storage tables. Each relation has typed endpoint IDs, explicit cardinality, and ordinal where ordered. List plumbing (`next`, `prev`, first/last caches) becomes one ordered relation rather than multiple independent graph edges. No record contains host addresses. A null edge retains its EDG-defined meaning.

| Family | Required records and relations |
|---|---|
| Translation unit / configuration | TU identity; language/dialect and enabled extension/build features; EDG version/revision; semantic phase; native target scalar/layout/ABI description; source mappings; successful compilation/export status (no promised retained warning collection); root and definition completeness status |
| Entity / definition / scope | Distinct routine, variable, field, label, namespace, class/enum/template identities; declaration versus available definition; linkage/storage/thread duration; generated/defaulted/deleted/builtin status; lexical owner; function-region owner; declaration-to-definition relation; function body, parameters, implicit `this`, return object and local entities |
| Type | Type constructor and typed operands; nominal declaration identity separate from structural type use; qualification/typedef wrappers; function signature; complete class/enum definition; specialization identity; target size/alignment and validity; ABI attributes |
| Statement / expression / operation | Discriminant; typed result, value category and original glvalue type; ordered children and operands with semantic role; resolved entity references; labels; side-effect/evaluation flags; source/provenance |
| Constant | Exact semantic representation and type; scalar value/target string bytes; address base plus offset/path; member pointer target and cast path; aggregate/repeat/designator program; folded-value-to-backing-expression provenance |
| Call | Explicit call category, selected routine where statically known, dynamic callee expression otherwise, receiver, argument ordinals, virtual flag, member-pointer operand, builtin identity, calling-convention/signature, converted argument expressions and initialization dependencies |
| Initialization / materialization | Initialization kind; destination owner/role; chosen constructor; explicit or context-implied source; ordered arguments; zero/value/default initialization flags; aggregate program; return/copy materialization; temporary/master relation; field/base/delegating ctor initialization |
| Lifetime / cleanup / exception | Lifetime kind and owner; parent/child and parent destruction checkpoint; EDG destruction ordering with unordered flags; selected destructor; conditional completion; goto exit boundary; try/handler order; throw/rethrow; exception specification; new-failure cleanup |
| Class layout / dispatch | Ordered fields; base-subobject identity and derivation paths; direct/virtual/ambiguous/access flags; byte/bit offsets; nonvirtual size/alignment; virtual-info sharing/offset; virtual declaration numbers; overriding-to-primary relation; receiver and covariant-return adjustment base relations |
| Template provenance | Template declaration identity/kind and parameters; instance-to-origin; ordered type/non-type/template arguments including pack boundaries; primary versus partial-specialization argument maps; retained prototype/dependent status; parent template class for member instances |
| Closure / coroutine / extension | Captures and closure field/source links; callable specialization; coroutine placeholders, promise/allocator/return/await operations; typed asm operands/constraints/clobbers/labels; VLA dimensions; MS property/SEH and GNU/UPC/vector descriptors where enabled |

Most relationships are derivable by direct relocation of existing EDG pointers. Context-implied initialization destinations/sources are represented by explicit role references to their owning constructor-init, handler, return slot, aggregate element or new-operation. This normalization records the meaning documented by EDG; it does not choose another constructor or synthesize a C++ conversion.

## 3. Executable node and auxiliary-union worksheet

The exhaustive enum ledger in section 8 covers all source-conditional variants. This section covers their discriminated payload families. All active pointer arms must be followed according to their discriminator; inactive union storage is never serialized. Common fields with semantic effect are required; caches, diagnostics-only flags, source reconstruction conveniences and front-end work pointers are not automatically included.

### Statements

`a_statement` (`src/il_def.h:16516–16852`) requires kind, primary expression when applicable, execution-sequence ordinal, ownership, applicable attributes/pragma semantics, initialization-guard semantics and generated status. `parent` is ownership, not an execution edge. The union families are:

| Kinds | Payload / dependencies |
|---|---|
| `expr`, `empty`, `coroutine_return`, `assigned_goto`, UPC notify/wait/barrier/fence | Primary expression where applicable; assigned-goto label value; null statement attributes. Co_return retains EDG's selected promise call and generated transfer structure. |
| `if`, `if_consteval`, `if_not_consteval` | Then/else statements; distinguish runtime and constant-evaluation condition modes. |
| `constexpr_if` | `a_constexpr_if`: arms plus `value_known/value`; retain EDG's discarded/empty arm distinction. No consumer re-evaluation. |
| `while`, `end_test_while` | Test and loop body; preserve test position. |
| `for`, `upc_forall` | Body plus `a_for_loop`: initialization statement, increment, for-init scope, optional UPC affinity. |
| `range_based_for` | Body plus `a_range_based_for_loop`: EDG-generated range/begin/end/iterator variables, comparison/increment and initialization, associated scopes. Do not synthesize begin/end lookup. |
| `for_each` | Body, collection expression, `a_for_each_loop` and its discriminated implementation pattern. |
| `switch`, `switch_case` | Body, `a_switch_stmt_descr`, ordered case descriptors, case/range constants/default marker, case-to-switch binding. |
| `goto`, `label` | Exact `a_label` identity and lifetime boundary. `goto.lifetime` identifies the innermost lifetime containing both endpoints; inner lifetimes are exited. This also carries EDG's break/continue transfers. |
| `return` | Either ordinary value expression or `return_dynamic_init`, with return-materialization destination context; never assume both. |
| `coroutine` | `a_coroutine_descr`; semantic protocol data rather than an empty marker. |
| `block` | Ordered contained statements, `a_block`, associated scope and lifetime when present. |
| `init` | Dynamic-init record. |
| `asm`, `asm_func_body` | `an_asm_entry` or raw assembly body; all input/output operands, constraints, clobbers, goto labels and dialect/volatile properties. An assembly string is not independently portable executable semantics. |
| `try_block`, `microsoft_try` | C++ ordered handler/try supplement or Microsoft guarded/filter/cleanup supplement. |
| `decl` | Typed declared entities; structured-binding container and its binding relation; static/thread marker. This node must not be discarded because the C printer does so. |
| `set_vla_size`, `vla_decl` | Dimension record or variable/typedef according to `is_typedef_decl`; allocation/evaluation point. |

Supporting definitions: loops `src/il_def.h:15878–16243`; switch descriptors `16245–16314`; handler/try/MS try `16316–16403`; coroutine `16409–16500`; constructor-init `16861–16943`; declaration/structured-binding note `16800–16810`; scope and lifetime relations `18295–18560`, `17612–17747`.

### Expressions and operations

Common semantic fields include type, original glvalue type, lvalue/xvalue flags, discarded-result flag, volatile fetch, initialization guard and generated-default-argument status (`src/il_def.h:14906–15129`). These flags can encode conversions without separate cast nodes. Do not infer value category from type. `operation.type_kind` can differ from both result and operand type kinds (`15132–15149`). Preserve evaluation direction and member-pointer receiver exception (`15309–15325`), cast semantics, conversion-call, qualified-call and virtual-call flags (`15159–15307`). A source-form flag is optional only when it has no remaining semantic effect.

| Kinds (prefix `enk_`) | Active payload / disposition |
|---|---|
| operation | Operator kind, operation type kind, ordered operands, all effectful conversion/call/sequencing flags. |
| constant / variable / routine / field | Exact referenced entity; minimum applicable name-reference semantic supplement. A routine can be null for a vacuous destructor, which has separately documented meaning. |
| temp_init / lambda | Dynamic-init, temporary type-source metadata where needed, or lambda descriptor. Includes closure initialization/destruction, not only lambda body. |
| new_delete / gcnew | Complete corresponding supplement, selected allocation/deallocation, arguments/dimensions, object init, new-failure cleanup. |
| throw | Throw supplement or null for rethrow; thrown type, chosen object initialization/destructor and applicable accessible-base information. |
| condition | Condition scope, variable init, converted tested expression, optional C++17 init-statement. Loop condition lifetime reenters each iteration. |
| object_lifetime | Wrapped expression and lifetime. |
| typeid | Type operand, optional expression and EDG's dynamic-result flag; preserve potentially evaluated versus unevaluated role. |
| sizeof / alignof / datasizeof | Type-or-expression union, standard-alignof flag; runtime VLA dimension references when applicable. |
| sizeof_pack | Type/expression/template branch. Provenance/dependent only unless EDG has resolved it. |
| address_of_ellipsis | No union payload; retain feature identity. |
| statement | Enclosed GNU compound statement, including result dynamic-init. |
| reuse_value | Exact dynamic-init whose value is reused; do not duplicate evaluation. |
| lowered_eh_construct | Only for a separately declared lowered phase; all `leck_*` discriminants and relevant handler/init/try/routine/checkpoint expressions. Not expected in selected unlowered contract. |
| result_of_overriding_function | Lowering-only covariant-wrapper result; no payload; not expected unlowered. |
| vla_dealloc | Referenced VLA variable if present in configured IL; validate phase/configuration. |
| type_operand | Type or null for C11 generic default; definition-needed information. |
| builtin_operation | Builtin discriminant and ordered type/expression/attribute operands; preserve EDG result rather than re-evaluate traits. |
| param_ref | Parameter number and enclosing-list depth; zero names contextual `this`. |
| braced_init_list | Ordered expressions, provenance/prototype-only: EDG resolves nondependent initializers to actual initialization operations. |
| c11_generic | All retained operand metadata plus exact selected-result edge into the operand list; only selected expression is executable. |
| builtin_choose_expr | Three operands and EDG `choose_first`; only selected expression is executable. |
| await / yield | Original awaiter/yield-value expression plus separately named `await_ready`, `await_resume`, `await_suspend` expressions. Stored list order is ready/resume/suspend, not runtime execution order. |
| fold | Operands, operator token and associativity; dependent provenance. Concrete instances are ordinary operations. |
| initializer | Folded dynamic-init backing provenance. |
| concept_id / template_name | Template identity plus concept arguments where applicable; provenance/compile-time. |
| token_sequence | Interpolations and token-sequence value; reflection compile-time provenance, not an executable token stream. |
| requires / compound_req / nested_req | Requirements/parameters, expression-plus-constraint/noexcept, or nested constraint; compile-time provenance. |
| const_eval_deferred | Wrapped expression and reattempt state; unresolved compile-time status, never claim a runtime definition is fully resolved on this basis. |
| pack_index | Pack and index expressions; only provenance if still dependent. |
| error / reclaimed / last | Reject as serialized semantic nodes; `last` is a sentinel. `runtime_sizeof` is only an alias of sizeof. |

Payload source: `src/il_def.h:15334–15796`. All operations in section 8 retain their exact EDG discriminant; grouping integer/pointer/complex/vector operations by a printed token loses semantics. Direct/member/member-pointer calls are defined separately at `14247–14303`.

### Constants, types and initialization

`a_constant` has 20 source-conditional representation kinds plus sentinel, with active payloads for integer, fixed-point, string, floating/imaginary, complex, address, member pointer, label difference, lowered stack offset, dynamic initializer, aggregate, repeat, template parameter, designator, UPC thread multiple, void and reflection. Retain original type and conversion flags where semantic, not just a string rendering (`src/il_def.h:4741–5570`). Exact scalar encoding follows the main schema design: integer signed-magnitude decimal text without SQLite REAL conversion; floating class/sign/radix/significand/exponent/payload scalar columns derived through EDG adapters, with exceptional payload support explicitly audited. Decimal floating pretty-printing is not claimed lossless. String bytes are already in target-machine representation (`5057–5068`); encode known character encodings losslessly as text with explicit length/NUL handling, otherwise use ordered numeric code-unit rows. Do not serialize raw host structures, hex dumps, base64 or opaque blobs. Source occurrences remain separate from interned scalar/literal payloads; never merge address identity merely because contents match.

Address constants carry typed base-kind union, byte offset, one-past-end and subobject path; address-to-temporary has identity semantics distinct from address-to-constant. Member-pointer constants retain function-versus-field discriminator, nullable target (null member pointer), casting base and cast direction. Aggregate constants are initialization programs: ordered nested values and dynamic-init entries, repeated values/count, array/field designators and GNU range-tail behavior. A repeat count of zero can mean runtime recorded allocation/VLA count, not an empty initialization (`src/il_def.h:5133–5380`). Nested template-constant union and reflection payloads are provenance, never fabricated executable targets (`5381–5540`).

`a_dynamic_init` has nine distinct kinds plus a historical alias. Common fields include whole-variable destination where present, destructor/deallocation cleanup, object lifetime, initializer-expression lifetime, destruction-list ordering, static temporary, conditional/unordered initialization, temporary-lifetime extension, partial-construction cleanup, overlap/completion constraints, initializer-list backing array, master entry and return/copy flags. Active union families are no-payload (none/zero), constant/aggregate/lambda, expression/class-result, constructor plus args and implied-source/value-init/sequence/array-copy flags, and bitwise-copy source (`src/il_def.h:3957–4388`). `dik_none` can still require destruction. A constructor-init can select its field's initializer with a null own initializer; that is an intentional semantic edge, not missing data (`16899–16926`).

Types separate identity, definition and use. One nominal class/enum identity owns its EDG definition; cv/atomic qualifiers, pointers/references, arrays, function types, typedef/decltype wrappers and member pointers are separate type constructors referring to it. Concrete template specializations are distinct nominal instances with origins/arguments; a template declaration or dependent prototype is not a concrete runtime type. Preserve reference flavor, qualifiers, pointer modifiers/address spaces, enum underlying integer semantics, bit-precise integer widths, floating/fixed-point/vector formats, array bound/zero-bound/VLA state, function return/parameter types and order, variadic/prototype status, cv/ref-qualified receiver, calling convention, exception specification, parameter copy-construction and class return convention. See `src/il_def.h:5573–5643`, `6244–6287`, `6385–6605`, `7394–7665`, `9768–10998`.

`a_type.size == 0` does **not** imply incomplete: typedef wrappers, complete GNU empty structures and zero-length arrays are counterexamples. Use EDG's completeness and variant flags (`src/il_def.h:9787–9815`). `based_types` is a reuse cache that explicitly need not enumerate all derived types (`9778–9786`); exclude it from semantic reachability. All type variant arms, including configured scalable/RISC-V vectors and low-precision float kinds, need a disposition. Unsupported consumer execution is reported explicitly while the schema can still preserve the type.

Other flag-discriminated auxiliary unions must not disappear between these major families:

| Record / source | Required active arms and relations |
|---|---|
| `an_initializer`, `src/il_def.h:11028–11063` | Static constant, dynamic-init or structured-binding expression; none/zero/function-local have no direct payload. Function-local initialization must be joined through its scope-owned local-static-init record. |
| `a_local_static_variable_init`, `11066–11099` | Variable, static/dynamic discriminator and initializer, retry/exception lifetime; the source variable cannot point down into the function region. |
| `a_variable.variant`, `11210–11246` | Parameter-to-parameter-type, binding-to-container, or container-to-ordered-bindings according to flags. The initializer's `initk_binding` is an lvalue binding, not an assignment. |
| `a_vla_dimension`, `11101–11173` | Array type, dimension expression, original-dimension relation for copied types, evaluation statement/prototype status; copied types must not duplicate bound side effects. |
| `a_subobject_path`, `4606–4650` | Ordered field, base-subobject or element-offset arms; element offsets are not byte offsets. Preserve array-to-pointer-converted marker. |
| `an_exception_specification`, `7100–7205` | Resolved exception-type list or noexcept argument plus EDG throw-any/indeterminate flags; cached-token and copy-from-prototype cases are explicitly unresolved/provenance. A noexcept backing argument can remain dependent even for a nondependent routine type; do not mistake it for the effective throw-any result. |
| `a_local_expr_node_ref`, `14835–14890` | Resolve referrer-kind/tag plus expression to a direct typed reference while owning function region exists. Do not export the front-end lookup mechanism as the semantic edge. |
| `a_name_reference`, `2637–2744` | Keep the semantic builtin/property/destructor supplement where needed; discard cache linkage and lookup-only spelling structure unless requested as provenance. |
| `a_reflection_value`, `4708–4720` | Typed reflected entity and local owner identity; no untyped pointer. `a_token_sequence.token_cache` is explicitly front-end-only and excluded (`4694–4700`). |

Remaining associated attribute, calling-convention, numeric-format, vector, pointer-modifier, class/enum supplement and extension subfields are included by semantic effect under the relevant typed family; the implementation field matrix must audit their guarded members. This is not permission to copy their C structures wholesale.

## 4. C++ semantic obligations that must remain explicit

### Exact calls, layout, inheritance and templates

For each resolved call preserve EDG's callee expression, selected overload identity where known, converted arguments/default arguments, receiver, dispatch category, virtual flag and chosen calling convention. `is_virtual` on a routine is insufficient: a qualified call to a virtual routine may have `is_virtual_call == false` (`src/il_def.h:15270–15278`). Pointer-to-member calls preserve the pointer value and receiver separately; their second operand evaluates before the first despite left-to-right operand flag (`14275–14301`, `15311–15319`). Do not replace an indirect call with a guessed routine. Overload candidate histories and parser symbol tables are unnecessary after resolution; `tpck_unknown_function.symbol` is explicitly front-end-only and unavailable to backends (`5441–5449`). A dependent prototype's unknown function is not a resolved executable call.

Export all referenced class fields with declared order, owner/type, byte offset, bit remainder/width/signedness, validity, container offset where configured, packed/explicit alignment, anonymous-parent, mutable, no-unique-address and optimized-empty status. Include field initializers and initializer-defined closure entities when required. Layout comes from EDG, including coincident offsets; do not recompute packing or assume one field per address (`src/il_def.h:11896–12079`).

A base-subobject record is contextual to its derived class, not merely an edge between two class names. Preserve EDG's direct base number, direct/virtual/ambiguous/empty flags, offset validity, offset, derivation paths and access. Multiple nonvirtual subobjects of the same nominal base remain distinct; shared virtual base paths remain shared according to EDG's entries. Preserve class nonvirtual size/alignment, primary base and virtual-info-sharing/offset facts; virtual numbers are declaration-relative facts, not a promised complete ABI slot table. `an_overriding_virtual_function` links overriding routine, primary routine, overriding subobject and covariant-return adjustment base (`src/il_def.h:7848–8320`, `8737–8830`). No downstream override discovery or C++ base-path search.

Template instance provenance includes class `assoc_template`, primary argument list and partial-specialization argument list (`src/il_def.h:9100–9127`), function `assoc_template/template_arg_list` (`13166–13181`), variable template supplement (`11182–11207`), ordered `a_template_arg` arms and pack markers (`7672–7842`), and minimal template declarations (`17142–17418`). A member of a class-template instance can have no own function-template argument list; retain its owning concrete class. Do not force instantiation of unused templates to manufacture bodies. A generic lambda's `lambda_routine` may denote a prototype or be cleared when prototypes are not retained (`17951–17961`); concrete callable specializations must instead be reached through actual available routine definitions.

### Lifetime, initialization, exceptions and closures

Every executable region carries applicable lifetime and cleanup relations. Preserve destruction order and conditional completion exactly, including `unordered` destruction flags and overlapping inner temporary lifetimes. The destruction linked list is normally reverse construction order, but runtime ordering constraints cannot be replaced by sorting source locations (`src/il_def.h:4002–4047`, `4076–4132`, `17646–17747`). Keep constructor/destructor `constructor_inits` in EDG's execution order, including virtual bases, direct bases, fields and delegation (`16861–16943`, `18398–18409`). Return/copy initialization, NRVO candidate, omitted copy sources and master-temporary reuse must not become invented extra copies or destructor calls (`4245–4388`, `18422–18430`).

Exception records include thrown object type/init/destructor, rethrow, ordered handlers, catch parameter init/destructor, function-try distinction, try lifetime, exception specifications and Microsoft guarded/filter/cleanup forms. A `destructor` pointer may actually name the selected delete routine for failed-new cleanup, and a destructor can be present on a potentially evaluated but unevaluated temporary without an active lifetime destruction (`src/il_def.h:4002–4024`, `14608–14639`, `16316–16403`). Export execution-role distinctions; do not enqueue every referenced destructor as an unconditional cleanup.

New/delete retain scalar/array, placement/global/aligned flags, selected routine, object type, count/arguments, dynamic init/destruction and exceptional storage freeing (`src/il_def.h:14677–14781`). EDG intentionally omits new's first size argument from the argument list and tells the backend to provide bytes (`14732–14741`). Thus the semantic contract knows the operation and element/count protocol, but cannot claim a concrete native allocation-cookie formula was already exported. Dynamic object lifetime is user-controlled, so the absence of an automatic lifetime here is expected.

Closure records carry closure class, callable/prototype status, captures in EDG order, captured variable/init-capture, source closure field for nested captures, destination closure field, by-reference/this-versus-star-this behavior, pack state and initializer. A null closure field can mean the variable never needed storage; pending parser capture pointers are not portable data (`src/il_def.h:17937–18180`). Initialization/destruction is also in the lambda dynamic-init; the body alone is insufficient.

### Coroutines, atomics and extensions

Coroutines preserve traits, handle/promise/initial-resume placeholders, parameter and `this` copies, final-suspend label, initial/final suspend, unhandled-exception/get-return-object/allocation-failure operations, selected new/delete routines and EDG's generated-body status (`src/il_def.h:16409–16500`). Await/yield carries selected ready/resume/suspend calls (`15670–15681`). The descriptor establishes semantic protocol, not a complete target-specific frame layout, resume-state numbering or GC root map. The serializer must preserve EDG-generated body work as well as descriptors without executing the same operation twice.

There is no dedicated `eok_atomic` in this revision's operator enum. Preserve atomic qualification (`TQ_C11_ATOMIC`, `src/il_def.h:6248,6287`), concrete builtin callee/category/kind and converted argument list (including memory-order and failure-order arguments), and special name-reference forms for GNU sync/atomic operations (`2608–2625`, `2678–2689`, `12121–12181`, `12307–12309`). Generic GNU atomic functions already have an initial size argument inserted by EDG. Preserve local-static initialization guard requirements (`14970–14974`, `16550–16556`). Intrinsic expansion and supported memory-model implementation remain a backend capability obligation; do not treat these as arbitrary ordinary calls or infer operation identity solely from printed names.

GNU/C extensions include statement expressions/results, local labels/label addresses/differences/computed goto, two-operand conditional/reuse value, VLA evaluation/allocation/deallocation, vector and complex operations, compound literals, designated/range aggregate initializers, builtin choice/shuffle/convert/bit-cast, asm operands/goto/clobbers and ABI attributes. Microsoft forms include calling conventions, SEH, property/accessor rewrite semantics, selective overrides, pointer modifiers and configured C++/CLI handles/boxing/gcnew/for-each/properties/events. UPC statements/types/affinity are source-conditional. Each enabled form requires full typed export and reader support. A missing adapter produces a precise exporter diagnostic during implementation but remains a completion-blocking gap; no generic opaque node may silently stand for executable behavior. Raw asm and its typed operands can be fully represented while remaining native-only for a future C# translator.

## 5. Minimal reachability that includes all executable definitions

1. Enumerate every available, concrete executable definition belonging to the TU, regardless of whether main or a call graph currently reaches it: ordinary/static/inline/member/template-specialization/lambda routines, compiler-generated semantic bodies that EDG actually materialized, available constexpr bodies with runtime applicability, and relevant coroutine bodies. Record declaration-only, deleted/defaulted-but-unmaterialized, prototype/dependent and emission/linkage states distinctly. Do not invoke C generator `needed` filtering as a semantic-root policy.
2. Root file/namespace variable definitions, static/TLS initialization and destruction, local static initialization records attached to rooted definitions, and file-scope asm/initialization attributes that affect execution. Retain source/semantic ordering and priority information; no invented cross-TU order.
3. Traverse ordered statement/expression/init payloads; follow selected routine, variable, field, label, type, constant, lifetime, cleanup, capture, coroutine and extension dependencies. Include all local variables/declarations and block/condition scopes owned by a root, even where they have no expression reference. Traverse default member initializer when the selected constructor-init explicitly delegates to it. Initializer and cleanup dependencies may create additional body roots when the definition is available.
4. Traverse namespaces/classes and function-region descriptors to **discover definitions**, while exporting only required ownership stubs for otherwise unrelated lexical containers. Class-associated scope includes every member routine and static member; file-level scope traversal alone misses them (`src/il_def.h:9088–9099`). Function scopes contain body, ctor/dtor init list, both static and nonstatic variable lists and semantic auxiliaries (`18295–18560`). Region numbers/downward references must be resolved through EDG's documented lifecycle rather than treated as absent pointers (`18299–18318`; routine `function_def_number`/region fields at `12245–12263`).
5. Close type/layout dependencies: nominal owners, fields/base subobjects required for the type definition, signatures, enum constants/underlying type, qualifiers/wrappers, VLA dimensions, member-pointer classes, exception and RTTI types, resolved template arguments and minimal origins. An incomplete external class remains incomplete until EDG supplies a definition; do not fill it from name guesses.
6. Split **execution edges** from **semantic metadata/provenance edges**. Unselected `_Generic`/choose arms, `sizeof` operands, constant backing expressions, requires/concept/template prototypes and other unevaluated metadata do not become runtime execution. Preserve backing expressions on demand for semantic provenance, and mark that they are unlowered even when surrounding IL is lowered (`src/il_def.h:4774–4785`). Required cross-region local expression references must be resolved while their region is available.
7. Stop at external declarations, builtin identities and template-origin summaries unless they have a required available definition. Exclude symbol tables, overload candidate graphs, token caches, rescan work state, type reuse caches, arbitrary `next` chains of unrelated declarations and front-end allocation/work lists. Resolve tagged semantic roles to typed references. A source token sequence required as a reflection value is an explicit exception, not permission to dump parser history.

The export manifest must report root counts and concrete definitions unavailable because EDG did not retain/materialize them. “All executable definitions” means all available concrete semantic definitions under an agreed retention configuration; it does not authorize speculative instantiation. Establishing retention configuration and lifecycle hooks is a separate design dependency. A live-pointer closure alone cannot establish completeness if a function region was already released.

## 6. Facts deliberately not claimed at this phase

| Fact | EDG evidence / disposition |
|---|---|
| Concrete native virtual table variables/groups, VTT/construction-table variables and certain vbase/vcall table indexes | Lowering-dependent fields `src/il_def.h:8241–8318`, `8792–8809`, `9145–9163`. Preserve prelower semantic overrides/layout; do not invent table rows or indices. |
| Native adjusting thunks/covariant wrapper entry points and numeric delta/vcall/return-delta/vbase wrapper details | Lowering-only wrapper fields `src/il_def.h:13195–13237`; prelower overriding/return-base relation exists independently. |
| Alternate native ctor/dtor entry points | Explicitly generated by lowering (`src/il_def.h:12187–12218`, `13231–13237`). Not interchangeable with source/semantic constructor identity. |
| Portable/native EH region tables, cleanup state instructions and exception-runtime storage objects | `enk_lowered_eh_construct`/`ck_stack_offset` and EH prologue table references are configuration/lowering forms (`13772–13818`, `14818–14833`, `15525–15582`). Unlowered lifetime/throw/handler protocol is the contract. |
| Lowered base-class fields and lowered C-compatible copy/memory representation | `src/il_def.h:12000–12010`, `14114–14151`. Preserve unlowered nominal/base-subobject records and class-copy semantics. |
| Synthesized allocation cookie/byte formula; fully lowered coroutine frame; physical stack/register locations; machine calling sequence | Not established by inspected unlowered records. Type/count, coroutine protocol and calling-convention semantics are available; native emission mechanics require a separately proven EDG/backend path. |
| Complete overload history or executable body for every unused template | Front-end-only symbol/cache data is not a backend contract; prototypes and actual instantiations differ. Export selected entities and accurate availability. |
| Lossless EDG scalar extraction, including exceptional floating payloads, and complete native builtin semantics | Schema representation is proposed, but adapter fidelity and builtin runtime behavior must be established using EDG numeric/builtin authorities during approved implementation. This inventory does not claim an already verified encoding or runtime. |

No full C++ lowering is proposed merely to populate these fields. If native ABI artifacts become a requirement, that changes the export boundary and requires a separate preservation strategy so lowering does not destroy the required C++ semantic forms.

## 7. Coverage and acceptance requirements before an implementation can be called complete

The source-wide enum ledger below is exhaustive for its named enums, including `#if` variants. It is **not** a claim that every bit field in `il_def.h` has a finished wire-field mapping. During approved implementation, complete a field/union matrix for each retained record: source member, build guard, discriminator, semantic purpose, wire destination or reason for omission, null meaning, phase availability, reference target and cardinality/order. Every active union arm and semantic flag requires an explicit disposition. Compiler warnings for enum coverage are useful but insufficient because macro configurations and nested flag-discriminated unions also matter.

Use valid source-feature fixtures and exact semantic assertions: known overload/receiver/dispatch, shared versus repeated base subobjects and offsets, ordered/conditional destructors, exception handler and cleanup dependencies, aggregate designators/repeats, temporary reuse, captures, coroutine protocol, volatile/atomic call identities and enabled dialect forms. Verify fresh exports contain every enumerated concrete definition and no dangling typed ID. Round-trip equality and consumer execution are separate checks. No tests based on corrupting artifacts are proposed. This document adds no tests, implementation or EDG changes.

## 8. Exhaustive source enum ledger

Counts include all source `#if` branches, aliases and sentinels. The active compiler build will contain a subset; the producer records its actual configuration. Stable schema tags must not use EDG's configuration-dependent raw enum ordinal as their sole meaning.

Disposition key: **E** = preserve as an executable/resolved semantic kind when valid; **P** = retain only as needed for dependent/compile-time/backing provenance, with no implied execution; **L** = lowering-stage form, not expected in the selected unlowered profile; **X** = error/reclaimed/sentinel, never a valid exported semantic node; **A** = alias, normalize to its canonical kind. `E` does not claim every consumer supports execution of every extension. Conditional or dependent payloads still require contextual validation.

### `a_statement_kind` — 35 declared names
Source: `src/il_def.h:15802–15859`.
| Disposition | Enumerators |
|---|---|
| E | `stmk_expr`, `stmk_if`, `stmk_constexpr_if`, `stmk_if_consteval`, `stmk_if_not_consteval`, `stmk_while`, `stmk_goto`, `stmk_label`, `stmk_return`, `stmk_coroutine`, `stmk_coroutine_return`, `stmk_block` |
| E | `stmk_end_test_while`, `stmk_for`, `stmk_range_based_for`, `stmk_for_each`, `stmk_switch_case`, `stmk_switch`, `stmk_init`, `stmk_asm`, `stmk_asm_func_body`, `stmk_try_block`, `stmk_microsoft_try`, `stmk_decl` |
| E | `stmk_set_vla_size`, `stmk_vla_decl`, `stmk_upc_notify`, `stmk_upc_wait`, `stmk_upc_barrier`, `stmk_upc_fence`, `stmk_upc_forall`, `stmk_assigned_goto`, `stmk_empty`, `stmk_stmt_expr_result` |
| X | `stmk_last` |


### `an_expr_node_kind` — 45 declared names
Source: `src/il_def.h:13570–13701`.
| Disposition | Enumerators |
|---|---|
| X | `enk_error`, `enk_reclaimed`, `enk_last` |
| E | `enk_operation`, `enk_constant`, `enk_variable`, `enk_field`, `enk_temp_init`, `enk_lambda`, `enk_new_delete`, `enk_gcnew`, `enk_throw`, `enk_condition`, `enk_object_lifetime`, `enk_typeid` |
| E | `enk_sizeof`, `enk_alignof`, `enk_datasizeof`, `enk_address_of_ellipsis`, `enk_statement`, `enk_reuse_value`, `enk_routine`, `enk_vla_dealloc`, `enk_type_operand`, `enk_builtin_operation`, `enk_param_ref`, `enk_c11_generic` |
| E | `enk_builtin_choose_expr`, `enk_yield`, `enk_await` |
| A | `enk_runtime_sizeof` → `enk_sizeof` |
| P | `enk_sizeof_pack`, `enk_braced_init_list`, `enk_fold`, `enk_initializer`, `enk_concept_id`, `enk_requires`, `enk_compound_req`, `enk_nested_req`, `enk_const_eval_deferred`, `enk_template_name`, `enk_token_sequence`, `enk_pack_index` |
| L | `enk_lowered_eh_construct`, `enk_result_of_overriding_function` |


### `an_expr_operator_kind` — 123 declared names
Source: `src/il_def.h:13855–14345`.
| Disposition | Enumerators |
|---|---|
| E | `eok_address_of`, `eok_reference_to`, `eok_handle_to`, `eok_indirect`, `eok_ref_indirect`, `eok_cast`, `eok_lvalue_cast`, `eok_ref_cast`, `eok_lvalue_adjust`, `eok_class_rvalue_adjust`, `eok_box`, `eok_handle_to_box` |
| E | `eok_unbox`, `eok_unbox_lvalue`, `eok_base_class_cast`, `eok_derived_class_cast`, `eok_pm_base_class_cast`, `eok_pm_derived_class_cast`, `eok_dynamic_cast`, `eok_ref_dynamic_cast`, `eok_bool_cast`, `eok_array_to_pointer`, `eok_dot_vacuous_destructor_call`, `eok_points_to_vacuous_destructor_call` |
| E | `eok_assume`, `eok_noexcept`, `eok_parens`, `eok_negate`, `eok_unary_plus`, `eok_complement`, `eok_not`, `eok_vector_not`, `eok_vector_fill`, `eok_xconj`, `eok_real_part`, `eok_imag_part` |
| E | `eok_post_incr`, `eok_post_decr`, `eok_pre_incr`, `eok_pre_decr`, `eok_add`, `eok_subtract`, `eok_multiply`, `eok_divide`, `eok_remainder`, `eok_jmultiply`, `eok_jdivide`, `eok_fjadd` |
| E | `eok_jfadd`, `eok_fjsubtract`, `eok_jfsubtract`, `eok_padd`, `eok_psubtract`, `eok_pdiff`, `eok_shiftl`, `eok_shiftr`, `eok_and`, `eok_or`, `eok_xor`, `eok_eq` |
| E | `eok_ne`, `eok_gt`, `eok_lt`, `eok_ge`, `eok_le`, `eok_spaceship`, `eok_vector_eq`, `eok_vector_ne`, `eok_vector_gt`, `eok_vector_lt`, `eok_vector_ge`, `eok_vector_le` |
| E | `eok_gnu_min`, `eok_gnu_max`, `eok_assign`, `eok_add_assign`, `eok_subtract_assign`, `eok_multiply_assign`, `eok_divide_assign`, `eok_remainder_assign`, `eok_shiftl_assign`, `eok_shiftr_assign`, `eok_and_assign`, `eok_or_assign` |
| E | `eok_xor_assign`, `eok_padd_assign`, `eok_psubtract_assign`, `eok_land`, `eok_lor`, `eok_vector_land`, `eok_vector_lor`, `eok_comma`, `eok_subscript`, `eok_vector_subscript`, `eok_dot_field`, `eok_points_to_field` |
| E | `eok_pm_field`, `eok_pm_points_to_field`, `eok_dot_pm_func_ptr`, `eok_points_to_pm_func_ptr`, `eok_dot_static`, `eok_points_to_static`, `eok_virtual_function_ptr`, `eok_question`, `eok_vector_question`, `eok_call`, `eok_dot_member_call`, `eok_points_to_member_call` |
| E | `eok_dot_pm_call`, `eok_points_to_pm_call`, `eok_cli_subscript`, `eok_va_start`, `eok_va_arg`, `eok_va_end`, `eok_va_copy`, `eok_va_start_single_operand` |
| L | `eok_bassign` |
| P | `eok_lvalue`, `eok_await`, `eok_yield`, `eok_splice` |
| X | `eok_error`, `eok_last` |


### `a_builtin_operation_kind` — 117 declared names
Source: `src/il_def.h:14367–14585`.
| Disposition | Enumerators |
|---|---|
| E | `bok_offsetof`, `bok_has_assign`, `bok_has_copy`, `bok_has_nothrow_assign`, `bok_has_nothrow_constructor`, `bok_has_nothrow_copy`, `bok_has_trivial_assign`, `bok_has_trivial_constructor`, `bok_has_trivial_copy`, `bok_has_trivial_destructor`, `bok_has_user_destructor`, `bok_has_virtual_destructor` |
| E | `bok_is_abstract`, `bok_is_base_of`, `bok_is_class`, `bok_is_convertible_to`, `bok_is_empty`, `bok_is_enum`, `bok_is_pod`, `bok_is_polymorphic`, `bok_is_union`, `bok_types_compatible`, `bok_intaddr`, `bok_is_trivial` |
| E | `bok_is_standard_layout`, `bok_is_trivially_copyable`, `bok_is_literal_type`, `bok_has_trivial_move_constructor`, `bok_has_trivial_move_assign`, `bok_has_nothrow_move_assign`, `bok_is_constructible`, `bok_is_nothrow_constructible`, `bok_has_finalizer`, `bok_is_delegate`, `bok_is_interface_class`, `bok_is_ref_array` |
| E | `bok_is_ref_class`, `bok_is_sealed`, `bok_is_simple_value_class`, `bok_is_value_class`, `bok_is_final`, `bok_is_trivially_constructible`, `bok_is_destructible`, `bok_is_nothrow_destructible`, `bok_is_trivially_destructible`, `bok_is_nothrow_assignable`, `bok_is_trivially_assignable`, `bok_builtin_shuffle` |
| E | `bok_builtin_complex`, `bok_is_valid_winrt_type`, `bok_is_win_class`, `bok_is_win_interface`, `bok_builtin_shufflevector`, `bok_builtin_convertvector`, `bok_is_assignable`, `bok_is_trivially_copy_assignable`, `bok_is_assignable_no_precondition_check`, `bok_builtin_addressof`, `bok_has_unique_object_representations`, `bok_is_aggregate` |
| E | `bok_reference_binds_to_temporary`, `bok_is_same`, `bok_is_same_as`, `bok_is_function`, `bok_builtin_has_attribute`, `bok_builtin_bit_cast`, `bok_is_layout_compatible`, `bok_is_pointer_interconvertible_base_of`, `bok_is_pointer_interconvertible_with_class`, `bok_builtin_is_pointer_interconvertible_with_class`, `bok_is_corresponding_member`, `bok_builtin_is_corresponding_member` |
| E | `bok_edg_is_deducible`, `bok_is_array`, `bok_array_rank`, `bok_array_extent`, `bok_is_arithmetic`, `bok_is_complete_type`, `bok_is_compound`, `bok_is_const`, `bok_is_floating_point`, `bok_is_fundamental`, `bok_is_integral`, `bok_is_lvalue_reference` |
| E | `bok_is_member_function_pointer`, `bok_is_member_object_pointer`, `bok_is_member_pointer`, `bok_is_object`, `bok_is_pointer`, `bok_is_reference`, `bok_is_rvalue_reference`, `bok_is_scalar`, `bok_is_signed`, `bok_is_unsigned`, `bok_is_void`, `bok_is_volatile` |
| E | `bok_is_bounded_array`, `bok_is_unbounded_array`, `bok_is_referenceable`, `bok_is_nothrow_convertible`, `bok_reference_constructs_from_temporary`, `bok_reference_converts_from_temporary`, `bok_is_convertible`, `bok_is_trivially_equality_comparable`, `bok_is_scoped_enum`, `bok_is_trivially_relocatable`, `bok_is_invocable`, `bok_is_nothrow_invocable` |
| E | `bok_is_bitwise_cloneable`, `bok_builtin_is_virtual_base_of`, `bok_builtin_is_implicit_lifetime`, `bok_builtin_lt_synthesizes_from_spaceship`, `bok_builtin_gt_synthesizes_from_spaceship`, `bok_builtin_le_synthesizes_from_spaceship`, `bok_builtin_ge_synthesizes_from_spaceship`, `bok_builtin_is_structural` |
| X | `bok_last` |


### `a_constant_repr_kind` — 21 declared names
Source: `src/il_def.h:3618–3673`.
| Disposition | Enumerators |
|---|---|
| X | `ck_error`, `ck_last` |
| E | `ck_integer`, `ck_fixed_point`, `ck_string`, `ck_float`, `ck_complex`, `ck_imaginary`, `ck_address`, `ck_ptr_to_member`, `ck_label_difference`, `ck_dynamic_init`, `ck_aggregate`, `ck_init_repeat` |
| E | `ck_designator`, `ck_upc_threads`, `ck_upc_mythread`, `ck_void` |
| L | `ck_stack_offset` |
| P | `ck_template_param`, `ck_reflection` |


### `an_address_base_kind` — 11 declared names
Source: `src/il_def.h:3676–3702`.
| Disposition | Enumerators |
|---|---|
| E | `abk_routine`, `abk_variable`, `abk_constant`, `abk_temporary`, `abk_uuidof`, `abk_typeid`, `abk_cli_typeid`, `abk_cli_array`, `abk_label` |
| P | `abk_param_ref` |
| X | `abk_last` |


### `a_type_kind` — 27 declared names
Source: `src/il_def.h:5573–5643`.
| Disposition | Enumerators |
|---|---|
| X | `tk_error`, `tk_unknown` |
| E | `tk_void`, `tk_integer`, `tk_fixed_point`, `tk_float`, `tk_imaginary`, `tk_complex`, `tk_pointer`, `tk_routine`, `tk_array`, `tk_class`, `tk_struct`, `tk_union` |
| E | `tk_typeref`, `tk_ptr_to_member`, `tk_vector`, `tk_scalable_vector`, `tk_scalable_vector_count`, `tk_riscv_vector`, `tk_mfp8`, `tk_float8e4m3`, `tk_float8e5m2`, `tk_nullptr` |
| A | `tk_enum` → `tk_integer` |
| P | `tk_template_param`, `tk_reflection` |


### `a_dynamic_init_kind` — 10 declared names
Source: `src/il_def.h:3957–3989`.
| Disposition | Enumerators |
|---|---|
| E | `dik_none`, `dik_zero`, `dik_constant`, `dik_expression`, `dik_class_result_via_ctor`, `dik_constructor`, `dik_nonconstant_aggregate`, `dik_bitwise_copy`, `dik_lambda` |
| A | `dik_call_returning_class_via_cctor` → `dik_class_result_via_ctor` |


### `a_constructor_init_kind` — 4 declared names
Source: `src/il_def.h:16861–16874`.
| Disposition | Enumerators |
|---|---|
| E | `cik_virtual_base_class`, `cik_direct_base_class`, `cik_field`, `cik_delegation` |


### `a_templ_arg_kind` — 4 declared names
Source: `src/il_def.h:7672–7680`.
| Disposition | Enumerators |
|---|---|
| E | `tak_type`, `tak_nontype`, `tak_template`, `tak_start_of_pack_expansion` |


### `a_template_kind` — 10 declared names
Source: `src/il_def.h:17258–17276`.
| Disposition | Enumerators |
|---|---|
| E | `templk_none`, `templk_class`, `templk_function`, `templk_variable`, `templk_member_function`, `templk_static_data_member`, `templk_member_class`, `templk_member_enum`, `templk_template_template_param`, `templk_concept` |


### `an_object_lifetime_kind` — 6 declared names
Source: `src/il_def.h:17612–17638`.
| Disposition | Enumerators |
|---|---|
| E | `olk_global_static`, `olk_block`, `olk_block_after_label`, `olk_function_static`, `olk_expr_temporary`, `olk_try_block` |


### `a_lowered_eh_construct_kind` — 12 declared names
Source: `src/il_def.h:13772–13808`.
| Disposition | Enumerators |
|---|---|
| L | `leck_caught_object_address`, `leck_thrown_object_address`, `leck_cleanup_state`, `leck_unreachable_cleanup_state`, `leck_function_prologue`, `leck_function_epilogue`, `leck_catch_epilogue`, `leck_try_epilogue`, `leck_exception_caught`, `leck_exception_started`, `leck_initialization_completed`, `leck_internal_try` |


### `a_template_param_constant_kind` — 16 declared names
Source: `src/il_def.h:4397–4471`.
| Disposition | Enumerators |
|---|---|
| P | `tpck_param`, `tpck_expression`, `tpck_member`, `tpck_unknown_function`, `tpck_address`, `tpck_sizeof`, `tpck_datasizeof`, `tpck_alignof`, `tpck_uuidof`, `tpck_typeid`, `tpck_noexcept`, `tpck_template_ref` |
| P | `tpck_integer_pack`, `tpck_destructor`, `tpck_dependent_constant`, `tpck_concat_string_literals` |


### `a_template_param_type_kind` — 4 declared names
Source: `src/il_def.h:9286–9321`.
| Disposition | Enumerators |
|---|---|
| P | `tptk_param`, `tptk_member`, `tptk_unknown`, `tptk_bit_precise_int` |


### `a_for_each_pattern_kind` — 5 declared names
Source: `src/il_def.h:16029–16038`.
| Disposition | Enumerators |
|---|---|
| E | `sfepk_none`, `sfepk_stl_pattern`, `sfepk_cli_pattern`, `sfepk_cli_array_pattern`, `sfepk_array_pattern` |


### `a_template_parameter_kind` — 4 declared names
Source: `src/il_def.h:17142–17147`.
| Disposition | Enumerators |
|---|---|
| X | `tpk_error` |
| P | `tpk_type`, `tpk_nontype`, `tpk_template` |


### `an_init_kind` — 6 declared names
Source: `src/il_def.h:11009–11025`.
| Disposition | Enumerators |
|---|---|
| E | `initk_none` |
| E | `initk_static` |
| E | `initk_dynamic` |
| E | `initk_zero` |
| E | `initk_function_local` |
| E | `initk_binding` |

### `a_local_expr_node_ref_kind` — 9 declared names
Source: `src/il_def.h:14835–14858`.
| Disposition | Enumerators |
|---|---|
| X | `lerk_none` |
| P | `lerk_generic_sizeof` |
| P | `lerk_tpl_param_expr` |
| P | `lerk_array_bound` |
| P | `lerk_dep_array_bound` |
| P | `lerk_decltype` |
| P | `lerk_bit_field_width` |
| P | `lerk_constant_expr` |
| P | `lerk_scoped_expr` |

These are retrieval roles, not exported expression kinds. P means resolve the corresponding metadata/backing expression edge when needed. An attribute expression with executable relevance remains subject to its own semantic role.

# `.roslynobj` semantic inventory — revised lowered profile

Status: design only; implementation requires the user's approval. This revision follows the user's preference for EDG to do general C++ lowering, leaving only deliberately selected residual operations that a future C# backend can implement without redoing deep C++ semantic decisions. Evidence comes from pristine EDG commit `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`, checkout `/tmp/roslynobj-edg-upstream-20261012`. All `src/...:line` citations refer to that revision. No previous object exporter/linker implementation or its findings were used. No core EDG changes or prelower snapshot hook are proposed.

## 1. Chosen boundary and authority

The primary object contains **EDG-lowered executable IL plus original semantic metadata that stock EDG actually retains**. Configuration starts with `DO_IL_LOWERING=1`, `DO_FULL_PORTABLE_EH_LOWERING=0`, `GENERATE_EH_TABLES=1`, and object-lifetime retention flags. The integration document owns the complete, internally consistent macro profile and its feasibility gate. Lowering-only nodes, generated routines, storage fields, dispatch tables and partial-EH records are required primary content when generated. Unlowered source forms are not a second executable truth.

EDG remains the authority for lookup, overloads, templates, implicit conversions, layout, constructor/destructor selection, dispatch adjustments and cleanup construction. The exporter translates representation: it relocates pointers into typed relationships, preserves ordered operands, and decodes documented generated tables into typed rows. It does not recover missing source C++ semantics from lowered pointer arithmetic, names, call sequences or text. A future consumer implements the exported operations and runtime protocol, rather than choosing C++ behavior again.

The object header records the phase/profile once. Retained backing/default-member/binding expressions have explicit metadata roles identifying their documented unlowered form. Shared nodes can have several structural parents. Original symbols/types, lowered execution and lowered storage layouts are related where EDG provides correspondence; they are not conflated. An original-looking field that EDG changed in place is not a pristine snapshot.

The public API exposes typed object properties and hides database IDs. Optional unavailable collections are empty; optional singular relationships are nullable. A separate profile/diagnostic surface can explain retention limits. No `Fact<T>` wrapper is required for ordinary navigation. Internally the schema still distinguishes false, absent, inapplicable and dependent data where necessary; an unimplemented required executable kind is an export failure, not an empty optional collection.

A separately declared source-only/compatibility profile may expose unlowered structures for metadata inspection. It is not the normal translation-ready object and does not imply that a C# consumer must implement arbitrary C++ rules. Residual executable forms in a supported primary compatibility profile need an explicit, bounded implementation recipe using EDG-selected facts; complicated residual semantics trigger further stock lowering/configuration work or a stated unsupported profile.

## 2. Coverage basis and stage map

The C generator now supplies the primary executable coverage baseline. Its switches are still not a complete inventory: preserved metadata, partial EH, extension profiles and generated runtime artifacts must also be traversed.

| Source / handler | Required coverage |
|---|---|
| `src/c_gen_be.c:5760`, `dump_expr`; assertions 5789–5808 | Lowered expression/operator variants; value category, conversion and sequencing flags that remain; typed entity and constant references |
| `src/c_gen_be.c:7093–7114` | Recognizes remaining await/yield but rejects new/delete, condition, typeid and other expected-to-be-lowered forms. Those source forms belong to retained metadata/source-only inventory unless explicitly admitted by a compatibility profile. |
| `src/c_gen_be.c:9530`, `dump_dynamic_init`; 7902, `dump_initializer_part` | Lowered initialization plus aggregate/repeat/designator execution data; do not stop at scalar constants |
| `src/c_gen_be.c:9681`, `dump_statement` | All enabled statement forms, including partial EH and extension statements; C printing ignores declaration nodes, but metadata export does not |
| `src/c_gen_be.c:9980–10010` | Remaining try printing is a debugging aid, so use actual partial-EH handler semantics instead of assuming this printer proves executable EH coverage |
| `src/c_gen_be.c:10089–10104` | Coroutine expects a generated body; co_return retains selected promise operations and generated final-suspend transfers |
| `src/c_gen_be.c:11704`, `generate_C_output_file` | Separate declarations/definitions and generated initialization routines; do not inherit its needed-entity filtering as export root policy |
| `src/lower_eh.c:3210–3306,3367–3462,3630–3943,5094–5107` | Symbolic cleanup handles, array and region tables, supplemental action data, and prologue roots |

The full enum ledger in section 8 covers every source-conditional enumerator. It is broader than the primary executable set. Classification depends on both node role and profile:

| Representation | Primary role | Original semantic form |
|---|---|---|
| Calls, implicit conversions, member dispatch, base adjustments | Export EDG's lowered callee, explicit arguments/receiver adjustments, generated thunks and table loads | Original member-call/virtual-call flags only where genuinely retained; never infer source dispatch from lowered code |
| Construction, destruction, new/delete, aggregate copy | Export generated calls, loops, assignments, allocation sizes and runtime-helper identities | Retained init/lifetime descriptors are metadata; cleared constructor-init lists are not reconstructed |
| Class layout and member pointers | Export current storage types/fields, base graph, subobject partner, member-pointer encoding and actual generated tables | Retain available backend-supported nominal/template/declared-type relations; internal `typeref.orig_type` and routine `unlowered_type` are excluded |
| Exceptions | Partial-EH expressions, try/handlers, symbolic cleanup regions/actions, array descriptors and runtime protocol | Retained lifetime/destruction graph supports navigation; never require freed lowering work descriptors |
| Source/backing/default member expressions | Metadata roles with no duplicate runtime execution | May remain unlowered by design; include active union payloads when reachable |
| Coroutine operations, VLA, atomics, vectors and extensions | Export all surviving configured forms; retain EDG-selected protocols and arguments | Residual operations require bounded recipes and feature/profile evidence, not silently delegated C++ semantic work |

`eok_assign` has stage-dependent class-copy semantics: unlowered C++ excludes tail padding while lowered/C struct copy uses `sizeof(struct)`. `eok_bassign` is an explicit lowered block copy whose source size controls the copy (`src/il_def.h:14114–14151`). Preserve the actual lowered discriminants/storage types; do not import the old unlowered rule into lowered assignment.

## 3. Normalized relation families and active-union worksheet

Typed endpoint IDs are internal storage. Lists become ordered relations, with semantic roles distinct from execution order. Inactive union memory, parser caches, host addresses and redundant first/last list pointers are excluded. Native EDG sizes/offsets retain their target units; they are not .NET field offsets.

| Family | Required primary data and applicable retained metadata |
|---|---|
| TU/profile | Revision/version, exact macro/language/target profile, phase, successful export/root completeness, sources and origin coverage; no promised retained EDG warning collection |
| Entities/scopes | Routine/variable/field/label/namespace/class/enum/template identities, available definitions, lexical/semantic ownership where retained, linkage/storage/TLS, generated/builtin/entry-point roles, body and parameter relations |
| Types/storage | Current type constructor, qualification, size/alignment validity, lowered routine signature, backend-supported declared-type metadata, class/subobject storage partner, numeric/vector/pointer format, typedef/reference/member-pointer provenance when available |
| Executable graph | Every active statement/expression/operator kind, ordered operands, result type/category, original-glvalue information if retained, volatile/guard flags, labels, generated/source correspondence |
| Constants | Lossless scalar values, target strings, symbolic addresses/stack offsets, typed member-pointer values where retained, aggregate/repeat/designator programs, backing-expression roles |
| Calls/runtime | Exact direct routine or indirect callee expression, explicit lowered arguments/hidden receiver/result/VTT parameters, calling convention, builtin/runtime identity, generated thunk/alternate entry relations |
| Layout/dispatch | Actual field order/offset/bitfield data; original contextual base graph; generated base/vptr fields; subobject partners; vtable/VTT variables and typed contents, vbase/vcall indices and wrapper adjustments when populated |
| Init/lifetime | Lowered initialization code, static/TLS initialization routines, retained dynamic-init/master/lifetime/destruction links and flags; no execution reconstructed from a source init list that lowering consumed |
| Partial EH | Function prologue/epilogue, cleanup-state operations, ordered try/handlers, thrown/caught-object operations, symbolic region/array/action records, condition/VTT/VLA adjuncts, runtime exception object cleanup |
| Templates/source | Concrete instances and surviving origin/argument links, primary/partial argument maps, source metadata or separate source-only profile; actual lowered arguments remain distinguishable from original sugar |
| Extensions | Coroutines, captures when reachable, asm constraints/operands/clobbers/labels, VLA dimension reuse, atomic/builtin identity, MS/GNU/UPC/vector variants admitted by profile |

### Statement payloads

`a_statement` (`src/il_def.h:16516–16852`) supplies sequence/parent relationships, primary expression, attributes, guard and generated flags. The following is the full union worksheet; the primary exporter includes surviving forms, while forms consumed by lowering occur only under retained metadata or admitted source-only/residual profiles.

| Kinds, prefix `stmk_` | Active data |
|---|---|
| expr, empty, coroutine_return, assigned_goto, UPC notify/wait/barrier/fence | Primary expression where applicable; computed destination and semantic attributes |
| if, if_consteval, if_not_consteval | Then/else; constant-evaluation mode distinguished from ordinary runtime if |
| constexpr_if | Arms and known/value selection; EDG-selected empty/discarded distinction |
| while, end_test_while | Test and body with test position |
| for, upc_forall | Body plus initialization, increment, init scope, affinity |
| range_based_for | Body, range/begin/end/iterator variables, comparison/increment, initialization/scopes; general lowering rewrites execution, do not synthesize lookup |
| for_each | Body/collection and discriminated implementation pattern; general lowering rewrites it |
| switch, switch_case | Body/selector, ordered case/range/default descriptors and switch/label relationships |
| goto, label | Exact label and retained lifetime boundary; break/continue/leave flags come from label data |
| return | Lowered value or retained return-init role; preserve lowered hidden result parameter |
| coroutine | Coroutine descriptor and generated body status |
| block | Ordered statements, scope and retained lifetime |
| init | Surviving dynamic-init record |
| asm, asm_func_body | Typed asm supplement or literal raw assembly body, with operands/constraints/clobbers/goto labels |
| try_block, microsoft_try | Partial-EH try/handler or guarded/filter/cleanup descriptors |
| decl | Retained entity list/structured-binding container; optional source completeness is profile-dependent |
| set_vla_size, vla_decl | Configured residual dimension/type/variable or EDG-generated replacement allocation/evaluation code |

Loop descriptors: `src/il_def.h:15878–16243`; switch `16245–16314`; handler/try/SEH `16316–16403`; coroutine `16409–16500`. Lowering dispatch including for-each/VLA/GNU results/coroutines is `src/lower_il.c:20300–20437`.

### Expression/operator payloads

Common fields are defined at `src/il_def.h:14906–15332`; payload union at `15334–15796`. Type is not a substitute for value category; `operation.type_kind` may differ from result/operand type kinds. Operand list order is not universal evaluation order. Cast, volatile, initialization-guard, generated-default-argument and retained original-glvalue flags need field-by-field disposition. Lowering may intentionally clear or replace original flags.

| Kinds, prefix `enk_` | Active data |
|---|---|
| operation | Stable operation discriminant, operation type kind, operands and semantic flags |
| constant, variable, routine, field | Typed target and minimum applicable name-reference semantic supplement; vacuous-destructor routine can be null |
| temp_init, lambda | Dynamic-init plus temporary type/source or lambda supplement, when retained; execution is the lowered materialization |
| new_delete, gcnew | Allocation/initialization/cleanup supplement only if this source form survives in metadata or admitted residual profile; primary execution uses lowered replacement |
| throw | In partial EH, thrown type/init metadata, lowered initialization expression and destructor; null supplement means rethrow |
| condition | Declaration scope/init/test/init-statement if retained; lowered control expression otherwise |
| object_lifetime | Wrapped expression plus retained lifetime; do not duplicate cleanup already present in code/tables |
| typeid | Type/optional expression/dynamic flag if retained; primary runtime form is lowered |
| sizeof, alignof, datasizeof | Type-or-expression and standard-alignof flag; VLA dimensions when applicable |
| sizeof_pack, braced_init_list, fold, pack_index | Metadata/prototype forms, not unresolved primary execution |
| address_of_ellipsis | Feature identity, no payload |
| statement | GNU statement block and actual selected result representation |
| reuse_value | Referenced initialization if retained; lowered execution must already preserve single evaluation |
| lowered_eh_construct | Required partial-EH kind and all active supplementary fields; section 4 |
| result_of_overriding_function | Required lowered covariant-wrapper result marker where configured |
| vla_dealloc | Exact VLA variable if present |
| type_operand | Type, or null C11 generic default; definition-needed role |
| builtin_operation | Builtin discriminant and ordered typed operands; no re-solving traits |
| param_ref | Parameter number/list depth and contextual-this meaning where retained |
| c11_generic, builtin_choose_expr | Operand metadata and exact selected-result branch, no execution of unselected branches |
| await, yield | Operand and separately named ready/resume/suspend calls; stored ready/resume/suspend order is not runtime order |
| initializer | Folded dynamic-init backing reference |
| concept_id, template_name | Template and applicable arguments |
| token_sequence | Reflection interpolations/tokens where retained, never front-end token-cache pointer |
| requires, compound_req, nested_req | Parameters/requirements, expression/constraint/noexcept, nested constraint |
| const_eval_deferred | Wrapped expression and deferred status, metadata only |
| error, reclaimed, last | Never valid exported semantic nodes; runtime_sizeof is a sizeof alias |

All operator kinds remain distinct. Lowered member calls, base adjustments, pointer-to-member encodings, constructor calls and implicit arguments are copied from EDG, not reconstructed from original C++ operators. Original call operands/flags are recorded only where still available. The exact unresolved-candidate symbol behind `tpck_unknown_function` is front-end-only (`src/il_def.h:5441–5449`).

**Exact selected source overload fidelity is not universal after stock lowering.** Ordinary nonvirtual member/conversion calls retain a direct routine operand while changing to `eok_call` (`src/lower_il.c:13504–13513`). Constructor/destructor calls can change to a complete alternate entry (`13480–13490`), with a separately documented primary-entry relation where supplied. Virtual-call lowering computes dispatch from the selected routine, then reuses that original `enk_routine` node as a new indirect call and overwrites the original call (`12413–12542`, especially `12519–12541`). No distinct original selected-routine edge is added there. The API therefore exposes the actual direct `TargetMethod` when present and optional `SourceTargetMethod` only through a surviving explicit EDG relation. Recovering the latter from vtable slot arithmetic, names or signature matching is prohibited. The user accepted this source-query limitation on 2026-10-12 in favor of lowered execution. Preserve explicit surviving source links without promising universal source-selected targets; exact emitted execution remains required.

### Constants, initialization, types and other auxiliary unions

`a_constant` (`src/il_def.h:4741–5570`) includes integers, fixed point, floating/imaginary/complex, strings, address/member pointer, label difference, **stack offset**, dynamic init, aggregate, repeat, designator, template parameter, UPC, void and reflection. `ck_stack_offset` is a symbolic variable-plus-offset and is required for partial EH; it is not a host address or assigned native stack location. Address constants retain base kind, byte offset, one-past-end and subobject path. Member-pointer original representations may be replaced by EDG constants/storage layouts; preserve both only when actually retained. Aggregate/repeat/designator lists are initialization programs, not arbitrary JSON values.

Exact integers use signed-magnitude canonical decimal scalar text without SQLite REAL conversion. Floating columns represent class/sign/radix/significand/exponent and retained NaN payload/quiet state through lossless EDG adapters. A printer emitting generic NaN is insufficient. Strings retain target units, explicit length and NULs using a proven lossless text fast path or ordered numeric code-unit rows (`src/il_def.h:5048–5068`). Source occurrences, value interning and addressable storage identities are separate. No content interning may merge distinct address identities. No opaque structure blobs, base64 or hex dumps.

`a_dynamic_init` has nine distinct kinds plus alias (`src/il_def.h:3957–4388`): none/zero, constant/aggregate/lambda, expression/class result, selected constructor and bitwise copy. Common lifetime/destructor/condition/unordered/overlap/master flags remain exportable if retained. Original constructor/source/destination associations are not universally recoverable after lowering. `destructible_entity_descr` and its `init_pos_descr` are work structures, expressly outside the post-lowering contract. Required executable cleanup addresses come from EH tables, not these freed structures.

Types include every constructor in the ledger, qualification/address-space/atomic/pointer modifiers, numeric/vector representations, complete/incomplete/zero-bound/VLA states, lowered routine signatures and backend-supported retained declared types (`src/il_def.h:5573–5643,6244–6287,6385–6605,7394–7665,9768–10998`). `size == 0` does not imply incomplete; typedef wrappers, GNU empty records and zero-length arrays are counterexamples. `based_types` is an incomplete reuse cache, excluded from semantic reachability (`9778–9815`). Internal lowering type copies are excluded even if a non-null pointer happens to survive.

| Auxiliary record | Active union and retention obligation |
|---|---|
| `an_initializer`, `src/il_def.h:11028–11063` | Static constant, dynamic init or structured-binding expression; local-static values are joined through region-owned records |
| `a_local_static_variable_init`, `11066–11099` | Variable, static/dynamic payload and retry/exception lifetime if retained; actual lowered guard/init code is authoritative execution |
| `a_variable.variant`, `11210–11246` | Parameter type association, binding container or ordered bindings; binding expressions are unlowered metadata copied/lowered on use |
| `a_vla_dimension`, `11101–11173` | Type, expression or original-dimension reference, evaluation point and configured lowered count/dimension variable; preserve single evaluation |
| `a_subobject_path`, `4606–4650` | Field/base/element-offset discriminator; element offsets differ from address byte offsets |
| `an_exception_specification`, `7100–7205` | Effective throw-any/indeterminate/noexcept flags separate from backing argument/type-list provenance; noexcept argument can remain dependent even for a nondependent type |
| `a_local_expr_node_ref`, `14835–14890` | Resolve typed referrer/expression while region lives; serialize the relationship, not the lookup mechanism |
| `a_name_reference`, `2637–2744` | Semantic builtin/property/destructor distinction where retained; no cache linkage |
| `a_reflection_value`, `4708–4720` | Typed reflected entity and local ownership; token cache excluded |
| Class/base/override supplements, `7848–8320,8737–9275` | Actual surviving contextual bases, virtual/return adjustment relations, generated tables and subobject partners |
| Lambda/capture, `17937–18180` | Closure/type/callable/capture metadata only where still reachable; actual generated fields and callables remain executable dependencies |

## 4. Partial EH: executable cleanup survives work-descriptor deletion

Stock `visit_object_lifetime_tree` unconditionally frees and nulls each destruction's `destructible_entity_descr`, even with `detach == false` (`src/lower_il.c:22882–22922`). Lifetime retention therefore **does not** preserve a complete init-position/work-state graph. Do not copy the direct C# campaign's lifetime-descriptor patch, require that patch, or dereference those cleared pointers.

Stock generated EH tables are the intended durable executable interface:

1. `leck_function_prologue` links routine plus `region_table` and `array_table` variables (`src/il_def.h:14815–14832`; `src/lower_eh.c:5094–5107`). These variables and their initializers must be rooted even if no ordinary source expression references them.
2. `leck_cleanup_state` and unreachable cleanup state carry **region numbers** with `GENERATE_EH_TABLES=1`, rather than pointers requiring freed descriptors (`src/il_def.h:15525–15545`; `src/lower_eh.c:6345–6358`). Region numbering includes supplemental rows; retain exact identities/order rather than renumbering only actionable rows.
3. `make_handle_for_entity` represents direct variable+offset, indirect variable+offset, or this-relative offset (`src/lower_eh.c:3210–3253`). `make_handle_constant` emits symbolic `ck_stack_offset` for automatic variables, address constants for static guards, or integers for this offsets/array-table indices (`3256–3306`). Decoder context distinguishes these meanings.
4. Array descriptors contain handle, element size and count (`3309–3462`). A count of -1 means runtime count; zero is used for nonarray supplemental size cases. VLA count location is a separate supplemental record. These values are not equivalent to “no elements.”
5. Region descriptors contain destructor/delete routine, handle, next-region number and flags (`3471–3514`, `3630–3693`). Selected routines include EDG's complete/subobject/delegation alternate destructor entries and runtime deallocation helpers (`3731–3913`). Supplemental rows encode conditional flag, subobject VTT or VLA count address in documented order (`3915–3943`).
6. Decode flags using the actual ABI/context: `RDF_SUBOBJECT_VTABLE` versus `RDF_LET_THIS` shares bit 0x20 across configurations; `RDF_BASE_CLASS_SUBOBJECT` versus `RDF_VLA` shares bit 0x40, distinguished by array context (`3088–3168`). Store typed action/condition/address-kind facts, not an uninterpreted integer bitfield. Preserve existing lowered control code that initializes conditions/counts and all selected helpers.
7. Preserve remaining EH operations: caught/thrown-object address, function/catch/try epilogues, exception-started/caught boundaries and internal try. `leck_initialization_completed` is absent in the primary tables profile because it is guarded by `!GENERATE_EH_TABLES` (`src/il_def.h:13772–13813`). Keep it in the source-wide ledger for the alternate profile.

This source evidence establishes where executable cleanup facts reside after work-descriptor cleanup; it is not an already executed proof for every exception case. M0 must inspect valid cases for automatic/indirect/this subobjects, conditional temporary completion, arrays/partial arrays/VLAs, sized/aligned/placement cleanup, local-static retry, catch object release and VTT-bearing destructors. Any uncovered field is a profile/exporter gap, not permission to recreate C++ destruction rules.

Normalize EH into function/region/action/address/array/condition/VTT tables. Keep the generated table variable's entity identity and a typed relation to its normalized contents if other IL takes its address. Do not persist a duplicate aggregate copy merely to mirror the same table, but do not drop address identity or ordinary executable references. Retained lifetime chains remain useful navigation metadata and must not cause cleanup to run twice.

## 5. Original metadata survival and loss at the lowered boundary

There is no promise of a full prelower tree. The following matrix is based on stock code and must be confirmed at the actual backend boundary under M0 retention flags.

| Topic | What survives / changes / is lost |
|---|---|
| Nominal class and base graph | `lower_class_struct_union_type` lowers fields/base types/scope/arguments and changes `tk_class` to `tk_struct`; it does not replace the class supplement or clear the base list (`src/lower_il.c:9039–9083`). Retain identity, available base/override/template metadata, but don't claim the original class-vs-struct token from the mutated kind. |
| Storage fields and partners | Prelowering adds base/vptr/virtual-base fields and vtable variables (`8836–8986`). `make_subobject_class_type` may copy fields into a distinct storage type with different nonvirtual size/tail padding and reciprocal `subobject_partner` (`8520–8684`; `src/il_def.h:9159–9168`). The original class's field list is itself modified; it is not a pristine source field list. |
| Property/empty fields | Nontrivial MS property/event fields are removed from the class field list (`src/lower_il.c:9980–10011`). Empty optimized member accesses are rewritten to equivalent offsets (`2223–2270`). Retain original metadata only through remaining valid relationships; do not infer deleted field declarations from arithmetic. |
| Member-pointer/nullptr/reflection types | `overwrite_type_with_new_type` creates an internal original copy while replacing the type (`9330–9350`, `9420–9433`, `9629–9641`). **Do not export `typeref.orig_type`: its contract says internal use in IL lowering only** (`src/il_def.h:10742–10749`). Original semantic type is optional unless another supported retained relation supplies it. References need not be converted into ordinary pointers (`src/lower_il.c:9397–9400`). |
| Routine signature | Hidden result/this parameters and void return are inserted; by-copy parameters become indirect. `this_class`/qualifiers intentionally survive (`9470–9551`). A special RTTI function copy exists internally, but **routine `unlowered_type` explicitly must not be used by a backend** (`src/il_def.h:10244–10253`). Declared types/source correspondence may help where retained but cannot justify guessing or reading that internal copy. |
| Default argument declarations | `default_arg_expr` is cleared on routine and declared-type parameters (`9551`, `10314–10337`). Selected default arguments already copied into calls survive through their lowered execution; the original declaration expression is optional unavailable metadata. |
| Constructor/destructor initialization | Wrapper generation emits executable calls and clears/consumes constructor-init lists (`src/lower_init.c:17099,17330–17345,17613`). Do not require the complete original mem-initializer list in primary objects. Field default initializers themselves can remain unlowered (`src/il_def.h:12055–12066`). |
| Class and namespace members | Member constants/variables/routines/types are promoted and original lists cleared (`src/lower_il.c:20447–20614,20817–20824`). Discover executable definitions from current promoted roots and retain EDG ownership correspondence where available. Empty original scope lists do not imply no members existed. |
| Template instances | Class/function/variable actual arguments are lowered in place (`9002–9036,9076,9830–9831,10314`). Keep separate primary and partial-specialization maps and available origin links. Do not promise unlowered argument values unless explicit copies/backing metadata survive. Full origin availability also depends on stock IL retention before frontend memory release. |
| Source/using/provenance | Using lists are cleared unless `PRESERVE_SOURCE_SEQUENCE_LISTS_WITH_IL_LOWERING`; hidden names are unconditionally cleared (`22239–22265`). Integration must verify stock source-retention flags are mutually compatible with lowering. No new prelower hook is proposed to fill a source-shape gap. |

Virtual base/vcall indices, vtable/VTT variables, ctor/dtor alternate entries and adjusting/covariant thunks are now primary export dependencies when generated (`src/il_def.h:8241–8318,8792–8809,9145–9168,13195–13237`; `src/lower_il.c:21913–21940`). Preserve actual selected entry points and table contents. Metadata about the source virtual declaration does not replace the actual lowered indirect call or thunk.

## 6. Reachability and residual feature obligations

Root all retained executable routine definitions, including static/inline/template instances, lambda callables, generated constructors/destructors, alternate entries/thunks, coroutine-generated bodies, runtime-support definitions and static/TLS init/termination routines. Root variable definitions and EH/vtable/VTT/RTTI tables and their symbolic references. Include local declaration/variable/type regions and promoted entities. Do not apply source-header exclusions or a main-reachability filter. Declaration-only external/runtime routines remain typed declarations, not fabricated bodies.

Traverse ordered lowered statement/expression/initializer edges and their type/field/routine/variable/label dependencies. Then traverse retained lifetime, source, backend-supported declared-type, template and backing-expression relationships under distinct metadata roles. Exclude `typeref.orig_type`, routine `unlowered_type` and other internal-lowering-only fields. Backing expressions are never lowered by EDG (`src/il_def.h:4774–4785`); default field and binding expressions can also be unlowered. Such metadata is not a reason to reject a valid lowered object or execute a second version of the program.

Keep exact definition availability and origin profile internally. Do not instantiate unused templates or regenerate omitted bodies. The function-region lifecycle remains significant (`src/il_def.h:18299–18318`, routine descriptors `12245–12263`). File-backed region support, if added, requires stable region identity; live pointer reachability cannot recover freed nodes.

Coroutines retain EDG's generated body, promise/handle/parameter copies and selected protocol calls where present (`src/il_def.h:16409–16500,15670–15681`). A future runtime must implement remaining await/suspend mechanics, but must not rediscover promise operations. Native frame/register/GC layout is not proved by the descriptor. VLA/complex/vector residuals require explicit configuration and bounded storage/operation recipes; no profile silently assumes the consumer knows C++ construction rules.

Atomic operations have no dedicated `eok_atomic` enumerator: preserve atomic qualifier, exact builtin callee/category/kind, inserted size argument and memory-order operands, plus static initialization guards (`src/il_def.h:2608–2625,2678–2689,6248,6287,12121–12181,14970–14974,16550–16556`). Builtin/runtime identity is not guessed from text. GNU statement expressions, computed goto, label addresses, two-operand conditional, asm, complex/vector ops, MS SEH/managed/property forms and UPC admitted by the actual upstream profile all need complete typed coverage. CUDA/GPU is not established by this pinned IL inventory and is not claimed without additional upstream evidence.

Raw asm text is retained with semantic operands/constraints, but this phase does not promise C# execution. Keeping an enabled extension in an object is distinct from implementing it on .NET. Empty optional metadata properties never excuse omitted executable extension data.

## 7. Implementation acceptance obligations

Before implementation completeness, freeze a field matrix for every active union/common semantic member: owner, discriminator/guard, source field, phase/role, normalized destination, API property, null/empty meaning, units/order and fixture. The exhaustive enums below are the start, not a substitute for the guarded-field audit. Unknown required kinds fail explicitly; optional unsupported metadata has nullable/empty API shape and profile documentation.

M0 must prove stock lowering plus partial EH retains sufficient executable data without the direct backend's core patch; confirm generated EH tables survive, source retention feasibility, template-origin availability and routine/type metadata losses. M1 must settle scalar extraction and table decoders. Valid feature fixtures then assert selected lowered calls/arguments, actual storage types/offsets/table entries, symbolic cleanup addresses and transitions, condition/VTT/VLA adjuncts, initialization and generated definitions, captures/coroutines/atomics and enabled dialect families. No corrupted-artifact tests and no tests that second-guess EDG semantics.

No native stack/register placement, complete original source AST, erased default declarations or universally pristine original type/signature is claimed. A source-only compatibility capture is explicitly separate. This design adds no EDG modifications, implementation or tests.

## 8. Exhaustive source enum ledger

Counts include all source `#if` branches, aliases and sentinels. The primary build contains a subset. Stable schema tags do not use configuration-dependent EDG ordinals as their sole meaning.

Disposition key: **E** = semantic kind supported wherever reachable under the declared profile/role; may be ordinary lowered execution or a surviving original/residual representation, as qualified in sections 2–3. **P** = compile-time/dependent/backing metadata, not unresolved primary execution. **L** = lowering-produced kind, now **required primary coverage when generated** (except explicitly inactive alternative-profile guards). **X** = error/reclaimed/sentinel, never a valid semantic node. **A** = alias normalized to its canonical kind. E/L are export requirements, not a promise of .NET runtime execution. Source-conditional names do not imply every extension is available in the primary build.

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

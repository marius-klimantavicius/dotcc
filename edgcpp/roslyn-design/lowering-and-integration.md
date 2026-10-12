# Lowering and integration decision

Design only, revised to follow the user's preference for the direct C# backend's lowering strategy. Upstream evidence is pristine EDG revision `89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`, inspected at `/tmp/roslynobj-edg-upstream-20261012`; upstream citations below use paths/lines at that revision. The newly authorized comparison uses only `edgcpp/config/cpfe-dot.cmakedef` and the direct emitter `edgcpp/backend/dot_gen_cleanup.h`, plus the parent investigation's effective flag inventory. No existing object exporter, object linker, or its findings are evidence. No implementation or EDG core changes are proposed here.

## Revised decision

The current proposal is **stock C++ lowering with partial exception lowering and EH tables**, matching the direct C# backend's general strategy. Serialize the resulting executable graph and the nominal type/member/source information that actually survives in **one normalized `.roslynobj`**. The public object model can expose convenient classes, members, operations and relationships without reproducing EDG's low-level allocation structure. It cannot claim that lowering left a pristine original C++ tree.

**Unresolved fidelity gate:** the original requirement includes exact source-selected overloads. Stock lowering overwrites some such links, including the routine operand of a virtual call. Therefore the current lowered proposal has not yet demonstrated that it satisfies the original semantic-query requirement, and this document does not authorize weakening that requirement. The user must resolve whether the object may expose exact lowered targets plus documented source-query losses, or whether those source queries take priority and require a different capture/lowering design within the no-core-change constraint. Implementation approval is pending that decision. Optional/null API properties are not a solution to a required semantic loss.

This puts difficult C++ work in EDG: constructor/destructor expansion, object initialization, reference/member-pointer transformations, virtual dispatch adjustment, allocation/deallocation, range-for expansion, temporaries, normal-exit cleanup, static/TLS initialization, and much of exception setup. The future C# backend consumes selected operations and cleanup records instead of implementing those language algorithms. Ordinary C-like expressions, storage, control flow and a defined exception protocol remain downstream code generation.

The earlier unlowered-primary proposal is superseded. It preserved source structure but left too much C++ execution work to the downstream consumer. Unlowered IL remains a clearly labeled compatibility stage for constructs stock lowering cannot handle; it is not interchangeable with the primary lowered execution stage and is not evidence that those features are easy to implement in C#.

There is no pre-lowering snapshot in this design. Stock `back_end()` runs after lowering and front-end wrapup. `finish_function_body_processing` calls `lower_function_scope` directly (`src/scope_stk.c:8912–9027`); no supported generic pre-lowering callback was found. The similarly named `IL_LOWERING_INIT_ROUTINE_PREFIX` merely names synthesized static-initialization routines (`src/host_envir.h:1431–1440`), not a hook for wrapping the compiler pass. Macro-renaming internal routines, interception, or copying parser state into a private snapshot is outside the allowed build/backend registration changes.

## Exact primary configuration

These settings are an implementation specification to verify in the staged build, not a claim that the new backend has already been built. Both stock generator identity flags remain false: this is a new backend. The existing direct configuration explicitly chooses lowering, tables, unordered EH processing, lifetime retention, resident IL, VLA/class-rvalue lowering and local-entity promotion (`edgcpp/config/cpfe-dot.cmakedef:1–24`). Its additional effective flags reported by the parent investigation are made explicit here rather than relying on inherited defaults.

| Macro | Primary value | Purpose / stock coupling |
|---|---:|---|
| `BACK_END_IS_C_GEN_BE`, `BACK_END_IS_CP_GEN_BE` | 0 | Use the new backend's `back_end()` symbol; do not select a source generator's unrelated behavior. |
| `BACK_END_SHOULD_BE_CALLED` | 1 | Stock invocation after `fe_wrapup` (`src/host_envir.h:1243–1258`). |
| `DO_IL_LOWERING` | 1 | EDG performs the C++ implementation transformations. |
| `DOING_SOURCE_ANALYSIS` | 0 | Required with lowering (`src/host_envir.h:1444–1457`). |
| `DO_FULL_PORTABLE_EH_LOWERING` | 0 | Keep structured try/catch/throw and backend EH markers; no EDG throw-to-setjmp implementation. |
| `GENERATE_EH_TABLES` | 1 | Have EDG produce the actual cleanup location/action/state recipe. |
| `DO_UNORDERED_EH_PROCESSING` | 1 | Match direct profile; preserve EDG's conditional/unordered cleanup decisions. |
| `KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED` | 1 | Keep surviving lifetime metadata in EH-enabled compilations; this does not preserve private lowering descriptors. |
| `IL_SHOULD_BE_WRITTEN_TO_FILE` | 0 | Serialize resident IL, not an EDG memory image. |
| `IA64_ABI` | 1 | Use EDG's supported Itanium-style lowering rules. Record all actual target sizes/alignment/ABI options. |
| `TARG_IA64_ABI_USE_VARIANT_PTR_TO_MEMBER_FUNCTION_REPR` | 1 | Match the direct backend's configured member-function-pointer variant; do not infer its representation from host pointers. |
| `MINIMAL_INLINING` | 0 | Preserve routine boundaries and avoid optional front-end inlining. |
| `MAINTAIN_NEEDED_FLAGS` | 1 | Match direct lowered bookkeeping; flags may be inspected as facts, not used as the sole serialization root. |
| `DEFAULT_REMOVE_UNNEEDED_ENTITIES` | 0 | Keep retained entities rather than prune around a guessed program entrypoint. |
| `PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE` | 1 | Match direct layout/entity placement, including alternate entries for local classes. Requires lowering (`src/targ_def.h:4892–4901`). |
| `NEW_CAN_BE_FOLDED_INTO_CTOR` | 0 | Required with Itanium ABI plus lowering (`src/targ_def.h:3053–3057`). |
| `DELETE_CAN_BE_FOLDED_INTO_DTOR` | 1 | Required by lowering's sized-delete path (`src/targ_def.h:3023–3025`). |
| `NEW_AND_DELETE_FOR_ARRAY_CAN_BE_FOLDED_INTO_RUNTIME_ROUTINE` | 1 | Required by stock lowering (`src/targ_def.h:3060–3075`). |
| `ASSIGNMENT_TO_THIS_ALLOWED` | 0 | Stock Itanium/lowering configuration forbids this archaic extension (`src/targ_def.h:3006–3017`). |
| `LOWER_VARIABLE_LENGTH_ARRAYS`, `LOWER_CLASS_RVALUE_ADJUST`, `LOWER_COMPLEX`, `LOWER_FIXED_POINT` | 1 | Match direct strategy; require the corresponding VLA/complex/fixed-point feature support (`src/targ_def.h:519–589`). |
| `LOWER_IFUNC` | 1 | Use EDG's wrapper/resolver implementation, with its documented semantic timing caveat below. |
| `LOWER_LVALUE_RETURNING_OPERATIONS` | 1 | EDG handles C++ assignments/conditional/comma/prefix operations whose results are lvalues (`src/targ_def.h:5203–5214`). |
| `LOWER_MICROSOFT_NONCONSTANT_AGGREGATE`, `LOWER_DESIGNATED_INITIALIZERS` | 1 | Preserve direct strategy for initializer elaboration (`src/targ_def.h:5216–5241`). |
| `GENERATE_SOURCE_SEQUENCE_LISTS` | 0 | Match direct profile. Do not advertise original declaration-occurrence order after lowering. |
| `RECORD_BACKING_EXPRS_WITH_IL_LOWERING` | 1 | Preserve useful original expression metadata where stock EDG records it; it is not executable lowered IL. |
| `ONE_INSTANTIATION_PER_OBJECT` | 0 | One primary TU object; no prelinker slicing feature in the new writer. |

Retain target/language capabilities needed by enabled profiles, including `C99_IL_EXTENSIONS_SUPPORTED`, `CPP11_IL_EXTENSIONS_SUPPORTED`, GNU/MS extension support and fixed-point support if `LOWER_FIXED_POINT=1`. Macro consistency is validated for each build profile; merely defining a lowering flag while disabling the source feature is not a supported configuration.

For ordinary independent TUs, use `INSTANTIATE_TEMPLATES_EVERYWHERE_USED=1`, `LINKER_CAN_DISCARD_DUPLICATE_DEFINITIONS=1`, `DEFAULT_INSTANTIATION_MODE=tim_used`, `AUTOMATIC_TEMPLATE_INSTANTIATION=0`. These are stock dependencies/defaults (`src/host_envir.h:860–961`). They let EDG materialize needed available definitions in each TU. Duplicate definitions are retained as a future linker responsibility; neither writer nor reader implements cross-object linking in these phases. Do not force unused instantiations, override explicit instantiation declarations, or pretend external definitions are present.

## Exception representation: tables are compiler facts, not a target runtime

The three stock EH modes are documented at `doc/source/lower_il.rst:2742–2775`:

| Mode | `DO_FULL_PORTABLE_EH_LOWERING` | `GENERATE_EH_TABLES` | Consequence |
|---|---:|---:|---|
| Full portable | 1 | forced 1 | Rewrites EH completely, including setjmp-based try handling. Rejected. |
| Partial/table-backed | 0 | 1 | Keeps EH constructs, emits cleanup-state markers and region/array tables. **Primary.** |
| Partial/no tables | 0 | 0 | Keeps lifetime-based cleanup pointers; leaves more work to backend. Not selected. |

Both partial modes lower expressions/statements beneath EH constructs and provide copying/setup for thrown and caught objects. Tables do not oblige a future C# backend to lay out a native stack or execute EDG's portable EH runtime. They describe a finite cleanup-state protocol that can be consumed at translation time and implemented using target exception support. The direct emitter already takes this approach (`edgcpp/backend/dot_gen_cleanup.h:1–5`, `49–159`). The new object stores those decisions as typed rows and references, not opaque table bytes or JSON.

### Critical stock lifetime limit

`KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED=1` retains object lifetimes only when language exceptions are enabled (`src/lower_il.c:23168–23180`). Even then, `visit_object_lifetime_tree` **unconditionally frees and nulls `a_dynamic_init.destructible_entity_descr`**, independently of whether the lifetime is detached (`src/lower_il.c:22882–22922`). Keeping that descriptor would require a core patch; none is allowed. The descriptor is lowering-private state (`src/il_def.h:4352–4359`), not an exporter API.

Consequently, the new writer must not rely on the direct effort's separately reported descriptor-retention patch. EH action/location completeness comes from stock surviving tables and markers. Retained lifetimes are supplementary metadata and must not cause duplicate emission of destructor actions already in the lowered stream.

### Surviving stock interface

- `leck_function_prologue` points to `an_eh_prologue_supplement`, whose `region_table` and `array_table` are ordinary IL variables with aggregate initializers (`src/il_def.h:14815–14832`). Table initializers can be on the scope's `local_static_variable_inits`; the direct emitter demonstrates collection there (`edgcpp/backend/dot_gen_cleanup.h:60–67`).
- `leck_cleanup_state` and `leck_unreachable_cleanup_state` identify a cleanup region **number**, not a private descriptor (`src/il_def.h:15525–15545`). Preserve the upstream null-region sentinel and raw row numbering before resolving typed links, because supplemental physical rows also occupy indexes.
- A region row is destructor/delete routine, handle, next-region number, and flags (`src/lower_eh.c:3630–3693`). Normalize these into selected routine, address/array relationship, successor and named scalar properties. Do not discard null-routine/special-action cases.
- Handles remain symbolic: `ck_stack_offset` supplies a variable reference plus offset; static/thread objects use address constants; `this`-relative and indirect addressing are distinguished by flags (`src/lower_eh.c:3210–3306`). No actual native stack offset is needed to export them. Preserve offset units and target integer range.
- Array-table rows supply handle, element size and element count (`src/lower_eh.c:3367–3462`). Counts can include special negative values; do not coerce everything to unsigned. Nonarrays can use this table to supply sized-deallocation information. VLA rows have a separate dynamic element-count location.
- Region supplements specify conditional construction flag, subobject/VTT or delegation-destructor argument, and VLA element-count location (`src/lower_eh.c:3749–3778`, `3915–3943`). Their order and overloaded flag meanings are specified by `RDF_*` at `src/lower_eh.c:3083–3169`. In particular `0x40` means base subobject or VLA depending on the array flag; `0x20` is a VTT/delegation supplement in the applicable base-subobject case, not a universal Boolean.
- Remaining EH marker kinds include thrown/caught object address, function/try/catch epilogues, exception-started/caught boundaries and an internal try with paired try/catch expressions (`src/il_def.h:13769–13810`, `15525–15577`). Handler typeinfo and parameter initialization survive (`src/il_def.h:16322–16352`). Preserve try handlers, exception specifications and lowered operand operations as well as table records; tables alone are not all exception semantics.

This is a verified stock representation with the information intended for a partial-EH backend. Its complete normalized adapter is an implementation gate, especially conditional/unordered objects, partially constructed arrays, sized/aligned/placement deletion, virtual bases/VTTs, failed static guards and exception-object cleanup. The direct emitter is a working strategy reference, not the new coverage limit: do not inherit its 4096-row cap, fixed array-size limits, or unsupported-ABI cases (`edgcpp/backend/dot_gen_cleanup.h:81`, `201–251`).

A future consumer still implements a runtime protocol for matching exceptions, rethrow/object ownership, cleanup failure/termination and invocation of exported actions. It does not invent destructor order, object addresses, selected deallocation functions, or constructor-failure cleanup from C++ declarations. Normal-flow cleanup operations are already in the lowered stream; exceptional cleanup follows EDG's state and tables. An EH-disabled build has no such exception behavior to invent and can lose lifetime metadata while retaining normal cleanup calls.

## What the model can expose after lowering

The main object carries one executable stage. High-level class/member/type convenience views use stock surviving metadata and links, not a second source snapshot. The executable type/signature and a retained nominal association are distinct facts.

| Original feature | Stock lowered execution and surviving information | Public-model boundary |
|---|---|---|
| Classes and inheritance | Layout fields, base/vptr fields, class supplement, base graph, generated vtable/VTT variables, and storage subobject partner (`src/lower_il.c:8836–8986`, `9039–9083`; `src/il_def.h:9145–9167`). | Expose nominal class identity/relationships when retained plus generated storage layout; class kind can have become struct. Do not call lowered fields a pristine source member list. |
| Members/local classes | Promotion moves members/local entities into enclosing/file lists; original class association can survive separately. | Recover association only from explicit retained links, never mangled-name parsing. Member-list promotion can empty original lists (`src/lower_il.c:20817–20824`). |
| Function types | Hidden arguments/return slots and constructor/destructor variants are elaborated. `this_class`/qualifiers may remain as metadata, but defaults are cleared (`src/lower_il.c:9519–9551`, `10334`). | Expose executable parameters separately from retained nominal membership; do not promise exact original signatures/default expressions. |
| References/member pointers/nullptr | Ordinary reference type descriptors/flags are retained: `lower_type` explicitly does not turn all reference types into pointers (`src/lower_il.c:9399–9401`). Reference operations are lowered, and an individual ref-indirect operand receives a pointer type (`15816–15835`). Member-pointer/nullptr types can be rewritten. | Preserve supported retained reference flags. Exclude `typeref.orig_type`: upstream marks it lowering-internal-only (`src/il_def.h:10742–10749`). Do not advertise recovery of all original member-pointer/nullptr types. |
| Range-for | Rewritten into block/for machinery and selected calls, including lifetime cleanup (`src/lower_il.c:19891–19964`). | Loop/call views remain useful; an exact original range-for node is not promised. |
| New/delete/constructors | Allocation, initialization, destructor/alternate-entry/runtime-array operations are elaborated by stock lowering. | Expose selected calls, initialization, memory operations and semantic runtime roles without reconstructing original syntax. |
| Lambdas/virtual dispatch | Closure types, bodies, adjustments, tables/thunks and lowered calls remain. Virtual-call lowering reuses/overwrites the original `enk_routine` operand as an indirect call (`src/lower_il.c:12524–12543`). | Exact lowered direct calls remain identifiable. The exact source-selected virtual routine can be lost even while tables/override facts survive; inferring it from generated code is forbidden. This is the unresolved fidelity gate, not automatically optional metadata. |
| Constants/backing expressions | `RECORD_BACKING_EXPRS_WITH_IL_LOWERING=1` retains original backing expression metadata, which EDG explicitly does not lower (`src/host_envir.h:1488–1508`). | Classify these as metadata, outside executable traversal; they may contain unlowered operators/types. Never execute both folded constant and backing expression. |
| Source positions | Surviving positions, names/unmangled names and macro records can be exported. Source sequences are off. | Best-effort source navigation, not complete declaration occurrences, original ordering or token-perfect C++ syntax. |

Do not follow a pointer merely because lowering saved it. `a_type.variant.routine.unlowered_type` is a special RTTI/mangling aid that upstream explicitly says should not be used by a backend (`src/il_def.h:10244–10253`). `a_type.variant.typeref.orig_type` is for internal use in lowering only (`src/il_def.h:10742–10749`). Both are excluded from the approved backend-accessible field inventory. Saved copies in `overwrite_type_with_new_type` or RTTI handling are not a supported source-signature/type restoration API.

The concrete virtual-call loss is also independent of source-sequence retention: `lower_virtual_function_call` changes the selected routine operand into `eok_call` with the function-selection expression, then overwrites or rewrites the original call node (`src/lower_il.c:12524–12543`). No stock universal source-selected-call backpointer is established by this code. Generated vtable entries describe dispatch implementation; a reverse analysis of those entries is not serialization of a retained overload-selection result.

Upstream warns that C++-specific fields remain after lowering and are not necessarily cleared/maintained (`doc/source/lower_il.rst:119–123`). Therefore each nominal field requires an adapter inventory showing meaning and validity at this stage. Blindly exporting every nonnull C++ pointer as current semantic truth is unsafe. The semantic inventory records the per-field distinctions.

`GENERATE_SOURCE_SEQUENCE_LISTS=1` with lowering is normally suppressed unless `ALLOW_SOURCE_SEQUENCE_LISTS_WITH_IL_LOWERING` is enabled. `PRESERVE_SOURCE_SEQUENCE_LISTS_WITH_IL_LOWERING` is explicitly an unusual/less-debugged mode and does not make lowering maintain a pristine source graph (`src/host_envir.h:1546–1590`). It is not part of the primary profile and does not solve lost signatures/default arguments. A later retention experiment would need evidence of each promised relationship; no such experiment authorizes a core patch.

## Which residuals are simple, and which are not

Keep the direct lowering settings by default. Deviating requires a concrete benefit and a bounded downstream rule, not the general observation that C# has objects or exceptions.

| Area | Decision | Reason |
|---|---|---|
| Class layout, base adjustment, virtual/member pointers, ctor/dtor variants | Let EDG lower. | Deep C++/ABI knowledge; ordinary C# class semantics are not equivalent. No verified independent switches preserve all these while retaining their generated execution. |
| Lifetimes/temporary destruction/new/delete | Let EDG lower; retain table protocol. | Construction order, partial initialization and exception exits are not a simple C# `using` substitution. |
| Range-for | Let EDG lower. | Already resolved expansion is available; selecting a subset pass is not a stock independent option. |
| C-like blocks, loops, direct/indirect calls, scalar conversions | Consumer handles emitted form. | Ordinary code generation over resolved operations, with target widths/evaluation order respected. |
| Aggregate constants, `ck_designator`, `ck_init_repeat`, mid-block `stmk_init`, block copies | Keep as explicit normalized residual operations. | These remain even for the C generator; typed ordered-data interpretation is bounded and does not require redoing C++ language analysis (`doc/source/c_gen_be.rst:76–110`; `src/targ_def.h:5229–5241`). |
| Lvalue-returning operations | Keep `LOWER_LVALUE_RETURNING_OPERATIONS=1`. | C# expression lvalueness is not a blanket match; EDG already has the correct transformation. |
| VLA/complex/fixed-point | Keep direct lowerings. | Consumer supplies named lowered runtime/intrinsic semantics and memory operations, avoiding separate dialect semantics. Runtime helper identity/signature must be exported. |
| Designated initializer preservation | Could disable `LOWER_DESIGNATED_INITIALIZERS` in an experimental profile. | Typed constant/member mapping can be mechanically emitted, but sparse/range/nonconstant cases still need a proved adapter. No present reason to diverge from direct profile. |
| GNU `ifunc` | Keep `LOWER_IFUNC=1` in primary, record caveat. | Stock lowering approximates load-time resolution with first-use wrapper resolution; upstream documents timing/address-identity caveats (`doc/source/lower_il.rst:3355–3416`). Exact native loader semantics require a distinct target obligation, not an invisible equivalence claim. |
| Coroutines | Preserve stock partially lowered coroutine records if enabled, but mark hard residual execution requirement. | `lower_coroutine` lowers implicit variables/selected expressions; `enk_await`/`enk_yield` remain with resolved protocol calls (`src/lower_il.c:20157–20195`, `17293–17299`, `20426–20432`). Frame layout, suspension/resumption, parameter/temporary lifetime and destroy behavior are not a trivial C# `async` mapping. |

Coroutines default disabled when lowering is on because upstream says lowering is incomplete (`src/host_envir.h:3877–3889`). The broad primary configuration may explicitly set `COROUTINE_ENABLING_POSSIBLE=1` so the writer preserves all stock residual records instead of rejecting valid coroutine input. That is **semantic serialization support**, not proof of ready-to-emit C# execution or full direct-backend parity. If a residual requires a field that stock lowering destroyed, retain an unlowered compatibility artifact for that input/stage and report the hard mismatch; do not add the direct descriptor patch. The implementation must probe ordinary/symmetric-transfer/exception/parameter-copy and destruction cases before claiming residual completeness. The upstream warning cannot be waived by a public API abstraction.

## Full-language coverage is a profile matrix

One combination cannot enable incompatible stock modes. Every object records its stage and actual build/runtime profile. The writer/reader must cover retained node families for each supported stage; a compatibility-stage artifact cannot masquerade as the primary execution stage. The future C# translator is out of scope and must explicitly reject or implement hard residuals.

- **Primary native lowering:** Itanium ABI plus settings above; independent TUs; upstream accepted C++03/11/14/17/20/23/26 modes and GNU/MS native language modes supported by that ABI. Latest C++ mode is explicit, not inferred from EDG version. C++26 requires nonclass prototype parsing (`src/cmd_line.c:12388–12396`). Keep GNU/MS source capability flags enabled, but record the chosen runtime dialect.
- **Coroutines:** Explicit enabling overrides the lowering-dependent default; export residual protocol and its known limitations as described above. Do not promise a generated frame machine.
- **Microsoft managed CLI/CX:** Stock lowering cannot handle this as the primary stage. `ALLOW_CPPCLI_AND_CPPCX_WITH_LOWERING` merely allows the build and then suppresses lowering **and the backend** for CLI/CX inputs (`src/host_envir.h:694–705`). Therefore use a separate source-semantic profile with `DO_IL_LOWERING=0`, `DOING_SOURCE_ANALYSIS=1`, CLI/CX capabilities enabled, local-entity promotion and lowering-dependent flags disabled. This preserves managed semantic input but leaves a substantial separate implementation task downstream. Native Windows metadata importer or upstream portable-assembly reader must be available (`src/CMakeLists.txt:239–247`; `src/host_envir.c:6374–6376`).
- **Microsoft native ABI-only extensions:** `TARG_MICROSOFT_PTR_TO_MEMBER_SIZING` conflicts with lowering (`src/targ_def.h:2767–2774`). Source-analysis mode also permits some virtual-base member-pointer casts that the lowering ABI cannot represent (`2776–2794`). Use the source-semantic compatibility profile for accepted cases requiring those capabilities; do not claim Itanium lowering gives Microsoft binary behavior.
- **Legacy cfront:** A separate cfront ABI lowering profile may retain stock-supported legacy behavior. Assignment-to-this is incompatible with primary Itanium lowering; in a source-semantic compatibility profile it requires both constructor/destructor allocation-folding macros (`src/targ_def.h:3006–3051`). Export class-associated allocation targets for folded operations. This is a real profile difference, not a missing visitor silently omitted.
- **Legacy exported templates:** `EXPORT_ENABLING_POSSIBLE=1`, automatic-instantiation/prelinker support enabled, everywhere-used mode disabled, ABI compatibility >=232. Preserve upstream definition-discovery/import workflow and resulting primary IL. Its operational integration remains an acceptance gate; current writer/reader do not link objects (`src/targ_def.h:404–423`; `src/host_envir.h:880–885`).

The source-semantic compatibility profile sets both stock backend flags false, uses the same new writer entrypoint, sets `DO_IL_LOWERING=0`, `PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE=0`, `MINIMAL_INLINING=0`, `LOWER_VARIABLE_LENGTH_ARRAYS=0`, `LOWER_COMPLEX=0`, `LOWER_FIXED_POINT=0`, `LOWER_CLASS_RVALUE_ADJUST=0`, `LOWER_IFUNC=0`, and `RECORD_BACKING_EXPRS_WITH_IL_LOWERING=0`. It may enable source sequences. Its object stage states that constructors/lifetimes/allocation remain higher-level; this is not a hidden change to the main model's executable meaning.

## Source retention and memory boundary

Keep the six ordinary provenance switches enabled for independent-TU profiles: `EXTRA_SOURCE_POSITIONS_IN_IL`, `RECORD_MACROS_IN_IL`, `FULLY_RESOLVED_MACRO_POSITIONS`, `RECORD_MACRO_INVOCATIONS`, `MACRO_INVOCATION_TREE_IN_IL`, `RECORD_MACRO_ARGS`. This preserves available macro/position evidence, not a source tree that lowering erased. Set `COMPILE_MULTIPLE_TRANSLATION_UNITS=0`: it conflicts with macro-tree retention (`src/host_envir.h:387–390`).

Legacy exported templates can import secondary TUs even with that macro false (`src/trans_unit.c:752–770`). This compile-time check alone does not prove macro-context correctness after copying. Until a probe establishes it, use a compatibility provenance profile with macro-tree/invocation/argument retention disabled, keeping definitions and available original positions. Record the optional provenance difference; never drop semantic execution to save provenance work.

Stock invocation is TU processing → `fe_wrapup()` → `back_end()` → `fe_wrapup_part_2()` (`src/cfe.c:125–180`). `fe_wrapup` has already freed front-end-only storage (`src/fe_wrapup.c:842–854`), and lowering-private descriptors were freed earlier. Do not follow `source_corresp.assoc_info` symbols, parser caches or other front-end-only fields. File/routine IL lives until post-backend cleanup (`src/fe_wrapup.c:865–885`). Assign database identities while pointers are valid and emit foreign keys, never addresses. Include orphan local types/static variables and generated routines/tables, not only original declaration lists. `scope_for_routine`/`mem_region_for_routine` are the stock traversal pattern (`src/c_gen_be.c:10360–10382`).

A later file-backed IL profile may load/unload routine regions following `src/c_gen_be.c:10522–10623,12212–12233`; then identities must withstand address reuse. The initial resident profile avoids this complication. No pointers survive backend return. No routine-at-a-time compiler callback is assumed; streaming SQLite writes during the final traversal do not require one.

Optional exact source content is an external attachment with scalar identity, encoding, hash and acquisition provenance. No source BLOB/encoded byte dump goes in `.roslynobj`. Post-compilation rereads are not claimed to be the exact compiled bytes. Full tokens/trivia cannot be reconstructed from surviving positions or canonical macro text.

## Build and output integration

Add `roslyn_gen_be.c` implementing `void back_end(void)` in the EDG namespace, a separate coherent CMake target/macro configuration, and SQLite linkage. Reuse upstream source-set/build mechanisms (`src/CMakeLists.txt:205–216,251–256`; `src/cmake/cpfe/CMakeLists.txt:7–17`). Do not share compiled EDG objects across conflicting macro profiles: conditional IL layouts differ. The stock `.c` files compile as C++ (`src/CMakeLists.txt:169–174`); place SQLite in its own C-compatible target or link the library rather than accidentally applying `-x c++` to its amalgamation. Exporting IL needs no EDG target runtime library build (`CMakeLists.txt:99–114`), although emitted runtime-helper references remain semantic dependencies for a future translator.

Stock `-o` is local preprocessing/IL output handling and errors if unused (`src/cmd_line.c:11030–11033,12989–13000,13047–13052`). With resident IL and no core CLI edit, derive `<input>.roslynobj` or let the new backend read `ROSLYNOBJ_OUTPUT`; an external wrapper can map its own `-o` to this variable. Do not use `--front_end` or `--write_unlowered_il` to select the stage: they suppress the backend (`src/cmd_line.c:10663–10670,10703–10715`).

Start with one-TU CLI execution. Open a temporary SQLite file inside `back_end`, write in a transaction, resolve required references, commit/close and atomically publish. Close/rollback before reporting an exporter failure through an EDG routine that may escape. `fe_cleanup` has no generic custom-backend cleanup callback (`src/fe_wrapup.c:890–925`); callable integration needs a host wrapper that invokes an explicit new-backend cleanup function after abnormal EDG return. Do not assume C++ unwinding: GCC/Clang builds use `-fno-exceptions` (`src/CMakeLists.txt:159–167`).

Implementation acceptance must first prove stock partial-EH tables suffice without any descriptor patch, all retained nominal claims have valid post-lowering fields, runtime/helper identities are explicit, and each enabled residual family is serialized completely. Tests exercise C++-heavy examples through the same configured EDG pipeline, plus independent profile probes. This is compiler-adapter validation, not a new C++ semantic analyzer or an already implemented C# translator.

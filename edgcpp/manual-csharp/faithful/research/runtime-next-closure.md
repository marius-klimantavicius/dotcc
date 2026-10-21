# Runtime next closure — source-shaped dispatch frontier / final HOLD

Authority and restrictions match initializer-width-r1.md. This report was
saved early at the supervisor's explicit request so documentation cannot block
serialized leaf validation. Ownership was explicitly released after the gate;
the final dispatch frontier and current receipt/macro facts below complete this
report. No child build/test/Git/native preparation/subdelegation. This is a
source-defined implementation proposal, not a claim that the entire transitive
runtime graph has been closed or that any routine is qualified.

## The actual roots

const_ints_init -> init_int_kind_min_max_values ->
get_integer_size_and_alignment (switch default: unexpected_condition_str) or
get_bit_precise_integer_size_and_alignment (check_assertion(width != 0)), then
the existing value arithmetic closure. Neither size helper allocates a_type or
a_constant. Thus initialization does NOT require a replacement semantic graph.
target_early_init -> set_target_configuration additionally reaches alloc_general
for a named target auxiliary directory and CHECKING failures for bad indices or
null setters. target_one_time_init -> check_target_configuration reaches
internal_error on incompatible target/representation configurations.

The actual const_ints type/range callers add skip_typerefs, real a_type union
fields and bit-precise integer supplements, and live int_kind_is_signed;
in_range_for_integer_kind/constant wrappers add real a_constant discriminants,
type pointers, integer-value union storage and min_con == max_con identity.
CHECKING adds assertion_failed. Preserve exact a_type/a_constant/source
correspondence layouts and all enabled alternatives before these callers can be
qualified. Do not invent an integer-only replacement graph to get past them.

## Direct allocator/text/error closure (enabled failures are edges)

| Source routine | Exact outgoing dependencies / branch obligations |
| --- | --- |
| mem_manage.c malloc_with_check (216) | size > PTRDIFF_MAX -> insufficient_address_space; malloc NULL -> catastrophe(ec_out_of_memory); DEBUG tracking excluded only by actual DEBUG=0 |
| realloc_with_check (298) | old_ptr NULL -> malloc_with_check; else realloc then catastrophe on NULL; do not add malloc's size guard to the other arm |
| alloc_general (1072) | malloc_with_check; TRACK_GENERAL_ALLOCATION map if enabled; DEBUG counters if enabled |
| free_general (1092) | ptr NULL asserts size==0; otherwise conditional map/set accounting, then free; no arbitrary managed ownership policy |
| alloc_resizable_buffer (1127), realloc_buffer (1144) | alloc_general/realloc_with_check; NULL and non-NULL arms; enabled map/set mutations preserve order |
| alloc_text_buffer (1672) | general allocation of a_text_buffer, writes allocated_size/increment/size, then resizable allocation; preserves identity and reallocation lifetime |
| expand_text_buffer (1702) | length > allocated_size; integer round-up by allocation_increment, realloc_buffer, double increment, update allocated_size in source order |
| reset_text_buffer (1693), remove_null_terminator_from_text_buffer (1782) | no callees; zero size; size>0 short circuit followed by final-byte check/decrement |
| add_to_text_buffer (1740), f_add_string_to_text_buffer (1757) | ensure_text_buffer_space -> expand_text_buffer, memcpy, size write; strlen in wrapper |
| set_buffer_position (1726), truncate_text_buffer_to (1771) | pointer bounds/size assertions -> assertion_failed; source native pointer difference and size semantics |
| checking.h check_assertion/_str/_str2, unexpected_condition variants | real test and exact filename/line/function/string payload -> assertion_failed; no DEBUG.Assert/no-op/Unsupported adaptation |
| error.c assertion_failed (3353) | start_of_file_name; a_number_buffer append/as_temp_characters; alloc_text_buffer; add_string/add_char buffer macros; strlen; internal_error |
| internal_error (3328) | recursive internal_error_loop arm -> error_text, fprintf, term_compilation; otherwise set loop flag, general_diagnostic, exit_compilation fallback |
| insufficient_address_space (3311) | catastrophe(ec_insufficient_address_space) |
| catastrophe (7378) -> pos_st_catastrophe (7254) | pos_st_diagnostic -> general_diagnostic; retained exit_compilation(es_internal_error) fallback |
| general_diagnostic (6199) | create_primary_diagnostic; ordered conditional string/symbol/type/template/position fills; wrap_up_diagnostic or detail::append_to_diag_list |
| create_primary_diagnostic (5099) | severity/in_front_end selects general vs frontend region, then create_diagnostic_entry |
| create_diagnostic_entry (5008 region) | alloc_diagnostic; primary position/source info and macro-origin conversion; submessage kind/list links; unexpected default |
| alloc_diagnostic (947) | conditional free-list reuse; alloc_general_or_in_region_of_type; all source lists, position, discriminants and fill-in pointers initialized |
| wrap_up_diagnostic (5659) | check_severity; once/prototype suppression; diagnostic counters and active template mutation; output control/buffers; catastrophe recursion guard; optional context; construct_message; end_of_diagnostic_actions; free_diagnostic |
| end_of_diagnostic_actions (4961) | conditional cancel_il_file; finish_raw_listing_file; term_compilation for catastrophic severities/error limit; error_text/stream output |
| host_envir.c term_compilation (2587 region) | write_signoff then exit_compilation |
| exit_compilation (2528) | output-mode messages; severity switch -> cfe_exit or fflush+abort according to EXIT_ON_INTERNAL_ERROR |
| cfe_exit (2506) | !MAKE_FRONT_END_CALLABLE -> exit; callable branch has explicit setjmp/longjmp state and termination_in_progress handling; preserve selected branch |

The full CHECKING path is therefore not dependency-closed by porting three
assertion wrappers and throwing a new exception. General diagnostics use real
diagnostic/position/source/template state even for early errors. The table is an
inspected direct-call frontier, NOT a complete recursive ledger for formatting,
source mapping, template-context and cleanup routines beyond it. That frontier
is the concrete remaining research dependency for the supervisor; qualification
must not erase those enabled edges.

## Source data and shim boundaries

a_text_buffer is mem_manage.h:652–664: allocated_size, size,
allocation_increment (sizeof_t), char *buffer, in that order. Allocate the
structure in general storage and preserve aliases to it; reallocation changes
the payload pointer and can invalidate earlier interior pointers. Do not use a
managed StringBuilder whose capacity, alias and lifetime differ.

General allocation tracking is conditional on MAKE_FRONT_END_CALLABLE ||
EXPENSIVE_CHECKING. cpfe commands/generated header supply neither macro;
basics.h defaults callable to false when EDG_MAIN was not provided, and
EXPENSIVE_CHECKING to false. Do not confuse high-level defines.h preset branches
with the USE_CMAKE_DEFINES branch. CHECKING=1 alone does not enable tracking.
Effective macro receipt remains supervisor-owned. mem_manage_init still creates
detail::freed_fe_map and detail::reusable_fe_list: those separate frontend reuse
structures cannot be discarded based on general tracking being disabled.

a_number_buffer is util.h:5216, Small_string<50>, not a guessed printf buffer.
Port its actually used append/temporary-character and number-format closure,
including allocation on growth, rather than inventing diagnostic formatting.

start_of_file_name -> end_of_directory_name -> strcmp/strlen/mbc_strrchr.
The latter is the multibyte scanning routine when that feature is compiled,
not always libc strrchr. end_of_directory_name preserves stdin pseudo-name,
slash, alternative-backslash and windows_paths_allowed drive-prefix branches;
it returns an interior pointer into the original input storage.

Host shims must preserve malloc/realloc/free failure (including zero-size host
behavior), pointer alignment, memcpy non-overlap preconditions, byte strings and
NULs, stderr/FILE identity, output order/flush, errno/locale where actually used,
and exit/abort statuses. No production native frontend calls are authorized.
Do not turn source abort into an invented recoverable exception protocol.

## Next actual port ownership, disjoint proposed files

1. Runtime allocation/text owner: source-grouped mem_manage.cs and
   mem_manage.types.cs, exact routines above, a_text_buffer and referenced
   macros/types. First deliver complete source-shaped bodies as dependency-
   blocked, explicitly naming actual diagnostic dependencies. No new integer
   algorithms. A standalone closed first subset is reset_text_buffer and
   remove_null_terminator_from_text_buffer plus exact a_text_buffer layout;
   this small subset is runtime progress only, not an initializer closure.
2. Checking/error owner: checking.cs, error.cs, error.types.cs; preserve all
   assertion payloads, general/internal diagnostic state, error-table identity,
   and the table's entire error frontier. A source-complete fatal diagnostic
   batch includes the real callees of general_diagnostic, not a special early
   error substitute. Request full source diagnostic structs/globals from the
   source-layout owner. Keep every unclosed method dependency-blocked.
3. Host owner: host_envir.cs and host_envir.shims.cs; path/multibyte helper
   closure, default_error_output_file, cfe_exit/exit_compilation/term_compilation
   and write_signoff closure; no independent error policy. Source-layout owner
   alone owns shared native type definitions; no duplicate partial graphs.

Then integrate the union of these closures with original target configuration
and const_ints_init. This is the first real initialization milestone after
INIT-001 evidence, not a license to continue indefinitely adding integer leaves.

For the subsequent real type/allocation batch, use original alloc_type,
alloc_constant, integer_type/bit_precise_integer_type and their source callees,
not fabricated wrappers. alloc_in_region/alloc_primary_file_scope pulls actual
region headers, tables, trimming/reuse, allocation sequence state and lifetimes.
mem_manage.c:alloc_mem_block includes the enabled PCH path:
precompiled_header_processing_required -> page alignment -> alloc_new_mem_block
-> open_memory_region_tmp_file/map_memory_region_file -> allocation history;
the other branch uses malloc_with_check. USE_MMAP_FOR_MEMORY_REGIONS defaults
true for the non-DOS frontend profile in host_envir.h. It cannot be silently
disabled just because initial inputs lack PCH files. Region initialization and
cleanup therefore need their own full call ledger and source-layout batch.

## Concrete open work at the initial temporary HOLD

The complete recursive diagnostic formatter/source/context/cleanup closure is
not yet enumerated; neither are all region/IL allocator routines or the actual
type/constant constructors. This report explicitly does not call that larger
set dependency-closed. Supervisor can dispatch the disjoint actual runtime
owners now, with closure completion as their first obligation, while this task
continues read-only inspection during the validation gate. No current research
item authorizes reduced semantic structs, disabled features, error stubs, native
fallback, or qualification of a caller with unresolved enabled failures.

The temporary write-HOLD was observed until explicit supervisor release. The
completed dispatch boundary below supersedes its scheduling status.

## Initially inspected source snapshot (historical tool bytes)

SHA-256, exact whole files; source review only, not execution or full routine acceptance.

```text
568c80d00f1887a37a076c35bd8123fd1de1910cbecf73f18151d25bfb86d609  edgcpp/build/compile_commands.json
35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f  edgcpp/build/src/includes/cmake_defines.h
7457e4f82e18fb9b1fe543d1420d56e7748ed2393583503316e9a39e4980d264  edgcpp/manual-csharp/faithful/tools/prepare_integer_oracle.py
65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9  edgcpp/upstream/src/basics.h
85ad71d47918c6686cf44099915ec0e9544b52c8d5c702cf31a4d72a2b9aa140  edgcpp/upstream/src/cfe.c
27aa5d201622e328f9b543af1e5b664c76777a19aa51b3364b2502dfd1cb874c  edgcpp/upstream/src/checking.h
5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c  edgcpp/upstream/src/const_ints.c
23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f  edgcpp/upstream/src/const_ints.h
4112aa9ba2c076dd9b83877bdfae6f107bd12952b6a12776baa6942ad114f18e  edgcpp/upstream/src/defines.h
e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60  edgcpp/upstream/src/error.c
6611a39baaf1de999aefa446966935878c7786d6fe142240526e830bda32d3e2  edgcpp/upstream/src/error.h
7224d6888ef3bf52e37019f8554a609dc4521b3cf3685612d4c5f948a31e827f  edgcpp/upstream/src/fe_init.c
7089dc53eb8fca9f3c55ed299850914d39a8fdd533937e8809d210a5f6459f2c  edgcpp/upstream/src/host_envir.c
a8c9c5e79dbdd6be8c82f360c58d8681f91015e5e14e61039076b7f8c3b9c2c1  edgcpp/upstream/src/host_envir.h
5d2e1ee19def818fc0012c7a862072c2149f3dfc382b032647863a9b0bb43fa6  edgcpp/upstream/src/il_def.h
4127523bae47ab55d89ed8ed99c067f70081bd44ee7aee3dfa5ea9c643b38305  edgcpp/upstream/src/mem_manage.c
4ca1801049d9ad90b47536dd4c097a0623b589ece80352c2153e28c23a8b50b6  edgcpp/upstream/src/mem_manage.h
59e87adb1b47d456609fbd71c5ae09c33e6cb61f446d803b028c94f910034d15  edgcpp/upstream/src/mem_tables.h
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  edgcpp/upstream/src/targ_def.h
c9566f5e831009c33946f1a25277dd376606b2a9bc60a21e48011472fa398b67  edgcpp/upstream/src/target.c
b1bdf56f2fa73f1f3fdb62f5bcf296c55a39f4350ad9d80467c6253a2365330e  edgcpp/upstream/src/target.h
63af3263fe35ac20336506353707f889d7c867f8cee749e9ed9bc966273ae639  edgcpp/upstream/src/target_cfg.h
60d76a8e67fe309e406ba32cf1e20d1e4014d0a81556086d055fec02db644990  edgcpp/upstream/src/target_map.h
72693d1744a5c0b16e388f3ac5f474baca3a63bdd3064d572822282bfdc73771  edgcpp/upstream/src/trans_unit.c
dfdac880faf2620d6098b570c46021b1bc56bdeac6a0d0ba0ba52a69b404306f  edgcpp/upstream/src/util.h
```

Manifest SHA-256 (LF-terminated lines above): `e31c1973f615201faaba50ae2c1ffe8cb19950ea5d1c801b27fd038f65e20b9f`.

## Final dispatch proposal: actual bodies, explicit dependency boundaries

The supervisor's fresh cpfe-derived macro dump now confirms CHECKING=1,
DEBUG=0, EXPENSIVE_CHECKING=FALSE, MAKE_FRONT_END_CALLABLE=FALSE,
MULTIPLE_THREAD_COMPILATION=FALSE, STANDALONE_UTILITY_PROGRAM=FALSE,
USE_MMAP_FOR_MEMORY_REGIONS=TRUE, EXIT_ON_INTERNAL_ERROR=FALSE,
HOST_ALIGNMENT_REQUIRED=8, HOST_ALLOCATION_INCREMENT=65536,
IL_SHOULD_BE_WRITTEN_TO_FILE=1, ORPHAN_PROCESSING_NEEDED=TRUE,
MULTIBYTE_CHARS_IN_SOURCE_SUPPORTED=TRUE, UNICODE_SOURCE_SUPPORTED=1,
NATIVE_MULTIBYTE_CHARS_SUPPORTED_WITH_UNICODE=FALSE,
USE_OWN_SJIS_MULTIBYTE_CHAR_PROCESSING=FALSE and
EDG_MULTIBYTE_CHAR_TEST_MODE=FALSE. This closes the earlier textual uncertainty
for the const_ints.c include/profile. General allocation tracking is therefore
false; CHECKING failures, Unicode path scanning, source tables, orphan metadata,
PCH/region branches and abort behavior remain actual enabled dependencies.

Dispatch these three disjoint source-owned batches immediately. They may return
complete source-shaped bodies with explicit dependency-blocked status; do not
hold all implementation until recursive frontend closure is complete. No
unported callee may acquire a fake executable body to make a project build.

| Owner | Proposed exclusive files under faithful/src/EdgFaithful | Concrete first bodies and completion boundary |
| --- | --- | --- |
| R-MEM, general allocation and diagnostic text | mem_manage.cs, mem_manage.types.cs; own matching mem_manage ledger files | malloc_with_check, realloc_with_check, alloc_general, free_general, alloc_resizable_buffer, realloc_buffer; alloc/reset/expand/set-position/add/f-add/truncate/remove-terminator text-buffer routines and their actual macros; a_text_buffer. Body-complete but dependency-blocked on R-ERROR failures until that closure exists. Do not implement region/PCH allocation in this first owned file slice. |
| R-ERROR, actual checking/diagnostic machinery | checking.cs, error.cs, error.types.cs, util.cs; own matching checking/error/util ledgers | CHECKING macro counterparts; error_text, assertion_failed, internal_error, insufficient_address_space, catastrophe, pos_st_catastrophe, pos_st_diagnostic; real general_diagnostic/create/allocate/fill/list/free/wrap routines from the table; actual error early/one-time/per-TU/per-compilation initialization. util.cs is restricted to exact required original a_number_buffer/Allocated_string/Dyn_array/allocator and formatter counterparts. Full recursive formatter/context/source mapping remains dependency-blocked and listed, never replaced. |
| R-HOST, path/stream and termination boundary | host_envir.cs, host_envir.types.cs, host_envir.shims.cs; own matching host ledgers | an_error_severity, mbc_strrchr, f_mbc_length, end_of_directory_name, start_of_file_name, default_error_output_file, write_text_signoff, write_sarif_signoff, write_signoff, cfe_exit, exit_compilation, term_compilation. Real stream/byte-string/exit/abort shims preserve source contract. Signoff depends on R-ERROR error_text/counters and existing format_file_name closure; do not hide it. |

Common-type ownership is explicit: R-MEM alone defines a_text_buffer; R-HOST
alone defines the original byte an_error_severity; R-ERROR alone defines
error.h/error.c diagnostic structures and counters. Existing IntegerRepresentation
remains with its established owner. a_source_position, a_type, a_constant,
a_translation_unit, source-correspondence/IL prefix and every full native graph
layout need a separately assigned source-layout owner; none of these three
workers may introduce a partial alternate semantic graph or duplicate layout.
If the source layout is unavailable, the corresponding body remains dependency-
blocked rather than manufacturing a structural substitute.

Each implementation worker owns its source-unit routine ledger and returns exact
native ranges/hashes, all branch/compile alternatives, fields/globals and call
edges, produced bytes and HOLD. Supervisor alone owns project inclusion, tests,
Git and integration. A fresh independent reviewer opens exact original/C# bytes;
this research report supplies no review acceptance. The three batches should
converge on target/configuration + const_ints_init; INIT-001 stays open and
blocks initializer qualification even if all value leaves are accepted.

### A genuinely closed, useful runtime sub-batch

The pinned-profile host path chain is dependency-closed without diagnostics or
IL allocation: start_of_file_name -> end_of_directory_name -> mbc_strrchr ->
mbc_length_simple/increment_mbc_ptr -> f_mbc_length, plus ordinary byte strlen/
strcmp. In the measured Unicode/non-native-multibyte profile f_mbc_length's
active branches use only pointer/byte reads, comparisons, loops and optional
err writes; the native-character arm returns Latin-1 length 1. The SJIS, EDG
multibyte test, non-Unicode mblen and native locale APIs are actually compiled
out in this profile. Preserve their alternative source mapping explicitly.
This is an actual runtime foundation for assertion_failed and later source
paths; it is not an invented parser or a claim to initialize the frontend.

The full active branch ledger for that host chain is:
- f_mbc_length: optional *err=FALSE before scanning; !is_native then ASCII <=127,
  else ordered 2/3/4-byte lead tests and short-circuit continuation checks;
  invalid lead/continuation sets local_err, optionally writes err=TRUE, sets
  len=1 and consumes continuation bytes; is_native arm sets len=1; return len.
  Preserve the source's actual validation/recovery, including what it does not
  reject. Do not replace this routine with a stricter framework UTF-8 decoder.
- mbc_strrchr: result=NULL; scan from original pointer while non-NUL; compare
  current byte with chr and update the original-storage pointer; advance using
  native multibyte length. It does not match an independent terminating NUL.
- end_of_directory_name: stdin pseudo-name -> NULL; otherwise slash search,
  conditional backslash search and ordered pointer selection; conditional
  windows_paths_allowed and no slash and length>=2 and byte[1]==':' -> drive
  prefix. VMS directory conventions are an inactive alternative here.
- start_of_file_name: call helper, then NULL ? original pointer : result+1.
  Preserve interior pointer identity and caller-owned storage lifetime.

Also closed independently: a_text_buffer layout + reset_text_buffer and
remove_null_terminator_from_text_buffer; source diagnostic counter constructors
and all_error_types; is_catastrophic_error_severity; source a_diag_list clear/
is_empty operations using its explicitly opaque diagnostic pointer. These
are runtime fragments, not substitutes for the fatal or region closures.
update_diagnostic_counter is NOT in that closed subset: its es_none/default
branch invokes unexpected_condition_str.

### Remaining direct edges made explicit

The initial table's body summaries expand as follows, so workers do not mistake
an omitted helper for a dependency waiver:

- alloc_diagnostic -> clear_source_info_for_pos and clear_diag_list as well as
  general/region allocation; alloc_diag_fill_in -> real discriminated union
  initialization, including reflection and module fields and dfk_last failure.
  free_diagnostic and free_diag_list_elements are mutually dependent; the former
  does not reclaim/reuse general-memory diagnostics. For region diagnostics it
  recursively releases four sublists, splices fill-ins into the availability
  list and pushes the diagnostic onto its free list. List head/tail and order
  are observable storage state, not replaceable by structural equality.
- create_diagnostic_entry -> check_for_overridden_severity,
  conv_seq_to_file_and_line, alloc_diagnostic, actual source/translation-unit
  state and unexpected_condition. Primary source-location conversion occurs
  even for these general-memory fatal diagnostics; do not omit source mapping.
  The macro-origin branch performs its second location conversion before the
  appropriate stored fields are replaced. The original full routine includes
  subordinate-list linkage and must remain intact.
- general_diagnostic has all nine ordered optional payload calls (two strings,
  two symbols, two types, two template lists, other position), then issue vs
  append. add_string_fill_in's lengthless overload calls strlen, its length
  overload calls alloc_diag_fill_in then add_fill_in_to_diagnostic. It retains
  the original pointer plus length; it does not clone caller bytes. Each
  symbol/type/template/position helper and its actual recursion is an enabled
  dependency for the full routine, even if the current fatal caller passes NULL.
- wrap_up_diagnostic's direct frontier is check_severity,
  diagnostic_already_issued_for_diag_once, diagnostic_already_issued_for_prototype,
  determine_reported_severity, update_diagnostic_counter,
  str_command_line_error, is_catastrophic_error_severity,
  set_up_output_control_block, alloc/reset_text_buffer, error_text, fprintf,
  term_compilation, add_error_context, construct_message,
  end_of_diagnostic_actions and free_diagnostic, plus live scope/template state
  writes. This frontier is explicit; the recursive closure of those helpers is
  still OPEN. The catastrophic-context default is true, so it cannot be erased
  as an irrelevant failure branch.
- assertion_failed's a_number_buffer is Small_string<50> =
  Allocated_string<Delegate_buffered_allocator<50, General_allocator>::Meta>.
  append checks/pops the old NUL, calls actual custom reserve/format machinery,
  and grows by max(capacity*2,min_new_size) when required. General_allocator's
  replace_alloc performs alloc_general, move_elements, free_general; it is NOT
  mem_manage.c realloc_buffer. Preserve template-specialization source mapping,
  actual capacity/lifetime and the emitted line/string formatting.
- error_early_init -> reset_f_error/default_error_output_file and memzero, and
  initializes internal loop flags, buffers, counters, thresholds, error-code
  defaults and annotation state. error_one_time_init conditionally registers
  actual saved PCH variables. error_trans_unit_init allocates/constructs the
  actual pragma stack. error_init resets counters/free lists/cache and error
  codes in source order; it calls clear_file_index_list. Include these state
  protocols before claiming early or repeated initialization.
- term_compilation -> write_signoff -> output_mode text/SARIF switch ->
  write_text_signoff or write_sarif_signoff, retaining unexpected defaults per
  CHECK_SWITCH_DEFAULT_UNEXPECTED. Text signoff uses diagnostic counters,
  error_text, source filename formatting and byte stream writes. Then
  exit_compilation preserves selected text termination messages and severity
  mapping. Under measured cpfe MAKE_FRONT_END_CALLABLE=FALSE, cfe_exit calls
  native exit; internal error under EXIT_ON_INTERNAL_ERROR=FALSE flushes f_error
  and aborts. A caller-return/exception policy would be a semantic divergence.

### Real type/arena coupling, not a partial replacement graph

Original integer_type (il.c:12066–12092) asserts a non-bit-precise kind, reuses
int_types[kind] or stores the new alloc_type result in that canonical slot
BEFORE setting the kind/size, then performs enabled orphan recording and
record_builtin_type. bit_precise_integer_type (12095–12118) always allocates a
new type, sets signedness/explicitly_signed/bit_width, computes size and records
an orphan; it does not use the same int_types canonical cache or builtin call.
Preserving these differing identity/call protocols is required.

alloc_type -> alloc_il -> do_fs_alloc/alloc_in_region and clear_type ->
set_default_source_corresp, clear_type_cached_flags, set_type_kind. The latter
retains its entire enabled switch, including integer, class, routine, typeref,
template and vector alternatives and corresponding supplement allocation;
it is not an integer constructor with Unsupported alternatives.
alloc_constant -> alloc_cil -> do_any_alloc and clear_constant ->
set_default_source_corresp and set_constant_kind. The latter reaches integer
setters, fixed-point initialization, enabled complex allocation, template-kind
initialization and all actual union arms. Thus this next type/constant batch is
not closed solely by the existing integer leaves.

set_default_source_corresp copies the actual default correspondence and then
conditionally reads module_entity_stack and writes the current module entity.
do_fs_alloc preserves file-scope prefix padding, secondary-TU copy-address
storage, orphan pointer storage, clear_il_entry_prefix, region metadata and the
final pointer advance. do_alloc preserves its own non-file-scope prefix and
current translation-unit metadata. A bare native allocation of sizeof(a_type)
would erase these identity/lifetime/source protocols.

The region expansion frontier remains: mem_manage_early_init,
mem_manage_one_time_init (PCH/TU registration), mem_manage_init (real front-end
reuse maps), ensure_mem_region_table_space (region, IL scope, enabled IL-file
index parallel arrays), init_memory_region_without_initial_allocation,
init_memory_region, alloc_in_region, do_host_alignment, alloc_mem_block,
trim_mem_block, free/reusable-block operations and enabled alloc_new_mem_block
PCH/mmap/history/host-file closure. The exact recursively closed region batch
has NOT been established by this report and must not be advertised as complete.

Final report state: concrete source-shaped next batch and direct frontier are
specified; recursive general_diagnostic/format/source/context and region/type
allocation closures remain OPEN. Full startup observation remains a supervisor
dependency. No task input asks the user for a semantic exception. Final HOLD
follows the exact native/evidence manifests below.

## Exact researched source ranges

All rows are source-researched / implementation-unstarted in this task; no C#
acceptance. Hashes retain original bytes and line endings, inclusive one-based
ranges. Startup/dispatch summaries above describe relevant branches and edges;
full implementation ledgers and original/C# review remain worker obligations.

| Entry | Native source range | SHA-256 |
| --- | --- | --- |
| find_target_configuration | target.c:267–284 | `ff6cfe28a73d162cc609584fb71ff31692b4f9544244dbe33dbb5dec15fce8ca` |
| set_target_configuration | target.c:287–329 | `f9d8aa86f2943084befeadf26a7cd8608339568ad62332c7103adf66fa613a76` |
| check_target_configuration | target.c:482–711 | `31766a8bc394fdccfc2fab453722c313e7972a84f34b62effdccb3d545a071ea` |
| target_early_init | target.c:933–1030 | `66bce48b88ebaccf9c2212667014aa565298711ec75291ccf209444c7c8bdf66` |
| target_one_time_init | target.c:1033–1063 | `031990dc89b3ad54cae390eed019e42b70ca5425ef88b545a37e179ef245e34b` |
| target_init | target.c:1066–1137 | `f795c7210b0e7331c93ff303ee900673c3f4e3a1d46dff30336453852cc2da3b` |
| make_integer_value_mask | const_ints.c:829–850 | `11aaa3e8db571d9a89c273ce683afefa2da6b79f2bfcf75efb9190d42e533877` |
| sign_extend_integer_value | const_ints.c:853–865 | `5c320ebeded1145e8df74252dfb1773da955485d6ba650aed751f230e1fbf7cc` |
| shift_left_integer_value | const_ints.c:921–977 | `8b8c950dbf5b7d205c1cf2f3c785555efb41bf967dbf2789cdbde2955eb2d9d6` |
| shift_right_integer_value | const_ints.c:980–1029 | `c87053cc3bef929a3d03d7c42c64731ffdd844b674e37e5b196bcb9ad7b18faf` |
| get_integer_size_and_alignment | const_ints.c:2426–2479 | `b75944da32c291ed7cf3913f5dc1668b1ce8b555301856a1450faaded5207a0c` |
| get_bit_precise_integer_size_and_alignment | const_ints.c:2482–2522 | `e550ba043ee8b7a968378bc1dbb0e2cad22eb2c6d87a8d1ef200cb8cbbebd050` |
| init_int_kind_min_max_values | const_ints.c:2681–2721 | `8b05e1a62eb765528be51e790cbd9b0a1d09ff4a50786c36e2240e20c03b1528` |
| const_ints_init | const_ints.c:2724–2736 | `c758e486cf6f453b65737d606ba81c79f08fb285de32482759e26f61552c44da` |
| malloc_with_check | mem_manage.c:216–249 | `fcad2df4cd699bbf6ade4f04cee1db40e648d9f011c207d56762587d9f4239eb` |
| realloc_with_check | mem_manage.c:298–333 | `30c3341286ce95465f90cb64c33ea8dbba439ba2744027f523bfed815b1eabaa` |
| alloc_general | mem_manage.c:1072–1089 | `71f80e9c78efc600b7305df9c3f32df59e7fe382d9b63021a98f3f999f343ae7` |
| free_general | mem_manage.c:1092–1124 | `78a1f1546b41f5137ceafcc897fc7bbf323d77ae49af49d45f469c8a3b4a07e7` |
| alloc_resizable_buffer | mem_manage.c:1127–1141 | `c23892299e23807a8e91e909ea0f58ac8210aeb80882c76a91ba8ed66677cea4` |
| realloc_buffer | mem_manage.c:1144–1181 | `a0f833f6326e16bb9f3011e8860050642cf2e30e202653eddadc5e09bee900b0` |
| alloc_text_buffer | mem_manage.c:1672–1690 | `226cc9c726e2a2da73d4a69915926c3869c68d058276000718bdbe180be922ac` |
| reset_text_buffer | mem_manage.c:1693–1699 | `d0103dce44ca17fb4b00e657ed473a36e8ce56be04371f1a33f4c694634f0b94` |
| expand_text_buffer | mem_manage.c:1702–1723 | `6a04d851008c97fa74cf630abbc41caa377215861a7eab6ec9f45c8767157d20` |
| set_buffer_position | mem_manage.c:1726–1737 | `1e896918f45ee0a3e56c21c32c99037d94d52331308dd3ac44414a3a7acd2278` |
| add_to_text_buffer | mem_manage.c:1740–1754 | `4bb959cbb31d18030eeb612f7302aab21dffe676fb8627183910a9e3ad2146e5` |
| f_add_string_to_text_buffer | mem_manage.c:1757–1768 | `5cdc32ada8f547c29ef91c8900e28d3f9ed9015ef9683f430f3047b9840cf8d1` |
| truncate_text_buffer_to | mem_manage.c:1771–1779 | `9c6c55cb6a8ccbf61338e99df01ffb8b26f585a1c8eb68ce1d9299aeb504c42e` |
| remove_null_terminator_from_text_buffer | mem_manage.c:1782–1792 | `5e7507ebd3aa3ee66920af6dc557a45ea234335cac9cfcd39283fcb273bbd144` |
| a_text_buffer | mem_manage.h:652–664 | `e4cb7a33492f1495d33d846c6e69804fe9e2d7c6d82bd464b3999c473455d71c` |
| ensure_text_buffer_space | mem_manage.h:763–767 | `b94bb73326e54a31d8c52f9799da0e1d62eb691a81324199338b9d18c3d2bad4` |
| add_char_to_text_buffer | mem_manage.h:775–779 | `62e716fdedc00769a939a3b4f8faf793a0a405d6df53962aeb7497502168cc76` |
| error_text | error.c:917–931 | `deace65fde9c47d0ae59be55caf5d00d6d68aae0779604d878dcb339db36a98a` |
| alloc_diagnostic | error.c:947–985 | `28c0140cf7dace738f9c0e5a311a13d8ee7f84fd02aca1162baec35942c79761` |
| free_diag_list_elements | error.c:992–1004 | `cb8e908597fde270062eee1277863e44a250e7591e61106aca4fcda0d8d3f220` |
| free_diagnostic | error.c:1007–1028 | `0ae82970be48b575063a14ea25f33b554f5baecf5524f924d5ac93d71deee106` |
| alloc_diag_fill_in | error.c:1031–1101 | `2eb48eb97354fb6726df4f0a0804a2b96b6a0b6c660ab65163cc17c585a173da` |
| determine_reported_severity | error.c:2958–2970 | `e529cfc32b74de79cb41cee473908193191ed9765349638e40b351bb864f5949` |
| update_diagnostic_counter | error.c:3065–3095 | `2bdea8bc63ae36daaf7c6044c2f89ec0bd7df4c02f0f7cdbb71bd2d800cd322e` |
| insufficient_address_space | error.c:3311–3318 | `c5290b20a875aca58a02560c14c60a71e5ef2dc78be906639d1152990005e1aa` |
| internal_error | error.c:3328–3350 | `d3c5dc9459d9f6b2c139a9dd9ff310abc61b6675267b97a0ed4bf8494d600ec3` |
| assertion_failed | error.c:3353–3404 | `32bd06978513eb74f1c345bf74dfb34ba8dff218d5675cd85eed5e684ba3f392` |
| is_catastrophic_error_severity | error.c:4940–4958 | `9d0a30b5a55fecc9a7642b7306ccae8f5e932e96fed9b376b531b89ebe61d05b` |
| end_of_diagnostic_actions | error.c:4961–5000 | `dbbec394a23ab560f40743af6666e005964136f3c89d8effe87a6c2ffafe1f6d` |
| create_diagnostic_entry | error.c:5003–5095 | `cb1ccc23f1cb2038cc272b3d04b39eb3d7ffd71d27eead0a384b8c986e361d0d` |
| create_primary_diagnostic | error.c:5099–5126 | `132e25edeae873c7623418ae272dcaad75a8c589ef47c93da74dbdfd7596bf2f` |
| add_fill_in_to_diagnostic | error.c:5159–5173 | `9c85b61355b3db6407a9abe1ead0a4fc80be12aa10b0e051d49774dc07485884` |
| add_string_fill_in length | error.c:5219–5233 | `c95eb6af114923ca8c7b4162431af6f9128e51e7b0992af0629b1fbf64451c37` |
| add_string_fill_in NUL | error.c:5236–5243 | `f78a3098475858e103d61588eb5b8f6ba4a20bd478e12eeffd87bada7342312c` |
| wrap_up_diagnostic | error.c:5659–5788 | `78dd5fbddebcc273f6392e33ec797bd2278e472e01fa3bce665b0ee843841f30` |
| general_diagnostic | error.c:6199–6238 | `3b39dee46000107c3775abb99d656685ee60c405854dd358f70a25bc6ba807fa` |
| pos_st_catastrophe | error.c:7254–7265 | `4616476457a8290654076e533dee20cfcbea55b6b1a121f7466afb7b3953c491` |
| catastrophe | error.c:7378–7385 | `c0d72e6857ee799c478901f2ec994924d531a569862de67155b16b65e2d8d1ac` |
| reset_f_error | error.c:8343–8354 | `8d37d85c54e262ae51447a0c504bcd27534908ddbc2ecb0341aa3832d22bb307` |
| error_early_init | error.c:8357–8415 | `f4fed27e3498f52ef50f18662915f0f9e9f5be4e47fc263151b4adb0d527c0b3` |
| error_one_time_init | error.c:8418–8447 | `ca29b8ea454fbe658cdba68cfe65c775e264bfcd95ce51823607121779d1708b` |
| error_init | error.c:8464–8508 | `e0e147bea4b1b17659b2e8bd6a959df87f94057309a6b3493f1f8987d92e435b` |
| mbc_strrchr | host_envir.c:292–307 | `82e733400b27eec743e007c473df97e1f0938647455753aee91b378590c00339` |
| end_of_directory_name | host_envir.c:784–823 | `64c8f3ebfea903ac7f76d86b1f9680ce36cfbeceae7248078c0a59da8669dcf4` |
| start_of_file_name | host_envir.c:826–839 | `3c45826208d518b793f001c3adafb57cba4f1eb06771029966508454d04d5f91` |
| write_signoff | host_envir.c:2479–2495 | `b3fdcbb4c28792566388c9af28ea8a2a6e02310c71f7e32b09ff40b2b4652237` |
| cfe_exit | host_envir.c:2506–2525 | `9b642e4bcf4645dbb09c804f7eb945045d403e4563aaa8031e55c856f71806c0` |
| exit_compilation | host_envir.c:2528–2572 | `b2072ebbbef94d69b609800b10beed0dcf8e0a8f13464c1877f676133b2a2103` |
| term_compilation | host_envir.c:2587–2598 | `a47a54c5ca4f2244052be7d79fd8f95b8adf42230ad2c5124eb2d9a9bcad7bff` |
| f_mbc_length | host_envir.c:4543–4694 | `3e69081a61c02dcf2be09fd47c809405eca8d558f8fd3c6578be78ca98ba08a7` |
| default_error_output_file | host_envir.c:6646–6658 | `7cc74e8529b5716a11ffacfdbbf2533994a17f6436c17bd68375db2081d306f1` |
| an_error_severity | host_envir.h:3732–3745 | `b31d6a1a0b342b0f7e0cd22207d13459a50933e92eca30670aa355f06551fa2e` |
| integer_type | il.c:12066–12092 | `1ac14cf9e1b8f686c3589903dc3d0114e4ca20463a75b57ccf11d4bc43f931db` |
| bit_precise_integer_type | il.c:12095–12118 | `201067980aba25bed640e4cc76fcc8f6e53fe5a9550c9bba527dc49757ce7f78` |
| set_default_source_corresp | il_alloc.c:125–140 | `90ba1d051eb700c2f2c7d2df902f0e1546a3ac62ca3a83cc008bf67da6215780` |
| set_constant_kind | il_alloc.c:784–912 | `608d3a2f0036f8639369ce4d892772871cd5f1ce5a4cdef4116178dc5203c9a4` |
| clear_constant | il_alloc.c:915–986 | `18773cb2fcf36454b3e1ee1bb1b1a7d65fb8c04850de1c8a4e674658efeb302b` |
| alloc_constant | il_alloc.c:989–1004 | `83c4c9f18b555ee2f2a1bd3791e4d053ceb72f220d3d5de652b10259ce8455be` |
| alloc_integer_type_supplement | il_alloc.c:1433–1464 | `7f92c2ab06f87fe72d6904ef206ec0be3f5acae147818a67a9cf2ea4dd94824f` |
| set_type_kind | il_alloc.c:1908–2239 | `cf4568cccda8629c1de774f81bd1386815a5feafb1bb41c13f2f44a9da235841` |
| clear_type | il_alloc.c:2253–2343 | `4ebed3bdd31441312273b654709fd8458215d42b7df86fd234988c7436942e88` |
| alloc_type | il_alloc.c:2346–2360 | `fd09bac77ce733a10f2aea51e23ee13790ced67244287a04e16ba26877ae4711` |

## Final current source/tool/evidence snapshot

The earlier manifest retains its historical inspection; these are current
bytes after supervisor gate and explicit research ownership release. Raw
initial failures and all supervisor receipts remain preserved.

```text
568c80d00f1887a37a076c35bd8123fd1de1910cbecf73f18151d25bfb86d609  edgcpp/build/compile_commands.json
35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f  edgcpp/build/src/includes/cmake_defines.h
08daa3060ada394fc5fd9e923da52fd8151a149c9ab52003d3ad8308befecd0d  edgcpp/build/src/includes/err_codes.h
95bfae78554e4921117b1ac833ce030f669b5d3c05cdeb9dfd1386cadf71af05  edgcpp/build/src/includes/err_data.h
0c27506bc36d734e71b7d13322a880d209addca8326e7414a2b715b932aa239c  edgcpp/manual-csharp/faithful/artifacts/initializer-probe-r1/initializer-probe
fed24e443d8577ab3f62275d1112fc3891f3b396c6948e1bdd2e6ae9c86a8d68  edgcpp/manual-csharp/faithful/artifacts/initializer-ubsan-r1/const_ints.o
28fb69a0d586fd4c07fdb26746c5c2062ef523cc98a73358a3c1f08ff75b9d85  edgcpp/manual-csharp/faithful/artifacts/integer-r3/header-abi.log
603760d85b6703c3cc66e6dd2f5144e8bc64359de572a9cd0699c26c79a850bd  edgcpp/manual-csharp/faithful/artifacts/integer-r3/native-abi.log
0d2e2c02b449b104410fd1f4e9c0346c3202b96a2a313cdae9f46f064b5b874a  edgcpp/manual-csharp/faithful/baseline/integer_abi_probe.cpp
0bca45789cede1c59d7ea505788c34ba999fbb7ea404361056e96ac038892d05  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-aot-arithmetic.json
5ee5a9e2b6c7e5ebb4feddade6ec19fc8123172463607af240ba8031620eb2de  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-aot-leaves.json
6b650b92d54c596fa8a4b89e8a284e95dfb159032587ac9fbf71182f1795f8cc  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-jit-arithmetic.json
a284d2b0345fd7acf8dc4ef6ca7b2f09ddb8f563e5956511141aaab642175663  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/diff-jit-leaves.json
6934cc24bed9936957843343f90499c75f9161db43029183578635de3903934a  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/initializer-ubsan-prepare-r1.json
43e7da00075bb5422337f79f7f756030ba3cc7d48d18a2dbe2877f74e1aece96  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/initializer-ubsan-probe-r1.json
51eeca0e0be6eb16cd5fb2cd2889321fee88dff02460ed41f5fe69429c30246d  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/integer-r3-gate-receipt.json
87362eb346c8676da4ae464243efcc7a3c00a0c61fdc540b47b336261a50d310  edgcpp/manual-csharp/faithful/evidence/integer-first-gate/native-effective-macros.txt
fe067fa053022fac7067433f799dcb35758036cbcc0930fcb73b8dd041d9bc79  edgcpp/manual-csharp/faithful/research/initializer-width-probe.cpp
93d32d4bc0ef2c01f09651e4ef1d99d9ce76dc20821accea7c00f61a9bcf4cda  edgcpp/manual-csharp/faithful/tools/prepare_integer_oracle.py
65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9  edgcpp/upstream/src/basics.h
85ad71d47918c6686cf44099915ec0e9544b52c8d5c702cf31a4d72a2b9aa140  edgcpp/upstream/src/cfe.c
27aa5d201622e328f9b543af1e5b664c76777a19aa51b3364b2502dfd1cb874c  edgcpp/upstream/src/checking.h
77ce3949dffb0fd85a8bca85a33ae590fc2c5b7b7f200b47ff672c35cca8bded  edgcpp/upstream/src/cmd_line.c
5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c  edgcpp/upstream/src/const_ints.c
23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f  edgcpp/upstream/src/const_ints.h
4112aa9ba2c076dd9b83877bdfae6f107bd12952b6a12776baa6942ad114f18e  edgcpp/upstream/src/defines.h
477909c104df131c14a5b50dfbd263d5f03441e5e2079594090e27dac6a29cdc  edgcpp/upstream/src/direct_allocator.h
e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60  edgcpp/upstream/src/error.c
6611a39baaf1de999aefa446966935878c7786d6fe142240526e830bda32d3e2  edgcpp/upstream/src/error.h
7224d6888ef3bf52e37019f8554a609dc4521b3cf3685612d4c5f948a31e827f  edgcpp/upstream/src/fe_init.c
7089dc53eb8fca9f3c55ed299850914d39a8fdd533937e8809d210a5f6459f2c  edgcpp/upstream/src/host_envir.c
a8c9c5e79dbdd6be8c82f360c58d8681f91015e5e14e61039076b7f8c3b9c2c1  edgcpp/upstream/src/host_envir.h
535768a37a2bd0f90dd18c41bd867398074289a305c3ff88589da67153514cd0  edgcpp/upstream/src/il.c
6a2125d77c052ee8a98c04d5cb3b6b51ba6abc9220314d273af499c83e334a88  edgcpp/upstream/src/il.h
aa12a01de83a70158cf2fe377630b850e668e66e81a43fc0136a4334524818fd  edgcpp/upstream/src/il_alloc.c
f2671903e3fb7c900bd5723cbbd9a36af5797b546ac033b94908b78e579e030b  edgcpp/upstream/src/il_alloc.h
5d2e1ee19def818fc0012c7a862072c2149f3dfc382b032647863a9b0bb43fa6  edgcpp/upstream/src/il_def.h
4127523bae47ab55d89ed8ed99c067f70081bd44ee7aee3dfa5ea9c643b38305  edgcpp/upstream/src/mem_manage.c
4ca1801049d9ad90b47536dd4c097a0623b589ece80352c2153e28c23a8b50b6  edgcpp/upstream/src/mem_manage.h
59e87adb1b47d456609fbd71c5ae09c33e6cb61f446d803b028c94f910034d15  edgcpp/upstream/src/mem_tables.h
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  edgcpp/upstream/src/targ_def.h
c9566f5e831009c33946f1a25277dd376606b2a9bc60a21e48011472fa398b67  edgcpp/upstream/src/target.c
b1bdf56f2fa73f1f3fdb62f5bcf296c55a39f4350ad9d80467c6253a2365330e  edgcpp/upstream/src/target.h
63af3263fe35ac20336506353707f889d7c867f8cee749e9ed9bc966273ae639  edgcpp/upstream/src/target_cfg.h
60d76a8e67fe309e406ba32cf1e20d1e4014d0a81556086d055fec02db644990  edgcpp/upstream/src/target_map.h
72693d1744a5c0b16e388f3ac5f474baca3a63bdd3064d572822282bfdc73771  edgcpp/upstream/src/trans_unit.c
7bc5d8c9b472a2d72c724b628f384a49fa726f3c8d7406fedaf9107596375495  edgcpp/upstream/src/types.h
dfdac880faf2620d6098b570c46021b1bc56bdeac6a0d0ba0ba52a69b404306f  edgcpp/upstream/src/util.h
```

Manifest SHA-256 (exact LF-terminated lines inside the fence): `437282eec1fa425392358ad900e7dcd54ed23dac503e782c906ec785f1a20e57`.

**HOLD.** No more writes remain to any owned file. No child compiler/test/native
preparation/Git execution, upstream change, production C# change, or
subdelegation occurred. Probe bytes were unchanged after supervisor execution.
Only the supervisor can release further writes or run additional validation.

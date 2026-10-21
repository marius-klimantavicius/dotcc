# Original frontend driver path and next source closures

This is a manually inspected driver slice, not a complete frontend call graph.
All source hashes are in `../baseline/local-files.sha256`. Approximate routine
hash/range records are separately in `routine-candidates.jsonl`; they do not
replace opening original code. No managed counterpart was reviewed here.

`cfe.c:220–240` defines `EDG_MAIN`. Its `MAKE_FRONT_END_CALLABLE` alternative
wraps `cfe_main` in a `setjmp` protocol, runs `fe_cleanup`, and returns the
saved exit status. The other alternative directly calls `cfe_main`. Do not
replace this with a new managed exception/lifetime contract without reviewing
the actual selected configuration and source protocol.

`cfe.c:65–214`, `cfe_main`, orders: initialize error output; `fe_early_init`;
timer; `proc_command_line`; `fe_one_time_init`; per-compilation `trans_unit_init`;
`process_translation_unit`; optional module write; `fe_wrapup`; conditional
backend; `fe_wrapup_part_2`; diagnostic severity selection; optional next file;
`exit_compilation`. Timing/diagnostic/output state and conditional alternatives
are source operations, not incidental cleanup that may be rearranged.

`fe_init.c:1752–1793`, `fe_early_init`, includes enabled CHECKING
`check_int_type_config` before memory and host initialization; then memory,
host, target, command line, errors, PCH, modules, conditional IL writing,
scope state, conditional near/far state, translation units, types and conditional
locale initialization. `NDEBUG` does not authorize suppressing CHECKING. The
full one-time and per-translation-unit init chains remain inventory obligations.

`trans_unit.c:746–880`, `process_translation_unit`, saves active translation
unit state; sets positions; sets primary/export flags; computes prefixes;
conditionally initializes part 1; allocates and links the translation unit;
pushes its stack; restores exported-file include/macro data; initializes unit
state; branches between preprocessing and normal parse/PCH; wraps up; pops;
conditionally processes secondary units. Allocation identity, list order,
saved state, and exported-unit include/macro aliases must remain original.

`decls.c:22846–22918`, `translation_unit`, performs PCH/preinclude handling,
toggles `next_token_is_top_level_decl_start` around initial `get_token`, handles
empty input diagnostics (C++ versus C), calls `scan_namespace_declaration_list`,
checks pending header stop, resets conditional source-sequence insertion state,
and performs end-of-source pragma handling. `declaration` is at 22798–22843;
`scan_namespace_declaration_list` at 14283–14324. These are the original parser
entry paths; a small replacement grammar is not a valid next batch.

`preproc.c:5383–5412`, `cpp_driver`, sets `fetch_pp_tokens` and expansion policy,
handles macro preincludes, repeatedly calls `get_token` to end of source,
and flushes modified final-line preprocessing/raw-listing output conditionally.
`lexical.c:17382–18958`, `get_token`, is a large interleaved lexical/preprocessor
path with source, macros, include, cache/rescan, diagnostics and recovery
dependencies. Its local body alone is not a dependency closure.

## First dependency-closed source batch

Under the selected multipart representation, the scalar-free integer storage
and arithmetic closure in `const_ints-initial-closure.json` requires only:
the exact native-width storage types/configuration; source macros
`signed_shift_right`, `sign_of`, `INT_VALUE_PART_BASE`, `get_part`, `clear_parts`,
`copy_parts`; and the listed local arithmetic routines. It does not need a
replacement frontend graph, allocator, formatter, IL type graph, diagnostics,
floating-point library, or native runtime fallback. Closure claims apply to the
listed active routine/macro bodies with their source preconditions, not to all
of `const_ints.c`.

Source order in the file is not a dependency-free implementation order:
`incr_integer_value` calls add; mask calls complement and right shift; sign
extension calls both shifts; negate calls subtract; mixed arithmetic calls
comparison; multiply calls negate and part-copy/clear; private divide calls
compare, negate, normalization helpers and ordered array work; public remainder
also has its own `-1` special case. Preserve those original routine names and
source grouping in C# and ledger, even when the work is split into batches.

The next integer extension is IL-aware wrappers and target configuration:
`int_constant_is_signed`, constant comparisons/range/width/trimming,
`get_integer_size_and_alignment`, bit-precise widths and min/max table init.
These require original `a_constant`, `a_type`, typeref/variant/discriminants,
`an_integer_kind`, `int_kind_is_signed`, target sizes/alignments and checking/
diagnostic dependencies. They are not closed by adding stub types/tables.
Initialization-width research is separately assigned and left to that worker.

Formatting and float conversion form a further closure: original
`String_formatter` specializations, dynamic array/string buffer lifetime,
internal division, `snprintf_impl`, checking, `float_pt.c`/`floating.c`
conversion routines and their native rounding/storage algorithm. Retain both
fast host extraction and multipart string fallback; do not substitute platform
formatters or mathematically cleaner arithmetic.

## Disjoint source/runtime work available to the supervisor

Small original leaf contracts can be assigned independently after routine-level
mapping, rather than inventing replacement utility semantics:

- `host_envir.c:2764–2780`, `smemcmp`: sequential unsigned-byte comparison,
  early mismatch stop, zero-length behavior, exact signed-int difference. No
  callee/global state/allocation. Do not replace with a vectorized comparison
  that reads ahead; the original comment explicitly forbids extra reads.
- `host_envir.c:4985–5018`, `unicode_to_utf8`: four ordered magnitude branches,
  writes only returned byte count, host `unsigned long` input. It does not
  reject surrogate or out-of-range code points; adding Unicode validation
  would change the native routine's semantics. No callees/global state.
- `host_envir.c:5151–5178`, `extract_character_from_string`: original byte
  traversal for each `targ_little_endian` alternative and shift by
  `targ_char_bit`; depends on pinned runtime target globals and unsigned-long
  arithmetic, not on the integer-value storage module.
- `floating.c:75–93`, `match`, and 118–133, `parse_infinity`: source-bounded
  matching and original decimal-input discriminant updates; include the original
  `an_fp_decimal_input`/float tags and exact immutable infinity strings. The
  separate `parse_nan` adds `strchr`'s native buffer/read contract. Do not claim
  this closes floating conversion or replace its arithmetic elsewhere.
- `error.c:2958–2970`, `determine_reported_severity`: severity promotion through
  original enum order, `dp->severity`, and `error_promotion_threshold`. This
  closes only this predicate and requires original layout/state. It does not
  implement the general diagnostics or recovery pipeline.

Allocator/arena, filesystem/input, and full diagnostics have larger failure/
cleanup/identity closures. Their native shims can be mapped in disjoint files,
but allocating arbitrary managed objects, suppressing assertion failure,
throwing generic exceptions or using convenient new diagnostic IDs does not
close them. `upstream/lib_src` is a separately pinned target runtime; it must
not be confused with the frontend host allocator/lifetime implementation.

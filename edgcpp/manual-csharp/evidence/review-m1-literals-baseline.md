# Independent remaining-M1 literal/builtin review baseline

Date: 2026-10-21. Reviewer: `/root/manual_edg_supervisor/review_m3`.
Source/design inspection only; **no implementation acceptance**. Prior bounded
preprocessing checkpoint is committed as `abb239b20`. The full PLAN continues.
Reviewer owns only `evidence/review-m1-literals-*.md`, with no implementation
edits, builds/tests/native processes or Git mutations.

This batch targets target-aware character literal values/types and PP character
operands, deterministic configurable date/time/hosted macros and configured
`__has_include`. Broader encoding/literals/filesystem and M2–M6 remain active.

## Actual source branches independently inspected

- `literals.c:conv_single_char` 1238+ returns an unsigned target character entity,
  recognizes escapes using explicit entity masks and conversion state, and
  distinguishes narrow/UTF/wide paths. Numeric hex consumes all following hex
  digits; octal consumes at most three. Range checking 1700+ uses an unsigned
  mask even for signed char. Unknown escapes warn and translate to the escaped
  character; numeric range errors use discretionary error outside GNU modes.
  Unicode/UCN, multibyte input and surrogate conversion are separate branches,
  not ordinary CLR-char conversion.
- `conv_char_literal` 1820+ chooses C++ ordinary single `char`, ordinary multiple
  `int`, `L` wchar_t, `u` char16_t, `U` char32_t and C++20 `u8` char8_t. Too many
  narrow entities beyond target sizeof(int) are errors in the selected strict
  branch. Prefix legality and representability remain target/dialect-sensitive.
- Packing 2059+ is target-controlled. For most-significant-first order, the
  FIRST signed character is sign-extended before accumulating later entities.
  Thus unsigned byte accumulation followed only by an int-width cast can differ
  for a short multichar literal beginning with a high-bit entity. Native values
  must witness the actual selected target, including `\377a` and `a\377`.
- `macro.c:scan_has_include` 5507+ parses header spelling, leaves header escapes
  unprocessed, and calls `header_can_be_found`. `lexical.c:header_can_be_found`
  7873+ shares `resolve_header` with diagnostics suppressed. `macro.c:6378+`
  rejects __has_include outside controlling-expression context in strict modern
  C++ while permitting nested macro rescan within that context.
- `macro.c:init_date_and_time_macros` 10943+ creates English `Mmm dd yyyy` and
  `hh:mm:ss`; single-digit days have a leading SPACE. These macros cannot be
  redefined. `11880+` creates __STDC_HOSTED__ from explicit configuration.

These working files retain the source hashes already audited in the preceding
checkpoint. New consulted branches must appear in the batch correspondence;
whole-file, Unicode, GNU, Microsoft or older-dialect implementation is not
established by these routine references.

## Concrete acceptance and design risks forwarded

1. Reuse one target-aware character decoder for actual language graph literals
   and PP operands, followed by each context's correct promotions. Expose actual
   literal type/value and conversion nodes; optional folded arithmetic alone is
   not literal semantics. Native witness single signed high-bit char, ordinary
   multichar packing, ASCII/control escapes, maximal hex/three-digit octal,
   empty/malformed/overlong/range errors and selected unknown-escape severity.
2. Prefix encodings need distinct semantic types and target widths. Unsupported
   prefix/Unicode paths must be explicit, never accepted as CLR char/int aliases.
   Preserve inactive/unused preprocessing-token laziness and raw/stringize/paste
   spelling origins. Extend constexpr and overload tests to decoded character
   operands, not only standalone parser tests.
3. A header-presence probe must not activate/process the file, execute its macros
   or #line/once, or mutate include activation/physical-source inventory. Factor
   shared ordered search rather than calling the existing loading ResolveInclude
   as if a probe were a real inclusion. No implicit filesystem/environment search.
   Retain physical including-file context through nested macro/argument expansion;
   presumed #line names must not redirect quoted search.
4. Test quoted/angle and macro-formed probe operands, existing/empty/missing
   headers, physical root '/', quote/system precedence, defined/ifdef visibility,
   probe then real include, inactive branches and syntax/restriction diagnostics.
   Operator expansion in dead arithmetic branches follows actual native behavior;
   short-circuit arithmetic must not silently skip required preprocessing syntax.
5. Date/time/hosted configuration must be immutable and deterministic, with a
   documented timezone/default. No ambient clock/culture/environment inference.
   Assert space-padded single-digit dates, boundaries, stable repeated analyses
   and distinct sessions. Generic configured macro collisions and source
   define/undef restrictions need deliberate tested behavior.
6. Builtin output retains configuration and invocation contributors/history;
   query facts survive disposal. New state/cache/probe behavior must isolate
   repeated and concurrent sessions. The accepted changed-stream difficult M4
   gate and prior rollback/ownership suites must keep passing.

Later review must inspect actual produced code, discriminating assertions,
exact-byte native witnesses and matching held managed/NativeAOT receipts before
any bounded acceptance. This baseline does not close an inventory row.

Policy correction from root during implementation: the authorized default is
the actual UTC translation time captured once per CompilationSession, with an
explicit immutable override for reproducible runs. This supersedes the earlier
deterministic-default/no-ambient-clock suggestion above. Review must ensure no
clock read per expansion/analysis, stable session Options and retained values,
separate session snapshots and explicit override behavior. It is a deliberate
session-level adaptation, not a reason to reject the authorized implementation.

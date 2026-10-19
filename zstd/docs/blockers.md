# Zstandard blocker ledger

Updated 2026-10-19. No active implementation blocker; final clean reproduction remains.

## ZSTD-C001 — aggregate member initializer lowering

- Classification: compiler, object emission, portable profile.
- Reproducer: `DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/probe.sh --tools build --fetch never --hashes off --jobs 4`.
- Current-tool evidence: `artifacts/campaign/20261019-195558-92624953/receipt.json`
  and `011-zstd_compress.log`. The recipe selected all 30 upstream C units and
  the authored ABI bridge. Unit `lib/compress/zstd_compress.c` exits 2 with
  `dotcc does not yet support: invalid initializer for aggregate member`.
- Native expectation: the identical complete selected core builds and passes
  native controls (`artifacts/native-baseline/native-report.json`).
- Initial diagnostic attempt reused pre-existing compiler binaries:
  `artifacts/campaign/20261019-195448-ae2cd766/receipt.json`. It emitted 13
  objects before the first error/cancellation. Its unsupported `-U` warning was
  caused by stale binaries; rebuilding current tools removed the warning and
  reproduced the initializer error. No product was published.
- Reduction: multidimensional arrays of structs were interpreted as a flat
  list of struct-field groups; upstream `ZSTD_defaultCParameters[4][23]` exposes
  the lost row shape. Four focused regressions failed before the repair.
- Shared fix: typed array lowering retains row dimensions and per-row zero
  initialization. Native-backed global/local/static and 3D execution fixtures
  pass; focused/adjacent tests pass. Review additionally requires preserving the
  previously supported local aggregate-copy initializer form; that regression
  passes in the committed repair.
- Full-source retry: `artifacts/campaign/20261019-195801-a865be3d/receipt.json`
  passes the original unit and reaches ZSTD-C002 in divsufsort. Required
  algorithms and upstream tables remain selected; no generated C# repair.

## ZSTD-C002 — GNU inline keyword alias

- Classification: compiler lexer/parser, portable profile, unit 027
  `lib/dictBuilder/divsufsort.c`.
- Reproducer: complete current-tool translation retry in
  `artifacts/campaign/20261019-195801-a865be3d/receipt.json`, log
  `027-divsufsort.log`.
- Observation: upstream `#define INLINE __inline` expands `static INLINE` to the
  GNU `__inline` spelling at line 153; parsing fails. Upstream source is valid
  under the native portable profile and remains unchanged.
- Regression/shared fix: reserved GNU aliases are promoted after macro expansion
  by the existing `DialectKeywordRewriter` table. Explicit macro overrides retain
  precedence. `inline-before.trx` records four failing alias cases and one passing
  macro-precedence control; the combined repaired tests pass (see validation).
  No grammar mutation or parser-table regeneration was needed.
- Full selected-source retry: all 31 units emit and link in
  `artifacts/campaign/20261019-200202-07ddc0d9/receipt.json`; raw build reaches
  ZSTD-C003. Dictionary training and suffix sorting remain selected.

## ZSTD-C003 — relational result cast to C enum

- Classification: compiler typed C# emission/build.
- Reproducer: complete translation in
  `artifacts/campaign/20261019-200202-07ddc0d9/receipt.json`, `raw-build.log`.
- Observation: all selected units emit and link, but raw C# build reports CS0030
  converting generated `CBool` to `ZSTD_longOffset_e` in `Zstd.00008.cs`.
- Expected: C relational results have integer conversion semantics, including
  explicit casts to enum types; native source build passes.
- Shared fix normalizes rendered C boolean values through an explicit integer
  conversion at enum casts and stores. Before-state TRX contains eight unit
  failures; the functional fixture has seven CS0030 diagnostics. Repaired
  focused tests: 13 pass, plus two execution fixtures pass. Native output agrees
  exactly, including one evaluation of a comma-expression side effect.
- Full-source retry `artifacts/campaign/20261019-200450-e4faef65/receipt.json`
  emits/links all 31 units, builds and publishes both canonical forms.
  Generated files are not repaired manually. Affected downstream checks pass;
  the shared fixes are committed as `fdd42a910`.

## ZSTD-C004 — preprocessor stringification inserts punctuation spaces

- Classification: compiler preprocessing; actual raw JIT ABI/version test.
- Reproducer: `artifacts/campaign/20261019-200625-94aeae63/receipt.json`.
  `ZSTD_versionString()` returns `1 . 5 . 7` while native returns `1.5.7`.
  Numeric version and actual stream struct layout/storage agree.
- Expected: standard macro stringification collapses actual whitespace runs;
  it does not invent spaces between adjacent punctuation/expanded tokens.
- Structural repair: retain leading/trailing preprocessing whitespace metadata
  through mapped Items and macro substitutions; stringification consults those
  boundaries instead of unrelated replacement-token locations. The native
  trailing-empty-macro case (`x +y`) is retained through nested/forward/prefix
  expansions. No punctuation-specific rewriting is used.
- Final focused/source-mapping/boundary tests: 91 pass; two functional fixtures
  pass. Native exact output, initial failures and full command lists are retained
  in `artifacts/compiler-regressions/shared-fix-evidence.json`.
- Full current-tool 31-unit retry:
  `artifacts/campaign/20261019-201437-73006882/receipt.json` passes raw/processed
  and final canonical builds. Published `ZSTD_versionString` now contains the
  exact `1.5.7` literal. Actual ABI/version passes in all four cells in receipt
  `20261019-203037-026bc24a`; downstream gates pass. Closed in `fdd42a910`.

For each observed failure record ID, stage/classification, exact reproducer and
tool identity, native expected/observed behavior, regression, shared structural
fix and the next full-source retry. Preserve superseded failures and distinguish
unrelated repository baseline failures.

## ZSTD-H001 — unbounded qualification collector

- Classification: authored lifecycle-test scheduling, not translated codec.
- Receipt `20261019-202322-a76910b4` stalled in an unbounded background full-GC
  loop. The exact owned command group was terminated; intervention evidence is
  retained. No unrelated process was stopped.
- Bound the collector to 32 iterations with a short scheduling yield. Targeted
  raw-JIT receipt `20261019-203002-1bb78126` passes all seven lifecycle cases in
  0.564 s; all four lifecycle cells also pass in `20261019-203037-026bc24a`.
- Closed; no generated-source or compiler change.

## ZSTD-H002 — upstream test input header identity

- Classification: test-only object/header closure; closed.
- Receipt `20261019-203037-026bc24a` passes all four main codec/API matrices but
  upstream raw-build fails on anonymous typedef identity. The private upstream
  test TU used acquired reference headers while reused core objects used staged
  copies. The test must use the exact same staged closure before a shared type
  defect can be inferred.
- Repair: emit the test TU and exact copied upstream data generator against the
  same staged lib headers as the complete reused core objects. Record all 31
  core object paths, header root and data-generator hash. Targeted raw-JIT
  receipt `20261019-203529-8adc43fa` passes all 33 upstream cases and deliberate
  failure control. No compiler, generated-C# or upstream-code patch was needed.

## ZSTD-H003 — native weak-tracing profile drift

- Classification: native-control feature auto-detection, not a shared ABI defect.
- The original upstream case prints `ZSTD_sizeof_CCtx` at level 1. It reported
  native 582680 versus translated 582672 bytes; this print has no original
  equality assertion, so the 33 original passing assertions remain valid.
- GNU ELF native compilation auto-enabled optional weak-symbol tracing and its
  8-byte `traceCtx`; the translated portable frontend selected tracing off.
  Explicitly pin `ZSTD_TRACE=0` in the shared JSON profile for both builds and
  document the optional host-hook exclusion. No required codec algorithm changes.
- Closure requires fresh complete translation/native controls, actual printed
  size agreement and the final four-cell matrix with that explicit profile.
- Source/binary classification evidence:
  `artifacts/native-profile-trace-20261019/classification.json`. Both fresh
  native controls in receipt `20261019-204241-00db2fbc` have zero trace symbols
  and actual context-struct size 5272 versus the old 5280-byte native value.
  The supplementary executed context-size comparison is distinct from the
  33 original upstream cases and must also pass every final matrix cell.
- Closed: final receipt `20261019-204241-00db2fbc` passes all four upstream
  cells with native/emitted size 582672 and zero skipped cases, plus the complete
  API/consumer/import/rooted-AOT matrix. No shared compiler repair was needed.

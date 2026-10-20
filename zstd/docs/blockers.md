# Zstandard blocker ledger

Updated 2026-10-19. All observed campaign blockers are closed. Final selected-scope
qualification and clean reproduction pass; no required gate remains incomplete.

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
  33 original upstream cases and passes every final matrix cell.
- Closed: final receipt `20261019-204241-00db2fbc` passes all four upstream
  cells with native/emitted size 582672 and zero skipped cases, plus the complete
  API/consumer/import/rooted-AOT matrix. No shared compiler repair was needed.


## Optimization follow-up — resolved test/provenance findings

- The first primitive native fixture build (`20261020-052220`) exposed GCC's
  omission of an addressable xxhash endian helper and builtin macro aliases for
  byte swaps. The test-only fixture includes system headers first, then selects
  the same portable upstream helper definitions as translated C. Delivered
  feature flags and upstream sources are unchanged. Final native and all eight
  original/replaced form/mode fixtures match 227486 checks and digest
  `9b3f85183c30d739`, with a real inaccessible-page LE24 guard control.
- Historical whole-header hashes initially acted as an unconditional helper
  compatibility gate. They now follow ordinary off/warn/explicit-strict
  provenance policy; actual typed signatures, physical declaration/TU identity,
  exact reports and functional tests establish compatibility. Unrelated header
  comment probes emit all 42 replacements in off/warn; strict rejects drift.
- Exact copied delivery requires a relative authored adapter link and evaluates
  every Compile/ProjectReference inside the copied source tree. The final
  47-file closure builds and runs both consumer forms without external source
  references; hashes remain unchanged. The audit's adapter-list variable is
  distinct from its per-file source variable, including explicit Compile items.
- The first paired timing preceded final regeneration. A strict proof allowing
  only bijective internal unit-suffix renaming failed because staging identity
  also changes generated member ordering and owner aliases. The proof was not
  weakened. Final timing reruns the same original/candidate harness against the
  exact final published products; all 116 comparisons pass and their full hashes
  remain bound to the qualification receipt.

These findings required no shared compiler repair, upstream algorithm change or
manual generated-source patch. Historical failed receipts remain available;
the current selected optimized scope has no open product blocker.


## Direct-intrinsic follow-up — shared feature and context repairs

- The accepted adapter implementation is runtime-correct, but direct calls still
  invoke selected C wrappers. Eight new actual-caller source assertions fail
  before direct lowering; the managedMethod isolation control passes. Intrinsic
  target metadata is now bound to the resolved symbol and consulted at emission,
  preserving late selection, shadowing, indirect wrappers and object contracts.
- ZSTD-C005: new native-backed conversion coverage exposes an existing enum
  constant narrowing failure. Native enum 65537 passed to an unsigned16 formal
  becomes 1; generated C# initially fails CS0221. The shared enum-to-integer
  coercion now uses the same unchecked out-of-range constant guard as ordinary
  integer coercion. Independent normal-call/assignment/return coverage passes.
- The first derived-expression lowering emits invalid C# statement forms in
  mixed for clauses (both CommaOp and CommaSeq). Result-discarded scalar targets
  now emit their underlying BCL invocation; unused no-argument endian facts may
  disappear. Mixed loops preserve calls, assignments and volatile/atomic reads,
  while proven-pure unused operands can disappear.
- ZSTD-C006: the first experimental discard lowering assigns to a legitimate C
  local/parameter named `_`. Native preservation passes while both pre-repair
  managed controls return the wrong result. Descriptor-level discarded scalar
  calls avoid any new `_ =` assignment, source renaming, helper, boxing or
  temporary sink. Native and direct/object/instance/context tests now preserve 99.
- An unrelated existing parser limitation for a `(void)` function-returning-
  pointer declarator is not repaired. The test-only address getter uses an unused
  int parameter and retains the intended stable/distinct pointer identity proof.

Evidence index: `../../artifacts/function-override-direct-20261020/shared-summary.json`.
Final focused results are 170 unit and 34 functional passes plus the native-backed
raw/processed JIT/NativeAOT fixture. First complete current-tool source retry
`20261020-064606-43b48ba9` passes all 31 units, both builds and exact 747 intrinsic /
48 LE24 selections, followed by actual caller IL audit and affected Blink,
SQLite and QuickJS checks. Final codec matrix/delivery/timing acceptance is
recorded separately in [direct-intrinsics.md](direct-intrinsics.md); historical
failed records and earlier accepted products are not relabeled.

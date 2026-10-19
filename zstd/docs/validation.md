# Zstandard validation ledger

Updated 2026-10-19. Accepted for the selected Linux x64 portable single-thread
scope. Results below are scoped to their exact receipts; no required gate is open.

The initial workspace inspection showed pre-existing unrelated changes in
EDG/C++, duckdb, libuv and repository summary/plans. They are outside this
campaign's ownership. No ancestor/root AGENTS.md applies; EDG-specific instructions
would apply if that tree were entered.

Planning inputs read: `docs/campaigns.md`, `docs/plans/translation-baseline.md`,
`docs/plans/translation-template.md`, QuickJS recipe and owning API/test delivery
conventions. Source release checked at the official GitHub v1.5.7 release page.

| Gate | Current status |
| --- | --- |
| Scope, decisions and plan | Frozen portable current-format single-thread scope, explicit tracing off. |
| Native oracle and first full-source attempt | Passed; initial compiler blockers repaired and committed. |
| Full raw/processed translation and import audit | Passed final receipt 20261019-204241-00db2fbc. |
| Actual translated ABI and first useful workflow | All four ABI and consumer cells pass in final receipt. |
| Required feature/lifetime/upstream/interoperability matrix | All four cells pass, including 33 actual upstream case labels and context-size equality per cell. |
| Normal consumer and whole-library-rooted AOT | Both forms compile/execute and pass actual IL/ELF/method-map/notice audit. |
| Final reproduction and affected regressions | Passed, including clean committed-source regeneration/solution/four-cell consumer and authored preservation. |

Append exact commands, receipt/log paths, case counts, tool/runtime identities
and milestone commit IDs as evidence becomes available. Build success alone does
not establish qualified output.

## Initial shared framework baseline

`python3 -B -m unittest discover -s Scripts/campaigns/tests -v`:
70 tests, 69 passed, one existing unrelated failure in
`DiscoveryTests.test_campaign_helpers_keep_hash_literals_in_metadata` because
`blink/scripts/campaign_delivery.py` contains an embedded 64-character digest.
No shared framework files or Blink files were modified by this campaign. This
does not establish a new zstd defect and is retained separately from campaign
qualification.

## P0 source/native baseline and first translation

The JSON metadata selects the official v1.5.7 release archive and commit. The
explicit 30-unit portable core manifest matches the four upstream library source
groups in `lib/libzstd.mk`; the authored ABI bridge makes 31 selected units.
Source copies contain no adaptations. Upstream LICENSE/COPYING and embedded
xxhash/divsufsort notices are preserved for delivery.

Native command:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/tests/NativeOracle/run.py --source zstd/ref/zstd-1.5.7 --output zstd/artifacts/native-baseline
```

`artifacts/native-baseline/native-report.json`: pass, 11 named self-test cases;
explicit failing control exits 1 as expected; ABI and fixture controls pass.
Plain and dictionary frame decodes compare exact original bytes; retraining the
fixed dictionary yields identical bytes. The native library uses the same source
manifest and defines. The test harness uses explicit CHECKs that remain enabled
with production NDEBUG. Native build is CMake `--parallel 2` through the shared
production command adapter.

Native ABI observations (12 ordered `dz_abi` fields):
`[8,8,24,8,0,8,16,24,8,0,8,16]`. These are native observations only until actual
emitted-code comparison passes P1.

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/probe.sh --tools reuse --fetch never --hashes off --jobs 4
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/probe.sh --tools build --fetch never --hashes off --jobs 4
```

First receipt: `artifacts/campaign/20261019-195448-ae2cd766/receipt.json`.
Current-tools rebuild/retry:
`artifacts/campaign/20261019-195558-92624953/receipt.json`. Both fail at
`lib/compress/zstd_compress.c` aggregate initializer lowering; see ZSTD-C001.
The rebuild eliminates the stale-tool unsupported `-U` warning. Emission siblings
are canceled and drained on failure. No translated product exists yet.

Canonical layout check and wrapper dry run pass from `/tmp`:

```bash
bash /home/marius/p/dotcc/Scripts/campaign.sh layout zstd --check
bash /home/marius/p/dotcc/zstd/scripts/translate.sh --dry-run --tools reuse --fetch never --hashes off --jobs 2
```

The selected profile is `portable`; both commands report no layout issues.

P0 committed as `c3a66ac65` (zstd-only source/configuration/native/scaffold/docs).
Authored API/test delivery and shared compiler repairs remain separate ongoing
milestones; this commit does not claim P1–P6 acceptance.

## Subsequent full-source retry

`artifacts/campaign/20261019-195801-a865be3d/receipt.json` records the next
complete translation attempt with the initial multidimensional aggregate repair.
It passes the original `zstd_compress.c` failure and reaches a GNU `__inline`
keyword parsing failure in unit 027 `divsufsort.c` (ZSTD-C002). All required
algorithms remain selected. Raw/processed products are not yet built/published.

`artifacts/compiler-regressions/shared-fixes-after.trx`: 24 focused and adjacent
unit cases pass; `shared-functional-after.trx`: three execution fixtures pass.
Native GCC outputs are retained in `aggregate-array-dimensions-native.log` and
`gnu-inline-aliases-native.log`. The valid local multidimensional aggregate-copy
initializer is retained; excess outer row braces are rejected consistently with
strict native GCC. The C001 initial failure is reproduced against an immutable
pre-fix snapshot in `aggregate-before.json`/`.log`; C002's before-state TRX records
four failed aliases and one passing explicit-macro precedence control.

Full retry `artifacts/campaign/20261019-200202-07ddc0d9/receipt.json` emits and
links all 31 selected units, then raw managed build fails on a CBool-to-enum
conversion (ZSTD-C003). No generated output is patched and nothing is published.

## First complete products and raw JIT discovery

`artifacts/campaign/20261019-200450-e4faef65/receipt.json` passes complete
31-unit emission/link, raw build, processed semantic-context build/postprocessing,
processed build and final canonical-path builds. Both products are published
through the shared transactional publisher. C003 focused tests pass 13 cases and
two functional fixtures; native bool-enum output agrees exactly.

Independent source/import inspection found 12 C# sources in each owned product,
3,906,121 raw and 3,839,302 processed source bytes. Both products contain exactly
one exported `ZSTD_compress2`, `ZSTD_decompressStream` and `ZDICT_trainFromBuffer`,
generated upstream enum constants, no `DotCcImports`/`DotCcStaticImports` classes
and zero `NativeImports.TryResolveExport`/`LoadLibrary` call sites. LICENSE,
COPYING and THIRD-PARTY-NOTICES are included with build/publish copy metadata.
The stronger automated closure/consumer audit remains part of final acceptance.

First actual raw JIT qualification attempt:
`artifacts/campaign/20261019-200625-94aeae63/receipt.json`. Emitted/native ABI
layout and actual stream-struct storage roundtrip pass; the version-string case
fails because preprocessing produced `1 . 5 . 7` instead of `1.5.7` (C004).
This initial launch omitted the opt-in production command-lock environment;
subsequent discovery and qualification commands use it.

Runtime-only follow-up using the already-built actual raw JIT harness passes:

- `artifacts/discovery/raw-jit/behavior-report.json`: 30 cases.
- `artifacts/discovery/raw-jit/lifecycle-report.json`: six cases.
- `artifacts/discovery/raw-jit-interop/interop-report.json`: six managed cases.
- `artifacts/discovery/raw-jit-interop/interop-comparison-report.json`: three
  exact native comparisons (frame bytes, dictionary training bytes and native
  decoding of a managed frame using a native dictionary).

These are discovery results for that raw JIT product only. Separate consumer,
processed and NativeAOT rows, exact translated upstream cases and final-tool
regeneration remain open. No native backend is used by the managed codec.

## Final shared repairs and regression window

`artifacts/compiler-regressions/shared-fix-evidence.json` records exact native,
before/after unit/functional commands and report hashes for all four shared
repairs. C004's final whitespace/mapping/boundary selection passes 91 unit cases
and two execution fixtures; the trailing-empty nested-macro edge agrees with
native output. The fix preserves macro hidesets and source mapping.

Current-tool full retry
`artifacts/campaign/20261019-201437-73006882/receipt.json` passes the complete
31-unit pipeline and both canonical final builds. .NET SDK: 10.0.112. Required
single-thread macro undefinition is explicit now that the current CLI supports
`-U`; native and translated profiles use the same configured undefinitions.

Serial downstream validation is running with explicit current compiler and
postprocessor paths and `DOTCC_CAMPAIGN_COMMAND_LOCKS=production`:
`artifacts/downstream/report.json`. Selected work is fresh SQLite translation,
raw/processed JIT consumer and native/translated API corpus; fresh QuickJS
translation plus raw/processed JIT behavior/lifecycle/upstream; fresh picotls
translation plus its retained raw/processed JIT qualification harness. The exact
CBool-to-enum scenario is native-backed in focused execution fixtures; no direct
equivalent enum cast was found in selected picotls/MsQuic sources. Existing
MsQuic artifacts are not claimed as new-emitter validation.

The owning API now retains SafeHandles throughout actual translated calls with
DangerousAddRef scopes. Its additional active-GC lifecycle case and revised audit
helpers await their actual rebuilt matrix; the earlier 30/6 discovery counts
must not be attributed to those later changes.

All six downstream commands passed. SQLite native/translated API stdout bytes are
identical, including the independent-model transaction corpus. QuickJS passed
both forms' behavior/lifecycle/upstream cases; picotls passed both forms' retained
JIT qualification, including independent peer sessions. Exact commands, current
tool hashes and logs are in `artifacts/downstream/report.json`.

Shared compiler milestone committed as `fdd42a910` after those gates passed.
[compiler-repairs.md](compiler-repairs.md) summarizes the focused changes and
reproducible checks. No unrelated workspace changes were staged.

Enhanced upstream native control:
`artifacts/upstream-native-expanded/report.json` passes six groups/33 original
case labels, with group counts `[1,1,1,11,13,6]` and failing CHECK control exit 1.
The exact excerpts retain 54 assertion sites, and source-range hashes/unique
staged-body inclusion were independently checked. This is native control evidence;
translated matrix execution is a separate required gate.

Final all-form/all-mode verification is running under production locks:

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --tools reuse --fetch never --form all --mode all --rid linux-x64 --jobs 2
```

Receipt: `artifacts/campaign/20261019-202322-a76910b4/receipt.json`.
Current acceptance remains open until its feature/consumer/upstream/audit matrix
and clean reproduction gates pass.

## Final API matrix and qualification-harness repairs

Receipt `artifacts/campaign/20261019-202322-a76910b4/receipt.json` passed
all four ABI, consumer and behavior cells but was stopped during a newly added
active-GC lifecycle case. Its collector loop was unbounded and continuously
requested full collections. Only that exact owned command process group was
terminated; the intervention is retained with the receipt. This was a harness
scheduling defect, not evidence of a codec crash. The bounded 32-iteration
collector passes the targeted raw-JIT lifecycle rerun, seven cases in 0.564 s:
`artifacts/campaign/20261019-203002-1bb78126/receipt.json`.

The facade also rejects unsupported cast experimental parameter values that
could enable stable input/output buffers: the owning API pins spans only for
each call. Its declared parameter allowlists preserve that contract; the raw
translated C interface remains complete. Tests exercise rejection, the real
unsupported-worker error and reset recovery. SafeHandle references remain held
through actual translated calls, including when collection is active.

Corrected final verification repeats the exact all-form/all-mode command above:
`artifacts/campaign/20261019-203037-026bc24a/receipt.json`. All four cells pass
ABI3, behavior30, lifecycle7, interop6 plus exact native comparisons, and the
four-file ordinary consumer workflow. Both whole-library-rooted AOT harnesses
and ordinary trimmed AOT consumers compile and execute. The upstream test-only
raw build then exposed distinct anonymous typedef identities from mixing acquired
reference headers with the staged header closure used by reused core objects.
Repairing that test input closure precedes the remaining upstream/audit gates;
this partial receipt does not establish final acceptance.

Header-closure repair target:
`artifacts/campaign/20261019-203529-8adc43fa/receipt.json` passes actual
translated upstream raw JIT: 33 cases in six groups with counts
`[1,1,1,11,13,6]`, native and translated deliberate CHECK controls exit 1.
`artifacts/upstream-source-review.json` independently recomputes all eight
original inclusive source-range hashes and checks the six executed counts.
The private test-only library uses the exact staged lib header identities of
its complete 31 reused core objects. The data generator is copied byte-for-byte
into that staged source closure; its source hash and paths are reported.

## Explicit native/translated trace profile alignment

The full follow-up receipt `20261019-203707-7e1950b8` was superseded when a
printed upstream memory-usage observation exposed native feature auto-detection
outside the explicit flags: GNU ELF enabled optional weak-symbol tracing, adding
an eight-byte private context member, while DotCC selected tracing off. This
changes the printed context size, not the original selected upstream assertions.
ZSTD-H003 pins optional host tracing off explicitly in the shared JSON profile;
final acceptance requires full regeneration, current native controls, actual
context-size agreement and a new complete matrix. Earlier receipts retain their
actual configured flags but do not establish this final explicit profile.

## Final explicit-profile qualification

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/verify.sh --tools reuse --fetch never --form all --mode all --rid linux-x64 --jobs 2
```

Final receipt `artifacts/campaign/20261019-204241-00db2fbc/receipt.json`:
passed in 232.6 s, no receipt warnings. Every selected unit emits/links and
canonical raw/processed builds publish through the transactional framework.
All eight declared verification suites pass. Native core and managed core share
explicit `ZSTD_TRACE=0`; both fresh native ELF controls have no trace symbols.

| Suite | Raw JIT | Raw NativeAOT | Processed JIT | Processed NativeAOT |
| --- | --- | --- | --- | --- |
| ABI | 3 | 3 | 3 | 3 |
| Behavior | 30 | 30 | 30 | 30 |
| Lifecycle | 7 | 7 | 7 | 7 |
| Interop | 6 + 3 exact native comparisons | 6 + 3 | 6 + 3 | 6 + 3 |
| Actual upstream | 33 | 33 | 33 | 33 |
| Ordinary owning consumer | Pass | Pass | Pass | Pass |
| Artifact audit | Pass | Pass | Pass | Pass |

All case-report failed/skipped counts are zero. Original upstream group counts
remain `[1,1,1,11,13,6]`; 54 excerpt assertion sites and three complete routines
are retained. Source ranges/hashes and actual all-four-cell case counts were
independently recomputed in `artifacts/upstream-source-review.json`. A separate
supplementary check requires native/emitted printed context size to equal
582672 in every cell, closing H003 with actual execution. Native/managed CHECK
failure controls exit 1 as expected and are explicitly marked in receipts;
their intentional nonzero command statuses are not omitted.

`audit-report.json` checks both 12-source products, ordinary project references,
exact authored/generated implementation closure, no required-symbol native
fallback, actual consumer/rooted-harness managed imports and dependency metadata,
ELF dependency allowlists and exact copied notices. Each rooted AOT method map
contains 12749 emitted methods, including required translated optimal, LDM,
streaming and training implementations. Both normal consumers retain ordinary
trimming. No native zstd library, subprocess or alternate codec is a backend.

The independent-context runtime/handle fixes, explicit parameter allowlists and
bounded GC test are included in these final rebuilt artifacts. Earlier partial
runs remain discovery/history evidence only. Clean committed-source reproduction
and the ordinary root solution check complete the remaining delivery gates.

The ordinary root solution check also passes:
`artifacts/campaign/20261019-204241-00db2fbc/root-solution/report.json` records
exact argv, applied shared production lock/budget flags and build-log hash.
`dotnet build zstd/ManagedConsumer.slnx -c Release --nologo -m:1
--artifacts-path PRIVATE` exits 0 in 8.727 s. Two existing generated CS0649
`VectorIoEntry` field warnings remain; there are zero errors. The receipt's empty
warning array refers to campaign policy warnings, not an assertion that compiler
logs contain no warnings. No build is left running before reproduction.

## Committed delivery and clean reproduction

Authored API, consumer, exact upstream tests, audits and final trace profile were
committed as `027c29a54` after the full matrix and root solution passed. The
previous focused milestones are `c3a66ac65` (source/native campaign) and
`fdd42a910` (shared compiler repairs with affected downstream regressions).
No push was performed.

`artifacts/reproduction/report.json` passes all six commands, retaining exact
argv, environment, cwd, logs, source-archive hash and copied tool-closure hashes.
The isolated ordinary-copy root is `build/reproduction/clean-delivery`. It
contains a `git archive` of committed `Scripts`, root MSBuild/NuGet metadata and
`zstd`, copied final compiler/postprocessor binary closures, and the existing
official local upstream source tree. It does not rebuild the compiler, require
an upstream archive, rely on old output products or use source/tool symlinks.
NuGet restore remains its separate normal .NET policy.

All commands ran from `/tmp`, using absolute paths resolved inside that clean
root and the selected copied final tools. The command sequence was:

```bash
bash CLEAN/Scripts/campaign.sh layout zstd --check
bash CLEAN/zstd/scripts/translate.sh --tools reuse --fetch never --hashes off --jobs 2
python3 CLEAN/Scripts/production_support.py dotnet build CLEAN/zstd/ManagedConsumer.slnx -c Release -m:1 --artifacts-path CLEAN/zstd/build/solution
bash CLEAN/zstd/scripts/test.sh --suite consumer --tools reuse --fetch never --hashes off --form all --mode all --rid linux-x64
bash CLEAN/zstd/scripts/fetch.sh --fetch never --hashes warn
bash CLEAN/zstd/scripts/fetch.sh --fetch never --hashes off
```

Here `CLEAN` abbreviates the absolute isolated root, not a required shell
variable. `DOTCC_CAMPAIGN_COMMAND_LOCKS=production` and explicit compiler and
postprocessor paths inside `CLEAN` applied to each command. Complete translation
passes in 54.05 s, ordinary solution in 8.73 s, and four-cell consumer in
41.08 s. All 39 committed authored campaign files retain identical before/after
hashes; the root uses only ordinary copies and no symlinks.

Clean receipts under `CLEAN/zstd/artifacts/campaign/`:

- `20261019-204805-2a1befc1`: full raw/processed translation, hashes off,
  local source with no cached upstream archive, fetch never, passed.
- `20261019-204908-25825a2c`: owning consumer raw/processed JIT/NativeAOT,
  all four meaningful file workflows passed.
- `20261019-204949-ef20a710`: after copying and deliberately appending a
  provenance-control marker to the isolated cached archive, hashes warn reports
  the mismatch and accepts the still-usable local source without overwriting it.
- `20261019-204949-d0b3c04a`: the same isolated inputs with hashes off pass
  without common provenance checks. The canonical original archive/tree remain
  untouched. Strict mode is optional and was not selected.

The clean archive proves output regeneration and ordinary project-reference
execution independently of the original build/output paths. The original full
matrix supplies the separate upstream, native interoperability and rooted AOT
artifact audits. The final documentation-only commit clarifies the stream
examples with explicit MemoryStreams and rewind; implementation files remain
identical to the qualified/reproduced source commit.

There are no incomplete required gates for the selected scope. Other
OS/architectures, worker compression, pre-v0.8 frames, optional trace hooks,
deprecated/contrib/CLI products and the explicitly omitted randomized, stress
and complete upstream drivers remain unqualified. The unrelated shared framework
baseline failure recorded above is not relabeled as a pass.

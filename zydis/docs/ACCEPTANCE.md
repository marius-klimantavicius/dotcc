# Zydis campaign acceptance

Current campaign policy: `default` uses `--aggregate-data` at the canonical
product paths. `aggregate-data` remains an explicit alias with separate profile
outputs; `typed` is the explicit unoptimized typed-initializer comparison. This
is a Zydis recipe choice; the standalone compiler option remains opt-in. The
root solution and ordinary sample keep their canonical project references.

**Default promotion passed on Linux x64, 2026-10-27.** The canonical raw and
processed products were regenerated through the framework's transactional
pipeline using the existing qualified compiler/postprocessor snapshots. The
invocation omitted `--profile`, selected `default`, and ran all nine existing
suites with unchanged native flags and feature scope.

Current authoritative evidence:

- [Default verify receipt](../artifacts/campaign/20261027-095741-6051ba79/receipt.json)
- [Default audit](../artifacts/campaign/20261027-095741-6051ba79/zydis-audit.json)
- [Combined acceptance and source counts](../artifacts/campaign/20261027-095741-6051ba79/optimized-default-acceptance.json)
- [Root solution and plain sample commands/results](../artifacts/campaign/20261027-095741-6051ba79/canonical-consumer-validation.json)
- [Python profile, identity and measurement tests](../artifacts/campaign/20261027-095741-6051ba79/python-tests.log)

All **18** product object commands carry `--aggregate-data`; product and
qualification link commands omit it. Both canonical shared `Zydis.cs` files are
**5,635,597 bytes**, with **18 encoder copies** totaling **70,281 bytes** and
**zero encoder field stores**. Total generated C# is 6,607,052 bytes raw and
6,582,354 bytes processed. Explicit optimized-default stamps and current
companion emission identities match at the canonical paths.

All nine suites pass: **393 native rows**, including all **385** retained upstream
cases; **1,572 core rows** across raw/processed × JIT/NativeAOT; **786 additional
rooted AOT rows**; **20 managed boundary rows**; and **four consumer workflows**.
There are zero failed or skipped required rows. The root solution builds with
zero errors (two existing CS0649 runtime-field warnings), and the plain sample
runs successfully without `TranslatedProject`. Evaluated MSBuild ProjectReference
resolves to `generated/TranslatedZydis/TranslatedZydis.csproj`.

All **19 Python tests** pass, including explicit typed omission, optimized default
and alias emission, rejection of missing/unoptimized default identities under
`--hashes off`, recorded-flag measurement pairing, historical receipt handling
and rejection of an optimized baseline. No compiler code changed, so the earlier
compiler regression and 120-execution benchmark results in AGGREGATE-DATA.md remain historical;
no new timing claims or alternate-profile execution claims are made here.

Reproduce the current canonical default serially from the repository root:

```bash
bash zydis/scripts/verify.sh --fetch never --tools reuse --restore normal \
  --hashes off --form all --mode all --rid linux-x64 --jobs 4
dotnet build zydis/ManagedConsumer.slnx -c Release -m:1
dotnet run --project zydis/samples/ManagedConsumer -c Release
python3 -B -m unittest discover -s zydis/tests -p 'test_*.py' -v
```

Use `--tools build` instead of `--tools reuse` when the compiler/postprocessor
have not yet been built. Alternate `aggregate-data` and `typed` reproduction
commands and the original benchmark provenance are in
[AGGREGATE-DATA.md](AGGREGATE-DATA.md). The canonical references were unchanged;
no generated files were copied from an optional profile. Generated products and
receipts remain ignored local artifacts. No push or PR was performed.

## Historical original typed-default acceptance

The original `default` profile used typed initialization. P0–P6 passed on
2026-10-27 for Linux x64. The complete Zydis 4.1.1 library's
18 C units remain translated: decoder, encoder, formatter/tokenizer, disassembler,
metadata, segment/address and NOP utilities, including AVX512/KNC. Required Zycore
product dependency is ten headers; original Terminal.c is test-only. Both MIT
notices accompany delivery. No upstream algorithms or generated C# were patched.

The owning API is `src/Api`; the ordinary consumer is `samples/ManagedConsumer`.
The complete public low-level API remains available. Both raw and processed
canonical projects build, and the root solution builds with zero errors.

Authoritative evidence for that historical run:

- [Final verify receipt](../artifacts/campaign/20261027-072757-6171e6fa/receipt.json)
- [Final qualified audit](../artifacts/campaign/20261027-072757-6171e6fa/zydis-audit.json)
- [Supervisor acceptance](../artifacts/supervisor-probe/acceptance.json)
- [Final root solution build](../artifacts/supervisor-probe/root-solution-final-build.log)
- [Changed files](CHANGED-FILES.txt)

The historical final command was invoked from `/tmp`, with newly rebuilt tools
and clean private framework staging, reusing local sources and normal NuGet restore:

```bash
bash /home/marius/p/dotcc/zydis/scripts/verify.sh --fetch never --tools build \
  --hashes off --form all --mode all --rid linux-x64 --jobs 4
```

All nine suites pass. Native: 393 declared rows, including every 385 retained
upstream case. Raw/processed × JIT/NativeAOT: 1,572 core rows (393 per cell).
Whole-library rooted companion AOT: 786 additional rows. Delivered-product
managed boundaries: 20 rows, including product-rooted AOT. Ordinary normally
trimmed consumer: four successful useful workflows. There are zero failed or
skipped required cases. Audit verifies 43 public functions and 66 public types,
original authored links/notices, current source identities and AOT dependencies.
The companion identity now includes all 15 generated lookup-table `.inc` files.

Shared validation: 3,489 compiler unit tests, 83 affected initializer/array
execution fixtures and the table-identity regression pass. Framework tests had
89 passes and one unrelated preexisting Blink metadata-literal failure; its log
is retained in `artifacts/supervisor-probe/framework-tests.log`.

Milestones: `b2c10fae1` (structural inferred aggregate-array bound repair),
`8ef927e6c` (pinned/native baseline), `a169238f7` (full execution qualification),
`45abc0701` (generated-table freshness repair). Final read-only reviews have no
unresolved findings. The fresh post-repair full verify is the P6 evidence.

Original API/consumer/test files are byte-preserved through final regeneration.
Preexisting work was excluded from campaign commits. The shared checkout later
returned nine initial compiler/CLI/docs edits to HEAD at 07:24; campaign commands
did not edit/reset those paths. Initial status and tracked diff are backed up in
`artifacts/supervisor-probe/workspace-baseline`; the precise closing observation
is `preexisting-preservation-final.json`. No unrelated content was restored or
overwritten. Closing inspection also records 41 initially untracked paths now
absent, including the two runtime-selection tests and sandbox-xed authored files;
their initial status is retained, but their contents were not in the tracked
diff backup. These concurrent removals were not campaign actions. The compiler
source change occurred before the final 07:27 tool rebuild. A subsequent
current-source rebuild confirms identical compiler/CLI method bodies, constants
and embedded resources; remaining binary differences are build revision/debug
metadata. The qualified frozen tool identities remain in the final receipt.

Other OS/architectures, continuous fuzzing, prolonged stress and performance
thresholds are outside this qualification. Original excluded applications/tests
are itemized in [QUALIFICATION.md](QUALIFICATION.md). No required gate remained
blocked in that qualification. No push or PR was performed.

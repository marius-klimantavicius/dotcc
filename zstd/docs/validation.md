# Zstandard validation ledger

Updated 2026-10-19. Acceptance incomplete; no translated runtime success claimed.

The initial workspace inspection showed pre-existing unrelated changes in
EDG/C++, duckdb, libuv and repository summary/plans. They are outside this
campaign's ownership. No ancestor/root AGENTS.md applies; EDG-specific instructions
would apply if that tree were entered.

Planning inputs read: `docs/campaigns.md`, `docs/plans/translation-baseline.md`,
`docs/plans/translation-template.md`, QuickJS recipe and owning API/test delivery
conventions. Source release checked at the official GitHub v1.5.7 release page.

| Gate | Current status |
| --- | --- |
| Scope, decisions and plan | Authored; implementation in progress. |
| Native oracle and first full-source attempt | Passed P0 discovery; current compiler initializer blocker reproduced. |
| Full raw/processed translation and import audit | Unrun. |
| Actual translated ABI and first useful workflow | Unrun. |
| Required feature/lifetime/upstream/interoperability matrix | Unrun. |
| Normal consumer and whole-library-rooted AOT | Unrun. |
| Final reproduction and affected regressions | Unrun. |

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

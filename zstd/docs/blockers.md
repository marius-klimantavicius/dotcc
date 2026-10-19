# Zstandard blocker ledger

Updated 2026-10-19. Acceptance remains open.

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
- Reduction/regression/fix: assigned to the xhigh source/compiler implementation
  track; pending. The generic diagnostic originates at
  `DotCC.Lib/Ir/IrBuilder.Aggregates.cs`.
- Full-source retry: pending after a structural repair. Required algorithms and
  upstream tables remain selected; no generated C# repair is allowed.

For each observed failure record ID, stage/classification, exact reproducer and
tool identity, native expected/observed behavior, regression, shared structural
fix and the next full-source retry. Preserve superseded failures and distinguish
unrelated repository baseline failures.

# Zydis campaign acceptance

P0–P6 passed on 2026-10-27 for Linux x64. The complete Zydis 4.1.1 library's
18 C units remain translated: decoder, encoder, formatter/tokenizer, disassembler,
metadata, segment/address and NOP utilities, including AVX512/KNC. Required Zycore
product dependency is ten headers; original Terminal.c is test-only. Both MIT
notices accompany delivery. No upstream algorithms or generated C# were patched.

The owning API is `src/Api`; the ordinary consumer is `samples/ManagedConsumer`.
The complete public low-level API remains available. Both raw and processed
canonical projects build, and the root solution builds with zero errors.

Current authoritative evidence:

- [Final verify receipt](../artifacts/campaign/20261027-072757-6171e6fa/receipt.json)
- [Final qualified audit](../artifacts/campaign/20261027-072757-6171e6fa/zydis-audit.json)
- [Supervisor acceptance](../artifacts/supervisor-probe/acceptance.json)
- [Final root solution build](../artifacts/supervisor-probe/root-solution-final-build.log)
- [Changed files](CHANGED-FILES.txt)

The final command was invoked from `/tmp`, with newly rebuilt tools and clean
private framework staging, reusing local sources and normal NuGet restore:

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
are itemized in [QUALIFICATION.md](QUALIFICATION.md). No required gate remains
blocked. No push or PR was performed.

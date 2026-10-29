# Accepted C-route baseline

Accepted on 2026-10-30 for Linux x64, little-endian LP64. The actual pinned
libghostty-vt engine is compiled by Zig's genuine C backend, then translated by
DotCC. There is no handwritten terminal replacement or native Ghostty runtime.
Source/toolchain pins and selected scalar features remain under `config/`.

The user explicitly deferred upstream tests to the direct Zig phase. All 2,622
case names, 20 core partitions, helpers and partial history remain available.
**Deferred upstream execution is not passed.** This acceptance covers every
remaining C gate in [`verification.json`](../config/verification.json).

## Current results

Normal WARN verification from `/` passed in
`artifacts/campaign/20261030-001153-ab4e8deb/receipt.json`; fresh qualification
preparation is `20261030-001153-122ad39a`. No upstream profile was selected.

| Retained gate | Result |
| --- | --- |
| Complete engine | Fresh emission/link, raw build, semantic postprocessing, processed build and final-path publication passed. |
| Native | 16 original-Zig/genuine-C oracle cases, two boundary cases and six physical ABI layouts passed; exact transcripts match. |
| Owning consumer | Raw/processed × JIT/NativeAOT: four passed workflows, including split UTF-8, screens, resize/reset, concurrent independent owners and disposal. |
| Qualification | 24 cases × four rows = 96 assertions; zero failures/skips. Both AOT rows root the entire translated assembly. |
| Factory output | Maximum factory/helper arity **2** in both forms; actual Release getter IL references RVA data without `newarr`. |
| Allocations | All four JIT/whole-rooted-AOT rows execute 3,277 getters and 92 zero-argument factories with zero measured warmed managed allocations. Positive controls allocate. |
| Audit | Both forms retain all 93 required exports, exact owned source sets, six full notices and attributed runtime OS services; no native Ghostty provider or diagnostic entrypoint leaks. |
| Ordinary root solution | One rebuild from `/` after the passed WARN master uses original sample → original API → canonical library. Exit 0 in 22.907 s; zero errors, four CS0649 warning occurrences (two distinct generated unused-field diagnostics). |
| Preservation | All 89 current authored/critical facts unchanged. Root rebuild additionally preserves 217 protected files, 265 tool hashes, prohibited compiler outputs, default pointer and HEAD/index bytes. |

The class remains `Managed.GhosttyVt`. Canonical projects are
[`TranslatedGhosttyVt`](../generated/TranslatedGhosttyVt/TranslatedGhosttyVt.csproj)
and [raw](../generated/TranslatedGhosttyVt.Raw/TranslatedGhosttyVt.csproj).
Aggregate-data emission, shared literal pool, 256 KB splitting and inline
deduplication remain enabled. Each form has 57 C# files; the main file is
8,910,469 bytes, with total C# of 26,267,501 bytes raw and 26,445,158 processed.
These sizes are observations, not performance thresholds.

The auditor warms the complete getter set before its strict acceptance pass.
The preliminary raw warmup observed 4,264 bytes; the second pass reports zero
for every getter and factory. No cold-start or native-allocation claim follows.
The argument-taking factory has IL/call-graph and separate native/direct/object/
consumer execution coverage; it is excluded from zero-argument measurement.

## Reproduction and regressions

OFF `20261029-205950-b1f45aae` passed fresh publication, consumer/factory four-row
matrices and audit from `/`, preserving its 86 facts. Original-reference OFF
rebuild r21 passed too. Both hash modes use fresh private work and local inputs
with `--fetch never`; restore is separate. Final WARN evidence is in
`artifacts/p6-c-route-warn/preservation.json` and
`artifacts/p6-c-route-root-rebuild-r24/report.json`.

Shared validation passed 3,769 unit and 203 affected functional executions.
Selected downstream checks passed Zydis's seven suites (`203355-4300ba66`) and
Valkey's 164-unit translation (`203338-dace6771`) plus 76 JIT/AOT consumer checks.
Earlier failures and repairs remain in the implementation ledger.

`build/c-route-evidence-r23/final-evidence-corrected-readers.json` validates every
retained gate. Its original rejected report and exact prior reader remain.
Reader fixes distinguish an MSBuild project property from a positional project,
and preliminary warmup from strict measurement. Seventeen pure controls include
nonzero measured-allocation rejection. No product/test threshold changed. The
root task's model-capacity failure happened after its successful build; the
supervisor completed evidence review without another build.

```bash
cd /
bash /home/marius/p/dotcc/libghostty/scripts/verify.sh \
  --profile scalar --fetch never --hashes warn --tools reuse --restore normal \
  --form all --mode all --rid linux-x64 --jobs 2 --timeout 7200
dotnet build /home/marius/p/dotcc/libghostty/ManagedConsumer.slnx \
  -c Release -t:Rebuild -m:1 --nologo
```

Observed compiler SHA-256:
`d42e96a8ea6a82fb4ac7746aaa857928a92a855d811c9885cbb2e422f8f23e6d`.
Postprocessor SHA-256:
`6e7d953454de88fec6c55ba3b29bda1decb54a52b3fad9c0e3e141e96e4a68dc`.
Receipts record full closures and commands. No other-platform execution, x87
arithmetic, security isolation or extended stress/performance claim is made.

The focused C completion commit preserves unrelated work. Only after that
commit does relevant true-upstream integration and actual direct Zig → DotCC
implementation begin, including the deferred upstream qualification. This C
route remains its independent reproducible baseline.

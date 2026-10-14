# Genuine upstream and real translation-unit acceptance

The final current-format gate uses three original positive EDG tests and one real
C++ translation unit from pristine revision
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`. All four objects have the exact 4bb
contract, 251 tables, 1 KiB pages, complete headers, zero FK failures and no
structured semantic BLOB/REAL columns. No source was rewritten to pass.

| Input | Entities | Object bytes | Assertions | Open ms | Cold ms | Warm ms | Cold/warm queries |
|---|---:|---:|---:|---:|---:|---:|---|
| templates | 4,252 | 985,088 | 105 | 61.28 | 52.69 | 0.47 | 285/5 |
| lifetimes | 552 | 646,144 | 41 | 60.02 | 43.90 | 0.32 | 69/3 |
| inheritance | 4,945 | 1,120,256 | 911 | 60.16 | 64.80 | 8.16 | 529/97 |
| types-tu | 1,235,097 | 106,483,712 | 389 | 56.37 | 194.13 | 0.85 | 339/5 |

Total: **1,446 public assertions** across four distinct objects. Each artifact
runs in a separate process. Cold means the first selected pass with a fresh reader
cache, without flushing OS caches; warm repeats the same selection. The harness
uses exact stored-name FindSymbols and canonical typed links, never C++ lookup or
demangling. It selects at most four routine roots and 256 operations per root,
plus limited shallow/call/reference checks. No whole-TU traversal or statistical
throughput claim is made.

The large `src/types.c` input is 799,640 bytes, compiled as C++14 using the actual
native Ninja command’s defines/includes. Supported EDG GNU13.3/no-exceptions/
no-RTTI options replace host language/codegen flags. The initial header failure
is preserved: only actual GCC predefines `__CHAR_BIT__=8` and `__SCHAR_MAX__=0x7f`
were added, discovered by GCC `-dM`. Both command attempts and the narrow delta
are recorded; no compiler/core/header/profile rewrite was made.

Stock rounded phase CPU is frontend 0.28 s, backend 7.12 s and total 7.48 s.
Combined wrapper wall is 7.678 s (external time reports 7.67 s), peak producer
RSS 254,620 KiB. Phase CPU is not derived by subtracting totals; stock reported
elapsed values have coarse resolution.

The 106,483,712-byte database comprises 87,066,624 table bytes, 19,209,216 index
bytes and 207,872 schema bytes. Its 157,692 text atoms store 3,893,693 unique text
bytes for 562,596 references; repeating text at every reference would require
23,757,904 text bytes alone, not a whole-database compression estimate. There are
59,597 constant occurrences using 37,908 values, with 3,942 shared values.

The real input reader opens in 56.37 ms, reads four roots/66 operations/three calls
without truncation in 194.13 ms, then repeats in 0.85 ms. Open working set is
87,875,584 bytes; final working set is 95,653,888 bytes. Cache use stays at 595
rows/81 pages and largest batch 27, below configured 4,096 rows/128 pages/900 keys.
The same-name `a_type` class and typedef remain distinct actual declarations.

Build passes with zero warnings/errors; generator emits 249 facades/1,941 properties,
literal-field and whitespace checks pass. Existing self73, cache eviction/concurrency,
maximum batch, workspace, packaging, logical comparison and synthetic scale proofs
are reused with their original scope. All 14 compiled stages retain 316 stock
inputs/tool pin and 26 canonical backend files; no new build or producer repeat.

Reproduction uses [upstream_acceptance.py](../build/upstream_acceptance.py) and the
isolated `--real-input <object> <templates|lifetimes|inheritance|types-tu>` reader
mode. The driver’s `--inspect-existing` requires every selected successful case;
it cannot pass with a missing or partial cohort. See [the managed guide](../managed/README.md).

Frozen evidence: [producer input/pin proof](real-inputs251-input-pin-proof.json),
[host recipe](real-inputs251-types-host-edg-recipe.json),
[all attempts](real-inputs251-attempt-results.json),
[storage/timing](real-inputs251-storage-timing-metrics.json),
[14-stage verification](real-inputs251-stage-verification-final.json),
[reader proof](../managed/real-inputs251-evidence.json), and
[native](native-real-inputs251-ready.json)/[managed](managed-real-inputs251-ready.json)
inventories and hash checks.

This gate closes the remaining M4/M5 real-input acceptance, alongside the explicit
[limits](LIMITS.md) and [public format policy](FORMAT.md).

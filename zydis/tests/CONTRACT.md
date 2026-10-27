# Qualification integration contract

Status: tests source frozen for parent integration. Native qualification passes;
managed qualification is pending the parent's complete regeneration/build/run.
The tests agent ran no managed/tool build, edited no generated C#, and created
no commit. Ownership is `tests/**` and `scripts/qualification.py` only.

## Companion integration

The parent wired `scripts/qualification.py:companions(ctx)` into
`Translation.companions`. It emits these test-only C Units and links them with
**the exact current product object files** under `ctx.work/objects`. It returns
`TranslatedZydisQualification` for shared raw/processed build/publication, with
class `Managed.Zydis.Tests.ZydisQualification`. Selection and flags come directly
from the recipe's `native_configuration`. No alternate CMake profile is used.
These Units and upstream tools are excluded from `TranslatedZydis`.

| Unit | Extra defines |
| --- | --- |
| `tests/qualification.c` | generated fixture-header include directory |
| Zydis `tools/ZydisInfo.c` | `main=ZqInfoMain` |
| Zydis `tools/ZydisToolsShared.c` | none |
| Zycore `src/API/Terminal.c` | original test-only CLI color-detection dependency |
| Zydis `tools/ZydisFuzzShared.c` | `main=ZqUnusedFuzzMain`, `ZydisFuzzTarget=ZqFuzzEncode` |
| Zydis `tools/ZydisFuzzEncoder.c` | `ZydisFuzzTarget=ZqFuzzEncode` |
| Zydis `tools/ZydisFuzzReEncoding.c` | `ZydisFuzzTarget=ZqFuzzReencode` |
| Zydis `tools/ZydisTestEncoderAbsolute.c` | `main=ZqAbsoluteMain` |

`tests/harness.py:test_units(ctx, source)` generates private fixture headers from
local upstream `.in` and JSON inputs, using actual upstream C types and enum
identifiers. Schema-only `ZYDIS_ATTRIB_NONE` maps to zero, exactly as in the
upstream Python fixture converter. No mirror structs or replacement algorithms.

Export `int ZqRun(unsigned group)` produces a deterministic transcript:

| Group | Scope |
| --- | --- |
| 0 | 18 actual type sizes/alignments, field offsets and two initialized storage byte images |
| 1 | six machine modes, split/full decode, Intel/AT&T/MASM, tokenizer reconstruction/lifetimes, hook/context identity and failures, ordinary errors, encoder conversion/roundtrip, absolute branches, 0–64 byte NOP fill, register contexts and segment/metadata utilities |
| 2 | all 210 original decoder/formatter snapshots |
| 3 | all 52 original encoder and 122 original re-encoder cases, including upstream C identity/range/roundtrip assertions |
| 4 | original absolute encoder test program |
| 5 | deliberate always-active assertion control; must exit 1 |

Alignment observations use **global actual wrapper structs**
`{ unsigned char prefix; TYPE value; }` and `offsetof(wrapper, value)`.
`_Alignof` is an assertion, never the reported observation. The managed
`ActualAbi.cs` additionally compares actual emitted CLR layouts/offsets/storage
against native, without relying on C metadata literals. `ManagedHooks.cs` uses
real generated formatter callback signatures with GCHandle-rooted contexts,
forced collections, callback identity, context separation and contained
exceptions. These managed checks are authored but not built/run by this agent.

## Native evidence

```bash
python3 -B zydis/scripts/qualification.py --native --work zydis/build/native-agent/controls-final
```

Passed with two native compiler jobs and exact effective full-library flags
(including explicit `__x86_64__` and `__linux`). Report:
`build/native-agent/controls-final/logs/zydis-native-report.json`.
393 case rows passed: 385 upstream and eight authored/ABI rows; zero failures
and zero skips. The absolute program runs its complete internal branching,
RIP/EIP-relative and range iterator cases, retained in the transcript.
The deliberate failure control returned 1 with `CHECK FAILED` output.

All 210 snapshots compare text exactly, allowing only upstream's CRLF-to-LF
normalization. Three original snapshots are empty/rejected. A separately
compiled **unrenamed original ZydisInfo** confirms each process exit and stderr;
the batch retains the full C return/status. Translated runs must reproduce its
exact bytes, statuses and stderr:

| Case | Full status | Original process exit | Original stderr classification |
| --- | --- | --- | --- |
| `info/mvex_025` | `80200001` | 1 | `DECODING_ERROR` |
| `info/vexc4_026` | `80200003` | 3 | `BAD_REGISTER` |
| `info/vexc4_027` | `80200003` | 3 | `BAD_REGISTER` |

Separate logs are `logs/zydis-original-info-<case>.log/.stderr`; original
controls and all case rows are retained in the native JSON report. Signals,
missing fixtures, incomplete/duplicate markers, unexpected stderr, assertion
failures and snapshot mismatches fail. Snapshots are never rebased.
`NO_COLOR=1` and empty `FORCE_COLOR` fix presentation.

Earlier attempts remain preserved: controls-01 exposed a schema zero-name
issue; controls-02 corrected an authored invalid-mnemonic expected status using
upstream sanity checks; controls-03 exposed read-only argv passed to original
code that rewrites it; controls-04 exposed the three intentional rejections.
These were harness defects, not accepted native crashes. Final runs provide
fresh writable arguments and preserve all failures.

## Parent execution and integration needs

`run(ctx, suite)` supports `native`, `abi`, `qualification`, `upstream`, and
`rooted-aot`, using `ctx.run`/`ctx.managed`. Audit delegates to separately owned
`scripts/audit.py`. Managed harness builds cache by product/form/mode/rooted
within a context; each new build runs the assertion failure control.

The companion carries `zydis-test-inputs.json`; managed runs reject changed
source/config/fixture identity against current native inputs. Parent must
regenerate after final alignment/fixture fixes, then run `abi`, `qualification`,
`upstream`, `rooted-aot` in selected forms/modes, the ordinary consumer and final
artifact audit. Shared tools, managed builds, postprocessing and execution are
parent-owned. No P1/P4/P6 managed acceptance is claimed from native checks.

# Native baseline observations

Current corrected native status: all 16 groups and 73 isolated cases pass in
release and debug. The globals-properties failure described below was resolved
by calculating GC relocation-buffer capacity in bytes; see the final section
for the trace and fix. Earlier sections retain the original observations.

Initial Linux x64 GCC 13.3.0 (Ubuntu 13.3.0-6ubuntu2~24.04.1), glibc 2.39
release probe compiled all 29 original selected C units
with the authored portable harness. `python3 pinta/scripts/native.py` produced
`artifacts/native-pristine-release/receipt.json`, `build.log`, `abi.txt`, and
per-group logs. This is native evidence only; it does not qualify translated code.

Thirteen groups completed with 1,832 successful checks and zero assertion
failures: GC, array, memory, code (original fixture set), buffer, format, integer,
decimal, property, object, function/imports, pattern, JSON. Three groups did not
complete: `code_v2` received SIGSEGV after opening `globals-properties-v2.pint`;
`weak` and `encoding` aborted with SPUT's `sput_run_test() omitted` message because
the original registrations call their bodies directly. These are not successful
or skipped assertions. Exact case diagnosis and corrected runs follow separately.

The original release build emitted no compiler diagnostics. ABI observations:
PintaHeapObject 24 bytes, union payload 16 bytes at offset 8; PintaReference 8;
PintaNativeFrame 24; PintaStackFrame 88; PintaModule 84 (alignment 4);
PintaModuleFunction 20; PintaApi and PintaApiEnvironment both 72; PintaApiString
16. All pointer-bearing records listed have alignment 8. `long` is 8, pointers
8, wchar and wchar_t 2. The literal `AŽ😀` stores 0041,017d,d83d,de00,0000 with
four UTF-16 code units plus terminator; decimal scale is 100000000.

Reproduction commands (run builds/tests serially):

```sh
python3 pinta/scripts/fetch.py
python3 pinta/scripts/native.py --stage pristine --profile release
python3 pinta/scripts/native.py --stage corrected --profile release --cases
python3 pinta/scripts/native.py --stage corrected --profile debug --cases
python3 pinta/scripts/native-boundaries.py --stage pristine
python3 pinta/scripts/native-boundaries.py --stage corrected
```

Nonzero exit is expected when any group/case fails or crashes; receipts survive
isolated failures. `--cases` additionally invokes each registered upstream test
body in an isolated process, plus the three weak/encoding bodies called directly
upstream. This permits the remainder of the corpus to run after a group crash.
The original 16 groups still execute as authored; supplemental cases do not hide
original registration or ordering defects.

The corrected stage applies five narrow engine creation-safety patches, two value-allocation safety patches, a GC
relocation-buffer sizing fix, two registration-only test patches, a test exception-message patch and test-only tracing;
`config/patches.json` contains their before,
patch, and after hashes. The weak/encoding patches wrap the existing invocations
with `sput_run_test`, preserving assertions. No original-native failures are
silently converted into corrected success.

Corrected release and debug completed 15 of 16 groups, with 1,839 checks in the
completed groups and zero failed assertions. Supplemental isolated execution
completed 72 of 73 upstream test bodies, with 1,980 checks and zero failed
assertions in the completed bodies (these overlap group checks; do not add the
totals). Both profiles fail `code_globals_properties_v2`: release SIGSEGV;
debug aborts at `pinta_core_get_object_type`'s `kind < PINTA_KIND_LENGTH` assertion.
This existing bytecode case remains unresolved and is not treated as parity.
Weak and encoding both pass with their registration-only repairs.

The suite opened 41 distinct fixture files, including the failing globals
property case. Nine checked-in fixtures were not opened by these registrations:
`construct-v2.pint`, `global-function-v2.pint`, `sample.pint`,
`simple-receipt-script-v2.pint`, `simple-receipt-script.pint`, `swed-receipt-v2.pint`,
`swed-receipt.pint`, `test-app-config-v2.pint`, and `test-app-config.pint`.
They remain unqualified. Merely defining the upstream construction test body
without registering it does not execute `construct-v2.pint`.

Ordinary creation boundary probes found 4,032 bad return/write observations for
pristine misaligned, undersized arenas; the corrected memory guard reduced these
to zero. Pristine `pinta_frame_init(..., UINT32_MAX)` incorrectly returns success;
its corrected frame guard returns `PINTA_EXCEPTION_INVALID_ARGUMENTS` (16).
An arena-size sweep initially still crashed after the first four engine repairs.
Source inspection found two further unchecked `pinta_heap_init` allocations; a
fifth engine patch guards the heap metadata and heap storage. The corrected rerun
passes all 4,097 tested arena sizes: 3,474 successful creations and 623 clean
rejections, with no writes beyond the declared arena. The two other creation
boundaries and repeated-execution observation also pass their expected checks.

The public API repeated-execution probe succeeds initially with global `WORD`
containing UTF-16 `Ąžuolas`. After the host writes `changed`, a second execute
returns success while leaving `changed` untouched and `code_finished=1`. This
observation is identical in pristine and corrected engine stages: success does
not establish that the bytecode ran again. The ordinary owning API must document
this behavior or supply a separately qualified execution-state extension.

`python3 pinta/scripts/upstream-tests.py` authors a separate qualification
executable by translating the same 29 core sources, real upstream assertions,
and portable C adapters. Its `translate` action preserves raw and postprocessed
sources under `generated/upstream-tests`; `run` builds each requested form under
JIT/NativeAOT and executes each selected case in a separate bounded process.
`--case NAME` narrows a diagnostic retry. Native comparison removes only SPUT
elapsed time and stream-interleaving differences: fixture-open callback order
and assertion output are compared independently. Crashes remain failures even
when both implementations crash. Creating this pipeline is not evidence of a
successful translated suite; consult `artifacts/upstream-tests/*/runs.json`.

The separately authored, fixed 256-byte `tests/fixtures/receipt.pint` also runs
natively through the public API. Its SHA-256 is
`5ef5dd52ffcee8c70a9afa6473db916b607c1a9d83a6ae30c63248e69e353a61`.
Inputs customer `Ada`, quantity 3, and price `12.50` produce total `37.5` and
52 exact UTF-16LE output bytes for `Customer: Ada\nTotal: 37.5\n`.
`native-boundaries.py --receipt-only` verifies both hex output and total against
the authored fixture manifest, rather than accepting success status alone.
This fixed assembler input supplies a real workload without building or depending
on an upstream source-language compiler. Numeric values are explicitly converted
to strings before `out`: its single-argument path writes strings/binary data and
does not implicitly format decimals.

The release and diagnostic upstream qualification executables now build and
execute under all four Linux x64 forms: raw JIT, optimized JIT, raw NativeAOT,
optimized NativeAOT.
Each executes all 73 original/registration-repaired test bodies, with **2,034
successful assertions and zero failed assertions**. In each form, 72 cases match
the native reference exactly after the documented time/stream normalization.
The remaining `code_globals_properties_v2` passes 54 assertions when translated,
while its native reference crashes. The runner deliberately reports this as a
failed differential case, so its overall exit code remains 1. It is not hidden
as a skip or called native parity.

Full four-form results for both profiles are retained in
`artifacts/upstream-tests/{release,debug}/matrix.json`, with generated source identities
in `translation.json`, binary hashes, and every case log. This test executable
is separate from the generated reusable library and its owning consumer; it
contains the actual C core, upstream C assertions, and portable adapters.
Windows execution is deferred at the user's request and is not a blocker for
the current Linux x64 target.

The corrected native release/debug baselines and both translated four-form
matrices were regenerated and executed after all nine staged patches and the
UTF-16 `swprintf` adapter, with unchanged case outcomes. `matrix.json` records
the native receipt and individual native log hashes alongside the actual
translated run results. Both sides therefore use the same final staged engine
source and portable adapter.

The authored `callback-return.pint` fixture consists of `CALL_INTERNAL 1,0`,
`STORE_GLOBAL 0` (`answer`), and `EXIT`. Its native oracle allocates an unreachable
string before returning `Ą😀`, assigns the interpreter caller's rooted return
slot, invokes compacting Pinta GC, and requires both actual relocation and exact
UTF-16 result units `0104,d83d,de00` after the guest stores the value. File reads
are capped at three bytes, with one open, 43 reads for the 128-byte fixture, and
one close required. Run `native-boundaries.py --callback-return-only`; the script
verifies fixture SHA-256 and exact result/callback/read counts. It passed: both
the result object and its character payload moved, and the guest retained the
exact three UTF-16 units. The receipt is in
`artifacts/native-boundaries-corrected/receipt.json`.

The same ordinary probe run also passed a metadata-swapped big-endian receipt
with identical total/output and unchanged source bytes. Each of the string,
uncached-character, and weak-reference allocation probes retained 41 integer
objects in a 1,024-byte heap, then returned upstream out-of-memory status 4 and
a null result. These validate the three narrow allocation guards in `string.c`
and `weak.c`; they do not claim every execution exhaustion boundary is covered.

## Filename-resolution recheck (2026-09-26)

Direct test-executable runs previously required `PINTA_FIXTURES`; without it the
portable file callback returned NULL even when the pinned fixtures were present.
Native and translated upstream runners now embed an absolute fixture-root
default and accept `--fixtures DIR` or `PINTA_FIXTURES`. The runtime environment
override takes precedence over the embedded default. Both slash conventions and
mixed paths, including Visual Studio-style relative and Windows absolute names,
resolve to fixture basenames under the selected root.

Fresh native and translated executables pass exact-byte checks for six path
spellings from an unrelated temporary working directory. Each executable passes
the default-root check, a runtime override with spaces in the directory name,
and rejection of an explicitly missing root. The normal upstream cases also run
outside the repository root. No upstream test assertion or bytecode fixture was
changed for this recheck.

| Profile | Native upstream bodies | Translated upstream bodies (each raw/optimized JIT/AOT form) | Assertions per translated form |
| --- | ---: | ---: | ---: |
| Release | 72/73 | 73/73 | 2,034 |
| Diagnostic | 72/73 | 73/73 | 2,034 |

Every recorded fixture open in the upstream case logs succeeded; there are no
missing-fixture records.
The native `code_globals_properties_v2` failure remains: its log confirms
`fixture-open globals-properties-v2.pint ok` before release SIGSEGV or the debug
object-kind assertion. Its translated body passes all 54 assertions. Thus
filename resolution does not explain that native failure, and the differential
runner continues to return exit 1 for it.

Commands, resolver conditions and aggregate results are saved under
`artifacts/filename-resolution/`. The current native receipts and translated
`runs.json`/`matrix.json` contain the rerun evidence. Earlier receipts and logs
are preserved under `artifacts/filename-resolution-before-20260926/`.

## Exception diagnostics and module-loading check (2026-09-26)

The test exception-message patch now includes enum names and decimal/hex codes
in expected/actual assertions. Native and translated runners also print failed
assertions and distinguish process signals from Pinta exceptions. Rechecking all
eight translated release/debug, raw/optimized, JIT/NativeAOT combinations kept
73/73 successful cases per combination and 72 native output matches. The native
globals-properties crash remains the sole mismatch.

A load-only probe with the globals-properties test's allocation setup confirms
that both its upstream test loader and the internal module loader return
`PINTA_OK` and a non-null domain in release and debug, even outside the repository
working directory. Module discovery and loading succeed before the execution
failure. Probe source, commands and logs are in `artifacts/error-diagnostics/`.

Deliberately selecting an empty fixture directory verifies the failure messages:
the internal file-open path produces `PINTA_EXCEPTION_FILE_NOT_FOUND` (15), while
the upstream test helper reports `PINTA_EXCEPTION_INVALID_MODULE` (14). The
public `pinta_api_load_module` API returns `NULL` on any loading exception;
`pinta_test_load_module` converts every such `NULL` to `INVALID_MODULE`, losing
the original cause. These missing-fixture errors are intentional diagnostic
checks, not failures observed with the normal fixture directory.

## Globals-properties root cause and fix (2026-09-26)

`PINTA_TRACE=1` confirms that `globals-properties-v2.pint` loads successfully and
fails later, after collection/compaction during a `LOAD_MEMBER` instruction at
bytecode offset 283. The next operations encounter corrupted cached integers.
The failure originates in `pinta_gc_compact`'s scratch-buffer sizing:

```c
/* Original: the byte distance is not necessarily a whole number of structures. */
count = (u32)((PintaHeapReloc*)stack_end - (PintaHeapReloc*)scratch_start);
/* Corrected: measure bytes, then round down to whole entries. */
count = (u32)(((u8*)stack_end - (u8*)scratch_start) / sizeof(PintaHeapReloc));
```

The failing Linux x64 GCC debug trace reports 928 available bytes and a 24-byte
entry size, but the original pointer subtraction produces a capacity of
1,431,655,804 instead of 38. The GC consequently writes beyond the available
stack storage, corrupting the adjacent integer cache. After the fix the same
trace reports 38 entries and the case completes all 54 assertions.

Subtracting the cast structure pointers here violates C's pointer-subtraction
requirements. The result can differ between compilers; the user's report that
Windows x64 passes is consistent with this defect. No Windows run was performed
in this workspace. The fix does not alter the module or enlarge the test heap.

Native release and debug now pass all 16 original groups and all 73 isolated
cases (2,034 assertions per isolated corpus). The before/after debug traces are
saved in `artifacts/globals-properties/`. Enable the same logging in native or
translated test executables with `PINTA_TRACE=1`; see [CONFIGURATION.md](CONFIGURATION.md).

The regenerated release/debug × raw/optimized × JIT/NativeAOT upstream matrix
passes all 73 cases per form with 73 native output matches and exit 0. Detailed
globals-properties traces match native in all eight forms, including the GC
capacity calculation and operand kinds. `artifacts/globals-properties/summary.json`
records the results and log/fixture hashes. Missing-fixture diagnostic checks
also confirm that tracing preserves the original `FILE_NOT_FOUND` (15) result
before the upstream test helper replaces it with `INVALID_MODULE` (14).

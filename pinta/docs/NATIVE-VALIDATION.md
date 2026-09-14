# Native baseline observations

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

The corrected stage applies five narrow engine creation-safety patches, two value-allocation safety patches, and two
registration-only test patches; `config/patches.json` contains their before,
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

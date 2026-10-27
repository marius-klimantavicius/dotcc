# Zydis qualification

The optional [`aggregate-data` profile](AGGREGATE-DATA.md) uses the same complete
source selection, suites and transcript comparisons as `default`. Its fresh
raw/processed × JIT/NativeAOT qualification passes with final rebuilt tools. A separate measurement
probe checks every native/emitted encoder-table byte and measures equivalent
build/initialization workloads without changing these acceptance transcripts.

Status (2026-10-27): P0–P6 accepted for Linux x64. The clean final
[verify receipt](../artifacts/campaign/20261027-072757-6171e6fa/receipt.json)
passes every declared suite with hashes off, rebuilt tools, both forms and both
execution modes. See [ACCEPTANCE.md](ACCEPTANCE.md) for exact totals, preserved
inputs, known baseline test findings and the workspace observation.

This document describes the current tests, not a promise of exhaustive x86
coverage. The [plan](PLAN.md) and [implementation ledger](IMPLEMENTATION.md) remain
parent-owned. Pins and file hashes live in [inputs.json](../config/inputs.json)
and [source-inventory.json](../config/source-inventory.json), rather than being
repeated here. The [case inventory](../config/upstream-test-inventory.json)
records every selected upstream case, portable fixture paths, native outcomes,
and every upstream tests/tools/examples file outside those fixture rows.

## Product and reference boundaries

The default [configuration](../config/translation.json) selects all 18 upstream
library C files, matching both the recursive `src/*.c` inventory and upstream
CMake declarations. Decoder, encoder, formatter, disassembler, segment utilities,
AVX512 and KNC remain enabled; minimal mode is disabled. Native and translated
qualification obtain the same source selection and C11/static-build flags from
`scripts/campaign.py:native_configuration`, including explicit x64/Linux
architecture/platform defines. The executed ABI target is linux-x64, LP64,
little endian. Other operating systems/architectures are unqualified here.

The upstream package tag is **v4.1.1**, while its public `ZYDIS_VERSION` and
`ZydisGetVersion()` are **4.1.0.0**. These values intentionally differ upstream.
The version test compares the function to the actual header macro; the artifact
audit also checks the expected public version. No upstream source patch changes
either value.

Required product Zycore closure consists of ten headers and **zero out-of-line
C units**; `ZyanAbsI64` is header-defined. Zycore `src/API/Terminal.c` belongs
only to the native tools and separate qualification companion, where the
original CLI uses it for TTY/color detection. It must not enter `TranslatedZydis`.
The source policy has no adaptations or upstream text patches.

`TranslatedZydisQualification` links the exact 18 product object files with
`tests/qualification.c` and original upstream `ZydisInfo`, `ZydisToolsShared`,
`ZydisFuzzShared`, `ZydisFuzzEncoder`, `ZydisFuzzReEncoding`,
`ZydisTestEncoderAbsolute`, plus test-only `Terminal.c`. Entry-point/target
renames are compile flags, not source edits. `tests/native_main.c` supplies the
native launcher. The generated fixture header uses actual upstream C types,
field names and enums. Test code, fixtures and fixed workloads stay outside the
delivered product. [CONTRACT.md](../tests/CONTRACT.md) lists the exact test units.

## Retained upstream cases and exclusions

The inventory is derived by importing `tests/harness.py:inventory` with the
shared `campaigns.recipes.helper`, resolving sources through the recipe and
[metadata](../config/inputs.json). Its order is sorted info filenames, then
zero-based encoder/reencoder JSON indices, then the absolute program. `input`
and `expected` paths are relative to the named upstream source root, never to a
particular checkout. Resolve that root from metadata or the current receipt.

- **210 info cases:** all 210 `tests/cases/*.in` and their 210 matching `.out`
  snapshots, using `shlex.split` on original arguments. The families are
  `3dnow` (25), `default` (49), `evex` (29), `jmp_far` (2), `mvex` (27),
  `vexc4` (28), `vexc5` (25), and `xop` (25). Every pair is individually listed.
  There are 207 nonempty snapshots and three intentional empty rejections.
- **52 encoder cases:** every element of `tests/enc_test_cases.json`, IDs
  `encoder/000` through `encoder/051`. Requests are constructed using actual C
  fields, including EVEX/MVEX fields. The schema-only `ZYDIS_ATTRIB_NONE` means
  integer zero, as in upstream's converter. Numeric bounds and operand counts
  are validated before header emission. The original fuzz target consumes the
  complete request after the operand-type offset expected by its sanitizer.
- **122 reencoder cases:** every element of `tests/re_enc_test_cases.json`, IDs
  `reencoder/000` through `reencoder/121`. Actual machine-mode/stack-width enum
  objects precede fixture payload bytes; complete stream consumption is checked.
- **One absolute case:** the entire original
  `tools/ZydisTestEncoderAbsolute.c` program (`absolute/original`), including
  branching, EIP/RIP-relative and range iterator loops. It is one report row,
  not one assertion or one encoding.

Thus there are **385 upstream case rows**, 422 fixture data files (420 info
files and two JSON collections), and one absolute test program. The original
Python `regression.py` and `regression_encoder.py` are behavioral drivers whose
complete retained inputs/programs are covered here. They are not separately
executed; the campaign runs their original C targets with stronger accounting.
`binary_reader.py`, `binary_writer.py`, `crash_tool.py` and
`zydis_encoder_types.py` are serialization/conversion helpers, not additional
behavioral suites. The harness avoids their hardcoded Python ABI layouts.

The inventory explicitly records the remaining files and reasons:

- `ZydisFuzzDecoder.c` is a real arbitrary-input fuzz target, but the archive has
  no separate retained corpus for it. Continuous fuzzing is not claimed;
  retained decoder cases run through `ZydisInfo`. `ZydisDisasm.c` is a streaming
  CLI application without a separate retained regression fixture collection.
- `Disassemble`, `DisassembleSimple`, `EncodeFromScratch`, `EncodeMov`,
  `Formatter01`, `Formatter02`, `Formatter03` and `RewriteCode` are usage
  examples, not retained behavioral regression suites. `ZydisPerfTest` is a
  throughput example; `ZydisWinKernel` requires Windows kernel integration.
  These applications, performance thresholds and kernel execution are outside
  this library qualification.
- Zycore `ArgParse.cpp`, `String.cpp` and `Vector.cpp` are actual behavioral
  suites for unused out-of-line implementations, outside this product closure.
  Zycore String/Vector examples likewise demonstrate those unused APIs.
  They are excluded, not counted as passing Zydis cases. Meson descriptions and
  example README files are build/documentation inputs, not behavioral tests.

No selected fixture is skipped or rebased. Excluded programs are distinct from
selected cases that execute and intentionally reject input.

## Native evidence and comparison rules

The final independent native report is
[zydis-native-report.json](../build/native-agent/controls-final/logs/zydis-native-report.json),
with [command receipt](../build/native-agent/controls-final/logs/receipt.json).
It records **393 passed rows: 385 upstream plus eight ABI/authored case rows,
zero failed and zero skipped**. These are case-level counts, not an instrumented
count of individual predicate evaluations. The original absolute program's
internal iterations and looped assertions are retained in transcripts rather
than inflated into additional report cases.

The 52 encoder rows mean successful upstream fuzz checks, not 52 successful
encodings. The native direct-encode observations are 10 successes (status
`00100000`), 35 impossible-instruction rejections (`8020000C`), and seven
invalid-argument rejections (`80100004`). The original fuzz target treats an
unencodable sanitized request as a successful test outcome; for successful
encodings it checks request/instruction identity and reencoding using its own
rules. The harness also prints the unsanitized request's direct status and
successful bytes. All of that output must agree with native. No local rule
requires byte identity for every semantically equivalent upstream reencoding.

Comparison has two levels:

1. Each info case's captured stdout must match its original `.out` bytes after
   **CRLF-to-LF only**, following upstream regression policy. No trimming,
   whitespace/case folding, status removal, numeric rewriting, pointer masking,
   token filtering or snapshot rebasing occurs. `NO_COLOR=1` and empty
   `FORCE_COLOR` fix CLI presentation. Ordinary info returns must be zero.
2. Each complete translated group stdout transcript must equal its newly built
   native transcript **byte for byte, without normalization**. This includes
   signed C returns, printed status words, metadata, formatted text, token
   sequences, encoded bytes and absolute-suite output. Stderr must equal the
   exact concatenated original rejection diagnostics for group 2, and be empty
   for all other normal groups.

Upstream's Python info driver ignores process status/stderr. This harness
separately compiles and invokes **unrenamed original `ZydisInfo`** for every
empty snapshot, verifying an empty stdout and recording its process exit/stderr.
Final independent controls establish:

| Case | Full C status | Original CLI exit | Exact stderr, followed by LF |
| --- | --- | --- | --- |
| `info/mvex_025` | `80200001` | 1 | `Could not decode instruction: DECODING_ERROR [0x80200001]` |
| `info/vexc4_026` | `80200003` | 3 | `Could not decode instruction: BAD_REGISTER [0x80200003]` |
| `info/vexc4_027` | `80200003` | 3 | `Could not decode instruction: BAD_REGISTER [0x80200003]` |

For these cases the batch C return's low byte must equal the independently
observed CLI exit; a nonzero return also requires an empty expected snapshot and
the Zyan failure bit. The exact native/managed transcript comparison preserves
the full signed return, not merely its low byte. The inventory stores both
representations and the exact diagnostics. See native
`logs/zydis-original-info-<case>.log/.stderr` and the report's
`original_rejection_controls`.

Group summary markers and all ordered upstream IDs must be present exactly.
Duplicate, missing, mismatched or incomplete BEGIN/END markers, `CHECK FAILED`,
unexpected stderr, nonzero group exits, signals and comparison failures fail
the run. Abort/crash output cannot disappear as an uncounted case. The companion's
`zydis-test-inputs.json` must match current source/config/fixture content identity;
a stale companion is rejected even with provenance hashes set to `off`.

Group **5** deliberately executes an always-active `ZQ_CHECK` failure. Every
new native/managed qualification executable must exit **1** and print both
`CHECK FAILED` and `deliberate failure control`. It is retained as an expected
failure command, not counted among passing cases. Authored `ZQ_CHECK`/`ZQ_OK`
checks run with `NDEBUG`; ordinary upstream `ZYAN_ASSERT` macros follow the
upstream release profile. Original explicit aborts, status checks and test-program
boolean results remain in force.

Native CLI harness repairs were confined to authored test code: use zero for
the fixture schema's zero-attribute name; expect upstream's actual invalid
mnemonic status; copy arguments into fresh writable storage because original
`ZydisInfo` edits argv in place; and validate intentional rejection status/stderr
instead of assuming every info input returns zero. Earlier failed attempts are
preserved. No upstream source or expected snapshot was patched.

## ABI, controls and callback ownership

Group 0 reports **18 actual type size/alignment pairs, 26 field offsets and two
initialized storage images**. Native alignment is observed with `offsetof` in
global `{ byte prefix; TYPE value; }` wrappers; `_Alignof` is checked against that
storage offset. Managed [ActualAbi.cs](../tests/Qualification/ActualAbi.cs) uses
`sizeof` on the actual emitted CLR types, actual field address differences and
wrapper storage offsets, then compares the same ordered observations to native.
It also writes the actual decoded-immediate union and encoder operand fields and
compares their entire byte images. There are no ABI mirror structs or acceptance
based solely on C layout literals. `ZydisFormatterFunc` uses pointer-sized CLR
storage; invocation is tested separately through real callback signatures.

The observations cover Zyan bool/size/status/string, the machine-mode enum,
decoder/context, decoded instruction/operand, encoder operand/request,
formatter/context/buffer/token/function pointer, register context and simple
disassembly result. Selected offsets include overlapping decoded operand arms,
encoder substructures/inline operands, instruction raw/AVX fields, the formatter
hook, context cookie, token linkage and disassembly text. Native receipts retain
the exact values; passing actual CLR storage remains a managed acceptance gate.

Group 1 has exactly seven named case rows:

- `version-features`: version equality and every upstream feature status printed
  for native/managed comparison.
- `decode-format-tokenize-modes`: all six upstream machine modes; full versus
  split decode; three formatter styles (Intel, AT&T, Intel MASM); token bounds,
  text reconstruction and terminal status; visible operand formatting/tokenizing;
  simple Intel/AT&T disassembly. Token views are consumed within their buffer's
  lifetime; retained text is copied.
- `formatter-hook-context-lifetimes`: two formatter instances with distinct
  contexts, 12 successful calls each, exact hook query/exchange identities,
  retained independent text, a thirteenth first-context call returning the exact
  failure status, then restoration and formatting after context invalidation.
- `ordinary-errors`: 12 exact-status checks covering null/bad decoder setup,
  empty/truncated/overlong input, illegal lock, invalid decoder option,
  formatter style/property, short formatter output, invalid mnemonic and short
  encoder output.
- `encode-convert-roundtrip-absolute`: new requests, decoded conversion and
  exact authored roundtrip bytes in 16/32/64-bit modes; absolute branch encoding
  and address calculation.
- `nop-fill`: every requested length 0 through 64, guards on both sides, and
  decoding every emitted padding instruction as NOP.
- `utilities-register-context-segments`: register-context effective address,
  segment bounds, register widths/enclosing names, mnemonic and ISA metadata.

Before group 1, companion `ManagedHooks.cs` tests a managed pre-instruction
callback, two separately `GCHandle`-rooted contexts, forced collections,
identity/query behavior and caught exception-to-status mapping. It adds checks,
not additional native report rows.

The separate [ManagedBoundary](../tests/ManagedBoundary/README.md) executable
references the delivered product and its emitted runtime. Its five cases are
`formatter-intel`, `formatter-att`, `formatter-status-failure`,
`formatter-contained-exception`, and `owning-snapshots-and-slices`. They exercise
both actual formatter callback signatures, saved defaults and exact restoration,
a 64-bit runtime address, exact instruction/operand/context identities, and
syntax-specific callback order/text. Normal GCHandle roots protect state;
compacting collections run before entry and inside callbacks before recovering
state. Borrowed arrays remain pinned through synchronous calls; fresh pin
addresses and buffer canaries check them after collections. State must become
collectible after restoration and release.

Callbacks use managed `delegate*` signatures matching the emitted fields. Test
callbacks catch their own deliberate exception, preserve its identity, and return
`ZYAN_STATUS_INVALID_OPERATION`; this is host policy, not automatic containment
by low-level Zydis. A separate exact `OUT_OF_RANGE` callback failure stops later
register callbacks. Callers must retain callback/context roots and pin borrowed
managed storage for the entire call, restore/remove hooks before releasing those
roots, and copy token text before reusing its backing buffer.

Owning tests verify both instruction flag pointers are cleared while their
values are copied; editing metadata/union/request copies cannot change owned
snapshots or inline operands. Mutating the caller's input and collecting leaves
owned bytes and exact `4883C02A` encoding intact. Success and short-destination
failure preserve guards around borrowed output slices; failure reports zero
written and the request remains reusable. Destination contents inside the failed
slice are unspecified.

## API workflow and acceptance matrix

The ordinary [consumer](../samples/ManagedConsumer/README.md) references
`TranslatedZydis` via `TranslatedProject`. `InstructionCodec` returns owning
instruction/operand/byte snapshots, formats Intel/AT&T text, and converts decoded
instructions to immutable `EncoderRequest` values for mutation and encoding.
The public emitted `Managed.Zydis.Zydis` surface remains available for advanced
settings/hooks. There is no runtime source generation or native Zydis backend.

Default execution asserts 16/32/64-bit decoding, metadata/flags, original and
mutated request independence, new request creation, operand-aware roundtrips,
absolute relocation, NOPs, exact ordinary status/byte-offset errors, survival of
input overwrite/GC, and 64 concurrent iterations with shared immutable results.
Its one workflow success marker is not a case-count report. `TryDecode` and
`TryEncode` preserve upstream status; throwing APIs use `ZydisException`, while
programming errors use argument exceptions. Inputs must remain stable during a
call. The product contains no demo entrypoint.

The managed rows below have passed. Each four-cell row means raw/JIT,
processed/JIT, raw/AOT and processed/AOT on linux-x64.

| Gate | Executed scope required | Status |
| --- | --- | --- |
| Native | 393 rows, independent rejection controls and failure control | Passed; final independent native receipts above |
| `abi` | Group 0 in all four cells, including actual CLR storage checks | Passed; 20261027-072757-6171e6fa |
| `qualification` | Group 1 in all four cells, including companion managed hooks | Passed; 20261027-072757-6171e6fa |
| `upstream` | Groups 2/3/4: 385 rows in each of four cells | Passed; 20261027-072757-6171e6fa |
| `rooted-aot` | Groups 0–4 in both forms; root `TranslatedZydisQualification` with `RootQualificationLibrary=true` | Passed; 20261027-072757-6171e6fa |
| `managed-boundary` | Five delivered-product cases in all four cells; AOT roots `TranslatedZydis` with `RootTranslatedLibrary=true` | Passed; 20261027-072757-6171e6fa |
| `consumer` | Ordinary useful workflow in all four cells; normal trimming | Passed; 20261027-072757-6171e6fa |
| `audit` | Both current delivered forms, exports/imports/dependencies/notices, exact current qualification/consumer evidence and AOT binaries | Passed; 20261027-072757-6171e6fa |
| Private reproduction | Fresh generation/matrix from current authored inputs and final tools, preserved inputs | Passed; 20261027-072757-6171e6fa |

Rooted companion and rooted delivered-product gates establish different
closures; neither substitutes for the normally trimmed ordinary consumer.
The audit uses current-context reports and command evidence, not a glob of
historical successes. Its [policy](../config/audit-policy.json) distinguishes
shared runtime platform-service imports from prohibited native Zydis backends.
Static source checks alone do not establish behavioral or AOT acceptance.

## Commands and clean private reproduction

These are parent execution commands, documented here but not run by this docs
worker. Requirements are Python 3.11+, .NET 10 SDK, linux-x64 NativeAOT toolchain,
a C11 compiler and the pinned local source trees. The shell wrappers dispatch to
the [shared framework](../../docs/campaigns.md); invoke them from any directory.
NuGet restore is separate from upstream fetching.

From the repository root, the full final gate is:

```bash
bash Scripts/campaign.sh layout zydis --check
bash zydis/scripts/verify.sh --fetch never --tools reuse --restore normal \
  --hashes warn --form all --mode all --rid linux-x64
```

`verify` regenerates both product forms and both companion forms from the full
selection, builds them, then runs inventory, native, abi, qualification, upstream,
rooted-aot, managed-boundary, consumer and audit in recipe order. `--tools reuse`
requires already built final compiler/postprocessor closures; use `--tools build`
when they must be rebuilt. `--hashes warn` accepts usable edited trees with
provenance warnings; `off` disables historical provenance enforcement and
`strict` explicitly requires pinned integrity. Structural and behavioral checks
remain mandatory in every mode.

For a targeted rerun against current generated products:

```bash
bash zydis/scripts/test.sh --fetch never --tools reuse --form all --mode all \
  --rid linux-x64 --suite native --suite abi --suite qualification \
  --suite upstream --suite rooted-aot --suite managed-boundary --suite consumer --suite audit
```

Keep these suites in one invocation when auditing current evidence. A native-only
control run never builds managed tools/libraries:

```bash
python3 -B zydis/scripts/qualification.py --native --work zydis/build/native-controls-new
```

For manual ordinary API use after generation:

```bash
dotnet run --project zydis/samples/ManagedConsumer -c Release
dotnet run --project zydis/samples/ManagedConsumer -c Release -- \
  disassemble 64 "48 83 C0 2A C3" 0x1000 att
dotnet run --project zydis/samples/ManagedConsumer -c Release -- \
  roundtrip 32 "B8 78 56 34 12 C3"
dotnet run --project zydis/samples/ManagedConsumer -c Release -- \
  rewrite-immediate 64 "48 83 C0 2A" 1 0x33
```

The following creates a new ordinary-copy stage of the **current working-tree
bytes**, including untracked authored files, not just the last commit. It
excludes build/generated/ref outputs and copies only the two metadata-selected
upstream trees. It reuses final tools through the framework's private tool
snapshots, while every translated library, companion and managed test executable
is freshly generated/built inside the stage. Run after authored inputs/tools are
frozen; do not run concurrently with parent tool replacement.

```bash
set -euo pipefail
ZYDIS_REPRO_SOURCE="$(pwd)"
ZYDIS_REPRO_STAGE="$(mktemp -d "${TMPDIR:-/tmp}/zydis-repro.XXXXXXXX")"
export ZYDIS_REPRO_SOURCE ZYDIS_REPRO_STAGE
export DOTCC_COMPILER="$ZYDIS_REPRO_SOURCE/DotCC/bin/Release/net10.0/dotcc.dll"
export DOTCC_POSTPROCESSOR="$ZYDIS_REPRO_SOURCE/DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"
python3 -B - <<'PY'
import json, os, shutil, subprocess
from pathlib import Path
source = Path(os.environ['ZYDIS_REPRO_SOURCE']).resolve()
stage = Path(os.environ['ZYDIS_REPRO_STAGE']).resolve()
assert source != stage and not stage.is_relative_to(source)
names = subprocess.check_output([
    'git', '-C', str(source), 'ls-files', '-z', '--cached', '--others', '--exclude-standard'
]).split(b'\0')
for name in sorted(set(names)):
    if not name:
        continue
    relative = Path(os.fsdecode(name))
    if {'bin', 'obj', '__pycache__'} & set(relative.parts):
        continue
    if len(relative.parts) > 1 and relative.parts[0] == 'zydis' and relative.parts[1] in {
        'build', 'artifacts', 'generated', 'ref'
    }:
        continue
    path = source / relative
    if path.is_file():
        target = stage / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, target)
pins = json.loads((source / 'zydis/config/inputs.json').read_text())
for key in ('zydis', 'zycore'):
    relative = Path('zydis/ref') / pins[key]['directory']
    shutil.copytree(source / relative, stage / relative)
print(stage)
PY
cd /tmp
bash "$ZYDIS_REPRO_STAGE/zydis/scripts/verify.sh" --fetch never --tools reuse \
  --restore normal --hashes warn --form all --mode all --rid linux-x64
```

This intentionally reuses local acquired inputs and tools; it is a clean-output
reproduction, not a network acquisition or compiler-bootstrap claim. To audit
acquisition, separately use a fresh ref stage with `--fetch missing`; an offline
`strict` audit also needs the metadata-selected archives. Preserve before/after
content manifests of authored inputs and original ref trees and compare them;
retain the existing parent's
[authored manifest](../artifacts/supervisor-probe/authored-before-reproduction.json)
and [preexisting preservation receipt](../artifacts/supervisor-probe/preexisting-preservation.json).
Do not delete canonical products or unrelated files to simulate cleanliness.

Each framework run records argv, exits, selected forms/modes, source policy,
private tool snapshots and attached reports under `artifacts/campaign/<run-id>`;
work is under `build/campaign/<run-id>`. Expected attachments include
`zydis-native-report.json`, `zydis-abi-report.json`,
`zydis-qualification-report.json`, `zydis-upstream-report.json`,
`zydis-rooted-aot-report.json`, `zydis-managed-boundary-report.json` and
`zydis-audit.json`. Consumer success/output and rooted-vs-ordinary AOT command
properties live in the same receipt/logs. Parent acceptance requires the full
current receipt, exact counts/comparisons, artifact audit and private reproduction
passed in the current final receipt. The optional ordinary-copy stage above is
a documented alternative; the accepted run used clean framework staging from /tmp.

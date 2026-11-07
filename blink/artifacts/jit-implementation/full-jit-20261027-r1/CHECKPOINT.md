# Full-JIT checkpoint — paused 2026-11-07

This checkpoint finishes the c179 shared-read/completion milestone. Development
is **PAUSED at the user's request**. The goal of repeatably beating Interpreter
is **not achieved**. No further optimization or coverage milestone is queued.
Full remains opt-in; Interpreter remains the default.

## Included implementation and validation

The checkpoint includes the retained multi-instruction native compiler/controller,
charged arenas and reusable scratch, direct links and worker caches, code/mapping
invalidation, precise Linux fault/signal/budget handling, native arithmetic,
checked memory, atomics, strings/REP, SIMD/MMX and the finite canonical raw80 x87
software profile. Syscall/mapping/fault and clock/entropy sampling remain explicit
runtime boundaries. Ordinary admitted computation executes emitted semantics,
not managed Op trampolines. This is an independent implementation with no QEMU
code reuse. Host/runtime glue, producer adapters and the bounded Libc mutex-cache
change required by the selected producer are included.

The last batch adds exact observer-owned **local memfd Read** cache authority and
withdrawal barriers. Writes, foreign/regular shared backing and RMW remain
conservative. Plain completion increments are selected only for proven
owner-private native counters and atomically published after RET and the x87
guard. General emitters and shared Issue/Budget counters retain locking.

Linux CoreCLR and NativeAOT each pass the targeted c179 family (9,758,014
assertions) and broad combined family (67,084,789 assertions). Reused numeric
assertions contribute to those counts; they do not measure instruction coverage.
The current expanded crossing-page atomic guest was rebuilt byte-exactly and
also passes both runtime atomic-family gates. The earlier broad gate used the
original guest; both identities and their separate receipts are retained.
R199/R208 found no new scoped source P1/P2; they give no blanket runtime clearance.
See [validation](checkpoint-validation.json), [scope](checkpoint-source-manifest.json)
and [review dispositions](checkpoint-review-dispositions.json).

## Matched service results

The completed c179-a18 pilot has one alternating matched pair per host/guest
cell, two fresh owners per process and 46 exact HTTP requests per owner. All
16 owners functionally passed; all eight processes were reaped. Import is
separate; Run includes startup, compilation and first-use costs. Warm covers
40 requests per owner. Compilation wall sums are never subtracted from CPU.
This is a small Kestrel health/echo service, not arbitrary service coverage.

| Host / Linux guest | Run CPU Full/Interpreter | Run wall | Warm CPU | Warm wall | Run managed allocation |
|---|---:|---:|---:|---:|---:|
| CoreCLR / NativeAOT | 1.194 | 1.113 | 1.354 | 1.355 | 7.555 |
| CoreCLR / CoreCLR | 2.637 | 2.810 | 0.698 | 0.834 | 2.439 |
| NativeAOT / NativeAOT | 0.969 | 0.924 | 1.131 | 1.247 | 8.179 |
| NativeAOT / CoreCLR | 2.110 | 2.322 | 0.526 | 0.799 | 2.352 |

One pair is **not repeatability acceptance**. Warm IL improvement does not erase
startup costs, slower warm AOT requests or greater allocation. The campaign
runner's exit 1 rejects the shortened pilot against its full acceptance shape;
no guest process failed in this completed pilot. Earlier failed/censored
c169/c172/c177 observations remain separate evidence and are not discarded.
[Machine-readable results](checkpoint-service-results.json) retain absolute
CPU/wall/allocation totals and receipt hashes. No build or profiler overlapped
this pilot; bounded source-only review reads did overlap.

## Remaining limits and next work, deferred

Full native completeness remains **MISSING**. The historical 538-arm inventory
and bounded 413-arm native witnesses are not all-form coverage. Unknown operand,
width, prefix, addressing and context cells remain explicit. Nonzero nested ENTER,
nonempty-GDT production ownership, broader AM/AC contexts and Linux port-string
recovery are among the visible gaps. Selected extensions and undefined aliases
are not architectural ordinary coverage. See [coverage](checkpoint-coverage.json).

Canonical x87 is active only inside its finite software profile. PC01 arithmetic
uses PC64 while preserving raw CW; omitted protected16 FOP restores as zero;
FX requires 512-byte permissions while copying the selected 416-byte data image.
The portable production catalog is integer-native; the historical host-private
x87 experiment has an unresolved host-state boundary and is not selected.
Windows execution is **UNVERIFIED** and generated asynchronous unwind/stackwalking
is **UNQUALIFIED**. R101-04 terminal generation cleanup remains **OPEN**.

On resume, profile substantive startup/native-return costs first. The measured
next candidates are authenticated private regular-file read ownership (remaining
shared reads still use tickets), precise physical write footprints instead of
page-wide code invalidation, and avoiding generic authority pulses for metadata-
only data identity registration. R209 is an **unapplied, unbuilt partial** preserved
in the local ledger, not completed checkpoint code. Resume those only after the
user resumes development; do not automatically restart agents or measurements.

## Inputs, build and resume commands

Generated source bulk and scratch/evidence binaries are deliberately **not in
this Git commit**. A fresh clone is not a complete producer distribution. The
required exact 4,409,099-byte producer ZIP is retained locally at:

`/mnt/storage/blink-full-jit-checkpoint-inputs-20261107/native14-processed.zip`

Its SHA256 is `e75000b2f921bffb8afaf83ec8730d2e60db7464595581ac7439f10f1181acef`.
The current workspace copy is installed under
`blink/src/Managed.Emulation.FullCore/Producer/` and ignored. Restore on another
checkout, then use a new disposable output tree:

```sh
python3 -B blink/scripts/restore-full-core-input.py /absolute/path/native14-processed.zip
checkpoint_out=/mnt/storage/blink-full-jit-resume
mkdir -p "$checkpoint_out/tmp"
export TMPDIR="$checkpoint_out/tmp" TMP="$checkpoint_out/tmp" TEMP="$checkpoint_out/tmp"
export DOTNET_CLI_HOME="$checkpoint_out/dotnet-home"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
project=blink/tests/GuestJit/ContractTests/ContractTests.csproj
# Run these serially. NativeAOT requires the platform's .NET10 native toolchain.
dotnet build "$project" -c Release --disable-build-servers -m:2   -p:UseSharedCompilation=false -p:BuildInParallel=false   --artifacts-path "$checkpoint_out/coreclr/artifacts" -o "$checkpoint_out/coreclr/out"
dotnet "$checkpoint_out/coreclr/out/ContractTests.dll" owned-shared-read-family
dotnet publish "$project" -c Release -r linux-x64 -p:PublishAot=true   --disable-build-servers -m:2 -p:UseSharedCompilation=false   -p:BuildInParallel=false -p:IlcSingleThreaded=true   --artifacts-path "$checkpoint_out/aot/artifacts" -o "$checkpoint_out/aot/out"
"$checkpoint_out/aot/out/ContractTests" owned-shared-read-family
# The registered nested-signal/crossing-page guest is an input, never a host probe.
gcc -O2 -static -fno-pie -no-pie blink/tests/FullJit/atomic-family-r156.c   -o "$checkpoint_out/atomic-family-r156.elf"
dotnet "$checkpoint_out/coreclr/out/ContractTests.dll" native-family-batch "$checkpoint_out/atomic-family-r156.elf"
"$checkpoint_out/aot/out/ContractTests" native-family-batch "$checkpoint_out/atomic-family-r156.elf"
```

Use `-p:BlinkX87StateOwner=false` only for the separately inactive compatibility
configuration. Do not silently substitute another generated producer: private
identifiers depend on absolute source paths, including the RNG binding. The
source archive includes legal/provenance data and compiler-resource proof.
Retained original producer tools/inputs permit explicit future reproduction;
a new producer still needs its own binding and qualification.

Source adapter tests use the upstream input from `blink/config/source-manifest.json`
and the retained 98-unit preclone corpus. The latter is locally preserved at
`checkpoint-inputs/x87-adapter-bound` in this ledger (not committed generated bulk).
Set `BLINK_X87_ADAPTER_INPUTS` to a restored corpus on another checkout. It fails
rather than silently skipping when inputs are missing. `BLINK_TEST_SCRATCH`
selects a disposable `/mnt/storage/blink-*` test directory.

The pinned AOT/IL guest images remain under the pre-existing
`blink/artifacts/jit-opportunities/astra-20261026-r1/images-{aot,il}` inputs;
[retained input identities](checkpoint-retained-inputs.json) specify the exact
paths. Shared NuGet caches, SDKs and unrelated projects are untouched. Historical
absolute scratch paths in receipts are provenance, not current build inputs.
[Cleanup](checkpoint-cleanup.json) records actual reclaimed space, retained
paths and process checks. Detailed raw records and authored frozen handoffs are
local ignored evidence; this commit contains concise plain-text resume records.

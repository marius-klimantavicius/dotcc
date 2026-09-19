# Blink through dotcc

The selected Blink interpreter executes Linux x86-64 instructions as translated
C# on Linux x64. The current compiler translates all 109 selected sources through
`blink/scripts/translate.sh`, which publishes the post-processed project at
`blink/generated/TranslatedBlink/TranslatedBlink.csproj` and preserves a separate
immutable raw snapshot. The actual normal-core usage sample passes JIT and
NativeAOT against native arithmetic, memory, budget and guest-exit observations.
The normal CPU corpus passes 504 cases in each of raw/optimized JIT/NativeAOT,
for 2,016 comparisons, with 46 historical custom fault cases explicitly excluded.
Actual guest-memory lifecycle and valid ELF/fixed TLS tests also pass all four
forms, completing the finite selected-profile P3 gate.

Earlier CPU, host-service and dependent-campaign results are historical where
they predate the shared compiler changes made during libsmb2. Custom fault
injection and invalid/malformed ELF cases are excluded; only existing pinned
upstream fault cases may enter new qualification.

P4's selected host contracts now pass native and all four managed forms through
actual guest syscall fixtures: files/descriptors, environment/signal state,
finite TCP exchange, and exact standard-stream capture. Callback I/O cancellation
is implemented, qualified and included in the final generated project. P4 stops
at the remaining owning execution/deadline and actual service-startup blocker;
see [the P4 ledger](docs/P4-HOST-SERVICES.md). No P5/P6 phase has started here.

The service product is incomplete. Actual managed HTTP service startup, the
translated worker and Windows execution remain open. The P3 pass is bounded
selected-profile coverage, not exhaustive ISA or general dynamic TLS support.
The independent controller has subprocess lifecycle tests; it does not
yet provide a qualified translated-service worker. See [progress](docs/PROGRESS.md),
[validation](docs/VALIDATION.md), [blockers](docs/BLOCKERS.md) and the
[implementation plan](docs/PLAN.md) for exact evidence and remaining gates.

## Generate and use the final project

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh
dotnet build blink/ManagedConsumer.slnx -c Release
dotnet run --project blink/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build
```

See [translation delivery](scripts/TRANSLATION.md), the
[usage sample](ManagedConsumer/README.md) and
[clean delivery verification](scripts/CLEAN-DELIVERY.md). Finish generation before
building consumers. The sample owns a single normal interpreter run; the planned
owning HTTP service API remains unqualified.

## Reproduce the core gate

Run from the repository root on Linux x64 with .NET 10, Python 3, GCC/binutils,
make and the .NET NativeAOT prerequisites. Build the compiler before beginning
a campaign run; changing its binaries invalidates frozen profile/object inputs.

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/fetch.sh
bash blink/scripts/native-oracle.sh --offline
bash blink/scripts/probe-core.sh --stage-only
```

The last command prints a new profile directory and writes it to
`blink/artifacts/core/latest-profile.txt`. Use that exact directory:

```bash
python3 blink/scripts/assemble-core.py --profile <profile-directory> --jobs 4
python3 blink/tests/CoreExecution/run.py --assembly-receipt <printed-object-receipt>
python3 blink/scripts/audit-published-core.py <printed-execution-receipt>
```

Profiles preserve immutable source/configuration/compiler hashes. Object reuse
requires matching emission identities, including canonical physical header
paths. Never combine objects from different storage formats or edit generated
C#. The execution runner builds separate raw/optimized libraries and consumers,
checks native/configured ABI and behavior, roots the complete library for AOT,
and audits direct IL imports and initializers before execution.

For an isolated repetition from an empty detached checkout, see
[clean reproduction](scripts/CLEAN-REPRODUCTION.md). That runner records the
reproduced commit and uses only the pinned source archive plus installed host
tools and normal package restore; it does not copy existing generated outputs.

Additional harnesses (consult the ledger for current versus historical evidence):

- [CPU conformance](tests/CpuConformance/README.md): hardware, original native,
  reviewed staged native, and four managed forms; original failures are retained.
- [Valid ELF loading](tests/ElfLoading/README.md): pinned bytes, BSS, page
  permissions, initial stack and cleanup; no service instructions execute.
- [Valid TLS startup](tests/TlsLoading/README.md): fixed static template, loaded
  program header, explicit runtime initialization and FS-relative access.
- [Core ABI](tests/CoreAbi/README.md): 236 actual layout/register measurements.
- [Controller lifecycle](tests/InstanceLifecycle/README.md): independent managed
  subprocess fixtures for stop, deadline, protocol bounds and pipe cleanup.
- [Interpreter throughput](tests/CoreThroughput/README.md): fixed hot-loop
  measurement with exact semantic gates and explicit performance limits.
- [Boundary inventory](tools/BoundaryAudit/README.md): direct IL graph and
  publication evidence with indirect-call/framework limitations stated.

Each harness writes attempt-specific receipts and logs under `blink/artifacts`.
The `ref`, `generated`, `build` and `artifacts` trees are ignored; scripts,
source adaptations, pins and durable interpretations are committed. Offline
reruns require their checksum-verified source/tool archives to be cached first.

## Profile and ownership

Guest addresses use Blink's explicit page translation. Host backing memory is
ordinary non-executable data; guest permissions are checked by the interpreter.
The authored host provides private files, descriptors, TCP endpoint mappings,
limits, clocks and entropy. See [host contracts](docs/HOST-CONTRACT.md) for the
implemented semantics and explicit unsupported operations.

The current ownership rule is one active translated context per worker process.
The qualified harnesses discard their worker after cleanup; reuse of arbitrary
process-global interpreter state is not established. Managed emulation and an
ordinary same-user worker are not a hardened sandbox. Dynamic distro userspace,
guest fork/exec/threads, native JIT, kernel boot and general Linux compatibility
are outside the first profile.

Automated review stopped the service-worker task. The partial worker was never
compiled or executed, and that gate remains unqualified. By explicit user
direction, malformed-ELF handling and qualification are excluded from campaign
completion; earlier blocked attempts remain historical evidence, not passes.

## Shared compiler regressions

Historical Linux checks cover the owning SQLite consumer and all seven SQLite C
corpora in raw/optimized JIT/NativeAOT, the complete existing picotls campaign,
MsQuic ABI/product/public-consumer cases, Lua/chibi JIT suites, 146 WAT cases and
all 205 Zig oracle cases. Exact scope and receipts are in
[validation](docs/VALIDATION.md). The `scripts/test-*-regression.py` runners and
`scripts/test-sqlite-corpora.py --cache <verified-SQLite-archives>` preserve
commands and identities; provide the pinned archive cache explicitly when
reproducing in another checkout. These runs do not qualify Windows or the
translated service worker.

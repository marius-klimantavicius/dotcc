# Blink through dotcc

The selected Blink interpreter executes Linux x86-64 instructions as translated
C# on Linux x64. The qualified delivery translates all 108 selected product sources through
`blink/scripts/translate.sh`, which publishes the post-processed project at
`blink/generated/TranslatedBlink/TranslatedBlink.csproj` and preserves a separate
immutable raw snapshot. Authored Host/bridge sources are linked directly from
`src`, and a separate authored C# API owns loading, execution and cleanup.
The actual ASP.NET Core/Kestrel NativeAOT guest and its final worker sample
pass the selected Linux x64 qualification.
Campaign C probes remain test-only.
The P5 normal CPU corpus passes 514 cases in each of raw/optimized JIT/NativeAOT,
for 2,056 comparisons, with 46 historical custom fault cases explicitly excluded.
Actual guest-memory lifecycle and valid ELF/fixed TLS tests also pass all four
forms, completing the finite selected-profile P3 gate.

Earlier CPU, host-service and dependent-campaign results are historical where
they predate the shared compiler changes made during libsmb2. Custom fault
injection and invalid/malformed ELF cases are excluded; only existing pinned
upstream fault cases may enter new qualification.

P4's selected host contracts now pass native and all four managed forms through
actual guest syscall fixtures: files/descriptors, environment/signal state,
finite TCP exchange, and exact standard-stream capture. Callback I/O cancellation
is implemented, qualified and included in the final generated project. Actual
service startup and all six pinned HTTP cases pass all four managed forms.
Eight native completion controls and 44 managed cases also qualify owning stop,
poll/sleep deadlines, inherited-I/O cancellation and instruction budgeting.
P4 is complete for the selected Linux x64 profile;
see [the P4 ledger](docs/P4-HOST-SERVICES.md).

P5 is complete for the selected genuine static-musl ASP.NET Core/Kestrel guest.
All four host modes pass 16 actual workers and 40 native-reference HTTP
comparisons, simultaneous private instances, restart, cooperative stop and
natural idle deadlines. The actual showcase solution and JIT/NativeAOT sample
also pass. Public translation defaults to that threaded profile. See
[runtime evidence and limits](docs/P5-KESTREL-RUNTIME.md) and
[final receipts](docs/VALIDATION.md).

The P6 public `BlinkMachine` API now passes JIT/NativeAOT in both execution
modes: in-process by default or explicitly separate-process with an
automatically deployed worker. It supports direct mounted ELF execution, live
RW mounts by default, RO/COW, private persistence, resource limits, binary
console streams, guest argv/cwd/environment and explicit network grants.
Both forms qualify overlapping Kestrel instances, restart and cleanup.
See [the machine API](src/Managed.Emulation/MACHINE-API.md) and
[current progress](docs/PROGRESS.md) for final receipts and limits.
Windows and the separate P7 qualification/performance campaign remain unrun.

Optional [IMDSv2 simulation](src/Managed.Emulation/IMDSV2.md) supplies private
instance metadata and explicit role credentials at guest `169.254.169.254:80`.
It uses a small BCL listener and works with ordinary AWS SDK credential providers
in both execution modes. See the [NativeAOT AWS SDK example](tests/ImdsMachine/README.md).

The P3 pass is finite selected-profile coverage. Broader ISA, dynamic ELF/TLS
and unrelated runtime compatibility are not implied. See [progress](docs/PROGRESS.md),
[validation](docs/VALIDATION.md), [blockers](docs/BLOCKERS.md) and the
[implementation plan](docs/PLAN.md) for evidence and remaining gates.

## Generate and use the final project

```bash
dotnet build dotcc.sln -c Release -p:UseLocalLalrCc=false
bash blink/scripts/translate.sh
dotnet build blink/ManagedConsumer.slnx -c Release
dotnet run --project blink/ManagedConsumer/ManagedConsumer.csproj -c Release --no-build -- \
  blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService
```

See [translation delivery](scripts/TRANSLATION.md), the
[usage sample](ManagedConsumer/README.md) and
[clean delivery verification](scripts/CLEAN-DELIVERY.md). Finish generation before
building consumers. Append `SeparateProcess` for an explicit worker; otherwise
execution is inside the caller. `--run ./work /work/my_app` demonstrates direct
mounted console/folder execution, with `--run-process` selecting a worker.
Both actual JIT and NativeAOT integration forms pass for the pinned Kestrel ELF.

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
guest fork/exec, arbitrary threading semantics, native JIT, kernel boot and
general Linux compatibility remain outside this finite profile.

An earlier automated review stopped a service-worker attempt. Its partial worker
was never qualified; that historical outcome is not a permanent rule against
the currently authorized P5 work. By explicit user
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

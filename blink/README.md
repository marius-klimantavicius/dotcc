# Blink through dotcc

The selected Blink interpreter executes Linux x86-64 instructions as translated
C# on Linux x64. All 109 selected sources build, and bounded instruction, fault,
exit and ABI checks pass under raw/optimized JIT and NativeAOT. The reviewed
CPU and scalar floating-point corpus passes 495 cases in each form. Valid loading of
the pinned service ELF is also qualified.

The service product is incomplete. Actual managed HTTP service startup, the
translated worker, complete CPU/ELF/memory coverage and Windows execution remain
open. The independent controller has subprocess lifecycle tests; it does not
yet provide a qualified translated-service worker. See [progress](docs/PROGRESS.md),
[validation](docs/VALIDATION.md), [blockers](docs/BLOCKERS.md) and the
[implementation plan](docs/PLAN.md) for exact evidence and remaining gates.

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

Additional qualified harnesses:

- [CPU conformance](tests/CpuConformance/README.md): hardware, original native,
  reviewed staged native, and four managed forms; original failures are retained.
- [Valid ELF loading](tests/ElfLoading/README.md): pinned bytes, BSS, page
  permissions, initial stack and cleanup; no service instructions execute.
- [Core ABI](tests/CoreAbi/README.md): 236 actual layout/register measurements.
- [Controller lifecycle](tests/InstanceLifecycle/README.md): independent managed
  subprocess fixtures for stop, deadline, protocol bounds and pipe cleanup.
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

Automated review stopped the service-worker and malformed-ELF tasks. The partial
worker was never compiled or executed, and both gates remain unqualified.

# P6: public machine API, mounts, console and isolation

Requested after P5 completion on 2026-09-23. This is the next implementation
phase; the former P6 platform/performance qualification phase becomes P7.
This document specifies proposed behavior, not implemented or qualified APIs.
Execution snapshots are a future extension, not a P6 completion requirement.

## User-facing workflow

Create a machine, configure its resources and filesystem, select a guest
executable, then execute it with arguments, environment and console streams
**inside the caller's .NET process**. This is the user's required default,
not a facade over an automatically spawned worker. Keep generated machine
pointers out of ordinary application code. The public surface must remain
usable from both JIT and NativeAOT applications. Deliver both execution modes
in P6 through the same public API: `InProcess` (default) and explicitly selected
`SeparateProcess`. The latter reuses the existing process runner where suitable,
with separate guarantees and automatic worker discovery/deployment plus an
advanced explicit worker-location option. Never switch modes implicitly.

Illustrative API shape; names will be finalized during implementation:

```csharp
await using var machine = await BlinkMachine.CreateAsync(new MachineOptions
{
    ExecutionMode = ExecutionMode.InProcess, // Default; or SeparateProcess.
    MemoryLimit = 256 * 1024 * 1024,
    Environment = new Dictionary<string, string> { ["LANG"] = "C" },
    Network = NetworkPolicy.Isolated,
});

await machine.MountDirectoryAsync("./published-service", "/app", MountAccess.ReadOnly);
await machine.MountDirectoryAsync("./work", "/work"); // Default: live read-write.

var result = await machine.ExecuteAsync(new ExecutionOptions
{
    Executable = "/app/MyService",
    Arguments = ["--example"], // Excludes argv[0]; executable supplies it once.
    WorkingDirectory = "/work",
    Environment = new Dictionary<string, string> { ["APP_MODE"] = "demo" },
    Console = ConsoleOptions.AttachCurrent(leaveOpen: true),
});
```

Also support starting execution and obtaining a run handle immediately, to
consume streams, publish endpoints, inspect status or request stop while it
runs. `ExecuteAsync` is the convenient start-and-wait form. Service readiness
must be an explicit caller-selected condition; general programs need neither
an HTTP port nor a particular stdout marker. No guest is auto-stopped after
a fixture-specific health check in the general API.

## Machine and execution lifecycle

- Choose execution mode when creating a machine; it remains fixed for that
  machine's lifetime. Both modes expose the same mounts, environment, console,
  execution handles and result semantics. Report mode-specific capabilities
  explicitly and reject unsupported guarantees before starting execution.
- Separate machine configuration/filesystem lifetime from one execution.
  State transitions are explicit: created, running, exited/stopped, disposed.
  The caller's process owns execution threads and instance resources. Resolve
  the P5 one-execution-per-process restriction: audit and make mutable translated
  globals, caches, initialization, TLS and host bindings instance-safe, with
  deterministic cleanup and rebinding between runs. Qualify sequential reuse
  and concurrent independent machines within one CLR process. Concurrent runs
  on the same machine are rejected initially; separate machines must not share
  mutable guest state. A subprocess or process-wide execution lock is not a
  substitute for qualifying in-process execution. In separate-process mode,
  create a fresh worker per run while retaining machine-owned configuration
  and private filesystem state across runs.
- Preserve the machine's private writable filesystem across sequential runs
  until reset or disposal, with explicit export/discard operations. Restarting
  an executable is distinct from restoring its CPU/memory state.
- Allow mount/environment changes before a run or between runs. Freeze accepted
  options for a running execution; do not mutate its namespace concurrently.
- Provide completion, exit code/signal/reason, bounded diagnostics, instruction
  and resource observations and cooperative stop. Do not expose a purported
  safe force-kill of arbitrary in-process execution threads. Forced process
  termination is available in separate-process mode; distinguish a forced
  worker exit from a normal guest exit or cooperative stop.
  Distinguish canceling a wait from requesting guest termination. Dispose must
  stop execution and release execution-thread, file, stream and endpoint
  ownership after quiescence; never free guest memory while it is still in use.
- Expose configurable guest memory, writable storage, descriptor/thread limits,
  console buffering, instruction budget and optional execution deadline.
  Support long-running services without the current fixture's mandatory short
  deadline. Validate what the selected execution mode can enforce; never silently
  ignore a limit. Document guest virtual-address/backing limits separately from
  host CLR memory/CPU overhead. In-process deadlines depend on cooperative
  checkpoints and cancellable host operations, not OS termination guarantees.
  Separate-process mode may enforce worker-level limits with supported OS
  facilities; unavailable requested limits must not be silently ignored.

## Filesystem and mounts

Support image files/directories, private writable temporary storage and host
directory mappings at absolute guest paths. Mounting over an existing guest
directory must be explicit: hide its underlying contents while mounted, never
delete them or overwrite the host source. Define nested mount precedence,
duplicate mount rejection and cross-mount rename/link behavior.

| Mount mode | Behavior |
| --- | --- |
| Read-only host directory | Read existing host contents; guest writes fail. Host changes can be visible and are not an immutable snapshot. |
| Read-write host directory (default) | Live host reads/writes, confined to the explicitly mounted folder. Changes persist on the host. |
| Copy-on-write | Read from an imported/frozen base and keep guest changes private, with explicit export/discard. Host source remains unchanged; record copy/import costs and quotas. |
| Private memory/image storage | Machine-owned content with no implicit host path access. |

User-selected default: host folder mounts are live read-write mappings. Guest
writes update the host folder directly and persist after execution, reset or
  machine disposal. Callers may explicitly select read-only or private
copy-on-write behavior for each mount. Importing a directory must not be
described as a live mount. Access outside the granted mount remains denied.
These semantics hold in both execution modes: a separate worker must not turn
a live host mount into an input copy with delayed write-back.

Resolve executable paths and cwd through the guest filesystem; load from a
mounted executable as well as an image. Preserve normal guest relative paths,
directory enumeration, metadata, file updates and descriptor offsets. Enforce
executable permissions explicitly. Define symlink/hard-link/reparse behavior
within mount roots and nested mounts, including races; lexical path-prefix
checks alone do not establish containment. Never fall back to the host root.

## Environment and console

Use a dictionary of environment names/values with explicit machine defaults and
per-execution overrides/removals. Reject malformed names/NULs, use Linux guest
case sensitivity and do not inherit the parent's environment implicitly.
Expose guest arguments, cwd and supported virtual identity separately. Remove
Kestrel-specific defaults from the general execution API; keep any qualified
runtime compatibility settings explicit in service examples/profiles.

Expose binary stdin/stdout/stderr streams with concurrent async pumping and
bounded buffering/backpressure. Support closed stdin/EOF, caller-provided streams,
live output, optional bounded capture, and connection to the current console.
Guest descriptors must remain separate from application diagnostics and, in
separate-process mode, worker control channels. Do not globally redirect
Console or mutate process environment/cwd to implement guest IO or settings.
Document stream ownership/leave-open behavior; completion must not hang on a
console reader or slow/unread consumer. Define output-limit behavior explicitly.
Provide an explicit Ctrl+C/interrupt policy using supported guest signals or
stop semantics, without accidentally terminating the controller application.
P6 console support means redirected streams; PTY/terminal editing, resize and
job-control semantics are separate capabilities and must not be implied.

## Isolation contract

The user's requirement is to sandbox the machine. Make allowed capabilities
explicit and deny ambient access: private guest memory, filesystem namespace,
environment, descriptors, process identity and network namespace. Host mounts,
ports and any outbound network access are deliberate grants. Network access
defaults to isolated; publish selected guest ports on explicit host addresses
(loopback by default), and separate publication from outbound network policy.
Do not launch host commands or pass guest instructions to native execution.

In-process guest mediation is not an OS security boundary between the unsafe
emulator and the calling application. P6 must enforce the supported guest
filesystem/network/resource policies and machine separation, but cannot claim
protection of the application from emulator memory-safety bugs or host process
failure. Do not apply process-wide OS restrictions, resource limits, signal
handlers or termination actions to the application as if they were per-machine.
Requests for guarantees unavailable in this mode must fail explicitly, not
silently switch execution to another process. Separate-process mode provides
an explicit process boundary and permits terminating the worker without killing
the application; it is not automatically a hardened sandbox when running with
the same host privileges. Qualify supported OS restrictions/resource limits
independently and expose the actual capabilities. Hardened OS containment is
not a gate for default in-process execution. Qualify both modes on Linux x64
and record platform-specific limitations honestly.

## Future execution snapshots

Design resource ownership for a later stop-at-a-consistent-point snapshot, but
do not expose save/restore methods that merely restart the executable. A future
snapshot must account for guest CPU/thread registers, memory mappings/contents,
pending signals, masks, futexes, timers, descriptor sharing/offsets, private
filesystem changes, pipes, environment and emulated process/kernel state.
Host CLR objects, raw host pointers and native handles are not portable state;
use versioned logical identities and explicit resource reconstruction. A future
pause must quiesce every guest thread and outstanding host operation coherently.

Live host folders and remote TCP peers cannot be frozen merely by serializing
the machine. Define which external resources can be captured, pinned, rebound
or require disconnection; reject unsupported resume conditions explicitly.
Version snapshots against the guest image, translated runtime, ABI/profile and
mount base identities. Keep configuration-copy helpers (`InstanceOptions.Snapshot`
today) distinct in naming/documentation from resumable execution snapshots.

## Delivery and completion

- Deliver the public machine API and small examples for a console program,
  host-folder data processing and the actual NativeAOT Kestrel service.
- Retain original authored `src` references in `ManagedConsumer.slnx`; update
  stale controller/worker documentation to the actual new public contracts.
- Qualify memory/settings enforcement, environment/argv/cwd, ordinary filesystem
  operations in every mount mode, default live host writes, persistent private
  changes, streaming console/EOF, explicit network grants, stop and disposal.
  Check concurrent independent machines and sequential executions in one host
  process, including resource cleanup, unchanged host environment/cwd/console,
  and ordinary denied access. Existing custom fault-injection and
  malformed-ELF exclusions still apply.
- Build and run the consumer through the public API under JIT and NativeAOT on
  Linux x64 in both in-process and separate-process modes, including their
  documented stop/termination and resource capabilities. Retain actual
  guest/reference and current-product evidence. Reuse
  unaffected P5 evidence with exact provenance, not stale source assumptions.
- Commit milestones. Complete P6 before the separately planned P7 qualification
  campaign. Snapshots, PTYs and arbitrary Linux software support are not claimed
  by this phase's completion.

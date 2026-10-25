# Ordinary CoreCLR guests

This phase targets framework-dependent Linux x86-64 .NET applications. The
guest `dotnet`, hostfxr, CoreCLR, guest JIT compiler, generated machine code,
managed libraries and libc all execute through the translated Blink CPU.
The original profile keeps authored app methods as IL with `PublishAot=false`
and `PublishReadyToRun=false`. The explicit normal ReadyToRun profile retains
CoreCLR and IL while adding precompiled app methods. A NativeAOT **host** build
is a separate consumer configuration; it does not turn the guest application
into NativeAOT.

Fresh r9 IL smoke and the complete console workload pass across raw/processed
translation, JIT/NativeAOT hosts and both public modes with the explicit virtual
budget. A separate minimal Kestrel middleware profile completes real health,
echo and HTTP shutdown in processed/NativeAOT-host/InProcess mode. Its finite
30-minute header timeout and measured 2,587-second guest command are fixture settings;
this single result does not establish the complete service matrix.

The explicit normal R2R publish has been reproduced byte-for-byte and verified
for native method payloads. Three public service cells pass: processed translation
with a NativeAOT host in both execution modes, and raw translation with a JIT host
in `InProcess`. Five of the eight possible service cells remain unqualified.
Original IL-app results and failures remain separate. No R2R speedup or technical
requirement for app R2R has been established.
Passing syscall, memory, ELF and thread fixtures establish their stated
contracts; they do not establish general CoreCLR support.

## Reproducible guest closure

[CoreClrRuntime](../tests/CoreClrRuntime/README.md) pins SDK 10.0.112 and
Microsoft.NETCore.App / Microsoft.AspNetCore.App 10.0.12 from `/usr/lib/dotnet`.
The runtime reports commit `95017c711e6afc1085133d440e42b4bd78155701`.
`build.py` retains and hashes the actual host, hostfxr, both shared frameworks,
guest app files, ELF interpreter and transitive native dependencies. Two
independent builds produced identical 365-file images. Installed runtime
identities are evidence; source references to the public v10.0.0 implementation
explain the architecture and are not an assertion that those sources exactly
match this installed build.

The fixtures use invariant globalization, disabled diagnostics and configuration
reload, two runtime processors, a 32 MiB GC heap hard limit, a 64 MiB GC region
range and 1 MiB regions. These are explicit guest environment settings in the
closure, not changes to Blink's public defaults. DNS, TLS, culture data and
diagnostic IPC are outside this fixture profile.

The original app profile publishes IL with `PublishReadyToRun=false`; that flag
does not disable precompiled code already present in shared frameworks. The R2R
profile inspects actual managed PE native headers and method-entrypoint/code
sections, including newly published app images. It pins `linux-x64` runtime and
compiler inputs rather than treating a publish flag as proof. CoreCLR still
loads the application, and generic, dynamic or other uncovered methods may use
the guest JIT. Every guest R2R or JIT instruction runs through Blink. No broad
interpreter optimization follows from enabling R2R.

The native oracle invokes the retained `dotnet` normally so Linux honors its
`PT_INTERP`. Invoking `ld-linux` explicitly changes `/proc/self/exe` and makes
the host reject its apparent renamed executable; the initial failed packaging
attempt is retained. Native success does not establish translated execution.

## Public API and lifecycle

Import every closure file with its guest path and executable flag, then use
`ExecutionOptions.Executable = "/dotnet/dotnet"`, the app DLL as the first
argument, and `AllowElfInterpreter = true`. Keep the runtime directory layout
intact. All paths resolve through the machine's guest filesystem; no runtime
or native library is supplied implicitly by the host.

The public probe exercises both `InProcess` and `SeparateProcess`. Its console
workload requires a JIT-compiled authored method, allocation and forced GC,
managed threads, thread-pool synchronization, and exported guest file contents.
The service probe publishes guest IPv4 TCP port 8080, waits for a real listener,
then compares health, echo and HTTP shutdown responses. HTTP `/stop` requests
guest application shutdown. `MachineRun.StopAsync` requests emulator
cancellation; it is not guest graceful SIGTERM delivery.

Each attempt has instruction/deadline bounds, a process-group timeout, retained
stdout/stderr and source/binary hashes, disposal outcomes and worker-process
cleanup. Supplementary scalar syscall traces are bounded and separately
identified from the public API results. A stale producer, partial matrix, or
diagnostic write-xor-execute override cannot qualify fresh CoreCLR support.

The [normal R2R profile](../tests/CoreClrReadyToRun/README.md) pins SDK 10.0.112,
runtime and Crossgen2 10.0.12, and `linux-x64`. It uses
`PublishReadyToRun=true`, `PublishReadyToRunComposite=false`, `PublishAot=false`,
`UseAppHost=false`, framework-dependent deployment, and default tiering and
write-xor-execute policy. The 365-file image includes its own runtime and native
library closure; the host's installed runtime is not a guest fallback.

The [public R2R harness](../tests/GuestReadyToRunPublic/README.md) builds an
ordinary consumer and matching worker in a new private tree. A selected cell is
run from the repository root with:

```sh
python3 -B blink/tests/GuestReadyToRunPublic/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --profile-receipt blink/artifacts/coreclr-runtime/readytorun-native/attempt-wcalj6ct/receipt.json \
  --form processed --host-runtime nativeaot --mode InProcess \
  --seconds 3600 --instruction-limit 3000000000
```

Choose `--mode SeparateProcess` for an actual worker process, `--host-runtime jit`
for a managed host, or `--form raw` for the raw translated body. These select
different qualification cells; a single pass does not qualify the others.
The [consumer source](../tests/GuestReadyToRunPublic/Probe/Program.cs) supplies
`MemoryLimit = 256L << 20`, `VirtualMemoryLimit = 4096L << 20`, the retained
closure environment, a finite `InstructionLimit` and `ExecutionDeadline`, and
`NetworkPolicy.Publications = [new(8080)]`. It imports each hashed `ImageFile`,
then calls `StartAsync` with the executable and app arguments described above.
`Ready` provides the published host endpoint after the guest listener opens.
The client must still verify full HTTP bodies and call `WaitAsync` after the
application's HTTP shutdown endpoint. `Ready` alone does not establish a
working service.

The minimal app explicitly permits 30 minutes for request headers. This finite
fixture setting is needed for cold interpreted request work; it is not a Blink
default or a demonstrated production startup target. Each public attempt has a
3,600-second execution bound and a 3-billion-instruction ceiling, including all
three responses and normal shutdown. The existing IL control's total duration
is available for context (2,586.742 guest-command seconds; 2,591.098 seconds for
the full workflow), but it did not record the new harness's individual
readiness and request timings. Cross-run timing differences therefore cannot
establish an R2R speedup.

## Timed normal ReadyToRun result

The first public normal-R2R case passes in processed translation with a
NativeAOT host and `InProcess`. Public readiness takes 1,112.941 seconds from
Start. Full health, echo and stop responses take 978.975, 183.479 and 45.453
seconds respectively. All three return HTTP 200 with the exact expected bodies.
The guest exits naturally with code 0 at 2,539.852 seconds since Start, having
executed 1,268,294,227 instructions. The complete probe command takes 2,540.646
seconds, including import and disposal. Resources are released, the run and
machine are disposed, and the owning process groups are gone without cleanup
signals. The finalized receipt is
`artifacts/coreclr-runtime/readytorun-public-processed/attempt-k8ulfvap/receipt.json`;
its independent source/deployment/report/event/cleanup audit is
`artifacts/coreclr-coordination/r3-adoption/first-final-audit.json`.

The historical minimal IL control executed 1,268,653,381 instructions. These
instruction totals are nearly identical. Different deployment metadata, host
probes and timing origins prevent a causal speedup comparison; neither this
observation nor the matched native controls demonstrates an R2R speedup. The
minimal IL control also passed, so R2R has not been shown to be a compatibility
requirement. It is an explicit tested publish profile, not a performance
guarantee. Cold startup and first-request latency remain substantial.

Two targeted follow-ups also pass all three exact HTTP responses, natural exit 0,
and complete resource/run/machine/worker cleanup:

- Processed / NativeAOT host / `SeparateProcess`: Ready 1,467.254 seconds;
  health 1,539.094, echo 216.721 and stop 62.659 seconds; natural exit at
  3,527.012 seconds since Start; 1,284,611,506 instructions; complete probe
  command 3,533.962 seconds. Actual worker PID 1782222 is gone.
- Raw / JIT host / `InProcess`: Ready 1,245.556 seconds; health 1,486.866,
  echo 243.924 and stop 53.283 seconds; natural exit at 3,280.779 seconds
  since Start; 1,265,619,635 instructions; complete probe command
  3,281.709 seconds.

Their finalized receipts are `readytorun-public-processed/attempt-cqrxogse` and
`readytorun-public-raw/attempt-ffdmbdts` under `artifacts/coreclr-runtime`.
All three selected cells share the pinned profile, producer, bounds and 123
authored source files. The combined independent audit passes 6,408 checks across
8,154 files:
`artifacts/coreclr-coordination/r3-adoption/all-final-audit.json`.
The follow-ups overlapped; these timings are observations, not a controlled
performance comparison. Every run retained the original 3,600-second and
3-billion-instruction bounds. No guest, worker or build remains active.

The imported app carries the verified R2R native payload. The runs establish
execution of that closure; they do not prove which precompiled methods CoreCLR
accepted. Official linux-x64 compiler/reference framework bytes differ from the
retained Ubuntu runtime despite matching version/source commit. Versioning,
tiering and residual guest JIT caveats remain. Frameworks already contain R2R
code, and these measurements provide no reason to add a composite profile.

Qualification is limited to these three cells and this minimal middleware app.
The complete eight-cell service matrix, original Slim app and arbitrary ASP.NET
Core applications remain unqualified. The 24 original host/IL console/workload
passes and all eight original Slim readiness failures are retained independently.

## Compatibility implementation

The authored memory owner supports sparse memfds with coherent shared RW/RX
aliases. The backing remains host RW/NX; Blink interprets guest code and enforces
each guest mapping's permissions. Descriptor references, page leases and mapping
transactions retain file identity across close, reuse and concurrent replacement.
Payload, aliases and conservative metadata charges share the execution memory
budget. The memory-file store also has a 64 MiB sublimit. Neither sparse logical
length nor additional aliases grant unbounded resident memory.

The optional `MachineOptions.VirtualMemoryLimit` separates the guest address-space
reservation budget from backing. Null retains the existing equal-budget default;
explicit values are page-aligned, at least `MemoryLimit`, and at most 4 GiB.
The scoped CoreCLR profile requests 4 GiB virtual space while retaining 256 MiB
backing and the 64 MiB memory-file sublimit. Native Kestrel measured about 2.6 GiB
virtual space and 60 MiB RSS. A retained translated failure needed an 8 MiB
pthread stack while only about 4 MiB of virtual space remained; charged backing
was about 74 MiB. The virtual option addresses that measured reservation limit.

Futex bitset waits use the selected absolute clock and mask-aware wake selection.
Shared memfd aliases use backing-byte identity; PRIVATE waits retain their own
address namespace. A separate native Blink page-registry race remains recorded
in the native oracle evidence; a passing managed run does not resolve it.

Alternate signal stacks may include a guard page at registration. Delivery still
validates the actual frame write. The guest-only `/proc/self/maps` view walks
page tables under the mapping lock and reports sorted, coalesced addresses,
permissions and sharing. It deliberately omits backing filenames, file offsets,
device numbers and inode identities. Each open owns an immutable snapshot capped
at 1 MiB and charged to the memory budget. `RLIMIT_STACK` reports the loader's
actual fixed 8 MiB main-stack reservation. CPU/node identity is 0/0, matching the
portable CPU/affinity profile; the fixture's configured .NET processor count is
an explicit runtime setting.

`ImportImage` continues to create mutable private files. Read-only shared regular
file pages use a guest-inode cache with a retained file description. Guest
writes, positional writes, append and truncation update resident shared pages;
close, rename, unlink and descriptor replacement preserve mapped file identity.
Private mappings retain their separate snapshot behavior. Writable regular-file
shared mappings remain unsupported; later write-permission upgrades fail.
Immutable image pages can use immutable snapshots. External host modifications
to granted directory contents have no new coherence guarantee.

## Retained startup boundaries

Native IL smoke, GC/thread/file workload and Kestrel health/echo/stop pass.
The initial translated baseline from the preceding ELF producer exits before
CoreCLR startup: `openat` with flags `0x90800` returns `EOPNOTSUPP`, preventing
hostfxr directory enumeration. All 66 syscall observations are retained for
smoke and service; both public modes dispose successfully. The runtime closure
is present, so bypassing hostfxr discovery would hide a file-semantics defect.

Later retained attempts pass hostfxr discovery and signal registration. A missing
`/proc/self/maps` caused libc's main-thread stack query to fail; exact CoreCLR
binary inspection established that its null stack limit led to an explicit
`0x8007000E` throw. The next service failure was `getcpu` returning `ENOSYS`,
making `sched_getcpu` return an invalid CPU index. After those fixes, a public
trace identified the CoreLib read-only `MAP_SHARED` request returning `ENOTSUP`
on mutable imported files; CoreCLR reported that error as out of memory. These
failures and the failed compile attempts remain separate from later results.

The r8 producer also supports ordinary user-mode segment-selector reads and
ignores wholly empty `PT_LOAD` segments when calculating the ELF address range.
Its public regression matrices pass 104 ELF cases, 24 managed thread-churn
cases, 16 shared-memory cases and 40 syscall/CPU cases across raw/processed,
JIT/NativeAOT hosts and both execution modes. These results are recorded in
`artifacts/coreclr-phase/resume-bbuxio4w/index.json`.

The r8 smoke reaches `Program.Main` and `Console.WriteLine`, then throws in
`ConsolePal.EnsureInitializedCore`. Exception formatting subsequently aborts
with an RW/RX mapping allocation error. The workload independently aborts with
out of memory, and the service aborts before publishing a listener. These
attempts release resources. Both causal traces identify a pthread stack
reservation rejected by the 256 MiB virtual limit. ENOTTY is accepted and pipe
creation succeeds. Fresh r9 public smoke and the full workload pass with the
separate virtual budget. The 1,200-second service cases miss listening readiness
without a recorded guest trap. A separate 2,400-second original service case
reaches `SERVICE READY` and accepts an IPv4 connection, but its first health
request remains incomplete when the execution deadline closes the connection.
Listening alone does not qualify HTTP support.

A separate ordinary `WebHostBuilder` middleware fixture returns a real HTTP
408 with Kestrel's default header timeout. Its next bounded control, with an
explicit finite 30-minute `RequestHeadersTimeout`, passes health, echo and stop
with exact HTTP 200 bodies, normal guest exit, and complete disposal. It executes
1,268,653,381 guest instructions in a 2,586.742-second guest command. The complete
Python workflow, including preparation, took 2,591.098 seconds. It does
not replace qualification of the original service fixture. A longer original
attempt interrupted by an observer exception is invalid, with no guest report;
its corrected retry was subsequently cancelled for the user's R2R scope change.
Neither interruption is classified as a runtime failure.
The late abort messages alone did not establish the preceding reservation
diagnosis.

Attempt paths, exact commands and delegation identities are retained in
`artifacts/coreclr-coordination/`. The final phase evidence index records the
actual achieved scope and all failed attempts.

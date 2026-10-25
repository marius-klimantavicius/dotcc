Ordinary Linux x86-64 CoreCLR fixtures
====================================

`build.py` pins the installed SDK **10.0.112**, host commit **95017c711e**,
Microsoft.NETCore.App **10.0.12**, and Microsoft.AspNetCore.App **10.0.12**.
It publishes framework-dependent **IL** applications, with AOT and app
ReadyToRun disabled and framework roll-forward disabled. The console's
`Work.JitAndAllocate` method must be compiled by the ordinary guest JIT.
It allocates objects, forces a blocking compacting GC, synchronizes two
managed threads, uses four thread-pool tasks with a semaphore, and checks
file read/write/seek/flush/rename/enumeration/delete. `smoke` is a smaller
managed startup scenario. The minimal ASP.NET Core application serves real
Kestrel health, echo and shutdown requests and exits after `/stop`.

The retained guest image contains the real dotnet host, hostfxr, both complete
shared frameworks, their actual `ldd` native dependencies (including ld-linux,
libc, libstdc++, libgcc, libunwind and liblzma), plus both apps. Files, authored
sources, framework configs, commands, logs and native responses have SHA256
identities in attempt receipts. There are no downloads or NuGet dependencies
for the fixture apps. The installed SDK's targeting packs build the apps.
Changing installed SDK/runtime version requires an explicit source pin update.
Compiler path mapping removes attempt-directory names from IL/PDB identities;
runtime-local dependencies stay under `/dotnet` in every guest image.

Run from the repository root:

```sh
python3 blink/tests/CoreClrRuntime/build.py

# Optional: repeat build.py and verify both entire images are byte-identical.
python3 blink/tests/CoreClrRuntime/compare.py RECEIPT-ONE.json RECEIPT-TWO.json

# Substitute the new passing receipt printed by build.py.
python3 blink/tests/CoreClrRuntime/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --native-receipt blink/artifacts/coreclr-runtime/native/attempt-EXAMPLE/receipt.json
```

The default public matrix runs `host`, `smoke`, `workload`, and `service` in
both `InProcess` and `SeparateProcess`, using only public `BlinkMachine`
import/start/readiness/port-publication/wait/export/dispose operations.
`service` publishes guest 8080 to a dynamically allocated host loopback port;
the client checks response status and body against the same native contract,
requests shutdown, waits for exit, and checks worker disposal. HTTP Date values
are intentionally excluded from the stable response contract. No host directory
is mounted in either guest execution mode. The separate worker's normal private
storage implementation is used through the public API.

For bounded first-blocker diagnosis using a prior clean producer:

```sh
python3 blink/tests/CoreClrRuntime/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --native-receipt blink/artifacts/coreclr-runtime/native/attempt-EXAMPLE/receipt.json \
  --diagnostic-stale --mode InProcess --scenario smoke --scenario service \
  --trace smoke --trace service --seconds 30 --instruction-limit 20000000
```

`--scenario` and `--mode` are repeatable; `--form raw` selects the raw delivery.
A partial scenario/mode/host-runtime selection cannot set
`fresh_qualification=true`. `--aot` keeps the JIT host runs and additionally
publishes the public Probe and its ordinary app-local worker with
`PublishAot=true`, `linux-x64`. This changes the **host** runtime; the guest
console and Kestrel applications remain the exact ordinary .NET 10.0.12 IL
fixtures, with the guest JIT and default W^X policy.

After a new strict producer completes, run both delivery forms separately:

```sh
python3 blink/tests/CoreClrRuntime/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --native-receipt blink/artifacts/coreclr-runtime/native/attempt-h40bq9ta/receipt.json \
  --form raw --aot --virtual-memory-mib 4096 --seconds 1200 --instruction-limit 3000000000 \
  --memory-handoff blink/artifacts/coreclr-coordination/memory-handoff-receipt.json

python3 blink/tests/CoreClrRuntime/run.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json \
  --native-receipt blink/artifacts/coreclr-runtime/native/attempt-h40bq9ta/receipt.json \
  --form processed --aot --virtual-memory-mib 4096 --seconds 1200 --instruction-limit 3000000000 \
  --memory-handoff blink/artifacts/coreclr-coordination/memory-handoff-receipt.json
```

Each command requests 20 public cases: two host runtimes, both execution
modes, and host/smoke/workload/service/memfd. Without `--memory-handoff`, it
requests the 16 CoreCLR cases. Without `--aot`, only the eight JIT-host
CoreCLR cases are requested. `requested_matrix`, `qualification_scope`,
`complete_public_matrix`, and `complete_host_runtime_matrix` record that scope.
`fresh_qualification` applies only to the selected form, requires both host
runtimes and modes and all four CoreCLR scenarios to pass, and remains false
for stale, compile-only, partial, or no-W^X diagnosis. Neither single-form
receipt claims both delivery forms. Check both receipts against the same
producer, native closure and authored source identities before reporting the
full raw/processed matrix.

`--memory-handoff` validates the memory handoff and passing native receipt,
source hash, exact build/run command, stdout and retained ELF hash. It captures
that ordinary x86-64 memfd fixture into its own attempt-local image and executes
it through the same public Probe in each selected host runtime and mode. The
CoreCLR image is left intact. The fixture checks shared RW/RX code changes
42 -> 43 -> 44, close/descriptor-reuse lifetime, partial unmap/protect, sparse
offsets, shared truncate/regrow/refetch, and private COW independence before
truncation. Success requires exit 0 and exactly
`guest memfd aliases passed` (26 bytes, no newline), plus normal public
release/disposal and worker exit checks. The retained fixture's write count
omits the C literal's trailing newline; this follows the actual native log
without changing the handed-off source or ELF.
`--scenario memfd --memory-handoff PATH` requests just this case;
it cannot qualify CoreCLR. `memfd_public_passed` records its selected cases
independently of CoreCLR. Native oracle success is never translated success.

For private compilation without guest execution, add `--compile-only`.
It builds the JIT Probe and worker and, with `--aot`, publishes the matching
NativeAOT pair. If `--trace` is supplied it also compiles the trace host but
does not run it. A successful receipt has `compile_only=true`, no executions,
`complete_public_matrix=false`, and `fresh_qualification=false`.

`--trace SCENARIO` adds a supplementary lower-level
`ThreadedGuestExecution` startup trace using its existing syscall-observation
hook. The trace retains the first 4096 and last 512 observations, syscall
counts, stop/trap data, retained memory bytes/mappings, and cleanup result;
truncation is explicit. It does not perform HTTP traffic or qualify the public
service lifecycle. Trace failures and missing reports remain failures in command
receipts; a successful trace process alone is not a qualification result.

`run.py` builds only attempt-private copies of the delivered product and
authored API/worker sources. It never translates, rebuilds shared tools, or
edits generated sources. The normal path uses `load_clean_delivery` and rejects
authored sources differing from the producer. `--diagnostic-stale` instead
validates the old strict producer's complete selected source/object set and
delivered file hashes, then freezes its staged host project and managed bridges.
It records the old producer identity, records the current API/worker sources,
and always sets `fresh_qualification=false`. A parent can continue editing
shared sources after capture without changing the retained execution closure.
Both JIT and NativeAOT consumers are built from the same frozen source closure;
fresh attempts copy no `bin` or `obj` inputs. JIT outputs are retained before
NativeAOT publishing. Deployment receipts include the app and actual discovered
worker path and SHA256, and the entire deployed file set is checked before and
after commands. Every public result records its observed `HostRuntime`,
`HostFramework`, and `HostDynamicCodeCompiled`; labels and receipts include the
delivery form, host runtime, mode and scenario. Separate-process cases require
a real worker PID and its exit, in addition to resources released and run and
machine disposal. The supplementary trace host remains JIT and is labeled so;
it does not fill NativeAOT public matrix entries.

The public guest backing-memory default and maximum remain **256 MiB**. Smaller values
can be explored with `--memory-mib`; larger values are rejected. The finite
workloads use a 32 MiB GC heap hard limit (`2000000` hexadecimal), 64 MiB GC
region range (`4000000`), and 1 MiB GC regions (`100000`), with two logical
processors. Native oracles pass with exactly that GC profile. Invariant
globalization, disabled diagnostic IPC and disabled configuration file reload
are explicit fixture settings, not syscall stubs. Guest image bytes count
against the normal 512 MiB storage limit; they are separate from the public
guest mapped-memory bound. Runtime memory requirements are not proven until
the public app executes successfully; failed traces report the actual stop
and charged bytes instead of raising/removing the memory cap.

`--virtual-memory-mib 4096` explicitly selects a separate virtual reservation
budget for this fixture. The flag defaults to zero, retaining the public default
of tying address-space limits to backing limits. `VirtualMemoryMiB` and receipt
bounds record the selection. Native Kestrel uses about 2.6 GiB of virtual space
with roughly 60 MiB RSS; translated baseline traces reject an 8 MiB thread stack
at the 256 MiB virtual ceiling while backing is well below its limit. The scoped
4 GiB option retains every backing, storage, descriptor and thread limit.
HTTP requests use the same finite execution deadline: first-request JIT work
also runs through the interpreter. The process-group bound remains independent.

W^X uses the runtime default. `--diagnostic-no-wx` is an explicit temporary
experiment setting `DOTNET_EnableWriteXorExecute=0` in the guest; such a run
always has `fresh_qualification=false`. No such experiment establishes normal
CoreCLR support. No native execution fallback, host filesystem passthrough,
fake syscall success, malformed ELF, or fault-injection scenario is used.

Initial evidence at baseline `80e4726c8970b968e00d8ac2c39c608ed04e98e9`:

- Native smoke/workload/Kestrel all pass:
  `blink/artifacts/coreclr-runtime/native/attempt-sxwylmvv/receipt.json`.
- Stale 09:25 producer public host/smoke/workload/service all reach dotnet and
  exit after 1,837,682 translated instructions with
  `Error: [/dotnet/host/fxr] does not contain any version-numbered child folders`.
  The hashed closure contains `10.0.12/libhostfxr.so`; all public resources
  release cleanly. Receipt:
  `blink/artifacts/coreclr-runtime/guest/attempt-48axhjr0/receipt.json`.
- Both public modes reproduce smoke/service in `guest/attempt-a8j115k5`.
  Corrected supplementary smoke/service traces retain all 66 observations.
  The terminal startup failure is `openat(AT_FDCWD, path, 0x90800, 0) = -95`,
  rejecting `O_RDONLY|O_NONBLOCK|O_DIRECTORY|O_CLOEXEC` before directory
  enumeration. The adjacent access check succeeds. This was the initial
  product blocker; subsequent implementation preserves ordinary open semantics.
  Both public workers are gone and both runs/machines dispose cleanly.
  The traced owner joins all workers and releases memory (270550 charged
  bytes and two mappings retained immediately before release).
- That attempt also retains a trace setup failure: the lower-level VFS image
  default was smaller than the complete runtime closure. The trace harness now
  explicitly uses the same 512 MiB image bound as public import.
- An earlier native launch via an explicit ld-linux command is retained as a
  failed attempt (`attempt-99ugh27t`): dotnet sees the loader in
  `/proc/self/exe` and rejects its basename. Normal PT_INTERP native launch
  succeeds; the actual host loader hash matches the retained guest loader.

Latest exact commands and first blockers are in
`blink/artifacts/coreclr-coordination/fixture-status.md`. Artifacts are
attempt-scoped, retained on failure, and intentionally ignored by git.

The r8 attempts reached managed `Main`, then exhausted their virtual reservation
budget while creating an ordinary pthread stack. Those failed attempts remain
retained. With the explicit virtual budget, fresh r9 passes all **24**
host/smoke/workload cases across raw/processed, JIT/NativeAOT hosts and both public
modes. The independent audit is
`blink/artifacts/coreclr-phase/resume-bbuxio4w/console-qualification.json`.

All eight original service cases miss the 1,200-second readiness bound, so both
complete receipts retain `fresh_qualification=false`. A separate longer case
reaches `SERVICE READY` and accepts TCP but completes no HTTP response by its
2,400-second deadline. A 7,200-second attempt interrupted by its observer is
invalid evidence, with no guest report; its cleanup and corrected retry are
recorded separately. Listening does not establish the health/echo/stop contract.
The separate `GuestKestrelMinimal` control and its explicit finite header-timeout
setting do not qualify the original service fixture.

Current receipts, passing syscall/ELF/thread/shared-memory matrices, retained
failures, and the prior automated content-filter stop are indexed in
`blink/artifacts/coreclr-phase/resume-bbuxio4w/index.json`. Safeguards were not
disabled or circumvented.

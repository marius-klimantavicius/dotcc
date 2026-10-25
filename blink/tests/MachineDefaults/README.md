# Machine default contracts

This focused consumer compiles the authored options, source-generated machine
protocol, shared framing code and real `MachineRun` console implementation against
the BCL host project. It does not build the translated product, execute guest
instructions or launch a service worker. The `ConsoleProbe` only exposes the
actual base class's console for ordinary writes and completion.

It checks the exact service defaults (256 MiB memory, 512 MiB private storage,
8,192 nodes, 1,024 descriptors, 64 concurrently owned guest thread slots, null instruction
budget/deadline), both execution-mode option selections, full `start-v1` framing
with nullable limits and `long.MaxValue` output, explicit low limits and validation,
the static-only `ExecutionOptions.AllowElfInterpreter` default, explicit opt-in
after freeze and both boolean values surviving source-generated worker framing,
descriptor/node counts above the former defaults, small storage/descriptor caps,
and output beyond 1 MiB with only 64 KiB retained capture. Creating the private
filesystem with the 512 MiB quota must allocate less than 1 MiB. Incomplete raw
option frames remain invalid; the actual controller serializes every option.

Run in an isolated authored-source snapshot from the repository root:

```bash
defaults_work=$(mktemp -d)
mkdir -p "$defaults_work/blink/src" "$defaults_work/blink/tests"
tar -C blink/src --exclude=bin --exclude=obj -cf "$defaults_work/sources.tar" \
  Managed.Emulation.Host Managed.Emulation
tar -C "$defaults_work/blink/src" -xf "$defaults_work/sources.tar"
cp -R blink/tests/MachineDefaults "$defaults_work/blink/tests/"
cp Directory.Build.props Directory.Packages.props nuget.config "$defaults_work/"
dotnet build "$defaults_work/blink/tests/MachineDefaults/Probe.csproj" -c Release \
  --disable-build-servers -p:UseSharedCompilation=false
dotnet "$defaults_work/blink/tests/MachineDefaults/bin/Release/net10.0/Probe.dll"
dotnet publish "$defaults_work/blink/tests/MachineDefaults/Probe.csproj" -c Release \
  -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -o "$defaults_work/aot"
"$defaults_work/aot/Probe"
```

Each execution prints `machine default contracts: PASS (no translated guest execution)`.
Linux x64 is the exercised host; Windows and other hosts need their own runs.
The earlier default-only versions of both forms passed using the isolated snapshot
`generated/machine-defaults/attempt-Xdehaa/`, with build/execution logs and verified
input hashes in `artifacts/machine-defaults/attempt-Xdehaa/`. JIT compilation
reported zero warnings and errors. The executed DLL SHA-256 was
`6b2d9e825168094a02ec7f66d917b251bf9c160fa92469f6bbb573d38d570614`;
the NativeAOT binary SHA-256 was
`a800d850e6ebd3d9f6e7aebe9bdf415047807324765c5617a82de25d56c1400e`.
Those receipts predate the added interpreter-option checks. A fresh execution
is required to qualify the updated program and linked protocol sources; the
historical binaries do not establish those new assertions.

The production propagation audit is separate source evidence:

- [BlinkMachine](../../src/Managed.Emulation/BlinkMachine.cs) freezes options and
  passes storage/node/descriptor quotas to both private store constructors.
  Mounts and copy operations receive the configured limits too. No full quota is
  allocated when a store is constructed.
- [InProcessMachineRun](../../src/Managed.Emulation/InProcessMachineRun.cs) passes
  descriptor and pipe quotas to `InstanceIo`, memory and thread quotas to
  `ThreadedGuestExecution`, the optional deadline to `HostExecutionStop`, and
  `InstructionLimit ?? long.MaxValue` to `Run`.
- [ProcessMachineRun](../../src/Managed.Emulation/ProcessMachineRun.cs) sends those
  same options and console values using source-generated `MachineJson`.
  [MachineWorkerHost](../../src/Managed.Emulation/MachineWorkerHost.cs) freezes the
  options again, creates the host-backed stores and uses `InProcessMachineRun`.
  It does not use the old `InstanceOptions`/`WorkerProfile` demonstration limits.
- [ThreadedGuestExecution](../../src/Managed.Emulation.ThreadedExecution/ThreadedGuestExecution.cs)
  accepts page-aligned memory through 256 MiB and 1–256 concurrently owned worker slots,
  including the main thread. Child slots are reclaimed after Machine/binding cleanup,
  host-thread join and resource disposal; clear-TID alone does not release a slot.
  The main slot remains occupied through run teardown. Completed results remain in
  history without retaining retired workers' host resources. The focused
  [GuestThreadChurn](../GuestThreadChurn/README.md) fixture tests churn and low-limit
  rejection/recovery separately from this host-only defaults probe. The
  [C backing allocator](../../src/Host/HostMemory.c) independently caps shared
  owners at 256 MiB and initially allocates only its owner record.
- [GuestResourcesBridge](../../src/GuestResources/GuestResourcesBridge.cs) and
  [GuestResources.c](../../src/GuestResources/GuestResources.c) initialize guest
  NOFILE from actual descriptor capacity and AS/DATA from actual memory-owner
  limits, without old fixed 128-descriptor/64-MiB ceilings.
- [MachineRun](../../src/Managed.Emulation/MachineRun.cs) passes all console
  settings to `HostConsole`. [HostConsole](../../src/Managed.Emulation.Host/HostConsole.cs)
  defaults its lifetime ceiling to `long.MaxValue` too; queue capacity and retained
  capture stay bounded independently of cumulative output.

The remaining 256 KiB per-store path-name budget and 4,096-byte path bound still
apply. Network grants remain explicit numeric IPv4 TCP, and the selected guest
default profile remains static Linux x86-64 ELF/NativeAOT; explicit interpreter
execution requires the separate [ELF runtime suite](../ElfRuntime/README.md).
Fresh translated-product tests
are needed to establish guest execution after any staged C changes; these
host-only checks provide no evidence for a stale translated DLL.

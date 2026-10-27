Coordinator-scheduled emitter qualification only. No native code is executed
by this project; run the existing ContractTests separately for execution gates.

This project compiles the real producer Machine layout and selected emitter.
Properties:
  BlinkProject    Absolute path to the coordinator-approved translated project.
  EmitterSource   Optional absolute frozen emitter source; default is current.
  ContractsSource Optional absolute frozen JitContracts source; default current.
  LegacyEmitter=true compiles out the new span/scratch/allocation assertions.

Run the baseline and candidate on the SAME OS/architecture with the same host
kind and approved producer closure. Serialized golden bytes include the ABI's
OS-specific argument register. Compare CoreCLR and NativeAOT separately.

After obtaining the coordinator's build/time slot, example CoreCLR commands
from the repository root (supply the approved producer path in emitter_blink):

  export TMPDIR=/mnt/storage/blink-jit-ir-20261027/emitter-allocation/qualification/tmp
  mkdir -p "$TMPDIR"
  export DOTNET_CLI_HOME=/mnt/storage/blink-jit-ir-20261027/emitter-allocation/qualification/dotnet-cli
  export NUGET_PACKAGES=/mnt/storage/blink-jit-ir-20261027/emitter-allocation/qualification/packages
  emitter_out=/mnt/storage/blink-jit-ir-20261027/emitter-allocation/qualification
  emitter_project="$PWD/blink/tests/GuestJit/EmitterAllocationTests/EmitterAllocationTests.csproj"
  emitter_frozen="$PWD/blink/artifacts/jit-implementation/ir-20261027-r1/initial-source/blink/src/Managed.Emulation.ThreadedExecution/Jit"

  dotnet build "$emitter_project" -c Release --artifacts-path "$emitter_out/r16" \
    -p:BlinkProject="$emitter_blink" -p:LegacyEmitter=true \
    -p:EmitterSource="$emitter_frozen/X64BlockEmitter.cs" \
    -p:ContractsSource="$emitter_frozen/JitContracts.cs"
  dotnet "$emitter_out/r16/bin/EmitterAllocationTests/release/EmitterAllocationTests.dll" \
    --write-golden "$emitter_out/r16-bytes.json" --probe > "$emitter_out/r16-probe.json"

  dotnet build "$emitter_project" -c Release --artifacts-path "$emitter_out/candidate" \
    -p:BlinkProject="$emitter_blink"
  dotnet "$emitter_out/candidate/bin/EmitterAllocationTests/release/EmitterAllocationTests.dll" \
    --verify-golden "$emitter_out/r16-bytes.json" --probe > "$emitter_out/candidate-probe.json"

Use separate artifact directories for any NativeAOT publish or other closure;
route its native compiler/temp outputs into the same task scratch as usual.
Ensure approved producer custom targets also write only to task scratch.
These commands intentionally do not create a benchmark/build schedule.

Without --probe the executable runs only byte/reuse/API checks. --write-golden
records exact byte arrays (not only hashes); --verify-golden compares every
byte. --probe adds 128 warmed emissions per corpus item and writes JSON.
Inputs, JSON, cross-thread checks, and warm-up are outside the measured scope.
Each row includes emitted size, allocated bytes, x64 result-array charge, and
elapsed time. Candidate scope should allocate only its independent result
array, allowing 256 total measurement/runtime bytes per row. This is a focused
allocation probe, not a service CPU qualification or a timing acceptance gate.

Match rows by Name and verify candidate AllocatedBytes <= baseline/2 in total
and for representative short, retained-hot, memory-heavy and 32-op cases.
Report exceptions rather than discarding failed rows. Parent must also measure
whole compile allocation against frozen r16 after removing the input ToArray.
Keep compilation/probes/service qualification serial as scheduled.

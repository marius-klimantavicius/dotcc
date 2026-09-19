# Normal CPU conformance corpus

The default runners select **449 normal cases** from the unchanged 495-input
corpus. The remaining 46 custom fault cases (39 SIGFPE, seven SIGSEGV) retain
their original bytes and stable indices but are excluded from execution under
the current user scope. They are not counted as passes. Native and managed
receipts contain the identical selected IDs/names, all excluded IDs/names with
reasons, full corpus digest and reviewed source hashes. Direct native or
translated case entry also refuses a row with a nonzero fault expectation.

The selected cases retain normal integer/flags operations, signed division,
valid instruction/data page crossings, SSE/SSE2 operations, masked FP status,
NaN/infinity/conversion/rounding cases and all 11 CPUID rows. A zero fault
expectation is decisive: `faultState` identifies a state-comparison contract,
not necessarily a trapping case. Explicitly unmasked but exact operations that
complete normally remain selected.

## Normal-return independent hardware witness

`hardware.c` executes the unchanged selected bytes on real Linux x86-64 hardware.
It appends LEA R15,[RIP-7] and RET, without INT3, deliberate signal delivery,
protected data holes or a signal handler. LEA records the actual address after
the tested bytes and changes no flags; RET returns to `hardware-capture.S`.
Unexpected faults fail the subprocess naturally instead of becoming expected
results. No expected ALU results are computed by the witness.

The System V assembly function preserves RBX/RBP/R12–R15, keeps the call stack
aligned and captures AX/BX/CX/DX, RFLAGS, both XMM lanes and MXCSR before any
flag-changing cleanup. It restores the calling C environment's MXCSR and clears
DF before returning. C compile-time offset assertions check the capture record.
The fixed reviewed corpus never modifies R12–R15 or RSP; those belong to the
witness. Source hashes in `selection.py` deliberately require renewed review
before any new corpus instructions can execute with this contract.

## Reproduction

With the pinned native archive and current matching core already qualified:

```sh
python3 blink/tests/CpuConformance/run.py --staged-fp
python3 blink/tests/CpuConformance/run-managed.py --staged-fp \
  --core-receipt <current-qualified-core-execution-receipt.json>
```

The native runner snapshots hardware/C/assembly/input sources and the pinned
archive, reviewed scalar correction and CPUID policy. It exports all original
case descriptions but executes only the recorded normal selection. The managed
runner recomputes that selection and rejects a different native selection or
comparison list. Both runners hash their live implementation inputs at start and
check those files again before success; the final comparison sequence must equal
the selected rows in every requested mode. It derives a CPU frontend over the exact qualified core objects;
unchanged object hashes, canonical include paths and compiler identities must
match. Reviewed FP replacements are reused only with matching source identities.
No generated C# is patched.

Each selected row runs in a fresh process in raw JIT, raw NativeAOT,
postprocessed JIT and postprocessed NativeAOT, for **1,796 comparisons**.
The managed consumer binds private owners, forces compacting GC, executes the
actual translated interpreter and discards the process after owner teardown.
Its instruction outputs are compared with real hardware; virtual CPUID outputs
instead match the exact staged native policy, while physical CPUID remains an
environment witness. Defined flag masks and full memory/state comparisons are
retained. No service worker or malformed-image tests are part of this harness.

The normal-return implementation passed native 449 and all **1,796 managed
comparisons** against core-execution/attempt-8vbjtywv. Native receipt:
`artifacts/cpu-conformance/attempt-7j03qmvz/receipt.json`, SHA256
`b33297b3c0ba375772763b274f8236dd042bdc8fa6e292ebe0206776dbb85c73`.
Managed receipt: `artifacts/cpu-conformance-managed/attempt-3r1msqyr/receipt.json`,
SHA256 `4692e7760c37899215cec4025fcd116fd0eeeb22ddd3c47a320067d6a0e75ff0`.
The derived link retains 108 exact qualified objects and replaces only the CPU
frontend; reviewed scalar corrections already match those retained producers.
All four modes agree with the staged native interpreter. The unchanged-original
native comparison retains 359 differing normal rows separately; this does not
reclassify them as passes or conceal the reviewed scalar corrections.
The dedicated launch temporary directory was `artifacts/cpu-normal-launch/tmp`.
No shared compiler was rebuilt. Each receipt verifies the exact ordered selection
and live implementation hashes at completion. [Historical evidence](HISTORICAL.md) preserves prior
matrices and defect receipts, including now-excluded cases. The historical
495-case pass is not a fresh pass for this changed witness. Remaining instruction
coverage is described in [COVERAGE.md](COVERAGE.md).

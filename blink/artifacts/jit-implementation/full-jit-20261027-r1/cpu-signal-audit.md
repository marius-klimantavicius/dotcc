# CPU policy and Linux guest SIGILL audit — 2026-10-27

The selected virtual CPU has software instruction semantics independent of the
physical host's optional SIMD features. Full-JIT B can call predecoded,
operation-specific helpers for those semantics. This establishes an execution
mode, not universal hardware ISA fidelity. The existing hybrid remains separate.

This audit began at clean HEAD
`d3b1ccd975fd883f4f6b75d4b70fa4c9f112fc81`. Its implementation ownership is
`scripts/signal_bindings.py`, focused `tests/FullJitSignals/` assets, and this
report/JSON. Parent-owned runtime, stage scripts, producer/build orchestration,
and other concurrent edits were preserved. No generated C# was edited, no agent
was spawned, and no commit, push, PR or branch change occurred.

The authoritative JIT-only research, IR report/design contract, Astra r2 report,
current CPU documentation, CPU staging, selected upstream semantic source and
managed signal owner were read. Exact source identities, source symbol inventory,
native command receipts, fixture hashes and limits are in
[cpu-signal-audit.json](cpu-signal-audit.json).

## Actual virtual policy

The current complete-core `core-config.h` enables the upstream x87/MMX paths;
the smaller historical managed-host profile that disables them is not the public
ELF execution profile. `DISABLE_BMI2`, `DISABLE_METAL`, `DISABLE_BCD` and upstream
`DISABLE_JIT` remain set. The last disables Blink's old native JIT, independently
of the new managed modes.

`OpCpuid` returns a synthetic GenuineIntel baseline. `stage-cpuid.py` verifies its
immutable source hash and each exact replacement. In the selected core:

- Leaf 1 ECX is `0x80000000` (hypervisor). SSE3, PCLMULQDQ, SSSE3, CMPXCHG16B,
  POPCNT and RDRAND advertisements are deliberately removed. AES, SSE4.1/4.2,
  XSAVE, OSXSAVE, AVX, FMA and F16C remain absent.
- Leaf 1 EDX retains FPU, TSC, PAE, CMPXCHG8B, CMOV, CLFLUSH, MMX, FXSR, SSE,
  SSE2. Extended leaf `80000001` retains FPU, CMPXCHG8B, SYSCALL, CMOV, NX, MMX,
  FXSR and long mode; LAHF/SAHF and RDTSCP advertisements are removed. The custom
  upstream JIT bit is clear.
- Leaf 7/subleaf 0 returns zero: FSGSBASE/ERMS/RDSEED/RDPID were removed;
  BMI2/ADX are excluded at compilation; BMI1/AVX2/AVX-512/AMX are not advertised.
  Subleaf 1, thermal/power leaf 6, direct leaf D and direct `80000007` return zero.
  Maximum basic/extended leaves are 7/`80000001`. Synthetic cache leaves 2/4 and
  build-derived OS/architecture information leaves remain unchanged.

Advertisement removal is expressly not an execution ban. A caller that bypasses
feature discovery can still reach many unadvertised software helpers. No host
CPUID or XCR0 is copied into the guest's feature results.

The actual Linux x64 machine used for the native oracle reports OSXSAVE and a
readable XCR0 (the exact CPUID words and XCR0 are retained in JSON). This is
**actual host evidence**, not a capability-mask simulation and not the virtual
CPU. No simulated masks and no real Windows observations were collected here.

## Feature-related semantic paths

Every feature family exposed or explicitly narrowed by this profile was traced
to its dispatch and implementation. JSON retains the complete source-level `Op`
symbol inventory for the relevant translation units; that is an audit inventory,
not an exhaustive instruction test result.

- x87: `OpFpu`/FWAIT and `fpu.c`/`ldbl.c`; MMX: `mmx.c` and shared SSE/MMX
  dispatch in `sse.c`, `sse2.c`, `ssemov.c`. The core selects these paths. x87 uses
  the upstream binary64 model and 80-bit serialization, with the documented
  precision/exception limitations.
- SSE/SSE2: `ssemov.c`, `sse.c`, `sse2.c`, `ssefloat.c`, `cvt.c`, and shared
  register/memory helpers. Scalar conversion/comparison blocks and comparison
  masks have separate reviewed staged corrections. Most arithmetic still uses
  C/BCL floating operations; guest MXCSR is not the ambient host FP environment.
- Unadvertised SSE3: HADD/HSUB/ADDSUB and LDDQU. Unadvertised SSSE3:
  PSHUFB, PHADD/PHSUB, PSIGN, PMADDUBSW, PMULHRSW, PABS and PALIGNR.
  Some unadvertised SSE4.1 operations, including PMULLD and MOVNTDQA, remain
  selected too. Unadvertised PCLMULQDQ uses portable `clmul.c`; unadvertised
  SSE4.2 CRC32 uses `Op2f01` in `crc32.c`. That dispatch explicitly rejects
  its TODO MOVBE path with `OpUd`.
- Integer features: CMOV, CMPXCHG8B/16B, POPCNT, LAHF/SAHF and REP-prefixed
  BSF/BSR live in `machine.c`. The staged integer patch fixes selected INC/NEG
  flags and CMPXCHG8B mismatch zero extension. BMI2/ADX dispatch aliases to
  `OpUd` under `DISABLE_BMI2`.
- FSGSBASE reads/writes guest FS/GS bases in `Op1ae`; it does not alter host TLS.
  REP MOVS/STOS use `string.c`; the removed ERMS bit is an advertisement policy,
  not removal of string semantics.
- RDTSC/RDTSCP/RDPID use `time.c`; the portable managed fallback uses the clock
  boundary, and virtual TSC_AUX identifies CPU/node 0. No physical invariant-TSC
  timing equivalence is established. RDRAND/RDSEED use software `GetRandom`
  consumers in `rdrand.c`, regardless of actual host RDRAND/RDSEED features.
- FXSAVE/FXRSTOR/LDMXCSR/STMXCSR and fences/CLFLUSH share `Op1ae` in `machine.c`.
  CLFLUSH models a fence rather than host cache contents. Privileged/metal/BCD
  exclusions and unknown dispatch entries route through the existing `OpUd`.
  SYSCALL and guest mapping permissions remain guest architecture/services.

XGETBV (`0f 01 d0`) reaches `Op101`'s undefined case: no XGETBV/XCR0 semantic
implementation is selected. That matches a guest with XSAVE/OSXSAVE clear.
Intel specifies #UD when XSAVE is absent or CR4.OSXSAVE is clear; AVX discovery
also requires OSXSAVE and enabled XMM/YMM XCR0 state before using AVX.
[Intel instruction reference, XGETBV 6-32/33](https://cdrdv2-public.intel.com/774492/325383-sdm-vol-2abcd.pdf),
[Intel AVX detection guidance, 5-6](https://cdrdv2-public.intel.com/821612/248966-Optimization-Reference-Manual-V1-050.pdf).

## Guest #UD and signal return

UD2 has no architectural effect before its exception and the saved instruction
pointer identifies UD2. Linux x86 delivers #UD as SIGILL with **ILL_ILLOPN=2**,
not ILL_ILLOPC=1, and `si_addr` identifies the faulting guest instruction.
[Intel instruction reference, UD 4-720](https://cdrdv2-public.intel.com/774492/325383-sdm-vol-2abcd.pdf),
[Linux v6.17 traps.c](https://github.com/torvalds/linux/blob/v6.17/arch/x86/kernel/traps.c#L345).

The existing path preadvances guest IP and records `oplen`. `OpUdImpl` calls
`RestoreIp`; `HaltMachine` calls it again with length already zero. Delivery sees
the original guest IP and fault address. `DeliverSignalToUser` reads the guest
action/mask: blocked/default/ignored serious faults terminate through the managed
guest event; a handler receives a stack frame and `(signal, siginfo*, ucontext*)`.
The internal jump unwinds managed execution after the frame is installed. It is
not a physical host illegal-instruction trap.

The guest handler may edit its saved RIP/registers and return through its
restorer's `rt_sigreturn`. Linux restores the frame's register state, flags,
signal mask and alternate-stack state; it does not ordinarily return to the
restorer after that syscall. Linux writes trap number/error and selectors into
the exposed context.
[Linux signal return documentation](https://man7.org/linux/man-pages/man2/sigreturn.2.html),
[Linux v6.17 signal_64.c](https://github.com/torvalds/linux/blob/v6.17/arch/x86/kernel/signal_64.c#L45).

The managed recursive `SignalActor` must also select Full's predecoded helper
path. Its halt jump can abandon the interrupted recursive syscall's host stack;
parent-owned cleanup must reset temporary syscall/page-lock state and resume the
already-installed inner handler. The guest's outer handler and restorer still
remain on its guest stack. The new nested fixture selects this path without
scheduling or another worker. Parent integration and executed managed results
are separate from the native oracle in this report.

## Corrections implemented within ownership

`signal_bindings.py` now performs guarded authored-C adaptations, applied during
fresh source staging to both interpreter and Full/Hybrid consumers:

1. Split undefined/decode faults from the unchanged HLT case. #UD gets
   ILL_ILLOPN=2 and `m->trapno=6`; `siglongjmp` retains its original internal
   halt code. No instruction is silently skipped.
2. Fill #UD's context trap number 6, error 0 (zero-initialized frame), and saved
   CS/SS. The old trap-number write sat in an unreachable SIGTRAP branch and did
   not describe #UD.
3. Serialize architectural flags including lazy parity, excluding internal
   FLAGS_LP/reserved bits. Restore through `ImportFlags`, which reconstructs the
   lazy parity byte. The ordinary `ExportFlags` helper also sets reserved/IOPL
   bits and retains its internal high byte, so signal serialization uses an
   explicit valid-bit mask rather than exposing those helper side effects.
4. Save/restore MXCSR in the existing embedded FP image. A handler's live MXCSR
   edits no longer leak into the interrupted instruction stream.

The adapter validates exact DeliverSignal/SigRestore function fingerprints,
allowing only the known threaded siginfo insertion. It validates the exact shared
throw-case block and unique case boundaries, so scalar-FP staging can still
change its separate cases. The tests pin both immutable whole files and reject
target drift, duplicate anchors and repeated application. Old bound profiles
need fresh staging; rewriting generated consumers is not equivalent provenance.

## Remaining exact gaps and minimum follow-up

- **Unadvertised XSAVE success-like behavior:** `OpXsave` is empty; `Op1ae`
  memory /5 and /6 reach LFENCE/MFENCE, covering encodings otherwise used by
  XRSTOR/XSAVEOPT. Minimum policy correction, if chosen, is explicit #UD for
  those unavailable feature forms, not fabricated state-save success. No
  machine/CPU adapter changes or extra fault experiments were made here.
- **REP BSF/BSR:** current BSF selects `AluLzcnt` and BSR selects `AluTzcnt`.
  Besides the reversed count families, the guest advertises neither BMI1 nor
  ABM/LZCNT. A chosen CPU contract must define ordinary BSF/BSR alias behavior
  for those prefixes or actually implement/advertise the corresponding features.
- **Packed double square root:** the SQRTPD branch of `OpSqrtpsd` reads and writes
  32-bit pieces for 64-bit lanes. This branch is outside the reviewed scalar-FP
  corrections. Correct lane widths and focused finite-value parity checks are
  required before claiming that form's hardware fidelity.
- **Wider SIMD/FP fidelity:** broad C arithmetic, reciprocal/rsqrt estimates,
  NaN/rounding/DAZ/FTZ and exception behavior, x87 precision and FXSAVE format
  remain their existing software model. Routing all selected operations through
  helpers does not close those prior conformance gaps.
- **Optional-prefix acceptance:** the decoder records VEX operands/width and
  dispatch shares legacy opcode maps. Cleared AVX/AVX2 advertisements alone do
  not establish that every VEX/EVEX form is correctly rejected or implemented.
  No AVX family rejection experiment or implementation was added here.
- **Wider signal ABI:** this focused change does not establish arbitrary edited
  FP-state-pointer support, x87 image fidelity, alt-stack restoration, all
  per-exception trap/error mappings, privilege/selector validation on return,
  or reserved MXCSR fault validation. Existing `SigRestore` uses its embedded FP
  image, unlike Linux following the supplied `fpstate` pointer. The selected
  ordinary #UD recovery uses that original valid pointer.

Those are source-audit findings and proposed work, not silently implemented ISA
expansion. The parent owns integration choices and full qualification.

## Windows host implications

Linux guest SIGILL, sigaction structures and ucontext offsets stay Linux guest
ABI even on Windows. A guest #UD helper should install that same guest frame and
throw the managed jump only after leaving generated native code. It should not
execute host UD2 and expect Windows to produce a Linux siginfo frame. Windows
reports physical illegal instructions through its own exception machinery.
[Microsoft exception codes](https://learn.microsoft.com/en-us/windows/win32/Debug/getexceptioncode).

The existing native platform adapter separately gates architecture, dynamic-code
policy, CFG/XFG and shadow-stack policy, and keeps Windows behind explicit
unverified opt-in. Hybrid's flag-using leaves require actual host LAHF/SAHF;
the Full semantic trampoline can check capability without that requirement.
Guest-advertised LAHF/SSE bits cannot authorize a physical optional host opcode.
Nonleaf generated calls or stack frames would additionally require a qualified
unwind design; Microsoft documents runtime function tables for dynamic nonleaf
code. Current semantic helpers execute after the leaf returns.
[Microsoft x64 exception handling](https://learn.microsoft.com/en-us/cpp/build/exception-handling-x64?view=msvc-170).
Actual Windows execution remains **UNVERIFIED**.

## Executed evidence and handoff

Six adapter tests PASS. Five valid static guest ELFs built. Actual Linux native
caught UD2 and recursive/nested UD2 controls PASS with exact output; uncaught UD2
emits its exact prefix and exits by signal 4. Core dumps were disabled and every
owned process group reaped. Native XGETBV was not run as a fault oracle because
the actual host supports it. The actual-host capability query used the proper
OSXSAVE guard. Latest native receipt is embedded in JSON; large files remain in
task storage. One earlier guard test correctly exposed a missing duplicate-case
guard; it was fixed before the passing runs.

The public `FullJitSignals` consumer checks five fixtures × three JIT modes ×
two public execution modes, preserves failed rows, verifies ELF hashes,
compares exact outcomes/output/completed counts, and requires Full to report
positive inline-plus-helper work with zero interpreter attempts/instructions.
Uncaught SIGILL also checks exact fault IP/address/code. It checks resource/cache
release and separate-worker reaping. Raw/processed CoreCLR/NativeAOT builds and
their 30-row runs belong to parent orchestration and are **not claimed run by
this sub-agent**. Run instructions are in the
[test README](../../../tests/FullJitSignals/README.md).

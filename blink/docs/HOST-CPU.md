# CPUID capability selection

The configured interpreter disabled x87 and MMX execution but still advertised
MMX in both feature leaves and FPU in the extended feature leaf. The staged
`src/HostCpu/stage-cpuid.py` adds the corresponding existing exclusion guards to
those three assignments. It checks the complete immutable `cpuid.c` SHA-256
(`0675b9d86847b17398936d052f863762b3ef367ad6d964f7bb64fade3d74abca`) and
each exact replacement boundary, and records the original/staged hashes and
replacement text. No instruction implementation, dispatch table, register write,
vendor string, cache field, or host identification macro changes. The subsequent
profile policy below additionally narrows optional advertisements and zeros leaf 6.

The bit meanings agree with the [AMD CPUID specification](https://www.amd.com/content/dam/amd/en/documents/archived-tech-docs/design-guides/25481.pdf)
and Intel's [CPUID feature definitions](https://www.intel.com/content/www/us/en/developer/articles/technical/trusted-cpu-feature-detection-library-for-intel-software-guard-extensions-intel-sgx.html).
All source references below are relative to the pinned upstream `blink/` tree.

## Exclusion audit

| Feature and CPUID location | Configured result | Actual selected implementation |
| --- | --- | --- |
| FPU: `1:EDX[0]`, `80000001:EDX[0]` | Both clear; extended bit corrected | `fpu.c:1199` retains only FLDCW/FSTCW compatibility operations and faults other x87 operations. `machine.c:1522` maps FWAIT to `OpUd`. |
| MMX: `1:EDX[23]`, `80000001:EDX[23]` | Both clear; both corrected | `sse.c:1354` and `sse2.c:389` select `NoMmx`, which raises undefined instruction. XMM variants remain selected. |
| BMI2: `7.0:EBX[8]`; ADX: `7.0:EBX[19]` | Both already clear | `machine.c:1526` maps `Op2f5`, `Op2f6`, `OpShx`, and `OpRorx` to `OpUd`. Existing CPUID guards already matched. |
| JIT: Blink custom `80000001:ECX[31]` | Already clear | `jit.h` selects the disabled `IsJitDisabled` branch. This is Blink metadata, not a standard CPU feature bit. |
| BCD and privileged/metal operations | No newly advertised corresponding bit | `machine.c:1487` and `:1533` replace excluded dispatch entries with `OpUd`. |

## Original advertisements and retained handlers

This historical table records the original advertisements and selected source
paths. The policy below clears its listed optional bits without deleting these
handlers. Except for the explicit native probes
below, it does **not** establish instruction-family correctness or completed
managed host integration. Features with MMX/x87 forms still require those
separately advertised prerequisites; retaining SSE bits does not enable MMX.

| Feature and CPUID location | Selected path and qualification limit |
| --- | --- |
| SSE/SSE2: `1:EDX[25,26]` | `sse.c`, `sse2.c`, `ssefloat.c`, and `ssemov.c` retain XMM handlers. Native SSE2 PXOR probe passed. |
| FXSR: `1:EDX[24]`, `80000001:EDX[24]` | `machine.c:1234` / `:1255` retain MXCSR/XMM save and restore, with x87 sections excluded. Native FXSAVE/PXOR/FXRSTOR probe passed. |
| SSE3: `1:ECX[0]` | Dispatch retains `OpHaddpsd`, `OpHsubpsd`, `OpAddsubpsd` in `ssefloat.c` and `OpLddquVdqMdq` in `ssemov.c`. |
| SSSE3: `1:ECX[9]` | XMM handlers such as `sse.c:1558` `OpSsePshufb` remain selected. |
| PCLMULQDQ: `1:ECX[1]` | `machine.c:2085` dispatches to the portable implementation in `clmul.c`. |
| POPCNT: `1:ECX[23]` | `machine.c:1136` `Op1b8` calls `Bitscan` with `AluPopcnt`. |
| CMPXCHG16B: `1:ECX[13]`; CMPXCHG8B: `1:EDX[8]`, `80000001:EDX[8]` | `machine.c:240` / `:259` retain both handlers. |
| CMOV: `1:EDX[15]`, `80000001:EDX[15]`; LAHF/SAHF: `80000001:ECX[0]` | `machine.c:995` and `:105` / `:109` retain handlers. |
| FSGSBASE: `7.0:EBX[0]` | `machine.c:1286` through `:1298` read/write guest FS/GS bases. |
| ERMS: `7.0:EBX[9]` | `string.c` retains REP MOVS/STOS paths; this audit makes no performance guarantee. |
| TSC: `1:EDX[4]`; RDTSCP: `80000001:EDX[27]`; invariant TSC: `80000007:EDX[8]`; RDPID: `7.0:ECX[22]` | `time.c:49`, `:81`, `:86` retain paths. Managed clock behavior remains a separate host contract. TSC_AUX is the upstream virtual processor value. |
| RDRAND: `1:ECX[30]`; RDSEED: `7.0:EBX[18]` | `rdrand.c:44` / `:53` retain `GetRandom` consumers. Managed entropy delivery is not qualified by CPUID execution. |
| CLFLUSH: `1:EDX[19]` | `machine.c:1318` performs the interpreter's memory fence; no physical host cache model is claimed. |
| PAE: `1:EDX[6]`; NX: `80000001:EDX[20]`; long mode: `80000001:EDX[29]` | Existing guest page-table, execute-permission, and long-mode paths remain selected in `memory.c` / `machine.c`; these describe guest architecture. |
| SYSCALL: `80000001:EDX[11]` | `machine.c` retains `OpSyscall`. This does not qualify the entire managed syscall/host-service surface. |
| Hypervisor: `1:ECX[31]` | Upstream emulation identity remains set. |

AES, SSE4.1, SSE4.2, AVX, AVX2, and XSAVE advertisements remain clear. The
synthetic cache/TLB leaves 2/4 are preserved, not qualified as hardware models.
The narrowed profile zeros thermal/power leaf 6. The existing maximum extended leaf reports
`80000001` even though `80000007` answers a direct query; this discrepancy is
unchanged. OS/architecture information leaves `40031337`/`40031338` remain
based on honest compilation macros and are deliberately outside cross-host
feature equality comparisons.

## Reproduction and evidence

Run `python3 blink/tests/HostCpu/run.py`. The passing receipt is
`artifacts/host-cpu/attempt-1hjhelms/receipt.json`; `artifacts/host-cpu/latest.json`
points to the latest successful run. Every run snapshots and hashes the host
profile, target storage, compiler, overrides, staged source, and native archive.
It verifies all immutable source-inventory hashes before building.

Eight native combinations independently toggle the x87/MMX/BMI2 exclusions.
For each combination, 16 queries compare original and staged `OpCpuid`: only the
three conditional corrections and explicit policy deltas below may differ, and re-enabled features retain their
bits. The current profile's 16 rows then match translated `OpCpuid` exactly in
raw JIT, raw AOT, postprocessed JIT, and postprocessed AOT. Upper halves of all
four result registers must be zero. Raw generated C# is retained unchanged.
The CPUID-only fixture provides storage for unused mapping inline references
and an aborting test link for the unexercised CPUID-trap branch; neither is a
product host implementation.

Separately, a probe linked to the actual configured native interpreter archive
executes guest instruction bytes through `ExecuteInstruction`, page mapping,
and upstream signal/unwind handling. FLD1, FWAIT, MMX PXOR, PDEP, and ADCX each
raise undefined instruction with SIGILL before completing an instruction.
SSE2 PXOR clears XMM0; FXSAVE/PXOR/FXRSTOR restores its exact bytes and MXCSR.
These seven native checks support the exclusion decision. The managed matrix
executes CPUID itself, not those seven instruction sequences or the full core.

The adaptation closes the demonstrated B004 exclusion/advertisement mismatch.
Remaining timing, entropy, syscall, and broad instruction conformance work stays
explicitly outside this evidence.

## Narrowed optional-feature policy

The new canonical profile clears these advertisements because their instruction
families lack sufficient actual translated-core qualification:

| Leaf/register | Cleared bits |
| --- | --- |
| `1:ECX` | 0 SSE3, 1 PCLMULQDQ, 9 SSSE3, 13 CMPXCHG16B, 23 POPCNT, 30 RDRAND |
| `7.0:EBX` | 0 FSGSBASE, 9 ERMS, 18 RDSEED |
| `7.0:ECX` | 22 RDPID |
| `80000001:ECX` | 0 LAHF/SAHF |
| `80000001:EDX` | 27 RDTSCP |
| `80000007:EDX` | 8 invariant TSC |

Leaf 6 now returns zero in all four registers. This is an explicit virtual CPU
advertisement policy: unadvertised handlers are not claimed to reject execution.
The same complete-file pin and exact replacement boundaries protect every edit;
receipts retain each before/after block. No OS/CPU compiler macros are invented.

The pinned service build uses the matching GCC 13 baseline `-march=x86-64`
default and no optional ISA flags; its exact binary, compiler and musl flags are
recorded in `artifacts/host-cpu/policy-preflight/receipt.json`. This is target
configuration evidence, not a service execution or whole-binary ISA proof.
Independent private clock and entropy boundary contracts remain valid; they do
not qualify RDTSC/RDTSCP/RDRAND/RDSEED instruction semantics or invariant timing.
Baseline RDTSC, FXSR, CMPXCHG8B, broad SSE/SSE2 and system instruction gaps remain
explicit in `tests/CpuConformance/COVERAGE.md`.

The fresh focused policy receipt is
`artifacts/host-cpu/attempt-wf7mfaa6/receipt.json`: eight native exclusion
combinations, seven separate native instruction probes, and all 16 CPUID rows
in raw/optimized JIT/AOT pass. Older focused receipts preserve their original
advertisements. Complete-core policy qualification is recorded separately;
focused direct `OpCpuid` evidence is not substituted for full-core execution.

The narrowed canonical profile is `generated/core-profile/attempt-7i4_ajz4`.
Its complete 109-object link uses 108 exact emission-identity cache matches and
one fresh `cpuid.c` object, with original producer receipts retained in
`artifacts/core/objects/73e56c32ca59cc50c7981a156064beb03e6a9e7d475f9f99aed0dd9923be9964/receipt.json`.
`artifacts/core-execution/attempt-n9ligxbc/receipt.json` passes raw/optimized
JIT/AOT, actual profile ABI comparison and direct IL inventories. The raw and
optimized inventories traverse 5,597/5,587 methods with zero traversed native
imports or audit errors; their 155 indirect sites and 594 virtual sites remain
explicit limitations. Seventeen native declarations exist in the scoped
assemblies; an absence of traversed imports is not an isolation proof.
`artifacts/publication-audit/attempt-803iuwqv/receipt.json` passes the separate
execution-hash-checked ELF publication inventory.

The actual instruction corpus uses that exact canonical receipt, retaining
108 objects and replacing only its authored frontend. All **1,980 comparisons**
(495 cases in raw/optimized JIT/AOT) pass at
`artifacts/cpu-conformance-managed/attempt-jwuzr1go/receipt.json`. Its eleven
actual CPUID instructions include leaf 6, `80000007`, leaf 7/subleaf 1, and
unknown basic/extended leaves. The original 31 inputs are unchanged. The native
reference passes all 495 cases; its 397 original scalar mismatches are retained,
not reclassified as expected results. The runner also rejects the older
canonical CPUID policy before managed emission, demonstrated by
`artifacts/cpu-conformance-managed/policy-mismatch-rejection.json`.

`artifacts/core/canonical-policy-integration.json` joins these exact hashes,
producer identities, current owned sources and scope limits. Earlier canonical
profiles, failure captures and scalar-only qualification receipts are unchanged.

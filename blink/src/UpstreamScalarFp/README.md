# Reviewed scalar floating-point correction

This stages three pinned upstream translation units; it does not edit the
immutable reference or generated C#. `stage.py` checks the complete original
file and replacement-block SHA256 in `patch-inputs.json`, preserves the upstream
license, and emits the staged files, an exact unified diff, and a receipt with
source, block, replacement, patch, script and manifest hashes. The checked-in
`scalar-fp.patch` is the review copy of that same diff.

The correction requires `DISABLE_JIT` at C compilation. The previous
`ComissKernel` and its Jitter path are replaced with the interpreter path under
that explicit restriction. No upstream JIT behavior is qualified.

## Scope and semantics

The Kestrel-discovered `OpCmppsd` repair adds a second nonoverlapping original
`ssefloat.c` block. It changes exactly eight result assignments from the floating
union member to the integer member, preserving true all-one/false-zero masks in
SS, SD, PS and PD. No predicate, NaN policy or unrelated arithmetic changes.
All block boundaries and hashes are checked against the original file before
replacements are applied in descending source order; receipts retain separate
additional-block replacement provenance. Existing adaptations remain intact.

Native qualification passes all514 normal CPU cases at
`artifacts/cpu-conformance/attempt-havagw22/receipt.json`, SHA256
`a400af6ae56ad7e0ba4e6a7b5a157caea94b3ab2d418c7857281ba3cb300c267`.
Ten new cases preserve the first550 descriptors and46 custom fault exclusions.
The exact Hashtable ordered-mask/truncate sequence now yields 2 and5 for2.16
and5.04, matching hardware instead of original Blink's zeros. Managed refresh
and actual corrected Kestrel execution remain pending.

`scalar-fp.patch` preserves exact unified-diff context, including the single-space
prefix on blank context lines. That patch-format whitespace is intentional.


* `cvt.c`: the four scalar CVT/CVTT single/double-to-signed-GPR handlers use an
  unsigned raw IEEE significand/exponent helper. Guest RC selects rounding;
  CVTT forces truncation. Shifts and signed 32/64-bit destination limits are
  checked before forming a result. NaN/infinity/out-of-range inputs raise invalid
  and produce the width-specific indefinite integer when masked. Inexact valid
  conversion raises precision. Invalid takes precedence over precision. DAZ is
  honored, and these instructions do not raise a denormal exception. FTZ does
  not change the integer result. No host FP operation, ambient rounding mode or
  undefined out-of-range C cast is involved.
* `ssefloat.c`: scalar COMIS/UCOMIS classifies raw NaN bits before comparison.
  COMIS raises invalid for any NaN; UCOMIS for signaling NaNs. An unordered
  result suppresses denormal status, as measured with both operand orders and
  DAZ/DM combinations. Finite denormals respect DAZ and the guest denormal mask.
  Successful comparisons assign CF/PF/ZF and clear AF/OF/SF.
* Both paths preserve old sticky MXCSR status and decide whether to trap from
  newly raised, unmasked conditions. They accrue status before a trap and leave
  destinations/EFLAGS untouched on the fault path. Operand memory access occurs
  before status changes. The existing halt path restores the faulting RIP.
* `throw.c`: only the SIMD branch gets Linux guest signal-code classification.
  It examines unmasked guest status, including previous sticky bits, in Linux
  priority order: invalid, zero divide, overflow, denormal/underflow, precision.
  The x87 branch and RIP/fault-address/signal-delivery sequence remain intact.

The architectural requirements are described in Intel's
[Volume 2A conversion entries](https://cdrdv2-public.intel.com/812383/253666-sdm-vol-2a.pdf)
and [Volume 2B UCOMIS entries](https://cdrdv2-public.intel.com/782151/253667-sdm-vol-2b.pdf).
The Linux signal-code ABI is independently checked against hardware and the
[`fpu__exception_code` behavior in Linux v6.12](https://github.com/torvalds/linux/blob/v6.12/arch/x86/kernel/fpu/core.c).
No Linux implementation code is copied into the correction.

## Evidence and boundaries

`tests/CpuConformance` preserves the original 31 input rows unchanged and adds
460 explicit scalar inputs. Hardware executes the actual instruction bytes in
separate processes. The native runner retains both original and staged native
outputs. The final 491-case native pass is
`artifacts/cpu-conformance/attempt-7c2xjo6w/receipt.json` (397 original mismatches);
its earlier all-427-case staged pass is
`artifacts/cpu-conformance/attempt-2vjtvv3r/receipt.json` (333 original mismatches).
Earlier receipts `attempt-tpkjyf1z`, `attempt-mzvm9l6v` and `attempt-quef7ttx`
preserve the measured QNaN/denormal priority and signal-code failures that led
to the reviewed changes. Original failures are not normalized into passes.

The test inputs cover ties under every RC, signed zero, exact values, signed
32/64 limits, both signs of NaN/infinity, masked/unmasked invalid and precision,
sticky status and Linux priority, DAZ/FTZ, compare denormals, and inaccessible
memory operands. Fault-state comparisons include the whole GPR destination,
XMM input, MXCSR, defined arithmetic EFLAGS, RIP, signal and SIMD `si_code`.
MXCSR zero has an explicit present bit. Expected halt classes distinguish
integer divide, SIMD and memory faults. Each managed case must use a fresh
whole-worker lifecycle; no hardware execution is used in the managed consumer.

This is a bounded scalar correction. Packed conversions, general FP arithmetic,
approximations, x87, JIT code generation, and other exception sources remain
outside its qualification. Tests are representative, not exhaustive of all
IEEE inputs, operand encodings or x86 implementations. Masked integer writes
and unmasked destination preservation must remain separate assertions.

## Reproduction

```
python3 blink/tests/CpuConformance/run.py --staged-fp
python3 blink/tests/CpuConformance/run-managed.py --staged-fp \
  --core-receipt blink/artifacts/core-execution/attempt-n9ligxbc/receipt.json
```

The current canonical profile already includes the three reviewed TUs and the
narrowed CPUID policy. Its managed derivation replaces only the CPU frontend,
retaining 108 canonical objects with their exact producer identity. The runner
verifies all three reviewed source bodies and the CPUID policy against the
fresh native reference before reuse.

The earlier scalar derivation replaced the frontend and three reviewed TUs,
retaining 105 original objects. That historical matrix used the earlier CPU
policy; its old CoreExecution baseline is now rejected by the current runner
because policy identity differs. Reproduction uses the current receipt above.
It uses the original canonical header paths, unchanged compiler, frozen host
bindings, and a separate raw/optimized JIT/NativeAOT consumer. The actual managed matrix passed all 1,964 comparisons (491 per mode) at
`artifacts/cpu-conformance-managed/attempt-kenqm2yg/receipt.json`, with
`completed: true`, `passed: true`, `native_agreement: true`. The strict run
compares every defined state field, including SIMD signal codes. No observation
waiver was used. This derived qualification does not mutate or replace the old
canonical core profile.

The identity/guard test `tests/CpuConformance/test-staging.py` checks the reviewed
diff, rejects changed source and block hashes in private copies, and actually
compiles each staged TU against a native profile lacking DISABLE_JIT to confirm
rejection. It never writes to immutable upstream files.

The later narrowed-policy canonical derivation passes all 1,980 comparisons
(495 cases per mode), receipt
`artifacts/cpu-conformance-managed/attempt-jwuzr1go/receipt.json`. It reuses all
three reviewed scalar objects from the canonical profile. The four additional
cases query CPUID policy; scalar source bytes and the original 31 inputs are
unchanged. Earlier scalar-only receipts above remain historical, not rewritten.

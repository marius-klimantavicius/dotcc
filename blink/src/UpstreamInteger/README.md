# Reviewed upstream integer corrections

This staging boundary preserves the immutable Blink reference at revision
`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`. It derives only `alu.c` and
`machine.c`, guarded to the configured `DISABLE_JIT` interpreter profile.

`stage.py --output <directory> --receipt <json>` verifies full source hashes,
nine exact function-block hashes and the complete checked-in `integer.patch`
before writing either source. Its deterministic receipt records the upstream
revision, script/patch hashes, original/staged source hashes and reviewed block
hashes. Config-bearing includes precede the compile guard. Canonical profile
integration supplies host bindings afterward; native verification compiles the
same staged bodies against the pinned native headers and original archive.

Four INC helpers incorrectly calculate auxiliary carry against the unused
second argument. Comparing the result low nibble with the input low nibble
restores carry from bit3 while preserving INC's existing CF behavior. The
CMPXCHG8B nonmatch path stores the observed32-bit halves through Write64 so
RAX/RDX are zero-extended, matching architectural writes to EAX/EDX.
No instruction comparison or immutable upstream source is relaxed or edited.

The motivating native hardware differences are preserved in
`artifacts/cpu-conformance/attempt-cjel3vz8/receipt.json`, SHA256
`75f0c3a7bccc23e6b014116d8430a0a3a849637f82ed0fb5228fcf7048150812`:
IDs495/496 differ in AF and ID510 retains upper32 register bits. That466-row
native attempt failed before any managed comparisons. IDs512/513 add normal
INC8/16 inputs to exercise the other two repaired widths; first512 descriptors
remain pinned. Corrected native validation passed all468 selected rows (46 fault rows excluded)
in `artifacts/cpu-conformance/attempt-lc9j96ag/receipt.json`, SHA256
`0a58ffb7f3a083709795ac1b86a93bff1fb44420f35690614b9227aefdbc2e41`.
It retains364 original-native differing rows, including AF at all four repaired
INC widths and the CMPXCHG8B register-width difference. All corrected defined
states match hardware; virtual CPUID follows the recorded profile policy.
Translated validation against the corrected canonical profile passes all 1,872
comparisons (468 per raw/optimized JIT/NativeAOT form) at
`artifacts/cpu-conformance-managed/attempt-sgren8zu/receipt.json`, SHA256
`e470675d116c505f377eff74671f709bac6c0b28a366646e5f6e1c94ffdf23d7`.
The matrix retains both exact corrected canonical producers; only the authored
CPU frontend is replaced.

CpuConformance requires explicit `--staged-integer` selection. It retains the
original-native captures/differences separately from corrected-native results
and independent hardware captures. Managed derivation reuses canonical objects
only when both exact staged source bytes and the canonical boundary receipt
match; integer staging rejects older or mismatched canonical profiles. This does
not qualify guest JIT paths, concurrent CMPXCHG atomicity, or broader instruction
families. No fault-injection or invalid-image cases are introduced.

## NEG auxiliary-carry extension awaiting qualification

The500-case native run at `artifacts/cpu-conformance/attempt-hmylb6uo/receipt.json`
(SHA256 `ed8badd8f2400f05e13f3fbee247a8d11c3b49b5b711c3e3e10c0e0c7fe5ad70`)
completed but failed ID518, NEG8 of0x80: hardware AF0, original/staged AF1.
The other31 added normal cases passed, including RDTSC width/preservation
invariants. No managed execution was started for that failing selection.

Pinned Neg8/16/32/64 all derive AF and CF together from `!!x`. NEG is subtraction
from zero: CF is set for any nonzero input, while AF is set only when the low
nibble is nonzero. The reviewed patch now separates `af = !!(x & 15);` and
`cf = !!x;` on the existing width-truncated input before negation. Full source
and exact function hashes are verified; no comparison masks are relaxed.

The first546 descriptors remain unchanged. IDs546–548 add NEG16/32/64 minimum
values to witness AF0 at every repaired width, and ID549 adds NEG8 input1 to
witness AF1. The504-row native matrix passed as recorded below; the2,016-comparison
managed matrix remains pending.
Earlier INC/CMPXCHG8B receipts document their own exact smaller patch identity
and remain valid historical evidence, not validation of this extension.

Corrected native504 qualification passed in
`artifacts/cpu-conformance/attempt-ovjovt6b/receipt.json`, SHA256
`798f5c3eb0a01dbcfe5135331ab746931895ee1bc1e56e511538af5ff0fdec33`.
All504 selected rows match the reviewed native reference/hardware contracts;
46 custom fault rows remain excluded and368 original-native differences remain
separately recorded. Hardware/original/staged RDTSC invariants pass. The corrected
alu.c body hash is `095e490901c5cdba26cd02d4c8381be008ce78da3802f7737618db77f7854301`.
Managed2,016-comparison qualification awaits the corrected canonical producer.

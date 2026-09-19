# Reviewed upstream integer corrections

This staging boundary preserves the immutable Blink reference at revision
`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`. It derives only `alu.c` and
`machine.c`, guarded to the configured `DISABLE_JIT` interpreter profile.

`stage.py --output <directory> --receipt <json>` verifies full source hashes,
five exact function-block hashes and the complete checked-in `integer.patch`
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
Translated validation against the new canonical profile is still pending.

CpuConformance requires explicit `--staged-integer` selection. It retains the
original-native captures/differences separately from corrected-native results
and independent hardware captures. Managed derivation reuses canonical objects
only when both exact staged source bytes and the canonical boundary receipt
match; integer staging rejects older or mismatched canonical profiles. This does
not qualify guest JIT paths, concurrent CMPXCHG atomicity, or broader instruction
families. No fault-injection or invalid-image cases are introduced.

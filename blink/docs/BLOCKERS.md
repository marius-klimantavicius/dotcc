# Observed blockers

These are observed campaign results. A passed native baseline is not translated
execution; P1–P6 remain open.

## B001 — Comma-separated bit-field declarations (fixed, validation running)

Untouched `blink/x86.h:37` contains valid C `uint8_t omode : 2, genmode : 6;`.
The first dotcc decoder attempt rejected its comma. A reduced native-checked
fixture now lives at `DotCC.FunctionalTests/Fixtures/bitfield-declaration-list`.
The generic parser and IR now preserve named/anonymous bit-field lists and their
widths. All five focused bit-field functional cases pass. The full affected
repository build/unit/functional check is being rerun before committing this fix.

## B002 — Native header coupling in decoder inclusion (probe adaptation)

`x86.c` includes `modrm.h` only for the selected profile's `Mode` macro, which also
pulls the entire Machine/POSIX host header closure into the decoder. The initial
attempt reports unresolved `sys/uio.h`, `termios.h` and a `signal.h` parse error.
`stage-decoder.py` verifies source hashes then replaces this include in staged C
with the exact upstream DISABLE_METAL `Mode` expansion. Untouched and staged
native decoder probes agree on all 11 cases and ABI output. This scoped decoder
seam does not resolve the complete core's POSIX dependencies.

## B003 — Multidimensional array parameters (open)

The staged decoder reaches `xed_set_simmz_imm_width_eosz` with parameter
`const u8 eosz[2][2][3]`; dotcc rejects the second array dimension. This requires
generic parameter declarator support preserving pointer-to-row stride, with a
native-checked regression and retry of the actual upstream decoder.

## B004 — Restricted CPU feature advertisement (open)

Upstream extended CPUID leaf 0x80000001 advertises FPU even under DISABLE_X87.
The selected managed product needs an explicit reviewed capability mask and
execution tests for every advertised bit. See HOST-CONTRACT.md.

## Platform and packaging limits

Linux x64 is the only observed execution host. Windows execution and the complete
raw/optimized × JIT/NativeAOT matrix have not run. Native host GCC/binutils/.NET
binaries are recorded by version/hash; their distribution packages are not yet
archived as hermetic toolchain inputs.

# Selected musl random-number routines

The selected provider translates musl 1.2.6's eleven PRNG translation units:
`rand`/`srand`, `rand_r`, the BSD `random` state family, and the 48-bit family.
Original source bytes, license, hashes and exact patches are retained under
`backend/runtime/musl/prng`. These are deterministic generators, not a source
of cryptographic randomness.

The arithmetic and state formats remain the original musl algorithms. The
private visibility annotation is removed, two `void*` returns are explicitly
cast for C++ input mode, and the private `random` lock uses same-width acquire/
release atomic operations with the portable C11 yield service while contended.
No native pthread layout or compile-host platform choice enters generated code.
`random` state changes are serialized; this does not add a thread-safety promise
to musl's other generator families.

Mutable generator state and saved seed buffers follow the CLI instance-global
convention. Caller-provided `initstate` and 48-bit buffers remain caller owned.
No per-call managed allocation or pinned allocation is introduced by the PRNG
algorithms; their persistent globals use ordinary generated global storage.

Acceptance comprises nine original/adapted and generated C/C++ static/instance
JIT/NativeAOT checks at `artifacts/campaign/libc-prng-translated-002`: exact
streams across extreme seeds, every BSD state-size boundary, state switching,
48-bit custom parameters, forced GC, independent owners and a barrier-started
four-thread `random` sequence sum. Eight automatic provider checks pass at
`musl-provider-prng-001`; exact patch replay is retained at
`libc-prng-patch-001`. The selected provider now contains 129 source units.
Only Linux-x64 execution has been verified. The broader libc plan remains open.

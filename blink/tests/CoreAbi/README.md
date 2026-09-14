# Actual upstream core storage probe

`python3 blink/tests/CoreAbi/run.py` verifies the pinned source tree, snapshots the
profile, and compiles the unchanged upstream type headers. Native C first uses
the original host jump storage, then the explicitly authored signal-jump record.
The latter is the matching storage oracle for the managed host profile. Only
that jump type changes in the native profile; standalone HostAbi checks qualify
the other authored host records against their native equivalents.

The emitted form is a nested managed library in `Managed.Emulation`, with a
separate consumer. Both raw and semantically postprocessed sources execute under
JIT and NativeAOT on Linux x64. The complete generated library is rooted during
AOT publishing. The raw snapshot is checked unchanged after optimization.

All 236 output rows match the native profile. They cover actual field addresses,
aggregate placement and array strides, not just compiler-reported metadata:

- Machine, System, machine snapshots, descriptor caches, page/TLB storage;
- register byte aliases, high-byte mutation, vector lane bytes and 16-byte alignment;
- descriptor callback table and file/socket storage;
- upstream ELF headers/segments/symbols and Linux syscall records.

The ordinary native Machine is 22432 bytes; the explicit signal-jump profile is
22576 bytes, including final 16-byte alignment. The jump record grows from 200 to
336 bytes at offset 1264. System remains 3016 bytes. The receipt records all measured
deltas and exact source/profile/compiler/generated/output identities. Builds and
AOT publishes passed without warnings in the observed run.

Two harness globals satisfy references in retained upstream header inline
functions. This probe calls none of those memory functions and provides no
mapping implementation. The probe does not execute guest instructions, create
an emulation instance or complete P1. Global executable output remains unsuitable
for Blink's `System` type; the planned nested-library output preserves its name
and uses qualified BCL references.

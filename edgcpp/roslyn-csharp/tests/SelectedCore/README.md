# Explicit managed roots

This checker is prepared for the actual captured core316 definition with indexed
lookup name `GetVectorType` and exact linkage
`_ZNK6duckdb12VectorBuffer13GetVectorTypeEv`. The pinned input SHA-256 is
`cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65`.
The captured five-node body reads the unsigned byte at offset 24 in a complete
56-byte, alignment-8 record. `CapturedBindings.cs` binds the actual first
publication's `Translated.Program.GetVectorType` and
`Translated.VectorBuffer_BorrowedReadView`. The standalone checker has not yet
been built or run.

The no-argument checker calls the selected getter twice on each of two independent
aligned raw buffers through five mutations. The first buffer receives
0/127/128/201/255 and the second receives 255/201/128/127/0. Every call must return
the fresh unsigned value and preserve all 56 input bytes plus both 8-byte guards.
The published typed view must physically measure
size 56, field offset 24, sequential-prefix offset 8 and array stride 56. The outer
receipt binds the input, root request, completion files, generated source, build
and checker binary. Constants printed by the checker are attribution labels, not
independent proof of the compiled project's identity.

These inputs are borrowed layout bytes. This test supplies no native DuckDB
oracle, C++ object construction, virtual dispatch, ownership/lifetime or whole-core
translation proof.

The bounded integration campaign also selects `fixture` plus `lifetime_count`,
`lifetime_overflow`, `lifetime_live`, `lifetime_event` and `lifetime_value` from the
unchanged pointer-call-chain input. Its existing checker and native oracle are
reused against that separate publication to exercise selected calls, storage and
recorded builtin closure.

One genuine unresolved boundary uses the same core316 input and indexed lookup
name `AssertNotNull`, exact linkage
`_ZN6duckdb10shared_ptrINS_12VectorBufferELb1EE13AssertNotNullEb`. Its saved actual
body selects declaration-only `_ZN6duckdb29ThrowNullSharedPtrDereferenceEv` in that
input. This request must remain unsupported and unpublished without a supplied
definition or native binding. The separate known defining object is not an
implicit input. This is the sole new boundary case; no malformed roots or input
corruption are required. Default retain-all preservation is checked against a
small frozen set of existing outputs by the coordinator.

# Backend-owned representation and separate translation units

The current implementation emits C# while traversing EDG IL. Semantic multiple
translation units already work for synthetic C and C++ fixtures. Separate
compilation remains an open architecture gate: libc++ C++20 header combinations
produce merged-TU diagnostics in both the unchanged C backend and this backend.
Explicit unity source probes establish individual library component behavior;
they are not the production solution to that gate.

## Migration boundary

`dot_gen_be.c` currently keys names, globals/TLS, routine addresses, aggregate
layouts, local frames, constants and RTTI descriptors by EDG pointers. Cleanup
and sequencing helpers also consume EDG lifetime/control-flow objects directly.
A linker cannot safely combine emitted C# fragments: independently generated
names, inline local statics, RTTI tokens, owner state and startup routines would
have incompatible identities. The owned representation must precede final
owner/layout/helper emission.

1. Copy source locations, explicit target types/layouts and symbol declarations
   to typed integer IDs. Pointer-to-ID adapter maps may exist during ingestion;
   serialized objects must contain no EDG pointer identities. Preserve linkage,
   mangled identity, qualifiers, declaration/definition status, alignment,
   field/bitfield offsets, complete/incomplete status and COMDAT association.
2. Copy typed operations, constants/relocations, object storage, sequence
   boundaries, statements, normal/exceptional edges and lifetime transitions.
   Normalize once-only evaluation here. C# text is output, not a substitute for
   typed operations. Copy function-region data before EDG releases its region.
3. Verify referential integrity, layouts, call/return and pointer signatures,
   branch targets, object lifetimes, cleanup ownership and target capabilities
   before C# publication. Unknown IL must retain a source-located diagnostic.
   Existing direct-emitter behavior remains the differential migration oracle.
4. Serialize a versioned object graph with target/header/provider/ABI hashes.
   Presentation options such as namespace, wrapper and split sizes bind only
   after semantic assembly; static/instance calling convention is a semantic
   compatibility condition.
5. Link declarations, definitions, COMDAT groups and relocations by semantic
   identity. Match externally named entities and verify their complete recursive
   type/layout definitions; names alone never establish compatibility. Preserve
   independent TU-local symbols. Unify inline function-local statics and RTTI,
   resolve startup/TLS ordering and reject missing or incompatible definitions.
6. Generate one owner/runtime/RTTI/global-storage graph from the linked program,
   then apply the existing physical C# split controls. No textual unity merge
   or post-hoc replacement of generated identifiers substitutes for linking.

## Verification gates

Use synthetic fixtures throughout this campaign: opaque C forward types;
independent same-spelled internal objects; function/variable templates and
COMDAT local statics; recursive records; cross-TU virtual and multiple
inheritance; RTTI/class and pointer exceptions; constructor/TLS ordering;
function pointers with explicit owners; and literal/global relocations.
Negative fixtures must reject duplicate strong definitions, incompatible
complete types, unresolved externals and mismatched target/owner ABI versions.
Compare semantic merged-TU output and the original C backend where applicable.

This document records a concrete migration boundary, not an implemented IR or
linker. Source diagnostics, semantic multi-TU support and output splitting are
already independently tested; they do not close these remaining gates.

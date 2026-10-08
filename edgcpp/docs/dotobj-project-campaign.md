# Owned semantic object implementation campaign

The acceptance target is translation and execution of SQLite and DuckDB through
independent SQLite `.dotobj` files produced from original translation units.
The managed linker and C# emitter must work without the original source files or
EDG. A successful small language fixture is supporting evidence, not completion.

## Coordination and ownership

The supervisor coordinates two implementation workers: native semantic capture
and managed linking/emission. Root owns the real-project manifests, restartable
translation driver and project acceptance probes. The supervisor owns schema and
packaging changes, contract integration, serialized native preparation/builds,
campaign evidence and milestone commits. File ownership is explicit; only one
compiler build and one Git mutation may run at a time. Retain intermediates.

The baseline is commit `d4d4a115e`, profile `scalar-control-v11`, with 527 checks
including NativeAOT. This baseline is deliberately bounded: it does not translate
either target project. Existing legacy direct-backend project outputs do not
satisfy the object-pipeline acceptance target.

## Implementation sequence

1. Establish restartable original-source project manifests and bounded-memory
   probes. Preserve all frontend commands, input hashes, logs and object results.
   SQLite uses the existing thread-safe Unix/musl source configuration. DuckDB
   includes its bundled dependencies and disables native extension loading.
2. Implement the complete C path driven by SQLite failures: global/static/TLS
   storage, constant data and symbolic relocations, literals, function types and
   indirect calls, record layouts, unions/bitfields/enums, expression sequencing,
   source control flow, varargs and selected runtime externs. Static initializers
   retain typed roots and field metadata so emission can use named aggregates and
   bulk constant spans rather than byte-by-byte assignments.
3. Link independently produced SQLite and runtime objects; bind the existing
   shared portable runtime. Verify SQL, allocation, threading and filesystem
   behavior against native SQLite, then run representative project tests and
   NativeAOT. Keep source names and structured control flow reviewable.
4. Implement the C++ path driven by independent DuckDB translation units:
   template/inline identity and COMDAT selection, references and object ABI,
   constructors/destructors, precise lifetime activation, exceptions/noexcept,
   virtual dispatch, RTTI/member pointers, static guards and library/runtime
   requirements. Consult stock EDG C lowering and the working direct backend;
   do not alter EDG frontend semantics to fit the emitter.
5. Link all required DuckDB objects and bundled dependencies using bounded body
   loading. Run SQL and DuckDB test selections against the native configuration,
   then NativeAOT. Resolve actual failures before broadening acceptance claims.

## Non-negotiable representation and output properties

- Objects contain owned typed semantic identities, expressions, statements,
  layouts, lifetime information, source snapshots/locations and captured macros.
  They do not contain opaque C# bodies or depend on the rejected old exporter.
- Renamable objects use identities throughout. Linking resolves cross-unit names
  and compatible definitions without retaining EDG addresses or frontend state.
- Internal preprocessing happens once per original source translation unit.
  Do not restore the memory-heavy whole-program merge or external `.ii` pipeline.
- C# preserves source names, named members and recognizable structured flow.
  Synthetic code is restricted to incompatible source semantics. Output remains
  splittable and namespace/class configurable; instance globals remain opt-in.
- Globals and TLS may use pinned arrays; heap and short-lived native storage use
  native allocation. Shared runtime sources support NativeAOT and cross-platform
  execution through BCL services or explicit runtime OS selection.
- Missing semantics fail explicitly. Emitting required extern declarations is
  distinct from providing implementations and passing execution tests.

## Evidence and completion

Each verified substantial milestone records commands, hashes, test results,
retained paths and remaining project blockers and is committed separately.
Acceptance requires independently packaged project/runtime objects, link without
source/EDG, compiled generated C#, native-result correspondence and NativeAOT
execution. Cross-platform design is not reported as cross-platform execution
unless that platform was actually tested. The broader C++ and libc language plan
remains explicit even where project acceptance does not exercise every feature.

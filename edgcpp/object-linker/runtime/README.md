# Shared runtime bindings

`bind.py --project OUTPUT` supplies implementations for the runtime contracts
emitted by `dotobj-link link --allow-externs`. It reads the owned link metadata
and source-layout map; it does not need EDG or the original C/C++ input files.
The generated project then builds normally with `dotnet build` or NativeAOT.

The implementation source remains in `edgcpp/backend/runtime`. Bindings reuse
the direct backend's maintained import tables and helper dependency graph.
Allocation helpers are selected from their existing canonical definitions in
`dot_gen_imports.h` and `dot_gen_lifecycle.h`; allocation algorithms are not
forked here. Heap allocations use the same `NativeMemory` implementation.

Platform imports live in `Edg.Runtime.LibC`. Imports whose signatures mention
translated records stay in their translated owner. Adapters bridge opaque
`FILE*`/`DIR*` and selected portable POSIX structures only after validating the
owned size, alignment, member names, member types, and offsets. A foreign
structure with the same tag name does not qualify. These boundaries connect to
the existing BCL runtime, including its runtime-selected platform operations.

`stdin`, `stdout`, and `stderr` use the actual pinned pointer cells owned by
`LibcFiles`; taking their address or changing their value does not access a
second copy. `errno` resolves to the runtime's per-thread pinned cell. Accessors
are evaluated on the accessing thread. A static pointer relocation to one of
the standard stream cells therefore aliases the same cell as later loads and
stores. The `LibcFiles` and POSIX services share descriptor state.

The current provider owns static per-assembly state. Independent engine
instances and explicit owner disposal are not implemented by this binder yet.
The binder is intentionally incomplete: unsupported imports remain unresolved
partial methods. No native fallback or fabricated success body is generated.
`--allow-unresolved` can stage the known bindings for review, but unresolved
methods still fail compilation. In strict mode a failed binding attempt writes
only its diagnostic receipt. Use a fresh linked output for each attempt; existing
outputs are retained rather than overwritten.

`runtime-bindings.json` records the exact canonical source hashes, import-table
hashes, reused runtime files, generated files, and unresolved requirements.
This is binding evidence, not execution evidence. Run
`tests/runtime_bindings_verify.py --output-dir FRESH --compiler CPFE_DOT
--linker DOTOBJ_LINK_DLL --nativeaot` for native/JIT/NativeAOT comparison,
standard stream address aliases, descriptor interoperability, typed layout
rejection, allocation semantics, and errno thread/GC isolation. The native
oracle currently requires a POSIX host; verified platform coverage is recorded
separately from the runtime's cross-platform design.

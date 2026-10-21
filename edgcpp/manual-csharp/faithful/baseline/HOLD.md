# Research HOLD — 2026-10-21

The research writer is explicitly **HOLD** after this bounded checkpoint.
Ownership was exclusively `faithful/baseline/` and `faithful/inventory/`.
No original EDG, old prototype, other worker files, or coordination records
were written. No subdelegation occurred. No native/compiler execution, build,
test, native preparation, or Git mutation occurred. Only Python report/hash
tools and read-only source, configuration and ELF inspection ran.

Selected first native profile: **cpfe**, including
`BACK_END_IS_C_GEN_BE=1`, `COMPILE_MULTIPLE_TRANSLATION_UNITS=1`,
`DO_IL_LOWERING=1`, `LOWER_FIXED_POINT=1`, `LOWER_IFUNC=1`,
`NEED_IL_DISPLAY=1`, `RECORD_BACKING_EXPRS_WITH_IL_LOWERING=1`,
`USE_CMAKE_DEFINES`, and the exact recorded `-I`/host compile/link flags in
`NATIVE-PROFILE.md` and `build-commands.json`. Do not mix the separate
cpfe-dot-oracle definitions into this profile.

Independent source/configuration confirmation: `an_integer_value` is an embedded
array of eight unsigned 16-bit parts, high part first, 128 bits; host-large
signed/unsigned map through ptrdiff_t/size_t to LP64 long/unsigned long;
`a_boolean` and `a_byte_boolean` are native bool. Active inspected macros:
`INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0`,
`USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1`, `FIXED_POINT_ALLOWED=1`,
`INT128_EXTENSIONS_ALLOWED=1`, `CHECKING=1`, `DEBUG=0`.
Derived size/alignment 16/2 is not a measurement. Supervisor owns the fresh
probe. `integer_abi_probe.cpp` is authored and deliberately uncompiled/unrun.

Current input manifest: 570 files, covering declared source/header/table,
runtime/configuration/generated diagnostic and existing executable scopes.
`local-files.sha256` SHA-256:
`593efce8d832d64aa95dcb2aac1db92dc4455224b0090c07b785f235181f3b1d`.
`const_ints.c` SHA-256:
`5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c`.
`cmake_defines.h` SHA-256:
`35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f`.
`build/bin/cpfe` SHA-256:
`e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.

All 283 files under `upstream/src` have exact whole-file hash accountability;
75 translation units belong to cpfe. Approximate discovery: 16,924 routine
candidates, 10,715 macro definitions, 7,453 type-declaration candidates,
1,299 includes, 44,071 conditional directives, and 8,707 unresolved
markers/regions. Counts include inactive alternatives and do not count enabled
or qualified routines. The graph/inventory remains explicitly partial.

The source-defined next closure is the 29 selected native integer leaf routines
and six supporting macro names in `../inventory/const_ints-initial-closure.json`.
Every selected routine has original range/file/hash, branch-token candidates,
native operation notes and manually selected multipart direct dependencies;
managed correspondence and independent review remain pending. Preserve native
post-mutation sign reads in add/subtract and quotient/remainder output order.
Disjoint next source contracts are identified in `../inventory/ENTRYPOINTS.md`.

P0 remains incomplete: effective expanded macros, complete enabled methods
(including templates/inlines/macro bodies), full type/ABI/schema dependencies,
unclassified regions, cross-file callbacks, state/identity/lifetime ledgers,
and original/managed reviews are not all resolved. Existing binary/current
source correspondence is unproven. Initialization-width research is owned by
the separate assigned worker. Supervisor may now serialize fresh ABI/native
probes and held managed validation after all other shared writers also HOLD.

`OWNED-SNAPSHOT.sha256` enumerates exact bytes of every owned deliverable except
itself, using repository-relative paths. Its own hash is returned in the task
handoff. Further writes require a new task/resume and a new snapshot.

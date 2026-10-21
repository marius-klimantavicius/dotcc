# Bounded frontend accountability checkpoint

Status: **PARTIAL; P0 is not accepted.** Research HOLD means the researcher has
stopped writing, not that inventory, translation, review, or parity is complete.
The first native profile is **cpfe** with its own recorded command definitions,
as pinned in `../baseline/NATIVE-PROFILE.md`; no mixed target macro set is used.

`files.jsonl` accounts for every one of the 283 files currently under
`edgcpp/upstream/src` (including its nested CMake files): 77 `.c`, 172 `.h`,
one `.cpp`, 23 `.def`, nine `.txt`, and one extensionless file. Every row has
its whole-file SHA-256 and its compile-database memberships. No source file is
silently excluded for having zero discovered routines.

`source-units.tsv` adds a subsystem grouping and explicit disposition to every
file. All 75 sources compiled in the selected cpfe target remain accountable;
compiled backend/lowering files are not silently removed just because code
generation is outside the initial deliverable. Their reachable dependencies
must be established before individual exclusions. Local `dot_gen_be.c` and
`dot_sys_predef.c` overlays belong only to the existing cpfe-dot build and are
preserved. `ms_metadata.cpp` is present but absent from the Linux compile
database; `src/CMakeLists.txt:237–244` adds it only under MSVC. That file is
explicit configuration backlog, not an implemented Linux dependency.

The reproducible textual discovery tool is `scan_inventory.py`. It invokes no
compiler, EDG, Git, build, or preparation. Its reports cover both sides of
preprocessor alternatives and deliberately do not assign enabled/disabled
status merely from textual presence:

- `routine-candidates.jsonl`: 16,924 candidate records with source ranges,
  exact selected byte hashes, file hashes, surrounding guard history, internal
  preprocessor directives, ordered lexical call candidates, branch-token
  locations, and explicit unstarted/unreviewed state.
- `macro-definitions.jsonl`: 10,715 textual macro definitions, preserving
  continuation text, exact block/file hashes, locations and guard history.
- `type-candidates.jsonl`: 7,453 declaration candidates with locations and
  line hashes. Multi-line typedef names/layouts are unresolved.
- `include-directives.jsonl`: 1,299 includes with locations/guards. This is
  textual include accountability, not a compiler-expanded include closure.
- `conditional-directives.jsonl`: 44,071 conditional directives retained in
  source order within each file, including enabled and excluded alternatives.
- `unresolved-regions.jsonl`: 8,707 unresolved end markers or regions outside
  discovered routine spans. These remain explicit review obligations.
- `summary.json`: machine-readable counts and limitations.

These are discovery counts, **not counts of enabled routines or parity-qualified
routines**. The index is not a complete call graph. Names before `(` can be
macros, casts, declarations or constructors. Calls through pointers, overloads,
virtual dispatch, macro expansion and templates are unresolved. Constructors,
operators, inline bodies and nested namespaces can be missed. A known example
is the `String_formatter` methods in `const_ints.c:1786–2168`: qualified template
method names are not fully recognized and declaration text can enter a candidate
span. Raw strings and macro-generated definitions also require manual handling.
All untouched candidates retain uninspected state/identity/lifetime contracts.

`ENTRYPOINTS.md` records the manually opened driver path. The initial integer
leaf closure is in `const_ints-initial-closure.json`; each selected candidate is
cross-referenced to its source/hash and has manually selected active direct
dependencies. It is a native research ledger, not an independent C# review.
Every implementation must still provide exact managed ranges/hashes, complete
source-block mapping, operation/effect/alias/lifetime contracts, diagnostics,
all branch alternatives and a reviewer who opens both C++ and C# bytes.

Native runtime sources/headers (81 files under `upstream/lib_src`), supplied
headers, build configuration, generated diagnostics/tables and executables are
separately pinned in `../baseline/local-files.json`. Runtime entry/ABI contracts
and routine coverage in those areas remain incomplete. Tests, benchmarks,
arbitrary system headers/libraries, most development tools, and the old prototype
are outside this bounded discovery scan. The manifest names its scope and
exclusions; no imported-revision-only provenance claim is made.

To reproduce reports after supervisor authorization:

```
python3 edgcpp/manual-csharp/faithful/baseline/pin_baseline.py
python3 edgcpp/manual-csharp/faithful/inventory/scan_inventory.py
python3 edgcpp/manual-csharp/faithful/inventory/pin_initial_closures.py
```

These commands only read original inputs and write their owned reports. An
input change invalidates the held snapshot and needs a new manifest/review.

Remaining P0 blockers: compiler-expanded effective configuration/header closure;
exact enabled routine enumeration including inline/template/macro bodies;
complete type/macro/schema ownership and source-block mapping; resolution of
unclassified regions; source-defined cross-file dependency closure and callback
targets; per-routine effects/identity/lifetime; executable/source correspondence;
supervisor ABI evidence; and independent original/managed method review. The
initialization-width investigation is separately assigned by the supervisor and
is not duplicated here. Do not mark P0 complete from these bounded counts.

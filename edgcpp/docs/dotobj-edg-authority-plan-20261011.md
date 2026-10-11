# EDG-authoritative dotobj continuation

This is the user's requested implementation direction, effective 2026-10-11.
It supersedes the previous source-semantic reconstruction campaign. Continue
from the existing compiler, SQLite objects, managed linker/generator and runtime;
do not restart the project or discard previous work.

## Authority and boundaries

EDG performs C/C++ semantic analysis. The exporter records its results. Neither
the exporter nor the managed reader may independently redo overload resolution,
template substitution, constraint satisfaction, lexical lookup, or the language
rules determining destruction and exception specifications.

The user explicitly requires the C# linker and code generator to assume that
`.dotobj` is semantically correct. Semantic diagnostics are appropriate only
when evidence identifies exporter omissions or mis-exported facts; repair the
exporter rather than implementing a parallel C++ semantic checker in C#.
The managed side consumes EDG's selected results, including selected overloads,
types, conversions, exception policy and lifetime decisions. It does not prove
again that EDG selected them correctly.

The SQLite object owns EDG's typed program graph: expressions, statements,
types, selected declarations, dependent forms, source positions, macros,
initialization, lifetimes and exception regions. Shared nodes, cycles and
original versus lowered representations remain explicit. Use object-local IDs
and typed references, never persisted frontend pointer addresses. Capture facts
before EDG mutates or releases them where necessary. Follow stock IL traversal,
serialization and lowering machinery instead of inferring source ownership from
whichever traversal happens to encounter a shared node first.

A dependent or uninstantiated original expression is retained as EDG represents
it. It does not need a second, normalized interpretation to be serialized. A
shared expression and its uses may have different contexts; preserve EDG's
actual context relationships rather than inventing one universal binder.

Object validation checks format and referential integrity, valid typed field
encodings, bounds, and compatibility of the recorded format/configuration.
Linking resolves EDG-provided symbol identities, selected definitions, weak and
COMDAT rules, and actual calling/storage ABI requirements. Generation consumes
resolved executable semantics and recorded lifetime facts. These responsibilities
must not become a second C++ semantic analyzer.
Ordinary link errors (such as a missing required definition) and unsupported C#
emission operations remain meaningful; neither licenses semantic reanalysis.

Retain all source/type information. Do not solve source-only failures by deleting
metadata, substituting source text or opaque blobs, suppressing required runtime
operations, or globally disabling checks. Remove or replace existing semantic
reconstruction when its recorded EDG facts make it unnecessary. A missing
serialization field is an exporter coverage defect; an uninterpreted but fully
preserved source recipe is not an executable-language failure.

## Preserve and reuse

Keep the working execution/lifetime/EH representation, readable C# emission,
shared runtime, source snapshots/macros, compression, tests and real-project
fixtures. Existing source tables can remain useful projections; they must not
remain mandatory semantic proofs for the faithful graph or executable output.
Do not force a rewrite of the generator merely to change metadata storage.

Baseline: accepted v62, commit `74cfa57dc`, native160 / managed191. Unaccepted
member-substitution and streamed-emitter/catalog work remains in the working
tree and retained artifacts. Review and reuse what is sound; do not silently
treat it as accepted. A pre-transition copy of all changed/untracked files in
the EDG implementation/documentation areas, full tracked diffs and repository
status is retained at `artifacts/campaign/edg-authority-handoff-20261011`.
No reset, cleanup, artifact deletion or broad Git staging is authorized.

## Implementation sequence

1. **Correct the representation boundary in the existing implementation.**
   Inspect the configured EDG IL declarations and existing stock walkers/writers;
   use their field coverage to implement faithful typed capture, reusing existing
   capture wherever it already records the facts. Separate retained original
   graph data from executable/link contracts. Remove the dependency on semantic
   reconstruction in native source serialization, `ObjectFile` loading and the
   linker's global comparisons of uninstantiated template/source contracts.
   Keep the change reviewable, but do not introduce another feature-by-feature
   semantic proof system. Document schema/configuration and genuinely missing
   fields as part of implementation, not as an open-ended preliminary project.

2. **Prove the architectural correction on real failures.**
   Use `unittest.cpp`, `cast_operators.cpp` and the existing shared-decltype,
   implicit-this and dependent-member fixtures. They must retain their complete
   original typed graph and export without reconstructing their language
   semantics. Check that relevant EDG identities/references and lifetime facts
   survive a source-hidden round trip. Existing passing executable fixtures
   must still generate equivalent behavior. Validate corruption at the format
   boundary and actual linking conflicts at the linker boundary.

3. **Finish coherent project export and linking.**
   Freeze one compiler/schema/reader and a documented consistent DuckDB/provider
   configuration. Export every selected original TU into its SQLite `.dotobj`,
   retaining manifests, receipts and real failures. Compare existing C++20
   configuration evidence; do not turn harmless uninstantiated header variants
   into cross-unit definition conflicts. Implement the saved disk catalog and
   bounded metadata loading using existing linker/emitter code. Generate the
   complete selected library and dependencies without EDG or original sources
   at link time. Track tests separately where they form separate executables.

4. **Then finish runtime closure and execution.**
   Address required libc++/libc/runtime imports only after full C# generation.
   Preserve cross-platform BCL/runtime platform selection, NativeAOT support and
   original source correspondence. Validate DuckDB behavior against native tests.

## Coordination and acceptance

The previous supervisor and its two workers were interrupted at the user's
request and must not be resumed. A fresh supervisor owns two fresh workers:
native typed capture and managed object/link/generation integration. Assign
disjoint files or explicit seams. Supervisor alone schedules canonical compiler
preparation/builds and Git operations. Root reviews progress and architecture.

Prioritize complete real-program capture and end-to-end generation, not numbers
of invented semantic cases or schema profiles. Batch related changes; use
focused tests per edit and broader regression at substantial integration points.
Commit verified significant milestones with exact owned paths. Preserve all
intermediates and unrelated working-tree edits.

Use bounded per-TU compilation and disk-backed linking. At handoff persistent
storage has about 29 GiB free and workspace about 84 GiB; refresh before large
runs, honor existing reserve floors, and retain all previous objects. Source
compression and sequential verification already work and should be reused.

Report independently: successfully serialized units, integrity-checked objects,
linked definitions, generated C#, compiled C#, and executed tests. Historical
2,634/2,635 exports across versions are not a coherent full-corpus result.
The user authorized continuous implementation, not a stop after this plan or
after the next small milestone.

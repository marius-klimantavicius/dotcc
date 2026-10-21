# Unicode additive integration handoff

Implementation owner `/root/manual_edg_supervisor/implementation_m3` integrated
the held Unicode contribution against accepted main storage checkpoint
`44640803b`, rather than replacing semantic files from its old `3b667ce73` base.
No builds, tests, native runs or Git operations were performed by this agent.
This is a continuing-campaign checkpoint, not a full M1–M6 completion claim.

## Imported bytes and provenance

The isolated source handoff is retained unchanged as
`evidence/unicode-implementation-handoff.md`, SHA-256
`774440b24afaeaaba25db2c6909215435c766f7131119f62913ddc60f31098dc`.
It records the exact EDG routines/source hashes, pinned Unicode policy,
algorithm choices, native qualifications, U1/U2 fixes and current bounds.

The official declarative Unicode17 files are copied without modification:

- `data/unicode/17.0.0/DerivedCoreProperties.txt`, SHA-256
  `24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08`.
- `data/unicode/17.0.0/LICENSE.txt`, SHA-256
  `e7a93b009565cfce55919a381437ac4db883e9da2126fa28b91d12732bc53d96`.
- `data/unicode/17.0.0/PROVENANCE.md` preserves official URLs, dates,
  attribution, exclusions and the source-data comparison qualification.

`UnicodeIdentifiers.cs` is the authored resource reader/scalar scanner/UCN
identity/UTF-8 encoder. `EdgSharp.csproj` embeds the unchanged data under its
fixed resource name. No generated frontend or generated C# table is involved.

## Path and conflict mapping

After inspecting current-versus-isolated diffs, these lexical/input/PP modules
were imported byte-identically; their differences from main were exclusively
the reviewed Unicode contribution:

- `Source.cs`, `Tokens.cs`, `CharacterLiterals.cs`, `UnicodeIdentifiers.cs`.
- `CompilationSession.cs`, `Preprocessing.cs`, `PreprocessingDirectives.cs`,
  `PreprocessingExpressions.cs`, `MacroExpansion.cs`, `FileSystemIncludes.cs`.

They retain the original raw token spelling and physical origins independently
of canonical scalar identity and effective replacement-token spelling. U1
pp-number/suffix diagnostic metadata survives token copies and active/skip
boundaries. U2 replacement literal identifiers use force-UCN spelling while
actual arguments retain their authored spelling. Encoding metadata stays
per-physical-source through logical/presentation buffers, providers and caches.

Central semantic files were merged manually with only these Unicode hunks:

- `Parser.cs`: eight identifier lookup/entity naming substitutions.
- `Expressions.cs`: canonical lookup and unresolved-function naming, two lines.
- `Templates.cs`: parameter identity, own-name shadow check and main check.
- `SemanticGraph.cs`: canonical public Scope query and Symbol.Name documentation.

ReferenceType, XValue, reference return/binding/load behavior, recursive scoped
signature cache guards, ErrorCount, replay/rollback and logical operators remain
from accepted main. ConstantEvaluation.cs, ConstantStorage.cs, References.cs,
Types.cs and OverloadResolution.cs were not edited during this integration.
This small identity-only merge is also the contract for the isolated future
assignment/control-flow contribution based at44640803b.

`UnicodeTests.cs` and five original Unicode fixtures are imported. Ten
registrations were added to the current Program.cs without removing the
accepted102 groups or the filesystem child CLI hook. CharacterTests.cs changes
only obsolete UCN/direct-Unicode Unsupported assertions to actual implemented
values/strict errors. PreprocessingTests.cs replaces only the obsolete UCN
character-expression Unsupported control. Floating/string/UDL and other genuine
Unsupported boundaries remain. Expected main group count is112.

## New Unicode + reference/storage gate

The existing UnicodeSemanticGate group additionally calls authored
UnicodeReferenceStorageGate in `UnicodeStorageTests.cs`; it adds no duplicate
registration. Exact source files are:

- `fixtures/m1-unicode-storage-gate.h` and `.cpp`.
- `fixtures/m1-unicode-storage-invalid-read.cpp`.
- `fixtures/m1-unicode-storage-native-witness.cpp` includes the unchanged gate
  and adds native-only result/reference/late-void type witnesses.

The gate combines included Unicode macros/aliases and supplementary metadata,
canonical direct/UCN queries, retained raw versus effective replacement spelling,
parameter identity, nested body replay/deduction, distinct/reused char16_t/long
instances, frozen earlier const-reference candidates despite a later void
nonconst-reference overload and caller-local hiding, actual local/reference
storage, return reference transport and required execution. Short circuit skips
a forbidden global load; address-only global transport succeeds with no scalar
value; the paired negative executes that load and must fail NotConstant.
Graph assertions run after session disposal and a second analysis, inspecting
selected identities/conversions, canonical types, object identity/liveness,
include/macro provenance and physical source encoding without provider I/O.

These are authored assertions awaiting supervisor-owned execution and produced
code review. Isolated89/89 receipts do not prove integrated112 behavior.

## Required work after this checkpoint

Full M1.3 string/raw/prefixed values and concatenation remain required, as do
floating/UDL values and remaining encoding/dialect branches. The concrete next
primary feature should be real target string code units and concatenation,
with honest literal array/category/storage/type dependencies; a placeholder
integer fold is not string support. The separate isolated assignment/control
flow worker owns that distinct next semantic contribution. All remaining
inventory rows and final full-plan audit remain active.

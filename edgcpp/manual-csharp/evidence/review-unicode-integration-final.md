# Independent final review — bounded Unicode integration

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21.
**Accepted within the produced and executed Unicode identifier/source/character
bounds, integrated with the accepted reference/storage frontend.** No known
unresolved critical/high integration finding remains. This is not full M1,
Unicode/string/dialect compatibility or full PLAN completion; work continues.

The reviewer read actual main source/assertions/fixtures, both isolated U1/U2
reviews, the repaired authoring handoff, upstream scanner/macro branches and
executed main evidence. No reviewer builds/tests, native/compiler execution,
implementation edits or Git operations occurred. Only authorized review reports
were written.

## Independent main evidence audit

- `unicode-integrated-build-round1.json`: Release, zero warnings/errors.
- `unicode-integrated-tests-round1.json`: direct dotnet-hosted DLL,
  **112/112 groups, zero failures/skips**.
- `unicode-integrated-nativeaot-publish-round1.json` and
  `unicode-integrated-nativeaot-tests-round1.json`: Linux-x64 publish/run
  succeed, **112/112 groups, zero failures/skips**.
- All four **111-file maps are identical and match current held bytes**,
  including all three declarative Unicode data/license/provenance files and
  three explicit oracle/receipt tools. The map in
  `unicode-integrated-held-snapshot.json` is identical. All three artifact
  sizes/SHA256s independently match actual DLL/AOT bytes.
- All14 consulted source/document SHA256s in `source-unicode.json` match.
  The repaired isolated handoff SHA is
  `774440b24afaeaaba25db2c6909215435c766f7131119f62913ddc60f31098dc`;
  its27 non-self authored hashes match. Isolated89/89 evidence is explicitly
  separate provenance and is not used in place of the main112-group gate.
- Six exact `native-unicode-integrated-*-round1.json` receipts match
  expectations and current source/additional-input/compiler hashes:
  **four accept0 and two reject2**. They cover the original included gate,
  actual character values, documented negative timing, included new storage
  gate, its included type witness and forbidden-load complement. All use the
  distinct `--unicode_source_kind UTF-8` argument pair with strict C++20 and
  instantiate-used. Tool/catastrophe status is not a language rejection.

The Unicode resource is the unmodified1,134,783-byte Unicode17 file, SHA256
`24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08`;
license/provenance hashes also match their records. NativeAOT's executed Unicode
boundary/semantic groups exercise the fixed embedded-resource path. Prior
declarative equality with EDG's UAX44 flags is data evidence, not broad runtime
or whole-file coverage.

## U1/U2 integration closure

The original high findings remain canonically **U1 and U2**, closed by the
repaired isolated review and independently confirmed in the actual main merge:

- **U1:** scanner syntax/scalar problems in pp-number parts and literal suffixes
  survive immutable Token/PpToken copies, paste validation and presentation.
  Active source/conditional/expansion boundaries diagnose them before DROP/STR
  can erase the argument. Skipped source and lazy literal contents retain their
  distinct policy. Imported malformed/dropped/stringized/raw-suffix/skip/lazy
  assertions execute in both main runs. The three discriminating native dropped
  pp-number/character-suffix/string-suffix receipts were independently rechecked:
  each rejects2 and matches its retained input hash.
- **U2:** effective literal replacement identifiers use lowercase force-UCN
  spelling; direct/substituted argument spellings remain context-specific.
  Raw replacement text, physical ranges and origin contributors stay separate.
  All nine nested contexts, formal-parameter/supplementary/redefinition/paste
  controls and raw contributor assertions execute in main. The replacement,
  context and nested ASCII-data native receipts accept0 and match their input
  hashes; observable stringization data cannot be replaced by provenance alone.

No new production defect arose during integration. An earlier in-flight read of
the new header omitted its final literal alias macro; rereading the held header
and matching executed map resolves that provisional test observation. The held
`#define ΗΧΩ ηχώ` supplies precisely the replacement branch needed by the
canonical-spelling assertion. It is not a failed test or a production repair
after the held run.

## Preserved semantics and actual coupled proof

Types, OverloadResolution, References, ConstantEvaluation, ConstantStorage,
ParseState and both accepted reference test files retain their previous
checkpoint hashes. Central parser/expression/template/graph changes are additive
canonical-name/query substitutions; ReferenceType/XValue, recursive scoped
cache guards, ErrorCount validity checks, lookup snapshots and replay/rollback
remain intact. All102 preceding groups execute beside ten Unicode groups.

Canonical scalar names identify declarations, aliases, templates, macro names/
parameters/suppression and public Scope queries. Direct and UCN spellings share
identity without Unicode normalization. Raw spelling and UTF-16 physical offsets
remain queryable, including supplementary/spliced contributors. The static
identifier tables retain only private ranges, not session/TU graphs.

The new included source connects Unicode macro and alias names, literal
replacement UCN spelling, char16/long template deduction and reused instances,
frozen earlier const-reference overloads, later void overload/caller hiding,
real reference storage and required constexpr values. After a second analysis
and disposal, assertions inspect canonical/raw names, exact selected entities,
two-candidate frozen sets, one replay per instance, type/rank, same-cell alias
identity, expired callee objects, macro/include origins and physical encoding.
Logical skipping creates no global storage; address-only mutable-global use
retains an unreadable static location; the paired real load fails NotConstant.
The native witness includes unchanged gate/header bytes and discriminates
char16/long result types, const-reference return types and later void selection.

Encoding metadata is immutable and per physical source. Managed strings do not
invent a byte BOM; strict UTF-8/BOM UTF-16 reads retain original metadata, while
probe/query paths do not decode or activate sources. Character conversion keeps
Unicode scalars distinct from numeric code units and preserves target type/
width/signed packing and PP promotions. Metadata, provider, independent-session
and retained graph controls execute under JIT and NativeAOT.

## Explicit remaining bounds and documentation

Full string/raw/prefixed/adjacent conversion and concatenation, floating/UDL,
legacy/native source/execution encodings, named/delimited escape profiles,
older dialect tables, complete diagnostic/recovery/security/header compatibility
and broader targets remain required. The strict ordinary nonraw native string
width rejection does not establish an ASCII execution charset. Historical
disproved timing/string expectations, mixed macro proposal and mistaken isolated
timing expectation remain retained/excluded. All broader reference/class/
template/constexpr/control-flow/lifetime and M5/M6 rows remain active.

Before committing, reconcile SUPPORT/UNICODE/feature/source notes to the accepted
main112/111 evidence, distinguishing historical isolated89/66 and accepted
storage102/96 snapshots. Replace superseded blanket Unicode-Unsupported wording
with these bounds; do not mark full M1 complete. That documentation follow-through
does not require another implementation cycle or a campaign stop.

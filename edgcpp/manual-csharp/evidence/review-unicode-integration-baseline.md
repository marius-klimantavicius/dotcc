# Independent Unicode integration baseline

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Current main base is
accepted scalar-storage checkpoint `44640803b`. This review is read-only except
for its own reports: no builds/tests, compiler/native runs or Git operations.
No integration acceptance is given before actual held main evidence.

The prior storage documentation reconciliation was read: current102/96 and
21 native runs are consistent; REFERENCES deliberately preserves historical95/73
and links to CONSTANT-STORAGE. Remaining general lifetime/constexpr branches
are still explicit.

## Reviewed authoring/source contracts

The complete isolated U1/U2 round1 report and repaired handoff were read. The
handoff SHA is `774440b24afaeaaba25db2c6909215435c766f7131119f62913ddc60f31098dc`;
all27 authored non-self fingerprints independently match its worktree. Its
89-group evidence is isolated and cannot establish main integration success.

Actual EDG branches inspected include lexical.c f_is_identifier_char10155ff,
is_valid_UCN_identifier_char13157ff, scan_universal_character13308ff and
make_canonical_identifier13739ff; macro.c ordinary replacement storage9911ff
and stringization3384ff. They require separate scalar identity, context-specific
effective spelling and retained raw source. Number/suffix UCN diagnostics have
different timing from lazy literal-content conversion. Skipped PP groups
suppress those diagnostics; a later discarded/stringized active argument does
not erase errors encountered while initially scanning it.

## Actual initial main merge observations

- UnicodeIdentifiers/Tokens/Source/MacroExpansion/Preprocessing/directives/
  expressions/CharacterLiterals/FileSystemIncludes currently match the repaired
  isolated bytes. This is a byte audit, not behavioral acceptance.
- Types, OverloadResolution, References, ConstantEvaluation, ConstantStorage,
  ParseState and both reference test files still match the accepted102-group
  snapshot hashes. Main retains ReferenceType/XValue, recursive cache guards,
  scalar storage/loads/cleanup/logical semantics and owner rollback contracts.
- Actual parser/expression/template name sites use IdentifierText while raw
  spellings remain for source diagnostics/literals. Templates still use
  DiagnosticBag.ErrorCount for validity/replay outcomes.
- U1's ScannedUcnDiagnostic is retained on number/suffix tokens and PpToken
  copies, diagnosed at active boundaries, carried through materialization and
  checked after paste. Literal contents stay outside that scanner path.
- U2's literal replacement identifiers get canonical force-UCN effective text;
  parameter actuals retain effective argument spelling. Raw replacement text,
  ranges and contributors remain distinct; nested stringization tests inspect
  their actual textual differences and origins.

## Required integration gate

The ten imported Unicode groups must execute beside all102 accepted groups,
with a source-text coupled Unicode/macro/include/template/reference/overload/
storage/constexpr witness. Verify canonical aliases, redeclarations, template
parameter and instance names, frozen lookup/caller hiding, query spellings,
raw contributors and retained origins after disposal. Exercise U1/U2 exact
controls, lazy contents/skipped groups, source policy/encoding metadata and
file-provider no-query-I/O behavior. Explicit UTF-8 native profile is necessary;
legacy-none and disproved timing/string proposals remain separate evidence.

Final acceptance requires actual produced main code/assertions and matching
held Release/JIT/NativeAOT/source/data/tool/artifact/native records. Isolated
data equality or isolated89/89 execution does not substitute for that gate.
Unicode17 XID/UCN/scalar character bounds do not complete full strings, floating/
UDL, alternate encodings/dialects, M1 or the continuing full PLAN.

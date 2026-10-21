# Independent final review — bounded scalar references

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21.
**Accepted for the implemented and executed scalar-reference bounds.** No
known unresolved critical/high finding remains. Complete M3/reference/template/
constexpr/lifetime support and the full campaign remain unfinished; the PLAN
continues immediately. The isolated Unicode work is not accepted here.

The reviewer read the isolated handoff, actual integrated source/tests, EDG
routine branches and executed evidence. No reviewer source edits, builds/tests,
native/compiler execution or Git operations were performed. Main integration
preserves the newer M1 character, diagnostic, preprocessing and input APIs.

## Independent evidence audit

- `references-build-round2.json`: Release succeeds with zero warnings/errors.
- `references-tests-round2.json`: direct dotnet-hosted DLL, **95/95 groups,
  zero failures/skips**.
- `references-nativeaot-publish-round2.json` and
  `references-nativeaot-tests-round2.json`: Linux x64 publication/run succeed;
  **95/95 groups, zero failures/skips**.
- All four **73-file maps are identical** and independently match held disk
  bytes. They explicitly include oracle.py, test-oracle.py and record-check.py.
  All three artifact sizes/SHA256s in `references-built-artifacts.json` match.
  All seven consulted source hashes in `source-references.json` match upstream.
- All15 native receipts have matching retained source/additional-input hashes:
  eight accept0 and seven reject with language status2. Included witnesses use
  distinct result types/decltype/signatures to establish scalar selection,
  reference categories, alias collapse and character-reference facts. The exact
  shared early-int/later-void fixture makes used-instantiation acceptance a
  discriminator for definition-context body lookup.
- Ten adapter controls pass. Their three tool fingerprints match the final
  snapshot. The optional Unicode option preserves omitted versus explicit none
  argv/profile state; actual Unicode semantics receive no credit at this gate.

Round1's one CS8602 nullable test warning is retained. Only its test assertion
flow was corrected before the final zero-warning snapshot; it was not a failed
semantic test or production repair.

## Reviewed behavior and finding closure

Canonical **REF-01 (medium)** is closed: origin lookup reuses completed immutable
binding facts instead of recursively retraversing alias chains. Self/uncompleted
references remain Unknown. Executed512-link local/global chains retain the
original temporary identity and owner, do not extend its lifetime, and preserve
the bounded local-return warning.

Actual graph assertions establish distinct canonical lvalue/rvalue reference
types over qualified integer referents, alias collapse, compatible entity
identity and separate binding/materialization descriptors. Related binding
checks cv/category before considering conversions; direct binding/return does
not load. Named rvalue references remain lvalues; reference-return calls retain
declared reference identity while exposing lvalue/xvalue referent categories.
Value uses explicitly load glvalues. Binding/materialization nodes fabricate
no integer ConstantValue.

Candidate comparison remains pairwise by argument, with rank before strict
reference-kind/cv ties and no declaration-order preference or summed scores.
By-value/reference equal matches and crossed matches stay ambiguous. Permutation,
invalid binding, earlier-candidate snapshots, hiding and actual template replay
controls execute. Character coupling proves char32 unsigned-int promotion versus
int conversion, exact char32 preference, char16 reference-return/alias identity,
wchar binding and char8 materialization payload/macro origin after disposal.

Reference/function cache ownership uses the recursive dependency walk through
all current compound return/parameter positions. Valid, invalid and unsupported
source WeakReference tests keep session/type table alive while dropped graphs
are collected. Replay rollback and retained earlier candidate/binding facts
execute. Future compound types must extend the ownership walk.

## Explicit adaptations and remaining bounds

Binding-expression type/category describe the referent, with declared identity
retained separately in Binding.ReferenceType. Temporary origin/lifetime-owner
facts are bounded query facts, not a complete cleanup or dangling-use engine;
Unknown never means safe and rebinding does not re-extend lifetime.

The native lifetime fixture emits three warnings for direct local/by-value
parameter/direct temporary returns. Managed KnownTemporary facts additionally
warn for two named temporary/rvalue-reference hazards while accepting the source.
This is an explicit diagnostic adaptation, **not native warning parity or an
invalid-binding classification**. Global temporary owners avoid that local
warning. Exact general diagnostic parity remains unqualified.

Valid dependent-reference syntax is native-accepted but managed Unsupported.
Native-only constexpr-reference assertions are future oracle facts, not managed
interpreter support. Required reference/object evaluation is explicitly
Unsupported; optional const-scalar folding does not invent a reference value.

Complete cv/declarators, pointer/array/function/class/dependent references,
forwarding/deduction, general conversions/ADL, reference constexpr execution,
full lifetime/cleanup semantics and remaining M1–M6 rows stay required.
Reconciled REFERENCES/SUPPORT/CORRESPONDENCE/FEATURE-INVENTORY preserve these
bounds. This is one accepted checkpoint, not campaign completion.

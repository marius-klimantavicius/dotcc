# String/array initial execution and finite repairs

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. No reviewer
implementation/test/tool edits, builds/tests, frontend/native execution or Git.
Read-only receipt and source inspection only; this is not final acceptance.

The initial clean Release build and JIT run (`strings-build-round2.json`,
`strings-tests-round2.json`) have identical118-file maps. Release reports zero
warnings/errors. JIT reports119/120, zero skipped; all112 prior groups and seven
new string groups pass. The sole failure is the test's direct SubscriptExpression
cast beneath a real two-step character load/promotion conversion chain. The
repair unwraps those explicit conversions and retains the subscript graph
assertions. Production conversion behavior is not changed for that test fix.
Historical failed build round1 records a C# local/pattern-name collision, not
semantic execution evidence.

## Additional canonical findings

| ID | Severity | Observation / required closure |
| --- | --- | --- |
| STR-13 | High | `MakeArray` initially added positive evaluator steps and array length as int, allowing overflow with valid int.MaxValue option values before allocating a huge cell array. Latest source widens the sum to long before comparison. The new required-evaluation control uses length/options int.MaxValue and must report ResourceLimit with zero materialized arrays. It must execute only after that guard repair. |
| STR-14 | Medium | Newly admitted pointer expressions outside the offset/logical subset (for example `p==p`) fall through the old integer-only InvalidSource diagnostic. Valid unsupported pointer operations must remain Unsupported until implemented; preserve actual invalid scalar diagnostics. Include the corresponding unary-plus-pointer boundary in this finite classification review. |
| STR-15 | Qualified / no held defect | The default-unreadable initializer observation came from an earlier editing read. Independent SHA verification confirms current ArrayInitialization dispatch already passes explicit true and ConstantEvaluation.cs is byte-identical in build round1, build round2 and test round2 (SHA344cfb7bcc60ef9a0a90db069ca7068c53b597200efcd34d5fac1291e56ae5df). No evaluator patch is required. Added initializer-result metadata assertions strengthen proof while named-global forbidden-load controls remain separate. |
| STR-16 | High, native-confirmed | EDG permits one-past subscript glvalue formation/discard and direct unnamed reference transport while rejecting real loads or later named-reference indirection. The discarded `constexpr int f(){"ab"[3];return 1;} static_assert(f()==1);`, direct ignore(subscript), and bare constexpr reference initializer accept0; required scalar load and later ignore(named-reference) reject2. Current managed Subscript rejects offset>=length immediately. Preserve a nondereferenceable address without inventing an element cell; public reference/binding facts need an explicit address-without-object variant. |
| STR-17 | High, native-confirmed | Convert already supports array/pointer-to-bool but MatchCandidate rejects those arguments for a bool parameter. Exact truth(bool) with a string and a competing-pointer-exact control accept natively. Match the bool conversion at Conversion rank, retaining decay/load flags, and prove that an exact pointer candidate beats the viable bool conversion. Otherwise this valid active branch must be explicitly Unsupported, not falsely InvalidSource. |

STR-10 is repaired by loading an index in its original integer kind. STR-11
allows representable type bounds independently from materialization budgets,
and classifies unrepresentable bounds ResourceLimit. A finite STR-11 follow-up
remains in the read round3 source: converting the bound to unsigned long before
checking positivity turns `char a[-1];` into a wrapped huge positive value and
misclassifies it ResourceLimit. Preserve the original signed value for the
negative-bound InvalidSource check before representation-limit classification.
STR-12 now derives named
global aggregate/pointer readability from declaration constexpr eligibility;
literal storage and invocation-local storage remain separate. The five native
const/constexpr/local eligibility controls have direct managed assertions, and
the ordinary const address snapshot remains readonly but unreadable with null
current values. These source fixes are inspected; final held execution remains
required.

STR-16 is backed by `native-string-contract-one-past-discard-round1.json` and
`native-string-contract-one-past-load-round1.json`, with exact retained main
inputs under tools/oracle-inputs/m1-string-contracts. EDG interpret.c31780–31814
carries the one-past address with CA_CANNOT_DEREFERENCE and checks later loads
separately. Subsequent split controls show reference_to transports that address,
while ref_indirect rejects it (interpret.c27321–27358). A blanket prohibition on
every binding context is therefore unwarranted. These native results do not
prove a managed repair.

The exploratory expectation that converting an escaped local-array pointer to
bool would reject was disproved by native accept0; no additional finding is
justified. interpret.c25158 checks pointer truth without loading its pointee,
whereas do_glvalue_to_prvalue25097 checks liveness and persistent pointer-result
eligibility is separate. Preserve this distinction in the implemented boundary.

The upcoming isolated control-flow merge must include string/array/subscript/
pointer expression children in its stack-safe dependency/effect traversal.
sizeof remains unevaluated. In particular, generic virtual fallback must not
reintroduce recursive walks for these newly introduced node kinds. This future
integration is not part of the current executed119/120 claim.

Final held code, all120 managed and NativeAOT groups, source/artifact maps,
exact included native witnesses and negative controls remain outstanding here.
No full string/array/pointer or campaign completion is approved.

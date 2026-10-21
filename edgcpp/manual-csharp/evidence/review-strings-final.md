# Independent final string/array integration review

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. **Accept the bounded
string/array/pointer-storage checkpoint represented by the final round4 held
bytes.** No unresolved concrete blocker remains within the implemented and
executed bounds below. This does not complete M1, general arrays/pointers or
the campaign; the full PLAN remains required.

The reviewer independently read actual EDG routines, produced implementation,
test assertions, fixtures and executed receipts. Only reviewer-owned Markdown
reports were written. No implementation/test/tool changes, builds/tests,
frontend/native execution or Git operations were performed by this reviewer.

## Verified final evidence

- `strings-{build,tests,nativeaot-publish,nativeaot-tests}-round4.json` all exit0
  in the main checkout and have **identical118-file maps matching current disk**.
  The maps include all three Unicode data/provenance files and all three tool
  sources. Release reports zero warnings/errors; direct-DLL JIT and Linux-x64
  NativeAOT each execute **120/120 groups, zero failed/skipped**. All112 accepted
  Unicode/reference/storage baseline groups remain in that run.
- `strings-held-snapshot.json` repeats the same map/hold. Its three artifacts
  match `strings-built-artifacts.json`; actual managed library, test DLL and
  NativeAOT executable SHA256 and byte counts were independently verified.
- `source-strings.json` records nine consulted upstream implementation files;
  all nine hashes match disk. This provenance covers the inspected branches,
  not a claim that the complete files are ported.
- Three `native-strings-integrated-*-round4.json` receipts match the unchanged
  coupled source/header and native-only type witness: two accept0, one language
  reject2. Source/additional-input and actual compiler hashes were independently
  checked. Commands explicitly use C++20 strict, `--instantiate used`, disabled
  predefined macro file and `--unicode_source_kind UTF-8`. Compiler SHA256 is
  `e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
- All32 finite `native-string-contract-*.json` copied source hashes match the
  retained main inputs. There are27 matched controls and five historical
  exclusions: two disproved expectations and three UTF8 numeric-concat native
  aborts (-6). Exclusions are not language-negative passes or support claims.
  The observed escaped-pointer-to-bool accept0 and one-past reference-use reject2
  remain useful explicit corrections to their original expectations.

Failed build round1 and119/120 round2 are retained historical evidence. The
round2 failure was a test cast beneath two real explicit conversions, repaired
without changing production conversion semantics. Round3 passed120 but preceded
the final reference-call and original-negative-bound refinements. Acceptance
uses only the coherent final round4 snapshot and executions.

## Canonical finding closure

| ID | Final disposition and inspected/executed proof |
| --- | --- |
| STR-01 | Closed within the qualified branch. Converted ordinary bytes widen with unsigned-byte fallback, preserve narrow numeric truncation before widening, combine valid UTF8 entities and never reinterpret escaped backslashes. Exact typed/unit/bound assertions match native controls. Invalid-byte ordinary-to-UTF8 concatenation remains explicit Unsupported UNS041 because the pinned native aborts; direct u8 numeric bytes are separate. |
| STR-02 | Closed. Unknown referents expose nullable IsReadOnly rather than invented object cv. Known allocated scalar/element readonly flags come from actual object types, not a const reference view; mutable local element/reference identity and readonly literal/global facts are inspected/asserted. |
| STR-03 | Closed. Raw decoded CRLF becomes LF, while retained token spelling and physical contributor ranges preserve CRLF. Native value control and actual graph assertions discriminate this. |
| STR-04 | Closed. Incomplete bound0 can infer a complete array without a factory exception; explicit zero, omitted-uninitialized, negative and too-short C++ arrays diagnose. Inference journals both declaration and symbol types. |
| STR-05 | Closed. sizeof(reference alias) uses referent size; actual unevaluated scalar/call/array/pointer/character-unit controls execute without object allocation or calls. |
| STR-06 | Closed. Dependent sizeof has an explicit value-dependent size_t node; replay substitutes real types. Unused template bodies needed only for sizeof remain NotInstantiated with zero body replays. |
| STR-07 | Closed. Array interning stops above EDG's80-character threshold; recursive wrapper guards avoid retaining large arrays or scoped dependent types. Structural Equivalent preserves compatible fresh array aliases/signatures. Long-array alias/parameter and live-owner weak-reference controls execute. Types carry no literal AST keys. |
| STR-08 | Closed. Array function returns diagnose; array parameters adjust to pointers preserving element const and retain ParameterDeclaredType. Compatible array/pointer redeclarations share the intended entity. |
| STR-09 | Closed. Reversed subscripts retain original complete range, SourceLeft/SourceRight and PointerOperandIsSecond; evaluator schedules original operand order. Explicit graph assertions execute. |
| STR-10 | Closed. A char16 index retains its original unpromoted kind after its explicit glvalue load, following expr.c2696. |
| STR-11 | Closed. Type-only large bounds are independent of literal/storage materialization budgets. The original signed bound is checked before representation limits, so direct/function-produced -1 is InvalidSource; positive unrepresentable bounds use ResourceLimit. sizeof(char[1000001]) succeeds without storage. |
| STR-12 | Closed. Named global const arrays/pointers are not made constant-readable merely by cv. Native/managed ordinary-const negative, constexpr positive and invocation-local positive controls execute together; address-only ordinary-global snapshots remain readonly but unreadable with no current element values. |
| STR-13 | Closed. Preallocation budget addition is widened to long. The int.MaxValue configured/bound regression reports ResourceLimit and zero materialized arrays before allocation. |
| STR-14 | Closed as an explicit support boundary. Valid unimplemented pointer comparisons/subtraction/unary-plus diagnose Unsupported; invalid pointer unary/arithmetic controls remain InvalidSource. General pointer operators are not claimed. |
| STR-15 | Qualified: no held implementation defect. The default-unreadable initializer observation was an earlier editing read. ConstantEvaluation.cs already passed explicit true in both initial held maps (SHA344cfb7b...); fresh initializer-result unit/readability assertions were added and execute. No evaluator repair was justified for this ID. |
| STR-16 | Closed with the actual native split. One-past formation/discard, direct unnamed reference transport and bare constexpr-reference initialization succeed; later named-reference indirection, scalar load and reference-returning call exposure reject. NonObjectLocation and separate public NonObjectReferenceValue/Bindings retain an address without fabricating an extra element/cell. Tests verify exactly three real string cells. Escaped-pointer truth versus forbidden persistent escaped-pointer results remain distinct. |
| STR-17 | Closed. Pointer/array-to-bool participates as a viable Conversion-rank candidate with explicit decay/load/pointer-bool descriptors. The sole bool call succeeds; an exact pointer candidate beats the viable bool conversion. |

STR-15 is not counted as a failed held implementation. The other IDs identify
concrete editing-source issues, some repaired before the first successful build;
their history is preserved in the baseline/round reports rather than rewritten
as claims that every validated snapshot failed.

## Difficult-path and ownership assessment

The shared changed-stream fixture exercises real included Unicode macros,
adjacent ordinary/UTF16 strings, retained source parts/origins, mutable local
array storage, reference element identity, by-value array-decay deduction,
three cached pointer-type instances and one actual body replay per instance.
Two helper candidates stay frozen despite a later better overload and caller
local hiding. Native type witnesses discriminate const char16_t array/reference
types and the later void overload; the negative complement forms a global
address successfully and rejects its forbidden element load. Assertions remain
usable after session disposal. This is executed integration, not disconnected
unit scaffolding or native acceptance substituted for graph proof.

Materialization checks its configured limits before allocation and charges
element work. Array cells share the established allocation/lifetime cleanup,
stable element identities and target signed conversion. Literal storage is
static; local arrays expire; reference aliases do not extend that array's
lifetime. Finally clears all new aggregate/literal/nonobject observation state.
Owner-mediated inferred-type rollback is tested across nested and replay-owner
checkpoints. Canonical source-independent small types may be reused; large
arrays use structural compatibility rather than universal identity.

The actual EDG correspondence includes literals.c conversion/rescan/concat,
lexical.c whole-sequence collection/raw line normalization, il.c bounded string
types, decl_inits.c character-array copy rules, expr.c unevaluated sizeof and
unpromoted/original-order subscript, exprutil.c explicit decay/reference
indirection, overload.c rank comparison and interpret.c storage/address/load
separation. Alternate profiles, conditionals and omitted branches remain
explicitly outside this acceptance.

Full floating/UDL/legacy literal branches, compound/dependent/multidimensional
arrays/references, general pointers/operators/object lifetime/classes, remaining
templates and all other open PLAN rows remain required. The separately authored
control-flow branch is not integrated or accepted by this report. Its eventual
merge must preserve actual readonly/BeganInEvaluation storage checks, aggregate
cleanup, nonobject address transport/indirection, and stack-safe dependency/
effect traversal for all new nodes; sizeof remains unevaluated. Continue the
campaign immediately through that reviewed integration and the remaining plan.

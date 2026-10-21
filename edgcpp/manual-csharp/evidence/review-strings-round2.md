# String/array produced-source review, round 2

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. This review reads
the actively edited main implementation after accepted Unicode `6c3848ca5`.
It is not acceptance of a held or executed string checkpoint. The reviewer ran
no builds/tests, frontend/native processes or Git operations and changed no
implementation, test, fixture or tool files.

## Canonical additional findings

| ID | Severity | Concrete source observation / closure requirement |
| --- | --- | --- |
| STR-07 | Medium | EDG `il.c:string_literal_type12779` caches only lengths at most `MAX_TRACKED_STRING_TYPE_LENGTH` (80), whereas the initial generic array dictionary retained every bound. Latest source now returns fresh large array types, recursively avoids caching wrappers that retain them or scoped types, and uses structural `Equivalent` for alias/function redeclarations. Verify small-type reuse, fresh structurally equivalent large arrays, wrapper nonretention and compatible long-array aliases/parameters. |
| STR-08 | High if retained | Adding array aliases initially allowed an invalid function returning an array; parameter adjustment also must preserve array element cv. Latest source diagnoses array results, adjusts array parameters to element pointers and retains `ParameterDeclaredType`. Execute invalid array-result and compatible const-element array-parameter/redeclaration assertions. |
| STR-09 | Medium | Reversed builtin subscript initially swapped operands before making its range and evaluating operands, losing source `1[` and original evaluation order. EDG `expr.c:2640–2731` swaps only checking pointers, stores a second-pointer flag and preserves original operands/range. Latest source retains source order/range and evaluator scheduling; the authored reversed-subscript graph assertion must execute. |
| STR-10 | Medium | `ScanSubscript` still calls `types.Promote` on the index. EDG `expr.c:2696` explicitly does not perform integral promotions on a builtin subscript. Preserve the index's unpromoted kind after its actual glvalue load; add a char/char16 graph discriminator in both operand orders. |
| STR-11 | Medium | `ScanArrayDeclarator` uses the literal code-unit limit as a type-bound limit and reports ARR001 InvalidSource. A valid `sizeof(char[1000001])` needs no array materialization. Separate representable type bounds from literal/storage budgets; oversized representation should be ResourceLimit, and actual huge required materialization must stop before allocating. |
| STR-12 | High, native-confirmed | `MakeArray` uses const element qualification to permit global array loads; global pointer slots likewise use top-level const. These facts establish readonly storage, not constexpr usability. Exact supervisor native controls reject required reads through an ordinary global const array or ordinary global const pointer, and accept constexpr counterparts plus an evaluation-local const array. Separate readonly from constant-readable; retain address formation while rejecting forbidden loads. Inspect ordinary const array snapshots as readonly=true, readable=false and value unavailable. |

All findings were sent promptly to the supervisor. STR-10–12 describe the read
source at the time of reporting and can be repaired before the first hold; they
are not claims of an executed managed failure.

## Current fixes and assertions inspected

STR-01 now uses unsigned-byte fallback for malformed converted ordinary bytes
when widening, while independently marking the unqualified ordinary-to-UTF8
malformed-byte branch Unsupported. STR-03 normalizes raw CRLF values to LF while
retaining physical raw spelling/origins. STR-04 admits the incomplete bound
sentinel, STR-05 sizes reference aliases and STR-06 keeps dependent sizeof as an
explicit dependent node. STR-02 exposes unknown readonly metadata as null.
These source repairs still require executed controls before closure.

The current `StringTests` includes target-unit/bound/category checks, whole
sequence concatenation and numeric rescanning, unevaluated sizeof and unforced
template bodies, local/global array storage and pointer transport, one-past
versus element reads, expired storage, explicit reference bindings, inferred
bound graph rollback under root/replay checkpoints, long-array alias/parameter
compatibility, scoped compound/TU collection and a changed-stream Unicode/
include/template/overload/reference/storage integration fixture. Tests are
authored but unexecuted at this review point.

The current `MakeArray` checks literal/storage limit and remaining evaluator
steps **before** allocating the cell array, charges its element count and then
allocates through the existing lifetime cleanup path. `Stop` throws the typed
evaluation stop exception. Mutable global array snapshots no longer expose a
plausible initialized value when their elements cannot be constant-read. These
are positive source observations, not a replacement for resource/error/reset
execution controls or the pending STR-11/12 repair.

Full112-group baseline preservation, final held code review, matching JIT/AOT
receipts/artifact maps and exact native value/type/negative witnesses remain
mandatory. Full string/array/pointer/declarator support and the remaining PLAN
are not accepted by this editing review.

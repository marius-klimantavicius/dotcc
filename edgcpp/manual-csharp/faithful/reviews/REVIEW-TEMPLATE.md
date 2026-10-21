# Faithful EDG routine/type review template

Copy this record for each routine overload, macro, inline/template
instantiation, representation type, global/table, or runtime shim. Preserve
source names and grouping. Filling a record is not acceptance. The independent
reviewer must open the exact native definition and actual managed counterpart.

## 1. Identity, authority and immutable inputs

- Review/task ID, date, author, implementer, independent reviewer:
- Native relative path, routine/macro/type including overload and signature:
- Whole-file SHA-256, inclusive source range, exact range SHA-256:
- Hash rule: SHA-256 of exact bytes; range = concatenated inclusive source lines
  retaining original terminators; no newline/encoding normalization:
- Referenced headers/macros/generated tables: paths, ranges and SHA-256:
- Managed relative path, containing type, signature, inclusive range:
- Managed whole-file and range SHA-256; every partial file/helper dependency:
- Original-to-managed blocks: one-to-one mappings, including relocated macros:
- Native build target/command and source-to-binary receipt, compiler/hash,
  options, generated-header/configuration manifest and executable SHA-256:
- Managed project/options/dependencies and exact held input manifest:
- Qualified host/ABI and target/dialect/runtime options; what is only inferred:
- Attribution/provenance retained:

Source grouping/name/splitting differences require a specific language necessity
and equivalence argument. A replacement Parser, graph, interpreter, generalized
numeric algorithm or semantic native fallback cannot satisfy correspondence.

## 2. Configuration alternatives

List every enclosing/nested #if/#elif/#else, including header macros, diagnostic
checking and library shims. For each give expression, definition chain and
hashes, resolved value for the named native build, matching managed selection,
and backlog entry for excluded alternatives. Distinguish compile-time exclusion
from runtime reachability and from an untested enabled branch.

| ID | Native selection expression/range | Definitions/hash | Native active? | Managed mapping | Evidence/backlog |
| --- | --- | --- | --- | --- | --- |
| C01 | | | | | |

Never disable an enabled branch to get a passing slice. Excluded alternatives
remain inventoried. Runtime strict C++20 does not remove compiled facilities.

## 3. Input/output, state and dependency contract

- Each parameter's native type/width/signedness, role, nullability/precondition,
  by-value/reference/pointer identity, mutation, lifetime and counterpart:
- Return, out/ref/pointer outputs, write timing/order and values on failures:
- Valid alias partitions (self/equal storage, shared referents/subobjects),
  evidence from callers, temporary binding, overlapping copies and traversal:
- Local copies versus aliases; original evaluation and call-order constraints:
- Global/thread/context/cache/table reads/writes, initialization and reset order:
- Ordered linked state, canonical identity and mutation visibility:
- All direct/transitive routines/macros/templates, allocation, cleanup,
  diagnostics and failure dependencies, each with ledger ID and status:

| Dependency | Exact original/managed snapshot | State/identity/error effects | Qualified configuration/status | Blocker |
| --- | --- | --- | --- | --- |
| | | | | |

A leaf's apparent similarity does not prove its dependencies' contracts. Do not
claim closure if a stub, Unsupported, unreviewed shim, native EDG call or
production compiler subprocess can be reached. Record intentionally unported
callers separately; qualification never propagates upward automatically.

## 4. Full operation and branch ledger

Every meaningful operation must occur in a mapped row/block. Include entry and
initialization, all predicate arms, each short-circuit operand and its order,
loop entry/body/update/exit and zero iterations, switch case/default/fallthrough,
break/continue/return, goto/label/retry, out writes and order, conditional reads,
allocation/free and failure, diagnostics/recovery, rollback/deferred work,
compile alternatives, and exceptional/platform-sensitive cases. Include implicit
casts/promotions, macro multiple evaluation and local-variable bindings.

| ID | Native file/range/block hash | Predicate/operation in source order | Reads, calls, writes, alias/error effects | Managed range/block hash | Adjustment/equivalence | Review verdict | Trace/test evidence or explicit untested |
| --- | --- | --- | --- | --- | --- | --- | --- |
| B001-entry | | | | | | | |
| B002-true | | | | | | | |
| B002-false | | | | | | | |

Do not group branches under “equivalent arithmetic” or record only happy-path
calls. A loop needs bounds, direction, intermediate widths, mutation order and
exit behavior. An untested enabled branch remains enabled and reviewable;
tests are separate evidence from source review. No partial routine is complete.

## 5. Representation, arithmetic and runtime/shim review

For types: original fields/order/discriminants/enumerators/union variants,
bitfields/masks, size/alignment/offset/stride/padding facts and their measured
or inferred status. Describe identity and copy/alias behavior, ownership,
allocation/free, stable addresses, pin duration, stack/arena/general lifetimes
and cleanup/reset. Explain each managed layout/accessor transformation.

For arithmetic: source operand/intermediate types, promotions, signedness,
width, truncation stages, shift domains, carry/borrow, rounding, overflow and
error output semantics. Match the original algorithm, not just a mathematical
value. Record exact native platform-sensitive assumptions; a native crash or
undefined operation is not a parity result. Do not silently fix native quirks.

For shims: quote the native API/storage/error contract, source mapping,
side effects/diagnostics, bytes/encoding/path conventions, allocation and
failure behavior; identify semantics that must stay in the original routines.
P/Invoke to EDG/libclang and production native-frontend subprocesses are barred.

## 6. Diagnostics, recovery and unresolved behavior

- IDs, severity, arguments/locations, order/suppression and state updates:
- Failure return/out values and partial mutation, retry and recovery path:
- Native assertion/internal-error/termination contract and dependency closure:
- Formatting/path-only differences explicitly bounded:
- Suspected native defects: exact reproducible source/configuration evidence,
  defined/implementation-defined/undefined status and unresolved supervisor work:
- Any intentional divergence requires explicit user authority and a separate
  label; it cannot count as exact parity. Existing prototype exceptions confer
  no authority. Do not create a blanket exception policy.

## 7. Validation evidence — supervisor only

Children do not build, test, prepare native artifacts or mutate Git. List planned
checks as **pending**; only attach actual supervisor receipts to executed checks.
Before validation, identify all shared writers and explicit HOLD receipts with
their exact hashes. Confirm these equal the source/C# snapshots reviewed here.

- Native/managed input/include bytes and options identical:
- Meaningful boundary and every available branch case:
- Alias partitions, value-copy independence and order-sensitive state traces:
- Stable logical IDs/edges and diagnostic order where applicable:
- Raw integer limbs, truncation/error flags, layout evidence where applicable:
- JIT and NativeAOT results on the held snapshot, commands, logs and hashes:
- Failed predictions/mismatches/crashes preserved with unresolved status:
- Normalization rules preserve identity/alias/order/lifetime distinctions:

Tests complement source review; finite cases are not universal proof. Do not
broaden qualification beyond the actual reviewed and executed configuration.

## 8. Findings, status and handoff

For every finding give ID/severity, exact native and managed range/hash,
observed mismatch or missing evidence, affected branches/callers, required
resolution, and verification. Record prior-round finding, implementer's response
and unresolved objection before closing it in a fresh independent round.

Choose a state (do not conflate these): unstarted; translated but
dependency-blocked; review-pending; reviewed with tests pending; parity-qualified
for the explicitly named configuration. An unexplained mismatch blocks all
acceptance. Report routine/branch totals only from complete records.

- Independent reviewer opened original and counterpart: yes/no, exact hashes:
- Qualified routines/branches and named configuration (or **none**):
- Verdict, concrete blockers and next review scope:
- Explicit **HOLD**, exact owned files and SHA-256, no outstanding writes:
- Any later changed source/configuration/managed dependency invalidates affected
  review/validation until re-reviewed and revalidated.

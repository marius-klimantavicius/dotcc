# Independent integer HOLD review, round 2

Date: 2026-10-21. Review ID: `faithful-integer-held-r2`. This is a fresh
independent produced-code review, following `REVIEW-TEMPLATE.md`; it does not
continue or adopt acceptance from the P0 contract reviewer. The current PLAN,
AGENTS and `coordination/RESUME-20261021.md` control. Explicit resume supersedes
historical STOP. Ownership is exclusively this file and `integer-held-r2.json`.
No implementation, upstream, old prototype/evidence, Git state, build output or
other writer's ledger was changed. No build, test, native preparation,
subdelegation or user question was performed by this reviewer. Hash/receipt
inspection below is read-only evidence auditing, not new execution evidence.

**Verdict: 18 routines are parity-qualified for their complete source-valid
contracts under P; 2 are restricted-domain reviewed and unrestricted-unqualified.**
The latter are `make_integer_value_mask` and `sign_extend_integer_value`: source
review and executed evidence cover widths 1..128, but source-defined out-of-D
cases have not received equivalence review. They are excluded from the qualified
routine total. The three macro counterparts have call-specific qualified
contracts, not unrestricted macro acceptance. The inline representation is
qualified under P. All enabled branches of the 20 bodies were inspected against
the actual original C/C++ and produced C#, including errors and alias effects;
no body mismatch was found within the stated domains. The r3 supervisor evidence
closes tests-pending within those domains, without closing missing domain review.
This is source review plus finite validation, not a formal universal proof or
completion of the integer subsystem, P0 inventory, initialization or frontend.
No partially qualified routine is counted as complete.

**Unrestricted mask/sign-extension caller domains and frontend initialization
remain UNQUALIFIED (INIT-001).** In particular, the enabled initializer's
65535/65536 widths are outside D. This restriction is recorded here, not enforced
by changing code or disabling a feature. Some out-of-D original calls can have
defined results; no equivalence is inferred for them. Every future caller needs
its own domain, binding, dependency and lifetime review. New batch files and
later changed bytes receive no acceptance from this report.

## 1. Identity, authority and immutable inputs

The reviewer opened the exact routine definitions in `upstream/src/const_ints.c`
and `.h`, representation/configuration definitions in `targ_def.h`, `basics.h`,
the actual three C# files, both full JSON and Markdown batch ledgers, the prior
P0 review, project options, ABI probe, test adapters and relevant test/gate source.
This is not a semantic audit of unrelated declarations in every included header.
Paths below are relative to `edgcpp/` unless stated otherwise.

| Exact file | SHA-256 |
| --- | --- |
| `upstream/src/const_ints.c` | `5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c` |
| `upstream/src/const_ints.h` | `23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f` |
| `upstream/src/targ_def.h` | `e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b` |
| `upstream/src/basics.h` | `65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9` |
| `manual-csharp/faithful/src/EdgFaithful/const_ints.cs` | `cd5132dba30a65020fc4de262b56a32e48189075a0b6c3a8cb3681777374ac98` |
| `manual-csharp/faithful/src/EdgFaithful/IntegerRepresentation.cs` | `4379a643ecfb4ac1e71f7ba07b5787b3fdc828a60e6c6ea8e94b4b1967ef5dd5` |
| `manual-csharp/faithful/src/EdgFaithful/const_ints.arithmetic.cs` | `dfd86a0dc1e39ce2f4796b19edc6b128ef7a65c963e23cd8e599450527e948c5` |
| `manual-csharp/faithful/ledgers/const_ints-batch1.json` | `bec971ee7aa97fc5d1fa85a78b2af126bc9d02933796291f8c3c9d828757de9f` |
| `manual-csharp/faithful/ledgers/const_ints-batch2.json` | `fa838d5f03cbdbdfd2793b3574c6fddea23b62fade8bdfd8cf84b9d1f884bcf0` |

`integer-held-r2.json` contains every routine, macro, type, constant and operation
range with full file/range hashes, signature/parameter contracts, dependencies,
individual reviewer verdicts and reasons. Source/managed range records were
independently recomputed from exact bytes: inclusive one-based lines, original
line terminators retained, no normalization. All 244 batch-1 and 269 batch-2
pinned file/range references matched. This hash audit authenticates the inputs;
the operation verdicts come from reading both implementations.

The supervisor's before/after manifests are identical, with 27 entries and
manifest digest `115a3cf01bae6d28e80f020abc7986beafe66b854f4befeae99743c8786c97ad`.
Each manifest file's own SHA-256 is
`2bcb27c3eb57df2ff861031698851398587766085d8349832e480521c3112bb3`.
All 27 preexisting entries still matched at this review's audit. This is an
entry-by-entry comparison, not a claim that the current project contains no new
files. All 155 native expanded
include-file hashes, saved gate-log hashes, listed immutable artifact hashes and
four differential streams' input/output hashes also matched their receipts.

The exact cpfe compile-database entry and full oracle commands are in
`evidence/integer-first-gate/integer-r3-native-prepare.json` (SHA-256
`9ffbadea5f65eca30393e290708d0b53e97c45ef6fa1e9d269bd3e430e697cd3`).
The original const_ints.c, not a copied arithmetic implementation, was compiled
by GCC 13.3.0, `x86_64-linux-gnu`, compiler SHA-256
`1353e9bdd29a7295c7226bf6c63abccce056d8cac31f112e5cdbecc3f28c2769`.
The selected command uses C++14, `-O3 -DNDEBUG -flto`,
`-fno-exceptions -fno-unwind-tables -fno-rtti`, `-DUSE_CMAKE_DEFINES`, and the
recorded cpfe definitions/include path. Test-only additions are function/data
sections, dead-section removal, wrapper main, wrapper quote-include search path,
and an isolated dynamic/default-linker executable. This is not a full static/gold
cpfe build or proof of frontend startup. No source rewrite or production native
fallback is involved. Oracle executable SHA-256:
`71560cfdc589f9b2e2beb3570031662c055cab1da8b03a5461bf20682c6bb038`.

Managed options are `net10.0`, C# 14, unsafe enabled, deterministic, overflow
checking disabled, `IsAotCompatible=true`; NativeAOT used Release/linux-x64,
self-contained and `PublishAot=true`. Qualification names the actual held JIT
assemblies and NativeAOT artifacts in the receipt, not every .NET 10 installation
or other host. Later runtime files outside the three reviewed production files
are explicitly unreviewed. The current project's inclusion of those files is a
new integration snapshot; this verdict covers only the saved r3 artifacts and
held three-file implementation closure. No frontend source dialect was exercised: strict C++20 is an
eventual runtime frontend profile, not an effect of compiling these leaves.
EDG source attribution and Apache-2.0 WITH LLVM-exception notices are retained.

## 2. Configuration alternatives and named profile P

P is the measured Linux x86-64 little-endian LP64 host and selected cpfe
compile-time configuration, with array representation, eight unsigned-short
limbs, 128 represented bits and native C++ bool. The effective macro dump has
SHA-256 `87362eb346c8676da4ae464243efcc7a3c00a0c61fdc540b47b336261a50d310`.
The JSON includes the original guarded ranges/definition hashes from both batch
ledgers, with an independent selection verdict on each record.

| Selection and original definition chain | Reviewed active mapping | Excluded alternatives/backlog |
| --- | --- | --- |
| `USE_CMAKE_DEFINES`; defines.h -> generated cmake_defines.h | Exactly the cpfe command/header, without mixing other targets | Other defines.h profiles require new review |
| `INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0` | Active array bodies/macros, including source c:32/633/852/1108 guards and per-body alternatives | Host-scalar bodies/types/macros and nested `_lint` conversion alternatives remain unported |
| `USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1`; basics.h:213-217 | bool and bool pointers; TRUE/FALSE preserve truth values | int a_boolean / byte a_byte_boolean alternatives excluded |
| `__ANSIC__=1`, `EDG_MSDOS=0`; basics.h:528-600 | size_t/ptrdiff_t, int/uint loop indices as reviewed | DOS wider sizeof_t/checking size_t_arg and non-ANSI branches excluded |
| host byte=8, short=2, int=4, size_t/ptrdiff_t/pointer=8 | ushort/int/uint/long/ulong/nuint mappings under P | LLP64, 32-bit, other widths/byte order not qualified |
| `LONG_LONG_ALLOWED=1`, `INT128_EXTENSIONS_ALLOWED=1`; targ_def.h:1172-1268 | Integer maximum literal 16, eight limbs | Smaller maxima, scalar int128 and absent long-long branches excluded; source #error checks remain configuration obligations |
| `FIXED_POINT_ALLOWED=1`; targ_def.h:1283-1462,1661-1667 | At representation definition, long accum defaults=8 and long fract=4; largest fixed point=8; 8>16 false | Disabled fixed point, overrides and larger fixed-point replacement not qualified |
| `TARG_CHAR_BIT=8`; targ_def.h:1633-1655 | INTERNAL_TARG_CHAR_BIT=8 | 16/24/32 and invalid-value #error alternatives excluded |
| `CHECKING=1`, `DEBUG=0` | No assertion/diagnostic edge in these bodies or their reviewed closure | CHECK-001 remains open for later checking/error/runtime dependencies; NDEBUG does not disable EDG checking |

The managed `MAX_SIZEOF_LARGEST_FIXED_POINT=8` is source-derived configuration
metadata, not a native measured ABI field. Original targ_def.h:1433-1455 derives
8 before choosing representation; target.h:1512/1518 later undefines constituent
size macros. Attempting to evaluate the surviving macro as a wrapper expression
then fails. The r3 gate correctly removed that invalid native measurement and
does not credit matching managed metadata as ABI evidence. The actual integer
layout and count were independently measured. This distinction is required if
later code starts using this metadata in a new semantic context.

## 3. Input/output, state, dependency and caller contract D

D requires live correctly aligned typed storage for all actual accesses and
native-valid bool values/outputs; objects from movable managed storage must be
pinned for the full alias lifetime. Inputs read by a routine are initialized;
pure setter/mask outputs need not have an old value. Null, expired, partially
overlapping or ill-typed output storage is not an accepted domain here. No new
invalid-pointer check or managed exception is introduced. Exact integer self
alias and disjoint integer operands, including both address orders where
relevant, are accepted. Bool/host outputs are distinct valid typed storage.

All 128-bit input limb patterns and all bool flag combinations are included
where the signatures admit them. Signed/unsigned scalar setters accept the full
P host long/ulong domain. Direct integer shift counts are **0..127**, the legal
count caller precondition for a 128-bit represented integer (source c:927-928
and c:986-987), not every numerically representable int count. Mask and
sign-extension widths are **1..128**; consequently size_t subtraction and int
conversion yield legal 0..127 counts. The shift bound names the source's
legal-count precondition. The mask/extension width bounds are narrower review
scope: mask's explicit source precondition is only bits>=1, and neither size_t
API has had every source-defined out-of-D case reviewed. These are not inserted
guards or a claim that all other native calls are undefined. Those two routines
remain unrestricted-unqualified, including enabled initializer calls. Exact
per-routine verdicts are below.

Native references in the two aggregate setters bind existing objects. The C#
pointer counterpart preserves that object and forward read/store sequence, with
no hidden value snapshot. Future callers must pass the original lvalue's address
and preserve native temporary call lifetimes. The opened transport passes its
live `other` object pointer; scalar setters first form the host scalar and
select the appropriate long/ulong overload. No production frontend caller of
these reference overloads is accepted by this review.

Native struct-copy locals (`orig_value`, `result`) remain independent inline
value copies. Local `one` and bool errors have nested-call lifetimes and do not
escape. No routine allocates, frees, pins, retains pointers, interns values or
touches globals, caches, TLS, target tables or semantic graphs. Only the specified
caller integer and output storage are changed. No diagnostic, recovery or
allocator stub is reachable within D. Caller-owned lifetime and mutation
visibility are unchanged; convenience managed owners or a replacement graph
were not introduced.

| Dependency closure, in source call order | Independent verdict under P/D |
| --- | --- |
| representation/constants -> sign_of, signed_shift_right, get_part | Reviewed below; no hidden semantic/runtime callee |
| signed setter -> signed_shift_right(value,16) | Pure local value and constant; macro repetition adaptation equivalent |
| conversion / comparison -> sign_of | Original evaluation point and short circuit retained |
| increment -> signed setter(one,1), unsigned add | Complete combined batch closure reviewed; local err discarded |
| add/subtract -> initial sign_of(op1), limb loop, late op2 and conditional op1 sign reads | Both alias-sensitive observations remain after mutation |
| mixed add -> selected original copy, add, optional cmp | Base error then optional overwrite preserved |
| mixed subtract -> original op1 copy, subtract, optional late op2 sign then cmp | Equality predicate and post-write alias sign preserved |
| mask -> signed setter(0), complement, logical right shift | All three calls and stages preserved |
| sign extension -> left shift/local err, signed extending right shift | Ignored local error and same value/count retained |
| shifts -> guarded get_part reads, sign_of only for right fill | Live pointers, widened shifts, traversal order and store point preserved |
| negate -> distinct local zero, signed subtract, whole result copy | Caller err written during subtract before final op1 copy |

Other native callers (initialization, type/constant/range operations,
multiply/divide, formatting and frontend facilities) remain outside this closure.
Qualification never propagates upward, sideways to new files, or to a new macro
call expression automatically.

## 4. Full operation and branch verdicts

The appended operation tables are the independent review, not a hash-presence
check. Every meaningful row in both full ledgers receives an explicit verdict
and reason against the actual opened original/counterpart. Exact native and
managed whole-file/range SHA-256 values accompany each row in the paired JSON.
The tables use C for const_ints.c, H for const_ints.h, L for const_ints.cs, A for
const_ints.arithmetic.cs and R for IntegerRepresentation.cs. `Equivalent` always
means P/D and the macro domain, never an unrestricted caller assertion.

Loop initializers, true/false entry, update, terminal exit and zero-count calls
were reviewed. The fixed-eight loops cannot have a zero-iteration initial entry
under P; their false exit is nevertheless mapped. Conditional/short-circuit
branches include skipped reads, ternary selection and error overwrite timing.
There are no labels/gotos, allocation failures, diagnostics or recovery steps
inside these selected bodies to map. Inactive native scalar alternatives remain
explicitly excluded with their source ranges and operation inventory in JSON.
No branch is called dynamically covered merely because a test invokes its method.

## 5. Representation, arithmetic and runtime review

Native an_integer_value is an inline unsigned-short[8] struct (targ_def.h:
1673-1675). C# fixed ushort[8], sequential Pack=2 Size=16, preserves its size,
field offset, stride, inline copy independence and alias identity. Native header
probe measured size/alignment 16/2, limb offset/stride 0/2, short=2, int=4,
size_t/ptrdiff_t/pointer=8, bool/byte-bool=1, little endian and promotion of a
limb to int. Managed tests check inline layout, copies and guard bytes under
both JIT and NativeAOT. Pack is an explicit P contract, not a universal layout.
Part 0 is most significant regardless of byte order inside each ushort.

Native a_host_large_integer=a_ptrdiff=ptrdiff_t and unsigned=sizeof_t=size_t
map to long/ulong only under P. `sizeof_t=ulong` in the leaf file and `size_t`
as nuint in mask/extension have equal width/range under P. Native sizeof-based
constants often have size_t type; managed int constants are value-equivalent at
each inspected use (loop bounds, exact narrowing, shifts and width arithmetic).
They are not a blanket substitute for size_t in future code. INT_VALUE_PART_BASE
preserves unsigned-host width and value 65536 before the source's signed cast.

Signed setter masks, stores, then shifts; unsigned setter's masked ulong-to-long
assignment is safe because it is 0..65535. Conversion assembles four retained
limbs as unsigned64, never exits early for discarded-bit errors, applies the
last signed-sign check, and stores low host bits before bool error. GCC's pinned
unsigned-to-signed conversion behavior matches unchecked bit reinterpretation;
no portability outside that host/compiler is claimed. Limb bitwise/complement
operations preserve native int promotion before narrowing, including ~ followed
by &0xffff.

Add work is 0..131071; subtract work is -65536..65535. After one base adjustment
it is 0..65535, so no signed64 overflow is needed or hidden by unchecked casts.
Carry/borrow stages, cached first sign and all late signs match exactly. In
particular signed self-add across the sign boundary can report false error and
negative self-subtraction can report true error; these native quirks survive.
Mixed subtraction's nonnegative-second predicate remains cmp<=0, including
error at equality. No mathematical correction was made.

Within D, shift division/remainder are bounded int operations; component counts
are 0..15 and 1..16, always legal for widened unsigned64 work. Left traverses
ascending indices, right descending, preserving unread source limbs in place.
Both guarded reads precede each store. Native relative evaluation order of the
two pure reads is unspecified; C# fixing an order has no observable difference
in this closure. Overflow remains sticky and is written only after left's loop.
Right fill is computed before mutation from short-circuited flags and original
sign. Bool-to-int conversion is explicitly 0/1 before native-style negation/mask.
CLR masked shift counts supply no semantics outside D.

Macro domains are separate: sign_of reads the live part[0] at each source
evaluation point; signed_shift_right is qualified for pure signed64 value and
pure count 0..63, with the only enabled production call here using 16; get_part
is qualified for the actual pure bounded int index expressions and stable
ushort fill locals in the two reviewed shifts. Negative then upper-bound checks
guard dereference, and selected ushort is widened before shifts. Native macro
argument repetition/captured-local binding is equivalent for these callers,
not for hypothetical side-effecting or volatile expressions. Out-of-range
get_part indices needed by these shifts are included; invalid integer shift
counts are not.

## 6. Diagnostics, recovery, prior findings and unresolved behavior

There is no assertion, diagnostic, termination, allocation or recovery dependency
on a P/D path in this closure. Error bools are behavior: add/subtract overwrite
them after all limbs, mixed operations may overwrite them a second time,
conversion stores a truncated value first, negate preserves subtract's error
before copying its local, and increment/extension intentionally discard locals.
No Unsupported, blanket managed exception, native EDG runtime call, custom
parser/graph/interpreter or fixture shortcut occurs in the production files.

| Finding | Prior objection, response and fresh verdict | Required next work |
| --- | --- | --- |
| ABI-001 | r1 lacked measured ABI/compiler/config/source-to-binary provenance. r3 links actual original routines, pins compiler and 155 includes, measures native/header/managed layouts; exact held artifacts and hashes match. CLOSED for P/D only. | Reopen for changed profile, storage representation, flags or bytes; do not call fixed-point metadata measured. |
| REVIEW-001 / DEPS-001 | r1 was a contract review, not produced-code acceptance. Both full batches and all three actual C# files are now independently compared; dependencies reconcile to the same held snapshot. CLOSED for the named closure. | Re-review later changed dependencies/callers. Original implementation ledgers remain historical pending-state records; supervisor must reference this overlay when updating acceptance. |
| VALIDATION-001 | Held JIT/AOT and native stream validation was missing. Supervisor r3 logs, streams and saved artifacts match and passed. CLOSED for P/D. | No repeated gate requested for unchanged bytes; new dependencies need their own gate after HOLD. |
| ALIAS-001 | Risk of operand-2 early sign snapshot or reference-parameter cloning. Actual C# preserves live bindings, post-write rereads and independent explicit copy locals; alias cases agree with native. CLOSED in D. | Audit every new caller and retain native quirks. |
| INIT-001 / DOMAIN-001 | Remains OPEN, blocks unrestricted mask/extension and initializer/frontend qualification. Native probe invokes original mask with signed width 65535 and UBSan reports const_ints.c:1024 negative shift -15, exit 1. Controls 127/128 pass; unsigned-mask and extension width 65536 return zero without UBSan diagnosis. The latter observations do not review or qualify the remaining defined domain. | Supervisor must resolve full caller/startup reachability and source-defined out-of-D cases before unrestricted acceptance. No clamp, feature reduction, silent fix or departure is authorized. |
| CHECK-001 | CHECKING=1 is confirmed; absence of checking edges in these leaves does not implement assertion/internal-error dependencies elsewhere. OPEN for future closures, not a local P/D blocker. | Port/review the actual assertion/diagnostic/recovery closure before accepting any reachable call. |
| P0 inventory | Prior P0 completeness objection is outside this produced-code review and not closed by leaf tests. | Supervisor retains full inventory/dependency plan work. |
| TEST-CA2014 | Warning at IntegerLeaves/Program.cs:226 is test-only stackalloc inside a loop. Values() has exactly 30 entries and one allocation of two 16-byte objects per entry: 960 bytes total payload per call. Both executed runtimes pass. NONBLOCKING for this held test. | Reassess if the bounded Values list/call shape changes; no production repair requested. |

INIT-001 original range c:2681-2735 computes initializer storage width 65536
from the enabled bit-precise maximum 65535 and 8-bit target bytes. The signed
maximum uses 65535, yielding size_t difference 18446744073709486209 and pinned
int count -65407, remainder -15. The unsigned/extension count is -65408 with
remainder 0. Those observations are a test-only callee probe with UBSan flags;
they are not evidence that the full uninstrumented native startup executed this
path, nor a portable output contract. The managed source remains unchanged;
unchecked conversion does not prove equivalence for native undefined operations.
The independently updated research report is not incorporated by reference into
acceptance: the exact saved probe receipt is pinned in JSON.

## 7. Validation evidence — executed only by supervisor

The r3 gate receipt (`evidence/integer-first-gate/integer-r3-gate-receipt.json`)
has SHA-256 `51eeca0e0be6eb16cd5fb2cd2889321fee88dff02460ed41f5fe69429c30246d`.
It records all shared writers HOLD (pin, leaf, P0 reviewer, arithmetic, research)
and their exact task IDs; the researcher was running but explicitly held. This
review relies on that recorded gate-time coordination, not on inferring HOLD
from a completed status. The prior `reviewedQualifiedRoutines=0` field correctly
describes pre-review state; this report supplies the new bounded verdict.

| Supervisor evidence | JIT | NativeAOT | Interpretation |
| --- | --- | --- | --- |
| IntegerLeaves independent self-checks | 30,036 passed | 30,036 passed | Layout, scalar bounds, reference binding, copy independence, comparison, conversion, bitwise, increment and macro checks |
| IntegerArithmetic independent self-checks | 51,020 passed | 51,020 passed | Carry/borrow, both initial error values, guards, aliases, mixed error overwrite/equality, limb shifts, every width 1..128, negation |
| Original-source differential leaves | 20,844 rows matched | 20,844 rows matched | Same input bytes; raw limbs, other mutation, host bits, error/comparison and alias partition |
| Original-source differential arithmetic | 12,546 rows matched | 12,546 rows matched | Same input bytes and no output normalization erasing alias/error relationships |

All four differential processes exited 0 with exact row counts and no mismatch.
Leaf input SHA-256 is
`ebea9829e9a592da895e7ba2adbc7b974548b940ffa68865e89a7cf538e6dac6`,
matching output SHA-256
`f627d6f69ab66a5d048c60aa74a5477c44ae150b57190640dd8d98556b22f7c3`.
Arithmetic input SHA-256 is
`a43101675d9cf4eb34244632a402cf9835574ea95c9e18d003be701a2a44937e`,
matching output SHA-256
`b3d7ad43ea850df7426929d78a4841830b96558e61a0f046ea3122a358a3359e`.
The deterministic differential driver uses 18 values (boundaries plus seeded
random samples), flag combinations, aliases and listed shift/width boundaries.
Its streams do not exhaust all inputs or instrument every branch. The separate
self-checks cover additional limb boundaries and all mask/extension widths.
Neither passing suite replaces the per-operation source review.

Immutable held production DLL SHA-256 is
`1dcb7b9a0f8c55b20ac196c56eb0a8c01fd3ce3b0eeaeb33df863f7aac8f11a8`.
NativeAOT executable SHA-256 values are
`d1393a7ac777cecad0ab2aca20d4a1d1ad9861140d61af5814ccd0ccdc563042`
(IntegerLeaves) and
`a48c91c6ae1d8f782c999cac5ad090f04856a704fa902ce585857cb8273b5986`
(IntegerArithmetic). Full artifact identities and command/log hashes are retained
in the paired JSON. Byte/string transport serializes every limb in order and
keeps alias partition explicitly; addresses/padding are not compared as values.
It does not trace every internal read, so source review establishes those orders.

Failed r1/r2 harness attempts are preserved: r1 lacked the wrapper's original
source quote-include directory; r2 tried to measure the out-of-scope fixed-point
macro after constituent target macros were undefined. The r3 response repairs
test harness only. None is a C# semantic repair or native-defect exception.

## 8. Per-routine verdicts, concrete repairs and HOLD

The appendix lists 20 routine review records: **18 complete source-valid contract
qualifications and 2 restricted-domain reviews**, plus 3 call-specific macro
records, with 171 individual operation/branch verdicts. The two restricted
routines have `unqualified` unrestricted verdicts and `domain-reviewed` bounded
verdicts; they are never included in a complete/qualified routine total. Every
enabled branch within each stated domain is reviewed. All have executed
validation receipts within that domain, so none is merely tests-pending there.
Legal-count shift contracts and pure macro call-site contracts are labeled
separately from review-imposed width restrictions. All frontend behavior and
later runtime files remain unqualified; INIT-001 prevents lifting width bounds,
and other defined out-of-D cases also require review. These counts are not full
inventory completion, branch-coverage measurement or a formal proof.

Concrete supervisor handoff:

1. Record the 18 source-valid qualifications and 2 restricted-domain reviews
   separately, using these exact hashes and this review
   overlay; preserve the immutable implementation ledgers and failure evidence.
   No production C# repair is requested within P/D.
2. Keep INIT-001 open, including caller/startup reachability and native UB limits.
   Retain all enabled features. Any intentional departure needs explicit user
   authority and separate labeling; this reviewer authorizes none.
3. Bring real checking/diagnostic/runtime dependencies into their own complete
   source and state/identity ledgers before qualifying their callers. Keep P0
   inventory and all remaining P1-P6 work open.
4. Require new review/validation for later dependency/caller/source/configuration
   changes. Newly handwritten batches in disjoint files are outside this snapshot
   and receive no acceptance, even if the old tests still pass.

**HOLD.** Owned files: `faithful/reviews/integer-held-r2.md` and
`faithful/reviews/integer-held-r2.json`. No outstanding writes after final hash
handoff. The JSON records this report's exact hash; both final output hashes are
returned to the supervisor. The three production snapshot hashes above are
unchanged. No source writer is released by this reviewer.

## Appendix: exact per-routine and operation verdicts

| Record | Classification / exact domain | Original -> counterpart |
| --- | --- | --- |
| `set_integer_value(host-signed)` | QUALIFIED complete source-valid contract under P/D | C:33-47 -> L:20-32 |
| `set_integer_value(const-reference)` | QUALIFIED complete source-valid contract under P/D | C:50-59 -> L:35-42 |
| `set_unsigned_integer_value(host-unsigned)` | QUALIFIED complete source-valid contract under P/D | C:62-76 -> L:45-57 |
| `set_unsigned_integer_value(const-reference)` | QUALIFIED complete source-valid contract under P/D | C:79-88 -> L:60-67 |
| `conv_integer_value_to_host_large_integer` | QUALIFIED complete source-valid contract under P/D | C:91-142 -> L:70-117 |
| `cmp_integer_values` | QUALIFIED complete source-valid contract under P/D | C:284-334 -> L:120-151 |
| `incr_integer_value` | QUALIFIED complete source-valid contract under P/D | C:634-643 -> L:154-160 |
| `or_integer_values` | QUALIFIED complete source-valid contract under P/D | C:868-879 -> L:163-171 |
| `and_integer_values` | QUALIFIED complete source-valid contract under P/D | C:882-893 -> L:174-182 |
| `xor_integer_values` | QUALIFIED complete source-valid contract under P/D | C:896-907 -> L:185-193 |
| `complement_integer_value` | QUALIFIED complete source-valid contract under P/D | C:1109-1120 -> L:196-203 |
| `signed_shift_right` | QUALIFIED call-specific macro contract | H:37-40 -> R:47-51 |
| `sign_of` | QUALIFIED call-specific macro contract | H:47-48 -> R:55-58 |
| `add_integer_values` | QUALIFIED complete source-valid contract under P/D | C:711-764 -> A:14-47 |
| `add_mixed_signed_integer_values` | QUALIFIED complete source-valid contract under P/D | C:767-792 -> A:49-63 |
| `subtract_mixed_signed_integer_values` | QUALIFIED complete source-valid contract under P/D | C:795-826 -> A:65-89 |
| `make_integer_value_mask` | UNQUALIFIED unrestricted; domain-reviewed widths1..128 | C:829-850 -> A:91-98 |
| `sign_extend_integer_value` | UNQUALIFIED unrestricted; domain-reviewed widths1..128 | C:853-865 -> A:100-106 |
| `get_part` | QUALIFIED call-specific macro contract | C:913-917 -> A:111-117 |
| `shift_left_integer_value` | QUALIFIED source legal count0..127 | C:921-977 -> A:119-150 |
| `shift_right_integer_value` | QUALIFIED source legal count0..127 | C:980-1029 -> A:152-181 |
| `subtract_integer_values` | QUALIFIED complete source-valid contract under P/D | C:1032-1091 -> A:183-216 |
| `negate_integer_value` | QUALIFIED complete source-valid contract under P/D | C:1094-1105 -> A:218-224 |

Every operation row has an explicit **Equivalent within stated domain** verdict.
This differs from unrestricted routine status: mask/extension remain unqualified.
Full file/range hashes, ordered source operations, parameters and dependencies
are in the paired JSON. No dynamic per-branch coverage or new caller acceptance.

### `set_integer_value(host-signed)`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `set_integer_value(host-signed).entry` | C:39-40 -> L:23-24 | Equivalent within domain: Int index and signed64 part local match with no added state. |
| `set_integer_value(host-signed).loop-init-true-false-update` | C:42-42 -> L:26-26 | Equivalent within domain: Both loop arms, descending7..0, decrement and exit-1 agree; all eight writes remain. |
| `set_integer_value(host-signed).mask` | C:43-43 -> L:28-28 | Equivalent within domain: Signed64 mask65535 gives the same low limb for negative and positive inputs. |
| `set_integer_value(host-signed).narrow-store` | C:44-44 -> L:29-29 | Equivalent within domain: Narrow/store targets the existing limb before local value changes. |
| `set_integer_value(host-signed).shift-call` | C:45-45 -> L:30-30 | Equivalent within domain: Pure local value and constant16 reach the shift helper after the store; assignment matches. |
| `set_integer_value(host-signed).exit` | C:46-47 -> L:31-32 | Equivalent within domain: Exit/void return adds no early fill shortcut or effect. |

### `set_integer_value(const-reference)`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `set_integer_value(const-reference).forward-loop` | C:56-56 -> L:38-38 | Equivalent within domain: Unsigned32 forward loop, bound promotion, increment and false exit8 match. |
| `set_integer_value(const-reference).copy-live-part` | C:57-57 -> L:40-40 | Equivalent within domain: Live reference binding is retained; same-index read precedes write including exact self alias. |
| `set_integer_value(const-reference).exit` | C:58-59 -> L:41-42 | Equivalent within domain: No retained pointer, allocation or extra source mutation at return. |

### `set_unsigned_integer_value(host-unsigned)`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `set_unsigned_integer_value(host-unsigned).entry` | C:68-69 -> L:48-49 | Equivalent within domain: Native this_part remains signed64; the index remains int. |
| `set_unsigned_integer_value(host-unsigned).loop` | C:71-71 -> L:51-51 | Equivalent within domain: Descending initialization, true/false condition, update and eight visits match. |
| `set_unsigned_integer_value(host-unsigned).mask` | C:72-72 -> L:53-53 | Equivalent within domain: Unsigned64 mask result0..65535 makes the explicit signed64 conversion exact. |
| `set_unsigned_integer_value(host-unsigned).store` | C:73-73 -> L:54-54 | Equivalent within domain: Ushort narrowing/store occurs at the original point. |
| `set_unsigned_integer_value(host-unsigned).shift` | C:74-74 -> L:55-55 | Equivalent within domain: Logical unsigned64 right shift16 follows the store without sign extension. |
| `set_unsigned_integer_value(host-unsigned).exit` | C:75-76 -> L:56-57 | Equivalent within domain: Loop exit and void return add no effect. |

### `set_unsigned_integer_value(const-reference)`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `set_unsigned_integer_value(const-reference).forward-loop` | C:85-85 -> L:63-63 | Equivalent within domain: The separate overload preserves unsigned32 forward traversal and terminal predicate. |
| `set_unsigned_integer_value(const-reference).copy-live-part` | C:86-86 -> L:65-65 | Equivalent within domain: The original referent is read live before the same-index destination write; no snapshot. |
| `set_unsigned_integer_value(const-reference).exit` | C:87-88 -> L:66-67 | Equivalent within domain: Return ends the call without retaining aliases or extra stores. |

### `conv_integer_value_to_host_large_integer`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `conv_integer_value_to_host_large_integer.entry` | C:105-113 -> L:76-84 | Equivalent within domain: P size_t/ulong counters, discarded64, unsigned result0 and overflow false match in order. |
| `conv_integer_value_to_host_large_integer.raw-sign` | C:118-118 -> L:86-86 | Equivalent within domain: Raw sign is captured from live part0 before iteration regardless of is_signed. |
| `conv_integer_value_to_host_large_integer.fill-short-circuit` | C:119-119 -> L:87-87 | Equivalent within domain: is_signed is evaluated first; false skips the is_negative condition operand. |
| `conv_integer_value_to_host_large_integer.fill-negative` | C:120-120 -> L:89-89 | Equivalent within domain: Signed-negative true arm chooses ushort65535. |
| `conv_integer_value_to_host_large_integer.fill-other` | C:121-123 -> L:91-94 | Equivalent within domain: Other flag/sign combinations choose zero without another sign read. |
| `conv_integer_value_to_host_large_integer.loop` | C:124-124 -> L:95-95 | Equivalent within domain: Ascending0..7 loop, increment and false exit8 match. |
| `conv_integer_value_to_host_large_integer.read-part` | C:125-125 -> L:97-97 | Equivalent within domain: Each live input limb is copied to ushort before its predicates. |
| `conv_integer_value_to_host_large_integer.discard-predicate` | C:126-126 -> L:98-98 | Equivalent within domain: The counter comparison divides four discarded high limbs from four retained limbs identically. |
| `conv_integer_value_to_host_large_integer.fill-match-predicate` | C:127-127 -> L:100-100 | Equivalent within domain: Promoted fill comparison has equal/unequal arms; equality retains sticky overflow. |
| `conv_integer_value_to_host_large_integer.fill-mismatch` | C:128-129 -> L:102-103 | Equivalent within domain: Mismatch sets overflow but does not stop later reads or low-result construction. |
| `conv_integer_value_to_host_large_integer.retained-part` | C:130-132 -> L:105-108 | Equivalent within domain: Unsigned64 shift16 then addition preserves original assembly stages. |
| `conv_integer_value_to_host_large_integer.advance-bits` | C:133-134 -> L:109-110 | Equivalent within domain: Counter advance16 occurs before the next iteration and reaches128 without overflow. |
| `conv_integer_value_to_host_large_integer.final-sign-short-circuit` | C:137-137 -> L:111-111 | Equivalent within domain: Unsigned mode skips signed cast/sign test; signed mode compares original and pinned signed-low64 signs. |
| `conv_integer_value_to_host_large_integer.final-sign-mismatch` | C:138-139 -> L:113-114 | Equivalent within domain: Sign mismatch sets sticky overflow; false arm leaves it unchanged. |
| `conv_integer_value_to_host_large_integer.value-output` | C:140-140 -> L:115-115 | Equivalent within domain: Pinned GCC signed conversion agrees with unchecked low64 bit cast; value output is stored first even on overflow. |
| `conv_integer_value_to_host_large_integer.error-output-return` | C:141-142 -> L:116-117 | Equivalent within domain: Bool error output is unconditional and second, then return leaves input intact. |

### `cmp_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `cmp_integer_values.entry-index` | C:298-300 -> L:125-125 | Equivalent within domain: Active array int index is preserved; scalar alternatives are separately excluded. |
| `cmp_integer_values.sign1-short-circuit` | C:301-301 -> L:126-126 | Equivalent within domain: First flag short-circuits its live sign read. |
| `cmp_integer_values.sign2-short-circuit` | C:302-302 -> L:127-127 | Equivalent within domain: Second flag/sign evaluation follows first with its own short circuit, even under self alias. |
| `cmp_integer_values.initialize-result` | C:303-303 -> L:128-128 | Equivalent within domain: Both initialize int result0. |
| `cmp_integer_values.sign-predicate` | C:305-305 -> L:130-130 | Equivalent within domain: Unequal signs bypass limbs; equal signs enter comparison. |
| `cmp_integer_values.different-sign-ternary` | C:309-309 -> L:132-132 | Equivalent within domain: Both ternary outcomes preserve negative-first=-1 and positive-first=1. |
| `cmp_integer_values.equal-sign-loop` | C:322-322 -> L:136-136 | Equivalent within domain: Ascending loop0..7 compares high significance first; all-equal exit retains0. |
| `cmp_integer_values.greater-predicate` | C:323-323 -> L:138-138 | Equivalent within domain: Greater comparison occurs first; false alone reaches less comparison. |
| `cmp_integer_values.greater-break` | C:324-325 -> L:140-141 | Equivalent within domain: Greater true stores1 then breaks before lower-significance reads. |
| `cmp_integer_values.less-predicate` | C:326-326 -> L:143-143 | Equivalent within domain: Less predicate is evaluated only after greater false; equality continues. |
| `cmp_integer_values.less-break` | C:327-328 -> L:145-146 | Equivalent within domain: Less true stores-1 then breaks at the same index. |
| `cmp_integer_values.loop-sign-exit` | C:329-332 -> L:147-149 | Equivalent within domain: Equal limbs update/continue; terminal false preserves0 and early breaks preserve their result. |
| `cmp_integer_values.return` | C:333-334 -> L:150-151 | Equivalent within domain: Return is exactly -1/0/1 with no input writes. |

### `incr_integer_value`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `incr_integer_value.locals` | C:639-640 -> L:156-157 | Equivalent within domain: Distinct inline one and bool err have nested-call lifetimes. |
| `incr_integer_value.set-one` | C:641-641 -> L:158-158 | Equivalent within domain: Explicit long literal selects signed scalar setter before addition. |
| `incr_integer_value.unsigned-add` | C:642-642 -> L:159-159 | Equivalent within domain: Original intval and distinct one reach unsigned add; local err is written then ignored. |
| `incr_integer_value.return-ignore-error` | C:643-643 -> L:160-160 | Equivalent within domain: Return discards err and retains no pointer. |

### `or_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `or_integer_values.entry` | C:875-875 -> L:166-166 | Equivalent within domain: Int loop-local declaration introduces no state. |
| `or_integer_values.descending-loop` | C:876-876 -> L:167-167 | Equivalent within domain: Descending7..0 traversal, decrement and false exit-1 match. |
| `or_integer_values.promote-operate-store` | C:877-877 -> L:169-169 | Equivalent within domain: Two live ushort reads promote to int for OR, then narrow/store; exact self alias survives. |
| `or_integer_values.exit` | C:878-879 -> L:170-171 | Equivalent within domain: Loop exit/return has no further write or retained alias. |

### `and_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `and_integer_values.entry` | C:889-889 -> L:177-177 | Equivalent within domain: Int loop-local declaration introduces no state. |
| `and_integer_values.descending-loop` | C:890-890 -> L:178-178 | Equivalent within domain: Descending7..0 traversal, decrement and false exit-1 match. |
| `and_integer_values.promote-operate-store` | C:891-891 -> L:180-180 | Equivalent within domain: Two live ushort reads promote to int for AND, then narrow/store; exact self alias survives. |
| `and_integer_values.exit` | C:892-893 -> L:181-182 | Equivalent within domain: Loop exit/return has no further write or retained alias. |

### `xor_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `xor_integer_values.entry` | C:903-903 -> L:188-188 | Equivalent within domain: Int loop-local declaration introduces no state. |
| `xor_integer_values.descending-loop` | C:904-904 -> L:189-189 | Equivalent within domain: Descending7..0 traversal, decrement and false exit-1 match. |
| `xor_integer_values.promote-operate-store` | C:905-905 -> L:191-191 | Equivalent within domain: Two live ushort reads promote to int for XOR, then narrow/store; exact self alias survives. |
| `xor_integer_values.exit` | C:906-907 -> L:192-193 | Equivalent within domain: Loop exit/return has no further write or retained alias. |

### `complement_integer_value`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `complement_integer_value.entry` | C:1115-1115 -> L:198-198 | Equivalent within domain: Int local matches with no persistent state. |
| `complement_integer_value.descending-loop` | C:1117-1117 -> L:199-199 | Equivalent within domain: Descending7..0 loop, update and false exit match, including all writes for zero. |
| `complement_integer_value.promote-complement-mask-store` | C:1118-1118 -> L:201-201 | Equivalent within domain: Ushort promotes to int before complement; mask65535 precedes narrowing and store. |
| `complement_integer_value.exit` | C:1119-1120 -> L:202-203 | Equivalent within domain: No error, cleanup or additional storage operation at return. |

### `signed_shift_right`

Status: **qualified-call-specific**; call-specific-qualified.

Domain: Call-specific: enabled production c45 only, pure local signed64 value and constant count16. Pure direct checks0..63 supplement review without accepting new macro expressions.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `shift.unsigned-cast-shift` | H:38-38 -> R:49-49 | Equivalent within domain: Unsigned64 cast precedes logical shift; repeated pure native operands equal managed parameter values at c45. |
| `shift.sign-predicate-and-arms` | H:39-40 -> R:50-50 | Equivalent within domain: Both sign arms preserve all-ones mask construction, OR and signed64 bit cast. Production acceptance is pure value/count16 at c45; pure direct self-checks0..63 do not accept new macro expressions. |

### `sign_of`

Status: **qualified-call-specific**; call-specific-qualified.

Domain: Call-specific: exact live lvalues and evaluation points in opened conversion/comparison/add/subtract/mixed-subtract/right-fill calls, preserving short circuits and post-write reads.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `sign.read-mask-predicate` | H:47-48 -> R:57-57 | Equivalent within domain: Live part0 promotes to int, masks0x8000 and compares with zero. Result is a raw sign bit; original caller short circuits/evaluation points are retained, never cached. |

### `add_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `A01-entry` | C:737-739 -> A:19-21 | Equivalent within domain: Signed64 carry0 and original first sign are initialized; no operand2 sign snapshot. |
| `A02-loop-init` | C:741-741 -> A:23-23 | Equivalent within domain: Size_t expression7 converts exactly to managed int7. |
| `A03-loop-true` | C:741-741 -> A:23-23 | Equivalent within domain: True predicate visits7..0 even for zero input. |
| `A04-work` | C:742-744 -> A:25-26 | Equivalent within domain: Both live parts widen separately to signed64; add then carry is0..131071 and both reads precede store under alias. |
| `A05-carry-true` | C:745-747 -> A:27-31 | Equivalent within domain: work>65535 true subtracts signed base65536 then sets carry1. |
| `A06-carry-false` | C:745-750 -> A:27-35 | Equivalent within domain: False carry arm sets0 without adjustment. |
| `A07-store` | C:751-751 -> A:36-36 | Equivalent within domain: Adjusted0..65535 work narrows/stores current limb. |
| `A08-loop-update` | C:741-741 -> A:23-23 | Equivalent within domain: Decrement follows store; smaller numeric index means higher significance, clarifying ledger wording. |
| `A09-loop-false` | C:741-752 -> A:23-37 | Equivalent within domain: False predicate at-1 follows eight writes; initial zero iterations are impossible under P. |
| `A10-unsigned` | C:754-757 -> A:38-41 | Equivalent within domain: Unsigned branch overwrites err using final carry after all writes. |
| `A11-signed` | C:754-761 -> A:38-45 | Equivalent within domain: Signed branch rereads operand2 after mutation, so exact alias observes result sign. |
| `A12-signed-short-false` | C:761-761 -> A:45-45 | Equivalent within domain: First signed comparison false skips operand1 sign read and writes false. |
| `A13-signed-short-true` | C:761-761 -> A:45-45 | Equivalent within domain: First comparison true reads live result sign; native alias overflow behavior is not repaired. |
| `A14-return` | C:762-764 -> A:46-47 | Equivalent within domain: Void return follows final err with no later operand write. |

### `add_mixed_signed_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `MA01-entry` | C:778-778 -> A:55-55 | Equivalent within domain: orig_value is an independent inline local. |
| `MA02-select-true` | C:780-780 -> A:57-57 | Equivalent within domain: True selection copies only original operand1 before base add. |
| `MA03-select-false` | C:780-780 -> A:57-57 | Equivalent within domain: False selection copies only original operand2, including matching unsigned flags; not an alias. |
| `MA04-base-call` | C:781-781 -> A:58-58 | Equivalent within domain: Base add completes its mutations/error before flag comparison. |
| `MA05-flags-equal` | C:784-791 -> A:59-62 | Equivalent within domain: Equal flags skip cmp and retain base error. |
| `MA06-flags-differ` | C:784-790 -> A:59-61 | Equivalent within domain: Different flags compare original-copy as signed against live result using first signedness, then overwrite err with cmp>0. |
| `MA07-return` | C:791-792 -> A:62-63 | Equivalent within domain: Optional replacement precedes local lifetime end; no rollback. |

### `subtract_mixed_signed_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `MS01-entry-copy` | C:806-808 -> A:71-73 | Equivalent within domain: Original operand1 is copied for every flag/alias combination before subtraction. |
| `MS02-base-call` | C:809-809 -> A:74-74 | Equivalent within domain: Base subtract receives live operands and writes err before later sign inspection. |
| `MS03-flags-equal` | C:816-825 -> A:75-88 | Equivalent within domain: Equal flags skip sign/cmp and retain base err. |
| `MS04-flags-differ` | C:816-816 -> A:75-75 | Equivalent within domain: Unequal flags enter replacement only after base completion. |
| `MS05-second-unsigned` | C:817-817 -> A:77-77 | Equivalent within domain: Unsigned second flag skips sign_of and yields false. |
| `MS06-second-signed` | C:817-817 -> A:77-77 | Equivalent within domain: Signed second flag reads live operand2 post-subtraction, including result0 under exact alias. |
| `MS07-comparison` | C:818-819 -> A:78-79 | Equivalent within domain: Sign calculation precedes cmp; both comparison operands use first signedness. |
| `MS08-negative-second` | C:820-821 -> A:80-82 | Equivalent within domain: Negative-second branch overwrites err with cmp>0 for both outcomes. |
| `MS09-nonnegative-second` | C:820-824 -> A:80-87 | Equivalent within domain: Nonnegative branch retains cmp<=0, including native error at equality. |
| `MS10-return` | C:825-826 -> A:88-89 | Equivalent within domain: No restoration of base error at void return. |

### `make_integer_value_mask`

Status: **unqualified**; restricted-domain-reviewed.

Domain: P; valid output storage; bits1..128 only. Native explicit precondition is bits>=1, not this upper bound. Remaining source-defined size_t cases unreviewed.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `M01-zero` | C:837-837 -> A:93-93 | Equivalent within domain: Long zero setter is called with all original descending stores. |
| `M02-complement` | C:838-838 -> A:94-94 | Equivalent within domain: Complement follows zero on the same output storage. |
| `M03-width-convert` | C:839-839 -> A:95-96 | Equivalent within domain: Unsigned native-size subtraction then int conversion matches widths1..128 only. Other source-defined cases lack equivalence review; unrestricted routine remains UNQUALIFIED. |
| `M04-right-call` | C:839-840 -> A:95-97 | Equivalent within domain: Logical right shift follows complement with computed count and both false flags; no direct formula. |
| `M05-return` | C:849-850 -> A:98-98 | Equivalent within domain: Bounded call returns without err/diagnostic; this does not complete the unrestricted routine. |

### `sign_extend_integer_value`

Status: **unqualified**; restricted-domain-reviewed.

Domain: P; all128-bit input patterns; bits1..128 only. Other source-defined size_t cases unreviewed.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `E01-width-convert` | C:860-860 -> A:102-102 | Equivalent within domain: Unsigned size_t subtraction followed by int narrowing matches widths1..128 only. Other source-defined size_t cases are unreviewed; unrestricted routine is UNQUALIFIED. |
| `E02-local-error` | C:861-861 -> A:103-103 | Equivalent within domain: Local bool has call lifetime, is overwritten by left shift and never read. |
| `E03-left-call` | C:862-862 -> A:104-104 | Equivalent within domain: Left shift finishes all mutations and local err before right shift; error is ignored exactly. |
| `E04-right-call` | C:863-864 -> A:105-105 | Equivalent within domain: Same value/count then reaches right shift with true/true; no shortcut cast. |
| `E05-return` | C:865-865 -> A:106-106 | Equivalent within domain: Bounded-domain void return adds no error or cleanup and does not close unrestricted qualification. |

### `get_part`

Status: **qualified-call-specific**; call-specific-qualified.

Domain: Call-specific: actual pure bounded int indices left0..15/right-8..7, stable nonvolatile caller ushort fill and live existing integer lvalue in two reviewed shifts. New effectful/volatile callers unqualified.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `G01-negative` | C:915-915 -> A:114-114 | Equivalent within domain: Negative index check comes first and true skips upper check/dereference. |
| `G02-upper` | C:915-916 -> A:114-115 | Equivalent within domain: Nonnegative indices alone test >=8; native int casts are identities for actual expressions. |
| `G03-fill` | C:915-917 -> A:114-116 | Equivalent within domain: Either out-of-range arm selects stable caller ushort fill then widens, without dereference. |
| `G04-in-range` | C:915-917 -> A:114-116 | Equivalent within domain: In-range arm reads the current live limb then widens before any caller shift. |
| `G05-evaluation-adaptation` | C:913-917 -> A:111-117 | Equivalent within domain: Only actual pure bounded int expressions left0..15/right-8..7 and stable nonvolatile fill/lvalues are accepted. Macro repetition versus once-evaluated parameters is unobservable there; new effectful/volatile callers are unqualified. |

### `shift_left_integer_value`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P; all limb patterns; valid bool output; source legal-count caller precondition0..127 for128-bit represented integer (c927-928), not arbitrary int counts.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `L01-entry` | C:944-948 -> A:123-127 | Equivalent within domain: Int offset/count locals and false sticky overflow match with no snapshot. |
| `L02-offset` | C:959-959 -> A:129-129 | Equivalent within domain: First compute int quotient/16,0..7 under source legal-count precondition0..127. |
| `L03-first-shift` | C:960-960 -> A:130-130 | Equivalent within domain: Then int remainder%16,0..15; CLR invalid-count masking supplies no native semantics. |
| `L04-second-shift` | C:961-961 -> A:131-131 | Equivalent within domain: Then16-remainder gives1..16, legal for unsigned64 work. |
| `L05-loop-init` | C:962-962 -> A:132-132 | Equivalent within domain: Ascending loop starts0 and preserves higher unread source indices. |
| `L06-loop-true` | C:962-962 -> A:132-132 | Equivalent within domain: True visits0..7, including all eight iterations at count0. |
| `L07-current-work` | C:963-965 -> A:134-136 | Equivalent within domain: Stable zero fill is set, then current original limb widens to unsigned64. |
| `L08-whole-loss-first-short` | C:966-966 -> A:137-137 | Equivalent within domain: Whole-loss short circuit preserves index-test first and skipped work!=0. |
| `L09-whole-loss-true` | C:966-967 -> A:137-139 | Equivalent within domain: Whole-loss true sets sticky overflow and skips else-if. |
| `L10-whole-loss-false` | C:966-968 -> A:137-141 | Equivalent within domain: Whole-loss false reaches boundary index test; false skips shift-loss check. |
| `L11-boundary-compare-true` | C:968-968 -> A:141-141 | Equivalent within domain: Equal boundary tests unsigned64 work shifted by second count. |
| `L12-boundary-loss-true` | C:968-969 -> A:141-143 | Equivalent within domain: Nonzero boundary loss sets sticky overflow at the original point. |
| `L13-no-loss` | C:966-970 -> A:137-144 | Equivalent within domain: Both loss routes false retain previous overflow, including an earlier true. |
| `L14-get-first` | C:971-971 -> A:145-145 | Equivalent within domain: Pure first index i+offset is guarded then widened before left shift0..15. |
| `L15-get-second` | C:972-972 -> A:146-146 | Equivalent within domain: Pure second index i+offset+1 is guarded then unsigned64 shifted right1..16; both reads precede store. |
| `L16-combine-store` | C:971-973 -> A:145-147 | Equivalent within domain: Widened OR, mask65535, ushort narrow/store preserve stages and destination. |
| `L17-loop-update` | C:962-962 -> A:132-132 | Equivalent within domain: Increment occurs after store then predicate repeats. |
| `L18-loop-false` | C:962-974 -> A:132-148 | Equivalent within domain: False exit8 retains all limbs and sticky overflow; initial zero iterations impossible under P. |
| `L19-final-error` | C:975-975 -> A:149-149 | Equivalent within domain: Final err store follows every limb and overwrites incoming bool even for count0. |
| `L20-return` | C:976-977 -> A:150-150 | Equivalent within domain: Void return adds no guard/diagnostic. Acceptance is legal0..127 caller contract, not arbitrary int counts. |

### `shift_right_integer_value`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P; all limb patterns/flags; source legal-count caller precondition0..127 (c986-987), not arbitrary int counts.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `R01-entry` | C:999-1003 -> A:157-161 | Equivalent within domain: Int locals/ushort fill match with no initial input copy. |
| `R02-offset` | C:1012-1012 -> A:163-163 | Equivalent within domain: Int quotient/16 is computed first under source legal0..127 contract. |
| `R03-first-shift` | C:1013-1013 -> A:164-164 | Equivalent within domain: Int remainder%16 follows and remains0..15. |
| `R04-second-shift` | C:1014-1014 -> A:165-165 | Equivalent within domain: Second count16-remainder follows and remains1..16. |
| `R05-unsigned-short` | C:1017-1017 -> A:166-166 | Equivalent within domain: False is_signed skips sign_extend and sign_of. |
| `R06-no-extend` | C:1017-1020 -> A:166-172 | Equivalent within domain: True signed flag evaluates sign_extend; false extension still skips sign read. |
| `R07-extend` | C:1017-1018 -> A:166-168 | Equivalent within domain: Both true read original sign before mutation; bool-to-int0/1, signed negation, mask, ushort fill match. |
| `R08-fill-zero` | C:1019-1020 -> A:170-172 | Equivalent within domain: The common false arm explicitly assigns zero fill. |
| `R09-loop-init` | C:1022-1022 -> A:174-174 | Equivalent within domain: Descending loop starts7, preserving lower unread source indices. |
| `R10-loop-true` | C:1022-1022 -> A:174-174 | Equivalent within domain: True visits7..0 including eight writes at count0. |
| `R11-get-first` | C:1023-1024 -> A:176-177 | Equivalent within domain: First guarded i-offset read uses captured fill, widens then shifts unsigned64 right0..15. |
| `R12-get-second` | C:1025-1025 -> A:178-178 | Equivalent within domain: Second guarded i-offset-1 read widens then shifts left1..16; same fill and both reads before write. |
| `R13-combine-store` | C:1024-1026 -> A:177-179 | Equivalent within domain: Widened OR, mask and ushort store retain truncation timing. |
| `R14-loop-update` | C:1022-1022 -> A:174-174 | Equivalent within domain: Decrement follows each write. |
| `R15-loop-false` | C:1022-1027 -> A:174-180 | Equivalent within domain: False exit-1 follows eight iterations; initial zero iterations impossible under P. |
| `R16-return` | C:1028-1029 -> A:181-181 | Equivalent within domain: No loss error/diagnostic at return; nonlegal counts, including INIT-001, remain outside acceptance. |

### `subtract_integer_values`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `S01-entry` | C:1062-1064 -> A:188-190 | Equivalent within domain: Signed64 borrow0 and only original first sign are captured; operand2 stays live. |
| `S02-loop-init` | C:1066-1066 -> A:192-192 | Equivalent within domain: Loop starts int7. |
| `S03-loop-true` | C:1066-1066 -> A:192-192 | Equivalent within domain: True visits7..0 and all eight stores. |
| `S04-work` | C:1067-1069 -> A:194-195 | Equivalent within domain: Both parts widen to signed64; subtract operand2 then borrow is-65536..65535 without overflow. |
| `S05-borrow-true` | C:1070-1072 -> A:196-199 | Equivalent within domain: Negative work adds signed base65536 then sets borrow1. |
| `S06-borrow-false` | C:1070-1075 -> A:196-204 | Equivalent within domain: Nonnegative work sets borrow0 without adjustment. |
| `S07-store` | C:1076-1076 -> A:205-205 | Equivalent within domain: Adjusted work narrows/stores only after current reads. |
| `S08-loop-update` | C:1066-1066 -> A:192-192 | Equivalent within domain: Decrement follows store toward higher significance. |
| `S09-loop-false` | C:1066-1077 -> A:192-206 | Equivalent within domain: False exit-1 retains final borrow; initial zero iterations impossible under P. |
| `S10-unsigned` | C:1079-1083 -> A:207-210 | Equivalent within domain: Unsigned err uses final borrow after all writes and overwrites initial bool. |
| `S11-signed` | C:1079-1088 -> A:207-214 | Equivalent within domain: Signed arm rereads operand2 after mutation; self alias sees result sign. |
| `S12-signed-short-false` | C:1088-1088 -> A:214-214 | Equivalent within domain: First comparison false skips second sign read and stores false err. |
| `S13-signed-short-true` | C:1088-1088 -> A:214-214 | Equivalent within domain: First comparison true reads result sign; negative self-subtract native error survives. |
| `S14-return` | C:1089-1091 -> A:215-216 | Equivalent within domain: No recovery, restoration or later operand write at return. |

### `negate_integer_value`

Status: **qualified**; complete-source-valid-contract-qualified.

Domain: P/D live typed storage, full128-bit limb or host-scalar domain admitted by signature, all bool flags, exact self alias or disjoint integer operands, distinct valid typed output storage.

| Operation ID | Original -> counterpart | Verdict and independent reason |
| --- | --- | --- |
| `N01-local` | C:1101-1101 -> A:220-220 | Equivalent within domain: Result is a distinct call-lived inline local, not an alias/heap owner. |
| `N02-zero-call` | C:1102-1102 -> A:221-221 | Equivalent within domain: Signed zero setter initializes all local limbs before subtract. |
| `N03-subtract-call` | C:1103-1103 -> A:222-222 | Equivalent within domain: Signed subtract uses local result and original caller operand, writing err before final operand copy. |
| `N04-result-copy` | C:1104-1104 -> A:223-223 | Equivalent within domain: Whole inline result copy follows subtract/error completion, preserving original operand as second during the call. |
| `N05-return` | C:1105-1105 -> A:224-224 | Equivalent within domain: Return retains native signed-minimum overflow with no special-case algorithm. |

**Final HOLD:** only the two assigned review files were written. No outstanding
writes, implementation changes, builds/tests, native preparation, Git mutations
or subdelegation. New runtime files receive no acceptance.

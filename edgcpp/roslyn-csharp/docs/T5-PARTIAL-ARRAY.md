# Fixed automatic array with constructor failure

Status: bounded automatic-array construction/partial cleanup now passes
production CLI and complete native/JIT/NativeAOT observations. The
[verified receipt](../acceptance/lifetimes/array-report.json) records actual
helper contracts, prefix coverage, regressions and remaining limits. Earlier
constructor-failure baseline rejection is preserved; general array EH remains open.

[partial-array.cpp](../tests/Fixtures/T5/partial-array.cpp) constructs three
automatic `ArrayElement` objects. Its default constructor records an attempted
id, optionally throws integer 7, and records completion/live ownership only
after the failure check. The fixture executes a successful path and a path
that fails during the second element, with the existing five-value protocol
and preserved prior live count. There is no heap allocation, inheritance,
class exception payload, nested throw, rethrow or native unwind boundary.

## Evidence and actual calls

Artifact paths are relative to `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `ba710a6e959a51bb060dd35b6f71d55578f0290516ca0c0d2981ad5b7da3570e` |
| `capture/partial-array.roslynobj` | `96cac6df29a417cc4f9c853ae2ef877fc6c6af665ccacdded4cbd50677cb7b62` |
| `partial-array-facts-1.json` | `d901894fd3324037249547ae968329c45a25f44a0366f9455c1f6128623b6f7e` |
| `partial-array-facts-6.json` (public handle payload audit) | `f8cba6d279777b146a12c5f0dd1feaf5ecdbf5d684c5cd6b41c639ff36a985dd` |
| `array-capture-provenance-1.json` | `2ddd6d84421bb59631b2740133962189cccb76e280f96ddb1d3c6df7a2971d45` |
| `native-partial-array-run-1/stdout.txt` | `c13fc6cdc5cb4fa426a93f531579798496812212e25ba840ddfe587d7904205e` |

Provenance pins producer, probe source, native compilers, harness, binary and
measured phases. The object is unchanged after inspection. Public locators
below are probe labels based on semantic equality, not stored IDs or binding
names. Its 12 bodies and 608 entities are inventory facts, not a schedule.

The selected execution calls declaration-only `__cxa_vec_ctor`, entity 15,
with signature `void(void*, u64, u64, void(*)(void*), void(*)(void*))`.
Call 108 supplies the actual array decay, count 3, element size 4, a cast of
constructor Reference 115 targeting C1 entity 16, and a cast of destructor
Reference 117 targeting D1 entity 17. All five formal/argument roles are
`abi_parameter`; the call explicitly records left-to-right evaluation.

Normal destruction is retained Call 186 to declaration-only `__cxa_vec_dtor`,
entity 18: `void(void*, u64, u64, void(*)(void*))`. It supplies the address of
the same array, count 3, size 4 and the same D1 identity through Reference 195
and compiler casts. It also records left-to-right evaluation. Both helpers
have prototype, default calling convention and C++ linkage, without variadic,
receiver, return-slot or copy-constructor roles. Their weak, alias, ifunc and
COMDAT policy flags are false. A same-spelled arbitrary routine is not this
validated semantic helper contract.

ArrayElement C1, entity 16, has one selected `this_parameter`, ordinary pointer
type 483 of size/alignment 8/8, and nullable noexcept. D1, entity 17, has the
same selected receiver shape and retained noexcept=true. Their C2/D2 bodies
remain separate receiver-only forwarding calls to the corresponding primary.
The helper's erased callback routine types instead have one `abi_parameter`
of type `void*`; the retained references and casts connect them to the actual
selected definitions. No callback identity is reconstructed from names.

## Complete-array state versus partial construction

ArrayElement type 482 is size/alignment 4/4. The automatic variable `items`,
entity 210, has complete array type 547, size/alignment 12/4, bound 3. Only
`array_path`, entity 14, has cleanup regions. The throwing C1 has no EH table,
cleanup regions or locals; there is no second cleanup-bearing function to
invent for the earlier exact-count admission rule.

Region 220 has one destructor action 559 selecting D1 entity 17 and
CleanupArray 222. That array describes the fully constructed array: element
size 4, count 3, no count locations and `IsNonarraySize=false`. Its single
storage location 562 is the actual direct variable 210, offset 0, null base,
without indirection or receiver-relative addressing. The action's array flag
is true; other action flags are false, with no condition or ABI detail.
Independent region 221 contains the no-object exception-release action.
Both next-region links are null.

The array table 214 and region table 215 have actual `element` and `cleanup`
projections owned by the same function, with reciprocal table associations.
Each has one empty `initk_static` local descriptor and no executable or
constant initializer. Metadata descriptor presence is not initialization work.

Metadata recipe 557 names items/type 547/D1 and links recipe 558. Recipe 558
has null target/type and `IsPartialAggregate=true`. Both are metadata-only
`dik_none`, without executable expression or operands. Neither supplies an
evaluated completion count or a second destructor loop. The original probe
identified CleanupArray.HandleConstant without decoding its value. The fresh
ProbeVersion 5 report, `partial-array-facts-6.json`, audits the same unchanged
object: handle 561 has `ValueKind=ck_stack_offset`, type 430 (unsigned 64-bit),
null original type, and public `StackOffset.Variable=items210`, `Offset=0`.
This actual constant payload matches action storage 562; no stack layout or
hidden partial count is inferred. The narrow projection reads only public
constant/value facets and does not evaluate backing expressions.

There is one Throw 255 in C1, with typed assignment operand 256 into the
thrown-object address. The thrown and caught type is the same signed integer
467, size/alignment 4/4. There is one typed Try 65/handler 541. Function initial
and null state are 65535. State 121 arms state 0 only after vector construction
Call 108 returns. Catch entry State 68 selects state 1. The public markers
retain those numeric states with null direct Region associations; the decoded
regions remain separate public facts. Normal return clears state before
explicit vector-destruction Call 186. The try's entry boundary is null, so a
throw during construction has not armed the complete-array region.

## Exact stock helper responsibility

The pristine source is revision
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`.
`runtime-array-contract-source-1/provenance.json` records read-only extraction
of `lib_src/vec_newdel.c` (SHA-256
`e8208f028a472d45a2ddac35ffb2b96a25113091e2d0e7e0b56cda4483499a4d`)
and its header. Neither is compiled or linked into generated execution.

`vec_newdel.c:814–829` defines construction in supplied storage. Its loop at
548–570 calls the selected constructor and increments `elements_processed`
only after that call returns. The cleanup at 853–900 destroys exactly those
completed elements in reverse order. `throw.c:1444–1448` dispatches this
in-progress vector cleanup during the cleanup pass, after handler search.
The full-array descriptor count 3 is not the partial-completion count.

A managed projection must therefore own completion tracking inside the
validated helper and preserve destination selection before cleanup. It must
use the captured count, stride and selected C1/D1 identities, borrowing the
existing automatic storage. No native helper may invoke a managed throwing
callback through an unmanaged entry point. The current integer payload owner
and handler-specific release contract remain unchanged.

## Preserved initial candidate observations

The native oracle reports the same result twice:

```json
{"result":130,"overflow":0,"live":0,"values":[123,0,7,0,0],"events":[101,201,102,202,103,203,601,612,623,303,302,301,900,101,201,102,502,301,707]}
```

The failure observation contains completion/destruction of the first element,
an attempted second element and the typed handler. It contains no completion
or destructor event for the failed second element. This is an independent
comparison oracle; retained EDG execution and stock contracts govern projection.

The preserved baseline CLI exits 1 with `ROC1301` for required
`__cxa_vec_ctor`, before publication. This unsupported binding diagnostic is
not execution evidence and does not authorize a native C++ helper provider.
This original baseline remains preserved separately from the stronger generated
proof below. Copy arrays, dynamic counts,
VLA/deallocation, throwing destructors, nested ownership, rethrow and native
unwinding remain outside this finite candidate.

## Additional zero-, one- and two-element prefix observations

The separate [partial-array-prefixes.cpp](../tests/Fixtures/T5/partial-array-prefixes.cpp)
preserves the original element definitions and array path, then observes success
and failures at elements 1, 2 and 3. Its nine-value protocol keeps each result/live
pair and prior live count last; the checker uses explicit counts `9 3`. The
original source/object, public reports and native observations above remain
unchanged and describe the earlier five-value candidate.

| Additional evidence | SHA-256 |
| --- | --- |
| Prefix fixture source | `e7bb21fb57be030c1bab1023d08b0b788ae407f90b509b790a8ace48de4ae71c` |
| `capture/partial-array-prefixes.roslynobj` | `6a9ddfd3dab932b17d98f1d254c3def8b6081b67a3e8dc33988ff29a8ed360a5` |
| `capture/partial-array-prefixes-inspect.json` | `d352e9fcda8e5fe3527def39ab69f8f57da7b6d831e35885d155843bdb16b43f` |
| `partial-array-prefixes-facts-6.json` | `9cd3a315b9755c15a2f027452b6b5ad7887cf1eb20e0eee24b9b22e94b3a48fa` |
| `native-partial-array-prefixes-run-1/stdout.txt` | `c9725e72040b0049b779ca21435aaa9e3a8ccd7a3a9ddcdf59c8ed9a1fe542e1` |

The fresh report confirms the unchanged prefix object and one actual Throw 255
with one typed Try 65. The same selected C1/D1 definitions and helper argument
roles remain, with captured count 3 and stride 4. CleanupArray 222 still describes
the complete array. Its stack-offset handle names items 210 at offset 0 and has
no count locations. Static occurrence counts remain distinct from three dynamic
throws/payload lifecycles per fixture invocation.

The separate native oracle reports twice:

```json
{"result":144,"overflow":0,"live":0,"values":[123,0,7,0,7,0,7,0,0],"events":[101,201,102,202,103,203,601,612,623,303,302,301,900,101,501,707,901,101,201,102,502,301,707,902,101,201,102,202,103,503,302,301,707]}
```

Failure at element 1 has no completed element to destroy. Failure at element 2
destroys only element 1. Failure at element 3 destroys elements 2 then 1 before
the handler, providing a partial reverse-order observation that the original
one-element prefix could not establish. No failed element is reported as
completed or destroyed. All paths finish with zero live objects; the repeated
invocation also records prior live count zero and no event overflow.

The checker's Created/Committed/Selected/Caught/Released deltas are aggregate
counts per fixture invocation, expected to be three for this variant.
`LastSelectedSequence`, `LastFirstCleanupSequence` and `LastReleasedSequence`
describe only the most recently released owner, not separate sequence records
for all three. Runtime guards and the total selection-before-cleanup assertions
check every encoded site. The helper-unwind and catch-prelude sites provide two
ownership assertions per throw; those assertions are not destructor counts.
In particular, the zero-completed-element path still needs handler selection
before its unwind/handler transition despite having no element to destroy.

These native observations provide the exact comparison oracle for the verified
source-3 generated proof below; they do not independently establish translator
success or broaden the semantic helper contract.


## Verified generated checkpoint

Immutable `t5/array-source-3` freezes 205 implementation/configuration/fixture
files. The final [receipt](../acceptance/lifetimes/array-report.json), SHA-256
`1967d9cce8818d122798dc2a630f4a110bfbeb79f0086a3d916f3c28d0687897`,
records 26 expected outcomes: 22 successful phases and four intended unpublished
admission rejections. Production output is `t5/array-cli-generated-3`; all 12
required bodies and three semantic requirements are selected. Completion hashes,
nine pinned input hashes and source freeze pass; repeat source/project bytes are
identical and program reports differ only by output directory.

The vector constructor/destructor select versioned
`generated-cpp-vector-constructor-v1` / `generated-cpp-vector-destructor-v1`
contracts with provider `Generated.AutomaticArrayHelpers`. The exception-owner
provider remains `RoslynObj.CSharp.Runtime.ManagedCppException`. Candidates only
defer CLI admission; actual public FunctionSymbol identities and matching
contracts are required before publication. A genuine same-spelled ordinary
vector declaration/call input, with no cleanup context and ordinary parameter
roles, remains rejected. Runtime code is unchanged. Generated projects depend
only on Generated and Runtime; the checker adds only Checks. NativeAOT ELF
imports are host libc/libm/loader, not a native application/vector implementation.

Private generated Construct/Destroy methods borrow the actual automatic array.
They call selected C1/D1 directly with typed pointer views; pure callback casts
are resolved only in the jointly validated helper context. No native function
address or UCO callback is materialized. Base/count/stride execute once in the
recorded LTR order. Construct increments completed only after successful C1;
a false observer filter allows outer handler selection before finally asserts
that selection and reverse-destroys the completed prefix. Normal Destroy runs
reverse closed-noexcept D1 callbacks. No cookie, allocation or free is invented.

The stronger prefix fixture passes exact native nine-value/33-event observations
twice in JIT and explicitly clang-18 NativeAOT. Success destroys three elements;
failures at elements 1/2/3 destroy prefixes 0/1/2, including the observed order
2 then 1. Each invocation creates/commits/selects/catches/releases three payloads,
with zero live owners, pending releases or protocol errors and six selected-before-
cleanup assertions. Last sequence fields describe the final owner only:
10 < 11 < 12, then 22 < 23 < 24. The assertion count is not a destructor count.

Prior normal, integer-EH and constructor-member CLI/JIT/AOT regressions pass.
Their generated application sources are byte-identical to accepted publications;
the checker's original one-argument defaults remain exercised. Ordinary runtime-
name, ordinary vector-name, handler-new-throw and noexcept-wrapper negatives
all reject before publication. The original failure-2 array source/object/native
receipts remain unchanged and are not relabeled as this final prefix execution.

Earlier nullable-guard build failure, normal-dtor pointer-to-array admission
failure and a coordinator pre-snapshot path typo are preserved separately from
the 26 accepted phases. Source 3 compiles without warnings/errors. Memory sampling
and waited high-water evidence remain separately labeled in each phase receipt.

The full-array exceptional action is emitted and reviewed but not activated by
this fixture; normal full destruction and helper partial-prefix cleanup are the
actual executed paths. Heap/new[], multidimensional/nonconstant arrays, conditions,
bases, throwing destructors, rethrow/termination, class payloads/native unwinding
and broader T5/T6 forms remain open. No metadata recipe or source-derived count
supplies a second cleanup schedule.

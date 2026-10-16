# Mutable and const references to the same trivial object

Status: this bounded slice passed production CLI publication, complete
native/JIT/NativeAOT comparisons and physical layout checks. The
[verified receipt](../acceptance/lifetimes/mutable-reference-report.json),
SHA-256 `68c1ac008b02db50422f18d46337128750894c8c36479816a1d3300e94045cce`,
records all 28 expected outcomes: 24 successful phases and four intended
unsupported-case rejections, with no unexpected outcome or budget violation.
The original capture and runtime remain unchanged. Broader T5 remains open.

[mutable-reference.cpp](../tests/Fixtures/T5/mutable-reference.cpp) defines
trivial `Cell { int first; int second; }` and a separate explicitly noexcept
`mutate(Cell& dst, const Cell& src)`. Its actual source statements assign
`dst.first = src.first + 3` before assigning
`dst.second = src.first + src.second`, then record `&dst == &src`. Bounded
events after each assignment and the alias comparison expose their effects.
The retained public operations established the representation, and independent
native observations supplied the comparison oracle. Source predictions did
not supply the accepted result array or physical layout.

The fixture first calls the same helper with distinct source and destination
objects, records the first destination snapshot, then calls it with destination
as both arguments. The second call exercises a write through the mutable
reference followed by reads through the const reference to the same storage.
A copied const input would not preserve that relationship. No copy-in/copy-out
adapter, reference return, constructor/destructor, EH, native callback or native
provider is requested.

The existing native harness used `VALUE_COUNT=9`. The focused generated checker
compared both invocations of the scalar fixture protocol:

| Index | Observation |
| --- | --- |
| 0, 1 | Destination first/second after the distinct-object call |
| 2, 3 | Final original source first/second |
| 4 | Distinct-object call alias comparison |
| 5, 6 | Destination first/second after the same-object call |
| 7 | Same-object call alias comparison |
| 8 | Zero prior-live protocol slot |

`lifetime_live()` and observation 8 are explicitly zero protocol values. Cell
has no counted lifetime/resource units, so these values do not prove resource
ownership, destructor behavior or cleanup. The event capacity is 64 with
observable overflow. All result, value and event observations matched the
separate native oracle twice in both JIT and NativeAOT; raw addresses were not
compared across engines.

Capture review inspected actual parameter roles and reference/const type edges,
selected direct-call argument associations, local Cell representation, retained
assignment/read occurrences and alias comparison. Inventory identities are not
an evaluation schedule, and qualifiers do not authorize copying a reference
argument. Projection preserved actual storage identity and recorded sequencing
without reconstructing source overloads.

The helper and source entry are explicitly noexcept; this is a nonthrowing
prerequisite fixture only. The retained DuckDB roots have nullable
`NoexceptEffective` facts and are not admitted by this candidate. DuckDB
exceptions, string/base/link closure and general mutable-reference execution
remain separate work. Acceptance covers one registered generated helper and
its two actual direct-call occurrences only.

## Frozen capture 1 and native observations

Artifacts are relative to `/mnt/storage/roslynobj-csharp-20261015/t5/`.

| Evidence | SHA-256 |
| --- | --- |
| Fixture source | `4f932276c0347cf491bcdd2cf776141dd186734f882ec6138ac31bc36536f056` |
| `capture/mutable-reference.roslynobj` | `7a596885dfdcd674fd406c390a4aa6a0f0a8f39aa73dbd40cbe2ca47ed47d023` |
| `capture/mutable-reference-inspect.json` | `fbcaee900c67148c1636b07d885ab825d35ce4e089328b949b5def85f6c6e829` |
| `mutable-reference-facts-1.json` | `7725e285ad8005b57d2a66882038f75ac458c1159c4fa24b90209fb5798244a1` |
| `mutable-reference-capture-provenance-1.json` | `b2ab8fd1a27329e5fb91bb87ff484e02c17c1df16b6b81968cf65bca761bb013` |
| `native-mutable-reference-run-1/stdout.txt` | `4e1773de6058750204a2e283587c08aa5716aa3d2527b4f8cd735327060b5c04` |

The six serialized preparation phases passed. The report contains eight bodies,
472 public semantic identities and 270 structural relationship occurrences.
These are inventory counts, not execution order. Locators below are local
semantic-equality labels in that report. Source capture 1 also remains at
`mutable-reference-original-source-1.cpp`; it has not been replaced or recaptured.

The separate native executable reports this complete observation twice:

```json
{"result":54,"overflow":0,"live":0,"values":[8,12,5,7,0,11,23,1,0],"events":[108,212,300,111,223,301]}
```

The distinct call reports nonaliasing and preserves the original source. The
same-object call reports aliasing and the later second-field observation uses
the changed first field. These are finite observations of the actual input,
not a general noalias rule or an inferred source snapshot. Zero live counts
retain their protocol-only meaning.

## Actual selected reference types and caller storage

Defined helper `mutate`, entity 14, has signature 360 and explicit
`NoexceptEffective=true`. Its selected signature and execution parameter order
is ordinary parameter `dst` then ordinary parameter `src`, with descriptors
362/364 and actual parameter variables 31/32. It has no receiver, return slot,
variadic or passed-via-copy-constructor role. Its body has no local variables,
reachable lifetime associations, cleanup regions, tables, prologue or epilogue.
The fixture entry, entity 15, also has explicit noexcept and no EH metadata.

Mutable reference type 361 is an 8/8 lvalue-reference pointer to complete Cell
385. Const reference type 363 is also an 8/8 lvalue-reference pointer, but its
pointee is const typeref 425 whose underlying type is the identical Cell 385.
Actual lowered value types are ordinary nonreference pointers: 386 points to
Cell 385 and 395 points to const type 425. Cell has complete size/alignment 8/4,
with signed-32-bit fields first 401 at offset 0 and second 407 at offset 4.
No type name or size comparison supplies owner identity.

| Selected call | Raw operands and actual argument storage |
| --- | --- |
| 164 | Callee 165 targets helper 14; mutable argument 166 is address-of Reference 167 targeting destination variable 37; const argument 168 reads lowered pointer temporary 33 |
| 201 | Callee 202 targets the same helper 14; mutable argument 203 is address-of Reference 204 targeting destination variable 37; const argument 205 reads lowered pointer temporary 34 |

Both calls have `EvalLeftToRight=true`, direct target 14 and selected function
type 360. Argument ordinals 0/1 map to raw operand indices 1/2 and descriptors
362/364. Receiver/result/argument facets add no second evaluation.

The first call is the second operand of comma 158. Its first operand is actual
pointer assignment 159: temporary 33 receives address-of 161 of lvalue-adjust
162. That adjustment has const type 425 and its operand Reference 163 names
source variable 36. The second call is the second operand of comma 195, after
pointer assignment 196: temporary 34 receives address-of 198 of lvalue-adjust
199, whose Reference 200 names destination variable 37. The mutable argument
203 and this const-view chain therefore name the same actual local variable.
The temporary stores a pointer view, not a copied Cell. Neither qualifier
adjustment nor a local pointer temporary licenses a source-value snapshot.

## Actual store and read occurrences

Helper body block 62 retains these statement positions:

1. Assignment 64 writes field first 401 through mutable parameter 31. Its RHS
   add 68 reads the same field through const parameter 32 at node 69 and adds
   constant 3. The assignment records `EvalRightToLeft=true`.
2. Direct note Call 74 observes the first destination field.
3. Assignment 82 writes second field 407 through mutable parameter 31. Its RHS
   add 86 contains fresh reads 87 and 90 of first/second through const parameter
   32. This assignment also records `EvalRightToLeft=true`.
4. Direct note Call 94 observes the second destination field.
5. Assignment 102 stores the result of pointer equality 105 into last-alias
   variable 29. The equality compares const-view cast 106 of mutable parameter
   read 107 with const parameter read 108.
6. Direct note Call 110 observes the stored alias result, then return 115.

The first assignment is an earlier statement than the second assignment's fresh
loads. The retained sequence and shared actual storage must survive projection;
const does not imply immutability through other references. This audit does not
invent an order between add operands when the captured flags specify none.

## Preserved aggregate declarations

Capture 1 source uses aggregate declarations for the two trivial locals, rather
than separate source field stores. Actual execution Initializations 153/156 are
`dik_constant`, targeting source 36/destination 37 with type Cell 385 and
constants 423/424. They have no constructor, destructor, expression, extra
operands or lifetime association. `FollowsExecutableStatement` is true; the
other recorded initializer flags are false. This is separate from reference
argument ownership: neither declaration creates a const source snapshot for a
call. Restart review confirmed this shape fits the existing aggregate-constant
initialization path, which remains unchanged. No new initializer adapter,
source edit or recapture was needed for the accepted generated execution.

## Production CLI, JIT and NativeAOT proof

After the user resumed implementation on 2026-10-16, the coordinator froze
product snapshot `mutable-reference-source-1` and the separate nine-file
`mutable-reference-checker-source-1`. Their source manifests have SHA-256
`853afdd65bdf6da48154c160fd40b7f64eebbd9621a3bcf582b90de92e09888d`
and `04351a0b0d1d94ed55bafb5367e801dbe165e75b65f8cc64ad5e9e04ddc23fa5`.
The receipt verified both source freezes and all 16 recorded unchanged inputs.
Every runtime source hash remained unchanged from the frozen class-payload
checkpoint `192640cd9`; no runtime or provider adapter was added.

Production output `t5/mutable-reference-cli-generated-1` published all eight
required bodies with no required imports. Its actual `Translated.Cell` has
complete size 8/pack 4 and public fields `first` at offset 0 and `second` at
offset 4. The generated helper directly read the const source again after the
first write, preserving both qualified pointer temporaries and aliasing.
The [focused checker](../tests/MutableReferences/Program.cs) used these actual
names for typed physical measurements, with no reflection or name guessing.
JIT and NativeAOT both measured complete size 8, offsets 0/4 and natural
alignment 4 through a sequential byte-prefix probe, independently reported as:

```json
{"mutableReferencePhysical":{"cellSize":8,"firstOffset":0,"secondOffset":4,"sequentialPrefixOffset":4}}
```

Both modes also matched the entire two-run native result/value/event object
above. Zero live/prior-live values remained protocol observations, not ownership
or lifetime evidence. The generated dependency set contained only the checker,
Generated and the existing runtime; native application code was not a fallback.

The three affected earlier slices—normal const-reference lifetimes, integer EH
and the exact class payload—retained byte-identical generated application
sources and passed their JIT/NativeAOT comparisons. The four existing negatives
for ordinary runtime names, ordinary vector names, handler-new-throw and the
unadmitted noexcept wrapper still rejected before publication. Repeat
translation produced byte-identical generated source/project and equal reports
after output-directory normalization. No broader inheritance or SQLite rerun
was claimed.

This closes only the exact noexcept helper, complete two-int Cell representation
and two captured direct calls. Reference returns/rvalues, native callbacks,
arbitrary record profiles, potentially throwing mutable-reference helpers and
the observed DuckDB roots remain outside this acceptance. Broader T5/T6 and
DuckDB class/string/base/link/exception obligations remain open.

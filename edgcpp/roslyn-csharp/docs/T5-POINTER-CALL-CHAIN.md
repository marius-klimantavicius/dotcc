# Ordinary pointer calls and borrowed field reads

Status: production publication, native/JIT/NativeAOT whole-chain observations,
physical checks and affected regressions pass from frozen
`t5/pointer-call-chain-product-source-5` and checker snapshot 2. The
[receipt](../acceptance/lifetimes/pointer-call-chain-report.json) records 57 expected
outcomes: 50 ordinary successes and seven intentional unsupported rejections,
zero unexpected outcomes or budget violations. All 259 product files, nine
checker files and 25 input hashes verify. Runtime is unchanged. Broader T5 and
actual DuckDB execution remain open.

The genuine [fixture](../tests/Fixtures/T5/pointer-call-chain.cpp) preserves the
whole borrowed-member, Boolean conversion, pointer-result, guarded branch and
unsigned-byte reader chain. Two independent native live polymorphic inputs supply
kinds 5/201 and then 37/19. Generated checks use aligned 56-byte layout buffers,
not constructed C++ objects. Their opaque vptr/padding bytes and outer guards must
remain unchanged. The selected native class construction and virtual implementation
are outside translated entry reachability and are never forwarded to native code.

The public capture records 17 bodies. Owner is complete 32/8 with payload at 8;
PointerProxy is complete 24/8 with route flag at 16, while its unused occupied
partner is 17/8. Only the complete carrier is emitted. Nested PointerStorage and
DerivedStorage are 16/8, RefCountCarrier 8/8. Actual generated-base fields and the
captured zero-offset empty-base address/reverse receiver operations govern the
pointer path; no source-language layout reconstruction supplies storage.

The dedicated borrowed read view is complete 56/8 and exposes only the selected
unsigned-byte field at 24 plus a CLR alignment alias. It gives no class construction,
virtual dispatch, ownership, copy, native by-value ABI or general pointee access.
Already supported full virtual carriers take precedence over this view. Read-only
admission follows exact expression-plus-owner uses; captured writes remain
unsupported. A valid independent write fixture tests that boundary before publication.

Ordinary pointer-result methods retain actual direct call targets/signatures,
receiver aliases, executable reciprocal pointer initialization, branches and
fresh field reads. Two nested operator calls have both retained evaluation-order
flags false; their pure direct callee and single receiver are evaluated once
without normalizing the flags. Other Boolean-call admission remains unchanged.
Declared const pointer-object wrappers and pointer values stay distinct captured
facts. The fixture does not admit distinct pointer-result aliases, reference
escape, arbitrary ordinary bodies or caller cleanup omission.

Recorded `bfk_expect` is restricted to two ordinary signed64 arguments/result and
an effect-free constant integer hint. The typed call path plans the first argument
once, retains its effects, and projects its value; the hint has no emitted effect.
The production report labels this `generated-expression` with versioned
`constant-hint-expect-signed64-v1` contract, not a Runtime or host function.
Nonconstant/effectful hints and unsupported signatures remain guarded. Pristine
EDG result evidence is preserved in the separately attributed
[guard/builtin audit](DUCKDB-GUARD-DEFINITION.md).

Candidate representation collection reaches a fixed point before required-use
validation. Selected calls, signature and nested callback ABI roles, executable
initializers, constants/storage and containing constructor/destructor obligations
are reconciled before rendering permission. Native/indirect/address-exposed,
aggregate-by-value, captured pointer-slot and occupied-partner uses retain their
unsupported guards. Runtime, IR, Program, NoThrow cache and initialization semantics
are unchanged. Zero-live observations are fixture protocol, not ownership proof.

Initial compiler and admission failures remain separate artifacts. A coordinator
checker-path failure and a diagnostic-wording freeze mismatch are also preserved;
they are not reclassified as successful final acceptance. Earlier SQLite and C++
receipts retain their original frozen source/runtime attribution.

Both native/generated scenarios run twice and return 608 and 94, with all 32
values and 25 events equal. JIT/AOT physically measure complete sizes
`[32,24,16,16,8,56,1]`, prefix alignments `[8,8,8,8,8,8,1]`, matching array strides
and ten captured field/base offsets. Both actual borrowed pointers equal their
owner payload addresses at +8. All five pointer routes preserve
`[null,first,second,null]`; both Boolean bodies observe fresh values. The physical
argument/guard/missing deltas are 4/4/2. Full-width canaries, cookies, raw inputs
and surrounding guards remain unchanged; unsigned 201 is preserved.

Seven earlier profiles preserve generated source bytes and JIT/AOT observations;
SI/MI/Vbase re-emission preserves bytes without relabeling their original runtime
proofs. One existing pinned native callback fixture still returns 751 and its seven
events. Repeated current source/project bytes are deterministic. The genuine
read-only write rejects with CSHARP2502 before publication. Initial compilation,
wrapper/order admission, coordinator path and diagnostic-freeze attempts remain
separate from final acceptance.

The next approved step is [explicit managed-root selection](MANAGED-ROOTS.md),
starting with the actual five-node captured VectorBuffer getter. The current
fixture proves reusable prerequisites, not that actual DuckDB entry or full
VectorOperations/And/Or closure.

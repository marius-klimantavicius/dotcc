# Reference constexpr address-only cases, round 3

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Still an editing
snapshot; no acceptance or reviewer execution is claimed.

## Canonical findings

**RCE-03 — high: unknown-reference transport rejected before any load.**
The actual non-frame reference branch of ConstantStorage.ResolveReference
rejects every non-TU reference. Native accepts this exact definition-context
transport, as retained in
`native-reference-constexpr-unknown-reference-transport-contract-round1.json`:

```cpp
constexpr int ignore(const int&){return 1;}
constexpr int f(const int&r){static_assert(ignore(r)==1);return 1;}
static_assert(f(3)==1);
```

The source needs a symbolic unknown-object address (or an explicit, truthful
Unsupported boundary if that branch is not implemented). It must not invent an
integer value, copy a runtime referent, or treat the unknown identity as a known
static object. `interpret.c:32262ff` and `32608ff` explicitly produce unknown
addresses under `reference_to_unknown_object_allowed`, while a later read or
identity comparison still fails. The pinned C++20 profile's positive control
now establishes that this conditional behavior is active for the tested case.
Required negative load and positive transport/identity observation regressions
must accompany the repair; broader P2280/class/array/this/pointer support is not
implied.

**RCE-04 — high: initializing scalar location blocked before its value is read.**
The retained globals-slot branch currently stops whenever Initializing is true,
even when the slot already has a scalar Object cell. Both exact native controls
accept0: optional const initialization and required constexpr initialization:

```cpp
constexpr int ignore(const int&){return 1;}
const int value=ignore(value);
static_assert(value==1);
```

The second control changes `const int` to `constexpr int`. Receipts are
`native-reference-constexpr-initializing-address-transport-contract-round1.json`
and `native-reference-constexpr-initializing-required-address-contract-round1.json`.
Forming/passing the scalar location must work before initialization completes;
an actual scalar load must still fail. An uninitialized reference slot has no
known referent and remains a different failure. Exact positive identity and
negative self-load/cycle assertions are required. The repair must not create
recursive CLR evaluation or initialize mutable globals opportunistically.

RCE-01 remains disproved. RCE-02 now has an actual source repair: const scalar
initializers replay their existing typed graphs under in-progress slots instead
of requiring earlier successful optional evaluation. Its new tests inspect
late bodies, frozen selected-long entity and cycles; execution remains pending.

## Actual assertions inspected

The new seven ConstantReferenceTests groups inspect shared storage snapshots,
reference binding identity, mutable-address versus forbidden-load outcomes,
caller argument temporaries, recursive same-Symbol activations and fresh by-value
parameters, expired returns/locals, optional replay, short-circuit evaluation,
cleanup/reused budgets and dropped-graph collection with a live interpreter.
The shared combined source connects changed macro tokens, char16/long instances,
frozen const-reference helper lookup and required constexpr results. Session
disposal precedes the helper's returned graph queries. These authored assertions
are useful review evidence but are not executed proof yet.

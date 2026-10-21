# Reference constexpr unknown-object and repair review, round 4

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Editing snapshot,
not an executed acceptance. No implementation/test/native/Git execution.

RCE-02/03/04 now have source repairs and authored assertions pending execution:
optional const scalar recipes are deferred until a load, original typed selected
entities are reused, uninitialized scalar locations remain available for
address-only use, recursive value loads stop, and unknown reference parameters
produce symbolic locations that can be transported without manufacturing a
scalar value. Tests include exact positive transport, negative load and
definition-context versus invocation-context observations.

## RCE-05 — medium: truthful unknown-object lifetime metadata

The new Known=false cells have no public Identity and no scalar Value, which is
correct. They currently advertise Automatic lifetime and IsAlive=true, although
the unspecified referent might be a static, automatic or temporary object.
Public metadata must explicitly represent unknown/inapplicable lifetime and
liveness, or document the precise validity gate for those fields. Symbol names
the reference parameter in this case, not a known referent object. Require a
direct unknown metadata assertion. This is a graph-query finding, independent
of successful transport evaluation and its no-load rule.

## Disproved additional hypothesis, no new semantic finding

The exact required local alias control

```cpp
constexpr int outer(const int& parameter) {
  constexpr const int& alias=parameter;
  return 1;
}
static_assert(outer(3)==1);
```

rejects2 under the pinned profile. The diagnostic explicitly says a pointer or
reference to the parameter cannot be used as a constant. The initially assumed
acceptance receipt
`native-reference-constexpr-unknown-reference-constant-contract-round1.json`
is retained as a mismatch and excluded from pass counts. Thus the current root
result rejection for an unknown persistent reference is correct for this tested
case; RCE-03's positive definition-context transport does not imply persistent
unknown reference constants or complete P2280 support.

No further exploratory branch is required before the first held managed cycle.
All repairs, graph assertions, cleanup/limits and coupled source gate still
require supervisor-owned matching Release/JIT/NativeAOT/native evidence.

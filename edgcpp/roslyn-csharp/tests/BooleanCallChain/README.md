# Composite Boolean call-chain checker

The frozen source-2 CLI build and production publication passed. This checker
is bound to the actual published names; its JIT/NativeAOT execution and physical
acceptance remain pending the coordinator's measured run.

Build separately against the actual positive `GeneratedProject` and invoke:

```text
Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON
```

The expected inputs are `native-boolean-call-chain-run-1/stdout.txt` and
`boolean-call-chain-capture-provenance-1.json` under
`/mnt/storage/roslynobj-csharp-20261015/t5/`. The checker pins that positive
source/object/provenance, verifies the native file's recorded hash and compares
the entire generated two-run JSON with the separate native oracle. It checks
result, all 19 values, every event, repeat equality and protocol bounds. Native
observations currently report result 3, values
`[0,1,1,1,0,1,1,1,1,1,0,1,1,1,1,1,1,1,0]` and events
`[100,201,301,400,500,601,700,801,901]` twice.

Explicit JsonNode output supports JIT/NativeAOT without reflective DTO
serialization. The coordinator receipt must bind the actual GeneratedProject,
completion/input hash, build commands and checker binary. Output named
`suppliedCaptureProvenance` is supplied attribution, not independent proof of
what was compiled, and it never selects application behavior. Final output
is native-compatible JSON, `booleanCallChainPhysical`, then
`suppliedCaptureProvenance`.

The typed physical check allocates separate stack storage for two owners and
uses actual `Translated.Program.view`, `operator_bool(Wrapper*)` and
`operator_bool__2(Storage*)`. Both conversions return `sbyte`, checked as exact
0/1 rather than assumed to be C# bool. All record/field/method bindings come
from `boolean-call-chain-cli-generated-2/Generated.cs`:

- It measures complete Owner/Wrapper/DerivedStorage/Storage/RefCountCarrier
  sizes 24/16/16/16/8, sequential-prefix alignment 8 for each and matching array
  strides. Actual offsets are Owner.canary/payload 0/8, Wrapper.@internal 0,
  DerivedStorage.__b_7Storage 0, Storage.pointer/refcount 0/8 and
  RefCountCarrier.control 0. Actual stack element addresses must be 8-aligned.
- It fully initializes two owners through their real fields, including distinct
  full-width canaries and opposite-owner scalar-address control cookies.
  It invokes the actual getter on each, verifies exact payload pointers at offset 8
  and distinct owners, then preserves those borrowed pointers across mutations.
- It invokes both real Boolean conversion bodies on their corresponding complete
  record addresses for null, nonnull and cleared states. It checks fresh results
  through the borrowed views, the actual generated-base field address and
  alias/member address identity without patching bytes or duplicating a body.
- It compares pointer address bits with the original source scalar-address casts,
  keeps both inert control cookies and full-width canaries unchanged, and
  verifies natural alignment before using any address-only opaque view.

Physical `wrapperResults` and `storageResults` independently record exact
`[0,1,1,0,1]` for null first owner, nonnull other owner, nonnull first owner,
cleared first owner and unchanged other owner. The remaining physical JSON
records the five sizes/prefix offsets/strides, seven field/base offsets, actual
aligned storage/scalar addresses, two returned offsets, exact/distinct payload
pointers, alias/base identity, stable borrowed views and preserved scalar bits,
cookies and full-width canaries. These assertions run on the published methods;
no copied predicate, manually patched vptr or synthesized object body replaces
them.

No Opaque CLR carrier/layout is to be introduced, constructed or dereferenced.
There is no virtual execution, reference-count operation, ownership/lifetime
transfer, native callback, reader or native application fallback. Zero live
and the unused value 18 are harness protocol only. The independent genuine
P** pointer-slot source is an expected unsupported input requiring separate
admission proof; it has no generated checker path and is not accepted
because it shares the positive native JSON. Writing this checker does not claim
its execution or close that negative outcome.

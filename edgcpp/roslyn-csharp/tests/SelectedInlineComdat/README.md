This checker uses the actual names in `selected-inline-comdat-generated-2`,
whose generated C# is byte-identical to publication 1. It is frozen for the
coordinator's measured JIT/AOT runs; no outcome is claimed by this source.

Build with `GeneratedProject` pointing to that immutable publication and run with
`NATIVE_OBSERVATIONS_JSON PREPARATION_PROVENANCE_JSON`. Use the native output and
preparation provenance for the unchanged two-TU fixture. Provenance binds four
source identities, two captures, two public-facts reports and native output; the
outer receipt additionally binds the explicit manifest-order selection policy,
original candidates, chosen C1/D1 representatives, completion and executable.
Neither header equality nor these observations establish a general source ODR
validation claim. The default strong-only policy is outside this opt-in test.

The first JSON line exactly compares startup, two complete fixture observations,
shutdown and physical data. The second separates actual source object counters
from runtime registration counters; the third attributes supplied provenance.
There are two registered global D1 calls and four automatic scope D1 calls. The
automatic calls must finish before shutdown while runtime destructor starts
remain zero. The global registry then drains its two exact objects in reverse
order, sharing the same opaque diagnostic DSO identity. No constructor or
destructor is called manually, and no function-pointer/UCO path is added.

Both TU carriers are measured as size 4, field offset 0, sequential-prefix offset
4 and stride 4. Distinct aligned global objects retain their bytes through both
calls; literal addresses or padding are never compared across processes. Public
fixture calls before initialize and after shutdown must reject without changing
application storage; repeated initialize/shutdown must produce no further
application effect. After shutdown only independent scalar/event/value storage
and runtime diagnostics are read, never an ended object's member or guarded
source protocol root. Throwing startup/destruction, registration failure, TLS,
concurrency, restart and general COMDAT/weak/alias resolution remain outside the
finite positive evidence.

This checker uses genuine capture 3 and native observation 3 for the two-TU
`selected-inline-metadata` fixture, with actual names from
`selected-inline-metadata-generated-5/Generated.cs`. It is ready for measured
JIT/AOT runs; no outcome is claimed by this source.

Build with `GeneratedProject` set to that immutable publication; run with
`NATIVE_OBSERVATIONS_JSON ACCEPTANCE_PROVENANCE_JSON`. The additive provenance
must bind four sources, two inputs, two facts reports and native output while
preserving the original preparation file/hash. The outer receipt binds those
inputs to the actual table classifications, representative selection, generated
project, runtime and executable; supplied hashes alone do not identify code.
Use `native-selected-inline-metadata-run-3/stdout.txt` and
`selected-inline-metadata-preparation-3/acceptance-provenance.json`.

The first JSON line compares the complete native observations: both results,
all nine values, both full event arrays, Member counters, physical data and
counter-unit label. The next object separates actual per-run lifetime deltas
from metadata attribution. Only Member increments live/created/destroyed; Whole
has distinct construction/destruction events and a scalar-prefix check. Both
real source scopes finish before each fixture return. The constructor-argument
helper must run twice per fixture, with its native events 801/802 then 803/804
before the corresponding Member construction; the complete event arrays and
actual argument counter are both checked. No source constructor or
destructor is called manually, and no global Whole/startup lifecycle is added.

Physical bindings measure actual Whole__2/Member__2 and Whole__3/Member__3
carriers. Raw stack probes measure sizeof, scalar/nested-member addresses,
sequential-prefix alignment, array stride and independent field stores. They
invoke no application constructor or destructor and make no C++ object claim.
Native measurements are Whole 8/4 with prefix at 0/member at 4 and Member 4/4
with id at 0. No exception propagation, throwing cleanup, expanded NoThrow
permission, arbitrary inline ODR validation or native application fallback is
claimed. Retained table identities and cleanup recipes remain metadata rather
than an inferred execution schedule.

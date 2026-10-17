This checker is bound to the actual
`selected-constructor-boundary-generated-1/Generated.cs` publication. The two
genuine source TUs define the global objects and the selected complete default
constructor separately. The constructor has captured nullable noexcept metadata;
this fixture does not turn that metadata into a general nonthrow permission.

Build with `GeneratedProject` set to the frozen publication. Run with
`NATIVE_OBSERVATIONS_JSON PREPARATION_PROVENANCE_JSON`, using
`native-selected-constructor-run-1/stdout.txt` and
`selected-constructor-boundary-preparation-1/provenance.json`. The provenance
contains the four source hashes, two input hashes, two public-facts hashes and
native hash. The outer receipt must bind actual compiled sources, selected
inputs, completion, generated project, runtime and executable. Supplied hashes
alone do not identify the compiled program.

The first JSON line compares all four complete native snapshots and its physical
row. The next records runtime/physical observations, then supplied provenance.
Actual startup executes both cross-TU constructor calls with the emitted receiver
cast. The checker never calls a constructor or destructor manually. Both distinct
CLR carriers are physically measured: size 16, sequential-prefix offset 8,
canary/serial/kind offsets 0/8/12, stride 16 and aligned storage. Generated object
bytes are compared to the same objects' own snapshots, including padding; no
cross-process padding or literal-address equivalence is claimed. Both unsigned
201 fields, full-width signed64 canaries, serials and provider counter are read
while the program is Ready.

Public fixture calls before initialize and after shutdown must reject without
application effects. Initialize and shutdown are each repeated; constructor
objects, independent counters and protocol remain unchanged. This trivial-
destruction fixture must register and invoke zero destructors, with zero pending
entries. Post-shutdown inspection reads only independent scalar/array storage.
The live-zero slot is a harness constant, not ownership or destruction evidence.
This source is ready for measured JIT/AOT verification; it does not claim those
runs have passed. Real DuckDB string/EH closure remains a separate obligation.

This checker exercises one explicitly initialized generated program from the
unchanged `selected-global-startup.cpp` capture. Its independent native oracle
observes actual C++ global startup, two calls, and actual registered destruction;
the native observer never calls a fixture destructor manually.

`CapturedBindings.cs` uses the actual names in
`selected-global-startup-generated-4/Generated.cs`. That publication also
advertises the retained `note` routine; actual constructor/destructor calls use
its private body while the public wrapper remains guarded. The checker is ready
for measured JIT/AOT runs; no outcome is claimed by this source.

Build with `GeneratedProject` pointing to that publication. Run with positional
`NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON`, using the frozen native output
and `selected-startup-preparation-1/provenance.json`. The outer acceptance receipt
must bind the generated project, completion, checker sources, executable, runtime,
and actual commands; supplied provenance alone does not identify compiled code.

The first JSON line must exactly match all four native observations, including
the complete nine values and bounded event arrays. A second object records actual
runtime diagnostics and a third records supplied provenance. Explicit initialize
and shutdown are each called twice; application observations and registration /
destruction sequences must remain unchanged on the successful repeat. The two
actual object addresses, reverse retirement and exact stable DSO address are
checked without comparing process-specific pointer values to native addresses.
The private module slot is observed only through actual registration/retirement
diagnostics. While the globals are alive, typed physical checks measure the
4-byte `StartupItem`, its field offset 0, sequential-prefix alignment 4, stride 4,
distinct aligned global addresses and actual initialized ids.

Advertised source roots remain guarded. Calls to the public fixture before
initialize and after shutdown must be rejected, leave all application storage
unchanged, and increment only the runtime protocol-rejection counter (two total).
After shutdown the checker reads only published independent scalar/event/value
storage; it does not call a guarded protocol function or read a destroyed class
object's members. Registration
failure, throwing startup/destruction, TLS, concurrent calls, restart, implicit
CLR startup, process-exit integration and general failed-initialization recovery
are outside this positive fixture's evidence.

This checker is bound to actual publication
`selected-suppressed-provider-generated-2/Generated.cs` for the genuine
extern-template consumer and explicit-instantiation provider companion. It is
ready for measured execution; the source alone makes no passing claim.

Build with `GeneratedProject` naming the immutable generated project and, when
supplied, `RuntimeProject` naming its exact frozen Runtime project. Run with
`NATIVE_OBSERVATIONS_JSONL EXPECTED_NATIVE_SHA256`; the hash must be an explicit
lowercase SHA-256. The outer receipt must bind the native bytes, all four genuine
sources, both captured inputs/facts, selected provider policy, generated project
and executable. The supplied native hash alone does not identify compiled code.
Use `native-selected-suppressed-provider-run-2/stdout.txt` with exact SHA-256
`1f4fe3ea6986f61fa2d4c93956b4850a3f4ea0267abb5745d3809dd4ee8f0356`;
the checker requires that pin. Preparation 2's `provenance.json` preserves both
capture/facts identities and the original fixture-source snapshot. Consumer input
SHA-256 is `b1f58383ac9fd73c65ae25637833605016aa18fcbb103077c397a9734e06b080`;
provider input is `10e50debf4e5d087cbf29b7a316f6e8fc2e24ce8d9c8b8c67d1e8531b6970027`.

The first two JSON lines compare the entire two native observations: result,
nine values, all events, count, overflow, live, constructed and destroyed. The
fixture resets constructed/destroyed per call but preserves live, so balance is
checked separately for each invocation, with the actual prior-live slot retained.
Actual source scopes and direct scalar-field observations supply the lifetime
evidence; the checker invokes no constructor or destructor manually.

The bindings invoke only selected fixture/protocol entries and independently
read actual generated scalar counters. Both full native observations return 30,
with values `[7,1,11,2,19,1,2,2,0]`, events `[107,111,530,311,319]`, constructed
and destroyed 2/2, live/overflow zero. Physical checks measure the actual complete
consumer `SelectedProviderCell__2` and provider `SelectedProviderCell__3`: size 4,
value-field offset 0, sequential-prefix alignment 4 and array stride 4, with actual
field/address checks and independent scalar stores through raw CLR stack storage. Such
measurements invoke no application lifetime and are separate from the genuine
fixture scopes. Physical inspection must leave every application observation
unchanged. No exception propagation, native application fallback, arbitrary
suppressed-body admission, source ODR/body-equivalence or whole-DuckDB claim is
made. Explicit JsonNode output is safe for NativeAOT without reflective generic
serialization.

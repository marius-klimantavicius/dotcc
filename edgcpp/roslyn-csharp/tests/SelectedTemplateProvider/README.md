This checker is bound to the actual publication
`selected-template-provider-generated-1/Generated.cs` for the independent
out-of-class template consumer/provider fixture. Native preparation 1 and both
public facts reports are complete. It is ready for measured JIT/AOT execution;
the source alone makes no passing claim. Published Generated.cs SHA-256 is
`99647be440c57992e9cc5cd6c51a5463d355034676654bfb4abd4b97eb53bd37`.

Build with `GeneratedProject` naming the immutable publication and, when supplied,
`RuntimeProject` naming its exact frozen Runtime project. Run with
`NATIVE_OBSERVATIONS_JSONL EXPECTED_NATIVE_SHA256`. Use
`native-selected-template-provider-run-1/stdout.txt` and SHA-256
`1f4fe3ea6986f61fa2d4c93956b4850a3f4ea0267abb5745d3809dd4ee8f0356`,
which the checker requires. Its bytes happen to equal the earlier inline
companion's observations; the outer receipt must independently bind this
template fixture's sources, captures/facts, preparation, GeneratedProject and
executable. Native hash equality does not establish source or code identity.

Preparation 1's `provenance.json` has SHA-256
`3f2a5d8c8cb963094b455be4346f791629bf504a0739fcd6ac5475b333300b84`.
Consumer input SHA-256 is
`b36443eaad6607eac677d2f0f6ea2ad2111a2e0fac9fb8587ca374580d12775a`;
provider input is
`a691e150dfe95599bfd125eb0ebf10108c90efcee8d52ba99ab468c23e38b83a`.
Their public facts hashes are respectively
`6e9e91288d2e49b9ba5804bd5f2bb39eaaaeacd04b83258af4c4ce4d7dee9ba1`
and `59cc58ab1e7e8933bcc3b06f43f2a7563505106c09c496bfc9bff76e78a218cc`.

The first two output lines must equal the entire native observations: result 30,
nine values `[7,1,11,2,19,1,2,2,0]`, five events `[107,111,530,311,319]`, count,
overflow/live zero and constructed/destroyed 2/2. Constructed/destroyed reset per
fixture invocation, while live is preserved and checked through the actual
prior-live slot. Only the fixture executes the selected automatic C1/D1 calls;
the checker invokes no source constructor or destructor manually. Output uses
explicit JsonNode construction suitable for NativeAOT.

Public capture 1 establishes both instantiated complete records as size 4,
alignment 4 with the value field at 0; the dependent template pattern has no
usable complete size and is not a carrier. Physical bindings measure the actual
consumer `SelectedTemplateCell__2` and provider `SelectedTemplateCell__3`, field
addresses, sequential-prefix alignment 4, array stride 4, aligned addresses and
independent scalar stores. The protocol getters are compared to their actual
generated scalar counter storage. Raw CLR measurements are separate from actual source lifetimes
and must leave all application observations unchanged. No source ODR/body
equivalence, native application fallback, arbitrary template binding, exception
propagation or whole-DuckDB acceptance is claimed.

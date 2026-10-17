This checker uses the genuine two-TU constant-local-static capture and the actual
names in `selected-constant-static-generated-6/Generated.cs`, byte-identical to
publication 5. It is ready for measured JIT/AOT runs; no outcome is claimed here.

The invocation is `Checks NATIVE_JSON ACCEPTANCE_PROVENANCE_JSON`, with
`GeneratedProject` set at build time. Use `native-selected-constant-static-run-1`
and `selected-constant-static-preparation-1/acceptance-provenance.json`; the latter
adds exact facts and preserves the original preparation file/hash. The first
output compares both complete
64-value/event native observations and every storage-protocol field, including
the two full 50-byte sequences and terminators. Later objects record physical
observations and supplied provenance with explicit AOT-safe JsonNode writers.
The outer receipt must bind the actual selected inputs, constant recipes,
representative mappings, generated sources and executable.

The checker calls both TU wrappers, retains their returned pointer identities,
executes the fixture twice, then repeats the wrappers and freshly reads all 50
bytes from each returned address. The same named array must share one address
across TUs and calls. The two differently named arrays must remain distinct even
though their contents match. No storage is written. Physical checks use the
actual emitted `shared_bytes`/`different_bytes` properties. Both array carriers
measure size 50, first element at 0, sequential-prefix offset 1 and stride 50.
All four wrapper results must equal the corresponding canonical property address.

Address/content stability is observed; no synthetic source counter or dynamic
guard is added to claim a measured count for constant initialization. Actual
capture determines constant versus dynamic initialization, scope and ambient
obligations. The zero live slot is protocol only. General dynamic/promoted
storage, TLS, source ODR validation, writes to const storage, ownership and native
application fallback are outside this fixture's scope.

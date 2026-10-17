# Direct class exception construction

This checker compares two generated fixture invocations with the immutable
nine-value native oracle. The actual captured source directly constructs the
exception payload and contains no selected copy call. Native observations are
result 24, nine values and 30 events on each invocation; they establish the exact
comparison protocol. Source, input, public-facts and native hashes are checked
against the supplied capture provenance. The outer receipt must bind the actual
generated project, source/build and binary.

Each fixture invocation must allocate three direct-construction owners, commit
and catch two, retire one distinct failed pending allocation without a payload
destructor, invoke two completed-payload destructors and release all three
allocations. Aggregate counters, zero live/pending/reference resources and the
complete native trace cover these finite paths. Copy-owner and rethrow operations
must remain absent. Cleanup assertions must all observe prior selection.

The additive runtime's `Last*` fields describe the most recent completed owner
and most recent failed pending owner. They are not a history of every owner.
The checker labels the aggregate and latest-owner evidence separately and checks
that the failed pending owner differs from the completed/destructed/freed owner.
The process-wide peak pending-owner counter is likewise not a per-run counter.

The bindings use the actual completed `class-construction-cli-generated-2`
publication: `Translated.ConstructedPayload__2` and `Translated.Program` fixture
and `lifetime_*` methods. The physical check measures complete payload size 16,
canary/value offsets 0/8, sequential-prefix alignment 8 and array stride 16,
and checks actual stack addresses for 8-byte alignment. The captured occupied
extent 12/8 is not a standalone CLR carrier claim. No synthetic runtime or
corruption tests, native unwinding, arbitrary exception classes or allocation
failure matrix are added. The final bound checker has not been built or run.

Invocation after build:
`Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON`.

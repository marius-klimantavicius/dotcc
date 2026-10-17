# Typed expression readability acceptance

The coordinator runs `run-expression-readability.py --artifacts ABSOLUTE_NEW_DIR`
after shared translator edits are frozen. It captures the genuine C17 fixture
with the pinned existing producer, builds through the public serialized reader,
and compares all 30 observations and six sequenced events between an independent
native oracle, generated JIT execution and generated NativeAOT execution.
`--objects` can reuse immutable captures; the receipt records that export was not
reproduced. `--skip-aot` is explicitly incomplete.

Coverage includes nonassociative arithmetic, both floating addition groupings,
unary operations, narrowing, unsigned wraparound, pointer loads/addresses, target
shifts, count wraparound followed by checked conversion, short-circuit and
conditional branches, comma sequencing, a once-only increment and an ordinary for
loop whose header must stay structured. Invalid large
shift counts are tested only against the generated target policy.

Nineteen additional intrinsic-type renderer probes check exact grouping,
associativity, unary token separation and conservative opaque leaves. The probes
write independent C# expressions and compile/execute them with checked arithmetic
enabled, without a reader or emitter dependency in that execution project.

The measured receipt preserves phase logs, source/tool/binary hashes, repeated
emission equality, representative emitted bodies and dependency checks. This is
a bounded expression presentation/behavior milestone, not a broader language,
TLS, C++ or provider admission gate.

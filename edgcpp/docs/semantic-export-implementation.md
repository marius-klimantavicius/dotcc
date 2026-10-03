# Semantic exporter: first implemented checkpoint

Status: **isolated native exporter and managed reader implemented and verified**.
This checkpoint realizes the extraction boundary proposed in
[the investigation](semantic-export-investigation.md). It does not replace the
current C# emitter and does not claim a complete semantic adapter or codegen.

The [implementation and commands](../semantic-export/README.md) live in
`semantic-export/`. All native work occurs in an isolated frontend copy.
Canonical `cpfe-dot`, `backend/prepare.py`, `dotnet.py`, and `build/` are untouched.

## Actual exported packages

| Fixture | Graph nodes | JSON bytes | Original files | Macro invocations | Explicit adapter diagnostics |
| --- | ---: | ---: | ---: | ---: | ---: |
| [C source](../semantic-export/tests/plain.c) / [package](../artifacts/campaign/persistent-continuation/semantic-export-implementation-002/plain-003/package.json) | 289 | 188,076 | 2 | 1 | 17 |
| [C++ source](../semantic-export/tests/probe.cpp) / [package](../artifacts/campaign/persistent-continuation/semantic-export-implementation-002/probe-003/package.json) | 1,046 | 646,099 | 2 | 3 | 77 |

Both graphs have closed references and record actual effective flags:
`DO_IL_LOWERING=0`, `DOING_SOURCE_ANALYSIS=0`, source sequences, extra positions,
all template information, macro definitions, invocation trees and arguments.
Each package includes original input/header bytes, hashes, producer compilation
command/configuration, frontend/header hashes and executable identity.

The managed C# consumer verifies the following semantic relationships:

- C `sum`: named typed parameters, `Item.value`, structured `for`/`if`, original
  `continue` label marker, function source order and source locations.
- C++ `work`: the call selects **`choose(int)`**, not its long overload; another
  call selects the concrete **`twice<int>`** instance.
- The called `Box<int*>::read` belongs to the real nonprototype instance whose
  selected template definition is the **`Box<T*>` partial specialization**.
- `first`, `inner` and `conditional` RAII objects link to actual destructors,
  nested lifetimes and cleanup records; `try` and its handler remain structured.
- Public macro definitions, `#undef`/redefinition and nested invocations retain
  definition identity, argument text, expansion versus spelling locations and
  source bytes. A macro graph is available rather than only expanded tokens.

These are assertions over exported semantic records, not an IL-display parser
or source-text heuristic for selecting an overload.

## Verification evidence

The final frozen packages pass **20 CLI checks under JIT and the same 20 under
NativeAOT**. Each run includes 17 C++ semantic assertions and nine C assertions,
plus validation failures for malformed identities/references, package integrity,
macro ancestry/source contexts and unsupported schemas. A negative-parent macro
stack-pop placeholder is explicitly accepted in its valid form and rejected
when malformed. Both packages are correctly rejected by strict-emission mode.

- [JIT results](../artifacts/campaign/persistent-continuation/semantic-export-implementation-managed-001/final-jit/results.json)
- [NativeAOT results](../artifacts/campaign/persistent-continuation/semantic-export-implementation-managed-001/final-nativeaot/results.json)
- [Durable hash manifest](../semantic-export/evidence-first-milestone.json)

The reader builds and publishes with NativeAOT compatibility and warnings as
errors. Validation here is Linux x64/.NET 10; no claim of executed Windows/macOS
or Arm64 validation is made. The native exporter is a build tool, not a runtime
component shipped in translated applications.

## What remains

Every graph deliberately says `completeForEmission: false`. The native adapter
records named fields for the tested semantic structures but still reports
unimplemented metadata explicitly. In the C fixture these are pragmas and type
spelling/integer supplements; the C++ fixture additionally reports exception
specifications, dependent template supplements/parameters, hidden names and a
name qualifier. Generic `nativeEdges` make these records inspectable and check
reference closure; they are not a substitute for typed semantic adapters.

Further work should first complete required adapters for a small managed C
emitter, then use project requirements to grow coverage. Unknown semantics must
continue to block emission. Managed lifetime lowering, normal/exceptional
execution-trace equivalence, C++ code emission, the per-TU semantic linker,
macro API generation, and full project export/emission remain future work.
The checked destruction records prove extraction of cleanup facts, **not** a
finished managed C++ unwinder. Non-ASCII native text decoding and exact
byte-offset/trivia attachment also remain explicit fidelity work.

The accepted T3 memory and cursor slice follows the pinned SQLite object's
public blocker facts: 51 selected bodies plus one separately recorded
representation diagnostic, then the final four failing bodies. Descendant
graph inspection supplies inventory facts, never an execution schedule.

Pointer compound assignments admit the observed natural unsigned 8/16/32/64
and signed 32/64 offsets. Scaling uses the immediate selected element: the
fixtures distinguish byte stride 1, record stride 12 and pointer stride 8 for
`T**`. The exercised compound shifts use unsigned 32/64 values and signed 32/64
counts. Target helpers reject counts outside the selected width instead of
using C# count masking. Captured place and operand order remain explicit.

Volatile execution covers naturally aligned unsigned16, unsigned32, signed32
and binary64 storage on Linux x64. A recorded fetch emits a primitive volatile
load; a qualified target emits a primitive store. Address-only qualification
adds no load. Supported local `dik_constant`/`dik_expression` scalar
initializers evaluate once and emit a store. The width fixture checks two
initializer calls, two selected compound places, stored assignment results and
binary64 rounding at `2^53`. Integer64 volatile access, aggregates, bitfields,
atomic-qualified objects and unaligned addresses remain outside this slice.

Typed `va_arg` requires one retained cursor and a scalar prvalue. Its typed
node captures the actual cursor once and reuses the existing scalar SysV
adapter, including tag size/alignment 24/8 and field offsets 0/4/8/16. The new
fixture exercises signed32, unsigned32, binary64 and pointer results; selected
branches advance only their cursor path. Lvalue/address-preserving `va_arg`
and typed `va_start`/`va_copy`/`va_end` remain unsupported. Existing guarded
simple cursor operations retain their established path.

The last fixture also proves an ordinary generated call with an unqualified
actual value and a selected top-level volatile binary64 formal. Aggregate
signature classification inspects representation without performing an
access; the formal's body still emits its recorded volatile fetch. Native
admission and the general execution qualifier guard remain separate.

| Accepted receipt | Exact observations | Expected phase outcomes | Receipt SHA-256 |
| --- | --- | --- | --- |
| [Initial memory plans](../acceptance/memory-plans/report.json) | 24 plus events 1–4 | 21, including volatile64 rejection | `63e9f1caf9b066b062dd8f8a4298b483d43881f07e14d2ac506d598b186af970` |
| [Width expansion](../acceptance/memory-widths/report.json) | 27 | 23, including volatile64 rejection | `c4258b438a6afc7257dbc512611d3cc02fb68a9d08b94192506c5fb8a35c9d11` |
| [Typed cursor and qualified formal](../acceptance/last-four/report.json) | 13 plus events 1–6 | 22 | `ebe6a208e16a5c68f377b3f2df45a8a759acda5223875374cc4c793776c889f0` |

Each receipt records deterministic repeated emission and exact native/JIT/
NativeAOT output, with JIT and AOT checked twice. The last receipt uses frozen
snapshot 9 and verifies both source and input stability. The initial receipt
explicitly reuses frozen genuine objects; the later two reproduce their
exports. All record toolchain, source, input, build and generated hashes.
Native application bodies serve only as independent oracles. Generated
managed dependencies are Generated and Runtime; the checker adds Checks.
No reader or native SQLite application is linked into generated execution.

The bounded reproduction drivers are
[MemoryPlans](../tests/MemoryPlans/run-memory-plans.py),
[MemoryWidths](../tests/MemoryWidths/run-memory-widths.py) and
[LastFour](../tests/LastFour/run-last-four.py). They use the pinned genuine
producer, fresh measured phases and clang-18 for NativeAOT. These fixture
receipts establish neither full SQLite execution nor every provider contract.

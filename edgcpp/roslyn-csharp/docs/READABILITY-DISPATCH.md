# Retained scalar switch dispatch

The emitter uses a C# `switch` only to dispatch ordinary exact scalar cases to
the existing captured labels. The selector still executes its retained prefix
and is captured once. Each C# case and default section jumps outside the dispatch
scope; all original case markers, statement bodies, fallthrough and goto targets
are emitted as before. This does not reconstruct source switch scopes or locals.

The guard requires an ordinary 8/16/32/64-bit signed or unsigned integral selector
representation. Every nondefault case must be one exact retained integer value,
without a range, represented by an ordinary integer type and within the selector
range. Case values must be distinct. The emitted literal uses the selector's C#
storage view only after those exact numeric checks. Ranges and other constant
shapes continue to use the existing comparisons; unsupported forms keep their
existing diagnostic behavior.

Standalone operand-free integer/floating ConstantOperations are validated through
the normal exact literal projector, then omitted when they have no volatile
fetch. Existing void-discard traversal reaches these constants. No reads, stores,
calls, runtime conversions, arithmetic or branch effects are folded or removed.

Focused coordinator validation reuses the genuine T3 control-flow object and
`tests/ControlFlow`: exact event trace, side-effectful selector once, ordinary
fallthrough/default/no-default and the unchanged GNU case-range fallback. The new
`tests/Fixtures/T4/readability-dispatch/dispatch.c` uses the same observer API for
unsigned64 maximum, signed64 extrema, a byte selector, effectful comma-selector
prefixes, call discards and pure scalar discards. It can reuse the existing native
oracle harness at `tests/Fixtures/T3/control-flow/native-oracle.c` and the existing
C# checker without modifying either. Native observations establish exact values.

These changes require focused native/JIT/AOT checks and one complete immutable
SQLite regeneration/build/runtime receipt before acceptance. Prepared code alone
does not establish that evidence. No app source, EDG capture or runtime changes
are involved.

The focused gate now passes all 14 measured phases. The exact native/JIT/AOT
transcripts contain 21 dispatch events and 23 existing control-flow events (the
current cross-arm fixture adds events beyond its earlier 20-event version).
[Receipt](../acceptance/sqlite/readability-focused-report.json) pins frozen
snapshot 13, genuine source/object/producer, compiler identities and generated
completion hashes. The C# dispatcher has actual switch sections for extrema,
retains existing body labels/default/end and keeps GNU ranges on the prior path.
The checker invokes each fixture twice per process. Call discards remain;
standalone validated scalar constant discards disappear.

The complete source-13 CLI publication is currently running under the unchanged
1800-second / 8-GiB budget at `t4/readability-production-translate-1`, output
`t4/sqlite-readable-generated-1`. Full publication/build/runtime evidence is
still required before closing the full SQLite readability gate.

# Faithful manual EDG port

Execution resumed by explicit user instruction on 2026-10-21 under the parent
[PLAN](../PLAN.md) and [AGENTS](../AGENTS.md). This tree is separate from the
historical prototype. No inherited semantic implementation or acceptance.

Production code is handwritten in-process C#, organized by original EDG source
unit. Native oracle code is test-only. The library currently has no qualified
frontend entry point. The first integer snapshot has 18 independently qualified source-valid routine
contracts under the pinned native profile. Mask and sign extension have bounded
domain review only and remain excluded from full qualification. Held JIT/AOT and
native differential evidence complements the exact source review. Runtime dependencies are being translated in disjoint files. See coordination/STATE.md and
coordination/tasks.json for live assignments and continuation.

EDG-derived files retain Apache-2.0 WITH LLVM-exception attribution. The pinned
local source/configuration and exact reviewed snapshots belong in baseline/,
ledgers/ and reviews/. A passing test never substitutes for source parity review.

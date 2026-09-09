# Nested switch entries and compilation cost

C permits a `case` label inside a block, conditional, or loop. The C# backend
handles these entries with a dispatch switch and ordinary labels. It preserves
source fallthrough, switch and loop exits, outer-loop continuation, and the
original execution points of initializers. Only local storage whose scope must
be opened for an entry is hoisted.

Statements before the first case are preserved as an initial section without
case labels. Their declarations bind names and reserve storage, while dispatch
skips their executable code. A named goto can still reach a label in this
prelude. SQLite's `yy_reduce` depends on this for its `yylhsminor` union.

For fixed arrays in scopes opened to case entry, storage allocation is separate
from initialization: storage is reserved before dispatch, and initializer
stores remain at their original source points. Thus jumping past a declaration
can still assign and read array elements without executing a skipped
initializer. The `switch-prelude` native fixture covers scalar and
aggregate storage, multidimensional and aggregate arrays, direct nested entry,
and a goto back into the prelude. Variable-length arrays in affected flattened
scopes are explicitly unsupported; case jumps into their scope are rejected.

Subtrees without a case or named C label retain their structured blocks,
conditionals, and loops. Their declarations retain those scopes. `Seq` nodes
are always traversed because they are statement sequences rather than C
scopes. An explicit nested switch owns its own case labels.

This distinction matters for compilation as well as readable output. Flattening
all branches of SQLite's VDBE dispatch produced 1,234 labels, 1,643 goto edges,
and 319 hoisted local defaults. An eight-second CPU sample showed Roslyn
spending 99.97% of its busy thread in definite-assignment analysis, primarily
cloning and joining assignment states. That build was stopped after 5:14
without diagnostics.

Preserving unrelated structured regions reduced the VDBE method to 167 labels,
590 goto edges, and 48 hoisted defaults. The complete engine C# compile then
finished in 7.32 seconds and reported the next 170 semantic diagnostics. These
numbers describe this development machine and compiler snapshot, not a general
performance guarantee.

`SwitchStructureTests` builds an 80-case stress program with arrays, local
variables, branches, loop continuation, and an entry that skips an initializer.
Its GCC-verified output is `28320`, `18 0`, and `18 2`. The regression limits
unnecessary generated labels and then compiles and executes the result with a
60-second compiler cancellation deadline. The baseline emitted 2,564 labels;
the corrected stress and existing nested-switch runtime fixtures pass together
in approximately two seconds. Existing Duff's-device tests also pass.

Trace files and summarized stacks are under ignored
`sqlite/artifacts/compiler-profile/`; reduced native sources, before/after
logs, and actual engine timing are under `sqlite/artifacts/switch-structure/`.

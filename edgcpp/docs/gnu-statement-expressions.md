# GNU statement expressions

The C backend preserves `enk_statement` as `({ ... })`; its block ends with a
`stmk_stmt_expr_result` when a value is produced. The C# backend now emits a
typed helper for contained statement expressions. Referenced storage is passed
by pointer, including the caller's hoisted local variables and aggregate frame
slots. There is no managed closure or per-evaluation allocation, and branch
execution and operand evaluation remain at the original call site.

Accepted coverage includes nested expressions, scalar and aggregate results,
local initialization, loops, switch statements, internal labels/gotos,
short-circuit conditional execution and returned pointers. Ten original EDG C
and static/instance JIT/NativeAOT checks pass in
`artifacts/campaign/statement-expressions-translated-003/results.json`.
Public C11 atomic generic macros additionally pass eight checks in
`artifacts/campaign/libc-stdatomic-translated-20261003-063400/results.json`.

Outgoing return/goto, lifetime-bearing expression nodes and protected regions
are rejected until the control-flow/lifetime representation can preserve those
semantics across helpers. EDG itself rejects the tested statement-expression
VLA declaration. Four negative checks are retained in
`artifacts/campaign/statement-expressions-negative-002/results.json`.
These boundaries are unfinished language work, not a declaration that those
GNU source constructs are generally invalid. EDG lowering is unchanged.

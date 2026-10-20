# Incremental state/ownership review — 2026-10-20

Status: **changes required; no milestone approved**. Reviewed actual
`ParseState.cs`, `CompilationSession.cs`, `Tokens.cs`, and `SemanticGraph.cs`.
Parser/test harness and executed managed validation were not yet available.
Reviewer ran no builds, tests, native processes or Git mutations.

## R2 — High — abandoned checkpoints can restore detached semantic state

Affected code: `ParseState.Restore`, initially `ParseState.cs:51–64`.

The checkpoint validity check compares only `UndoCount` with the current undo
list length. An abandoned branch's checkpoint becomes accepted again after new
work brings the undo list back to the same length. This restores its old scope,
function, cursor and pending replay snapshot without restoring the matching
graph mutations.

Code-level reproduction:

```csharp
var outer = state.Checkpoint();
var scopeA = state.PushScope(ScopeKind.Block);
var branch = state.Checkpoint();
state.Restore(outer);
var scopeB = state.PushScope(ScopeKind.Block);
state.Restore(branch); // currently accepted: both undo counts are 1
```

`CurrentScope` becomes detached scopeA while `GlobalScope.Children` retains
scopeB. Later declarations can be inserted into a scope absent from the retained
graph. A stale diagnostic count can also exceed the current bag and cause a
range exception during restoration. The supervisor independently raised the
same reproduction while this review was inspecting it.

Acceptance criteria:

- A checkpoint identifies its actual branch/stack ancestry; matching operation
  counts cannot revive an abandoned descendant.
- A stale/cross-state checkpoint is rejected before mutating tokens,
  diagnostics, scopes, symbols, declarations or pending work.
- Tests cover abandoned descendants after divergent work, equal undo counts,
  diagnostic changes without semantic mutations, nested valid restoration,
  repeated restore of a still-valid checkpoint and cross-state rejection.
- Audit every parser mutation against the journal: direct changes to existing
  declaration ranges, initializers and bodies need restoration when a selected
  speculative algorithm can touch them. New nodes that become unreachable on
  rollback do not individually require undo merely for their own fields.

Status: **open**, sent to supervisor immediately. Parser usage and later fix
must be reviewed before this can close.

## R1 — assignment category — scope response pending code/test confirmation

`BinaryExpression` always exposes PRValue, which cannot represent C++ built-in
assignment. EDG `expr.c:process_simple_assignment` explicitly changes assignment
results to lvalues (~37261 onward). The implementation worker's response is that
all assignment operators are deliberately UnsupportedFeature in the initial
subset, while BinaryExpression represents arithmetic/comparison/bitwise forms.
That is an acceptable scope resolution if the parser and documented support
consistently enforce it, including `(x=1)=2`. No accepted assignment semantics
may use this representation. Full parser/tests review remains pending.

## Other inspected contracts

- Cursor checkpoints are cursor-owned and cross-cursor/default values reject;
  Advance/Peek preserve a stable EOF. The API explicitly promises lexical
  position restoration only.
- Retained token intervals receive a separate EOF and retain immutable source.
  Tokens are currently constructed from private owned arrays and exposed through
  read-only wrappers. Retain must not be advertised as template instantiation.
- Graph collections are read-only wrappers; parameter/argument/statement arrays
  are cloned. Internal setters do not expose public graph mutation.
- CompilationSession validates the selected language and null target. Analyze
  creates fresh source, diagnostics, token cursor and scope state for each call;
  immutable canonical types are session-owned. The lock serializes analysis and
  disposal. Completed results retain no mutable parser/session state that would
  obviously invalidate queries after disposal.
- Library and test project dependency files still show only their direct managed
  project reference; executed build/AOT validation is pending.

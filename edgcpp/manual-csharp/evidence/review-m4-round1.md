# Independent M4 review — first produced-code findings

Date: 2026-10-21. Source inspection of the worker's in-progress code, not an
integrated build or acceptance review. Reviewer executed no tests/compiler.
Findings were sent promptly to the supervisor; the stable candidate must be
independently rechecked after fixes and executed regressions.

## M4-R1 — High: dependent signature interning retains completed TU graphs

Confirmed reference chain in current `Types.cs`: the long-lived session owns
`TypeTable.functions`; `Function` appends signatures containing newly created
`TemplateParameterType` objects; those objects hold `ParameterScope`; scope
parent/declaration links reach the complete translation unit, template body
tokens/source, instances and bodies. Every new analysis creates a new template
parameter identity, so these signatures cannot match a previous analysis.

Reproduction contract: keep one `CompilationSession` alive, repeatedly analyze
`template<class T> T id(T x){return x;}`, discard each result, then collect. The
session's strong function-type list keeps every previous TU graph alive.
This is source-confirmed; the reviewer has not run a GC experiment.

Acceptance: dependent/scope-owning function canonicalization belongs to the
analysis/template owner, or an equivalent design must eliminate the session's
strong graph-retention path while preserving required within-owner identity.
Primitive/concrete immutable type interning can remain session-wide. Execute a
repeated-template analysis/isolation test plus a non-inlined WeakReference
collection regression with the session explicitly kept alive. Do not claim
template lifecycle coverage from primitive-only repeated analysis tests.

## M4-R2 — High: initial function parameters can redeclare template type parameters

The current `TemplateDeclaration` parses `FunctionParameters` before an
`activeTemplate` exists. `NewLocalSymbol` checks only
`activeTemplate.TypeParameters` for illegal parameter-name shadowing. Therefore
the initial parameter path has no matching check, even though the later body
replay path checks local declarations.

Concrete acceptance probes:

```cpp
template<class T> int f(int T) { return T; }
template<class T> int T(int value) { return value; }
```

These declarations illegally redeclare a template parameter. The first current
path creates an ordinary int parameter named T, then prototype analysis binds
the return to that ordinary parameter. The same-name template function entity
is inserted only after its template scope is popped, which also needs the
initial declaration-name check. Reviewer has not executed the in-progress code.

Acceptance: enforce the actual template parameter scope's redeclaration rule
during signature parsing, function-name creation and body parsing, including
nested local scopes. Preserve valid shadowing of unrelated ordinary outer names.
Add unused-template regressions and native controls.

## EDG branch requirement forwarded before static_assert implementation review

`decls.c:static_assert_declaration` (15468–15598; branch 15551–15574) implements
CWG2518/P2593R1: in the selected non-emulation strict profile, a nondependent
false assertion is not itself an error during function prototype instantiation.
It must still fail in a selected concrete instance. Invalid/nonconstant
conditions and malformed assertions remain definition-time errors as applicable.

The supervisor was asked to native-check unused `static_assert(false)` acceptance
and selected-instance rejection, and to record the exact source hash/routine.
This is an algorithmic requirement to inspect in produced code, not a confirmed
static_assert defect: that implementation was not yet present at inspection.

## Oracle adapter inspection

Reviewed actual `tools/oracle.py`, `tools/test-oracle.py`,
`host_envir.c:exit_compilation` (2528–2572), and non-VMS return-code definitions
in `host_envir.h` (172–180). Matching normal 0 / language-error 2 is correct for
this selected Linux oracle profile. Catastrophe 4, command-line/internal failure,
signals and arbitrary nonzero statuses cannot count as negative language passes.
The CLI restricts expectation values. The test controls cover normal/error,
catastrophe and arbitrary/signalled statuses. No adapter finding identified;
supervisor-executed unittest evidence remains to be reviewed at the final gate.

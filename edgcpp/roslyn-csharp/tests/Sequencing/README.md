# Focused sequencing checks

`Probe.csproj` depends only on the public RoslynObj reader. Run its executable
with one genuine object path to record retained operand constraints, exact call
associations, copy sizes, temporary recipes and bitfield coordinates. Its
`DescendantOperations` walks are inventory only, never an execution schedule.

`GeneratedChecks.csproj` references a generated application through
`-p:GeneratedProject=/absolute/path/Generated.csproj`. It has no reader reference.
Run it with `branches`, `places`, `callee`, or `aggregate-snapshot`; it executes
the generated application twice, checks every expected event and observed value,
and prints the same observation format as the independent native oracle. Run
both JIT and NativeAOT versions before recording acceptance. Application bodies
and native oracle objects never belong in generated runtime dependencies.

The original `aggregate` candidate is explicitly rejected by this checker. Its
retained EDG comma/copy roots disagree with the independent GCC result, and that
disagreement is unresolved. See `../Fixtures/T2/sequencing/README.md` for the
preserved evidence and separate explicit snapshot candidate.

These checks are focused single-TU candidates. They establish neither multi-TU
generated execution nor bounded-scale translation or SQLite execution.

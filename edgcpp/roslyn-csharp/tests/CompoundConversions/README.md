This focused C17 fixture activates the mixed integer compound assignments and
conditional record assignment found when compiling the unchanged SQLite
candidate. The independent native oracle and generated checker must produce
the same 22 observations.

The observations cover signed64 computation with unsigned64 operands,
signed32/unsigned32 destinations with retained signed64 or unsigned64
computation, signed32 with unsigned32 computation, unsigned8 narrowing, and a
value-used compound whose selected place and RHS each run once. Two conditional
record assignments select opposite branches; each source selector runs once,
the destination selector runs twice, and the record remains size 16/alignment 8.
Existing signed16/int32 and unsigned8/int32 helper cases remain exercised.

The signed out-of-range stores use the selected Linux x64 GNU conversion
profile. The high-bit cases compute in unsigned representations before the
recorded destination conversion; the native oracle confirms their exact
values for this profile. This fixture does not add arithmetic rules inferred
from source syntax or C# overload resolution.

The coordinator exports
[operations.c](../Fixtures/T3/compound-conversions/operations.c) with the frozen
producer using `--gcc --c17 --gnu_version 130300`. It builds the independent
oracle with `gcc -std=c17` and
[native-oracle.c](../Fixtures/T3/compound-conversions/native-oracle.c), then
emits through the frozen Projection harness. Build [Checks.csproj](Checks.csproj)
with `-p:GeneratedProject=/absolute/path/Generated.csproj` for JIT, and publish
the same checker for `linux-x64` with NativeAOT and clang-18. The oracle is
never linked into generated execution.

See [the compiler-fix contract](../../docs/T3-COMPILER-FIXES.md) for the public
facts, stock computation rule, and bounded scope. The coordinator records the
measured phase commands and hashes; this checker is an activation proof, not a
full SQLite execution claim.

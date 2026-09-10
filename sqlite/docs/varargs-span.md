# Span varargs (M10)

Generated variadic methods use C# 14 params collections on .NET 10:

```csharp
public static int sum(int count, params ReadOnlySpan<VaArg> _va);
```

Expanded arguments, an empty tail, existing `VaArg[]`, `Span<VaArg>`,
`ReadOnlySpan<VaArg>` and stack-allocated packs are accepted. Arrays remain an
explicit caller-owned backing option for large or reused packs. No array is
created by `VaList`; it holds a readonly borrowed span and a mutable cursor.
`VaArg` remains a readonly value type with the existing integer/pointer and
floating representation. These changes avoid boxing and remove the temporary
array from measured expanded calls.

This is a managed binary signature change. Re-emit C sources and
rebuild libraries and consumers together. Regenerate object fragments from C
source with the updated compiler. Previously compiled array-signature
assemblies are not binary compatible. Ordinary recompiled direct calls, including
calls passing an existing array, retain their source syntax. C# method-group or
function-pointer signatures explicitly naming `VaArg[]` must change to the span.
The compiler's generated projects already select the required language version.

## Borrowing and C semantics

`va_start` creates a cursor over exactly the supplied tail. `va_arg` advances that
cursor. Assignment, by-value forwarding and `va_copy` copy the cursor position;
subsequent advances are independent, while the backing storage is shared.
`va_end` remains a no-op and does not own or release the backing storage. A new
`va_start` restarts at the beginning. C default promotions remain explicit:
narrow integers and small enums become `int`, `float` becomes `double`, and
pointer/function-pointer arguments use the existing `void*` carrier. Emitted
argument expressions are evaluated once, in their existing left-to-right order.

Cursors started from a method's own pack, and aliases derived from those cursors,
are emitted with `scoped` declarations/parameters. Incoming borrowed cursor
parameters may be forwarded or returned when their borrow remains valid. A return
of a locally started cursor is rejected. The analysis conservatively tracks the
whole function: overwriting such a cursor later with an incoming borrow does not
make it returnable again. Validation occurs before unsafe C# can
turn a ref-escape failure into a warning.

Not every valid C storage pattern has a managed ref-struct equivalent. dotcc
reports `C# va_list lifetime` diagnostics for cursor fields, globals, arrays,
pointers/address-taking, unsupported comma-expression tuple/delegate storage or
capture, cursor values packed into `...`, and returns escaping a local pack.
`sizeof`, `_Alignof` and `offsetof` cannot treat a cursor as an unmanaged layout.
Callers needing persistent argument data retain an owned `VaArg[]` and create a
fresh borrowed cursor when needed; there is no automatic heap cursor wrapper.

The application inventory includes SQLite configuration/printf forwarding and
its callback tables, Lua formatting forwarders, and Chibi's variadic wrapper.
dotcc libc's existing fluent `PrintfBuilder` uses its own representation and
requires no `VaArg[]` conversion; formatted output may still allocate strings.
The zero-allocation benchmark concerns argument packs and cursor operations,
not all work performed by variadic functions.

## Managed callbacks

Variadic function pointers use a final explicit span parameter, since `delegate*`
does not carry a `params` modifier. For a C `int sum(int count, ...)`:

```csharp
private static readonly unsafe delegate*<int, ReadOnlySpan<VaArg>, int> Sum =
    DotCcFunctionPointers.sum;
// Inside an unsafe method:
int result = Sum(3, [1, 2, 3]);
```

The translator packs indirect C call tails with the same default promotions as
direct calls. Source and object-link paths, external C# callbacks, callback
arguments and aggregate callback fields are covered. Canonical static readonly
address fields include variadic definitions. Functions taking `VaList` by value
also remain valid managed callbacks. Native C export wrappers omit variadic
methods and direct `VaList` signatures; these borrowed managed APIs have no native
ABI. SQLite extensions continue to be explicitly registered C# code.

## Repeatable checks

Run `scripts/test-varargs-span.sh` from `sqlite/` after building dotcc. It emits a
fresh C fixture, builds a separate managed consumer and checks JIT plus linux-x64
NativeAOT. `SQLITE_AOT=0` selects JIT only. `scripts/verify.sh --with-ports` includes
this gate with the full SQLite and shared-port regression campaign. Timing is
reported without pass/fail thresholds; allocation counts and results are checked.

The fixture covers 0, 1, 8 and 64 expanded arguments from C# and translated C;
explicit arrays/spans/stack spans; forwarding and copying; variadic callbacks;
and scalar/pointer/function-pointer promotions. Each of 15 cases warms for 4,096
calls, then measures 20,000 calls with `GC.GetAllocatedBytesForCurrentThread`.
A non-inlined `params VaArg[]` sum provides an allocating array baseline. Results
and elapsed times are in `artifacts/varargs-span-{jit,aot}.out`; compiler, build and
publish logs use the same prefix. The baseline isolates pack allocation; it is
not a benchmark of the entire previous SQLite implementation.

The 64-argument case exercises a larger inline pack (64 × 16-byte carriers =
1,024 bytes of payload before other frame storage). This is bounded coverage,
not proof of stack safety at arbitrary arity or recursion depth. An explicitly
owned reusable array avoids repeatedly materializing a large expanded pack.
Measured allocation improvements do not imply a throughput improvement; timing
also includes generated cursor access, checks and runtime optimization effects.
Current run results are recorded in [validation.md](validation.md).

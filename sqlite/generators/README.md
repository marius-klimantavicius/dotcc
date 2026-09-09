# dotcc offsetof generation

`DotCC.OffsetGenerator` is a Roslyn incremental generator targeting
`netstandard2.0`, independent of the repository's application target. The compiler
and generator compile the same `Shared/OffsetLayout.cs`; the compiler does not
reference Roslyn. There is no native interop, runtime reflection, dynamic code,
null-pointer dereference, or delegate allocation in offset evaluation.

The campaign's `scripts/layout-native.sh` extracts all active offsetof requests
from full preprocessed SQLite and generates native checks without storing any
offset values in the recipe. The current profile has 30 distinct requests;
their native sizes, alignments, constant offsets, and address differences are
recorded in `tests/layout-native.expected`. The same probe is ready for the
translated compiler once full lowering succeeds.

The compiler resolves integer-constant contexts before C# generation, recording
those requests as well as runtime expression requests. Each request produces an
`internal static class __DotccOffset_<UTF8 hex identity>` with `const ulong Value`
and `const int Size`/`Alignment`. The generator recomputes every requested offset
from the typed storage description and rejects a mismatch with the compiler's
constant (`DOTCCOFF001`). Names use the canonical aggregate and normalized member
path, without timestamps, random identifiers, or process-dependent hashes.

## Contract

Generated C# carries a structured comment between `/* dotcc-layout-v1` and
`end-dotcc-layout */`. It is an equivalent structured input to `AdditionalFiles`,
kept in source so object fragments and linked programs retain their own metadata.
Records are tab-separated, with free-text fields encoded as UTF-8 base64:

* `abi\tlp64-le-dotcc-v1`: 64-bit pointers/function pointers, LP64 arithmetic,
  little endian, natural sequential storage, explicit unions, optional byte pack.
* `aggregate\t<name>\t<union 0/1>\t<packed 0/1>` introduces an aggregate.
* `field\t<name>\t<type>\t<bit width or ->` preserves field/storage order.
* `request\t<generated identifier>\t<aggregate>\t<path>\t<compiler offset>`
  requests a constant. A normalized path is dot-separated; `[N]` is a constant
  array index segment, and anonymous promoted members include their actual
  generated container fields.

Types are recursively described as `p:<size>:<alignment>`, `n:<aggregate>`, and
`a:<count>:<element type>`. Pointer and function-pointer storage is `p:8:8`.
Typedefs and enum underlying types are resolved by typed IR. Fixed buffers and
inline-array wrappers share the same array descriptor. Only aggregates reachable
from a request are serialized. Unsupported/incomplete storage, invalid requests,
bit-field addresses, overflow, invalid input, and compiler/generator disagreement
produce diagnostics, never guessed offsets.

Bitfield storage follows the backing units emitted by dotcc: adjacent fields of
the same declared size share a unit until full; zero-width members start a new
unit. This fixes the old layout calculator counting each bitfield as a separate
integer. It does **not** establish compatibility with every host compiler's
bitfield ABI. Native comparison is required for each relied-on shape.

## Integration

Standalone files and ordinary generated projects contain materialized declarations
from the exact same implementation under `#if !DOTCC_OFFSET_GENERATOR`. They need
no hidden analyzer installation. Object fragments preserve aggregate declarations
and each self-contained offset contract; the linker deduplicates them by identity.

To execute the incremental generator in an emitted project, build this project,
add its DLL as an MSBuild `Analyzer`, and define `DOTCC_OFFSET_GENERATOR`.
`Compiler.BuildGeneratedCsproj(offsetGeneratorAssembly: <absolute DLL path>)`
writes both settings. The analyzer has no non-Roslyn assembly dependencies.
`DotCC.FunctionalTests/FixtureRunner.cs` always defines that symbol and runs
`GeneratorDriver` before emitting its in-process test assembly. Generator tests
live in `sqlite/tests/OffsetGeneratorTests.cs` and are linked into that project.

## Validation and remaining audit

The `offsetof-generated` C fixture compares emitted constants and actual storage
address differences for nested structs/unions, an inline array, a function-pointer
field, adjacent bitfields, anonymous promotion, and enum/array/case/static-assert
contexts. Its checked-in output was verified using the host LP64 `cc -std=c17`.
Release solution build and 10 focused OffsetofTests passed. The focused functional
run passed all generator tests and all offset fixtures, including the new native
layout fixture; the independent callback-return fixture still exposed a compiler
argument-conversion blocker. The full amalgamation retry advanced to the next
parser error at preprocessed line 7990. See `artifacts/offsetof/` for these local
logs. Generator tests cover determinism, malformed input, compiler mismatch diagnostics,
unknown layouts, bitfield address rejection, and object/link compilation.

M2 is not complete until the campaign records the full SQLite retry, complete
suite results, NativeAOT validation, and SQLite-specific layout comparisons.
Flexible arrays now use count-zero metadata. Their header has explicit size and
field offsets, an overlapping scalar alignment anchor, and a pointer property for
the tail; there is no phantom element in storage. Generator constants supply the
header's `StructLayout.Size`/`Pack` and tail pointer offset. The
`flexible-array-layout` fixture compares native and translated sizes, alignment,
offsets, actual addresses and overallocated access for scalar, aggregate, pointer
and function-pointer tails, including a double tail following integer bitfields.

General GCC/MSVC bitfield ABI differences and >8-byte primitive alignment remain
an explicit audit item. In particular, GCC may place a char tail directly after
the used bits of an integer bitfield, while dotcc's current backing-unit model
places it after the complete storage unit; that shape is not yet claimed to match
GCC. Empty flexible headers and tail-header alignments beyond 8 bytes produce a
clear unsupported-storage diagnostic.

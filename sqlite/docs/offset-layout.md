# Offset calculation and direct C# emission

dotcc computes aggregate layouts and emits offset constants directly. The layout
model lives in `DotCC.Lib/Layout/OffsetLayout.cs` and has no Roslyn dependency.
There is no separate offset source generator, analyzer DLL, or conditional
compilation switch in the generated program/project.

C integer-constant expressions are resolved in the typed binder. C# emission uses
the same model and checks its result against the folded value. Each requested
offset gets a deterministic `internal static class __DotccOffset_<UTF8 hex identity>`
with `const ulong Value` and `const int Size`/`Alignment`. This supports array
bounds, enums, case labels, static assertions, ordinary expressions, and flexible
header layout attributes. Source, standalone-file, managed-library, and object-link
output all contain the declarations directly.

## Layout and metadata

The LP64 model describes actual dotcc storage: 64-bit pointers/function pointers,
resolved primitive and enum widths, sequential structs, explicit unions, optional
byte packing, nested/anonymous aggregates, inline arrays, and bit-field backing
units. Pointer inline-array cells are single-field unmanaged wrappers, so their
size/alignment remains pointer-sized. Flexible tails have zero storage; explicit
header size/alignment and a tail pointer accessor use the same emitted constants.
Invalid/incomplete layouts, invalid designators, bit-field addresses, arithmetic
overflow, and disagreement with folded constants fail explicitly.

Generated source retains `/* dotcc-layout-v1 ... end-dotcc-layout */` metadata for
native layout audits and object-fragment identity. Tab-separated records describe
`abi`, `aggregate`, `field`, and `request`; names/paths are UTF-8 base64 encoded.
Type descriptions are `p:<size>:<alignment>`, `n:<aggregate>`, or
`a:<count>:<element type>`. Only aggregates reachable from a request are included.
This metadata is not an instruction to run an analyzer during the C# build.

## Verification

`DotCC.FunctionalTests/OffsetLayoutTests.cs` verifies direct emission and metadata
determinism, invalid metadata/layouts, folded-value consistency, and ordinary
file/project/object-link compilation without a source generator. Existing offset
and flexible-tail C fixtures compare constants with actual unsafe storage and
cover nested/indexed designators, bitfields, callback fields, arrays, enums,
case labels and static assertions.

SQLite's native probe extracts all 30 active offset requests from the pinned
amalgamation. The translated probe compares those results, actual sizes/alignment
for 33 aggregates, and eight pointer-array layouts under JIT and linux-x64
NativeAOT. See [layout checks](layout-storage.md) and [validation](validation.md).
These execution checks independently verify the model against actual storage;
recomputing constants with the same model alone is not independent validation.

The supported native comparison uses unsigned plain char and GCC `-mms-bitfields`
to match dotcc's bit-field backing units. Default GCC's distinct bit-field ABI is
recorded separately. Empty flexible headers and header alignment beyond eight
bytes remain explicitly unsupported.

The original campaign used an optional Roslyn offset generator at the user's
request. It was removed on 2026-09-10 after confirming direct emission supplies
all required constants. Earlier validation entries document that historical path.

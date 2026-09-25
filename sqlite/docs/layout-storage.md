# Actual translated layout checks

Run `scripts/layout-native.sh` to compute the pinned native oracle and
`scripts/test-layout-translated.sh` to compare translated execution. Set
`SQLITE_AOT=1` on the latter command to run the same checks under linux-x64
NativeAOT as well as the JIT. Both use the configured LP64, unsigned-char,
GNU/System V bit-field storage profile. The checked oracle is
`tests/layout-native.expected`.

For simple bit-field prefix or tail reuse, dotcc narrows the private backing
integer to `byte`, `ushort` or `uint` when it covers all fields sharing the unit without
touching an ordinary member. Unused prefix bytes are excluded by moving the
backing field to the group's first occupied byte and rebasing its shifts. For
example, `WhereInfo.__bf0` uses a byte at offset 68 for its six flags; `nRowOut`
remains at offset 70. Accessors use ordinary
integer masks and shifts. Explicit aggregate size, alignment and member offsets
remain unchanged. Scalar backing fields must use their natural alignment.
Groups that cannot use a single safe integer are split across aligned
`byte`, `ushort`, `uint` or `ulong` backing fields. Accessors read and update
those fields directly; only a bitfield crossing a storage boundary combines
multiple integers. No bitfield accessor uses byte pointers.

For example, `__Anon13` uses byte backing fields at offsets 1 and 2, preserving
`sortFlags` at offset 0. `SrcItem.fg` uses a byte at offset 1 and a ushort at
offset 2. Ordinary members and zero-width bitfields separate independent
storage groups. `sqlite3InitInfo`, `Index` and `Parse` keep their rebased byte
backing fields at offsets 6, 99 and 39 respectively.

SQLite's `uSrc.fromSpace` buffer (emitted in `__Anon40`) uses
`SZ_SRCLIST_1 = offsetof(SrcList, a) + sizeof(SrcItem)`. Native probes of the same
source/configuration give 8 + 80 = 88 bytes with MS bit-field packing and
8 + 72 = 80 bytes with GNU packing. `SrcItem.fg` shrinks from 8 to 4 bytes;
the resulting offsets also remove padding before `colUsed`. This size change
comes from the GNU ABI migration, not from narrowing or aligning backing fields.

`tests/layout_probe.c` includes the unchanged amalgamation and tests all 39 active
`offsetof` requests extracted from its preprocessed source. It compares native
and translated aggregate sizes, alignment constants, generated offsets, and
actual member address differences. Eight additional pointer-array probes cover
`FuncDefHash.a`, `JsonCache.a`, `FKey.apTrigger`, `MemPage.apOvfl`,
`BtCursor.apPage`, `CellArray.apEnd`, `PragmaVtabCursor.azArg`, and
`WhereLoop.aLTermSpace`. Their transcript includes enclosing size/alignment,
array and element sizes, counts, first/last member addresses, and successful
independent writes and reads of the first and last pointers.

C `_Alignof` becomes a compiler constant, so comparing that column alone does
not measure CLR alignment. `scripts/generate-layout-storage-checks.py` therefore
creates an additional C# test source file, `LayoutStorageChecks.cs`, in the emitted
project. For every aggregate in the native transcript, its module initializer
constructs a `{ byte Prefix; T Value; }` wrapper and compares the actual address
difference to native alignment. It also checks actual `sizeof(T)` and the sizes
of all eight emitted pointer-element inline-array wrapper types. These startup
checks run before any probe writes, under both JIT and AOT, and fail by exception
on a mismatch. There are currently 42 aggregate checks and eight array checks.

The sidecar contains no reflection or generic pointer arguments. It does not
edit `DotCcProgram.cs`, implement missing storage, or change the engine. Expected
values come from native execution, not a handwritten layout table. SQLite's
`Mem` typedef is explicitly mapped to its emitted `sqlite3_value` tag. C# test
wrappers allow testing flexible-array headers without introducing nonstandard
nested flexible-array structures into the C probe.

Pointer and function-pointer inline arrays now use one-field unmanaged element
wrappers, enabling normal C# indexing and generic spans through `element.Value`.
Their layout remains pointer-sized; CS9184 is eliminated rather than suppressed.

Native, translated JIT, and linux-x64 NativeAOT validation of the FTS5-enabled
profile passes with explicit `SQLITE_MAX_MMAP_SIZE=0`. All 39 active offsets,
42 actual aggregate sizes/alignments and eight pointer-array storage checks
match. Native, JIT and AOT transcripts are byte-identical. The final clean
implementation snapshot is `3d4dbc0`; evidence is under
`artifacts/fts5-clean-checkout/sqlite/artifacts/clean-layout.log` and that
checkout's `translated-layout*.out` / `translated-layout*-build.log` files.

The first translated run exposed two incorrect addresses, for `Parse.aTempReg`
and `WalIndexHdr.aCksum`. Their sizes and offset constants were correct, but the
backend emitted `&object.fixedArray`, taking the address of a C# buffer-pointer
temporary. The generic `array-address` regression checks local/global arrays and
primitive/nonprimitive member arrays. C `&array` now reuses the emitted storage
pointer, which has the same base address as array decay. No generated probe or
engine source was patched.

The broader address reduction separately exposed missing row strides in flattened
pointer-to-array arithmetic. `pointer-array-stride` now compares native and
translated addition in both operand orders, subtraction and signed pointer
differences, prefix/postfix increments/decrements, compound assignments, and
`for` increments. It checks aggregate-array strides, one evaluation of a
side-effecting member target, and an address-taken global pointer slot. The
backend retains the IR row type and scales scalar-pointer operations by its
fixed element count. Postfix operations return the old pointer without a second
target evaluation or a delegate allocation. Atomic/volatile pointer-to-array
updates and unknown/empty row bounds receive explicit unsupported diagnostics.
The address and stride fixtures both passed the combined focused regression run.

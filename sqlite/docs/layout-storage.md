# Actual translated layout checks

Run `scripts/layout-native.sh` to compute the pinned native oracle and
`scripts/test-layout-translated.sh` to compare translated execution. Set
`SQLITE_AOT=1` on the latter command to run the same checks under linux-x64
NativeAOT as well as the JIT. Both use the configured LP64, unsigned-char,
MS-compatible bit-field storage profile. The checked oracle is
`tests/layout-native.expected`.

`src/layout_probe.c` includes the unchanged amalgamation and tests all 30 active
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
on a mismatch. There are currently 33 aggregate checks and eight array checks.

The sidecar contains no reflection or generic pointer arguments. It does not
edit `Program.cs`, implement missing storage, or change the engine. Expected
values come from native execution, not a handwritten layout table. SQLite's
`Mem` typedef is explicitly mapped to its emitted `sqlite3_value` tag. C# test
wrappers allow testing flexible-array headers without introducing nonstandard
nested flexible-array structures into the C probe.

CS9184 on a pointer-element inline array means C#'s inline-array language
operations are unavailable for that element type. dotcc accesses these fields
through raw element pointers. The span, wrapper-size, and read/write checks
validate the storage those operations require; warning suppression is not used
as proof.

Native, translated JIT, and linux-x64 NativeAOT validation of the expanded probe
passed with the explicit `SQLITE_MAX_MMAP_SIZE=0` profile. All 30 active offsets,
33 actual aggregate sizes/alignments, and eight pointer-array storage checks
matched. The native, JIT, and AOT transcripts are byte-identical. Local evidence
is in `artifacts/layout-aot-validation.log`, `translated-layout-aot.out`,
`translated-layout-aot-build.log`, and `layout-aot-total.time`.

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

# Pending Zig campaign work completed, 2026-11-10

This follow-up covers the compiler, IR, tests and source patches left outside
`9f8c94a5b47d950d3e35d3446a92e7965a75417b`, including files outside `libghostty/`.
The earlier "unvalidated" labels were checked against actual native and managed
execution. These changes do not replace the qualified canonical 1.0.0 products.
The updated compiler freshly emits, links and builds the real engine, and both
new source forms pass the owning consumer under JIT and whole-library-rooted
NativeAOT. The ordinary no-override canonical solution and consumer also pass.
Full upstream managed qualification remains pending.

## Completed source groups

All frontend names below are relative to `DotCC.Lib/Frontends/`.

| Group | Files and result |
| --- | --- |
| Composite type operands | `ZigLowering.BitcastWidth.cs`, `ZigLowering.CompositeTypeLocations.cs`, `ZigLowering.Generics.cs`, `ZigLowering.TypeInfo.cs`, `ZigLowering.Types.cs`: retain actual source element widths/sentinels through `@TypeOf`, aligned slice types, type arguments and generic cache keys. Equal CLR carriers do not supply source widths. Without these changes, four of six generic forwarding/cache controls fail; with them all six pass. Six native Debug/Fast controls pass. |
| Loop else arms | `ZigLowering.Loops.cs`, `ZigLowering.Stmts.cs`: statement-form loop expressions retain normal-completion else arms, empty/noreturn statements and existing break/continue/capture semantics. Expanded native/source/object tests cover switch prongs, return/unreachable arms, while/error captures, labeled continue and lazy completion. |
| Comptime slices | `ZigLowering.LocalDecls.cs`, `ZigLowering.TypeInfo.Lists.cs`, `ZigLowering.Types.cs`: preserve pure immutable slice/array-copy initializers for comptime argument reads while retaining ordinary runtime locals. Named, sliced, computed and typed-copy keys pass. Runtime parameters and ordinary runtime calls still reject as comptime slice arguments; `Generics.cs` reports the actual source/caller. |
| Vector optional layout | `ZigSyntheticModules.cs` and `DotCC.Lib/Ir/IrBuilder.Layout.cs`: the bounded 128-byte numeric-vector path follows the existing host CPU feature model instead of inventing 128-byte alignment. Native x64 baseline/v3/native controls show alignments 16/32/64 and optional extents 144/160/192. C GNU vector types use a separate path. |
| Optional/null controls | Forwarded null values already work with the current implementation; their previously uncommitted tests now validate that behavior. Wide optional vectors execute source/object with actual source stdlib types. A Lib-mode test host avoids demanding unrelated Zig executable startup support. Empty-record/zero-array optional size queries are valid; stale rejection tests now check their proven one-byte discriminant layout. Nested unsupported optional cases still reject. |

## Test inventory

- `DotCC.FunctionalTests/ZigGenericElementWidthExecutionTests.cs` and
  `Fixtures/ZigGenericElementWidths/forward-u8.zig`, `hash-bytes.zig`, `cache.zig`.
- `DotCC.FunctionalTests/ZigStaticPointerExecutionTests.cs` and
  `Fixtures/ZigStaticPointers/for-else-return.zig`, `comptime-slice-arguments.zig`,
  `runtime-comptime-slice.zig`, `runtime-call-comptime-slice.zig`.
- `DotCC.FunctionalTests/ZigRenderOptionalExecutionTests.cs` and
  `Fixtures/ZigRenderOptional/forward-null.zig`.
- `DotCC.FunctionalTests/ZigOptionalStorageVectorExecutionTests.cs` and
  `Fixtures/ZigWideVectors/optional-storage.zig`.
- `DotCC.Tests/ZigOptionalLayoutTests.cs`.

The integrated focused run passes 136 functional cases with zero skips/failures;
22 optional-layout units pass. Two preexisting testing-allocator cases are
explicitly outside that passing total, as described below. Evidence and commands
are in [summary.json](../artifacts/zig-pending-20261110/summary.json).

## Legacy Patch16 checkpoint

`patches/direct/0016-concrete-size-report.patch`, its README and manifest now
form a qualified **legacy C-ABI-profile** checkpoint for pin `93967b7f9478`.
Actual original/patched source encoders and real `std.Io.Writer` compare equal
for 11,700 native C/Zig Debug/Fast cases. Managed source and object/link both
pass the C profile's native layout and Writer checks. Non-C managed Writer
parity passes separately; strict native non-C layout does not pass. See the
[exact scope and proof](../artifacts/zig-pending-20261110/legacy-size/REPORT.txt).
The active 1.0.0 recipe still uses `patches/vt-1.0.0` and does not apply this
legacy patch. Native/reflection type names change as documented by the patch.

## Concrete remaining limitations

There are no deliberately parked source fragments in the completed groups above.
The following existing limitations are recorded, not silently counted as passes:

1. `DotCC.FunctionalTests/Fixtures/ZigStaticPointers/testing-allocator.zig`,
   selected by `ZigStaticPointerExecutionTests`: both source/object routes reach
   unsupported syntax in pinned `std/Thread.zig:1178:42` (`LinuxThreadImpl`,
   unexpected `switch`). The already accepted 1.0.0 compiler reproduces the same
   failure. No parser changes were made in this follow-up. Finishing it requires
   supporting that stdlib grammar path and rerunning the two cases.
2. `libghostty/patches/direct/0016-concrete-size-report.patch`, non-C profile:
   native Zig auto-layout offsets are 8/10/0/4, while both original and patched
   managed structs use 0/2/4/8. Strict source/object layout probes return 15.
   Native C layout is qualified; non-C native ABI parity requires an explicit
   compiler auto-layout policy and is not a claim of this checkpoint.

The full upstream managed campaign is still open. Concurrent CUI/gallery,
managed-symbol and printf changes are independent work and were not included
as Zig campaign edits. Disposable builds, native controls and test products stay
under `/mnt/storage/dotcc-libghostty-disposable/zig-pending-20261110`.

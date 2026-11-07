# libghostty-vt 1.0.0 direct compatibility patches

This chain targets the official `vt-1.0.0` release archive
`libghostty-vt-1.0.0.tar.gz`, SHA-256
`4a6ef758be721923e1969e7b81b443dcc92d8dd3fb5a39600c03a8817a753676`.
The archive's application `build.zig.zon` still says `1.4.1-dev`; the library
release tag identifies this baseline. Native Zig remains `0.17.0`.

The chain ports the fourteen source adaptations required by the actual direct
engine at the previously accepted commit
`93967b7f9478e94dcbb42125c8a90f34feb40d75`. Every adapted idiom remains in this
release, so none of patches 0001–0014 is obsolete. For those patches, the ordered removed/added lines
are byte-for-byte identical to the old patches. Only source context and line
numbers changed: notably, foreground color now uses `pen.fg_color.value()`.
Each manifest entry records the exact before, after, and patch SHA-256, plus
the compatibility rationale. No source files are vendored in this directory.

| Patch | Limited adaptation |
| --- | --- |
| 0001 | Forward `Action.Value` to the same generic payload-type helper. |
| 0002 | Calculate X11 table dimensions with scalar comptime loops; make map construction explicitly comptime. |
| 0003 | Widen verified ASCII vector lanes into the same `u32` outputs individually. |
| 0004 | Traverse the same packed field paths recursively to calculate their masks. |
| 0005 | Share the unchanged color-scheme response bytes with their comptime size bound. |
| 0006 | Build XTGETTCAP replies explicitly at comptime with equivalent decimal, hex, and framing loops. |
| 0007–0008 | Keep public `Parser.next`; give the stream caller fixed storage through `nextInto`. |
| 0009 | Derive the unchanged mode encoder's size from its framing and decimal digits. |
| 0010 | Result-locate the unchanged DCS hook switch in a `?Hook` local. |
| 0011 | Reject duplicate comptime keys by evaluating `unreachable`; only the diagnostic changes. |
| 0012 | Annotate the existing cursor hyperlink-ID result as `?[]const u8`. |
| 0013 | Pass the same RGB channels to Writer as ordered positional arguments. |
| 0014 | Interleave the same wide and spacer lanes with explicit fixed-size assignments. |
| 0015 | Defer the complete ABI-manifest validation block to the existing lazy JSON manifest initializer. |
| 0016 | Make the existing sized-struct validation body's comptime context explicit. |
| 0017 | Cap only `Stream.printRuns` scanner batches at 64 lanes. |
| 0018 | Use the existing scalar cell-store loop when the build's SIMD option is false. |
| 0019 | Derive the same C Style reserved mask from its actual field types and layout. |

The previous chain's separate 0015 generic color-scheme diagnostic-name workaround is
deferred unless the release engine demonstrates it is required. Pending 0016
from that old chain (`concrete-size-report`) and optional experiments are outside
this chain. The release's algorithms,
capability data, and public parser entry point remain intact.

Release patch `0015-lazy-abi-manifest-validation.patch` addresses a demonstrated
engine blocker when importing `c/types.zig` for required `CursorStyle` and
`sized_init`: the original module-level comptime block eagerly traverses the
complete standalone ABI-manifest table, including callbacks and features beyond
the selected runtime surface. The header-based runtime does not consume that
JSON manifest. The patch moves the exact validation body into an ordinary nested
block at the start of the existing lazy `json` initializer, retaining its
100,000 quota before the initializer's existing 10,000,000 quota and both writers.
Every `type_decls` row, validation, ordering rule, and JSON/export function remains
unchanged. Consumers of the manifest still execute every validation; the native
library's `ghostty_type_json` export demands that constant.

Release patch `0016-explicit-sized-validation.patch` retains the original
`const info` reflection binding in normal function scope and wraps every following
`validateSized` statement in `comptime { ... }`. Its type parameter is already comptime
and its callers already invoke it at comptime. The explicit lexical context lets
the direct frontend run the reflection loop as compile-time work in a deferred
generic body. Every extern-layout, leading-size, base-field, and padding check
retains its source order and diagnostic; runtime helpers and layout calculations
are unchanged. This release patch is unrelated to the old chain's pending 0016.

Release patch `0017-bounded-print-batches.patch` caps only the local vector scan
in `Stream.printRuns` with `@min(64, native_lanes)`. The global helper doubles
the host's native vector width, producing 128 byte lanes on AVX-512 even when
the optional C++ SIMD engine is disabled. The cap avoids requiring a 128-bit
control mask for this incidental scanning optimization. It finds the same first
control in smaller scan iterations and retains every predicate, scalar tail,
dispatch, parser state, and consumed boundary offset. The native lane control
also verifies that u16/u32 counts remain unchanged at or below 64.

Release patch `0018-scalar-cell-store.patch` makes only the existing manually
vectorized block in `printSliceStoreRun` conditional on the build's SIMD option.
For the selected `simd=false` product, its original scalar tail processes the
entire run using the same cell-template/codepoint merge. The vector body remains
available unchanged when SIMD is enabled. The charset branch and complete
scalar tail are byte-identical to their prior source. This affects optimization
and throughput, preserving the actual terminal algorithms and cell contents.

Release patch `0019-reflected-style-reserved-mask.patch` derives the reserved
mask from the real flags-padding/reserved field widths, byte offsets, packed bit
offset, and target byte order. It replaces only the constant initializer's
temporary extern-struct mutation and pointer reinterpretation. It introduces
no hardcoded layout and preserves the native-u128 meaning on both byte orders.
Runtime `toBits`, `write`, conversions, and `style_is_default` are unchanged.

Apply the chain to pristine source, resume a partial chain, or check final bytes:

```sh
python3 libghostty/patches/vt-1.0.0/apply.py libghostty/ref/libghostty-vt-1.0.0
python3 libghostty/patches/vt-1.0.0/apply.py libghostty/ref/libghostty-vt-1.0.0 --check
```

`--manifest PATH` selects an alternate explicit manifest; the default is beside
the script. The script validates every source and patch before writing, recognizes
ordered intermediate states for multiply patched files, uses zero patch fuzz,
and preserves unknown source bytes.

Reproduce bounded checks without mutating the active release tree:

```sh
python3 libghostty/artifacts/vt-1.0.0/patches/verify-replay.py
python3 libghostty/artifacts/vt-1.0.0/patches/verify-native.py
python3 libghostty/artifacts/vt-1.0.0/patches/verify-graph.py
python3 libghostty/artifacts/vt-1.0.0/patches/verify-lazy-abi.py
python3 libghostty/artifacts/vt-1.0.0/patches/verify-sized-validator.py
python3 libghostty/artifacts/vt-1.0.0/patches/verify-lazy-abi.py --label sized-validation
python3 libghostty/artifacts/vt-1.0.0/patches/print-batches/build.py
python3 libghostty/artifacts/vt-1.0.0/patches/print-batches/verify.py
python3 libghostty/artifacts/vt-1.0.0/patches/style-mask/build.py
python3 libghostty/artifacts/vt-1.0.0/patches/style-mask/verify.py
```

Scratch files, caches, and executables stay under
`/mnt/storage/dotcc-libghostty-disposable/vt-1.0.0/patches`.
Compact receipts are in `libghostty/artifacts/vt-1.0.0/patches/`.
Replay verifies pristine application, idempotence, partial chains after patches
0007, 0013, 0015, 0016, 0017, and 0018, preservation of unknown edits before any mutation, identical
adaptation lines to the accepted chain, and the active tree's final hashes.
It also verifies that 0015 changes only the validation block's location and
indentation and 0016 only adds its explicit comptime wrapper after `const info`.
For 0017 it verifies that only the local batch cap changed and the global lane
helper still exactly matches the pristine release.
For 0018 it proves that only the vector block was guarded, retaining the charset
branch and complete scalar tail.
For 0019 it verifies that only the reserved-mask initializer changed.
`verify-lazy-abi.py` rebuilds the native library into its own scratch
prefix and compares the manifest bytes and the existing 24 native boundary,
oracle, and layout cases against the recorded pre-patch library. It requires
the pre-patch snapshot recorded in `lazy-abi-manifest-before.json`.
The patched full native shared/static build passes. Its JSON is byte-for-byte
identical to the original 75,223-byte manifest (SHA-256
`715c2af072a2ac2a671258a9bd5f8d1747d5aa33b7e41b2b0bdc25218e0a2c5a`),
and all 24 cases pass with identical direct/indirect outputs.
The focused sized-validator controls compare the original and adapted five
native sized-helper tests and rejection of six malformed layout/base/padding
declarations. `--label sized-validation` records the rebuilt full native proof
for the chain including 0016 while reusing the established native caches.

The print-batch differential consumer compares actual uncapped/capped native
LLVM shared engines rather than fixed expected terminal output. This forces the
128-to-64 byte scanner change; Zig's self-hosted native backend has smaller
suggested widths. Its 2,940 cases cover
lengths through 1,023, including 63/64/65, 127/128/129, 255/256/257 and
511/512/513, across seven row widths, whole/chunked writes, and string/ground
`write_until` stops. It records exact snapshots, cursor positions, every visible
cell's codepoint/wide/graphemes/style, callbacks/replies, and all stop fields.
It includes standalone long u32 Unicode, wide-cell, combining-character, and
DEC charset runs, comparing the final 0017+0018 engines to the recorded original
`simd=false` native engine.
Exact old/new observations agree: 5,670 snapshots, 4,322,970 cells, 2,730 boundary
stops, and 3,212,468 input bytes. The native LLVM control reports u8 lanes
128 → 64, with u16=64 and u32=32 unchanged.

The style-mask control compares the actual original and adapted declarations
using the captured native dependency graph. It checks the exact mask, null and
default behavior, all 128 basis bits, padding-only/default classification,
4,096 deterministic random values, and matching `write`/`fromStyle` bytes for
4,096 valid colored styles with randomized flags and padding. Its source views
copy only `c/style.zig`, expose the private mask to the probe, and symlink the
remaining original implementation. The full native library rebuild and existing
24-case direct/indirect boundary oracle are recorded beside this focused proof.
Large observation streams and the recorded uncapped native archive stay in
disposable storage; compact provenance, hashes, commands, and counts are under
`artifacts/vt-1.0.0/patches/print-batches/`.

The native controls pass 85,184 RGB comparisons and 1,028 interleave comparisons
in each of Debug and ReleaseFast. They compare 4,112 actual release
original/adapted scalar UTF-8 decoder cases, including the new `ascii` result
flag; all 273 XTGETTCAP entries plus 20 edge entries and their sentinels; two
actual Source tests; and four expected invalid-capability rejections.
After the fresh native graph is available, `verify-graph.py` compares every
recursively nested actual release Row/Cell mask with the original traversal,
including combined, duplicate, and empty paths (compile 0, run 42).
Existing
transformation-specific proofs and their limits remain documented in
`../direct/README.md` and `artifacts/direct-resume-20261104/`. These bounded
checks support this port; full release engine qualification is a separate gate.

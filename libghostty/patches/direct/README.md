# Direct-route compatibility patches

Pinned upstream: `93967b7f9478e94dcbb42125c8a90f34feb40d75`.

`0001-action-value-forwarder.patch` replaces `Action.Value`'s generic function
alias with a forwarding function of the same signature, calling the same
`c_union.Value(tag)` helper. The helper still selects `@FieldType(Union,
@tagName(tag))`. This avoids the frontend's generic function-alias lookup gap
without duplicating payload mapping or changing terminal algorithms, layouts,
conversion logic or exported names. The declaration and comptime function identity
become those of the forwarder; runtime callers use the same payload types.

`0002-x11-comptime-dimensions.patch` makes the X11 table dimensions explicit
scalar comptime calculations. It counts the same newline bytes as
`std.mem.count(u8, data, "\n")` and derives one name-buffer bound from the longest
input line. The existing parser still reads every RGB value and trimmed name;
the returned name slices keep their original lengths and zero terminators.
Only intermediate comptime buffer capacity changes. This avoids interpreting
std's SIMD count and re-lowering a different array type on each loop iteration.
No table entry, lookup, order or runtime algorithm is substituted.

The same patch wraps `colorMap`'s existing body in `comptime { ... }`. Its only
use already initializes a module constant. Making that context explicit lets
the frontend evaluate the local `kvs` array before specializing std's
`comptime kvs_list: anytype` parameter. The original array fill, sorting and map
construction stay intact; no runtime engine operation changes.

Pinned native Zig builds the actual patched table and checks every name, zero
terminator and RGB value against a digest independently computed from `rgb.txt`.
It also checks every entry through the original case-insensitive map. This passes
with exit 42. Commands and receipts are under
`artifacts/direct-resume-20261104/x11-comptime-dimensions`.

`0003-ascii-vector-widening.patch` preserves the scalar UTF-8 decoder's ASCII
vector scan, then widens its verified ASCII lanes into the same `u32` output
with an `inline for`. The original forms a vector with four times the input's
byte width; the native host's suggested byte vector has 64 lanes, which would
require an unsupported 256-byte destination vector. The lane copies preserve
every codepoint and count, retain the complete Unicode/error fallback, and
introduce no lane-count constant. This limited source adaptation is shorter
than extending the backend's vector storage and widening operations.

Pinned native Zig compares original and patched decoding in 4,112 cases,
including every byte value, ASCII vector boundaries, ESC stops, valid Unicode,
malformed sequences and truncated inputs. Consumed/decoded counts and all
codepoints agree (exit 42, empty streams). The oracle copies only reroute the
shared scalar index helper and use the equivalent ReleaseFast assertion to
avoid duplicate font/terminal module ownership. The managed source/object lane
copy reduction checks all 64 outputs. Exact commands and receipts are under
`artifacts/direct-resume-20261104/actual-threaded-dependency-worker`; these
checks establish the adaptation's behavior, not full engine acceptance.

`0004-field-mask-path.patch` expresses `page.fieldMask`'s dot-separated field
traversal recursively. Each component uses the same source `@bitOffsetOf`,
`@FieldType` and `@bitSizeOf`; packed-union members still add zero offset and
require packed layout. The same final all-ones value is shifted by the summed
offset, and the outer loop still ORs every requested path. This avoids a mutable
comptime type and a stateful split iterator with an optional inline-while capture.
It changes how the helper computes its compile-time result, without replacing
the mask algorithm, fields, layouts or runtime page scan.

Pinned native Zig compares the patched actual `Row` and `Cell` masks against
the extracted original helper for every top-level field and every recursively
nested packed struct/union field path. Combined, duplicate and empty field sets
also agree (compile0/run42, empty streams). Invalid field paths still fail source
reflection; the diagnostic can name the remaining path suffix. The original
helper, native probe and commands are under
`artifacts/direct-resume-20261104/field-mask-path`. Managed execution and the
full-engine retry remain separate from this native adaptation proof.

`0005-color-scheme-size.patch` shares the encoder's existing color-scheme byte
selection with its compile-time maximum-size calculation. The public encoder
still calls the real `Writer.writeAll`, with the same switch-selected byte
strings and error propagation. The maximum still visits every actual enum value
and takes the maximum encoded length. It no longer constructs and invokes a
discarding Writer just to count those known bytes at compile time. No bound or
response is hardcoded and no runtime terminal behavior is removed.

Pinned native Zig compares both actual scheme encodings and the maximum against
the extracted original Discarding/Writer implementation. Output, surrounding
guards and short-buffer `WriteFailed` behavior agree (compile0/run42, empty
streams). The native comparison, original helper and reduced managed control
are under `artifacts/direct-resume-20261104/color-scheme-size`. This bounded
adaptation avoids requiring compile-time virtual Writer calls and containing
object pointer recovery for a size constant; runtime Writer remains translated.

`0006-xtgettcap-comptime-bytes.patch` makes construction of the terminfo reply
map explicitly comptime. It keeps the same capability traversal, escape and
parameter rules, canceled-capability errors, zero-terminated replies and original
`StaticStringMap` builder/runtime lookup. Small decimal, uppercase hex and framing
loops encode the same bytes without invoking virtual Writer machinery at comptime
or needing a slice argument's length in a stdlib function return signature. No
capability or response is replaced by a precomputed Ghostty-specific table.

Pinned native Zig compares every one of the actual map's 273 keys, values and
terminators against the original source, plus a 20-entry edge map with u32
boundaries and control/parameter strings. Four negative checks retain the original
canceled/middle-control rejection. Managed source/object controls execute the
actual map builder and runtime lookups. Reproduction and limits are under
`artifacts/direct-resume-20261104/actual-xtgettcap-worker`.

`0007-parser-action-output.patch` and `0008-stream-action-output.patch` together
add caller-owned output for the byte-at-a-time parser. Public `Parser.next` keeps
its original signature through a wrapper. `nextInto` evaluates the same exit,
transition and entry expressions, in order, into three caller-provided slots;
the original deferred state update still follows them. The actual stream caller
provides a fixed local array, then performs the same transition observation and
ordered action dispatch. Borrowed payload lifetimes stay unchanged. This avoids
a retained array-return allocation per input byte without changing parser logic
or claiming to solve the general compiler array-return ABI.

Native observations agree across all 14 parser states and 256 byte inputs,
17,505 bytes of transitions/CSI/OSC/DCS payload corpora, public-wrapper parity and
57 selected native tests. Managed source/object reductions each run 200,002 calls
and preserve effects, state timing, nulls, borrowed slices and independent action
copies using bounded caller storage. The full engine remains separately gated.
Commands, patches and limits are under
`artifacts/direct-resume-20261104/parser-array-lifetime`.

`0009-mode-report-size.patch` retains the original maximal `Report`, then derives
its size from the unchanged encoder framing and decimal field digit counts. It
avoids constructing a discarding Writer merely to count bytes at compile time.
The real runtime encoder is unchanged. Native original/patched output and guards
agree for all 65,536 IDs, five states and both ANSI flags (655,360 reports); both
bounds and the observed maximum are exactly 12. Managed source/object controls
also check the exact bound and original encoder output. Commands and receipts
are under `artifacts/direct-resume-20261104/actual-mode-report-size-worker`.

`0010-dcs-hook-result.patch` result-locates the unchanged `Handler.tryHook`
switch in a `?Hook` local, then returns that payload from `!?Hook`. Every
branch, block scope, `try`, allocation and public catch/state update remains
unchanged. This avoids a general compiler extension for mixed error/payload
block returns; it adds no support for those otherwise-rejected expressions.

Pinned native Zig compares the original and patched handlers in 133,120 hook
cases under the selected tmux-disabled profile, including allocation counts,
ownership/free pairing, allocation failure and recovery, uppercase XTGETTCAP
keys, eight DECRQSS replies and both byte limits. The switch body is also checked
byte-for-byte. A native-backed compiler control of the local-result idiom runs
through source and object/link, checking error identity, selected effects,
`defer` and `errdefer`. The isolated actual DCS managed control remains blocked
in std logging's comptime slice loop; it is not counted as executed. The actual
92-root engine clears `tryHook` and reaches a separate `FlagStack.set` bitcast
width failure. Commands, receipts and limits are under
`artifacts/direct-resume-20261104/actual-dcs-hook-return-worker`.

Apply to the pristine pin, resume a partial chain, or check an already-applied tree:

```sh
python3 libghostty/patches/direct/apply.py libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75
```

The script recognizes ordered patches to the same source file and preserves
sources that differ from any recorded chain state. Actual replay, idempotence,
partial-chain and unknown-source-preservation checks are reproducible with
`artifacts/direct-resume-20261104/parser-array-lifetime/verify-patch-chain.py`.
The patch and manifest are durable; `ref/` is the working upstream input. Accepted
C generated products remain unchanged. A fresh original C-route reproduction
uses pristine pinned inputs; direct-route reproduction applies these patches.

Pinned native Zig checks every actual `StreamAction` tag's forwarded payload type
against `@FieldType(Action, name)` and exits 42. Reduced native, emitted-source and
serialized-object/link executions exercise struct, slice, pointer and void
payloads through the forwarding signature. Commands and receipts are under
`artifacts/direct-resume-20261104/action-value-forwarder` and the adjacent
`native-action-value-forwarder.json`. The actual engine clears the original
`Action.Value` failure and reaches `@cmpxchgStrong` in the concurrent log-once
slot table. Engine emission/build/execution is still incomplete.

`0011-comptime-set-duplicate-trap.patch` changes only the data-dependent
comptime duplicate-key failure from `@compileError` to `unreachable`. The
original sort, table construction, key values, width selection and runtime
binary search remain unchanged. Zig must reject executing `unreachable` during
comptime evaluation in every optimization mode; duplicate inputs still fail
compilation, with the generic unreachable diagnostic replacing the custom
"keys must be unique" message. This lets DotCC's existing interpreter decide
whether the duplicate guard is taken, avoiding a larger source-stepped block
implementation merely for that incidental diagnostic idiom.

Original/patched native controls agree for two exhaustive unsigned sets and a
signed set in Debug, ReleaseFast, ReleaseSafe and ReleaseSmall. Both reject the
duplicate control in all four modes. Managed source/object controls also reject
duplicates and execute the lookup checks. Commands and limits, including the
separate existing zero-bit-integer cut, are under
`artifacts/direct-resume-20261104/computed-index/`.

`0012-cursor-hyperlink-id-result.patch` annotates the existing `cursorCopy`
hyperlink-ID switch result as `?[]const u8`, the type already accepted by
`startHyperlink`. The original explicit/implicit union branches, borrowed page
memory, null versus present empty ID, and error handling are unchanged. It avoids
requiring general optional peer inference for a capturing statement-valued switch.
Native Debug/ReleaseFast comparisons verify inferred and annotated forms agree
for every bounded span of a small source buffer and implicit IDs, including exact
pointer identity and evaluation counts. Managed source/object controls verify the
annotated path. Commands and limits are under
`artifacts/direct-resume-20261104/cursor-hyperlink-result/`.

`0013-rgb-positional-format.patch` changes the two `printAttributes` RGB
formats to positional arguments `.{ rgb.r, rgb.g, rgb.b }`. Decimal formatting,
foreground/background framing, channel order and Writer error propagation are
preserved. Native Debug/ReleaseFast compare original and adapted buffered bytes
and error codes for 1,331 colors, both formats and capacities 0 through 31
(85,184 comparisons per mode). Managed source/object checks include a short
Writer failure. It avoids named-argument reflection; it does not claim a general
`std.meta.fieldIndex` fix.

`0014-wide-cell-interleave.patch` replaces the single `std.simd.interlace` call
in the wide-cell print loop with fixed-size, source-order lane assignments to
the same local array. Each wide lane still precedes its corresponding spacer.
Vector construction/arithmetic, bounds, packed stores, scalar tail and behavior
remain unchanged. Native Debug/ReleaseFast compare the arrays against the original
for 257 seeds and 1/2/4/8 pairs. Managed source/object controls check the adapted
lane order. No general comptime-vector support is claimed. Reproduction and
receipts for both changes live under
`artifacts/direct-resume-20261104/rgb-writer/`.

`0015-concrete-color-scheme.patch` declares the two ColorScheme tags explicitly
with the same target-dependent backing (`c_int` for C, `u1` for Zig), the same
light=0/dark=1 values, size/alignment and exhaustive range. This avoids Zig's
generic-argument diagnostic type-name printer in the reached callback signature.
The nominal/reflection name becomes the ordinary ColorScheme declaration rather
than the generic factory instance; selected API/protocol behavior is unchanged.
Native original/patched C/Zig × Debug/Fast controls check backing identity,
layout, ordinals and both encoded reports at 17 bounded Writer capacities,
including failure propagation and guard bytes. Evidence and exact commands are
in `artifacts/direct-resume-20261104/color-scheme-declaration/`. Existing full
native and managed upstream results precede this patch; the final matrix must
qualify the final source set. Qualified canonical products were not regenerated
by this patch.

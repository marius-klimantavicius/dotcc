# Scoped UTF-8 input storage

The safe wrapper uses call-scoped marshalling for **206 audited native entrypoints**
in CUI 1.4.0 (`5a9c12b433d4501bc2f5632ead3a1cc05e5b1ee5`), including their
string, byte-span and `TextBuffer` overloads. Convenience overloads forward to
these implementations. `Text(ReadOnlySpan<char>)` uses the same scoped encoder.
All direct scalar text arguments are classified: the three retained paths below,
collections and option pointees keep their stable storage policy. This audit
supersedes the blanket stable-string policy in earlier storage/UTF-8 documentation
only for the listed arguments.

The complete argument inventory, reasons and native definition hashes are in
[`utf8-lifetimes.json`](../scripts/managed-api/utf8-lifetimes.json). Sixty-one supporting
definitions bind the copy, hashing, rendering, shaping and arena behavior used
in the review. The source is the pinned CUI profile with the committed source
adaptations. `generate.py --check --native-root "$CUI_SOURCE"` validates these
definitions in addition to the existing singleton/storage evidence. A changed
definition requires another retention review. Host source hashes also bind the
managed filesystem and clipboard bridges used by the translated APIs.

## Why a temporary pointer is valid on these paths

* `cui_text_ex`, headings, labels, badges and table/grid text cells reach
  `cui__text_node` in `src/cui_layout.c`. It calls `cui__frame_strdup` and stores
  the native-owned copy in the node. The nowrap tooltip path copies text into
  tooltip state with `snprintf` before returning.
* Buttons reach `cui__button_impl`, `cui__label_text` and `cui__icon_node` in
  `src/cui_widgets.c`. Their submitted text is copied by that same text-node
  helper. Avatar names produce a numeric tone and local initials; emoji widgets
  copy their sequence into a native paint record.
* Direct `id`, `scroll_id` and TOC-anchor arguments have been traced through
  their helper calls. `cui_make_id`, `cui_push_id` and `cui__anchor_id` hash
  synchronously; downstream state receives numeric IDs. For example,
  `cui_range_calendar` uses both `cui_make_id` and `cui_push_id`. Chart, code,
  calendar and terminal entrypoints also forward IDs through private helpers;
  those helpers are included in the evidence. The checklist formats a derived
  ID into native frame storage. Knob/fader IDs can also be display labels; those
  labels are copied by `cui__text_node`/`cui__dl_caption` before returning.
* Drawing APIs consume the supplied text while producing glyph vertices. Emoji
  caches retain hash keys/pixels; shaping caches copy paragraph text in
  `cui__paragraph`. They do not retain these caller input addresses.
* Progress-ring and gauge captions are copied with `cui__frame_strdup` into
  native paint records. Code blocks also copy their source into frame storage;
  JSON rows copy displayed keys/values while parsing synchronously. Dot displays
  decode text into owned glyph indices. These paths do not depend on the caller's
  pointer surviving deferred paint.
* Checkbox, radio, switch and slider labels reach copying text helpers through
  `cui__toggle_row`/`cui__slider_header`. Confirm/status buttons reach them through
  `cui__morph_button` and `cui__face_content`; animated text uses
  `cui__fx_text_node`. Dialog and sheet titles reach them through `cui__modal_begin`.
  Multi-text calls scope each eligible scalar independently.
* Input events copy accepted text into the context immediately; password strength
  scans synchronously; URL parsing decodes into output records; image/font file
  paths are consumed during loading. The translated filesystem host decodes its
  path into an owned managed string. Copy-button clipboard submission reaches a
  host bridge that also decodes into an owned string before forwarding it.

Three direct scalar paths still need stable wrapper copies:

* `NodeBegin.title`: `cui_node_begin_ex` assigns it to deferred card paint data.
* `DockItem.icon`: `cui_dock_item_ex` saves it in deferred paint data. Its label
  uses scoped storage because `cui_tooltip` copies it before returning.
* `ChatMessage.text` when `opts.Markdown` is true: Markdown headings and table
  inline segments can retain slices of the original source in paint records.
  `md_parse_inline` → `md_seg_add` → `md_inline_emit` preserves those pointers.
  Plain chat text uses scoped storage; its text node copies immediately.

This is not a blanket lifetime guarantee for every CUI API: node pin helpers,
wheel-picker arrays and retained option/record fields have separate policies.
The overload generator rejects scalar arguments passed through `Utf8` or
`Utf8Required` unless listed in `retained_scalars`. New paths require a retention
review, rather than assuming that a name such as `id` or `label` establishes safety.

## Marshalling rules

Each audited implementation supplies `stackalloc byte[256]` to `ScopedUtf8` and
pins its byte span across the call and associated synchronization. The generated
UTF-8 overloads copy this implementation from the reviewed source body.

* UTF-16 strings and char spans are encoded with a trailing zero into the scratch
  buffer. Larger inputs rent a byte array, returned in `Dispose` even when a call
  throws. Capacity is measured in encoded bytes and includes the terminator.
* A byte span with an explicit final zero, such as `"Save\0"u8`, is validated and
  pinned directly. A bounded drawing call with an explicit nonnegative byte
  count can also borrow an unterminated span. No out-of-span byte is inspected.
* Other byte spans, including ordinary `"Save"u8`, are copied to scratch/pool
  storage to append a terminator. An arbitrary span provides no guarantee about
  the byte immediately after its bounds, even if a particular literal's backing
  storage contains a zero there.
* `TextBuffer` already owns terminated native storage. A temporary read lease
  prevents disposal from freeing it during the call. It neither binds a new
  context nor registers a frame-long editor lease; it is released on return.

Bytes must be valid UTF-8 and contain no embedded NUL. A single explicit trailing
NUL is accepted by byte-span method inputs; string inputs and editable-buffer
setters retain their existing embedded-NUL rejection. Native-owned copies preserve
the existing snapshot behavior after a submitted source is mutated or disposed.
The public API still exposes no pointer or span into disposable native storage.

## CUI's own arena

CUI does have `cui__frame_alloc`, declared in its **internal** header. It allocates
zeroed, aligned storage, keeps overflow pointers stable for the current frame,
and frees/resizes storage at the beginning of the next native frame. It is not a
public CUI allocation API, although the translated implementation exposes it.

It could back a future wrapper allocator adapter; a second arena is not an
intrinsic requirement of wrapping CUI. It is not a drop-in substitution for the
current `FrameArena`, however. The wrapper enforces `FrameArenaLimit`, exposes its
own reservation/usage diagnostics, reuses bounded blocks, and retires storage
after native frame adoption. Some marshaling helpers and bound-state writes can
also run outside an active frame. The native arena has a different reset point,
overflow/growth policy and fatal allocation-failure path. Those contracts need
review before sharing its storage.

This change removes wrapper arena allocations from the audited scalar paths.
It keeps the existing allocator for retained arrays, option pointees and the
three retained scalar paths, with its limits and diagnostics intact.

## Validation

`ManagedCui.Tests --utf8` checks native pixel equivalence, stack/array mutation,
buffer disposal, UTF-8 and NUL rejection, bounded slices, scratch boundaries at
255/256 bytes, multibyte expansion, an 8 KiB pooled fallback and warmed managed
allocation behavior. The audited text/button/input workload consumes zero
wrapper arena bytes, including the long inputs. Existing delayed-temporary,
retained-array, lifecycle and public-surface tests remain applicable.

Executed on Linux x64: raw/processed × JIT/NativeAOT each passed 1,687 UTF-8,
75 safety and 21,306 public-surface assertions. All 27 original-layout processed JIT gallery
captures matched the pre-change captures exactly. The existing processed JIT
construction/rendering performance and allocation gates passed (27 assertions).
Regeneration was repeatable and the native definition hashes matched. Actual
Windows execution was not performed for this change.

The ID coverage regression checks exercise RangeCalendar with string, span and
buffer IDs; helper-forwarded Counter IDs; TOC anchors; and knob/fader fallback
labels surviving source mutation/disposal. Each workload uses zero wrapper-arena
bytes. Generator checks also reject injected arena regressions in RangeCalendar
and both scalar inputs of the mixed-text CodeBlock entrypoint. The broader
scalar guard also rejects a ProgressRing label regression.

The scalar coverage tests compare exact pixels for string, terminated span,
unterminated span and buffer inputs after mutation/disposal and stack/pool reuse.
They exercise ring/gauge deferred captions, pooled code text, JSON, morph-button
and toggle labels, animated text, dot displays, plain chat and multi-text readouts.
Those workloads consume zero wrapper-arena bytes. Markdown headings, dock icons
and node titles are checked separately: they must allocate stable wrapper storage
and preserve exact pixels after caller inputs are overwritten. Each example also
compares a different input to confirm that its text/icon is actually visible.

The added code editor's ID is scoped; its body and bar/paint builders are included
in the evidence. `ManagedCui.Tests --code-editor` passed 12 assertions in all four
Linux variants, covering UTF-8 editing, caret offsets, read-only mode, refused
inserts, mark validation, context ownership and delayed paint after disposal.
Raw and processed JIT gallery OS-input checks passed editing, indentation, undo,
find, go-to-line, navigation persistence and reset. Default gallery captures
change only the extended Code page; original-layout captures still match all 27
pre-extension pages. The full palette/OS matrix was not repeated for this addition.

# Pinned upstream and native control

The selected implementation is Ghostty's actual `libghostty-vt`, not the GUI
library or an alternate terminal. Source and toolchain pins live in
[`source.json`](../config/source.json) and [`toolchain.json`](../config/toolchain.json).
The selected source revision is `93967b7f9478e94dcbb42125c8a90f34feb40d75`,
committed on 2026-10-29, identifying itself as `1.4.1-dev`. Its archive SHA-256 is
`af551aa496cf54ff734e11563d45fe2c49739ee754d1740d1ac984a67a42dc70`.
The native library version is explicitly `0.1.0`, and the compiler is official
Zig `0.17.0`. Scripts read these values from metadata.

## Useful terminal-core scope

The workflow is to feed a VT byte recording into an independently owned terminal,
inspect its cursor/modes/cells/styles, resize it with primary-screen reflow,
extract plain UTF-8 text, reset it, and dispose it. Actual upstream parser,
screen/page storage, Unicode/grapheme/width handling, escape-sequence processing,
alternate-screen state, formatting, and render-state extraction remain engine
code. Render-state extraction supplies logical rows/cells; it draws no pixels.

The upstream scalar profile in [`translation.json`](../config/translation.json)
enables `formatter`, `render_state`, and `grid_introspection`. GUI integration,
PTY/process management, fonts/shaping, GPU/software drawing, keyboard/mouse
encoding, binary snapshot persistence, selection/search, optional color utility
exports, Kitty images, glyph registration, and regex/Tmux integrations are outside
this product. Ordinary VT color/style processing still belongs to the terminal.
Disabling a feature uses upstream build gates, without source edits.

## Actual implementation and generated inputs

`src/lib_vt.zig` is the engine compilation root. It reaches `src/terminal/main.zig`
and exports the C API declared in `src/terminal/c/main.zig` with `@export`.
`src/build/GhosttyZig.zig`, `GhosttyLibVt.zig`, `SharedDeps.zig`, and
`src/terminal/build_options.zig` define the real module and feature closure.
The public C header root is `include/ghostty/vt.h`.

The scalar product requires the pinned `uucode` implementation and Unicode 18
data. The upstream build graph also acquires `zigimg`, `translate_c`, `aro`,
zlib sources, and the pinned theme archive during configuration. Those five
inputs do not contribute terminal algorithm code in this profile. Their URLs,
Zig package content hashes, and owning `.zon` manifests are in `dependencies`.
`observed_extra_packages` records incidental acquisitions from broader upstream
build/help discovery; those packages are not selected product dependencies.

Generation runs the pinned uucode table generator with
`src/build/uucode_config.zig`, then `src/unicode/props_uucode.zig` and
`src/unicode/symbols_uucode.zig` to obtain `tables.zig`, `props.zig`, and
`symbols.zig`. Upstream emits `build_options` and `terminal_options` modules.
No Unicode tables are replaced by host character APIs. The C backend then
monomorphizes the entire selected Zig engine and its actual module graph into
`engine.c`; its support header is the pinned toolchain's `lib/zig.h`.

`tests/native/run.py:prepare_engine(ctx)` captures the actual upstream verbose
`build-lib` argv instead of reconstructing module imports. It preserves module
options and generated paths, changes the root target to the explicitly selected
`x86_64-freestanding-none`, and adds `-ofmt=c -femit-bin=.../engine.c`.
It records complete argv, generated module paths, output size, and target in
`native/emit-c-command.json` and `native/engine-closure.json` under the private
campaign work directory. This is a genuine whole-engine intermediate for DotCC;
native C execution is only an oracle, never the managed product backend.

The first original-target output was 19,327,091 bytes, 542,652 lines, and retained
Linux syscall assembly in standard-library host paths. Adding libc retained
assembly. The validated freestanding output was approximately 19 MB and had no
inline assembly. Exact current sizes are recorded with each generated artifact.
No terminal algorithm was removed to obtain this result.

## Host and ABI contract

The initial target is little-endian x86-64, LP64: pointers/`size_t` are 8 bytes,
`uint16_t` is 2 bytes, C enums are 4 bytes, and C `bool` is 1 byte. Native
observations give `GhosttyAllocator` size 16, its four-function vtable size 32,
and `GhosttyFormatterTerminalOptions` size 56. The C backend emits layout
assertions for its actual generated structs. These native observations are not
evidence of managed layout; managed/emitted layout must be checked separately.

The pinned constructor is
`ghostty_terminal_new(allocator, out_terminal, uint16_t cols, uint16_t rows)`;
older `GhosttyTerminalOptions` examples do not apply. `vt_write` consumes bytes
and retains parser state between calls. `get` writes a value of the documented
type into its final pointer. `reset` preserves dimensions; `resize` also accepts
cell pixel dimensions. C enum/status values come from the real headers.

Freestanding upstream intentionally has no default allocator. Every allocating
entry point, including formatter/render/iterator creation, must receive a valid
allocator. The authored ABI adapter supplies allocation/free only, using the
existing public `GhosttyAllocator` extension point. Its resize/remap callbacks
decline in-place changes; upstream performs relocation and copying. Allocation
alignment is a base-2 exponent, not a byte count. Allocator storage and vtable
must stay address-stable until every object using them has been freed.

Upstream's freestanding `TinyIo` becomes `std.Io.failing`; the selected workflow
requires no filesystem, clock, entropy, network, subprocess, or rendering
services. All actual terminal algorithms remain upstream. Logging is discarded
unless the caller installs the documented logger. Panic/trap handling and
compiler support intrinsics remain explicit translation/runtime concerns.

Terminal instances are not concurrently reentrant. A consumer must serialize
access to each instance and must not feed it recursively from callbacks. Multiple
instances may be independently owned. Callback userdata remains caller-owned;
PTY response bytes are borrowed for the synchronous callback and must be copied
if retained. Render row/cell views are invalid after the next render-state update.
A formatter borrows its terminal, which must outlive it. Returned allocated
buffers are freed with the same allocator and exact length through `ghostty_free`.

## Native assertion evidence

[`oracle.c`](../tests/native/oracle.c) passes 16 cases with the original native
Zig static library and with native-compiled whole-engine freestanding C.
Transcripts match [`expected.txt`](../tests/native/expected.txt) exactly:

1. Creation, dimensions, empty text, initial cursor.
2. ASCII feed, line breaks, formatter text, render cells, render cursor.
3. Cursor CUP addressing and the updated cell.
4. Erase display and cursor home.
5. Bold/palette SGR and style reset.
6. Split UTF-8, wide character/tail, combining grapheme, exact codepoints.
7. Cursor visibility modes.
8. Alternate-screen contents and primary-screen/cursor restoration.
9. Resize/reflow with retained text, exact row cells, and pixel dimensions.
10. Reset with dimensions preserved.
11. DSR reply callback bytes, bell, OSC title and notification count.
12. Formatter sizing, short buffer, allocated result and allocator pairing.
13. Ordinary invalid dimensions/queries and documented null cleanup.
14. Allocator failure returns out-of-memory.
15. Instance independence and survival after another instance is freed.
16. Thirty-two create/feed/format/free cycles, no tracked live allocations.

The allocator checks every free's original owner, length, and alignment.
`CHECK` assertions remain active regardless of `NDEBUG`. Feed checks the terminal's
VT-processing-error flag. A mismatch or failed assertion exits nonzero.

[`boundary_control.c`](../tests/native/boundary_control.c) separately exercises
the real authored delivery ABI against both engines: snapshot sizing and short
buffers, UTF-8 width/cursor, screen switching, resize/reset, two independent
instances, errors, and cleanup. Its two cases match
[`boundary-expected.txt`](../tests/native/boundary-expected.txt).

Current native evidence is under `build/native-tools/context-validation/artifacts/`
(16 engine cases) and `build/native-tools/boundary-context/artifacts/` (two adapter
cases). The integrated recipe's 2026-10-29 native run also passed its then-current
16-case suite. Future native suite runs include both controls. These results do
not establish successful DotCC translation or managed execution.

## Reproduction and offline behavior

Use the normal campaign commands from any working directory:

```bash
bash libghostty/scripts/fetch.sh --fetch missing
bash libghostty/scripts/test.sh --suite native --fetch never
```

The helper uses `ctx.sources['product']` and `ctx.sources['zig']`, shared
`Context.run` logging/process handling, and private campaign outputs. It never
downloads sources or toolchains itself. The pinned Zig build acquires its own
manifest-pinned package dependencies when acquisition is allowed. With
`--fetch never`, `zig build --system <source>/zig-pkg` disables remote package
fetching, and explicit `-fno-sys` selections retain the scalar profile. A missing
package fails with Zig's missing-package diagnostic. A fresh private build cache
with these offline settings passed; existing generated inputs are not required.

The native suite requires a GNU-compatible host C compiler (`CC` or `cc`) and
links its controls with `-lm`. The selected C backend profile has currently been
executed on Linux x86-64. Native tools do not write shared DotCC compiler bins.
Full commands and logs are in campaign receipts. The core native operations are:

```text
zig build <native_build_arguments from translation.json> --prefix <private>/install
    --cache-dir <private>/zig-cache -j2 --verbose <offline/version options>
zig build-lib <captured actual upstream module argv>
    -target x86_64-freestanding-none -ofmt=c -femit-bin=<private>/engine.c
cc -std=c11 -Wall -Wextra -Werror -I<upstream>/include -c tests/native/oracle.c
cc <oracle.o> <original static library> -lm -o <native oracle>
cc -std=gnu11 -O0 -w -I<zig>/lib -c <engine.c> -o <engine.o>
cc <oracle.o> <engine.o> -lm -o <C oracle>
```

## License and notice closure

Distribute the original Ghostty MIT `LICENSE`, uucode MIT `LICENSE.md`, the
Unicode license, the Hoehrmann UTF-8 decoder license, and the Zig MIT `LICENSE`
with both emitted forms. The pinned uucode package omits its referenced
`licenses/` directory from Zig package exports. `license_sources` therefore
declares a separate exact-commit codeload archive selecting `LICENSE.md` and
`licenses/`. It was acquired with the shared campaign `Source`/`acquire` path;
the required Unicode and Hoehrmann notices are present under that reference tree.
`license_inputs` maps the original source paths and metadata records its checksum.
Do not use the historical loose `ref/licenses/` copies for reproducible delivery.

Build-only translate-c/aro/zigimg/zlib/theme sources retain their own notices in
their reference trees. Their acquisition does not authorize delivering their
algorithms as part of the selected product. No native library is a managed
runtime dependency.

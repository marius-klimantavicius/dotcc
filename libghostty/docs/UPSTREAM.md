# Pinned upstream and native control

Official **libghostty-vt 1.0.0 is the canonical direct managed default**. Its
source/API closure is described below. Both raw/processed products pass owning
terminal behavior, 45 native differential/version/API cases, allocation and
delivery checks under JIT and whole-library-rooted NativeAOT. Normal regeneration
and the ordinary no-override solution/consumer pass. See the
[upgrade receipt](../artifacts/vt-1.0.0/DELIVERY.json) and
[upgrade documentation](VT-1.0.0.md). Full upstream managed qualification remains
pending; historical 0.1.0 results are separated below.

## Official release and provenance

The selected implementation is Ghostty's actual terminal library. Its source
and toolchain metadata are [source.json](../config/source.json) and
[toolchain.json](../config/toolchain.json).

| Input | Current pin |
| --- | --- |
| Library release | `vt-1.0.0`; `LIB_VERSION` is `1.0.0` |
| Annotated tag object | `6b38d55f04e66ebecc472d8be19946edcc6d2e9d` |
| Peeled source commit | `0fa3d577a5614150ff8e2c152de5fb9d5c6d2dec` |
| Tag date | `2026-11-06T22:55:16Z` |
| Official source archive SHA-256 | `4a6ef758be721923e1969e7b81b443dcc92d8dd3fb5a39600c03a8817a753676` |
| Matching dependency archive SHA-256 | `4d5ea5e448e4c7ea8fb55ccbaa8668f2a97d44f438cee831e2b7ecd162d85c29` |
| Compiler | Official Zig `0.17.0` |

The archives are the official
[source release](https://release.files.ghostty.org/libghostty-vt/1.0.0/libghostty-vt-1.0.0.tar.gz)
and matching
[dependency bundle](https://release.files.ghostty.org/libghostty-vt/1.0.0/libghostty-vt-1.0.0-deps.tar.gz).
The signed `SHA256SUMS` content and trusted-comment signatures were verified
against Ghostty's published minisign key; both archive hashes match that
manifest. The annotated tag and its peeled commit were also checked.
[provenance.json](../artifacts/vt-1.0.0/provenance.json) records the key, signatures,
checksums, and tag refs. This does not assert a separate Git-tag signature check.

The library has its own release version. The source's application
`build.zig.zon` still identifies Ghostty as `1.4.1-dev`; that is distinct from
libghostty-vt `1.0.0`. Use the official library archive, which includes
`LIB_VERSION` and the library's source subset. The
[official installation documentation](https://lib.ghostty.org/vt/installing.html)
and [version-pinned packaging guide](https://github.com/ghostty-org/ghostty/blob/vt-1.0.0/PACKAGING-LIBGHOSTTY.md)
explain the release files, signature verification, required Zig version, and
offline dependency layout.

## Useful terminal-core scope

The workflow feeds VT bytes into an independently owned terminal, inspects its
cursor/modes/cells/styles, resizes it with primary-screen reflow, extracts plain
UTF-8 text, resets it, and disposes it. The parser, screen/page storage,
Unicode/grapheme/width handling, escape processing, alternate screens,
formatting, and render-state extraction remain actual upstream engine code.
Render-state extraction supplies logical rows/cells without drawing pixels.

The selected [scalar profile](../config/translation.json) uses ReleaseFast,
`simd=false`, and `-Dvt-features=-all,+formatter,+render-state,+grid-introspection`.
GUI/PTY process management, fonts/shaping, drawing, input encoding, binary
snapshot persistence, selection/search, optional color utility exports, Kitty
images, glyph registration, and regex/Tmux integrations remain outside the
product. Ordinary terminal color/style processing remains enabled. Upstream
feature gates select this scope; the explicit compatibility patches are recorded
separately below.

## Actual implementation and dependency closure

`src/lib_vt.zig` is the compilation root. It reaches `src/terminal/main.zig` and
exports the C API through `src/terminal/c/main.zig` and `@export`. The public
header root is `include/ghostty/vt.h`. `GhosttyLibVt.zig`, `SharedDeps.zig`,
`GhosttyZig.zig`, and `src/terminal/build_options.zig` define the upstream graph
and generated options.

The release's `deps/libghostty-vt.txt` and matching dependency bundle contain
exactly five packages:

| Package | Source commit | Role in this profile |
| --- | --- | --- |
| uucode | `1fb73433bba5d93366c57f23ff2e9d7939746500` | Unicode implementation and generated tables |
| aro | `d0c8c4d9c55daa7ef6e40cf0f630a5b5e900989b` | Upstream build-graph tooling input |
| translate_c | `02ff0c523fb92a38939b04d0145c4a9602a74d56` | Upstream build-graph tooling input |
| highway | `66486a10623fa0d72fe91260f96c892e41aceb06` | Upstream build-graph input; optional C++ SIMD is disabled |
| wuffs | `7411f488fe2e2c205c3d3b3d28638b7356522930` | Upstream build-graph input; image features are disabled |

URLs and exact Zig package content hashes are in
[source.json](../config/source.json). The current uucode package is
`uucode-0.2.0-ZZjBPm6FVgBcY-79AHV8ckQj86z0JwOyVyj89VeZrEiv`; it supplies
Unicode 18 data. The remaining four packages do not supply terminal algorithm
code to the selected engine. No incidental extra packages are recorded for this
release closure.

The real uucode generator and `src/build/uucode_config.zig` produce
`tables.zig`; `src/unicode/props_uucode.zig` and `symbols_uucode.zig` produce
`props.zig` and `symbols.zig`. Upstream also generates `build_options` and
`terminal_options`. No host character API replaces these tables.

The private native shared/static builds use the captured upstream invocation in
[native/command.json](../artifacts/vt-1.0.0/native/command.json); the actual engine
module argv is in [engine-command.json](../artifacts/vt-1.0.0/native/engine-command.json).
The direct frontend consumes the captured graph's eleven modules: `root`,
`build_options`, `terminal_options`, `unicode_tables`, `symbols_tables`, `uucode`,
`types.zig`, `config.zig`, `tables`, `storage.zig`, and `build_config`.
[direct.json](../config/direct.json) records the current 96 selected roots,
Lib output, fast optimization, source stdlib types, and managed function pointers.
The authored C boundary is translated separately and linked with the actual Zig
engine object. The direct route does not create a C engine intermediate.

The [release compatibility manifest](../patches/vt-1.0.0/manifest.json) records
all nineteen patches and exact source/patch hashes. Its
[README](../patches/vt-1.0.0/README.md) explains each bounded adaptation,
including lazy manifest validation, explicit sized validation, bounded print
scans, scalar cell storage for `simd=false`, and a reserved-style mask derived
from actual field layout rather than comptime pointer reinterpretation. The old pin's pending patches
are not imported implicitly. [Replay evidence](../artifacts/vt-1.0.0/patches/replay.json)
checks pristine application, partial chains, idempotence, preservation of unknown
edits, and final source hashes.

The actual Zig engine and authored boundary emit and link into the published
managed product. Raw/processed builds and execution, four-row differential and
allocation checks, and canonical publication pass. Consult
[translation stages](../artifacts/vt-1.0.0/translation/stages.json) and the
[progress receipt](../artifacts/vt-1.0.0/RESUME.json) for the current frontier.

## Current 1.0.0 host and ABI contract

The selected host is little-endian Linux x86-64 LP64: pointers and `size_t` are
8 bytes, `uint16_t` is 2, C enums are 4, and C `bool` is 1. Current native header
and library observations are:

| Type | Size / relevant layout |
| --- | --- |
| `GhosttyAllocator` | 16 bytes; `ctx` at 0, `vtable` at 8 |
| `GhosttyAllocatorVtable` | 64 bytes; alloc/resize/remap/free at 0/8/16/24; four reserved pointer slots at 32 |
| `GhosttyGridRef` | 16 bytes; node/x/y at 0/8/10 |
| `GhosttyPoint` | 24 bytes; tag at 0, value at 8 |
| `GhosttyStyle` | 16 bytes; three colors at 0/4/8, 16-bit flags at 12, reserved at 14 |
| `GhosttyCodepointBuffer` | 24 bytes; pointer/capacity/length at 0/8/16 |
| `GhosttyFormatterOption` | 4-byte C enum |

These are 1.0.0 facts from [API/layout assessment](../artifacts/vt-1.0.0/boundary/api-assessment.json)
and [native layout output](../artifacts/vt-1.0.0/boundary/abi-layout-native.stdout).
[Standalone managed layout checks](../artifacts/vt-1.0.0/boundary/managed-layout-checks.json)
are recorded separately; native layouts alone do not qualify translated engine
interoperability.

The terminal constructor remains
`ghostty_terminal_new(allocator, out_terminal, uint16_t cols, uint16_t rows)`.
`ghostty_terminal_vt_write` retains parser/UTF-8 state between calls;
`ghostty_terminal_vt_write_until` additionally reports the exact consumed prefix
and requested string/ground boundaries. `get` writes the header-documented type
through its output pointer. Reset preserves dimensions; resize accepts cell pixel
dimensions as well as rows/columns. Unknown option/data keys return
`GHOSTTY_UNSUPPORTED`; enum/status values must come from the release headers.

The 1.0.0 formatter constructor is the three-argument
`ghostty_formatter_terminal_new(allocator, out_formatter, terminal)`.
Configuration uses `ghostty_formatter_set`/`ghostty_formatter_get`; the boundary
sets `GHOSTTY_FORMATTER_OPT_UNWRAP` explicitly. Formatter output defaults to
trimmed plain text. The formatter borrows its terminal, which must outlive it.
Allocated output is freed with the same allocator and exact length through
`ghostty_free`. See the pinned
[formatter header](https://github.com/ghostty-org/ghostty/blob/vt-1.0.0/include/ghostty/vt/formatter.h)
and [API-delta controls](../artifacts/vt-1.0.0/boundary/api-delta-checks.json).

Extensible sized structs start with `size_t size`. Initialize them with
`GHOSTTY_INIT_SIZED` or `ghostty_sized_init`. Their declared base layout is the
minimum accepted size. Input extension bytes beyond the known struct must be
zero; output helpers write only fields that fit and retain the caller's size.
New fields cannot occupy the base layout's trailing padding. The selected
`GhosttyTerminalWriteStop` follows this contract. Fixed `GhosttyStyle` does not
have a size field. See the pinned
[types header](https://github.com/ghostty-org/ghostty/blob/vt-1.0.0/include/ghostty/vt/types.h)
and [sized-validation controls](../artifacts/vt-1.0.0/patches/sized-validator-summary.json).

The authored boundary supplies an address-stable allocator and 64-byte vtable,
including zeroed reserved slots. Its resize/remap callbacks decline in-place
changes; upstream performs relocation/copying. Alignment is a base-2 exponent.
The freestanding C preparation route requires an explicit allocator; native
host defaults are not a translated-runtime allocation contract. Allocator and
vtable storage must outlive every object using them. See the pinned
[allocator header](https://github.com/ghostty-org/ghostty/blob/vt-1.0.0/include/ghostty/vt/allocator.h).

Terminal access must be serialized per instance, including synchronous callbacks;
callbacks must not feed the same terminal recursively. Independent instances may
have independent owners. Userdata remains caller-owned, and PTY reply bytes must
be copied if retained after the callback. Render row/cell views expire at the
next render-state update. The workflow requires no filesystem, clocks, entropy,
network, subprocess, or drawing service; required allocator, callback, panic,
and compiler-runtime services remain explicit translation/runtime concerns.

## Current release qualification and native evidence

The release's [native controls](../artifacts/vt-1.0.0/boundary/native-checks.json)
pass 16 whole-engine oracle cases, two delivery-boundary cases, and six ABI
layouts. Direct/indirect native transcripts agree. Seven additional
[API-delta cases](../artifacts/vt-1.0.0/boundary/api-delta-checks.json) and fourteen
[version/feature cases](../artifacts/vt-1.0.0/boundary/version-build-info-checks.json)
pass, including library version `1.0.0` and the selected scalar feature flags.

The final patches additionally pass
[long-input LLVM native differential checks](../artifacts/vt-1.0.0/patches/print-batches/parity.json):
2,940 cases, 5,670 snapshots, 4,322,970 cells, 2,730 boundary stops, and
3,212,468 input bytes. Exact original/patched observations agree for long ASCII,
controls/CSI, upper bytes, Unicode/graphemes/wide cells, charset mappings, and
`write_until` offsets across batch and row boundaries. The scanner control
exercises u8 lanes 128 to 64 while u16=64 and u32=32 remain unchanged.

The corresponding actual managed engine passes all 45 combined native controls
in raw/processed JIT/whole-library-rooted NativeAOT, with exact transcripts:
[qualification](../artifacts/vt-1.0.0/qualification/qualification.json). Owning
consumer, allocation and published delivery checks pass separately. Full upstream
managed qualification is still pending. Prepared qualification inputs are in
[qualification/prepared.json](../artifacts/vt-1.0.0/qualification/prepared.json).
The old pin's 287-case raw/JIT subset and 2,622-case inventory are historical,
not vt-1.0.0 managed acceptance. [UPSTREAM-TESTS.md](UPSTREAM-TESTS.md) retains
that inventory/history.

## Offline reproduction

Use the matching official source and dependency bundles and the pinned Zig
compiler. Pass the unpacked dependency directory to `zig build --system`;
its hash-named packages supply the complete release closure without remote
fetching. The current recipe can expose that directory as the source tree's
`zig-pkg`. Captured commands include the explicit version strings, scalar feature
flags, and `-fno-sys` integration selections. Missing offline inputs must fail
rather than trigger an alternate engine or fallback.

The reusable patch command is:

```sh
python3 libghostty/patches/vt-1.0.0/apply.py libghostty/ref/libghostty-vt-1.0.0
```

Add `--check` to require the complete recorded source result without writing.
Native commands and private cache/install paths are in the receipts linked
above; direct delivery commands are documented in [DIRECT-ZIG.md](DIRECT-ZIG.md).
Native C consumers require `CC`/`cc` and link with `-lm`. Sources, generated
inputs, tools, scratch products, and native oracles remain distinct from the
managed delivered implementation.

## Historical 0.1.0 C-route and direct baseline

The earlier source was `93967b7f9478e94dcbb42125c8a90f34feb40d75`, committed
2026-10-29, with archive SHA-256
`af551aa496cf54ff734e11563d45fe2c49739ee754d1740d1ac984a67a42dc70`.
It identified the app as `1.4.1-dev` and explicitly built library `0.1.0` with
Zig 0.17.0. Its allocator vtable size 32 and formatter-options size 56 were
observations of that older ABI; they do not describe the 1.0.0 types above.

The C route was accepted on 2026-10-30 for Linux x64 LP64. It used the genuine
Zig C backend's whole-engine intermediate, then DotCC translation. The original
host-target emission retained Linux assembly; explicitly selecting
`x86_64-freestanding-none` produced approximately 19 MB of genuine engine C
without inline assembly. The original Zig/C native oracles covered creation,
ASCII, cursor addressing, erasure, SGR, split UTF-8/wide/graphemes, visibility,
alternate screens, resize/reflow, reset, callbacks, formatter buffers, ordinary
errors, allocator failure, independent owners, and repeated disposal. Two
separate authored-boundary cases and six layouts completed its 24-case suite.

[C-ROUTE-ACCEPTANCE.md](C-ROUTE-ACCEPTANCE.md) retains the accepted raw/processed
JIT/NativeAOT evidence and reproduction receipts. Its products remain under
`generated/c-route/`. [DIRECT-DEFAULT-MILESTONE.md](DIRECT-DEFAULT-MILESTONE.md)
records the working 0.1.0 direct product made canonical on 2026-11-06. Neither
historical acceptance qualifies the current 1.0.0 source upgrade, and no native
Ghostty library is a managed runtime dependency of those delivered products.

## License and notice closure

Distribute Ghostty's MIT `LICENSE`, uucode's MIT `LICENSE.md`, the Unicode and
Hoehrmann notices, Zig's MIT `LICENSE`, and this repository's MIT `LICENSE` for
the embedded shared runtime with both emitted forms. The pinned uucode package
omits its supplemental `licenses/` directory from package exports, so
[source.json](../config/source.json) records an exact-commit license archive at
`1fb73433bba5d93366c57f23ff2e9d7939746500`, SHA-256
`9e574f5b72c07f426eb31e2604860f22ba0fc5f9ccb44ef44e3212a5e9001828`.
Its `license_inputs` map the selected original notices. Historical loose
`ref/licenses/` files are not the current reproducible inputs.

Build-only dependencies retain their notices in their own source trees. Native
controls and acquired build packages do not become native Ghostty providers in
the managed product.

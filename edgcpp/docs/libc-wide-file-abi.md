# Wide FILE and position ABI

The public wide-stream imports and version2 position ABI have passed native
and translated C/C++ static/instance JIT/NativeAOT acceptance. Existing byte,
formatter and scanf regressions also pass. Milestone commit scheduling remains
owned by the coordinator.

The position ABI is explicitly version2 (`__EDG_FPOS_ABI_VERSION=2`):

| Field | Portable type | Offset |
| --- | --- | --- |
| offset | signed 64-bit long | 0 |
| state | unsigned 32-bit opaque decoder state | 8 |
| value | unsigned 32-bit opaque decoder accumulator | 12 |

The struct tag is `__edg_fpos`, size16, alignment8. Strict FPOS* import validation
checks nominal tag, field names/types/offsets, total size and alignment. A
source declaration retaining the old long* shape must be rejected. Old managed
long* helpers may temporarily remain for already generated artifacts; they
must not cause the old source ABI to bind silently.

FILE orientation starts neutral. The first byte or wide operation establishes
it and captures the C/POSIX or UTF8 encoding. Subsequent locale changes do not
change stream encoding. Calls that mix incompatible orientations are undefined
in C; the runtime deliberately reports EINVAL and sets the error indicator.
fwide reports the existing orientation and cannot reverse it. Byte-only
programs retain their prior data, buffering, append and error behavior.

Wide reads use private byte-buffer operations under one FILE lock and decode
UTF8 incrementally without a managed allocation. The C locale maps high bytes
to musl's DF80–DFFF byte-escape code points. Wide writes encode into a four-byte
stack buffer with Rune; unsupported C-locale values and invalid scalars report
EILSEQ. One wide-character pushback is guaranteed, clears EOF, and participates
in logical byte position accounting. Save/restore positions preserve decoder
state; ordinary seek clears pending conversion state. The public state words
are opaque and must only be reused from a successful fgetpos.

Evidence:

* [Native plus eight translated C/C++ static/instance JIT/NativeAOT configurations](../artifacts/campaign/libc-wide-files-translated-20261003-052200/results.json).
* [Sixteen malformed or obsolete position ABI declarations rejected](../artifacts/campaign/libc-fpos-abi-20261003-052200/results.json).
* [Sixteen translated byte-stream and formatter regressions](../artifacts/campaign/libc-wide-files-byte-translated-regression-20261003-052310/results.json).
* [233 scanf cases across native and eight translated configurations](../artifacts/campaign/libc-wide-files-scanf-regression-20261003-052310/results.json).

* [All Unicode scalars, captured encoding, C escapes, orientation, pushback, malformed input, decoder-state replay, strings and owner isolation](../artifacts/campaign/libc-wide-files-runtime-20261003-050200/results.json).
* [Accepted byte-stream, concurrent append, read-lock and write-lock regressions under JIT/NativeAOT](../artifacts/campaign/libc-wide-files-byte-regression-20261003-050500/results.json).

The native oracle writes every valid Unicode scalar through a native wide FILE
under C.UTF-8. The managed runtime decodes it and produces exactly the same
bytes. Saved partial-decoder replay is a synthetic extension test rather than
a promise about host libc state after an encoding error. Executed targets are
Linux x64; other OS execution remains separate. Stream reset via freopen,
wide printf/scanf families and complete stateful encodings remain further work.

Correction: the initial private C-byte test repeated an incorrect DC80 offset.
The wide scanf cross-component check exposed the mismatch with pinned musl
`CODEUNIT`/`IS_CODEUNIT` and the existing LibcText/multibyte implementation.
The runtime now uses DF80–DFFF. An independent native probe over every C-locale
byte uses original pinned mbrtowc/wcrtomb; its output drives the managed check.
The frozen broad stream/timezone snapshot predates this correction; its C-byte
coverage does not validate the correct mapping. Focused correction evidence is
recorded separately before milestone integration.

Correction acceptance:

* [Original pinned musl all256 C-byte oracle plus exhaustive UTF-8/JIT/AOT](../artifacts/campaign/libc-wide-files-c-escape-20261003-055700/results.json).
* [Corrected wide FILE public APIs in native and all eight translated modes](../artifacts/campaign/libc-wide-files-c-escape-translated-20261003-055500/results.json).
* [Wide scanf cross-component C and UTF-8 checks in all eight translated modes](../artifacts/campaign/libc-wide-scanf-translated-20261003-055500/results.json).

# Selected resumable conversion algorithms

This component retains all twenty C sources from musl 1.2.6's multibyte directory,
its UTF-8 state table, original license, original files and replayable patches.
The release hash is
`d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a`.
source.json records every original/adapted file hash. Rebuild in a fresh directory
with `backend/tests/vendor_musl_multibyte.py --musl-root PATH --output-dir PATH`.

The selected definitions cover mbrtowc/mbrlen/mbsinit/wcrtomb, bounded and
unbounded restartable string conversion, legacy mbtowc/mblen/mbstowcs/wcstombs/
wctomb, btowc/wctob and C11 mbrtoc16/mbrtoc32/c16rtomb/c32rtomb. This is byte-string
conversion, not wide FILE I/O or wide numeric parsing.

Caller-provided mbstate_t follows the existing portable eight-byte ABI. These
musl algorithms use its first 32-bit word for partial UTF-8 or a pending UTF-16
surrogate. The meaning of that word is private to the implementation. A zero
state is initial; valid completion resets it. Native OS mbstate_t pointers never
cross the managed boundary.

Original musl's function-local null-state variables are deliberately changed from
static unsigned to translated TLS. Each of mbrtowc, mbrlen, mbrtoc16, mbrtoc32,
c16rtomb and mbsnrtowcs owns a distinct hidden state. Instance mode supplies an
independent set per program owner and per thread; ReleaseThread discards them.
Functions forwarding explicit state do not touch another API's hidden state.
This strengthens concurrency isolation without changing the conversion algorithm.
Wcrtomb/c32rtomb and the legacy stateless forms need no restartable hidden state.

Locale selection remains in the single LibcText/LibcLocale provider, reached
through the existing MB_CUR_MAX bridge. C/POSIX preserve high bytes through
musl's U+DF80..U+DFFF code-unit escape convention. C.UTF-8 accepts Unicode scalar
values, rejects surrogates/overlong encodings/out-of-range values, and encodes up
to four bytes. A later switch between incompatible locales while holding a
partial conversion state does not gain a new defined contract from this port.

Other adaptations are bounded platform/C++ glue: remove internal Linux locale
layout/visibility dependencies, use the portable MB_CUR_MAX, replace implicit
void-pointer conversions with exact typed casts, put the UTF-8 table declaration
under C linkage for mixed input, map restrict for C++, and express wcstombs's
compound-literal cursor as an ordinary local pointer. Mbstowcs likewise uses a
local cursor to avoid implicitly dropping restrict on a C++ pointer-to-pointer. The original state-machine
arithmetic and return/count/error logic remain unchanged. Scratch arrays are
ordinary translated automatic storage; no short-lived pinned arrays are added.

`libc_multibyte_verify.py` independently compiles original and adapted C under a
synthetic fixture. Its scalar corpus covers every Unicode scalar through UTF-8
encoding/decoding and every split/resume boundary, UTF-32, sampled UTF-16 pairs,
malformed/overlong/surrogate encodings, zero/reset contracts, bounded string
positions, C byte escapes and error behavior. The generated C/C++ static and
instance harness additionally exercises six independent hidden states,
foreign-owner separation, worker-thread separation and ReleaseThread reset.
Native original/adapted evidence is in
`artifacts/campaign/libc-multibyte-native-20261003-030020/results.json`.
All nine checks passed at
`artifacts/campaign/libc-multibyte-translated-20261003-030450/results.json`, including
all C/C++ static/instance JIT/NativeAOT modes and the six-API hidden state tests.
The initial C++ cursor-qualification failure is retained in the preceding
`libc-multibyte-translated-20261003-030110` campaign. Automatic provider integration
uses the accepted twenty translation units.

Native reference runs use the host's locale name only to choose MB_CUR_MAX==1
versus multibyte. The conversion implementation and table themselves come from
the pinned source. Execution evidence is Linux x64 until Windows/macOS runners
are available.

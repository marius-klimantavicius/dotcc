# Selected scanf source port

The component provides scanf, fscanf, sscanf, vscanf, vfscanf and vsscanf.
The core remains pinned musl 1.2.6 vfscanf, using the same integer and binary80
floating scanners as the existing numeric conversions. It supports suppression,
field widths, scan sets, count stores, positional arguments, allocation forms,
ordinary/wide destinations, and the existing C/POSIX/C.UTF-8 locale profile.
It is not a full libc, wide-stream, or floating-environment implementation.

## Input and locking

A new callback-input mode extends the shared 32-byte numeric scan cursor. Its
separate 48-byte input state lives on the stack and records count, limit,
EOF/limit state, a single pending byte and a typed read callback. Existing
narrow and wide conversions keep their direct-memory input paths. Scanf calls
the numeric cores in their strict, single-pushback mode; strto/wcsto retain
unlimited pointer rewind. Both previous numeric corpora were rerun.

Memory scanning reads the caller's bytes directly. FILE scanning reads through
the existing BCL-backed fgetc implementation, holds one lookahead byte locally,
and restores it with ungetc at return. Width exhaustion does not synthesize
an underlying stream EOF. Public fgetc/ftell/feof can therefore observe the
correct state after a scan. Neither path allocates a transcoding buffer.
The `%m` extension uses the ordinary translated malloc/realloc/free heap and
preserves musl's failure cleanup.

The private `__edg_with_file_read_lock` import has the same strictly validated
FILE/callback/va_list signature as the output formatter lock. It invokes a
managed function pointer with the explicit owner, after PrepareRead, under a
monitor whose release is protected by finally. Reentry is supported; another
thread cannot read between callback chunks. Canonical routine-address adapters
remain in use. The lock itself allocates nothing after initialization.

Dotcc's ScanfReader and FILE interfaces were reviewed. This port retains the
source parser and software binary80 arithmetic instead of relying on its
managed text reader and narrower typed conversion paths. EDG required no change.
C++ parse adjustments are explicit void-pointer conversions, size_t -1/-2 case
labels and a local byte replacing an addressed compound literal.

## Provenance and oracle

The musl release SHA256 is
`d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a`.
The original vfscanf source, patch, copyright, adapters and source hashes are
retained in `backend/runtime/musl/scanf`. The deterministic generator reproduces
all retained files exactly:
[reproduction proof](../artifacts/campaign/libc-scanf-revendor-20261003-044720/reproduction.json).
[Patch/reference replay](../artifacts/campaign/libc-scanf-patch-replay-20261003-044930/results.json)
checks the core and private adapters.

The native reference compiles the original vfscanf, intscan, floatscan and
shgetc sources against a minimal memory FILE boundary. Its multibyte routines
are also pinned original musl. An expanded malformed-input case exposed why
that matters: this host's glibc mbrtowc accepts U+110000, while musl rejects it.
The first contaminated comparison and subsequent correction are preserved;
the generated implementation did not change. The verifier now snapshots all
reference dependencies from versioned component files, without depending on
an ignored full source download.

## Acceptance and limits

* [Initial 219-case native and all8 generated configurations](../artifacts/campaign/libc-scanf-translated-20261003-043120/results.json).
* [Expanded 233-case original/adapted native proof](../artifacts/campaign/libc-scanf-durable-native-20261003-044930/results.json), including malformed UTF-8, malformed formats and count-store widths. [All8 generated binaries pass the expanded corpus](../artifacts/campaign/libc-scanf-expanded-native-20261003-044110/expanded-generated-results.json).
* [Allocation failure matrix](../artifacts/campaign/libc-scanf-failure-20261003-044400/results.json): first malloc failure, realloc growth failure, and failure after an earlier successful assignment.
* [Read-lock JIT/NativeAOT proof](../artifacts/campaign/libc-scan-lock-20261003-043350/results.json): static/owner callbacks, foreign-owner and null rejection, deterministic concurrency, reentry, foreign CLR exception cleanup, and zero steady-state allocation.
* [Seventeen import ABI rejections](../artifacts/campaign/libc-read-lock-abi-20261003-044400/results.json), including six malformed read-lock contracts.
* Shared cursor regressions: [2,575 narrow cases](../artifacts/campaign/libc-scanf-narrow-regression-20261003-043520/results.json) and [12,277 wide cases](../artifacts/campaign/libc-scanf-wide-regression-20261003-043520/results.json), each in all8 generated configurations.

FILE tests cover lookahead, position, width-limited reads, EOF, subsequent reads,
va_list entry points, stdin projection, and write-only input failure. Variadic
function addresses are exercised. All generated evidence is Linux x64,
C/C++ × static/instance × JIT/NativeAOT.

Wide-destination conversions retain the pinned core's byte-based input limit,
audited below. The separate wscanf family and wide FILE orientation have their
own accepted components.
Numeric overflow cases beyond representable scanf destination results have C
undefined behavior; observed musl behavior is not a new guarantee. Arbitrary
locales, directed rounding/fenv, and Windows/macOS execution remain open.


## Wide destination field-width audit (2026-10-03)

The byte limit is deliberate. C11 N1570 section7.21.6.2 footnote286 explains
that c/s/[ input boundaries are determined byte by byte. The l modifier adds
multibyte-to-wide conversion after that matching rule; it does not redefine a
narrow stream's field extent as a count of destination wchar_t elements. The
2013 POSIX fscanf reference also describes c and scanset matching in bytes.
The separate fwscanf family counts wide input characters.
[C11 draft](https://www.open-std.org/jtc1/sc22/wg14/www/docs/n1570.pdf),
[POSIX reference](https://pubs.opengroup.org/onlinepubs/9699919799.2013edition/functions/fscanf.html).

Consequently, UTF-8 `éx` with a narrow `%2lc` field consumes the two bytes of é
and stores one wchar_t under this profile. The observed host glibc consumes é
and x for the same format. Matching glibc here would change the chosen ISO byte
matching contract. When a byte limit cuts a multibyte sequence, the retained
musl path stops with input failure; this observation is not a portable guarantee
for every implementation or invalid source input.

`libc_scanf_wide_fields_verify.py` adds 246 cases covering widths1–7, lc/ls/l[,
assignment suppression, one-to-four-byte UTF-8, exact boundaries, incomplete
characters and invalid input. Original isolated musl and adapted native sources
agree on all246. The native host differs on160 records; complete results,
errno/count and guarded destination bytes are retained as observations.
All eight previously accepted generated C/C++, static/instance, JIT/NativeAOT
binaries pass the same new records. Their executable, fixture and actual source
inputs are hashed in `reused-inputs.json`; documentation/reference-only additions
to provenance do not pretend to be generated code changes.
Evidence: `artifacts/campaign/libc-scanf-wide-fields-20261003-052000/results.json`.
The corpus is now included in the ordinary scanf verifier (479 total memory
records), so future full builds exercise it without an extra translation matrix.
No musl or EDG implementation was modified for this audit.

This closes the specific byte-versus-wide-count question for the chosen
C/POSIX/C.UTF-8 profile. It does not establish support for arbitrary locales,
stateful non-UTF8 encodings, or every library's extension policy.


## Representable decimal-width arithmetic correction

Both pinned vfscanf and vfwscanf parsed each digit using
`10*width + *p - '0'`. At a valid decimal width of2147483600, adding the encoded
last digit before subtracting the character zero overflows a signed int, even
though the final width is representable. The adaptation computes the digit
first: `10*width + (*p - '0')`. For every prefix of a width in0..INT_MAX, each
multiplication and addition now remains representable. No character matching,
field extent or EDG lowering was changed. The separate upstream discussion
identifies the same intermediate boundary:
[musl width regression discussion](https://www.openwall.com/lists/musl/2026/08/05/29).

Original sources remain unchanged, with updated exact replay patches and
adapted hashes for both cores. `libc_scanf_width_verify.py` retains UBSan failures
from the originals and verifies widths2147483599,2147483600,2147483646 and INT_MAX
using suppressed c and ordinary s conversions, tiny input, count stores and
guarded destinations. The corrected sources pass UBSan and all eight combined
translated C/C++, static/instance, JIT/NativeAOT modes:
`artifacts/campaign/libc-scanf-width-translated-20261003-052800/results.json`.
Patch proof: `libc-scanf-width-replay-20261003-052600/results.json`.
The broader479 narrow records and271 wide records retain original/adapted native
agreement in the corresponding `libc-scanf-width-{narrow,wide}-regression-20261003-053000`
outputs.

This does **not** establish a C-imposed INT_MAX maximum for the format's decimal
width. Larger widths still exceed the pinned cores' signed-int representation;
extending/checking that input-extent contract is an explicit remaining gate.
The accepted change does not claim defined behavior for arbitrarily large widths
or adopt a new rejection/saturation policy silently.

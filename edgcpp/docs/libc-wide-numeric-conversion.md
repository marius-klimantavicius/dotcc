# Wide numeric conversion with the shared musl scanner

The accepted component supplies `wcstol`, `wcstoul`, `wcstoll`, `wcstoull`,
`wcstoimax`, `wcstoumax`, `wcstof`, `wcstod`, `wcstold` and seven explicit-locale
forms (all except intmax/uintmax). This is selected libc coverage, not a claim
that libc or wide I/O is complete.

## Source adaptation and storage

The pinned musl 1.2.6 release has SHA256
`d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a`.
Original `src/stdlib/wcstol.c` and `wcstod.c`, their replayable patches, MIT
copyright text and exact original/adapted hashes are retained in
`backend/runtime/musl/wide_numeric`. The deterministic vendoring script reproduces
all ten retained source/provenance files byte-for-byte:
[`reproduction.json`](../artifacts/campaign/libc-wide-numeric-revendor-20261003-034730/reproduction.json).
Patch replay and adapter hashes also pass:
[`results.json`](../artifacts/campaign/libc-wide-numeric-patch-replay-20261003-034930/results.json).

Musl's original wide wrapper uses a fake FILE and a 64-byte conversion buffer
before calling the same intscan/floatscan algorithms as narrow conversions.
The adaptation replaces only that input boundary. The cursor now records a
one-byte or four-byte element width, reads wchar_t directly, and computes end
positions in elements. Non-ASCII and terminating NUL map to musl's existing
`@` stopping marker. Wide whitespace uses the pinned iswspace implementation.
Failed conversion returns the original input address even after whitespace.
The remaining wrapper bodies and shared parse algorithms retain their source
behavior, except for the already documented float errno corrections.

The cursor is a 32-byte stack value in the LP64 profile. There is no temporary
transcoding allocation, pinned character buffer, or duplicate scan. Narrow
reads incur a predictable width branch; no throughput improvement is claimed.
Binary80 arithmetic can still allocate BigInteger intermediates. This work
therefore does not claim that the entire floating parser allocates nothing.

Explicit locale wrappers validate owner handles before input access. Supported
locales are C/POSIX/C.UTF-8; arbitrary locales and non-dot decimal separators
remain unsupported. NaN/infinity leave errno unchanged; finite overflow and
all nonzero subnormals set ERANGE, including exact subnormals. Invalid parsing
retains musl's EINVAL extension. The profile remains nearest-even, without
claiming full fenv support.

## Verification

The pre-change native narrow parser and width-aware native parser agree on all
2,575 existing cases. The new cursor also passes all eight generated
C/C++ × static/instance × JIT/NativeAOT configurations:
[`narrow results`](../artifacts/campaign/libc-wide-cursor-narrow-regression-20261003-032310/results.json).

The wide verifier derives expected values, errno and consumed counts from that
narrow oracle. It checks plain text, Unicode whitespace prefixes, non-ASCII
terminators and explicit locale forms, as well as null endptr and invalid
locale contracts. All 12,277 cases agree in native C and every generated
configuration:
[`wide results`](../artifacts/campaign/libc-wide-numeric-translated-20261003-034510/results.json).
The corpus includes invalid bases, signed/unsigned boundaries, hex and decimal
floats, binary80 subnormals/extreme exponents, incomplete exponents, NaNs,
infinities, and 19,000-digit strings. Source snapshots and all intermediates
remain retained. The first generated fixture omitted its selected `wcslen`
dependency; that failure is retained in the earlier `...034250` directory.
It required no EDG or backend fix.

Execution evidence is Linux x64. The generated implementation uses the portable
LP64/binary80 profile; Windows/macOS execution remains separate acceptance.
Wide stream orientation/scanning, most math/fenv, and other libc domains remain
open in the completion ledger.

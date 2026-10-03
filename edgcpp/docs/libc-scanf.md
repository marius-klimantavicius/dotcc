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

Wide-destination conversions retain the pinned core's byte-based input limit;
complete cross-implementation wide-field conformance remains an audit item.
The separate wscanf family and wide FILE orientation are not supplied here.
Numeric overflow cases beyond representable scanf destination results have C
undefined behavior; observed musl behavior is not a new guarantee. Arbitrary
locales, directed rounding/fenv, and Windows/macOS execution remain open.

# Native and translation configuration

The starting manifest is `config/core-sources.txt`, relative to the pinned
repository root. `config/core-defines.txt` selects release diagnostics off and
real wide literals on. `pinta-preinclude.h` provides explicit standard type and
wide-character declarations. Debug uses `PINTA_DEBUG=1` and keeps diagnostic code.

Linux x64 native uses `-std=c11 -fshort-wchar -funsigned-char
-fno-strict-aliasing` with `-O2 -g` (release) or `-O0 -g` (debug). The short-wchar
ABI changes literals and storage to 16-bit; system libc still has 32-bit wide
string routines, so `native-preinclude.h` maps all upstream test uses of `wcslen`,
`wcscmp`, `wcstombs_s`, and the lone literal-only `swprintf` call to authored
UTF-16 adapters. The `swprintf` adapter deliberately rejects percent directives,
since the sole upstream use copies `Some data` without formatting. The upstream Windows main
is replaced with `tests/native-main.c`, which returns SPUT's real exit status,
supports individual group processes, and removes interactive input. Message
formatting handles the actual upstream `%d`/`%ls` surface with UTF-16 to UTF-8
conversion; diagnostic putwchar also uses this converter. Test assertions remain unchanged. Pristine registrations stay untouched; the
corrected stage wraps three weak/encoding direct calls with the existing
`sput_run_test` macro so those groups satisfy SPUT's registration contract.

File callbacks are test-only, opened read-only under explicit `PINTA_FIXTURES`.
Windows-style upstream test paths map to fixture basenames, with no access outside
the configured fixture directory. Each open has an independent FILE cursor;
short reads return actual bytes. This adapter is not the managed product host.

`tests/native-abi.c` prints actual sizes, alignment, key field offsets, wide
literal storage including Ž and a surrogate pair, terminator, decimal scale, and
stack offset bound. Pointer-bearing records are separate from the 84-byte
fixed-width module header; native measurements must be compared with actual
translated storage before declaring managed ABI parity.

`PINTA_FRAME_PREV_OFFSET` is 0x3fffffff reference slots. On x64 its largest
interframe gap is 8,589,934,584 bytes (mask times eight), not the historical
comment's one gigabyte. The API's uint32 arena/stack lengths are stricter.
Creation additionally requires `stack_length <= UINT32_MAX - sizeof(PintaStackFrame)`
to prevent the frame allocation's uint32 length from wrapping. The corrected
stage enforces that bound and checks missing heap/frame/cache/internal-function
allocations plus short misaligned arena underflow. Owning hosts must provide
practical arena limits too; these guards do not establish hostile-input safety.

Windows execution and its LLP64 profile are deferred at the user's request.
The current qualification target is Linux x64; no Windows result is claimed.

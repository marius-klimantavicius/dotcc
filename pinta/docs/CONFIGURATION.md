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

File callbacks are test-only. The runners embed an absolute default fixture
directory (`PINTA_TEST_FIXTURE_ROOT`), so direct executable runs do not require
the Visual Studio working directory. `--fixtures DIR` selects that directory
when building/running; otherwise the runners honor `PINTA_FIXTURES` and then use
the pinned `Marius.Pinta.Test.Files` directory. At runtime, a nonempty
`PINTA_FIXTURES` overrides the embedded default, including after moving a binary.
An explicitly missing override fails instead of silently selecting another root.
Windows, Unix and mixed separators map to fixture basenames inside that root.
Each open has an independent read-only FILE cursor; short reads return actual
bytes. This adapter is not the managed product host.

Both upstream runners invoke `--check-fixtures` from a temporary directory.
It compares complete fixture bytes through six path spellings, using the
embedded default and an override directory containing spaces. A missing-root
check verifies that a bad override is reported. These resolver checks are
recorded separately from the original upstream assertions.

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

## Upstream test diagnostics

The corrected native and translated upstream harnesses report expected and actual
Pinta results by enum name, decimal value, and hexadecimal value, for example
`PINTA_EXCEPTION_INVALID_MODULE (14 / 0x0000000E)`. Unknown values retain their
numeric codes under `PINTA_EXCEPTION_UNKNOWN`. This changes only the assertion
message through `test-exception-diagnostics.patch`; the assertion and engine
behavior are unchanged. Failed assertions appear in runner output and JSON
`failure_details`, as well as the full per-case logs. Native process signals are
reported separately (for example `SIGSEGV`), never interpreted as Pinta codes.

Set `PINTA_TRACE=1` when invoking an upstream test executable to log resolved
fixture paths, the loader's original Pinta result, bytecode offsets/opcodes,
operand-stack object kinds, GC traversal and relocation-buffer capacity. For example:

```sh
PINTA_TRACE=1 pinta/build/native-corrected-debug/tests --case code_globals_properties_v2 > pinta-trace.log 2>&1
```

The same tracing is available in translated upstream executables. Trace hooks
are test-only and disabled at runtime unless requested; product builds do not
define them. The corrected GC calculates relocation capacity from the available
byte count, then divides by `sizeof(PintaHeapReloc)`. It does not subtract cast
structure pointers, whose distance may not represent a whole number of entries.

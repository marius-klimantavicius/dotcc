# Foreign target inputs and first translation probes

The SDK source group acquires Zig 0.15.2 from its [official release
index](https://ziglang.org/download/index.json). `config/toolchains.json` records
host archives, SHA-256 values, target triples and ordered definitions. The compiler
and bundled actual MinGW/Darwin headers remain under ignored `ref/`; no invented
SDK declarations or system-wide installation is used. Zig's MIT license and all
bundled component/header notices remain in the acquired tree. This is a header
and compiler input, not a Windows/macOS execution runner or a full Apple SDK claim.

```bash
bash libuv/scripts/fetch.sh --group sdk --fetch missing
bash libuv/scripts/test.sh --suite foreign-headers --fetch never --jobs 4
```

Receipt `artifacts/campaign/20260929-075104-0fa631fa/receipt.json` proves:

| Target | Preprocessed / native syntax passed | ABI observations |
| --- | --- | --- |
| Windows x64 MinGW/UCRT | 37 / 37 | pointer 8, long 4, wchar_t 2; uv_loop_t 472, uv_tcp_t 320, OVERLAPPED 32 bytes |
| Windows arm64 MinGW/UCRT | 37 / 37 | pointer 8, long 4, wchar_t 2; same listed libuv/OVERLAPPED sizes |
| Darwin x64, macOS 11 target | 37 / 37 | pointer/long 8, wchar_t 4; uv_loop_t 1072, uv_tcp_t 264 bytes |
| Darwin arm64, macOS 11 target | 37 / 37 | pointer/long 8, wchar_t 4; same listed libuv sizes |

All 148 source/target combinations use unchanged pinned source and genuine target
headers. `foreign-headers.json` records every source, header dependency, exact
preprocessed path/hash, native syntax exit, selected macros, record measurements
and full AST/layout transcript locations. Both Windows profiles define `_UCRT`
after including `uv.h`. These are native **compile-time** layouts; emitted/native
ABI equality and execution on these hosts remain unproven. MSVC configuration,
Apple restricted SDKs, OS-version branches and other architecture variants remain
open even though these four source sets preprocess.

The initial invocation of `zig cc -fsyntax-only` reached Clang diagnostics but the
Zig wrapper expected an object afterward and returned `FileNotFound`. The suite
now uses `zig cc -E` for the real target preprocessing and bundled `zig clang`
with the corresponding Clang triple for syntax/AST/layout diagnostics. Every
native command runs in a private directory. No source/header change fixed this
harness issue. The initial failed receipt is retained separately.

Direct whole-preprocessed-body DotCC probes of version/core/filesystem units fail
12/12 at unsupported SDK declaration syntax. The original native line markers
are canonicalized to standard `#line` without changing C tokens. Logs and compiler
fingerprint: `artifacts/foreign-dotcc-probes/result.json`. Command:

```bash
python3 libuv/scripts/foreign-dotcc-probe.py \
  --compiler libuv/build/foreign-compiler-snapshot/dotcc.dll \
  --headers-report libuv/artifacts/campaign/20260929-075104-0fa631fa/foreign-headers.json
```

The exact native AST dependency-closure helper now accepts `--preprocessed`,
`--ast`, `--target-id`, `--target-triple` and `--input-receipt`. It does not pretend
that a native target triple changes DotCC's current ABI. Using the matching
native AST/preprocessor inputs, actual `src/version.c` definitions materialize
and emit in **all four** target configurations. Evidence:
`artifacts/foreign-dotcc-probes/version-closures.json` and per-target
`artifacts/native-closures/*/src/version/receipt.json`. These small actual source
bodies do not establish platform backend translation. Full native-header
translation, target ABI support, external-call binding and backend aggregation
remain required.

# Native / translated layout probe

Run after building the actual translated product:

```sh
python3 valkey/tests/abi/probe.py \
  --assembly valkey/generated/TranslatedValkey/bin/Release/net10.0/TranslatedValkey.dll \
  --output valkey/artifacts/abi/product
```

The probe verifies the already downloaded source pin, compiles a native C probe
against those headers, and compiles a small C# consumer against a copied product
assembly. It never rewrites reference or staged sources and does not rebuild the
compiler or product. The receipt includes source pin/manifest identity, assembly
SHA256, commands, checks and differences. `dictEntry` is opaque in `dict.h`; its
exact private declaration is copied from `dict.c` into a probe-only header with
both original file and extracted declaration hashes.

The Linux little-endian LP64 comparison covers 133 measurements across 21 types:
size, alignment, ordinary/fixed-array/flexible-tail/promoted-member offsets, plus
raw `serverObject` bitfield bytes. It includes packed SDS headers, `dictEntry`,
`dict`, callback tables, `client`, `valkeyServer`, event-loop and connection
records, and Lua state, value, call-frame and debug records.

Two expected differences remain visible in the receipt: native `aeEventLoop`
has size 128 and `flags` at offset 120; the managed layout has size 88 and `flags`
at offset 84. `pthread_mutex_t` is a managed integer registry handle, while
native glibc embeds a 40-byte mutex. All translated code and managed pthread
helpers use the same handle layout. Such containing records cannot be exchanged
with a native library. The probe reports `exact_match: false` for these deltas;
`passed` means there are no additional differences.

Callback pointer sizes and positions match, but translated instance callbacks
also take their managed owner argument. Layout equality does not imply callable
native callback ABI compatibility. The probe is a representation check, not
coverage of every struct or the entire command surface.

Qualification on the current startup product: all 133 checks passed with the two
expected mutex-containing deltas; the other 131 measurements matched exactly.
The receipt is `artifacts/abi/latest/receipt.json`.

# Unsupported runtime surface audit

The translated program contains unsupported POSIX functions because the full
upstream source closure is compiled. They are not evidence of required-command
runtime support:

- `getrlimit`/`setrlimit`, process signal setup, pthread cancellation setup and
  watchdog setup are bypassed through typed host function overrides. `signal`,
  `sigaction` and `pthread_sigmask` explicitly report unsupported operations.
- The hosted entry bypasses upstream `main`, daemonization, sentinel startup and
  host system checks. Thus `umask`, `setsid`, process `execve` and system-check
  `mmap` calls are outside this entry path. RDMA is not enabled.
- Startup/config guards disable syslog, watchdogs, crash diagnostics, extra IO
  threads and automatic fork-based persistence. Command admission also checks
  execution from Lua/modules and transactions. Background SAVE/rewrite,
  replication, dynamic module loading, debug commands and process-policy
  mutations are outside the admitted profile. `fork` still returns `EPERM`.
- Cooperative BIO sentinel drain/join handles normal cleanup. Crash-only
  `pthread_cancel`, native symbol diagnostics (`dladdr`) and debug mmap behavior
  remain unsupported; they have not been qualified as normal host operations.
- Unix-socket group changes (`getgrnam`) and dynamic Lua native loaders require
  configurations that the managed options guard does not admit.

This is static source/guard inspection, not an exhaustive reachability proof.
No additional unsupported call was identified on the admitted startup and
foreground persistence paths in this audit. Native file/process query imports
remain in the shared libc implementation where used; those are distinct from
unsupported stubs and must remain part of NativeAOT/platform qualification.

# Notice propagation audit

`config/licenses.json` inventories six standalone notice/license files and the
inline-notice-bearing `deps/fast_float/ffc.h`. Generated C# omits original source
comments. Standalone notices alone omit individual notices such as Lua CJSON's
Mark Pulford MIT notice and per-file Redis/Valkey copyright text. Lua's full MIT
notice in `deps/lua/src/lua.h` occurs near the end of that header.

`scripts/notices.py` now creates a deterministic distribution bundle preserving
complete matching comment blocks from every pinned source/header in
`config/sources.json`, including
contiguous `//` blocks and comments outside the file prefix, together with all
standalone notice files. Original relative paths and verified file hashes should
identify each extracted notice. `ffc.h` contributes its complete inline
MIT/Apache/Boost notice block. The bundle accompanies build/publish output as
well as generated sources.

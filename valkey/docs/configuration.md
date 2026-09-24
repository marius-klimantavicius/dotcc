# Valkey standalone translation profile

The pinned 9.1.2 native control was built and exercised on Linux x64 on
2026-09-24. This is source/build evidence; the managed server is not yet qualified.
[sources.json](../config/sources.json) freezes the observed C/dependency closure.
[profile.json](../config/profile.json) classifies all 425 upstream command and
subcommand definitions: 307 required, 76 deferred and 42 translated but
unqualified. These labels express the delivery requirement, not passing managed
results. Deferred handlers remain in the source closure; C# host guards reject
their dispatch and unsupported configuration before upstream side effects.

[licenses.json](../config/licenses.json) records the selected dependency notice
files and hashes, including Lua, libvalkey, fpconv, histogram and the fast-float
header. Generated distributions must retain these and each translated source's
own notices; the source manifest is not a replacement for redistribution notices.

## Reproduction and staging

```sh
./valkey/scripts/oracle.sh
./valkey/scripts/oracle.sh --no-fetch
```

Both verify pinned inputs through `inputs.prepare_inputs`. Fetching is enabled
by default. The explicit `--no-fetch` argument reuses verified cached references
and never authorizes downloading missing references. The shell wrapper sources
`common.sh`; Python subprocesses and Make's `PYTHON` use the resolved interpreter.
Native compiler, GNU Make, binutils and Tcl 8.5 or newer must already be installed.
`--build-only` omits execution checks; `--jobs N` controls native build parallelism.

The verified tree is copied to `build/native/source/`; compilation and generators
never write to `ref/`. Command tables and formatting definitions are regenerated
with the selected interpreter. `release.h` records the pinned eight-character
commit, clean source state and fixed `dotcc-valkey-9.1.2` build identity. The staged
`src/mkreleasehdr.sh` is replaced with a no-op after writing that header: GNU Make
still evaluates the upstream immediate shell expression even when `release_hdr=`
is passed on its command line. Without disabling the staged hook, it picks up
this repository's Git identity and host/time metadata. This one build-hook change
is hashed in `artifacts/native/generated-inputs.json`; it does not modify product
C or reference files. Generator inputs, outputs, interpreter identity and
command JSON hashes are retained in that receipt.

## Observed native build and source closure

| Setting | Selected value |
| --- | --- |
| Allocator | `MALLOC=libc` |
| Lua | `BUILD_LUA=yes` (statically registered module/interpreter) |
| TLS / RDMA | `BUILD_TLS=no`, `BUILD_RDMA=no` |
| systemd | `USE_SYSTEMD=no` |
| Optimization | `OPTIMIZATION=-O1`, avoiding default LTO |
| Dependency tracking | `CFLAGS=-MMD`; server adds its own `-MMD` |
| Language | Server/module GNU C11; libvalkey/histogram C99; upstream default Lua |
| ABI | Linux x64 LP64, little endian, 64-bit pointers |

The native server links libc/libm and platform threading/dynamic-loader services.
These are native oracle dependencies, not authorization for native product
backends. Per-group `defines` and `include_dirs` in `sources.json` are the explicit
upstream options. Compiler-predefined operating-system/CPU macros are not copied
into managed flags. In particular, native Linux preprocessing selects epoll and
other OS facilities which still require an explicit managed host contract.

| Source group | C units | Evidence |
| --- | ---: | --- |
| Server | 119 | Recorded `ENGINE_SERVER_OBJ`, including seven trace units |
| Static Lua module | 5 | Whole-archive members in the actual server linker map |
| Bundled Lua | 32 | Interpreter/extension members selected by the actual link |
| libvalkey | 6 | `async`, `net`, `valkey`, `alloc`, `conn`, `read` members |
| Histogram | 1 | `hdr_histogram` archive member |
| Floating conversion | 1 | `fpconv_dtoa` archive member |
| **Total** | **164** | Complete native server C closure |

There are 141 transitive project dependency records from compiler `.d` files,
including generated definitions and C files that upstream includes textually.
System headers are not copied into the managed include set. Fast-float is a
header dependency. The server's retained Sentinel code pulls six libvalkey units
even though Sentinel execution is deferred. The library's standalone SDS/dict,
cluster client and other unselected archive members are not added speculatively.
Lua CLI/compiler and unused Lua library members are likewise excluded based on
the actual server link. CLI sources and linenoise are native test-only inputs;
Tcl tests and C++ GoogleTest drivers are never translated as C.

`artifacts/native/build.log` records every native invocation. `server-link.map`
and `cli-link.map` preserve their distinct archive evidence; `link.map` is the
server map used by the manifest generator. CLI and server builds use identical
flags because changing the link flags triggers upstream object/dependency cleanup.

## Runtime selection and native evidence

The baseline binds loopback, uses one I/O thread, disables scheduled RDB saves
and automatic AOF rewrites, and tests two startup profiles: RDB without AOF, and
AOF enabled from startup with `appendfsync always`. It never invokes fork-based
background persistence. The user's authorized interim libc behavior is
`fork() == -1` with an appropriate error `EPERM`; fake child success is
not permitted. Foreground SAVE and initial AOF replay remain required managed
features. The managed configuration and dispatch layers reject deferred
background commands and related automatic options explicitly.

The native control passed 20 checks covering binary SET/GET, pipelined and
fragmented requests, lists/hashes/sets/sorted sets, bundled Lua with JSON and bit
helpers, MULTI/EXEC, RESP3 negotiation, foreground SAVE/RDB reload, startup AOF
append/replay and native CLI connectivity. The pinned upstream `unit/protocol`
Tcl suite passed 35 assertions with zero failures. The suite config disables
scheduled saves and automatic AOF rewrites. These bounded baseline results do
not cover the full required command profile, durability across host crashes,
managed execution or interoperability with a managed implementation.

The machine-readable execution receipt is `artifacts/native/receipt.json`;
server data/logs are under `build/native/baseline/`. Required reproducibility
includes running the script again: ignored local artifacts alone are not a
portable completion claim.

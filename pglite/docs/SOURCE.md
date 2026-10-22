# PGlite source, profiles and native control

The acquired product is the real
[electric-sql/postgres-pglite fork](https://github.com/electric-sql/postgres-pglite/tree/29b6424aa312ec3556247db069939006dd27f878).
On 2026-10-22, GitHub's repository API and `git ls-remote --symref ... HEAD`
identified its live default branch as `REL_18_6-pglite` and its HEAD as
`29b6424aa312ec3556247db069939006dd27f878` (PostgreSQL 18.6).
All pins, URLs, archive layouts, checksum and notice paths are in
[`config/sources.json`](../config/sources.json). Source upgrades change metadata.

The immutable archive SHA-256 is
`175f2e1bbef0b726d7bf18b3533689ca501b738d127d72d917c00fc42c91342b`.
Its extracted tree is under `ref/postgres-pglite-<revision>/`. Acquisition uses
the framework's safe archive extraction when invoked through the recipe.
Existing usable references are accepted and never patched by preparation. A
content fingerprint records the observed local tree independently of the
requested revision; it does not certify that an edited tree equals the pin.

The product archive preserves PostgreSQL's `COPYRIGHT`, all per-file notices,
and the separately licensed minimal ICU data notice. Third-party extension
submodules are absent from GitHub archives. They are excluded from this first
workload and are not silently replaced by other sources.

## Upstream embedding and separate wrapper inputs

The pinned fork contains its own
[PGlite development guide](https://github.com/electric-sql/postgres-pglite/blob/29b6424aa312ec3556247db069939006dd27f878/README-PGLITE-DEV.md),
[build script](https://github.com/electric-sql/postgres-pglite/blob/29b6424aa312ec3556247db069939006dd27f878/build-pglite.sh),
Docker launcher and builder Dockerfile. The embedding changes use
`__PGLITE__`, `PostgresMainLoopOnce`, `PostgresMainLongJmp`, callback transport,
libc macro interception, synthetic keep-alive exit 99 and error exit 100.
`pglitec.c` is compiled **before** the libc remap defines are applied to backend
objects; applying those defines to the wrapper itself would recurse.

The launcher selects `electricsql/pglite-builder:3.1.74-7`. Its immutable OCI index
digest and Linux amd64 manifest/config digests are recorded separately from the
PostgreSQL source. Registry manifests and configuration were inspected and
saved under ignored `ref/`; image layers were not downloaded or executed.
The image has approximately 1.05 GB of compressed layers. Its configuration
exposes `/emsdk`, `/install/libs`, library export files and LLVM tools. The tag
names SDK 3.1.74, while the current source Dockerfile defaults to SDK 6.0.5;
neither binary SDK version nor toolchain equivalence is asserted from the tag.

The separate `electric-sql/pglite-build` repository is pinned for research to
`c195113dbaf09488f8d5eeb2db91dacd123b74d0` (default branch `portable`). Its actual
`.buildconfig` selects PostgreSQL 17.4 and SDK `3.1.74.6bi`, and its tree includes
17.5 scripts. Its `native.sh` uses WASM-to-C machinery. It is **not** selected as
the 18.6 build wrapper. No top-level license was found in this research snapshot;
its contents remain reference material, not a product dependency.

The JavaScript lifecycle reference is separately pinned to
`electric-sql/pglite@967432ba0e8d2c71e0c9899edc1325bd8d68d6e8`. Selected source,
package metadata and Apache-2.0/PostgreSQL notices are saved in its own ignored
reference directory. Its actual driver initializes a data directory, marks
PGlite active, starts single-user mode expecting exit 99, enables PGlite's Port,
calls the unrolled loop per message, handles exit 100 with error cleanup,
and runs shutdown/atexit callbacks. It is documentation input, not a native
binary dependency.

## Effective profiles

[`profile.json`](../config/profile.json) separates the upstream WASM build from
the implemented `embedding-native` variant.

* Upstream WASM uses the Emscripten wasm32/ILP32 ABI, `__EMSCRIPTEN__`,
  `__PGLITE__`, emulated libc/runtime, filesystem bundles and ICU/XML/XSLT/zlib/
  OSSP UUID dependencies. Optional pgcrypto/PostGIS need additional libraries.
  It has not been built here. The upstream `--disable-spinlocks` configure
  argument is unrecognized by this 18.6 configure; its presence is not evidence
  of an effective setting.
* The native control uses Linux x86_64 little-endian LP64, GNU C11, the actual
  fork's `__PGLITE__` code and all of its listed libc interception macros.
  Generated configuration verifies 8-byte pointer, long, size_t and off_t.
  Readline, ICU, TLS, XML/XSLT, compression providers, external authentication
  integrations and extensions are explicitly excluded. The installed native
  build includes ordinary upstream bootstrap assets such as PL/pgSQL and
  encoding modules; those are not evidence of translated extension support.

The native variant is useful embedding evidence, but its platform configuration,
dependencies and ABI do not establish parity with the upstream WASM profile.
A normal native PostgreSQL build is not substituted for this evidence.

## Preparation interface and manifests

`scripts/prepare.py` exports:

* `sources(root)`: framework `Source` objects from metadata.
* `prepare(ctx)`: acquire/reuse `ctx.sources['product']`, fingerprint it, copy
  it to a private source stage, compile the original libc override TU, configure,
  generate headers and parser sources, and return a JSON-compatible info dict.
* `build_native(ctx, info)`: bounded native build/install, actual backend linker
  and per-TU compilation inventory, and generated-header inspection.

The returned dict includes `source`, `build`, `prefix`, `libc_object`, ordered
`include_dirs` (also `include`), `defines`, `flags`, `manifest`, tool identities,
observed source identity, ABI sizes and generated input fingerprints. The stage
uses an in-source native build **in the copy**, under
`build/native-control/<identity>/source`; reference inputs remain preserved.
The libc TU uses standard stdbool/stdlib forced includes because the upstream
file omits those native C11 declarations. No engine source text is patched.

All child commands use framework `ctx.run`; native make is bounded to `-j2`.
Preparation/build uses a private lock. Generated header links are materialized
as ordinary copies; make uses `LN_S=cp -p`. Native compilation requires Linux
build tools; these platform prerequisites do not redefine campaign orchestration
portability. Host compiler, make, Bison, Flex and Perl versions are recorded.
`--hashes off` uses a private per-run stage instead of trusting a cached one.

`prepared.json`, `native-build-manifest.json` and
`native-source-manifest.json` live alongside the stage and/or in the framework
artifact directory. The source manifest follows the native backend linker
objects and common/port archive members, with each actual source, working
directory and compiler argv. It distinguishes this backend link closure from
the larger embedded-init product closure. A serial upstream make plan collects
per-file flags while private object files are temporarily parked and restored;
it performs no native recompilation. It avoids make's forced-remake mode, which
can rerun configure even during a dry run.

The final observed native linker closure contains **863 C translation units**
from backend, timezone, common and port, plus the separately compiled libc
wrapper TU. The completed build records **169 generated/changed C/header/config
inputs**. Effective configuration enables runtime-selected SSE4.2/AVX512 CRC and
AVX512 popcount implementations, SysV shared-memory code (intercepted by
pglitec), and unnamed POSIX semaphores. These native feature macros need an
explicit translation-profile decision; they must not be presented as proof of
compiler intrinsics support in DotCC.

## Native workflow and commands

Once the parent recipe is present, run:

```bash
bash pglite/scripts/fetch.sh --fetch missing
bash pglite/scripts/test.sh --suite native-control --fetch never
```

The specialist runner uses the same framework command logger/cancellation and
works from an unrelated current directory:

```bash
python3 -B /home/marius/p/dotcc/pglite/scripts/native-control.py --fetch never
python3 -B /home/marius/p/dotcc/pglite/scripts/native-control.py --prepare-only --fetch never
python3 -B /home/marius/p/dotcc/pglite/scripts/native-control.py --build-only --fetch never
```

Prerequisites: Linux x86_64, Python 3.11+, GCC-compatible `cc`, GNU make,
Bison, Flex, Perl, GNU binutils `ar`/`objcopy`, standard C headers and libc.
Emscripten, Docker, npm and external database servers are not used by this
native variant. Missing required tools fail explicitly.

The native suite relinks upstream `initdb` with a test-only callback constructor
to permit its normal helper subprocesses. A separate test host links the actual
backend objects, renaming only its `main` symbol, and models Emscripten's host
ExitStatus boundary using linker-wrapped libc exit and a native setjmp target.
Linux polling needs a real socket descriptor at upstream's dummy fd 1 and a
postmaster-liveness pipe; the test host supplies both. PostgreSQL query/result
bytes still travel through upstream PGlite read/write callbacks.

[`tests/native/cases.json`](../tests/native/cases.json) declares 21 assertions:
create/insert/query with UTF-8 and NULL, rollback and commit, transaction states,
duplicate-key and aborted-transaction SQLSTATEs, syntax-error recovery, shutdown,
and persistence over two reopen cycles. The parser checks framed rows, command
completion and ReadyForQuery states; malformed/truncated output is rejected.
Negative controls reject a deliberately wrong row and require harness exit 24
to propagate. Original wire transcripts and per-command logs are retained.

Native SQL runs in-process in the test executable. Native **setup** invokes
initdb helper processes, and each reopened lifetime uses a fresh native test
process. This does not prove same-process reinitialization, managed embedded
initialization, concurrent independent instances, native/managed file interchange,
WASM profile equivalence, or any managed JIT/AOT SQL workflow. No native process
wrapper is delivered as `TranslatedPGlite`.

Current commands, case counts and receipt locations are in
[`source-progress.md`](source-progress.md). The native report is separate from
the parent campaign's translated-product acceptance gates.

## Redistributed notices

Each delivered backend bootstrap prefix includes upstream `COPYRIGHT`, the
DotCC MIT license, and `notices/upstream-source.zip` with the complete observed
upstream source tree, preserving all per-file regex, Snowball and third-party
terms verbatim. `upstream-source-manifest.json` records every path, size and hash
and distinguishes the requested revision from observed local input. The archive
is documentation only; compilation and execution never read it. A second archive,
`generated-inputs.zip`, preserves all 168 prepared C/header inputs differing from
the observed tree, including Bison notices and their special exceptions; the
referenced GPL text is included verbatim. Notice assets
are covered by ordinary delivery ownership and output hashes. The separate
NativeAOT publication also preserves the actual resolved runtime pack's license
and third-party notices beside its binary; final publication evidence records
the selected pack and exact notice hashes.

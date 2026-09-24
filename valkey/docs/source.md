# Valkey source review

Reviewed on 2026-09-24 for the translation plan. This records actual source
inspection and download verification, not a native build or a dotcc execution.

## Proposed implementation pin

| Input | Identity |
| --- | --- |
| Project | [valkey-io/valkey](https://github.com/valkey-io/valkey) |
| Release | [9.1.2](https://github.com/valkey-io/valkey/releases/tag/9.1.2) |
| Commit | `7f1dffedff6de73058b2c2a389422b6ecd56c8fb` |
| Archive | `https://codeload.github.com/valkey-io/valkey/tar.gz/7f1dffedff6de73058b2c2a389422b6ecd56c8fb` |
| Archive SHA-256 | `c34214a05899f52771ec0441ff0557d87e6fa73ae0c16204cbecc59ca30798bb` |
| Local archive | `ref/valkey-7f1dffedff6de73058b2c2a389422b6ecd56c8fb.tar.gz` |
| Extracted root | `ref/valkey-7f1dffedff6de73058b2c2a389422b6ecd56c8fb/` |
| Version check | `src/version.h`: `VALKEY_VERSION "9.1.2"`, release stage `ga` |

The [official download page](https://valkey.io/download/) listed 9.1.2 as the
current release at review time. `git ls-remote` resolved its tag to the commit
above. The commit-addressed archive was downloaded, hashed with SHA-256 and
extracted under `ref/`; its version header was inspected. These inputs are
ignored by Git. Future regeneration must use the immutable URL and recorded
digest, never a moving release/branch URL. P0 must turn this record into the
machine-readable fetch and file-integrity manifests.

## Findings from the downloaded snapshot

All paths below are relative to the extracted root. These are planning facts,
not observed compiler failures.

| Source | Consequence for the plan |
| --- | --- |
| `src/Makefile`, `deps/Makefile` | Derive the server source closure from `ENGINE_SERVER_OBJ`, trace objects and dependency rules. CLI, benchmark, Sentinel and test entry points need separate classification. Linux defaults to jemalloc; select `MALLOC=libc` explicitly for the managed profile. |
| `src/Makefile`, `utils/generate-command-code.py`, `utils/generate-fmtargs.py` | Command definitions and formatting headers have Python generation steps. Use the same resolved Python 3 for campaign drivers and upstream generators; generate in staging, preserving `ref/`. Record release-header generation too. |
| `src/modules/lua/Makefile`, `deps/lua/src/Makefile` | Lua is a built-in static module by default, with separate module and interpreter sources. Include the engine registration and bundled Lua extensions, not just a generic Lua interpreter. |
| `deps/fast_float/ffc.h`, `deps/fpconv/`, `deps/hdr_histogram/` | The bundled fast-float header is a C port. Inventory portable numeric conversion and histogram code; do not assume fast-float requires translating C++. Qualify portable paths instead of selecting unsupported CPU intrinsics. |
| `src/server.c`, `src/entry.c`, `src/ae.c`, `src/networking.c`, `src/socket.c` | Server globals, startup, event callbacks and nonblocking transport need explicit embedding and lifecycle boundaries. Calling the CLI entry point is not an owning library API. |
| `src/rdb.c`, `src/aof.c`, `src/server.c` | Foreground serialization exists, but background save/rewrite and child completion have fork-dependent behavior. AOF startup and runtime enablement follow different paths and need distinct qualification. |
| `src/bio.c`, `src/io_threads.c`, `src/lazyfree.c` | One command execution thread does not prove the absence of background workers. Inventory synchronization, queues and thread shutdown separately from optional I/O parallelism. |
| `deps/lua/src/ldo.c` | Protected Lua calls depend on nonlocal error handling. Existing repository Lua coverage is useful, but must be rerun against this exact bundled interpreter and its server callbacks. |
| `tests/test_helper.tcl`, `tests/unit/`, `tests/integration/` | Much of the server test suite is Tcl driving a server process. Run it against a managed server launcher and preserve assertions; Tcl is a test tool, not input C for dotcc. |
| `src/unit/*.cpp`, `deps/Makefile` | GoogleTest unit drivers are C++, and the dependency recipe can fetch gtest-parallel. Keep these as native references or port assertions to a managed runner; explicitly pin any separately acquired test tools. |

Immutable references: [server build](https://github.com/valkey-io/valkey/blob/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/src/Makefile),
[dependencies](https://github.com/valkey-io/valkey/blob/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/deps/Makefile),
[Lua module](https://github.com/valkey-io/valkey/tree/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/src/modules/lua),
[server](https://github.com/valkey-io/valkey/blob/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/src/server.c),
[RDB](https://github.com/valkey-io/valkey/blob/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/src/rdb.c),
[AOF](https://github.com/valkey-io/valkey/blob/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/src/aof.c),
[tests](https://github.com/valkey-io/valkey/tree/7f1dffedff6de73058b2c2a389422b6ecd56c8fb/tests).

## Provenance work remaining in P0

- Record the exact included C units, headers, generators, defines, dependency
  versions and file hashes. Distinguish built-in Lua registration from optional
  native module loading.
- Inventory upstream `COPYING` (BSD-3-Clause) and bundled dependency notices,
  including Lua, fpconv, fast-float, hdr_histogram and any libvalkey sources in
  the selected closure. Preserve notices in generated distributions.
- Record .NET/native compiler/Python/Tcl versions, host ABI and test-only tools.
  Verify which native build targets need additional inputs before invoking them.
- Build the native reference in `build/`, never inside `ref/`. Record effective
  options, baseline tests and the first actual dotcc translation results.
- Use a verified archive to authenticate a pre-extracted tree, or retain a
  trusted per-file manifest from a verified fetch. A directory name, tag label,
  version string or self-created local receipt alone is not integrity evidence.

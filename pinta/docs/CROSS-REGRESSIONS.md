# Regression recipes after shared compiler changes

Run these serially with the final compiler. This is a command inventory, not a
passing receipt. Executed results belong in [VALIDATION.md](VALIDATION.md) and
`artifacts/regressions/`; preserve pre-existing failures separately.

The Pinta changes affect C token pasting, typedef/member namespaces, identifier
escaping and integer promotion of switch subjects. The shared unit/functional
suites cover those paths and the shared Zig/WAT backends:

```sh
dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release
dotnet test DotCC.FunctionalTests/DotCC.FunctionalTests.csproj -c Release
DOTCC_RUN_WAT=1 dotnet test DotCC.FunctionalTests/DotCC.FunctionalTests.csproj \
  -c Release --no-build --filter FullyQualifiedName~WatOracleTests
```

The WAT execution oracle needs `wat2wasm` and Node. Zig frontend checks run in
the ordinary suites; the optional external Zig differential additionally requires
a `zig` compiler, which is absent on this host. No external Zig oracle result is
claimed.

SQLite's product recipe regenerates the actual engine, invokes semantic
postprocessing, and tests the separate consumer. Its corpus recipes independently
regenerate executables and compare their output with the native expectations:

```sh
SQLITE_AOT=1 sqlite/scripts/test-managed-consumer.sh
sqlite/scripts/test-translated.sh core
SQLITE_AOT=1 sqlite/scripts/test-translated.sh api
sqlite/scripts/test-translated.sh vfs
sqlite/scripts/test-translated.sh vtable
sqlite/scripts/test-translated.sh allocation
sqlite/scripts/test-translated.sh upstream
sqlite/scripts/test-translated.sh fts5
```

For the wider SQLite raw/optimized comparison, first emit with
`sqlite/scripts/emit-engine.sh --no-postprocess`, restore the generated project,
then invoke `dotcc-postprocess.dll` on it with `--output` pointing to a new
snapshot directory. `python3 sqlite/scripts/test-postprocess.py <snapshot>
--aot --corpora` tests both snapshots, host VFS, threading and existing emitted
corpora. Do not reuse a snapshot made with an earlier compiler as new evidence.

Existing TLS/QUIC product recipes regenerate before qualification:

```sh
picotls/scripts/translate.sh --no-build-tools
python3 picotls/scripts/test-campaign.py --all --aot
msquic/scripts/translate.sh --no-build-tools
python3 msquic/scripts/test-public-consumer.py
python3 msquic/scripts/test-abi.py
```

The MsQuic product also depends on the picotls product. Its freeze step reads
assembly informational versions from the hash-verified compiler snapshot, since
repository intermediate build metadata can belong to another revision. The
initial stale-metadata failure and successful freeze-only retry are preserved in
the Pinta regression logs. Follow its current source
closure receipts rather than substituting native implementations for failures.
These scripts preserve their own campaign logs and validation constraints.

The executed Lua regression follows the full interpreter recipe in
[the existing Lua CI workflow](../../.github/workflows/lua.yml), including
`loadlib.c` and `lua.c` (33 translation units), then runs:

```sh
cd examples/lua/lua-src/testes
dotnet /absolute/path/to/Lua.dll -e '_U=true' all.lua
```

Qualification requires exit zero and `final OK !!!` in the output. The smaller
`examples/lua/link.sh all` driver is not equivalent to this conformance suite.

Chibi uses the nine translation units and static-module flags from
`examples/chibi/link.sh`, then runs its full R7RS suite from the source directory:

```sh
cd examples/chibi/chibi-src
CHIBI_IGNORE_SYSTEM_PATH=1 CHIBI_MODULE_PATH=lib \
  dotnet /absolute/path/to/Chibi.dll tests/r7rs-tests.scm
```

Its static module registry requires the relative `lib` path. Require all 1,225
checks and 18 subgroups to pass, and compare against
`examples/chibi/baseline-r7rs.txt` after removing only timing and ANSI escapes.
The Pinta regression run emits these interpreters under
`pinta/build/regressions/Lua` and `Chibi`, preserving existing ignored example
outputs. Exact commands, logs and checks are retained under
`pinta/artifacts/regressions/cross-campaigns.json` and adjacent logs.

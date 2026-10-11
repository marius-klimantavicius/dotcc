# Per-translation-unit project builds

`project.py` compiles each original source directly with EDG's internal
preprocessor, then packages the owned semantic graph in a separate SQLite
`.dotobj`. There is no external preprocessing pass or merged EDG program.
Successful packaging is **not** a C# linking or execution claim. The receipt
keeps `executionVerified` and `nativeAotVerified` false.

Prepare a thread-safe SQLite Unix-profile source and manifest using the same
audited source preparation as the direct backend:

```sh
python3 -B edgcpp/object-linker/project_manifest.py sqlite \
  --output-dir /absolute/new/sqlite-inputs --with-musl
```

For DuckDB, use an existing validated portable source stage and pinned libc++
acquisition. This preserves the per-unit CMake definitions and include order,
selects the established C++17 portable profile, and disables dynamic extension
loading. Library sources include the configured static extensions and bundled
dependencies. `--include-tests` includes the upstream test-runner units.

```sh
python3 -B edgcpp/object-linker/project_manifest.py duckdb \
  --portable-manifest /absolute/portable/portable.json \
  --libcxx-acquisition /absolute/libcxx-acquisition \
  --include-tests --with-musl --with-libcxx --output-dir /absolute/new/duckdb-inputs
```

`--with-libcxx` adds 55 independent C++20 implementation units from the pinned
LLVM 18.1.8 source and existing shared managed ABI adapters. Every `.cpp` remains
its own original translation unit. The shared preinclude configures the library;
it does not include or merge implementation files. DuckDB itself retains its
C++17 configuration and the same public headers and ABI namespace.

The provider selects the portable source branches for threads, localization,
random devices, filesystem and allocation. Its managed exception, atomic wait
and synchronization-destructor adapters replace the corresponding upstream
native ABI definitions. Sources, private includes, public headers and generated
configuration are checked against acquisition hashes; the selected source list
is checked against the pinned upstream CMake inventory. This preserves a single
runtime implementation source and avoids a native libc++ dependency.

The manifest records these substitutions and hashes. Source assembly does not
claim completed C++ translation or runtime binding: those remain explicit
acceptance gates. Without `--with-libcxx`, only libc++ headers are included and
`libcxxProviderIncluded` stays false. `--with-musl` separately adds the C provider.
`tests/libcxx_provider_verify.py --acquisition PATH --output-dir FRESH` checks
the independent source manifest and rejects modified or unrecorded inputs.

Build a project or probe a selected source:

```sh
python3 -B edgcpp/object-linker/project.py build \
  --manifest /absolute/inputs/project.json \
  --output-dir /absolute/new/object-build --jobs 1
```

`--select REGEX` and `--limit COUNT` restrict probes. The result records the
full manifest size, selection size, and whether the entire manifest was selected.
`--jobs` bounds simultaneous worker/compiler processes. Each worker has a
600-second timeout and, on POSIX hosts, an 8-GiB address-space cap by default;
configure these with `--timeout` and `--memory-limit-mib` (zero disables the cap).
The timeout kills the worker's whole process group on POSIX. Every failed
attempt, compiler snapshot, raw wire, diagnostic and partial database remains.
Each worker exits after one object, releasing the in-memory wire before another
unit is assigned. The coordinator retains only small result records.

`--min-free-space-mib N` defers new compiler attempts when the output filesystem
has less than N MiB free. Already running workers finish, so reserve additional
space for those attempts. Deferred units produce a `storage-limited` project
receipt and a nonzero exit; completed objects are preserved. Resume after space
becomes available, or with a different scheduling floor. The floor does not
change semantic inputs and never removes artifacts. Its default is zero.

Repeat the same command with `--resume` to reuse unchanged successful objects
and retry failures in fresh attempt directories. Compiler, manifest, driver,
packager, schema, environment, original sources and include-directory contents
must still match. Changes to searched directories—including adding a header
that can shadow an earlier lookup—require a fresh output directory. Each reused
object and its embedded source hashes are checked. A corrupt object is retained
and rebuilt in a new attempt. The result manifest's object paths, rather than a
directory glob containing old attempts, are the authoritative link inputs.

All units receive the same deterministic `__DATE__`/`__TIME__` values. File
modification times are recorded for `__TIMESTAMP__` provenance. Macros remain
part of the semantic object; no `.i`/`.ii` input is accepted.

The JSON manifest has `schema: 1`, an optional `project` name, and a nonempty
`units` array. Every unit supplies a unique `id`, an absolute original `source`
path, `standard` (`c11`, `c17`, `c++17`, or `c++20`), ordered `arguments`, and an
optional `role`. Supported arguments are `-I`, `-D`, `-U`, `--preinclude`, and
explicit exception/RTTI switches. Code-generation, preprocessing, and extra
input-file options are rejected. Missing include directories are allowed and
their absence is recorded, matching ordinary compiler include-path behavior.

Verify the driver's real compiler packaging/restart behavior:

```sh
python3 -B edgcpp/object-linker/tests/project_verify.py \
  --compiler edgcpp/build/bin/cpfe-dot --output-dir /absolute/new/driver-test
```

The test checks separate original TUs, retained included headers and macros,
successful resume, fresh retry after object corruption, header-search changes,
forbidden frontend switches, storage deferral/resume without starting compilers,
and compiler/child termination on timeout.

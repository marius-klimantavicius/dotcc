# Experimental upstream VFS translation

HostVfs remains the default product implementation in `Managed.Database`.
The experiment builds a separate engine in `Managed.Database.UpstreamUnix`.
Reference SQLite 3.53.4 stays unchanged under `ref/`; the experiment uses the
same checksum-guarded APPDEF mutex adaptation as the product.

The amalgamation contains the upstream `os_unix.c` and `os_win.c` modules. Enable
the corresponding `SQLITE_OS_*` switch to translate these modules with the rest
of SQLite. Building them inside their original engine preserves access to
SQLite internals, allocator/mutex state and the VFS registry. This experiment
does not substitute our HostVfs locking algorithms into the upstream module.

## Unix: working Linux x64 prototype

From `sqlite/`:

```bash
experimental/upstream-vfs/emit-unix.sh
experimental/upstream-vfs/test-unix.sh
SQLITE_AOT=1 experimental/upstream-vfs/test-unix.sh
```

Build dotcc in Release first. Output is `generated/UpstreamUnixSqlite/` with
API class `Managed.Database.UpstreamUnix.Sqlite`. Emission is raw dotcc output,
split by function. The existing standalone postprocessor can be applied to this
project separately. `test-unix.sh` regenerates, builds, and runs the experiment.

Upstream C implements VFS registration, file opening/lifetime, POSIX advisory
lock bookkeeping, rollback journals, WAL shared memory and database mmap.
`managed/UnixNative.cs` forwards OS calls through P/Invoke to
`libdotcc_sqlite_os.so`. The small `native/unix.c` shim calls the OS and converts
`stat`/`flock` structures; it contains no SQLite implementation or locking policy.
Ship this shim alongside the experimental assembly/native executable.
Dotcc's libraries still supply the ordinary C runtime. APPDEF mutexes use the
existing BCL-backed HostMutex, compiled into the experiment's own namespace.

The bindings currently **require Linux x64**. Their flag/errno profile is Linux;
the shim rejects other targets instead of assuming compatible ABIs. macOS, BSD,
and ARM64 need their own reviewed bindings. No native SQLite library is linked.
Core, JSONB, FTS5, math, percentile, metadata, multithreading, WAL and mmap remain
enabled. `mremap` is disabled; upstream's mmap/unmap fallback remains available.

JIT and linux-x64 NativeAOT tests pass. Tests cover real disk CRUD, same-process and child-process writer contention,
WAL snapshots/checkpoints, an actual `xFetch`/`xUnfetch` mapping, JSONB, FTS5,
integrity, persistent reopen and mutex cleanup. This is experimental coverage,
not the full product VFS fault/durability campaign.

The product and experiment have separate types and registries. Choose the API
when opening a connection; their `sqlite3*` handles are not interchangeable.
Upstream VFS names such as `unix` are selectable within the experimental engine.
A common runtime-selection facade has not been added. Avoid opening the same
file through independent providers in one process until their combined lock and
descriptor-lifetime behavior is verified.

## Windows: reproducible translation probe

```bash
experimental/upstream-vfs/probe-windows.sh
# Optional Windows header directory for further compiler investigation:
SQLITE_WINDOWS_HEADERS=/path/to/headers experimental/upstream-vfs/probe-windows.sh
```

The probe enables `os_win.c` with Unicode Windows APIs, APPDEF mutexes, WAL and
mmap, and disables background worker threads. It currently fails because dotcc
has no `windows.h`/Windows SDK declarations. The first downstream parse error is
at the `HANDLE` member of `winFile` in the amalgamation. Supplying an arbitrary
SDK directory is not a working implementation: Windows scalar/structure layouts,
calling conventions, SDK compiler extensions and the P/Invoke bindings still
need review. There is **no usable translated Windows VFS artifact yet**.

Next steps are a checked Windows ABI header surface (notably 32-bit `LONG`/`DWORD`
in dotcc's otherwise LP64 model), real OS bindings, isolated regressions for
parser/emitter failures, then execution of locking/WAL/mmap tests on Windows.
Retain HostVfs throughout this work; provider selection remains the user's choice.

# Persistence and unsupported fork

Reviewed 2026-09-24 against Valkey 9.1.2, commit
`7f1dffedff6de73058b2c2a389422b6ecd56c8fb`. The findings below are source-level;
managed file interoperability and durability remain unqualified.

## Fork contract

The implementation request explicitly permits libc `fork()` to return an error.
`DotCC.Libc/ProcessSignalLib.cs:111` already returns `-1` and sets `errno = EPERM`;
no success stub or CLR process fork is needed. Without LTTng,
`src/server.h:100` defines `valkey_fork()` as `fork()`.

`serverFork` (`src/server.c:7071`) creates the child-info pipe before calling
fork. On `-1` it closes that pipe and restores the original errno, returning
failure before recording a child pid/type or incrementing the fork count.
This is a valid failure path but does not eliminate the independent real-pipe
requirement in module startup.

`rdbSaveBackground` (`src/rdb.c:1668`) records the attempt and sets
`lastbgsave_status = C_ERR` on fork failure. `rewriteAppendOnlyFileBackground`
(`src/aof.c:2595`) sets `aof_lastbgrewrite_status = C_ERR` and returns failure.
Its preparation already flushes and opens a new incremental AOF before fork;
therefore failing fork is not a side-effect-free feature guard.

Reject daemonization before calling `daemonize` (`server.c:6885`): its test
`valkey_fork() != 0` treats a negative result like the parent branch and exits
successfully. Replication, Sentinel, module fork, forked script debugger and
background persistence remain unavailable in the initial managed profile.

## Foreground paths which do not require fork

`SAVE` (`rdb.c:3945`) calls translated `rdbSave` synchronously. Startup RDB loading
is reached through `loadDataFromDisk` (`server.c:7231`). Preserve upstream rio,
RDB encoding, compression and checksums; only file/clock/error boundaries belong
in the host.

At startup, `initServer` sets AOF state from the configured `appendonly` value.
After the static Lua engine and workers initialize, `main` calls:

1. `aofLoadManifestFromDisk`.
2. `loadDataFromDisk`, which chooses AOF replay when AOF is on.
3. `aofOpenIfNeededOnServerStart` (`aof.c:700`).
4. `aofDelHistoryFiles`.

When the manifest has neither a base nor incremental files,
`aofOpenIfNeededOnServerStart` calls `rewriteAppendOnlyFile` directly to create
the base, then opens the incremental file with `O_APPEND` and persists the
manifest. This is a real foreground initial-AOF path. It must be exercised with
fresh storage and both RDB and command-format bases, then with existing multipart
manifests. Several failures call `exit(1)` and therefore require the owning error
boundary described in [host-contract.md](host-contract.md).

Runtime `startAppendOnly` (`aof.c:963`) instead sets `AOF_WAIT_REWRITE` and invokes
or schedules a background rewrite. It restores AOF_OFF when an immediate rewrite
fails, but when invoked inside EXEC it schedules work and initially returns
success. Startup AOF support does not imply runtime enabling is supported.

## Required configuration and command guards

Use `save ""`, `auto-aof-rewrite-percentage 0`, one I/O execution thread,
`daemonize no`, no supervision/cluster/replication and no arbitrary native module
loading in the first profile. Preserve configured startup AOF and explicit SAVE.

Guard `BGSAVE` and `BGREWRITEAOF` before scheduling or changing persistence state;
guard runtime enabling of `appendonly` before `startAppendOnly`, including inside
MULTI/EXEC. `BGREWRITEAOF` can otherwise reply that it was scheduled inside EXEC
without attempting fork. Guard runtime changes that reinstate automatic saves
or automatic AOF rewrites. `serverCron` (`server.c:1592,1603,1622`) has separate
scheduled-rewrite, save-policy and size-growth triggers, so guarding only command
handlers is insufficient. Test both commands and configuration mutation paths,
and prove child state remains empty. Do not silently disable all persistence.

## File ownership and durability

Each owner needs a file root that resolves relative RDB/AOF/configuration paths,
including upstream temporary filenames. Rewriting only `dbfilename` or
`appenddirname` is insufficient because temporary files and other operations use
relative paths. Neither `CONFIG SET dir` nor startup `dir` may mutate process cwd.

Real host operations must cover short reads/writes, append, flush/fsync, seek,
truncate, rename, unlink, directory creation and manifest replacement, with
translated errno handling. Qualify `appendfsync always`, `everysec` and `no`
according to upstream semantics. `everysec` requires actual background fsync
workers and their atomic status/offset publication; a successful no-op job is
not persistence. File flush and directory durability are distinct capabilities;
no power-loss or cross-filesystem rename guarantee is established by this audit.

The current BCL runtime flushes file contents with `FileStream.Flush(true)` but
cannot open directory handles. `open` reports `-1/EISDIR` for a directory; the
unmodified upstream `fsyncFileDir` explicitly accepts this platform limitation.
SAVE and AOF operations therefore cannot promise durable directory entries after
power loss, even when file flushing succeeds. No directory flush is simulated.

`finishShutdown` (`server.c:4806`) flushes AOF, calls `valkey_fsync`, optionally
performs a synchronous final RDB save and closes listeners/unloads modules. Its
AOF fsync failure is logged rather than automatically changing every shutdown
result; the managed API must expose diagnostics faithfully. The host additionally
must close descriptors, drain jobs and finish owner cleanup that native process
exit normally supplies.

## Required executable evidence

Pending P6 evidence: foreground SAVE/load, fresh and existing AOF startup,
append/replay, fsync modes, transactions, scripts/functions, graceful restart,
ordinary file failures, and native-to-managed plus managed-to-native loading.
Compare logical keys/types/TTLs, allowing valid ordering and metadata differences.
A native control with fork forced to `-1`/EPERM can verify foreground feasibility
and negative background replies, but cannot qualify managed host I/O or ownership.
Keep native-only and managed results distinct in validation receipts.

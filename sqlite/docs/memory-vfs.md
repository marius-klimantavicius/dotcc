# Test-only memory VFS

`tests/memory_vfs.c` implements the deterministic `dotcc-memory` VFS used by
native and translated C test corpora. GCC compiles it for reference runs; dotcc
translates it as a separate input alongside SQLite and each harness. There is
no authored C# memory VFS or corpus-specific managed override profile.

The production library registers only HostVfs through the OS-init/end function
overrides. Applications use SQLite's built-in `:memory:` mode for in-memory
SQL databases; no custom memory VFS is required. ManagedConsumer verifies SQL,
compacting GC, close/reopen isolation, absence of host file handles, and absence
of the test VFS from the product's registry.

The test VFS models named files stored in RAM rather than SQLite's `:memory:`
database path. It exercises file reads/writes, rollback journals, zero-filled
growth and short reads, lock transitions, delete-on-close, import/export,
deterministic time/randomness, and one-shot I/O failure injection. Named files
survive closing their connections until deletion or reset. It has no disk
persistence, cross-process locking, WAL, mmap, or dynamic loading. File methods
advertise version 1. The deterministic corpora use `SQLITE_THREADSAFE=0`;
production concurrency, WAL and mmap are tested independently against HostVfs.

Useful checks:

```sh
scripts/test-vfs-native.sh
SQLITE_AOT=1 scripts/test-translated.sh vfs
SQLITE_AOT=1 scripts/test-managed-consumer.sh
SQLITE_AOT=1 scripts/test-threading.sh
scripts/test-image-exchange.sh
```

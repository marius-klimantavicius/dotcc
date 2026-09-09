# Campaign configuration

`config/defines.txt` is the shared native/translated profile: C17,
`SQLITE_OS_OTHER=1`, `SQLITE_THREADSAFE=0`, `SQLITE_TEMP_STORE=3`, and
`SQLITE_OMIT_LOAD_EXTENSION`. Default SQLite core and JSON/JSONB remain enabled;
all FTS opt-in defines remain absent. There is no native interop or dynamic
loading. A portable memory VFS supplies the OS interface.

Initial host: Linux x64, little-endian LP64; .NET SDK 10.0.111, runtime 10.0.11,
GCC on Ubuntu 24.04/Zorin 18. dotcc targets 64-bit pointers/long/size_t.
Initial solution build resolves LALR.CC locally; the forced NuGet path remains
a required validation item. Calls are serialized; WAL/shared memory, mmap,
process durability and concurrent hosting are unsupported platform capabilities.

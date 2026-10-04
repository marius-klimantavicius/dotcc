# POSIX compiler host services

`--posix` configures `LibcFiles` with a shared `ILibcFileDescriptors` provider
implemented by `PosixContext`. The provider is optional: ordinary ISO C builds
retain their existing BCL stream implementation without a POSIX dependency.

`fopen`, `fdopen`, `fileno`, and `tmpfile` share one owner descriptor namespace.
`fdopen` does not truncate the descriptor, rejects incompatible access modes,
and enables atomic append for `a`. `fclose` flushes C buffering and closes the
same descriptor. Descriptor I/O and stream I/O share the unbuffered offset;
callers must obey the normal C/POSIX buffering and active-handle rules when
switching between them. `fstat` and `mmap` use the same underlying native file
handle. Neither `FILE` nor a native libc stream crosses the managed boundary.

The compiler's standard streams occupy descriptors 0, 1, and 2. Their wrappers
close the translated association without closing the host console or injected
BCL streams. Real standard descriptors retain native metadata; custom streams
have a synthetic character-device record. The diagnostic `OpenDescriptorCount`
counts nonstandard descriptors. Dispose the FILE provider before the POSIX
provider so buffered output is flushed while descriptors still exist.

Additional services:

- `mkstemp`: random six-character suffix, exclusive atomic creation, mode 0600.
- `chdir`: owner-specific working directory, shared with stdio path operations;
  it does not change the .NET process working directory. Unix canonicalization
  follows actual symlinks before retaining the new directory.
- `opendir`/`readdir`/`closedir`: BCL directory enumeration, including `.` and
  `..`; opaque owner tokens and a reusable `char d_name[4096]` ABI. Clean EOF
  leaves errno unchanged. Closed tokens are tombstoned until owner disposal.
- `isatty`: standard console/injected-stream classification and native Unix
  terminal query for regular descriptor entries.
- `getrlimit`/`setrlimit`: `RLIMIT_CPU` only, using two unsigned 64-bit values.
  Unix maps to the real process-wide limits; Darwin's infinity spelling is
  converted at the boundary. On Windows the unlimited state is supported;
  finite CPU limits fail with `ENOTSUP`, rather than claiming unsupported
  POSIX soft-limit signal delivery.

This covers the compiler host-service imports; it is not complete POSIX stream
compatibility. Descriptor duplication, process spawning/fd inheritance,
seekable redirected standard streams, and preservation of the descriptor number
across `freopen` are not supplied by this bridge. Ordinary `freopen` retains the
FILE token while reassociating its backing stream. Filesystem names use the
runtime's existing UTF-8 path profile.

Verification lives in `backend/tests/posix_stdio_runtime_verify.py` and
`backend/tests/posix_stdio_translated_verify.py`. The former exercises actual
shared descriptor/FILE offsets, closing and error ownership, mmap, append,
virtual cwd, temporary files, directory lifetime, and CPU-limit ABI. The latter
compares a native POSIX program with translated C11/C++14 static and instance
programs under JIT and NativeAOT, with unresolved externs rejected.

The self-translation campaign verified 47 shared-runtime checks under Linux x64
JIT and NativeAOT, and 44 under Windows .NET/Wine (platform-specific CPU-limit
and permission checks differ). All four translated language/owner combinations
passed Linux JIT, Linux NativeAOT, and Windows .NET/Wine with the same native
oracle output. These runs are retained in `self-posix-runtime-003` and
`self-posix-translated-002` under the persistent campaign directory. Wine does
not establish native Windows or Windows NativeAOT support; macOS was not run.

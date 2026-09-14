# Private access checks

`InstanceIo.Access.cs` supplies access checks over the existing shared
descriptor table and `StatAt` path resolution. It adds no path resolver,
host filesystem lookup, or operating-system credential query. Its policy
matches the current virtual identity, whose real/effective UID and GID are
all zero; these virtual credentials grant no host capabilities.

`F_OK` verifies existence. Existing private files allow `R_OK` under the
virtual-root policy. `X_OK` on a regular file requires at least one execute
bit in its actual metadata; an image is executable only when explicitly
selected through `executablePaths`. Newly created private files retain
mode 0600 and fail `X_OK`. Existing directories permit read, write, and
search access. Their immutable directory metadata describes stable image
structure, not a prohibition on creating private writable children.

`W_OK` on immutable image files fails with `EROFS`, including images marked
executable. This is an explicit private read-only policy. It differs from
native UID-zero access to a mode-0444 file on a writable filesystem, where
DAC override permits writing. Mutable private files allow writing.
Permission checks do not reserve capacity: a later create/write can still
fail descriptor, node, pathname, or byte quotas, and successful access is
not a promise that a later operation cannot race with owner teardown.

`AT_FDCWD` uses the owned current directory. Real directory descriptors
supply their own base, including duplicates surviving the original close.
Absolute paths ignore the directory descriptor. Missing/closed descriptors
and nondirectory bases preserve the errors from the existing resolver;
attempts to walk above the private root remain `EACCES`.

Only `F_OK`, `R_OK`, `W_OK`, and `X_OK` bits are valid; other mode bits yield
`EINVAL`. `AT_EACCESS` is supported because real and effective private
identities coincide. `AT_SYMLINK_NOFOLLOW` is supported because the model
contains no symlinks. Their combination is accepted; other flags, including
`AT_EMPTY_PATH`, return `ENOTSUP`. Empty paths remain `ENOENT` rather than
silently becoming descriptor queries. This does not qualify arbitrary
Linux faccessat extensions or variable virtual user identities.

## C integration

`src/HostAccess/host-access.h` explicitly redirects `access` and `faccessat`
to `HostAccessBridge.cs`. The bridge shares the existing worker-local
`BindHostIo` binding, scans inputs within the established 4096-byte bound,
and decodes strict UTF-8. Null input is `EFAULT`, invalid UTF-8 is `EINVAL`,
and overlong/unterminated input is `ENAMETOOLONG`. Unbound calls report
`ENODEV`; disposed owners report `EBADF`. Successful checks preserve
`errno` and do not change the working directory or descriptor state.

Actual `commandv.c` reaches executable checks through `VfsAccess`, and
`overlays.c:337` forwards its resolved arguments to `faccessat`. These
callbacks implement that host boundary while retaining the upstream
command/path-search algorithms. They do not execute a selected file.

## Qualification

Run `python3 blink/tests/HostAccess/run.py`. The fixture compares native
common access invariants against raw and optimized JIT/AOT consumers of
translated C, with frozen Host sources, bridges, headers, and compiler
identities. It retains raw generated code before postprocessing a copy
and treats `CS8500` as a build error.
Receipt `artifacts/host-access/attempt-d0up77d8/receipt.json` records the
native common oracle, isolated UID-zero oracle, and all four managed modes
passing. Both JIT builds reported zero warnings and zero errors.

A separate real native oracle runs under `unshare -Ur`, with UID zero
inside an isolated user namespace and no host-root credentials. It checks
that a regular file with no execute bits fails `X_OK`, each individual
owner/group/other execute bit permits it, UID zero can read/write a
mode-0000 file, and writing a mode-0444 file succeeds on the native writable
filesystem. That last check records the deliberate immutable-image policy
difference rather than hiding it behind a passing equality assertion.

The common C fixture checks existence, readable/executable image files,
directories, created writable files, invalid modes, missing paths,
nondirectory path components, current-directory and descriptor-relative
lookups, accepted flags, absolute-path descriptor bypass, duplicate lifetime,
and success preserving `errno`. Private tests add immutable-image `EROFS`,
unsupported flags, input validation, root escape denial, disposed/unbound
owners, and two simultaneous workers whose same pathname has different
executable metadata across compacting GC. This qualifies the authored
boundary, not full interpreter or guest-service execution.

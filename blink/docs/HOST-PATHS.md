# Private current directory and canonical paths

Each `InstanceIo` owns a current directory, initially `/`.
`InstanceIo.Paths.cs` changes it only after the VFS validates an existing
directory. `fchdir` uses a real open directory description in the shared
descriptor table, including duplicated descriptors; files and streams
produce `ENOTDIR`, and unknown/closed descriptors produce `EBADF`.
Failed changes leave the current directory unchanged.

Default `OpenFile` and `Stat` calls now use the owned current directory.
An explicit `cwd` argument retains its override semantics. `AT_FDCWD`
uses the same owned directory for `openat` and `fstatat`; actual directory
descriptors still supply their own base, and absolute paths still ignore
the supplied directory descriptor. All these operations hold the existing
instance lock while selecting their base directory.

`VirtualFileSystem.CanonicalPath` calls the existing locked `Resolve`
component walk and then verifies final existence, optionally requiring a
directory. There is no second normalization algorithm in the bridge.
Thus `file/../directory` fails at the nondirectory component, missing
components remain errors, and attempting to walk above the private root
returns `EACCES`. This root-escape policy intentionally differs from a
native filesystem's root-clamping behavior. The model has no symlinks;
`realpath` verifies and canonicalizes existing private files/directories
without claiming symlink traversal support.

## C boundary and string ownership

`src/HostPaths/host-paths.h` explicitly redirects `getcwd`, `chdir`,
`fchdir`, and `realpath` to `HostPathsBridge.cs`. The bridge requires the
existing worker-local `BindHostIo` binding. Unbound callbacks report
`ENODEV`; a disposed owner reports `EBADF`. No operation consults or
changes the process working directory, filesystem, or environment.

C input paths are borrowed for the call, scanned for a terminating zero
within 4096 bytes, and decoded using strict UTF-8. Null input is `EFAULT`,
invalid UTF-8 is `EINVAL`, and an unterminated/overlong input is
`ENAMETOOLONG`. Empty paths retain the VFS's `ENOENT` behavior. Encoding,
validation, and size checks complete before an output buffer is written.
Unlike native implementations that expose a partially resolved prefix,
this boundary leaves caller output unchanged on `realpath` failure.

`getcwd(buffer, size)` returns the supplied buffer on success. A nonnull
buffer with zero size is `EINVAL`; insufficient space including the final
zero is `ERANGE`. `getcwd(NULL, 0)` allocates the exact required byte count.
For `getcwd(NULL, nonzero)`, the requested allocation must fit the path and
may be at most 4097 bytes; an undersized request is `ERANGE`, and a larger
allocation request exceeds this boundary's budget and returns `ENOMEM`.
This bound is an explicit private allocation policy, not a native maximum.

`realpath(path, buffer)` requires the ordinary caller contract of a
`PATH_MAX`-sized buffer (4096 bytes). A private canonical result that cannot
fit is `ENAMETOOLONG`. `realpath(path, NULL)` allocates the exact result
size subject to that same path bound. Both allocating APIs use the existing
C runtime `malloc`, so the caller releases the result with ordinary `free`.
Returned allocations contain bytes only and survive compacting GC, later
directory changes, and owner disposal. They do not borrow movable CLR
storage. These individual allocation bounds do not establish a total
generic-malloc quota; the separate memory budget question remains open.

## Evidence

Run `python3 blink/tests/HostPaths/run.py`. The runner snapshots the Host
project, bridges, headers, and compiler, checks native common invariants,
then compiles and executes raw/optimized JIT/AOT consumers of translated C.
It also reruns the existing HostFiles and InstanceIo model regressions
against the same copied Host project. Raw generated code is retained
unchanged, and `CS8500` is treated as a build error.
Receipt `artifacts/host-paths/attempt-iqcf_fef/receipt.json` records native
and all four managed executions passing, plus both model regressions.
Both JIT consumer builds reported zero warnings and zero errors.

The common C oracle uses a native temporary tree or the corresponding
private image. It verifies `getcwd` sizes/allocation forms, directory
changes and failure preservation, relative open/stat, `AT_FDCWD`, duplicated
directory descriptors, canonical `.`/`..` traversal with component checks,
missing paths, UTF-8 output, and caller-owned allocations released by C
`free`. Native fixture filesystem/cwd operations are confined to that
test subprocess.

Private tests add strict UTF-8 and input bounds, root escape denial,
oversized allocation refusal, null inputs, unchanged failed output,
explicit public-API cwd overrides, disposed/unbound owners, and independent
working directories on two simultaneous bound workers. C-owned snapshots
of cwd and realpath survive forced compacting GC and owner disposal and
are then checked and freed through translated C. These are boundary tests,
not evidence of a successful full-interpreter or service run.

# Private namespace changes

`HostNamespaceBridge.cs` implements mkdir/mkdirat, unlink/unlinkat/rmdir and
rename/renameat against the bound `InstanceIo`. `host-namespace.h` selects only
these authored callbacks. No host filesystem operation is used, and there is no
new owner lifecycle.

Relative paths use the current private directory or an open private directory
fd. Absolute paths ignore dirfd. Directory creation accepts ordinary permission
bits 0777 and applies the instance's private umask; unsupported special bits
return ENOTSUP. Creating mutable entries under existing image directories is
allowed, as with private open(O_CREAT), but image nodes cannot be renamed,
removed or replaced. No links, symlinks, mount points or cross-device behavior
are added.

Rename validates both paths, source/target types, replacement emptiness,
ancestry and the complete renamed subtree's path-byte budget before publishing
replacement dictionaries under the namespace lock. All directory descendants
move together. Open directory descriptions and the private cwd follow a moved
subtree; open descriptions of replaced targets continue to refer to the old
node. Renaming a directory into its own descendant fails with EINVAL. Immutable
image protection returns EROFS. Failed validation preserves names, inode
identities, timestamps and accounting.

Unlinked files and removed directories remain available through existing file
descriptions with nlink=0. File bytes and node quota remain charged until the
last underlying description closes, including internal directory-stream leases.
The removed name releases namespace path-byte quota immediately. A removed
directory fd can still be stat'ed; new relative lookups or snapshots through it
return ENOENT. Existing captured directory streams keep their original snapshot.
Duplicated descriptors share their ordinary description and lifetime.

Deliberate bounded policies: removing or replacing the current private working
directory returns EBUSY (renaming its subtree is supported); root cannot be
removed/replaced; explicit final `.`/`..` removal or rename is rejected. Unlinkat
accepts only 0/AT_REMOVEDIR. The existing private path length and traversal rules
apply, including rejecting attempts to walk above the private root. Path and
inode budgets are separate from descriptor limits. No asynchronous file watcher,
persistent storage or host permission enforcement is implied.

`python3 blink/tests/HostNamespace/run.py` runs a native oracle and the same
translated C operations in raw/optimized JIT and NativeAOT consumers. It also
checks exact private errors, cwd rebasing, detached inode/byte quota, replacement
fd lifetime, directory leases, forced GC, path-budget rollback and independent
instances, plus copied HostFiles/InstanceIo regressions. Generated C# is never
rewritten. Initial complete qualification is retained in
`artifacts/host-namespace/attempt-q1qneafn/receipt.json`. Final current-source
qualification, including immediate advisory-lock cleanup and replacement-target
quota tests, passed in `artifacts/host-namespace/attempt-qu5h_znj/receipt.json`.

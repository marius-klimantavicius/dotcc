# Private permission, ownership and creation-mask policy

The typed C boundary implements `fchmod`, `fchmodat`, `chmod`, `fchown`,
`fchownat` and `umask` over actual private VFS nodes. It never calls host
filesystem metadata APIs or the process-wide OS umask. Retained Blink syscall
paths call fchmod/fchmodat, fchown/fchownat and umask; path resolution and guest
instruction algorithms are unchanged.

Modes are the ordinary low 0777 permission bits. Chmod preserves the node's
file/directory type and inode, changes its real metadata, and updates ctime.
Set-ID, sticky and other high mode bits are explicitly unsupported (EOPNOTSUPP)
rather than stored without their semantics. The existing private UID-zero
access rule means regular-file X_OK requires at least one execute bit; chmod
therefore changes real execute access. Root-style R/W and directory traversal
bypass remain the documented policy. Chmod does not revoke rights already held
by an open descriptor. Immutable image files/directories reject metadata
changes with EROFS, even if the requested mode or owner is identical.

Every node is owned by fixed private UID/GID zero. The C metadata setters require
bound I/O and identity owners, and validate that identity before operating.
Chown resolves a live existing node/descriptor, accepts zero and the POSIX
UINT_MAX unchanged-ID sentinel, and updates ctime. Other IDs fail with EPERM.
No user/group transition, set-ID credential behavior or arbitrary ownership is
claimed. Missing/closed nodes fail normally even for unchanged-ID requests.

Fchmodat accepts zero or AT_SYMLINK_NOFOLLOW; fchownat additionally accepts
AT_EMPTY_PATH on an actual descriptor (or current directory for AT_FDCWD).
There are no private symlinks. Unsupported flags fail explicitly. Absolute
paths ignore dirfd; relative paths use the existing shared directory resolver,
including current directory, renamed directory descriptions and root-escape
policy. The bridge uses bounded strict UTF-8. Errors preserve metadata, file
contents and timestamps; successful mutations affect every duplicate/open
reference to the same node. Live non-file descriptors are unsupported.

Each InstanceIo starts with private umask 0022. Umask returns the old mask and
stores only low0777, matching the native masking operation. File and directory
creation applies requested mode AND NOT mask under the instance lock. Existing
nodes' modes are unchanged by later opens or mask changes. Read-only image
parent directories can still contain mutable-layer children under the existing
namespace policy; their own metadata remains immutable.

Both the inline open wrapper and actual open/openat C functions evaluate all
arguments normally and consume a mode only when O_CREAT is present. The mode
reaches VFS creation through a typed four-argument callback. Legacy managed
OpenFile/OpenFileAt and old three-argument callback callers retain requested
mode0600 defaults; current umask still applies to any newly created node.
Unsupported creation mode bits fail before node creation or truncation. Missing
a required C O_CREAT mode is the ordinary invalid variadic C call, not a
supported defaulting convention. O_TMPFILE and special creation flags remain
unsupported through the existing open policy.

The module's umask return type is unsigned mode_t. Unbound/disposed owners
return UINT_MAX with ENODEV/EBADF; this private-lifecycle error path is explicit
because native POSIX umask normally has no failure case. Unbind existing
identity/I/O bindings and dispose the owner as before; no new TLS owner or
native pointer registry is introduced.

Integration uses `HostPermissionsBridge.cs` with the existing HostIo and
HostIdentity bridges, plus `host-permissions.h` for typed chown/umask aliases.
Existing sys/stat.h supplies chmod aliases. The changed HostFileControl.c,
host-io.h and HostIoBridge.cs must be snapshotted together; old emitted C
objects lack the new mode callback and cannot qualify this creation contract.
New InstanceIo/VFS permission partials accompany the guest worker's mutable
Node.Mode, VFS creationMode parameter and namespace mkdir mask hook.

`blink/tests/HostPermissions/run.py` compares native common invariants with
raw/optimized JIT/NativeAOT C callbacks on copied sources, rejects CS8500 and
retains raw generated hashes. It checks requested creation modes, argument
side effects, private umask changes, duplicate inode/mode visibility, X_OK,
existing descriptor writes after chmod, ID sentinels, absolute/relative/error
resolution, immutable images and failure atomicity. Two owners use different
masks on the same private paths across compacting GC. The native oracle mutates
only files in a disposable test directory. The private fixed-identity and image
policies are additional managed assertions, not claimed equal to host process
credentials or a mutable native mount. HostFiles and InstanceIo regressions run
against the same copied Host snapshot. Final qualified receipt is
`blink/artifacts/host-permissions/attempt-pskdga_g/receipt.json`. It uses shared
VFS SHA-256 `feee5791dbd9d8c0a7ff8c321cfdb11041494482d35a571b53071f30bab80946`,
the same VFS snapshot as the final namespace matrix. All native/four-mode checks
and both model regression consumers passed. This qualifies the boundary and
its dependencies, not execution of the complete Blink core.

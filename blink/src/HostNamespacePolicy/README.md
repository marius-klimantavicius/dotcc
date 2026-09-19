# Selected namespace and protocol boundaries

The initial filesystem profile contains regular files and directories. It does
not provide hard links, symbolic links, or named FIFOs. Those creation requests
validate the private owner, paths/dirfds and existing entries, then fail with
EOPNOTSUPP without publishing a node. Existing destinations return EEXIST; absent
parents, invalid dirfds and root escapes keep the namespace's ordinary errors.
Hard-link sources must exist, directories are forbidden, and immutable image
nodes cannot gain writable namespace aliases. No host path operation is called.

`readlinkat` performs a real lookup: absent paths return ENOENT, invalid path
components keep their normal error, and every existing node is a non-link and
returns EINVAL. The output buffer remains unchanged. These semantics follow the
profile's actual representable node types; returning a fabricated target would
be incorrect. This module does not qualify general symlink traversal or hard-link
sharing. Anonymous pipes are a separate descriptor capability.

The network profile is IPv4 TCP, which has no socketpair operation. IPv4 pair
requests fail EOPNOTSUPP, matching the native common probe; other families fail
EAFNOSUPPORT because they are outside the profile. No descriptors are allocated,
and the caller's pair array is untouched. Rejected requests need not dereference
the output pointer; type/protocol/output-pointer precedence on rejected families
is this profile's policy, not a promise of native interchangeability. Guest
UNIX-domain IPC is not advertised. The observed Linux negative socketpair calls
changed the output array despite failure; output preservation is the stronger
private contract and is tested separately from the native common error codes.

Native checks cover actual non-link lookup errors and unsupported IPv4 pairs.
Private tests cover the deliberately narrower creation/family policy, owner
lifetime, output preservation, no namespace mutations and two owners under GC.
These are explicit unsupported-feature contracts, not successful stand-ins for
link, FIFO or UNIX-domain implementations.

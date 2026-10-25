`python3 tests/GuestRegularShared/run.py` captures authored sources into a private
attempt, builds with at most two jobs, and checks the real InstanceIo and bridge,
the C memory owner in both configurations, and a normal static Linux guest.
Every child process is bounded and its process group is cleaned up. It does not
translate or build the shared delivery.

The authored checks cover independent opens and dup/replacement during a pinned
mapping transaction, descriptor writes and positioned writes, Linux append
semantics, pathname/fd/open truncation, EOF availability and zeroed regrowth,
resident-page eviction and reload, rename/replacement/unlink lifetime, descriptor
reuse, cancellation, memory and node caps, combined owner accounting, rollback,
and disposal with retained leases. A 14 MiB mutable file shares each resident page
between two aliases. A 2 TiB sparse file retains only requested pages. Mounted
stores with overlapping backend inode numbers stay distinct. HostDirectoryFileSystem
is checked using a temporary private directory and guest operations only; this
does not claim coherence with concurrent external host mutation.

`python3 tests/GuestRegularShared/check-staging.py` checks successful pathname
truncate and O_TRUNC cache invalidation against the pinned upstream archive and
verifies that changed anchors fail. It writes only private derived files.

`python3 tests/GuestRegularShared/run-managed.py --prepare-only --runs 3` builds,
inspects, and runs the valid ordinary Linux guest without any product build.
The fixture maps two read-only aliases of the same mutable regular inode and
checks file writes, append, truncate/regrow/refetch, an initially unmapped EOF
tail, private COW before truncation, permission denial, rename replacement,
unlink, descriptor close and reuse. Expected stdout is exactly:

```
guest-regular-shared: coherence/identity/private/EOF/permissions: PASS
```

The parent coordinator runs the translated guest with a new strict producer:
`python3 tests/GuestRegularShared/run-managed.py --clean-delivery POINTER --aot`.
This captures both raw and processed source closures before compiling either,
then checks JIT and NativeAOT host/worker pairs in both public execution modes.
It reports freshness separately from captured-source execution. Native Linux
and authored-layer results alone do not qualify translated execution.

The mutable regular mapping cache uses mounted `stat.Inode` identity and an
internal pinned backend description, with no extra guest descriptor slots.
Read-only mappings share resident backing; MAP_SHARED write access is rejected.
Immutable image mappings keep their prior immutable snapshot path. Memfd writable
sharing keeps its existing path. All cache payload, nodes, and leases use the
existing 64 MiB memory-file sublimit and combined public memory accounting.
Cache node count is bounded by the instance descriptor limit; cached pages and
the pinned description are released when their last leases disappear. The
public import API continues to copy into its mutable private filesystem.

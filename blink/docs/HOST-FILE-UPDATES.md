# Private file updates

Positioned writes and resizing use the existing private file nodes, descriptions
and aggregate writable-byte quota. Ordinary write and pwrite share one write
implementation; pwrite keeps the shared descriptor position unchanged. The
explicit Linux profile retains Linux's O_APPEND behavior for pwrite: append to
EOF regardless of its supplied offset. Native comparison checks that behavior.

ftruncate and truncate resize mutable files, zero-fill growth, release quota on
shrink and preserve every open description's position. Growth beyond the quota
or the managed array bound returns ENOSPC before mutation; allocation failure
returns ENOMEM. Image paths remain immutable (EROFS). Read-only ftruncate fails
EINVAL, unknown descriptors EBADF, directories fail and missing paths return
ENOENT. Paths use the same private resolver/cwd and strict UTF-8 boundary.

fsync/fdatasync validate a private file or directory descriptor and return success
because every successful operation is already committed to the in-memory node;
there is no delayed write queue. This filesystem is ephemeral and offers no
host-disk durability. Standard streams and sockets return EINVAL, unknown or
closed descriptors EBADF. No native filesystem or flush operation is called.

New C callbacks bind to the existing InstanceIo owner and use the existing64KiB
transfer bound. No C pointer is retained. These changes do not add namespace
mutation, mapping coherence after file truncation, or persistent storage.

Native comparison and raw/optimized JIT/AOT pass at
artifacts/host-file-updates/attempt-z_73a10u/receipt.json. Tests check data bytes,
zero-filled holes/growth, duplicate cursor preservation, append, failed output
state, aggregate quota reuse, actual C function pointers, immutable images,
independent workers and compacting GC. Existing HostFiles and InstanceIo model
regressions also pass against the captured final host sources. Reproduce with
python3 blink/tests/HostFileUpdates/run.py.

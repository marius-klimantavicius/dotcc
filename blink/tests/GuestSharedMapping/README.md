`fixture.c` is a normal freestanding Linux x86-64 guest workload covering the
CoreCLR memfd double-mapping sequence: 2 TiB sparse logical length, PROT_NONE
shared fixed reservation, RX promotion, RW aliases, executing/updating code
through those aliases, private snapshots, positional fd I/O, partial unmap,
descriptor reuse and mapping lifetime after fd close. The RX function is guest
code: Blink interprets every instruction. No malformed inputs or fault injection.

Run `python3 -B blink/tests/GuestSharedMapping/run.py` for isolated authored
host/bridge and C memory-owner checks plus the native Linux fixture oracle.
It copies the host library to an attempt directory and does **not** translate
or build the shared Blink delivery. Receipts retain input hashes, exact commands,
failures and logs under `blink/artifacts/coreclr-coordination/memory-focused-*`.

The coordinator must run the built `guest-memfd` ELF through a fresh full Blink
delivery. That full translated execution is deliberately outside this runner.

The private memfd store has a default 64 MiB sublimit including conservative
node/page/lease metadata charges; `SetMemoryFileLimit` configures it (up to
256 MiB) before creating files. Once file mappings are bound, this storage and
all C memory-owner allocations share the execution's `MemoryLimit`. Alias
charging is conservative: both C aliases and the retained sparse payload are
charged. fd writes and backing retained after unmap continue to count. Sparse
ftruncate does not allocate its logical length. Live anonymous file objects,
including those retained only by mappings, are additionally capped at descriptor
capacity. `FileNodeLimit` bounds pathname namespace nodes; memfds are auxiliary
descriptor objects, like sockets, and use RAM rather than `WritableStorageLimit`.

Immutable ordinary guest files support shared read/execute mappings because
their content cannot change through the guest VFS. Later write permission
upgrades reject before guest PTE mutation. Mutable regular files, seals, hugetlb
and anonymous MAP_SHARED remain explicitly unsupported. Private-file snapshot
and EOF handling remain unchanged.

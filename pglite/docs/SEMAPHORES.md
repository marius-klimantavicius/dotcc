# POSIX semaphore and duplicated descriptor host services

The Linux x64 profile uses actual libc unnamed semaphore services, with the
32-byte/alignment-8 opaque guest sem_t ABI. A runtime owner maps each initialized
guest address to a separate stable native semaphore allocation. The original
PostgreSQL pg_sema.c algorithms are retained. Semaphore counts and waits are
real; another host thread can release a waiting operation. Active operations
prevent destruction, and runtime disposal releases remaining host allocations
before the guest heap is freed. Foreign and retired handles fail explicitly.
Multiprocess sharing of this owner-local map is outside the selected embedding
profile. Other host ABIs reject initialization.

Native Linux, emitted JIT and normally trimmed NativeAOT execute
`tests/Semaphore/probe.c`: sizeof/alignment, initial count, wait/trywait exhaustion,
post/getvalue and destroy. Both emitted modes use two fresh generated owners.
Evidence is under `artifacts/supervisor-portable/semaphore-{emission,jit,aot-build,aot-run}.log`;
private output is `build/semaphore-owner`. Two focused runtime tests additionally
cover owner separation and a true cross-thread blocking wait/post.

The same continuation implements dup/dup2 for owned file and console
descriptors. File duplicates share one FileStream/open description and therefore
its offset and status flags; descriptor flags and close lifetimes are separate.
Replacing an association through freopen releases only its reference. Closing
stdout retains duplicated stderr through upstream cleanup. Pipe/socket duplicate
endpoint accounting is explicitly unsupported and returns ENOTSUP.

Three new duplication tests and four affected stdio checks pass alongside the
two semaphore tests (`dup-tests.log`: nine passes). The actual complete fork
pglitec and emitted adapter exercise merged stderr, real pgl_exit, retained argv,
atexit draining and fresh owner state: 18 JIT and trimmed-AOT assertions pass on
immutable compiler 1820. See INITIALIZATION.md and
`artifacts/initdb-owner-20261022-r2/backend-adapter-1820/receipt.json`. These host
controls do not establish managed PostgreSQL SQL or P6 completion.

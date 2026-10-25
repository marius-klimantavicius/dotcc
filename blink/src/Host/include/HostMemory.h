#ifndef BLINK_HOST_MEMORY_H
#define BLINK_HOST_MEMORY_H
#include <stddef.h>
#include <stdint.h>
#include <sys/types.h>
#ifdef BLINK_MANAGED_GUEST_THREADS
#include <stdint.h>
/* Explicit opt-in shared owner; creation itself does not attach the caller.
 * Bind the same private I/O owner on every worker before attaching. Tokens are
 * generation IDs, never raw pointers or reused IDs. Exactly one active legacy
 * or shared owner may be bound per thread. Attach/detach do not allocate/free
 * guest backing. Creator joins all workers before final destroy; destroy refuses
 * attached owners and releases retained backing only after attachments reach 0.
 * Legacy End/DisposeWorker cannot destroy an attached shared context.
 *
 * One real process-private pthread mutex serializes shared registry, accounting,
 * protections and synchronous file callbacks. Callbacks must NOT reenter any
 * memory-owner API. Lock order: upstream mmap lock -> owner gate -> private I/O.
 * Raw backing access/lifetime still follows upstream guest synchronization.
 * Shared quota includes SharedOverhead() plus each rounded mapping and record.
 * The overhead is ABI-dependent and includes a conservative per-owner charge
 * for the process-wide registry gate and roots; it is not process RSS. */
uint64_t BlinkHostMemoryCreateShared(size_t);
int BlinkHostMemoryAttach(uint64_t);
int BlinkHostMemoryDetach(void);
int BlinkHostMemoryDestroyShared(uint64_t);
int BlinkHostMemorySharedSnapshot(uint64_t, size_t *, size_t *, size_t *);
size_t BlinkHostMemorySharedOverhead(void);
#endif

/* One active allocation owner per worker. The limit includes rounded payload
 * ownership records and one protection byte per page. No persistent CLR references live in C memory. */
int BlinkHostMemoryBegin(size_t);
typedef ssize_t (*BlinkHostMemoryReadAt)(int, void *, size_t, off_t);
typedef int (*BlinkHostMemoryReadLength)(int, off_t *);
/* Explicit per-owner callbacks; anonymous mappings need no file binding.
 * Callbacks borrow host buffers synchronously and must preserve fd position. */
int BlinkHostMemorySetFileReader(BlinkHostMemoryReadAt, BlinkHostMemoryReadLength);
/* Per-page shared guest-file leases. Acquire returns RW/NX backing and an opaque
 * generation token. Release/available must work after worker I/O is unbound.
 * Mutable shared backing is limited to guest memfds; immutable guest files
 * use read-only owned bytes. Other descriptors fail explicitly.
 * Callbacks must not reenter this memory owner; lock order remains owner -> I/O. */
typedef int (*BlinkHostMemoryAcquireShared)(int, off_t, void **, uint64_t *);
typedef int (*BlinkHostMemoryReleaseShared)(uint64_t);
typedef int (*BlinkHostMemorySharedAvailable)(uint64_t);
/* 0 = mutable memfd; 1 = immutable regular guest file; -1 = unsupported/error. */
typedef int (*BlinkHostMemorySharedReadOnly)(int);
/* SharedReadOnly: 0 = writable leased memory file, 1 = immutable snapshot,
 * 2 = mutable regular file with read-only leased coherent pages. */
int BlinkHostMemorySetSharedFiles(BlinkHostMemoryAcquireShared,
                                BlinkHostMemoryReleaseShared,
                                BlinkHostMemorySharedAvailable,
                                BlinkHostMemorySharedReadOnly);
/* Aggregate C-owned memory and sparse memfd storage under one owner limit.
 * Reserve callbacks may fail before allocation/publication; decreasing charges
 * and releasing the ledger must work after worker I/O unbinding. */
typedef int (*BlinkHostMemoryBindBudget)(size_t, size_t, uint64_t *);
typedef int (*BlinkHostMemoryAccountBudget)(uint64_t, size_t);
typedef int (*BlinkHostMemoryReleaseBudget)(uint64_t);
int BlinkHostMemorySetFileBudget(BlinkHostMemoryBindBudget,
                               BlinkHostMemoryAccountBudget,
                               BlinkHostMemoryReleaseBudget);
/* Configured mapping-owner budget; zero means no active owner. This is not
 * total process address space or an accounting of generic malloc allocations. */
size_t BlinkHostMemoryLimit(void);
size_t BlinkHostMemoryBytes(void);
size_t BlinkHostMemoryMappings(void);
/* Nonempty range wholly within one live mapping on the current owner. Pure
 * ownership test: no dereference, errno change, or native memory probing. */
int BlinkHostMemoryContains(const void *, size_t);
/* Software-only page mode, or -1 if not currently owned. No errno changes.
 * Modes NONE/READ/WRITE/RW never change raw backing's RW accessibility.
 * Guest page tables, not this metadata, enforce guest permissions. */
int BlinkHostMemoryProtection(const void *);
/* Read-only sharing metadata query. Both mutable and immutable shared file
 * pages return 1; private or unowned pages return 0. Does not probe backing. */
int BlinkHostMemoryFilePageShared(const void *);
/* File page availability, independent of protection. Private files use their
 * snapshot EOF; shared memfds use their current length, including after close.
 * Whole file pages
 * beyond EOF return 0 and must never be exposed through guest translation.
 * Partial final file pages return 1 and contain a zero-filled tail. Unowned
 * pointers return 0. This query does not dereference backing or change errno. */
int BlinkHostMemoryFilePageAvailable(const void *);
/* Query used before guest mprotect mutates any PTE. Private snapshots may be
 * writable; shared immutable images cannot gain write permission. */
int BlinkHostMemoryFilePageWritable(const void *);
/* mprotect supports aligned ranges within one owned mapping, rounded upward
 * to pages. No zero lengths, cross-mapping ranges, hardware protection or EXEC. */
int BlinkHostMemoryEnd(void);
/* Only after the entire upstream worker state and its slab-cache references
 * are discarded. This is deliberately distinct from FreeSystem(). */
void BlinkHostMemoryDisposeWorker(void);
long BlinkHostMemoryPageSize(void);
int BlinkHostMemoryAddressBits(void);
void *blink_host_mmap(void *, size_t, int, int, int, off_t);
int blink_host_munmap(void *, size_t);
int blink_host_mprotect(void *, size_t, int);
int blink_host_msync(void *, size_t, int);
#endif

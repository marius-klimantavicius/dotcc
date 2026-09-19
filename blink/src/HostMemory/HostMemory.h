#ifndef BLINK_HOST_MEMORY_H
#define BLINK_HOST_MEMORY_H
#include <stddef.h>
#include <sys/types.h>

/* One active allocation owner per worker. The limit includes rounded payload
 * ownership records and one protection byte per page. No persistent CLR references live in C memory. */
int BlinkHostMemoryBegin(size_t);
typedef ssize_t (*BlinkHostMemoryReadAt)(int, void *, size_t, off_t);
typedef int (*BlinkHostMemoryReadLength)(int, off_t *);
/* Explicit per-owner callbacks; anonymous mappings need no file binding.
 * Callbacks borrow host buffers synchronously and must preserve fd position. */
int BlinkHostMemorySetFileReader(BlinkHostMemoryReadAt, BlinkHostMemoryReadLength);
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

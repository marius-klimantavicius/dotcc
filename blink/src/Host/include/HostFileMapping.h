#ifndef BLINK_HOST_FILE_MAPPING_H
#define BLINK_HOST_FILE_MAPPING_H
#include "HostMemory.h"
/* Enable only after memory-owner Begin and InstanceIo binding on this worker. */
int BlinkHostMemoryEnablePrivateFiles(void);
/* Pin one open description across all per-page callbacks. The mapping caller
 * holds mmap_lock, but no memory-owner gate while beginning/ending this lease.
 * End on both publication and rollback; no guest fd slot is allocated. */
int BlinkHostFileMappingBegin(int);
int BlinkHostFileMappingEnd(void);
/* Guest-owned anonymous file, not a native host memfd. MFD_CLOEXEC is 1. */
int blink_host_memfd_create(const char *, unsigned);
#endif

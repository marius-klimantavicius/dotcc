/* Only the mapping operations used by portable C++/CLI assembly metadata.
   Constants belong to this managed contract, not the current host OS. */
#ifndef EDG_SELF_SYS_MMAN_H
#define EDG_SELF_SYS_MMAN_H
#include <sys/types.h>
#define PROT_READ 1
#define MAP_PRIVATE 2
#define MAP_FAILED ((void *)-1)
#ifdef __cplusplus
extern "C" {
#endif
void *mmap(void *, size_t, int, int, int, off_t);
int munmap(void *, size_t);
#ifdef __cplusplus
}
#endif
#endif

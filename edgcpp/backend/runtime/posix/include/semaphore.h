/* Portable unnamed, process-local POSIX semaphore ABI. The token refers to
   an owner-specific BCL object; this is never a native host sem_t layout. */
#ifndef EDG_POSIX_SEMAPHORE_H
#define EDG_POSIX_SEMAPHORE_H
#include <time.h>
#include <errno.h>
typedef struct __edg_posix_semaphore { unsigned long __token; } sem_t;
#ifndef SEM_VALUE_MAX
#define SEM_VALUE_MAX 2147483647
#endif
#ifdef __cplusplus
extern "C" {
#endif
int sem_init(sem_t *, int, unsigned int);
int sem_destroy(sem_t *);
int sem_wait(sem_t *);
int sem_trywait(sem_t *);
int sem_timedwait(sem_t *, const struct timespec *);
int sem_post(sem_t *);
int sem_getvalue(sem_t *, int *);
#ifdef __cplusplus
}
#endif
#endif

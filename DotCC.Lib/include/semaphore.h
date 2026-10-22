#ifndef DOTCC_SEMAPHORE_H
#define DOTCC_SEMAPHORE_H
/* Linux x64 LP64 opaque semaphore storage. The host owns its backing object. */
typedef union { long __align; unsigned char __size[32]; } sem_t;
int sem_init(sem_t *, int, unsigned int);
int sem_destroy(sem_t *);
int sem_wait(sem_t *);
int sem_trywait(sem_t *);
int sem_post(sem_t *);
int sem_getvalue(sem_t *, int *);
#endif

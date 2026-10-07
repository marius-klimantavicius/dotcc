#ifndef EDG_POSIX_PTHREAD_H
#define EDG_POSIX_PTHREAD_H
/* Declaration subset for portable clients. These are owner tokens, not native
   pthread layouts. No pthread function is bound or silently emulated: clients
   that call them require a provider. The implemented thread API is threads.h. */
#include <time.h>
typedef unsigned long pthread_t;
typedef unsigned long pthread_key_t;
typedef unsigned long pthread_once_t;
typedef struct { unsigned long __token; } pthread_mutex_t;
typedef struct { unsigned long __token; } pthread_cond_t;
typedef struct { unsigned long __token; } pthread_attr_t;
typedef struct { unsigned long __token; } pthread_mutexattr_t;
typedef struct { unsigned long __token; } pthread_condattr_t;
#define PTHREAD_MUTEX_INITIALIZER {0UL}
#define PTHREAD_COND_INITIALIZER {0UL}
#define PTHREAD_ONCE_INIT 0UL
#ifdef __cplusplus
extern "C" {
#endif
int pthread_create(pthread_t *, const pthread_attr_t *, void *(*)(void *), void *);
int pthread_join(pthread_t, void **);
int pthread_detach(pthread_t);
pthread_t pthread_self(void);
int pthread_equal(pthread_t, pthread_t);
int pthread_once(pthread_once_t *, void (*)(void));
int pthread_key_create(pthread_key_t *, void (*)(void *));
int pthread_key_delete(pthread_key_t);
void *pthread_getspecific(pthread_key_t);
int pthread_setspecific(pthread_key_t, const void *);
int pthread_mutex_init(pthread_mutex_t *, const pthread_mutexattr_t *);
int pthread_mutex_destroy(pthread_mutex_t *);
int pthread_mutex_lock(pthread_mutex_t *);
int pthread_mutex_trylock(pthread_mutex_t *);
int pthread_mutex_unlock(pthread_mutex_t *);
int pthread_cond_init(pthread_cond_t *, const pthread_condattr_t *);
int pthread_cond_destroy(pthread_cond_t *);
int pthread_cond_wait(pthread_cond_t *, pthread_mutex_t *);
int pthread_cond_timedwait(pthread_cond_t *, pthread_mutex_t *, const struct timespec *);
int pthread_cond_signal(pthread_cond_t *);
int pthread_cond_broadcast(pthread_cond_t *);
#ifdef __cplusplus
}
#endif
#endif

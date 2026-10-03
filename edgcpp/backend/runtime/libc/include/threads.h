/* Portable LP64 owner-token ABI v1; no native pthread layout. */
#ifndef __EDG_C11_THREADS_H
#define __EDG_C11_THREADS_H
#include <time.h>
#ifndef __cplusplus
#define thread_local _Thread_local
#endif
#ifdef __cplusplus
extern "C" {
#endif
typedef unsigned long thrd_t;
typedef unsigned long tss_t;
typedef unsigned long once_flag;
typedef struct __edg_c11_mutex { unsigned long __token; } mtx_t;
typedef struct __edg_c11_condition { unsigned long __token; } cnd_t;
typedef int (*thrd_start_t)(void *);
typedef void (*tss_dtor_t)(void *);
#define ONCE_FLAG_INIT 0UL
#define TSS_DTOR_ITERATIONS 4
enum { thrd_success=0, thrd_busy=1, thrd_error=2, thrd_nomem=3, thrd_timedout=4 };
enum { mtx_plain=0, mtx_recursive=1, mtx_timed=2 };
int thrd_create(thrd_t *,thrd_start_t,void *);
#ifdef __cplusplus
[[noreturn]] void thrd_exit(int);
#else
_Noreturn void thrd_exit(int);
#endif
int thrd_detach(thrd_t);
int thrd_join(thrd_t,int *);
int thrd_sleep(const struct timespec *,struct timespec *);
void thrd_yield(void);
thrd_t thrd_current(void);
int thrd_equal(thrd_t,thrd_t);
void call_once(once_flag *,void (*)(void));
int mtx_init(mtx_t *,int);
void mtx_destroy(mtx_t *);
int mtx_lock(mtx_t *);
int mtx_timedlock(mtx_t *,const struct timespec *);
int mtx_trylock(mtx_t *);
int mtx_unlock(mtx_t *);
int cnd_init(cnd_t *);
void cnd_destroy(cnd_t *);
int cnd_broadcast(cnd_t *);
int cnd_signal(cnd_t *);
int cnd_wait(cnd_t *,mtx_t *);
int cnd_timedwait(cnd_t *,mtx_t *,const struct timespec *);
int tss_create(tss_t *,tss_dtor_t);
void tss_delete(tss_t);
int tss_set(tss_t,void *);
void *tss_get(tss_t);
#ifdef __cplusplus
}
#endif
#endif

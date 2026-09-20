#include "config.h"
#ifdef THREAD_SIGNAL_FIRST
#include <signal.h>
#include <pthread.h>
#else
#include <pthread.h>
#include <signal.h>
#endif
#if !defined(BLINK_HOST_GUEST_THREADS_H) || !defined(BLINK_MANAGED_SIGNAL_H)
#error Both reviewed overlays are required
#endif
_Static_assert(sizeof(pthread_t)==8 && _Alignof(pthread_t)==8, "thread identity");
_Static_assert((pthread_t)-1<0, "signed internal thread identity");
_Static_assert(sizeof(pthread_mutex_t)==4 && _Alignof(pthread_mutex_t)==4, "mutex handle");
_Static_assert(sizeof(pthread_cond_t)==4 && _Alignof(pthread_cond_t)==4, "condition handle");
_Static_assert(sizeof(sigset_t)==128, "campaign signal storage");
#ifdef THREAD_SIGNAL_FIRST
int *ThreadedLayoutSignalFirst(pthread_mutexattr_t *value) { return value; }
#else
int *ThreadedLayoutPthreadFirst(pthread_mutexattr_t *value) { return value; }
#endif

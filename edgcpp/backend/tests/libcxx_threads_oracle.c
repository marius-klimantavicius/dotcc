/* Synthetic native reference adapter for the fixed external thread ABI.
   It runs only as the Linux C-oracle harness, never as translated runtime code. */
#define _GNU_SOURCE
#include <pthread.h>
#include <stdlib.h>
#include <errno.h>
#include <time.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include "__edg_thread_abi.h"
/* Select the host's supported thread-exit hook, not the EDG generic runtime's
   unconfigured fallback. The original EDG-generated C remains untouched. */
extern int __cxa_thread_atexit_impl(void (*)(void *), void *, void *);
int __cxa_thread_atexit(void (*destroy)(void *), void *object, void *dso) {
  return __cxa_thread_atexit_impl(destroy, object, dso);
}
static pthread_mutex_t registry = PTHREAD_MUTEX_INITIALIZER;
struct resource { union { pthread_mutex_t mutex; pthread_cond_t condition; pthread_once_t once; } value; };
static struct resource *get(unsigned long *slot, int kind) {
  struct resource *resource;
  pthread_mutex_lock(&registry);
  resource = (struct resource *)*slot;
  if (!resource) {
    resource = calloc(1, sizeof(*resource));
    if (resource) {
      int error = 0;
      if (kind == 2) error = pthread_cond_init(&resource->value.condition, 0);
      else if (kind == 3) resource->value.once = PTHREAD_ONCE_INIT;
      else {
        pthread_mutexattr_t attributes;
        pthread_mutexattr_init(&attributes);
        if (kind == 1) pthread_mutexattr_settype(&attributes, PTHREAD_MUTEX_RECURSIVE);
        error = pthread_mutex_init(&resource->value.mutex, &attributes);
        pthread_mutexattr_destroy(&attributes);
      }
      if (error) { free(resource); resource = 0; }
      else *slot = (unsigned long)resource;
    }
  }
  pthread_mutex_unlock(&registry);
  return resource;
}
int __edg_libcpp_recursive_mutex_init(unsigned long *slot) { return get(slot, 1) ? 0 : ENOMEM; }
int __edg_libcpp_mutex_lock(unsigned long *slot, int recursive) {
  struct resource *r = get(slot, recursive); return r ? pthread_mutex_lock(&r->value.mutex) : ENOMEM;
}
int __edg_libcpp_mutex_trylock(unsigned long *slot, int recursive) {
  struct resource *r = get(slot, recursive); return r && pthread_mutex_trylock(&r->value.mutex) == 0;
}
int __edg_libcpp_mutex_unlock(unsigned long *slot, int recursive) {
  (void)recursive; return pthread_mutex_unlock(&((struct resource *)*slot)->value.mutex);
}
int __edg_libcpp_mutex_destroy(unsigned long *slot, int recursive) {
  (void)recursive; if (!*slot) return 0;
  struct resource *r = (struct resource *)*slot;
  int error = pthread_mutex_destroy(&r->value.mutex);
  if (!error) { free(r); *slot = 0; } return error;
}
int __edg_libcpp_condvar_signal(unsigned long *slot, int broadcast) {
  struct resource *r = get(slot, 2); if (!r) return ENOMEM;
  return broadcast ? pthread_cond_broadcast(&r->value.condition) : pthread_cond_signal(&r->value.condition);
}
int __edg_libcpp_condvar_wait(unsigned long *condition, unsigned long *mutex, const struct timespec *deadline) {
  struct resource *cv = get(condition, 2), *m = get(mutex, 0); if (!cv || !m) return ENOMEM;
  return deadline ? pthread_cond_timedwait(&cv->value.condition, &m->value.mutex, deadline) :
                    pthread_cond_wait(&cv->value.condition, &m->value.mutex);
}
int __edg_libcpp_condvar_destroy(unsigned long *slot) {
  if (!*slot) return 0; struct resource *r = (struct resource *)*slot;
  int error = pthread_cond_destroy(&r->value.condition);
  if (!error) { free(r); *slot = 0; } return error;
}
int __edg_libcpp_execute_once(unsigned long *slot, void (*function)(void)) {
  struct resource *r = get(slot, 3); return r ? pthread_once(&r->value.once, function) : ENOMEM;
}
_Static_assert(sizeof(pthread_t) == sizeof(unsigned long), "native reference harness thread width");
int __edg_libcpp_thread_create(unsigned long *slot, void *(*function)(void *), void *argument) {
  return pthread_create((pthread_t *)slot, 0, function, argument);
}
unsigned long __edg_libcpp_thread_current_id(void) { return (unsigned long)pthread_self(); }
unsigned long __edg_libcpp_thread_get_id(const unsigned long *slot) { return *slot; }
int __edg_libcpp_thread_join(unsigned long *slot) { int error = pthread_join((pthread_t)*slot, 0); if (!error) *slot = 0; return error; }
int __edg_libcpp_thread_detach(unsigned long *slot) { int error = pthread_detach((pthread_t)*slot); if (!error) *slot = 0; return error; }
void __edg_libcpp_thread_yield(void) { sched_yield(); }
void __edg_libcpp_thread_sleep_for(long ns) {
  if (ns <= 0) return; struct timespec remaining = { ns / 1000000000L, ns % 1000000000L };
  while (nanosleep(&remaining, &remaining) && errno == EINTR) { }
}
int __edg_libcpp_tls_create(unsigned long *slot, void (*destructor)(void *)) {
  pthread_key_t key; int error = pthread_key_create(&key, destructor); if (!error) *slot = (unsigned long)key + 1; return error;
}
void *__edg_libcpp_tls_get(unsigned long key) { return pthread_getspecific((pthread_key_t)(key - 1)); }
int __edg_libcpp_tls_set(unsigned long key, void *value) { return pthread_setspecific((pthread_key_t)(key - 1), value); }
extern int thread_cases(void);
extern int thread_diagnostics(int);
int main(void) {
  int result = thread_cases();
  if (result) for (int i = 0; i != 5; ++i) printf("thread diagnostic %d=%d\n", i, thread_diagnostics(i));
  return result;
}

/* Atomic lock state remains in translated owner globals. Yield uses the
   portable C11/BCL thread service; no pthread layout or compile-host choice. */
#ifndef EDG_PRNG_LOCK_H
#define EDG_PRNG_LOCK_H
#include <threads.h>
static void __edg_prng_lock(volatile int *word) {
  while (__atomic_exchange_n(word,1,__ATOMIC_ACQUIRE)) {
    while (__atomic_load_n(word,__ATOMIC_RELAXED)) thrd_yield();
  }
}
#define LOCK(p) __edg_prng_lock(p)
#define UNLOCK(p) __atomic_store_n(p,0,__ATOMIC_RELEASE)
#endif

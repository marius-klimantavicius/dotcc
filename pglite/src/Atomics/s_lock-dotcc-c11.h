/*
 * PostgreSQL spinlock compiler port for the private DotCC C11 overlay.
 * Only the hardware/compiler primitives live here. storage/s_lock.h keeps
 * S_LOCK and src/backend/storage/lmgr/s_lock.c keeps all contention/backoff.
 */
#ifndef S_LOCK_H
#error "include via storage/s_lock.h"
#endif
#ifndef PGLITE_DOTCC_C11_SPINLOCKS
#error "explicit C11 spinlock port selection required"
#endif

#include <stdatomic.h>

typedef _Atomic unsigned char slock_t;

_Static_assert(sizeof(void *) == 8 && sizeof(long) == 8, "LP64 port only");
_Static_assert(sizeof(slock_t) == 1 && _Alignof(slock_t) == 1,
               "one-byte PostgreSQL spinlock required");
_Static_assert(ATOMIC_CHAR_LOCK_FREE == 2, "lock-free byte atomics required");

#define HAS_TEST_AND_SET

static inline int
pglite_c11_tas(volatile slock_t *lock)
{
    /* TAS returns ZERO on acquisition (opposite to pg_atomic_flag). */
    return atomic_exchange_explicit(lock, 1, memory_order_acquire);
}

static inline int
pglite_c11_tas_spin(volatile slock_t *lock)
{
    /* Preserve the x64 test-before-exchange contention optimization. */
    return atomic_load_explicit(lock, memory_order_relaxed) != 0
        ? 1 : pglite_c11_tas(lock);
}

#define TAS(lock) pglite_c11_tas(lock)
#define TAS_SPIN(lock) pglite_c11_tas_spin(lock)
#define S_INIT_LOCK(lock) atomic_init((lock), 0)
#define S_UNLOCK(lock) atomic_store_explicit((lock), 0, memory_order_release)
#define S_LOCK_FREE(lock) (atomic_load_explicit((lock), memory_order_relaxed) == 0)

/* PostgreSQL's portable SPIN_DELAY fallback is an empty processor hint.
 * The upstream spin-count, sleep, randomized backoff and timeout remain live.
 */

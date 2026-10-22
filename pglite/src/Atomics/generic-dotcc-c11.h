/*
 * Experimental PostgreSQL compiler port for DotCC's shared C11 atomics.
 * Include only through the privately staged port/atomics.h selection.
 *
 * LP64 Linux x64 only. Numeric _Atomic fields retain the selected native
 * PostgreSQL sizes/alignment; a C11 atomic_flag would change the flag layout.
 * DotCC maps these standard operations to shared same-width Interlocked
 * primitives. Its memory orders are conservatively strengthened to seq-cst.
 */
#ifndef INSIDE_ATOMICS_H
#error "include via port/atomics.h"
#endif
#ifndef PGLITE_DOTCC_C11_ATOMICS
#error "explicit experimental port selection required"
#endif
#if defined(PG_DISABLE_64_BIT_ATOMICS)
#error "this bounded port requires native-width 64-bit atomics"
#endif

#include <stdatomic.h>

#define PG_HAVE_ATOMIC_U32_SUPPORT
typedef struct pg_atomic_uint32
{
    volatile _Atomic uint32 value;
} pg_atomic_uint32;

#define PG_HAVE_ATOMIC_U64_SUPPORT
typedef struct pg_atomic_uint64
{
    volatile _Atomic uint64 value;
} pg_atomic_uint64;

#define PG_HAVE_ATOMIC_FLAG_SUPPORT
typedef struct pg_atomic_flag
{
    volatile _Atomic unsigned char value;
} pg_atomic_flag;

/* Fail closed on another native atomic representation or data model. */
_Static_assert(sizeof(void *) == 8 && sizeof(long) == 8, "LP64 port only");
_Static_assert(sizeof(pg_atomic_uint32) == 4 && _Alignof(pg_atomic_uint32) == 4,
               "pg_atomic_uint32 layout");
_Static_assert(sizeof(pg_atomic_uint64) == 8 && _Alignof(pg_atomic_uint64) == 8,
               "pg_atomic_uint64 layout");
_Static_assert(sizeof(pg_atomic_flag) == 1 && _Alignof(pg_atomic_flag) == 1,
               "pg_atomic_flag layout");
_Static_assert(ATOMIC_CHAR_LOCK_FREE == 2 && ATOMIC_INT_LOCK_FREE == 2 &&
               ATOMIC_LONG_LOCK_FREE == 2, "lock-free numeric atomics required");

#define pg_compiler_barrier_impl() atomic_signal_fence(memory_order_seq_cst)
#define pg_memory_barrier_impl() atomic_thread_fence(memory_order_seq_cst)
#define pg_read_barrier_impl() atomic_thread_fence(memory_order_acquire)
#define pg_write_barrier_impl() atomic_thread_fence(memory_order_release)

#define PG_HAVE_ATOMIC_INIT_U32
static inline void
pg_atomic_init_u32_impl(volatile pg_atomic_uint32 *ptr, uint32 value)
{
    atomic_init(&ptr->value, value);
}

#define PG_HAVE_ATOMIC_READ_U32
static inline uint32
pg_atomic_read_u32_impl(volatile pg_atomic_uint32 *ptr)
{
    return atomic_load_explicit(&ptr->value, memory_order_relaxed);
}

#define PG_HAVE_ATOMIC_WRITE_U32
static inline void
pg_atomic_write_u32_impl(volatile pg_atomic_uint32 *ptr, uint32 value)
{
    atomic_store_explicit(&ptr->value, value, memory_order_relaxed);
}

#define PG_HAVE_ATOMIC_COMPARE_EXCHANGE_U32
static inline bool
pg_atomic_compare_exchange_u32_impl(volatile pg_atomic_uint32 *ptr,
                                   uint32 *expected, uint32 value)
{
    return atomic_compare_exchange_strong_explicit(&ptr->value, expected, value,
                    memory_order_seq_cst, memory_order_seq_cst);
}

#define PG_HAVE_ATOMIC_INIT_U64
static inline void
pg_atomic_init_u64_impl(volatile pg_atomic_uint64 *ptr, uint64 value)
{
    atomic_init(&ptr->value, value);
}

#define PG_HAVE_ATOMIC_READ_U64
static inline uint64
pg_atomic_read_u64_impl(volatile pg_atomic_uint64 *ptr)
{
    return atomic_load_explicit(&ptr->value, memory_order_relaxed);
}

#define PG_HAVE_ATOMIC_WRITE_U64
static inline void
pg_atomic_write_u64_impl(volatile pg_atomic_uint64 *ptr, uint64 value)
{
    atomic_store_explicit(&ptr->value, value, memory_order_relaxed);
}

#define PG_HAVE_ATOMIC_COMPARE_EXCHANGE_U64
static inline bool
pg_atomic_compare_exchange_u64_impl(volatile pg_atomic_uint64 *ptr,
                                   uint64 *expected, uint64 value)
{
    return atomic_compare_exchange_strong_explicit(&ptr->value, expected, value,
                    memory_order_seq_cst, memory_order_seq_cst);
}

#define PG_HAVE_ATOMIC_INIT_FLAG
static inline void
pg_atomic_init_flag_impl(volatile pg_atomic_flag *ptr)
{
    atomic_init(&ptr->value, 0);
}

#define PG_HAVE_ATOMIC_TEST_SET_FLAG
static inline bool
pg_atomic_test_set_flag_impl(volatile pg_atomic_flag *ptr)
{
    /* PostgreSQL returns true on acquiring a previously clear flag. */
    return atomic_exchange_explicit(&ptr->value, 1, memory_order_acquire) == 0;
}

#define PG_HAVE_ATOMIC_UNLOCKED_TEST_FLAG
static inline bool
pg_atomic_unlocked_test_flag_impl(volatile pg_atomic_flag *ptr)
{
    return atomic_load_explicit(&ptr->value, memory_order_relaxed) == 0;
}

#define PG_HAVE_ATOMIC_CLEAR_FLAG
static inline void
pg_atomic_clear_flag_impl(volatile pg_atomic_flag *ptr)
{
    atomic_store_explicit(&ptr->value, 0, memory_order_release);
}

/* Higher operations and spin-delay fallback remain PostgreSQL generic.h.
 * No PG_HAVE_8BYTE_SINGLE_COPY_ATOMICITY assertion is needed: this port supplies
 * explicit C11 read/write primitives instead of the generic plain accesses.
 */

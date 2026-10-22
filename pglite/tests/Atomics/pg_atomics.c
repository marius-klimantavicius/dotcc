/* Actual pinned PostgreSQL wrappers and generic CAS state machines. */
#include "c.h"
#include "port/atomics.h"

typedef struct dpga_state
{
    uint32 guard;
    pg_atomic_uint32 u32;
    pg_atomic_uint64 u64;
    unsigned char before;
    pg_atomic_flag flag;
    unsigned char after;
    uint64 protected_value;
    pg_atomic_uint32 ready;
    uint64 payload;
} dpga_state;

size_t dpga_layout(unsigned field)
{
    switch (field)
    {
        case 0: return sizeof(pg_atomic_uint32);
        case 1: return _Alignof(pg_atomic_uint32);
        case 2: return offsetof(pg_atomic_uint32, value);
        case 3: return sizeof(pg_atomic_uint64);
        case 4: return _Alignof(pg_atomic_uint64);
        case 5: return offsetof(pg_atomic_uint64, value);
        case 6: return sizeof(pg_atomic_flag);
        case 7: return _Alignof(pg_atomic_flag);
        case 8: return offsetof(pg_atomic_flag, value);
        case 9: return sizeof(dpga_state);
        case 10: return _Alignof(dpga_state);
        case 11: return offsetof(dpga_state, u32);
        case 12: return offsetof(dpga_state, u64);
        case 13: return offsetof(dpga_state, flag);
        case 14: return offsetof(dpga_state, after);
        case 15: return offsetof(dpga_state, protected_value);
        default: return 0;
    }
}

/* Failures identify actual C assertions by their stable source line. */
#define CHECK(condition) do { if (!(condition)) return __LINE__; } while (0)

int dpga_check(void)
{
    pg_atomic_uint32 a;
    pg_atomic_uint64 b;
    uint32 e32;
    uint64 e64;
    dpga_state state;

    pg_atomic_init_u32(&a, 17);
    CHECK(pg_atomic_read_u32(&a) == 17);
    pg_atomic_write_u32(&a, 0x80000000U);
    CHECK(pg_atomic_read_u32(&a) == 0x80000000U);
    e32 = 0;
    CHECK(!pg_atomic_compare_exchange_u32(&a, &e32, 42));
    CHECK(e32 == 0x80000000U && pg_atomic_read_u32(&a) == 0x80000000U);
    e32 = 0x80000000U;
    CHECK(pg_atomic_compare_exchange_u32(&a, &e32, 9));
    CHECK(e32 == 0x80000000U && pg_atomic_read_u32(&a) == 9);
    e32 = 6;
    CHECK(!pg_atomic_compare_exchange_u32(&a, &e32, 42));
    CHECK(e32 == 9 && pg_atomic_read_u32(&a) == 9);
    CHECK(pg_atomic_fetch_add_u32(&a, 7) == 9);
    CHECK(pg_atomic_read_u32(&a) == 16);
    CHECK(pg_atomic_fetch_sub_u32(&a, 3) == 16);
    CHECK(pg_atomic_read_u32(&a) == 13);
    CHECK(pg_atomic_fetch_add_u32(&a, -4) == 13);
    CHECK(pg_atomic_fetch_sub_u32(&a, -7) == 9);
    CHECK(pg_atomic_read_u32(&a) == 16);
    CHECK(pg_atomic_exchange_u32(&a, 0xf0U) == 16);
    CHECK(pg_atomic_fetch_and_u32(&a, 0x3cU) == 0xf0U);
    CHECK(pg_atomic_read_u32(&a) == 0x30U);
    CHECK(pg_atomic_fetch_or_u32(&a, 0x83U) == 0x30U);
    CHECK(pg_atomic_read_u32(&a) == 0xb3U);
    CHECK(pg_atomic_add_fetch_u32(&a, 2) == 0xb5U);
    CHECK(pg_atomic_sub_fetch_u32(&a, 3) == 0xb2U);
    pg_atomic_write_membarrier_u32(&a, 0xffffffffU);
    CHECK(pg_atomic_read_membarrier_u32(&a) == 0xffffffffU);
    CHECK(pg_atomic_fetch_add_u32(&a, 1) == 0xffffffffU);
    CHECK(pg_atomic_read_u32(&a) == 0);
    pg_atomic_unlocked_write_u32(&a, 7);
    CHECK(pg_atomic_read_u32(&a) == 7);

    pg_atomic_init_u64(&b, 0x8000000100000002ULL);
    CHECK(pg_atomic_read_u64(&b) == 0x8000000100000002ULL);
    pg_atomic_write_u64(&b, 0x8000000200000003ULL);
    e64 = 0;
    CHECK(!pg_atomic_compare_exchange_u64(&b, &e64, 42));
    CHECK(e64 == 0x8000000200000003ULL && pg_atomic_read_u64(&b) == 0x8000000200000003ULL);
    e64 = 0x8000000200000003ULL;
    CHECK(pg_atomic_compare_exchange_u64(&b, &e64, 9));
    CHECK(e64 == 0x8000000200000003ULL && pg_atomic_read_u64(&b) == 9);
    e64 = 0x100000009ULL;
    CHECK(!pg_atomic_compare_exchange_u64(&b, &e64, 42));
    CHECK(e64 == 9 && pg_atomic_read_u64(&b) == 9);
    CHECK(pg_atomic_fetch_add_u64(&b, 0x100000007LL) == 9);
    CHECK(pg_atomic_read_u64(&b) == 0x100000010ULL);
    CHECK(pg_atomic_fetch_sub_u64(&b, 3) == 0x100000010ULL);
    CHECK(pg_atomic_read_u64(&b) == 0x10000000dULL);
    CHECK(pg_atomic_fetch_add_u64(&b, -4) == 0x10000000dULL);
    CHECK(pg_atomic_fetch_sub_u64(&b, -7) == 0x100000009ULL);
    CHECK(pg_atomic_exchange_u64(&b, 0xf0000000f0ULL) == 0x100000010ULL);
    CHECK(pg_atomic_fetch_and_u64(&b, 0x3c0000003cULL) == 0xf0000000f0ULL);
    CHECK(pg_atomic_read_u64(&b) == 0x3000000030ULL);
    CHECK(pg_atomic_fetch_or_u64(&b, 0x8300000083ULL) == 0x3000000030ULL);
    CHECK(pg_atomic_read_u64(&b) == 0xb3000000b3ULL);
    CHECK(pg_atomic_add_fetch_u64(&b, 2) == 0xb3000000b5ULL);
    CHECK(pg_atomic_sub_fetch_u64(&b, 3) == 0xb3000000b2ULL);
    pg_atomic_write_membarrier_u64(&b, 0xffffffffffffffffULL);
    CHECK(pg_atomic_read_membarrier_u64(&b) == 0xffffffffffffffffULL);
    CHECK(pg_atomic_fetch_add_u64(&b, 1) == 0xffffffffffffffffULL);
    CHECK(pg_atomic_read_u64(&b) == 0);
    CHECK(pg_atomic_monotonic_advance_u64(&b, 0x100000001ULL) == 0x100000001ULL);
    CHECK(pg_atomic_monotonic_advance_u64(&b, 3) == 0x100000001ULL);

    state.before = 0xa5;
    state.after = 0x5a;
    pg_atomic_init_flag(&state.flag);
    CHECK(pg_atomic_unlocked_test_flag(&state.flag));
    CHECK(pg_atomic_test_set_flag(&state.flag));
    CHECK(!pg_atomic_unlocked_test_flag(&state.flag));
    CHECK(!pg_atomic_test_set_flag(&state.flag));
    pg_atomic_clear_flag(&state.flag);
    CHECK(pg_atomic_unlocked_test_flag(&state.flag));
    CHECK(pg_atomic_test_set_flag(&state.flag));
    pg_atomic_clear_flag(&state.flag);
    CHECK(state.before == 0xa5 && state.after == 0x5a);

    pg_compiler_barrier();
    pg_memory_barrier();
    pg_read_barrier();
    pg_write_barrier();
    return 0;
}

void dpga_init(void *storage)
{
    dpga_state *s = storage;
    s->guard = 0xdeadbeefU;
    s->before = 0xa5;
    s->after = 0x5a;
    s->protected_value = 0;
    s->payload = 0;
    pg_atomic_init_u32(&s->u32, 0);
    pg_atomic_init_u64(&s->u64, 0x100000000ULL);
    pg_atomic_init_flag(&s->flag);
    pg_atomic_init_u32(&s->ready, 0);
}

void dpga_worker(void *storage, unsigned iterations)
{
    dpga_state *s = storage;
    for (unsigned i = 0; i < iterations; ++i)
    {
        pg_atomic_fetch_add_u32(&s->u32, 1);
        pg_atomic_fetch_add_u64(&s->u64, 1);
        while (!pg_atomic_test_set_flag(&s->flag)) pg_spin_delay();
        ++s->protected_value;
        pg_atomic_clear_flag(&s->flag);
    }
}

int dpga_finish(void *storage, unsigned total)
{
    dpga_state *s = storage;
    CHECK(pg_atomic_read_u32(&s->u32) == total);
    CHECK(pg_atomic_read_u64(&s->u64) == 0x100000000ULL + total);
    CHECK(s->protected_value == total);
    CHECK(pg_atomic_unlocked_test_flag(&s->flag));
    CHECK(s->guard == 0xdeadbeefU && s->before == 0xa5 && s->after == 0x5a);
    return 0;
}

/* Handshake exercises release/acquire barriers around non-atomic payload. */
void dpga_publish(void *storage, unsigned iterations)
{
    dpga_state *s = storage;
    for (unsigned i = 1; i <= iterations; ++i)
    {
        while (pg_atomic_read_u32(&s->ready) != 0) pg_spin_delay();
        pg_read_barrier();
        s->payload = 0x100000000ULL + i;
        pg_write_barrier();
        pg_atomic_write_u32(&s->ready, 1);
    }
}

int dpga_consume(void *storage, unsigned iterations)
{
    dpga_state *s = storage;
    int failed = 0;
    for (unsigned i = 1; i <= iterations; ++i)
    {
        while (pg_atomic_read_u32(&s->ready) == 0) pg_spin_delay();
        pg_read_barrier();
        if (s->payload != 0x100000000ULL + i) failed = __LINE__;
        pg_write_barrier();
        pg_atomic_write_u32(&s->ready, 0);
    }
    return failed;
}

#ifdef DPGA_NATIVE_MAIN
#include <stdio.h>
#include <pthread.h>
#undef printf
#undef fprintf
typedef struct native_worker { dpga_state *s; unsigned n; } native_worker;
static void *native_add(void *arg)
{
    native_worker *w = arg;
    dpga_worker(w->s, w->n);
    return NULL;
}
static void *native_publish(void *arg)
{
    native_worker *w = arg;
    dpga_publish(w->s, w->n);
    return NULL;
}
int main(void)
{
    dpga_state s;
    pthread_t threads[4];
    native_worker w = {&s, 5000};
    int status = dpga_check();
    if (status) { fprintf(stderr, "C assertion failed at %d\n", status); return 1; }
    dpga_init(&s);
    for (unsigned i = 0; i < 4; ++i)
        if (pthread_create(&threads[i], NULL, native_add, &w)) return 2;
    for (unsigned i = 0; i < 4; ++i)
        if (pthread_join(threads[i], NULL)) return 3;
    status = dpga_finish(&s, 20000);
    if (status) { fprintf(stderr, "Concurrent assertion failed at %d\n", status); return 4; }
    w.n = 2000;
    if (pthread_create(&threads[0], NULL, native_publish, &w)) return 5;
    status = dpga_consume(&s, w.n);
    if (pthread_join(threads[0], NULL) || status) return 6;
    printf("{\"layout\":[");
    for (unsigned i = 0; i < 16; ++i)
        printf("%s%zu", i ? "," : "", dpga_layout(i));
    printf("],\"behavior\":true,\"workers\":4,\"increments\":20000,\"messages\":2000}\n");
    return 0;
}
#endif

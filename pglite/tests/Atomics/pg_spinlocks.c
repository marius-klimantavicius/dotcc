/* Actual PostgreSQL S_LOCK, contention/backoff, PRNG and sleep implementations.
 * This fixture's sleep observer records entry into the upstream wait path and
 * calls the original pg_usleep. No algorithm text is copied or replaced.
 */
#include "postgres.h"
#include <stdatomic.h>

/* Storage only for the actual header's unused MemoryContextSwitchTo inline;
 * this test implements and invokes no memory-context/database operation. */
MemoryContext CurrentMemoryContext;

/* Use the standard test host I/O, not backend printf-name interception. */
#undef printf
#undef fprintf

static void dpgs_observed_usleep(long microsec);
#define S_LOCK_TEST
#define main dpgs_upstream_test_main
#define pg_usleep dpgs_observed_usleep
#include "src/backend/storage/lmgr/s_lock.c"
#undef pg_usleep
#undef main
#undef S_LOCK_TEST
#include "src/common/pg_prng.c"
#include "src/port/pg_bitutils.c"
#include "src/port/pgsleep.c"

static _Atomic unsigned dpgs_sleeps;
static _Atomic unsigned dpgs_wait_errors;
static long dpgs_last_sleep;
static unsigned dpgs_backoff[5];

static void
dpgs_observed_usleep(long microsec)
{
    if (*my_wait_event_info != WAIT_EVENT_SPIN_DELAY || microsec < 1000 || microsec > 1000000)
        atomic_fetch_add(&dpgs_wait_errors, 1);
    dpgs_last_sleep = microsec;
    atomic_fetch_add(&dpgs_sleeps, 1);
    pg_usleep(microsec);
}

typedef struct dpgs_state
{
    unsigned char before;
    slock_t lock;
    unsigned char after;
    uint64 value;
    uint64 payload;
    _Atomic unsigned phase;
} dpgs_state;

#define CHECK(condition) do { if (!(condition)) return __LINE__; } while (0)

size_t dpgs_layout(unsigned field)
{
    switch (field)
    {
        case 0: return sizeof(slock_t);
        case 1: return _Alignof(slock_t);
        case 2: return sizeof(dpgs_state);
        case 3: return _Alignof(dpgs_state);
        case 4: return offsetof(dpgs_state, lock);
        case 5: return offsetof(dpgs_state, after);
        case 6: return offsetof(dpgs_state, value);
        case 7: return offsetof(dpgs_state, phase);
        default: return 0;
    }
}

void dpgs_init(void *storage)
{
    dpgs_state *s = storage;
    s->before = 0xa5;
    s->after = 0x5a;
    s->value = 0;
    s->payload = 0;
    atomic_init(&s->phase, 0);
    S_INIT_LOCK(&s->lock);
}

int dpgs_check(void)
{
    dpgs_state s;
    SpinDelayStatus delay;
    dpgs_init(&s);
    CHECK(S_LOCK_FREE(&s.lock));
    CHECK(TAS(&s.lock) == 0);
    CHECK(!S_LOCK_FREE(&s.lock));
    CHECK(TAS(&s.lock) != 0 && TAS_SPIN(&s.lock) != 0);
    S_UNLOCK(&s.lock);
    CHECK(S_LOCK_FREE(&s.lock));
    CHECK(TAS_SPIN(&s.lock) == 0);
    S_UNLOCK(&s.lock);
    CHECK(S_LOCK(&s.lock) == 0);
    S_UNLOCK(&s.lock);
    CHECK(s.before == 0xa5 && s.after == 0x5a);

    /* Deterministic checks of the original adaptive delay/PRNG algorithm. */
    pg_prng_seed(&pg_global_prng_state, 42);
    set_spins_per_delay(DEFAULT_SPINS_PER_DELAY);
    init_local_spin_delay(&delay);
    CHECK(delay.spins == 0 && delay.delays == 0 && delay.cur_delay == 0);
    perform_spin_delay(&delay);
    CHECK(delay.spins == 1 && delay.delays == 0 && delay.cur_delay == 0);
    finish_spin_delay(&delay);
    CHECK(update_spins_per_delay(DEFAULT_SPINS_PER_DELAY) == 106);
    set_spins_per_delay(MIN_SPINS_PER_DELAY);
    for (unsigned i = 0; i < 4; ++i)
    {
        unsigned before = i ? (unsigned) delay.cur_delay : MIN_DELAY_USEC;
        delay.spins = MIN_SPINS_PER_DELAY - 1;
        perform_spin_delay(&delay);
        CHECK(delay.spins == 0 && delay.delays == (int) i + 1);
        CHECK(dpgs_last_sleep == (long) before);
        CHECK(delay.cur_delay >= (int) before && delay.cur_delay <= (int) before * 2);
        CHECK(*my_wait_event_info == 0);
        dpgs_backoff[i] = (unsigned) delay.cur_delay;
    }
    delay.cur_delay = MAX_DELAY_USEC;
    delay.spins = MIN_SPINS_PER_DELAY - 1;
    perform_spin_delay(&delay);
    CHECK(dpgs_last_sleep == MAX_DELAY_USEC && delay.cur_delay == MIN_DELAY_USEC);
    dpgs_backoff[4] = (unsigned) delay.cur_delay;
    set_spins_per_delay(DEFAULT_SPINS_PER_DELAY);
    finish_spin_delay(&delay);
    CHECK(update_spins_per_delay(DEFAULT_SPINS_PER_DELAY) == 99);
    CHECK(atomic_load(&dpgs_wait_errors) == 0);
    return 0;
}

unsigned dpgs_backoff_value(unsigned i) { return i < 5 ? dpgs_backoff[i] : 0; }

/* Four workers exercise byte TAS mutual exclusion. s_lock.c's adaptive state
 * is process-local upstream; only one slow-path waiter uses it below. */
void dpgs_tas_worker(void *storage, unsigned iterations)
{
    dpgs_state *s = storage;
    for (unsigned i = 0; i < iterations; ++i)
    {
        while (TAS_SPIN(&s->lock)) SPIN_DELAY();
        ++s->value;
        S_UNLOCK(&s->lock);
    }
}

int dpgs_finish(void *storage, unsigned total)
{
    dpgs_state *s = storage;
    CHECK(s->value == total && S_LOCK_FREE(&s->lock));
    CHECK(s->before == 0xa5 && s->after == 0x5a);
    CHECK(atomic_load(&dpgs_wait_errors) == 0);
    return 0;
}

void dpgs_holder_begin(void *storage, unsigned round)
{
    dpgs_state *s = storage;
    S_LOCK(&s->lock);
    s->payload = 0x100000000ULL + round;
    atomic_store(&dpgs_sleeps, 0);
    /* Coordination is relaxed: payload publication relies on the spinlock. */
    atomic_store_explicit(&s->phase, 1, memory_order_relaxed);
}

unsigned dpgs_sleeping(void) { return atomic_load(&dpgs_sleeps); }

int dpgs_holder_end(void *storage, unsigned round)
{
    dpgs_state *s = storage;
    CHECK(atomic_load(&dpgs_sleeps) > 0);
    CHECK(s->value == round - 1); /* Waiter cannot enter while holder owns it. */
    S_UNLOCK(&s->lock);
    while (atomic_load_explicit(&s->phase, memory_order_relaxed) != 0) { }
    S_LOCK(&s->lock);
    CHECK(s->payload == 0x200000000ULL + round && s->value == round);
    S_UNLOCK(&s->lock);
    return 0;
}

int dpgs_waiter(void *storage, unsigned rounds)
{
    dpgs_state *s = storage;
    set_spins_per_delay(MIN_SPINS_PER_DELAY);
    for (unsigned i = 1; i <= rounds; ++i)
    {
        while (atomic_load_explicit(&s->phase, memory_order_relaxed) != 1) { }
        int delays = S_LOCK(&s->lock);
        CHECK(delays > 0 && *my_wait_event_info == 0);
        CHECK(s->payload == 0x100000000ULL + i);
        ++s->value;
        s->payload = 0x200000000ULL + i;
        S_UNLOCK(&s->lock);
        atomic_store_explicit(&s->phase, 0, memory_order_relaxed);
    }
    return 0;
}

/* Bound the actual upstream timeout by starting at its final failed attempt.
 * This invokes s_lock_stuck and must terminate with exit 1, never succeed. */
void dpgs_stuck(void)
{
    SpinDelayStatus delay;
    set_spins_per_delay(MIN_SPINS_PER_DELAY);
    init_local_spin_delay(&delay);
    delay.delays = NUM_DELAYS;
    delay.spins = MIN_SPINS_PER_DELAY - 1;
    perform_spin_delay(&delay);
}

#ifdef DPGS_NATIVE_MAIN
#include <pthread.h>
#undef printf
#undef fprintf

typedef struct dpgs_worker_arg { dpgs_state *s; unsigned iterations; int status; } dpgs_worker_arg;
static void *dpgs_native_tas(void *arg)
{
    dpgs_worker_arg *w = arg;
    dpgs_tas_worker(w->s, w->iterations);
    return NULL;
}
static void *dpgs_native_waiter(void *arg)
{
    dpgs_worker_arg *w = arg;
    w->status = dpgs_waiter(w->s, w->iterations);
    return NULL;
}
int main(int argc, char **argv)
{
    dpgs_state s;
    pthread_t threads[4];
    dpgs_worker_arg w = {&s, 5000, 0};
    if (argc == 2 && strcmp(argv[1], "--stuck") == 0) { dpgs_stuck(); return 20; }
    int status = dpgs_check();
    if (status) { fprintf(stderr, "Spinlock assertion failed at %d\n", status); return 1; }
    dpgs_init(&s);
    for (unsigned i = 0; i < 4; ++i)
        if (pthread_create(&threads[i], NULL, dpgs_native_tas, &w)) return 2;
    for (unsigned i = 0; i < 4; ++i)
        if (pthread_join(threads[i], NULL)) return 3;
    status = dpgs_finish(&s, 20000);
    if (status) { fprintf(stderr, "TAS assertion failed at %d\n", status); return 4; }
    dpgs_init(&s);
    w.iterations = 64;
    if (pthread_create(&threads[0], NULL, dpgs_native_waiter, &w)) return 5;
    for (unsigned i = 1; i <= w.iterations; ++i)
    {
        dpgs_holder_begin(&s, i);
        while (!dpgs_sleeping()) { }
        status = dpgs_holder_end(&s, i);
        if (status) { fprintf(stderr, "Holder assertion failed at %d\n", status); return 6; }
    }
    if (pthread_join(threads[0], NULL) || w.status) return 7;
    status = dpgs_finish(&s, w.iterations);
    if (status) return 8;
    printf("\n{\"layout\":[");
    for (unsigned i = 0; i < 8; ++i)
        printf("%s%zu", i ? "," : "", dpgs_layout(i));
    printf("],\"backoff\":[");
    for (unsigned i = 0; i < 5; ++i)
        printf("%s%u", i ? "," : "", dpgs_backoff_value(i));
    printf("],\"behavior\":true,\"workers\":4,\"increments\":20000,\"delayed_rounds\":64}\n");
    return 0;
}
#endif

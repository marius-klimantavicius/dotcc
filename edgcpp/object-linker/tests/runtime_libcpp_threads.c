/* Exercise the fixed C boundary consumed by libc++'s external thread headers. */
#include <__edg_thread_abi.h>
#include <errno.h>
#include <stdlib.h>
static unsigned long mutex, condition, once_flag, key;
static int count, once_calls, destructions, failures, notified, detached_done;
static _Thread_local int source_tls;

static void initialize(void) { ++once_calls; }
static void destroy_key(void *argument) {
    if (source_tls != 19 || errno != 37) __atomic_fetch_add(&failures, 1, __ATOMIC_SEQ_CST);
    free(argument);
    __atomic_fetch_add(&destructions, 1, __ATOMIC_SEQ_CST);
}
static void *worker(void *unused) {
    (void)unused;
    if (source_tls || !__edg_libcpp_thread_current_id()) __atomic_fetch_add(&failures, 1, __ATOMIC_SEQ_CST);
    source_tls = 19;
    errno = 37;
    void *p = malloc(16);
    if (!p || __edg_libcpp_tls_set(key, p) || __edg_libcpp_tls_get(key) != p) return 0;
    if (__edg_libcpp_execute_once(&once_flag, initialize)) return 0;
    for (int i = 0; i < 1000; ++i) {
        if (__edg_libcpp_mutex_lock(&mutex, 0)) return 0;
        ++count;
        if (__edg_libcpp_mutex_unlock(&mutex, 0)) return 0;
    }
    return 0;
}
static void *notify(void *unused) {
    (void)unused;
    __edg_libcpp_mutex_lock(&mutex, 0);
    notified = 1;
    __edg_libcpp_condvar_signal(&condition, 1);
    __edg_libcpp_mutex_unlock(&mutex, 0);
    return 0;
}
static void *detached(void *unused) {
    (void)unused;
    __edg_libcpp_thread_sleep_for(1000000);
    __atomic_store_n(&detached_done, 1, __ATOMIC_RELEASE);
    return 0;
}
int thread_cases(void) {
    unsigned long handles[4] = {0};
    if (__edg_libcpp_tls_create(&key, destroy_key)) return 1;
    for (int i = 0; i < 4; ++i) if (__edg_libcpp_thread_create(&handles[i], worker, 0)) return 2;
    for (int i = 0; i < 4; ++i)
        if (!__edg_libcpp_thread_get_id(&handles[i]) || __edg_libcpp_thread_join(&handles[i]) || handles[i]) return 3;
    if (count != 4000 || once_calls != 1 || destructions != 4 || failures || source_tls) return 4;
    if (__edg_libcpp_mutex_lock(&mutex, 0) || __edg_libcpp_mutex_trylock(&mutex, 0)) return 5;
    if (__edg_libcpp_thread_create(&handles[0], notify, 0)) return 6;
    while (!notified) if (__edg_libcpp_condvar_wait(&condition, &mutex, 0)) return 7;
    struct timespec past = {0, 0};
    if (__edg_libcpp_condvar_wait(&condition, &mutex, &past) != ETIMEDOUT) return 8;
    if (__edg_libcpp_mutex_unlock(&mutex, 0) || __edg_libcpp_thread_join(&handles[0])) return 9;
    if (__edg_libcpp_condvar_destroy(&condition) || __edg_libcpp_mutex_destroy(&mutex, 0)) return 10;
    unsigned long recursive = 0;
    if (__edg_libcpp_recursive_mutex_init(&recursive) || __edg_libcpp_mutex_lock(&recursive, 1) ||
        !__edg_libcpp_mutex_trylock(&recursive, 1) || __edg_libcpp_mutex_unlock(&recursive, 1) ||
        __edg_libcpp_mutex_unlock(&recursive, 1) || __edg_libcpp_mutex_destroy(&recursive, 1)) return 11;
    if (__edg_libcpp_thread_create(&handles[0], detached, 0) || __edg_libcpp_thread_detach(&handles[0]) || handles[0]) return 12;
    while (!__atomic_load_n(&detached_done, __ATOMIC_ACQUIRE)) __edg_libcpp_thread_yield();
    return 0;
}
int thread_diagnostics(int unused) { (void)unused; return failures; }

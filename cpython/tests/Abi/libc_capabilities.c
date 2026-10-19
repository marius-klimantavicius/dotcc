/* Independent CPython host prerequisite probe; no copied interpreter layouts. */
#include <assert.h>
#include <pthread.h>
#include <stdatomic.h>
#include <stdint.h>
#include <stdio.h>
#include <wchar.h>

static pthread_mutex_t gate = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t condition = PTHREAD_COND_INITIALIZER;
static pthread_once_t once = PTHREAD_ONCE_INIT;
static pthread_key_t key;
static _Atomic int count;
static _Atomic int destructors;
static int ready;
static int go;
static int initialized;

static void initialize_once(void) { ++initialized; }
static void destroy_tls(void *value) {
    assert(value == (void *)(intptr_t)1 || value == (void *)(intptr_t)2);
    atomic_fetch_add(&destructors, 1);
}

static void *worker(void *value) {
    assert(pthread_setspecific(key, value) == 0);
    assert(pthread_once(&once, initialize_once) == 0);
    assert(pthread_mutex_lock(&gate) == 0);
    ++ready;
    assert(pthread_cond_broadcast(&condition) == 0);
    while (!go) assert(pthread_cond_wait(&condition, &gate) == 0);
    assert(pthread_mutex_unlock(&gate) == 0);
    for (int i = 0; i < 2000; ++i) atomic_fetch_add_explicit(&count, 1, memory_order_relaxed);
    assert(pthread_getspecific(key) == value);
    return value;
}

int main(void) {
    pthread_t first, second;
    void *one = 0, *two = 0;
    assert(sizeof(void *) == 8 && sizeof(long) == 8 && sizeof(int) == 4);
    assert(pthread_key_create(&key, destroy_tls) == 0);
    assert(pthread_setspecific(key, (void *)(intptr_t)99) == 0);
    assert(pthread_create(&first, 0, worker, (void *)(intptr_t)1) == 0);
    assert(pthread_create(&second, 0, worker, (void *)(intptr_t)2) == 0);
    assert(!pthread_equal(first, second));
    assert(!pthread_equal(first, pthread_self()));
    assert(pthread_mutex_lock(&gate) == 0);
    while (ready != 2) assert(pthread_cond_wait(&condition, &gate) == 0);
    go = 1;
    assert(pthread_cond_broadcast(&condition) == 0);
    assert(pthread_mutex_unlock(&gate) == 0);
    assert(pthread_join(first, &one) == 0 && one == (void *)(intptr_t)1);
    assert(pthread_join(second, &two) == 0 && two == (void *)(intptr_t)2);
    assert(initialized == 1 && atomic_load(&count) == 4000 && atomic_load(&destructors) == 2);
    assert(pthread_getspecific(key) == (void *)(intptr_t)99);
    int expected = 4000;
    assert(atomic_compare_exchange_strong(&count, &expected, 42));
    expected = 7;
    assert(!atomic_compare_exchange_strong(&count, &expected, 9) && expected == 42);
    assert(pthread_key_delete(key) == 0);
    assert(pthread_cond_destroy(&condition) == 0);
    assert(pthread_mutex_destroy(&gate) == 0);

    /* UTF-16 ABI check. Matching-native mode never calls glibc wide functions. */
#if defined(DOTCC_EXPECT_SHORT_WCHAR)
    assert(sizeof(wchar_t) == 2 && (wchar_t)-1 > 0);
    wchar_t text[] = { 0x03bb, 0xd83d, 0xdc0d, 0, 0x78 };
#if defined(DOTCC_NATIVE_SHORT_WCHAR)
    unsigned long units = 0;
    while (text[units] != 0) ++units;
    assert(units == 3);
#else
    assert(wcslen(text) == 3);
    wchar_t copied[5] = { 0, 0, 0, 0, 0 };
    assert(wmemcpy(copied, text, 5) == copied);
    assert(copied[3] == 0 && copied[4] == 0x78);
#endif
#endif
    puts("{\"threads\":2,\"increments\":4000,\"tls_destructors\":2,\"once\":1,\"cas\":42}");
    return 0;
}

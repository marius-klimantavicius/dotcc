#include <threads.h>
#include <errno.h>

extern int __cxa_thread_atexit(void (*)(void *), void *, void *);
static thread_local int source_value = 17;
static int trace;
static tss_t key;

static void cpp_destructor(void *unused) {
    (void)unused;
    if (source_value != 99 || errno != 41 || trace != 0) { trace = -1; return; }
    source_value = 88;
    trace = 1;
}

static void tss_destructor(void *unused) {
    (void)unused;
    if (source_value != 88 || errno != 41 || trace != 1) { trace = -2; return; }
    source_value = 77;
    trace = 2;
}

int host_setup(void) {
    if (source_value != 17) return 1;
    source_value = 99;
    errno = 41;
    if (tss_create(&key, tss_destructor) != thrd_success || tss_set(key, (void*)1) != thrd_success) return 2;
    return __cxa_thread_atexit(cpp_destructor, 0, 0);
}

int host_after_release(void) {
    if (trace != 2) return 3;
    if (source_value != 17 || errno != 0) return 4;
    if (tss_get(key) != 0) return 5;
    tss_delete(key);
    return 0;
}

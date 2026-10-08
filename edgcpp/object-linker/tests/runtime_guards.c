#include <threads.h>
#include <stdlib.h>

extern int __cxa_guard_acquire(unsigned long long *);
extern void __cxa_guard_release(unsigned long long *);
extern void __cxa_guard_abort(unsigned long long *);
static unsigned long long once_guard, retry_guard, outer_guard, inner_guard;
static int constructions, attempts, value, retry_value;

static int worker(void *unused) {
    (void)unused;
    for (int i = 0; i != 1000; ++i) {
        if (__cxa_guard_acquire(&once_guard)) {
            if (++constructions != 1) return 1;
            value = 12345;
            __cxa_guard_release(&once_guard);
        }
        if (value != 12345) return 2;
        if (__cxa_guard_acquire(&retry_guard)) {
            if (++attempts == 1) {
                retry_value = -1;
                __cxa_guard_abort(&retry_guard);
                continue;
            }
            retry_value = 67890;
            __cxa_guard_release(&retry_guard);
        }
        if (retry_value != 67890) return 3;
    }
    return 0;
}
static int inner(void *unused) {
    (void)unused;
    if (!__cxa_guard_acquire(&inner_guard)) return 1;
    __cxa_guard_release(&inner_guard);
    return 0;
}
int main(void) {
    thrd_t workers[8];
    for (int i = 0; i != 8; ++i) if (thrd_create(workers+i, worker, 0) != thrd_success) return 4;
    for (int i = 0; i != 8; ++i) {
        int result;
        if (thrd_join(workers[i], &result) != thrd_success || result) return 5;
    }
    if (constructions != 1 || attempts != 2 || value != 12345 || retry_value != 67890) return 6;
    if (__cxa_guard_acquire(&once_guard) || __cxa_guard_acquire(&retry_guard)) return 7;
    if (!__cxa_guard_acquire(&outer_guard)) return 8;
    thrd_t other; int result;
    if (thrd_create(&other, inner, 0) != thrd_success || thrd_join(other, &result) != thrd_success || result) return 9;
    __cxa_guard_release(&outer_guard);
    return 0;
}

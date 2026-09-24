#include <stdio.h>
#include <threads.h>

_Thread_local int thread_index = -1;
_Thread_local unsigned long mask = 1UL << 32;
static int step(void) { static _Thread_local int count = 7; return ++count; }
static int worker(void *arg) {
    if (thread_index != -1 || mask != (1UL << 32) || step() != 8) return -100;
    thread_index = *(int *)arg;
    return thread_index + step();
}
int main(void) {
    int first = 20, second = 30, a = 0, b = 0;
    thrd_t one, two;
    if (thrd_create(&one, worker, &first) || thrd_create(&two, worker, &second)) return 1;
    thrd_join(one, &a); thrd_join(two, &b);
    printf("%d %d %d %d\n", a, b, thread_index, step());
    return a == 29 && b == 39 && thread_index == -1 ? 0 : 2;
}

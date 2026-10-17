#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

extern "C" int thread_observe(int);
extern "C" void thread_change(int);
extern "C" int *thread_address(int);
static int before[4][8], after[4][8];
static int *addresses[4][6];
static pthread_barrier_t barrier;

static void capture(int index) {
    for (int i = 0; i < 8; ++i) before[index][i] = thread_observe(i);
    for (int i = 0; i < 6; ++i) addresses[index][i] = thread_address(i);
}
static void check_addresses(int index) {
    for (int i = 0; i < 6; ++i) if (addresses[index][i] != thread_address(i)) abort();
}
static void check_physical(int index, int salt) {
    if (*addresses[index][0] != salt || *addresses[index][1] != 7 + salt ||
        addresses[index][2][0] != 11 || addresses[index][2][1] != 13 + salt || addresses[index][2][2] != 17 ||
        *addresses[index][3] != 23 + salt || *addresses[index][4] != 31 + salt) abort();
}
static void *worker(void *argument) {
    int index = (int)(intptr_t)argument;
    capture(index);
    pthread_barrier_wait(&barrier);
    pthread_barrier_wait(&barrier);
    thread_change((index + 1) * 100);
    for (int i = 0; i < 8; ++i) after[index][i] = thread_observe(i);
    check_addresses(index);
    pthread_barrier_wait(&barrier);
    pthread_barrier_wait(&barrier);
    return NULL;
}
int main(void) {
    pthread_t threads[3];
    if (pthread_barrier_init(&barrier, NULL, 4)) abort();
    for (int i = 0; i < 3; ++i)
        if (pthread_create(&threads[i], NULL, worker, (void*)(intptr_t)i)) abort();
    pthread_barrier_wait(&barrier);
    capture(3);
    for (int thread = 0; thread < 4; ++thread) {
        if ((uintptr_t)addresses[thread][3] % 64) abort();
        for (int item = 0; item < 5; ++item)
            for (int other = 0; other < thread; ++other)
                if (addresses[thread][item] == addresses[other][item]) abort();
        if (addresses[thread][5] != addresses[0][5] || *addresses[thread][5] != 29) abort();
        check_physical(thread, 0);
    }
    pthread_barrier_wait(&barrier);
    pthread_barrier_wait(&barrier);
    thread_change(400);
    for (int i = 0; i < 8; ++i) after[3][i] = thread_observe(i);
    for (int thread = 0; thread < 4; ++thread) check_physical(thread, (thread + 1) * 100);
    check_addresses(3);
    pthread_barrier_wait(&barrier);
    for (int i = 0; i < 3; ++i) pthread_join(threads[i], NULL);
    pthread_barrier_destroy(&barrier);
    for (int thread = 0; thread < 4; ++thread) {
        printf("thread%d-before=", thread);
        for (int i = 0; i < 8; ++i) printf("%s%d", i ? "," : "", before[thread][i]);
        printf("\nthread%d-after=", thread);
        for (int i = 0; i < 8; ++i) printf("%s%d", i ? "," : "", after[thread][i]);
        puts("");
    }
    puts("thread-addresses=stable-aligned-distinct-shared-process");
    return 0;
}

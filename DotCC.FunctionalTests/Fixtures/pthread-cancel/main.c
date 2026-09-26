#include <pthread.h>
#include <stdio.h>

static pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t condition = PTHREAD_COND_INITIALIZER;
static int ready, order;

static void cleanup(void *arg) {
    int value = *(int *)arg;
    order = order * 10 + value;
    if (value == 1) pthread_mutex_unlock(&mutex);
}

static void *worker(void *arg) {
    int values[2] = {1, 2};
    pthread_mutex_lock(&mutex);
    pthread_cleanup_push(cleanup, &values[0]);
    pthread_cleanup_push(cleanup, &values[1]);
    ready = 1;
    pthread_cond_signal(&condition);
    while (1) pthread_cond_wait(&condition, &mutex);
    pthread_cleanup_pop(0);
    pthread_cleanup_pop(0);
    return 0;
}

int test(void) {
    pthread_t thread;
    int values[2] = {1, 2};
    void *result = 0;
    ready = order = 0;
    pthread_mutex_lock(&mutex);
    if (pthread_create(&thread, 0, worker, values)) return 1;
    while (!ready) pthread_cond_wait(&condition, &mutex);
    if (pthread_cancel(thread)) return 2;
    pthread_mutex_unlock(&mutex);
    if (pthread_join(thread, &result)) return 3;
    if (result != PTHREAD_CANCELED || order != 21) return 4;
    if (pthread_mutex_trylock(&mutex)) return 5;
    pthread_mutex_unlock(&mutex);
    return 0;
}

int main(void) {
    printf("cancellation=%d\n", test());
    return 0;
}

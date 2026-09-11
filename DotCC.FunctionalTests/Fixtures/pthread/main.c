#include <pthread.h>
#include <stdio.h>
#include <errno.h>

static pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t condition = PTHREAD_COND_INITIALIZER;
static pthread_once_t once = PTHREAD_ONCE_INIT;
static pthread_key_t key;
static int count, initialized, destroyed, ready;
static void initialize(void) { initialized++; }
static void destructor(void *value) {
    pthread_mutex_lock(&mutex);
    destroyed++;
    pthread_mutex_unlock(&mutex);
}
static void *worker(void *value) {
    pthread_once(&once, &initialize);
    pthread_setspecific(key, value);
    for (int i = 0; i < 100; i++) {
        pthread_mutex_lock(&mutex);
        count++;
        pthread_mutex_unlock(&mutex);
    }
    return pthread_getspecific(key);
}
static void *consumer(void *value) {
    pthread_mutex_lock(&mutex);
    while (!ready) pthread_cond_wait(&condition, &mutex);
    pthread_mutex_unlock(&mutex);
    pthread_exit(value);
    return NULL;
}
int main(void) {
    pthread_t threads[4];
    int values[4] = { 1, 2, 3, 4 };
    pthread_key_create(&key, &destructor);
    for (int i = 0; i < 4; i++) pthread_create(&threads[i], NULL, &worker, &values[i]);
    int sum = 0;
    for (int i = 0; i < 4; i++) {
        void *result;
        pthread_join(threads[i], &result);
        sum += *(int *)result;
    }
    printf("count=%d once=%d sum=%d destructors=%d\n", count, initialized, sum, destroyed);
    pthread_key_delete(key);
    pthread_t thread;
    pthread_create(&thread, NULL, &consumer, &values[2]);
    pthread_mutex_lock(&mutex);
    ready = 1;
    pthread_cond_signal(&condition);
    pthread_mutex_unlock(&mutex);
    void *result;
    pthread_join(thread, &result);
    printf("condition=%d self=%d\n", *(int *)result, pthread_equal(pthread_self(), pthread_self()));
    struct timespec expired;
    expired.tv_sec = 0;
    expired.tv_nsec = 0;
    pthread_mutex_lock(&mutex);
    int timedout = pthread_cond_timedwait(&condition, &mutex, &expired);
    pthread_mutex_unlock(&mutex);
    printf("timeout=%d\n", timedout == ETIMEDOUT);
    pthread_cond_destroy(&condition);
    pthread_mutex_destroy(&mutex);
    pthread_mutexattr_t attr;
    pthread_mutexattr_init(&attr);
    pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
    pthread_mutex_init(&mutex, &attr);
    pthread_mutex_lock(&mutex);
    int recursive = pthread_mutex_trylock(&mutex);
    pthread_mutex_unlock(&mutex);
    pthread_mutex_unlock(&mutex);
    printf("recursive=%d\n", recursive == 0);
    pthread_mutex_destroy(&mutex);
    pthread_mutexattr_destroy(&attr);
    return 0;
}

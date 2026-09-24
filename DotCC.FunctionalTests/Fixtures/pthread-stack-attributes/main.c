#include <pthread.h>
#include <stdint.h>
#include <stdio.h>
#include <errno.h>
static void *worker(void *value) { return value; }
int main(void) {
    pthread_attr_t attr;
    size_t size;
    int detached;
    if (pthread_attr_init(&attr) || pthread_attr_getstacksize(&attr, &size)) return 1;
    if (!size || pthread_attr_setstacksize(&attr, 4 * 1024 * 1024)) return 2;
    if (pthread_attr_getstacksize(&attr, &size) || size != 4 * 1024 * 1024) return 3;
    if (pthread_attr_setstacksize(&attr, 1) != EINVAL) return 4;
    if (pthread_attr_getstacksize(&attr, &size) || size != 4 * 1024 * 1024) return 5;
    if (pthread_attr_getdetachstate(&attr, &detached) || detached != PTHREAD_CREATE_JOINABLE) return 6;
    pthread_t thread;
    void *value;
    if (pthread_create(&thread, &attr, worker, (void *)(intptr_t)73)) return 7;
    if (pthread_attr_destroy(&attr) || pthread_join(thread, &value) || (intptr_t)value != 73) return 8;
    puts("stack attributes and worker passed");
    return 0;
}

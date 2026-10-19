#ifdef TARGET_ABI_ONLY
#include <pthread.h>
_Static_assert(sizeof(pthread_t) == 8, "target pthread_t");
_Static_assert(sizeof(pthread_attr_t) == 4, "target logical attr handle");
_Static_assert(sizeof(pthread_key_t) == 4, "target TLS key");
_Static_assert(sizeof(pthread_mutex_t) == 4, "target mutex handle");
_Static_assert(sizeof(pthread_cond_t) == 4, "target condition handle");
_Static_assert(PTHREAD_STACK_MIN == 16384, "target stack validation minimum");
int target_abi_control;
#else
#define _GNU_SOURCE
#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <stdint.h>
#define CHECK(c) do { if (!(c)) { fprintf(stderr,"native attr check failed line %d\n",__LINE__); exit(2); } } while(0)
static size_t observed;
static void *worker(void *argument) {
    pthread_attr_t current;
    void *address;
    CHECK(pthread_getattr_np(pthread_self(), &current) == 0);
    CHECK(pthread_attr_getstack(&current, &address, &observed) == 0);
    CHECK(address != NULL);
    CHECK(pthread_attr_destroy(&current) == 0);
    return argument;
}
int main(void) {
    pthread_attr_t attr;
    size_t value, before;
    int detach;
    CHECK(pthread_attr_init(&attr) == 0);
    CHECK(pthread_attr_getstacksize(&attr, &before) == 0);
    CHECK(pthread_attr_setstacksize(&attr, 0) == EINVAL);
    CHECK(pthread_attr_getstacksize(&attr, &value) == 0 && value == before);
    CHECK(pthread_attr_setstacksize(&attr, PTHREAD_STACK_MIN - 1) == EINVAL);
    CHECK(pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED) == 0);
    CHECK(pthread_attr_getdetachstate(&attr, &detach) == 0 && detach == PTHREAD_CREATE_DETACHED);
    CHECK(pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_JOINABLE) == 0);
    const size_t sizes[] = {0, 1024*1024, 65*1024*1024};
    for (size_t i=0; i<sizeof sizes/sizeof sizes[0]; ++i) {
        pthread_t thread;
        void *result;
        if (sizes[i]) {
            CHECK(pthread_attr_setstacksize(&attr, sizes[i]) == 0);
            CHECK(pthread_attr_getstacksize(&attr, &value) == 0 && value == sizes[i]);
        }
        CHECK(pthread_create(&thread, sizes[i] ? &attr : NULL, worker, (void *)(uintptr_t)42) == 0);
        CHECK(pthread_join(thread, &result) == 0 && result == (void *)(uintptr_t)42);
        CHECK(observed >= (sizes[i] ? sizes[i] : PTHREAD_STACK_MIN));
        printf("native logical=%zu observed=%zu\n",sizes[i],observed);
    }
    CHECK(pthread_attr_destroy(&attr) == 0);
    printf("PASS native host attr=%zu; dotcc logical ABI measured separately\n",sizeof attr);
    return 0;
}
#endif

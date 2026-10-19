#include <assert.h>
#include <stdatomic.h>
#include <pthread.h>
#include <stdio.h>
struct payload { int index; int checksum; };
static struct payload storage;
static _Atomic(struct payload *) published;
static void *writer(void *unused) {
    for(int i=1;i<=200;++i) {
        while(atomic_load_explicit(&published,memory_order_acquire)!=0) { }
        storage.index=i; storage.checksum=i*7;
        atomic_store_explicit(&published,&storage,memory_order_release);
    }
    return 0;
}
static void *reader(void *unused) {
    for(int i=1;i<=200;++i) {
        struct payload *current;
        do { current=atomic_load_explicit(&published,memory_order_acquire); } while(current==0);
        assert(current->index==i && current->checksum==i*7);
        atomic_store_explicit(&published,0,memory_order_release);
    }
    return 0;
}
int main(void) {
    pthread_t first,second;
    atomic_init(&published,0);
    assert(pthread_create(&first,0,writer,0)==0);
    assert(pthread_create(&second,0,reader,0)==0);
    assert(pthread_join(first,0)==0 && pthread_join(second,0)==0);
    assert(atomic_load(&published)==0);
    puts("200 pointer publications passed");
    return 0;
}

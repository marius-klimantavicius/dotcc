#include <assert.h>
#include <stdatomic.h>
#include <stdio.h>
static int locations, expectations, values, orders;
static _Atomic(void *) *location(_Atomic(void *) *p) { ++locations; return p; }
static void **expectation(void **p) { ++expectations; return p; }
static void *value(void *p) { ++values; return p; }
static memory_order order(memory_order n) { ++orders; return n; }
static int plus(int n) { return n+1; }
int main(void) {
    int one=1,two=2,three=3;
    _Atomic(void *) pointer;
    atomic_init(&pointer,&one);
    assert(atomic_is_lock_free(&pointer));
    void *expected=&one;
    assert(atomic_compare_exchange_strong(location(&pointer),expectation(&expected),value(&two)));
    assert(atomic_load(&pointer)==&two && expected==&one);
    expected=&one;
    assert(!atomic_compare_exchange_weak_explicit(location(&pointer),expectation(&expected),value(&three),
        order(memory_order_acq_rel),order(memory_order_acquire)));
    assert(atomic_load_explicit(&pointer,order(memory_order_acquire))==&two && expected==&two);
    assert(atomic_exchange_explicit(location(&pointer),value(&three),order(memory_order_release))==&two);
    atomic_store_explicit(location(&pointer),value(&one),order(memory_order_release));
    assert(atomic_exchange(&pointer,&two)==&one);
    atomic_store(&pointer,&three);
    assert(atomic_load(&pointer)==&three);
    assert(locations==4 && expectations==2 && values==4 && orders==5);
    int *typed_expected=&one;
    _Atomic(int *) typed;
    atomic_init(&typed,&one);
    assert(atomic_compare_exchange_strong(&typed,&typed_expected,&two));
    assert(atomic_exchange(&typed,&three)==&two);
    assert(atomic_load(&typed)==&three);
    typedef int (*callback)(int);
    _Atomic(callback) function;
    atomic_init(&function,plus);
    callback saved=atomic_load(&function);
    assert(saved(4)==5);
    printf("%d %d %d %d %d\n",locations,expectations,values,orders,saved(4));
    return 0;
}

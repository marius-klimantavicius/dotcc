/* Reduced from actual pyatomic_std.h pointer exchange/CAS, independently of
   the interpreter, with GNU controls. Correct lowering uses pointer-width
   integer atomics: a C# pointer cannot be an Atomic<T> type argument. */
#include <assert.h>
#include <stdatomic.h>

static int cp_b_compare(void *object, void *expected, void *desired) {
    return atomic_compare_exchange_strong((_Atomic(void *)*)object, (void **)expected, desired);
}

static void *cp_b_exchange(void *object, void *value) {
    return atomic_exchange((_Atomic(void *)*)object, value);
}

int main(void) {
    int one = 1, two = 2, three = 3;
    void *value = &one;
    void *expected = &one;
    assert(__atomic_compare_exchange_n(&value, &expected, &two, 0, __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST));
    assert(value == &two && expected == &one);
    expected = &one;
    assert(!__atomic_compare_exchange_n(&value, &expected, &three, 0, __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST));
    assert(value == &two && expected == &two);
    assert(__atomic_exchange_n(&value, &three, __ATOMIC_SEQ_CST) == &two);
    assert(value == &three);
    _Atomic(void *) c11_value = &one;
    expected = &one;
    assert(cp_b_compare(&c11_value, &expected, &two));
    assert(atomic_load(&c11_value) == &two && expected == &one);
    expected = &one;
    assert(!cp_b_compare(&c11_value, &expected, &three));
    assert(expected == &two);
    assert(cp_b_exchange(&c11_value, &three) == &two);
    assert(atomic_load(&c11_value) == &three);
    return 0;
}

/* Reduced independently from the actual cpython/pyhash.h parse failure.
   This is a C qualifier regression, not a substitute CPython object layout. */
#include <assert.h>
struct cp_b_const_slot {
    long (*const invoke)(const void *, long);
    const int tag;
};
static long add(const void *context, long value) { return *(const long *)context + value; }
int main(void) {
    long base = 40;
    struct cp_b_const_slot slot = { add, 7 };
    assert(slot.tag == 7 && slot.invoke(&base, 2) == 42);
    return 0;
}

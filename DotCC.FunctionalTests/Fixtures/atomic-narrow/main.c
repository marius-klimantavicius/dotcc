#include <stdatomic.h>
#include <stdio.h>
#include <string.h>

#define CHECK_TYPE(type, label) do { \
    struct { unsigned char before; _Atomic type value; unsigned char after; } guarded; \
    memset(&guarded, 0xa5, sizeof guarded); \
    atomic_init(&guarded.value, 7); \
    int initial = atomic_load(&guarded.value); \
    atomic_store_explicit(&guarded.value, 11, memory_order_seq_cst); \
    int stored = atomic_load_explicit(&guarded.value, memory_order_seq_cst); \
    int exchanged = atomic_exchange(&guarded.value, 13); \
    type expected = 12; \
    int failed = atomic_compare_exchange_strong(&guarded.value, &expected, 17); \
    int observed = expected; \
    int changed = atomic_compare_exchange_strong(&guarded.value, &expected, 17); \
    int added = atomic_fetch_add(&guarded.value, 3); \
    int subtracted = atomic_fetch_sub(&guarded.value, 2); \
    int ored = atomic_fetch_or(&guarded.value, 1); \
    int xored = atomic_fetch_xor(&guarded.value, 3); \
    int anded = atomic_fetch_and(&guarded.value, 7); \
    atomic_store(&guarded.value, atomic_load(&guarded.value) + 1); \
    printf("%s lock=%d values=%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d guards=%d\n", \
        label, atomic_is_lock_free(&guarded.value), initial, stored, exchanged, \
        failed, observed, changed, added, subtracted, ored, xored, anded, \
        guarded.before == 0xa5 && guarded.after == 0xa5 && atomic_load(&guarded.value) == 1); \
} while (0)

int main(void)
{
    CHECK_TYPE(signed char, "i8");
    CHECK_TYPE(unsigned char, "u8");
    CHECK_TYPE(short, "i16");
    CHECK_TYPE(unsigned short, "u16");
    return 0;
}

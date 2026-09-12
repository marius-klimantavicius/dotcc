#include <stdio.h>
#include <stdint.h>
struct Narrow { unsigned char before; unsigned char value; unsigned char after; short count; };
static int order_calls;
static int next_order(void) { ++order_calls; return __ATOMIC_RELAXED; }
int main(void) {
    struct Narrow n = {0x55, 254, 0xaa, 32766};
    unsigned char old_byte = __sync_fetch_and_add(&n.value, 3);
    short new_short = __sync_add_and_fetch(&n.count, (short)1);
    short old_short = __sync_val_compare_and_swap(&n.count, (short)32767, (short)-12);
    printf("%u %u %u %u %d %d %d\n", n.before, old_byte, n.value, n.after, new_short, old_short, n.count);
    long value = 7;
    long old = __sync_fetch_and_add(&value, 5);
    long latest = __sync_sub_and_fetch(&value, 2);
    long failed = __sync_val_compare_and_swap(&value, 8, 90);
    long exchanged = __sync_val_compare_and_swap(&value, 10, 19);
    printf("%ld %ld %ld %ld %ld\n", old, latest, failed, exchanged, value);
    long expected = 0;
    int match = __atomic_compare_exchange_n(&value, &expected, 20, 0, __ATOMIC_SEQ_CST, __ATOMIC_RELAXED);
    printf("%d %ld %ld\n", match, expected, value);
    match = __atomic_compare_exchange_n(&value, &expected, 20, 1, __ATOMIC_ACQ_REL, __ATOMIC_ACQUIRE);
    long loaded = __atomic_load_n(&value, next_order());
    long added = __atomic_add_fetch(&value, 3, __ATOMIC_SEQ_CST);
    long subtracted = __atomic_sub_fetch(&value, 2, __ATOMIC_RELAXED);
    printf("%d %ld %ld %ld %ld %d\n", match, expected, loaded, added, subtracted, order_calls);
    unsigned int bits = 0xf0;
    unsigned int anded = __sync_and_and_fetch(&bits, 0x3f);
    unsigned int ored = __sync_or_and_fetch(&bits, 3);
    unsigned int prior = __sync_fetch_and_or(&bits, 4);
    printf("%u %u %u %u\n", anded, ored, prior, bits);
    unsigned char flag = 1;
    int flag_match = __sync_bool_compare_and_swap(&flag, 1, 3);
    __sync_lock_release(&bits);
    __sync_synchronize();
    __atomic_store_n(&bits, 14, __ATOMIC_RELEASE);
    unsigned int before_exchange = __atomic_exchange_n(&bits, 21, __ATOMIC_ACQ_REL);
    _Bool boolean = 0;
    __atomic_store_n(&boolean, 1, __ATOMIC_RELEASE);
    int read_boolean = __atomic_load_n(&boolean, __ATOMIC_ACQUIRE);
    printf("%d %u %u %u %d\n", flag_match, flag, before_exchange, bits, read_boolean);
    int array[2] = {7, 9};
    int index = 0;
    int prior_element = __sync_fetch_and_add(&array[index++], 3);
    void *pointer = &array[0];
    void *previous = __sync_lock_test_and_set(&pointer, &array[1]);
    int changed = pointer == &array[1] && previous == &array[0];
    void *cleared = __sync_fetch_and_and(&pointer, 0);
    printf("%d %d %d %d %d %d\n", index, prior_element, array[0], changed, cleared == &array[1], pointer == 0);
    pointer = &array[0];
    void *read_pointer = __atomic_load_n(&pointer, __ATOMIC_RELAXED);
    printf("%d %x %x %llx\n", read_pointer == &array[0], __builtin_bswap16(0x1234),
        __builtin_bswap32(0x12345678u), (unsigned long long)__builtin_bswap64(0x0123456789abcdefULL));
    return 0;
}

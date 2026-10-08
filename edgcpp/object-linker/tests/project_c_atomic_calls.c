typedef int (*callback)(int);
static int plus_one(int value) { return value + 1; }
static int plus_two(int value) { return value + 2; }
static callback target;
int main(void) {
    callback initial = plus_one, observed = 0;
    __atomic_store(&target, &initial, __ATOMIC_RELEASE);
    __atomic_load(&target, &observed, __ATOMIC_ACQUIRE);
    if (observed(7) != 8) return 1;
    callback expected = plus_one, replacement = plus_two;
    int weak = 256;
    if (!__atomic_compare_exchange(&target, &expected, &replacement, weak,
                                  __ATOMIC_ACQ_REL, __ATOMIC_ACQUIRE)) {
        expected = plus_one;
        if (!__atomic_compare_exchange(&target, &expected, &replacement, 0,
                                      __ATOMIC_ACQ_REL, __ATOMIC_ACQUIRE)) return 2;
    }
    __atomic_exchange(&target, &initial, &observed, __ATOMIC_ACQ_REL);
    if (observed(7) != 9 || target(7) != 8) return 3;
    return 0;
}

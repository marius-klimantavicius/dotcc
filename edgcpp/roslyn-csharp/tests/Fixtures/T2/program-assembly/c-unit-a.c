#include "c-api.h"
static int local_value = 3;
static int local_helper(int value) { return value + local_value; }
static int *address_from_a = &shared_value;
static program_callback callback_from_a = &shared_add;

int program_observation_count(void) { return 10; }
int program_observe(int selector) {
    if (selector == 0) return local_value;
    if (selector == 1) return local_helper(5);
    if (selector == 2) return unit_b_observe(0);
    if (selector == 3) return unit_b_observe(1);
    if (selector == 4) return shared_value;
    if (selector == 5) return opaque_read(shared_pointer);
    if (selector == 6) return &shared_value == shared_address_from_b();
    if (selector == 7) return &shared_add == shared_callback_from_b();
    if (selector == 8) return address_from_a == shared_address_from_b();
    if (selector == 9) return callback_from_a == shared_callback_from_b();
    return -1;
}

int fixture(void) {
    shared_value = 11;
    int total = local_helper(shared_add(shared_value)) + unit_b_observe(2);
    shared_value += 5;
    total += unit_b_increment();
    total += 100 * (&shared_value == shared_address_from_b());
    total += 1000 * (&shared_add == shared_callback_from_b());
    return total;
}

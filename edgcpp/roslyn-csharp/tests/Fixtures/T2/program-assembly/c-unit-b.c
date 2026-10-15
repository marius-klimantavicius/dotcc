#include "c-api.h"
struct Opaque { int value; };
static struct Opaque opaque_storage = {9};
struct Opaque *shared_pointer = &opaque_storage;
int shared_value = 11;
static int local_value = 7;
static int local_helper(int value) { return value + local_value; }
static int *address_from_b = &shared_value;
static program_callback callback_from_b = &shared_add;

/* The declaration's top-level parameter qualifier differs without changing ABI. */
int shared_add(const int value) { return value + 2; }
int opaque_read(struct Opaque *value) { return value->value; }
int *shared_address_from_b(void) { return address_from_b; }
program_callback shared_callback_from_b(void) { return callback_from_b; }
int unit_b_increment(void) { shared_value += 2; return shared_value; }
int unit_b_observe(int selector) {
    if (selector == 0) return local_value;
    if (selector == 1) return local_helper(5);
    if (selector == 2) return local_helper(shared_pointer->value);
    return -1;
}

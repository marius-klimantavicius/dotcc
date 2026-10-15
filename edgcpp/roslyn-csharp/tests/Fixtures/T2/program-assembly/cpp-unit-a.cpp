#include "cpp-api.h"
namespace captured_program {
int choose(const int value) { return value + 11; }
unsigned choose(unsigned value) { return value + 17u; }
}
static int local_value = 4;
static int local_helper(int value) { return value * 3 + local_value; }
extern "C" {
int cpp_unit_a(int value) {
    ++captured_program::shared_value;
    return captured_program::choose(value) + (int)captured_program::choose((unsigned)value) + local_helper(value);
}
int cpp_unit_a_observe(int selector) { return selector == 0 ? local_value : local_helper(4); }
int *cpp_shared_address_from_a(void) { return &captured_program::shared_value; }
int (*cpp_shared_callback_from_a(void))(int) { return &captured_program::choose; }
}

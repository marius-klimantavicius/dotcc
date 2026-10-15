#include "cpp-api.h"
namespace captured_program { int shared_value = 5; }
static int local_value = 7;
static int local_helper(int value) { return value + local_value; }
extern "C" {
int program_observation_count(void) { return 8; }
int program_observe(int selector) {
    if (selector == 0) return cpp_unit_a_observe(0);
    if (selector == 1) return cpp_unit_a_observe(1);
    if (selector == 2) return local_value;
    if (selector == 3) return local_helper(3);
    if (selector == 4) return captured_program::shared_value;
    if (selector == 5) return &captured_program::shared_value == cpp_shared_address_from_a();
    int (*callback)(int) = &captured_program::choose;
    if (selector == 6) return callback == cpp_shared_callback_from_a();
    if (selector == 7) return cpp_shared_callback_from_a()(5);
    return -1;
}
int fixture(void) {
    captured_program::shared_value = 5;
    int first = cpp_unit_a(4);
    int result = first + captured_program::choose(7)
        + (int)captured_program::choose(9u) + local_helper(3) + captured_program::shared_value;
    result += 100 * (&captured_program::shared_value == cpp_shared_address_from_a());
    int (*callback)(int) = &captured_program::choose;
    result += 1000 * (callback == cpp_shared_callback_from_a());
    return result;
}
}

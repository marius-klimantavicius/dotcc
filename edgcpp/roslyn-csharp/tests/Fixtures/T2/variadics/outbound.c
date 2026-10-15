/* Managed application fixture. Native helpers only observe supplied ABI values. */
#include "outbound.h"

static int observations(void) {
    int result = 0;
    for (int index = 0; index < 10; ++index) {
        result += native_captured_integer(index);
        result += (int)(native_captured_double(index) * 2.0);
    }
    return result;
}

int fixture(void) {
    fixture_outbound_callback supplied = native_outbound_provider();
    fixture_outbound_callback echoed = native_outbound_echo(supplied);
    int first = echoed(7, 1, 0.5, 2, 1.5, 3, 2.5, 4, 3.5, 5, 4.5,
                       6, 5.5, 7, 6.5, 8, 7.5, 9, 8.5, 10, 9.5);
    int first_observations = observations();
    int second = native_capture_variadic(9, 11, 10.5, 12, 11.5, 13, 12.5,
        14, 13.5, 15, 14.5, 16, 15.5, 17, 16.5, 18, 17.5, 19, 18.5, 20, 19.5);
    int second_observations = observations();
    return first + first_observations + second + second_observations + (supplied == echoed);
}

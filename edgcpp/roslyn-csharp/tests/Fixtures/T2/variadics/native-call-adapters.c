/* Narrow ABI test adapters only: invoke the supplied callback with fixed test
   values. This file contains no translated application's implementation. */
#include "addressed.h"

fixture_variadic_callback native_echo_variadic(fixture_variadic_callback callback) {
    return callback;
}

int native_call_integer_variadic(fixture_variadic_callback callback) {
    return callback(0, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
}

int native_call_double_variadic(fixture_variadic_callback callback) {
    return callback(1, 10, 1.5, 2.5, 3.5, 4.5, 5.5,
                          6.5, 7.5, 8.5, 9.5, 10.5);
}

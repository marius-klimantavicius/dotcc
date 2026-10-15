/* Application fixture. No native entry adapter implementation is exported. */
#include "addressed.h"

int addressed_consume(int kind, int count, fixture_va_list *cursor) {
    if (kind == 0) {
        int total = 0;
        for (int index = 0; index < count; ++index) total += fixture_va_arg(*cursor, int);
        return total;
    }
    double total = 0.0;
    for (int index = 0; index < count; ++index) total += fixture_va_arg(*cursor, double);
    return (int)(total * 2.0);
}

int addressed_sum(int kind, int count, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, count);
    int result = addressed_consume(kind, count, &cursor);
    fixture_va_end(cursor);
    return result;
}

struct VariadicTable {
    fixture_variadic_callback invoke;
};

static struct VariadicTable table = {addressed_sum};

int fixture(void) {
    fixture_variadic_callback echoed = native_echo_variadic(table.invoke);
    fixture_erased_variadic erased = (fixture_erased_variadic)table.invoke;
    fixture_variadic_callback restored = (fixture_variadic_callback)erased;
    int direct = addressed_sum(0, 3, 4, 5, 6);
    int internal = table.invoke(0, 3, 7, 8, 9);
    int native_integer = native_call_integer_variadic(echoed);
    int native_double = native_call_double_variadic(echoed);
    return direct + internal + native_integer + native_double
        + (echoed == addressed_sum) + (restored == table.invoke);
}

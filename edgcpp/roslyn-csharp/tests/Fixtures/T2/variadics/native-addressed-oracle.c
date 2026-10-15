/* Native reference process only. oracle_fixed_bridge calls the independently
   compiled source body as its oracle; this file must NEVER enter a generated
   application's dependency closure. */
#include <stdio.h>
#include "addressed.h"

static int oracle_fixed_bridge(int kind, int count, fixture_va_list *cursor) {
    return addressed_consume(kind, count, cursor);
}

int main(void) {
    printf("result=%d\n", fixture());
    native_set_variadic_bridge(oracle_fixed_bridge);
    fixture_variadic_callback entry = native_variadic_entry;
    fixture_variadic_callback echoed = native_echo_variadic(entry);
    printf("entry-integer=%d\n", native_call_integer_variadic(echoed));
    printf("entry-double=%d\n", native_call_double_variadic(echoed));
    printf("entry-identity=%d\n", entry == echoed);
    return 0;
}

/* Candidate ABI-only host entry adapter. The compiler captures the incoming
   variadic ABI. All application execution occurs in the registered fixed bridge.
   A generated workload may link this file, never addressed.c or an oracle. */
#include "addressed.h"

static fixture_variadic_bridge registered_bridge;

void native_set_variadic_bridge(fixture_variadic_bridge bridge) {
    registered_bridge = bridge;
}

int native_variadic_entry(int kind, int count, ...) {
    if (!registered_bridge) __builtin_trap();
    fixture_va_list cursor;
    fixture_va_start(cursor, count);
    int result = registered_bridge(kind, count, &cursor);
    fixture_va_end(cursor);
    return result;
}

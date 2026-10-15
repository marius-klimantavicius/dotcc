#ifndef ROSLYN_CSHARP_T2_ADDRESSED_H
#define ROSLYN_CSHARP_T2_ADDRESSED_H

#include "fixture-va.h"
typedef int (*fixture_variadic_callback)(int kind, int count, ...);
typedef int (*fixture_variadic_bridge)(int kind, int count, fixture_va_list *cursor);
typedef void (*fixture_erased_variadic)(void);

int addressed_consume(int kind, int count, fixture_va_list *cursor);
int addressed_sum(int kind, int count, ...);
int fixture(void);

/* Narrow host ABI test calls and identity echo. */
fixture_variadic_callback native_echo_variadic(fixture_variadic_callback callback);
int native_call_integer_variadic(fixture_variadic_callback callback);
int native_call_double_variadic(fixture_variadic_callback callback);

/* Candidate ABI-only entry adapter; bridge registration is mandatory. */
void native_set_variadic_bridge(fixture_variadic_bridge bridge);
int native_variadic_entry(int kind, int count, ...);

#endif

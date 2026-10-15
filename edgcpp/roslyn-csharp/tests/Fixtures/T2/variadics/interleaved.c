/* Exhaust GP and SSE independently, then occupy all registers in the fixed
   prefix and spill named arguments before the interleaved variadic tail. */
#include "fixture-va.h"

static int alternating(int marker, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, marker);
    int result = marker;
    for (int index = 0; index < 10; ++index) {
        result += fixture_va_arg(cursor, int);
        result += (int)(fixture_va_arg(cursor, double) * 2.0);
    }
    fixture_va_end(cursor);
    return result;
}

static int occupied(int a0, int a1, int a2, int a3, int a4, int a5,
                    int spilled_named, double f0, double f1, double f2,
                    double f3, double f4, double f5, double f6, double f7,
                    int count, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, count);
    int result = a0 + a1 + a2 + a3 + a4 + a5 + spilled_named + count
        + (int)((f0 + f1 + f2 + f3 + f4 + f5 + f6 + f7) * 2.0);
    for (int index = 0; index < count; ++index) {
        result += fixture_va_arg(cursor, int);
        result += (int)(fixture_va_arg(cursor, double) * 4.0);
    }
    fixture_va_end(cursor);
    return result;
}

static int discarded_pointer(int marker, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, marker);
    fixture_va_arg(cursor, void *);
    int result = fixture_va_arg(cursor, int);
    fixture_va_end(cursor);
    return result;
}

int fixture(void) {
    int alternating_result = alternating(7, 1, 0.5, 2, 1.5, 3, 2.5,
        4, 3.5, 5, 4.5, 6, 5.5, 7, 6.5, 8, 7.5, 9, 8.5, 10, 9.5);
    int occupied_result = occupied(1, 2, 3, 4, 5, 6, 7,
        0.5, 1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 2, 11, 1.25, 13, 2.5);
    int discarded_result = discarded_pointer(1, (void *)0, 19);
    return alternating_result + occupied_result + discarded_result;
}

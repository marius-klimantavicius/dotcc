/* Application-only internal varargs, explicit cursor pointers and va_copy. */
#include "fixture-va.h"

static int sentinel;

static int consume_cursor(int count, fixture_va_list *cursor) {
    int total = 0;
    for (int index = 0; index < count; ++index) total += fixture_va_arg(*cursor, int);
    return total;
}

static int consume_copy(int count, fixture_va_list *cursor) {
    fixture_va_list copy;
    fixture_va_copy(copy, *cursor);
    int total = consume_cursor(count, &copy);
    fixture_va_end(copy);
    return total;
}

static int sum_integers(int count, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, count);
    int result = consume_cursor(count, &cursor);
    fixture_va_end(cursor);
    return result;
}

static int copy_and_forward(int count, ...) {
    fixture_va_list original;
    fixture_va_list copy;
    fixture_va_start(original, count);
    fixture_va_copy(copy, original);
    int copied = consume_cursor(count, &copy);
    int first = fixture_va_arg(original, int);
    int snapshot = consume_copy(count - 1, &original);
    int second = fixture_va_arg(original, int);
    int remainder = consume_cursor(count - 2, &original);
    fixture_va_end(copy);
    fixture_va_end(original);
    return copied + first + snapshot + second + remainder;
}

static int sum_doubles(double seed, int count, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, count);
    double total = seed;
    for (int index = 0; index < count; ++index) total += fixture_va_arg(cursor, double);
    fixture_va_end(cursor);
    return (int)(total * 2.0);
}

static int mixed_arguments(int bias, ...) {
    fixture_va_list cursor;
    fixture_va_start(cursor, bias);
    int narrow_unsigned = fixture_va_arg(cursor, int);
    double promoted_float = fixture_va_arg(cursor, double);
    void *pointer = fixture_va_arg(cursor, void *);
    int narrow_signed = fixture_va_arg(cursor, int);
    double fraction = fixture_va_arg(cursor, double);
    long long wide = fixture_va_arg(cursor, long long);
    fixture_va_end(cursor);
    return bias + narrow_unsigned + (int)(promoted_float * 4.0)
        + (pointer == &sentinel ? 100 : 0) + narrow_signed
        + (int)(fraction * 4.0) + (int)wide;
}

int fixture(void) {
    int integers = sum_integers(10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    int forwarded = copy_and_forward(8, 1, 2, 3, 4, 5, 6, 7, 8);
    int floating = sum_doubles(0.5, 10, 1.0, 2.0, 3.0, 4.0, 5.0,
                              6.0, 7.0, 8.0, 9.0, 10.0);
    int mixed = mixed_arguments(7, (unsigned char)250, (float)1.5,
                                (void *)&sentinel, (short)-9, 2.25, 1234LL);
    return integers + forwarded + floating + mixed;
}

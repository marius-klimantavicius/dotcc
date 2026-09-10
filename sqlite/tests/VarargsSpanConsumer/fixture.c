#include <stdarg.h>

static int sum_list(int count, va_list arguments) {
    int result = 0;
    for (int i = 0; i < count; i++) result += va_arg(arguments, int);
    return result;
}

int span_sum(int count, ...) {
    va_list arguments;
    va_start(arguments, count);
    int result = sum_list(count, arguments);
    va_end(arguments);
    return result;
}

int span_copy(int count, ...) {
    va_list arguments, copy;
    va_start(arguments, count);
    va_copy(copy, arguments);
    int first = sum_list(count, arguments);
    int second = sum_list(count, copy);
    va_end(copy);
    va_end(arguments);
    return first + second;
}

int span_c_empty(void) { return span_sum(0); }
int span_c_one(void) { return span_sum(1, 7); }
int span_c_eight(void) { return span_sum(8, 1, 2, 3, 4, 5, 6, 7, 8); }
int span_c_many(void) {
    return span_sum(64,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
        17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
        33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
        49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64);
}

typedef int (*callback)(int);
static int increment(int value) { return value + 1; }
static int inspect_promotions(int unused, ...) {
    va_list arguments;
    va_start(arguments, unused);
    int a = va_arg(arguments, int);
    int b = va_arg(arguments, int);
    int c = va_arg(arguments, int);
    double d = va_arg(arguments, double);
    int *pointer = va_arg(arguments, int*);
    callback function = va_arg(arguments, callback);
    int result = a + b + c + (int)(d * 10) + *pointer + function(*pointer);
    va_end(arguments);
    return result;
}
int span_c_promotions(void) {
    unsigned char byte_value = 255;
    short short_value = -7;
    unsigned short unsigned_value = 65535;
    float float_value = 1.5f;
    int integer = 42;
    return inspect_promotions(0, byte_value, short_value, unsigned_value,
                              float_value, &integer, increment);
}

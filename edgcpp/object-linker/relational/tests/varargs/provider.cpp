#include "program.h"
int next(va_list arguments) { return va_arg(arguments, int); }
int scan(int count, ...) {
    va_list first, copy;
    va_start(first, count);
    va_copy(copy, first);
    int result = 0;
    if (next(first) != 7 || next(copy) != 7) result = 1;
    if (next(first) != 9 || next(copy) != 9) result = 2;
    if (va_arg(first, double) != 2.5) result = 3;
    int *pointer = va_arg(first, int*);
    if (*pointer != 42) result = 4;
    if (va_arg(first, int) != 1) result = 5;
    if (count != 5) result = 6;
    va_end(first);
    va_end(copy);
    return result;
}
int empty(int value, ...) {
    va_list arguments;
    va_start(arguments, value);
    va_end(arguments);
    return value;
}
struct Guard {
    int *cleanup;
    Guard(int *p) : cleanup(p) {}
    ~Guard() { ++*cleanup; }
};
int raises(int *cleanup, int marker, ...) {
    Guard guard(cleanup);
    va_list arguments;
    va_start(arguments, marker);
    throw va_arg(arguments, int);
}
Scanner select_scan() { return scan; }

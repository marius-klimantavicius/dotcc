#include "api.h"
int read_cursor(va_list ap) {
    va_list copy;
    va_copy(copy, ap);
    int first = va_arg(copy, int);
    int same = va_arg(ap, int);
    va_end(copy);
    return first == same;
}
int consume(int mode, ...) {
    if (!mode) return 42;
    va_list ap;
    va_start(ap, mode);
    int (*reader)(va_list) = read_cursor;
    va_list preview;
    va_copy(preview, ap);
    int matching = reader(preview);
    va_end(preview);
    int wide = va_arg(ap, int);
    int negative = va_arg(ap, int);
    double real = va_arg(ap, double);
    int *value = va_arg(ap, int *);
    Unary callback = va_arg(ap, Unary);
    unsigned long large = va_arg(ap, unsigned long);
    va_end(ap);
    return matching && wide == 65530 && negative == -5 && real == 1.5 &&
        *value == 10 && callback(40) == 42 && large == 4294967313UL ? 42 : -1;
}

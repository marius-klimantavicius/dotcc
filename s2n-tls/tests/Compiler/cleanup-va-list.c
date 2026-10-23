#include <stdio.h>
#include <stdarg.h>
static void end(va_list *args) { va_end(*args); }
static int print(const char *format, ...) {
    __attribute__((cleanup(end))) va_list args = { 0 };
    va_start(args, format);
    return vprintf(format, args);
}
int main(void) { print("varargs=%d\n", 42); return 0; }

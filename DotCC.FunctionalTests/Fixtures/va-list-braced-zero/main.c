#include <stdarg.h>
#include <stdio.h>
static int sum(int count, ...)
{
    va_list first = { 0 };
    va_list second = { 0 };
    va_start(first, count);
    va_copy(second, first);
    int a = va_arg(first, int);
    int b = va_arg(first, int);
    int c = va_arg(second, int);
    va_end(second);
    va_end(first);
    return a * 100 + b * 10 + c;
}
int main(void) { printf("%d\n", sum(2, 3, 7)); return 0; }

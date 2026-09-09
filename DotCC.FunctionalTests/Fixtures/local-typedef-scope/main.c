#include <stdio.h>
#include <stdarg.h>
typedef int Number;
static int increment(int x) { return x + 1; }
static int add(int x, int y) { return x + y; }
static int configured(int value, ...) {
    va_list ap;
    va_start(ap, value);
    typedef int (*LocalCallback)(int);
    LocalCallback callback = va_arg(ap, LocalCallback);
    va_end(ap);
    return callback(value);
}
static int work(void) {
    typedef int (*Callback)(int);
    Callback invoke = increment;
    int total = invoke(41);
    {
        typedef long Number;
        typedef int (*Callback)(int, int);
        Number value = 0;
        Callback pair = add;
        total += (int)sizeof(value) + pair(1, 2);
    }
    Number value = 0;
    Callback again = increment;
    total += (int)sizeof(value) + again(1);
    return total;
}
static int after(void) {
    int Callback = 7;
    Number number = 9;
    return Callback + number;
}
int main(void) {
    typedef int (*Callback)(int);
    Callback callback = increment;
    printf("%d %d %d %d\n", work(), after(), configured(41, increment), configured(16, callback));
    return 0;
}

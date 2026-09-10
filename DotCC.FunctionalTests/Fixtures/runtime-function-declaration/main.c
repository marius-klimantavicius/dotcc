#include <stdio.h>
int abs(int);
typedef int (*Callback)(int);
int main(void) {
    Callback p = abs;
    printf("%d %d\n", p(-42), p == abs);
    return 0;
}

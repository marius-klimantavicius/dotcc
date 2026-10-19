/* Reduced next actual-source parser blocker: floatobject.c's `int ..., digit`. */
#include <stdio.h>
typedef long digit;
int main(void) {
    int half_eps, digit;
    half_eps = 2;
    digit = 5;
    printf("%d %zu\n", half_eps + digit, sizeof(digit));
    return 0;
}

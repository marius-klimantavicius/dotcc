#include <stdio.h>
int main(void) {
    const unsigned char values[2] = {20,22};
    register const unsigned char *a = values, *b = values + 1;
    register int total = *a + *b;
    for (register int i = 0; i < 2; ++i) total += i;
    printf("%d\n", total);
    return 0;
}

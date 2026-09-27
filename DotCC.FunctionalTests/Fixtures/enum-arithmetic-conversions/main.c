#include <stdio.h>

enum Tag { FIRST = 4, SECOND = 5 };

static int length(unsigned int kind) {
    int result;
    result = kind - FIRST + 1;
    return result;
}

int main(void) {
    unsigned int kind = 5;
    int nested = 2 + ((kind - FIRST) >> 1);
    int mixed = ((kind - FIRST) & 1) + (nested > 2);
    if (length(kind) != 2 || length(0) != -3 || nested != 2 || mixed != 1) return 1;
    if (sizeof(1UL + FIRST) != 8 || sizeof(1.0 + FIRST) != 8) return 2;
    if ((1UL << 40) + SECOND != 1099511627781UL) return 3;
    if (0.5 + FIRST != 4.5) return 4;
    printf("enum length %d wrap %d nested %d mixed %d widths %zu %zu\n",
           length(kind), length(0), nested, mixed, sizeof(1UL + FIRST), sizeof(1.0 + FIRST));
    return 0;
}

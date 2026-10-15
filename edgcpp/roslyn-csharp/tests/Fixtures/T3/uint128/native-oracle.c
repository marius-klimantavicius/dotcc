#include <stdio.h>
int fixture(void);
unsigned long uint128_observe(int index);
int main(void) {
    printf("result=%d\n", fixture());
    for (int index = 0; index < 8; ++index) printf("%d:%lu\n", index, uint128_observe(index));
    return 0;
}

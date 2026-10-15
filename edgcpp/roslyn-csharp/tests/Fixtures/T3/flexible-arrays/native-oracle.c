#include <stdio.h>
int fixture(void);
long flexible_observe(int index);
int main(void) {
    printf("result=%d\n", fixture());
    for (int index = 0; index < 12; ++index) printf("%d:%ld\n", index, flexible_observe(index));
    return 0;
}

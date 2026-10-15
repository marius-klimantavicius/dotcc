/* Native link/reference process only, compiled as C before linking with C++. */
#include <stdio.h>
extern int fixture(void);

int main(void) {
    printf("result=%d\n", fixture());
    return 0;
}

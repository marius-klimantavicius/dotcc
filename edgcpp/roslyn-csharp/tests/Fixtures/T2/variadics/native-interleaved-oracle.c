/* Independent native reference process only. */
#include <stdio.h>
int fixture(void);
int main(void) {
    printf("%d\n", fixture());
    return 0;
}

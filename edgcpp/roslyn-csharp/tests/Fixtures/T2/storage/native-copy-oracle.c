/* Native application oracle only. Never link into a generated workload. */
#include <stdio.h>
extern int fixture(void);

int main(void) {
    printf("result=%d\n", fixture());
    return 0;
}

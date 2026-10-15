/* Native reference process only. Never link native application bodies into a
   generated-code workload. */
#include <stdio.h>
extern int fixture(void);

int main(void) {
    printf("result=%d\n", fixture());
    return 0;
}

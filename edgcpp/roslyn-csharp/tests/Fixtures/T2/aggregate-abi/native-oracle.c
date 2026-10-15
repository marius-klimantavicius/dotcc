/* Independent native reference process ONLY; never a generated dependency. */
#include <stdio.h>
#include "aggregate-abi.h"
int main(void) {
    printf("result=%d\n", fixture());
    int count = aggregate_observation_count();
    printf("observations=%d\n", count);
    for (int index = 0; index < count; ++index)
        printf("%d:%d\n", index, aggregate_observe(index));
    return 0;
}
